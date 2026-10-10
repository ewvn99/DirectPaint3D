using System.Diagnostics.CodeAnalysis;
using Wew.Control;
using Wew.Media;
using Vector = Wew.Media.Vector;
using Math = System.Math;
using MathF = System.MathF;
using mMath = Wew.mMath;
using Wew;

//¿calc tang
//float3 vUp, vLeft;
//
//if (abs(dot(g_UpVector, normal)) < 0.9)									// Hacer escuadra con Up
//{	vLeft = cross(g_UpVector, normal); vUp = cross(normal, vLeft);
//} else																	// Hacer escuadra con Left
//{	vUp = cross(normal, g_LeftVector); vLeft = cross(vUp, normal);
//}

class uFigs : cContainer
{	struct Patch
	{	public bool ReflejaZ;
		public Vector[] Inds;
	};
	class cNumTxtBox : cNumericTextBox
	{	public cNumTxtBox()									{	Width = 70; Type = eNumberType.PositiveOrZeroFloating;}
	}
	cTurbulenceEffect? m_teTurbu;
	cEditableGeometry? m_egRevolGeo; Point m_ptRevolOrig, m_ptRevolUlt; cBitmap? m_bmpRevol; cBrush m_brRevol;
	cEditableGeometry? m_egFormaGeo; Point m_ptFormaUlt; cBitmap? m_bmpForma; cBrush m_brForma;
	cFont m_fntChars; short m_shChGli;
	readonly cTabControl tcTipo;
	readonly cNumericTextBox ntbEsfeAngPrecH, ntbEsfeAngPrecV, ntbEsfeAngIniH, ntbEsfeAngFinH, ntbEsfeAngIniV, ntbEsfeAngFinV, ntbEsfeRadio;
	readonly cNumericTextBox ntbCilAngPrec, ntbCilRadio; readonly cCheckBox chkCilTapaSup, chkCilTapaInf;
		readonly cNumericTextBox ntbCilAngIni, ntbCilAngFin, ntbCilAlto;
	readonly cNumericTextBox ntbCubPrecH, ntbCubPrecF, ntbCubPrecV, ntbCubAncho, ntbCubFondo, ntbCubAlto;
		readonly cCheckBox chkDer, chkTapa, chkFrente, chkBase, chkFondo, chkIzq;
	readonly cNumericTextBox ntbPlaPrecH, ntbPlaPrecF, ntbPlaAncho, ntbPlaFondo;
	readonly cNumericTextBox ntbCirAngPrec, ntbCirRadio, ntbCirAngIni, ntbCirAngFin;
	readonly cNumericTextBox ntbRevolAngPrecH, ntbRevolPrecV, ntbRevolAngIniH, ntbRevolAngFinH, ntbRevolEscala, ntbRevolArcAng;
		readonly uPoint uptRevolArcRadio;
		readonly cToolButton tbbRevolPto, tbbRevolLin, tbbRevolArc, tbbRevolBezier, tbbRevolQbezier; readonly cPaintControl paiRevol;
		readonly cLabel lblRevolPos; readonly cColorButton cbtRevol;
	readonly cNumTxtBox ntbTesel; readonly uVector uvBezierEscala; readonly cGrid grdBezier; readonly cCheckBox chkBezierVerts;
	readonly cNumericTextBox ntbFormaEscala, ntbFormaArcAng; readonly uPoint uptFormaArcRadio;
		readonly cToolButton tbbFormaPto, tbbFormaLin, tbbFormaArc, tbbFormaBezier, tbbFormaQbezier; readonly cPaintControl paiForma;
		readonly cLabel lblFormaPos; readonly cColorButton cbtForma;
	readonly cLabel lblCharFnt; readonly cButton btnCharGli; readonly cRadioButton rbtCharTxt;
		readonly cTextBox txtCharChars; readonly cGeometryControl gctCharGli; readonly cNumericTextBox ntbCharFondo;
	readonly cNumericTextBox ntbTerrPrecH, ntbTerrPrecF, ntbTerrAncho, ntbTerrFondo, ntbTerrAlto; readonly uSlider uslTerrFrec, uslTerrOctaves;
		readonly cNumericTextBox ntbTerrSeed; readonly cCheckBox chkTerrNoise, chkTerrStitch; readonly cPaintControl paiTerr;
	readonly cTextBox txtNomb;
private static void s_CreaPlano(int iCantCols, int iCantFils, Vector vIni, Vector vStrideV, Vector vColFin
	, Vector vDer, Vector vAba, Vector vNormal, Vector vTang
	, float fPerimHIni, float fAncho, float fPrecisAncho, float fPerimHTot
	, float fPerimVIni, float fAlto, float fPrecisAlto, float fPerimVTot
	, Vertex[] a_vxVerts, int[] a_iInds, ref int iVert, ref int iIdx)
{	Vector v; int iVertSigFila; Point pt; float fTuDelta, fTvDelta, fTuFin, fTvFin;

	// ** Gen cols
	vIni /= 2; vColFin /= 2;
	pt.X = fPerimHIni / fPerimHTot; fTuDelta = fPrecisAncho / fPerimHTot; fTvDelta = fPrecisAlto / fPerimVTot;
	fTuFin = (fPerimHIni + fAncho) / fPerimHTot; fTvFin = (fPerimVIni + fAlto) / fPerimVTot; fPerimVIni /= fPerimVTot;
	for (int i = 0; i <= iCantCols; i++, vIni += vDer, pt.X += fTuDelta)
	{	if (i == iCantCols) {	vIni = vColFin; pt.X = fTuFin;}				// Ult col: evitar exceso; la ult col sí puede quedar incompleta
		v = vIni; pt.Y = fPerimVIni;
		// ** Gen col: crear fils
		for (int j = 0; j <= iCantFils; j++, v += vAba, pt.Y += fTvDelta)
		{	if (j == iCantFils)												// Ult fil: evitar exceso; la ult fil sí puede quedar incompleta
			{	v = vIni + vStrideV; pt.Y = fTvFin;
			// ** Agregar idx (excepto para la ult col y la ult fil)
			} else if (i < iCantCols)
			{	iVertSigFila = iVert + iCantFils + 1;
				a_iInds[iIdx++] = iVert; a_iInds[iIdx++] = iVertSigFila; a_iInds[iIdx++] = iVert + 1;
				a_iInds[iIdx++] = iVert + 1; a_iInds[iIdx++] = iVertSigFila; a_iInds[iIdx++] = iVertSigFila + 1;
			}
			// ** Agregar vert
			a_vxVerts[iVert].Position = v; a_vxVerts[iVert].Normal = vNormal;
			a_vxVerts[iVert].Tangent = vTang; a_vxVerts[iVert].Texcoord = pt;
			iVert++;
		}
	}
}
private static void s_CalcNormal(Vertex[] a_verVerts, int iV1, int iV2, int iV3)
{	Vector vNorm;

	vNorm = (a_verVerts[iV2].Position - a_verVerts[iV1].Position).Cross(a_verVerts[iV3].Position - a_verVerts[iV1].Position); // Sin normalizar
	s_AsigNorm(ref a_verVerts[iV1].Normal, vNorm); s_AsigNorm(ref a_verVerts[iV2].Normal, vNorm);
		s_AsigNorm(ref a_verVerts[iV3].Normal, vNorm);
}
private static void s_AsigNorm(ref Vector vDest, Vector vNorm)
{	if (!vDest.IsNaN)	vDest.Lerp(vNorm, 0.5f);	else	vDest = vNorm;	// Si ya tiene normal: promediar
}
public uFigs()
{	cTabControl.cTab tab; cStackPanel spnCli, spn; uPropertyGroup grp; uPropertySubgroup sgr; cContainer cnt;
	cLabel lbl; cToolButton tbt; cRadioButton rbt; cButton btn;

	Width = 600; Height = 395;
	tcTipo = new cTabControl {	Margins = new Rect(0, 0, 2, 39)}; tcTipo.SelectionChanged += tcTipo_SelectionChanged;
	// ** Esfera
	tab = tcTipo.Tabs.Add("Sphere");
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, Border = new Rect(5), RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup {	Text = "Precision angles", Direction = eDirection.Right};
		sgr = new uPropertySubgroup {	Text = "Horizontal", Width = 190, RightMargin = float.NaN};
			ntbEsfeAngPrecH = new cNumTxtBox {	FloatValue = 10};
				sgr.AddControl(ntbEsfeAngPrecH);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "Vertical", Width = 190, RightMargin = float.NaN};
			ntbEsfeAngPrecV = new cNumTxtBox {	FloatValue = 10};
				sgr.AddControl(ntbEsfeAngPrecV);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Horizontal arc angles", Direction = eDirection.Right};
		sgr = new uPropertySubgroup {	Text = "Start", Width = 190, RightMargin = float.NaN};
			ntbEsfeAngIniH = new cNumTxtBox();
				sgr.AddControl(ntbEsfeAngIniH);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "End", Width = 190, RightMargin = float.NaN};
			ntbEsfeAngFinH = new cNumTxtBox {	FloatValue = 360};
				sgr.AddControl(ntbEsfeAngFinH);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Vertical arc angles", Direction = eDirection.Right};
		sgr = new uPropertySubgroup {	Text = "Start", Width = 190, RightMargin = float.NaN};
			ntbEsfeAngIniV = new cNumTxtBox();
				sgr.AddControl(ntbEsfeAngIniV);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "End", Width = 190, RightMargin = float.NaN};
			ntbEsfeAngFinV = new cNumTxtBox {	FloatValue = 180};
				sgr.AddControl(ntbEsfeAngFinV);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Radius", Direction = eDirection.Right};
		sgr = new uPropertySubgroup {	Text = "Radius", Width = 190, RightMargin = float.NaN};
			ntbEsfeRadio = new cNumTxtBox {	FloatValue = 10};
				sgr.AddControl(ntbEsfeRadio);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	tab.Content = spnCli;
	// ** Cilindro
	tab = tcTipo.Tabs.Add("Cylinder");
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, Border = new Rect(5), RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup {	Text = "Planes"};
		sgr = new uPropertySubgroup {	Text = "Precision angle"};
			ntbCilAngPrec = new cNumTxtBox {	FloatValue = 10};
				sgr.AddControl(ntbCilAngPrec);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "Radius"};
			ntbCilRadio = new cNumTxtBox {	FloatValue = 10};
				sgr.AddControl(ntbCilRadio);
			grp.AddControl(sgr);
		cnt = new cContainer {	AutoSize = eAutoSize.Height};
			chkCilTapaSup = new cCheckBox {	Text = "Add top plane", Checked = true, LeftMargin = 110};
				cnt.AddControl(chkCilTapaSup);
			chkCilTapaInf = new cCheckBox {	Text = "Add bottom plane", Checked = true, LeftMargin = 310};
				cnt.AddControl(chkCilTapaInf);
			grp.AddControl(cnt);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Arc angles", Direction = eDirection.Right};
		sgr = new uPropertySubgroup {	Text = "Start", Width = 190, RightMargin = float.NaN};
			ntbCilAngIni = new cNumTxtBox();
				sgr.AddControl(ntbCilAngIni);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "End", Width = 190, RightMargin = float.NaN};
			ntbCilAngFin = new cNumTxtBox {	FloatValue = 360};
				sgr.AddControl(ntbCilAngFin);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Cylinder", Direction = eDirection.Right};
		sgr = new uPropertySubgroup {	Text = "Height", Width = 190, RightMargin = float.NaN};
			ntbCilAlto = new cNumTxtBox {	FloatValue = 10};
				sgr.AddControl(ntbCilAlto);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	tab.Content = spnCli;
	// ** Cubo
	tab = tcTipo.Tabs.Add("Cube");
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, Border = new Rect(5), RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup {	Text = "Precision", Direction = eDirection.Right};
		sgr = new uPropertySubgroup {	Text = "Horizontal", Width = 190, RightMargin = float.NaN};
			ntbCubPrecH = new cNumTxtBox {	FloatValue = 1};
				sgr.AddControl(ntbCubPrecH);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "Depth", Width = 190, RightMargin = float.NaN};
			ntbCubPrecF = new cNumTxtBox {	FloatValue = 1};
				sgr.AddControl(ntbCubPrecF);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "Vertical", Width = 190, RightMargin = float.NaN};
			ntbCubPrecV = new cNumTxtBox {	FloatValue = 1};
				sgr.AddControl(ntbCubPrecV);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Size", Direction = eDirection.Right};
		sgr = new uPropertySubgroup {	Text = "Width", Width = 190, RightMargin = float.NaN};
			ntbCubAncho = new cNumTxtBox {	FloatValue = 10};
				sgr.AddControl(ntbCubAncho);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "Depth", Width = 190, RightMargin = float.NaN};
			ntbCubFondo = new cNumTxtBox {	FloatValue = 10};
				sgr.AddControl(ntbCubFondo);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "Height", Width = 190, RightMargin = float.NaN};
			ntbCubAlto = new cNumTxtBox {	FloatValue = 10};
				sgr.AddControl(ntbCubAlto);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Faces"};
		cnt = new cContainer {	AutoSize = eAutoSize.Height};
			chkDer = new cCheckBox {	Text = "Right", Checked = true, LeftMargin = 63, TopMargin = 20};
				cnt.AddControl(chkDer);
			chkTapa = new cCheckBox {	Text = "Top", Checked = true, LeftMargin = 194};
				cnt.AddControl(chkTapa);
			chkFrente = new cCheckBox {	Text = "Front", Checked = true, LeftMargin = 194, TopMargin = 20};
				cnt.AddControl(chkFrente);
			chkBase = new cCheckBox {	Text = "Bottom", Checked = true, LeftMargin = 194, TopMargin = 40};
				cnt.AddControl(chkBase);
			chkFondo = new cCheckBox {	Text = "Back", Checked = true, LeftMargin = 325, TopMargin = 20};
				cnt.AddControl(chkFondo);
			chkIzq = new cCheckBox {	Text = "Left", Checked = true, LeftMargin = 435, TopMargin = 20};
				cnt.AddControl(chkIzq);
			grp.AddControl(cnt);
		spnCli.AddControl(grp);
	tab.Content = spnCli;
	// ** Plano
	tab = tcTipo.Tabs.Add("Plane");
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, Border = new Rect(5), RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup {	Text = "Precision", Direction = eDirection.Right};
		sgr = new uPropertySubgroup {	Text = "Horizontal", Width = 190, RightMargin = float.NaN};
			ntbPlaPrecH = new cNumTxtBox {	FloatValue = 1};
				sgr.AddControl(ntbPlaPrecH);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "Depth", Width = 190, RightMargin = float.NaN};
			ntbPlaPrecF = new cNumTxtBox {	FloatValue = 1};
				sgr.AddControl(ntbPlaPrecF);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Size", Direction = eDirection.Right};
		sgr = new uPropertySubgroup {	Text = "Width", Width = 190, RightMargin = float.NaN};
			ntbPlaAncho = new cNumTxtBox {	FloatValue = 10};
				sgr.AddControl(ntbPlaAncho);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "Depth", Width = 190, RightMargin = float.NaN};
			ntbPlaFondo = new cNumTxtBox {	FloatValue = 10};
				sgr.AddControl(ntbPlaFondo);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	tab.Content = spnCli;
	// ** Círculo
	tab = tcTipo.Tabs.Add("Circle");
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, Border = new Rect(5), RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup {	Text = "Size"};
		sgr = new uPropertySubgroup {	Text = "Precision angle"};
			ntbCirAngPrec = new cNumTxtBox {	FloatValue = 10};
				sgr.AddControl(ntbCirAngPrec);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "Radius"};
			ntbCirRadio = new cNumTxtBox {	FloatValue = 10};
				sgr.AddControl(ntbCirRadio);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Arc angles", Direction = eDirection.Right};
		sgr = new uPropertySubgroup {	Text = "Start", Width = 190, RightMargin = float.NaN};
			ntbCirAngIni = new cNumTxtBox();
				sgr.AddControl(ntbCirAngIni);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "End", Width = 190, RightMargin = float.NaN};
			ntbCirAngFin = new cNumTxtBox {	FloatValue = 360};
				sgr.AddControl(ntbCirAngFin);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	tab.Content = spnCli;
	// ** Revolución
	tab = tcTipo.Tabs.Add("Revolution");
	spnCli = new cStackPanel {	Border = new Rect(5), RightMargin = 1, AutoSize = eAutoSize.Height};
	spn = new cStackPanel {	AutoSize = eAutoSize.Height, Width = 200, Direction = eDirection.Bottom};
		grp = new uPropertyGroup {	Text = "Precision"};
			sgr = new uPropertySubgroup {	Text = "Horizontal angles"};
				ntbRevolAngPrecH = new cNumTxtBox {	FloatValue = 10};
					sgr.AddControl(ntbRevolAngPrecH);
				grp.AddControl(sgr);
			sgr = new uPropertySubgroup {	Text = "Vertical"};
				ntbRevolPrecV = new cNumTxtBox {	FloatValue = 10};
					sgr.AddControl(ntbRevolPrecV);
				grp.AddControl(sgr);
			spn.AddControl(grp);
		grp = new uPropertyGroup {	Text = "Horizontal arc angles"};
			sgr = new uPropertySubgroup {	Text = "Start"};
				ntbRevolAngIniH = new cNumTxtBox();
					sgr.AddControl(ntbRevolAngIniH);
				grp.AddControl(sgr);
			sgr = new uPropertySubgroup {	Text = "End"};
				ntbRevolAngFinH = new cNumTxtBox {	FloatValue = 360};
					sgr.AddControl(ntbRevolAngFinH);
				grp.AddControl(sgr);
			spn.AddControl(grp);
		grp = new uPropertyGroup {	Text = "Scale"};
			sgr = new uPropertySubgroup {	Text = "Scale"};
				ntbRevolEscala = new cNumTxtBox {	FloatValue = 0.1f};
					sgr.AddControl(ntbRevolEscala);
				grp.AddControl(sgr);
			spn.AddControl(grp);
		grp = new uPropertyGroup {	Text = "Arc"};
			sgr = new uPropertySubgroup {	Text = "Angle"};
				ntbRevolArcAng = new cNumTxtBox();
					sgr.AddControl(ntbRevolArcAng);
				grp.AddControl(sgr);
			uptRevolArcRadio = new uPoint {	Text = "Radius", Value = new Point(10, 10), Height = 47};
				uptRevolArcRadio.TextBoxX.Width = 70; uptRevolArcRadio.TextBoxY.Bounds = new Rectangle(110, 26, 70, 21);
				grp.AddControl(uptRevolArcRadio);
			spn.AddControl(grp);
		spnCli.AddControl(spn);
	spn = new cStackPanel {	AutoSize = eAutoSize.Height, BorderStyle = eBorderStyle.Default, Width = 36, Direction = eDirection.Bottom
			, Separation = 0};
		tbbRevolPto = new cToolButton {	ToolTip = "Free line", Type = eButtonType.Radio, Bitmap = eRes.BmpLinLibre, Checked = true};
			spn.AddControl(tbbRevolPto);
		tbbRevolLin = new cToolButton {	ToolTip = "Straight Line", Type = eButtonType.Radio, Bitmap = eRes.BmpLin};
			spn.AddControl(tbbRevolLin);
		tbbRevolArc = new cToolButton {	ToolTip = "Arc", Type = eButtonType.Radio, Bitmap = eRes.BmpArc};
			spn.AddControl(tbbRevolArc);
		tbbRevolBezier = new cToolButton {	ToolTip = "Bezier (press Ctrl to add points)", Type = eButtonType.Radio
				, Bitmap = eRes.BmpBezier};
			spn.AddControl(tbbRevolBezier);
		tbbRevolQbezier = new cToolButton {	ToolTip = "Quadratic bezier (press Ctrl to add points)", Type = eButtonType.Radio
				, Bitmap = eRes.BmpQuadraticBezier};
			spn.AddControl(tbbRevolQbezier);
		spn.AddControl(new cSeparator());
		tbt = new cToolButton {	ToolTip = "Undo", Bitmap = eBitmap.BmpUndo};
			tbt.Click += RevolDeshacer_Click; spn.AddControl(tbt);
		tbt = new cToolButton {	ToolTip = "Clear", Bitmap = eBitmap.BmpNew};
			tbt.Click += RevolLimpiar_Click; spn.AddControl(tbt);
		tbt = new cToolButton {	ToolTip = "Set background", Bitmap = eBitmap.BmpOpen};
			tbt.Click += RevolFondo_Click; spn.AddControl(tbt);
		tbt = new cToolButton {	ToolTip = "Remove background", Bitmap = eRes.BmpLimpiaFondo};
			tbt.Click += RevolQuitaFondo_Click; spn.AddControl(tbt);
		spnCli.AddControl(spn);
	cnt = new cContainer {	AutoSize = eAutoSize.Height, Width = 350, LeftMargin = 10};
		paiRevol = new cPaintControl {	FocusMode = eFocusMode.FocusControl, Width = 230, Height = 230, BorderStyle = eBorderStyle.Default};
			paiRevol.Paint += paiRevol_Paint; paiRevol.MouseDown += paiRevol_MouseDown; paiRevol.MouseMove += paiRevol_MouseMove;
			paiRevol.KeyDown += paiRevol_KeyDown; cnt.AddControl(paiRevol);
		cbtRevol = new cColorButton {	ToolTip = "Line color", Color = eColor.Black, TopMargin = 240, Width = 30, Height = 30};
			cbtRevol.ColorChanged += cbtRevol_Changed; cnt.AddControl(cbtRevol);
		lbl = new cLabel {	Text = "Current position", Width = 120, TextAlignment = eTextAlignment.Right
				, AutoSize = eAutoSize.None, LeftMargin = 40, TopMargin = 245};
			cnt.AddControl(lbl);
		lblRevolPos = new cLabel {	Text = "0", Width = 90, TextAlignment = eTextAlignment.Center
				, AutoSize = eAutoSize.None, BorderStyle = eBorderStyle.Default, LeftMargin = 170, TopMargin = 245};
			cnt.AddControl(lblRevolPos);
		lbl = new cLabel {	Text = "Draw the outline at the right of the axis from top to bottom"
				, AutoSize = eAutoSize.Height, Wrapping = eWrapping.Wrap, TopMargin = 275, RightMargin = 0};
			cnt.AddControl(lbl);
		spnCli.AddControl(cnt);
	tab.Content = spnCli;
	// ** Bezieres
	tab = tcTipo.Tabs.Add("Beziers");
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, Border = new Rect(5), RightMargin = 1, AutoSize = eAutoSize.Height};
	cnt = new cContainer {	RightMargin = 0, AutoSize = eAutoSize.Height};
		grp = new uPropertyGroup {	Text = "Tesselation", RightMargin = float.NaN, Width = 190};
			sgr = new uPropertySubgroup {	Text = "Tesselation"};
				ntbTesel = new cNumTxtBox {	Type = eNumberType.Positive, IntValue = 5};
					sgr.AddControl(ntbTesel);
				grp.AddControl(sgr);
			cnt.AddControl(grp);
		grp = new uPropertyGroup {	Text = "Scale", LeftMargin = 210};
			uvBezierEscala = new uVector {	Text = "Scale", Constraint = eNumberType.PositiveFloating, Value = new Vector(10, 10, 10)};
				grp.AddControl(uvBezierEscala);
			cnt.AddControl(grp);
		spnCli.AddControl(cnt);
	grp = new uPropertyGroup {	Text = "Patches"};
		grdBezier = new cGrid {	Height = 200, ColumnCount = 5, RowCount = 1, AllowColumnReorder = false};
			grdBezier.ConfigureColumn(0, "Reflex Z", 60, TextFormat: eTextFormat.CenterMiddle);
			grdBezier.ConfigureColumn(1, "Column 1", 210, default, null, false, true, typeof(Vector));
			grdBezier.ConfigureColumn(2, "Column 2", 210, default, null, false, true, typeof(Vector));
			grdBezier.ConfigureColumn(3, "Column 3", 210, default, null, false, true, typeof(Vector));
			grdBezier.ConfigureColumn(4, "Column 4", 210, default, null, false, true, typeof(Vector));
			grp.AddControl(grdBezier);
		cnt = new cContainer {	Height = 50};
			btn = new cButton {	Text = "Add"};
				btn.Click += BezierAgrega_Click;
				cnt.AddControl(btn);
			btn = new cButton {	Text = "Remove", LeftMargin = 110};
				btn.Click += BezierQuita_Click;
				cnt.AddControl(btn);
			btn = new cButton {	Text = "Clear", LeftMargin = 220};
				btn.Click += BezierLimpia_Click;
				cnt.AddControl(btn);
			btn = new cButton {	Text = "Open", LeftMargin = 330};
				btn.Click += BezierAbre_Click;
				cnt.AddControl(btn);
			btn = new cButton {	Text = "Save", LeftMargin = 440};
				btn.Click += BezierGraba_Click;
				cnt.AddControl(btn);
			btn = new cButton {	Text = "Teapot", LeftMargin = 440, TopMargin = 25};
				btn.Click += BezierTete_Click;
				cnt.AddControl(btn);
			chkBezierVerts = new cCheckBox {	Text = "Create vertices", TopMargin = 25};
				cnt.AddControl(chkBezierVerts);
			grp.AddControl(cnt);
		spnCli.AddControl(grp);
	tab.Content = spnCli;
	// ** Forma
	tab = tcTipo.Tabs.Add("Shape");
	spnCli = new cStackPanel {	Border = new Rect(5), RightMargin = 1, AutoSize = eAutoSize.Height};
	spn = new cStackPanel {	AutoSize = eAutoSize.Height, Width = 200, Direction = eDirection.Bottom};
		grp = new uPropertyGroup {	Text = "Scale"};
			sgr = new uPropertySubgroup {	Text = "Scale"};
				ntbFormaEscala = new cNumTxtBox {	FloatValue = 0.1f};
					sgr.AddControl(ntbFormaEscala);
				grp.AddControl(sgr);
			spn.AddControl(grp);
		grp = new uPropertyGroup {	Text = "Arc"};
			sgr = new uPropertySubgroup {	Text = "Angle"};
				ntbFormaArcAng = new cNumTxtBox();
					sgr.AddControl(ntbFormaArcAng);
				grp.AddControl(sgr);
			uptFormaArcRadio = new uPoint {	Text = "Radius", Value = new Point(10, 10), Height = 47};
				uptFormaArcRadio.TextBoxX.Width = 70; uptFormaArcRadio.TextBoxY.Bounds = new Rectangle(110, 26, 70, 21);
				grp.AddControl(uptFormaArcRadio);
			spn.AddControl(grp);
		spnCli.AddControl(spn);
	spn = new cStackPanel {	AutoSize = eAutoSize.Height, BorderStyle = eBorderStyle.Default, Width = 36, Direction = eDirection.Bottom
			, Separation = 0};
		tbbFormaPto = new cToolButton {	ToolTip = "Free line", Type = eButtonType.Radio, Bitmap = eRes.BmpLinLibre, Checked = true};
			spn.AddControl(tbbFormaPto);
		tbbFormaLin = new cToolButton {	ToolTip = "Straight Line", Type = eButtonType.Radio, Bitmap = eRes.BmpLin};
			spn.AddControl(tbbFormaLin);
		tbbFormaArc = new cToolButton {	ToolTip = "Arc", Type = eButtonType.Radio, Bitmap = eRes.BmpArc};
			spn.AddControl(tbbFormaArc);
		tbbFormaBezier = new cToolButton {	ToolTip = "Bezier (press Ctrl to add points)", Type = eButtonType.Radio
				, Bitmap = eRes.BmpBezier};
			spn.AddControl(tbbFormaBezier);
		tbbFormaQbezier = new cToolButton {	ToolTip = "Quadratic bezier (press Ctrl to add points)", Type = eButtonType.Radio
				, Bitmap = eRes.BmpQuadraticBezier};
			spn.AddControl(tbbFormaQbezier);
		spn.AddControl(new cSeparator());
		tbt = new cToolButton {	ToolTip = "Undo", Bitmap = eBitmap.BmpUndo};
			tbt.Click += FormaDeshacer_Click; spn.AddControl(tbt);
		tbt = new cToolButton {	ToolTip = "Clear", Bitmap = eBitmap.BmpNew};
			tbt.Click += FormaLimpiar_Click; spn.AddControl(tbt);
		tbt = new cToolButton {	ToolTip = "Set background", Bitmap = eBitmap.BmpOpen};
			tbt.Click += FormaFondo_Click; spn.AddControl(tbt);
		tbt = new cToolButton {	ToolTip = "Remove background", Bitmap = eRes.BmpLimpiaFondo};
			tbt.Click += FormaQuitaFondo_Click; spn.AddControl(tbt);
		spnCli.AddControl(spn);
	cnt = new cContainer {	AutoSize = eAutoSize.Height, Width = 350, LeftMargin = 10};
		paiForma = new cPaintControl {	FocusMode = eFocusMode.FocusControl, Width = 230, Height = 230, BorderStyle = eBorderStyle.Default};
			paiForma.Paint += paiForma_Paint; paiForma.MouseDown += paiForma_MouseDown; paiForma.MouseMove += paiForma_MouseMove;
			paiForma.KeyDown += paiForma_KeyDown; cnt.AddControl(paiForma);
		cbtForma = new cColorButton {	ToolTip = "Line color", Color = eColor.Black, TopMargin = 240, Width = 30, Height = 30};
			cbtForma.ColorChanged += cbtForma_ColorChanged; cnt.AddControl(cbtForma);
		lbl = new cLabel {	Text = "Current position", Width = 120, TextAlignment = eTextAlignment.Right
				, AutoSize = eAutoSize.None, LeftMargin = 40, TopMargin = 245};
			cnt.AddControl(lbl);
		lblFormaPos = new cLabel {	Text = "0", Width = 90, TextAlignment = eTextAlignment.Center
				, AutoSize = eAutoSize.None, BorderStyle = eBorderStyle.Default, LeftMargin = 170, TopMargin = 245};
			cnt.AddControl(lblFormaPos);
		spnCli.AddControl(cnt);
	tab.Content = spnCli;
	// ** Letra
	tab = tcTipo.Tabs.Add("Characters");
	spnCli = new cStackPanel {	Border = new Rect(5), RightMargin = 1, AutoSize = eAutoSize.Height, Direction = eDirection.Bottom};
	grp = new uPropertyGroup {	Text = "Font", Direction = eDirection.Right};
		sgr = new uPropertySubgroup {	Text = "Font", Width = 460, RightMargin = float.NaN};
			lblCharFnt = new cLabel {	AutoSize = eAutoSize.None, Width = 350, BorderStyle = eBorderStyle.Default};
				sgr.AddControl(lblCharFnt);
			grp.AddControl(sgr);
		btn = new cButton {	Text = "...", Width = 30}; btn.Click += btnCharFnt_Click;
			grp.AddControl(btn);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Characters"};
		cnt = new cContainer {	AutoSize = eAutoSize.Height};
			rbtCharTxt = new cRadioButton {	Text = "Text", AutoSize = eAutoSize.None, Width = 100, Checked = true};
				cnt.AddControl(rbtCharTxt);
			txtCharChars = new cTextBox {	LeftMargin = 110, Width = 350};
				cnt.AddControl(txtCharChars);
			rbt = new cRadioButton {	Text = "Glyph", TopMargin = 30};
				cnt.AddControl(rbt);
			gctCharGli = new cGeometryControl {	LeftMargin = 110, TopMargin = 30, Width = 150, Height = 150
					, Border = new Rect(1), BorderStyle = eBorderStyle.Default};
				cnt.AddControl(gctCharGli);
			btnCharGli = new cButton {	Text = "...", LeftMargin = 265, TopMargin = 30, Width = 30};
				btnCharGli.Click += btnChGli_Click; cnt.AddControl(btnCharGli);
			grp.AddControl(cnt);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Depth", Direction = eDirection.Right};
		sgr = new uPropertySubgroup {	Text = "Depth"};
			ntbCharFondo = new cNumericTextBox {	Type = eNumberType.Positive | eNumberType.Fractional, FloatValue = 2};
				sgr.AddControl(ntbCharFondo);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	tab.Content = spnCli;
	AddControl(tcTipo);
	// ** Terreno
	tab = tcTipo.Tabs.Add("Terrain");
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, Border = new Rect(5), RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup {	Text = "Precision", Direction = eDirection.Right};
		sgr = new uPropertySubgroup {	Text = "Horizontal", Width = 190, RightMargin = float.NaN};
			ntbTerrPrecH = new cNumTxtBox {	FloatValue = 1};
				sgr.AddControl(ntbTerrPrecH);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "Depth", Width = 190, RightMargin = float.NaN};
			ntbTerrPrecF = new cNumTxtBox {	FloatValue = 1};
				sgr.AddControl(ntbTerrPrecF);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Size", Direction = eDirection.Right};
		sgr = new uPropertySubgroup {	Text = "Width", Width = 190, RightMargin = float.NaN};
			ntbTerrAncho = new cNumTxtBox {	FloatValue = 30};
				sgr.AddControl(ntbTerrAncho);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "Depth", Width = 190, RightMargin = float.NaN};
			ntbTerrFondo = new cNumTxtBox {	FloatValue = 30};
				sgr.AddControl(ntbTerrFondo);
			grp.AddControl(sgr);
		sgr = new uPropertySubgroup {	Text = "Height", Width = 190, RightMargin = float.NaN};
			ntbTerrAlto = new cNumTxtBox {	FloatValue = 3};
				sgr.AddControl(ntbTerrAlto);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	grp = new uPropertyGroup {	Text = "Noise", Direction = eDirection.Right};
		spn = new cStackPanel {	AutoSize = eAutoSize.Height, Direction = eDirection.Bottom, Width = 375};
			uslTerrFrec = new uSlider {	Text = "Base frequency", RightMargin = 0, Value = 0.03f * 500, Maximum = 100};
				uslTerrFrec.ValueChanged += uslTerr_Changed; spn.AddControl(uslTerrFrec);
			uslTerrOctaves = new uSlider {	Text = "Octaves", RightMargin = 0, Value = 1, Maximum = 5};
				uslTerrOctaves.ValueChanged += uslTerr_Changed; spn.AddControl(uslTerrOctaves);
			sgr = new uPropertySubgroup {	Text = "Seed"};
				ntbTerrSeed = new cNumericTextBox {	Width = 71, Type = eNumberType.Zero | eNumberType.Positive};
					ntbTerrSeed.ValueChanged += uslTerr_Changed; sgr.AddControl(ntbTerrSeed);
				spn.AddControl(sgr);
			cnt = new cContainer {	AutoSize = eAutoSize.Height, RightMargin = 0};
				chkTerrNoise = new cCheckBox {	Text = "Noise", Checked = true};
					chkTerrNoise.CheckStateChanged += uslTerr_Changed; cnt.AddControl(chkTerrNoise);
				chkTerrStitch = new cCheckBox {	Text = "Stitchable", LeftMargin = 194};
					chkTerrStitch.CheckStateChanged += uslTerr_Changed; cnt.AddControl(chkTerrStitch);
				spn.AddControl(cnt);
			grp.AddControl(spn);
		paiTerr = new cPaintControl {	BorderStyle = eBorderStyle.Dark, LeftMargin = 10, Width = 180, Height = 180, BackColor = null};
			paiTerr.Paint += paiTerr_Paint; grp.AddControl(paiTerr);
		spnCli.AddControl(grp);
	tab.Content = spnCli;
	// ** Pie
	lbl = new cLabel {	Text = "Name", Margins = new Rect(2, float.NaN, float.NaN, 8)};
		AddControl(lbl);
	txtNomb = new cTextBox {	Margins = new Rect(126, float.NaN, 138, 10)};
		AddControl(txtNomb);
	// ** Cfg
	m_ptRevolOrig = new Point(5, 5); m_brRevol = new cSolidBrush(eColor.Black); m_brForma = new cSolidBrush(eColor.Black);
	m_ChFont = new cFont("Segoe UI Symbol", 7.5f);
}
	[MemberNotNull(nameof(m_fntChars))]
private cFont m_ChFont
{	set
	{	m_fntChars = value; lblCharFnt.Text = $"{m_fntChars.Name}, {m_fntChars.Size}pt";
		txtCharChars.Font = new cFont(value, 10);
	}
}
public cModelo? ShowDialog()
{	cModelo? mdl;

	txtNomb.Text = null; mdl = null;
	_ = mDialog.ShowDialog(this, "Add figure"
		, () =>
		{		if (string.IsNullOrEmpty(txtNomb.Text))	{	mDialog.MsgBoxExclamation("Enter name", "Create figure"); txtNomb.Focus(); return false;}
			mdl = tcTipo.SelectedIndex switch
				{	0	=>	m_mdlCreaEsfera(),								// Esfera
					1	=>	m_mdlCreaCilindro(),							// Cilindro
					2	=>	m_mdlCreaCubo(),								// Cubo
					3	=>	m_mdlCreaPlano(),								// Plano
					4	=>	m_mdlCreaCírculo(),								// Círculo
					5	=>	m_mdlCreaRevol(),								// Revolución
					6	=>	m_mdlCreaBezier(),								// Bezieres
					7	=>	m_mdlCreaForma(),								// Forma libre
					8	=>	m_mdlCreaLetras(),								// Letras
					9	=>	m_mdlCreaTerreno(),								// Terreno
					_	=>	null
				};
				if (mdl is null)	return false;
			mdl.Name = txtNomb.Text;
			return true;
		}, false, KeepRootOnClose: true);
	return mdl;
}
private void tcTipo_SelectionChanged(object sender)			{	if (tcTipo.SelectedIndex == 5)	uslTerr_Changed(null);}
	[MemberNotNull(nameof(m_teTurbu))]
private void uslTerr_Changed(object? sender)
{	m_teTurbu ??= new cTurbulenceEffect();
	m_teTurbu.Bounds = new Rectangle(default, paiTerr.Size);
	m_teTurbu.BaseFrequency = new Point(uslTerrFrec.Value, uslTerrFrec.Value) / 500;
	m_teTurbu.Octaves = (int)uslTerrOctaves.Value; m_teTurbu.Seed = ntbTerrSeed.IntValue;
	m_teTurbu.Noise = chkTerrNoise.Checked; m_teTurbu.Stitchable = chkTerrStitch.Checked;
	paiTerr.Invalidate();
}
private void paiTerr_Paint(object sender, PaintArgs e)		{	if (m_teTurbu is not null)	e.Graphics.DrawImage(m_teTurbu, default);}
private void RevolDeshacer_Click(object sender)
{	if (m_egRevolGeo?.Primitives.Count > 0)	{	m_egRevolGeo.Primitives.RemoveLast(); m_egRevolGeo.Refresh(); paiRevol.Invalidate();}
}
private void RevolLimpiar_Click(object sender)
{	if (m_egRevolGeo?.Primitives.Count > 0)	{	m_egRevolGeo.Primitives.Clear(); m_egRevolGeo.Refresh(); paiRevol.Invalidate();}
}
private void RevolFondo_Click(object sender)
{	string? s;

try
{	s = mDialog.ShowOpenFile(mMod.FILTRO_IMG, mMod.DLG_GUID_IMG);	if (s is null) return;
	m_bmpRevol = new cBitmap(s); paiRevol.Invalidate();
} catch (System.Exception ex)
{	mDialog.MsgBoxExclamation(ex.Message, "Error");
}
}
private void RevolQuitaFondo_Click(object sender)			{	m_bmpRevol = null; paiRevol.Invalidate();}
private void paiRevol_Paint(object sender, PaintArgs e)
{	Rectangle rt;

	e.Graphics.Antialias = true;
	rt = paiRevol.ContentRectangle;
	if (m_bmpRevol is not null)	e.Graphics.DrawBitmap(m_bmpRevol, rt);		// Fondo
	e.Graphics.DrawLine(m_ptRevolOrig.X, m_ptRevolOrig.Y					// Eje
		, m_ptRevolOrig.X, rt.Height - m_ptRevolOrig.Y - m_ptRevolOrig.Y, eBrush.Red, 2);
	if (m_egRevolGeo is not null)	e.Graphics.DrawGeometry(m_egRevolGeo.Geometry, m_brRevol); // Geom
}
private void paiRevol_MouseDown(object sender, MouseArgs e)
{		if (e.Button != eMouseButton.Left)	return;
	m_ptRevolUlt = e.Location;
	m_egRevolGeo ??= new cEditableGeometry();
	if (m_egRevolGeo.Primitives.Count == 0)	m_egRevolGeo.StartPoint = m_ptRevolUlt;
	if (tbbRevolPto.Checked)
	{	m_egRevolGeo.Primitives.Add(new cEditableGeometry.cPoints(m_ptRevolUlt));
	} else if (tbbRevolLin.Checked)
	{	m_egRevolGeo.Primitives.Add(new cEditableGeometry.cLine(m_ptRevolUlt));
	} else if (tbbRevolArc.Checked)
	{	m_egRevolGeo.Primitives.Add(new cEditableGeometry.cArc(m_ptRevolUlt, uptRevolArcRadio.Value, ntbRevolArcAng.FloatValue, false, false));
	} else if (tbbRevolBezier.Checked)
	{	m_egRevolGeo.Primitives.Add(new cEditableGeometry.cBezier(m_ptRevolUlt));
	} else if (tbbRevolQbezier.Checked)
	{	m_egRevolGeo.Primitives.Add(new cEditableGeometry.cQuadraticBezier(m_ptRevolUlt));
	}
	m_egRevolGeo.Refresh(); paiRevol.Invalidate();
}
private void paiRevol_MouseMove(object sender, MouseArgs e)
{	m_ptRevolUlt = e.Location;
	lblRevolPos.Text = $"({m_ptRevolUlt.X - m_ptRevolOrig.X:0.##}; {m_ptRevolUlt.Y - m_ptRevolOrig.Y:0.##})";
		if (e.Button != eMouseButton.Left || m_egRevolGeo is null)	return;
	m_egRevolGeo.OnMouseDrag(m_ptRevolUlt); paiRevol.Invalidate();
}
private void paiRevol_KeyDown(object sender, ref KeyArgs e)
{	if ((e.Key == eKey.LeftControl || e.Key == eKey.RightControl) && m_egRevolGeo is not null)
		m_egRevolGeo.AddControlPoint(m_ptRevolUlt);
}
private void cbtRevol_Changed(object sender)
{	m_brRevol.Dispose(); m_brRevol = new cSolidBrush(cbtRevol.Color); paiRevol.Invalidate();
}
private void BezierAgrega_Click(object? sender)
{	int iFil = grdBezier.RowCount;

	grdBezier.RowCount += 4; grdBezier.Rows(iFil).BackColor = eBrush.LightYellow;
	grdBezier.Cells(iFil, 0).Type = typeof(bool); grdBezier.Cells(iFil, 0).ReadOnly = false; grdBezier[iFil, 0] = false;
}
private void BezierQuita_Click(object sender)
{	if (grdBezier.CurrentRow > 0)	grdBezier.RemoveRows(((grdBezier.CurrentRow - 1) / 4 * 4 + 1), 4);
}
private void BezierLimpia_Click(object sender)				{	grdBezier.RowCount = 1;}
private void BezierAbre_Click(object sender)
{	string? sArchi; string[] a_s; int iFil;

	sArchi = mDialog.ShowOpenFile(mMod.FILTRO_GRAB, mMod.DLG_GUID_BEZ);	if (sArchi is null) return;
	grdBezier.RowCount = 1; iFil = 1;										// Limpiar
try
{	a_s = System.IO.File.ReadAllLines(sArchi);
	foreach (string s in a_s)
	{	a_s = s.Split('\t', 17);	if (a_s.Length != 17)	continue;
		BezierAgrega_Click(null);
		_ = bool.TryParse(a_s[0], out bool b); grdBezier[iFil, 0] = b;
		for (int j = 0, k = 1; j < 4; j++, iFil++)
		{	for (int i = 1; i < 5; i++, k++)	{	_ = Vector.TryParseInvariant(a_s[k], out Vector v); grdBezier[iFil, i] = v;}
		}
	}
} catch (System.Exception ex)
{	mDialog.MsgBoxExclamation(ex.Message, "Error");
}
}
private void BezierGraba_Click(object sender)
{	string? sArchi;

		if (grdBezier.RowCount == 1)	return;
	sArchi = mDialog.ShowSaveFile(mMod.FILTRO_GRAB, mMod.DLG_GUID_BEZ);	if (sArchi is null) return;
try
{	using System.IO.StreamWriter stw = new (sArchi);

	for (int j = 1; j < grdBezier.RowCount; )
	{	stw.Write((bool)grdBezier[j, 0]!);
		for (int k = 0; k < 4; k++, j++)
		{	for (int i = 1; i < 5; i++)	{	object? o = grdBezier[j, i];	stw.Write('\t'); stw.Write(o is not null ? (Vector)o : default);}
		}
		stw.WriteLine();
	}
} catch (System.Exception ex)
{	mDialog.MsgBoxExclamation(ex.Message, "Error");
}
}
private void BezierTete_Click(object sender)
{	/*const*/ Patch[] a_pthTete =
		[	new() {	ReflejaZ = true, Inds = [new (-0.35f, 0.27f, 0), new (-0.35f, 0.27f, -0.196f), new (-0.196f, 0.27f, -0.35f), new (0, 0.27f, -0.35f), new (-0.334375f, 0.3028125f, 0), new (-0.334375f, 0.3028125f, -0.18725f), new (-0.18725f, 0.3028125f, -0.334375f), new (0, 0.3028125f, -0.334375f), new (-0.359375f, 0.3028125f, 0), new (-0.359375f, 0.3028125f, -0.20125f), new (-0.20125f, 0.3028125f, -0.359375f), new (0, 0.3028125f, -0.359375f), new (-0.375f, 0.27f, 0), new (-0.375f, 0.27f, -0.21f), new (-0.21f, 0.27f, -0.375f), new (0, 0.27f, -0.375f)]} // Rim
			, new() {	ReflejaZ = true, Inds = [new (-0.375f, 0.27f, 0), new (-0.375f, 0.27f, -0.21f), new (-0.21f, 0.27f, -0.375f), new (0, 0.27f, -0.375f), new (-0.4375f, 0.13875f, 0), new (-0.4375f, 0.13875f, -0.245f), new (-0.245f, 0.13875f, -0.4375f), new (0, 0.13875f, -0.4375f), new (-0.5f, 0.007499993f, 0), new (-0.5f, 0.007499993f, -0.28f), new (-0.28f, 0.007499993f, -0.5f), new (0, 0.007499993f, -0.5f), new (-0.5f, -0.105f, 0), new (-0.5f, -0.105f, -0.28f), new (-0.28f, -0.105f, -0.5f), new (0, -0.105f, -0.5f)]} // Body
			, new() {	ReflejaZ = true, Inds = [new (-0.5f, -0.105f, 0), new (-0.5f, -0.105f, -0.28f), new (-0.28f, -0.105f, -0.5f), new (0, -0.105f, -0.5f), new (-0.5f, -0.2175f, 0), new (-0.5f, -0.2175f, -0.28f), new (-0.28f, -0.2175f, -0.5f), new (0, -0.2175f, -0.5f), new (-0.375f, -0.27375f, 0), new (-0.375f, -0.27375f, -0.21f), new (-0.21f, -0.27375f, -0.375f), new (0, -0.27375f, -0.375f), new (-0.375f, -0.2925f, 0), new (-0.375f, -0.2925f, -0.21f), new (-0.21f, -0.2925f, -0.375f), new (0, -0.2925f, -0.375f)]}
			, new() {	ReflejaZ = true, Inds = [new (0, 0.4575f, 0), new (0, 0.4575f, 0), new (0, 0.4575f, 0), new (0, 0.4575f, 0), new (-0.2f, 0.4575f, 0), new (-0.2f, 0.4575f, -0.1125f), new (-0.1125f, 0.4575f, -0.2f), new (0, 0.4575f, -0.2f), new (0, 0.3825f, 0), new (0, 0.3825f, 0), new (0, 0.3825f, 0), new (0, 0.3825f, 0), new (-0.05f, 0.345f, 0), new (-0.05f, 0.345f, -0.028f), new (-0.028f, 0.345f, -0.05f), new (0, 0.345f, -0.05f)]} // Lid
			, new() {	ReflejaZ = true, Inds = [new (-0.05f, 0.345f, 0), new (-0.05f, 0.345f, -0.028f), new (-0.028f, 0.345f, -0.05f), new (0, 0.345f, -0.05f), new (-0.1f, 0.3075f, 0), new (-0.1f, 0.3075f, -0.056f), new (-0.056f, 0.3075f, -0.1f), new (0, 0.3075f, -0.1f), new (-0.325f, 0.3075f, 0), new (-0.325f, 0.3075f, -0.182f), new (-0.182f, 0.3075f, -0.325f), new (0, 0.3075f, -0.325f), new (-0.325f, 0.27f, 0), new (-0.325f, 0.27f, -0.182f), new (-0.182f, 0.27f, -0.325f), new (0, 0.27f, -0.325f)]}
			, new() {	ReflejaZ = false, Inds = [new (0, 0.2325f, 0.375f), new (-0.075f, 0.2325f, 0.375f), new (-0.075f, 0.17625f, 0.4f), new (0, 0.17625f, 0.4f), new (0, 0.2325f, 0.625f), new (-0.075f, 0.2325f, 0.625f), new (-0.075f, 0.17625f, 0.575f), new (0, 0.17625f, 0.575f), new (0, 0.2325f, 0.75f), new (-0.075f, 0.2325f, 0.75f), new (-0.075f, 0.17625f, 0.675f), new (0, 0.17625f, 0.675f), new (0, 0.12f, 0.75f), new (-0.075f, 0.12f, 0.75f), new (-0.075f, 0.12f, 0.675f), new (0, 0.12f, 0.675f)]} // Handle
			, new() {	ReflejaZ = false, Inds = [new (0, 0.12f, 0.75f), new (-0.075f, 0.12f, 0.75f), new (-0.075f, 0.12f, 0.675f), new (0, 0.12f, 0.675f), new (0, 0.007499993f, 0.75f), new (-0.075f, 0.007499993f, 0.75f), new (-0.075f, 0.06375f, 0.675f), new (0, 0.06375f, 0.675f), new (0, -0.09562501f, 0.6625f), new (-0.075f, -0.09562501f, 0.6625f), new (-0.075f, -0.04875001f, 0.625f), new (0, -0.04875001f, 0.625f), new (0, -0.18f, 0.475f), new (-0.075f, -0.18f, 0.475f), new (-0.075f, -0.105f, 0.5f), new (0, -0.105f, 0.5f)]}
			, new() {	ReflejaZ = false, Inds = [new (0, -0.18f, -0.425f), new (-0.165f, -0.18f, -0.425f), new (-0.165f, 0.02624997f, -0.425f), new (0, 0.02624997f, -0.425f), new (0, -0.12375f, -0.775f), new (-0.165f, -0.12375f, -0.775f), new (-0.165f, 0.02624997f, -0.65f), new (0, 0.02624997f, -0.65f), new (0, 0.17625f, -0.6f), new (-0.0625f, 0.17625f, -0.6f), new (-0.0625f, 0.195f, -0.575f), new (0, 0.195f, -0.575f), new (0, 0.27f, -0.825f), new (-0.0625f, 0.27f, -0.825f), new (-0.0625f, 0.27f, -0.675f), new (0, 0.27f, -0.675f)]} // Spout
			, new() {	ReflejaZ = false, Inds = [new (0, 0.27f, -0.825f), new (-0.0625f, 0.27f, -0.825f), new (-0.0625f, 0.27f, -0.675f), new (0, 0.27f, -0.675f), new (0, 0.2934375f, -0.88125f), new (-0.0625f, 0.2934375f, -0.88125f), new (-0.0625f, 0.28875f, -0.7f), new (0, 0.28875f, -0.7f), new (0, 0.298125f, -0.8625f), new (-0.0375f, 0.298125f, -0.8625f), new (-0.0375f, 0.28875f, -0.725f), new (0, 0.28875f, -0.725f), new (0, 0.27f, -0.8f), new (-0.0375f, 0.27f, -0.8f), new (-0.0375f, 0.27f, -0.7f), new (0, 0.27f, -0.7f)]}
			, new() {	ReflejaZ = true, Inds = [new (0, -0.33f, 0), new (0, -0.33f, 0), new (0, -0.33f, 0), new (0, -0.33f, 0), new (0, -0.33f, -0.35625f), new (-0.1995f, -0.33f, -0.35625f), new (-0.35625f, -0.33f, -0.1995f), new (-0.35625f, -0.33f, 0), new (0, -0.31125f, -0.375f), new (-0.21f, -0.31125f, -0.375f), new (-0.375f, -0.31125f, -0.21f), new (-0.375f, -0.31125f, 0), new (0, -0.2925f, -0.375f), new (-0.21f, -0.2925f, -0.375f), new (-0.375f, -0.2925f, -0.21f), new (-0.375f, -0.2925f, 0)]} // Bottom
		];
	int iFil;
	
	grdBezier.RowCount = 1; iFil = 1;										// Limpiar
	foreach (Patch pth in a_pthTete)
	{	BezierAgrega_Click(null);
		grdBezier[iFil, 0] = pth.ReflejaZ;
		for (int j = 0, k = 0; j < 4; j++, iFil++)
			for (int i = 1; i < 5; i++, k++)	grdBezier[iFil, i] = pth.Inds[k];
	}
}
private void FormaDeshacer_Click(object sender)
{	if (m_egFormaGeo?.Primitives.Count > 0)	{	m_egFormaGeo.Primitives.RemoveLast(); m_egFormaGeo.Refresh(); paiForma.Invalidate();}
}
private void FormaLimpiar_Click(object sender)
{	if (m_egFormaGeo?.Primitives.Count > 0)	{	m_egFormaGeo.Primitives.Clear(); m_egFormaGeo.Refresh(); paiForma.Invalidate();}
}
private void FormaFondo_Click(object sender)
{	string? s;

try
{	s = mDialog.ShowOpenFile(mMod.FILTRO_IMG, mMod.DLG_GUID_IMG);	if (s is null) return;
	m_bmpForma = new cBitmap(s); paiForma.Invalidate();
} catch (System.Exception ex)
{	mDialog.MsgBoxExclamation(ex.Message, "Error");
}
}
private void FormaQuitaFondo_Click(object sender)			{	m_bmpForma = null; paiForma.Invalidate();}
private void paiForma_Paint(object sender, PaintArgs e)
{	e.Graphics.Antialias = true;
	if (m_bmpForma is not null)		e.Graphics.DrawBitmap(m_bmpForma, paiForma.ContentRectangle); // Fondo
	if (m_egFormaGeo is not null)	e.Graphics.FillGeometry(m_egFormaGeo.Geometry, m_brForma); // Geom
}
private void paiForma_MouseDown(object sender, MouseArgs e)
{		if (e.Button != eMouseButton.Left)	return;
	m_ptFormaUlt = e.Location;
	m_egFormaGeo ??= new cEditableGeometry {	Closed = true, Filled = true};
	if (m_egFormaGeo.Primitives.Count == 0)	m_egFormaGeo.StartPoint = m_ptFormaUlt;
	if (tbbFormaPto.Checked)
	{	m_egFormaGeo.Primitives.Add(new cEditableGeometry.cPoints(m_ptFormaUlt));
	} else if (tbbFormaLin.Checked)
	{	m_egFormaGeo.Primitives.Add(new cEditableGeometry.cLine(m_ptFormaUlt));
	} else if (tbbFormaArc.Checked)
	{	m_egFormaGeo.Primitives.Add(new cEditableGeometry.cArc(m_ptFormaUlt, uptFormaArcRadio.Value, ntbFormaArcAng.FloatValue, false, false));
	} else if (tbbFormaBezier.Checked)
	{	m_egFormaGeo.Primitives.Add(new cEditableGeometry.cBezier(m_ptFormaUlt));
	} else if (tbbFormaQbezier.Checked)
	{	m_egFormaGeo.Primitives.Add(new cEditableGeometry.cQuadraticBezier(m_ptFormaUlt));
	}
	m_egFormaGeo.Refresh(); paiForma.Invalidate();
}
private void paiForma_MouseMove(object sender, MouseArgs e)
{	m_ptFormaUlt = e.Location;
	lblFormaPos.Text = $"({m_ptFormaUlt.X:0.##}; {m_ptFormaUlt.Y:0.##})";
		if (e.Button != eMouseButton.Left || m_egFormaGeo is null)	return;
	m_egFormaGeo.OnMouseDrag(m_ptFormaUlt); paiForma.Invalidate();
}
private void paiForma_KeyDown(object sender, ref KeyArgs e)
{	if ((e.Key == eKey.LeftControl || e.Key == eKey.RightControl) && m_egFormaGeo is not null)
		m_egFormaGeo.AddControlPoint(m_ptFormaUlt);
}
private void cbtForma_ColorChanged(object sender)
{	m_brForma.Dispose(); m_brForma = new cSolidBrush(cbtForma.Color); paiForma.Invalidate();
}
private void btnCharFnt_Click(object sender)
{	uFont ufn;

	ufn = new uFont {	Value = m_fntChars};	if (ufn.ShowDialog())	m_ChFont = ufn.Value;
}
private void btnChGli_Click(object sender)
{	uGlyphs ugl;

	ugl = new uGlyphs {	Font = m_fntChars, Glyph = m_shChGli};	if (!ugl.ShowDialog())	return;
	gctCharGli.Geometry = ugl.GetGeometry(); m_ChFont = ugl.Font; m_shChGli = ugl.Glyph;
}
private cModelo? m_mdlCreaEsfera()
{	float fAngPrecisH, fAngPrecisV, fH, fHFin, fV, fVIni, fVFin, fRadio, x, z, fSen, fCos;
	Vertex[] a_vx; int[] a_i; int iVert, iIdx, iCantMerids, iCantParals, iVertSigFila;

	// ** Validar
	fAngPrecisH = ntbEsfeAngPrecH.FloatValue;	if (fAngPrecisH is < 0.01f or > 90)	{	mDialog.MsgBoxExclamation("Horizontal precision angle must be between 0.01 and 90", "Create sphere"); ntbEsfeAngPrecH.Focus(); return null;}
	fAngPrecisV = ntbEsfeAngPrecV.FloatValue;	if (fAngPrecisV is < 0.01f or > 90)	{	mDialog.MsgBoxExclamation("Vertical precision angle must be between 0.01 and 90", "Create sphere"); ntbEsfeAngPrecV.Focus(); return null;}
	fH = ntbEsfeAngFinH.FloatValue - ntbEsfeAngIniH.FloatValue;	if (ntbEsfeAngIniH.FloatValue < 0 || ntbEsfeAngFinH.FloatValue < 0 || fH < 0.01f || fH > 360)	{	mDialog.MsgBoxExclamation("Horizontal angles must be between 0.01 and 360", "Create sphere"); ntbEsfeAngIniH.Focus(); return null;}
	fV = ntbEsfeAngFinV.FloatValue - ntbEsfeAngIniV.FloatValue;	if (ntbEsfeAngIniV.FloatValue < 0 || ntbEsfeAngFinV.FloatValue < 0 || fV < 0.01f || fV > 180)	{	mDialog.MsgBoxExclamation("Vertical angles must be between 0.01 and 180", "Create sphere"); ntbEsfeAngIniV.Focus(); return null;}
	fRadio = ntbEsfeRadio.FloatValue;	if (fRadio < 0.01f)	{	mDialog.MsgBoxExclamation("Radius must be greater than 0.01", "Create sphere"); ntbEsfeRadio.Focus(); return null;}
	// ** Calc tams
	iCantMerids = mMath.Ceil(fH / fAngPrecisH); iCantParals = mMath.Ceil(fV / fAngPrecisV);
	fAngPrecisH = fAngPrecisH.ToRadians(); fAngPrecisV = fAngPrecisV.ToRadians(); // Conv a rads
	a_vx = new Vertex[(iCantMerids + 1) * (iCantParals + 1)]; a_i = new int[iCantMerids * iCantParals * 6]; iVert = 0; iIdx = 0;
	// ** Gen meridianos
	fH = ntbEsfeAngIniH.FloatValue.ToRadians(); fHFin = ntbEsfeAngFinH.FloatValue.ToRadians(); // Conv a rads
	fVIni = ntbEsfeAngIniV.FloatValue.ToRadians(); fVFin = ntbEsfeAngFinV.FloatValue.ToRadians();
	for (int i = 0; i <= iCantMerids; i++, fH += fAngPrecisH)
	{	if (i == iCantMerids)	fH = fHFin;									// Ult merid: evitar exceso; el ult merid sí puede quedar incompleto
		(z, x) = MathF.SinCos(fH); fV = fVIni;
		// ** Gen meridiano: crear paralelos
		for (int j = 0; j <= iCantParals; j++, fV += fAngPrecisV)
		{	if (j == iCantParals)											// Ult paral: evitar exceso; el ult paralelo sí puede quedar incompleto
			{	fV = fVFin;
			// ** Agregar idx (excepto para el ult meridiano y el ult paralelo)
			} else if (i < iCantMerids)
			{	iVertSigFila = iVert + iCantParals + 1;
				a_i[iIdx++] = iVert; a_i[iIdx++] = iVertSigFila; a_i[iIdx++] = iVert + 1;
				a_i[iIdx++] = iVert + 1; a_i[iIdx++] = iVertSigFila; a_i[iIdx++] = iVertSigFila + 1;
			}
			// ** Agregar vert
			(fSen, fCos) = MathF.SinCos(fV);
			a_vx[iVert].Normal = new Vector(fSen * x, fCos, fSen * z); a_vx[iVert].Position.Add(a_vx[iVert].Normal, fRadio); // Pos = normal * radio
			a_vx[iVert].Texcoord = new Point((float)i / iCantMerids, (float)j / iCantParals);
			iVert++;
		}
	}
	return new cModelo(a_vx, iVert, a_i, iIdx);
}
private cModelo? m_mdlCreaCilindro()
{	float fAngPrecis, f, fFin, fRadio, fAlto, fTvSup, fTvInf;
	Vertex[] a_vx; int[] a_i; Vertex vx = default; int iVert, iIdx, i, iVertAnt, iCantMerids, iVertSigFila;

	// ** Validar
	fAngPrecis = ntbCilAngPrec.FloatValue;	if (fAngPrecis is < 0.01f or > 90)	{	mDialog.MsgBoxExclamation("Precision angle must be between 0.01 and 90", "Create cylinder"); ntbCilAngPrec.Focus(); return null;}
	f = ntbCilAngFin.FloatValue - ntbCilAngIni.FloatValue;	if (ntbCilAngIni.FloatValue < 0 || ntbCilAngFin.FloatValue < 0 || f < 0.01f || f > 360)	{	mDialog.MsgBoxExclamation("Angles must be between 0.01 and 360", "Create cylinder"); ntbCilAngIni.Focus(); return null;}
	fRadio = ntbCilRadio.FloatValue;	if (fRadio < 0.01f)	{	mDialog.MsgBoxExclamation("Radius must be greater than 0.01", "Create cylinder"); ntbCilRadio.Focus(); return null;}
	fAlto = ntbCilAlto.FloatValue;	if (fAlto < 0.01f)	{	mDialog.MsgBoxExclamation("Height must be greater than 0.01", "Create cylinder"); ntbCilAlto.Focus(); return null;}
	// ** Calc tams
	iCantMerids = mMath.Ceil(f / fAngPrecis);
	fAngPrecis = fAngPrecis.ToRadians();								// Conv a rads
	iVert = 2; iIdx = 6; f = fAlto;
		if (chkCilTapaSup.Checked)	{	iVert++; iIdx += 6; f += fRadio;}
		if (chkCilTapaInf.Checked)	{	iVert++; iIdx += 6; f += fRadio;}
		iVert *= iCantMerids + 1; iIdx *= iCantMerids;
		fTvSup = (chkCilTapaSup.Checked ? fRadio / f : 0); fTvInf = (chkCilTapaInf.Checked ? 1 - fRadio / f : 1);
	a_vx = new Vertex[iVert]; a_i = new int[iIdx]; iVert = 0; iIdx = 0;
	// ** Gen meridianos
	f = ntbCilAngIni.FloatValue.ToRadians(); fFin = ntbCilAngFin.FloatValue.ToRadians(); // Conv a rads
	for (i = 0; i <= iCantMerids; i++, f += fAngPrecis)
	{	if (i == iCantMerids)												// Ult merid: evitar exceso; el ult merid sí puede quedar incompleto
		{	f = fFin;
		// ** Agregar idx (excepto para el ult meridiano)
		} else
		{	iVertSigFila = iVert + 2;
			a_i[iIdx++] = iVert; a_i[iIdx++] = iVertSigFila; a_i[iIdx++] = iVert + 1; // Pared
			a_i[iIdx++] = iVert + 1; a_i[iIdx++] = iVertSigFila; a_i[iIdx++] = iVertSigFila + 1;
		}
		// ** Agregar verts
		(float fSen, float fCos) = MathF.SinCos(f);
		a_vx[iVert].Normal = new Vector(fCos, 0, fSen);
		a_vx[iVert].Position.Add(a_vx[iVert].Normal, fRadio); a_vx[iVert].Position.Y = fAlto; // Pos = normal * radio
		a_vx[iVert].Texcoord = new Point((float)i / iCantMerids, fTvSup);
		iVert++;
		///precis
		a_vx[iVert] = a_vx[iVert - 1]; a_vx[iVert].Position.Y = 0; a_vx[iVert].Texcoord.Y = fTvInf;
		iVert++;
	}
	// ** Gen tapa sup
	if (chkCilTapaSup.Checked)
	{	iVertAnt = 0; vx.Normal = new Vector(0, 1, 0); vx.Position = new Vector(0, fAlto, 0);
		for (i = 0; i <= iCantMerids; i++)
		{	// ** Agregar idx
			if (i < iCantMerids)
			{	a_i[iIdx++] = iVert; a_i[iIdx++] = iVert + 1; a_i[iIdx++] = iVertAnt;
				a_i[iIdx++] = iVertAnt; a_i[iIdx++] = iVert + 1; a_i[iIdx++] = iVertAnt + 2;
			}
			// ** Agregar vert
			a_vx[iVert] = vx; a_vx[iVert].Texcoord.X = a_vx[iVertAnt].Texcoord.X;//¿
			iVert++; iVertAnt += 2;
		}
	}
	// ** Gen tapa inf
	if (chkCilTapaInf.Checked)
	{	iVertAnt = 1; vx.Normal = new Vector(0, -1, 0); vx.Position = new Vector(0, 0, 0); vx.Texcoord.Y = 1; 
		for (i = 0; i <= iCantMerids; i++)
		{	// ** Agregar idx
			if (i < iCantMerids)
			{	a_i[iIdx++] = iVertAnt; a_i[iIdx++] = iVertAnt + 2; a_i[iIdx++] = iVert;
				a_i[iIdx++] = iVert; a_i[iIdx++] = iVertAnt + 2; a_i[iIdx++] = iVert + 1;
			}
			// ** Agregar vert
			a_vx[iVert] = vx; a_vx[iVert].Texcoord.X = a_vx[iVertAnt].Texcoord.X;///
			iVert++; iVertAnt += 2;
		}
	}
	return new cModelo(a_vx, iVert, a_i, iIdx);
}
private cModelo? m_mdlCreaCubo()
{	float fPrecisH, fPrecisV, fPrecisF, fAncho, fAlto, fFondo, fPerimHTot, fPerimVTot, fPerimFrente, fPerimHTapa, fPerimVTapa, fPerim;
	Vertex[] a_vx; int[] a_i; int iVert, iIdx, iCantCols, iCantFils, iCantColsF;

	// ** Validar
	fPrecisH = ntbCubPrecH.FloatValue; fPrecisV = ntbCubPrecV.FloatValue; fPrecisF = ntbCubPrecF.FloatValue;
		if (fPrecisH < 0.01f || fPrecisV < 0.01f || fPrecisF < 0.01f)	{	mDialog.MsgBoxExclamation("Precision must be greater than 0.01", "Create cube"); ntbCubPrecH.Focus(); return null;}
	fAncho = ntbCubAncho.FloatValue; fAlto = ntbCubAlto.FloatValue; fFondo = ntbCubFondo.FloatValue;
		if (fAncho < 0.01f || fAlto < 0.01f || fFondo < 0.01f)	{	mDialog.MsgBoxExclamation("Size must be greater than 0.01", "Create cube"); ntbCubAncho.Focus(); return null;}
	if (!chkDer.Checked && !chkFrente.Checked && !chkIzq.Checked && !chkFondo.Checked)	{	mDialog.MsgBoxExclamation("Select at least one wall", "Create cube"); chkDer.Focus(); return null;}
	// ** Calc tams
	iCantCols = mMath.Ceil(fAncho / fPrecisH); iCantFils = mMath.Ceil(fAlto / fPrecisV);
		iCantColsF = mMath.Ceil(fFondo / fPrecisF);
	iVert = 0; iIdx = 0; fPerimHTot = 0; fPerimVTot = fAlto; fPerimVTapa = 0;
	if (chkDer.Checked)
	{	iVert += (iCantColsF + 1) * (iCantFils + 1); iIdx += iCantColsF * iCantFils * 6; fPerimHTot += fFondo;
	}
	fPerimFrente = fPerimHTapa = fPerimHTot;
	if (chkFrente.Checked)
	{	iVert += (iCantCols + 1) * (iCantFils + 1); iIdx += iCantCols * iCantFils * 6; fPerimHTot +=fAncho;
	}
	if (chkIzq.Checked)
	{	iVert += (iCantColsF + 1) * (iCantFils + 1); iIdx += iCantColsF * iCantFils * 6; fPerimHTot += fFondo;
	}
	if (chkFondo.Checked)
	{	iVert += (iCantCols + 1) * (iCantFils + 1); iIdx += iCantCols * iCantFils * 6; fPerimHTot += fAncho;
	}
	if (chkTapa.Checked)
	{	iVert += (iCantCols + 1) * (iCantColsF + 1); iIdx += iCantCols * iCantColsF * 6; fPerimVTapa = fFondo; fPerimVTot += fFondo;
		fPerimHTapa += fAncho;
	}
	if (chkBase.Checked)
	{	iVert += (iCantCols + 1) * (iCantColsF + 1); iIdx += iCantCols * iCantColsF * 6; fPerimVTapa = fFondo;
		if (!chkTapa.Checked)	fPerimVTot += fFondo;
		fPerimHTapa += fAncho;
	}
	if (fPerimHTot < fPerimHTapa)	fPerimHTot = fPerimHTapa;
	a_vx = new Vertex[iVert]; a_i = new int[iIdx]; iVert = 0; iIdx = 0;
	fPerim = 0;
	// ** Gen der
	if (chkDer.Checked)
	{	s_CreaPlano(iCantColsF, iCantFils
			, new Vector(fAncho, fAlto, -fFondo), new Vector(0, -fAlto, 0), new Vector(fAncho, fAlto, fFondo)
			, new Vector(0, 0, fPrecisF), new Vector(0, -fPrecisV, 0), new Vector(1, 0, 0), new Vector(0, 0, -1)
			, fPerim, fFondo, fPrecisF, fPerimHTot, fPerimVTapa, fAlto, fPrecisV, fPerimVTot, a_vx, a_i, ref iVert, ref iIdx);
		fPerim += fFondo;
	}
	// ** Gen frente
	if (chkFrente.Checked)
	{	s_CreaPlano(iCantCols, iCantFils
			, new Vector(fAncho, fAlto, fFondo), new Vector(0, -fAlto, 0), new Vector(-fAncho, fAlto, fFondo)
			, new Vector(-fPrecisH, 0, 0), new Vector(0, -fPrecisV, 0), new Vector(0, 0, 1), new Vector(1, 0, 0)
			, fPerim, fAncho, fPrecisH, fPerimHTot, fPerimVTapa, fAlto, fPrecisV, fPerimVTot, a_vx, a_i, ref iVert, ref iIdx);
		fPerim += fAncho;
	}
	// ** Gen izq
	if (chkIzq.Checked)
	{	s_CreaPlano(iCantColsF, iCantFils
			, new Vector(-fAncho, fAlto, fFondo), new Vector(0, -fAlto, 0), new Vector(-fAncho, fAlto, -fFondo)
			, new Vector(0, 0, -fPrecisF), new Vector(0, -fPrecisV, 0), new Vector(-1, 0, 0), new Vector(0, 0, 1)
			, fPerim, fFondo, fPrecisF, fPerimHTot, fPerimVTapa, fAlto, fPrecisV, fPerimVTot, a_vx, a_i, ref iVert, ref iIdx);
		fPerim += fFondo;
	}
	// ** Gen fondo
	if (chkFondo.Checked)
	{	s_CreaPlano(iCantCols, iCantFils
			, new Vector(-fAncho, fAlto, -fFondo), new Vector(0, -fAlto, 0), new Vector(fAncho, fAlto, -fFondo)
			, new Vector(fPrecisH, 0, 0), new Vector(0, -fPrecisV, 0), new Vector(0, 0, -1), new Vector(-1, 0, 0)
			, fPerim, fAncho, fPrecisH, fPerimHTot, fPerimVTapa, fAlto, fPrecisV, fPerimVTot, a_vx, a_i, ref iVert, ref iIdx);
		//fPerim += fAncho;
	}
	// ** Gen tapa
	if (chkTapa.Checked)
	{	s_CreaPlano(iCantCols, iCantColsF
			, new Vector(fAncho, fAlto, -fFondo), new Vector(0, 0, fFondo), new Vector(-fAncho, fAlto, -fFondo)
			, new Vector(-fPrecisH, 0, 0), new Vector(0, 0, fPrecisF), new Vector(0, 1, 0), new Vector(1, 0, 0)
			, fPerimFrente, fAncho, fPrecisH, fPerimHTot, 0, fFondo, fPrecisF, fPerimVTot, a_vx, a_i, ref iVert, ref iIdx);
		fPerimFrente += fAncho;
	}
	// ** Gen base
	if (chkBase.Checked)
	{	s_CreaPlano(iCantCols, iCantColsF
			, new Vector(fAncho, -fAlto, fFondo), new Vector(0, 0, -fFondo), new Vector(-fAncho, -fAlto, fFondo)
			, new Vector(-fPrecisH, 0, 0), new Vector(0, 0, -fPrecisF), new Vector(0, -1, 0), new Vector(1, 0, 0)
			, fPerimFrente, fAncho, fPrecisH, fPerimHTot, 0, fFondo, fPrecisF, fPerimVTot, a_vx, a_i, ref iVert, ref iIdx);
	}
	return new cModelo(a_vx, iVert, a_i, iIdx);
}
private cModelo? m_mdlCreaPlano()
{	float fPrecisH, fPrecisF, fAncho, fFondo;
	Vertex[] a_vx; int[] a_i; int iVert, iIdx, iCantCols, iCantColsF;

	// ** Validar
	fPrecisH = ntbPlaPrecH.FloatValue; fPrecisF = ntbPlaPrecF.FloatValue;
		if (fPrecisH < 0.01f || fPrecisF < 0.01f)	{	mDialog.MsgBoxExclamation("Precision must be greater than 0.01", "Create plane"); ntbPlaPrecH.Focus(); return null;}
	fAncho = ntbPlaAncho.FloatValue; fFondo = ntbPlaFondo.FloatValue;
		if (fAncho < 0.01f || fFondo < 0.01f)	{	mDialog.MsgBoxExclamation("Size must be greater than 0.01", "Create plane"); ntbPlaAncho.Focus(); return null;}
	// ** Calc tams
	iCantCols = mMath.Ceil(fAncho / fPrecisH); iCantColsF = mMath.Ceil(fFondo / fPrecisF);
	a_vx = new Vertex[(iCantCols + 1) * (iCantColsF + 1)]; a_i = new int[iCantCols * iCantColsF * 6]; iVert = 0; iIdx = 0;
	// ** Gen
	s_CreaPlano(iCantCols, iCantColsF
		, new Vector(fAncho, 0, -fFondo), new Vector(0, 0, fFondo), new Vector(-fAncho, 0, -fFondo)
		, new Vector(-fPrecisH, 0, 0), new Vector(0, 0, fPrecisF), new Vector(0, 1, 0), new Vector(1, 0, 0)
		, 0, fAncho, fPrecisH, fAncho, 0, fFondo, fPrecisF, fFondo, a_vx, a_i, ref iVert, ref iIdx);
	return new cModelo(a_vx, iVert, a_i, iIdx);
}
private cModelo? m_mdlCreaCírculo()
{	float fAngPrecis, f, fFin, fRadio, fSen, fCos; Vector vNormal;
	Vertex[] a_vx; int[] a_i; int iVert, iIdx, iCantPtos;

	// ** Validar
	fAngPrecis = ntbCirAngPrec.FloatValue;	if (fAngPrecis is < 0.01f or > 90)	{	mDialog.MsgBoxExclamation("Precision angle must be between 0.01 and 90", "Create circle"); ntbCirAngPrec.Focus(); return null;}
	f = ntbCirAngFin.FloatValue - ntbCirAngIni.FloatValue;	if (ntbCirAngIni.FloatValue < 0 || ntbCirAngFin.FloatValue < 0 || f < 0.01f || f > 360)	{	mDialog.MsgBoxExclamation("Angles must be between 0.01 and 360", "Create circle"); ntbCirAngIni.Focus(); return null;}
	fRadio = ntbCirRadio.FloatValue;	if (fRadio < 0.01f)	{	mDialog.MsgBoxExclamation("Radius must be greater than 0.01", "Create circle"); ntbCirRadio.Focus(); return null;}
	// ** Calc tams
	iCantPtos = mMath.Ceil(f / fAngPrecis);
	fAngPrecis = fAngPrecis.ToRadians();								// Conv a rads
	a_vx = new Vertex[iCantPtos + 2]; a_i = new int[iCantPtos * 3]; iVert = 0; iIdx = 0;
	// ** Gen
	f = ntbCirAngIni.FloatValue.ToRadians(); fFin = ntbCirAngFin.FloatValue.ToRadians(); // Conv a rads
	vNormal = new Vector(0, 1, 0);
	a_vx[iVert].Position = new Vector(0, 0, 0); a_vx[iVert].Normal = vNormal; a_vx[iVert].Texcoord = new Point(0.5f, 0.5f); // ** Centro
		iVert++;
	for (int i = 0; i <= iCantPtos; i++, f += fAngPrecis)
	{	if (i == iCantPtos)													// Ult punto: evitar exceso; el ult punto sí puede quedar incompleto
		{	f = fFin;
		// ** Agregar idx (excepto para el ult punto)
		} else
		{	a_i[iIdx++] = iVert; a_i[iIdx++] = 0; a_i[iIdx++] = iVert + 1;
		}
		// ** Agregar vert
		(fSen, fCos) = MathF.SinCos(f);
		a_vx[iVert].Position = new Vector(fCos * fRadio, 0, fSen * fRadio); a_vx[iVert].Normal = vNormal;
		a_vx[iVert].Texcoord = new Point(-fCos / 2 + 0.5f, fSen / 2 + 0.5f);
		iVert++;
	}
	return new cModelo(a_vx, iVert, a_i, iIdx);
}
private cModelo? m_mdlCreaRevol()
{	float fAngPrecisH, fPrecisV, fH, fHFin, fV, fVFin, fEscala, fMaxY, fSen, fCos; Point[, ] aa_pt;
	Vertex[] a_vx; int[] a_i; int iVert, iIdx, iCantMerids, iCantParals, iVertSigFila, j;

	// ** Validar
	if (m_egRevolGeo is null || m_egRevolGeo.Primitives.Count == 0)	{	mDialog.MsgBoxExclamation("Add geometry", "Create revolution"); paiRevol.Focus(); return null;}
	fAngPrecisH = ntbRevolAngPrecH.FloatValue;	if (fAngPrecisH is < 0.01f or > 90)	{	mDialog.MsgBoxExclamation("Horizontal precision angle must be between 0.01 and 90", "Create revolution"); ntbRevolAngPrecH.Focus(); return null;}
	fPrecisV = ntbRevolPrecV.FloatValue;	if (fPrecisV is < 0.01f or > 200)	{	mDialog.MsgBoxExclamation("Vertical precision must be between 0.01 and 200", "Create revolution"); ntbRevolPrecV.Focus(); return null;}
	fH = ntbRevolAngFinH.FloatValue - ntbRevolAngIniH.FloatValue;	if (ntbRevolAngIniH.FloatValue < 0 || ntbRevolAngFinH.FloatValue < 0 || fH < 0.01f || fH > 360)	{	mDialog.MsgBoxExclamation("Horizontal angles must be between 0.01 and 360", "Create revolution"); ntbRevolAngIniH.Focus(); return null;}
	fEscala = ntbRevolEscala.FloatValue;	if (fEscala is < 0.001f or > 1000)	{	mDialog.MsgBoxExclamation("Scale must be between 0.001 and 1000", "Create revolution"); ntbRevolEscala.Focus(); return null;}
	// ** Calc tams
	fVFin = m_egRevolGeo.Geometry.GetLength();
	iCantMerids = mMath.Ceil(fH / fAngPrecisH); iCantParals = mMath.Ceil(fVFin / fPrecisV);
	fAngPrecisH = fAngPrecisH.ToRadians();								// Conv a rads
	a_vx = new Vertex[(iCantMerids + 1) * (iCantParals + 1)]; a_i = new int[iCantMerids * iCantParals * 6]; iVert = 0; iIdx = 0;
	// ** Tomar pts del polig
	aa_pt = new Point[2, iCantParals + 1]; fV = 0; fMaxY = 0;
	for (j = 0; j <= iCantParals; j++, fV += fPrecisV)
	{	if (j == iCantParals)	fV = fVFin;									// Ult paral: evitar exceso; el ult paralelo sí puede quedar incompleto
		aa_pt[0, j] = (m_egRevolGeo.Geometry.GetPointAndTangentAt(fV, out aa_pt[1, j]) - m_ptRevolOrig) * fEscala;
		aa_pt[1, j] = new Point(aa_pt[1, j].Y, aa_pt[1, j].X);			// Rot tang e invertir Y
		if (aa_pt[0, j].Y > fMaxY)	fMaxY = aa_pt[0, j].Y;					// Tomar max Y
	}
	for (j = 0; j <= iCantParals; j++)	aa_pt[0, j].Y = fMaxY - aa_pt[0, j].Y; // Invertir Y de ptos
	// ** Gen meridianos
	fH = ntbRevolAngIniH.FloatValue.ToRadians(); fHFin = ntbRevolAngFinH.FloatValue.ToRadians(); // Conv a rads
	for (int i = 0; i <= iCantMerids; i++, fH += fAngPrecisH)
	{	if (i == iCantMerids)	fH = fHFin;									// Ult merid: evitar exceso; el ult merid sí puede quedar incompleto
		(fSen, fCos) = MathF.SinCos(fH);
		// ** Gen meridiano: crear paralelos
		for (j = 0; j <= iCantParals; j++)
		{	// ** Agregar idx (excepto para el ult meridiano y el ult paralelo)
			if (j < iCantParals && i < iCantMerids)
			{	iVertSigFila = iVert + iCantParals + 1;
				a_i[iIdx++] = iVert; a_i[iIdx++] = iVertSigFila; a_i[iIdx++] = iVert + 1;
				a_i[iIdx++] = iVert + 1; a_i[iIdx++] = iVertSigFila; a_i[iIdx++] = iVertSigFila + 1;
			}
			// ** Agregar vert
			a_vx[iVert].Position = new Vector(aa_pt[0, j].X * fCos, aa_pt[0, j].Y, aa_pt[0, j].X * fSen);
			a_vx[iVert].Normal = new Vector(aa_pt[1, j].X * fCos, aa_pt[1, j].Y, aa_pt[1, j].X * fSen);
			a_vx[iVert].Texcoord = new Point((float)i / iCantMerids, (float)j / iCantParals);
			iVert++;
		}
	}
	return new cModelo(a_vx, iVert, a_i, iIdx);
}
private cModelo? m_mdlCreaBezier()
{	Vector[] a_vPatch = new Vector[16];
	int iPrecis, j;
	Array<Vertex> a_vtxVerts = default; Array<int> a_iInds = default; Vector vEscala, vEscalaRefleX, vEscalaRefleZ, vEscalaRefleXZ;

	// ** Validar
		if (grdBezier.RowCount == 1)	{	mDialog.MsgBoxExclamation("Enter patches", "Create bezier"); grdBezier.Focus(); return null;}
	iPrecis = Math.Min(ntbTesel.IntValue, 64); vEscala = uvBezierEscala.Value;
	// ** Contar pts e inds
	int iVertsPorPat, iIndsPorPat;

	if (chkBezierVerts.Checked)
	{	iVertsPorPat = (iPrecis + 1) * (iPrecis + 1); iIndsPorPat = iPrecis * iPrecis * 6;
	} else
	{	iVertsPorPat = 16; iIndsPorPat = 16;
	}
	for (j = 1; j < grdBezier.RowCount; j += 4)
	{	a_vtxVerts.MinimumCapacity += iVertsPorPat * 2; a_iInds.MinimumCapacity += iIndsPorPat * 2; // Reflejado en X
		if ((bool)grdBezier[j, 0]!)											// Reflejado en Z
		{	a_vtxVerts.MinimumCapacity += iVertsPorPat * 2; a_iInds.MinimumCapacity += iIndsPorPat * 2;
		}
	}
	// ** Crear verts
	vEscalaRefleX = new Vector(-vEscala.X, vEscala.Y, vEscala.Z);
	vEscalaRefleZ = new Vector(vEscala.X, vEscala.Y, -vEscala.Z); vEscalaRefleXZ = new Vector(-vEscala.X, vEscala.Y, -vEscala.Z);
	for (j = 1; j < grdBezier.RowCount; j += 4)
	{	i_TeselPatch(j, vEscala, false); i_TeselPatch(j, vEscalaRefleX, true);
		if ((bool)grdBezier[j, 0]!)	{	i_TeselPatch(j, vEscalaRefleZ, true); i_TeselPatch(j, vEscalaRefleXZ, false);}
	}
	// ** Crear mdl: salir
	cModelo mdl = new (a_vtxVerts.Data!, a_vtxVerts.Count, a_iInds.Data!, a_iInds.Count);
		if (!chkBezierVerts.Checked)
		{	mdl.Topology = eVertexTopology.Patch16; mdl.Tessellation = iPrecis;
			mdl.VertexShader = mMod.MainWnd.VSBezier; mdl.HullShader = mMod.MainWnd.HSBezier; mdl.DomainShader = mMod.MainWnd.DSBezier;
		}
	return mdl;
// ** Submets
	void i_TeselPatch(int iFil, Vector vEscala, bool bRefle)
	{	int j, i, k, iVerts; float fU, fV;

		iVerts = a_vtxVerts.Count;
		// ** Aplicar escala y reflejo
		for (j = 0, k = 0; j < 4; j++)
		{	for (i = 1; i < 5; i++, k++)
			{	object? o = grdBezier[iFil + j, i];	a_vPatch[k] = (o is not null ? (Vector)o : default) * vEscala;
			}
		}
		// ** Creat verts (no ctlpts)
		if (chkBezierVerts.Checked)
		{	// ** Agregar verts
			for (j = 0; j <= iPrecis; j++)
			{	fV = j / (float)iPrecis;
				for (i = 0; i <= iPrecis; i++)
				{	Vertex vtx; Quaternion qU, qV; Vector vBinorm;

					fU = i / (float)iPrecis; qU = is_qBernsteinBasis(fU); qV = is_qBernsteinBasis(fV);
					// ** Calc pos
					vtx.Position = is_vCubicInterpol(a_vPatch, qU, qV);
					// ** Calc tang y norm
					vtx.Tangent = is_vCubicInterpol(a_vPatch, is_qCubicTangent(fU), qV);
						vBinorm = is_vCubicInterpol(a_vPatch, qU, is_qCubicTangent(fV));
					vtx.Normal = vtx.Tangent.Cross(vBinorm);
					if (!vtx.Normal.IsNear0)
					{	vtx.Normal.Normalize();	if (bRefle)	vtx.Normal *= -1;
					} else													// Pts incorrectos simular una normal
					{	vtx.Normal = new Vector(0, (vtx.Position.Y >= 0 ? 1 : -1), 0);
					}
					// ** Calc tex
					vtx.Texcoord = new Point((!bRefle ? fU : 1 - fU), fV);
					// ** Agregar vtx
					a_vtxVerts.Add(vtx);
				}
			}
			// ** Agregar inds
			for (j = 0; j < iPrecis; j++)
				for (i = 0; i < iPrecis; i++)
				{	int iIndA, iIndB, iIndC, iIndD;

					iIndA = iVerts + j * (iPrecis + 1) + i; iIndB = iIndA + 1;
					iIndC = iIndA + (iPrecis + 1); iIndD = iIndC + 1;
					if (!bRefle)
					{	a_iInds.Add(iIndA); a_iInds.Add(iIndB); a_iInds.Add(iIndD);
						a_iInds.Add(iIndA); a_iInds.Add(iIndD); a_iInds.Add(iIndC);
					} else
					{	a_iInds.Add(iIndD); a_iInds.Add(iIndB); a_iInds.Add(iIndA);
						a_iInds.Add(iIndC); a_iInds.Add(iIndD); a_iInds.Add(iIndA);
					}
				}
		// ** Creat ctlpts
		} else
		{	// ** Agregar verts
			for (k = 0; k < 16; k++)	a_vtxVerts.Add(new (a_vPatch[k], default, default, default));
			// ** Agregar inds
			for (j = 0; j < 4; j++)
				if (!bRefle)
					for (i = 0; i < 4; i++)		a_iInds.Add(iVerts + j * 4 + i);
				else
					for (i = 3; i >= 0; i--)	a_iInds.Add(iVerts + j * 4 + i);
		}
	}
	static Vector is_vCubicInterpol(Vector[] a_vPatch, Quaternion qU, Quaternion qV)
		=>	(a_vPatch[0]  * qU.X + a_vPatch[1]  * qU.Y + a_vPatch[2]  * qU.Z + a_vPatch[3]  * qU.W) * qV.X
			+  (a_vPatch[4]  * qU.X + a_vPatch[5]  * qU.Y + a_vPatch[6]  * qU.Z + a_vPatch[7]  * qU.W) * qV.Y
			+  (a_vPatch[8]  * qU.X + a_vPatch[9]  * qU.Y + a_vPatch[10] * qU.Z + a_vPatch[11] * qU.W) * qV.Z
			+  (a_vPatch[12] * qU.X + a_vPatch[13] * qU.Y + a_vPatch[14] * qU.Z + a_vPatch[15] * qU.W) * qV.W;
	static Quaternion is_qBernsteinBasis(float fFrac)
	{	float fComp = (1 - fFrac);

		return new Quaternion((fComp * fComp * fComp), (3 * fFrac * fComp * fComp), (3 * fFrac * fFrac * fComp), (fFrac * fFrac * fFrac));
	}
	static Quaternion is_qCubicTangent(float fFrac)
	{	float fFracCuad = fFrac * fFrac;
		return new Quaternion((-1 + 2 * fFrac - fFracCuad), (1 - 4 * fFrac + 3 * fFracCuad), (2 * fFrac - 3 * fFracCuad), fFracCuad);
	}
}
private cModelo? m_mdlCreaForma()
{	Vertex[] a_ver; float fEscala; byte[] a_by; int[] a_i; int iVert; Matrix4x4 m44;

	// ** Validar
	if (m_egFormaGeo is null || m_egFormaGeo.Primitives.Count == 0)	{	mDialog.MsgBoxExclamation("Add geometry", "Create shape"); paiForma.Focus(); return null;}
	fEscala = ntbFormaEscala.FloatValue;	if (fEscala is < 0.001f or > 1000)	{	mDialog.MsgBoxExclamation("Scale must be between 0.001 and 1000", "Create shape"); ntbFormaEscala.Focus(); return null;}
	// ** Gen
	a_by = m_egFormaGeo.Geometry.GetVertices(Vertex.SIZE_OF, out iVert, out a_i, Vertex.NORMAL_OFFSET, Vertex.TEXCOORD_OFFSET);
	// ** Ajustar
	m44 = Matrix4x4.Identity; m44.M00 = fEscala; m44.M11 = fEscala; m44.M22 = fEscala; // Aplicar escala
	m44.Offset.X = -paiForma.Width / 2 * fEscala; m44.Offset.Y = -paiForma.Height / 2 * fEscala; // Llevar al centro
	Vector.Transform(m44, a_by, Vertex.SIZE_OF, 0, 0, a_by, Vertex.SIZE_OF, 0, 0, iVert);
	a_ver = new Vertex[iVert];	using cArrayStream ast = new (a_ver);	ast.WriteArray(a_by, Vertex.SIZE_OF); // Conv a Vertex[]
	return new cModelo(a_ver, a_ver.Length, a_i, a_i.Length);
}
private cModelo? m_mdlCreaLetras()											// Las letras miran hacia atrás y van de 0 a +x
{	const float TOLERA = 0.25f / 2;
	cGeometry geo; cGeometry[] a_geo;
	cGeometryData.eAction acc; int i, iSegs, iPt, iPts, iPtsTot, iVert, iIdx, iVertIni, iIdxIni, iVertsCara; bool b; Vector v;
	Matrix4x4 m44; cArrayStream ast; Vertex[] a_vx; int[] a_i, a_iCara; byte[] a_byVertCaras; float fEspesor, fTex;
	Rectangle rtBounds; Point pt;

	// ** Validar
		if (rbtCharTxt.Checked)
		{	if (txtCharChars.Text.Length == 0)	{	mDialog.MsgBoxExclamation("Enter text", "Create characters"); txtCharChars.Focus(); return null;}
		} else if (gctCharGli.Geometry is null)
		{	mDialog.MsgBoxExclamation("Select glyph", "Create characters"); btnCharGli.Focus(); return null;
		}
	fEspesor = ntbCharFondo.FloatValue;
	// ** Tomar glifos
	geo = (rbtCharTxt.Checked ? new cTextLayout(txtCharChars.Text, m_fntChars!).GetGeometry(0) : gctCharGli.Geometry!);
	using cGeometryData gd = geo.GetData(true, TOLERA);

	a_geo = new cGeometry[gd.FigureCount]; iPtsTot = 0;
	for (i = 0; i < a_geo.Length; i++)										// ** Crear geo por cada fig
	{	acc = gd.Read(out iSegs, out iPt, out iPts, out b);	if ((acc & cGeometryData.eAction.BeginMask) == 0)	continue;
		a_geo[i] = new cPathGeometry((cPathGeometry.Sink snk) =>
			{	snk.BeginFigure(gd.Points[iPt], (acc == cGeometryData.eAction.BeginFilled)); iPtsTot++;
				while (((acc = gd.Read(out iSegs, out iPt, out iPts, out b)) & cGeometryData.eAction.EndMask) == 0)
				{	snk.AddLines(gd.Points, iPt, iPts); iPtsTot += iPts;
				}
				snk.EndFigure(acc == cGeometryData.eAction.EndClosed);
			}
			);
	}
	// ** Agregar caras
	a_byVertCaras = geo.GetVertices(Vertex.SIZE_OF, out iVertsCara, out a_iCara, Vertex.NORMAL_OFFSET, Vertex.TEXCOORD_OFFSET, false, TOLERA);
	a_vx = new Vertex[(iPtsTot + iVertsCara) * 2]; a_i = new int[(iPtsTot * 3 + a_iCara.Length) * 2];
	ast = new cArrayStream(a_vx);
	ast.Write(a_byVertCaras, 0, a_byVertCaras.Length); iVert = iVertsCara;	// ** Frente
		for (i = 0; i < iVert; i++)	{	a_vx[i].Normal.Z = -1; a_vx[i].Texcoord = new Point(1 - a_vx[i].Texcoord.X, 1 - a_vx[i].Texcoord.Y);} // Invert normales y txcoord
		System.Array.Copy(a_iCara, a_i, a_iCara.Length); iIdx = a_iCara.Length;
	ast.Write(a_byVertCaras, 0, a_byVertCaras.Length); iVert += iVertsCara; // ** Espalda
		for (i = iVertsCara; i < iVert; i++)	a_vx[i].Texcoord = new Point(1 - a_vx[i].Texcoord.X, 1 - a_vx[i].Texcoord.Y); // Invert txcoord
		m44 = Matrix4x4.Identity; m44.M32 = fEspesor;						// Empujar espalda
			Vector.Transform(m44, a_vx, Vertex.SIZE_OF, iVertsCara, 0, a_vx, Vertex.SIZE_OF, iVertsCara, 0, iVertsCara);
		System.Array.Copy(a_iCara, 0, a_i, iIdx, a_iCara.Length); iIdxIni = iIdx; iIdx += a_iCara.Length;
			for (i = iIdxIni; i < iIdx; i++)	a_i[i] += iVertsCara;		// Apuntar a verts de la espalda
			Triangle3D.InvertIndices(a_i, iIdxIni, a_iCara.Length);			// Invert triangs
	// ** Agregar contorno
	gd.ResetCursor(); rtBounds = geo.GetBounds();
	for (i = 0; i < a_geo.Length; i++)
	{	acc = gd.Read(out iSegs, out iPt, out iPts, out b);	if ((acc & cGeometryData.eAction.BeginMask) == 0)	continue;
		iIdxIni = iIdx; iVertIni = iVert; acc = cGeometryData.eAction.AddLines;
		do
		{		if (acc != cGeometryData.eAction.AddLines)	continue;
			while (iPts-- > 0)
			{	pt = gd.Points[iPt++]; v = new Vector(pt.X, pt.Y, 0).GetNormalized(); fTex = (pt.Y - rtBounds.Y) / rtBounds.Height;///norm
				a_vx[iVert].Position = new Vector(pt.X, pt.Y, 0); a_vx[iVert].Normal = v; // Pt 1, triang 1
					a_vx[iVert].Texcoord.X = 0; a_vx[iVert].Texcoord.Y = fTex; a_i[iIdx++] = iVert++;
				a_vx[iVert].Position = new Vector(pt.X, pt.Y, fEspesor); a_vx[iVert].Normal = v; // Pt 2
					a_vx[iVert].Texcoord.X = 1; a_vx[iVert].Texcoord.Y = fTex; a_i[iIdx++] = iVert++; a_i[iIdx++] = iVert;
				a_i[iIdx++] = iVert; a_i[iIdx++] = iVert - 1; a_i[iIdx++] = iVert + 1; // Triang 2
			}
		} while (((acc = gd.Read(out iSegs, out iPt, out iPts, out b)) & cGeometryData.eAction.EndMask) == 0);
		a_i[iIdx - 4] = iVertIni; a_i[iIdx - 3] = iVertIni; a_i[iIdx - 1] = iVertIni + 1; // ** Cerrar fig
	}
	// ** Invertir Y
	m44 = Matrix4x4.Identity; m44.M11 = -1; Vector.Transform(m44, a_vx, Vertex.SIZE_OF, 0, 0, a_vx, Vertex.SIZE_OF, 0, 0, iVert);
	for (i = 0; i < a_geo.Length; i++)	a_geo[i].Dispose();
	return new cModelo(a_vx, iVert, a_i, iIdx);
}
private cModelo? m_mdlCreaTerreno()
{	float fPrecisH, fPrecisF, fAncho, fAlto, fFondo, fTuDelta, fTvDelta; nint ptr; Vector v; Point pt;
	Vertex[] a_vx; int[] a_i; int i, iVert, iIdx, iCantCols, iCantColsF, iVertSigFila, iPixBytes, iPixX, iPixY; PointI ptiTamBmp;
	cWindow w;

	// ** Validar
	fPrecisH = ntbTerrPrecH.FloatValue; fPrecisF = ntbTerrPrecF.FloatValue;
		if (fPrecisH < 0.01f || fPrecisF < 0.01f)	{	mDialog.MsgBoxExclamation("Precision must be greater than 0.01", "Create terrain"); ntbTerrPrecH.Focus(); return null;}
	fAncho = ntbTerrAncho.FloatValue; fAlto = ntbTerrAlto.FloatValue; fFondo = ntbTerrFondo.FloatValue;
		if (fAncho < 0.01f || fAlto < 0.01f || fFondo < 0.01f)	{	mDialog.MsgBoxExclamation("Size must be greater than 0.01", "Create terrain"); ntbTerrAncho.Focus(); return null;}
	// ** Crear noise (si no hay)
	uslTerr_Changed(null);
	w = FindWindow()!;
	w.Render2D((cGraphics g) =>
		{	g.PrimitiveBlend = ePrimitiveBlend.Copy; g.FillRectangle(new Rectangle(0, 0, paiTerr.Width, paiTerr.Height), eBrush.Transparent);
			g.DrawImage(m_teTurbu, default);
		});
	// ** Calc tams
	iCantCols = mMath.Ceil(fAncho / fPrecisH); iCantColsF = mMath.Ceil(fFondo / fPrecisF);
	a_vx = new Vertex[(iCantCols + 1) * (iCantColsF + 1)]; a_i = new int[iCantCols * iCantColsF * 6]; iVert = 0; iIdx = 0;
	// ** Gen
	using cBitmap bmp = w.BackBuffer.CopyPixelsToBitmap(new RectangleI(0, 0, (int)paiTerr.Width, (int)paiTerr.Height));
	using cBitmap.cBitmapLock blk = bmp.Lock();

	// ** Gen cols
	fTuDelta = fPrecisH / fAncho; fTvDelta = fPrecisF / fFondo; fAncho /= 2; fFondo /= 2;
	ptiTamBmp = bmp.PixelSize - new PointI(1, 1); iPixBytes = mImaging.GetPixelFormat(bmp.PixelFormat).BitsPerPixel / 8;
	v.X = fAncho; pt.X = 0;
	for (i = 0; i <= iCantCols; i++, v.X -= fPrecisH, pt.X += fTuDelta)
	{	if (i == iCantCols) {	v.X = -fAncho; pt.X = 1;}					// Ult col: evitar exceso; la ult col sí puede quedar incompleta
		v.Z = -fFondo; pt.Y = 0; iPixX = (int)(pt.X * ptiTamBmp.X);
		// ** Gen col: crear fils
		for (int j = 0; j <= iCantColsF; j++, v.Z += fPrecisF, pt.Y += fTvDelta)
		{	if (j == iCantColsF)											// Ult fil: evitar exceso; la ult fil sí puede quedar incompleta
			{	v.Z = fFondo; pt.Y = 1;
			// ** Agregar idx (excepto para la ult col y la ult fil)
			} else if (i < iCantCols)
			{	iVertSigFila = iVert + iCantColsF + 1;
				a_i[iIdx++] = iVert; a_i[iIdx++] = iVertSigFila; a_i[iIdx++] = iVert + 1;
				a_i[iIdx++] = iVert + 1; a_i[iIdx++] = iVertSigFila; a_i[iIdx++] = iVertSigFila + 1;
			}
			// ** Agregar vert
			iPixY = (int)(pt.Y * ptiTamBmp.Y); ptr = blk.Pointer + iPixY * blk.Stride + iPixX * iPixBytes;
			v.Y = Color.ReadBits(ref ptr, bmp.PixelFormat).A * fAlto; a_vx[iVert].Position = v;
			a_vx[iVert].Texcoord = pt;
			iVert++;
		}
	}
	// ** Calc normales
	for (i = 0; i < iVert; i++)	a_vx[i].Normal = Vector.NaN;				// Limpiar
	for (i = 0, iVert = 0, iVertSigFila = iCantColsF + 1; i < iCantCols; i++)
	{	for (int j = 0; j < iCantColsF; j++, iVert++, iVertSigFila++)
		{	s_CalcNormal(a_vx, iVert, iVertSigFila, iVert + 1); s_CalcNormal(a_vx, iVert + 1, iVertSigFila, iVertSigFila + 1);
		}
		iVert++; iVertSigFila++;
	}
	for (i = 0, iVert = a_vx.Length; i < iVert; i++)	a_vx[i].Normal.Normalize(); // Normalizar
	// ** Crear mdl
	return new cModelo(a_vx, a_vx.Length, a_i, iIdx);
}
}
using CultInf = System.Globalization.CultureInfo;
using System.Linq;
using Wew.Control;
using Wew.Media;
using Wew;

namespace DirectPaint3D
{
public class kParte : cDockControl
{	u3DModelControl u3dParte; cCheckBox chkTransformParts; cCheckBox chkAplicarDesp;
	readonly cComboBox cboMaterTipo; readonly cColorButton cbtMaterColor, cbtMaterSpecClr; readonly uSlider uslMaterSpecPow, uslMaterReflec;
		readonly cButton btnCambiarLayout; readonly cLabel lblLayout;
	readonly cTextBox txtNomb; readonly cLabel lblCantVerts, lblCnt, lblCantTriangs; readonly cCheckBox chkElimSubparts;
		readonly cListBox lbPartes;
// ** Estat
private static void s_Quita(cModelo mdlMdl, bool bQuitarSubpartes)
{	c3DModel[] a_mdl; c3DModel mdlNuevoPad;

	// ** Quitar sólo la parte (mantener subpartes)
	if (!bQuitarSubpartes && mdlMdl.HasParts)
	{	if (mdlMdl == wMain.MdlRaíz)
		{	a_mdl = new c3DModel[mdlMdl.Parts.Count - 1]; mdlMdl.Parts.CopyTo(a_mdl, 1, 0, a_mdl.Length); // Copiar hijos (excepto el 1°)
			mdlNuevoPad = wMain.MdlRaíz = (cModelo)mdlMdl.Parts[0];			// Conv al 1° hijo en raíz
			mMod.Modelos.Remove(mdlMdl); mMod.Modelos.Add(mdlNuevoPad);
		} else
		{	a_mdl = mdlMdl.Parts.ToArray();									// Copiar hijos
			mdlNuevoPad = (cModelo)mdlMdl.Container;
			mdlNuevoPad.Parts.Remove(mdlMdl);
		}
		mdlMdl.Parts.Clear();
		foreach (c3DModel mdl in a_mdl)	mdlNuevoPad.Parts.Add(mdl);			// Mover a los hijos al nuevo pad
		mMod.Partes.Remove(mdlMdl);											// Quitar de la lista de partes
	// ** Quitar parte con subpartes
	} else
	{	if (mdlMdl == wMain.MdlRaíz)
		{	mMod.Modelos.Remove(mdlMdl); wMain.MdlRaíz = null;
		} else
		{	mdlMdl.Container.Parts.Remove(mdlMdl);
		}
		mdlMdl.ApplyToTree((c3DModel mdl) =>	mMod.Partes.Remove((cModelo)mdl)); // Quitar de la lista de partes
	}
	// ** Quitar puntos visuales
	mdlMdl.DescargaPuntos();
}
// ** Ctor/dtor
public kParte()
{	cTabControl tc; cTabControl.cTab tab; cScrollableControl scCli; cStackPanel spnCli, spn; uPropertyGroup grp; uPropertySubgroup sgr;
	cContainer cnt; cToolButton tbt; cButton btn; cLabel lbl; cSeparator sep;

	IsChildForm = false; Text = "Part"; Width = 390; Height = 400; HideOnClose = true; Dock = eDirection.Left;
	tc = new cTabControl() {	Margins = new Rect(0)};
	tc.LayoutSuspended = true; tc.Header.LayoutSuspended = true;
	// ** Pos
	tab = tc.Tabs.Add("Position");
		scCli = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical};
		spnCli = new cStackPanel() {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
		grp = new uPropertyGroup() {	Text = "Part"};
			u3dParte = new u3DModelControl(); u3dParte.ModelChanged += u3dParte_Changed;
				u3dParte.ModelInvalidated += u3dParte_ModelInvalidated; grp.AddControl(u3dParte);
			spnCli.AddControl(grp);
		grp = new uPropertyGroup() {	Text = "Transform"};
			cnt = new cContainer() {	AutoSize = eAutoSize.Height};
				chkTransformParts = new cCheckBox() {	Text = "Transform subparts"};
					cnt.AddControl(chkTransformParts);
				chkAplicarDesp = new cCheckBox() {	Text = "Apply location", LeftMargin = 220};
					cnt.AddControl(chkAplicarDesp);
				grp.AddControl(cnt);
			sep = new cSeparator();
				grp.AddControl(sep);
			btn = new cButton() {	Text = "Transform vertices", Width = 150};
				btn.Click += btnTransform_Click;
				grp.AddControl(btn); btn.RightMargin = float.NaN;
			sep = new cSeparator();
				grp.AddControl(sep);
			cnt = new cContainer() {	AutoSize = eAutoSize.Height};
				btn = new cButton() {	Text = "Flip X", Width = 102};
					btn.Click += btnFlipX_Click;
					cnt.AddControl(btn);
				btn = new cButton() {	Text = "Flip Y", Width = 102, LeftMargin = 117};
					btn.Click += btnFlipY_Click;
					cnt.AddControl(btn);
				btn = new cButton() {	Text = "Flip Z", Width = 102, LeftMargin = 232};
					btn.Click += btnFlipZ_Click;
					cnt.AddControl(btn);
				grp.AddControl(cnt);
			sep = new cSeparator();
				grp.AddControl(sep);
			btn = new cButton() {	Text = "Recalculate normals", Width = 150};
				btn.Click += btnRecalcNorms_Click;
				grp.AddControl(btn); btn.RightMargin = float.NaN;
			spnCli.AddControl(grp);
		scCli.ClientArea.AddControl(spnCli);
		tab.Content = scCli;
	// ** Mat
	tab = tc.Tabs.Add("Material");
		scCli = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical};
		spnCli = new cStackPanel() {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
		grp = new uPropertyGroup() {	Text = "Material"};
			sgr = new uPropertySubgroup() {	Text = "Type"};
				cboMaterTipo = new cComboBox() {	Data = new string[] {	"Flat", "Light", "Specular"}};
					cboMaterTipo.SelectionChanged += Mater_Changed; sgr.AddControl(cboMaterTipo);
				grp.AddControl(sgr);
			sgr = new uPropertySubgroup() {	Text = "Color"};
				cbtMaterColor = new cColorButton() {	Width = 30, Height = 30}; cbtMaterColor.ColorChanged += Mater_Changed;
					sgr.AddControl(cbtMaterColor);
				lbl = new cLabel() {	Text = "Specular color", AutoSize = eAutoSize.Height, Width = 130
						, TextAlignment = eTextAlignment.Right};
					sgr.AddControl(lbl);
				cbtMaterSpecClr = new cColorButton() {	Width = 30, Height = 30}; cbtMaterSpecClr.ColorChanged += Mater_Changed;
					sgr.AddControl(cbtMaterSpecClr);
				grp.AddControl(sgr);
			uslMaterSpecPow = new uSlider() {	Text = "Specular power", Maximum = 300}; uslMaterSpecPow.ValueChanged += Mater_Changed;
				grp.AddControl(uslMaterSpecPow);
			uslMaterReflec = new uSlider() {	Text = "Reflectance", IsPercent = true}; uslMaterReflec.ValueChanged += Mater_Changed;
				grp.AddControl(uslMaterReflec);
			sgr = new uPropertySubgroup() {	Text = "Texture"};
				tbt = new cToolButton() {	ToolTip = "Open", Bitmap = mRes.BmpAbrir}; tbt.Click += TexAbrir_Click;
					sgr.AddControl(tbt);
				tbt = new cToolButton() {	ToolTip = "Remove", Bitmap = mRes.BmpElim}; tbt.Click += TexElim_Click;
					sgr.AddControl(tbt);
				tbt = new cToolButton() {	ToolTip = "Save", Bitmap = mRes.BmpGrabar}; tbt.Click += TexGrab_Click;
					sgr.AddControl(tbt);
				grp.AddControl(sgr);
			spnCli.AddControl(grp);
			sgr = new uPropertySubgroup() {	Text = "Normal map"};
				tbt = new cToolButton() {	ToolTip = "Open", Bitmap = mRes.BmpAbrir}; tbt.Click += TexNormAbrir_Click;
					sgr.AddControl(tbt);
				tbt = new cToolButton() {	ToolTip = "Remove", Bitmap = mRes.BmpElim}; tbt.Click += TexNormElim_Click;
					sgr.AddControl(tbt);
				tbt = new cToolButton() {	ToolTip = "Save", Bitmap = mRes.BmpGrabar}; tbt.Click += TexNormGrab_Click;
					sgr.AddControl(tbt);
				grp.AddControl(sgr);
			spnCli.AddControl(grp);
		grp = new uPropertyGroup() {	Text = "Layout"};
			lblLayout = new cLabel() {	BorderStyle = eBorderStyle.Default, AutoSize = eAutoSize.None, TextAlignment = eTextAlignment.Center};
				grp.AddControl(lblLayout);
			btnCambiarLayout = new cButton() {	Text = "Change vertex layout", Width = 178}; btnCambiarLayout.Click += btnCambiarLayout_Click;
				grp.AddControl(btnCambiarLayout); btnCambiarLayout.RightMargin = float.NaN;
			spnCli.AddControl(grp);
		scCli.ClientArea.AddControl(spnCli);
		tab.Content = scCli;
	// ** Estruc
	tab = tc.Tabs.Add("Structure");
		scCli = new cScrollableControl() {	BarVisibility = eScrollBars.Vertical};
		spnCli = new cStackPanel() {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
		grp = new uPropertyGroup() {	Text = "Basic information"};
			sgr = new uPropertySubgroup() {	Text = "Name"};
				txtNomb = new cTextBox() {	RightMargin = 0}; txtNomb.TextChanged += txtNomb_Changed;
					sgr.AddControl(txtNomb);
				grp.AddControl(sgr);
			sgr = new uPropertySubgroup() {	Text = "Vertices"};
				lblCantVerts = new cLabel() {	BorderStyle = eBorderStyle.Default, AutoSize = eAutoSize.None
						, Width = 71, TextAlignment = eTextAlignment.Center};
					sgr.AddControl(lblCantVerts);
				grp.AddControl(sgr);
			sgr = new uPropertySubgroup() {	Text = "Triangles"};
				lblCantTriangs = new cLabel() {	Text = "0", BorderStyle = eBorderStyle.Default, AutoSize = eAutoSize.None
						, Width = 71, TextAlignment = eTextAlignment.Center};
					sgr.AddControl(lblCantTriangs);
				grp.AddControl(sgr);
			spnCli.AddControl(grp);
		grp = new uPropertyGroup() {	Text = "Deletion"};
			cnt = new cContainer() {	AutoSize = eAutoSize.Height};
				chkElimSubparts = new cCheckBox() {	Text = "Remove subparts"};
					cnt.AddControl(chkElimSubparts);
				btn = new cButton() {	Text = "Delete part", Width = 136, LeftMargin = 160}; btn.Click += btnElim_Click;
					cnt.AddControl(btn);
				grp.AddControl(cnt);
			spnCli.AddControl(grp);
		grp = new uPropertyGroup() {	Text = "Structure"};
			sgr = new uPropertySubgroup() {	Text = "Container"};
				lblCnt = new cLabel() {	BorderStyle = eBorderStyle.Default, RightMargin = 0, AutoSize = eAutoSize.None};
					sgr.AddControl(lblCnt);
				grp.AddControl(sgr);
			sgr = new uPropertySubgroup() {	Text = "Subparts"};
				spn = new cStackPanel() {	Direction = eDirection.Bottom, RightMargin = 0, AutoSize = eAutoSize.Height};
					lbPartes = new cListBox() {	RightMargin = 0, Height = 150};
						spn.AddControl(lbPartes);
					btn = new cButton() {	Text = "Add", Width = 75}; btn.Click += btnAgregarSubparte_Click;
						spn.AddControl(btn);
					sgr.AddControl(spn);
				grp.AddControl(sgr);
			spnCli.AddControl(grp);
		scCli.ClientArea.AddControl(spnCli);
		tab.Content = scCli;
	tc.LayoutSuspended = false; tc.Header.LayoutSuspended = false;
	AddControl(tc);
}
public void Ini()
{	u3dParte.RelativeRotation = Settings.Default.RotRel;
	u3dParte.KeepAspect = Settings.Default.MantAspec;
}
public override void Close()
{	Settings.Default.RotRel = u3dParte.RelativeRotation;
	Settings.Default.MantAspec = u3dParte.KeepAspect;
	base.Close();
}
// ** Props
public cModelo Modelo
{	get														{	return (cModelo)u3dParte.Model;}
	set
	{	foreach (cModelo mdl in mMod.Partes)	mdl.Sel = false;
		u3dParte.Model = value; u3dParte.Refresh();							// Refres en caso de ser el mismo
		if (value != null)													// ** Cargar
		{	value.Sel = true;
			txtNomb.Text = value.Name; lblCantVerts.Text = value.VertexData.VertexCount.ToString(CultInf.CurrentCulture);
			lblCantTriangs.Text = (value.Topology == eVertexTopology.Triangle ? value.VertexData.IndexCount / 3 : 0).ToString(CultInf.CurrentCulture);
			cboMaterTipo.SelectedIndex = (int)value.Material.Type; cbtMaterColor.Color = value.Material.Color;
				 cbtMaterSpecClr.Color = value.Material.SpecularColor;
			uslMaterSpecPow.Value = value.Material.SpecularPower; uslMaterReflec.Value = value.Material.Reflectance;
			lblLayout.Text = (value.VertexData.Layout == c3DModel.eVertexLayout.PositionNormalTexcoord ? "Textured" : "Solid color");
			lblCnt.Text = value.Container?.Name;
			lbPartes.Data = value.Parts;
			wMain.Refresca();
		} else																// ** Limpiar
		{	txtNomb.Text = null; lblCantVerts.Text = "0"; lblCantTriangs.Text = "0";
			cboMaterTipo.SelectedIndex = (int)eMaterial.Light; cbtMaterColor.Color = eColor.White; cbtMaterSpecClr.Color = eColor.White;
			uslMaterSpecPow.Value = 100; uslMaterReflec.Value = 0;
			lblLayout.Text = null;
			lblCnt.Text = null; lbPartes.Data = null;
		}
	}
}
// ** Mets
public void RefrescaPos()									{	u3dParte.Refresh();}
public void Elimina(bool bSubpartes)
{		if (Modelo == null || !mDialog.MsgBoxQuestion("Delete part '" + Modelo.Name + "'?", "Delete"))	return; // Sin mdl o cancelado: salir
	s_Quita(Modelo, bSubpartes);											// Quitar
	wMain.RefrescaListaPartes(); mMod.PuntoProps.PuntosSel = null; mMod.MainWnd.Changed = true; // Refrescar
}
private void u3dParte_Changed(object sender)				{	mMod.MainWnd.Changed = true;}
private void u3dParte_ModelInvalidated(object sender)		{	wMain.Refresca();}
private void btnTransform_Click(object sender)				{	Matrix4x4 m44 = Matrix4x4.Identity; m_Transform(ref m44);}
private void btnFlipX_Click(object sender)					{	Matrix4x4 m44 = Matrix4x4.Identity; m44._11 = -1; m_Transform(ref m44);}
private void btnFlipY_Click(object sender)					{	Matrix4x4 m44 = Matrix4x4.Identity; m44._22 = -1; m_Transform(ref m44);}
private void btnFlipZ_Click(object sender)					{	Matrix4x4 m44 = Matrix4x4.Identity; m44._33 = -1; m_Transform(ref m44);}
private void btnRecalcNorms_Click(object sender)			{	Modelo?.RecalcNorms(chkTransformParts.Checked); m_Refresca();}
private void Mater_Changed(object sender)
{		if (Modelo == null)	return;
	Modelo.Material.Type = (eMaterial)cboMaterTipo.SelectedIndex;
	Modelo.Material.Color = cbtMaterColor.Color; Modelo.Material.SpecularColor = cbtMaterSpecClr.Color;
	Modelo.Material.SpecularPower = (int)uslMaterSpecPow.Value; Modelo.Material.Reflectance = uslMaterReflec.Value;
	mMod.MainWnd.Changed = true;
}
private void TexAbrir_Click(object sender)					{	m_AbreTex(false);}
private void TexElim_Click(object sender)
{		if (Modelo == null)	return;
	Modelo.Material.Texture = null; mMod.MainWnd.Changed = true;
}
private void TexGrab_Click(object sender)					{	m_GrabaTex(false);}
private void TexNormAbrir_Click(object sender)				{	m_AbreTex(true);}
private void TexNormElim_Click(object sender)
{		if (Modelo == null)	return;
	Modelo.Material.NormalMap = null; mMod.MainWnd.Changed = true;
}
private void TexNormGrab_Click(object sender)				{	m_GrabaTex(true);}
private void btnCambiarLayout_Click(object sender)
{	c3DModel.eVertexLayout vl;

		if (Modelo == null || !mDialog.MsgBoxQuestion("Change layout?", "Change layout"))	return;
	if (Modelo.VertexData.Layout == c3DModel.eVertexLayout.PositionNormal)
		vl = c3DModel.eVertexLayout.PositionNormalTexcoord;
	else
	{	vl = c3DModel.eVertexLayout.PositionNormal;
	}
	Modelo.CambiaLayout(vl);
	m_Refresca();
}
private void txtNomb_Changed(object sender)
{		if (Modelo == null)	return;
	Modelo.Name = txtNomb.Text; wMain.RefrescaNombs(); mMod.MainWnd.Changed = true;
}
private void btnElim_Click(object sender)					{	Elimina(chkElimSubparts.Checked);}
private void btnAgregarSubparte_Click(object sender)
{	cModelo mdl; cListBox lb;

		if (Modelo == null)	return;											// Sin mdl: salir
	lb = new cListBox() {	Width = 400, Height = 350, Margins = new Rect(0)}; // ** Tomar parte
		lb.Data = (from todos in mMod.Partes join hijo in Modelo.Parts on todos equals hijo into rel // Sel partes que no son ni están contenidas en este mdl
				from hijo2 in rel.DefaultIfEmpty()
				where hijo2 == null && todos != Modelo select todos
			).ToList();
		if (lb.Items.Count == 0)	{	mDialog.MsgBoxExclamation("No parts to add", "Add subpart"); return;} // Sin partes: salir
		lb.SelectedIndex = 0; lb.ItemActivated += (object sender2) =>	{	((cDialog)lb.FindWindow()).DialogResult = true;};
	if (mDialog.ShowDialog(lb, "Select part") != true)	return;				// Cancelado: salir
	mdl = (cModelo)lb.SelectedItem;
	if (Modelo.IsContainedBy(mdl))	s_Quita(mdl, false);	else	{	mdl.Container.Parts.Remove(mdl);} // ** Quitar parte
	Modelo.Parts.Add(mdl); lbPartes.Refresh(); mMod.MainWnd.Changed = true;	// ** Agregar parte
}
private void m_Transform(/*const*/ ref Matrix4x4 m44Efecto)
{	Matrix4x4 m44World;

		if (Modelo == null)	return;
	m44World = Modelo.ScaleMatrix * Modelo.RotationMatrix;					// Componer world (no tomar Modelo.Matrix porque incluye al cnt)
		if (chkAplicarDesp.Checked)	m44World.Offset = Modelo.Location;
		m44World.MultiplyAssign(ref m44Efecto);
	Modelo.TransformaPuntos(ref m44World, m44World.Offset, chkTransformParts.Checked); // Transformar
		Modelo.RecalcLims();
	if (chkAplicarDesp.Checked)	Modelo.Location = new Vector();				// Quitar desp
	Modelo.AbsoluteRotation = new Vector(); Modelo.ScaleX = 1; Modelo.ScaleY = 1; Modelo.ScaleZ = 1; // Quitar rot y escala
	m_Refresca();
}
private void m_AbreTex(bool bNormal)
{	string s;

		if (Modelo == null)	return;
	s = mDialog.ShowOpenFile(mMod.FILTRO_IMG, mMod.DLG_GUID_IMG);	if (s == null) return;
try
{	if (!bNormal)
		Modelo.Material.Texture = new cTexture(mMod.MainWnd, s, true);
	else
	{	Modelo.Material.NormalMap = new cTexture(mMod.MainWnd, s, true);
	}
	mMod.MainWnd.Changed = true;
} catch (System.Exception ex)
{	mDialog.MsgBoxExclamation(ex.Message, "Error");
}
}
private void m_GrabaTex(bool bNormal)
{	cTexture tex = (!bNormal ? Modelo?.Material.Texture : Modelo?.Material.NormalMap); string sArchi;

		if (tex == null)	return;
	sArchi = mDialog.ShowSaveFile(mMod.FILTRO_IMG_GRAB, mMod.DLG_GUID_IMG);	if (sArchi == null) return;
try
{	using (System.IO.FileStream fst = System.IO.File.Create(sArchi))	fst.WriteBuffer(tex.Data, tex.DataSize);
} catch (System.Exception ex)
{	mDialog.MsgBoxExclamation(ex.Message, "Error");
}
}
private void m_Refresca()										{	Modelo = Modelo; mMod.PuntoProps.Refresca(); mMod.MainWnd.Changed = true;}
}
}
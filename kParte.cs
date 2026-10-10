using System.Linq;
using Wew.Control;
using Wew.Media;
using Wew;

class kParte : cDockControl
{	readonly u3DModelControl u3dParte; readonly cCheckBox chkTransformParts, chkAplicarDesp; readonly uSlider uslTesel;
	readonly cComboBox cboMaterTipo; readonly cColorButton cbtMaterColor, cbtMaterSpecClr; readonly uSlider uslMaterSpecPow, uslMaterReflec;
		readonly uSlider uslMaterTransp;
		 readonly cCheckBox chkTransp, chkClipAlfa;
		 readonly cComboBox cboLayout;
	readonly cTextBox txtNomb; readonly cLabel lblCantVerts, lblCnt, lblCantTriangs, lblCantSubparts; readonly cCheckBox chkElimSubparts;
		readonly cListBox lbPartes;
	readonly cStackPanel spnHuesos;
// ** Estat
private static void s_Quita(cModelo mdlMdl, bool bQuitarSubpartes)
{	c3DModel[] a_mdl; c3DModel mdlNuevoPad;

	// ** Quitar sólo la parte (mantener subpartes)
	if (!bQuitarSubpartes && mdlMdl.HasParts)
	{	if (mdlMdl == wMain.MdlRaíz)
		{	a_mdl = new c3DModel[mdlMdl.Parts.Count - 1]; mdlMdl.Parts.CopyTo(a_mdl, 1); // Copiar hijos (excepto el 1°)
			mdlNuevoPad = wMain.MdlRaíz = (cModelo)mdlMdl.Parts[0]; mMod.Raíces.Add(mdlNuevoPad); // Conv al 1° hijo en raíz
		} else
		{	a_mdl = [.. mdlMdl.Parts];										// Copiar hijos
			mdlNuevoPad = (cModelo)mdlMdl.Container!;
		}
		mdlMdl.Parts.Clear();												// No destruir a los hijos
		mdlNuevoPad.Parts.AddRange(a_mdl);									// Mover los hijos al nuevo pad
	// ** Quitar parte con subpartes
	} else
	{	if (mdlMdl == wMain.MdlRaíz)	wMain.MdlRaíz = null;
	}
	// ** Destruir
	mdlMdl.Dispose();
}
// ** Ctor/dtor
public kParte()
{	cTabControl tc; cTabControl.cTab tab; cScrollableControl scCli; cStackPanel spnCli, spn; uPropertyGroup grp; uPropertySubgroup sgr;
	cContainer cnt; cToolButton tbt; cButton btn; cLabel lbl; cSeparator sep;

	IsForm = false; Text = "Part"; Width = 390; Height = 400; HideOnClose = true; Dock = eDirection.Left;
	tc = new cTabControl {	Margins = new Rect(0)};
	// ** Pos
	tab = tc.Tabs.Add("Position");
		scCli = new cScrollableControl(); scCli.VerticalBar.Visible = true;
		spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
		grp = new uPropertyGroup {	Text = "Part"};
			u3dParte = new u3DModelControl(); u3dParte.ModelChanged += u3dParte_Changed;
				u3dParte.ModelInvalidated += u3dParte_ModelInvalidated;
				grp.AddControl(u3dParte);
			uslTesel = new uSlider {	Text = "Tessellation", Minimum = 1, Maximum = 64, Value = 1};
				uslTesel.ValueChanged += uslTesel_ValueChanged;
				grp.AddControl(uslTesel);
			spnCli.AddControl(grp);
		grp = new uPropertyGroup {	Text = "Transform"};
			cnt = new cContainer {	AutoSize = eAutoSize.Height};
				chkTransformParts = new cCheckBox {	Text = "Transform subparts"};
					cnt.AddControl(chkTransformParts);
				chkAplicarDesp = new cCheckBox {	Text = "Apply location", LeftMargin = 220};
					cnt.AddControl(chkAplicarDesp);
				grp.AddControl(cnt);
			sep = new cSeparator();
				grp.AddControl(sep);
			btn = new cButton {	Text = "Transform vertices", Width = 150};
				btn.Click += btnTransform_Click;
				grp.AddControl(btn); btn.RightMargin = float.NaN;
			sep = new cSeparator();
				grp.AddControl(sep);
			cnt = new cContainer {	AutoSize = eAutoSize.Height};
				btn = new cButton {	Text = "Flip X", Width = 102};
					btn.Click += btnFlipX_Click;
					cnt.AddControl(btn);
				btn = new cButton {	Text = "Flip Y", Width = 102, LeftMargin = 117};
					btn.Click += btnFlipY_Click;
					cnt.AddControl(btn);
				btn = new cButton {	Text = "Flip Z", Width = 102, LeftMargin = 232};
					btn.Click += btnFlipZ_Click;
					cnt.AddControl(btn);
				grp.AddControl(cnt);
			sep = new cSeparator();
				grp.AddControl(sep);
			btn = new cButton {	Text = "Recalculate normals", Width = 150};
				btn.Click += btnRecalcNorms_Click;
				grp.AddControl(btn); btn.RightMargin = float.NaN;
			spnCli.AddControl(grp);
		scCli.ClientArea.AddControl(spnCli);
		tab.Content = scCli;
	// ** Mat
	tab = tc.Tabs.Add("Material");
		scCli = new cScrollableControl(); scCli.VerticalBar.Visible = true;
		spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
		grp = new uPropertyGroup {	Text = "Material"};
			sgr = new uPropertySubgroup {	Text = "Type"};
				cboMaterTipo = new cComboBox();
					cboMaterTipo.Load(new string[] {	"Flat", "Light", "Specular"});
					cboMaterTipo.SelectionChanged += Mater_Changed;
					sgr.AddControl(cboMaterTipo);
				grp.AddControl(sgr);
			sgr = new uPropertySubgroup {	Text = "Color"};
				cbtMaterColor = new cColorButton {	Width = 30, Height = 30}; cbtMaterColor.ColorChanged += Mater_Changed;
					sgr.AddControl(cbtMaterColor);
				lbl = new cLabel {	Text = "Specular color", AutoSize = eAutoSize.Height, Width = 130
						, TextAlignment = eTextAlignment.Right};
					sgr.AddControl(lbl);
				cbtMaterSpecClr = new cColorButton {	Width = 30, Height = 30}; cbtMaterSpecClr.ColorChanged += Mater_Changed;
					sgr.AddControl(cbtMaterSpecClr);
				grp.AddControl(sgr);
			uslMaterSpecPow = new uSlider {	Text = "Specular power", Maximum = 300}; uslMaterSpecPow.ValueChanged += Mater_Changed;
				grp.AddControl(uslMaterSpecPow);
			uslMaterReflec = new uSlider {	Text = "Reflectance", IsPercent = true}; uslMaterReflec.ValueChanged += Mater_Changed;
				grp.AddControl(uslMaterReflec);
			uslMaterTransp = new uSlider {	Text = "Opacity", IsPercent = true}; uslMaterTransp.ValueChanged += Mater_Changed;
				grp.AddControl(uslMaterTransp);
			sgr = new uPropertySubgroup {	Text = "Texture"};
				tbt = new cToolButton {	ToolTip = "Open", Bitmap = eBitmap.BmpOpen}; tbt.Click += TexAbrir_Click;
					sgr.AddControl(tbt);
				tbt = new cToolButton {	ToolTip = "Remove", Bitmap = eBitmap.BmpDelete}; tbt.Click += TexElim_Click;
					sgr.AddControl(tbt);
				tbt = new cToolButton {	ToolTip = "Save", Bitmap = eBitmap.BmpSave}; tbt.Click += TexGrab_Click;
					sgr.AddControl(tbt);
				grp.AddControl(sgr);
			spnCli.AddControl(grp);
			sgr = new uPropertySubgroup {	Text = "Normal map"};
				tbt = new cToolButton {	ToolTip = "Open", Bitmap = eBitmap.BmpOpen}; tbt.Click += TexNormAbrir_Click;
					sgr.AddControl(tbt);
				tbt = new cToolButton {	ToolTip = "Remove", Bitmap = eBitmap.BmpDelete}; tbt.Click += TexNormElim_Click;
					sgr.AddControl(tbt);
				tbt = new cToolButton {	ToolTip = "Save", Bitmap = eBitmap.BmpSave}; tbt.Click += TexNormGrab_Click;
					sgr.AddControl(tbt);
				grp.AddControl(sgr);
			spnCli.AddControl(grp);
			cnt = new cContainer {	AutoSize = eAutoSize.Height};
				chkTransp = new cCheckBox {	Text = "Allow transparency"};
					chkTransp.CheckStateChanged += chkTransp_CheckStateChanged;
					cnt.AddControl(chkTransp);
				chkClipAlfa = new cCheckBox {	Text = "Clip alpha", LeftMargin = 220};
					chkClipAlfa.CheckStateChanged += chkClipAlfa_CheckStateChanged;
					cnt.AddControl(chkClipAlfa);
				grp.AddControl(cnt);
			btn = new cButton {		Text = "Remove duplicated textures", Width = 200};
				btn.Click += btnQuitarTexDup_Click;
				grp.AddControl(btn); btn.RightMargin = float.NaN;
		grp = new uPropertyGroup {	Text = "Layout"};
			cboLayout = new cComboBox();
				cboLayout.Load(new cTextValuePair<eVertexLayout>[]
					{	new ("Solid color", eVertexLayout.PosNorm)
						, new ("Textured", eVertexLayout.PosNormTex)
						, new ("Textured with tangent", eVertexLayout.PosNormTexTang)
						, new ("Bone", eVertexLayout.PosNormTexTangWeiBon)
					});
				cboLayout.SelectionChanged += cboLayout_SelectionChanged;
				grp.AddControl(cboLayout);
			spnCli.AddControl(grp);
		scCli.ClientArea.AddControl(spnCli);
		tab.Content = scCli;
	// ** Estruc
	tab = tc.Tabs.Add("Structure");
		scCli = new cScrollableControl(); scCli.VerticalBar.Visible = true;
		spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
		grp = new uPropertyGroup {	Text = "Basic information"};
			sgr = new uPropertySubgroup {	Text = "Name"};
				txtNomb = new cTextBox {	RightMargin = 0}; txtNomb.TextChanged += txtNomb_Changed;
					sgr.AddControl(txtNomb);
				grp.AddControl(sgr);
			sgr = new uPropertySubgroup {	Text = "Vertices"};
				lblCantVerts = new cLabel {	BorderStyle = eBorderStyle.Default, AutoSize = eAutoSize.None
						, Width = 71, TextAlignment = eTextAlignment.Center};
					sgr.AddControl(lblCantVerts);
				grp.AddControl(sgr);
			sgr = new uPropertySubgroup {	Text = "Triangles"};
				lblCantTriangs = new cLabel {	Text = "0", BorderStyle = eBorderStyle.Default, AutoSize = eAutoSize.None
						, Width = 71, TextAlignment = eTextAlignment.Center};
					sgr.AddControl(lblCantTriangs);
				grp.AddControl(sgr);
			sgr = new uPropertySubgroup {	Text = "Subparts"};
				lblCantSubparts = new cLabel {	Text = "0", BorderStyle = eBorderStyle.Default, AutoSize = eAutoSize.None
						, Width = 71, TextAlignment = eTextAlignment.Center};
					sgr.AddControl(lblCantSubparts);
				grp.AddControl(sgr);
			spnCli.AddControl(grp);
		grp = new uPropertyGroup {	Text = "Deletion"};
			cnt = new cContainer {	AutoSize = eAutoSize.Height};
				chkElimSubparts = new cCheckBox {	Text = "Remove subparts"};
					cnt.AddControl(chkElimSubparts);
				btn = new cButton {	Text = "Delete part", Width = 136, LeftMargin = 160}; btn.Click += btnElim_Click;
					cnt.AddControl(btn);
				grp.AddControl(cnt);
			spnCli.AddControl(grp);
		grp = new uPropertyGroup {	Text = "Structure"};
			sgr = new uPropertySubgroup {	Text = "Container"};
				lblCnt = new cLabel {	BorderStyle = eBorderStyle.Default, RightMargin = 0, AutoSize = eAutoSize.None};
					sgr.AddControl(lblCnt);
				grp.AddControl(sgr);
			sgr = new uPropertySubgroup {	Text = "Subparts"};
				spn = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 0, AutoSize = eAutoSize.Height};
					lbPartes = new cListBox {	RightMargin = 0, Height = 150};
						spn.AddControl(lbPartes);
					btn = new cButton {	Text = "Add", Width = 75}; btn.Click += btnAgregarSubparte_Click;
						spn.AddControl(btn);
					sgr.AddControl(spn);
				grp.AddControl(sgr);
			spnCli.AddControl(grp);
		scCli.ClientArea.AddControl(spnCli);
		tab.Content = scCli;
	// ** Huesos
	tab = tc.Tabs.Add("Bones");
		scCli = new cScrollableControl(); scCli.VerticalBar.Visible = true;
		spnHuesos = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
		scCli.ClientArea.AddControl(spnHuesos);
		tab.Content = scCli;
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
public cModelo? Parte
{	get														=>	(cModelo?)u3dParte.Model;
	set
	{	foreach (cModelo mdl in mMod.Partes)	mdl.Sel = false;
		u3dParte.Model = value; u3dParte.Refresh();							// Refres en caso de ser el mismo
		if (value is not null)												// ** Cargar
		{	value.Sel = true; uslTesel.Value = value.Tessellation;
			txtNomb.Text = value.Name; lblCantVerts.Text = value.VertexCount.ToString();
			lblCantTriangs.Text = (value.Topology == eVertexTopology.Triangle ? value.IndexCount / 3 : 0).ToString();
			lblCantSubparts.Text = value.Parts.Count.ToString();
			cboMaterTipo.SelectedIndex = (int)value.Material.Type; cbtMaterColor.Color = value.Material.Color;
				 cbtMaterSpecClr.Color = value.Material.SpecularColor;
			uslMaterSpecPow.Value = value.Material.SpecularPower; uslMaterReflec.Value = value.Material.Reflectance;
			uslMaterTransp.Value = value.Material.Alpha;
			chkTransp.Checked = value.Material.AllowTransparency; chkClipAlfa.Checked = value.Material.AllowTransparency;
			cboLayout.SelectedValue = new cTextValuePair<eVertexLayout>(null, value.VertexData!.Layout);
			lblCnt.Text = value.Container?.Name;
			lbPartes.Load(value.Parts);
			wMain.Refresca();
		} else																// ** Limpiar
		{	uslTesel.Value = 1;
			txtNomb.Text = null; lblCantVerts.Text = "0"; lblCantTriangs.Text = "0"; lblCantSubparts.Text = "0";
			cboMaterTipo.SelectedIndex = (int)eMaterial.Light; cbtMaterColor.Color = eColor.White; cbtMaterSpecClr.Color = eColor.White;
			uslMaterSpecPow.Value = 100; uslMaterReflec.Value = 0; uslMaterTransp.Value = 100;
			chkTransp.Checked = false; chkClipAlfa.Checked = false;
			cboLayout.Text = null;
			lblCnt.Text = null; lbPartes.Load(null);
		}
	}
}
// ** Mets
public void LimpiaHuesos()									{	spnHuesos.Controls.Clear();}
public void CargaHuesos()//¿
{	int i = 0;

		if (wMain.MdlRaíz!.Bones is null)	return;
	foreach (c3DModel.cBoneAnimation.Bone bon in wMain.MdlRaíz!.Bones)
	{	cLabel lbl; uMatrix uma;

		lbl = new () {	Text = $"Bone {i}", Font = eFont.SystemBoldText}; i++;
			spnHuesos.AddControl(lbl);
		uma = new () {	Tag = i};
			uma.Reset(["X", "Y", "Z", "W"], 4); uma.Matrix4x4 = bon.Matrix;
			uma.Changed += umaHuesos_Changed;
			spnHuesos.AddControl(uma);
	}
	//wMain.MdlRaíz.Animating = true;
}
public void RefrescaPos()									{	u3dParte.Refresh();}
public void Elimina(bool bSubpartes)
{		if (Parte is null || !mDialog.MsgBoxQuestion($"Delete part '{Parte.Name}'?", "Delete"))	return; // Sin mdl o cancelado: salir
	s_Quita(Parte, bSubpartes);											// Quitar
	wMain.RefrescaListaPartes(); mMod.PropsPunto.PuntosSel = null; mMod.MainWnd.Changed = true; // Refrescar
}
private void u3dParte_Changed(object sender)				{	mMod.MainWnd.Changed = true;}
private void u3dParte_ModelInvalidated(object sender)		{	wMain.Refresca();}
private void uslTesel_ValueChanged(object sender)
{	if (Parte is not null)	{	Parte.Tessellation = uslTesel.Value; mMod.MainWnd.Changed = true;}
}
private void btnTransform_Click(object sender)				{	Matrix4x4 m44 = Matrix4x4.Identity; m_Transform(m44);}
private void btnFlipX_Click(object sender)					{	Matrix4x4 m44 = Matrix4x4.Identity; m44.M00 = -1; m_Transform(m44);}
private void btnFlipY_Click(object sender)					{	Matrix4x4 m44 = Matrix4x4.Identity; m44.M11 = -1; m_Transform(m44);}
private void btnFlipZ_Click(object sender)					{	Matrix4x4 m44 = Matrix4x4.Identity; m44.M22 = -1; m_Transform(m44);}
private void btnRecalcNorms_Click(object sender)			{	Parte?.RecalcNorms(chkTransformParts.Checked); m_Refresca();}
private void Mater_Changed(object sender)
{		if (Parte is null)	return;
	Parte.Material.Type = (eMaterial)cboMaterTipo.SelectedIndex;
	Parte.Material.Color = cbtMaterColor.Color; Parte.Material.SpecularColor = cbtMaterSpecClr.Color;
	Parte.Material.SpecularPower = (int)uslMaterSpecPow.Value; Parte.Material.Reflectance = uslMaterReflec.Value;
	Parte.Material.Alpha = uslMaterTransp.Value;
	mMod.MainWnd.Changed = true;
}
private void TexAbrir_Click(object sender)					{	m_AbreTex(false);}
private void TexElim_Click(object sender)
{		if (Parte is null)	return;
	Parte.Material.Texture = null; mMod.MainWnd.Changed = true;
}
private void TexGrab_Click(object sender)					{	m_GrabaTex(false);}
private void TexNormAbrir_Click(object sender)				{	m_AbreTex(true);}
private void TexNormElim_Click(object sender)
{		if (Parte is null)	return;
	Parte.Material.NormalMap = null; mMod.MainWnd.Changed = true;
}
private void TexNormGrab_Click(object sender)				{	m_GrabaTex(true);}
private void chkTransp_CheckStateChanged(object sender)
{		if (Parte is null)	return;
	Parte.Material.AllowTransparency = chkTransp.Checked; mMod.MainWnd.Changed = true;
}
private void chkClipAlfa_CheckStateChanged(object sender)
{		if (Parte is null)	return;
	Parte.Material.ClipAlpha = chkClipAlfa.Checked; mMod.MainWnd.Changed = true;
}
private void btnQuitarTexDup_Click(object sender)			{	wMain.MdlRaíz?.ElimTexDup();}
private void cboLayout_SelectionChanged(object sender)
{		if (Parte is null)	return;
	Parte.CambiaLayout(((cTextValuePair<eVertexLayout>)cboLayout.SelectedValue!).Value);
	m_Refresca();
}
private void txtNomb_Changed(object sender)
{		if (Parte is null)	return;
	Parte.Name = txtNomb.Text; wMain.RefrescaNombs(); mMod.MainWnd.Changed = true;
}
private void btnElim_Click(object sender)					{	Elimina(chkElimSubparts.Checked);}
private void btnAgregarSubparte_Click(object sender)
{	cModelo mdl; cListBox lb;

		if (Parte is null)	return;											// Sin mdl: salir
	lb = new cListBox														// ** Tomar parte
		{	Width = 400, Height = 350, Margins = new Rect(0)
		};
		lb.Load((from todos in mMod.Partes join hijo in Parte.Parts on todos equals hijo into rel // Sel partes que no son ni están contenidas en este mdl
				from hijo2 in rel.DefaultIfEmpty()
				where hijo2 is null && todos != Parte select todos
			).ToList());
		if (lb.Items.Count == 0)	{	mDialog.MsgBoxExclamation("No parts to add", "Add subpart"); return;} // Sin partes: salir
		lb.SelectedIndex = 0; lb.ItemActivated += (object sender2) =>	((cDialog)lb.FindWindow()!).DialogResult = true;
	if (mDialog.ShowDialog(lb, "Select part") != true)	return;				// Cancelado: salir
	mdl = (cModelo)lb.SelectedValue!;
	if (Parte.IsContainedBy(mdl))	s_Quita(mdl, false);	else	mdl.Container!.Parts.Remove(mdl); // ** Quitar parte
	Parte.Parts.Add(mdl); lbPartes.Load(Parte.Parts, null, true); mMod.MainWnd.Changed = true; // ** Agregar parte
}
private void m_Transform(in Matrix4x4 m44Efecto)
{	Matrix4x4 m44World;

		if (Parte is null)	return;
	m44World = Parte.ScaleMatrix * Parte.RotationMatrix;					// Componer world (no tomar Modelo.Matrix porque incluye al cnt)
		if (chkAplicarDesp.Checked)	m44World.Offset = Parte.Location;
		m44World *= m44Efecto;
	Parte.TransformaPuntos(m44World, m44World.Offset, chkTransformParts.Checked); // Transformar
		Parte.RecalcLims();
	if (chkAplicarDesp.Checked)	Parte.Location = default;					// Quitar desp
	Parte.AbsoluteRotation = default; Parte.ScaleX = 1; Parte.ScaleY = 1; Parte.ScaleZ = 1; // Quitar rot y escala
	m_Refresca();
}
private void m_AbreTex(bool bNormal)
{	string? s;

		if (Parte is null)	return;
	s = mDialog.ShowOpenFile(mMod.FILTRO_IMG, mMod.DLG_GUID_IMG);	if (s is null) return;
try
{	if (!bNormal)
		Parte.Material.Texture = cModelo.GetTextura(s);
	else
	{	Parte.Material.NormalMap = cModelo.GetTextura(s);
	}
	mMod.MainWnd.Changed = true;
} catch (System.Exception ex)
{	mDialog.MsgBoxExclamation(ex.Message, "Error");
}
}
private void m_GrabaTex(bool bNormal)
{	cTexture? tex = (!bNormal ? Parte?.Material.Texture : Parte?.Material.NormalMap); string? sArchi;

		if (tex is null)	return;
	sArchi = mDialog.ShowSaveFile(mMod.FILTRO_GRAB, mMod.DLG_GUID_IMG);	if (sArchi is null) return;
try
{	using System.IO.FileStream fst = System.IO.File.Create(sArchi);

	fst.WriteBuffer(tex.Data, tex.DataSize);
} catch (System.Exception ex)
{	mDialog.MsgBoxExclamation(ex.Message, "Error");
}
}
private void m_Refresca()
{	Parte = (cModelo?)u3dParte.Model; mMod.PropsPunto.Refresca(); mMod.MainWnd.Changed = true;
}
private void umaHuesos_Changed(object sender)
{	uMatrix uma = (uMatrix)sender;

	wMain.MdlRaíz!.Bones![(int)uma.Tag!].Matrix = uma.Matrix4x4; wMain.Refresca();
}
}
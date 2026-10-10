using System.Diagnostics.CodeAnalysis;
using Wew.Control;
using Wew.Media;
using Wew;

class kHerrs : cDockControl
{	cModelo m_mdlMolde; readonly cList<cModelo> ml_mdlMoldes;
	readonly u3DModelControl u3dMolde; readonly cColorButton clbMoldeColor; readonly cListBox lbMolde;
	readonly cCheckBox chkClipEnabled; readonly uSlider uslPitch, uslYaw, uslDist;
	readonly cColorButton clbAmbiente; readonly uLightControl lgcSol, lgcLuz;
// ** Ctor/dtor
public kHerrs()
{	cTabControl tc; cTabControl.cTab tab; cScrollableControl scCli; cStackPanel spnCli; uPropertyGroup grp; uPropertySubgroup sgr;
	cContainer cnt; cLabel lbl; cButton btn;

	IsForm = false; Text = "Tools"; Width = 358; Height = 517; HideOnClose = true; Dock = eDirection.Left;
	tc = new cTabControl {	Margins = new Rect(0)};
	// ** Molde
	tab = tc.Tabs.Add("Mold");
		scCli = new cScrollableControl(); scCli.VerticalBar.Visible = true;
		spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
		lbl = new cLabel {	Text = "Select part and molder, and press Alt to move"
				, Font = eFont.SystemBoldText, AutoSize = eAutoSize.None, TextAlignment = eTextAlignment.Center, RightMargin = 0};
			spnCli.AddControl(lbl);
		u3dMolde = new u3DModelControl {	RightMargin = 0}; u3dMolde.ModelChanged += u3dMolde_ModelChanged;
			u3dMolde.ModelInvalidated += u3dMolde_ModelInvalidated; spnCli.AddControl(u3dMolde);
		sgr = new uPropertySubgroup {	Text = "Color"};
			clbMoldeColor = new cColorButton {	Width = 30, Height = 30}; clbMoldeColor.ColorChanged += clbMoldeColor_ColorChanged;
				sgr.AddControl(clbMoldeColor);
			spnCli.AddControl(sgr);
		grp = new uPropertyGroup {	Text = "Molders"};
			cnt = new cContainer {	AutoSize = eAutoSize.Height};
				lbMolde = new cListBox {	Height = 135, RightMargin = 80};
					lbMolde.ItemActivated += lbMolde_ItemActivated;
					cnt.AddControl(lbMolde);
				btn = new cButton {	Text = "Add", Width = 75, Margins = new Rect(float.NaN, 0, 0, float.NaN)};
					btn.Click += btnMoldeAgrega_Click;
					cnt.AddControl(btn);
				btn = new cButton {	Text = "Remove", Width = 75, Margins = new Rect(float.NaN, 30, 0, float.NaN)};
					btn.Click += btnMoldeQuita_Click;
					cnt.AddControl(btn);
				btn = new cButton {	Text = "Locate", Width = 75, Margins = new Rect(float.NaN, 60, 0, float.NaN)};
					btn.Click += btnMoldeBusca_Click;
					cnt.AddControl(btn);
				grp.AddControl(cnt);
			spnCli.AddControl(grp);
		scCli.ClientArea.AddControl(spnCli);
		tab.Content = scCli;
	// ** Corte
	tab = tc.Tabs.Add("Clip");
		scCli = new cScrollableControl(); scCli.VerticalBar.Visible = true;
		spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
		chkClipEnabled = new cCheckBox {	Text = "Enable clip plane", Font = eFont.SystemBoldText};
			chkClipEnabled.CheckStateChanged += chkClipEnabled_CheckStateChanged;
			spnCli.AddControl(chkClipEnabled);
		uslPitch = new uSlider {	Text = "Pitch", Minimum = -180, Maximum = 180, RightMargin = 0};
			uslPitch.ValueChanged += Clip_Changed;
			spnCli.AddControl(uslPitch);
		uslYaw = new uSlider {	Text = "Yaw", Minimum = -180, Maximum = 180, RightMargin = 0};
			uslYaw.ValueChanged += Clip_Changed;
			spnCli.AddControl(uslYaw);
		uslDist = new uSlider {	Text = "Distance", Minimum = -1000, Maximum = 1000, RightMargin = 0};
			uslDist.ValueChanged += Clip_Changed;
			spnCli.AddControl(uslDist);
		scCli.ClientArea.AddControl(spnCli);
		tab.Content = scCli;
	// ** Luces
	tab = tc.Tabs.Add("Lights");
		scCli = new cScrollableControl(); scCli.VerticalBar.Visible = true;
		spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
		grp = new uPropertyGroup {	Text = "Ambient"};
			sgr = new uPropertySubgroup {	Text = "Color"};
				clbAmbiente = new cColorButton() {	Color = new Color(0.9f, 0.9f, 0.9f)};
					clbAmbiente.ColorChanged += Luz_Changed;
					sgr.AddControl(clbAmbiente);
				grp.AddControl(sgr);
			spnCli.AddControl(grp);
		grp = new uPropertyGroup {	Text = "Light 1"};
			lgcSol = new uLightControl() {	Light = mMod.MainWnd.SunLight};
				lgcSol.Changed += Luz_Changed;
				grp.AddControl(lgcSol);
			spnCli.AddControl(grp);
		grp = new uPropertyGroup {	Text = "Light 2"};
			lgcLuz = new uLightControl() {	Light = wMain.LuzAtrás};
				lgcLuz.Changed += Luz_Changed;
				grp.AddControl(lgcLuz);
			spnCli.AddControl(grp);
		scCli.ClientArea.AddControl(spnCli);
		tab.Content = scCli;
	AddControl(tc);
	// ** Crear moldes
	ml_mdlMoldes = [];
	foreach (string s in Settings.Default.Moldes.Split('|'))
	{	try
		{	ml_mdlMoldes.Add(s.StartsWithOrdinal("@") ? new cModelo(GetType(), s[1..]) : new cModelo(s, true)); // Recurso o archi
		} catch (System.Exception ex)
		{	mDialog.MsgBoxExclamation(ex.Message, "Error");
		}
	}
	if (ml_mdlMoldes.Count == 0)	ml_mdlMoldes.Add(new cModelo(GetType(), "Res.MoldeCurvo.mdl")); // Evitar vacío
	lbMolde.Load(ml_mdlMoldes); Molde = ml_mdlMoldes[0];
	// ** Crear clip
	Clip_Changed(null);
}
public override void Close()
{	string? s = null;

	Settings.Default.MoldeVisi = m_mdlMolde.Visible;
	for (int i = 0; i < ml_mdlMoldes.Count; i++)	s += (i > 0 ? "|" : null) + ml_mdlMoldes[i].Ruta;
	Settings.Default.Moldes = s;
	base.Close();
}
internal void f_Dispose()						{	foreach (cModelo mdl in ml_mdlMoldes)	mdl.Dispose();}
// ** Props
public cModelo Molde
{	get											=>	m_mdlMolde;
		[MemberNotNull(nameof(m_mdlMolde))]
	private set
	{		if (value == m_mdlMolde)	return;
		if (m_mdlMolde is not null)	{	_ = mMod.Raíces.Remove(m_mdlMolde); Settings.Default.MoldeVisi = m_mdlMolde.Visible;}
		m_mdlMolde = value; mMod.Raíces.Add(m_mdlMolde); m_mdlMolde.Visible = Settings.Default.MoldeVisi;
		u3dMolde.Model = m_mdlMolde; clbMoldeColor.Color = m_mdlMolde.Material.Color; wMain.Refresca();
	}
}
public Color ColorAmbiente									=>	clbAmbiente.Color;
// ** Mets
private void u3dMolde_ModelChanged(object sender)			{	wMain.Refresca();}
private void u3dMolde_ModelInvalidated(object sender)		{	wMain.Refresca();}
private void clbMoldeColor_ColorChanged(object sender)		{	m_mdlMolde.Material.Color = clbMoldeColor.Color;wMain.Refresca();}
private void btnMoldeAgrega_Click(object sender)
{	string? s;

	s = mDialog.ShowOpenFile(mMod.FILTRO_MDL, mMod.DLG_GUID_MDL);	if (s is null)	return;
	try
	{	ml_mdlMoldes.Add(new cModelo(s, true));
	} catch (System.Exception ex)
	{	mDialog.MsgBoxExclamation(ex.Message, "Error");
	}
	lbMolde.Load(ml_mdlMoldes);
}
private void btnMoldeQuita_Click(object sender)
{	bool bCambiarSel;

		if (ml_mdlMoldes.Count <= 1 || lbMolde.SelectedItem is null)	return;
	bCambiarSel = (lbMolde.SelectedValue == m_mdlMolde);
	ml_mdlMoldes.Remove((cModelo?)lbMolde.SelectedValue); lbMolde.Load(ml_mdlMoldes, null, true);
	if (bCambiarSel)	Molde = ml_mdlMoldes[0];
}
private void btnMoldeBusca_Click(object sender)
{	wMain.Cam.Frame(m_mdlMolde, -Vector.ZAxis, Vector.YAxis);
}
private void lbMolde_ItemActivated(object sender)			{	Molde = (cModelo)lbMolde.SelectedValue!;}
private void chkClipEnabled_CheckStateChanged(object sender)	{	Clip_Changed(null);}
private void Clip_Changed(object? sender)
{	/*Matrix4x4 mm= wMain.visVista.Camera.TransposedViewProjectionMatrix.GetTransposed();*/ //¿
		mMod.ConstBuf.Data.Plane.Normal = Vector.FromAngles(uslPitch.Value, uslYaw.Value)/*.GetNormalized().GetTransformed(ref mm).GetNormalized()*/;
	mMod.ConstBuf.Data.Plane.Distance = (chkClipEnabled.Checked ? uslDist.Value : 1e+20f);
	mMod.ConstBuf.Write(); wMain.Refresca();
}
private void Luz_Changed(object sender)						{	wMain.Refresca();}
}
using Wew.Control;
using Wew.Media;
using Wew;

class wMain : wDocumentWindow
{	uFigs? m_figDlg;
	readonly cToolButton tbbVerPuntos, tbbMoverParte, tbbMoverPunto;
	readonly cToolComboBox cboParte;
	readonly cVista visVista;
	readonly cLabel lblInfo;
// ** Estat
		#pragma warning disable CS8618											// Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de declararlo como que admite un valor NULL.
	public static /*readonly*/ cCamera Cam;
	public static /*readonly*/ cTexture TexEnvCube;
		#pragma warning restore CS8618
	public static /*readonly*/ cSunLight LuzAtrás = new ()	{	Color = new Color(0.1f, 0.1f, 0.1f)};
	public static cModelo? MdlRaíz;
	public static bool PuntosVisi;
	public static cModelo MdlSel							{	set	=>	mMod.MainWnd.cboParte.SelectedValue = value;}
	public static bool MoverParte							=>	mMod.MainWnd.tbbMoverParte.Checked;
	public static bool MoverPunto							=>	mMod.MainWnd.tbbMoverPunto.Checked;
	public static void Refresca()							{	mMod.MainWnd.visVista.Invalidate();}
	public static void RefrescaNombs()						{	mMod.MainWnd.cboParte.Load(mMod.Partes, null, true);}
	public static void RefrescaListaPartes()
	{	mMod.MainWnd.cboParte.Load(mMod.Partes, null, true);
			if (mMod.MainWnd.cboParte.SelectedIndex == -1 && mMod.Partes.Count != 0)	mMod.MainWnd.cboParte.SelectedIndex = 0;
		mMod.MainWnd.cboParte_SelectionChanged(null); mMod.MainWnd.visVista.Invalidate();
	}
// ** Ctor/dtor
public wMain()
{	cToolBar tb; cToolButton tbb; cStatusBar sb;
	
	Icon = eRes.BmpCubo;
	WindowState = eWindowState.Maximized;
	mMod.MainWnd = this;
	// ** Cfg 3d
	mMod.ConstBuf = new c3DSimpleBuffer<mMod.BufConst>(this, e3DBind.ConstantBuffer);
		mMod.ConstBuf.AssignToVS((int)eSlot.Glob); mMod.ConstBuf.AssignToHS((int)eSlot.Glob);
	VSStandard.Dispose(); VSStandard = new cVertexShader(this, mShaders.VSStd, VertexBone.InputLayout);
	mMod.PsSimple = new cPixelShader(this, mShaders.PSSimple);
	TexEnvCube = new cTexture(this, GetType(), "Res.EnvCube.dds");
	SunLight.AbsoluteRotation = new Vector(0, 180, 0); SunLight.Color = new Color(0.1f, 0.1f, 0.1f);
	LuzAtrás.AbsoluteRotation = new Vector(0, 0, 0);
	// ** Cfg cmds
	LoadCommands(this, new cCommandState(eCommand.Copy, s_Copiar_Exec), new cCommandState(eCommand.Paste, Pegar_Exec));
	// ** Cfg ctls
	tb = new cToolBar {	Height = 36};
		tbb = tb.Items.Add("New", eBitmap.BmpNew, Nuevo_Click, eKey.ControlModifier | eKey.N);
			tbb.Type = eButtonType.Split;
			tbb.Menu = new cMenu();
			_ = tbb.Menu.Items.Add("Add empty container", null, AgregarVacío_Click);
			_ = tbb.Menu.Items.Add("Add figure", null, AgregarFig_Click);
		tbb = tb.Items.Add("Open", eBitmap.BmpOpen, Abrir_Click, eKey.ControlModifier | eKey.O);
			tbb.Type = eButtonType.Split;
			tbb.Menu = new cMenu();
			_ = tbb.Menu.Items.Add("Add model", null, AgregarMdl_Click);
		tbb = tb.Items.Add("Save", eBitmap.BmpSave, Grabar_Click, eKey.ControlModifier | eKey.S);
			tbb.Type = eButtonType.Split;
			tbb.Menu = new cMenu();
			_ = tbb.Menu.Items.Add("Save as...", null, GrabarComo_Click);
		tb.Add(eCommand.Separator, eCommand.Copy, eCommand.Paste, eCommand.Separator);
		_ = tb.Items.Add("Part", eRes.BmpParte, Parte_Click, eKey.ControlModifier | eKey.P);
		_ = tb.Items.Add("Point", eRes.BmpPunto, Punto_Click, eKey.ControlModifier | eKey.I);
		_ = tb.Items.Add("Tools", eRes.BmpHerrs, Herrs_Click, eKey.ControlModifier | eKey.T);
		_ = tb.Items.Add("Right view", eRes.BmpVerDer, Der_Click, eKey.AltModifier | eKey.Right);
		_ = tb.Items.Add("Left view", eRes.BmpVerIzq, Izq_Click, eKey.AltModifier | eKey.Left);
		_ = tb.Items.Add("Front view", eRes.BmpVerFrente, Frente_Click, eKey.AltModifier | eKey.F);
		_ = tb.Items.Add("Back view", eRes.BmpVerFondo, Fondo_Click, eKey.AltModifier | eKey.B);
		_ = tb.Items.Add("Top view", eRes.BmpVerSup, Sup_Click, eKey.AltModifier | eKey.Up);
		_ = tb.Items.Add("Bottom view", eRes.BmpVerInf, Inf_Click, eKey.AltModifier | eKey.Down);
		tbbMoverParte = tb.Items.Add("Move part", eRes.BmpManoParte, MoverParte_Click, eKey.None, "Move part (press Alt to move)");
			tbbMoverParte.Type = eButtonType.Check;
		tbbMoverPunto = tb.Items.Add("Move point", eRes.BmpManoPunto, MoverPunto_Click, eKey.None, "Move point (press Alt to move)");
			tbbMoverPunto.Type = eButtonType.Check;
		tb.Add(eCommand.Separator);
		cboParte = new cToolComboBox {	Label = "Selected part", ShowLabel = true, Width = 250};
			cboParte.SelectionChanged += cboParte_SelectionChanged;
			tb.Items.Add(cboParte);
		tbbVerPuntos = tb.Items.Add("Show points", eRes.BmpPuntos, VerPuntos_Click, eKey.AltModifier | eKey.V);
			tbbVerPuntos.Type = eButtonType.Check;
		tbb = tb.Items.Add("Animate", eRes.BmpPlay, Anim_Click, eKey.F5);
			tbb.Type = eButtonType.Check;
		Root.AddControl(tb); ToolStrips.Add(tb);
	visVista = new cVista {	Margins = new Rect(0)};
		Root.AddControl(visVista);
	Cam = visVista.Camera;
	sb = new cStatusBar();
		lblInfo = sb.AddLabel("Properties", 0); lblInfo.RightMargin = 0;
		Root.AddControl(sb);
	// ** Cfg dockctls
	mMod.PropsParte = new kParte {	Container = Root};
	mMod.PropsPunto = new kPunto {	Container = Root};
	mMod.Herrs = new kHerrs {	Container = Root};
	mMod.PropsParte.Ini();
	if (Settings.Default.VerParte)	Parte_Click(null);
	if (Settings.Default.VerPunto)	Punto_Click(null);
	if (Settings.Default.VerClip)	Herrs_Click(null);
	if (Settings.Default.VerPuntos)	tbbVerPuntos.PerformClick();
	// ** Abrir (o crear nuevo)
	r_FileOpenFilter = mMod.FILTRO_MDLS; r_FileOpenGuid = mMod.DLG_GUID_MDL;
	r_FileSaveFilter = mMod.FILTRO_MDL; r_FileSaveGuid = mMod.DLG_GUID_MDL;
	ProcessCommandLine();
}
protected override void OnClosed(eCloseReason reason)
{	Settings.Default.VerParte = mMod.PropsParte.Visible; Settings.Default.VerPunto = mMod.PropsPunto.Visible;
	Settings.Default.VerClip = mMod.Herrs.Visible;
	Settings.Default.VerPuntos = PuntosVisi;
	Settings.Default.Save();												// Guardar cfg
	m_Reini();
	base.OnClosed(reason);
}
// ** Mets
protected override void OnNew()								{	m_Reini(); Frente_Click(null);}
protected override bool OnOpen(string FileName)				{	m_Abre(FileName); return true;}
protected override bool OnSave()							{	MdlRaíz!.RecalcLims(); MdlRaíz.Save(r_FileName!); return true;}
private void Nuevo_Click(object? sender)					{	r_New();}
private void AgregarVacío_Click(object sender)				{	m_AgregaMdl(new cModelo([], 0, [], 0), true);}
private void AgregarFig_Click(object sender)
{	cModelo? mdl;

	m_figDlg ??= new uFigs();
	mdl = m_figDlg.ShowDialog();	if (mdl is not null)	m_AgregaMdl(mdl, true);
}
private void Abrir_Click(object sender)						{	r_Open();}
private void AgregarMdl_Click(object sender)
{	string? s;

	s = mDialog.ShowOpenFile(mMod.FILTRO_MDLS, mMod.DLG_GUID_MDL);	if (s is null) return;
	m_Abre(s, true);
}
private void Grabar_Click(object sender)					{	m_PideGrab(false);}
private void GrabarComo_Click(object sender)				{	m_PideGrab(true);}
private static void s_Copiar_Exec(cCommand command, object? args = null)
{	if (mMod.PropsParte.Parte is not null)	mClipboard.SetDataObject(mMod.PropsParte.Parte.GetDataObject());
}
private void Pegar_Exec(cCommand command, object? args = null)
{	cModelo? mdl;

	mdl = cModelo.FromDataObject();	if (mdl is not null) m_AgregaMdl(mdl, true);
}
private void Parte_Click(object? sender)					{	mMod.PropsParte.Show();}
private void Punto_Click(object? sender)					{	mMod.PropsPunto.Show(mMod.PropsParte);}
private void Herrs_Click(object? sender)					{	mMod.Herrs.Show(mMod.PropsParte);}
private void Der_Click(object sender)						{	visVista.VerDer();}
private void Izq_Click(object sender)						{	visVista.VerIzq();}
private void Frente_Click(object? sender)					{	visVista.VerFrente();}
private void Fondo_Click(object sender)						{	visVista.VerFondo();}
private void Sup_Click(object sender)						{	visVista.VerSup();}
private void Inf_Click(object sender)						{	visVista.VerInf();}
private void MoverParte_Click(object sender)
{	if (tbbMoverParte.Checked)	{	tbbMoverPunto.Checked = false; visVista.Cursor = eRes.CurManoParte;}	else	visVista.Cursor = null;
}
private void MoverPunto_Click(object sender)
{	if (tbbMoverPunto.Checked)	{	tbbMoverParte.Checked = false; visVista.Cursor = eRes.CurManoPunto;}	else	visVista.Cursor = null;
}
private void cboParte_SelectionChanged(object? sender)		{	mMod.PropsParte.Parte = (cModelo)cboParte.SelectedValue!;}
private void VerPuntos_Click(object sender)
{	PuntosVisi = !PuntosVisi;	foreach (cModelo mdl in mMod.Partes)	mdl.MuestraPuntos();
}
private void Anim_Click(object sender)						{	visVista.Anim();}
private void m_Abre(string sRuta, bool bAgregar = false)
{try
{	if (!bAgregar)	m_Reini();												// Abrir: cerrar primero
	m_AgregaMdl(cModelo.Abre(sRuta), bAgregar); mMod.PropsParte.CargaHuesos(); // ** Abrir mdl
	Frente_Click(null);
} catch (System.Exception ex)
{	mDialog.MsgBoxExclamation(ex.Message, "Error");
}
}
private void m_AgregaMdl(cModelo mdlMdl, bool bAgregar)
{	string sNomb; int i = 0;

	if (MdlRaíz is null)	{	MdlRaíz = mdlMdl; mMod.Raíces.Add(mdlMdl);}	else	mMod.PropsParte.Parte!.Parts.Add(mdlMdl);
	// ** Listar partes
	foreach (cModelo mdl in mMod.Partes)									// ** Asig nomb a las que no tienen
	{		if (mdl.Name is not null)	continue;							// Ya tiene nomb: omitir
		do	{	i++; sNomb = "Part " + i;} while (mMod.Partes.Find((cModelo mdl3) => (mdl3.Name == sNomb)) is not null); // Crear nomb correl
		mdl.Name = sNomb;
	}
	mMod.Partes.Sort((a, b) => string.Compare(a.Name, b.Name, System.StringComparison.CurrentCulture));
	cboParte.Load(mMod.Partes, null, true);
	cboParte.SelectedValue = mdlMdl; cboParte_SelectionChanged(null); visVista.Invalidate();
	if (bAgregar)	Changed = true;											// Agregado
	System.GC.Collect(); System.GC.WaitForPendingFinalizers();
}
private void m_PideGrab(bool bGrabComo)
{	if (MdlRaíz is null)	{	mDialog.MsgBoxExclamation("Nothing to save.  Add a model", "Save"); return;}
	_ = r_Save(bGrabComo || System.IO.Path.GetExtension(r_FileName)?.ToUpperInvariant() != ".MDL");
}
private void m_Reini()
{	if (MdlRaíz is not null)	
	{	mMod.PropsParte.Parte = null; mMod.PropsPunto.PuntosSel = null;		// Descargar mdls (excepto las herrs)
		MdlRaíz.Dispose(); MdlRaíz = null;
	}
	cModelo.LiberaTexturas(); mMod.PropsParte.LimpiaHuesos();
	cboParte.Load(null); System.GC.Collect(); System.GC.WaitForPendingFinalizers();
}
}
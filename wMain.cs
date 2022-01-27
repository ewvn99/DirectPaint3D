using CultInf = System.Globalization.CultureInfo;
using Wew.Control;
using Wew.Media;

namespace DirectPaint3D
{
partial class wMain : cWindow
{	string m_sRuta;
	uFigs m_figDlg;
	cLabel lblInfo;
	readonly cToolButton tbbVerPuntos;
// ** Estat
	/*readonly*/ static cToolComboBox cboParte;
	public static cTexture TexEnvCube;
	public static cSunLight LuzAba = new cSunLight() {	Color = new Color(0.1f, 0.1f, 0.1f)};
	public /*readonly*/ static cVista visVista;
	public static cModelo MdlRaíz;
	public static bool PuntosVisi;
	public /*readonly*/ static cToolButton tbbMoverParte, tbbMoverPunto;
	public static cModelo MdlSel
	{	set													{	cboParte.SelectedItem = value;}
	}
	public static void Refresca()							{	visVista.Invalidate();}
	public static void RefrescaNombs()						{	cboParte.Refresh();}
	public static void RefrescaListaPartes()
	{	cboParte.Refresh();	if (cboParte.SelectedIndex == -1 && mMod.Partes.Count != 0)	cboParte.SelectedIndex = 0;
		mMod.MainWnd.cboParte_SelectionChanged(null); visVista.Invalidate();
	}
// ** Ctor/dtor
public wMain()
{	cToolBar tb; cToolButton tbb; cStatusBar sb; string[] a_s;
	
	Icon = mRes.BmpCubo;
	WindowState = eWindowState.Maximized;
	mMod.MainWnd = this;
	// ** Cfg 3d
	VSStandard = new cVertexShader(this, mShaders.VSStd, ILStandard);
	mMod.PsSimple = new cPixelShader(this, mShaders.PSSimple);
	TexEnvCube = new cTexture(this, GetType(), "Graf.EnvCube.dds");
	SunLight.LookAt(new Vector(-1, -1, -1)); SunLight.Color = new Color(0.1f, 0.1f, 0.1f);
	LuzAba.LookAt(new Vector(1, 1, 1));
	mMod.Modelos = new System.Collections.Generic.List<c3DModel>();
	mMod.Partes = new System.Collections.Generic.List<cModelo>();
	// ** Cfg cmds
	eCommand.Cut.Icon = mRes.BmpCortar;
	eCommand.Copy.Icon = mRes.BmpCopiar;
	eCommand.Paste.Icon = mRes.BmpPegar;
	eCommand.Undo.Icon = mRes.BmpDeshacer;
	eCommand.Redo.Icon = mRes.BmpRehacer;
	LoadCommands(this, new cCommandState(eCommand.Copy, Copiar_Exec), new cCommandState(eCommand.Paste, Pegar_Exec));
	// ** Cfg ctls
	tb = new cToolBar() {	Height = 36};
		tbb = tb.Items.Add("New", mRes.BmpNuevo, Nuevo_Click, eKey.ControlModifier | eKey.N);
			tbb.Type = eButtonType.Split;
			tbb.Menu = new cMenu();
			tbb.Menu.Items.Add("Add empty container", null, AgregarVacío_Click);
			tbb.Menu.Items.Add("Add figure", null, AgregarFig_Click);
		tbb = tb.Items.Add("Open", mRes.BmpAbrir, Abrir_Click, eKey.ControlModifier | eKey.O);
			tbb.Type = eButtonType.Split;
			tbb.Menu = new cMenu();
			tbb.Menu.Items.Add("Add model", null, AgregarMdl_Click);
		tbb = tb.Items.Add("Save", mRes.BmpGrabar, Grabar_Click, eKey.ControlModifier | eKey.S);
			tbb.Type = eButtonType.Split;
			tbb.Menu = new cMenu();
			tbb.Menu.Items.Add("Save as...", null, GrabarComo_Click);
		tb.Add(eCommand.Separator, eCommand.Copy, eCommand.Paste, eCommand.Separator);
		tb.Items.Add("Part", mRes.BmpParte, Parte_Click, eKey.ControlModifier | eKey.P);
		tb.Items.Add("Point", mRes.BmpPunto, Punto_Click, eKey.ControlModifier | eKey.I);
		tb.Items.Add("Tools", mRes.BmpHerrs, Herrs_Click, eKey.ControlModifier | eKey.T);
		tb.Items.Add("Right view", mRes.BmpVerDer, Der_Click, eKey.AltModifier | eKey.Right);
		tb.Items.Add("Left view", mRes.BmpVerIzq, Izq_Click, eKey.AltModifier | eKey.Left);
		tb.Items.Add("Front view", mRes.BmpVerFrente, Frente_Click, eKey.AltModifier | eKey.F);
		tb.Items.Add("Back view", mRes.BmpVerFondo, Fondo_Click, eKey.AltModifier | eKey.B);
		tb.Items.Add("Top view", mRes.BmpVerSup, Sup_Click, eKey.AltModifier | eKey.Up);
		tb.Items.Add("Bottom view", mRes.BmpVerInf, Inf_Click, eKey.AltModifier | eKey.Down);
		tbbMoverParte = tb.Items.Add("Move part", mRes.BmpManoParte, MoverParte_Click, eKey.None, "Move part (press Alt to move)");
			tbbMoverParte.Type = eButtonType.Check;
		tbbMoverPunto = tb.Items.Add("Move point", mRes.BmpManoPunto, MoverPunto_Click, eKey.None, "Move point (press Alt to move)");
			tbbMoverPunto.Type = eButtonType.Check;
		tb.Add(eCommand.Separator);
		cboParte = new cToolComboBox() {	Label = "Selected part", ShowLabel = true, Width = 250};
			cboParte.SelectionChanged += cboParte_SelectionChanged; tb.Items.Add(cboParte);
		tbbVerPuntos = tb.Items.Add("Show points", mRes.BmpPuntos, VerPuntos_Click, eKey.AltModifier | eKey.V);
			tbbVerPuntos.Type = eButtonType.Check;
		Root.AddControl(tb); ToolStrips.Add(tb);
	visVista = new cVista() {	Margins = new Rect(0)};
		Root.AddControl(visVista);
	sb = new cStatusBar();
		lblInfo = sb.AddLabel("Properties", 0); lblInfo.RightMargin = 0;
		Root.AddControl(sb);
	// ** Cfg dockctls
	mMod.ParteProps = new kParte(); mMod.ParteProps.Container = Root;
	mMod.PuntoProps = new kPunto(); mMod.PuntoProps.Container = Root;
	mMod.Herrs = new kHerrs(); mMod.Herrs.Container = Root;
	mMod.ParteProps.Ini();
	if (Settings.Default.VerParte)	Parte_Click(null);
	if (Settings.Default.VerPunto)	Punto_Click(null);
	if (Settings.Default.VerClip)	Herrs_Click(null);
	if (Settings.Default.VerPuntos)	tbbVerPuntos.PerformClick();
	// ** Abrir (o crear nuevo)
	a_s = System.Environment.GetCommandLineArgs();
		if (a_s.Length == 2)	m_Abre(a_s[1]);	else	Nuevo_Click(null);
}
protected override void OnClosed(eCloseReason reason)
{	Settings.Default.VerParte = mMod.ParteProps.Visible; Settings.Default.VerPunto = mMod.PuntoProps.Visible;
	Settings.Default.VerClip = mMod.Herrs.Visible;
	Settings.Default.VerPuntos = PuntosVisi;
	Settings.Default.Save();										// Guardar cfg
	base.OnClosed(reason);
}
// ** Mets
public override bool CanClose(eCloseReason reason = eCloseReason.User)
{	base.CanClose();														// Grabar docktools
		if (!Changed) return true;											// ** Ya está grabado: salir
	// ** Pedir confirmación
	switch (mDialog.MsgBoxQuestionCancel("Save changes?", "Save"))
	{	case false:		return true;										// No grabar: salir
		case null:		return false;										// No grabar y cancelar operación sgte: salir
	}
	// ** Grabar
	return m_bGraba();
}
protected override void OnChanged()
{	///m_ActualizaCmds();
	if (Changed)	visVista.Invalidate();									// Modi: invalidar
	Text = string.Format(CultInf.CurrentCulture, "DirectPaint 3D - {0}{1}", (m_sRuta ?? "<new>"), (Changed ? "*" : null));
}
private void Nuevo_Click(object sender)						{	if (CanClose()) {	m_Reini(null); 	Frente_Click(null);}}
private void AgregarVacío_Click(object sender)				{	m_AgregaMdl(new cModelo(new Vertex[0], 0, new int[0], 0), true);}
private void AgregarFig_Click(object sender)
{	if (m_figDlg == null)	m_figDlg = new uFigs();
	if (m_figDlg.ShowDialog())	m_AgregaMdl(m_figDlg.Modelo, true);
}
private void Abrir_Click(object sender)
{	string s;

	if (!CanClose()) return;												// No se pudo cerrar: salir
	s = mDialog.ShowOpenFile(mMod.FILTRO_MODELOS, mMod.DLG_GUID_MDL);	if (s == null) return;
	m_Abre(s);
}
private void AgregarMdl_Click(object sender)
{	string s;

	s = mDialog.ShowOpenFile(mMod.FILTRO_MODELOS, mMod.DLG_GUID_MDL);	if (s == null) return;
	m_Abre(s, true);
}
private void Grabar_Click(object sender)					{	m_bGraba();}
private void GrabarComo_Click(object sender)				{	m_bGraba(true);}
private static void Copiar_Exec(cCommandState command, object args = null)
{	if (mMod.ParteProps.Modelo != null)	mClipboard.SetDataObject(mMod.ParteProps.Modelo.GetDataObject());
}
private void Pegar_Exec(cCommandState command, object args = null)
{	cModelo mdl;

	mdl = cModelo.FromDataObject();	if (mdl != null) m_AgregaMdl(mdl, true);
}
private void Parte_Click(object sender)						{	mMod.ParteProps.Show();}
private void Punto_Click(object sender)						{	mMod.PuntoProps.Show(mMod.ParteProps);}
private void Herrs_Click(object sender)						{	mMod.Herrs.Show(mMod.ParteProps);}
private void Der_Click(object sender)						{	visVista.VerDer();}
private void Izq_Click(object sender)						{	visVista.VerIzq();}
private void Frente_Click(object sender)					{	visVista.VerFrente();}
private void Fondo_Click(object sender)						{	visVista.VerFondo();}
private void Sup_Click(object sender)						{	visVista.VerSup();}
private void Inf_Click(object sender)						{	visVista.VerInf();}
private void MoverParte_Click(object sender)
{	if (tbbMoverParte.Checked)	{	tbbMoverPunto.Checked = false; visVista.Cursor = mRes.CurManoParte;}	else	visVista.Cursor = null;
}
private void MoverPunto_Click(object sender)
{	if (tbbMoverPunto.Checked)	{	tbbMoverParte.Checked = false; visVista.Cursor = mRes.CurManoPunto;}	else	visVista.Cursor = null;
}
private void cboParte_SelectionChanged(object sender)		{	mMod.ParteProps.Modelo = (cModelo)cboParte.SelectedItem;}
private void VerPuntos_Click(object sender)
{	PuntosVisi = !PuntosVisi;	foreach (cModelo mdl in mMod.Partes)	mdl.MuestraPuntos();
}
private void m_Abre(string sRuta, bool bAgregar = false)
{try
{	if (!bAgregar)	m_Reini(sRuta);											// Abrir: cerrar primero
	m_AgregaMdl(cModelo.Abre(sRuta), bAgregar); Frente_Click(null);			// ** Abrir mdl
} catch (System.Exception ex)
{	mDialog.MsgBoxExclamation(ex.Message, "Error");
}
}
private void m_AgregaMdl(cModelo mdlMdl, bool bAgregar)
{	string sNomb; int i = 0;

	if (MdlRaíz == null)	{	MdlRaíz = mdlMdl; mMod.Modelos.Add(mdlMdl);}	else	mMod.ParteProps.Modelo.Parts.Add(mdlMdl);
	// ** Listar partes
	foreach (cModelo mdl2 in mMod.Partes)									// ** Asig nomb a las que no tienen
	{		if (mdl2.Name != null)	continue;								// Ya tiene nomb: omitir
		do	{	i++; sNomb = "Part " + i;} while (mMod.Partes.Find((cModelo mdl3) => (mdl3.Name == sNomb)) != null); // Crear nomb correl
		mdl2.Name = sNomb;
	}
	mMod.Partes.Sort((a, b) => string.Compare(a.Name, b.Name, System.StringComparison.CurrentCulture)); cboParte.Data = mMod.Partes;
	cboParte.SelectedItem = mdlMdl; cboParte_SelectionChanged(null); visVista.Invalidate();
	if (bAgregar)	Changed = true;											// Agregado
}
private bool m_bGraba(bool bGuardarComo = false)
{	string sRuta;

		if (MdlRaíz == null)	{	mDialog.MsgBoxExclamation("Nothing to save.  Add a model", "Save"); return false;}
	// ** Tomar ruta
	sRuta = (!bGuardarComo ? m_sRuta : null);
try
{	switch (System.IO.Path.GetExtension(sRuta)?.ToUpperInvariant())
	{	case ".MDL":
			break;
		default:															// ** No soportado (o nuevo): tomar otro fmt
			sRuta = mDialog.ShowSaveFile(mMod.FILTRO_MDL, mMod.DLG_GUID_MDL);	if (sRuta == null) return false; // Cancelado: salir
			m_sRuta = sRuta; Changed = true;
			break;
	}
	// ** Grabar
	MdlRaíz.RecalcLims(); MdlRaíz.Save(m_sRuta); Changed = false;
} catch (System.Exception ex)
{	mDialog.MsgBoxExclamation(ex.Message, "Error");
	return false;
}
	return true;
}
private void m_Reini(string sRuta)
{	mMod.ParteProps.Modelo = null; mMod.PuntoProps.PuntosSel = null;		// Descargar mdls (excepto las herrs)
	mMod.Partes.Clear(); cboParte.Refresh();
	if (MdlRaíz != null)	{	mMod.Modelos.Remove(MdlRaíz); MdlRaíz.Dispose(); MdlRaíz = null;}
	m_sRuta = sRuta;
	Changed = true; Changed = false;
}
}
}
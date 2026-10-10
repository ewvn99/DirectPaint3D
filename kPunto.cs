using Wew.Control;
using Wew.Media;

class kPunto : cDockControl
{	public struct PartePunto
	{	public cModelo Parte;
		public int Punto;
		public Vector PosAbs;
	}
	Wew.cList<PartePunto>? ml_ppPuntos;
	readonly uVector uvePos; readonly cLabel lblCantSel; readonly cContainer cntLayout;
// ** Ctor/dtor
public kPunto()
{	cScrollableControl scCli; cStackPanel spnCli; uPropertyGroup grp; uPropertySubgroup sgr;

	IsForm = false; Text = "Point"; Width = 373; Height = 463; HideOnClose = true; Dock = eDirection.Left; BackColor = eBrush.White;
	scCli = new cScrollableControl {	RightMargin = 0, BottomMargin = 0}; scCli.VerticalBar.Visible = true;
	spnCli = new cStackPanel {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
		grp = new uPropertyGroup {	Text = "Location"};
			uvePos = new uVector {	Text = "Location"}; uvePos.ValueChanged += uvePos_ValueChanged;
				grp.AddControl(uvePos);
			sgr = new uPropertySubgroup {	Text = "Selected points"};
				lblCantSel = new cLabel {	BorderStyle = eBorderStyle.Default, AutoSize = eAutoSize.None
						, TextAlignment = eTextAlignment.Center, Width = 80};
					sgr.AddControl(lblCantSel);
				grp.AddControl(sgr);
			spnCli.AddControl(grp);
		cntLayout = new cContainer {	RightMargin = 0, AutoSize = eAutoSize.Height};
			spnCli.AddControl(cntLayout);
	scCli.ClientArea.AddControl(spnCli);
	AddControl(scCli);
}
// ** Props
public Wew.cList<PartePunto>? PuntosSel										// Muestra los vals del primer punto sel
{	get														=>	ml_ppPuntos;
	set
	{	PartePunto pp; Vector v; cControl ctl;

		cntLayout.Controls.Clear();
		if (value is not null && value.Count != 0)							// Hay puntos
		{	pp = value[0];
			v = pp.Parte.getPositions(pp.Punto); uvePos.Value = v;
			pp.Parte.EditorVert.Carga(pp.Parte, pp.Punto);
			ctl = pp.Parte.EditorVert; ctl.RightMargin = 0; ctl.AutoSize = eAutoSize.Height;
				cntLayout.AddControl(ctl);
			lblCantSel.Text = value.Count.ToString();
		} else																// Sin puntos
		{	uvePos.Value = default; lblCantSel.Text = "0";
		}
		ml_ppPuntos = value;
	}
}
// ** Mets
public void Refresca()										{	PuntosSel = ml_ppPuntos;}
public void AplicaPos(Vector vPos)
{	cModelo? mdl = null; Vector vPosLocal = default;

	foreach (PartePunto pp in ml_ppPuntos!)									// Aplicar a toda la sel
	{	if (pp.Parte != mdl)												// ** Cambio de parte: quitar world
		{	mdl = pp.Parte; Matrix4x4 m44 = mdl.Matrix; vPosLocal = Vector.FromWorld(vPos, m44);
		}
		pp.Parte.ActualizaPunto(pp.Punto, vPosLocal);
	}
	mMod.MainWnd.Changed = true; Refresca();
}
private void uvePos_ValueChanged(object sender)
{		if (ml_ppPuntos is null || ml_ppPuntos.Count == 0)	return;
	foreach (PartePunto pp in ml_ppPuntos)	pp.Parte.ActualizaPunto(pp.Punto, uvePos.Value); // Aplicar a toda la sel
	mMod.MainWnd.Changed = true;
}
}
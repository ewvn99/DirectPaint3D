using Wew.Control;
using Wew.Media;
using CultInf = System.Globalization.CultureInfo;

namespace DirectPaint3D
{
public class kPunto : cDockControl
{	public struct PartePunto
	{	public cModelo Parte;
		public int Punto;
		public Vector PosAbs;
	}
	Wew.cList<PartePunto> ml_ppPuntos;
	uVector uvePos; cLabel lblCantSel; cContainer cntLayout;
// ** Ctor/dtor
public kPunto()
{	cStackPanel spnCli; uPropertyGroup grp; uPropertySubgroup sgr;

	IsChildForm = false; Text = "Point"; Width = 373; Height = 463; HideOnClose = true; Dock = eDirection.Left; BackColor = eBrush.White;
	spnCli = new cStackPanel() {	Direction = eDirection.Bottom, RightMargin = 1, AutoSize = eAutoSize.Height};
	grp = new uPropertyGroup() {	Text = "Location"};
		uvePos = new uVector() {	Text = "Location"}; uvePos.ValueChanged += uvePos_ValueChanged;
			grp.AddControl(uvePos);
		sgr = new uPropertySubgroup() {	Text = "Selected points"};
			lblCantSel = new cLabel() {	BorderStyle = eBorderStyle.Default, AutoSize = eAutoSize.None
					, TextAlignment = eTextAlignment.Center, Width = 80};
				sgr.AddControl(lblCantSel);
			grp.AddControl(sgr);
		spnCli.AddControl(grp);
	cntLayout = new cContainer() {	RightMargin = 0, AutoSize = eAutoSize.Height};
		spnCli.AddControl(cntLayout);
	AddControl(spnCli);
}
// ** Props
public Wew.cList<PartePunto> PuntosSel										// Muestra los vals del primer punto sel
{	get														{	return ml_ppPuntos;}
	set
	{	PartePunto pp; Vector v; cControl ctl;

		cntLayout.Controls.Clear();
		if ((value?.Count).GetValueOrDefault() != 0)						// Hay puntos
		{	pp = value[0];
			v = pp.Parte.getPositions(pp.Punto); uvePos.Value = v;
			ctl = pp.Parte.EditorVert.Carga(pp.Parte, pp.Punto); ctl.LeftMargin = ctl.RightMargin = 0;
				cntLayout.Controls.Add(ctl);
			lblCantSel.Text = value.Count.ToString(CultInf.CurrentCulture);
		} else																// Sin puntos
		{	uvePos.Value = Vector.Zero; lblCantSel.Text = "0";
		}
		ml_ppPuntos = value;
	}
}
// ** Mets
public void Refresca()										{	PuntosSel = PuntosSel;}
public void AplicaPos(Vector vPos)
{	cModelo mdl = null; Vector vPosLocal = default(Vector);

	foreach (PartePunto pp in ml_ppPuntos)									// Aplicar a toda la sel
	{	if (pp.Parte != mdl)												// ** Cambio de parte: quitar world
		{	mdl = pp.Parte; Matrix4x4 m44 = mdl.Matrix; vPosLocal = Vector.FromWorld(vPos, ref m44);
		}
		pp.Parte.ActualizaPunto(pp.Punto, vPosLocal);
	}
	mMod.MainWnd.Changed = true; Refresca();
}
private void uvePos_ValueChanged(object sender)
{		if ((ml_ppPuntos?.Count).GetValueOrDefault() == 0)	return;
	foreach (PartePunto pp in ml_ppPuntos)	pp.Parte.ActualizaPunto(pp.Punto, uvePos.Value); // Aplicar a toda la sel
	mMod.MainWnd.Changed = true;
}
}
}
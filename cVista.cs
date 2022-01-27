using Wew.Control;
using Wew.Media;
using Vector = Wew.Media.Vector;
using Math = System.Math;

namespace DirectPaint3D
{
class cVista : cRenderControl
{// ** Tps
	enum ePres
	{	Ninguno	= 0,
		Cam,
		Parte,
		Molde,
		Punto
	}
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	struct VertInstanGrid
	{	public const int TAM	= 28;
		public float Desp, DespSemiEje;
		public int Perpend;
		public Color Color;
	}
	class cCam : cCamera
	{	public cCam() : base(1300, 700)						{}
		protected override void OnPositionChanged()
		{	base.OnPositionChanged();	if (mMod.Partes == null)	return;
			for (int i = 0, iCant = mMod.Partes.Count; i < iCant; i++)	mMod.Partes[i].AplicaMatCam(); // Hacer que los puntos miren a la cam
			wMain.visVista.Invalidate();
		}
	}
// ** Cpos
	ePres m_preMousePres; Point m_ptMouseAnt; Plane m_plPlanoPres; Vector m_vPres, m_vRayoPresDesp;
	c3DModel m_mdlGrid;
// ** Ctor/dtor
public cVista()
{	VertInstanGrid[] a_vig; int i, j, iDesp;

	BackColor = eBrush.LightSteelBlue; FocusMode = eFocusMode.FocusControl;
	// ** Crear grid de 1m x 1m
	a_vig = new VertInstanGrid[104];
		j = 0; iDesp = -25 * 10;
		// ** Paralelas a X
		for (i = 0; i < 50; i++, j++)	{	a_vig[j].Color = eColor.LightGray; a_vig[j].Desp = iDesp + i * 10;}
		a_vig[25].Color = eColor.Red; a_vig[25].DespSemiEje = 50 * 10;		// +X
		a_vig[100] = a_vig[25]; a_vig[100].Color = eColor.Pink; a_vig[100].DespSemiEje = -50 * 10; // -X
		// ** Paralelas a Z
		for (i = 0; i < 50; i++, j++)	{	a_vig[j].Perpend = 2; a_vig[j].Color = eColor.DarkGray; a_vig[j].Desp = iDesp + i * 10;}
		a_vig[75].Color = eColor.Blue; a_vig[75].DespSemiEje = 50 * 10;		// +Z
		a_vig[101] = a_vig[75]; a_vig[101].Color = eColor.Cyan; a_vig[101].DespSemiEje = -50 * 10; // -Z
		// ** Eje Y
		a_vig[102].Perpend = 1; a_vig[102].Color = eColor.Green; a_vig[102].DespSemiEje = 50 * 10; // +Y
		a_vig[103].Perpend = 1; a_vig[103].Color = eColor.DarkGreen; a_vig[103].DespSemiEje = -50 * 10; // -Y
	m_mdlGrid = new c3DModel(mMod.MainWnd
			, new VertexSolidColor[] {	new VertexSolidColor(-500, 0, 0), new VertexSolidColor(500, 0, 0)}, VertexSolidColor.SIZE, 2
			, new short[] {	0, 1}, 2, 2)
		{	Topology = eVertexTopology.Line, Name = "grid", ShadowModel = null
		};
		m_mdlGrid.CreateInstances(a_vig.Length, a_vig, VertInstanGrid.TAM, a_vig.Length
			, new cVertexShader(mMod.MainWnd, mShaders.VSGrid
				, new cVertexShader.Field[]
					{	new cVertexShader.Field(cVertexShader.eFieldType.Position) // 1° buf
						, new cVertexShader.Field("DESP", 0, eVertexFieldType.Float, 1) // 2° buf
						, new cVertexShader.Field("DESP_SEMIEJE", 0, eVertexFieldType.Float, 1)
						, new cVertexShader.Field("PERPEND", 0, eVertexFieldType.Int, 1)
						, new cVertexShader.Field(cVertexShader.eFieldType.Color, 0, 1)
					})
			, mMod.PsSimple);
		m_mdlGrid.Material.Type = eMaterial.Flat;
		mMod.Modelos.Add(m_mdlGrid);
	// ** Crear cam
	Camera = new cCam();
}
// ** Mets
public void VerDer()
{		if (mMod.ParteProps.Modelo == null)
		{	Camera.Location = new Vector(20, 0, 0); Camera.AbsoluteRotation = new Vector(); Camera.Yaw = -90; return;
		}
	Camera.Frame(mMod.ParteProps.Modelo, -Vector.XAxis, Vector.YAxis);
}
public void VerIzq()
{		if (mMod.ParteProps.Modelo == null)
		{	Camera.Location = new Vector(-20, 0, 0); Camera.AbsoluteRotation = new Vector(); Camera.Yaw = 90; return;
		}
	Camera.Frame(mMod.ParteProps.Modelo, Vector.XAxis, Vector.YAxis);
}
public void VerFrente()
{		if (mMod.ParteProps.Modelo == null)
		{	Camera.Location = new Vector(0, 0, 20); Camera.AbsoluteRotation = new Vector(); Camera.Yaw = 180; return;
		}
	Camera.Frame(mMod.ParteProps.Modelo, -Vector.ZAxis, Vector.YAxis);
}
public void VerFondo()
{		if (mMod.ParteProps.Modelo == null)
		{	Camera.Location = new Vector(0, 0, -20); Camera.AbsoluteRotation = new Vector(); Camera.Yaw = 0; return;
		}
	Camera.Frame(mMod.ParteProps.Modelo, Vector.ZAxis, Vector.YAxis);
}
public void VerSup()
{		if (mMod.ParteProps.Modelo == null)
		{	Camera.Location = new Vector(0, 10, 0); Camera.AbsoluteRotation = new Vector(); Camera.Pitch = -90; return;
		}
	Camera.Frame(mMod.ParteProps.Modelo, -Vector.YAxis, -Vector.ZAxis);
}
public void VerInf()
{		if (mMod.ParteProps.Modelo == null)
		{	Camera.Location = new Vector(0, -20, 0); Camera.AbsoluteRotation = new Vector(); Camera.Pitch = 90; Camera.Roll = 180; return;
		}
	Camera.Frame(mMod.ParteProps.Modelo, Vector.YAxis, -Vector.ZAxis);
}
protected override void OnRender(c3DGraphics g3d)
{	g3d.SetBlendState(g3d.TransparentBlend);
	g3d.AmbientColorFactor = new Color(0.9f, 0.9f, 0.9f); g3d.SetLight(wMain.LuzAba, 1);
	g3d.Render(mMod.Modelos);
}
protected override void OnMouseDown(MouseArgs e)
{	Vector vRayoOrig, vRayoDir, vPuntoMásCercano; Wew.cList<kPunto.PartePunto> l_ppPts = null; float fMenorDist; cModelo mdlCerca = null;

	m_preMousePres = ePres.Ninguno;
		if (e.Button != eMouseButton.Left)	return;
	m_ptMouseAnt = e.Location; m_preMousePres = ePres.Cam;
	Camera.CalculateRay(e.Location, out vRayoOrig, out vRayoDir);			// Calc rayo
	// ** Tomar sel
	if (wMain.PuntosVisi)	l_ppPts = new Wew.cList<kPunto.PartePunto>();
	fMenorDist = float.PositiveInfinity; vPuntoMásCercano = default(Vector);
	foreach (cModelo mdl in mMod.Partes)
	{	if (mdl.HitTest(vRayoOrig, vRayoDir, e.Location, l_ppPts, ref fMenorDist, ref vPuntoMásCercano))	mdlCerca = mdl; // Tomar mdl más cercano
	}
	if (mMod.Herrs.Molde.HitTest(vRayoOrig, vRayoDir, e.Location, l_ppPts, ref fMenorDist, ref vPuntoMásCercano)) // Molde presionado
	{	l_ppPts?.Clear(); mdlCerca = mMod.Herrs.Molde; m_preMousePres = ePres.Molde;
	}
	// ** Mostrar nueva sel de puntos (si no hay mantener la actual)
	if (wMain.PuntosVisi && l_ppPts.Count != 0)
	{	foreach (cModelo mdl in mMod.Partes)	mdl.LimpiaPuntosSel();		// Desel todo
		for (int i = 0; i < l_ppPts.Count; )	{	if (l_ppPts[i].PosAbs != vPuntoMásCercano)	l_ppPts.RemoveAt(i);	else	i++;} // Quitar puntos adyacentes
		foreach (kPunto.PartePunto pp in l_ppPts)	pp.Parte.setPuntoSel(pp.Punto, 1); // Sel lo nuevo
		mMod.PuntoProps.PuntosSel = l_ppPts;								// Mostrar vals de puntos sel
		if (wMain.tbbMoverPunto.Checked && e.Alt)	m_preMousePres = ePres.Punto; // ** Ini mov de punto
	}
	if (mdlCerca?.EsMolde == false)											// Sel parte más cercana
	{	wMain.MdlSel = mdlCerca; mMod.ParteProps.Modelo = mdlCerca;
		if (m_preMousePres != ePres.Punto && wMain.tbbMoverParte.Checked && e.Alt)	m_preMousePres = ePres.Parte; // Ini mov de parte
	}
	// ** Ini arrast
	if (!float.IsInfinity(fMenorDist))
	{	vRayoOrig.Add(vRayoDir, fMenorDist); m_vPres = vRayoOrig; m_plPlanoPres = new Plane(-Camera.ZAxis, vRayoOrig); // Crear superficie para arrast
		m_vRayoPresDesp = (mdlCerca.Container != null ? mdlCerca.Container.WorldToVector(vRayoOrig) : vRayoOrig) - mdlCerca.Location; // Calc desp de sujeción
	}
}
protected override void OnMouseMove(MouseArgs e)
{		if (e.Button != eMouseButton.Left)	return;
	switch (m_preMousePres)
	{	case ePres.Parte: case ePres.Molde:									// ** Mover parte o molde
			Vector vDest, v; cModelo mdl;

			vDest = Camera.CalculateRayTarget(e.Location, m_plPlanoPres);
			mdl = (m_preMousePres == ePres.Molde ? mMod.Herrs.Molde : mMod.ParteProps.Modelo);
			v = vDest;	if (mdl.Container != null)	{	v = mdl.Container.WorldToVector(v);} // Mover mdl: quitar world del cnt
				mdl.Location = v - m_vRayoPresDesp;
			if (!mdl.EsMolde)												// Parte movida
			{	mMod.MainWnd.Changed = true; mMod.ParteProps.RefrescaPos();
			} else if (e.Alt && mMod.ParteProps.Modelo?.Visible == true)	// ** Molde movido (y hábil): mover punto de mdl sel (y visible)
			{	mMod.ParteProps.Modelo.Moldea(vDest - m_vPres); m_vPres = vDest;
			}
			break;
		case ePres.Punto:													// ** Mover punto
			mMod.PuntoProps.AplicaPos(Camera.CalculateRayTarget(e.Location, m_plPlanoPres));
			break;
		case ePres.Cam:														// ** Mover cam
			Point ptDif;

			Camera.LookAt((mMod.ParteProps.Modelo?.AbsoluteCenter).GetValueOrDefault());
			ptDif = e.Location - m_ptMouseAnt;
			if (Math.Abs(ptDif.X) > Math.Abs(ptDif.Y))
				Camera.RotateY(ptDif.X / Width * 360, 20);
			else
			{	Camera.RotateX(ptDif.Y / Height * 360, 20);
			}
			m_ptMouseAnt = e.Location;
			break;
	}
}
protected override void OnKeyDown(ref KeyArgs e)
{	switch (e.Key)
	{	case eKey.Delete:	mMod.ParteProps.Elimina(false);
			return;
	}
	base.OnKeyDown(ref e);
}
}
}
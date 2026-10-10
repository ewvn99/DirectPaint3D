using Wew.Control;
using Wew.Media;
using Vector = Wew.Media.Vector;
using Math = System.Math;

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
	{	public const int SIZE	= 28;
		public static cVertexShader.Field[] InputLayout		=>				// Se usa sólo una vez
			[   new (eSemantic.Position)									// 1° buf
				, new ("DESP", 0, eVertexFieldType.Float, 1)				// 2° buf
				, new ("DESP_SEMIEJE", 0, eVertexFieldType.Float, 1)
				, new ("PERPEND", 0, eVertexFieldType.Int, 1)
				, new (eSemantic.Color, 0, 1)
			];
		public float Desp, DespSemiEje;
		public int Perpend;
		public Color Color;
	}
	class cCam : cCamera
	{	public cCam() : base(1300, 700, FarPlane: 10_000)	{}
		protected override void OnPositionChanged()
		{	base.OnPositionChanged();	if (mMod.Partes is null)	return;
			for (int i = 0, iCant = mMod.Partes.Count; i < iCant; i++)	mMod.Partes[i].AplicaMatCam(); // Hacer que los puntos miren a la cam
			wMain.Refresca();
		}
	}
// ** Cpos
	ePres m_preMousePres; Point m_ptMouseAnt; Plane m_plPlanoPres; Vector m_vPres, m_vRayoPresDesp;
	readonly c3DModel m_mdlGrid;
	readonly cStoryboard m_stoAni; readonly cFloatAnimation m_fanAniVar; readonly System.Diagnostics.Stopwatch m_swTiempo;
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
	m_mdlGrid = new c3DModel(mMod.MainWnd, new VertexColor[] {	new (-500, 0, 0), new (500, 0, 0)}, new short[] {	0, 1})
		{	Topology = eVertexTopology.Line, Name = "grid"
		};
		m_mdlGrid.CreateInstances(a_vig.Length, a_vig
			, new cVertexShader(mMod.MainWnd, mShaders.VSGrid, VertInstanGrid.InputLayout), PixelShader: mMod.PsSimple);
		m_mdlGrid.Material.Type = eMaterial.Flat;
		mMod.Raíces.Add(m_mdlGrid);
	// ** Crear cam
	Camera = new cCam();
	// ** Crear anim
	m_fanAniVar = new cFloatAnimation(); m_swTiempo = new System.Diagnostics.Stopwatch();
	m_stoAni = new cStoryboard(this) {	DisposeOnFinished = false};
		m_stoAni.RepeatStoryboard(m_stoAni.AddFloatLinearTransition(1, 1, m_fanAniVar), false);
}
public override void Close()			{	m_mdlGrid.VertexShader.Dispose(); m_mdlGrid.Dispose(); m_stoAni.Dispose(); base.Close();}
// ** Mets
public void VerDer()
{		if (mMod.PropsParte.Parte is null)
		{	Camera.Location = new Vector(20, 0, 0); Camera.AbsoluteRotation = default; Camera.Yaw = -90; return;
		}
	Camera.Frame(mMod.PropsParte.Parte, -Vector.XAxis, Vector.YAxis);
}
public void VerIzq()
{		if (mMod.PropsParte.Parte is null)
		{	Camera.Location = new Vector(-20, 0, 0); Camera.AbsoluteRotation = default; Camera.Yaw = 90; return;
		}
	Camera.Frame(mMod.PropsParte.Parte, Vector.XAxis, Vector.YAxis);
}
public void VerFrente()
{		if (mMod.PropsParte.Parte is null)
		{	Camera.Location = new Vector(0, 0, 20); Camera.AbsoluteRotation = default; Camera.Yaw = 180; return;
		}
	Camera.Frame(mMod.PropsParte.Parte, -Vector.ZAxis, Vector.YAxis);
}
public void VerFondo()
{		if (mMod.PropsParte.Parte is null)
		{	Camera.Location = new Vector(0, 0, -20); Camera.AbsoluteRotation = default; Camera.Yaw = 0; return;
		}
	Camera.Frame(mMod.PropsParte.Parte, Vector.ZAxis, Vector.YAxis);
}
public void VerSup()
{		if (mMod.PropsParte.Parte is null)
		{	Camera.Location = new Vector(0, 10, 0); Camera.AbsoluteRotation = default; Camera.Pitch = -90; return;
		}
	Camera.Frame(mMod.PropsParte.Parte, -Vector.YAxis, -Vector.ZAxis);
}
public void VerInf()
{		if (mMod.PropsParte.Parte is null)
		{	Camera.Location = new Vector(0, -20, 0); Camera.AbsoluteRotation = default; Camera.Pitch = 90; Camera.Roll = 180; return;
		}
	Camera.Frame(mMod.PropsParte.Parte, Vector.YAxis, -Vector.ZAxis);
}
public void Anim()
{	if (wMain.MdlRaíz?.Animating == false)	{	m_stoAni.Start(); m_swTiempo.Restart();}	else	{	m_stoAni.Abandon(); m_swTiempo.Stop();}
	if (wMain.MdlRaíz is null)	return;
	wMain.MdlRaíz.Animating = !wMain.MdlRaíz.Animating;
		foreach (cModelo mdl in mMod.Partes)	mdl.AnimaPuntos(wMain.MdlRaíz.Animating);
}
protected override void OnRender(c3DGraphics g3d)
{	if (wMain.MdlRaíz?.Animating == true && wMain.MdlRaíz.Animations is not null)
	{	c3DModel.cBoneAnimation ani = wMain.MdlRaíz.Animations["Take1"];

		ani.Update((float)m_swTiempo.Elapsed.TotalSeconds);
		if (m_swTiempo.Elapsed.TotalSeconds > ani.Duration)	m_swTiempo.Restart();
	}
	g3d.AmbientColorFactor = mMod.Herrs.ColorAmbiente; g3d.SetLight(wMain.LuzAtrás, 1);
	g3d.Render(mMod.Raíces);
}
protected override void OnMouseDown(MouseArgs e)
{	Vector vRayoOrig, vRayoDir, vPuntoMásCercano; Wew.cList<kPunto.PartePunto>? l_ppPts = null; float fMenorDist; cModelo? mdlCerca = null;

	m_preMousePres = ePres.Ninguno;
		if (e.Button != eMouseButton.Left)	return;
	m_ptMouseAnt = e.Location; m_preMousePres = ePres.Cam;
	Camera.CalculateRay(e.Location, out vRayoOrig, out vRayoDir);			// Calc rayo
	// ** Tomar sel
	if (wMain.PuntosVisi)	l_ppPts = [];
	fMenorDist = float.PositiveInfinity; vPuntoMásCercano = default;
	foreach (cModelo mdl in mMod.Partes)
	{	if (mdl.HitTest(vRayoOrig, vRayoDir, e.Location, l_ppPts, ref fMenorDist, ref vPuntoMásCercano))	mdlCerca = mdl; // Tomar mdl más cercano
	}
	if (mMod.Herrs.Molde.HitTest(vRayoOrig, vRayoDir, e.Location, l_ppPts, ref fMenorDist, ref vPuntoMásCercano)) // Molde presionado
	{	l_ppPts?.Clear(); mdlCerca = mMod.Herrs.Molde; m_preMousePres = ePres.Molde;
	}
	// ** Mostrar nueva sel de puntos (si no hay mantener la actual)
	if (wMain.PuntosVisi && l_ppPts!.Count != 0)
	{	foreach (cModelo mdl in mMod.Partes)	mdl.LimpiaPuntosSel();		// Desel todo
		for (int i = 0; i < l_ppPts.Count; )	{	if (l_ppPts[i].PosAbs != vPuntoMásCercano)	l_ppPts.RemoveAt(i);	else	i++;} // Quitar puntos adyacentes
		foreach (kPunto.PartePunto pp in l_ppPts)	pp.Parte.setPuntoSel(pp.Punto, true); // Sel lo nuevo
		mMod.PropsPunto.PuntosSel = l_ppPts;								// Mostrar vals de puntos sel
		if (wMain.MoverPunto && e.Alt)	m_preMousePres = ePres.Punto;		// ** Ini mov de punto
	}
	if (mdlCerca?.EsMolde == false)											// Sel parte más cercana
	{	wMain.MdlSel = mdlCerca; mMod.PropsParte.Parte = mdlCerca;
		if (m_preMousePres != ePres.Punto && wMain.MoverParte && e.Alt)	m_preMousePres = ePres.Parte; // Ini mov de parte
	}
	// ** Ini arrast
	if (!float.IsInfinity(fMenorDist))
	{	vRayoOrig.Add(vRayoDir, fMenorDist); m_vPres = vRayoOrig; m_plPlanoPres = new Plane(-Camera.ZAxis, vRayoOrig); // Crear superficie para arrast
		m_vRayoPresDesp = (mdlCerca!.Container is not null ? mdlCerca.Container.WorldToVector(vRayoOrig) : vRayoOrig) - mdlCerca.Location; // Calc desp de sujeción
	}
}
protected override void OnMouseMove(MouseArgs e)
{		if (e.Button != eMouseButton.Left)	return;
	switch (m_preMousePres)
	{	case ePres.Parte: case ePres.Molde:									// ** Mover parte o molde
			Vector vDest, v; cModelo mdl;

			vDest = Camera.CalculateRayTarget(e.Location, m_plPlanoPres);
			mdl = (m_preMousePres == ePres.Molde ? mMod.Herrs.Molde : mMod.PropsParte.Parte)!;
			v = vDest;	if (mdl.Container is not null)	{	v = mdl.Container.WorldToVector(v);} // Mover mdl: quitar world del cnt
				mdl.Location = v - m_vRayoPresDesp;
			if (!mdl.EsMolde)												// Parte movida
			{	mMod.MainWnd.Changed = true; mMod.PropsParte.RefrescaPos();
			} else if (e.Alt && mMod.PropsParte.Parte?.Visible == true)	// ** Molde movido (y hábil): mover punto de mdl sel (y visible)
			{	mMod.PropsParte.Parte.Moldea(vDest - m_vPres); m_vPres = vDest;
			}
			break;
		case ePres.Punto:													// ** Mover punto
			mMod.PropsPunto.AplicaPos(Camera.CalculateRayTarget(e.Location, m_plPlanoPres));
			break;
		case ePres.Cam:														// ** Mover cam
			Point ptDif;

			Camera.LookAt((mMod.PropsParte.Parte?.AbsoluteCenter).GetValueOrDefault());
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
	{	case eKey.Delete:	mMod.PropsParte.Elimina(false);
			return;
	}
	base.OnKeyDown(ref e);
}
}
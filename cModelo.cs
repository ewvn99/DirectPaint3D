using System.IO;
using System.Collections.Generic;
using System.Xml;
using Vector = Wew.Media.Vector;
using Array = System.Array;
using Wew;
using Wew.Media;
using Wew.Control;

namespace DirectPaint3D
{
public class cModelo : c3DModel
{// ** Tps
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	internal struct VertInstanPunto
	{	public const int TAM	= 16;
		public Vector Desp;
		public byte Sel;													// Si se usa bool, no se puede hacer pin con GCHandle
	}
	internal class cPunto : c3DModel										// ** No debe tener partes
	{// ** Tps
		struct Buf
		{	public Matrix4x4 World;
			public Color Color;
		}
	// ** Cpos
		cConstantBuffer<Buf> m_bbConst;
	// ** Estat
		static c3DModel s_mdlPunto; static cVertexShader s_vsPunto;
		static c3DModel s_Punto
		{	get
			{	if (s_mdlPunto == null)										// Ini recursos
				{	s_mdlPunto = new c3DModel(mMod.MainWnd, typeof(cPunto), "Graf.Punto.mdl");
					s_vsPunto = new cVertexShader(mMod.MainWnd, mShaders.VSPunto
						, new cVertexShader.Field[]
							{	new cVertexShader.Field(cVertexShader.eFieldType.Position) // 1° buf
								, new cVertexShader.Field("DESP", 0, eVertexFieldType.Vector, 1) // 2° buf
								, new cVertexShader.Field("SEL", 0, eVertexFieldType.Byte, 1)
							});
				}
				return s_mdlPunto;
			}
		}
	// ** Ctor/dtor
		internal cPunto(VertInstanPunto[] a_vipPuntos) : base(s_Punto)
		{	// ** Ini
			m_bbConst = new cConstantBuffer<Buf>(mMod.MainWnd);
			m_bbConst.Data.World = Matrix4x4.Identity; m_bbConst.Data.Color = eColor.Gray; m_bbConst.Update();
			mMod.Modelos.Add(this);
			// ** Crear instancias
			CreateInstances(a_vipPuntos.Length, a_vipPuntos, VertInstanPunto.TAM, a_vipPuntos.Length, s_vsPunto, mMod.PsSimple);
		}
		protected override void Dispose(bool disposing)		{	mMod.Modelos.Remove(this); base.Dispose(disposing);}
		// ** Mets
		public void AplicaSel(bool value)
		{	m_bbConst.Data.Color = (value ? eColor.LightBlue : eColor.Gray); m_bbConst.Update(); wMain.Refresca();
		}
		public void AplicaMat(ref Matrix4x4 value)
		{	m_bbConst.Data.World = value; m_bbConst.Update(); wMain.Refresca();
		}
		protected override void OnPrepareRender(c3DGraphics g3d)	{	base.OnPrepareRender(g3d); m_bbConst.AssignToVS(2);}
	}
	public abstract class cManVert
	{	public abstract cControl Carga(cModelo mdlParte, int iPunto);
	}
	internal class cMVPosNorm : cManVert
	{	uVector uveNorm;
		protected cStackPanel _spnCnt;
		public cMVPosNorm()
		{	uPropertyGroup grp;

			_spnCnt = new cStackPanel() {	Direction = eDirection.Bottom, BackColor = eBrush.White, AutoSize = eAutoSize.Height};
			grp = new uPropertyGroup() {	Text = "Normal"};
				uveNorm = new uVector() {	Text = "Normal", ReadOnly = true};
					grp.AddControl(uveNorm);
				_spnCnt.AddControl(grp);
		}
		public override cControl Carga(cModelo mdlParte, int iPunto)
		{	uveNorm.Value = mdlParte.ma_verVerts[iPunto].Normal;
			return _spnCnt;
		}
	}
	internal class cMVTex : cMVPosNorm
	{	uSlider uslTexcoordX, uslTexcoordY;
		public cMVTex()
		{	uPropertyGroup grp;

			grp = new uPropertyGroup() {	Text = "Texcoord"};
				uslTexcoordX = new uSlider() {	Text = "X", IsPercent = true};
					uslTexcoordX.ValueChanged += Tex_ValueChanged; grp.AddControl(uslTexcoordX);
				uslTexcoordY = new uSlider() {	Text = "Y", IsPercent = true};
					uslTexcoordY.ValueChanged += Tex_ValueChanged; grp.AddControl(uslTexcoordY);
				_spnCnt.AddControl(grp);
		}
		private static Point getTex(cModelo mdlParte, int iPunto)	{	return mdlParte.ma_verVerts[iPunto].Texcoord;}
		private static void setTex(cModelo mdlParte, int iPunto, Point value)	{	mdlParte.ma_verVerts[iPunto].Texcoord = value;}
		public override cControl Carga(cModelo mdlParte, int iPunto)
		{	Point pt;

			base.Carga(mdlParte, iPunto);
			pt = getTex(mdlParte, iPunto); uslTexcoordX.Value = pt.X; uslTexcoordY.Value = pt.Y;
			return _spnCnt;
		}
		private void Tex_ValueChanged(object sender)
		{	foreach (kPunto.PartePunto pp in mMod.PuntoProps.PuntosSel)		// Aplicar a toda la sel
			{	setTex(pp.Parte, pp.Punto, new Point(uslTexcoordX.Value, uslTexcoordY.Value)); // Actualizar en cpu
				pp.Parte.UpdateVertices(pp.Parte.ma_verVerts, pp.Punto, 1);	// Actualizar en gpu
			}
			mMod.MainWnd.Changed = true;
		}
	}
// ** Cpos
	Vertex[] ma_verVerts; int m_iVertStride; int[] ma_iInds;
	cPunto m_punPuntos; VertInstanPunto[] ma_vipInstan;						// Mdl punto.  Datos de instan (puntos)
	bool m_bSel;
	/*readonly*/ Triangle3D[] ma_triTriangs, ma_triTransform;				// Sólo para molde: triangs iniciales y transformados a coords de buf de la parte sel
	public /*readonly*/ cManVert EditorVert;
	public /*readonly*/ bool EsMolde; public /*readonly*/ string Ruta;		// Sólo para molde
// ** Estat
	static cMVPosNorm s_mvsSolid = new cMVPosNorm();
	static cMVTex s_mvsTex = new cMVTex();
	static bool s_bLimsCambiados;
	public static cModelo Abre(string sArchi)
	{	string sExt = Path.GetExtension(sArchi).ToUpperInvariant();

		switch (sExt)
		{	case ".3MF": case ".OBJ": case ".X":							// Importar: salir
				cModelo mdl;

				mdl = new cModelo() {	Name = Path.GetFileNameWithoutExtension(sArchi)}; // Crear cnt
				switch (sExt)
				{	case ".3MF":	mdl.m_Carga3mf(sArchi); break;
					case ".OBJ":	mdl.m_CargaObj(sArchi); break;
					case ".X":		mdl.m_CargaX(sArchi); break;
				}
				mdl.OnLoaded();
				return mdl;
			//case ".TXT":													// Importar de Frank Luna: salir
			//	System.Globalization.CultureInfo cui = System.Globalization.CultureInfo.InvariantCulture;
			//	string[] a_s, a_s2; int j, iCantVerts, iCantTris; Vertex[] a_ver; int[] a_i;
			//
			//	a_s = File.ReadAllLines(sArchi); j = 0;
			//	iCantVerts = int.Parse(a_s[j++].Split(' ')[1]);
			//	iCantTris = int.Parse(a_s[j++].Split(' ')[1]);
			//	j += 2;
			//	a_ver = new Vertex[iCantVerts];
			//	for (int i = 0; i < iCantVerts; i++, j++)
			//	{	a_s2 = a_s[j].Split(' ');
			//		a_ver[i].Position = new Vector(float.Parse(a_s2[0], cui), float.Parse(a_s2[1], cui), float.Parse(a_s2[2], cui));
			//		a_ver[i].Normal = new Vector(float.Parse(a_s2[3], cui), float.Parse(a_s2[4], cui), float.Parse(a_s2[5], cui));
			//	}
			//	j += 3;
			//	a_i = new int[iCantTris * 3];
			//	for (int i = 0; i < iCantTris; i++, j++)
			//	{	a_s2 = a_s[j].Split(' ');
			//		a_i[i * 3] = int.Parse(a_s2[0]); a_i[i * 3 + 1] = int.Parse(a_s2[1]); a_i[i * 3 + 2] = int.Parse(a_s2[2]);
			//	}
			//	return new cModelo(a_ver, a_ver.Length, a_i, a_i.Length) {	Name = Path.GetFileNameWithoutExtension(sArchi)}; // Crear mdl
		}
		return new cModelo(sArchi);											// Abrir
	}
	public static cModelo FromDataObject()
	{	return (cModelo)FromDataObject(null, mClipboard.GetDataObject(), (c3DModel mdlPad, int iIdxHijo) => new cModelo(), true);
	}
	private static void s_CalcNormalSmooth(Vertex[] a_verVers, int iV1, int iV2, int iV3)
	{	Vector vNorm;

		vNorm = ((a_verVers[iV3].Position - a_verVers[iV1].Position) * (a_verVers[iV2].Position - a_verVers[iV1].Position)).GetNormalized();
		s_AsigNormSmooth(a_verVers, iV1, vNorm);
		s_AsigNormSmooth(a_verVers, iV2, vNorm);
		s_AsigNormSmooth(a_verVers, iV3, vNorm);
	}
	private static void s_CalcNormalFlat(ref Vertex[] a_verVers, ref int iVerts, ref int iV1, ref int iV2, ref int iV3)
	{	Vector vNorm;

		vNorm = ((a_verVers[iV3].Position - a_verVers[iV1].Position) * (a_verVers[iV2].Position - a_verVers[iV1].Position)).GetNormalized();
		s_AsigNormFlat(ref a_verVers, ref iVerts, ref iV1, vNorm);
		s_AsigNormFlat(ref a_verVers, ref iVerts, ref iV2, vNorm);
		s_AsigNormFlat(ref a_verVers, ref iVerts, ref iV3, vNorm);
	}
	private static void s_AsigNormSmooth(Vertex[] a_verVers, int iV, Vector vNorm)
	{	if (!a_verVers[iV].Normal.IsNull)									// ** Ya tiene normal: promediar
			a_verVers[iV].Normal = a_verVers[iV].Normal.Lerp(vNorm, 0.5f).GetNormalized();
		else																// ** Aún no tiene normal: asig
			a_verVers[iV].Normal = vNorm;
	}
	private static void s_AsigNormFlat(ref Vertex[] a_verVers, ref int iVerts, ref int iV, Vector vNorm)
	{	if (a_verVers[iV].Normal.IsNull)									// ** Aún no tiene normal: asig
			a_verVers[iV].Normal = vNorm;
		else if (a_verVers[iV].Normal != vNorm)								// ** Tiene norm dif: duplicar vert
		{	if (iVerts == a_verVers.Length)	Array.Resize(ref a_verVers, iVerts * 2); // Expandir
			a_verVers[iVerts].Position = a_verVers[iV].Position; a_verVers[iVerts].Normal = vNorm; // Agregar vert.  Asig norm
			iV = iVerts; iVerts++;											// Actualizar idx
		}
	}
	private static string s_sCargaObjLeeLin(StreamReader srArchi)
	{	string sRes;

		sRes = srArchi.ReadLine()?.Trim();
		while (sRes != null && (sRes.Length == 0 || sRes.StartsWithOrdinal("#")))	sRes = srArchi.ReadLine(); // Avanzar; saltar espacios y coments
		return sRes;
	}
	private static float s_fWrapTexcoord(float fTc)							// Aplicar wrap a texcoord
	{	if (fTc > 1)	return (fTc == (int)fTc ? 1 : fTc % 1);				// Si es entero, es 1 sino la fracción
		if (fTc < 0)	return fTc == (int)fTc ? 0 : 1 + (fTc % 1);			// Si es entero, es 0 sino el complemento a 1
		return fTc;
	}
// ** Ctor/dtor
	public cModelo(string sArchi, bool bEsMolde = false) : base(mMod.MainWnd, sArchi, (c3DModel mdlPad, int iIdxHijo) => new cModelo(), true)
	{	OnLoaded();	if (bEsMolde)	m_CfgMolde(sArchi);
	}
	public cModelo(System.Type tpTp, string sRes) : base(mMod.MainWnd, tpTp, sRes, (c3DModel mdlPad, int iIdxHijo) => new cModelo(), true) // Crea molde
	{	OnLoaded(); m_CfgMolde("@" + sRes);
	}
	public cModelo(Vertex[] a_verVerts, int iVertCant, int[] a_iInds, int iIdxCant) // Crea fig
		: base(mMod.MainWnd, a_verVerts, Vertex.SIZE, iVertCant, a_iInds, 4, iIdxCant, true, eVertexLayout.PositionNormalTexcoord)
	{	OnLoaded();
	}
	private cModelo(Vertex[] a_verVerts, int iVertCant, int[] a_iInds, int iIdxCant, eVertexLayout vlVertLayReal)
		: base(mMod.MainWnd, a_verVerts, Vertex.SIZE, iVertCant, a_iInds, 4, iIdxCant, true, eVertexLayout.PositionNormalTexcoord)
	{	VertexData.Layout = vlVertLayReal;
	}
	private cModelo() : base(mMod.MainWnd, true)			{}
	protected override void Dispose(bool disposing)
	{	if (disposing)	m_punPuntos.Dispose();
		base.Dispose(disposing);
	}
// ** Props
	public bool Sel
	{	get													{	return m_bSel;}
		set													{	if (value != m_bSel)	{	m_bSel = value; m_punPuntos.AplicaSel(value);}}
	}
	public Vector getPositions(int index)					{	return ma_verVerts[index].Position;}
	public void setPuntoSel(int index, byte value)
	{		if (value == ma_vipInstan[index].Sel)	return;					// Sin cambio: salir
		ma_vipInstan[index].Sel = value; m_punPuntos.UpdateInstances(ma_vipInstan, index, 1); // ** Actualizar cpu y gpu
	}
// ** Mets
	public void MuestraPuntos()								{	m_punPuntos.Visible = Visible & wMain.PuntosVisi; wMain.Refresca();}
	public void AplicaMatCam()								{	m_punPuntos.RotationMatrix = wMain.visVista.Camera.RotationMatrix;}
	public bool HitTest(Vector vRayoOrig, Vector vRayoDir, Point ptMouse	// Verifica si este mdl es el más cercano a la cam
		, cList<kPunto.PartePunto> l_ppPts, ref float fMenorDist, ref Vector vPuntoMásCercano)
	{			if (!AbsolutelyVisible)	return false;						// Oculto: salir
		float fMenorDistAnt, f; bool bEnPunto = false;

		vRayoDir += vRayoOrig; vRayoOrig = Vector.FromWorld(vRayoOrig, ref r_Matrix); // Quitar transforms.  Se transforma puntos, no dirs: la dir se conv en pto final; se transforma y se regresa a dir
		vRayoDir = Vector.FromWorld(vRayoDir, ref r_Matrix) - vRayoOrig; vRayoDir.Normalize();
		fMenorDistAnt = fMenorDist;
		//-considerar clipplane
		// ** Probar en puntos
		if (l_ppPts != null)
		{	Vector vSupIzq, vSupDer, vSupIzqPanta, vSupDerPanta, v; Rectangle rtPuntoPanta = default(Rectangle);
		
			vSupIzq = new Vector(-0.05f, 0.05f, 0); m_punPuntos.Transform(ref vSupIzq); // Calc borde de punto
			vSupDer = new Vector(0.05f, 0.05f, 0); m_punPuntos.Transform(ref vSupDer);
			for (int i = 0; i < VertexData.VertexCount; i++)				// Recorrer puntos
			{	v = ma_verVerts[i].Position;
				vSupIzqPanta = wMain.visVista.Camera.VectorToScreen(v, this, vSupIzq);
					vSupDerPanta = wMain.visVista.Camera.VectorToScreen(v, this, vSupDer); // Calc lims de punto en panta
					rtPuntoPanta.X = vSupIzqPanta.X; rtPuntoPanta.Y = vSupIzqPanta.Y;
					rtPuntoPanta.Width = vSupDerPanta.X - vSupIzqPanta.X; rtPuntoPanta.Height = rtPuntoPanta.Width;
					if (!rtPuntoPanta.Contains(ptMouse))	continue;		// No interceptado: omitir
				f = (v - vRayoOrig).Length;	if (f > fMenorDist)	continue;	// Calc dist a la panta; si es mayor: omitir
				bEnPunto = true; fMenorDist = f; vPuntoMásCercano = vSupIzqPanta; // ** Interceptado: recordar el punto más cercano
				l_ppPts.Add(new kPunto.PartePunto() {	Parte = this, Punto = i, PosAbs = vPuntoMásCercano}); // Agregar punto
			}
		}
		// ** Probar en triangs
		if (!bEnPunto && Topology == eVertexTopology.Triangle)
		{	for (int i = 0; i < VertexData.IndexCount; i += 3)				// Recorrer triangs
			{	f = new Triangle3D(ma_verVerts[ma_iInds[i]].Position, ma_verVerts[ma_iInds[i + 1]].Position
					, ma_verVerts[ma_iInds[i + 2]].Position).HitTestLine(vRayoOrig, vRayoDir);
				if (!float.IsNaN(f) && f < fMenorDist)	{	fMenorDist = f; vPuntoMásCercano = Vector.Null;} // Tomar la menor dist a la cam (desel puntos posteriores)
			}
		}
		return (fMenorDistAnt != fMenorDist);								// Si la menor dist cambió, éste es el mdl más cercano
	}
	public void LimpiaPuntosSel()							{	for (int i = 0; i < ma_vipInstan.Length; i++)	setPuntoSel(i, 0);}
	public void DescargaPuntos()							{	mMod.Modelos.Remove(m_punPuntos);}
	public void ActualizaPunto(int iIdx, Vector vPunto)
	{	ma_verVerts[iIdx].Position = vPunto;								// Actualizar pos y norm
		//-actualizar normal
		UpdateVertices(ma_verVerts, iIdx, 1);								// Actualizar en gpu
		ma_vipInstan[iIdx].Desp = vPunto; m_punPuntos.UpdateInstances(ma_vipInstan, iIdx, 1); // Mover instan
		s_bLimsCambiados = true;
	}
	public void TransformaPuntos(/*const*/ ref Matrix4x4 m44Mat, Vector vDesp, bool bTransformPartes)
	{	// ** Transform parte princ
		if (VertexData.VertexCount != 0)
		{	// ** Transformar y actualizar buf de verts (en cpu y gpu)
			Vector.Transform(ref m44Mat, ma_verVerts, m_iVertStride, 0, ma_verVerts, m_iVertStride, 0, VertexData.VertexCount);
			Vector.TransformNormal(ref m44Mat, ma_verVerts, m_iVertStride, 12, ma_verVerts, m_iVertStride, 12, VertexData.VertexCount);
			UpdateVertices(ma_verVerts, 0, VertexData.VertexCount);
			// ** Actualizar puntos (en cpu y gpu)
			for (int i = 0; i < ma_vipInstan.Length; i++)	ma_vipInstan[i].Desp = ma_verVerts[i].Position;
			m_punPuntos.UpdateInstances(ma_vipInstan, 0, ma_vipInstan.Length);
		}
		// ** Transform partes
		if (bTransformPartes)
		{	m44Mat.Offset = new Vector();									// No pasar desp a las partes para no descentrarlas
			foreach (cModelo mdl in Parts)
			{	mdl.TransformaPuntos(ref m44Mat, vDesp, bTransformPartes);	// Transform verts
				mdl.Location = mdl.Location.GetTransformed(ref m44Mat) + vDesp; // Transform desp
			}
		}
		s_bLimsCambiados = true;
	}
	public void RecalcNorms(bool bTransformPartes)
	{	// ** Transform parte princ
		Vector.CalculateNormal(ma_verVerts, m_iVertStride, 0, VertexData.VertexCount, ma_iInds, 0, VertexData.IndexCount); // Actualizar en cpu
		UpdateVertices(ma_verVerts, 0, VertexData.VertexCount);				// Actualizar en gpu
		// ** Transform partes
		if (bTransformPartes)	foreach (cModelo mdl in Parts)	mdl.RecalcNorms(bTransformPartes);
	}
	public void Moldea(Vector vArrast)
	{	cModelo mdlMolde; Matrix4x4 m44, m44Parte; Triangle3D[] a_triMolde; float fDistArrast, f; bool bCambio = false;
	
		mdlMolde = mMod.Herrs.Molde; a_triMolde = mdlMolde.ma_triTransform;
		// ** Preparar triangs de molde
		m44 = mdlMolde.Matrix; m44Parte = Matrix.GetInverted(); m44.MultiplyAssign(ref m44Parte); // Llevar a world y luego a buf de parte sel
			Triangle3D.Transform(ref m44, mdlMolde.ma_triTriangs, 0, a_triMolde, 0, a_triMolde.Length);
		fDistArrast = vArrast.Length; vArrast.TransformNormal(ref m44Parte); // Transform dir
		// ** Mover puntos (en cpu)
		for (int j = 0; j < a_triMolde.Length; j++)							// Recorrer triangs de molde
		{	for (int i = 0; i < VertexData.VertexCount; i++)				// Recorrer puntos
			{	f = a_triMolde[j].HitTestLine(ma_verVerts[i].Position, vArrast);	if (float.IsNaN(f) || f > fDistArrast)	continue; // No intersecado o fuera del rango arrastrado: omitir
				ma_verVerts[i].Position.Add(vArrast, f + 0.0001f); ma_vipInstan[i].Desp = ma_verVerts[i].Position; bCambio = true; // Agregar fracción para poder retroceder el molde sin arrastrar la parte
			}
		}
		// ** Actualizar gpu
		if (bCambio)
		{	UpdateVertices(ma_verVerts, 0, VertexData.VertexCount);
			m_punPuntos.UpdateInstances(ma_vipInstan, 0, ma_vipInstan.Length);
			mMod.MainWnd.Changed = true; s_bLimsCambiados = true;
		}
	}
	public void CambiaLayout(eVertexLayout vlNuevo)
	{	switch (vlNuevo)
		{	case eVertexLayout.PositionNormal:	EditorVert = s_mvsSolid;
				break;
			default:							EditorVert = s_mvsTex;
				break;
		}
		VertexData.Layout = vlNuevo;
	}
	public void RecalcLims()												// Se debe llamar desde el mdl raíz
	{		if (!s_bLimsCambiados)	return;
		m_RecalcLims(ref MinimumVector, ref MaximumVector); s_bLimsCambiados = false;
	}
	public new void UpdateVertices(Array buffer, int index, int count)
	{	base.UpdateVertices(buffer, index, count);
		VertexData.Vertices = ma_verVerts; VertexData.VertexStride = m_iVertStride; // ** Liberar el buf creado al grabar
	}
	public new void Save(string sArchi)
	{	// ** Conv verts de textured a solid (si es nece)
		ApplyToTree((c3DModel mdl3d) =>
			{	cModelo mdl = (cModelo)mdl3d;

				if (mdl.VertexData.Layout == eVertexLayout.PositionNormal && !(mdl.VertexData.Vertices is VertexSolidColor[]))
				{	mdl.VertexData.Vertices = Array.ConvertAll(mdl.ma_verVerts, (Vertex vsc) =>
						new VertexSolidColor(vsc.Position, vsc.Normal));
					mdl.VertexData.VertexStride = VertexSolidColor.SIZE;
				}
			});
		// ** Grabar
		base.Save(sArchi);
	}
	protected override void OnPositionChanged()
	{	base.OnPositionChanged();	if (m_punPuntos != null)	{	m_punPuntos.AplicaMat(ref r_TransposedMatrix);}
	}
	protected override void OnLoaded()
	{	// ** Conv verts de solid a textured
		ma_verVerts = VertexData.Vertices as Vertex[]; m_iVertStride = Vertex.SIZE;
		if (ma_verVerts == null)
		{	ma_verVerts = Array.ConvertAll((VertexSolidColor[])VertexData.Vertices, (VertexSolidColor vsc) =>
				new Vertex(vsc.Position, vsc.Normal, Point.Zero));
			VertexData.Vertices = ma_verVerts; VertexData.VertexStride = m_iVertStride;
			ChangeVertexBuffer(ma_verVerts, m_iVertStride, ma_verVerts.Length);
		}
		// ** Conv inds de short a int
		ma_iInds = VertexData.Indices as int[];
		if (ma_iInds == null)
		{	ma_iInds = Array.ConvertAll((short[])VertexData.Indices, (short sh) => (int)sh);
			VertexData.Indices = ma_iInds; VertexData.IndexStride = 4;
			ChangeIndexBuffer(ma_iInds, 4, ma_iInds.Length);
		}
		// ** Tomar puntos
		ma_vipInstan = Array.ConvertAll(ma_verVerts, (Vertex ver) => new VertInstanPunto() {	Desp = ver.Position});
		// ** Crear puntos
		m_punPuntos = new cPunto(ma_vipInstan); MuestraPuntos();
		m_punPuntos.AplicaMat(ref r_TransposedMatrix); AplicaMatCam();		// Refres cbuf de puntos
		// ** Cfg
		switch (VertexData.Layout)
		{	case eVertexLayout.PositionNormalTexcoord:	EditorVert = s_mvsTex; break;
			default:									EditorVert = s_mvsSolid; break;
		}
		if (!EsMolde)	Material.CubeTexture = wMain.TexEnvCube;
		// ** Enlistar parte
		mMod.Partes.Add(this);
	}
	protected override void OnVisibleChanged()				{	base.OnVisibleChanged(); MuestraPuntos();}
	private void m_Carga3mf(string sArchi)
	{	XmlDocument xd; XmlNode ndResources, ndBuild; int i;
		Dictionary<string, Color> d_scColors = new Dictionary<string, Color>();
		Dictionary<string, Color> d_smMats = new Dictionary<string, Color>();

		MinimumVector = MaximumVector = new Vector(float.NaN);
		using (System.IO.Compression.ZipArchive za = System.IO.Compression.ZipFile.OpenRead(sArchi))
		{	foreach (System.IO.Compression.ZipArchiveEntry ze in za.Entries)
			{	// ** Abrir archi de mdl
					if (Path.GetExtension(ze.Name).ToUpperInvariant() != ".MODEL")	continue; // No es archi de mdl: omitir
				xd = new XmlDocument();	using (Stream stm = ze.Open())	xd.Load(stm);
				ndBuild = xd["model"]["build"]; ndResources = xd["model"]["resources"];
				// ** Cargar colores
				foreach (XmlNode nd in ndResources.ChildNodes)
				{	if (nd.Name == "color")
					{	i = nd.Attributes["value"].Value.ToInt(1, -1, System.Globalization.NumberStyles.HexNumber);
						d_scColors.Add(nd.Attributes["id"].Value, Color.FromBGR(i));
					}
				}
				// ** Cargar materiales
				foreach (XmlNode nd in ndResources.ChildNodes)
				{	if (nd.Name == "material")	d_smMats.Add(nd.Attributes["id"].Value, d_scColors[nd.Attributes["colorid"].Value]);
				}
				// ** Cargar build: agregar partes (items)
				foreach (XmlNode ndIt in ndBuild.ChildNodes)
				{	if (ndIt.Name == "item")	m_Carga3mfItem(ndIt, ndResources, Matrix4x4.Identity, d_scColors, d_smMats);
				}
			}
		}
		// ** Completar
		m_ActualizaLimCnt(ref MinimumVector, ref MaximumVector, false);
		OnPositionChanged();
	}
	private void m_Carga3mfItem(XmlNode ndIt, XmlNode ndResources, Matrix4x4 matPad // ** Y y Z están intercambiadas; x y z son neg; y los triángulos son contrarreloj
		, Dictionary<string, Color> d_scColors, Dictionary<string, Color> d_sclMaters)
	{	cModelo mdl; string sId; Matrix4x4 matTrans = default(Matrix4x4); string[] a_s; XmlNode ndTmp, nd;
		Vertex[] a_ver; int[] a_i; int iVerts, iIdx, iV1, iV2, iV3;
	
		sId = ndIt.Attributes["objectid"].Value;
		// ** Cargar transform
		a_s = ndIt.Attributes["transform"].Value.Split(new char[] {' '}, 12); // ** Leer transform
			matTrans._11 = a_s[0].ToFloat(); matTrans._12 = a_s[2].ToFloat(); matTrans._13 = a_s[1].ToFloat(); matTrans._14 = 0;
			matTrans._21 = a_s[6].ToFloat(); matTrans._22 = a_s[8].ToFloat(); matTrans._23 = a_s[7].ToFloat(); matTrans._24 = 0;
			matTrans._31 = a_s[3].ToFloat(); matTrans._32 = a_s[5].ToFloat(); matTrans._33 = a_s[4].ToFloat(); matTrans._34 = 0;
			matTrans._41 = -a_s[9].ToFloat(); matTrans._42 = a_s[11].ToFloat(); matTrans._43 = -a_s[10].ToFloat(); matTrans._44 = 1;
			matTrans.MultiplyAssign(ref matPad);							// Aplicar trans del pad
		// ** Buscar obj
		ndIt = null;
		foreach (XmlNode nd2 in ndResources.ChildNodes)
		{	if (nd2.Name == "object" && nd2.Attributes["id"].Value == sId) {	ndIt = nd2; break;} // Encont: saltar
		}
		// ** Cargar mesh
		ndTmp = ndIt["mesh"];
		if (ndTmp != null)													// ** Tiene verts
		{	// ** Leer verts
			nd = ndTmp["vertices"]; a_ver = new Vertex[nd.ChildNodes.Count]; iVerts = 0;
			foreach (XmlNode ndVert in nd)
			{		if (ndVert.Name != "vertex")	continue;				// No es vert (nunca): omitir
				a_ver[iVerts].Position = new Vector(-ndVert.Attributes["x"].Value.ToFloat(), ndVert.Attributes["z"].Value.ToFloat()
					, -ndVert.Attributes["y"].Value.ToFloat());
				a_ver[iVerts].Normal = Vector.Null;
				iVerts++;
			}
			Vector.Transform(ref matTrans, a_ver, Vertex.SIZE, 0, a_ver, Vertex.SIZE, 0, iVerts); // ** Transform verts
			// ** Leer idxs: calc normales
			nd = ndTmp["triangles"]; a_i = new int[nd.ChildNodes.Count * 3]; iIdx = 0;
			foreach (XmlNode ndTri in nd)
			{		if (ndTri.Name != "triangle")	continue;				// No es triang (nunca): omitir
				iV1 = ndTri.Attributes["v3"].Value.ToInt(); iV2 = ndTri.Attributes["v2"].Value.ToInt(); // Contrarreloj
					iV3 = ndTri.Attributes["v1"].Value.ToInt();
				//-s_CalcNormalSmooth(a_ver, iVerts, iV1, iV2, iV3);
				s_CalcNormalFlat(ref a_ver, ref iVerts, ref iV1, ref iV2, ref iV3);
				a_i[iIdx++] = iV1; a_i[iIdx++] = iV2; a_i[iIdx++] = iV3;	// Tomar pts
			}
			// ** Crear mdl
			mdl = new cModelo(a_ver, iVerts, a_i, iIdx, eVertexLayout.PositionNormal);
			mdl.Material.Type = eMaterial.Specular;							// Aplicar mater
				if (ndIt.Attributes["materialid"] != null)	mdl.Material.Color = d_sclMaters[ndIt.Attributes["materialid"].Value];
		} else																// ** No tiene verts
		{	mdl = new cModelo();											// Crear cnt
			mdl.MinimumVector = mdl.MaximumVector = new Vector(float.NaN);
		}
		Parts.Add(mdl); mdl.Name = sId;
		// ** Cargar partes
		ndTmp = ndIt["components"];
		if (ndTmp != null)
		{	foreach (XmlNode nd2 in ndTmp.ChildNodes)
				if (nd2.Name == "component")	mdl.m_Carga3mfItem(nd2, ndResources, matTrans, d_scColors, d_sclMaters);
		}
		// ** Completar
		mdl.m_ActualizaLimCnt(ref MinimumVector, ref MaximumVector);
		mdl.OnLoaded();														// ** Notif
	}
	private void m_CargaObj(string sArchi)
	{	const int BLK_TAM = 5000;
		Dictionary<string, Material> d_smtMaters; Array<Vector> a_vPts, a_vNormales; Array<Point> a_ptTexcoords; // Bufs totales
		Vertex[] a_ver; int[] a_iIdx, a_iPtId; string s; char[] a_cSepar = {	' '}; // Bufs de mdl

		// ** Leer archivo
		d_smtMaters = new Dictionary<string, Material>();
		a_vPts = new Array<Vector>(BLK_TAM); a_vNormales = new Array<Vector>(BLK_TAM); a_ptTexcoords = new Array<Point>(BLK_TAM); // Los bufs (vecs y tcs) de los mdls están separados pero pertenecen a uno solo.  Los inds se basan en este buf
		a_iIdx = new int[BLK_TAM]; a_iPtId = new int[BLK_TAM];
		MinimumVector = MaximumVector = new Vector(float.NaN);
		using (StreamReader sr = new StreamReader(File.OpenRead(sArchi)))
		{	s = s_sCargaObjLeeLin(sr);										// Avanzar
			// ** Leer maters
			if (s != null && s.StartsWithOrdinal("mtllib "))
			{	m_CargaObjMtl(Path.Combine(Path.GetDirectoryName(sArchi), s.Substring(7)), d_smtMaters);
				s = s_sCargaObjLeeLin(sr);									// Avanzar
			}
			// ** Leer partes
			while (s != null && (s.StartsWithOrdinal("g ") || s.StartsWithOrdinal("usemtl ")))
			{	cModelo mdl; string sNomb = null, sMater = null; int i, iVerts, iTcs, iInds, iIdx;
				string[] a_s, a_s2; bool bLeído;

				a_ver = null; iVerts = 0; iTcs = 0; iInds = 0;
				if (s.StartsWithOrdinal("g "))
					sNomb = s.Substring(2);
				else if (s.StartsWithOrdinal("usemtl "))
				{	sMater = s.Substring(7);
				}
				s = s_sCargaObjLeeLin(sr);									// Avanzar
				// ** Leer elementos de parte
				while (s != null && !s.StartsWithOrdinal("g "))
				{	bLeído = false;
					// ** Leer mater
					if (s.StartsWithOrdinal("usemtl "))
					{	sMater = s.Substring(7);
						s = s_sCargaObjLeeLin(sr); bLeído = true;			// Avanzar
					}
					// ** Leer pts
					while (s != null && s.StartsWithOrdinal("v "))
					{	s = s.Substring(2); a_s = s.Split(a_cSepar, 3);	if (a_s.Length != 3)	throw new System.FormatException("vector");
						a_vPts.Add(new Vector(a_s[0].ToFloat(), a_s[1].ToFloat(), -a_s[2].ToFloat())); // Usar left hand
						s = s_sCargaObjLeeLin(sr); bLeído = true;			// Avanzar
					}
					// ** Leer texcoords
					while (s != null && s.StartsWithOrdinal("vt "))
					{	s = s.Substring(3); a_s = s.Split(a_cSepar, 2);	if (a_s.Length < 2)		throw new System.FormatException("texcoord");
						a_ptTexcoords.Add(new Point(s_fWrapTexcoord(a_s[0].ToFloat()), 1 - s_fWrapTexcoord(a_s[1].ToFloat()))); // Cambiar la V a left hand
						s = s_sCargaObjLeeLin(sr); bLeído = true;			// Avanzar
					}
					// ** Leer normales
					while (s != null && s.StartsWithOrdinal("vn "))
					{	s = s.Substring(3); a_s = s.Split(a_cSepar, 3);	if (a_s.Length != 3)	throw new System.FormatException("vector");
						a_vNormales.Add(new Vector(a_s[0].ToFloat(), a_s[1].ToFloat(), -a_s[2].ToFloat())); // Usar left hand
						s = s_sCargaObjLeeLin(sr); bLeído = true;			// Avanzar
					}
					// ** Leer caras (debe ser el ult bloque)
					if (s?.StartsWithOrdinal("f ") == true)
					{	if (a_iPtId.Length < a_vPts.Count)	Array.Resize(ref a_iPtId, a_vPts.Count); // Los ids y pts deben tener la misma cant
						for (i = 0; i < a_iPtId.Length; i++)	a_iPtId[i] = -1; // Limpiar ids
						a_ver = new Vertex[a_vPts.Count];
						while (s != null && s.StartsWithOrdinal("f "))
						{	if (iInds + 3 >= a_iIdx.Length) Array.Resize(ref a_iIdx, iInds + BLK_TAM); // Expandir inds
							s = s.Substring(2); a_s = s.Split(a_cSepar, 3);	if (a_s.Length != 3)	throw new System.FormatException("face coord"); // Tomar verts
								s = a_s[0]; a_s[0] = a_s[2]; a_s[2] = s;	// Ordenar verts a contrarreloj para left hand
							a_cSepar[0] = '/';
							for (i = 0; i < 3; i++)
							{	a_s2 = a_s[i].Split(a_cSepar, 3);	if (a_s2.Length < 1)	throw new System.FormatException("face coord"); // Tomar inds de pt, tex y normal
								iIdx = a_s2[0].ToInt() - 1;					// Los inds están basados en 1
								if (a_iPtId[iIdx] == -1)					// ** Pt nuevo: agregar vert
								{	a_ver[iVerts].Position = a_vPts[iIdx];	// Pt
									if (a_s2.Length > 1)
									{	if (a_s2.Length > 2)	a_ver[iVerts].Normal = a_vNormales[a_s2[2].ToInt() - 1]; // Normal
										if (!string.IsNullOrEmpty(a_s2[1]))	// Texcoord
										{	a_ver[iVerts].Texcoord = a_ptTexcoords[a_s2[1].ToInt() - 1]; iTcs++;
										}
									}
									a_iPtId[iIdx] = iVerts; iVerts++;		// Asig id a pt; avanzar
								}
								a_iIdx[iInds] = a_iPtId[iIdx]; iInds++;						
							}
							a_cSepar[0] = ' ';
							s = s_sCargaObjLeeLin(sr); bLeído = true;		// Avanzar
						}
					}
					if (!bLeído)	s = sr.ReadLine();						// Etiq desconocida: omitir, avanzar
				}
				// ** Crear mdl
				mdl = new cModelo(a_ver, iVerts, a_iIdx.Clone(0, iInds), iInds
					, (iTcs == iVerts ? eVertexLayout.PositionNormalTexcoord : eVertexLayout.PositionNormal)) {	Name = sNomb}; // Tiene texcoord (en todos los verts)
				Parts.Add(mdl);
				if (sMater != null)	mdl.Material = d_smtMaters[sMater];
				mdl.m_ActualizaLimCnt(ref MinimumVector, ref MaximumVector);
				mdl.OnLoaded();
			}
		}
		// ** Completar
		OnPositionChanged(); OnLoaded();
		m_ActualizaLimCnt(ref MinimumVector, ref MaximumVector, false);
	}
	private void m_CargaObjMtl(string sArchi, Dictionary<string, Material> d_smtMaters)
	{	string s, sNomb; Material mtl; Dictionary<string, cTexture> d_stxTexs;

		d_stxTexs = new Dictionary<string, cTexture>();
		using (StreamReader sr = new StreamReader(File.OpenRead(sArchi)))
		{	s = s_sCargaObjLeeLin(sr);										// Avanzar
			while (s != null && s.StartsWithOrdinal("newmtl "))
			{	sNomb = s.Substring(7); mtl = Material.Default; mtl.PixelShader = Device.PSStandard;
				s = s_sCargaObjLeeLin(sr);									// Avanzar
				while (s != null && !s.StartsWithOrdinal("newmtl "))
				{	if (s.StartsWithOrdinal("illum "))
						mtl.Type = (eMaterial)s.ToInt(6);
					else if (s.StartsWithOrdinal("Kd "))
						mtl.Color = new Color(s.Substring(3));
					else if (s.StartsWithOrdinal("Ks "))
						mtl.SpecularColor = new Color(s.Substring(3));
					else if (s.StartsWithOrdinal("d "))
						mtl.Alpha = s.ToFloat(2);
					else if (s.StartsWithOrdinal("Ns "))
						mtl.SpecularPower = (int)s.ToFloat(3);
					else if (s.StartsWithOrdinal("map_Kd "))
					{	s = Path.Combine(Path.GetDirectoryName(sArchi), s.Substring(7));
						if (!d_stxTexs.TryGetValue(s, out mtl.Texture))
						{	mtl.Texture = new cTexture(Device, s, true); d_stxTexs.Add(s, mtl.Texture);
						}
					} else if (s.StartsWithOrdinal("bump "))
					{	s = Path.Combine(Path.GetDirectoryName(sArchi), s.Substring(5));
						if (!d_stxTexs.TryGetValue(s, out mtl.NormalMap))
						{	mtl.NormalMap = new cTexture(Device, s, true); d_stxTexs.Add(s, mtl.NormalMap);
						}
					}
					s = s_sCargaObjLeeLin(sr);								// Avanzar
				}
				d_smtMaters.Add(sNomb, mtl);								// ** Agregar mater
			}
		}
	}
	private void m_CargaX(string sArchi)
	{	Array<mXMeshLoader.IXDataObject> a_doArchi; string meshDirectory;
		int i, iFin; mXMeshLoader.IXDataObject[] a_do; mXMeshLoader.IXDataObject obj;

		// ** Leer archivo
		meshDirectory = Path.GetDirectoryName(sArchi);
		using (StreamReader xFile = File.OpenText(sArchi))
		{	mXMeshLoader.ValidateHeader(xFile.ReadLine());
			a_doArchi = mXMeshLoader.XDataObjectFactory.ExtractDataObjects(xFile.ReadToEnd());
		}
		a_do = a_doArchi.Data; iFin = a_doArchi.Count;
		// ** Cargar estructura
		for (i = 0; i < iFin; i++)
		{	obj = a_do[i]; if (obj.IsVisualObject)	mXMeshLoader.LoadMeshes(Device, meshDirectory, obj);
		}
		// ** Cargar raíz
		for (i = 0; i < iFin; i++)
		{	obj = a_do[i];	if (!obj.IsVisualObject)	continue;
			// ** Grupo
			if (obj.DataObjectType == "Frame")
			{	MinimumVector = MaximumVector = new Vector(float.NaN);
				m_CargaXItems(obj);
			// ** Obj
			} else if (obj.DataObjectType == "Mesh")
			{	cModelo mdl; mXMeshLoader.XDataObjectFactory.XDataObject doMesh = (mXMeshLoader.XDataObjectFactory.XDataObject)obj;

				mdl = new cModelo(doMesh.Vertices, doMesh.VertexCant, doMesh.Indices.Data, doMesh.Indices.Count, doMesh.VertexLayout);
				Parts.Add(mdl);
				mdl.Name = obj.Name; mdl.Material = doMesh.Material;
				mdl.m_ActualizaLimCnt(ref MinimumVector, ref MaximumVector);
				mdl.OnLoaded();												// ** Notif
			}
			break;
		}
		// ** Completar
		m_ActualizaLimCnt(ref MinimumVector, ref MaximumVector, false);
	}
	private void m_CargaXItems(mXMeshLoader.IXDataObject xdoCnt)
	{	mXMeshLoader.IXDataObject[] a_do, a_do2; mXMeshLoader.IXDataObject childObject, childObject2;

		a_do = xdoCnt.Children.Data;
		for (int i = 0, iFin = a_do.Length; i < iFin; i++)
		{	childObject = a_do[i];	if (!childObject.IsVisualObject)	continue;
			// ** Grupo
			if (childObject.DataObjectType == "Frame")
			{	cModelo mdl; mXMeshLoader.XDataObjectFactory.XDataObject doMesh;
				
				a_do2 = childObject.Children.Data; doMesh = null;
					for (int j = 0, iFin2 = a_do2.Length; j < iFin2; j++)
					{	childObject2 = a_do2[j];
						if (childObject2.DataObjectType == "Mesh")
						{	doMesh = (mXMeshLoader.XDataObjectFactory.XDataObject)childObject2;
							break;
						}
					}
				if (doMesh != null)
				{	mdl = new cModelo(doMesh.Vertices, doMesh.VertexCant, doMesh.Indices.Data, doMesh.Indices.Count, doMesh.VertexLayout);
					mdl.Material = doMesh.Material;
				} else
				{	mdl = new cModelo();									// Crear cnt
					mdl.MinimumVector = mdl.MaximumVector = new Vector(float.NaN);
				}
				Parts.Add(mdl);
				mdl.Name = childObject.Name;
				mdl.m_CargaXItems(childObject);
				mdl.m_ActualizaLimCnt(ref MinimumVector, ref MaximumVector);
				mdl.OnLoaded();												// ** Notif
			}
		}
	}
	private void m_CfgMolde(string sRuta)
	{	EsMolde = true; Material.Type = eMaterial.Flat;
		mMod.Partes.Remove(this); ShadowModel = null; InteriorVisible = true;
		ApplyToTree((c3DModel mdl) =>	mMod.Modelos.Remove(((cModelo)mdl).m_punPuntos));
		if (Topology == eVertexTopology.Triangle)
		{	ma_triTriangs = new Triangle3D[VertexData.IndexCount / 3];
			for (int i = 0, j = 0; i < ma_triTriangs.Length; i++, j += 3)			// Recorrer triangs
				ma_triTriangs[i] = new Triangle3D(ma_verVerts[ma_iInds[j]].Position
					, ma_verVerts[ma_iInds[j + 1]].Position, ma_verVerts[ma_iInds[j + 2]].Position);
		} else
		{	ma_triTriangs = new Triangle3D[0];
		}
		ma_triTransform = new Triangle3D[ma_triTriangs.Length];
		Ruta = sRuta;
	}
	private void m_RecalcLims(ref Vector vMinCnt, ref Vector vMaxCnt)
	{	MinimumVector = MaximumVector = new Vector(float.NaN);				// Iniciar
		Vector.GetBounds(out MinimumPartVector, out MaximumPartVector, ma_verVerts, m_iVertStride, 0, VertexData.VertexCount); // Tomar lim propio
		m_ActualizaLimCnt(ref MinimumVector, ref MaximumVector, false);		// Actualizar lim total propio
		if (HasParts)
		{	for (int i = 0; i < Parts.Count; i++)	((cModelo)Parts[i]).m_RecalcLims(ref MinimumVector, ref MaximumVector); // Recalc parte y actualizar lim total propio
		}
		m_ActualizaLimCnt(ref vMinCnt, ref vMaxCnt);						// Actualizar lim total de cnt
	}
	private void m_ActualizaLimCnt(ref Vector vMinCnt, ref Vector vMaxCnt, bool bUsarCnt = true)
	{	Vector vMinPart, vMaxPart;

		vMinPart = (bUsarCnt ? MinimumVector : MinimumPartVector); vMaxPart = (bUsarCnt ? MaximumVector : MaximumPartVector);
		if (float.IsNaN(vMinCnt.X))
		{	vMinCnt = vMinPart; vMaxCnt = vMaxPart;
		} else
		{	Vector.GetBounds(ref vMinCnt, ref vMaxCnt, vMinPart); Vector.GetBounds(ref vMinCnt, ref vMaxCnt, vMaxPart);
		}
	}
}
}
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using Vector = Wew.Media.Vector;
using Array = System.Array;
using CultInf = System.Globalization.CultureInfo;
using Wew;
using Wew.Media;
using Wew.Control;

class cModelo : c3DModel
{// ** Tps
		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	struct VertInstanPunto
	{	public const int SIZE	= Vector.SIZE_OF + 4 + VertexBone.Weights.SIZE_OF + VertexBone.Bones.SIZE_OF;
		public static readonly cVertexShader.Field[] InputLayout =
			[	new (eSemantic.Position)									// 1° buf
				, new ("DESP", 0, eVertexFieldType.Vector, 1)				// 2° buf
				, new ("SEL", 0, eVertexFieldType.Byte, 1)
				, new ("WEIGHTS", 0, eVertexFieldType.Vector, 1)
				, new ("BONES", 0, eVertexFieldType.Byte4, 1)
			];
		public Vector Desp;
		public bool Sel;
		public VertexBone.Weights Pesos;
		public VertexBone.Bones Huesos;
	}
	class cPunto : c3DModel													// ** No debe tener partes
	{// ** Tps
		struct Buf
		{	public Matrix4x4 World;
			public Color Color;
		}
	// ** Cpos
		readonly c3DSimpleBuffer<Buf> m_bbConst;
	// ** Stat
		static readonly c3DModel s_mdlPunto = new (mMod.MainWnd, typeof(cPunto), "Res.Punto.mdl");
		static readonly cVertexShader s_vsPunto = new (mMod.MainWnd, mShaders.VSPunto, VertInstanPunto.InputLayout);
		public static void sf_Dispose()		{	s_mdlPunto.Dispose(); s_vsPunto.Dispose();}
	// ** Ctor
		public cPunto(VertInstanPunto[] a_vipPuntos) : base(s_mdlPunto)
		{	// ** Ini
			m_bbConst = new c3DSimpleBuffer<Buf>(mMod.MainWnd, e3DBind.ConstantBuffer);
			m_bbConst.Data.World = Matrix4x4.Identity; m_bbConst.Data.Color = eColor.Gray; m_bbConst.Write();
			mMod.Raíces.Add(this);
			// ** Crear instancias
			CreateInstances(a_vipPuntos.Length, a_vipPuntos, s_vsPunto, PixelShader: mMod.PsSimple);
		}
		protected override void Dispose(bool disposing)		{	m_bbConst.Dispose(); _ = mMod.Raíces.Remove(this); base.Dispose(disposing);}
	// ** Mets
		public void AplicaSel(bool value)
		{	m_bbConst.Data.Color = (value ? eColor.OrangeRed : eColor.Gray); m_bbConst.Write(); wMain.Refresca();
		}
		public void AplicaMat(in Matrix4x4 value)	{	m_bbConst.Data.World = value; m_bbConst.Write(); wMain.Refresca();}
		protected override void OnPrepareRender(c3DGraphics g3d)	{	base.OnPrepareRender(g3d); m_bbConst.AssignToVS((int)eSlot.Instan);}
	}
	public abstract class cManVert : cStackPanel
	{	protected delegate void dAplicaProp(ref VertexBone vxbVert);
		protected static void s_AplicaProp(dAplicaProp ap_Aplicador)
		{	foreach (kPunto.PartePunto pp in mMod.PropsPunto.PuntosSel!)	// Aplicar a toda la sel
			{	ap_Aplicador(ref pp.Parte.ma_vxbVerts[pp.Punto]);			// Actualizar en cpu
				pp.Parte.ActualizaVerts(pp.Parte.ma_vxbVerts, pp.Punto, 1);	// Actualizar en gpu
			}
			mMod.MainWnd.Changed = true;
		}
		protected cManVert()					{	Direction = eDirection.Bottom; AutoSize = eAutoSize.Height;}
		public abstract void Carga(cModelo mdlParte, int iPunto);
	}
	public class cMVPosNorm : cManVert
	{	readonly uVector uveNorm;
		public cMVPosNorm()
		{	uPropertyGroup grp;

			grp = new uPropertyGroup {	Text = "Normal"};
				uveNorm = new uVector {	Text = "Normal"}; uveNorm.ValueChanged += uveNorm_ValueChanged;
					grp.AddControl(uveNorm);
				AddControl(grp);
		}
		public override void Carga(cModelo mdlParte, int iPunto)	{	uveNorm.Value = mdlParte.ma_vxbVerts[iPunto].Vertex.Normal;}
		private void uveNorm_ValueChanged(object sender)	{	s_AplicaProp((ref VertexBone vxbVert) =>	vxbVert.Vertex.Normal = uveNorm.Value);}
	}
	public class cMVTex : cMVPosNorm
	{	readonly uSlider uslTexcoordX, uslTexcoordY;
		readonly uVector uveTang;
		public cMVTex()
		{	uPropertyGroup grp;

			grp = new uPropertyGroup {	Text = "Texcoord"};
				uslTexcoordX = new uSlider {	Text = "X", IsPercent = true};
					uslTexcoordX.ValueChanged += Tex_ValueChanged;
					grp.AddControl(uslTexcoordX);
				uslTexcoordY = new uSlider {	Text = "Y", IsPercent = true};
					uslTexcoordY.ValueChanged += Tex_ValueChanged;
					grp.AddControl(uslTexcoordY);
				AddControl(grp);
			grp = new uPropertyGroup {	Text = "Texcoord"};
				uveTang = new uVector {	Text = "Tangent"}; uveTang.ValueChanged += uveTang_ValueChanged;
					grp.AddControl(uveTang);
				AddControl(grp);
		}
		public override void Carga(cModelo mdlParte, int iPunto)
		{	Point pt;

			base.Carga(mdlParte, iPunto);
			pt = mdlParte.ma_vxbVerts[iPunto].Vertex.Texcoord; uslTexcoordX.Value = pt.X; uslTexcoordY.Value = pt.Y;
			uveTang.Value = mdlParte.ma_vxbVerts[iPunto].Vertex.Tangent;
		}
		private void Tex_ValueChanged(object sender)
		{	s_AplicaProp((ref VertexBone vxbVert) =>	vxbVert.Vertex.Texcoord = new Point(uslTexcoordX.Value, uslTexcoordY.Value));
		}
		private void uveTang_ValueChanged(object sender)
		{	s_AplicaProp((ref VertexBone vxbVert) =>	vxbVert.Vertex.Tangent = uveTang.Value);
		}
	}
	public class cMVHue : cMVTex
	{	readonly uSlider uslPeso0, uslPeso1, uslPeso2;
		readonly uSlider uslHueso0, uslHueso1, uslHueso2, uslHueso3;
		public cMVHue()
		{	uPropertyGroup grp;

			grp = new uPropertyGroup {	Text = "Weights"};
				uslPeso0 = new uSlider {	Text = "0", IsPercent = true}; uslPeso0.ValueChanged += uslPesos_ValueChanged;
					grp.AddControl(uslPeso0);
				uslPeso1 = new uSlider {	Text = "1", IsPercent = true}; uslPeso1.ValueChanged += uslPesos_ValueChanged;
					grp.AddControl(uslPeso1);
				uslPeso2 = new uSlider {	Text = "2", IsPercent = true}; uslPeso2.ValueChanged += uslPesos_ValueChanged;
					grp.AddControl(uslPeso2);
				AddControl(grp);
			grp = new uPropertyGroup {	Text = "Bones"};
				uslHueso0 = new uSlider {	Text = "0", Maximum = 255}; uslHueso0.ValueChanged += uslHuesos_ValueChanged;
					grp.AddControl(uslHueso0);
				uslHueso1 = new uSlider {	Text = "1", Maximum = 255}; uslHueso1.ValueChanged += uslHuesos_ValueChanged;
					grp.AddControl(uslHueso1);
				uslHueso2 = new uSlider {	Text = "2", Maximum = 255}; uslHueso2.ValueChanged += uslHuesos_ValueChanged;
					grp.AddControl(uslHueso2);
				uslHueso3 = new uSlider {	Text = "3", Maximum = 255}; uslHueso3.ValueChanged += uslHuesos_ValueChanged;
					grp.AddControl(uslHueso3);
				AddControl(grp);
		}
		public override void Carga(cModelo mdlParte, int iPunto)
		{	VertexBone.Weights w; VertexBone.Bones bn;

			base.Carga(mdlParte, iPunto);
			w = mdlParte.ma_vxbVerts[iPunto].Weight; uslPeso0.Value = w.Weight0; uslPeso1.Value = w.Weight1; uslPeso2.Value = w.Weight2;
			bn = mdlParte.ma_vxbVerts[iPunto].Bone; uslHueso0.Value = bn.Bone0; uslHueso1.Value = bn.Bone1; uslHueso2.Value = bn.Bone2;
				uslHueso3.Value = bn.Bone3;
		}
		private void uslPesos_ValueChanged(object sender)
		{	s_AplicaProp((ref VertexBone vxbVert) =>	vxbVert.Weight = new VertexBone.Weights(uslPeso0.Value
				, uslPeso1.Value, uslPeso2.Value));
		}
		private void uslHuesos_ValueChanged(object sender)
		{	uSlider usl = (uSlider)sender;

				if (usl.Value > 255)	usl.Value = 255;	else if (usl.Value < 0)	usl.Value = 0;
			s_AplicaProp((ref VertexBone vxbVert) =>	vxbVert.Bone = new VertexBone.Bones((byte)uslHueso0.Value
				, (byte)uslHueso1.Value, (byte)uslHueso2.Value, (byte)uslHueso3.Value));
		}
	}
// ** Cpos
	VertexBone[] ma_vxbVerts; int m_iVertStride; int[] ma_iInds;
	cPunto m_punPuntos; VertInstanPunto[] ma_vipInstan;						// Mdl punto.  Datos de instan (puntos)
	bool m_bSel;
	/*readonly*/ Triangle3D[] ma_triTriangs, ma_triTransform;				// Sólo para molde: triangs iniciales y transformados a coords de buf de la parte sel
	public cManVert EditorVert;
	public /*readonly*/ bool EsMolde; public /*readonly*/ string Ruta;		// Sólo para molde
// ** Stat
	static readonly cMVPosNorm s_mvsSolid = new();
	static readonly cMVTex s_mvsTex = new();
	static readonly cMVHue s_mvsTexHue = new();
	static bool s_bLimsCambiados;
	static readonly Dictionary<string, System.WeakReference<cTexture>> sd_stxTexturas = [];
	public static cModelo Abre(string sArchi)
	{	string sExt = Path.GetExtension(sArchi).ToUpperInvariant();

		switch (sExt)
		{	case ".3MF": case ".OBJ": case ".X":							// Importar: salir
				cModelo mdl;

				mdl = new cModelo {	Name = Path.GetFileNameWithoutExtension(sArchi)}; // Crear cnt
				switch (sExt)
				{	case ".3MF":	mdl.m_Carga3mf(sArchi); break;
					case ".OBJ":	mdl.m_CargaObj(sArchi); break;
					case ".X":		mdl.m_CargaX(sArchi); break;
				}
				mdl.OnLoaded();
				return mdl;
		}
		return new cModelo(sArchi);											// Abrir
	}
	public static cModelo? FromDataObject()
	{	return (cModelo?)mClipboard.Get3DModel(null, (c3DModel? mdlPad, int iIdxHijo) => new cModelo(), true);
	}
	public static cTexture GetTextura(string sArchi)
	{	cTexture? texRes; System.WeakReference<cTexture>? w_tex;
	
		if (!sd_stxTexturas.TryGetValue(sArchi, out w_tex) || !w_tex.TryGetTarget(out texRes))
		{	texRes = new cTexture(mMod.MainWnd, sArchi, e3DBind.ShaderResource, true); sd_stxTexturas[sArchi] = new (texRes);
		}
		return texRes;
	}
	public static void LiberaTexturas()
	{	foreach (System.WeakReference<cTexture> w_tex in sd_stxTexturas.Values)	if (w_tex.TryGetTarget(out cTexture? tex))	tex.Dispose();
		sd_stxTexturas.Clear();
	}
	//private static void s_CalcNormalSmooth(Vertex[] a_verVers, int iV1, int iV2, int iV3)
	//{	Vector vNorm;

	//	vNorm = (a_verVers[iV2].Position - a_verVers[iV1].Position).Cross(a_verVers[iV3].Position - a_verVers[iV1].Position).GetNormalized();
	//	s_AsigNormSmooth(a_verVers, iV1, vNorm);
	//	s_AsigNormSmooth(a_verVers, iV2, vNorm);
	//	s_AsigNormSmooth(a_verVers, iV3, vNorm);
	//}
	private static void s_CalcNormalFlat(ref Vertex[] a_verVers, ref int iVerts, ref int iV1, ref int iV2, ref int iV3)
	{	Vector vNorm;

		vNorm = (a_verVers[iV2].Position - a_verVers[iV1].Position).Cross(a_verVers[iV3].Position - a_verVers[iV1].Position).GetNormalized();
		s_AsigNormFlat(ref a_verVers, ref iVerts, ref iV1, vNorm);
		s_AsigNormFlat(ref a_verVers, ref iVerts, ref iV2, vNorm);
		s_AsigNormFlat(ref a_verVers, ref iVerts, ref iV3, vNorm);
	}
	//private static void s_AsigNormSmooth(Vertex[] a_verVers, int iV, Vector vNorm)
	//{	a_verVers[iV].Normal = !a_verVers[iV].Normal.IsNull ? a_verVers[iV].Normal.Lerp(vNorm, 0.5f).GetNormalized() : vNorm;
	//}
	private static void s_AsigNormFlat(ref Vertex[] a_verVers, ref int iVerts, ref int iV, Vector vNorm)
	{	if (a_verVers[iV].Normal.IsNaN)										// ** Aún no tiene normal: asig
			a_verVers[iV].Normal = vNorm;
		else if (a_verVers[iV].Normal != vNorm)								// ** Tiene norm dif: duplicar vert
		{	if (iVerts == a_verVers.Length)	Array.Resize(ref a_verVers, iVerts * 2); // Expandir
			a_verVers[iVerts].Position = a_verVers[iV].Position; a_verVers[iVerts].Normal = vNorm; // Agregar vert.  Asig norm
			iV = iVerts; iVerts++;											// Actualizar idx
		}
	}
	private static string? s_sCargaObjLeeLin(StreamReader srArchi)
	{	string? sRes;

		sRes = srArchi.ReadLine()?.Trim();
		while (sRes is not null && (sRes.Length == 0 || sRes.StartsWithOrdinal("#")))	sRes = srArchi.ReadLine()?.Trim(); // Avanzar; saltar espacios y coments
		return sRes;
	}
	private static float s_fWrapTex(float fTc)								// Aplicar wrap a texcoord
	{	if (fTc > 1)	return (fTc == (int)fTc ? 1 : fTc % 1);				// Si es entero, es 1 sino la fracción
		if (fTc < 0)	return fTc == (int)fTc ? 0 : 1 + (fTc % 1);			// Si es entero, es 0 sino el complemento a 1
		return fTc;
	}
// ** Ctor
	public cModelo(string sArchi, bool bEsMolde = false) : base(mMod.MainWnd, sArchi, (c3DModel? mdlPad, int iIdxHijo) => new cModelo(), true)
	{	OnLoaded();	if (bEsMolde)	m_CfgMolde(sArchi);
	}
	public cModelo(System.Type tpTp, string sRes) : base(mMod.MainWnd, tpTp, sRes, (c3DModel? mdlPad, int iIdxHijo) => new cModelo(), true) // Crea molde
	{	OnLoaded(); m_CfgMolde("@" + sRes);
	}
	public cModelo(Vertex[] a_verVerts, int iVertCant, int[] a_iInds, int iIdxCant) // Crea fig y vacío
			: base(mMod.MainWnd, a_verVerts, a_iInds, iVertCant, iIdxCant, default, default, true, eVertexLayout.PosNormTexTang)
	{   OnLoaded();
	}
	private cModelo(Array a_Verts, int iVertCant, int[] a_iInds, int iIdxCant, eVertexLayout vlVertLayReal)
		: base(mMod.MainWnd, a_Verts, a_iInds, iVertCant, iIdxCant, default, default, true, vlVertLayReal)
	{
	}
	private cModelo() : base(mMod.MainWnd, true)			{}
		[MemberNotNull(nameof(ma_vxbVerts))][MemberNotNull(nameof(ma_iInds))][MemberNotNull(nameof(ma_vipInstan))]
		[MemberNotNull(nameof(m_punPuntos))][MemberNotNull(nameof(EditorVert))]
	protected override void OnLoaded()
	{   // ** Conv verts a vertbone
		ma_vxbVerts = (VertexData!.Vertices as VertexBone[])!; m_iVertStride = VertexBone.SIZE_OF;
		if (ma_vxbVerts is null)
		{	ma_vxbVerts = VertexData.Vertices switch
			{	Vertex[] ver		=> Array.ConvertAll(ver, (Vertex ver2) => new VertexBone { Vertex = ver2 }),
				VertexColor[] vsc	=> Array.ConvertAll(vsc, (VertexColor vsc2) => new VertexBone { Vertex = new Vertex(vsc2.Position, vsc2.Normal, default, default) }),
				VertexTexture[] vtx	=> Array.ConvertAll(vtx, (VertexTexture vtx2) => new VertexBone { Vertex = new Vertex(vtx2.Position, vtx2.Normal, vtx2.Texcoord, default) }),
				_					=> throw new System.FormatException("vertex type"),
			};
			VertexData.Vertices = ma_vxbVerts; VertexData.VertexStride = m_iVertStride;
			Vertices?.Change(ma_vxbVerts);
		}
		// ** Conv inds de short a int
		ma_iInds = (VertexData.Indices as int[])!;
		if (ma_iInds is null)
		{	ma_iInds = Array.ConvertAll((short[])VertexData.Indices, (short sh) => (int)sh);
			VertexData.Indices = ma_iInds; VertexData.IndexStride = 4;
			Indices?.Change(ma_iInds);
		}
		// ** Tomar puntos
		ma_vipInstan = Array.ConvertAll(ma_vxbVerts
			, (VertexBone vxb) => new VertInstanPunto {	Desp = vxb.Vertex.Position, Pesos = vxb.Weight, Huesos = vxb.Bone});
		// ** Crear puntos
		m_punPuntos = new cPunto(ma_vipInstan); MuestraPuntos();
		m_punPuntos.AplicaMat(r_TransposedMatrix); AplicaMatCam();		  // Refres cbuf de puntos
		CambiaLayout(VertexData.Layout);
		if (!EsMolde)
		{	Material.CubeTexture = wMain.TexEnvCube;
			// ** Enlistar parte
			mMod.Partes.Add(this);
		}
	}
		[MemberNotNull(nameof(ma_triTriangs))][MemberNotNull(nameof(ma_triTransform))][MemberNotNull(nameof(Ruta))]
	private void m_CfgMolde(string sRuta)
	{	EsMolde = true; Material.Type = eMaterial.Flat;
		_ = mMod.Partes.Remove(this); ShadowModel = null; InteriorVisible = true;
		ApplyToTree((c3DModel mdl) =>	mMod.Raíces.Remove(((cModelo)mdl).m_punPuntos));
		if (Topology == eVertexTopology.Triangle)
		{	ma_triTriangs = new Triangle3D[IndexCount / 3];
			for (int i = 0, j = 0; i < ma_triTriangs.Length; i++, j += 3)			// Recorrer triangs
				ma_triTriangs[i] = new Triangle3D(ma_vxbVerts[ma_iInds[j]].Vertex.Position
					, ma_vxbVerts[ma_iInds[j + 1]].Vertex.Position, ma_vxbVerts[ma_iInds[j + 2]].Vertex.Position);
		} else
		{	ma_triTriangs = [];
		}
		ma_triTransform = new Triangle3D[ma_triTriangs.Length];
		Ruta = sRuta;
	}
	protected override void Dispose(bool disposing)
	{	if (disposing)
		{	Material.Texture = null; Material.NormalMap = null; Material.CubeTexture = null; // No destruir texturas (pueden ser compartidas)
			_ = mMod.Raíces.Remove(this); _ = mMod.Partes.Remove(this);		// Quitar de listas
			m_punPuntos.Dispose();											// Destruir puntos
		}
		base.Dispose(disposing);
	}
	internal static void sf_Dispose()		{	cPunto.sf_Dispose();}
// ** Props
	public bool Sel
	{	get													=>	m_bSel;
		set													{	if (value != m_bSel)	{	m_bSel = value; m_punPuntos.AplicaSel(value);}}
	}
	public Vector getPositions(int index)					{	return ma_vxbVerts[index].Vertex.Position;}
	public void setPuntoSel(int index, bool value)
	{		if (value == ma_vipInstan[index].Sel)	return;					// Sin cambio: salir
		ma_vipInstan[index].Sel = value; m_punPuntos.InstanceVertices!.Write(ma_vipInstan, index, 1, index); // ** Actualizar cpu y gpu
	}
// ** Mets
	public void MuestraPuntos()								{	m_punPuntos.Visible = Visible & wMain.PuntosVisi; wMain.Refresca();}
	public void AnimaPuntos(bool bAnim)						{	m_punPuntos.Animating = bAnim; wMain.Refresca();}
	public void AplicaMatCam()								{	m_punPuntos.RotationMatrix = wMain.Cam.RotationMatrix;}
	public bool HitTest(Vector vRayoOrig, Vector vRayoDir, Point ptMouse	// Verifica si este mdl es el más cercano a la cam
		, cList<kPunto.PartePunto>? l_ppPts, ref float fMenorDist, ref Vector vPuntoMásCercano)
	{			if (!AbsolutelyVisible)	return false;						// Oculto: salir
		float fMenorDistAnt, f; bool bEnPunto = false;

		vRayoDir += vRayoOrig; vRayoOrig = Vector.FromWorld(vRayoOrig, r_Matrix); // Quitar transforms.  Se transforma puntos, no dirs: la dir se conv en pto final; se transforma y se regresa a dir
		vRayoDir = Vector.FromWorld(vRayoDir, r_Matrix) - vRayoOrig; vRayoDir.Normalize();
		fMenorDistAnt = fMenorDist;
		//¿considerar clipplane
		// ** Probar en puntos
		if (l_ppPts is not null)
		{	Vector vSupIzq, vSupDer, vSupIzqPanta, vSupDerPanta, v; Rectangle rtPuntoPanta = default;
		
			vSupIzq = new Vector(-0.05f, 0.05f, 0); m_punPuntos.Transform(ref vSupIzq); // Calc borde de punto
			vSupDer = new Vector(0.05f, 0.05f, 0); m_punPuntos.Transform(ref vSupDer);
			for (int i = 0; i < VertexCount; i++)							// Recorrer puntos
			{	v = ma_vxbVerts[i].Vertex.Position;
				vSupIzqPanta = wMain.Cam.VectorToScreen(v, this, vSupIzq);
					vSupDerPanta = wMain.Cam.VectorToScreen(v, this, vSupDer); // Calc lims de punto en panta
					rtPuntoPanta.X = vSupIzqPanta.X; rtPuntoPanta.Y = vSupIzqPanta.Y;
					rtPuntoPanta.Width = vSupDerPanta.X - vSupIzqPanta.X; rtPuntoPanta.Height = rtPuntoPanta.Width;
					if (!rtPuntoPanta.Contains(ptMouse))	continue;		// No interceptado: omitir
				f = (v - vRayoOrig).Length;	if (f > fMenorDist)	continue;	// Calc dist a la panta; si es mayor: omitir
				bEnPunto = true; fMenorDist = f; vPuntoMásCercano = vSupIzqPanta; // ** Interceptado: recordar el punto más cercano
				l_ppPts.Add(new kPunto.PartePunto {	Parte = this, Punto = i, PosAbs = vPuntoMásCercano}); // Agregar punto
			}
		}
		// ** Probar en triangs
		if (!bEnPunto && Topology == eVertexTopology.Triangle)
		{	for (int i = 0; i < IndexCount; i += 3)							// Recorrer triangs
			{	f = new Triangle3D(ma_vxbVerts[ma_iInds[i]].Vertex.Position, ma_vxbVerts[ma_iInds[i + 1]].Vertex.Position
					, ma_vxbVerts[ma_iInds[i + 2]].Vertex.Position).HitTestRay(vRayoOrig, vRayoDir);
				if (!float.IsNaN(f) && f < fMenorDist)	{	fMenorDist = f; vPuntoMásCercano = Vector.NaN;} // Tomar la menor dist a la cam (desel puntos posteriores)
			}
		}
		return (fMenorDistAnt != fMenorDist);								// Si la menor dist cambió, éste es el mdl más cercano
	}
	public void LimpiaPuntosSel()							{	for (int i = 0; i < ma_vipInstan.Length; i++)	setPuntoSel(i, false);}
	public void DescargaPuntos()							{	_ = mMod.Raíces.Remove(m_punPuntos);}
	public void ActualizaPunto(int iIdx, Vector vPunto)
	{	ma_vxbVerts[iIdx].Vertex.Position = vPunto;							// Actualizar pos y norm
		//¿actualizar normal
		ActualizaVerts(ma_vxbVerts, iIdx, 1);								// Actualizar en gpu
		ma_vipInstan[iIdx].Desp = vPunto; m_punPuntos.InstanceVertices!.Write(ma_vipInstan, iIdx, 1, iIdx); // Mover instan
		s_bLimsCambiados = true;
	}
	public void TransformaPuntos(Matrix4x4 m44Mat, Vector vDesp, bool bTransformPartes)
	{	// ** Transform parte princ
		if (VertexCount != 0)
		{	// ** Transformar y actualizar buf de verts (en cpu y gpu)
			Vector.Transform(m44Mat, ma_vxbVerts, m_iVertStride, 0, 0, ma_vxbVerts, m_iVertStride, 0, 0, VertexCount);
			Vector.TransformNormal(m44Mat, ma_vxbVerts, m_iVertStride, 0, Vertex.NORMAL_OFFSET
				, ma_vxbVerts, m_iVertStride, 0, Vertex.NORMAL_OFFSET, VertexCount);
			ActualizaVerts(ma_vxbVerts, 0, VertexCount);
			// ** Actualizar puntos (en cpu y gpu)
			for (int i = 0; i < ma_vipInstan.Length; i++)	ma_vipInstan[i].Desp = ma_vxbVerts[i].Vertex.Position;
			m_punPuntos.InstanceVertices!.Write(ma_vipInstan);
		}
		// ** Transform partes
		if (bTransformPartes)
		{	m44Mat.Offset = default;										// No pasar desp a las partes para no descentrarlas
			foreach (cModelo mdl in Parts)
			{	mdl.TransformaPuntos(m44Mat, vDesp, bTransformPartes);		// Transform verts
				mdl.Location = mdl.Location * m44Mat + vDesp;				// Transform desp
			}
		}
		s_bLimsCambiados = true;
	}
	public void RecalcNorms(bool bTransformPartes)
	{	// ** Transform parte princ
		Vector.CalculateNormal(ma_vxbVerts, m_iVertStride, 0, VertexCount, ma_iInds, 0, IndexCount); // Actualizar en cpu
		ActualizaVerts(ma_vxbVerts, 0, VertexCount);						// Actualizar en gpu
		// ** Transform partes
		if (bTransformPartes)	foreach (cModelo mdl in Parts)	mdl.RecalcNorms(bTransformPartes);
	}
	public void Moldea(Vector vArrast)
	{	cModelo mdlMolde; Matrix4x4 m44, m44Parte; Triangle3D[] a_triMolde; float fDistArrast, f; bool bCambio = false;
	
		mdlMolde = mMod.Herrs.Molde; a_triMolde = mdlMolde.ma_triTransform;
		// ** Preparar triangs de molde
		m44 = mdlMolde.Matrix; m44Parte = Matrix.GetInverted(); m44 *= m44Parte; // Llevar a world y luego a buf de parte sel
			Triangle3D.Transform(m44, mdlMolde.ma_triTriangs, 0, a_triMolde, 0, a_triMolde.Length);
		fDistArrast = vArrast.Length; vArrast.TransformNormal(m44Parte);	// Transform dir
		// ** Mover puntos (en cpu)
		for (int j = 0; j < a_triMolde.Length; j++)							// Recorrer triangs de molde
		{	for (int i = 0; i < VertexCount; i++)							// Recorrer puntos
			{	f = a_triMolde[j].HitTestRay(ma_vxbVerts[i].Vertex.Position, vArrast);	if (float.IsNaN(f) || f > fDistArrast)	continue; // No intersecado o fuera del rango arrastrado: omitir
				ma_vxbVerts[i].Vertex.Position.Add(vArrast, f + 0.0001f); ma_vipInstan[i].Desp = ma_vxbVerts[i].Vertex.Position; bCambio = true; // Agregar fracción para poder retroceder el molde sin arrastrar la parte
			}
		}
		// ** Actualizar gpu
		if (bCambio)
		{	ActualizaVerts(ma_vxbVerts, 0, VertexCount);
			m_punPuntos.InstanceVertices!.Write(ma_vipInstan);
			mMod.MainWnd.Changed = true; s_bLimsCambiados = true;
		}
	}
		[MemberNotNull(nameof(EditorVert))]
	public void CambiaLayout(eVertexLayout vlNuevo)
	{	EditorVert = vlNuevo switch
			{	eVertexLayout.PosNorm			=> s_mvsSolid,
				eVertexLayout.PosNormTex		=> s_mvsTex,
				eVertexLayout.PosNormTexTang	=> s_mvsTex,
				_								=> s_mvsTexHue				// eVertexLayout.PosNormTexTangWeiBon
			};
		VertexData!.Layout = vlNuevo;
	}
	public void RecalcLims()												// Se debe llamar desde el mdl raíz
	{		if (!s_bLimsCambiados)	return;
		m_RecalcLims(ref MinimumVector, ref MaximumVector); s_bLimsCambiados = false;
	}
	public void ActualizaVerts(Array buffer, int index, int count)
	{	Vertices!.Write(buffer, index, count, index);
		VertexData!.Vertices = ma_vxbVerts; VertexData.VertexStride = m_iVertStride; // ** Liberar el buf creado al grabar
	}
	public void ElimTexDup()
	{	List<cTexture> l_tex; int iCantElim;
	
		l_tex = GetTextures(); iCantElim = 0;
		for (int i = 1; i < l_tex.Count; i++)
		{	for (int j = 0; j < i; j++)										// Buscar igual
				if (l_tex[j] is not null && l_tex[j].DataSize == l_tex[i].DataSize // Encontrado: quitar, saltar
						&& mHelper.MemCmp(l_tex[j].Data, l_tex[i].Data, l_tex[j].DataSize) == 0)
				{	ApplyToTree((c3DModel mdl) =>							// ** Usar la compartida
						{	if (mdl.Material.Texture == l_tex[i])	mdl.Material.Texture = l_tex[j];
							if (mdl.Material.NormalMap == l_tex[i])	mdl.Material.NormalMap = l_tex[j];
						});
					l_tex[i] = null!; mMod.MainWnd.Changed = true; iCantElim++; break; // No comparar con esta tex; saltar
				}
		}
		mDialog.MsgBoxInformation($"{iCantElim} textures removed", "Remove duplicated textures");
	}
	public override void Save(Stream stream)
	{	// ** Conv verts grandes a peque (si es nece)
		ApplyToTree((c3DModel mdl3d) =>
			{	cModelo mdl = (cModelo)mdl3d;

				switch (mdl.VertexData!.Layout)
				{	case eVertexLayout.PosNorm:
						if (mdl.VertexData.Vertices is not VertexColor[])
						{	mdl.VertexData.Vertices = Array.ConvertAll(mdl.ma_vxbVerts, (VertexBone vxb) =>
								new VertexColor(vxb.Vertex.Position, vxb.Vertex.Normal));
							mdl.VertexData.VertexStride = VertexColor.SIZE_OF;
						}
						break;
					case eVertexLayout.PosNormTex:
						if (mdl.VertexData.Vertices is not VertexTexture[])
						{	mdl.VertexData.Vertices = Array.ConvertAll(mdl.ma_vxbVerts, (VertexBone vxb) =>
								new VertexTexture(vxb.Vertex.Position, vxb.Vertex.Normal, vxb.Vertex.Texcoord));
							mdl.VertexData.VertexStride = VertexTexture.SIZE_OF;
						}
						break;
					case eVertexLayout.PosNormTexTang:
						if (mdl.VertexData.Vertices is not Vertex[])
						{	mdl.VertexData.Vertices = Array.ConvertAll(mdl.ma_vxbVerts, (VertexBone vxb) => vxb.Vertex);
							mdl.VertexData.VertexStride = Vertex.SIZE_OF;
						}
						break;
					case eVertexLayout.PosNormTexTangWeiBon:				// Sin cambio
						break;
					default:
						throw new System.FormatException("vertex layout");
				}
			});
		base.Save(stream);													// ** Grabar
	}
	protected override void OnPositionChanged()				{	base.OnPositionChanged(); m_punPuntos?.AplicaMat(r_TransposedMatrix);}
	protected override void OnVisibleChanged()				{	base.OnVisibleChanged(); MuestraPuntos();}
	private void m_Carga3mf(string sArchi)
	{	XmlDocument xd; XmlNode ndResources, ndBuild; int i;
		Dictionary<string, Color> d_scColors = [];
		Dictionary<string, Color> d_smMats = [];
		using System.IO.Compression.ZipArchive za = System.IO.Compression.ZipFile.OpenRead(sArchi);
		
		MinimumVector = MaximumVector = Vector.NaN;
		foreach (System.IO.Compression.ZipArchiveEntry ze in za.Entries)
		{	// ** Abrir archi de mdl
				if (!Path.GetExtension(ze.Name).Equals(".MODEL", System.StringComparison.InvariantCultureIgnoreCase))	continue; // No es archi de mdl: omitir
			xd = new XmlDocument();	using Stream stm = ze.Open();
			
			xd.Load(stm);
			ndBuild = xd["model"]!["build"]!; ndResources = xd["model"]!["resources"]!;
			// ** Cargar colores
			foreach (XmlNode nd in ndResources.ChildNodes)
			{	if (nd.Name == "color")
				{	i = nd.Attributes!["value"]!.Value.ToIntInvariant(1, -1, System.Globalization.NumberStyles.HexNumber);
					d_scColors.Add(nd.Attributes["id"]!.Value, Color.FromBGR(i));
				}
			}
			// ** Cargar materiales
			foreach (XmlNode nd in ndResources.ChildNodes)
			{	if (nd.Name == "material")	d_smMats.Add(nd.Attributes!["id"]!.Value, d_scColors[nd.Attributes!["colorid"]!.Value]);
			}
			// ** Cargar build: agregar partes (items)
			foreach (XmlNode ndIt in ndBuild.ChildNodes)
			{	if (ndIt.Name == "item")	m_Carga3mfItem(ndIt, ndResources, Matrix4x4.Identity, d_scColors, d_smMats);
			}
		}
		// ** Completar
		m_ActualizaLimCnt(ref MinimumVector, ref MaximumVector, false);
		OnPositionChanged();
	}
	private void m_Carga3mfItem(XmlNode ndIt, XmlNode ndResources, Matrix4x4 matPad // ** Y y Z están intercambiadas; x y z son neg; y los triángulos son contrarreloj
			, Dictionary<string, Color> d_scColors, Dictionary<string, Color> d_sclMaters)
	{	cModelo mdl; string sId; Matrix4x4 matTrans = default; XmlNode? ndTmp; XmlNode nd;
		Vertex[] a_ver; int[] a_i; int iVerts, iIdx, iV1, iV2, iV3; TextSpan[] a_tsSplitInds; float[] a_fSplit; int iSplitCant;
	
		sId = ndIt.Attributes!["objectid"]!.Value;
		// ** Cargar transform
		a_tsSplitInds = new TextSpan[12]; a_fSplit = new float[12]; iSplitCant = 12;
		_ = ndIt.Attributes["transform"]!.Value.SplitFloat(' ', a_tsSplitInds, a_fSplit, ref iSplitCant, CultInf.InvariantCulture); // ** Leer transform
			matTrans.M00 = a_fSplit[0]; matTrans.M01 = a_fSplit[2]; matTrans.M02 = a_fSplit[1]; matTrans.M03 = 0;
			matTrans.M10 = a_fSplit[6]; matTrans.M11 = a_fSplit[8]; matTrans.M12 = a_fSplit[7]; matTrans.M13 = 0;
			matTrans.M20 = a_fSplit[3]; matTrans.M21 = a_fSplit[5]; matTrans.M22 = a_fSplit[4]; matTrans.M23 = 0;
			matTrans.M30 = -a_fSplit[9]; matTrans.M31 = a_fSplit[11]; matTrans.M32 = -a_fSplit[10]; matTrans.M33 = 1;
			matTrans *= matPad;												// Aplicar trans del pad
		// ** Buscar obj
		ndIt = null!;
		foreach (XmlNode nd2 in ndResources.ChildNodes)
		{	if (nd2.Name == "object" && nd2.Attributes!["id"]!.Value == sId) {	ndIt = nd2; break;} // Encont: saltar
		}
		// ** Cargar mesh
		ndTmp = ndIt?["mesh"];
		if (ndTmp is not null)												// ** Tiene verts
		{	// ** Leer verts
			nd = ndTmp["vertices"]!; a_ver = new Vertex[nd.ChildNodes.Count]; iVerts = 0;
			foreach (XmlNode ndVert in nd)
			{		if (ndVert.Name != "vertex")	continue;				// No es vert (nunca): omitir
				a_ver[iVerts].Position = new Vector(-ndVert.Attributes!["x"]!.Value.ToFloatInvariant(), ndVert.Attributes["z"]!.Value.ToFloatInvariant()
					, -ndVert.Attributes["y"]!.Value.ToFloatInvariant());
				a_ver[iVerts].Normal = Vector.NaN;
				iVerts++;
			}
			Vector.Transform(matTrans, a_ver, Vertex.SIZE_OF, 0, 0, a_ver, Vertex.SIZE_OF, 0, 0, iVerts); // ** Transform verts
			// ** Leer idxs: calc normales
			nd = ndTmp["triangles"]!; a_i = new int[nd.ChildNodes.Count * 3]; iIdx = 0;
			foreach (XmlNode ndTri in nd)
			{		if (ndTri.Name != "triangle")	continue;				// No es triang (nunca): omitir
				iV1 = ndTri.Attributes!["v3"]!.Value.ToIntInvariant(); iV2 = ndTri.Attributes["v2"]!.Value.ToIntInvariant(); // Contrarreloj
					iV3 = ndTri.Attributes["v1"]!.Value.ToIntInvariant();
				//¿s_CalcNormalSmooth(a_ver, iVerts, iV1, iV2, iV3);
				s_CalcNormalFlat(ref a_ver, ref iVerts, ref iV1, ref iV2, ref iV3);
				a_i[iIdx++] = iV1; a_i[iIdx++] = iV2; a_i[iIdx++] = iV3;	// Tomar pts
			}
			// ** Crear mdl
			mdl = new cModelo(a_ver, iVerts, a_i, iIdx, eVertexLayout.PosNorm);
			mdl.Material.Type = eMaterial.Specular;							// Aplicar mater
				if (ndIt!.Attributes!["materialid"] is not null)	mdl.Material.Color = d_sclMaters[ndIt.Attributes["materialid"]!.Value];
		} else																// ** No tiene verts
		{	mdl = new cModelo();											// Crear cnt
			mdl.MinimumVector = mdl.MaximumVector = Vector.NaN;
		}
		Parts.Add(mdl); mdl.Name = sId;
		// ** Cargar partes
		ndTmp = ndIt!["components"];
		if (ndTmp is not null)
		{	foreach (XmlNode nd2 in ndTmp.ChildNodes)
				if (nd2.Name == "component")	mdl.m_Carga3mfItem(nd2, ndResources, matTrans, d_scColors, d_sclMaters);
		}
		// ** Completar
		mdl.m_ActualizaLimCnt(ref MinimumVector, ref MaximumVector);
		mdl.OnLoaded();														// ** Notif
	}
	private void m_CargaObj(string sArchi)
	{	const int BLK_TAM			= 5000;
		const int MAX_INDS_SPLIT	= 11;
		Dictionary<string, Material> d_smtMaters; Array<cBoneAnimation.Bone> a_bonHuesos; // Bufs totales
		Array<Vector> a_vPts, a_vNormales; Array<Vector> a_vTangs; Array<Point> a_ptTexcoords;
		Array<VertexBone.Weights> a_vwPesos; Array<VertexBone.Bones> a_vbVertHuesos;
		cModelo mdl; string? s, sNomb = null, sMater = null; int[] a_iIdx, a_iPtId;
		TextSpan[] a_tsSplitInds, a_tsSplInd2; int[] a_iSplit; float[] a_fSplit; int iSplitCant; Vector v; Point pt;
		using StreamReader sr = new (File.OpenRead(sArchi));

		d_smtMaters = [];
		a_vPts = new Array<Vector>(BLK_TAM); a_vTangs = new Array<Vector>(BLK_TAM); // Los bufs (vecs y tcs) de los mdls están separados pero pertenecen a uno solo.  Los inds se basan en este buf
		a_vNormales = new Array<Vector>(BLK_TAM); a_ptTexcoords = new Array<Point>(BLK_TAM);
		a_bonHuesos = new Array<cBoneAnimation.Bone>(BLK_TAM);
		a_vwPesos = new Array<VertexBone.Weights>(BLK_TAM); a_vbVertHuesos = new Array<VertexBone.Bones>(BLK_TAM);
		a_iIdx = new int[BLK_TAM]; a_iPtId = new int[BLK_TAM];
		a_tsSplitInds = new TextSpan[MAX_INDS_SPLIT]; a_tsSplInd2 = new TextSpan[MAX_INDS_SPLIT];
			a_iSplit = new int[4]; a_fSplit = new float[MAX_INDS_SPLIT];
		MinimumVector = MaximumVector = Vector.NaN;
		// ** Leer archivo
		s = s_sCargaObjLeeLin(sr);										// Avanzar
		// ** Leer maters
		if (s?.StartsWithOrdinal("mtllib ") == true)
		{	m_CargaObjMtl(Path.Combine(Path.GetDirectoryName(sArchi)!, s[7..]), d_smtMaters);
			s = s_sCargaObjLeeLin(sr);									// Avanzar
		}
		//¿ ** Leer huesos
		if (s?.StartsWithOrdinal("bo ") == true)
		{	// ** Leer desps
			do
			{	Matrix4x4 m44;	_ = Matrix4x4.TryParseInvariant(s, out m44, 2, ' '); a_bonHuesos.Add(new cBoneAnimation.Bone {	Matrix = m44});
				s = s_sCargaObjLeeLin(sr);								// Avanzar
			} while (s?.StartsWithOrdinal("bo ") == true);
			// ** Leer jerarq
			if (s?.StartsWithOrdinal("bh ") == true)
			{	int i = 0;
				
				do
				{	a_bonHuesos[i].Parent = s.ToIntInvariant(2);
					a_bonHuesos[i].AbsoluteLocation = (i == 0 ? a_bonHuesos[i].Matrix : a_bonHuesos[i].Matrix * a_bonHuesos[a_bonHuesos[i].Parent].AbsoluteLocation);
					i++;
					s = s_sCargaObjLeeLin(sr);							// Avanzar
				} while (s?.StartsWithOrdinal("bh ") == true);
			}
			BoneTransforms = new Matrix4x4[a_bonHuesos.Count]; Array.Fill(BoneTransforms, Matrix4x4.Identity.GetTransposed());
			Bones = a_bonHuesos.Data.Clone(0, a_bonHuesos.Count);
			// ** Leer anims
			if (s?.StartsWithOrdinal("ani ") == true)
			{	cBoneAnimation ba; int i, j; cBoneAnimation.FrameList bfs; float fDura;

				Animations = []; fDura = 0;
				do
				{	s = s[4..].Trim();
					ba = new cBoneAnimation(this) {	Frames = new cBoneAnimation.FrameList[a_bonHuesos.Count]};
					Animations.Add(s, ba);
					s = s_sCargaObjLeeLin(sr);							// Avanzar
					// ** Leer fmes
					if (s?.StartsWithOrdinal("bfs ") == true)
					{	i = 0;
						do
						{	bfs = new cBoneAnimation.FrameList {	Frames = new cBoneAnimation.Frame[s.ToIntInvariant(3)]};
							ba.Frames[i] = bfs; i++; j = 0;
							s = s_sCargaObjLeeLin(sr);					// Avanzar
							// ** Leer fme
							if (s?.StartsWithOrdinal("bf ") == true)
							{	do
								{	iSplitCant = 11; _ = s.SplitFloat(' ', a_tsSplitInds, a_fSplit, ref iSplitCant, CultInf.InvariantCulture, 2);
										if (iSplitCant != 11)	throw new System.FormatException("bone frame");
									bfs.Frames[j] = new cBoneAnimation.Frame
										{	Time = a_fSplit[0]
											, Offset = new Vector(a_fSplit[1], a_fSplit[2], a_fSplit[3])
											, Scale = new Vector(a_fSplit[4], a_fSplit[5], a_fSplit[6])
											, Rotation = new Quaternion(a_fSplit[7], a_fSplit[8], a_fSplit[9], a_fSplit[10])
										};
										if (bfs.Frames[j].Time > fDura)	fDura = bfs.Frames[j].Time;
									j++;
									s = s_sCargaObjLeeLin(sr);			// Avanzar
								} while (s?.StartsWithOrdinal("bf ") == true);
							}
						} while (s?.StartsWithOrdinal("bfs ") == true);
					}
					ba.Duration = fDura;
				} while (s?.StartsWithOrdinal("ani ") == true);
			}
		}
		// ** Leer datos
		while (s is not null)
		{	// ** Nuevo mdl
			if (s.StartsWithOrdinal("g "))
			{	sNomb = s[2..];
				s = s_sCargaObjLeeLin(sr);								// Avanzar
			// ** Cambio de mater
			} else if (s.StartsWithOrdinal("usemtl "))
			{	sMater = s[7..];
				s = s_sCargaObjLeeLin(sr);								// Avanzar
			//? ** 
			} else if (s.StartsWithOrdinal("s "))
			{	s = s_sCargaObjLeeLin(sr);								// Avanzar
			// ** Leer pts
			} else if (s.StartsWithOrdinal("v "))
			{	do
				{	_ = Vector.TryParseInvariant(s, out v, 2, ' '); v.Z = -v.Z; a_vPts.Add(v); // Usar left hand
					s = s_sCargaObjLeeLin(sr);							// Avanzar
				} while (s?.StartsWithOrdinal("v ") == true);
			// ** Leer texcoords
			} else if (s.StartsWithOrdinal("vt "))
			{	do
				{	_ = Point.TryParseInvariant(s, out pt, 3, ' ');
					pt.X = s_fWrapTex(pt.X); pt.Y = 1 - s_fWrapTex(pt.Y); a_ptTexcoords.Add(pt); // Usar left hand
					s = s_sCargaObjLeeLin(sr);							// Avanzar
				} while (s?.StartsWithOrdinal("vt ") == true);
			// ** Leer normales
			} else if (s.StartsWithOrdinal("vn "))
			{	do
				{	_ = Vector.TryParseInvariant(s, out v, 3, ' '); v.Z = -v.Z; a_vNormales.Add(v); // Usar left hand
					s = s_sCargaObjLeeLin(sr);							// Avanzar
				} while (s?.StartsWithOrdinal("vn ") == true);
			// ** Leer tangentes
			} else if (s.StartsWithOrdinal("vg "))
			{	do
				{	_ = Vector.TryParseInvariant(s, out v, 3, ' '); v.Z = -v.Z; a_vTangs.Add(v); // Usar left hand
					s = s_sCargaObjLeeLin(sr);							// Avanzar
				} while (s?.StartsWithOrdinal("vn ") == true);
			// ** Leer pesos
			} else if (s.StartsWithOrdinal("vw "))
			{	do
				{	_ = Vector.TryParseInvariant(s, out v, 3, ' '); a_vwPesos.Add(new VertexBone.Weights(v.X, v.Y, v.Z)); // Usar left hand
					s = s_sCargaObjLeeLin(sr);							// Avanzar
				} while (s?.StartsWithOrdinal("vw ") == true);
			// ** Leer huesos
			} else if (s.StartsWithOrdinal("vb "))
			{	do
				{	iSplitCant = 4; _ = s.SplitInt(' ', a_tsSplitInds, a_iSplit, ref iSplitCant, CultInf.InvariantCulture, 3);
						if (iSplitCant != 4)	throw new System.FormatException("bone indices");
					a_vbVertHuesos.Add(new VertexBone.Bones((byte)a_iSplit[0], (byte)a_iSplit[1], (byte)a_iSplit[2], (byte)a_iSplit[3]));
					s = s_sCargaObjLeeLin(sr);							// Avanzar
				} while (s?.StartsWithOrdinal("vb ") == true);
			// ** Leer caras: crear mdl (o parte)
			} else if (s?.StartsWithOrdinal("f ") == true)
			{	VertexBone[] a_vxb; int i, iInds, iVerts, iIdx, iFaceVerts, iFaceVertParts; bool bTieneTex = false, bTieneHue = false;
				System.ReadOnlySpan<TextSpan> ros = new (a_tsSplitInds), ros2 = new (a_tsSplInd2);

				if (a_iPtId.Length < a_vPts.Count)	Array.Resize(ref a_iPtId, a_vPts.Count); // Los ids y pts deben tener la misma cant
				for (i = 0; i < a_iPtId.Length; i++)	a_iPtId[i] = -1; // Limpiar ids
				a_vxb = new VertexBone[1000]; iInds = 0; iVerts = 0;
				do
				{	// ** Tomar verts
					if (iInds + 6 >= a_iIdx.Length) Array.Resize(ref a_iIdx, iInds + BLK_TAM); // Expandir inds
					iFaceVerts = s.IndicesOf(' ', ros, 4, 2);	if (iFaceVerts < 3)	throw new System.FormatException("face coord");
					(a_tsSplitInds[0], a_tsSplitInds[2]) = (a_tsSplitInds[2], a_tsSplitInds[0]); // Ordenar verts a contrarreloj para left hand
					// ** Agregar inds (y verts evitando repetición)
					for (i = 0; i < iFaceVerts; i++)
					{	iFaceVertParts = s.IndicesOf('/', ros2, 6, a_tsSplitInds[i].Start, a_tsSplitInds[i].Count);
							if (iFaceVertParts < 1)	throw new System.FormatException("face coord"); // Tomar inds de pt, tex y normal
						iIdx = s.ToIntInvariant(a_tsSplInd2[0].Start, a_tsSplInd2[0].Count) - 1; // Los inds están basados en 1
						if (a_iPtId[iIdx] == -1)						// ** Pt nuevo: agregar vert
						{	VertexBone vxb = default;

							vxb.Vertex.Position = a_vPts[iIdx];			// Pos
							if (iFaceVertParts > 1)
							{	if (a_tsSplInd2[1].Count != 0)			// Texcoord
								{	vxb.Vertex.Texcoord = a_ptTexcoords[s.ToIntInvariant(a_tsSplInd2[1].Start, a_tsSplInd2[1].Count) - 1];
									bTieneTex = true;
								}
								if (iFaceVertParts > 2)
								{	vxb.Vertex.Normal = a_vNormales[s.ToIntInvariant(a_tsSplInd2[2].Start, a_tsSplInd2[2].Count) - 1]; // Normal
									if (iFaceVertParts > 3)
									{	vxb.Vertex.Tangent = a_vTangs[s.ToIntInvariant(a_tsSplInd2[3].Start, a_tsSplInd2[3].Count) - 1]; // Tang
										if (iFaceVertParts > 4)
										{	vxb.Weight = a_vwPesos[s.ToIntInvariant(a_tsSplInd2[4].Start, a_tsSplInd2[4].Count) - 1]; // Peso
											bTieneHue = true;
											if (iFaceVertParts > 5)		// Hueso
											{	vxb.Bone = a_vbVertHuesos[s.ToIntInvariant(a_tsSplInd2[5].Start, a_tsSplInd2[5].Count) - 1];
											}
										}
									}
								}
							}
							a_vxb[iVerts] = vxb;
							a_iPtId[iIdx] = iVerts;						// Asig id a pt
							iVerts++;	if (iVerts == a_vxb.Length)	Array.Resize(ref a_vxb, iVerts * 4); // Expand verts
						}
						a_iIdx[iInds] = a_iPtId[iIdx]; iInds++;			// Agregar ind
					}
					// ** Conv cuadrados en 2 triangs
					if (iFaceVerts == 4)								// Agregar 2 inds (a contrarreloj)
					{	a_iIdx[iInds] = a_iIdx[iInds - 4]; a_iIdx[iInds + 1] = a_iIdx[iInds - 2]; iInds += 2;
					}
					s = s_sCargaObjLeeLin(sr);							// ** Avanzar
					if (s?.StartsWithOrdinal("s ") == true)	s = s_sCargaObjLeeLin(sr); //? Avanzar
				} while (s?.StartsWithOrdinal("f ") == true);
				// ** Crear mdl
				mdl = new cModelo(a_vxb.Clone(0, iVerts), iVerts, a_iIdx.Clone(0, iInds), iInds
					, (bTieneHue ? eVertexLayout.PosNormTexTangWeiBon : (bTieneTex ? eVertexLayout.PosNormTex : eVertexLayout.PosNorm)))
					{	Name = sNomb + " " + sMater
					};
				Parts.Add(mdl);
				if (sMater is not null && d_smtMaters.TryGetValue(sMater, out Material value))	mdl.Material = value;
				mdl.m_ActualizaLimCnt(ref MinimumVector, ref MaximumVector);
				mdl.OnLoaded();
			// ** No reconocido: ignorar
			} else
			{	s = s_sCargaObjLeeLin(sr);								// Avanzar
			}
		}
		// ** Completar
		m_ActualizaLimCnt(ref MinimumVector, ref MaximumVector, false);
		OnPositionChanged();
	}
	private void m_CargaObjMtl(string sArchi, Dictionary<string, Material> d_smtMaters)
	{	string? s; string sNomb; Material mtl; using StreamReader sr = new (File.OpenRead(sArchi));

		s = s_sCargaObjLeeLin(sr);											// Avanzar
		while (s?.StartsWithOrdinal("newmtl ") == true)
		{	sNomb = s[7..]; mtl = Material.Default; mtl.PixelShader = Device.PSStandard;
			s = s_sCargaObjLeeLin(sr);										// Avanzar
			while (s is not null && !s.StartsWithOrdinal("newmtl "))
			{	if (s.StartsWithOrdinal("illum "))
					mtl.Type = (eMaterial)s.ToIntInvariant(6);
				else if (s.StartsWithOrdinal("Ka "))
				{
				} else if (s.StartsWithOrdinal("Kd "))
					_ = Color.TryParseInvariant(s, out mtl.Color, 3, ' ');
				else if (s.StartsWithOrdinal("Ks "))
					_ = Color.TryParseInvariant(s, out mtl.SpecularColor, 3, ' ');
				else if (s.StartsWithOrdinal("d "))
				{	mtl.Alpha = s.ToFloatInvariant(2); mtl.AllowTransparency = true; mtl.ClipAlpha = true;
				} else if (s.StartsWithOrdinal("Ns "))
					mtl.SpecularPower = (int)s.ToFloatInvariant(3);
				else if (s.StartsWithOrdinal("map_Kd "))
					mtl.Texture = GetTextura(Path.Combine(Path.GetDirectoryName(sArchi)!, s[7..]));
				else if (s.StartsWithOrdinal("bump "))
				{	mtl.NormalMap = GetTextura(Path.Combine(Path.GetDirectoryName(sArchi)!, s[5..]));
				}
				s = s_sCargaObjLeeLin(sr);									// Avanzar
			}
			d_smtMaters.Add(sNomb, mtl);									// ** Agregar mater
		}
	}
	private void m_CargaX(string sArchi)
	{	Array<mXMeshLoader.iXDataObject> a_doArchi; string meshDirectory;
		int i, iFin; mXMeshLoader.iXDataObject[] a_do; mXMeshLoader.iXDataObject obj;
		using StreamReader xFile = File.OpenText(sArchi);

		// ** Leer archivo
		meshDirectory = Path.GetDirectoryName(sArchi)!;
		mXMeshLoader.ValidateHeader(xFile.ReadLine()!);
		a_doArchi = mXMeshLoader.XDataObjectFactory.ExtractDataObjects(xFile.ReadToEnd());
		a_do = a_doArchi.Data!; iFin = a_doArchi.Count;
		// ** Cargar estructura
		for (i = 0; i < iFin; i++)
		{	obj = a_do[i]; if (obj.IsVisualObject)	mXMeshLoader.LoadMeshes(Device, meshDirectory, obj);
		}
		// ** Cargar raíz
		for (i = 0; i < iFin; i++)
		{	obj = a_do[i];	if (!obj.IsVisualObject)	continue;
			// ** Grupo
			if (obj.DataObjectType == "Frame")
			{	MinimumVector = MaximumVector = Vector.NaN;
				m_CargaXItems(obj);
			// ** Obj
			} else if (obj.DataObjectType == "Mesh")
			{	cModelo mdl; mXMeshLoader.XDataObjectFactory.XDataObject doMesh = (mXMeshLoader.XDataObjectFactory.XDataObject)obj;

				mdl = new cModelo(doMesh.Vertices!, doMesh.VertexCant, doMesh.Indices.Data!, doMesh.Indices.Count, doMesh.VertexLayout);
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
	private void m_CargaXItems(mXMeshLoader.iXDataObject xdoCnt)
	{	mXMeshLoader.iXDataObject[] a_do, a_do2; mXMeshLoader.iXDataObject childObject, childObject2;

		a_do = xdoCnt.Children.Data!;
		for (int i = 0, iFin = a_do.Length; i < iFin; i++)
		{	childObject = a_do[i];	if (!childObject.IsVisualObject)	continue;
			// ** Grupo
			if (childObject.DataObjectType == "Frame")
			{	cModelo mdl; mXMeshLoader.XDataObjectFactory.XDataObject? doMesh;
				
				a_do2 = childObject.Children.Data!; doMesh = null;
					for (int j = 0, iFin2 = a_do2.Length; j < iFin2; j++)
					{	childObject2 = a_do2[j];
						if (childObject2.DataObjectType == "Mesh")
						{	doMesh = (mXMeshLoader.XDataObjectFactory.XDataObject)childObject2;
							break;
						}
					}
				if (doMesh is not null)
				{	mdl = new cModelo(doMesh.Vertices!, doMesh.VertexCant, doMesh.Indices.Data!, doMesh.Indices.Count, doMesh.VertexLayout)
						{	Material = doMesh.Material};
				} else
				{	mdl = new cModelo() {	MinimumVector = Vector.NaN, MaximumVector = Vector.NaN}; // Crear cnt
				}
				Parts.Add(mdl);
				mdl.Name = childObject.Name;
				mdl.m_CargaXItems(childObject);
				mdl.m_ActualizaLimCnt(ref MinimumVector, ref MaximumVector);
				mdl.OnLoaded();												// ** Notif
			}
		}
	}
	private void m_RecalcLims(ref Vector vMinCnt, ref Vector vMaxCnt)
	{	MinimumVector = MaximumVector = Vector.NaN;							// Iniciar
		Vector.GetBounds(out MinimumPartVector, out MaximumPartVector, ma_vxbVerts, m_iVertStride, 0, VertexCount); // Tomar lim propio
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
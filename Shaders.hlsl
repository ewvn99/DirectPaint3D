#include "VS.hlsli"

struct PixelSimple
{	float4 Pos		: SV_POSITION;
	float4 Clr	    : COLOR;
};

cbuffer BufGlob : register(b3)
{	float4 g_ClipPlane;
};
cbuffer BufInstan : register(b4)											// Datos de instancia
{	matrix g_WorldInstan;													// World
	float4 g_ClrInstan;														// Color
};

namespace VSStd
{
		[clipplanes(g_ClipPlane)]
	Pixel main(VertexBone vboVert)
	{	Pixel pix;
	
		if (g_Animating)	CalculateBone(vboVert);
		pix = CalculatePixel(vboVert.Vertex);
		return pix;
	}
}

namespace VSPunto
{
	struct VertexPto
	{	float4 Pos			: POSITION;
		float4 PosInst		: DESP;
		bool Sel			: SEL;
		float3 WeightsInst	: WEIGHTS;
		uint4 BonesInst		: BONES;
	};

	PixelSimple main(VertexPto vptVer)
	{	PixelSimple res; float4 f4PosInst, f4; float3 norm; float4 tang = 0;

		if (g_Animating)													// Aplicar anim
		{	VertexBone vxb;
		
			vxb.Vertex.Position = vptVer.PosInst; vxb.Vertex.Normal = 0; vxb.Vertex.Tangent = 0; vxb.Vertex.Texcoord = 0;
				vxb.Weights = vptVer.WeightsInst; vxb.Bones = vptVer.BonesInst;
			CalculateBone(vxb); vptVer.PosInst = vxb.Vertex.Position;
		}
		f4PosInst = mul(vptVer.PosInst, g_WorldInstan); f4PosInst.w = 0;	// Calc pos de inst; limpiar w
		f4 = mul(vptVer.Pos, g_WorldMatrix);								// Calc pos de esquina
		f4 = mul(f4PosInst + f4, g_ViewProjectionMatrix);					// Sumar ambas pos; aplicar vista
		res.Pos = f4;
		res.Clr = saturate((!vptVer.Sel ? g_ClrInstan : float4(0, 0, 0.7, 1)) + vptVer.Pos * 5);
		return res;
	}
}

namespace VSGrid
{
	struct VertexGrid
	{	float4 Pos			: POSITION;
		float4 InstClr		: COLOR;
		float Desp			: DESP;
		float DespSemiEje	: DESP_SEMIEJE;
		int Perpend			: PERPEND;
	};

	PixelSimple main(VertexGrid vgrVert)
	{	PixelSimple res; float4 f4 = vgrVert.Pos;

		if (vgrVert.Perpend == 0)											// ** Paralelas a X
		{	f4.z += vgrVert.Desp; f4.x += vgrVert.DespSemiEje;
		} else if (vgrVert.Perpend == 1)									// ** Eje Y
		{	f4.xy = f4.yx; f4.y += vgrVert.DespSemiEje;
		} else																// ** Paralelas a Z
		{	f4.xz = f4.zx; f4.x += vgrVert.Desp; f4.z += vgrVert.DespSemiEje;
		}
		res.Pos = mul(f4, g_ViewProjectionMatrix);
		res.Clr = vgrVert.InstClr;
		return res;
	}
}

namespace PSSimple
{
	float4 main(PixelSimple psiPix) : SV_TARGET
	{	return psiPix.Clr;
	}
}
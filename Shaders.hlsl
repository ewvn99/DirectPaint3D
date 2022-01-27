#include "VS.hlsli"

struct PixelSimple
{	float4 Pos		: SV_POSITION;
	float4 Clr	    : COLOR;
};

namespace VSStd
{
	cbuffer BufClip : register(b3)
	{	float4 ClipPlane;
	};

	[clipplanes(ClipPlane)]
	Pixel main(Vertex verVert)
	{	Pixel pix;
	
		pix = CalculatePixel(verVert.Position, verVert.Normal, verVert.Texcoord);
		return pix;
	}
}

namespace VSPunto
{
	struct VertexPto
	{	float4 Pos			: POSITION;
		float4 PosInst		: DESP;
		bool Sel			: SEL;
	};

	cbuffer BufInstan : register(b2)										// Datos de instancia
	{	matrix g_WorldInstant;												// World
		float4 g_ClrInstan;													// Color
	};

	PixelSimple main(VertexPto vptVer)
	{	PixelSimple res; float4 f4PosInst, f4;

		f4PosInst = mul(vptVer.PosInst, g_WorldInstant); f4PosInst.w = 0;	// Calc pos de inst; limpiar w
		f4 = mul(vptVer.Pos, g_WorldMatrix);								// Calc pos de punto
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
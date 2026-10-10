using Cursor = System.Windows.Forms.Cursor;
using Wew.Media;

// ** eSlot -------------------------------------------------------------------------------------------------------------------------------
enum eSlot
{	Glob	= 3,
	Instan,
	Huesos2
}
// ** eRes --------------------------------------------------------------------------------------------------------------------------------
static class eRes
{	public static readonly Cursor CurManoParte	= new (typeof(eRes), "Res.ManoParte.cur");
	public static readonly Cursor CurManoPunto	= new (typeof(eRes), "Res.ManoPunto.cur");
	public static readonly cBitmap BmpCubo		= new (typeof(eRes), "Res.Cubo.png");
	public static readonly cBitmap BmpParte		= new (typeof(eRes), "Res.Parte.png");
	public static readonly cBitmap BmpPlay		= new (typeof(eRes), "Res.Play.png");
	public static readonly cBitmap BmpPunto		= new (typeof(eRes), "Res.Punto.png");
	public static readonly cBitmap BmpPuntos	= new (typeof(eRes), "Res.Puntos.png");
	public static readonly cBitmap BmpHerrs		= new (typeof(eRes), "Res.Herrs.png");
	public static readonly cBitmap BmpVerDer	= new (typeof(eRes), "Res.Der.png");
	public static readonly cBitmap BmpVerIzq	= new (typeof(eRes), "Res.Izq.png");
	public static readonly cBitmap BmpVerFrente	= new (typeof(eRes), "Res.Frente.png");
	public static readonly cBitmap BmpVerFondo	= new (typeof(eRes), "Res.Fondo.png");
	public static readonly cBitmap BmpVerSup	= new (typeof(eRes), "Res.Sup.png");
	public static readonly cBitmap BmpVerInf	= new (typeof(eRes), "Res.Inf.png");
	public static readonly cBitmap BmpManoParte	= new (typeof(eRes), "Res.ManoParte.png");
	public static readonly cBitmap BmpManoPunto	= new (typeof(eRes), "Res.ManoPunto.png");
	public static readonly cBitmap BmpLinLibre	= new (typeof(eRes), "Res.LinLibre.png");
	public static readonly cBitmap BmpLin		= new (typeof(eRes), "Res.Lin.png");
	public static readonly cBitmap BmpArc		= new (typeof(eRes), "Res.Arc.png");
	public static readonly cBitmap BmpBezier	= new (typeof(eRes), "Res.Bezier.png");
	public static readonly cBitmap BmpQuadraticBezier = new (typeof(eRes), "Res.QuadraticBezier.png");
	public static readonly cBitmap BmpLimpiaFondo = new (typeof(eRes), "Res.LimpiaFondo.png");
}
// ** mMod --------------------------------------------------------------------------------------------------------------------------------
static class mMod
{	public const string FILTRO_MDL				= "3D models|*.mdl|All files|*.*";
	public const string FILTRO_MDLS				= "3D models|*.mdl;*.3mf;*.obj;*.x|Mdl|*.mdl|3mf|*.3mf|Obj|*.obj|X|*.x|All files|*.*";
	public const string FILTRO_GRAB				= "All files|*.*";
	public static readonly string FILTRO_IMG	= mImaging.GetFileDialogFilter(false);
	public static readonly System.Guid DLG_GUID_MDL = new ("{AC33F544-7E1A-44b0-9EFC-AA3C043BAC4D}");
	public static readonly System.Guid DLG_GUID_IMG = new ("{8DA960AD-24DE-4aca-8E66-3F7AFFB7C0BA}");
	public static readonly System.Guid DLG_GUID_BEZ = new ("{8DA342AD-24DE-4aca-8E66-3F7AFFB7C0BA}");
		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Size = 32)]
	public struct BufConst
	{	public Plane Plane;
	}
		#pragma warning disable CS8618											// Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de declararlo como que admite un valor NULL.
	public static wMain MainWnd;
	public static c3DSimpleBuffer<BufConst> ConstBuf;
	public static kParte PropsParte;
	public static kPunto PropsPunto;
	public static kHerrs Herrs;
	public static cPixelShader PsSimple;
		#pragma warning restore CS8618
	public static System.Collections.Generic.List<c3DModel> Raíces = [];
	public static System.Collections.Generic.List<cModelo> Partes = [];
}
using Cursor = System.Windows.Forms.Cursor;
using Wew.Media;

namespace DirectPaint3D
{
// ** mRes --------------------------------------------------------------------------------------------------------------------------------
internal static class mRes
{	public static readonly Cursor CurManoParte = new Cursor(typeof(mRes), "Graf.ManoParte.cur");
	public static readonly Cursor CurManoPunto = new Cursor(typeof(mRes), "Graf.ManoPunto.cur");
	public static readonly cBitmap BmpAbrir = new cBitmap(typeof(mRes), "Graf.Abrir.png");
	public static readonly cBitmap BmpCortar = new cBitmap(typeof(mRes), "Graf.Cortar.png");
	public static readonly cBitmap BmpCopiar = new cBitmap(typeof(mRes), "Graf.Copiar.png");
	public static readonly cBitmap BmpCubo = new cBitmap(typeof(mRes), "Graf.Cubo.png");
	public static readonly cBitmap BmpPegar = new cBitmap(typeof(mRes), "Graf.Pegar.png");
	public static readonly cBitmap BmpDeshacer = new cBitmap(typeof(mRes), "Graf.Deshacer.png");
	public static readonly cBitmap BmpRehacer = new cBitmap(typeof(mRes), "Graf.Rehacer.png");
	public static readonly cBitmap BmpParte = new cBitmap(typeof(mRes), "Graf.Parte.png");
	public static readonly cBitmap BmpPunto = new cBitmap(typeof(mRes), "Graf.Punto.png");
	public static readonly cBitmap BmpPuntos = new cBitmap(typeof(mRes), "Graf.Puntos.png");
	public static readonly cBitmap BmpHerrs = new cBitmap(typeof(mRes), "Graf.Herrs.png");
	public static readonly cBitmap BmpVerDer = new cBitmap(typeof(mRes), "Graf.Der.png");
	public static readonly cBitmap BmpVerIzq = new cBitmap(typeof(mRes), "Graf.Izq.png");
	public static readonly cBitmap BmpVerFrente = new cBitmap(typeof(mRes), "Graf.Frente.png");
	public static readonly cBitmap BmpVerFondo = new cBitmap(typeof(mRes), "Graf.Fondo.png");
	public static readonly cBitmap BmpVerSup = new cBitmap(typeof(mRes), "Graf.Sup.png");
	public static readonly cBitmap BmpVerInf = new cBitmap(typeof(mRes), "Graf.Inf.png");
	public static readonly cBitmap BmpManoParte = new cBitmap(typeof(mRes), "Graf.ManoParte.png");
	public static readonly cBitmap BmpManoPunto = new cBitmap(typeof(mRes), "Graf.ManoPunto.png");
	public static readonly cBitmap BmpElim = new cBitmap(typeof(mRes), "Graf.Elim.png");
	public static readonly cBitmap BmpGrabar = new cBitmap(typeof(mRes), "Graf.Grabar.png");
	public static readonly cBitmap BmpLinLibre = new cBitmap(typeof(mRes), "Graf.LinLibre.png");
	public static readonly cBitmap BmpLin = new cBitmap(typeof(mRes), "Graf.Lin.png");
	public static readonly cBitmap BmpArc = new cBitmap(typeof(mRes), "Graf.Arc.png");
	public static readonly cBitmap BmpBezier = new cBitmap(typeof(mRes), "Graf.Bezier.png");
	public static readonly cBitmap BmpQuadraticBezier = new cBitmap(typeof(mRes), "Graf.QuadraticBezier.png");
	public static readonly cBitmap BmpNuevo = new cBitmap(typeof(mRes), "Graf.Nuevo.png");
	public static readonly cBitmap BmpLimpiaFondo = new cBitmap(typeof(mRes), "Graf.LimpiaFondo.png");
}
// ** mMod --------------------------------------------------------------------------------------------------------------------------------
internal static class mMod
{	public const string FILTRO_MODELOS	= "3D models|*.mdl;*.3mf;*.obj;*.x|Mdl|*.mdl|3mf|*.3mf|Obj|*.obj|X|*.x|All files|*.*";
	public const string FILTRO_MDL	= "3D models|*.mdl|All files|*.*";
	public const string FILTRO_IMG	= "Images|*.png;*.jpg;*.gif;*.tif;*.tiff;*.dds;*.bmp;*.ico|All files|*.*";
	public const string FILTRO_IMG_GRAB	= "All files|*.*";
	public readonly static System.Guid DLG_GUID_MDL = new System.Guid("{AC33F544-7E1A-44b0-9EFC-AA3C043BAC4D}");
	public readonly static System.Guid DLG_GUID_IMG = new System.Guid("{8DA960AD-24DE-4aca-8E66-3F7AFFB7C0BA}");
	public static wMain MainWnd;
	public static kParte ParteProps;
	public static kPunto PuntoProps;
	public static kHerrs Herrs;
	public static cPixelShader PsSimple;
	public static System.Collections.Generic.List<c3DModel> Modelos;
	public static System.Collections.Generic.List<cModelo> Partes;
}
}
static class Program
{		[System.STAThread]
	static void Main()
	{
		//Wew.mHelper.SetBrkId(506);
	
		Wew.Control.cWindow.Main(() => new wMain());

		foreach (var item in typeof(eRes).GetFields())		((System.IDisposable)item.GetValue(null)!).Dispose();
		mMod.ConstBuf.Dispose(); mMod.PsSimple.Dispose();
		mMod.Herrs.f_Dispose();
		cModelo.sf_Dispose();
		wMain.TexEnvCube.Dispose();

		//Wew.mHelper.DisposeAll();
	}
}
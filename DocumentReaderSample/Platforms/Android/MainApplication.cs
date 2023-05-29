using Android.App;
using Android.Runtime;
using DocumentReaderSample.Platforms.Android;

namespace DocumentReaderSample;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
        DependencyService.Register<IDocReaderInit, DocReaderInit>();
        DependencyService.Register<IDocReaderScanner, DocReaderScanner>();
        DependencyService.Register<IPhotoPickerService, PhotoPickerService>();
    }

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
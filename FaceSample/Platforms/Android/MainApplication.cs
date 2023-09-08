using Android.App;
using Android.Runtime;
using FaceSample.Platforms.Android;

namespace FaceSample;

[Application(UsesCleartextTraffic = true)]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
        DependencyService.Register<IFaceSdkInit, FaceSdkInit>();
        DependencyService.Register<IFaceSdk, FaceSdk>();
        DependencyService.Register<IPhotoPickerService, PhotoPickerService>();
    }

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}


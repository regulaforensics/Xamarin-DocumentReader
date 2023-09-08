using FaceSample.Platforms.iOS;
using Foundation;

namespace FaceSample;

#pragma warning disable CA1806

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp()
	{
        //WARNING!!!!
        //Initialization FaceSDK from FaceCoreSDK is required
        new FaceCoreSDK.iOS.FaceSDK();

        DependencyService.Register<IFaceSdkInit, FaceSdkInit>();
        DependencyService.Register<IFaceSdk, FaceSdk>();
        DependencyService.Register<IPhotoPickerService, PhotoPickerService>();

        return MauiProgram.CreateMauiApp();
	}
}
using DocumentReaderSample.Platforms.iOS;
using Foundation;

namespace DocumentReaderSample;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp()
    {
        // WARNING: necessary!
        // These 3 lines prevent MAUI from shrinking assemblies
        new DocReaderCore.iOS.DocumentReader();
        new RegulaCommon.iOS.RGLCCamera();
        new BTDevice.iOS.RGLBTManager();

        DependencyService.Register<IDocReaderInit, DocReaderInit>();
        DependencyService.Register<IDocReaderScanner, DocReaderScanner>();
        DependencyService.Register<IPhotoPickerService, PhotoPickerService>();

        return MauiProgram.CreateMauiApp();
    }
}
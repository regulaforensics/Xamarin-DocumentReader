using DocumentReaderSample.Platforms.iOS;
using Foundation;

namespace DocumentReaderSample;

#pragma warning disable CA1806

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp()
    {
        //WARNING!!!!
        //Initialization of DocReaderCore is required
        new DocReaderCore.iOS.DocumentReader();

        DependencyService.Register<IDocReaderInit, DocReaderInit>();
        DependencyService.Register<IDocReaderScanner, DocReaderScanner>();
        DependencyService.Register<IPhotoPickerService, PhotoPickerService>();

        return MauiProgram.CreateMauiApp();
    }
}
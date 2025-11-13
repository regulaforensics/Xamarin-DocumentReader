using DocumentReaderSample.Platforms.iOS;
using Foundation;

namespace DocumentReaderSample;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp()
    {
        DependencyService.Register<IDocReaderInit, DocReaderInit>();
        DependencyService.Register<IDocReaderScanner, DocReaderScanner>();
        DependencyService.Register<IPhotoPickerService, PhotoPickerService>();

        return MauiProgram.CreateMauiApp();
    }
}
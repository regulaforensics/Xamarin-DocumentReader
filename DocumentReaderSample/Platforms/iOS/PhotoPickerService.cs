using UIKit;
#pragma warning disable CA1422

namespace DocumentReaderSample.Platforms.iOS
{
    public class PhotoPickerService : IPhotoPickerService
    {
        TaskCompletionSource<Stream> taskCompletionSource;
        UIImagePickerController imagePicker;
        public Task<Stream> GetImageStreamAsync()
        {
            imagePicker = new UIImagePickerController
            {
                SourceType = UIImagePickerControllerSourceType.PhotoLibrary,
                MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary)
            };
            imagePicker.FinishedPickingMedia += OnImagePickerFinishedPickingMedia;
            imagePicker.Canceled += OnImagePickerCancelled;

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(imagePicker, true, null);
            taskCompletionSource = new TaskCompletionSource<Stream>();
            return taskCompletionSource.Task;
        }
        void OnImagePickerFinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs args)
        {
            UIImage image = args.EditedImage ?? args.OriginalImage;
            if (image == null)
            {
                UnregisterEventHandlers();
                taskCompletionSource.SetResult(null);
                imagePicker.DismissModalViewController(true);
                return;
            }
            Stream stream;
            if (args.ReferenceUrl.PathExtension.Equals("PNG") || args.ReferenceUrl.PathExtension.Equals("png")) stream = image.AsPNG().AsStream();
            else stream = image.AsJPEG(1).AsStream();
            UnregisterEventHandlers();
            taskCompletionSource.SetResult(stream);
            imagePicker.DismissModalViewController(true);
        }
        void OnImagePickerCancelled(object sender, EventArgs args)
        {
            UnregisterEventHandlers();
            taskCompletionSource.SetResult(null);
            imagePicker.DismissModalViewController(true);
        }
        void UnregisterEventHandlers()
        {
            imagePicker.FinishedPickingMedia -= OnImagePickerFinishedPickingMedia;
            imagePicker.Canceled -= OnImagePickerCancelled;
        }
    }
}
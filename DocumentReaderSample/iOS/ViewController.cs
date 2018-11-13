using System;

using UIKit;
using DocReaderApi.Beta.iOS;
using Foundation;
using Photos;
using CoreFoundation;

namespace DocumentReaderSample.iOS
{
    public partial class ViewController : UIViewController
    {
        DocReader docReader;

        protected ViewController(IntPtr handle) : base(handle) =>
        //WARNING!!!!
        //Initialization DocumentReader from DocReaderCore is required
        new DocReaderCore.Full.Beta.iOS.DocumentReader();

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitializationReader();
        }

        void InitializationReader()
        {
            initLabel.Hidden = false;
            initIndocator.Hidden = false;
            scenariosView.Hidden = true;

            //initialize license
            var licenseData = NSData.FromFile(NSBundle.MainBundle.PathForResource("regula.license", null));
            if (licenseData == null)
            {
                initIndocator.Hidden = true;
                return;
            }

            //set scenario by default
            var processParams = new ProcessParams
            {
                Scenario = "Ocr"
            };
            //create DocReader object with default ProcessParams
            docReader = new DocReader(processParams);
            docReader.PrepareDatabaseWithDatabaseID("Full", (NSProgress obj) => {
                Console.WriteLine(obj.FractionCompleted);
            },(arg1, arg2) => {
                docReader.InitilizeReaderWithLicense(licenseData, DocReaderInitCompleted);
            });
        }

        void DocReaderInitCompleted(bool successfull, NSString error)
        {
            initLabel.Hidden = true;
            initIndocator.Hidden = true;
            scenariosView.Hidden = false;
            if (successfull)
            {

                btnImage.Enabled = true;
                bntCamera.Enabled = true;

                var picker = new ScenarioPickerModel(docReader.AvailableScenarios);
                scenariosView.Model = picker;
                picker.ValueChanged += (object sender, EventArgs e) =>
                {
                    docReader.ProcessParams.Scenario = picker.SelectedValue;
                };


                //Get available scenarios
                foreach (var scenarion in docReader.AvailableScenarios)
                {
                    Console.WriteLine(scenarion);
                }

                docReader.ProcessParams.Scenario = docReader.AvailableScenarios[0].Identifier;
                docReader.Functionality.ShowCloseButton = false;

            }
            else
            {
                btnImage.Enabled = false;
                bntCamera.Enabled = false;

                var initError = "Initialization error: " + (error == null ? "Unknown error" : error);
                Console.WriteLine(initError);
            }
        }

        partial void UseGaleryButtonTouch(UIButton sender)
        {
            //docReader.ShowScanner(this, ShowScannerCompleted);
            PHPhotoLibrary.RequestAuthorization((status) => {
                switch (status)
                {
                    case PHAuthorizationStatus.Authorized:
                        DispatchQueue.MainQueue.DispatchAsync(() => {
                            if (UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.SavedPhotosAlbum))
                            {
                                var imagePicker = new UIImagePickerController();
                                imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
                                imagePicker.AllowsEditing = false;
                                imagePicker.FinishedPickingMedia += OnFinishedPickingMedia;
                                imagePicker.Canceled += (_sender, _evt) =>
                                {
                                    Console.WriteLine("picker cancelled");
                                    imagePicker.DismissModalViewController(true);
                                };
                                imagePicker.NavigationBar.TintColor = UIColor.Black;
                                PresentViewController(imagePicker, true, null);
                            }
                        });
                        Console.WriteLine("PHPhotoLibrary status: authorized");
                        break;
                    case PHAuthorizationStatus.Denied:
                        DispatchQueue.MainQueue.DispatchAsync(() => {
                            var alertController = UIAlertController.Create("Gallery Unavailable", "Application doesn't have permission to use the camera, please change privacy settings", UIAlertControllerStyle.Alert);
                            alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, null));
                            alertController.AddAction(UIAlertAction.Create("Settings", UIAlertActionStyle.Default, (obj) => {
                                var settingsURL = new NSUrl(UIApplication.OpenSettingsUrlString);
                                UIApplication.SharedApplication.OpenUrl(settingsURL);
                            }));
                            PresentViewController(alertController, true, null);
                        });
                        Console.WriteLine("PHPhotoLibrary status: denied");
                        break;
                    case PHAuthorizationStatus.NotDetermined:
                        Console.WriteLine("PHPhotoLibrary status: notDetermined");
                        break;
                    case PHAuthorizationStatus.Restricted:
                        Console.WriteLine("PHPhotoLibrary status: restricted");
                        break;
                }
            });
            Console.WriteLine("galery button touched using the action method");
        }

        // Use this code for recognize on photo from camera
        partial void UseCameraButtonTouch(UIButton sender)
        {
            //start recognize
            docReader.ShowScanner(this, ShowScannerCompleted);
            Console.WriteLine("camera button touched using the action method");
        }

        void ShowScannerCompleted(DocReaderAction action, DocumentReaderResults result, NSString error)
        {
            switch (action)
            {
                case DocReaderAction.Cancel:
                    Console.WriteLine("Cancelled by user");
                    break;
                case DocReaderAction.Complete:
                    Console.WriteLine("Completed");
                    var name = result.GetTextFieldValueByTypeWithFieldType(FieldType.Surname_And_Given_Names);
                    Console.WriteLine("Name: " + name);
                    nameLbl.Text = name;
                    documentImage.Image = result.GetGraphicFieldImageByTypeWithFieldType(GraphicFieldType.DocumentFront, ResultType.RawImage);
                    portraitImageView.Image = result.GetGraphicFieldImageByTypeWithFieldType(GraphicFieldType.Portrait);

                    // through all available text fields
                    foreach (var textField in result.TextResult.Fields)
                    {
                        var value = result.GetTextFieldValueByTypeWithFieldType(textField.FieldType, textField.Lcid);
                        if (value != null)
                            Console.WriteLine("Field type name: {0}, value: {1}", textField.FieldName, value);
                    }
                    break;
                case DocReaderAction.Error:
                    Console.WriteLine("Error");
                    Console.WriteLine("Error string: " + error);
                    break;
                case DocReaderAction.Process:
                    Console.WriteLine("Scaning not finished. Result: " + result);
                    break;
            }
        }

        void OnFinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs e)
        {
            var image = e.Info[UIImagePickerController.OriginalImage] as UIImage;
            if (image != null)
            {
                Console.WriteLine("got the original image");

                docReader.RecognizeImage(image, false, ShowScannerCompleted);
            }
            else
            {
                Console.WriteLine("Something went wrong");
            }

            (sender as UIImagePickerController).DismissModalViewController(true);
        }
    }
}

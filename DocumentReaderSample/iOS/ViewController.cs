using System;

using UIKit;
using DocReaderApi.iOS;
using Foundation;
using Photos;
using CoreFoundation;
using DocumentReaderSample.iOS;

namespace DocumentReaderSingleSample.iOS
{
    public partial class ViewController : UIViewController
    {
        protected ViewController(IntPtr handle) : base(handle) =>
        //WARNING!!!!
        //Initialization DocumentReader from DocReaderCore is required
        new DocReaderCore.iOS.DocumentReader();

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
            var processParams = new RGLProcessParams
            {
                Scenario = "Ocr"
            };
            //create DocReader object with default ProcessParams
            RGLDocReader.Shared.PrepareDatabase("Full", (NSProgress obj) => {
                Console.WriteLine(obj.FractionCompleted);
            },(arg1, arg2) => {
                RGLDocReader.Shared.InitializeReader(licenseData, (bool successfull, string error) => {
                    initLabel.Hidden = true;
                    initIndocator.Hidden = true;
                    scenariosView.Hidden = false;
                    if (successfull)
                    {

                        btnImage.Enabled = true;
                        bntCamera.Enabled = true;
                        rfidSwitch.Enabled = true;
                        rfidLabel.Enabled = true;

                        var picker = new ScenarioPickerModel(RGLDocReader.Shared.AvailableScenarios);
                        scenariosView.Model = picker;
                        picker.ValueChanged += (object sender, EventArgs e) =>
                        {
                            RGLDocReader.Shared.ProcessParams.Scenario = picker.SelectedValue;
                        };


                        //Get available scenarios
                        foreach (var scenarion in RGLDocReader.Shared.AvailableScenarios)
                        {
                            Console.WriteLine(scenarion);
                        }

                        RGLDocReader.Shared.ProcessParams.Scenario = RGLDocReader.Shared.AvailableScenarios[0].Identifier;
                        RGLDocReader.Shared.Functionality.ShowCloseButton = true;

                    }
                    else
                    {
                        btnImage.Enabled = false;
                        bntCamera.Enabled = false;

                        var initError = "Initialization error: " + (error == null ? "Unknown error" : error);
                        Console.WriteLine(initError);
                    }
                });
            });
        }

        partial void UseGaleryButtonTouch(UIButton sender)
        {
            //docReader.ShowScanner(this, ShowScannerCompleted);
            PHPhotoLibrary.RequestAuthorization((status) => {
                switch(status)
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
            RGLDocReader.Shared.ShowScanner(this, HandleRGLDocumentReaderCompletion);
            Console.WriteLine("camera button touched using the action method");
        }

        void OnFinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs e)
        {
            var image = e.Info[UIImagePickerController.OriginalImage] as UIImage;
            if (image != null)
            {
                Console.WriteLine("got the original image");

                RGLDocReader.Shared.RecognizeImage(image, false, HandleRGLDocumentReaderCompletion);
            } else {
                Console.WriteLine("Something went wrong");
            }

            (sender as UIImagePickerController).DismissModalViewController(true);
        }

        void HandleRGLDocumentReaderCompletion(RGLDocReaderAction action, RGLDocumentReaderResults result, string error)
        {
            switch (action)
            {
                case RGLDocReaderAction.Cancel:
                    Console.WriteLine("Cancelled by user");
                    break;
                case RGLDocReaderAction.Complete:
                    if (rfidSwitch.On)
                    {
                        RGLDocReader.Shared.StartRFIDReaderFromPresenter(this, HandleGLDocumentReaderRfidCompletion);
                    } else
                    {
                        showResults(result);
                    }
                    break;
                case RGLDocReaderAction.Error:
                    Console.WriteLine("Error");
                    Console.WriteLine("Error string: " + error);
                    break;
                case RGLDocReaderAction.Process:
                    Console.WriteLine("Scaning not finished. Result: " + result);
                    break;
            }
        }

        void HandleGLDocumentReaderRfidCompletion(RGLDocReaderAction action, RGLDocumentReaderResults result, string error)
        {
            switch (action)
            {
                case RGLDocReaderAction.Cancel:
                case RGLDocReaderAction.Error:
                case RGLDocReaderAction.Complete:
                    showResults(result);
                    break;
            }
        }

        void showResults(RGLDocumentReaderResults result)
        {
            Console.WriteLine("Completed");

            if (result == null)
                return;
            
            var name = result.GetTextFieldValueByType(RGLFieldType.Surname_And_Given_Names);
            Console.WriteLine("Name: " + name);
            nameLbl.Text = name;
            documentImage.Image = result.GetGraphicFieldImageByType(RGLGraphicFieldType.DocumentImage, RGLResultType.RawImage);
            var rfidImage = result.GetGraphicFieldImageByType(RGLGraphicFieldType.Portrait, RGLResultType.RfidImageData);
            if (rfidImage == null)
            {
                portraitImageView.Image = result.GetGraphicFieldImageByType(RGLGraphicFieldType.Portrait, RGLResultType.Graphics);
            }
            else
            {
                portraitImageView.Image = rfidImage;
            }

            // through all available text fields
            foreach (var textField in result.TextResult.Fields)
            {
                var value = result.GetTextFieldValueByType(textField.FieldType, textField.Lcid);
                if (value != null)
                    Console.WriteLine("Field type name: {0}, value: {1}", textField.FieldName, value);
            }
        }

    }
}

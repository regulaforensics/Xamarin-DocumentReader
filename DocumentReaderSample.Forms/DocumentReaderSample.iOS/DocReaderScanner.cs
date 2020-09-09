using System;
using System.IO;
using DocReaderApi.iOS;
using DocumentReaderSample.Droid;
using Foundation;
using UIKit;

[assembly: Xamarin.Forms.Dependency(
          typeof(DocReaderScanner))]
namespace DocumentReaderSample.Droid
{
    public class DocReaderScannerEvent : EventArgs, IDocReaderScannerEvent
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public string SurnameAndGivenNames { get; set; }
        public byte[] PortraitField { get; set; }
        public byte[] DocumentField { get; set; }
    }

    public class DocReaderScanner: IDocReaderScanner
    {
        public DocReaderScanner()
        {
        }

        public event EventHandler<IDocReaderScannerEvent> ResultsObtained;

        private bool IsReadRfid = false;

        private UIViewController CurrentPresenter
        {
            get
            {
                var window = UIApplication.SharedApplication.KeyWindow;
                var vc = window.RootViewController;
                while (vc.PresentedViewController != null)
                {
                    vc = vc.PresentedViewController;
                }
                return vc;
            }
        }

        public void ShowScanner(bool IsReadRfid)
        {
            this.IsReadRfid = IsReadRfid;

            RGLDocReader.Shared.ShowScanner(CurrentPresenter, OnResultsObtained);
        }

        public void SelectScenario(string scenarioName)
        {
            RGLDocReader.Shared.ProcessParams.Scenario = scenarioName;
        }

        protected byte[] ConvertImage(UIImage image)
        {
            if (image == null)
                return null;

            using (NSData imageData = image.AsPNG())
            {
                Byte[] myByteArray = new Byte[imageData.Length];
                System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, myByteArray, 0, Convert.ToInt32(imageData.Length));
                return myByteArray;
            }
        }

        private void ReadRfid()
        {
            IsReadRfid = false;
            RGLDocReader.Shared.RfidScenario.AutoSettings = true;
            RGLDocReader.Shared.StartRFIDReaderFromPresenter(CurrentPresenter, OnResultsObtained);
        }

        public void RecognizeImage(Stream stream)
        {
            var imageData = NSData.FromStream(stream);
            var image = UIImage.LoadFromData(imageData);

            RGLDocReader.Shared.RecognizeImage(image, OnResultsObtained);
        }

        private void OnResultsObtained(RGLDocReaderAction action, RGLDocumentReaderResults result, NSError error)
        {
            DocReaderScannerEvent readerScannerEvent = null;
            if (action == RGLDocReaderAction.Complete)
            {
                if (result == null)
                {
                    readerScannerEvent = new DocReaderScannerEvent() { IsSuccess = false, Error = "Document Reader results is empty" };
                }
                else
                {
                    if(IsReadRfid)
                        ReadRfid();
                    else
                        readerScannerEvent = GenerateDocReaderScannerEvent(result);
                }
            }

            if (readerScannerEvent != null)
            {
                ResultsObtained(this, readerScannerEvent);
            }
        }

        private DocReaderScannerEvent GenerateDocReaderScannerEvent(RGLDocumentReaderResults result)
        {
            DocReaderScannerEvent readerScannerEvent = new DocReaderScannerEvent() { IsSuccess = true };
            var name = result.GetTextFieldValueByType(RGLFieldType.Surname_And_Given_Names);

            if (!System.String.IsNullOrEmpty(name))
            {
                readerScannerEvent.SurnameAndGivenNames = name;
            }

            // through all available text fields
            foreach (var textField in result.TextResult.Fields)
            {
                var value = result.GetTextFieldValueByType(textField.FieldType, textField.Lcid);
                if (value != null)
                    Console.WriteLine("Field type name: {0}, value: {1}", textField.FieldName, value);
            }

            using (var portraitImage = result.GetGraphicFieldImageByType(RGLGraphicFieldType.Portrait))
            {
                if (portraitImage != null)
                    readerScannerEvent.PortraitField = ConvertImage(portraitImage);
            }

            using (var documentImage = result.GetGraphicFieldImageByType(RGLGraphicFieldType.DocumentImage, RGLResultType.RawImage))
            {
                if (documentImage != null)
                    readerScannerEvent.DocumentField = ConvertImage(documentImage);
            }

            return readerScannerEvent;
        }
    }
}

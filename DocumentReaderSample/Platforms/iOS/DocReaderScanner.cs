using DocReaderApi.iOS;
using Foundation;
using UIKit;

namespace DocumentReaderSample.Platforms.iOS
{
    public class DocReaderScannerEvent : EventArgs, IDocReaderScannerEvent
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public string SurnameAndGivenNames { get; set; }
        public byte[] PortraitField { get; set; }
        public byte[] DocumentField { get; set; }
    }

    public class DocReaderScanner : IDocReaderScanner
    {
        public DocReaderScanner()
        {
        }

        public event EventHandler<IDocReaderScannerEvent> ResultsObtained;

        private bool IsReadRfid = false;

        private string selectedScenario = "Mrz";

        private static UIViewController CurrentPresenter
        {
            get
            {
                #pragma warning disable CA1422
                var window = UIApplication.SharedApplication.KeyWindow;
                #pragma warning restore CA1422
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
            RGLScannerConfig config = new()
            {
                Scenario = selectedScenario
            };
            RGLDocReader.Shared.ShowScannerFromPresenter(CurrentPresenter, config, OnResultsObtained);
        }

        public void SelectScenario(string scenarioName)
        {
            selectedScenario = scenarioName;
        }

        protected static byte[] ConvertImage(UIImage image)
        {
            if (image == null)
                return null;

            using NSData imageData = image.AsPNG();
            byte[] myByteArray = new byte[imageData.Length];
            System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, myByteArray, 0, Convert.ToInt32(imageData.Length));
            return myByteArray;
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
            RGLRecognizeConfig config = new(image)
            {
                Scenario = selectedScenario
            };
            RGLDocReader.Shared.RecognizeWithConfig(config, OnResultsObtained);
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
                    if (IsReadRfid)
                        ReadRfid();
                    else
                        readerScannerEvent = GenerateDocReaderScannerEvent(result);
                }
            }
            else if (action == RGLDocReaderAction.ProcessTimeout)
            {
                if (result == null)
                {
                    readerScannerEvent = new DocReaderScannerEvent() { IsSuccess = false, Error = "Document Reader results is empty" };
                }
                else
                {
                    readerScannerEvent = GenerateDocReaderScannerEvent(result);
                }
            }

            if (readerScannerEvent != null)
            {
                ResultsObtained(this, readerScannerEvent);
            }
        }

        private static DocReaderScannerEvent GenerateDocReaderScannerEvent(RGLDocumentReaderResults result)
        {
            DocReaderScannerEvent readerScannerEvent = new() { IsSuccess = true };
            var name = result.GetTextFieldValueByType(RGLFieldType.Surname_And_Given_Names);

            if (!string.IsNullOrEmpty(name))
            {
                readerScannerEvent.SurnameAndGivenNames = name;
            }

            // through all available text fields
            if(result.TextResult != null)
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
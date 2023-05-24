using System;
using DocReaderApi.iOS;
using Foundation;
using UIKit;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

            NSString str = new NSString("{\"objects\": [\n  {\n    \"button\": {\n      \"title\": \"Click\",\n      \"tag\": 101,\n      \"fontStyle\": \"normal\",\n      \"fontColor\": \"#FF000000\",\n      \"background\": \"#FF00FF00\",\n      \"fontSize\": 16,\n      \"fontName\": \"Arial\",\n      \"alignment\": \"center\",\n      \"borderRadius\": 20,\n      \"margin\": {\n        \"end\": 24\n      },\n      \"position\": {\n        \"v\": 0.5\n      },\n      \"image\": {\n        \"name\": \"id\",\n        \"placement\": \"bottom\",\n        \"padding\": 0\n      }\n    }\n  },\n  {\n    \"button\": {\n      \"title\": \"Close 2\",\n      \"tag\": 102,\n      \"backLayer\": 1,\n      \"textSize\": 8,\n      \"style\": \"PrimaryButton\",\n      \"fontColor\": \"#FF00FF00\",\n      \"alignment\": \"center\",\n      \"margin\": {\n        \"end\": 10\n      },\n      \"padding\": {\n        \"start\": 2,\n        \"end\": 2,\n        \"top\": 2,\n        \"bottom\": 2\n      },\n      \"position\": {\n        \"v\": 1.5\n      }\n    }\n  },\n  {\n    \"button\": {\n      \"title\": \"Button 1\",\n      \"tag\": 103,\n      \"textSize\": 1,\n      \"alignment\": \"end\",\n      \"margin\": {\n        \"top\": 20,\n        \"start\": 20\n      },\n      \"image\": {\n        \"data\": \"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\",\n        \"padding\": 5,\n        \"placement\": \"top\"\n      }\n    }\n  }\n]\n}");
            NSError error;
            object obj = NSJsonSerialization.Deserialize(NSData.FromString(str), 0, out error);
            NSDictionary dict = (NSDictionary)NSDictionary.FromObject(obj);
            RGLDocReader.Shared.Customization.CustomUILayerJSON = dict;
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

        private DocReaderScannerEvent GenerateDocReaderScannerEvent(RGLDocumentReaderResults result)
        {
            DocReaderScannerEvent readerScannerEvent = new DocReaderScannerEvent() { IsSuccess = true };
            var name = result.GetTextFieldValueByType(RGLFieldType.Surname_And_Given_Names);

            if (!System.String.IsNullOrEmpty(name))
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
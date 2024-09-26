using DocReaderApi.iOS;
using Foundation;
using UIKit;
#pragma warning disable CA1422

namespace DocumentReaderSample.Platforms.iOS
{
    public class DocReaderScannerEvent : EventArgs, IDocReaderScannerEvent
    {
        public string SurnameAndGivenNames { get; set; }
        public byte[] PortraitField { get; set; }
        public byte[] DocumentField { get; set; }
    }
    public class DocReaderScanner : IDocReaderScanner
    {
        public event EventHandler<IDocReaderScannerEvent> ResultsObtained;
        private bool IsReadRfid = false;
        private string selectedScenario = "Mrz";
        public void ShowScanner(bool IsReadRfid)
        {
            this.IsReadRfid = IsReadRfid;
            RGLScannerConfig config = new(selectedScenario);
            RGLDocReader.Shared.ShowScannerFromPresenter(UIApplication.SharedApplication.KeyWindow.RootViewController, config, OnResultsObtained);
        }
        public void RecognizeImage(Stream stream, bool IsReadRfid)
        {
            this.IsReadRfid = IsReadRfid;
            var imageData = NSData.FromStream(stream);
            var image = UIImage.LoadFromData(imageData);
            RGLRecognizeConfig config = new(image) { Scenario = selectedScenario };
            RGLDocReader.Shared.RecognizeWithConfig(config, OnResultsObtained);
        }
        private void OnResultsObtained(RGLDocReaderAction action, RGLDocumentReaderResults result, NSError error)
        {
            if (action != RGLDocReaderAction.Complete && action != RGLDocReaderAction.ProcessTimeout) return;
            if (IsReadRfid && result != null && result.ChipPage != 0)
            {
                RGLDocReader.Shared.StartRFIDReaderFromPresenter(UIApplication.SharedApplication.KeyWindow.RootViewController, OnResultsObtained);
                IsReadRfid = false;
                return;
            }

            var portrait = result.GetGraphicFieldImageByType(RGLGraphicFieldType.Portrait);
            var rfidPortrait = result.GetGraphicFieldImageByType(RGLGraphicFieldType.Portrait, RGLResultType.RfidImageData);
            if(rfidPortrait != null) portrait = rfidPortrait;
            ResultsObtained(this, new DocReaderScannerEvent
            {
                SurnameAndGivenNames = result.GetTextFieldValueByType(RGLFieldType.Surname_And_Given_Names),
                PortraitField = ConvertImage(portrait),
                DocumentField = ConvertImage(result.GetGraphicFieldImageByType(RGLGraphicFieldType.DocumentImage))
            });
        }
        protected static byte[] ConvertImage(UIImage image)
        {
            if (image == null) return null;
            using NSData imageData = image.AsPNG();
            byte[] myByteArray = new byte[imageData.Length];
            System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, myByteArray, 0, Convert.ToInt32(imageData.Length));
            return myByteArray;
        }
        public void SelectScenario(string scenarioName) { selectedScenario = scenarioName; }
    }
}
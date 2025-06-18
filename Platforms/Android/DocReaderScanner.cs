using Android.Graphics;
using Com.Regula.Documentreader.Api;
using Com.Regula.Documentreader.Api.Config;
using Com.Regula.Documentreader.Api.Completions;
using Com.Regula.Documentreader.Api.Completions.Rfid;
using Com.Regula.Documentreader.Api.Enums;
using Com.Regula.Documentreader.Api.Errors;
using Com.Regula.Documentreader.Api.Results;

namespace DocumentReaderSample.Platforms.Android
{
    public class DocReaderScannerEvent : EventArgs, IDocReaderScannerEvent
    {
        public string SurnameAndGivenNames { get; set; }
        public byte[] PortraitField { get; set; }
        public byte[] DocumentField { get; set; }
    }
    public class DocReaderScanner : Java.Lang.Object, IDocReaderScanner, IDocumentReaderCompletion
    {
        public event EventHandler<IDocReaderScannerEvent> ResultsObtained;
        private bool IsReadRfid = false;
        private string selectedScenario = "Mrz";
        public void ShowScanner(bool IsReadRfid)
        {
            ScannerConfig config = new ScannerConfig.Builder(selectedScenario).Build();
            DocumentReader.Instance().StartScanner(Platform.AppContext, config, this);
            this.IsReadRfid = IsReadRfid;
        }
        public void OnCompleted(int action, DocumentReaderResults results, DocumentReaderException error)
        {
            if (action != DocReaderAction.Complete && action != DocReaderAction.Timeout) return;
            if (IsReadRfid && results != null && results.ChipPage != 0)
            {
                DocumentReader.Instance().StartRFIDReader(Platform.AppContext, new RfidCallback(this));
                IsReadRfid = false;
                return;
            }

            var portrait = results.GetGraphicFieldImageByType(EGraphicFieldType.GfPortrait);
            var rfidPortrait = results.GetGraphicFieldImageByType(EGraphicFieldType.GfPortrait, ERPRM_ResultType.RfidResultTypeRfidImageData);
            if (rfidPortrait != null) portrait = rfidPortrait;
            ResultsObtained(this, new DocReaderScannerEvent
            {
                SurnameAndGivenNames = results.GetTextFieldValueByType(EVisualFieldType.FtSurnameAndGivenNames),
                PortraitField = ConvertBitmap(portrait),
                DocumentField = ConvertBitmap(results.GetGraphicFieldImageByType(EGraphicFieldType.GfDocumentImage))
            });
        }
        public void SelectScenario(string scenarioName) { selectedScenario = scenarioName; }
        private static Bitmap CompressBitmap(Bitmap bitmap)
        {
            int sizeMultiplier = bitmap.ByteCount / 5000000;
            if (bitmap.ByteCount > 5000000) bitmap = Bitmap.CreateScaledBitmap(bitmap, bitmap.Width / (int)Math.Sqrt(sizeMultiplier), bitmap.Height / (int)Math.Sqrt(sizeMultiplier), false);
            return bitmap;
        }
        private static byte[] ConvertBitmap(Bitmap bitmap)
        {
            if (bitmap == null) return null;
            bitmap = CompressBitmap(bitmap);
            byte[] bitmapData = null;
            using (var stream = new MemoryStream())
            {
                bitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
                bitmapData = stream.ToArray();
            }
            return bitmapData;
        }
        public void RecognizeImage(Stream stream, bool IsReadRfid)
        {
            Bitmap bitmap = BitmapFactory.DecodeStream(stream);
            RecognizeConfig config = new RecognizeConfig.Builder(selectedScenario).SetBitmap(bitmap).Build();
            DocumentReader.Instance().Recognize(Platform.AppContext, config, this);
            this.IsReadRfid = IsReadRfid;
        }
    }
    public class RfidCallback(DocReaderScanner scanner) : IRfidReaderCompletion
    {
        public DocReaderScanner instance = scanner;
        public override void OnCompleted(int action, DocumentReaderResults results, DocumentReaderException error)
        {
            instance.OnCompleted(action, results, error);
        }
    }
}
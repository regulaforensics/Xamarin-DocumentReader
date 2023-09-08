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
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public string SurnameAndGivenNames { get; set; }
        public byte[] PortraitField { get; set; }
        public byte[] DocumentField { get; set; }
    }

    public class DocReaderScanner : Java.Lang.Object, IDocReaderScanner, IDocumentReaderCompletion
    {
        public DocReaderScanner()
        {
        }

        public event EventHandler<IDocReaderScannerEvent> ResultsObtained;

        private bool IsReadRfid = false;

        private string selectedScenario = "Mrz";

        public void ShowScanner(bool IsReadRfid)
        {
            ScannerConfig config = new ScannerConfig.Builder(selectedScenario).Build();
            DocumentReader.Instance().ShowScanner(Platform.AppContext, config, this);
            this.IsReadRfid = IsReadRfid;
        }

        public void OnCompleted(int action, DocumentReaderResults results, DocumentReaderException error)
        {
            DocReaderScannerEvent readerScannerEvent = null;
            if (action == DocReaderAction.Complete)
            {
                if (results == null)
                {
                    readerScannerEvent = new DocReaderScannerEvent() { IsSuccess = false, Error = "Document Reader results is empty" };
                }
                else
                {
                    if (IsReadRfid)
                        ReadRfid();
                    else
                        readerScannerEvent = GenerateDocReaderScannerEvent(results);
                }
            }
            else if (action == DocReaderAction.Timeout)
            {
                if (results == null)
                {
                    readerScannerEvent = new DocReaderScannerEvent() { IsSuccess = false, Error = "Document Reader results is empty" };
                }
                else
                {
                    readerScannerEvent = GenerateDocReaderScannerEvent(results);
                }
            }
            else if (action == DocReaderAction.Error)
            {
                readerScannerEvent = new DocReaderScannerEvent() { IsSuccess = false, Error = error.Message };
            }
            else if (action == DocReaderAction.Cancel)
            {
                readerScannerEvent = new DocReaderScannerEvent() { IsSuccess = false, Error = "Cancelled by user" };
            }

            if (readerScannerEvent != null)
            {
                ResultsObtained(this, readerScannerEvent);
            }
        }

        public void SelectScenario(string scenarioName)
        {
            selectedScenario = scenarioName;
        }

        private static Bitmap CompressBitmap(Bitmap bitmap)
        {
            int sizeMultiplier = bitmap.ByteCount / 5000000;
            if (bitmap.ByteCount > 5000000)
                bitmap = Bitmap.CreateScaledBitmap(bitmap, bitmap.Width / (int)Math.Sqrt(sizeMultiplier), bitmap.Height / (int)Math.Sqrt(sizeMultiplier), false);
            return bitmap;
        }

        private static byte[] ConvertBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                return null;
            bitmap = CompressBitmap(bitmap);

            byte[] bitmapData = null;
            using (var stream = new MemoryStream())
            {
                bitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
                bitmapData = stream.ToArray();
            }
            return bitmapData;
        }

        private static DocReaderScannerEvent GenerateDocReaderScannerEvent(DocumentReaderResults results)
        {
            DocReaderScannerEvent readerScannerEvent = new() { IsSuccess = true };
            var name = results.GetTextFieldValueByType(EVisualFieldType.FtSurnameAndGivenNames);
            if (!string.IsNullOrEmpty(name))
            {
                readerScannerEvent.SurnameAndGivenNames = name;
            }

            // through all text fields
            if (results.TextResult != null && results.TextResult.Fields != null)
            {
                foreach (DocumentReaderTextField textField in results.TextResult.Fields)
                {
                    var value = results.GetTextFieldValueByType(textField.FieldType, textField.Lcid);
                    Console.WriteLine(value);
                }
            }

            using var portraitImage = results.GetGraphicFieldImageByType(EGraphicFieldType.GfPortrait);
            readerScannerEvent.PortraitField = ConvertBitmap(portraitImage);

            using var documentImage = results.GetGraphicFieldImageByType(EGraphicFieldType.GfDocumentImage);
            readerScannerEvent.DocumentField = ConvertBitmap(documentImage);

            return readerScannerEvent;
        }

        private void ReadRfid()
        {
            IsReadRfid = false;
            DocumentReader.Instance().RfidScenario().AutoSettings = true;
            DocumentReader.Instance().StartRFIDReader(Platform.AppContext, new RfidCallback(this));
        }

        public void RecognizeImage(Stream stream)
        {
            Bitmap bitmap = BitmapFactory.DecodeStream(stream);
            RecognizeConfig config = new RecognizeConfig.Builder(selectedScenario).SetBitmap(bitmap).Build();
            DocumentReader.Instance().Recognize(Platform.AppContext, config, this);
        }
    }

    public class RfidCallback : IRfidReaderCompletion
    {
        public DocReaderScanner instance;

        public RfidCallback(DocReaderScanner scanner)
        {
            instance = scanner;
        }

        public override void OnCompleted(int action, DocumentReaderResults results, DocumentReaderException error)
        {
            instance.OnCompleted(action, results, error);
        }
    }
}
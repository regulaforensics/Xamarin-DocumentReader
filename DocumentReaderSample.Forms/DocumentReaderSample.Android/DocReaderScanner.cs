using System;
using System.IO;
using Android.Graphics;
using Android.Runtime;
using Com.Regula.Documentreader.Api;
using Com.Regula.Documentreader.Api.Completions;
using Com.Regula.Documentreader.Api.Enums;
using Com.Regula.Documentreader.Api.Errors;
using Com.Regula.Documentreader.Api.Results;
using DocumentReaderSample.Droid;
using Java.Lang;
using Xamarin.Forms;

[assembly: Dependency(
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

    public class DocReaderScanner: Java.Lang.Object, IDocReaderScanner, IDocumentReaderCompletion
    {
        public DocReaderScanner()
        {
        }

        public event EventHandler<IDocReaderScannerEvent> ResultsObtained;

        private bool IsReadRfid = false;

        [Obsolete]
        public void ShowScanner(bool IsReadRfid)
        {
            DocumentReader.Instance().ShowScanner(Forms.Context, this);
            this.IsReadRfid = IsReadRfid;
        }

        public void OnCompleted([IntDef(Type = "Com.Regula.Documentreader.Api.Enums.DocReaderAction", Fields = new[] { "Complete", "Process", "Cancel", "Error", "Notification", "ProcessWhiteUvImages", "MorePagesAvailable" })] int action, DocumentReaderResults results, DocumentReaderException error)
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
            DocumentReader.Instance().ProcessParams().Scenario = scenarioName;
        }

        private byte[] ConvertBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                return null;

            byte[] bitmapData = null;
            using (var stream = new MemoryStream())
            {
                bitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
                bitmapData = stream.ToArray();
            }
            return bitmapData;
        }

        private DocReaderScannerEvent GenerateDocReaderScannerEvent(DocumentReaderResults results)
        {
            DocReaderScannerEvent readerScannerEvent = new DocReaderScannerEvent() { IsSuccess = true };
            var name = results.GetTextFieldValueByType(EVisualFieldType.FtSurnameAndGivenNames);
            if (!System.String.IsNullOrEmpty(name))
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
            DocumentReader.Instance().StartRFIDReader(Android.App.Application.Context, this);
        }

        public void RecognizeImage(Stream stream)
        {
            Bitmap bitmap = BitmapFactory.DecodeStream(stream);
            DocumentReader.Instance().RecognizeImage(bitmap, this);
        }
    }
}

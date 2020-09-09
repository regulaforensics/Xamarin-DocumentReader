using System;
using System.IO;
using Xamarin.Forms;

namespace DocumentReaderSample
{
    public interface IDocReaderScanner
    {
        void ShowScanner(bool IsReadRfid);
        void RecognizeImage(Stream stream);
        void SelectScenario(string scenarioName);
        event EventHandler<IDocReaderScannerEvent>
            ResultsObtained;
    }

    public interface IDocReaderScannerEvent
    {
        bool IsSuccess { get; set; }
        string Error { get; set; }
        string SurnameAndGivenNames { get; set; }
        byte[] PortraitField { get; set; }
        byte[] DocumentField { get; set; }
    }
}

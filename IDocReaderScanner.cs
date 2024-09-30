namespace DocumentReaderSample
{
    public interface IDocReaderScanner
    {
        void ShowScanner(bool IsReadRfid);
        void RecognizeImage(Stream stream, bool IsReadRfid);
        void SelectScenario(string scenarioName);
        event EventHandler<IDocReaderScannerEvent> ResultsObtained;
    }
    public interface IDocReaderScannerEvent
    {
        string SurnameAndGivenNames { get; set; }
        byte[] PortraitField { get; set; }
        byte[] DocumentField { get; set; }
    }
}


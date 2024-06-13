using DocReaderApi.iOS;
using Foundation;

namespace DocumentReaderSample.Platforms.iOS
{
    public class DocReaderInitEvent : EventArgs, IDocReaderInitEvent
    {
        public IList<Scenario> Scenarios { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsRfidAvailable { get; set; }
    }
    public class DocReaderInit : IDocReaderInit
    {
        public event EventHandler<IDocReaderInitEvent> ScenariosObtained;
        public void InitDocReader()
        {
            var licenseData = NSData.FromFile(NSBundle.MainBundle.PathForResource("regula.license", null));
            RGLConfig config = new(licenseData) { DelayedNNLoadEnabled = true };
            RGLDocReader.Shared.InitializeReaderWithConfig(config, (bool success, NSError error) =>
            {
                DocReaderInitEvent readerInitEvent = new() { IsSuccess = success };
                if (!success)
                {
                    Console.WriteLine("Init failed:" + error);
                    ScenariosObtained(this, readerInitEvent);
                    return;
                }
                IList<Scenario> data = [];
                foreach (RGLScenario scenario in RGLDocReader.Shared.AvailableScenarios)
                    data.Add(new Scenario() { Name = scenario.Identifier, Description = scenario.Description });
                readerInitEvent.Scenarios = data;
                readerInitEvent.IsRfidAvailable = RGLDocReader.Shared.RfidAvailable;
                ScenariosObtained(this, readerInitEvent);
            });
        }
        void IDocReaderInit.CheckPermissionsAndConnect(string btDeviceName) { }
    }
}
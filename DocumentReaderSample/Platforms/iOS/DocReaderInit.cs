using DocReaderApi.iOS;
using Foundation;

namespace DocumentReaderSample.Platforms.iOS
{
    public class DocReaderInitEvent : EventArgs, IDocReaderInitEvent
    {
        public IList<Scenario> Scenarios { get; set; }

        public bool IsSuccess { get; set; }

        public bool IsRfidAvailable { get; set; }

        public bool dbPrepared { get; set; }

        public int dbProgress { get; set; }
    }

    public class DocReaderInit : IDocReaderInit
    {
        public DocReaderInit()
        {
        }

        public event EventHandler<IDocReaderInitEvent> ScenariosObtained;

        public void InitDocReader(bool btDevice = false)
        {
            RGLDocReader.Shared.PrepareDatabase("Full", delegate (NSProgress progress)
            {
                DocReaderInitEvent readerInitEvent = new() { dbProgress = (int)(progress.FractionCompleted * 100) };
                ScenariosObtained(this, readerInitEvent);
            }, delegate (bool status, NSError error)
            {
                DocReaderInitEvent readerInitEvent = new() { dbPrepared = status };
                if (status)
                    InitReader();
                else
                    readerInitEvent.dbProgress = -1;

                ScenariosObtained(this, readerInitEvent);
            });
        }

        protected void InitReader()
        {
            var licenseData = NSData.FromFile(NSBundle.MainBundle.PathForResource("regula.license", null));
            if (licenseData == null)
            {
                return;
            }

            RGLConfig config = new(licenseData)
            {
                DelayedNNLoadEnabled = true
            };
            RGLDocReader.Shared.InitializeReaderWithConfig(config, (bool success, NSError error) =>
            {
                DocReaderInitEvent readerInitEvent = new() { IsSuccess = success };
                if (success)
                {
                    Console.WriteLine("Initialized sucessfully");


                    IList<Scenario> data = new List<Scenario>();
                    foreach (RGLScenario scenario in RGLDocReader.Shared.AvailableScenarios)
                    {
                        var scenarion = new Scenario() { Name = scenario.Identifier, Description = scenario.Description };
                        data.Add(scenarion);
                    }
                    readerInitEvent.Scenarios = data;
                    readerInitEvent.IsRfidAvailable = RGLDocReader.Shared.RfidAvailable;
                }
                else
                {
                    Console.WriteLine("Initialization failed:" + error);
                }

                ScenariosObtained(this, readerInitEvent);
            });
        }

        void IDocReaderInit.CheckPermissionsAndConnect(string btDeviceName)
        {
        }
    }
}
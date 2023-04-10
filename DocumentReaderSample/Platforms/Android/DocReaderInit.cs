using System;
using Com.Regula.Documentreader.Api;
using Com.Regula.Documentreader.Api.Completions;
using Com.Regula.Documentreader.Api.Errors;
using Com.Regula.Documentreader.Api.Params;
using Com.Regula.Documentreader.Api.Results;

namespace DocumentReaderSample.Platforms.Android
{
    public class DocReaderInitEvent : EventArgs, IDocReaderInitEvent
    {
        public IList<Scenario> Scenarios { get; set; }

        public bool IsSuccess { get; set; }

        public bool IsRfidAvailable { get; set; }
    }

    public class DocReaderInit : Java.Lang.Object, IDocReaderInit, IDocumentReaderInitCompletion
    {
        public DocReaderInit()
        {
        }

        public event EventHandler<IDocReaderInitEvent> ScenariosObtained;

        public void InitDocReader()
        {
            Console.WriteLine("Native Android");
            InitReader();
        }

        protected void InitReader()
        {
            var bytes = default(byte[]);
            using (var streamReader = new StreamReader(Platform.AppContext.Assets.Open("regula.license")))
            {
                using (var memstream = new MemoryStream())
                {
                    streamReader.BaseStream.CopyTo(memstream);
                    bytes = memstream.ToArray();
                }
            }

            DocReaderConfig config = new DocReaderConfig(bytes);
            config.DelayedNNLoad = true;
            DocumentReader.Instance().InitializeReader(Platform.AppContext, config, this);
        }

        //Document Reader Completions
        public void OnInitCompleted(bool success, DocumentReaderException error)
        {
            DocReaderInitEvent readerInitEvent = new DocReaderInitEvent() { IsSuccess = success };
            if (success)
            {
                Console.WriteLine("Initialized sucessfully");

                IList<Scenario> data = new List<Scenario>();
                foreach (DocumentReaderScenario scenario in DocumentReader.Instance().AvailableScenarios)
                {
                    var scenarion = new Scenario() { Name = scenario.Name, Description = scenario.Description };
                    data.Add(scenarion);
                }

                readerInitEvent.Scenarios = data;
                readerInitEvent.IsRfidAvailable = DocumentReader.Instance().IsRFIDAvailableForUse;
            }
            else
            {
                Console.WriteLine("Initialization failed:" + error);
            }

            ScenariosObtained(this, readerInitEvent);
        }
    }
}
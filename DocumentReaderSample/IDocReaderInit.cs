using System;
namespace DocumentReaderSample
{
    public interface IDocReaderInit
    {
        void InitDocReader(bool btDeviceSample = false);
        event EventHandler<IDocReaderInitEvent>
            ScenariosObtained;

        void CheckPermissionsAndConnect(string btDeviceName);
    }

    public interface IDocReaderInitEvent
    {
        bool IsSuccess { get; }
        IList<Scenario> Scenarios { get; }
        bool IsRfidAvailable { get; }
        bool dbPrepared { get; }
        int dbProgress { get; }
    }
}


using System;
namespace DocumentReaderSample
{
    public interface IDocReaderInit
    {
        void InitDocReader();
        event EventHandler<IDocReaderInitEvent> ScenariosObtained;
        void CheckPermissionsAndConnect(string btDeviceName);
    }

    public interface IDocReaderInitEvent
    {
        bool IsSuccess { get; }
        IList<Scenario> Scenarios { get; }
        bool IsRfidAvailable { get; }
    }
}


﻿using System;
using System.Collections.Generic;

namespace DocumentReaderSample
{
    public interface IDocReaderInit
    {
        void InitDocReader();
        event EventHandler<IDocReaderInitEvent>
            ScenariosObtained;
    }

    public interface IDocReaderInitEvent
    {
        bool IsSuccess { get; }
        IList<Scenario> Scenarios { get; }
        bool IsRfidAvailable { get; }
    }
}

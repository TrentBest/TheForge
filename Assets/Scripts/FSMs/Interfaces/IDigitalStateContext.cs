using System;
using System.Collections.Generic;
using System.Text;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.FSMs.Interfaces
{
    public interface IDigitalStateContext : IStateContext
    {
        bool ReadSignal(string channel);
        float ReadFloatValue(string channel);
        int ReadIntValue(string channel);
        void WriteSignal(string channel, bool value);
        void WriteFloatValue(string channel, float value);
        void WriteIntValue(string channel, int value);
    }
}

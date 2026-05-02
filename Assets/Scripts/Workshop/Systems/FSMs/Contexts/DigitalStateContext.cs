using System.Collections.Generic;
using Workshop.Systems.FSMs.Interfaces;

namespace Workshop.Systems.FSMs.Contexts
{
   public class DigitalStateContext : IDigitalStateContext
    {
        public bool IsValid { get; set; } = false;
        public string Name { get  ; set  ; }
        private Dictionary<string, bool> signals = new Dictionary<string, bool>();
        private Dictionary<string, float> floatValues = new Dictionary<string, float>();
        private Dictionary<string, int> intValues = new Dictionary<string, int>();

        public bool ReadSignal(string channel)
        {
            return signals[channel];
        }

        public void WriteSignal(string channel, bool value)
        {
            signals[channel] = value;
        }

        public void WriteFloatValue(string channel, float value)
        {
            floatValues[channel] = value;
        }


        public float ReadFloatValue(string channel)
        {
            return floatValues[channel];
        }

        public int ReadIntValue(string channel)
        {
            return intValues[channel];
        }

        public void WriteIntValue(string channel, int value)
        {
            intValues[channel] = value;
        }
    }
}

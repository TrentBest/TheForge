using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Armada2525.Economy
{
    // The ultimate anti-enum data structure for generative text
    public class LexiconCategory : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "New Linguistic Set";
        public string Description { get; set; } = "Usage context for these strings.";

        // The raw string data, separated by newlines for easy editing in the UI
        public string RawData { get; set; } = "";

        // Helper to instantly give the procedural generators an array
        public string[] GetEntries()
        {
            if (string.IsNullOrWhiteSpace(RawData)) return new string[] { "Unknown" };
            return RawData.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
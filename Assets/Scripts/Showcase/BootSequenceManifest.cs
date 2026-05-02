namespace Workshop.UI_And_Tools.Showcase
{

    using System;
    using System.Collections.Generic;

    namespace Workshop.UI_And_Tools.Showcase
    {
        [Serializable]
        public class BootSequenceManifest
        {
            public string SequenceID;
            public string WarningTitle;
            public string WarningMessage;
            public string ButtonText;

            // The H.E.R.M.I.T. Terminal Script
            public List<string> TerminalLines;

            // Timing settings (so we don't hardcode magic numbers)
            public float TypewriterSpeed;
            public float LineDelay;

            // Data for the next phases
            public int GlobeDotCount;
            public float CountdownDuration;
        }
    }
}
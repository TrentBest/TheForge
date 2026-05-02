using System;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Hermit.Bridge
{
    [Serializable]
    public class CortexPromptData
    {
        public string UserMessage;
        public string AvailableTools;
        public string SensedEnvironment;
        public Vector3 TargetPosition;
    }
}
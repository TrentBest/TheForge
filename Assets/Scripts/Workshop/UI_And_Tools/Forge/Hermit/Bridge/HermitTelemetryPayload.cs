using System;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace Workshop.UI_And_Tools.Forge.Hermit.Bridge
{
    [Serializable]
    public class HermitTelemetryPayload
    {
        public string AgentId;
        public string CurrentState;
        public Vector3 Position;
        public string[] SensedEntities;
        public string[] AvailableActions;
    }
}
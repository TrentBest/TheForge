#if UNITY_EDITOR
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class RotatorContext : IStateContext
    {
        public string Name { get; set; } = "Rotator";
        public bool IsValid { get; set; } = true;
        public FSMHandle Status { get; set; }

        public Transform Target { get; set; }
        public Vector3 Axis { get; set; }
        public float Speed { get; set; } = 1f;
    }
}
#endif
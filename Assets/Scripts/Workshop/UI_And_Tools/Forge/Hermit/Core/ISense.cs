using System.Collections.Generic;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Core
{
    // Defined here to ensure visibility across the Hermit namespace
    public struct PerceptionData
    {
        public string Name;
        public string Description;
        public float Distance;
        public float NeglectScore;
        public Vector3 Position;
        public GameObject TargetObject;
    }

    public interface ISense
    {
        Sense Type { get; }
        List<PerceptionData> Observe(HermitContext context);
    }
}
using TheSingularityWorkshop.Armada2525.DataModels;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace TheSingularityWorkshop.Armada2525.Builders
{
    public enum CosmicViewLevel { Universe, ZoomingToGalaxy, Galaxy, ZoomingToUniverse }

    public class UniverseForgeContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "UniverseForgeSim";

        public UniverseData Universe { get; set; } = new UniverseData();

        // Navigation State
        public CosmicViewLevel CurrentView { get; set; } = CosmicViewLevel.Universe;
        public GalaxyData ActiveGalaxy { get; set; }

        // Diorama Tracking
        public bool NeedsDioramaRebuild { get; set; } = true;
        public GameObject DioramaRoot { get; set; }
        public GameObject UniverseContainer { get; set; }
        public GameObject ActiveGalaxyContainer { get; set; }

        // Animation state for the graceful zoom
        public float ZoomProgress { get; set; } = 0f;
        public Vector3 ZoomStartPos { get; set; }
        public Vector3 ZoomTargetPos { get; set; }
    }
}
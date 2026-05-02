using TheSingularityWorkshop.FSM_API;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.WorldBuilding
{
    /// <summary>
    /// The Editor Workbench for the GUI. This holds heavy Unity objects (Prefabs, Markers) 
    /// so the underlying PlanetContext can remain lightweight for the live game.
    /// </summary>
    public class WorldBuilderContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "WorldBuilderSim";

        // The actual data model being edited
        public PlanetContext Data { get; set; }

        // --- DELEGATED PROPERTIES (The Fix) ---
        // These transparently pass data to and from the GPU-backed PlanetContext
        public string Seed { get => Data.Seed; set => Data.Seed = value; }
        public List<ContinentData> Continents { get => Data.Continents; set => Data.Continents = value; }
        public Mesh PlanetMesh { get => Data.PlanetMesh; set => Data.PlanetMesh = value; }

        // --- Editor/Workbench Only Variables ---
        public int SimulationSpeed { get; set; } = 1;
        public bool NeedsMeshRebuild { get; set; } = true;
        public GameObject GeneratedPlanetPrefab { get; set; }
        public Dictionary<string, Transform> CentroidMarkers { get; set; } = new Dictionary<string, Transform>();

        // A private 1-element array to satisfy the GPU-struct architecture while in the editor
        private readonly PlanetGpuData[] _sandboxGpuData = new PlanetGpuData[1];

        public WorldBuilderContext()
        {
            Data = new PlanetContext(0, _sandboxGpuData);
        }

        // --- THE LINKED SLIDER ALGORITHM (Editor Only) ---
        public void BalanceMasses(ContinentData activePlate, float targetMass)
        {
            var unlocked = Continents.Where(p => p != activePlate && !p.IsLocked).ToList();
            if (unlocked.Count == 0) return;

            targetMass = Mathf.Clamp(targetMass, 0f, 100f);
            float delta = targetMass - activePlate.MassPercentage;

            activePlate.MassPercentage = targetMass;
            float unlockedTotalMass = unlocked.Sum(p => p.MassPercentage);

            foreach (var p in unlocked)
            {
                if (unlockedTotalMass <= 0.001f)
                {
                    p.MassPercentage = Mathf.Max(0f, p.MassPercentage - (delta / unlocked.Count));
                }
                else
                {
                    float weight = p.MassPercentage / unlockedTotalMass;
                    p.MassPercentage = Mathf.Max(0f, p.MassPercentage - (delta * weight));
                }
            }

            UpdateCentroidVisuals();
        }

        public void UpdateCentroidVisuals()
        {
            foreach (var plate in Continents)
            {
                if (CentroidMarkers.TryGetValue(plate.Id, out Transform marker))
                {
                    float visualScale = 0.02f + (plate.MassPercentage / 100f) * 0.38f;
                    marker.localScale = Vector3.one * visualScale;
                }
            }
        }
    }
}
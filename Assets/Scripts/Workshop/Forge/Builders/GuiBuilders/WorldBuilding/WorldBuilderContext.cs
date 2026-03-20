using TheSingularityWorkshop.FSM_API;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders.WorldBuilding
{
    public class WorldBuilderContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "WorldBuilderSim";

        public WorldSimulationData Data { get; set; } = new WorldSimulationData();
        public int SimulationSpeed { get; set; } = 1;

        public bool NeedsMeshRebuild { get; set; } = true;
        public GameObject GeneratedPlanetPrefab { get; set; }

        // Cache the spheres so we can scale them in real-time without rebuilding the planet
        public Dictionary<string, Transform> CentroidMarkers { get; set; } = new Dictionary<string, Transform>();

        // --- THE LINKED SLIDER ALGORITHM ---
        public void BalanceMasses(ContinentData activePlate, float targetMass)
        {
            var unlocked = Data.Continents.Where(p => p != activePlate && !p.IsLocked).ToList();
            if (unlocked.Count == 0) return; // Everything else is locked, cannot steal mass!

            targetMass = Mathf.Clamp(targetMass, 0f, 100f);
            float delta = targetMass - activePlate.MassPercentage;

            activePlate.MassPercentage = targetMass;
            float unlockedTotalMass = unlocked.Sum(p => p.MassPercentage);

            // Distribute the drained/added mass proportionally among unlocked plates
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

            // Immediately update the physical 3D scales in the previewer!
            UpdateCentroidVisuals();
        }

        public void UpdateCentroidVisuals()
        {
            foreach (var plate in Data.Continents)
            {
                if (CentroidMarkers.TryGetValue(plate.Id, out Transform marker))
                {
                    // Scale from 0.02 (tiny) up to 0.4 (massive) based on percentage
                    float visualScale = 0.02f + (plate.MassPercentage / 100f) * 0.38f;
                    marker.localScale = Vector3.one * visualScale;
                }
            }
        }
    }
}
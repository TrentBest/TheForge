using TheSingularityWorkshop.FSM_API;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders.WorldBuilding
{
    public class StarSystemForgeContext : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "SystemForgeSim";

        public StarSystemData System { get; set; } = new StarSystemData();

        // Tells the 3D viewport which planet to put in the center of the screen
        public int ActivePlanetIndex { get; set; } = 0;

        public bool NeedsMeshRebuild { get; set; } = true;
        public GameObject GeneratedSystemPrefab { get; set; }

        public Dictionary<string, Transform> CentroidMarkers { get; set; } = new Dictionary<string, Transform>();

        // Fetches the planet the user is currently editing
        public PlanetData GetActivePlanet()
        {
            if (System.Planets.Count == 0) return null;
            if (ActivePlanetIndex >= System.Planets.Count) ActivePlanetIndex = 0;
            return System.Planets[ActivePlanetIndex];
        }

        public void BalanceMasses(ContinentData activePlate, float targetMass)
        {
            var planet = GetActivePlanet();
            if (planet == null) return;

            var unlocked = planet.Continents.Where(p => p != activePlate && !p.IsLocked).ToList();
            if (unlocked.Count == 0) return;

            targetMass = Mathf.Clamp(targetMass, 0f, 100f);
            float delta = targetMass - activePlate.MassPercentage;
            activePlate.MassPercentage = targetMass;

            float unlockedTotalMass = unlocked.Sum(p => p.MassPercentage);

            foreach (var p in unlocked)
            {
                if (unlockedTotalMass <= 0.001f) p.MassPercentage = Mathf.Max(0f, p.MassPercentage - (delta / unlocked.Count));
                else p.MassPercentage = Mathf.Max(0f, p.MassPercentage - (delta * (p.MassPercentage / unlockedTotalMass)));
            }
            UpdateCentroidVisuals(planet);
        }

        public void UpdateCentroidVisuals(PlanetData planet)
        {
            foreach (var plate in planet.Continents)
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
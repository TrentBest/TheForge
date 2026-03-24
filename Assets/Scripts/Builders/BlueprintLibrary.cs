using System;
using System.Collections.Generic;
using System.Linq;
using TheSingularityWorkshop.Builders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders.WorldBuilding;
using TheSingularityWorkshop.Forge.WorldBuilding;
using UnityEngine;

namespace TheSingularityWorkshop.Libraries
{
    public static class BlueprintLibrary
    {
        // The "Database"
        public static Dictionary<string, MotorBuilder> Motors { get; private set; } = new Dictionary<string, MotorBuilder>();
        public static Dictionary<string, DroneBuilder> Drones { get; private set; } = new Dictionary<string, DroneBuilder>();

        // NEW: Urban Blueprint Storage
        private static Dictionary<UrbanZoneType, List<UrbanBlueprint>> UrbanRegistry = new Dictionary<UrbanZoneType, List<UrbanBlueprint>>();

        static BlueprintLibrary()
        {
            // Seed with defaults for testing
            Register(new MotorBuilder("Standard_100N", 100f));
            Register(new MotorBuilder("Heavy_500N", 500f));

            // Seed a default Urban Zone for testing
            RegisterUrbanTemplate(UrbanZoneType.Residential, 1, SettlementAdaptation.Terrestrial, "Default_House_Prefab");
        }

        // --- CRUD Helpers ---

        public static void Register(MotorBuilder builder)
        {
            if (builder == null || string.IsNullOrEmpty(builder.Name)) return;

            if (Motors.ContainsKey(builder.Name))
                Motors[builder.Name] = builder;
            else
                Motors.Add(builder.Name, builder);
        }

        public static void Register(DroneBuilder builder)
        {
            if (builder == null || string.IsNullOrEmpty(builder.DroneName)) return;

            if (Drones.ContainsKey(builder.DroneName))
                Drones[builder.DroneName] = builder;
            else
                Drones.Add(builder.DroneName, builder);
        }

        // NEW: Urban Registration
        public static void RegisterUrbanTemplate(UrbanZoneType zone, int techLevel, SettlementAdaptation adaptation, string prefabId)
        {
            if (!UrbanRegistry.ContainsKey(zone))
                UrbanRegistry[zone] = new List<UrbanBlueprint>();

            UrbanRegistry[zone].Add(new UrbanBlueprint
            {
                TechLevel = techLevel,
                Adaptation = adaptation,
                PrefabID = prefabId
            });
        }

        public static void DeleteMotor(string name)
        {
            if (Motors.ContainsKey(name)) Motors.Remove(name);
        }

        public static void RenameMotor(string oldName, MotorBuilder builder)
        {
            if (oldName != builder.Name)
            {
                DeleteMotor(oldName);
                Register(builder);
            }
            else
            {
                Register(builder);
            }
        }

        public static List<string> GetMotorNames() => Motors.Keys.ToList();
        public static MotorBuilder GetMotor(string name) => Motors.ContainsKey(name) ? Motors[name] : null;

        // --- Implementation of the Urban Logic ---
        internal static GameObject GetUrbanBlueprint(UrbanZoneType zone, int techLevel, SettlementAdaptation adaptation)
        {
            if (!UrbanRegistry.ContainsKey(zone)) return null;

            // Find the best match: highest tech level available that doesn't exceed the request, matching adaptation
            var match = UrbanRegistry[zone]
                .Where(b => b.Adaptation == adaptation && b.TechLevel <= techLevel)
                .OrderByDescending(b => b.TechLevel)
                .FirstOrDefault();

            if (match == null) return null;

            // In a real Unity implementation, you'd use Addressables or Resources.Load here
            Debug.Log($"[BlueprintLibrary] Loading Urban Blueprint: {match.PrefabID}");
            return null; // Replace with actual loading logic (e.g., Resources.Load<GameObject>(match.PrefabID))
        }
    }

    // Helper classes for the new Urban logic
    public class UrbanBlueprint
    {
        public int TechLevel;
        public SettlementAdaptation Adaptation;
        public string PrefabID;
    }

    //public enum UrbanZoneType { Residential, Industrial, Commercial, Military, Research }
    //public enum SettlementAdaptation { Terrestrial, Orbital, Subterranean, Aquatic }
}
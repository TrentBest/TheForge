using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace Assets.Scripts.MastersOfOrionII
{
    public class HullRole : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "New Strategic Role";
        public string Description { get; set; } = "Defines the strategic intent and AI behavior profile for this vessel.";

        // This is a mock static database to share data between our Editor windows.
        // In your final architecture, this would live in your DataWarehouse or SingularityRegistry.
        public static List<HullRole> Database = new List<HullRole>
        {
            new HullRole { Name = "Strike Craft", Description = "High-speed interception and payload delivery." },
            new HullRole { Name = "Artillery", Description = "Long-range bombardment. Maintain distance from targets." },
            new HullRole { Name = "Science Vessel", Description = "Exploration and anomaly analysis. Flees combat." },
            new HullRole { Name = "Freighter Tug", Description = "Logistical transport of modular mass." }
        };
    }

    public class HullClass : IStateContext
    {
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "New Hull Classification";

        // Dynamic Object Reference instead of an Enum!
        public HullRole PrimaryRole { get; set; }

        // Logarithmic Float (e.g. 1.0 = Fighter, 4.5 = Cruiser, 9.0 = Moon, 15.0 = Unicron)
        public float Magnitude { get; set; } = 3f;

        public float BaseMass => Mathf.Pow(10, Magnitude) * 1000f; // in kg
        public int MaxDecks => Mathf.Max(1, Mathf.FloorToInt(Mathf.Pow(1.8f, Magnitude - 1)));
        public int VolumePerDeck => Mathf.FloorToInt(Mathf.Pow(2.5f, Magnitude));

        public int RequiredTechLevel { get; set; } = 1;
        public bool IsTheoreticalPrototype { get; set; } = false;
    }
}
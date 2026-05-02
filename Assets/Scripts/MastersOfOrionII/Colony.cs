using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.FSM_API;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    /// <summary>
    /// The Planetary Management Context.
    /// Manages the demographic, industrial, and infrastructural state of a world.
    /// Reforged to follow the Experience Model: Logic is driven by Processing Groups.
    /// </summary>
    [Serializable]
    public class Colony : IStateContext, IGuiProvider
    {
        // --- ISTATECONTEXT IMPLEMENTATION ---
        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "Planetary_Colony_Context";

        // --- CORE IDENTITY ---
        public int PlanetId;
        public string ColonyName; // Renamed from Name to avoid IStateContext conflict

        // --- POPULATION & MORALE ---
        public float PopulationMillions;
        public float MaxPopulation;
        public float Morale;             // 0.0 to 1.0
        public float GrowthRate;

        // --- INFRASTRUCTURE & INDUSTRY ---
        public Dictionary<string, int> Buildings = new Dictionary<string, int>();
        // public List<Mine> Mines = new List<Mine>(); // Assuming Mine is defined elsewhere
        // public List<ProductionItem> BuildQueue = new List<ProductionItem>(); 

        // --- LOGISTICS & ESTABLISHMENT ---
        public bool IsEstablishing;
        public float EstablishmentProgress;
        public int MonthlyFunding;

        // --- CALCULATED OUTPUTS ---
        public float NetProduction;
        public float NetScience;
        public float NetIncome;
        public float NetDefense;
        public List<int> InOrbit = new List<int>();

        public Colony(int planetId, string name)
        {
            PlanetId = planetId;
            ColonyName = name;
            IsValid = true; // Signal health to the Forge
        }

        // --- IGUIPROVIDER IMPLEMENTATION (Planetary Dashboard) ---

        public string Title => $"COLONY: {ColonyName.ToUpper()}";

        public UniversePosition Position { get; internal set; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Root Container - High-Tech Planetary Aesthetic
            var rootBuilder = new ForgeContainerBuilder($"{ColonyName}_Root")
                .WithFlexGrow(1)
                .WithPadding(20f)
                .WithBackgroundColor(new Color(0.05f, 0.08f, 0.08f)); // Planetary Teal Tint

            // Header Section
            rootBuilder.AddChild(new ForgeContainerBuilder("Header")
                .WithMarginBottom(15f)
                .WithBorderColor(new Color(0.2f, 0.8f, 0.8f)) // Atmospheric Cyan
                .WithBorderWidth(0, 0, 3, 0)
                .AddChild(new ForgeLabelBuilder(Title)
                    .WithFontSize(22f).WithFontStyle(FontStyle.Bold).WithColor(new Color(0.4f, 1.0f, 0.9f)))
                .AddChild(new ForgeLabelBuilder(IsEstablishing ? "STATUS: ESTABLISHING FRONTIER CAMP" : "STATUS: ESTABLISHED COLONY")
                    .WithColor(IsEstablishing ? Color.yellow : Color.green).WithFontSize(11f))
            );

            // Three-Column Layout for Logistics, Population, and Industry
            var body = new ForgeContainerBuilder("MainBody")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Stretch)
                .WithFlexGrow(1);

            // Column 1: Demographics
            body.AddChild(new ForgeContainerBuilder("Demographics")
                .WithWidth(new StyleLength(Length.Percent(32f)))
                .AddChild(new ForgeLabelBuilder("DEMOGRAPHICS").WithBold().WithMarginBottom(10f))
                .AddChild(new ForgeTextFieldBuilder("Population (M)", PopulationMillions.ToString("F2"))
                    .OnValueChanged(v => { if (float.TryParse(v.newValue, out float r)) PopulationMillions = r; }))
                .AddChild(new ForgeTextFieldBuilder("Growth Rate", GrowthRate.ToString("P2"))
                    .OnValueChanged(v => { if (float.TryParse(v.newValue, out float r)) GrowthRate = r; }))
                .AddChild(new ForgeLabelBuilder($"Morale Index: {Morale:P0}").WithColor(Color.Lerp(Color.red, Color.green, Morale)))
            );

            // Column 2: Logistics & Funding
            body.AddChild(new ForgeContainerBuilder("Logistics")
                .WithWidth(new StyleLength(Length.Percent(32f)))
                .WithPadding(0, 10f, 0, 10f)
                .AddChild(new ForgeLabelBuilder("LOGISTICS").WithBold().WithMarginBottom(10f))
                .AddChild(new ForgeTextFieldBuilder("Monthly Funding", MonthlyFunding.ToString())
                    .OnValueChanged(v => { if (int.TryParse(v.newValue, out int r)) MonthlyFunding = r; }))
                .AddChild(new ForgeLabelBuilder($"Establishment: {EstablishmentProgress:P0}").WithMarginTop(10f))
                // Visual progress bar via OnBuild
                .AddChild(new ForgeContainerBuilder("ProgressBarBG")
                    .WithHeight(4f).WithBackgroundColor(Color.black)
                    .AddChild(new ForgeContainerBuilder("ProgressFill")
                        .WithHeight(4f).WithBackgroundColor(Color.cyan)
                        .WithWidth(new StyleLength(Length.Percent(EstablishmentProgress * 100f)))))
            );

            // Column 3: Industrial Output
            body.AddChild(new ForgeContainerBuilder("Output")
                .WithWidth(new StyleLength(Length.Percent(32f)))
                .AddChild(new ForgeLabelBuilder("OUTPUT MATRIX").WithBold().WithMarginBottom(10f))
                .AddChild(CreateOutputRow("Industry", NetProduction, new Color(0.8f, 0.4f, 0.2f)))
                .AddChild(CreateOutputRow("Science", NetScience, new Color(0.2f, 0.5f, 0.8f)))
                .AddChild(CreateOutputRow("Tax Rev.", NetIncome, new Color(0.8f, 0.7f, 0.2f)))
                .AddChild(CreateOutputRow("Defense", NetDefense, new Color(0.8f, 0.2f, 0.2f)))
            );

            rootBuilder.AddChild(body);

            return rootBuilder.Build();
        }

        private IGuiProvider CreateOutputRow(string label, float val, Color iconColor)
        {
            return new DynamicGuiProvider(ctx => {
                return new ForgeContainerBuilder($"Row_{label}")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithMarginBottom(5f)
                    .AddChild(new ForgeLabelBuilder($"• {label}").WithFontSize(10f).WithColor(iconColor))
                    .AddChild(new ForgeLabelBuilder(val.ToString("F1")).WithFontSize(10f).WithBold())
                    .Build();
            }); // Fixed conversion error using DynamicGuiProvider
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        /// <summary>
        /// Serializes the current colony state to UXML for archival.
        /// </summary>
        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(new GuiContext());
            string fileName = string.IsNullOrEmpty(assetPath) ? $"Colony_{PlanetId}_Manifest" : System.IO.Path.GetFileNameWithoutExtension(assetPath);
            WorkshopUxmlBaker.Bake(root, fileName);
        }

        public void FromUIDocument(string assetPath) { }
    }

    [Serializable]
    public struct UniversePosition
    {
        // The "Mailing Address"
        public Vector3Int OctantCoord;   // Which 1/8th of the Universe
        public Vector3Int ClusterCoord;  // Which Cluster inside that Octant
        public int SystemId;             // The unique ID of the Star System

        // The "Precise Location"
        public Vector3 LocalOffset;      // XYZ relative to the System Center (Star)

        // Helper to get distance between two points in the same system
        public static float Distance(UniversePosition a, UniversePosition b)
        {
            if (a.SystemId != b.SystemId) return float.MaxValue; // Or calculate Interstellar distance
            return Vector3.Distance(a.LocalOffset, b.LocalOffset);
        }
    }
}
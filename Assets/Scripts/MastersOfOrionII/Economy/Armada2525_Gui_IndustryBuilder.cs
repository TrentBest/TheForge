#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII.Economy
{
    public class MastersOfOrionII_Gui_IndustryBuilder : IGuiProvider
    {
        public string Title => "MACRO-ECONOMIC SECTORS";

        private GuiContext _lastCtx;
        private CRUD_Builder<GalacticIndustry> _crudInterface;

        // Mock DB using the new Category string for perfect grouping!
        public static List<GalacticIndustry> IndustryDatabase = new List<GalacticIndustry>
        {
            // TIER 1
            new GalacticIndustry { Name = "Deep Core Extraction", Category = "Tier 1: Extraction", VolatilityIndex = 1.5f, CapitalIntensity = 3.5f, Description = "Planetary fracturing to extract ultra-dense base elements." },
            new GalacticIndustry { Name = "Asteroid Belt Fracturing", Category = "Tier 1: Extraction", VolatilityIndex = 0.8f, CapitalIntensity = 1.5f, Description = "Standard kinetic mining of common atomic materials." },
            new GalacticIndustry { Name = "Biological Harvesting", Category = "Tier 1: Extraction", VolatilityIndex = 2.2f, CapitalIntensity = 1.0f, Description = "Farming extreme-environment flora/fauna." },

            // TIER 2
            new GalacticIndustry { Name = "Heavy Isotope Smelting", Category = "Tier 2: Refinement", VolatilityIndex = 2.0f, CapitalIntensity = 2.5f, Description = "Refining raw elements into reactor-grade material." },
            new GalacticIndustry { Name = "Naval Metallurgy", Category = "Tier 2: Refinement", VolatilityIndex = 0.9f, CapitalIntensity = 2.8f, Description = "Forging elemental alloys into standardized armor plating." },

            // TIER 3
            new GalacticIndustry { Name = "FTL Drive Yards", Category = "Tier 3: Manufacturing", VolatilityIndex = 1.0f, CapitalIntensity = 5.0f, Description = "Construction of hyperspace propulsion systems." },
            new GalacticIndustry { Name = "Life-Support Engineering", Category = "Tier 3: Manufacturing", VolatilityIndex = 0.4f, CapitalIntensity = 1.8f, Description = "Creation of public gravity plating and infantry exoskeletons." },

            // TIER 4
            new GalacticIndustry { Name = "Guided Munitions Assembly", Category = "Tier 4: Ordnance", VolatilityIndex = 2.8f, CapitalIntensity = 1.8f, Description = "The construction of smart-missiles and EMP harpoons." },
            
            // TIER 5
            new GalacticIndustry { Name = "Quantum Cybernetics", Category = "Tier 5: High-Tech", VolatilityIndex = 2.5f, CapitalIntensity = 1.2f, Description = "AI matrices and targeting prediction networks." }
        };

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            _crudInterface = new CRUD_Builder<GalacticIndustry>(
                title: "GLOBAL INDUSTRY DEFINITIONS",
                dataSource: () => IndustryDatabase,
                getDisplayName: (ind) => string.IsNullOrEmpty(ind.Name) ? "Unregistered Sector" : ind.Name,

                // --- DYNAMIC FOLD-OUT MAGIC ---
                getGroupCategory: (ind) => string.IsNullOrEmpty(ind.Category) ? "Uncategorized" : ind.Category,

                buildEditorForm: (ind) =>
                {
                    var form = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.FlexStart } };

                    var leftCol = new VisualElement { style = { width = 350, marginRight = 20 } };
                    leftCol.Add(new Label("INDUSTRY CLASSIFICATION") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                    var leftBox = new VisualElement { style = { backgroundColor = Color.black, paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15, borderLeftWidth = 2, borderLeftColor = Color.cyan } };

                    var nameField = new TextField("Sector Name") { value = ind.Name };
                    nameField.RegisterValueChangedCallback(e => ind.Name = e.newValue);
                    leftBox.Add(nameField);

                    // --- THE TEXT FIELD THAT CONTROLS THE GROUPS ---
                    var catField = new TextField("Macro Category") { value = ind.Category, tooltip = "Type an exact string here to group industries together in the sidebar." };
                    catField.RegisterValueChangedCallback(e => ind.Category = e.newValue);
                    leftBox.Add(catField);

                    var descField = new TextField("Workforce Skill Domain") { value = ind.Description, multiline = true, style = { height = 60, marginTop = 10 } };
                    descField.RegisterValueChangedCallback(e => ind.Description = e.newValue);
                    leftBox.Add(descField);

                    leftCol.Add(leftBox);
                    form.Add(leftCol);

                    var rightCol = new VisualElement { style = { flexGrow = 1 } };
                    rightCol.Add(new Label("MARKET BEHAVIOR PROFILES") { style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                    var rightBox = new VisualElement { style = { backgroundColor = Color.black, paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15, borderLeftWidth = 2, borderLeftColor = Color.yellow } };

                    var volField = new FloatField("Volatility Index") { value = ind.VolatilityIndex, tooltip = "Higher means violent stock swings based on random events." };
                    volField.RegisterValueChangedCallback(e => ind.VolatilityIndex = e.newValue);
                    rightBox.Add(volField);

                    var capField = new FloatField("Capital Intensity") { value = ind.CapitalIntensity, tooltip = "Higher means a massive barrier to entry. Fewer, richer firms." };
                    capField.RegisterValueChangedCallback(e => ind.CapitalIntensity = e.newValue);
                    rightBox.Add(capField);

                    rightCol.Add(rightBox);
                    form.Add(rightCol);

                    return form;
                },

                onSave: (ind) => { if (!IndustryDatabase.Contains(ind)) IndustryDatabase.Add(ind); },
                onDelete: (ind) => { IndustryDatabase.Remove(ind); },
                getSubtitle: (ind) => $"Vol: {ind.VolatilityIndex:F1} | Cap: {ind.CapitalIntensity:F1}"
            );

            return _crudInterface.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }
}
#endif
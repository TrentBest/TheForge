#if UNITY_EDITOR
using TheSingularityWorkshop.Armada2525.Shipyard;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Armada2525.Editor
{
    public class Armada2525_Gui_HullClassBuilder : IGuiProvider
    {
        public string Title => "HULL ARCHITECTURE FORGE";

        private GuiContext _lastCtx;
        private CRUD_Builder<HullClass> _crudInterface;

        private static List<HullClass> _hullDatabase = new List<HullClass>();
        private int _playerCurrentTechLevel = 3;

        public Armada2525_Gui_HullClassBuilder()
        {
            // Seed mock data if empty
            if (_hullDatabase.Count == 0 && HullRole.Database.Count > 0)
            {
                _hullDatabase.Add(new HullClass { Name = "Star Sparrow Chassis", PrimaryRole = HullRole.Database[0], Magnitude = 1.2f, RequiredTechLevel = 1 });
            }
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            _crudInterface = new CRUD_Builder<HullClass>(
                title: "ARCHITECTURAL BLUEPRINTS",
                dataSource: () => _hullDatabase,
                getDisplayName: (hull) => string.IsNullOrEmpty(hull.Name) ? "Unregistered Architecture" : hull.Name,

                // Group by the dynamic Role name!
                getGroupCategory: (hull) => hull.PrimaryRole != null ? hull.PrimaryRole.Name : "Unassigned",

                buildEditorForm: (hull) =>
                {
                    var form = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.FlexStart } };

                    // --- LEFT SIDE: DESIGN SPECIFICATIONS ---
                    var designCol = new VisualElement { style = { width = 320, marginRight = 20 } };
                    designCol.Add(new Label("ENGINEERING SPECIFICATIONS") { style = { color = new Color(0.85f, 0.40f, 0.10f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                    var designBox = new VisualElement { style = { backgroundColor = Color.black, paddingLeft = 10, paddingRight = 10, paddingTop = 10, paddingBottom = 10, borderLeftWidth = 2, borderLeftColor = new Color(0.85f, 0.40f, 0.10f) } };

                    var nameField = new TextField("Class Name") { value = hull.Name };
                    nameField.RegisterValueChangedCallback(e => hull.Name = e.newValue);
                    designBox.Add(nameField);

                    // --- THE DYNAMIC ROLE DROPDOWN ---
                    var roleNames = HullRole.Database.Select(r => r.Name).ToList();
                    int defaultIndex = hull.PrimaryRole != null ? roleNames.IndexOf(hull.PrimaryRole.Name) : 0;
                    if (defaultIndex < 0) defaultIndex = 0;

                    var roleDropdown = new DropdownField("Primary Role", roleNames, defaultIndex);
                    roleDropdown.RegisterValueChangedCallback(e => {
                        hull.PrimaryRole = HullRole.Database.Find(r => r.Name == e.newValue);
                    });
                    designBox.Add(roleDropdown);

                    // Expanded to 15.0f for Planetoid scale!
                    var magField = new Slider("Magnitude (Scale)", 0.5f, 15.0f) { value = hull.Magnitude, showInputField = true };

                    var massLabel = new Label();
                    var deckLabel = new Label();
                    var techWarningLabel = new Label() { style = { color = Color.red, whiteSpace = WhiteSpace.Normal, marginTop = 10, unityFontStyleAndWeight = FontStyle.Bold } };

                    Action updateDerivedStats = () => {
                        massLabel.text = $"Displacement Mass: {hull.BaseMass:N0} kg";
                        deckLabel.text = $"Internal Grid: {hull.MaxDecks} Decks x {hull.VolumePerDeck} Tiles";

                        hull.RequiredTechLevel = Mathf.CeilToInt(hull.Magnitude / 1.5f);

                        if (hull.RequiredTechLevel > _playerCurrentTechLevel)
                        {
                            hull.IsTheoreticalPrototype = true;
                            techWarningLabel.text = $"⚠️ THEORETICAL PROTOTYPE: Requires Materials Tech Level {hull.RequiredTechLevel} (Current: {_playerCurrentTechLevel}). Fabrication is locked until research completes.";
                            techWarningLabel.style.display = DisplayStyle.Flex;
                        }
                        else
                        {
                            hull.IsTheoreticalPrototype = false;
                            techWarningLabel.style.display = DisplayStyle.None;
                        }
                    };

                    magField.RegisterValueChangedCallback(e => {
                        hull.Magnitude = e.newValue;
                        updateDerivedStats();
                    });

                    designBox.Add(magField);
                    designCol.Add(designBox);
                    form.Add(designCol);

                    // --- RIGHT SIDE: PHYSICAL MANIFESTATION ---
                    var physicsCol = new VisualElement { style = { flexGrow = 1 } };
                    physicsCol.Add(new Label("PHYSICAL MANIFESTATION") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                    var physicsBox = new VisualElement { style = { backgroundColor = Color.black, paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15, borderLeftWidth = 2, borderLeftColor = Color.cyan } };

                    massLabel.style.color = Color.white; massLabel.style.marginBottom = 5;
                    deckLabel.style.color = Color.white; deckLabel.style.marginBottom = 5;

                    physicsBox.Add(massLabel);
                    physicsBox.Add(deckLabel);
                    physicsBox.Add(techWarningLabel);

                    updateDerivedStats();

                    physicsCol.Add(physicsBox);
                    form.Add(physicsCol);

                    return form;
                },

                onSave: (hull) => { if (!_hullDatabase.Contains(hull)) _hullDatabase.Add(hull); },
                onDelete: (hull) => { _hullDatabase.Remove(hull); },
                getSubtitle: (hull) => hull.IsTheoreticalPrototype ? $"[PROTOTYPE] Mag: {hull.Magnitude:F1}" : $"Mag: {hull.Magnitude:F1} | Mass: {hull.BaseMass:N0}kg"
            );

            return _crudInterface.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }
}
#endif
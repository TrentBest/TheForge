#if UNITY_EDITOR
using Assets.Scripts.MastersOfOrionII.Government;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements; // Required for ColorField
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_FactionBuilder : IGuiProvider
    {
        public string Title => "GALACTIC FACTION FORGE";

        private GuiContext _lastCtx;
        private CRUD_Builder<PoliticalFaction> _crudInterface;

        // The Master Database of all political entities in the galaxy
        public static List<PoliticalFaction> FactionDatabase = new List<PoliticalFaction>();

        // --- MOCK DATABASES FOR DROPDOWNS ---
        public static List<FactionIdeology> MockIdeologyDB = new List<FactionIdeology> {
            new FactionIdeology { Name = "Imperial Doctrine" },
            new FactionIdeology { Name = "Anarcho-Capitalism" },
            new FactionIdeology { Name = "Expansionist" }
        };

        public static List<FactionDoctrine> MockDoctrineDB = new List<FactionDoctrine> {
            new FactionDoctrine { Name = "Balanced Fleet" },
            new FactionDoctrine { Name = "Industrial Heavy" },
            new FactionDoctrine { Name = "Technocratic" }
        };

        public MastersOfOrionII_Gui_FactionBuilder()
        {
            // Seed mock data to demonstrate the hierarchy if empty
            if (FactionDatabase.Count == 0)
            {
                var loyalists = new PoliticalFaction
                {
                    Name = "Core World Loyalists",
                    LeaderName = "High Chancellor Vane",
                    CoreIdeology = MockIdeologyDB[0],
                    PrimaryDoctrine = MockDoctrineDB[0],
                    FactionColor = Color.cyan,
                    PoliticalCapital = 5000000
                };

                var syndicate = new PoliticalFaction
                {
                    Name = "Outer Rim Syndicate",
                    LeaderName = "Kael Thorne",
                    CoreIdeology = MockIdeologyDB[1],
                    PrimaryDoctrine = MockDoctrineDB[1],
                    FactionColor = Color.yellow,
                    PoliticalCapital = 2000000
                };

                var playerVanguard = new PoliticalFaction
                {
                    Name = "Player Vanguard",
                    LeaderName = "The Commander",
                    CoreIdeology = MockIdeologyDB[2],
                    PrimaryDoctrine = MockDoctrineDB[2],
                    FactionColor = Color.green,
                    IsPlayerFaction = true,
                    ParentFaction = syndicate
                };

                FactionDatabase.Add(loyalists);
                FactionDatabase.Add(syndicate);
                FactionDatabase.Add(playerVanguard);
            }
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            _crudInterface = new CRUD_Builder<PoliticalFaction>(
                title: "POLITICAL ENTITIES",
                dataSource: () => FactionDatabase,
                getDisplayName: (faction) => string.IsNullOrEmpty(faction.Name) ? "Unknown Syndicate" : faction.Name,

                // Group them visually by whether they answer to the Empress or another Faction
                getGroupCategory: (faction) => faction.ParentFaction == null ? "Major Factions (Crown Direct)" : $"Sub-Factions of {faction.ParentFaction.Name}",

                buildEditorForm: (faction) =>
                {
                    var form = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.FlexStart } };

                    // --- LEFT SIDE: IDENTITY ---
                    var identityCol = new VisualElement { style = { width = 350, marginRight = 20 } };
                    identityCol.Add(new Label("FACTION IDENTITY") { style = { color = new Color(0.8f, 0.2f, 0.2f), unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                    var identityBox = new VisualElement { style = { backgroundColor = Color.black, paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15, borderLeftWidth = 2, borderLeftColor = new Color(0.8f, 0.2f, 0.2f) } };

                    var nameField = new TextField("Syndicate Name") { value = faction.Name };
                    nameField.RegisterValueChangedCallback(e => faction.Name = e.newValue);
                    identityBox.Add(nameField);

                    var leaderField = new TextField("Figurehead / Leader") { value = faction.LeaderName };
                    leaderField.RegisterValueChangedCallback(e => faction.LeaderName = e.newValue);
                    identityBox.Add(leaderField);

                    var colorField = new ColorField("Faction Colors") { value = faction.FactionColor };
                    colorField.RegisterValueChangedCallback(e => faction.FactionColor = e.newValue);
                    identityBox.Add(colorField);

                    // Dropdown for Ideology
                    var ideologyNames = MockIdeologyDB.Select(i => i.Name).ToList();
                    int idIndex = faction.CoreIdeology != null ? ideologyNames.IndexOf(faction.CoreIdeology.Name) : 0;
                    var idDropdown = new DropdownField("Core Ideology", ideologyNames, idIndex >= 0 ? idIndex : 0);
                    idDropdown.RegisterValueChangedCallback(e => faction.CoreIdeology = MockIdeologyDB.Find(i => i.Name == e.newValue));
                    identityBox.Add(idDropdown);

                    // Dropdown for Doctrine
                    var doctrineNames = MockDoctrineDB.Select(d => d.Name).ToList();
                    int docIndex = faction.PrimaryDoctrine != null ? doctrineNames.IndexOf(faction.PrimaryDoctrine.Name) : 0;
                    var docDropdown = new DropdownField("Strategic Doctrine", doctrineNames, docIndex >= 0 ? docIndex : 0);
                    docDropdown.RegisterValueChangedCallback(e => faction.PrimaryDoctrine = MockDoctrineDB.Find(d => d.Name == e.newValue));
                    identityBox.Add(docDropdown);

                    identityCol.Add(identityBox);
                    form.Add(identityCol);

                    // --- RIGHT SIDE: HIERARCHY & POWER ---
                    var powerCol = new VisualElement { style = { flexGrow = 1 } };
                    powerCol.Add(new Label("ALLEGIANCE & POWER") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

                    var powerBox = new VisualElement { style = { backgroundColor = Color.black, paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15, borderLeftWidth = 2, borderLeftColor = Color.cyan } };

                    // Dynamic Liege Lord Dropdown
                    var validParents = FactionDatabase.Where(f => f != faction).ToList();
                    var parentNames = validParents.Select(f => f.Name).ToList();
                    parentNames.Insert(0, "NONE (Answers directly to Empress)");

                    int defaultIndex = faction.ParentFaction != null ? validParents.IndexOf(faction.ParentFaction) + 1 : 0;
                    if (defaultIndex < 0) defaultIndex = 0;

                    var parentDropdown = new DropdownField("Liege Lord (Parent)", parentNames, defaultIndex);
                    parentDropdown.RegisterValueChangedCallback(e => {
                        if (e.newValue.Contains("NONE")) faction.ParentFaction = null;
                        else faction.ParentFaction = validParents.Find(f => f.Name == e.newValue);
                    });
                    powerBox.Add(parentDropdown);

                    var capField = new DoubleField("Political Capital") { value = faction.PoliticalCapital };
                    capField.RegisterValueChangedCallback(e => faction.PoliticalCapital = e.newValue);
                    powerBox.Add(capField);

                    var playerToggle = new Toggle("Is Player Faction") { value = faction.IsPlayerFaction };
                    playerToggle.RegisterValueChangedCallback(e => faction.IsPlayerFaction = e.newValue);
                    powerBox.Add(playerToggle);

                    // Display Sub-Factions dynamically
                    var subFactions = faction.GetSubFactions(FactionDatabase);
                    if (subFactions.Count > 0)
                    {
                        powerBox.Add(new Label($"Vassal Entities ({subFactions.Count}):") { style = { color = Color.gray, marginTop = 15, marginBottom = 5 } });
                        foreach (var sub in subFactions)
                        {
                            powerBox.Add(new Label($"- {sub.Name}") { style = { color = new Color(0.7f, 0.7f, 0.7f), marginLeft = 10 } });
                        }
                    }

                    powerCol.Add(powerBox);
                    form.Add(powerCol);

                    return form;
                },

                onSave: (faction) => { if (!FactionDatabase.Contains(faction)) FactionDatabase.Add(faction); },
                onDelete: (faction) => {
                    // Safety check: disconnect vassals before deleting the liege lord
                    foreach (var sub in faction.GetSubFactions(FactionDatabase)) sub.ParentFaction = null;
                    FactionDatabase.Remove(faction);
                },
                getSubtitle: (faction) => faction.IsPlayerFaction ? "[ PLAYER SYNDICATE ]" : $"Cap: {faction.PoliticalCapital:N0}"
            );

            return _crudInterface.CreateGui(ctx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }
}
#endif
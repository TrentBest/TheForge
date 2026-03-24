using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.Builders.GuiBuilders;
using Assets.Scripts.raWWar;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts.raWWar.Editors
{
    public enum EchelonScale:long
    {
        // --- Tactical ---
        Squad = 10,
        Platoon = 40,
        Company = 150,
        Battalion = 500,
        Regiment = 2000,
        Legion = 8000,

        // --- Sci-Fi / Strategic ---
        Division = 20000,        // Standard modern large-scale unit
        Corps = 100000,          // Planetary invasion force
        ArmyGroup = 500000,      // Continental theater force
        Expedition = 2000000,    // Interstellar task force
        Crusade = 10000000,      // System-wide conquest force (The "40k" Scale)

        // --- Astronomical (The Latin "Big Numbers") ---
        Grandis = 100000000,     // Latin: "Great" (Global occupation force)
        Immensas = 1000000000,   // Latin: "Immeasurable" (A Billion soldiers)
        Infinitum = 10000000000, // Latin: "Unbounded" (Sector-wide swarm)
        Aeterna = 100000000000   // Latin: "Eternal" (The final number)
    }

    public class raWWar_Gui_SoldierEditor : IGuiProvider
    {
        public string Title => "raWWar Tactical Forge";

        // --- Theme Palette ---
        private readonly Color _appBackground = new Color(0.04f, 0.04f, 0.06f);
        private readonly Color _panelBackground = new Color(0.10f, 0.10f, 0.14f);
        private readonly Color _accentColor = new Color(0.65f, 0.2f, 0.95f);
        private readonly Color _textDim = new Color(0.6f, 0.6f, 0.7f);

        // --- State ---
        private EchelonScale _currentScale = EchelonScale.Squad;
        private string _unitName = "Vanguard Infantry";
        private float _health = 100f;
        private float _discipline = 80f;
        private float _speed = 5f;
        private int _armorRating = 2;
        private FactionAllegiance _currentFaction = FactionAllegiance.Blue;

        // Reference to the active LMP so we can trigger updates/cleanup
        private LiveModelPreviewBuilder _activeLmp;

        public VisualElement CreateGui(GuiContext ctx)
        {
            // Pattern: SplitPanelBuilder for layout abstraction
            return new SplitPanelBuilder(sidebarWidth: 380, Side.Left)
                .WithSidebar(BuildControlsSidebar(ctx))
                .WithMain(BuildMainPreview(ctx))
                .CreateGui(ctx);
        }

        private IGuiProvider BuildControlsSidebar(GuiContext ctx)
        {
            return new GraphicalUserInterfaceBuilder("SoldierControls")
                .WithPadding(20)
                .WithBackgroundColor(_panelBackground)
                .WithScrollable(true)

                // Designation Section
                .AddChild(new Label("UNIT DESIGNATION") { style = { color = _textDim, letterSpacing = 2, fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold } })
                .AddStringData("Name", _unitName, v => _unitName = v)
                .AddSeparator(_accentColor, 1)

                // Tactical Scale (Echelon Tabs refactored to Button Data)
                .AddChild(new Label("TACTICAL DEPLOYMENT SCALE") { style = { color = Color.white, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 15 } })
                .OnBuild(ve => {
                    // Logic to build echelon buttons using your control pattern
                    foreach (EchelonScale scale in Enum.GetValues(typeof(EchelonScale)))
                    {
                        var isAct = _currentScale == scale;
                        // Injecting a button into the build stream
                        ve.Add(new Button(() => {
                            _currentScale = scale;
                            Refresh(ctx);
                        })
                        {
                            text = scale.ToString(),
                            style = {
                                backgroundColor = isAct ? _accentColor : new Color(0.18f, 0.18f, 0.22f),
                                color = isAct ? Color.white : _textDim
                            }
                        });
                    }
                })

                // Stats Section
                .AddSeparator(_accentColor, 1)
                .AddSliderData("Base Health", 10, 500, _health, v => _health = v)
                .AddSliderData("Mobility Speed", 1, 20, _speed, v => _speed = v)
                .AddIntSliderData("Armor Rating", 0, 10, _armorRating, v => _armorRating = v)

                // Faction Section
                .AddSeparator(_accentColor, 1)
                .AddEnumData("Faction Allegiance", _currentFaction, v => {
                    _currentFaction = v;
                    Refresh(ctx);
                });
        }

        private IGuiProvider BuildMainPreview(GuiContext ctx)
        {
            Mesh soldierMesh = null;
            Material soldierMaterial = null;

#if UNITY_EDITOR
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/unit_Infantry_Light_A_yup.fbx");
            if (fbx != null)
            {
                var mf = fbx.GetComponentInChildren<MeshFilter>();
                if (mf != null) soldierMesh = mf.sharedMesh;
            }
            string matPath = $"Assets/Materials/raWWar/mtrl_canopus-iii_set01-{_currentFaction.ToString().ToLower()}.mat";
            soldierMaterial = AssetDatabase.LoadAssetAtPath<Material>(matPath);
#endif

            // Coordination: Using the NEW LiveModelPreviewBuilder which handles relational staging
            // Instead of just a mesh, we pass the dummy object representing the formation if needed.
            // For now, we utilize the standard LMP interface.
            if (soldierMesh != null)
            {
                var previewObj = new GameObject("Soldier_Preview_Ghost");
                var mf = previewObj.AddComponent<MeshFilter>();
                var mr = previewObj.AddComponent<MeshRenderer>();
                mf.sharedMesh = soldierMesh;
                mr.sharedMaterial = soldierMaterial;

                // Enforce zero truncation of your previous logic:
                // We return the LMP which uses the -500 / +500 relational logic we just built.
                return new LiveModelPreviewBuilder(previewObj)
                    .WithBackgroundColor(_appBackground)
                    .WithMouseControl(true)
                    .WithGizmos(true);
            }

            return new GraphicalUserInterfaceBuilder("Empty")
                .AddChild(new Label("RESOURCES NOT FOUND") { style = { color = Color.red } });
        }

        private void Refresh(GuiContext ctx)
        {
            // Standard coordination for refreshing the UI in your ecosystem
            ctx.OnBuilt?.Invoke(null);
            // In a real scenario, the Forge handles the swap of providers
        }

        public void ToUIDocument(string id) { }
        public void FromUIDocument(string document) { }
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    }
}
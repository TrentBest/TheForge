using Assets.Scripts.Workshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using TheSingularityWorkshop.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using UnityEngine;
using UnityEngine.UIElements;


#if UNITY_EDITOR
using UnityEditor; // Required for AssetDatabase
#endif


namespace Assets.Scripts.raWWar.Editors
{
    public enum EchelonScale
    {
        Squad = 10,
        Platoon = 40,
        Company = 150,
        Battalion = 500,
        Regiment = 2000,
        Legion = 8000
    }

    public class raWWar_Gui_SoldierEditor : IGuiProvider
    {
        public string Title => "raWWar Tactical Forge";

        private SplitPanelBuilder _splitBuilder;
        private VisualElement _root;
        private FactionAllegiance _currentFaction = FactionAllegiance.Blue;
        // --- Theme Palette ---
        // Deep space backdrop, sleek translucent panels, and an electric purple accent
        private readonly Color _appBackground = new Color(0.04f, 0.04f, 0.06f);
        private readonly Color _panelBackground = new Color(0.10f, 0.10f, 0.14f);
        private readonly Color _accentColor = new Color(0.65f, 0.2f, 0.95f); // Vibrant Sci-Fi Purple
        private readonly Color _textDim = new Color(0.6f, 0.6f, 0.7f);
        private readonly float _softCornerRadius = 12f;

        // --- State ---
        private EchelonScale _currentScale = EchelonScale.Squad;
        private string _unitName = "Vanguard Infantry";
        private float _health = 100f;
        private float _discipline = 80f;
        private float _speed = 5f;
        private int _armorRating = 2;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _splitBuilder = new SplitPanelBuilder(sidebarWidth: 380, Side.Left);

            _splitBuilder.WithSidebar(new DynamicGuiProvider(c => BuildControlsContext()));
            _splitBuilder.WithMain(new DynamicGuiProvider(c => BuildLivePreviewContext()));

            _root = _splitBuilder.CreateGui(ctx);
            // Give the absolute root that deep space backdrop
            _root.style.backgroundColor = _appBackground;
            return _root;
        }

        private VisualElement BuildControlsContext()
        {
            var builder = new GraphicalUserInterfaceBuilder("SoldierControls")
                .WithPadding(20)
                .WithBackgroundColor(_panelBackground)
                .WithBorderRightColor(new Color(0.2f, 0.2f, 0.25f)) // Subtle seam
                .WithBorderRightWidth(1)
                .WithScrollable(true);

            // --- HEADER ---
            builder.AddChild(new Label("UNIT DESIGNATION") { style = { color = _textDim, letterSpacing = 2, fontSize = 11, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
            builder.AddStringData("Name", _unitName, v => _unitName = v);

            // --- ECHELON TABS ---
            builder.AddSeparator(_accentColor, 1);
            builder.AddChild(new Label("TACTICAL DEPLOYMENT SCALE") { style = { color = Color.white, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 15, marginBottom = 10 } });

            var tabsBuilder = new GraphicalUserInterfaceBuilder("EchelonTabs")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .WithFlexWrap(Wrap.Wrap)
                .WithMarginBottom(15);

            foreach (EchelonScale scale in Enum.GetValues(typeof(EchelonScale)))
            {
                bool isActive = _currentScale == scale;
                var btn = new Button(() => {
                    _currentScale = scale;
                    RefreshEditor();
                })
                { text = scale.ToString() };

                // Soft rounded "pill" style buttons for the sci-fi look
                btn.style.flexGrow = 1;
                btn.style.height = 32;
                btn.style.marginTop = 4;
                btn.style.marginBottom = 4;
                btn.style.marginLeft = 4;
                btn.style.marginRight = 4;
                btn.style.borderTopLeftRadius = _softCornerRadius;
                btn.style.borderTopRightRadius = _softCornerRadius;
                btn.style.borderBottomLeftRadius = _softCornerRadius;
                btn.style.borderBottomRightRadius = _softCornerRadius;
                btn.style.borderTopWidth = 0; btn.style.borderBottomWidth = 0; btn.style.borderLeftWidth = 0; btn.style.borderRightWidth = 0;

                btn.style.backgroundColor = isActive ? _accentColor : new Color(0.18f, 0.18f, 0.22f);
                btn.style.color = isActive ? Color.white : _textDim;
                btn.style.unityFontStyleAndWeight = isActive ? FontStyle.Bold : FontStyle.Normal;

                // Simple hover effect
                btn.RegisterCallback<MouseEnterEvent>(e => { if (!isActive) btn.style.backgroundColor = new Color(0.25f, 0.25f, 0.3f); });
                btn.RegisterCallback<MouseLeaveEvent>(e => { if (!isActive) btn.style.backgroundColor = new Color(0.18f, 0.18f, 0.22f); });

                tabsBuilder.AddChild(btn);
            }
            builder.AddChild(tabsBuilder);

            // --- STATS ---
            builder.AddSeparator(_accentColor, 1);
            builder.AddChild(new Label("COMBAT DOCTRINE") { style = { color = Color.white, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 15, marginBottom = 10 } });

            builder.AddSliderData("Base Health", 10, 500, _health, v => _health = v);
            builder.AddSliderData("Morale Resilience", 0, 100, _discipline, v => _discipline = v);
            builder.AddSliderData("Mobility Speed", 1, 20, _speed, v => _speed = v);
            builder.AddIntSliderData("Armor Rating", 0, 10, _armorRating, v => _armorRating = v);

            // --- LOADOUT ---
            builder.AddSeparator(_accentColor, 1);
            builder.AddChild(new Label("EQUIPMENT LOADOUT") { style = { color = Color.white, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 15, marginBottom = 10 } });

            List<string> weapons = new List<string> { "Mk4 Assault Rifle", "Heavy Mag-Repeater", "Plasma Incinerator" };
            builder.AddDropdownData("Primary Weapon", weapons, 0, w => { Debug.Log($"Weapon set to {w}"); });

            List<string> armor = new List<string> { "Standard Flak", "Carapace Armor", "Powered Exosuit" };
            builder.AddDropdownData("Armor Type", armor, 0, a => { Debug.Log($"Armor set to {a}"); });
            builder.AddSeparator(_accentColor, 1);
            builder.AddChild(new Label("FACTION ALLEGIANCE") { style = { color = Color.white, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 15, marginBottom = 10 } });

            builder.AddEnumData("Material Colors", _currentFaction, v => {
                _currentFaction = v;
                RefreshEditor(); // Rebuild the 3D view with the new material!
            });
            return builder.Build();
        }

        private VisualElement BuildLivePreviewContext()
        {
            var builder = new GraphicalUserInterfaceBuilder("FormationPreviewPane")
                .WithBackgroundColor(Color.clear)
                .WithAutoGrow()
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center);

            // --- 1. THE HOLOGRAPHIC OVERLAY ---
            var overlayPanel = new VisualElement
            {
                style = {
            position = Position.Absolute, top = 20, left = 20,
            backgroundColor = new Color(0.05f, 0.05f, 0.08f, 0.7f),
            paddingTop = 10, paddingBottom = 10, paddingLeft = 15, paddingRight = 15,
            borderTopLeftRadius = _softCornerRadius, borderTopRightRadius = _softCornerRadius,
            borderBottomLeftRadius = _softCornerRadius, borderBottomRightRadius = _softCornerRadius,
            borderLeftColor = _accentColor, borderLeftWidth = 3
        }
            };
            overlayPanel.Add(new Label("TACTICAL FEED : ONLINE") { style = { color = _accentColor, fontSize = 10, letterSpacing = 2, unityFontStyleAndWeight = FontStyle.Bold } });
            overlayPanel.Add(new Label($"RENDER QUEUE: {(int)_currentScale} UNITS") { style = { color = Color.white, fontSize = 16, marginTop = 5 } });

            builder.AddChild(overlayPanel);

            // --- 2. ASSET INJECTION & GPU BUILDER ---
            Mesh soldierMesh = null;
            Material soldierMaterial = null;

#if UNITY_EDITOR
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/unit_Infantry_Light_A_yup.fbx");
            if (fbx != null)
            {
                var meshFilter = fbx.GetComponentInChildren<MeshFilter>();
                if (meshFilter != null) soldierMesh = meshFilter.sharedMesh;
            }

            // DYNAMIC MATERIAL LOADING!
            string matPath = $"Assets/Materials/raWWar/mtrl_canopus-iii_set01-{_currentFaction.ToString().ToLower()}.mat";
            soldierMaterial = AssetDatabase.LoadAssetAtPath<Material>(matPath);
#endif

            if (soldierMesh != null && soldierMaterial != null)
            {
                // MIC DROP: Inject the actual GPU Instancing Preview!
                builder.AddChild(new LiveFormationPreviewBuilder(soldierMesh, soldierMaterial, (int)_currentScale));
            }
            else
            {
                // Fallback if the paths are slightly off
                builder.AddChild(new Label("ERROR: MISSING MESH OR MATERIAL ASSETS")
                {
                    style = { color = Color.red, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold }
                });
            }

            return builder.Build();
        }

        private void RefreshEditor()
        {
            if (_root != null && _root.parent != null)
            {
                var parent = _root.parent;
                int index = parent.IndexOf(_root);
                parent.Remove(_root);
                parent.Insert(index, CreateGui(new GuiContext()));
            }
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}
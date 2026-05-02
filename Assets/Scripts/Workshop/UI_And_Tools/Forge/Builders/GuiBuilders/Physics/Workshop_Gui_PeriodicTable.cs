using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TheSingularityWorkshop.Core.Physics.Chemistry;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes;
using yWorkshop.Forge.Builders.GuiBuilders.Physics;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders.Physics
{
    public class Workshop_Gui_PeriodicTable : IGuiProvider
    {
        public string Title => "Periodic Table of Elements";

        private List<AtomicElement> _elementDatabase;
        private AtomicElement? _inspectedElement = null;

        // UI Containers
        private VisualElement _rootContainer;
        private VisualElement _tableContainer;
        private VisualElement _inspectorContainer;

        // Memory Management
        private GameObject _currentPreviewModel;
        private LiveModelPreviewBuilder _activePreviewBuilder;

        public Workshop_Gui_PeriodicTable()
        {
            _elementDatabase = ElementData.GetElements();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _rootContainer = new VisualElement { style = { flexGrow = 1 } };

            _rootContainer.RegisterCallback<DetachFromPanelEvent>(evt => {
                ScrubPreviewMemory();
            });

            RebuildUI();

            return _rootContainer;
        }

        private void RebuildUI()
        {
            _rootContainer.Clear();
            ScrubPreviewMemory();

            var theme = GuiSkin.Active;
            var defaultStyle = theme.GlobalDefault;

            var mainLayoutBuilder = new GraphicalUserInterfaceBuilder("PeriodicTableLayout")
                .WithAutoGrow()
                .WithFlexLayout(FlexDirection.Row)
                .WithBackgroundColor(defaultStyle.BackgroundColor);

            // --- LEFT PANEL: TABLE ---
            var tablePanelBuilder = new GraphicalUserInterfaceBuilder("TableContainer")
                .WithFlexGrow(3)
                .WithPadding(15)
                .WithScrollable(true)
                .WithBorderRightWidth(defaultStyle.BorderRightWidth)
                .WithBorderRightColor(defaultStyle.BorderColor);

            // Top control bar for theme switching
            var topBar = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginBottom = 10 } };

            var headerLabel = new Label("ELEMENTAL REPOSITORY")
            {
                style = {
                    fontSize = 24,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = theme.PrimaryAccent
                }
            };
            topBar.Add(headerLabel);

            topBar.Add(BuildThemeSelector());
            tablePanelBuilder.AddChild(topBar);

            tablePanelBuilder.AddChild(BuildPeriodicGrid());
            mainLayoutBuilder.AddChild(tablePanelBuilder);

            // --- RIGHT PANEL: INSPECTOR ---
            var inspectorPanelBuilder = new GraphicalUserInterfaceBuilder("InspectorWrapper")
                .WithWidth(350)
                .WithPadding(15);

            inspectorPanelBuilder.AddHeader("THERMODYNAMIC INSPECTOR", defaultStyle.TextColor);

            inspectorPanelBuilder.AddChild(context =>
            {
                _inspectorContainer = new GraphicalUserInterfaceBuilder("InspectorData").WithAutoGrow().Build();
                return _inspectorContainer;
            });

            mainLayoutBuilder.AddChild(inspectorPanelBuilder);
            _rootContainer.Add(mainLayoutBuilder.Build());

            RefreshInspector();
        }

        private VisualElement BuildThemeSelector()
        {
            // Dynamically load available themes from Resources
            var themes = Resources.LoadAll<GuiTheme>("Themes").ToList();

            // Safety: Ensure the currently active skin is always in the list, 
            // even if it hasn't been saved to the Resources folder yet.
            if (GuiSkin.Active != null && !themes.Contains(GuiSkin.Active))
            {
                themes.Add(GuiSkin.Active);
            }

            if (themes.Count == 0) return new Label("No Themes Found") { style = { color = Color.red } };

            var themeNames = themes.Select(t => string.IsNullOrEmpty(t.name) ? "Unnamed Theme" : t.name).ToList();

            string currentName = GuiSkin.Active != null && !string.IsNullOrEmpty(GuiSkin.Active.name)
                ? GuiSkin.Active.name
                : themeNames[0];

            // Use a numeric index to initialize. This completely prevents the "Value not present" UI crash.
            int safeIndex = Mathf.Max(0, themeNames.IndexOf(currentName));

            var dropdown = new DropdownField("Style Interface:", themeNames, safeIndex);
            dropdown.style.width = 250;

            var effTheme = GuiSkin.Active != null ? GuiSkin.Active.GlobalDefault.TextColor : Color.white;
            dropdown.style.color = effTheme;

            dropdown.RegisterValueChangedCallback(evt =>
            {
                var selected = themes.FirstOrDefault(t => (string.IsNullOrEmpty(t.name) ? "Unnamed Theme" : t.name) == evt.newValue);
                if (selected != null)
                {
                    GuiSkin.Active = selected;
                    RebuildUI(); // Completely refresh the layout with new colors
                }
            });

            return dropdown;
        }

        private void ScrubPreviewMemory()
        {
            if (_activePreviewBuilder != null)
            {
                _activePreviewBuilder.Dispose();
                _activePreviewBuilder = null;
            }
            if (_currentPreviewModel != null)
            {
                UnityEngine.Object.DestroyImmediate(_currentPreviewModel);
                _currentPreviewModel = null;
            }
        }

        private VisualElement CreateEmptySlotTile(int period, int group)
        {
            var theme = GuiSkin.Active.GlobalDefault;

            var tileBuilder = new GraphicalUserInterfaceBuilder($"EmptySlot_{period}_{group}")
                .WithWidth(65).WithHeight(85)
                .WithMarginRight(4)
                .WithBackgroundColor(new Color(theme.BackgroundColor.r, theme.BackgroundColor.g, theme.BackgroundColor.b, 0.4f))
                .WithBorderWidth(1)
                .WithBorderAllColor(theme.BorderColor);

            var tile = tileBuilder.Build();
            tile.style.borderTopLeftRadius = theme.BorderTopLeftRadius;
            tile.style.borderBottomRightRadius = theme.BorderBottomRightRadius;
            tile.style.justifyContent = Justify.Center;
            tile.style.alignItems = Align.Center;

            var plusLabel = new Label("+")
            {
                style = { fontSize = 32, color = theme.BorderColor, unityFontStyleAndWeight = FontStyle.Bold }
            };
            tile.Add(plusLabel);

            tile.RegisterCallback<MouseEnterEvent>(evt =>
            {
                tile.style.backgroundColor = theme.BorderColor;
                plusLabel.style.color = theme.TextColor;
            });
            tile.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                tile.style.backgroundColor = new Color(theme.BackgroundColor.r, theme.BackgroundColor.g, theme.BackgroundColor.b, 0.4f);
                plusLabel.style.color = theme.BorderColor;
            });

            tile.RegisterCallback<ClickEvent>(evt =>
            {
                ScrubPreviewMemory();
                _inspectedElement = null;

                if (_inspectorContainer != null)
                {
                    _inspectorContainer.Clear();

                    var headerBuilder = new GraphicalUserInterfaceBuilder("SynthHeader").WithMarginBottom(10);
                    var header = headerBuilder.Build();
                    header.Add(new Label($"SYNTHESIZING NEW ELEMENT (P:{period}, G:{group})")
                    {
                        style = { color = GuiSkin.Active.PrimaryAccent, unityFontStyleAndWeight = FontStyle.Bold }
                    });

                    _inspectorContainer.Add(header);

                    var builderGui = new Assets.Scripts.Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Physics.AtomBuilderGui();
                    _inspectorContainer.Add(builderGui.CreateGui(new GuiContext()));
                }
            });

            return tile;
        }

        private VisualElement BuildPeriodicGrid()
        {
            var gridBuilder = new ForgeGridGuiBuilder("PeriodicGrid", 9, 18)
                .WithRowModifier((rowBuilder, rowIndex) =>
                {
                    if (rowIndex == 7) rowBuilder.WithMarginTop(20);
                });

            for (int period = 1; period <= 9; period++)
            {
                for (int group = 1; group <= 18; group++)
                {
                    var element = _elementDatabase.FirstOrDefault(e => GetGridPosition(e.AtomicNumber) == (period, group));

                    VisualElement cellContent = (element.AtomicNumber != 0)
                        ? CreateElementTile(element)
                        : CreateEmptySlotTile(period, group);

                    // Corrected: Wraps the built VisualElement into the Provider ecosystem
                    gridBuilder.SetCell(period - 1, group - 1, new DynamicGuiProvider(cellContent));
                }
            }

            return gridBuilder.CreateGui(new GuiContext());
        }

        private VisualElement CreateElementTile(AtomicElement element)
        {
            Color catColor = GetCategoryColor(element.Category);
            var defaultStyle = GuiSkin.Active.GlobalDefault;

            // Blend the category color with the theme's background so it fits seamlessly 
            // into either a bright textbook or a dark hologram.
            Color blendedBg = Color.Lerp(defaultStyle.BackgroundColor, catColor, 0.2f);

            var tileBuilder = new GraphicalUserInterfaceBuilder($"Tile_{element.Symbol}")
                .WithWidth(65).WithHeight(85)
                .WithMarginRight(4)
                .WithBackgroundColor(blendedBg)
                .WithBorderWidth(2).WithBorderAllColor(catColor);

            var tile = tileBuilder.Build();
            tile.style.borderTopLeftRadius = defaultStyle.BorderTopLeftRadius;
            tile.style.borderBottomRightRadius = defaultStyle.BorderBottomRightRadius;

            var topRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, paddingLeft = 4, paddingRight = 4, paddingTop = 2 } };
            topRow.Add(new Label(element.AtomicNumber.ToString()) { style = { fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, color = defaultStyle.TextColor } });
            topRow.Add(new Label(element.Electronegativity > 0 ? element.Electronegativity.ToString("F2") : "") { style = { fontSize = 9, color = catColor } });
            tile.Add(topRow);

            tile.Add(new Label(element.Symbol) { style = { fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, color = defaultStyle.TextColor, unityTextAlign = TextAnchor.MiddleCenter, marginTop = 2 } });
            tile.Add(new Label(element.Name) { style = { fontSize = 9, color = defaultStyle.TextColor, opacity = 0.7f, unityTextAlign = TextAnchor.MiddleCenter, marginTop = -2 } });
            tile.Add(new Label(element.AtomicMass.ToString("F3")) { style = { fontSize = 9, color = defaultStyle.TextColor, opacity = 0.5f, unityTextAlign = TextAnchor.LowerCenter, position = Position.Absolute, bottom = 4, left = 0, right = 0 } });

            tile.RegisterCallback<ClickEvent>(evt =>
            {
                ScrubPreviewMemory();
                _inspectedElement = element;
                RefreshInspector();
            });

            tile.RegisterCallback<MouseEnterEvent>(evt => tile.style.backgroundColor = Color.Lerp(defaultStyle.BackgroundColor, catColor, 0.5f));
            tile.RegisterCallback<MouseLeaveEvent>(evt => tile.style.backgroundColor = blendedBg);

            return tile;
        }

        private void RefreshInspector()
        {
            if (_inspectorContainer == null) return;
            _inspectorContainer.Clear();

            var theme = GuiSkin.Active;
            var defaultStyle = theme.GlobalDefault;

            if (_inspectedElement == null)
            {
                _inspectorContainer.Add(new Label("Select an element to view thermodynamic and quantum properties.") { style = { color = defaultStyle.TextColor, opacity = 0.5f, unityFontStyleAndWeight = FontStyle.Italic, marginTop = 20 } });
                return;
            }

            var e = _inspectedElement.Value;
            Color catColor = GetCategoryColor(e.Category);

            // Large Display Card
            var heroCard = new VisualElement { style = { backgroundColor = Color.Lerp(defaultStyle.BackgroundColor, catColor, 0.1f), borderLeftWidth = 4, borderLeftColor = catColor, paddingTop = 15, paddingBottom = 15, paddingLeft = 15, paddingRight = 15, marginBottom = 20, borderTopRightRadius = defaultStyle.BorderTopRightRadius, borderBottomRightRadius = defaultStyle.BorderBottomRightRadius } };
            heroCard.Add(new Label($"{e.AtomicNumber} | {e.Symbol}") { style = { fontSize = 36, unityFontStyleAndWeight = FontStyle.Bold, color = defaultStyle.TextColor } });
            heroCard.Add(new Label(e.Name.ToUpper()) { style = { fontSize = 16, color = catColor, unityFontStyleAndWeight = FontStyle.Bold, letterSpacing = 2 } });
            heroCard.Add(new Label(FormatCategoryName(e.Category)) { style = { fontSize = 11, color = defaultStyle.TextColor, opacity = 0.6f, marginTop = 5 } });
            _inspectorContainer.Add(heroCard);

            // ---------------------------------------------------------
            // THE LIVE 3D PREVIEW
            // ---------------------------------------------------------
            _currentPreviewModel = GenerateBohrModel(e, theme);

            float dynamicZoom = Mathf.Clamp(3f + (e.AtomicNumber * 0.08f), 4f, 15f);

            _activePreviewBuilder = new LiveModelPreviewBuilder(_currentPreviewModel)
                 .WithBackgroundColor(defaultStyle.BackgroundColor)
                 .WithAutoRotate(true, 15f)
                 .WithMouseControl(true)
                 .WithZoom(dynamicZoom)
                 .WithControls(true)
                 .WithPreviewLifecycle(
                     onInit: (ghostModel, processGroup) =>
                     {
                         foreach (Transform child in ghostModel.transform)
                         {
                             if (child.name.StartsWith("Shell_"))
                             {
                                 var rotator = child.gameObject.AddComponent<OrbitalRotator>();
                                 rotator.Axis = Vector3.up;
                                 int shellIndex = int.Parse(child.name.Split('_')[1]);
                                 rotator.Speed = 50f - (shellIndex * 4f);
                             }
                         }
                     },
                     onDestroy: (ghostModel, processGroup) => { }
                 )
                 .WithGizmos(false);

            var previewContainer = new VisualElement { style = { height = 250, marginBottom = 15, borderTopLeftRadius = 4, borderTopRightRadius = 4, borderBottomLeftRadius = 4, borderBottomRightRadius = 4, overflow = Overflow.Hidden, borderBottomWidth = 1, borderBottomColor = catColor } };
            previewContainer.Add(_activePreviewBuilder.CreateGui(new GuiContext()));
            _inspectorContainer.Add(previewContainer);
            // ---------------------------------------------------------

            var dataBuilder = new GraphicalUserInterfaceBuilder("DataReadout")
                .AddHeader("MACROSCOPIC THERMODYNAMICS", defaultStyle.TextColor)
                .AddStringData("Melting Point", e.MeltingPointK > 0 ? $"{e.MeltingPointK} K" : "Unknown", null)
                .AddStringData("Boiling Point", e.BoilingPointK > 0 ? $"{e.BoilingPointK} K" : "Unknown", null)
                .AddStringData("Specific Heat", e.SpecificHeat > 0 ? $"{e.SpecificHeat} J/(kg·K)" : "Unknown", null)

                .AddHeader("ATOMIC PROPERTIES", defaultStyle.TextColor)
                .AddStringData("Atomic Mass", $"{e.AtomicMass} u", null)
                .AddStringData("Electronegativity", e.Electronegativity > 0 ? $"{e.Electronegativity} (Pauling)" : "None", null)

                .AddHeader("QUANTUM COUNTS", defaultStyle.TextColor)
                .AddStringData("Protons (Z)", e.AtomicNumber.ToString(), null)
                .AddStringData("Electrons", e.AtomicNumber.ToString(), null)
                .AddStringData("Neutrons", (Mathf.RoundToInt(e.AtomicMass) - e.AtomicNumber).ToString(), null);

            _inspectorContainer.Add(dataBuilder.Build());
        }

        private GameObject GenerateBohrModel(AtomicElement e, GuiTheme theme)
        {
            GameObject root = new GameObject($"AtomModel_{e.Symbol}");
            root.SetActive(false);

            int protons = e.AtomicNumber;
            int neutrons = Mathf.RoundToInt(e.AtomicMass) - protons;

            // Determine rendering style based on background brightness
            float bgLuminance = theme.GlobalDefault.BackgroundColor.r * 0.3f + theme.GlobalDefault.BackgroundColor.g * 0.59f + theme.GlobalDefault.BackgroundColor.b * 0.11f;
            bool isLightTheme = bgLuminance > 0.5f;

            // Use standard shader to allow flat colors, ensuring shadow casting is disabled to prevent the "black plane" artifact
            Material CreateParticleMaterial(Color color)
            {
                // Dynamic fallback sequence to prevent the "Magenta Error Shader" across different Render Pipelines (URP/HDRP/Built-In)
                Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                             ?? Shader.Find("Standard")
                             ?? Shader.Find("Unlit/Color");

                var mat = new Material(shader);
                mat.color = color;

                // URP Compatibility mapping
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", color);
                }

                if (!isLightTheme)
                {
                    mat.EnableKeyword("_EMISSION");
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        mat.SetColor("_EmissionColor", color * 0.5f);
                    }
                }
                return mat;
            }

            Material protonMat = CreateParticleMaterial(Color.red);
            Material neutronMat = CreateParticleMaterial(Color.blue);
            Material electronMat = CreateParticleMaterial(Color.yellow);

            float nucleusRadius = Mathf.Pow(protons + neutrons, 1f / 3f) * 0.12f;

            // Build Nucleus
            for (int i = 0; i < protons + neutrons; i++)
            {
                GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                UnityEngine.Object.DestroyImmediate(particle.GetComponent<Collider>());

                var renderer = particle.GetComponent<Renderer>();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.sharedMaterial = i < protons ? protonMat : neutronMat;

                particle.transform.SetParent(root.transform);
                particle.transform.localPosition = UnityEngine.Random.insideUnitSphere * nucleusRadius;
                particle.transform.localScale = Vector3.one * 0.25f;
            }

            int[] shellCapacities = { 2, 8, 18, 32, 32, 18, 8 };
            int remainingElectrons = protons;
            float shellRadius = nucleusRadius + 0.8f;

            // Build Shells
            for (int shell = 0; shell < shellCapacities.Length && remainingElectrons > 0; shell++)
            {
                GameObject shellRoot = new GameObject($"Shell_{shell}");
                shellRoot.transform.SetParent(root.transform);
                shellRoot.transform.localPosition = Vector3.zero;

                int electronsInShell = Mathf.Min(remainingElectrons, shellCapacities[shell]);
                for (int eIdx = 0; eIdx < electronsInShell; eIdx++)
                {
                    float angle = (eIdx * Mathf.PI * 2f) / electronsInShell;
                    float yOffset = (shell % 2 == 0) ? Mathf.Sin(angle) * 0.5f : Mathf.Cos(angle) * -0.5f;
                    Vector3 pos = new Vector3(Mathf.Cos(angle) * shellRadius, yOffset, Mathf.Sin(angle) * shellRadius);

                    GameObject electron = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.DestroyImmediate(electron.GetComponent<Collider>());

                    var renderer = electron.GetComponent<Renderer>();
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    renderer.sharedMaterial = electronMat;

                    electron.transform.SetParent(shellRoot.transform);
                    electron.transform.localPosition = pos;
                    electron.transform.localScale = Vector3.one * 0.15f;
                }
                shellRadius += 0.6f;
                remainingElectrons -= electronsInShell;
            }

            return root;
        }

        // ====================================================================
        // THE MATH: Maps Atomic Number to a specific (Row, Column) Coordinate
        // ====================================================================
        private (int period, int group) GetGridPosition(int z)
        {
            if (z == 1) return (1, 1);
            if (z == 2) return (1, 18);
            if (z >= 3 && z <= 4) return (2, z - 2);
            if (z >= 5 && z <= 10) return (2, z + 8);
            if (z >= 11 && z <= 12) return (3, z - 10);
            if (z >= 13 && z <= 18) return (3, z + 0);
            if (z >= 19 && z <= 36) return (4, z - 18);
            if (z >= 37 && z <= 54) return (5, z - 36);
            if (z >= 57 && z <= 71) return (8, z - 53);
            if (z >= 55 && z <= 56) return (6, z - 54);
            if (z >= 72 && z <= 86) return (6, z - 68);
            if (z >= 89 && z <= 103) return (9, z - 85);
            if (z >= 87 && z <= 88) return (7, z - 86);
            if (z >= 104 && z <= 118) return (7, z - 100);

            return (0, 0);
        }

        private Color GetCategoryColor(ElementCategory cat)
        {
            // Base hues derived algorithmically from the theme's accents to ensure they match ANY theme.
            Color baseColor;
            Color primary = GuiSkin.Active.PrimaryAccent;
            float h, s, v;
            Color.RGBToHSV(primary, out h, out s, out v);

            baseColor = cat switch
            {
                ElementCategory.Alkali => Color.HSVToRGB((h + 0.5f) % 1f, 0.7f, 0.9f),
                ElementCategory.AlkalineEarth => Color.HSVToRGB((h + 0.1f) % 1f, 0.8f, 0.9f),
                ElementCategory.TransitionMetal => Color.HSVToRGB((h + 0.15f) % 1f, 0.6f, 0.8f),
                ElementCategory.PostTransition => Color.HSVToRGB((h + 0.3f) % 1f, 0.6f, 0.8f),
                ElementCategory.Metalloid => Color.HSVToRGB((h + 0.4f) % 1f, 0.7f, 0.7f),
                ElementCategory.NonMetal => Color.HSVToRGB(h, 0.6f, 0.9f), // Closest to primary
                ElementCategory.Halogen => Color.HSVToRGB((h + 0.7f) % 1f, 0.7f, 0.8f),
                ElementCategory.NobleGas => Color.HSVToRGB((h + 0.8f) % 1f, 0.5f, 0.9f),
                ElementCategory.Actinide => Color.HSVToRGB((h + 0.9f) % 1f, 0.6f, 0.8f),
                _ => Color.gray
            };

            // If it's a light theme, darken the vibrant colors so they don't blind the user on white pages
            float bgLuminance = GuiSkin.Active.GlobalDefault.BackgroundColor.r * 0.3f + GuiSkin.Active.GlobalDefault.BackgroundColor.g * 0.59f + GuiSkin.Active.GlobalDefault.BackgroundColor.b * 0.11f;
            if (bgLuminance > 0.5f)
            {
                return Color.Lerp(baseColor, Color.black, 0.25f);
            }

            return baseColor;
        }

        private string FormatCategoryName(ElementCategory cat)
        {
            return Regex.Replace(cat.ToString(), "([A-Z])", " $1").Trim();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }

    public static class ElementData
    {
        public static List<AtomicElement> GetElements()
        {
            return new List<AtomicElement>
            {
                // --- PERIOD 1 ---
                new AtomicElement { AtomicNumber = 1, Symbol = "H", Name = "Hydrogen", AtomicMass = 1.008f, Electronegativity = 2.20f, MeltingPointK = 13.99f, BoilingPointK = 20.27f, SpecificHeat = 14300, Category = ElementCategory.NonMetal },
                new AtomicElement { AtomicNumber = 2, Symbol = "He", Name = "Helium", AtomicMass = 4.0026f, Electronegativity = 0f, MeltingPointK = 0.95f, BoilingPointK = 4.22f, SpecificHeat = 5193, Category = ElementCategory.NobleGas },

                // --- PERIOD 2 ---
                new AtomicElement { AtomicNumber = 3, Symbol = "Li", Name = "Lithium", AtomicMass = 6.94f, Electronegativity = 0.98f, MeltingPointK = 453.65f, BoilingPointK = 1603f, SpecificHeat = 3582, Category = ElementCategory.Alkali },
                new AtomicElement { AtomicNumber = 4, Symbol = "Be", Name = "Beryllium", AtomicMass = 9.012f, Electronegativity = 1.57f, MeltingPointK = 1560f, BoilingPointK = 2742f, SpecificHeat = 1825, Category = ElementCategory.AlkalineEarth },
                new AtomicElement { AtomicNumber = 5, Symbol = "B", Name = "Boron", AtomicMass = 10.81f, Electronegativity = 2.04f, MeltingPointK = 2349f, BoilingPointK = 4200f, SpecificHeat = 1026, Category = ElementCategory.Metalloid },
                new AtomicElement { AtomicNumber = 6, Symbol = "C", Name = "Carbon", AtomicMass = 12.011f, Electronegativity = 2.55f, MeltingPointK = 3800f, BoilingPointK = 4300f, SpecificHeat = 710, Category = ElementCategory.NonMetal },
                new AtomicElement { AtomicNumber = 7, Symbol = "N", Name = "Nitrogen", AtomicMass = 14.007f, Electronegativity = 3.04f, MeltingPointK = 63.15f, BoilingPointK = 77.36f, SpecificHeat = 1040, Category = ElementCategory.NonMetal },
                new AtomicElement { AtomicNumber = 8, Symbol = "O", Name = "Oxygen", AtomicMass = 15.999f, Electronegativity = 3.44f, MeltingPointK = 54.36f, BoilingPointK = 90.18f, SpecificHeat = 918, Category = ElementCategory.NonMetal },
                new AtomicElement { AtomicNumber = 9, Symbol = "F", Name = "Fluorine", AtomicMass = 18.998f, Electronegativity = 3.98f, MeltingPointK = 53.48f, BoilingPointK = 85.03f, SpecificHeat = 824, Category = ElementCategory.Halogen },
                new AtomicElement { AtomicNumber = 10, Symbol = "Ne", Name = "Neon", AtomicMass = 20.180f, Electronegativity = 0f, MeltingPointK = 24.56f, BoilingPointK = 27.10f, SpecificHeat = 1030, Category = ElementCategory.NobleGas },

                // --- PERIOD 3 ---
                new AtomicElement { AtomicNumber = 11, Symbol = "Na", Name = "Sodium", AtomicMass = 22.990f, Electronegativity = 0.93f, MeltingPointK = 370.94f, BoilingPointK = 1156.09f, SpecificHeat = 1228, Category = ElementCategory.Alkali },
                new AtomicElement { AtomicNumber = 12, Symbol = "Mg", Name = "Magnesium", AtomicMass = 24.305f, Electronegativity = 1.31f, MeltingPointK = 923f, BoilingPointK = 1363f, SpecificHeat = 1023, Category = ElementCategory.AlkalineEarth },
                new AtomicElement { AtomicNumber = 13, Symbol = "Al", Name = "Aluminum", AtomicMass = 26.982f, Electronegativity = 1.61f, MeltingPointK = 933.47f, BoilingPointK = 2743f, SpecificHeat = 897, Category = ElementCategory.PostTransition },
                new AtomicElement { AtomicNumber = 14, Symbol = "Si", Name = "Silicon", AtomicMass = 28.085f, Electronegativity = 1.90f, MeltingPointK = 1687f, BoilingPointK = 3538f, SpecificHeat = 705, Category = ElementCategory.Metalloid },
                new AtomicElement { AtomicNumber = 15, Symbol = "P", Name = "Phosphorus", AtomicMass = 30.974f, Electronegativity = 2.19f, MeltingPointK = 317.3f, BoilingPointK = 553.7f, SpecificHeat = 769, Category = ElementCategory.NonMetal },
                new AtomicElement { AtomicNumber = 16, Symbol = "S", Name = "Sulfur", AtomicMass = 32.06f, Electronegativity = 2.58f, MeltingPointK = 388.36f, BoilingPointK = 717.8f, SpecificHeat = 710, Category = ElementCategory.NonMetal },
                new AtomicElement { AtomicNumber = 17, Symbol = "Cl", Name = "Chlorine", AtomicMass = 35.45f, Electronegativity = 3.16f, MeltingPointK = 171.6f, BoilingPointK = 239.11f, SpecificHeat = 479, Category = ElementCategory.Halogen },
                new AtomicElement { AtomicNumber = 18, Symbol = "Ar", Name = "Argon", AtomicMass = 39.95f, Electronegativity = 0f, MeltingPointK = 83.81f, BoilingPointK = 87.30f, SpecificHeat = 520, Category = ElementCategory.NobleGas },

                // --- PERIOD 4 ---
                new AtomicElement { AtomicNumber = 19, Symbol = "K", Name = "Potassium", AtomicMass = 39.098f, Electronegativity = 0.82f, MeltingPointK = 336.7f, BoilingPointK = 1032f, SpecificHeat = 757, Category = ElementCategory.Alkali },
                new AtomicElement { AtomicNumber = 20, Symbol = "Ca", Name = "Calcium", AtomicMass = 40.078f, Electronegativity = 1.00f, MeltingPointK = 1115f, BoilingPointK = 1757f, SpecificHeat = 647, Category = ElementCategory.AlkalineEarth },
                new AtomicElement { AtomicNumber = 21, Symbol = "Sc", Name = "Scandium", AtomicMass = 44.956f, Electronegativity = 1.36f, MeltingPointK = 1814f, BoilingPointK = 3109f, SpecificHeat = 568, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 22, Symbol = "Ti", Name = "Titanium", AtomicMass = 47.867f, Electronegativity = 1.54f, MeltingPointK = 1941f, BoilingPointK = 3560f, SpecificHeat = 520, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 23, Symbol = "V", Name = "Vanadium", AtomicMass = 50.942f, Electronegativity = 1.63f, MeltingPointK = 2183f, BoilingPointK = 3680f, SpecificHeat = 489, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 24, Symbol = "Cr", Name = "Chromium", AtomicMass = 51.996f, Electronegativity = 1.66f, MeltingPointK = 2180f, BoilingPointK = 2944f, SpecificHeat = 449, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 25, Symbol = "Mn", Name = "Manganese", AtomicMass = 54.938f, Electronegativity = 1.55f, MeltingPointK = 1519f, BoilingPointK = 2334f, SpecificHeat = 479, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 26, Symbol = "Fe", Name = "Iron", AtomicMass = 55.845f, Electronegativity = 1.83f, MeltingPointK = 1811f, BoilingPointK = 3134f, SpecificHeat = 449, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 27, Symbol = "Co", Name = "Cobalt", AtomicMass = 58.933f, Electronegativity = 1.88f, MeltingPointK = 1768f, BoilingPointK = 3200f, SpecificHeat = 421, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 28, Symbol = "Ni", Name = "Nickel", AtomicMass = 58.693f, Electronegativity = 1.91f, MeltingPointK = 1728f, BoilingPointK = 3003f, SpecificHeat = 444, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 29, Symbol = "Cu", Name = "Copper", AtomicMass = 63.546f, Electronegativity = 1.90f, MeltingPointK = 1357.77f, BoilingPointK = 2835f, SpecificHeat = 384, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 30, Symbol = "Zn", Name = "Zinc", AtomicMass = 65.38f, Electronegativity = 1.65f, MeltingPointK = 692.68f, BoilingPointK = 1180f, SpecificHeat = 390, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 31, Symbol = "Ga", Name = "Gallium", AtomicMass = 69.723f, Electronegativity = 1.81f, MeltingPointK = 302.91f, BoilingPointK = 2673f, SpecificHeat = 371, Category = ElementCategory.PostTransition },
                new AtomicElement { AtomicNumber = 32, Symbol = "Ge", Name = "Germanium", AtomicMass = 72.630f, Electronegativity = 2.01f, MeltingPointK = 1211.4f, BoilingPointK = 3106f, SpecificHeat = 320, Category = ElementCategory.Metalloid },
                new AtomicElement { AtomicNumber = 33, Symbol = "As", Name = "Arsenic", AtomicMass = 74.922f, Electronegativity = 2.18f, MeltingPointK = 1090f, BoilingPointK = 887f, SpecificHeat = 328, Category = ElementCategory.Metalloid },
                new AtomicElement { AtomicNumber = 34, Symbol = "Se", Name = "Selenium", AtomicMass = 78.971f, Electronegativity = 2.55f, MeltingPointK = 494f, BoilingPointK = 958f, SpecificHeat = 321, Category = ElementCategory.NonMetal },
                new AtomicElement { AtomicNumber = 35, Symbol = "Br", Name = "Bromine", AtomicMass = 79.904f, Electronegativity = 2.96f, MeltingPointK = 265.8f, BoilingPointK = 332.0f, SpecificHeat = 474, Category = ElementCategory.Halogen },
                new AtomicElement { AtomicNumber = 36, Symbol = "Kr", Name = "Krypton", AtomicMass = 83.798f, Electronegativity = 3.00f, MeltingPointK = 115.78f, BoilingPointK = 119.93f, SpecificHeat = 248, Category = ElementCategory.NobleGas },

                // --- PERIOD 5 ---
                new AtomicElement { AtomicNumber = 37, Symbol = "Rb", Name = "Rubidium", AtomicMass = 85.468f, Electronegativity = 0.82f, MeltingPointK = 312.45f, BoilingPointK = 961f, SpecificHeat = 363, Category = ElementCategory.Alkali },
                new AtomicElement { AtomicNumber = 38, Symbol = "Sr", Name = "Strontium", AtomicMass = 87.62f, Electronegativity = 0.95f, MeltingPointK = 1050f, BoilingPointK = 1650f, SpecificHeat = 300, Category = ElementCategory.AlkalineEarth },
                new AtomicElement { AtomicNumber = 39, Symbol = "Y", Name = "Yttrium", AtomicMass = 88.906f, Electronegativity = 1.22f, MeltingPointK = 1799f, BoilingPointK = 3203f, SpecificHeat = 298, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 40, Symbol = "Zr", Name = "Zirconium", AtomicMass = 91.224f, Electronegativity = 1.33f, MeltingPointK = 2128f, BoilingPointK = 4682f, SpecificHeat = 278, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 41, Symbol = "Nb", Name = "Niobium", AtomicMass = 92.906f, Electronegativity = 1.6f, MeltingPointK = 2750f, BoilingPointK = 5017f, SpecificHeat = 265, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 42, Symbol = "Mo", Name = "Molybdenum", AtomicMass = 95.95f, Electronegativity = 2.16f, MeltingPointK = 2896f, BoilingPointK = 4912f, SpecificHeat = 251, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 43, Symbol = "Tc", Name = "Technetium", AtomicMass = 98f, Electronegativity = 1.9f, MeltingPointK = 2430f, BoilingPointK = 4538f, SpecificHeat = 210, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 44, Symbol = "Ru", Name = "Ruthenium", AtomicMass = 101.07f, Electronegativity = 2.2f, MeltingPointK = 2607f, BoilingPointK = 4423f, SpecificHeat = 238, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 45, Symbol = "Rh", Name = "Rhodium", AtomicMass = 102.91f, Electronegativity = 2.28f, MeltingPointK = 2237f, BoilingPointK = 3968f, SpecificHeat = 243, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 46, Symbol = "Pd", Name = "Palladium", AtomicMass = 106.42f, Electronegativity = 2.2f, MeltingPointK = 1828.05f, BoilingPointK = 3236f, SpecificHeat = 244, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 47, Symbol = "Ag", Name = "Silver", AtomicMass = 107.87f, Electronegativity = 1.93f, MeltingPointK = 1234.93f, BoilingPointK = 2435f, SpecificHeat = 235, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 48, Symbol = "Cd", Name = "Cadmium", AtomicMass = 112.41f, Electronegativity = 1.69f, MeltingPointK = 594.22f, BoilingPointK = 1040f, SpecificHeat = 231, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 49, Symbol = "In", Name = "Indium", AtomicMass = 114.82f, Electronegativity = 1.78f, MeltingPointK = 429.748f, BoilingPointK = 2345f, SpecificHeat = 233, Category = ElementCategory.PostTransition },
                new AtomicElement { AtomicNumber = 50, Symbol = "Sn", Name = "Tin", AtomicMass = 118.71f, Electronegativity = 1.96f, MeltingPointK = 505.08f, BoilingPointK = 2875f, SpecificHeat = 228, Category = ElementCategory.PostTransition },
                new AtomicElement { AtomicNumber = 51, Symbol = "Sb", Name = "Antimony", AtomicMass = 121.76f, Electronegativity = 2.05f, MeltingPointK = 903.78f, BoilingPointK = 1908f, SpecificHeat = 207, Category = ElementCategory.Metalloid },
                new AtomicElement { AtomicNumber = 52, Symbol = "Te", Name = "Tellurium", AtomicMass = 127.60f, Electronegativity = 2.1f, MeltingPointK = 722.66f, BoilingPointK = 1261f, SpecificHeat = 202, Category = ElementCategory.Metalloid },
                new AtomicElement { AtomicNumber = 53, Symbol = "I", Name = "Iodine", AtomicMass = 126.90f, Electronegativity = 2.66f, MeltingPointK = 386.85f, BoilingPointK = 457.4f, SpecificHeat = 214, Category = ElementCategory.Halogen },
                new AtomicElement { AtomicNumber = 54, Symbol = "Xe", Name = "Xenon", AtomicMass = 131.29f, Electronegativity = 2.6f, MeltingPointK = 161.40f, BoilingPointK = 165.05f, SpecificHeat = 158, Category = ElementCategory.NobleGas },

                // --- PERIOD 6 ---
                new AtomicElement { AtomicNumber = 55, Symbol = "Cs", Name = "Cesium", AtomicMass = 132.91f, Electronegativity = 0.79f, MeltingPointK = 301.7f, BoilingPointK = 944f, SpecificHeat = 240, Category = ElementCategory.Alkali },
                new AtomicElement { AtomicNumber = 56, Symbol = "Ba", Name = "Barium", AtomicMass = 137.33f, Electronegativity = 0.89f, MeltingPointK = 1000f, BoilingPointK = 2118f, SpecificHeat = 204, Category = ElementCategory.AlkalineEarth },
                
                // Lanthanides
                new AtomicElement { AtomicNumber = 57, Symbol = "La", Name = "Lanthanum", AtomicMass = 138.91f, Electronegativity = 1.1f, MeltingPointK = 1193f, BoilingPointK = 3737f, SpecificHeat = 195, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 58, Symbol = "Ce", Name = "Cerium", AtomicMass = 140.12f, Electronegativity = 1.12f, MeltingPointK = 1068f, BoilingPointK = 3716f, SpecificHeat = 192, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 59, Symbol = "Pr", Name = "Praseodymium", AtomicMass = 140.91f, Electronegativity = 1.13f, MeltingPointK = 1208f, BoilingPointK = 3403f, SpecificHeat = 193, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 60, Symbol = "Nd", Name = "Neodymium", AtomicMass = 144.24f, Electronegativity = 1.14f, MeltingPointK = 1297f, BoilingPointK = 3347f, SpecificHeat = 190, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 61, Symbol = "Pm", Name = "Promethium", AtomicMass = 145f, Electronegativity = 1.13f, MeltingPointK = 1315f, BoilingPointK = 3273f, SpecificHeat = 0f, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 62, Symbol = "Sm", Name = "Samarium", AtomicMass = 150.36f, Electronegativity = 1.17f, MeltingPointK = 1345f, BoilingPointK = 2173f, SpecificHeat = 197, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 63, Symbol = "Eu", Name = "Europium", AtomicMass = 151.96f, Electronegativity = 1.2f, MeltingPointK = 1099f, BoilingPointK = 1802f, SpecificHeat = 182, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 64, Symbol = "Gd", Name = "Gadolinium", AtomicMass = 157.25f, Electronegativity = 1.2f, MeltingPointK = 1585f, BoilingPointK = 3273f, SpecificHeat = 236, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 65, Symbol = "Tb", Name = "Terbium", AtomicMass = 158.93f, Electronegativity = 1.2f, MeltingPointK = 1629f, BoilingPointK = 3396f, SpecificHeat = 182, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 66, Symbol = "Dy", Name = "Dysprosium", AtomicMass = 162.50f, Electronegativity = 1.22f, MeltingPointK = 1680f, BoilingPointK = 2840f, SpecificHeat = 170, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 67, Symbol = "Ho", Name = "Holmium", AtomicMass = 164.93f, Electronegativity = 1.23f, MeltingPointK = 1734f, BoilingPointK = 2873f, SpecificHeat = 165, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 68, Symbol = "Er", Name = "Erbium", AtomicMass = 167.26f, Electronegativity = 1.24f, MeltingPointK = 1802f, BoilingPointK = 3141f, SpecificHeat = 168, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 69, Symbol = "Tm", Name = "Thulium", AtomicMass = 168.93f, Electronegativity = 1.25f, MeltingPointK = 1818f, BoilingPointK = 2223f, SpecificHeat = 160, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 70, Symbol = "Yb", Name = "Ytterbium", AtomicMass = 173.05f, Electronegativity = 1.1f, MeltingPointK = 1097f, BoilingPointK = 1469f, SpecificHeat = 155, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 71, Symbol = "Lu", Name = "Lutetium", AtomicMass = 174.97f, Electronegativity = 1.27f, MeltingPointK = 1925f, BoilingPointK = 3675f, SpecificHeat = 154, Category = ElementCategory.TransitionMetal },

                // Post-Lanthanide Transition Metals
                new AtomicElement { AtomicNumber = 72, Symbol = "Hf", Name = "Hafnium", AtomicMass = 178.49f, Electronegativity = 1.3f, MeltingPointK = 2506f, BoilingPointK = 4876f, SpecificHeat = 144, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 73, Symbol = "Ta", Name = "Tantalum", AtomicMass = 180.95f, Electronegativity = 1.5f, MeltingPointK = 3290f, BoilingPointK = 5731f, SpecificHeat = 140, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 74, Symbol = "W", Name = "Tungsten", AtomicMass = 183.84f, Electronegativity = 2.36f, MeltingPointK = 3695f, BoilingPointK = 6203f, SpecificHeat = 132, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 75, Symbol = "Re", Name = "Rhenium", AtomicMass = 186.21f, Electronegativity = 1.9f, MeltingPointK = 3459f, BoilingPointK = 5903f, SpecificHeat = 137, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 76, Symbol = "Os", Name = "Osmium", AtomicMass = 190.23f, Electronegativity = 2.2f, MeltingPointK = 3306f, BoilingPointK = 5285f, SpecificHeat = 130, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 77, Symbol = "Ir", Name = "Iridium", AtomicMass = 192.22f, Electronegativity = 2.2f, MeltingPointK = 2719f, BoilingPointK = 4701f, SpecificHeat = 131, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 78, Symbol = "Pt", Name = "Platinum", AtomicMass = 195.08f, Electronegativity = 2.28f, MeltingPointK = 2041.4f, BoilingPointK = 4098f, SpecificHeat = 133, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 79, Symbol = "Au", Name = "Gold", AtomicMass = 196.97f, Electronegativity = 2.54f, MeltingPointK = 1337.33f, BoilingPointK = 3243f, SpecificHeat = 129, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 80, Symbol = "Hg", Name = "Mercury", AtomicMass = 200.59f, Electronegativity = 2.0f, MeltingPointK = 234.32f, BoilingPointK = 629.88f, SpecificHeat = 140, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 81, Symbol = "Tl", Name = "Thallium", AtomicMass = 204.38f, Electronegativity = 1.62f, MeltingPointK = 577f, BoilingPointK = 1746f, SpecificHeat = 129, Category = ElementCategory.PostTransition },
                new AtomicElement { AtomicNumber = 82, Symbol = "Pb", Name = "Lead", AtomicMass = 207.2f, Electronegativity = 2.33f, MeltingPointK = 600.61f, BoilingPointK = 2022f, SpecificHeat = 129, Category = ElementCategory.PostTransition },
                new AtomicElement { AtomicNumber = 83, Symbol = "Bi", Name = "Bismuth", AtomicMass = 208.98f, Electronegativity = 2.02f, MeltingPointK = 544.7f, BoilingPointK = 1837f, SpecificHeat = 122, Category = ElementCategory.PostTransition },
                new AtomicElement { AtomicNumber = 84, Symbol = "Po", Name = "Polonium", AtomicMass = 209f, Electronegativity = 2.0f, MeltingPointK = 527f, BoilingPointK = 1235f, SpecificHeat = 0f, Category = ElementCategory.Metalloid },
                new AtomicElement { AtomicNumber = 85, Symbol = "At", Name = "Astatine", AtomicMass = 210f, Electronegativity = 2.2f, MeltingPointK = 575f, BoilingPointK = 610f, SpecificHeat = 0f, Category = ElementCategory.Halogen },
                new AtomicElement { AtomicNumber = 86, Symbol = "Rn", Name = "Radon", AtomicMass = 222f, Electronegativity = 2.2f, MeltingPointK = 202f, BoilingPointK = 211.3f, SpecificHeat = 94, Category = ElementCategory.NobleGas },

                // --- PERIOD 7 ---
                new AtomicElement { AtomicNumber = 87, Symbol = "Fr", Name = "Francium", AtomicMass = 223f, Electronegativity = 0.7f, MeltingPointK = 300f, BoilingPointK = 950f, SpecificHeat = 0f, Category = ElementCategory.Alkali },
                new AtomicElement { AtomicNumber = 88, Symbol = "Ra", Name = "Radium", AtomicMass = 226f, Electronegativity = 0.9f, MeltingPointK = 973f, BoilingPointK = 2010f, SpecificHeat = 94, Category = ElementCategory.AlkalineEarth },
                
                // Actinides
                new AtomicElement { AtomicNumber = 89, Symbol = "Ac", Name = "Actinium", AtomicMass = 227f, Electronegativity = 1.1f, MeltingPointK = 1323f, BoilingPointK = 3471f, SpecificHeat = 120, Category = ElementCategory.Actinide },
                new AtomicElement { AtomicNumber = 90, Symbol = "Th", Name = "Thorium", AtomicMass = 232.04f, Electronegativity = 1.3f, MeltingPointK = 2023f, BoilingPointK = 5061f, SpecificHeat = 113, Category = ElementCategory.Actinide },
                new AtomicElement { AtomicNumber = 91, Symbol = "Pa", Name = "Protactinium", AtomicMass = 231.04f, Electronegativity = 1.5f, MeltingPointK = 1841f, BoilingPointK = 4300f, SpecificHeat = 0f, Category = ElementCategory.Actinide },
                new AtomicElement { AtomicNumber = 92, Symbol = "U", Name = "Uranium", AtomicMass = 238.03f, Electronegativity = 1.38f, MeltingPointK = 1405.3f, BoilingPointK = 4404f, SpecificHeat = 116, Category = ElementCategory.Actinide },
                new AtomicElement { AtomicNumber = 93, Symbol = "Np", Name = "Neptunium", AtomicMass = 237f, Electronegativity = 1.36f, MeltingPointK = 912f, BoilingPointK = 4273f, SpecificHeat = 0f, Category = ElementCategory.Actinide },
                new AtomicElement { AtomicNumber = 94, Symbol = "Pu", Name = "Plutonium", AtomicMass = 244f, Electronegativity = 1.28f, MeltingPointK = 912.5f, BoilingPointK = 3505f, SpecificHeat = 130, Category = ElementCategory.Actinide },
                new AtomicElement { AtomicNumber = 95, Symbol = "Am", Name = "Americium", AtomicMass = 243f, Electronegativity = 1.13f, MeltingPointK = 1449f, BoilingPointK = 2880f, SpecificHeat = 0f, Category = ElementCategory.Actinide },
                new AtomicElement { AtomicNumber = 96, Symbol = "Cm", Name = "Curium", AtomicMass = 247f, Electronegativity = 1.28f, MeltingPointK = 1613f, BoilingPointK = 3383f, SpecificHeat = 0f, Category = ElementCategory.Actinide },
                new AtomicElement { AtomicNumber = 97, Symbol = "Bk", Name = "Berkelium", AtomicMass = 247f, Electronegativity = 1.3f, MeltingPointK = 1259f, BoilingPointK = 2900f, SpecificHeat = 0f, Category = ElementCategory.Actinide },
                new AtomicElement { AtomicNumber = 98, Symbol = "Cf", Name = "Californium", AtomicMass = 251f, Electronegativity = 1.3f, MeltingPointK = 1173f, BoilingPointK = 1745f, SpecificHeat = 0f, Category = ElementCategory.Actinide },
                new AtomicElement { AtomicNumber = 99, Symbol = "Es", Name = "Einsteinium", AtomicMass = 252f, Electronegativity = 1.3f, MeltingPointK = 1133f, BoilingPointK = 1269f, SpecificHeat = 0f, Category = ElementCategory.Actinide },
                new AtomicElement { AtomicNumber = 100, Symbol = "Fm", Name = "Fermium", AtomicMass = 257f, Electronegativity = 1.3f, MeltingPointK = 1800f, BoilingPointK = 0f, SpecificHeat = 0f, Category = ElementCategory.Actinide },
                new AtomicElement { AtomicNumber = 101, Symbol = "Md", Name = "Mendelevium", AtomicMass = 258f, Electronegativity = 1.3f, MeltingPointK = 1100f, BoilingPointK = 0f, SpecificHeat = 0f, Category = ElementCategory.Actinide },
                new AtomicElement { AtomicNumber = 102, Symbol = "No", Name = "Nobelium", AtomicMass = 259f, Electronegativity = 1.3f, MeltingPointK = 1100f, BoilingPointK = 0f, SpecificHeat = 0f, Category = ElementCategory.Actinide },
                new AtomicElement { AtomicNumber = 103, Symbol = "Lr", Name = "Lawrencium", AtomicMass = 266f, Electronegativity = 1.3f, MeltingPointK = 1900f, BoilingPointK = 0f, SpecificHeat = 0f, Category = ElementCategory.Actinide },

                // Superheavy / Transactinides
                new AtomicElement { AtomicNumber = 104, Symbol = "Rf", Name = "Rutherfordium", AtomicMass = 267f, Electronegativity = 0f, MeltingPointK = 2400f, BoilingPointK = 5800f, SpecificHeat = 0f, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 105, Symbol = "Db", Name = "Dubnium", AtomicMass = 268f, Electronegativity = 0f, MeltingPointK = 0f, BoilingPointK = 0f, SpecificHeat = 0f, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 106, Symbol = "Sg", Name = "Seaborgium", AtomicMass = 269f, Electronegativity = 0f, MeltingPointK = 0f, BoilingPointK = 0f, SpecificHeat = 0f, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 107, Symbol = "Bh", Name = "Bohrium", AtomicMass = 270f, Electronegativity = 0f, MeltingPointK = 0f, BoilingPointK = 0f, SpecificHeat = 0f, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 108, Symbol = "Hs", Name = "Hassium", AtomicMass = 277f, Electronegativity = 0f, MeltingPointK = 0f, BoilingPointK = 0f, SpecificHeat = 0f, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 109, Symbol = "Mt", Name = "Meitnerium", AtomicMass = 278f, Electronegativity = 0f, MeltingPointK = 0f, BoilingPointK = 0f, SpecificHeat = 0f, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 110, Symbol = "Ds", Name = "Darmstadtium", AtomicMass = 281f, Electronegativity = 0f, MeltingPointK = 0f, BoilingPointK = 0f, SpecificHeat = 0f, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 111, Symbol = "Rg", Name = "Roentgenium", AtomicMass = 282f, Electronegativity = 0f, MeltingPointK = 0f, BoilingPointK = 0f, SpecificHeat = 0f, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 112, Symbol = "Cn", Name = "Copernicium", AtomicMass = 285f, Electronegativity = 0f, MeltingPointK = 0f, BoilingPointK = 340f, SpecificHeat = 0f, Category = ElementCategory.TransitionMetal },
                new AtomicElement { AtomicNumber = 113, Symbol = "Nh", Name = "Nihonium", AtomicMass = 286f, Electronegativity = 0f, MeltingPointK = 700f, BoilingPointK = 1400f, SpecificHeat = 0f, Category = ElementCategory.PostTransition },
                new AtomicElement { AtomicNumber = 114, Symbol = "Fl", Name = "Flerovium", AtomicMass = 289f, Electronegativity = 0f, MeltingPointK = 340f, BoilingPointK = 420f, SpecificHeat = 0f, Category = ElementCategory.PostTransition },
                new AtomicElement { AtomicNumber = 115, Symbol = "Mc", Name = "Moscovium", AtomicMass = 290f, Electronegativity = 0f, MeltingPointK = 670f, BoilingPointK = 1400f, SpecificHeat = 0f, Category = ElementCategory.PostTransition },
                new AtomicElement { AtomicNumber = 116, Symbol = "Lv", Name = "Livermorium", AtomicMass = 293f, Electronegativity = 0f, MeltingPointK = 709f, BoilingPointK = 1085f, SpecificHeat = 0f, Category = ElementCategory.PostTransition },
                new AtomicElement { AtomicNumber = 117, Symbol = "Ts", Name = "Tennessine", AtomicMass = 294f, Electronegativity = 0f, MeltingPointK = 673f, BoilingPointK = 883f, SpecificHeat = 0f, Category = ElementCategory.Halogen },
                new AtomicElement { AtomicNumber = 118, Symbol = "Og", Name = "Oganesson", AtomicMass = 294f, Electronegativity = 0f, MeltingPointK = 325f, BoilingPointK = 350f, SpecificHeat = 0f, Category = ElementCategory.NobleGas }
            };
        }
    }
}
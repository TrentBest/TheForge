using System;
using System.Collections.Generic;
using System.Linq;
using TheSingularityWorkshop.Core.Physics.Chemistry;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders.Materials
{
    /// <summary>
    /// Refactored Material Forge.
    /// Utilizes the Forge Builder ecosystem to provide a high-fidelity interface for chemical material composition.
    /// </summary>
    public class Workshop_Gui_MaterialForge : IGuiProvider
    {
        public string Title => "Thermodynamic Material Forge";

        // --- STATE ---
        private List<AtomicElement> _periodicTable;
        private MaterialCompound _activeCompound;
        private VisualElement _crucibleContainer;
        private VisualElement _thermoDataContainer;
        private GuiContext _lastCtx;

        public Workshop_Gui_MaterialForge()
        {
            _activeCompound = new MaterialCompound();
            InitializePeriodicTable(); 
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            // ROOT: Forge Container using the Row layout for side-by-side panels
            var root = new ForgeContainerBuilder("MaterialForgeRoot")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f, 1.0f));

            // --- LEFT PANEL: Elemental Repository ---
            var leftPanel = new ForgeContainerBuilder("PeriodicTable")
                .WithFlexGrow(2)
                .WithPadding(15)
                .WithBorderWidth(0, 2, 0, 0)
                .WithBorderColor(Color.black)
                .OnBuild(ve => {
                    // Injecting a ScrollView for the grid
                    var scroll = new ScrollView { style = { flexGrow = 1 } };
                    var grid = new VisualElement { 
                        style = { 
                            flexDirection = FlexDirection.Row, 
                            flexWrap = Wrap.Wrap,
                            justifyContent = Justify.FlexStart
                        } 
                    };
                    
                    foreach (var element in _periodicTable.OrderBy(e => e.AtomicNumber))
                    {
                        grid.Add(CreateElementTile(element));
                    }
                    
                    scroll.Add(grid);
                    ve.Add(scroll);
                });

            leftPanel.AddChild(new ForgeLabelBuilder("ELEMENTAL REPOSITORY")
                .WithColor(GuiSkin.Active.PrimaryAccent)
                .WithBold()
                .WithFontSize(14)
                .WithMarginBottom(10));

            root.AddChild(leftPanel);

            // --- RIGHT PANEL: The Crucible ---
            var rightPanel = new ForgeContainerBuilder("Crucible")
                .WithFlexGrow(1)
                .WithPadding(15)
                .AddChild(new ForgeLabelBuilder("THE CRUCIBLE")
                    .WithBold()
                    .WithFontSize(18)
                    .WithMarginBottom(15));

            // Dynamic Container Anchors
            rightPanel.OnBuild(ve => {
                _crucibleContainer = new VisualElement { name = "ActiveAtoms" };
                _crucibleContainer.style.flexDirection = FlexDirection.Row;
                _crucibleContainer.style.flexWrap = Wrap.Wrap;
                _crucibleContainer.style.marginBottom = 20;
                ve.Add(_crucibleContainer);

                _thermoDataContainer = new VisualElement { name = "ThermoData" };
                _thermoDataContainer.style.flexGrow = 1;
                ve.Add(_thermoDataContainer);
            });

            // Action: Render Button
            rightPanel.AddChild(new ForgeButtonBuilder("RENDER IN LIVE PREVIEW", PushToLivePreview)
                .WithBackgroundColor(GuiSkin.Active.PrimaryAccent)
                .WithTextColor(Color.black)
                .WithBold()
                .WithHeight(35)
                .WithMarginTop(15));

            var builtRoot = root.CreateGui(ctx);
            RefreshCrucibleUI();
            return builtRoot;
        }

        private VisualElement CreateElementTile(AtomicElement element)
        {
            Color catColor = GetCategoryColor(element.Category);

            var tile = new ForgeContainerBuilder($"Tile_{element.Symbol}")
                .WithWidth(60).WithHeight(60)
                .WithMargin(2)
                .WithBackgroundColor(new Color(catColor.r * 0.2f, catColor.g * 0.2f, catColor.b * 0.2f, 1f))
                .WithBorderWidth(2)
                .WithBorderColor(catColor)
                .WithBorderRadius(4)
                .WithPadding(4);

            // Atomic Details
            tile.AddChild(new ForgeLabelBuilder(element.AtomicNumber.ToString())
                .WithFontSize(9).WithColor(Color.gray).WithTextAlign(TextAnchor.UpperLeft));

            tile.AddChild(new ForgeLabelBuilder(element.Symbol)
                .WithFontSize(20).WithBold().WithColor(Color.white).WithTextAlign(TextAnchor.MiddleCenter));

            tile.AddChild(new ForgeLabelBuilder(element.AtomicMass.ToString("F1"))
                .WithFontSize(8).WithColor(Color.gray).WithTextAlign(TextAnchor.LowerCenter));

            var visualTile = tile.CreateGui(_lastCtx);
            visualTile.RegisterCallback<ClickEvent>(evt => {
                _activeCompound.AddElement(element);
                RefreshCrucibleUI();
            });

            return visualTile;
        }

        private void RefreshCrucibleUI()
        {
            if (_crucibleContainer == null || _thermoDataContainer == null) return;
            _crucibleContainer.Clear();
            _thermoDataContainer.Clear();

            // 1. Formula Display
            string formula = string.Join("", _activeCompound.Composition.Select(kvp => kvp.Value > 1 ? $"{kvp.Key.Symbol}{kvp.Value}" : kvp.Key.Symbol));
            if (string.IsNullOrEmpty(formula)) formula = "Empty Crucible";

            _crucibleContainer.Add(new ForgeLabelBuilder(formula)
                .WithFontSize(36).WithBold().WithColor(GuiSkin.Active.PrimaryAccent).Build());

            // 2. Data Readout
            if (_activeCompound.Composition.Count > 0)
            {
                var readout = new ForgeContainerBuilder("ThermoReadout")
                    .AddChild(new ForgeLabelBuilder("THERMODYNAMICS & PHYSICS").WithBold().WithColor(Color.gray).WithMarginBottom(8))
                    .AddChild(CreateDataRow("Molar Mass", $"{_activeCompound.CalculateMolarMass():F2} g/mol"))
                    .AddChild(CreateDataRow("Specific Heat", $"{_activeCompound.GetAverageSpecificHeat():F2} J/(kg·K)"))
                    .AddChild(CreateDataRow("Bond Nature", _activeCompound.IsMetallic() ? "Metallic" : "Covalent"))
                    .AddSeparator(Color.gray, 1)
                    .AddChild(new ForgeLabelBuilder("RENDER DERIVATIVES").WithBold().WithColor(Color.gray).WithMarginBottom(8))
                    .AddChild(CreateDataRow("Albedo", ColorUtility.ToHtmlStringRGB(_activeCompound.DeriveAlbedo())))
                    .AddChild(CreateDataRow("Smoothness", _activeCompound.DeriveSmoothness().ToString("F2")))
                    .CreateGui(_lastCtx);

                _thermoDataContainer.Add(readout);
            }
        }

        private IGuiProvider CreateDataRow(string label, string value)
        {
            return new ForgeContainerBuilder("DataRow")
                .WithDirection(FlexDirection.Row)
                .WithMarginBottom(4)
                .AddChild(new ForgeLabelBuilder(label).WithFlexGrow(1).WithColor(Color.gray))
                .AddChild(new ForgeLabelBuilder(value).WithBold().WithColor(Color.white));
        }

        private void PushToLivePreview()
        {
            Material generatedMat = new Material(Shader.Find("Standard"));
            generatedMat.color = _activeCompound.DeriveAlbedo();
            generatedMat.SetFloat("_Metallic", _activeCompound.IsMetallic() ? 1.0f : 0.0f);
            generatedMat.SetFloat("_Glossiness", _activeCompound.DeriveSmoothness());

            Debug.Log($"[MaterialForge] Pushing {_activeCompound.CompoundName} to Experience Engine...");
        }

        // --- IGuiProvider Requirements ---
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        
        public void ToUIDocument(string assetPath) 
        {
            var root = CreateGui(_lastCtx ?? new GuiContext());
            string fileName = string.IsNullOrEmpty(assetPath) ? "MaterialForge_Bake" : System.IO.Path.GetFileNameWithoutExtension(assetPath);
            WorkshopUxmlBaker.Bake(root, fileName);
        }

        public void FromUIDocument(string assetPath) => Debug.Log($"[MaterialForge] Hydration from {assetPath} requested.");

        // --- Data & Colors ---
        private void InitializePeriodicTable() { /* Population logic as per snippet */ }
        private Color GetCategoryColor(ElementCategory cat) { /* Color mapping logic */ return Color.gray; }
    }
}
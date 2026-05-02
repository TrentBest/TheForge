using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    /// <summary>
    /// The Universal Asset Forge: Centralizes mesh modification, material painting, 
    /// and UI binding into a single contextual workspace.
    /// Uses 100% decoupled Forge Builders.
    /// </summary>
    public class GUITools_AssetForge : IGuiProvider
    {
        public string Title => "THE ASSET FORGE";

        // Dynamic State Containers
        private VisualElement _inspectorTabContent;

        // State
        private string _activeTab = "Materials"; // Default Tab

        public VisualElement CreateGui(GuiContext ctx)
        {
            // 1. Compile the master layout using builders entirely
            var rootBuilder = new GraphicalUserInterfaceBuilder("AssetForgeRoot")
                .WithFlexGrow(1)
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f)) // Deep void background
                .AddChild(BuildSchematicPane(ctx))
                .AddChild(BuildViewportPane(ctx))
                .AddChild(BuildInspectorPane(ctx));

            return rootBuilder.Build();
        }

        // ==========================================
        // PANE 1: THE SCHEMATIC (Hierarchy / Layers)
        // ==========================================
        private IGuiProvider BuildSchematicPane(GuiContext ctx)
        {
            var schematic = new GraphicalUserInterfaceBuilder("SchematicPane")
                .WithWidth(280)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithBorderRightColor(new Color(0.2f, 0.2f, 0.2f))
                .WithBorderRightWidth(1)
                .WithPadding(10);

            schematic.AddHeader("OBJECT SCHEMATIC", Color.cyan);
            schematic.AddSeparator(Color.cyan, 1);

            // Mocking a hierarchy tree
            schematic.AddChild(CreateHierarchyItem("Base Chassis (Mesh)", true));
            schematic.AddChild(CreateHierarchyItem("  ├─ Left Thruster", false));
            schematic.AddChild(CreateHierarchyItem("  ├─ Right Thruster", false));
            schematic.AddChild(CreateHierarchyItem("Main Control Terminal (UI)", false));
            schematic.AddChild(CreateHierarchyItem("Targeting System (Logic)", false));

            schematic.AddChild(new GraphicalUserInterfaceBuilder("AddLayerBtn")
                .WithMarginTop(15)
                .AddChild(new ForgeButtonBuilder("+ ADD COMPONENT")
                    .OnClick(() => Debug.Log("Add Layer Protocol Engaged"))));

            return schematic;
        }

        private IGuiProvider CreateHierarchyItem(string name, bool isSelected)
        {
            return new GraphicalUserInterfaceBuilder($"Node_{name}")
                .WithPadding(8)
                .WithMarginBottom(2)
                .WithBorderRadius(4)
                .WithBackgroundColor(isSelected ? new Color(0.15f, 0.3f, 0.4f) : new Color(0.1f, 0.1f, 0.12f))
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .AddChild(new ForgeLabelBuilder(name)
                    .WithColor(isSelected ? Color.white : Color.gray)
                    .WithBold(isSelected));
        }

        // ==========================================
        // PANE 2: THE VIEWPORT (Live Preview & Tools)
        // ==========================================
        private IGuiProvider BuildViewportPane(GuiContext ctx)
        {
            var viewportWrapper = new GraphicalUserInterfaceBuilder("ViewportWrapper")
                .WithFlexGrow(1)
                .WithFlexLayout(FlexDirection.Column)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.02f));

            // Top Toolbar (Tool Selection)
            var toolbar = new GraphicalUserInterfaceBuilder("ViewportToolbar")
                .WithHeight(40)
                .WithBackgroundColor(new Color(0.06f, 0.06f, 0.08f))
                .WithBorderBottomWidth(1).WithBorderBottomColor(new Color(0.3f, 0.3f, 0.3f))
                .WithFlexLayout(FlexDirection.Row, Justify.Center, Align.Center);

            toolbar.AddChild(new ForgeButtonBuilder("✋ Pan"));
            toolbar.AddChild(new ForgeButtonBuilder("🔄 Orbit"));
            toolbar.AddChild(new ForgeButtonBuilder("🖌️ Paint"));
            toolbar.AddChild(new ForgeButtonBuilder("📐 Modify Mesh"));

            viewportWrapper.AddChild(toolbar);

            // The actual 3D Canvas
            viewportWrapper.AddChild(new GraphicalUserInterfaceBuilder("LMPB_Canvas")
                .WithFlexGrow(1)
                .OnBuild(ve => {
                    // Injecting the mockup LiveModelPreviewBuilder label via ForgeLabelBuilder
                    var mockPreview = new ForgeLabelBuilder("LIVE MODEL PREVIEW CANVAS")
                        .WithColor(new Color(0.2f, 0.2f, 0.2f))
                        .WithFontSize(24)
                        .WithBold(true)
                        .WithTextAlign(TextAnchor.MiddleCenter)
                        .WithFlexGrow(1);

                    ve.Add(mockPreview.Build());
                }));

            return viewportWrapper;
        }

        // ==========================================
        // PANE 3: THE INSPECTOR (Contextual Tabs)
        // ==========================================
        private IGuiProvider BuildInspectorPane(GuiContext ctx)
        {
            var inspector = new GraphicalUserInterfaceBuilder("InspectorPane")
                .WithWidth(380)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithBorderLeftColor(new Color(0.2f, 0.2f, 0.2f))
                .WithBorderLeftWidth(1)
                .WithFlexLayout(FlexDirection.Column);

            // Tab Buttons Header
            var tabHeader = new GraphicalUserInterfaceBuilder("TabHeader")
                .WithFlexLayout(FlexDirection.Row)
                .WithHeight(40)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f));

            tabHeader.AddChild(CreateTabButton("Transform"));
            tabHeader.AddChild(CreateTabButton("Materials"));
            tabHeader.AddChild(CreateTabButton("Logic Bindings"));

            inspector.AddChild(tabHeader);

            // Content Area (Dynamic based on Tab)
            inspector.AddChild(new GraphicalUserInterfaceBuilder("TabContent")
                .WithPadding(15)
                .WithFlexGrow(1)
                .WithScrollable(true)
                .OnBuild(ve => {
                    _inspectorTabContent = ve;
                    RefreshInspectorContent();
                }));

            return inspector;
        }

        private IGuiProvider CreateTabButton(string tabName)
        {
            bool isActive = _activeTab == tabName;

            return new ForgeButtonBuilder(tabName)
                .OnClick(() => {
                    _activeTab = tabName;
                    RefreshInspectorContent();
                })
                .WithFlexGrow(1)
                .WithBackgroundColor(isActive ? new Color(0.12f, 0.12f, 0.14f) : new Color(0.05f, 0.05f, 0.06f))
                .WithTextColor(isActive ? Color.white : Color.gray)
                .WithFontStyle(isActive ? FontStyle.Bold : FontStyle.Normal)
                .OnBuild(ve => {
                    ve.style.borderTopWidth = 0;
                    ve.style.borderLeftWidth = 0;
                    ve.style.borderRightWidth = 0;
                    ve.style.borderBottomWidth = isActive ? 2 : 0;
                    ve.style.borderBottomColor = new Color(0.8f, 0.1f, 0.8f); // Singularity Purple
                });
        }

        // ==========================================
        // TAB LOGIC & CONTENT GENERATION
        // ==========================================
        private void RefreshInspectorContent()
        {
            if (_inspectorTabContent == null) return;
            _inspectorTabContent.Clear();

            var contentBuilder = new GraphicalUserInterfaceBuilder("CurrentTab");

            switch (_activeTab)
            {
                case "Transform":
                    contentBuilder.AddHeader("SPATIAL DATA", Color.cyan);
                    contentBuilder.AddStringData("Position", "X: 0  Y: 10  Z: -5", null);
                    contentBuilder.AddStringData("Rotation", "X: 0  Y: 45  Z: 0", null);
                    contentBuilder.AddStringData("Scale", "X: 1  Y: 1  Z: 1", null);
                    break;

                case "Materials":
                    contentBuilder.AddHeader("MATERIAL UNROLL", new Color(0.8f, 0.1f, 0.8f));
                    contentBuilder.AddChild(CreateMaterialSlot("Mat_Hull_Primary", new Color(0.3f, 0.3f, 0.3f)));
                    contentBuilder.AddChild(CreateMaterialSlot("Mat_Hull_Secondary", new Color(0.8f, 0.4f, 0.1f)));
                    contentBuilder.AddChild(CreateMaterialSlot("Mat_Energy_Core", Color.cyan));
                    break;

                case "Logic Bindings":
                    contentBuilder.AddHeader("FSM & EVENT BINDINGS", Color.yellow);
                    contentBuilder.AddStringData("Interaction FSM", "Terminal_Hack_Sequence", null);
                    contentBuilder.AddStringData("On Click Action", "REST: UserAuth/Ping", null);
                    contentBuilder.AddChild(new GraphicalUserInterfaceBuilder("BindBtnWrapper")
                        .WithMarginTop(15)
                        .AddChild(new ForgeButtonBuilder("🔗 Bind New Sensory Event")));
                    break;
            }

            _inspectorTabContent.Add(contentBuilder.Build());
        }

        // The "Material Unroller" widget
        private IGuiProvider CreateMaterialSlot(string matName, Color previewColor)
        {
            var slot = new GraphicalUserInterfaceBuilder($"Slot_{matName}")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithPadding(10)
                .WithMarginBottom(8)
                .WithBorderRadius(6)
                .WithBorderLeftWidth(4)
                .WithBorderLeftColor(previewColor);

            var infoCol = new GraphicalUserInterfaceBuilder("InfoCol")
                .WithFlexLayout(FlexDirection.Column)
                .AddChild(new ForgeLabelBuilder(matName).WithColor(Color.white).WithBold(true))
                .AddChild(new ForgeLabelBuilder("Shader: Standard (PBR)").WithColor(Color.gray).WithFontSize(10));

            slot.AddChild(infoCol);

            var actionsCol = new GraphicalUserInterfaceBuilder("ActionsCol")
                .WithFlexLayout(FlexDirection.Row)
                .AddChild(new ForgeButtonBuilder("🎨 Edit")
                    .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f))
                    .WithTextColor(Color.white));

            slot.AddChild(actionsCol);

            return slot;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}
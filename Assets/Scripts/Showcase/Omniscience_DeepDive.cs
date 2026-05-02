using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Showcase
{
    public class Omniscience_DeepDive<T> : IGuiProvider where T : class, IGuiProvider
    {
        public string Title => "Omniscience Mode";

        private T _liveSimulation;
        private IGuiProvider _parentMenu;
        private VisualElement _parentContainer;

        // Tracks the dynamically instantiated type during Explorer Mode
        private T _exploredInstance;

        /// <summary>
        /// EXPLORER MODE: Scrapes assembly for all available types and allows dynamic instantiation.
        /// </summary>
        public Omniscience_DeepDive()
        {
            _liveSimulation = null;
            _parentMenu = null;
            _parentContainer = null;
        }

        /// <summary>
        /// TARGETED MODE: Attaches to an active, running simulation.
        /// </summary>
        public Omniscience_DeepDive(T liveSimulation, IGuiProvider parentMenu, VisualElement parentContainer)
        {
            _liveSimulation = liveSimulation;
            _parentMenu = parentMenu;
            _parentContainer = parentContainer;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("DeepDiveRoot")
                .WithFlexGrow(1).WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

            // --- HEADER ---
            var header = new GraphicalUserInterfaceBuilder("Header")
                .WithPadding(20).WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))
                .WithBorderBottomColor(Color.cyan).WithBorderBottomWidth(2)
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center);

            string modeTitle = _liveSimulation != null ? "OMNISCIENCE MODE: LIVE REFLECTION" : "OMNISCIENCE MODE: ASSEMBLY EXPLORER";
            header.AddChild(new ForgeLabelBuilder(modeTitle)
                .WithFontSize(20).WithColor(Color.cyan).WithFontStyle(FontStyle.Bold));

            if (_liveSimulation != null)
            {
                var timeControls = new GraphicalUserInterfaceBuilder("TimeControls")
                    .WithFlexLayout(FlexDirection.Row, Justify.Center, Align.Center)
                    .AddChild(new ForgeButtonBuilder("⏸ FREEZE LOGIC")
                        .WithBackgroundColor(new Color(0.8f, 0.6f, 0.1f)).WithWidth(150).WithMarginRight(10)
                        .OnClick(() => Debug.Log("FSM Logic Paused.")))
                    .AddChild(new ForgeButtonBuilder("STEP TICK")
                        .WithBackgroundColor(new Color(0.4f, 0.4f, 0.4f)).WithWidth(120).WithMarginRight(20)
                        .OnClick(() => Debug.Log("FSM Tick Stepped.")));
                header.AddChild(timeControls);
            }

            if (_parentContainer != null && _parentMenu != null)
            {
                header.AddChild(new ForgeButtonBuilder("RETURN TO MENU")
                    .WithBackgroundColor(new Color(0.6f, 0.2f, 0.2f)).WithTextColor(Color.white)
                    .WithHeight(40).WithWidth(180).WithFontStyle(FontStyle.Bold)
                    .OnClick(() => {
                        _parentContainer.Clear();
                        _parentContainer.Add(_parentMenu.CreateGui(ctx));
                    }));
            }

            root.AddChild(header);

            // --- CONTENT AREA ---
            var contentArea = new VisualElement { name = "ContentArea", style = { flexGrow = 1 } };

            if (_liveSimulation != null)
            {
                RenderSplitPanel(contentArea, _liveSimulation, ctx);
            }
            else
            {
                RenderTypeExplorer(contentArea, ctx);
            }

            root.OnBuild(ve => ve.Add(contentArea));
            return root.Build();
        }

        private void RenderTypeExplorer(VisualElement container, GuiContext ctx)
        {
            container.Clear();

            // 1. Gather all non-abstract types that implement T across all assemblies
            var availableTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            // 2. Build the Dropdown Toolbar
            var toolbar = new GraphicalUserInterfaceBuilder("ExplorerToolbar")
                .WithPadding(10).WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .Build();

            toolbar.Add(new Label("Select Type to Inspect: ") { style = { color = Color.white, marginRight = 10, unityFontStyleAndWeight = FontStyle.Bold } });

            var typeNames = availableTypes.Select(t => t.Name).ToList();
            var dropdown = new DropdownField(typeNames, 0);
            dropdown.style.width = 300;

            var displayArea = new VisualElement { style = { flexGrow = 1, marginTop = 10 } };

            dropdown.RegisterValueChangedCallback(evt =>
            {
                var selectedType = availableTypes.FirstOrDefault(t => t.Name == evt.newValue);
                if (selectedType != null)
                {
                    InjectAndRenderType(selectedType, displayArea, ctx);
                }
            });

            toolbar.Add(dropdown);
            container.Add(toolbar);
            container.Add(displayArea);

            // Auto-load the first type if available
            if (availableTypes.Any())
            {
                InjectAndRenderType(availableTypes[0], displayArea, ctx);
            }
        }

        private void InjectAndRenderType(Type targetType, VisualElement displayArea, GuiContext ctx)
        {
            displayArea.Clear();

            try
            {
                // Attempt to instantiate the type dynamically (requires a parameterless constructor)
                _exploredInstance = (T)Activator.CreateInstance(targetType);

                // Render the standard God Mode split panel using our newly minted instance
                RenderSplitPanel(displayArea, _exploredInstance, ctx);
            }
            catch (Exception ex)
            {
                var errorBuilder = new ForgeLabelBuilder($"Failed to instantiate {targetType.Name}.\nRequires a parameterless constructor.\nError: {ex.Message}")
                    .WithColor(Color.red).WithAlignment(TextAnchor.MiddleCenter).WithMarginTop(50);
                displayArea.Add(errorBuilder.Build());
            }
        }

        private void RenderSplitPanel(VisualElement container, T targetInstance, GuiContext ctx)
        {
            container.Clear();

            var splitLayout = new ForgeSplitPanelBuilder(1, 2)
                .WithSector(0, 0, new GraphicalUserInterfaceBuilder("GameWrapper")
                    .WithPadding(10)
                    .AddChild(new ForgeLabelBuilder("NATIVE RENDER").WithColor(Color.white).WithFontStyle(FontStyle.Bold).WithMarginBottom(10))
                    // Executes the actual UI generation of the target
                    .AddChild(targetInstance))
                .WithSector(0, 1, new GraphicalUserInterfaceBuilder("ReflectionWrapper")
                    .WithPadding(10)
                    .AddChild(new ForgeLabelBuilder("DATA X-RAY").WithColor(Color.white).WithFontStyle(FontStyle.Bold).WithMarginBottom(10))
                    // Reflects the underlying variables of the target
                    .AddChild(new ReflectiveGuiBuilder<T>(targetInstance).WithRecursion(2)));

            container.Add(splitLayout.CreateGui(ctx));
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}
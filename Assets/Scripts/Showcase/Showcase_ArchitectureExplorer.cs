using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Showcase
{
    public class Showcase_ArchitectureExplorer : IGuiProvider
    {
        public string Title => $"ARCHITECTURE EXPLORER: {_targetDomain}";

        private string _targetDomain;
        private object _domainInstance;

        // Reactivity State
        private VisualElement _inspectorContainer;
        private bool _autoRefresh = false;

        public Showcase_ArchitectureExplorer()
        {
            _targetDomain = "Awaiting Target...";
            _domainInstance = null;
        }

        public Showcase_ArchitectureExplorer(string domainName, object instance)
        {
            SetTarget(domainName, instance);
        }

        public void SetTarget(string domainName, object instance)
        {
            _targetDomain = domainName;
            _domainInstance = instance;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var splitPanel = new ForgeSplitPanelBuilder("ExplorerMatrix")
                .WithFlexDirection(FlexDirection.Row)
                .WithLeftPane(BuildFileSystemPane())
                .WithRightPane(BuildReflectiveInspectorPane());

            var root = splitPanel.CreateGui(ctx);

            // 1-Second Heartbeat for Live Data
            root.schedule.Execute(() => {
                if (_autoRefresh) RefreshInspector();
            }).Every(1000);

            return root;
        }

        private IGuiProvider BuildFileSystemPane()
        {
            return new GraphicalUserInterfaceBuilder("SourceTree")
                .WithTitle("SOURCE ARCHITECTURE")
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f))
                .WithPadding(10)
                .AddChild(new ForgeLabelBuilder($"Root: Assets/Scripts/{_targetDomain}")
                    .WithFontSize(12)
                    .WithColor(Color.gray))
                .AddSeparator();
        }

        private IGuiProvider BuildReflectiveInspectorPane()
        {
            var wrapper = new GraphicalUserInterfaceBuilder("InspectorWrapper")
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.14f))
                .WithFlexGrow(1);

            // --- Top Toolbar for Reactivity Controls ---
            var topBar = new GraphicalUserInterfaceBuilder("InspectorTopBar")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithPadding(5)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithBorderBottomWidth(1).WithBorderBottomColor(new Color(0.3f, 0.3f, 0.3f))
                .AddChild(new ForgeLabelBuilder("LIVE MEMORY INSPECTOR").WithColor(Color.cyan).WithFontStyle(FontStyle.Bold))
                .AddChild(ctx => {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };

                    var autoToggle = new Toggle("Auto-Refresh (1s)") { value = _autoRefresh };
                    autoToggle.RegisterValueChangedCallback(e => _autoRefresh = e.newValue);
                    autoToggle.style.color = Color.gray;
                    autoToggle.style.marginRight = 15;

                    var manualBtn = new Button(RefreshInspector) { text = "Refresh Data" };
                    manualBtn.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
                    manualBtn.style.color = Color.white;

                    row.Add(autoToggle);
                    row.Add(manualBtn);
                    return row;
                });

            wrapper.AddChild(topBar);

            // --- The Dynamic Container ---
            wrapper.AddChild(ctx => {
                _inspectorContainer = new VisualElement { style = { flexGrow = 1 } };
                RefreshInspector();
                return _inspectorContainer;
            });

            return wrapper;
        }

        private void RefreshInspector()
        {
            if (_inspectorContainer == null || _domainInstance == null) return;

            _inspectorContainer.Clear();

            // Build the fresh snapshot of memory
            var builder = new ReflectiveGuiBuilder<object>(_domainInstance).WithRecursion(1);
            _inspectorContainer.Add(builder.CreateGui(new GuiContext()));
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}
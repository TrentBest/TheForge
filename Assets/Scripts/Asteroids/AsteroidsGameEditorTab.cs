#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Asteroids
{ 
    public class AsteroidsGameEditorTab : IGuiProvider
    {
        public string TabName => "Asteroids Builder";
        public string Title => "ASTEROIDS ARCADE SYSTEM";

        private string _activeSubTab = "Master Settings";
        private VisualElement _rootContainer;
        private GuiContext _guiContext;

        // Our recursive builders for the specific contexts
        private readonly Dictionary<string, IGuiProvider> _subBuilders = new Dictionary<string, IGuiProvider>
        {
            { "Master Settings", new AsteroidsContextBuilderGui() },
            { "Fighter Settings", new AsteroidFighterBuilderGui() },
            { "Asteroid Settings", new AsteroidEntityBuilderGui() }
        };

        public VisualElement CreateGui(GuiContext context)
        {
            _guiContext = context;
            _rootContainer = new VisualElement();
            Refresh();
            return _rootContainer;
        }

        private void Refresh()
        {
            _rootContainer.Clear();

            var splitPanel = new ForgeSplitPanelBuilder(200)
                .WithSidebar(CreateSidebar())
                .WithMain(CreateMainContent());

            _rootContainer.Add(splitPanel.CreateGui(_guiContext));
        }

        private IGuiProvider CreateSidebar()
        {
            var sidebar = new GraphicalUserInterfaceBuilder("AsteroidsSubNav")
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                .WithPadding(10)
                .AddChild(new Label("ASTEROIDS EDITORS") { style = { color = Color.cyan, marginBottom = 15, unityFontStyleAndWeight = FontStyle.Bold } });

            foreach (var tabName in _subBuilders.Keys)
            {
                var isSelected = _activeSubTab == tabName;

                // Need to capture the loop variable for the closure
                var currentTabName = tabName;

                var btn = new Button(() => { _activeSubTab = currentTabName; Refresh(); })
                {
                    text = currentTabName,
                    style = {
                        height = 30,
                        marginBottom = 5,
                        backgroundColor = isSelected ? new Color(0.3f, 0.4f, 0.5f) : new Color(0.2f, 0.2f, 0.2f),
                        color = isSelected ? Color.white : Color.gray
                    }
                };

                sidebar.AddChild(new GenericGuiProvider(() => btn));
            }

            return sidebar;
        }

        private IGuiProvider CreateMainContent()
        {
            var mainContent = new GraphicalUserInterfaceBuilder("AsteroidsMainContent")
                .WithPadding(20)
                .WithTitle($"{_activeSubTab.ToUpper()}");

            if (_subBuilders.TryGetValue(_activeSubTab, out var builder))
            {
                mainContent.AddChild(builder);
            }

            return mainContent;
        }

        // --- IGuiProvider Implementation ---
        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        public void FromUIDocument(string assetPath) =>
            _guiContext?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));

        public void ToUIDocument(string assetPath) =>
            GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }

    // A simple wrapper to map raw VisualElements (like standard UI Builder buttons) into your IGuiProvider pipelines
    public class GenericGuiProvider : IGuiProvider
    {
        private System.Func<VisualElement> _creator;
        public string Title => "Generic UI Element";

        public GenericGuiProvider(System.Func<VisualElement> creator) => _creator = creator;

        public VisualElement CreateGui(GuiContext context) => _creator();

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        public void FromUIDocument(string assetPath) { /* No-op for generic wrappers */ }
        public void ToUIDocument(string assetPath) { /* No-op for generic wrappers */ }
    }
}
#endif
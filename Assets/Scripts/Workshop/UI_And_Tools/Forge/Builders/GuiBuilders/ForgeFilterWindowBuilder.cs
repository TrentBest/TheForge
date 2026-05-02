using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

// --- THE FIX: Wrap the Editor namespace ---
#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ForgeFilterWindowBuilder : IGuiProvider
    {
        public string Title { get; set; } = "Search";

        private readonly IFilterWindowProvider _provider;
        private readonly List<ForgeFilterWindow.Element> _fullTree = new();
        private VisualElement _resultsContainer;
        private string _currentFilter = "";

        private Action<ForgeFilterWindow.Element> _onSelection;
        private Color _accentColor = Color.magenta;

        public ForgeFilterWindowBuilder(IFilterWindowProvider provider)
        {
            _provider = provider;
            _provider.CreateComponentTree(_fullTree);
        }

        public ForgeFilterWindowBuilder WithTitle(string title) { Title = title; return this; }
        public ForgeFilterWindowBuilder WithAccentColor(Color color) { _accentColor = color; return this; }
        public ForgeFilterWindowBuilder OnSelection(Action<ForgeFilterWindow.Element> callback) { _onSelection = callback; return this; }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("FilterWindow_Root")
                .WithPadding(10)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f))
                .WithBorderAllColor(new Color(0.3f, 0.3f, 0.3f))
                .WithBorderWidth(1)
                .WithBorderRadius(4)
                .WithWidth(300)
                .WithMaxHeight(400);

            // 1. Header
            root.AddChild(new ForgeLabelBuilder(Title.ToUpper())
                .WithColor(_accentColor)
                .WithBold()
                .WithFontSize(10)
                .WithMarginBottom(8));

            // 2. Search Field
            var searchField = new TextField { value = _currentFilter };
            searchField.RegisterValueChangedCallback(evt => {
                _currentFilter = evt.newValue;
                RefreshResults();
            });
            searchField.style.marginBottom = 10;
            root.AddChild(searchField);

            // 3. Results Area (Scrollable)
            _resultsContainer = new ScrollView { style = { flexGrow = 1 } };
            RefreshResults();
            root.AddChild(_resultsContainer);

            return root.Build();
        }

        private void RefreshResults()
        {
            _resultsContainer.Clear();

            // Filter items based on search string
            var filtered = _fullTree.Where(e =>
                string.IsNullOrEmpty(_currentFilter) ||
                e.name.Contains(_currentFilter, StringComparison.OrdinalIgnoreCase) ||
                e is ForgeFilterWindow.GroupElement // Keep groups visible for context
            ).ToList();

            foreach (var element in filtered)
            {
                bool isGroup = element is ForgeFilterWindow.GroupElement;

                var item = new GraphicalUserInterfaceBuilder($"Item_{element.name}")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                    .WithPadding(4)
                    .WithPaddingLeft(element.level * 15 + 5) // Indentation logic
                    .OnBuild(ve => {
                        // Hover/Selection Highlight
                        if (!isGroup)
                        {
                            ve.AddManipulator(new Clickable(() => HandleClick(element)));
                            ve.RegisterCallback<MouseEnterEvent>(e => ve.style.backgroundColor = new Color(1, 1, 1, 0.1f));
                            ve.RegisterCallback<MouseLeaveEvent>(e => ve.style.backgroundColor = Color.clear);
                        }
                    });

                // Icon/Bullet
                string prefix = isGroup ? "▼ " : "• ";
                item.AddChild(new ForgeLabelBuilder(prefix)
                    .WithColor(isGroup ? _accentColor : Color.gray)
                    .WithBold(isGroup));

                // Label
                item.AddChild(new ForgeLabelBuilder(element.name)
                    .WithColor(isGroup ? Color.white : new Color(0.8f, 0.8f, 0.8f))
                    .WithBold(isGroup));

                _resultsContainer.Add(item.Build());
            }

            if (filtered.Count == 0)
            {
                _resultsContainer.Add(new ForgeLabelBuilder("No matches found...")
                    .WithColor(Color.gray)
                    .WithFontStyle(FontStyle.Italic)
                    .WithMargin(10, 10)
                    .Build());
            }
        }

        private void HandleClick(ForgeFilterWindow.Element element)
        {
            // Execute the API logic
            bool closeWindow = _provider.GoToChild(element, true);

            // Notify the calling UI
            if (closeWindow)
            {
                _onSelection?.Invoke(element);
            }
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}
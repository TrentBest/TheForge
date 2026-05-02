using Assets.Scripts.GURPS;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.GURPS
{
    /// <summary>
    /// The Multiverse Library Selector.
    /// Reforged to follow the Forge Protocol and utilize Rulebook UI Helpers.
    /// </summary>
    public class GURPS_Gui_BookSelector : IGuiProvider
    {
        public string Title => "Multiverse Library";
        public Action<GURPSBook> OnEditSourceRequested;

        private GURPSBook _currentSelectedBook;
        private GURPSUniverse _currentUniverse;
        private GuiContext _lastCtx;
        private VisualElement _mainContainer;
        private ScrollView _rootScroll;
        private string _searchQuery = "";

        public GURPS_Gui_BookSelector() { }

        public VisualElement CreateGui(GURPSUniverse uni)
        {
            DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem();
            _currentUniverse = uni ?? new GURPSUniverse { UniverseSeed = 42 };
            return CreateGui(new GuiContext());
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            if (_currentUniverse == null)
            {
                DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem();
                _currentUniverse = new GURPSUniverse { UniverseSeed = 42 };
            }

            // Standard Forge Root
            var rootBuilder = new ForgeContainerBuilder("Library_Root")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.07f))
                .OnBuild(ve => _mainContainer = ve);

            // Header Section
            rootBuilder.AddChild(new ForgeContainerBuilder("LibraryHeader")
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithBorderBottomWidth(1)
                .WithBorderBottomColor(Color.cyan)
                .AddChild(new ForgeLabelBuilder("GURPS: MULTIVERSE LIBRARY")
                    .WithFontSize(24).WithBold().WithColor(Color.cyan))
                .AddChild(new ForgeLabelBuilder("Select the rulebooks and supplements active in this quantum seed.")
                    .WithFontSize(11).WithColor(Color.gray))
            );

            // Controls Section
            rootBuilder.AddChild(new ForgeContainerBuilder("LibraryControls")
                .WithPadding(15, 20, 5, 20)
                .AddChild(new ForgeTextFieldBuilder("Filter Catalog", _searchQuery)
                    .OnValueChanged(evt => {
                        _searchQuery = evt.newValue;
                        ShowSelector();
                    }))
            );

            // Scrollable Content
            rootBuilder.AddChild(new ForgeContainerBuilder("LibraryBody")
                .WithFlexGrow(1)
                .WithPadding(10)
                // Use DynamicGuiProvider to resolve the ScrollView wrapper
                .AddChild(new DynamicGuiProvider(c => {
                    var sv = new ForgeScrollViewBuilder().WithMode(ScrollViewMode.Vertical).CreateGui(c) as ScrollView;
                    sv.style.flexGrow = 1;
                    _rootScroll = sv;
                    return sv;
                }))
            );

            var root = rootBuilder.Build();
            ShowSelector();
            return root;
        }

        private void ShowSelector()
        {
            if (_rootScroll == null) return;
            _rootScroll.Clear();

            var allBooks = DigitalGenericUniversalRolePlayingSystem.Books.GetAll();
            var filteredBooks = allBooks.Where(b =>
                string.IsNullOrEmpty(_searchQuery) ||
                b.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ||
                b.Category.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));

            var groups = filteredBooks.GroupBy(b => b.Category).OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                var groupName = string.IsNullOrEmpty(group.Key) ? "UNCATEGORIZED" : group.Key.ToUpper();

                var groupContainer = new ForgeContainerBuilder($"Group_{groupName}")
                    .WithMarginBottom(20)
                    .AddChild(new ForgeLabelBuilder(groupName)
                        .WithBold().WithColor(Color.cyan)
                        .WithBorderBottomWidth(1).WithBorderBottomColor(new Color(0.3f, 0.3f, 0.3f))
                        .WithMarginBottom(10).WithPadding(0))
                    .Build();

                foreach (var book in group)
                {
                    groupContainer.Add(CreateDataCard(book));
                }

                _rootScroll.Add(groupContainer);
            }
        }

        /// <summary>
        /// The Rulebook Data Card.
        /// Leverages GURPS_RulebookUIHelper for icon persistence and tooltips.
        /// </summary>
        private VisualElement CreateDataCard(GURPSBook book)
        {
            bool isIncluded = _currentUniverse.IncludedRuleBooks.Contains(book.Name);
            Color accentColor = isIncluded ? Color.green : new Color(0.3f, 0.3f, 0.3f);

            var cardBuilder = new ForgeContainerBuilder($"Card_{book.Name}")
                .WithDirection(FlexDirection.Row)
                .WithMarginBottom(10).WithPadding(12)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                .WithBorderWidth(2).WithBorderColor(accentColor)
                .WithBorderRadius(8)
                .OnBuild(ve => {
                    // Hover effects via RegisterCallback
                    ve.RegisterCallback<MouseEnterEvent>(e => ve.style.backgroundColor = new Color(0.18f, 0.18f, 0.22f));
                    ve.RegisterCallback<MouseLeaveEvent>(e => ve.style.backgroundColor = new Color(0.12f, 0.12f, 0.15f));
                });

            // 1. RULEBOOK COVER (via UI Helper)
            // Icon acts as a mini-preview that triggers the high-fidelity tooltip
            var coverIcon = GURPS_RulebookUIHelper.CreateBookIcon(book.Name, 90f);
            coverIcon.style.height = 130f;
            coverIcon.style.marginRight = 15;
            cardBuilder.AddChild(new DynamicGuiProvider(c => coverIcon));

            // 2. INFORMATION COLUMN
            var infoCol = new ForgeContainerBuilder("InfoCol")
                .WithFlexGrow(1)
                .AddChild(new ForgeLabelBuilder(book.Name.ToUpper()).WithBold().WithFontSize(16).WithColor(Color.white))
                .AddChild(new ForgeLabelBuilder(book.Description)
                    .WithFontSize(12).WithColor(Color.gray).WithWordWrap().WithMarginTop(8));

            cardBuilder.AddChild(infoCol);

            // 3. ACTIONS COLUMN
            var actionCol = new ForgeContainerBuilder("ActionCol")
                .WithWidth(120).WithMarginLeft(15)
                .WithAlignItems(Align.FlexEnd)
                .WithJustifyContent(Justify.SpaceBetween);

            // Toggle - Wrapped as it's a standard control
            actionCol.AddChild(new DynamicGuiProvider(c => {
                var toggle = new Toggle("Included")
                {
                    value = isIncluded,
                    style = {
                        color = accentColor,
                        unityFontStyleAndWeight = FontStyle.Bold,
                        marginBottom = 10
                    }
                };

                toggle.RegisterValueChangedCallback(evt => {
                    if (evt.newValue)
                    {
                        if (!_currentUniverse.IncludedRuleBooks.Contains(book.Name))
                            _currentUniverse.IncludedRuleBooks.Add(book.Name);
                    }
                    else
                    {
                        _currentUniverse.IncludedRuleBooks.Remove(book.Name);
                    }
                    ShowSelector(); // Refresh to update border highlights
                });
                return toggle;
            }));

            actionCol.AddChild(new ForgeButtonBuilder("EDIT SOURCE", () => ShowEditor(book))
                .WithBackgroundColor(new Color(0.2f, 0.4f, 0.6f))
                .WithWidth(110).WithHeight(30));

            cardBuilder.AddChild(actionCol);

            return cardBuilder.Build();
        }

        private void ShowEditor(GURPSBook book)
        {
            _mainContainer.Clear();

            _mainContainer.Add(new ForgeButtonBuilder("< BACK TO LIBRARY", ShowSelector)
                .WithBackgroundColor(Color.black).WithColor(Color.yellow)
                .WithMarginBottom(10).WithHeight(30).Build());

            var editor = new GURPS_BookEditor();
            var editorGui = editor.CreateGui(_lastCtx ?? new GuiContext());
            _mainContainer.Add(editorGui);

            OnEditSourceRequested?.Invoke(book);
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "LibrarySnapshot");
        public void FromUIDocument(string path) { }
    }
}
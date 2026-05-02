using Assets.Scripts.GURPS;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    public class GURPS_CRUD_Builder<T> where T : class, new()
    {
        public class CRUDStyle
        {
            public Color AccentColor = Color.cyan;
            public float ListWidth = 300f;
        }

        public CRUDStyle Style { get; set; } = new CRUDStyle();
        public string Title { get; set; }
        private GuiContext _lastCtx;

        private Func<IEnumerable<T>> _getGlobalData;
        private Func<T, string> _getDisplayName;
        private Func<T, string> _getSourceBook;
        private Action<T, string> _setSourceBook;
        private Func<T, VisualElement> _buildEditorForm;
        private Action<T> _onSaveGlobal;
        private Action<T> _onDeleteGlobal;
        private Func<T, string> _getSubtitle;
        private Func<T, float> _getSortKey;

        private bool _showAllBooksFilter;
        private string _selectedBook;
        private T _selectedItem;
        private bool _isCreatingNew;

        private ScrollView _bookPane;
        private ScrollView _availablePane;
        private ScrollView _editorPane;

        public GURPS_CRUD_Builder(
            string title,
            Func<IEnumerable<T>> getGlobalData,
            Func<T, string> getDisplayName,
            Func<T, string> getSourceBook,
            Action<T, string> setSourceBook,
            Func<T, VisualElement> buildEditorForm,
            Action<T> onSaveGlobal,
            Action<T> onDeleteGlobal,
            Func<T, string> getSubtitle = null,
            Func<T, float> getSortKey = null,
            bool showAllBooksFilter = true)
        {
            Title = title;
            _getGlobalData = getGlobalData;
            _getDisplayName = getDisplayName;
            _getSourceBook = getSourceBook;
            _setSourceBook = setSourceBook;
            _buildEditorForm = buildEditorForm;
            _onSaveGlobal = onSaveGlobal;
            _onDeleteGlobal = onDeleteGlobal;
            _getSubtitle = getSubtitle;
            _getSortKey = getSortKey;

            _showAllBooksFilter = showAllBooksFilter;
            _selectedBook = _showAllBooksFilter ? "ALL BOOKS" : null;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            // Ensure system is booted for data access
            if (!DigitalGenericUniversalRolePlayingSystem.IsInitialized)
                DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem(new DataWarehouse());

            var builder = new GraphicalUserInterfaceBuilder($"{typeof(T).Name}_GURPS_CRUD")
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100);

            // 1. LEFT PANE: Book Filter
            builder.AddChild(new GraphicalUserInterfaceBuilder("BookPane")
                .WithWidth(200)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                .WithBorderRightWidth(2)
                .WithBorderRightColor(Style.AccentColor)
                .AddHeader("1. Book Filter", Style.AccentColor)
                .AddSeparator(Color.gray, 1)
                .AddChild(c => {
                    _bookPane = new ScrollView { style = { flexGrow = 1 } };
                    return _bookPane;
                })
            );

            // 2. MIDDLE PANE: Item DB
            builder.AddChild(new GraphicalUserInterfaceBuilder("ItemsPane")
                .WithWidth(Style.ListWidth)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithBorderRightWidth(1)
                .WithBorderRightColor(Color.gray)
                .AddChild(new GraphicalUserInterfaceBuilder("ItemsHeader")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithPadding(10)
                    .WithBorderBottomWidth(1)
                    .WithBorderBottomColor(Color.gray)
                    .AddChild(new Label($"2. {typeof(T).Name.Replace("GURPS", "").ToUpper()} DB") { style = { color = Color.white, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold } })
                    .AddButton("+ NEW", PrepareNewItem)
                )
                .AddChild(c => {
                    _availablePane = new ScrollView { style = { flexGrow = 1 } };
                    return _availablePane;
                })
            );

            // 3. RIGHT PANE: The Editor
            builder.AddChild(new GraphicalUserInterfaceBuilder("EditorPane")
                .WithAutoGrow()
                .AddChild(c => {
                    _editorPane = new ScrollView { style = { flexGrow = 1, paddingBottom = 20, paddingTop = 20, paddingLeft = 20, paddingRight = 20 } };
                    return _editorPane;
                })
            );

            var root = builder.Build();
            RefreshAll();
            return root;
        }

        private void RefreshAll()
        {
            RenderBooks();
            RenderAvailable();
            RenderEditor();
        }

        private void RenderBooks()
        {
            _bookPane.Clear();
            var globalBooks = DigitalGenericUniversalRolePlayingSystem.Books?.GetAll()?.ToList() ?? new List<GURPSBook>();

            if (_showAllBooksFilter)
                _bookPane.Add(CreateBookCard("ALL BOOKS", null, _selectedBook == "ALL BOOKS"));

            foreach (var book in globalBooks)
                _bookPane.Add(CreateBookCard(book.Name, book.CoverImagePath, _selectedBook == book.Name));
        }

        private Button CreateBookCard(string name, string coverPath, bool isSelected)
        {
            var card = new Button(() => {
                if (_selectedBook != name) { _selectedItem = null; _isCreatingNew = false; }
                _selectedBook = name;
                RefreshAll();
            })
            {
                style = {
                    flexDirection = FlexDirection.Column, height = coverPath != null ? 120 : 40, marginBottom = 10,
                    backgroundColor = isSelected ? new Color(0, 0.3f, 0.5f) : new Color(0.15f, 0.15f, 0.15f),
                    borderLeftWidth = isSelected ? 4 : 0, borderLeftColor = Style.AccentColor, alignItems = Align.Center, justifyContent = Justify.Center,
                    flexShrink = 0
                }
            };

            if (!string.IsNullOrEmpty(coverPath))
            {
                var thumb = new VisualElement { style = { width = 60, height = 80, backgroundColor = Color.black, marginBottom = 5 } };
                Texture2D tex = Resources.Load<Texture2D>(coverPath);
                if (tex != null)
                {
                    thumb.style.backgroundImage = new StyleBackground(tex);
                    thumb.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                }
                card.Add(thumb);
            }

            card.Add(new Label(name) { style = { color = isSelected ? Color.white : Color.gray, fontSize = 10, unityTextAlign = TextAnchor.MiddleCenter, whiteSpace = WhiteSpace.Normal } });
            return card;
        }

        private void RenderAvailable()
        {
            _availablePane.Clear();
            var allGlobal = _getGlobalData()?.ToList() ?? new List<T>();

            var filtered = _selectedBook == "ALL BOOKS"
                ? allGlobal.OrderBy(_getDisplayName).ToList()
                : allGlobal.Where(i => _getSourceBook(i) == _selectedBook).OrderBy(_getDisplayName).ToList();

            foreach (var item in filtered)
            {
                bool isSelected = _selectedItem == item;
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, backgroundColor = isSelected ? new Color(0.2f, 0.2f, 0.25f) : Color.clear, borderBottomWidth = 1, borderBottomColor = new Color(0.2f, 0.2f, 0.2f), flexShrink = 0 } };

                string sourceBookName = _getSourceBook(item);
                if (!string.IsNullOrEmpty(sourceBookName))
                    row.Add(GURPS_RulebookUIHelper.CreateBookIcon(sourceBookName, 20f));

                var btn = new Button(() => { _selectedItem = item; _isCreatingNew = false; RenderEditor(); RenderAvailable(); }) { style = { flexGrow = 1, backgroundColor = Color.clear, flexDirection = FlexDirection.Column, alignItems = Align.FlexStart } };
                btn.Add(new Label(_getDisplayName(item)) { style = { color = isSelected ? Color.cyan : Color.white, unityFontStyleAndWeight = FontStyle.Bold } });
                if (_getSubtitle != null) btn.Add(new Label(_getSubtitle(item)) { style = { color = Color.gray, fontSize = 10 } });

                row.Add(btn);
                _availablePane.Add(row);
            }
        }

        private void RenderEditor()
        {
            _editorPane.Clear();

            if (_selectedItem == null && !_isCreatingNew)
            {
                _editorPane.Add(new Label("Select an item to view Data Card.") { style = { color = Color.gray, alignSelf = Align.Center, marginTop = 100 } });
                return;
            }

            T workingItem = _isCreatingNew ? new T() : _selectedItem;

            if (_isCreatingNew && _setSourceBook != null)
                _setSourceBook(workingItem, _selectedBook == "ALL BOOKS" ? "GURPS Basic Set" : _selectedBook);

            _editorPane.Add(new Label("3. DATA CARD") { style = { color = Style.AccentColor, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20, borderBottomWidth = 1, borderBottomColor = Color.gray } });
            _editorPane.Add(_buildEditorForm(workingItem));

            var actionsRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 20, paddingTop = 20, borderTopWidth = 1, borderTopColor = Color.gray } };
            actionsRow.Add(new Button(() => { _onSaveGlobal(workingItem); _isCreatingNew = false; _selectedItem = workingItem; RefreshAll(); }) { text = "SAVE TO GLOBAL DB", style = { backgroundColor = new Color(0, 0.4f, 0.2f), color = Color.white, height = 40, width = 180, unityFontStyleAndWeight = FontStyle.Bold, marginRight = 10 } });

            if (!_isCreatingNew)
                actionsRow.Add(new Button(() => { _onDeleteGlobal(workingItem); _selectedItem = null; RefreshAll(); }) { text = "DELETE ENTIRELY", style = { backgroundColor = new Color(0.5f, 0.1f, 0.1f), color = Color.white, height = 40, width = 150, unityFontStyleAndWeight = FontStyle.Bold } });

            _editorPane.Add(actionsRow);
        }

        private void PrepareNewItem()
        {
            _selectedItem = null;
            _isCreatingNew = true;
            RefreshAll();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_API.Scripts;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.GURPS
{
    public class BooksAPI : IGurpsApiProvider, IFilterWindowProvider
    {
        public int Id => 1001;
        public ProviderType ProviderType => ProviderType.GurpsApi;
        public string ModuleName => "Library & Books";
        public string Title => "GURPS Library Manager";

        private Dictionary<string, List<GURPSBook>> _allBooks = new();
        private Dictionary<string, FSMHandle> _bookHandles = new();

        private DataWarehouse _warehouse;
        private const string CacheKey = "DURPS_LIBRARY_DB";

        private GURPSBook _selectedBook;
        private ScrollView _leftListPanel;
        private VisualElement _rightEditorPanel;

        // Default constructor for Factory compatibility
        public BooksAPI() { }

        public BooksAPI(DataWarehouse warehouse) => _warehouse = warehouse;

        public void Bind(DataWarehouse warehouse) => _warehouse = warehouse;
        public List<GURPSBook> GetAll() => _allBooks.Values.SelectMany(x => x).ToList();

        /// <summary>
        /// Retrieves a rulebook by its exact name.
        /// </summary>
        public GURPSBook GetByName(string name) =>
            GetAll().FirstOrDefault(b => b.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public void Initialize()
        {
            DefineBookFSM();
            LoadFromCache();

            if (FSM_UnityIntegrationAdvanced.Instance != null)
            {
                FSM_UnityIntegrationAdvanced.Instance.AddProcessingGroup("EditorUpdate", "Rules");
            }
        }

        private void DefineBookFSM()
        {
            FSM_API.Create.CreateFiniteStateMachine("BookLifecycle", 0, "Rules")
                .State("Inactive", context => { }, null, null)
                .State("Indexing", context => { }, null, null)
                .State("Active", context => { }, null, null)
                .BuildDefinition();
        }

        public void LoadFromCache()
        {
            if (_warehouse != null && _warehouse.TryRetrieveTemporary(CacheKey, out string json))
            {
                var library = JsonUtility.FromJson<LibraryWrapper>(json);
                _allBooks = library?.Books ?? new();
            }

            if (_allBooks.Count == 0) SeedStandardLibrary();

            // Re-sync FSM Handles for all books in the dictionary
            _bookHandles.Clear();
            foreach (var book in _allBooks.Values.SelectMany(list => list))
            {
                var handle = FSM_API.Create.CreateInstance("BookLifecycle", book, "Rules");
                _bookHandles[book.Name] = handle;
            }
        }

        public void SaveToCache()
        {
            if (_warehouse == null) return;
            string json = JsonUtility.ToJson(new LibraryWrapper { Books = _allBooks }, true);
            _warehouse.StoreTemporary(CacheKey, json);
        }

        private void SeedStandardLibrary()
        {
            Register(new GURPSBook { Name = "GURPS Basic Set: Characters", SpecificityTier = 1, Category = "Core" });
            Register(new GURPSBook { Name = "GURPS Basic Set: Campaigns", SpecificityTier = 1, Category = "Core" });
        }

        public void Register(GURPSBook book)
        {
            if (book == null) return;
            string category = string.IsNullOrEmpty(book.Category) ? "Uncategorized" : book.Category;

            if (!_allBooks.ContainsKey(category)) _allBooks[category] = new List<GURPSBook>();
            if (!_allBooks[category].Contains(book)) _allBooks[category].Add(book);

            var handle = FSM_API.Create.CreateInstance("BookLifecycle", book, "Rules");
            _bookHandles[book.Name] = handle;
            SaveToCache();
        }

        public void UnRegister(GURPSBook book)
        {
            if (book == null || !_allBooks.ContainsKey(book.Category)) return;
            if (_allBooks[book.Category].Remove(book))
            {
                _bookHandles.Remove(book.Name);
                SaveToCache();
            }
        }

        // --- GUI GENERATION ---

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new ForgeContainerBuilder("BooksAPI_Root")
                .WithFlexGrow(1f)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.09f));

            rootBuilder.AddChild(new ForgeContainerBuilder("Header")
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithPadding(15f)
                .WithBorderBottomWidth(1)
                .WithBorderBottomColor(new Color(0.2f, 0.2f, 0.25f))
                .AddChild(new ForgeLabelBuilder(Title).WithFontSize(18).WithBold().WithColor(new Color(0.8f, 0.6f, 0.2f))));

            var splitBody = new ForgeContainerBuilder("SplitBody").WithDirection(FlexDirection.Row).WithFlexGrow(1f);

            var listPane = new ForgeContainerBuilder("ListPane")
                .WithWidth(new StyleLength(Length.Percent(30f)))
                .WithBorderRightWidth(1)
                .WithBorderRightColor(new Color(0.2f, 0.2f, 0.25f))
                .WithPadding(10f)
                .AddChild(new DynamicGuiProvider(c => {
                    var sv = new ForgeScrollViewBuilder().WithMode(ScrollViewMode.Vertical).CreateGui(c) as ScrollView;
                    _leftListPanel = sv;
                    return sv;
                }))
                .AddChild(new ForgeButtonBuilder("➕ Add Book")
                    .WithBackgroundColor(new Color(0.1f, 0.5f, 0.2f))
                    .OnClick(() => {
                        var newBook = new GURPSBook { Name = "New Book", Category = "Custom", SpecificityTier = 2 };
                        Register(newBook);
                        SelectBook(newBook);
                    }));

            var editorPane = new ForgeContainerBuilder("EditorPane").WithFlexGrow(1f).WithPadding(20f).OnBuild(ve => _rightEditorPanel = ve);

            splitBody.AddChild(listPane).AddChild(editorPane);
            rootBuilder.AddChild(splitBody);

            var root = rootBuilder.Build();
            RefreshBookList();
            return root;
        }

        private void RefreshBookList()
        {
            if (_leftListPanel == null) return;
            _leftListPanel.Clear();

            foreach (var kvp in _allBooks.OrderBy(x => x.Key))
            {
                _leftListPanel.Add(new ForgeLabelBuilder(kvp.Key).WithColor(Color.gray).WithBold().WithMarginTop(10f).Build());

                foreach (var book in kvp.Value)
                {
                    var b = book;
                    var handle = _bookHandles.GetValueOrDefault(b.Name);
                    string status = (handle != null && handle.CurrentState == "Active") ? "[LIVE] " : "[OFF] ";
                    var btnColor = b == _selectedBook ? new Color(0.3f, 0.3f, 0.4f) : new Color(0.15f, 0.15f, 0.18f);

                    _leftListPanel.Add(new ForgeButtonBuilder(status + b.Name)
                        .OnClick(() => SelectBook(b))
                        .WithBackgroundColor(btnColor)
                        .Build());
                }
            }
        }

        private void SelectBook(GURPSBook book)
        {
            _selectedBook = book;
            RefreshBookList();

            if (_rightEditorPanel == null || book == null) return;
            _rightEditorPanel.Clear();

            var handle = _bookHandles.GetValueOrDefault(book.Name);
            string currentState = handle?.CurrentState ?? "Unknown";

            var editor = new ForgeContainerBuilder("Editor")
                .AddChild(new ForgeLabelBuilder($"Editing: {book.Name}").WithFontSize(18).WithBold().WithMarginBottom(15f))
                .AddChild(new ForgeContainerBuilder("StatePanel")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithPadding(10f).WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f))
                    .AddChild(new ForgeLabelBuilder($"Status: {currentState}").WithColor(currentState == "Active" ? Color.green : Color.gray))
                    .AddChild(new ForgeButtonBuilder(currentState == "Active" ? "Deactivate" : "Active")
                        .OnClick(() => {
                            if (handle == null) return;
                            if (handle.CurrentState == "Active") handle.TransitionTo("Inactive");
                            else handle.TransitionTo("Active");
                            SelectBook(book);
                        })))
                .AddChild(new ForgeTextFieldBuilder("Book Name", book.Name).OnValueChanged(v => {
                    string oldName = book.Name;
                    book.Name = v.newValue;
                    if (_bookHandles.Remove(oldName, out var h)) _bookHandles[book.Name] = h;
                    RefreshBookList();
                }))
                .AddChild(new ForgeTextFieldBuilder("Category", book.Category).OnValueChanged(v => {
                    UnRegister(book);
                    book.Category = v.newValue;
                    Register(book);
                    RefreshBookList();
                }));

            _rightEditorPanel.Add(editor.Build());
        }

        private Guid _guid = Guid.NewGuid();
        public Vector2 position { get; set; }

        // FIXED: Properly return the _guid instead of throwing an exception
        Guid IGurpsApiProvider.Id { get => _guid; set => _guid = value; }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) => WorkshopUxmlBaker.Bake(CreateGui(new GuiContext()), "LibrarySnapshot");
        public void FromUIDocument(string path) { }

        public void CreateComponentTree(List<ForgeFilterWindow.Element> tree)
        {
            tree.Add(new ForgeFilterWindow.GroupElement(0, "GURPS Library"));
            foreach (var book in GetAll())
            {
                var handle = _bookHandles.GetValueOrDefault(book.Name);
                string status = (handle?.CurrentState == "Active") ? "[LIVE]" : "[OFF]";
                tree.Add(new ForgeFilterWindow.Element(1, $"{status} {book.Name}") { userData = book });
            }
        }

        public bool GoToChild(ForgeFilterWindow.Element el, bool add)
        {
            if (el.userData is GURPSBook book)
            {
                SelectBook(book);
                return true;
            }
            return false;
        }

        // --- THE FIX: Wrap Editor-Only Code ---
#if UNITY_EDITOR
        public void CreateComponentTree(List<UnityEditor.Rendering.FilterWindow.Element> tree) { }
        public bool GoToChild(UnityEditor.Rendering.FilterWindow.Element e, bool a) => false;
#endif

        [Serializable]
        private class LibraryWrapper { public Dictionary<string, List<GURPSBook>> Books = new(); }
    }
}
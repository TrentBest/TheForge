using TheSingularityWorkshop.Armada2525.GURPS;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Armada2525_Gui_GURPS_BookSelector : IGuiProvider
{
    public string Title => "Universe Library";
    public Action<GURPSBook> OnEditSourceRequested;
    private GURPSBook currentSelectedBook;
    private List<GURPSBook> includedBooks = new List<GURPSBook>();
    private GURPSUniverse _currentUniverse;
    private GuiContext _lastCtx;
    private VisualElement _mainContainer;

    public Armada2525_Gui_GURPS_BookSelector() { }

    public VisualElement CreateGui(GURPSUniverse uni)
    {
        DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem();
        _currentUniverse = uni ?? new GURPSUniverse { UniverseSeed = 42 };
        currentSelectedBook = DigitalGenericUniversalRolePlayingSystem.Books.GetAll().First(s => s.Name.Contains("Basic Set"));
        _lastCtx = new GuiContext();
        return CreateGui(_lastCtx);
    }

    public VisualElement CreateGui(GuiContext ctx)
    {
        _lastCtx = ctx;

        if (_currentUniverse == null)
        {
            DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem();
            _currentUniverse = new GURPSUniverse { UniverseSeed = 42 };
        }
        currentSelectedBook = DigitalGenericUniversalRolePlayingSystem.Books.GetAll().FirstOrDefault(s => s.Name.Contains("Basic Set"));
        _mainContainer = new VisualElement { style = { flexGrow = 1 } };
        ShowSelector();
        return _mainContainer;
    }

    private void ShowSelector()
    {
        _mainContainer.Clear();
        var rootScroll = new ScrollView { style = { paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10, flexGrow = 1 } };

        var searchField = new TextField("Search Library");
        _mainContainer.Add(searchField);
        _mainContainer.Add(rootScroll);

        var allBooks = DigitalGenericUniversalRolePlayingSystem.Books.GetAll();

        // MATCHING THE BOOK EDITOR GROUPINGS
        var groups = allBooks.GroupBy(b => b.Category).OrderBy(g => g.Key);

        foreach (var group in groups)
        {
            var groupContainer = new VisualElement { style = { marginBottom = 20 } };

            // Category Header
            var header = new Label(group.Key.ToUpper())
            {
                style = {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = Color.cyan,
                    borderBottomWidth = 1, borderBottomColor = Color.gray,
                    marginBottom = 10, paddingBottom = 2
                }
            };
            groupContainer.Add(header);

            foreach (var book in group)
            {
                var card = CreateDataCard(book, searchField);
                groupContainer.Add(card);
            }
            rootScroll.Add(groupContainer);
        }
    }

    private VisualElement CreateDataCard(GURPSBook book, TextField searchField)
    {
        bool isIncluded = _currentUniverse.IncludedRuleBooks.Contains(book.Name);

        // --- THE DATA CARD CONTAINER ---
        var card = new VisualElement
        {
            style = {
                flexDirection = FlexDirection.Row,
                marginBottom = 10,
                backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.9f),
                paddingTop = 10, paddingRight = 10, paddingLeft = 10, paddingBottom = 10,
                borderTopLeftRadius = 8, borderTopRightRadius = 8, borderBottomLeftRadius = 8, borderBottomRightRadius = 8,
                borderTopWidth = 2, borderBottomWidth = 2, borderLeftWidth = 2, borderRightWidth = 2,
                // Highlight border if included
                borderTopColor = isIncluded ? Color.green : Color.clear,
                borderBottomColor = isIncluded ? Color.green : Color.clear,
                borderLeftColor = isIncluded ? Color.green : Color.clear,
                borderRightColor = isIncluded ? Color.green : Color.clear,
            }
        };

        // --- 1. COVER IMAGE ---
        var img = new VisualElement { style = { width = 90, height = 130, marginRight = 15 } };

        // This will now use the specific CoverImagePath you defined in the Book Editor
        Texture2D coverTex = book.GetCoverImage();

        if (coverTex != null)
        {
            img.style.backgroundImage = new StyleBackground(coverTex);
            img.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            img.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
            img.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
            img.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
        }
        else
        {
            img.style.backgroundColor = Color.black;
            img.Add(new Label("NO\nCOVER") { style = { unityTextAlign = TextAnchor.MiddleCenter, flexGrow = 1, color = Color.white } });
        }

        // --- 2. READ-ONLY DATA (INFO COLUMN) ---
        var infoColumn = new VisualElement { style = { flexGrow = 1, justifyContent = Justify.FlexStart } };

        var nameLabel = new Label(book.Name.ToUpper()) { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 16, color = Color.white } };
        infoColumn.Add(nameLabel);

        // Display the exact same description as the editor, but in a wrapping label
        var descLabel = new Label(book.Description)
        {
            style = {
                whiteSpace = WhiteSpace.Normal, // Allows text wrapping
                color = Color.gray,
                marginTop = 8,
                flexGrow = 1,
                fontSize = 12
            }
        };
        infoColumn.Add(descLabel);

        // --- 3. ACTIONS & INCLUSION COLUMN ---
        var actionCol = new VisualElement { style = { width = 110, marginLeft = 10, justifyContent = Justify.SpaceBetween, alignItems = Align.FlexEnd } };

        var includeToggle = new Toggle("Included")
        {
            value = isIncluded,
            style = { marginBottom = 10, unityFontStyleAndWeight = FontStyle.Bold, color = isIncluded ? Color.green : Color.gray }
        };

        // Toggle Logic handles the data binding AND visual updates
        includeToggle.RegisterValueChangedCallback(e =>
        {
            bool included = e.newValue;
            includeToggle.style.color = included ? Color.green : Color.gray;

            if (included)
            {
                if (!_currentUniverse.IncludedRuleBooks.Contains(book.Name))
                    _currentUniverse.IncludedRuleBooks.Add(book.Name);

                // Highlight borders
                card.style.borderTopColor = Color.green; card.style.borderBottomColor = Color.green; card.style.borderLeftColor = Color.green; card.style.borderRightColor = Color.green;
            }
            else
            {
                _currentUniverse.IncludedRuleBooks.Remove(book.Name);

                // Clear borders
                card.style.borderTopColor = Color.clear; card.style.borderBottomColor = Color.clear; card.style.borderLeftColor = Color.clear; card.style.borderRightColor = Color.clear;
            }
        });

        var editBtn = new Button(() => ShowEditor(book))
        {
            text = "EDIT SOURCE",
            style = { height = 30, backgroundColor = new Color(0.2f, 0.4f, 0.6f), color = Color.white }
        };

        actionCol.Add(includeToggle);
        actionCol.Add(editBtn);

        // --- 4. CLICK-TO-SELECT LOGIC ---
        // Make clicking anywhere on the card toggle the selection, unless they click the Edit button
        card.RegisterCallback<ClickEvent>(e =>
        {
            if (e.target != includeToggle && !(e.target is Button) && !(e.target is TextElement))
            {
                includeToggle.value = !includeToggle.value;
                if (includeToggle.value)
                    includedBooks.Add(book);
                else
                    includedBooks.Remove(book);
            }
        });

        // Hover Effect
        card.RegisterCallback<MouseEnterEvent>(e => card.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.9f));
        card.RegisterCallback<MouseLeaveEvent>(e => card.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.9f));

        card.Add(img);
        card.Add(infoColumn);
        card.Add(actionCol);

        // Search filtering checks Name, Category, AND Description
        searchField.RegisterValueChangedCallback(e =>
        {
            bool match = book.Name.ToLower().Contains(e.newValue.ToLower()) ||
                         book.Category.ToLower().Contains(e.newValue.ToLower()) ||
                         book.Description.ToLower().Contains(e.newValue.ToLower());
            card.style.display = match ? DisplayStyle.Flex : DisplayStyle.None;
        });

        return card;
    }

    private void ShowEditor(GURPSBook book)
    {
        _mainContainer.Clear();

        var backBtn = new Button(ShowSelector)
        {
            text = "< BACK TO MULTIVERSE LIBRARY",
            style = { backgroundColor = Color.black, color = Color.yellow, marginBottom = 10, height = 30 }
        };
        _mainContainer.Add(backBtn);

        // CONTEXT BINDING: Set active so the BookEditor automatically opens on this book
        currentSelectedBook = book;

        var editor = new Armada2525_Gui_GURPS_BookEditor();
        var editorGui = editor.CreateGui(_lastCtx ?? new GuiContext());

        _mainContainer.Add(editorGui);

        OnEditSourceRequested?.Invoke(book);
    }

    public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    public void FromUIDocument(string path) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(path));
    public void ToUIDocument(string assetPath) { /* Editor only */ }
}
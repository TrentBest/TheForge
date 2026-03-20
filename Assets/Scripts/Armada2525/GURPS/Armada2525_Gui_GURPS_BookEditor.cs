using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Armada2525.GURPS;

public class Armada2525_Gui_GURPS_BookEditor : IGuiProvider
{
    public string Title => "GURPS BOOK EDITOR";
    private GuiContext _lastCtx;
    private CRUD_Builder<GURPSBook> _crudInterface;

    public VisualElement CreateGui(GuiContext ctx)
    {
        _lastCtx = ctx;

        // Ensure the system is booted BEFORE we try to bind the data
        DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem();

        

        _crudInterface = new CRUD_Builder<GURPSBook>(
            title: "GLOBAL LIBRARY DB",
            dataSource: () => DigitalGenericUniversalRolePlayingSystem.Books.GetAll(),
            getDisplayName: (book) => book.Name,
            getGroupCategory: (book) => string.IsNullOrEmpty(book.Category) ? "Uncategorized" : book.Category,

            buildEditorForm: (book) =>
            {
                Debug.Log($"Book:  {book.Name}");
                VisualElement coverPreview = null;

                Action<string> refreshCoverImage = (path) => {
                    if (coverPreview == null) return;
                    if (string.IsNullOrEmpty(path)) { coverPreview.style.backgroundImage = null; return; }
                    Texture2D tex = Resources.Load<Texture2D>(path);
                    if (tex != null)
                    {
                        coverPreview.style.backgroundImage = new StyleBackground(tex);

                        // FIXED: Replaced obsolete unityBackgroundScaleMode with new CSS properties
                        coverPreview.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                        coverPreview.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
                        coverPreview.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                        coverPreview.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
                    }
                    else { coverPreview.style.backgroundImage = null; }
                };

                // PURIFIED BUILDER SYNTAX
                var formBuilder = new GraphicalUserInterfaceBuilder("EditorForm")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.FlexStart)

                    // --- LEFT SIDE: COVER IMAGE ---
                    .AddChild(new GraphicalUserInterfaceBuilder("CoverColumn")
                        .WithWidth(180).WithMarginRight(20)
                        .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Center)
                        .AddChild(c => {
                            coverPreview = new VisualElement
                            {
                                style = {
                                    width = 160, height = 210, marginBottom = 10, backgroundColor = new Color(0.15f, 0.15f, 0.15f),
                                    borderTopWidth = 2, borderBottomWidth = 2, borderLeftWidth = 2, borderRightWidth = 2,
                                    borderTopColor = Color.gray, borderBottomColor = Color.gray, borderLeftColor = Color.gray, borderRightColor = Color.gray
                                }
                            };
                            refreshCoverImage(book.CoverImagePath);
                            return coverPreview;
                        })
                        .AddChild(new Label("RESOURCES PATH") { style = { color = Color.gray, fontSize = 10, marginBottom = 2 } })
                        .AddChild(new GraphicalUserInterfaceBuilder("PathControls")
                            .WithFlexLayout(FlexDirection.Row).WithWidth(180)
                            .AddChild(c => {
                                var pathField = new TextField() { value = book.CoverImagePath, style = { flexGrow = 1 } };
                                pathField.RegisterValueChangedCallback(e => { book.CoverImagePath = e.newValue; refreshCoverImage(e.newValue); });
                                return pathField;
                            })
                            .AddChild(c => {
                                return new Button(() => {
#if UNITY_EDITOR
                                    string startDir = Application.dataPath + "/Resources";
                                    string selectedPath = UnityEditor.EditorUtility.OpenFilePanel("Select Cover Image", startDir, "png,jpg,jpeg");
                                    if (!string.IsNullOrEmpty(selectedPath))
                                    {
                                        selectedPath = selectedPath.Replace('\\', '/');
                                        int resIndex = selectedPath.IndexOf("/Resources/", StringComparison.OrdinalIgnoreCase);
                                        if (resIndex >= 0)
                                        {
                                            string relPath = selectedPath.Substring(resIndex + 11); // + length of "/Resources/"
                                            relPath = System.IO.Path.ChangeExtension(relPath, null);
                                            book.CoverImagePath = relPath;
                                            refreshCoverImage(relPath);
                                        }
                                    }
#endif
                                })
                                { text = "..", style = { width = 30, backgroundColor = new Color(0.2f, 0.2f, 0.4f), color = Color.white } };
                            })
                        )
                    )

                    // --- RIGHT SIDE: METADATA ---
                    .AddChild(new GraphicalUserInterfaceBuilder("MetadataColumn")
                        .WithAutoGrow().WithFlexLayout(FlexDirection.Column)
                        .AddChild(c => CreateStyledTextField("Book Title", book.Name, v => book.Name = v))
                        .AddChild(c => CreateStyledTextField("Category", book.Category, v => book.Category = v))
                        .AddChild(c => CreateStyledIntField("Specificity Tier", book.SpecificityTier, v => book.SpecificityTier = v))
                        .AddChild(c => CreateStyledIntField("Load Order", book.LoadOrder, v => book.LoadOrder = v))
                        .AddChild(c => CreateStyledIntField("Page Count", book.PageCount, v => book.PageCount = v, 20))
                        .AddChild(c => {
                            var descField = new TextField("Description") { value = book.Description, multiline = true, style = { marginBottom = 10, height = 80 } };
                            descField.Q<Label>().style.minWidth = 150; descField.Q<Label>().style.color = Color.gray;
                            descField.RegisterValueChangedCallback(e => book.Description = e.newValue);
                            return descField;
                        })
                    );

                return formBuilder.Build();
            },
            onSave: (book) => { DigitalGenericUniversalRolePlayingSystem.Books.Register(book); },
            onDelete: (book) => {
                DigitalGenericUniversalRolePlayingSystem.Books.GetAll().ToList().Remove(book);
                DigitalGenericUniversalRolePlayingSystem.Books.SaveToCache();
            },
            getSubtitle: (book) => $"Tier: {book.SpecificityTier} | Pages: {book.PageCount}"
        );

        _crudInterface.PresentationMode = CrudPresentationMode.Tree;
        return _crudInterface.CreateGui(ctx);
    }

    // Helper methods to keep the builder visually clean!
    private TextField CreateStyledTextField(string label, string value, Action<string> onValueChanged)
    {
        var field = new TextField(label) { value = value, style = { marginBottom = 10 } };
        field.Q<Label>().style.minWidth = 150; field.Q<Label>().style.color = Color.gray;
        field.RegisterValueChangedCallback(e => onValueChanged(e.newValue));
        return field;
    }

    private IntegerField CreateStyledIntField(string label, int value, Action<int> onValueChanged, int marginBottom = 10)
    {
        var field = new IntegerField(label) { value = value, style = { marginBottom = marginBottom } };
        field.Q<Label>().style.minWidth = 150; field.Q<Label>().style.color = Color.gray;
        field.RegisterValueChangedCallback(e => onValueChanged(e.newValue));
        return field;
    }

    public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
    public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
#if UNITY_EDITOR
    public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
}
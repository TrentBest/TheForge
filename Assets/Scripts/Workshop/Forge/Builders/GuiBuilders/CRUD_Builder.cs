using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public enum CrudPresentationMode { List, Tree, Grid }

    public class CrudStyle
    {
        public Color RootBackground = new Color(0.1f, 0.1f, 0.12f);
        public Color ListBackground = new Color(0.12f, 0.12f, 0.15f);
        public Color SelectedItemBackground = new Color(0.2f, 0.2f, 0.25f);
        public Color BorderColor = Color.gray;
        public Color AccentColor = Color.cyan;

        public float ListWidth = 300f;
        public float HeaderPadding = 15f;
        public float ItemPadding = 8f;
        public float ActionButtonWidth = 150f;
        public float ActionButtonHeight = 40f;
        public float TreeIndent = 15f;
    }

    public class CRUD_Builder<T> : IForgeBuilder where T : class, new()
    {
        public string Title { get; set; }
        private GuiContext _lastCtx;
        public CrudStyle Style { get; set; } = new CrudStyle();

        // --- NEW: Presentation Mode ---
        public CrudPresentationMode PresentationMode { get; set; } = CrudPresentationMode.List;

        public string ToolName => $"{Title}-CRUD";

        // Changed to IEnumerable to prevent crashes with LINQ queries like .GetAll()
        private Func<IEnumerable<T>> _dataSource;
        private Func<T, string> _getDisplayName;
        private Func<T, string> _getSubtitle;
        private Func<T, string> _getGroupCategory;
        private Func<T, VisualElement> _buildEditorForm;

        private Action<T> _onSave;
        private Action<T> _onDelete;

        private T _selectedItem;
        private bool _isCreatingNew;

        // Using explicit ScrollViews fixes UI Toolkit cutoff bugs
        private ScrollView _listContainerRoot;
        private ScrollView _editorContainerRoot;

        public CRUD_Builder(
            string title,
            Func<IEnumerable<T>> dataSource,
            Func<T, string> getDisplayName,
            Func<T, VisualElement> buildEditorForm,
            Action<T> onSave,
            Action<T> onDelete,
            Func<T, string> getSubtitle = null,
            Func<T, string> getGroupCategory = null)
        {
            Title = title;
            _dataSource = dataSource;
            _getDisplayName = getDisplayName;
            _buildEditorForm = buildEditorForm;
            _onSave = onSave;
            _onDelete = onDelete;
            _getSubtitle = getSubtitle;
            _getGroupCategory = getGroupCategory;

            if (_getGroupCategory != null) PresentationMode = CrudPresentationMode.Tree;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var rootBuilder = new GraphicalUserInterfaceBuilder($"{typeof(T).Name}_CRUD_Root")
                .WithBackgroundColor(Style.RootBackground)
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100);

            // --- LEFT PANE: THE LIST/TREE/GRID ---
            rootBuilder.AddChild(new GraphicalUserInterfaceBuilder("ListPane")
                .WithWidth((int)Style.ListWidth)
                .WithBackgroundColor(Style.ListBackground)
                .WithBorderRightWidth(1)
                .WithBorderRightColor(Style.BorderColor)
                .AddChild(new GraphicalUserInterfaceBuilder("ListHeader")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithPadding((int)Style.HeaderPadding)
                    .WithBorderBottomWidth(1)
                    .WithBorderBottomColor(Style.BorderColor)
                    .AddChild(new Label(Title) { style = { color = Color.white, fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold } })
                    .AddButton("+ NEW", PrepareNewItem)
                )
                // Explicitly inject a ScrollView to guarantee scrolling isn't stripped by flex rules
                .AddChild(c => {
                    _listContainerRoot = new ScrollView { style = { flexGrow = 1 } };
                    return _listContainerRoot;
                })
            );

            // --- RIGHT PANE: THE EDITOR ---
            rootBuilder.AddChild(new GraphicalUserInterfaceBuilder("EditorPane")
                .WithAutoGrow()
                .WithBackgroundColor(Style.RootBackground)
                .AddChild(c => {
                    _editorContainerRoot = new ScrollView { style = { flexGrow = 1, paddingBottom = 20, paddingTop = 20, paddingLeft = 20, paddingRight = 20 } };
                    return _editorContainerRoot;
                })
            );

            var root = rootBuilder.Build();
            RefreshAll();
            return root;
        }

        private void RefreshAll()
        {
            RenderList();
            RenderEditor();
        }

        private void RenderList()
        {
            if (_listContainerRoot == null) return;
            _listContainerRoot.Clear();

            // Configure the ScrollView's internal flexbox based on Presentation Mode
            if (PresentationMode == CrudPresentationMode.Grid)
            {
                _listContainerRoot.contentContainer.style.flexDirection = FlexDirection.Row;
                _listContainerRoot.contentContainer.style.flexWrap = Wrap.Wrap;
            }
            else
            {
                _listContainerRoot.contentContainer.style.flexDirection = FlexDirection.Column;
                _listContainerRoot.contentContainer.style.flexWrap = Wrap.NoWrap;
            }

            var data = _dataSource()?.ToList() ?? new List<T>();

            if (PresentationMode == CrudPresentationMode.Tree && _getGroupCategory != null)
            {
                var groupedData = data.GroupBy(_getGroupCategory).OrderBy(g => g.Key);
                foreach (var group in groupedData)
                {
                    var foldout = new Foldout { text = group.Key, style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 5 } };
                    foreach (var item in group) foldout.Add(CreateItemPanel(item, true).Build());
                    _listContainerRoot.Add(foldout);
                }
            }
            else
            {
                foreach (var item in data) _listContainerRoot.Add(CreateItemPanel(item, false).Build());
            }
        }

        private GraphicalUserInterfaceBuilder CreateItemPanel(T item, bool isChildNode)
        {
            bool isSelected = _selectedItem == item;

            var builder = new GraphicalUserInterfaceBuilder($"Item_{_getDisplayName(item)}")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithPadding((int)Style.ItemPadding)
                .WithBackgroundColor(isSelected ? Style.SelectedItemBackground : Color.clear)
                .WithBorderBottomWidth(1)
                .WithBorderBottomColor(new Color(0.2f, 0.2f, 0.2f))
                .WithBorderLeftWidth(isSelected ? 4 : 0)
                .WithBorderLeftColor(Style.AccentColor)
                .WithMarginLeft(isChildNode ? (int)Style.TreeIndent : 0)
                .AddChild(new Label(_getDisplayName(item)) { style = { color = isSelected ? Style.AccentColor : Color.white, unityFontStyleAndWeight = isSelected ? FontStyle.Bold : FontStyle.Normal } })
                .OnBuild(ve => {
                    ve.style.flexShrink = 0; // Prevent UI Toolkit from squishing lists
                    ve.RegisterCallback<ClickEvent>(evt => {
                        _selectedItem = item;
                        _isCreatingNew = false;
                        RefreshAll();
                    });
                });

            // Add subtitle if in List/Tree mode
            if (_getSubtitle != null && PresentationMode != CrudPresentationMode.Grid)
            {
                builder.AddChild(new Label(_getSubtitle(item)) { style = { color = Color.gray, fontSize = 10 } });
            }

            return builder;
        }

        private void RenderEditor()
        {
            if (_editorContainerRoot == null) return;
            _editorContainerRoot.Clear();

            if (_selectedItem == null && !_isCreatingNew)
            {
                _editorContainerRoot.Add(new Label("Select an item to begin.") { style = { color = Color.gray, alignSelf = Align.Center, marginTop = 100 } });
                return;
            }

            T workingItem = _isCreatingNew ? new T() : _selectedItem;

            var editorBuilder = new GraphicalUserInterfaceBuilder("EditorContent")
                .AddChild(new Label(_isCreatingNew ? $"CREATE {typeof(T).Name.ToUpper()}" : $"EDIT {typeof(T).Name.ToUpper()}") { style = { color = Color.white, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } })
                .AddChild(_buildEditorForm(workingItem))
                .AddSeparator(Style.BorderColor, 1)
                .AddChild(new GraphicalUserInterfaceBuilder("ActionRow")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                    .WithMarginTop(20)
                    .AddButton("SAVE TO DB", () => {
                        _onSave(workingItem);
                        _isCreatingNew = false;
                        _selectedItem = workingItem;
                        RefreshAll();
                    })
                    .AddChild(ctx => {
                        if (_isCreatingNew) return new VisualElement();
                        var delBtn = new Button(() => {
                            _onDelete(workingItem);
                            _selectedItem = null;
                            RefreshAll();
                        })
                        { text = "DELETE" };
                        delBtn.style.backgroundColor = new Color(0.5f, 0.1f, 0.1f);
                        delBtn.style.marginLeft = 10;
                        delBtn.style.width = Style.ActionButtonWidth;
                        delBtn.style.height = Style.ActionButtonHeight;
                        return delBtn;
                    })
                );

            _editorContainerRoot.Add(editorBuilder.Build());
        }

        private void PrepareNewItem()
        {
            _selectedItem = null;
            _isCreatingNew = true;
            RefreshAll();
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public Type GetProductType() => typeof(T);
        public object Build() => null;
        public IGuiProvider GetGuiProvider() => null;
    }
}
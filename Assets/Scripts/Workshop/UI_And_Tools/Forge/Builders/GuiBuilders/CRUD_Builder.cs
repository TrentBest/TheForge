using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public enum CrudPresentationMode { List, Tree, Grid }

    public class CrudStyle
    {
        public Color RootBackground = new Color(0.1f, 0.1f, 0.12f);
        public Color ListBackground = new Color(0.12f, 0.12f, 0.15f);
        public Color SelectedItemBackground = new Color(0.2f, 0.2f, 0.25f);
        public Color HoverItemBackground = new Color(0.16f, 0.16f, 0.20f);
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

        public CrudPresentationMode PresentationMode { get; set; } = CrudPresentationMode.List;
        public string ToolName => $"{Title}-CRUD";

        private string LogPrefix => $"[CRUD<{typeof(T).Name}>]";

        // --- Core Delegates ---
        private Func<IEnumerable<T>> _dataSource;
        private Func<T, string> _getDisplayName;
        private Func<T, string> _getSubtitle;
        private Func<T, string> _getGroupCategory;
        private Func<T, VisualElement> _buildEditorForm;

        private Action<T> _onSave;
        private Action<T> _onDelete;

        // --- Extensibility & "Abuse" Hooks ---
        public Func<T> OnCreateNewItem { get; set; }
        public Func<T, bool> OnValidateSave { get; set; }
        public Func<T, bool> CanDelete { get; set; }
        public Action<T> OnItemSelected { get; set; }
        private List<Func<T, IGuiProvider>> _customActionInjectors = new List<Func<T, IGuiProvider>>();

        // State
        private T _selectedItem;
        private bool _isCreatingNew;

        // Captured Native Containers (Extracted from Fluent Builders)
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

            Debug.Log($"{LogPrefix} Initialized. Title: '{Title}', Mode: {PresentationMode}");
        }

        /// <summary>
        /// Allows external tools to inject custom action buttons into the Editor's action row (e.g. Duplicate, Print, Export)
        /// </summary>
        public CRUD_Builder<T> WithCustomAction(Func<T, IGuiProvider> actionButtonProvider)
        {
            _customActionInjectors.Add(actionButtonProvider);
            return this;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            Debug.Log($"{LogPrefix} 🟢 CreateGui started.");
            _lastCtx = ctx;

            var rootBuilder = new GraphicalUserInterfaceBuilder($"{typeof(T).Name}_CRUD_Root")
                .WithBackgroundColor(Style.RootBackground)
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100);

            // --- LEFT PANE: THE LIST/TREE/GRID ---
            rootBuilder.AddChild(new GraphicalUserInterfaceBuilder("ListPane")
                .WithWidth((int)Style.ListWidth)
                .WithFlexShrink(0)
                .WithBackgroundColor(Style.ListBackground)
                .WithBorderRightWidth(1)
                .WithBorderRightColor(Style.BorderColor)
                .AddChild(new GraphicalUserInterfaceBuilder("ListHeader")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithPadding((int)Style.HeaderPadding)
                    .WithBorderBottomWidth(1)
                    .WithBorderBottomColor(Style.BorderColor)
                    .AddChild(new ForgeLabelBuilder(Title)
                        .WithColor(Color.white)
                        .WithFontSize(16)
                        .WithFontStyle(FontStyle.Bold))
                    .AddChild(new ForgeButtonBuilder("+ NEW")
                        .OnClick(PrepareNewItem)
                        .WithBackgroundColor(Style.AccentColor)
                        .WithTextColor(Color.black)
                        .WithFontStyle(FontStyle.Bold))
                )
                // Fully Fluent ScrollView Capture
                .AddChild(new GraphicalUserInterfaceBuilder("ListScrollWrapper")
                    .WithFlexGrow(1)
                    .WithScrollable(true)
                    .OnBuild(ve => _listContainerRoot = ve.Q<ScrollView>("gui-scrollview"))
                )
            );

            // --- RIGHT PANE: THE EDITOR ---
            rootBuilder.AddChild(new GraphicalUserInterfaceBuilder("EditorPane")
                .WithAutoGrow()
                .WithBackgroundColor(Style.RootBackground)
                // Fully Fluent ScrollView Capture
                .AddChild(new GraphicalUserInterfaceBuilder("EditorScrollWrapper")
                    .WithFlexGrow(1)
                    .WithScrollable(true)
                    .OnBuild(ve => {
                        _editorContainerRoot = ve.Q<ScrollView>("gui-scrollview");
                        if (_editorContainerRoot != null)
                        {
                            _editorContainerRoot.style.paddingBottom = 20;
                            _editorContainerRoot.style.paddingTop = 20;
                            _editorContainerRoot.style.paddingLeft = 20;
                            _editorContainerRoot.style.paddingRight = 20;
                        }
                    })
                )
            );

            var root = rootBuilder.Build();
            RefreshAll();
            return root;
        }

        public void RefreshAll()
        {
            Debug.Log($"{LogPrefix} 🔄 RefreshAll triggered.");
            RenderList();
            RenderEditor();
        }

        private void RenderList()
        {
            if (_listContainerRoot == null) return;
            _listContainerRoot.Clear();

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

            List<T> data = new List<T>();
            try
            {
                data = _dataSource()?.ToList() ?? new List<T>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} ❌ FATAL ERROR fetching Data Source: {ex.Message}");
                _listContainerRoot.Add(new ForgeLabelBuilder("Failed to load data.")
                    .WithColor(Color.red)
                    .WithMargin(10)
                    .Build());
                return;
            }

            if (PresentationMode == CrudPresentationMode.Tree && _getGroupCategory != null)
            {
                var groupedData = data.GroupBy(_getGroupCategory).OrderBy(g => g.Key);
                foreach (var group in groupedData)
                {
                    var foldoutBuilder = new ForgeFoldoutBuilder(group.Key, false)
                        .WithMarginTop(5);

                    foreach (var item in group)
                        foldoutBuilder.AddChild(CreateItemPanel(item, true).CreateGui(_lastCtx));

                    _listContainerRoot.Add(foldoutBuilder.Build());
                }
            }
            else
            {
                foreach (var item in data)
                    _listContainerRoot.Add(CreateItemPanel(item, false).Build());
            }
        }

        private GraphicalUserInterfaceBuilder CreateItemPanel(T item, bool isChildNode)
        {
            bool isSelected = _selectedItem == item;
            string displayName = "Unknown";

            try { displayName = _getDisplayName(item) ?? "Unnamed Item"; }
            catch { displayName = "[Error resolving name]"; }

            var builder = new GraphicalUserInterfaceBuilder($"Item_{displayName}")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithPadding((int)Style.ItemPadding)
                .WithBackgroundColor(isSelected ? Style.SelectedItemBackground : Color.clear)
                .WithBorderBottomWidth(1)
                .WithBorderBottomColor(new Color(0.2f, 0.2f, 0.2f))
                .WithBorderLeftWidth(isSelected ? 4 : 0)
                .WithBorderLeftColor(Style.AccentColor)
                .WithMarginLeft(isChildNode ? (int)Style.TreeIndent : 0)
                .AddChild(new ForgeLabelBuilder(displayName)
                    .WithColor(isSelected ? Style.AccentColor : Color.white)
                    .WithFontStyle(isSelected ? FontStyle.Bold : FontStyle.Normal))
                .OnBuild(ve => {
                    ve.style.flexShrink = 0;

                    if (!isSelected)
                    {
                        ve.RegisterCallback<MouseEnterEvent>(evt => ve.style.backgroundColor = Style.HoverItemBackground);
                        ve.RegisterCallback<MouseLeaveEvent>(evt => ve.style.backgroundColor = Color.clear);
                    }

                    ve.RegisterCallback<ClickEvent>(evt => {
                        Debug.Log($"{LogPrefix} 🖱️ Clicked item: {displayName}");
                        _selectedItem = item;
                        _isCreatingNew = false;
                        OnItemSelected?.Invoke(item); // Abusable Hook
                        RefreshAll();
                    });
                });

            if (_getSubtitle != null && PresentationMode != CrudPresentationMode.Grid)
            {
                try
                {
                    string subtitle = _getSubtitle(item);
                    if (!string.IsNullOrEmpty(subtitle))
                    {
                        builder.AddChild(new ForgeLabelBuilder(subtitle)
                            .WithColor(Color.gray)
                            .WithFontSize(10));
                    }
                }
                catch { /* Ignore subtitle errors */ }
            }

            return builder;
        }

        private void RenderEditor()
        {
            if (_editorContainerRoot == null) return;
            _editorContainerRoot.Clear();

            if (_selectedItem == null && !_isCreatingNew)
            {
                _editorContainerRoot.Add(new GraphicalUserInterfaceBuilder("EmptyState")
                    .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                    .WithMarginTop(100)
                    .AddChild(new ForgeLabelBuilder("Select an item to begin.")
                        .WithColor(Color.gray)
                        .WithFontStyle(FontStyle.Italic))
                    .Build());
                return;
            }

            // Hook for creating heavily pre-configured "New" items
            T workingItem = _isCreatingNew
                ? (OnCreateNewItem != null ? OnCreateNewItem() : new T())
                : _selectedItem;

            var editorBuilder = new GraphicalUserInterfaceBuilder("EditorContent")
                .AddChild(new ForgeLabelBuilder(_isCreatingNew ? $"CREATE {typeof(T).Name.ToUpper()}" : $"EDIT {typeof(T).Name.ToUpper()}")
                    .WithColor(Color.white)
                    .WithFontSize(24)
                    .WithFontStyle(FontStyle.Bold)
                    .WithMarginBottom(20));

            try
            {
                VisualElement formPayload = _buildEditorForm(workingItem);
                if (formPayload != null) editorBuilder.AddChild(formPayload);
                else Debug.LogWarning($"{LogPrefix} _buildEditorForm returned null.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogPrefix} ❌ FATAL ERROR building Form: {ex.Message}");
                editorBuilder.AddChild(new ForgeLabelBuilder($"Error building UI:\n{ex.Message}")
                    .WithColor(Color.red).WithWhiteSpace(WhiteSpace.Normal));
            }

            var actionRow = new GraphicalUserInterfaceBuilder("ActionRow")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .WithMarginTop(20);

            // SAVE BUTTON
            actionRow.AddChild(new ForgeButtonBuilder("💾 SAVE TO DB")
                .WithBackgroundColor(new Color(0.2f, 0.6f, 0.2f))
                .WithTextColor(Color.white)
                .WithFontStyle(FontStyle.Bold)
                .WithWidth(Style.ActionButtonWidth)
                .WithHeight(Style.ActionButtonHeight)
                .OnClick(() => {
                    if (OnValidateSave != null && !OnValidateSave(workingItem))
                    {
                        Debug.LogWarning($"{LogPrefix} ⚠️ Save rejected by validation hook.");
                        return;
                    }

                    try { _onSave(workingItem); } catch (Exception ex) { Debug.LogError($"{LogPrefix} ❌ Save failed: {ex.Message}"); }
                    _isCreatingNew = false;
                    _selectedItem = workingItem;
                    RefreshAll();
                }));

            // DELETE BUTTON
            if (!_isCreatingNew && (CanDelete == null || CanDelete(workingItem)))
            {
                actionRow.AddChild(new ForgeButtonBuilder("🗑️ DELETE")
                    .WithBackgroundColor(new Color(0.6f, 0.1f, 0.1f))
                    .WithTextColor(Color.white)
                    .WithFontStyle(FontStyle.Bold)
                    .WithMarginLeft(10)
                    .WithWidth(Style.ActionButtonWidth)
                    .WithHeight(Style.ActionButtonHeight)
                    .OnClick(() => {
                        try { _onDelete(workingItem); } catch (Exception ex) { Debug.LogError($"{LogPrefix} ❌ Delete failed: {ex.Message}"); }
                        _selectedItem = null;
                        RefreshAll();
                    }));
            }

            // CUSTOM INJECTED ACTIONS
            foreach (var customAction in _customActionInjectors)
            {
                var customProvider = customAction?.Invoke(workingItem);
                if (customProvider != null)
                {
                    actionRow.AddChild(new GraphicalUserInterfaceBuilder().WithMarginLeft(10).AddChild(customProvider));
                }
            }

            editorBuilder.AddSeparator(Style.BorderColor, 1);
            editorBuilder.AddChild(actionRow);

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
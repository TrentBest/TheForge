using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Core;

namespace Workshop.UI_And_Tools.Forge.Tools
{
    /// <summary>
    /// Forge Builder for the Incinerator.
    /// Manages the visual interface for objects currently held in Stasis/Trash.
    /// </summary>
    public class Workshop_Gui_TrashBin : MonoBehaviour, IForgeBuilder
    {
        public string ToolName => "Incinerator";
        public string Title => ToolName;
        public Type GetProductType() => typeof(TrashBinGuiProvider);

        public IGuiProvider GetGuiProvider() => new TrashBinGuiProvider();

        private void OnEnable() => BuilderRegistry.Register(this);

        /// <summary>
        /// Returns the manifestation provider for the Incinerator.
        /// </summary>
        public object Build() => GetGuiProvider();
    }

    /// <summary>
    /// The concrete GUI Provider refactored for the Forge Builder Ecosystem.
    /// Orchestrates the list of trashed entities and their restoration to reality.
    /// </summary>
    public class TrashBinGuiProvider : IGuiProvider
    {
        public string Title => "Forge Incinerator";
        private VisualElement _listContainer;
        private GuiContext _activeCtx;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _activeCtx = ctx;

            // ROOT: Forge Container using the standard window background and padding
            var root = new ForgeContainerBuilder("TrashBin_Root")
                .WithPadding(15)
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f, 1.0f));

            // 1. Header & Global Actions
            root.AddChild(new ForgeLabelBuilder("THE INCINERATOR")
                .WithColor(Color.red).WithBold().WithFontSize(18).WithMarginBottom(10));

            root.AddChild(new ForgeButtonBuilder("🔥 EMPTY TRASH", ForgeTrashBin.Empty)
                .WithBackgroundColor(new Color(0.4f, 0.1f, 0.1f, 1.0f))
                .WithTextColor(Color.white).WithBold().WithHeight(30));

            root.AddSeparator(Color.gray, 1);

            // 2. The Trashed Objects List container
            root.OnBuild(ve =>
            {
                _listContainer = new VisualElement { name = "IncineratorList" };
                _listContainer.style.flexGrow = 1;
                _listContainer.style.marginTop = 10;
                ve.Add(_listContainer);
                RefreshList();
            });

            // Subscription Management
            ForgeTrashBin.OnTrashUpdated += RefreshList;

            var finalRoot = root.CreateGui(ctx);

            // Teardown to prevent memory leaks or dual-updates
            finalRoot.RegisterCallback<DetachFromPanelEvent>(e =>
            {
                ForgeTrashBin.OnTrashUpdated -= RefreshList;
            });

            return finalRoot;
        }

        private void RefreshList()
        {
            if (_listContainer == null) return;
            _listContainer.Clear();

            if (ForgeTrashBin.TrashedObjects.Count == 0)
            {
                _listContainer.Add(new ForgeLabelBuilder("The bin is empty.")
                    .WithColor(Color.gray).WithFontStyle(FontStyle.Italic)
                    .WithTextAlign(TextAnchor.MiddleCenter).WithMarginTop(20).Build());
                return;
            }

            foreach (var obj in ForgeTrashBin.TrashedObjects)
            {
                if (obj == null) continue;

                // ITEM ROW: Refactored to use ForgeContainerBuilder and ForgeButtonBuilder
                var row = new ForgeContainerBuilder($"Row_{obj.GetInstanceID()}")
                    .WithDirection(FlexDirection.Row)
                    .WithJustifyContent(Justify.SpaceBetween)
                    .WithAlignItems(Align.Center)
                    .WithBackgroundColor(new Color(0.15f, 0.15f, 0.15f, 1.0f))
                    .WithPadding(5).WithMarginBottom(5).WithBorderRadius(3)
                    .AddChild(new ForgeLabelBuilder(obj.name).WithColor(Color.white).WithFlexGrow(1))
                    .AddChild(new ForgeButtonBuilder("♻ Restore", () => ForgeTrashBin.Restore(obj))
                        .WithWidth(80).WithHeight(22).WithFontSize(10));

                _listContainer.Add(row.CreateGui(_activeCtx));
            }
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        /// <summary>
        /// Bakes the current state of the Incinerator list into a UXML asset.
        /// </summary>
        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(_activeCtx ?? new GuiContext());
            string fileName = string.IsNullOrEmpty(assetPath) ? "Incinerator_Bake" : System.IO.Path.GetFileNameWithoutExtension(assetPath);
            WorkshopUxmlBaker.Bake(root, fileName);
        }

        public void FromUIDocument(string assetPath)
        {
            Debug.Log($"[Incinerator] Hydration from {assetPath} requested. Directives synced.");
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Core;
using Workshop.UI_And_Tools.Forge.Hermit.UI;

namespace Workshop.GURPS
{
    public class GURPS_BookEditor : IGuiProvider
    {
        public string Title => "GURPS BOOK EDITOR";
        private GuiContext _lastCtx;
        private GURPS_CRUD_Builder<GURPSBook> _crudInterface;
        private DataWarehouse _warehouse;

        public GURPS_BookEditor()
        {
            _warehouse = new DataWarehouse();
            DigitalGenericUniversalRolePlayingSystem.InitializeCoreSystem(_warehouse);
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            _crudInterface = new GURPS_CRUD_Builder<GURPSBook>(
                title: "GLOBAL LIBRARY DB",
                getGlobalData: () => DigitalGenericUniversalRolePlayingSystem.Books.GetAll(),
                getDisplayName: (book) => book.Name,
                getSourceBook: (book) => book.Category,
                setSourceBook: (book, cat) => book.Category = cat,
                buildEditorForm: (book) => BuildBookForm(book),
                onSaveGlobal: (book) => DigitalGenericUniversalRolePlayingSystem.Books.Register(book),
                onDeleteGlobal: (book) => DigitalGenericUniversalRolePlayingSystem.Books.UnRegister(book),
                getSubtitle: (book) => $"Tier: {book.SpecificityTier} | Pages: {book.PageCount}"
            );

            var root = _crudInterface.CreateGui(ctx);

            // Add Hermit CORTEX Link
            new GraphicalUserInterfaceBuilder("BookHermit")
                .WithHermit(HermitSettings.LoadOrCreate())
                .OnBuild(ve => root.Add(ve))
                .Build();

            return root;
        }

        private VisualElement BuildBookForm(GURPSBook book)
        {
            var form = new ForgeContainerBuilder("BookForm_Root")
                .WithPadding(15)
                .WithBackgroundColor(new Color(0.05f, 0.02f, 0.06f)) // Void theme
                .WithBorderColor(Color.magenta)
                .WithBorderWidth(1)
                // Add a stylistic header
                .AddChild(new ForgeLabelBuilder("ARCHIVE ENTRY: VOLUME METADATA")
                    .WithBold()
                    .WithColor(Color.cyan)
                    .WithFontSize(14)
                    .WithMarginBottom(15)
                    .OnBuild(ve => ve.style.letterSpacing = 2))
                // The actual form fields
                .AddChild(new ForgeTextFieldBuilder("Volume Title", book.Name)
                    .WithMarginBottom(10)
                    .OnChanged(v => book.Name = v))
                .AddChild(new ForgeTextFieldBuilder("Classification", book.Category)
                    .WithMarginBottom(10)
                    .OnChanged(v => book.Category = v));

            return form.CreateGui(_lastCtx);
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#else
        public void ToUIDocument(string assetPath) { }
#endif
    }
}
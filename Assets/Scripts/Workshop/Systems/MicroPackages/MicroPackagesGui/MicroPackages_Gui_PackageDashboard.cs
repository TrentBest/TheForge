using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Showcase.MicroPackagesGui
{
    public class MicroPackages_Gui_PackageDashboard : IGuiProvider
    {
        public string Title => "MicroPackage Command Center";

        private GuiContext _activeContext;
        private VisualElement _contentContainer;

        // Global state for the dashboard
        private DynamicMicroPackage _activePackage;

        // 1. Core Paradigm: Parameterless Safe Default Constructor
        public MicroPackages_Gui_PackageDashboard() { }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        public VisualElement CreateGui(GuiContext ctx)
        {
            _activeContext = ctx;
            _contentContainer = new VisualElement { style = { flexGrow = 1, marginTop = 10 } };

            // 2. The CRUD Ribbon: Added the 'Package Forge' back into the loop
            var ribbon = new ForgeRibbonMenuBuilder("Ecosystem", (tabName) =>
            {
                if (tabName == "Ecosystem") LoadTab(BuildEcosystemView());
                if (tabName == "Package Forge") LoadTab(BuildForgeView());
                if (tabName == "Inspector") LoadTab(BuildInspectorView());
                if (tabName == "Test Chamber") LoadTab(BuildTestChamberView());
            })
            .AddTab("Ecosystem", new Color(0.2f, 0.4f, 0.2f))       // Read / Delete
            .AddTab("Package Forge", new Color(0.5f, 0.4f, 0.1f))   // Create / Update
            .AddTab("Inspector", new Color(0.2f, 0.3f, 0.5f))       // Deep Read
            .AddTab("Test Chamber", new Color(0.5f, 0.2f, 0.2f));   // Execute

            var rootLayout = new GraphicalUserInterfaceBuilder("MicroPackageDashboardRoot")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.12f, 1f))
                .WithPadding(10)
                .AddChild(new ForgeLabelBuilder("MicroPackage Forge & Operations")
                    .WithFontSize(20)
                    .WithFontStyle(FontStyle.Bold)
                    .WithColor(Color.white)
                    .WithMarginBottom(10))
                .AddChild(ribbon)
                .CreateGui(ctx);

            rootLayout.Add(_contentContainer);

            // Safe Default: Always start on Ecosystem
            LoadTab(BuildEcosystemView());

            return rootLayout;
        }

        private void LoadTab(IGuiProvider tabView)
        {
            _contentContainer.Clear();
            if (tabView != null)
            {
                _contentContainer.Add(tabView.CreateGui(_activeContext));
            }
        }

        // --- CRUD ROUTING VIEWS ---

        private IGuiProvider BuildEcosystemView()
        {
            var root = new GraphicalUserInterfaceBuilder("EcosystemWrapper")
                .WithFlexLayout(FlexDirection.Column);

            // Adding a Quick-Action row above the Cache Builder for pure CRUD flow
            var actionRow = new GraphicalUserInterfaceBuilder("ActionRow")
                .WithFlexLayout(FlexDirection.Row)
                .WithMarginBottom(10)
                .AddChild(new ForgeButtonBuilder("Create New Package", () => LoadTab(BuildForgeView()))
                    .WithBackgroundColor(new Color(0.5f, 0.4f, 0.1f))
                    .WithTextColor(Color.white)
                    .WithMarginRight(10))
                .AddChild(new ForgeLabelBuilder("Select a package below to Inspect or Test it.")
                    .WithColor(Color.gray)
                    .WithFontSize(12)
                    .WithMarginTop(5)); // Aligning with the button

            root.AddChild(actionRow);

            // The Gondola List (Read / Delete)
            var cacheBuilder = new Workshop_CacheBuilder<DynamicMicroPackage>(
                title: "Local Gondola Yard",
                cacheKey: "MicroPackages",
                onItemSelected: (selectedPkg) =>
                {
                    _activePackage = selectedPkg;
                    Debug.Log($"[Dashboard] Selected Package: {_activePackage.PackageId}.");
                },
                onFetchAll: () => { return SingularityPackageManager.GetLocalPackages(); }, // TODO: Wire to DataWarehouse / IO
                onSaveItem: (pkg) => { },
                onClearTemporaryCache: () => { }
            );

            root.AddChild(cacheBuilder);
            return root;
        }

        private IGuiProvider BuildForgeView()
        {
            // CREATE / UPDATE
            // If we have an active package, we should theoretically inject it here to edit it.
            var forge = new MicroPackages_GuiBuilder();

            // Note: If you add a "LoadExisting(DynamicMicroPackage)" method to your BuilderGui, 
            // you would call it right here: 
            // if (_activePackage != null) forge.LoadExisting(_activePackage);

            return forge;
        }

        private IGuiProvider BuildInspectorView()
        {
            // READ (Deep)
            // Utilizes the safe parameterless constructor we added, then injects state
            var inspector = new MicroPackages_Gui_MicroPackageInspector();
            inspector.SetTargetPackage(_activePackage);
            return inspector;
        }

        private IGuiProvider BuildTestChamberView()
        {
            // EXECUTE / VERIFY
            // Utilizes the safe parameterless constructor, handles null gracefully
            var tester = new MicroPackages_TestChamber();
            tester.SetTargetPackage(_activePackage);
            return tester;
        }

        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;
// using Newtonsoft.Json; // Assuming you use this for serialization

namespace Workshop.Systems.MicroPackages
{
    public class MicroPackageNexusGui : IGuiProvider
    {
        public string Title => "MicroPackage Nexus Forge";

        private VisualElement _contentContainer;
        private GuiContext _activeContext;

        public MicroPackageNexusGui(IGuiRouter r)
        {
        }

        // Where we store the local physical files
        private string CacheDirectory => Path.Combine(Application.persistentDataPath, "MicroPackages_Cache");

        public Action<VisualElement> GetGuiBuilder()
        {
            return (root) => root.Add(CreateGui(new GuiContext()));
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _activeContext = ctx;

            if (!Directory.Exists(CacheDirectory)) Directory.CreateDirectory(CacheDirectory);

            _contentContainer = new VisualElement { style = { flexGrow = 1, paddingBottom = 10, paddingTop = 10 } };

            var ribbon = new ForgeRibbonMenuBuilder("Ecosystem", (tabName) =>
            {
                if (tabName == "Ecosystem") LoadTool(BuildEcosystemView());
                if (tabName == "Package Forge") LoadTool(new MicroPackages_GuiBuilder().CreateGui(ctx));
                if (tabName == "Arbitration Simulator") LoadTool(BuildArbitrationView());
            })
            .AddTab("Ecosystem", new Color(0.2f, 0.4f, 0.2f))
            .AddTab("Package Forge", new Color(0.2f, 0.3f, 0.5f))
            .AddTab("Arbitration Simulator", new Color(0.5f, 0.3f, 0.2f));

            var rootLayout = new GraphicalUserInterfaceBuilder("NexusRoot")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f, 1f))
                .AddChild(ribbon)
                .CreateGui(ctx);

            rootLayout.Add(_contentContainer);

            LoadTool(BuildEcosystemView());

            return rootLayout;
        }

        private void LoadTool(VisualElement newToolGui)
        {
            _contentContainer.Clear();
            if (newToolGui != null) _contentContainer.Add(newToolGui);
        }

        // --- THE ECOSYSTEM VIEW (Powered by Workshop_CacheBuilder) ---
        private VisualElement BuildEcosystemView()
        {
            var root = new GraphicalUserInterfaceBuilder("EcosystemRoot")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

            // 1. Sync Button using your IO NetworkBus
            root.AddChild(new ForgeButtonBuilder("Sync with Nexus Server", SyncEcosystemWithServer)
                .WithBackgroundColor(new Color(0.15f, 0.4f, 0.6f))
                .WithTextColor(Color.white)
                .WithMarginBottom(10));

            // 2. The Native Workshop Cache Builder
            var cacheManager = new Workshop_CacheBuilder<DynamicMicroPackage>(
                title: "Local Packages",
                cacheKey: "MicroPackages",

                onItemSelected: (selectedPkg) => {
                    ForgeLogger.Log($"[Nexus] Selected Package: {selectedPkg.PackageId} v{selectedPkg.Version}");
                    // Logic to open this package in the Package Forge for editing
                },

                onFetchAll: () => {
                    // Logic to read local files (or pull from DataWarehouse)
                    var list = new List<DynamicMicroPackage>();
                    foreach (var file in Directory.GetFiles(CacheDirectory, "*.json"))
                    {
                        // var json = File.ReadAllText(file);
                        // var pkg = JsonConvert.DeserializeObject<DynamicMicroPackage>(json);
                        // list.Add(pkg);
                    }
                    return list;
                },

                onSaveItem: (pkgToSave) => {
                    // string json = JsonConvert.SerializeObject(pkgToSave, Formatting.Indented);
                    // File.WriteAllText(Path.Combine(CacheDirectory, $"{pkgToSave.PackageId}.json"), json);
                    ForgeLogger.Log($"[Nexus] Saved {pkgToSave.PackageId} to disk cache.");
                },

                onClearTemporaryCache: () => {
                    // Leverage DataWarehouse or delete local files
                    ForgeLogger.Log("[Nexus] Cleared local package cache.");
                }
            );

            root.AddChild(cacheManager);

            return root.CreateGui(_activeContext);
        }

        // --- THE SERVER SYNC (Powered by ForgeNetworkBus) ---
        private void SyncEcosystemWithServer()
        {
            ForgeLogger.Log("[Nexus] Firing Sync Intent to Server...");

            var syncIntent = new NetworkIntent
            {
                FullUrl = "https://your-singularity-server.com/api/packages/verify",
                Method = "POST",
                Payload = "{}", // You would inject the list of local versions here
                OnComplete = (result) =>
                {
                    if (result.IsSuccess)
                    {
                        ForgeLogger.Log("[Nexus] Sync successful. Initiating downloads for outdated packages.");
                        // Handle payload...
                    }
                    else
                    {
                        ForgeLogger.LogError($"[Nexus] Sync Failed: {result.StatusCode}");
                    }
                }
            };

            ForgeNetworkBus.Instance.QueueRequest(syncIntent);
        }

        private VisualElement BuildArbitrationView()
        {
            var split = new ForgeSplitPanelBuilder(300, Side.Left)
                .WithSidebar(new GraphicalUserInterfaceBuilder("ArbControls")
                    .WithTitle("Conflict Resolution")
                    .WithPadding(10)
                    .AddChild(new ForgeLabelBuilder("Select a package to simulate its arbitration phase against the ecosystem.")
                        .WithColor(Color.white)
                        .WithFontSize(12)
                        .WithWhiteSpace(WhiteSpace.Normal))
                    .AddChild(new ForgeButtonBuilder("Run Convergence Sequence", () => ForgeLogger.Log("Running..."))
                        .WithBackgroundColor(new Color(0.2f, 0.4f, 0.2f))
                        .WithTextColor(Color.white)
                        .WithMarginTop(15)))
                .WithMain(new GraphicalUserInterfaceBuilder("ArbLog")
                    .WithTitle("Convergence Matrix")
                    .WithBackgroundColor(new Color(0.08f, 0.08f, 0.08f, 1f))
                    .AddChild(new ForgeLabelBuilder("Awaiting simulation start...")
                        .WithColor(Color.gray)
                        .WithFontSize(12)));

            return split.CreateGui(_activeContext);
        }

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) { }
#endif
        public void FromUIDocument(string assetPath) { }
    }
}
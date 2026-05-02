#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Unity
{
    /// <summary>
    /// Reforged Service Router: Standardized for the Singularity Forge.
    /// Manages Unity Gaming Services (UGS) module activation via a pure Forge interface.
    /// </summary>
    public class Forge_Gui_UnityServiceRouter : IGuiProvider
    {
        public string Title => "SERVICE ACTIVATION ROUTER";

        private UnityServicesConfig _config;
        private Action _onConfigurationChanged;

        public Forge_Gui_UnityServiceRouter(UnityServicesConfig config, Action onConfigurationChanged)
        {
            _config = config;
            _onConfigurationChanged = onConfigurationChanged;
        }

        public VisualElement CreateGui(GuiContext context)
        {
            // Root Container - High-Tech Void Aesthetic
            var rootBuilder = new ForgeContainerBuilder("ServiceRouter_Root")
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.07f))
                .WithFlexGrow(1);

            // Header Section
            rootBuilder.AddChild(new ForgeLabelBuilder("UGS MODULE DEPLOYMENT")
                .WithFontSize(18)
                .WithFontStyle(FontStyle.Bold)
                .WithColor(Color.cyan)
                .WithMarginBottom(15));

            rootBuilder.AddChild(new ForgeLabelBuilder("Toggle desired services to expand the Workshop's cloud capabilities.")
                .WithColor(Color.gray)
                .WithMarginBottom(20));

            // Service List Container
            var listContainer = new ForgeContainerBuilder("ServiceList")
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithPadding(10)
                .WithBorderColor(Color.clear, Color.clear, Color.clear, Color.cyan)
                .WithBorderWidth(0, 0, 0, 2);

            // Register Service Rows
            listContainer.AddChild(CreateServiceToggle("Identity & Authentication", "Secure player login and profile management.",
                _config.RequireAuthentication, v => _config.RequireAuthentication = v));

            listContainer.AddChild(CreateServiceToggle("Remote Config", "Update game variables instantly without a redeploy.",
                _config.EnableRemoteConfig, v => _config.EnableRemoteConfig = v));

            listContainer.AddChild(CreateServiceToggle("Cloud Save", "Store player progress and cross-platform data.",
                _config.EnableCloudSave, v => _config.EnableCloudSave = v));

            listContainer.AddChild(CreateServiceToggle("Lobby & Relay", "Establish peer-to-peer and hosted multiplayer sessions.",
                _config.EnableLobbyServices, v => _config.EnableLobbyServices = v));

            rootBuilder.AddChild(listContainer);

            return rootBuilder.Build();
        }

        private IGuiProvider CreateServiceToggle(string name, string description, bool initialValue, Action<bool> onToggle)
        {
            // Using an anonymous DynamicGuiProvider to wrap the toggle row
            return new DynamicGuiProvider(ctx =>
            {
                var row = new ForgeContainerBuilder($"ServiceRow_{name}")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                    .WithMarginBottom(10)
                    .WithPadding(0, 0, 5, 0)
                    .WithBorderColor(new Color(0.2f, 0.2f, 0.2f))
                    .WithBorderWidth(0, 0, 1, 0);

                // Internal Toggle logic wrapped via OnBuild to maintain Forge standards
                row.OnBuild(ve =>
                {
                    var toggle = new Toggle { value = initialValue };
                    toggle.style.marginRight = 10;
                    toggle.RegisterValueChangedCallback(evt => {
                        onToggle(evt.newValue);
                        EditorUtility.SetDirty(_config);
                        AssetDatabase.SaveAssetIfDirty(_config);
                        _onConfigurationChanged?.Invoke();
                    });
                    ve.Add(toggle);
                });

                // Label Stack
                var textStack = new ForgeContainerBuilder("LabelStack").WithFlexGrow(1);
                textStack.AddChild(new ForgeLabelBuilder(name).WithBold().WithColor(Color.white));
                textStack.AddChild(new ForgeLabelBuilder(description).WithFontSize(10).WithColor(Color.gray));

                row.AddChild(textStack);
                return row.Build();
            });
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(new GuiContext());
            string fileName = string.IsNullOrEmpty(assetPath) ? "UGS_Router_Snapshot" : System.IO.Path.GetFileNameWithoutExtension(assetPath);

            // Standardizing UXML baking via the Workshop tool
            WorkshopUxmlBaker.Bake(root, fileName);
        }

        public void FromUIDocument(string assetPath)
        {
            Debug.LogWarning("[ServiceRouter] FromUIDocument is bypassed. UI is generated from Config State.");
        }
    }
}
#endif
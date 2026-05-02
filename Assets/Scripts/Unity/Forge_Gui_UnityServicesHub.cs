#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Unity
{
    public class Forge_Gui_UnityServicesHub : IGuiProvider
    {
        public string Title => "UNITY CLOUD SERVICES HUB";

        private GuiContext _guiContext;
        private VisualElement _sidebarPanel;
        private VisualElement _activeWorkspace;
        private UnityServicesConfig _configAsset;
        private string _activeTab = "Dashboard";

        // Recursive Fluent Builders
        private RemoteConfigServiceBuilder _rcBuilder;
        private CloudSaveServiceBuilder _csBuilder;
        private LobbyServiceBuilder _lobbyBuilder;

        // Modules
        private Forge_Gui_UnityServiceRouter _router;
        private Forge_Gui_UnityAuthModule _authModule;

        public Forge_Gui_UnityServicesHub()
        {
            EnsureConfigAssetExists();
            InitializeBuilders();
        }

        private void InitializeBuilders()
        {
            _rcBuilder = new RemoteConfigServiceBuilder(_configAsset);
            _csBuilder = new CloudSaveServiceBuilder(_configAsset);
            _lobbyBuilder = new LobbyServiceBuilder(_configAsset);

            _router = new Forge_Gui_UnityServiceRouter(_configAsset, RefreshLayout);
            _authModule = new Forge_Gui_UnityAuthModule();
        }

        private void EnsureConfigAssetExists()
        {
            string[] guids = AssetDatabase.FindAssets("t:UnityServicesConfig");
            if (guids.Length > 0)
            {
                _configAsset = AssetDatabase.LoadAssetAtPath<UnityServicesConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            else
            {
                _configAsset = ScriptableObject.CreateInstance<UnityServicesConfig>();
                if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.CreateAsset(_configAsset, "Assets/Resources/UnityServicesConfig.asset");
                AssetDatabase.SaveAssets();
            }
        }

        public VisualElement CreateGui(GuiContext context)
        {
            _guiContext = context;
            var builder = new GraphicalUserInterfaceBuilder("UnityServices_Root")
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.03f)) // Deep Workshop Charcoal
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100);

            _sidebarPanel = new VisualElement
            {
                style = {
                    width = 240,
                    backgroundColor = new Color(0.05f, 0.05f, 0.07f),
                    borderRightWidth = 2,
                    borderRightColor = new Color(0.5f, 0f, 0.5f) // Singularity Workshop Purple
                }
            };

            _activeWorkspace = new VisualElement { style = { flexGrow = 1 } };

            builder.AddChild(_sidebarPanel);
            builder.AddChild(_activeWorkspace);

            // --- CRITICAL UI TICK PATTERN ---
            // Drive heartbeat evaluation locally for the Editor panels
            builder.OnBuild(ve => {
                ve.schedule.Execute(() => {
                    // System_Services is ticked here while the Hub is in focus
                    // FSM_API.Interaction.Update("System_Services");
                }).Every(500);
            });

            RefreshLayout();
            return builder.Build();
        }

        private void RefreshLayout()
        {
            RenderSidebar();
            RenderWorkspace();
        }

        private void RenderSidebar()
        {
            _sidebarPanel.Clear();
            _sidebarPanel.Add(new Label("CLOUD ORCHESTRATOR")
            {
                style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, paddingLeft = 15, paddingTop = 20, marginBottom = 20, fontSize = 14 }
            });

            _sidebarPanel.Add(CreateTabButton("Dashboard", "Deployment Router"));

            if (_configAsset.RequireAuthentication)
                _sidebarPanel.Add(CreateTabButton("Authentication", "Identity Guards"));

            if (_configAsset.EnableRemoteConfig)
                _sidebarPanel.Add(CreateTabButton("Remote Config", "Live Tuning"));

            if (_configAsset.EnableCloudSave)
                _sidebarPanel.Add(CreateTabButton("Cloud Save", "Persistent Shelves"));

            if (_configAsset.EnableLobbyServices)
                _sidebarPanel.Add(CreateTabButton("Relay / Lobby", "Multiplayer Routing"));
        }

        private void RenderWorkspace()
        {
            _activeWorkspace.Clear();
            switch (_activeTab)
            {
                case "Dashboard":
                    _activeWorkspace.Add(_router.CreateGui(_guiContext));
                    break;
                case "Authentication":
                    _activeWorkspace.Add(_authModule.CreateGui(_guiContext));
                    break;
                case "Remote Config":
                    _activeWorkspace.Add(_rcBuilder.GetGuiBuilder().Build());
                    break;
                case "Cloud Save":
                    _activeWorkspace.Add(_csBuilder.GetGuiBuilder().Build());
                    break;
                case "Relay / Lobby":
                    _activeWorkspace.Add(_lobbyBuilder.GetGuiBuilder().Build());
                    break;
            }
        }

        private Button CreateTabButton(string tabName, string subtext)
        {
            bool isActive = _activeTab == tabName;
            var btn = new Button(() => { _activeTab = tabName; RefreshLayout(); })
            {
                style = {
                    height = 55, marginBottom = 2, paddingLeft = 15, justifyContent = Justify.Center, alignItems = Align.FlexStart,
                    backgroundColor = isActive ? new Color(0.12f, 0.05f, 0.15f) : Color.clear,
                    borderLeftWidth = isActive ? 4 : 0, borderLeftColor = Color.cyan
                }
            };
            btn.Add(new Label(tabName.ToUpper()) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 11 } });
            btn.Add(new Label(subtext) { style = { color = Color.gray, fontSize = 8, marginTop = 2 } });
            return btn;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
    }
}
#endif
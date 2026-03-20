#if UNITY_EDITOR
using TheSingularityWorkshop.UnityServices;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class Forge_Gui_UnityServicesHub : IGuiProvider
    {
        public string Title => "UNITY CLOUD SERVICES HUB";

        private GuiContext _guiContext;
        private VisualElement _rootContainer;
        private VisualElement _sidebarPanel;
        private VisualElement _activeWorkspace;

        private UnityServicesConfig _configAsset;
        private string _activeTab = "Dashboard";

        public Forge_Gui_UnityServicesHub()
        {
            EnsureConfigAssetExists();
        }

        private void EnsureConfigAssetExists()
        {
            // Try to find an existing config asset in the project
            string[] guids = AssetDatabase.FindAssets("t:UnityServicesConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _configAsset = AssetDatabase.LoadAssetAtPath<UnityServicesConfig>(path);
            }
            else
            {
                // Automatically create one if the user hasn't set it up yet!
                _configAsset = ScriptableObject.CreateInstance<UnityServicesConfig>();

                // Ensure a standard directory exists for Forge settings
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    AssetDatabase.CreateFolder("Assets", "Resources");

                string savePath = "Assets/Resources/UnityServicesConfig.asset";
                AssetDatabase.CreateAsset(_configAsset, savePath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[Forge] Generated new UnityServicesConfig at {savePath}");
            }
        }

        public VisualElement CreateGui(GuiContext context)
        {
            _guiContext = context;

            var builder = new GraphicalUserInterfaceBuilder("UnityServices_Root")
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100);

            builder.AddChild(c => {
                _sidebarPanel = new VisualElement { style = { width = 250, backgroundColor = new Color(0.08f, 0.08f, 0.1f), borderRightWidth = 2, borderRightColor = Color.cyan } };
                return _sidebarPanel;
            });

            builder.AddChild(c => {
                _activeWorkspace = new VisualElement { style = { flexGrow = 1, paddingLeft = 20, paddingTop = 20, paddingRight = 20 } };
                return _activeWorkspace;
            });

            _rootContainer = builder.Build();
            RefreshLayout();

            return _rootContainer;
        }

        private void RefreshLayout()
        {
            RenderSidebar();
            RenderWorkspace();
        }

        private void RenderSidebar()
        {
            _sidebarPanel.Clear();
            _sidebarPanel.Add(new Label("SERVICE MODULES") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, paddingLeft = 10, paddingTop = 10, marginBottom = 15 } });

            _sidebarPanel.Add(CreateTabButton("Dashboard", "Core Hub Configuration"));

            if (_configAsset.RequireAuthentication) _sidebarPanel.Add(CreateTabButton("Authentication", "Identity & Profiles"));
            if (_configAsset.EnableRemoteConfig) _sidebarPanel.Add(CreateTabButton("Remote Config", "Live Tuning Variables"));
            if (_configAsset.EnableCloudSave) _sidebarPanel.Add(CreateTabButton("Cloud Save", "Remote Data Sync"));
            if (_configAsset.EnableLobbyServices) _sidebarPanel.Add(CreateTabButton("Relay / Lobby", "Multiplayer Routing"));
        }

        private Button CreateTabButton(string tabName, string subtext)
        {
            bool isActive = _activeTab == tabName;
            var btn = new Button(() => { _activeTab = tabName; RefreshLayout(); })
            {
                style = {
                    height = 50, marginBottom = 5, paddingLeft = 15, justifyContent = Justify.Center, alignItems = Align.FlexStart,
                    backgroundColor = isActive ? new Color(0.15f, 0.25f, 0.35f) : new Color(0.12f, 0.12f, 0.15f),
                    borderLeftWidth = isActive ? 4 : 0, borderLeftColor = Color.cyan
                }
            };
            btn.Add(new Label(tabName) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });
            btn.Add(new Label(subtext) { style = { color = Color.gray, fontSize = 9 } });
            return btn;
        }

        private void RenderWorkspace()
        {
            _activeWorkspace.Clear();

            // Bind the active workspace directly to our ScriptableObject to track changes instantly
            var serializedObject = new SerializedObject(_configAsset);
            _activeWorkspace.Bind(serializedObject);

            if (_activeTab == "Dashboard") RenderDashboardTab();
            else if (_activeTab == "Authentication") RenderAuthTab();
            else if (_activeTab == "Remote Config") RenderRemoteConfigTab();
            else _activeWorkspace.Add(new Label("MODULE ACTIVE - Awaiting Configuration Fields") { style = { color = Color.gray } });
        }

        private void RenderDashboardTab()
        {
            _activeWorkspace.Add(new Label("UNITY GAMING SERVICES INITIALIZATION") { style = { color = Color.white, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });

            var enableBox = new VisualElement { style = { backgroundColor = new Color(0.05f, 0.05f, 0.05f), paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15, borderLeftWidth = 2, borderLeftColor = Color.green, marginBottom = 20 } };
            enableBox.Add(new Label("ENABLED MODULES") { style = { color = Color.green, marginBottom = 10, unityFontStyleAndWeight = FontStyle.Bold } });

            // We use standard Toggles but hook into the dirtying mechanism
            enableBox.Add(CreateRefreshedToggle("Require Authentication", _configAsset.RequireAuthentication, v => _configAsset.RequireAuthentication = v));
            enableBox.Add(CreateRefreshedToggle("Enable Remote Config", _configAsset.EnableRemoteConfig, v => _configAsset.EnableRemoteConfig = v));
            enableBox.Add(CreateRefreshedToggle("Enable Cloud Save", _configAsset.EnableCloudSave, v => _configAsset.EnableCloudSave = v));
            enableBox.Add(CreateRefreshedToggle("Enable Lobby Services", _configAsset.EnableLobbyServices, v => _configAsset.EnableLobbyServices = v));

            _activeWorkspace.Add(enableBox);

            var initBox = new VisualElement { style = { backgroundColor = new Color(0.05f, 0.05f, 0.05f), paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15, borderLeftWidth = 2, borderLeftColor = Color.yellow, marginBottom = 20 } };
            initBox.Add(new PropertyField() { bindingPath = "AutoInitializeOnAwake" });
            initBox.Add(new PropertyField() { bindingPath = "MaxInitRetries" });
            initBox.Add(new PropertyField() { bindingPath = "RetryBackoffSeconds" });
            _activeWorkspace.Add(initBox);
        }
       
        private void RenderAuthTab()
        {
            _activeWorkspace.Clear();
            _activeWorkspace.Add(new Label("IDENTITY & AUTHENTICATION") { style = { color = Color.white, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });
            var authBox = new VisualElement { style = { backgroundColor = new Color(0.05f, 0.05f, 0.05f), paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15, borderLeftWidth = 2, borderLeftColor = Color.cyan } };

            authBox.Add(new Label("Use a Dev Profile to sandbox your Editor sessions and avoid polluting production analytics.") { style = { color = Color.gray, whiteSpace = WhiteSpace.Normal, marginBottom = 15 } });
            authBox.Add(new PropertyField() { bindingPath = "EditorDevProfile" });

            _activeWorkspace.Add(authBox);

            var authModule = new Forge_Gui_UnityAuthModule();
            _activeWorkspace.Add(authModule.CreateGui(_guiContext));
        }

        private void RenderRemoteConfigTab()
        {
            _activeWorkspace.Add(new Label("REMOTE CONFIGURATION") { style = { color = Color.white, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 20 } });
            var rcBox = new VisualElement { style = { backgroundColor = new Color(0.05f, 0.05f, 0.05f), paddingLeft = 15, paddingRight = 15, paddingTop = 15, paddingBottom = 15, borderLeftWidth = 2, borderLeftColor = Color.cyan } };

            rcBox.Add(new PropertyField() { bindingPath = "AutoRefreshRemoteConfig" });
            rcBox.Add(new PropertyField() { bindingPath = "RemoteConfigRefreshIntervalSeconds" });

            _activeWorkspace.Add(rcBox);
        }

        private Toggle CreateRefreshedToggle(string label, bool val, Action<bool> applyValue)
        {
            var t = new Toggle(label) { value = val };
            t.RegisterValueChangedCallback(e => {
                applyValue(e.newValue);
                EditorUtility.SetDirty(_configAsset); // Flag asset as changed
                AssetDatabase.SaveAssetIfDirty(_configAsset); // Save to disk immediately
                RefreshLayout(); // Redraws sidebar tabs immediately!
            });
            return t;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _guiContext?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }
}
#endif
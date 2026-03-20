#if UNITY_EDITOR
using TheSingularityWorkshop.UnityServices;
using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

#if UNITY_SERVICES_AVAILABLE
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Core.Environments;
#endif

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class Forge_Gui_UnityAuthModule : IGuiProvider
    {
        public string Title => "IDENTITY & AUTHENTICATION";

        private static AddRequest _installRequest;
        private GuiContext _guiContext;
        private UnityAuthConfig _authConfig;

        // UI Elements we need to update dynamically
        private VisualElement _statusOrb;
        private Label _statusLabel;
        private Button _testConnectionBtn;

        public Forge_Gui_UnityAuthModule()
        {
            EnsureConfigAssetExists();
        }

        private void EnsureConfigAssetExists()
        {
            string[] guids = AssetDatabase.FindAssets("t:UnityAuthConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _authConfig = AssetDatabase.LoadAssetAtPath<UnityAuthConfig>(path);
            }
            else
            {
                _authConfig = ScriptableObject.CreateInstance<UnityAuthConfig>();
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    AssetDatabase.CreateFolder("Assets", "Resources");

                AssetDatabase.CreateAsset(_authConfig, "Assets/Resources/UnityAuthConfig.asset");
                AssetDatabase.SaveAssets();
            }
        }

        public VisualElement CreateGui(GuiContext context)
        {
            _guiContext = context;
            var serializedObject = new SerializedObject(_authConfig);

            // The Root Workspace
            var builder = new GraphicalUserInterfaceBuilder("AuthScrollRoot")
                .WithScrollable(true, ScrollViewMode.Vertical)
                .WithAutoGrow()
                .WithPadding(10)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f));

            // --- HEADER ---
            _statusLabel = new Label("DISCONNECTED") { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } };

            var headerBuilder = new GraphicalUserInterfaceBuilder("HeaderRow")
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .OnBuild(ve => ve.style.marginBottom = 15)
                .AddChild(new Label("IDENTITY & AUTHENTICATION") { style = { color = Color.cyan, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1 } })
                .AddChild(new GraphicalUserInterfaceBuilder("StatusBox")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                    .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                    .WithPadding(5)
                    .OnBuild(ve => {
                        ve.style.paddingLeft = 10; ve.style.paddingRight = 10;
                        ve.style.borderTopLeftRadius = 5; ve.style.borderTopRightRadius = 5;
                        ve.style.borderBottomLeftRadius = 5; ve.style.borderBottomRightRadius = 5;
                    })
                    .AddChild(new GraphicalUserInterfaceBuilder("StatusOrb")
                        .OnBuild(ve => {
                            ve.style.width = 12; ve.style.height = 12;
                            ve.style.borderTopLeftRadius = 6; ve.style.borderTopRightRadius = 6;
                            ve.style.borderBottomLeftRadius = 6; ve.style.borderBottomRightRadius = 6;
                            ve.style.backgroundColor = Color.red;
                            ve.style.marginRight = 8;
                            _statusOrb = ve; // Capture the reference for the FSM/Logic
                        })
                    )
                    .AddChild(_statusLabel)
                );

            builder.AddChild(headerBuilder);

            // --- AGENT MESSAGE ---
            var agentBuilder = new GraphicalUserInterfaceBuilder("AgentMessage")
                .WithBackgroundColor(new Color(0.05f, 0.15f, 0.2f))
                .WithPadding(15)
                .WithBorderLeftWidth(4)
                .WithBorderLeftColor(Color.cyan)
                .OnBuild(ve => ve.style.marginBottom = 20)
                .AddChild(new Label("Greetings, Creator. Before we can welcome players into this reality, we must establish the border guards. This module links your local Forge to the Unity Cloud Servers.") { style = { color = new Color(0.8f, 0.9f, 1.0f), whiteSpace = WhiteSpace.Normal } });

            builder.AddChild(agentBuilder);

            // --- STEP 1: ENGINE INTEGRITY ---
            builder.AddChild(CreatePackageStep());

            // --- STEP 2: PROJECT IDENTITY ---
            var idBox = CreateSectionBox("STEP 2: PROJECT CONNECTION", new Color(0.8f, 0.4f, 0.1f));
#if !UNITY_SERVICES_AVAILABLE
            idBox.OnBuild(ve => ve.SetEnabled(false)); // Subconscious Beast: Disable until packages are present
#endif
            idBox.AddChild(new PropertyField(serializedObject.FindProperty("CloudProjectID")));
            idBox.AddChild(new Button(() => {
                // FIX: Pulling the ID from the editor instead of pushing a read-only field
                _authConfig.CloudProjectID = PlayerSettings.cloudProjectId;
                serializedObject.Update();
                EditorUtility.SetDirty(_authConfig);
                AssetDatabase.SaveAssets();
                Debug.Log($"[Forge] Cloud Project ID synced from Unity Services into Forge Config.");
            })
            { text = "PULL PROJECT ID FROM EDITOR", style = { marginTop = 10, height = 30, backgroundColor = new Color(0.1f, 0.3f, 0.4f) } });

            builder.AddChild(idBox);

            // --- STEP 3: THE HANDSHAKE ---
            var testBox = CreateSectionBox("STEP 3: SANDBOX TESTING", Color.magenta);
#if !UNITY_SERVICES_AVAILABLE
            testBox.OnBuild(ve => ve.SetEnabled(false));
#endif
            testBox.AddChild(new PropertyField(serializedObject.FindProperty("EnvironmentName")));
            testBox.AddChild(new PropertyField(serializedObject.FindProperty("EditorSandboxProfile")));

            _testConnectionBtn = new Button(async () => await TestConnectionAsync())
            {
                text = "PING UNITY SERVERS",
                style = { marginTop = 15, height = 35, backgroundColor = new Color(0.3f, 0.1f, 0.3f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold }
            };
            testBox.AddChild(_testConnectionBtn);

            builder.AddChild(testBox);

            // Final Assembly
            var root = builder.Build();
            root.Bind(serializedObject);
            return root;
        }

        private GraphicalUserInterfaceBuilder CreatePackageStep()
        {
            var box = CreateSectionBox("STEP 1: SERVICE DEPLOYMENT", Color.cyan);
#if UNITY_SERVICES_AVAILABLE
            box.AddChild(new Label("✓ Unity Services SDK is active in the project kernel.") { style = { color = Color.green } });
#else
            box.AddChild(new Label("The necessary Unity Packages are missing. Would you like me to install the Authentication package for you?") { style = { color = Color.yellow, whiteSpace = WhiteSpace.Normal } });
            var installBtn = new Button(StartPackageInstallation)
            {
                text = "INSTALL AUTHENTICATION SERVICES",
                style = { height = 35, backgroundColor = new Color(0.15f, 0.35f, 0.45f), marginTop = 10 }
            };
            box.AddChild(installBtn);
#endif
            return box;
        }

        private GraphicalUserInterfaceBuilder CreateSectionBox(string title, Color accent)
        {
            return new GraphicalUserInterfaceBuilder($"Section_{title.Replace(" ", "")}")
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f))
                .WithPadding(15)
                .WithBorderLeftWidth(4)
                .WithBorderLeftColor(accent)
                .OnBuild(ve => ve.style.marginBottom = 20)
                .AddChild(new Label(title) { style = { color = accent, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
        }

        private void StartPackageInstallation()
        {
            Debug.Log("[TheForge] Initializing deployment of Unity Authentication packages..");
            _installRequest = Client.Add("com.unity.services.authentication");
            EditorApplication.update += ProgressPackageInstallation;
        }

        private void ProgressPackageInstallation()
        {
            if (_installRequest.IsCompleted)
            {
                if (_installRequest.Status == StatusCode.Success)
                    Debug.Log($"[TheForge] Package Breached: {_installRequest.Result.packageId}. Reloading kernel..");
                else if (_installRequest.Status >= StatusCode.Failure)
                    Debug.LogError($"[TheForge] Installation failed: {_installRequest.Error.message}");

                EditorApplication.update -= ProgressPackageInstallation;
            }
        }

        private async Task TestConnectionAsync()
        {
#if UNITY_SERVICES_AVAILABLE
            _testConnectionBtn.text = "CONNECTING..";
            _statusOrb.style.backgroundColor = Color.yellow;
            _statusLabel.text = "HANDSHAKING";

            try
            {
                var options = new InitializationOptions();
                options.SetEnvironmentName(_authConfig.EnvironmentName);
                options.SetProfile(_authConfig.EditorSandboxProfile);

                await UnityServices.InitializeAsync(options);

                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                _statusOrb.style.backgroundColor = Color.green;
                _statusLabel.text = $"CONNECTED: {AuthenticationService.Instance.PlayerId}";
                _testConnectionBtn.text = "CONNECTION VERIFIED";
                _testConnectionBtn.style.backgroundColor = new Color(0.1f, 0.4f, 0.1f);
            }
            catch (Exception ex)
            {
                _statusOrb.style.backgroundColor = Color.red;
                _statusLabel.text = "CONNECTION FAILED";
                _testConnectionBtn.text = "RETRY PING";
                _testConnectionBtn.style.backgroundColor = new Color(0.4f, 0.1f, 0.1f);
                Debug.LogError($"[Forge] Unity Firewall Breach Failed: {ex.Message}");
            }
#endif
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _guiContext?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
    }
}
#endif
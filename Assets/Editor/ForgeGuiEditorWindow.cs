// File: Assets/Editor/ForgeGuiEditorWindow.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Extensions;
using Workshop.Core.Diagnostics; // Injected for ForgeLogger
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes.Effects;

namespace Assets.Editor
{
    public class ForgeGuiEditorWindow : EditorWindow
    {
        private VisualElement _previewContainer;
        private Label _headerTitle;
        private VisualElement _bakeBtnContainer;
        private ScrollView _leftScrollContainer;

        private IGuiProvider _activeProvider;
        private Dictionary<string, List<Type>> _groupedProviders;
        private string _activeCategory = "";

        [MenuItem("TheSingularityWorkshop/The Forge/Workshop Hub", false, 10)]
        public static void ShowWindow()
        {
            ForgeLogger.Log("Workshop Hub Opening Request Received.")
                .WithHeader("Editor")
                .WithColor("#00FFCC") // Neon Cyan
                .SendToUnity();

            var window = GetWindow<ForgeGuiEditorWindow>("Singularity Workshop");
            window.minSize = new Vector2(900, 650);
        }

        public void CreateGUI()
        {
            ForgeLogger.Log("Forge Hub Root Initializing.")
                .WithHeader("Forge Hub")
                .WithColor("#00FFCC")
                .SendToUnity();

            var theme = GuiSkin.Active;
            BuildProviderGroups();

            var hubLayout = new GraphicalUserInterfaceBuilder("ForgeHubRoot")
                .WithAutoGrow()
                .AddChild(new ForgeSplitPanelBuilder(sidebarWidth: 280, side: Side.Left)
                    .WithSidebar(new ActionGuiProvider(BuildSidebar))
                    .WithMain(new ActionGuiProvider(BuildMainWorkspace)));

            rootVisualElement.Add(hubLayout.Build());

            if (_previewContainer != null)
            {
                ForgeLogger.Log("Applying Breathing Effect to Preview Container.")
                    .WithHeader("Aesthetics")
                    .WithColor(Color.cyan)
                    .SendToUnity();

                new BreathingEffect(
                    _previewContainer,
                    0,
                    1.5f,
                    "EditorUpdate",
                    new List<Color> { theme.PrimaryAccent, new Color(0.3f, 0.3f, 0.3f) }
                );
            }

            ShowDashboard();
        }

        private VisualElement BuildSidebar(GuiContext ctx)
        {
            var sidebarBuilder = new GraphicalUserInterfaceBuilder("Sidebar")
                .WithPadding(12)
                .AddHeader("EXPERIENCE FORGE", GuiSkin.Active.PrimaryAccent)
                .AddChild(new GraphicalUserInterfaceBuilder("LeftScrollContainer")
                    .WithAutoGrow()
                    .WithScrollable(true));

            var sidebarTree = sidebarBuilder.Build();
            _leftScrollContainer = sidebarTree.Q<ScrollView>();

            return sidebarTree.ApplyProfile(GuiElementType.Panel);
        }

        private VisualElement BuildMainWorkspace(GuiContext ctx)
        {
            var workspaceBuilder = new GraphicalUserInterfaceBuilder("Workspace")
                .WithPadding(15)
                .WithAutoGrow()
                .AddChild(new GraphicalUserInterfaceBuilder("Toolbar")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithHeight(45)
                    .AddChild(new GraphicalUserInterfaceBuilder("TitleContainer")
                        .WithFlexGrow(1)
                        .AddHeader("SYSTEM DASHBOARD", Color.white))
                    .AddChild(new GraphicalUserInterfaceBuilder("BakeBtnContainer")
                        .AddButton("BAKE TO UXML", BakeActiveProvider)))
                .AddChild(new GraphicalUserInterfaceBuilder("PreviewContainer")
                    .WithFlexGrow(1)
                    .WithBackgroundColor(GuiSkin.Active.GetProfile(GuiElementType.Window).BackgroundColor));

            var workspaceTree = workspaceBuilder.Build();

            _headerTitle = workspaceTree.Q<Label>();
            _bakeBtnContainer = workspaceTree.Q<VisualElement>("BakeBtnContainer");
            _previewContainer = workspaceTree.Q<VisualElement>("PreviewContainer");

            if (_bakeBtnContainer != null)
                _bakeBtnContainer.style.display = DisplayStyle.None;

            return workspaceTree;
        }

        private void BuildProviderGroups()
        {
            ForgeLogger.Log("Reflecting Assemblies for IGuiProviders...")
                .WithHeader("Reflection")
                .WithColor("#FFA500") // Network Orange
                .SendToUnity();

            var providerTypes = TypeCache.GetTypesDerivedFrom<IGuiProvider>()
                .Where(t => !t.IsAbstract && !t.IsInterface &&
                            t != typeof(ForgeSplitPanelBuilder) &&
                            t != typeof(ActionGuiProvider) &&
                            t.Name != "ForgeDashboardProvider")
                .OrderBy(t => t.Name)
                .ToList();

            _groupedProviders = new Dictionary<string, List<Type>>();
            foreach (var type in providerTypes)
            {
                string category = type.Name.Contains("_") ? type.Name.Split('_')[0] : "General";
                if (!_groupedProviders.ContainsKey(category)) _groupedProviders[category] = new List<Type>();
                _groupedProviders[category].Add(type);
            }

            ForgeLogger.Log($"Reflection Complete. Cataloged {_groupedProviders.Count} categories.")
                .WithHeader("Reflection")
                .WithColor("#FFA500")
                .SendToUnity();
        }

        private void ShowDashboard()
        {
            _activeCategory = "DASHBOARD";

            ForgeLogger.Log("Mounting Sovereign Dashboard.")
                .WithHeader("Dashboard")
                .WithColor("#FF00FF") // Magenta
                .SendToUnity();

            Type dashType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == "ForgeDashboardProvider");

            if (dashType != null)
            {
                _activeProvider = (IGuiProvider)Activator.CreateInstance(dashType, new object[] { _groupedProviders });

                if (_headerTitle != null) _headerTitle.text = _activeProvider.Title.ToUpper();
                if (_bakeBtnContainer != null) _bakeBtnContainer.style.display = DisplayStyle.None;

                if (_previewContainer != null)
                {
                    _previewContainer.Clear();
                    _previewContainer.Add(_activeProvider.CreateGui(new GuiContext()));
                }
            }
            else
            {
                ForgeLogger.LogError("Dashboard failed to manifest: ForgeDashboardProvider not found in any assembly.")
                    .WithHeader("Dashboard")
                    .SendToUnity();
            }

            RefreshLeftPanel();
        }

        private void RefreshLeftPanel()
        {
            if (_leftScrollContainer == null) return;
            _leftScrollContainer.Clear();

            var theme = GuiSkin.Active;
            var listBuilder = new GraphicalUserInterfaceBuilder("NavListRoot").WithAutoGrow();

            listBuilder.AddButton("✦ SYSTEM DASHBOARD", ShowDashboard);

            foreach (var kvp in _groupedProviders.OrderBy(k => k.Key))
            {
                string catName = kvp.Key;
                bool isExpanded = _activeCategory == catName;

                listBuilder.AddButton((isExpanded ? "▼ " : "► ") + catName.ToUpper(), () => {
                    _activeCategory = isExpanded ? "" : catName;
                    RefreshLeftPanel();
                });

                if (isExpanded)
                {
                    foreach (var type in kvp.Value)
                    {
                        string displayName = type.Name.Replace(catName + "_", "");
                        listBuilder.AddButton("    " + displayName, () => SelectProvider(type));
                    }
                }
            }

            var builtNavList = listBuilder.Build();
            var buttons = builtNavList.Query<Button>().ToList();
            int btnIndex = 0;

            if (buttons.Count > 0)
            {
                StyleButton(buttons[btnIndex++], _activeCategory == "DASHBOARD", true, theme.PrimaryAccent);

                foreach (var kvp in _groupedProviders.OrderBy(k => k.Key))
                {
                    bool isExpanded = _activeCategory == kvp.Key;
                    if (btnIndex < buttons.Count) StyleButton(buttons[btnIndex++], isExpanded, false, theme.PrimaryAccent);

                    if (isExpanded)
                    {
                        foreach (var type in kvp.Value)
                        {
                            if (btnIndex < buttons.Count)
                            {
                                var childBtn = buttons[btnIndex++];
                                childBtn.style.unityTextAlign = TextAnchor.MiddleLeft;
                                childBtn.style.backgroundColor = new Color(0.08f, 0.08f, 0.1f, 0.8f);
                                childBtn.style.borderLeftWidth = 3;
                                childBtn.style.borderLeftColor = theme.PrimaryAccent;
                                childBtn.style.color = Color.white;
                            }
                        }
                    }
                }
            }

            _leftScrollContainer.Add(builtNavList);
        }

        private void StyleButton(Button btn, bool isActive, bool isHero, Color accent)
        {
            btn.style.unityTextAlign = TextAnchor.MiddleLeft;
            btn.style.backgroundColor = isActive ? accent : (isHero ? new Color(0.2f, 0.4f, 0.6f, 0.6f) : new Color(0.12f, 0.12f, 0.15f));
            btn.style.color = isActive ? Color.black : Color.white;

            if (isHero)
            {
                btn.style.unityFontStyleAndWeight = FontStyle.Bold;
                btn.style.marginTop = 5;
                btn.style.marginBottom = 15;
                btn.style.paddingTop = 10;
                btn.style.paddingBottom = 10;
            }
        }

        private void SelectProvider(Type type)
        {
            ForgeLogger.Log($"Switching Context: {type.Name}")
                .WithHeader("Context")
                .WithColor("#3399FF") // Sky Blue
                .SendToUnity();

            try
            {
                _activeProvider = (IGuiProvider)Activator.CreateInstance(type);

                if (_headerTitle != null) _headerTitle.text = _activeProvider.Title?.ToUpper() ?? type.Name.ToUpper();
                if (_bakeBtnContainer != null) _bakeBtnContainer.style.display = DisplayStyle.Flex;

                if (_previewContainer != null)
                {
                    _previewContainer.Clear();
                    _previewContainer.Add(_activeProvider.CreateGui(new GuiContext()));
                }

                if (_activeCategory == "DASHBOARD") _activeCategory = "";
                RefreshLeftPanel();
            }
            catch (Exception ex)
            {
                ForgeLogger.LogError($"Manifestation Failed: {ex.Message} on Type: {type.Name}")
                    .WithHeader("The Forge")
                    .SendToUnity();
            }
        }

        private void BakeActiveProvider()
        {
            if (_activeProvider == null) return;

            string path = EditorUtility.SaveFilePanelInProject("Bake Component", _activeProvider.GetType().Name, "uxml", "Save UI");

            if (!string.IsNullOrEmpty(path))
            {
                ForgeLogger.Log($"Baking Provider Manifest to UXML: {path}")
                    .WithHeader("Bake")
                    .WithColor("#FF00FF") // Magenta
                    .SendToUnity();

                _activeProvider.ToUIDocument(path);
                AssetDatabase.Refresh();
            }
        }
    }
}
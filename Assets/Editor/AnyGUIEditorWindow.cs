// File: Assets/Editor/AnyGUIEditorWindow.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders.Themes;

namespace Assets.Editor
{
    public class AnyGUIEditorWindow : EditorWindow
    {
        private VisualElement _previewContainer;
        private IGuiProvider _activeProvider;
        private Label _headerTitle;
        private Button _bakeBtn;
        private ScrollView _leftScroll;

        private Dictionary<string, List<Type>> _groupedProviders;
        private string _activeCategory = "";

        [MenuItem("TheSingularityWorkshop/The Forge/Workshop Hub", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<AnyGUIEditorWindow>("Singularity Workshop");
            window.minSize = new Vector2(900, 650);
        }

        public void CreateGUI()
        {
            var theme = GuiSkin.Active;
            var ctx = new GuiContext();

            BuildProviderGroups();

            var hubLayout = new GraphicalUserInterfaceBuilder("ForgeHubRoot")
                .WithAutoGrow()
                .AddChild(new SplitPanelBuilder(sidebarWidth: 280, side: Side.Left)
                    .WithSidebar(new ActionGuiProvider(BuildSidebar))
                    .WithMain(new ActionGuiProvider(BuildMainWorkspace)));

            rootVisualElement.Add(hubLayout.Build());

            // Boot directly into the new Dashboard
            ShowDashboard();
        }

        private VisualElement BuildSidebar(GuiContext ctx)
        {
            var sidebar = new GraphicalUserInterfaceBuilder("Sidebar")
                .WithPadding(12)
                .AddChild(new Label("EXPERIENCE FORGE")
                    .Bold()
                    .FontSize(20)
                    .Color(GuiSkin.Active.PrimaryAccent))
                .AddChild(context => {
                    _leftScroll = new ScrollView();
                    return _leftScroll;
                })
                .Build();

            return sidebar.ApplyProfile(GuiElementType.Panel);
        }

        private VisualElement BuildMainWorkspace(GuiContext ctx)
        {
            var workspace = new GraphicalUserInterfaceBuilder("Workspace")
                .WithPadding(15)
                .WithAutoGrow()
                .AddChild(new GraphicalUserInterfaceBuilder("Toolbar")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithHeight(45)
                    .AddChild(context => {
                        _headerTitle = new Label("SYSTEM DASHBOARD").Bold().FontSize(16).Color(Color.white);
                        _headerTitle.style.flexGrow = 1;
                        return _headerTitle;
                    })
                    .AddChild(context => {
                        _bakeBtn = new Button(BakeActiveProvider) { text = "BAKE TO UXML" };
                        _bakeBtn.style.display = DisplayStyle.None;
                        return _bakeBtn.ApplyProfile(GuiElementType.ButtonPrimary);
                    }))
                .AddChild(context => {
                    _previewContainer = new VisualElement()
                        .FlexGrow(1)
                        .Background(GuiSkin.Active.GetProfile(GuiElementType.Window).BackgroundColor)
                        .Border(2, GuiSkin.Active.PrimaryAccent)
                        .Radius(GuiSkin.Active.GetProfile(GuiElementType.Window).CornerRadius);
                    return _previewContainer;
                })
                .Build();

            return workspace;
        }

        private void BuildProviderGroups()
        {
            var providerTypes = TypeCache.GetTypesDerivedFrom<IGuiProvider>()
                .Where(t => !t.IsAbstract && !t.IsInterface &&
                            t != typeof(SplitPanelBuilder) &&
                            t != typeof(ActionGuiProvider) &&
                            t != typeof(ForgeDashboardProvider)) // Exclude dashboard from lists
                .OrderBy(t => t.Name)
                .ToList();

            _groupedProviders = new Dictionary<string, List<Type>>();
            foreach (var type in providerTypes)
            {
                string category = type.Name.Contains("_") ? type.Name.Split('_')[0] : "General";
                if (!_groupedProviders.ContainsKey(category)) _groupedProviders[category] = new List<Type>();
                _groupedProviders[category].Add(type);
            }
        }

        private void ShowDashboard()
        {
            _activeCategory = "DASHBOARD";
            _activeProvider = new ForgeDashboardProvider(_groupedProviders);

            _headerTitle.text = _activeProvider.Title.ToUpper();
            _bakeBtn.style.display = DisplayStyle.None; // Hide bake button for dashboard

            _previewContainer.Clear();
            _previewContainer.Add(_activeProvider.CreateGui(new GuiContext()));

            RefreshLeftPanel(); // Update visual selection
        }

        private void RefreshLeftPanel()
        {
            if (_leftScroll == null) return;
            _leftScroll.Clear();
            var theme = GuiSkin.Active;

            // --- THE DASHBOARD HERO BUTTON ---
            var dashBtn = new Button(ShowDashboard) { text = "✦ SYSTEM DASHBOARD" };
            dashBtn.style.unityTextAlign = TextAnchor.MiddleLeft;
            dashBtn.style.backgroundColor = _activeCategory == "DASHBOARD" ? theme.PrimaryAccent : new Color(0.2f, 0.4f, 0.6f, 0.6f);
            dashBtn.style.color = _activeCategory == "DASHBOARD" ? Color.black : Color.white;
            dashBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            dashBtn.style.marginTop = 5;
            dashBtn.style.marginBottom = 15;
            dashBtn.style.paddingTop = 10;
            dashBtn.style.paddingBottom = 10;
            _leftScroll.Add(dashBtn);

            // Populate remaining categories
            foreach (var kvp in _groupedProviders.OrderBy(k => k.Key))
            {
                string catName = kvp.Key;
                bool isExpanded = _activeCategory == catName;

                var catBtn = new Button(() => { _activeCategory = isExpanded ? "" : catName; RefreshLeftPanel(); })
                {
                    text = (isExpanded ? "▼ " : "► ") + catName.ToUpper()
                };
                catBtn.style.unityTextAlign = TextAnchor.MiddleLeft;
                catBtn.style.backgroundColor = isExpanded ? theme.PrimaryAccent : new Color(0.12f, 0.12f, 0.15f);
                catBtn.style.color = isExpanded ? Color.black : Color.white;

                _leftScroll.Add(catBtn);

                if (isExpanded)
                {
                    foreach (var type in kvp.Value)
                    {
                        string displayName = type.Name.Replace(catName + "_", "");
                        var typeBtn = new Button(() => SelectProvider(type)) { text = "   " + displayName };
                        typeBtn.style.unityTextAlign = TextAnchor.MiddleLeft;
                        typeBtn.style.backgroundColor = new Color(0.08f, 0.08f, 0.1f, 0.8f);
                        typeBtn.Border(0, Color.clear).style.borderLeftWidth = 3;
                        typeBtn.style.borderLeftColor = theme.PrimaryAccent;
                        _leftScroll.Add(typeBtn);
                    }
                }
            }
        }

        private void SelectProvider(Type type)
        {
            try
            {
                _activeProvider = (IGuiProvider)Activator.CreateInstance(type);
                _headerTitle.text = _activeProvider.Title?.ToUpper() ?? type.Name.ToUpper();
                _bakeBtn.style.display = DisplayStyle.Flex;

                _previewContainer.Clear();
                _previewContainer.Add(_activeProvider.CreateGui(new GuiContext()));

                // Keep the category expanded but clear dashboard styling
                if (_activeCategory == "DASHBOARD") _activeCategory = "";
                RefreshLeftPanel();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[The Forge] Failed to load: {ex.Message}");
            }
        }

        private void BakeActiveProvider()
        {
            if (_activeProvider == null) return;
            string path = EditorUtility.SaveFilePanelInProject("Bake Component", _activeProvider.GetType().Name, "uxml", "Save UI");
            if (!string.IsNullOrEmpty(path))
            {
                _activeProvider.ToUIDocument(path);
                AssetDatabase.Refresh();
            }
        }
    }
}
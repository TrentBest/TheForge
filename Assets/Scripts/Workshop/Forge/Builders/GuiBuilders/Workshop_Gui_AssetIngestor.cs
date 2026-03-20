#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using TheSingularityWorkshop.Forge.Intents;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders
{
    public class Workshop_Gui_AssetIngestor : IGuiProvider
    {
        public string Title => "ASSET INGESTION & INTENT";

        private string _targetAssetPath = "Assets/";
        private ScrollView _listContainer;
        private VisualElement _detailContainer;

        private string _selectedAssetPath;
        private string _selectedAssetType;

        private ScriptableObject _inMemoryConfigData;
        private IAssetIntentBuilder _activeIntentBuilder;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var splitPanel = new SplitPanelBuilder(350)
                .WithSidebar(CreateSidebar())
                .WithMain(CreateMainDetail());

            return splitPanel.CreateGui(ctx);
        }

        private IGuiProvider CreateSidebar()
        {
            var sidebar = new GraphicalUserInterfaceBuilder("Sidebar")
                .WithPadding(10)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f))
                .WithFlexGrow(1).WithFlexShrink(1);

            sidebar.AddChild(new Label("1. SELECT SOURCE FOLDER") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });

            var pathRow = new GraphicalUserInterfaceBuilder("PathRow").WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center);

            var pathField = new TextField { value = _targetAssetPath, style = { flexGrow = 1 } };
            pathField.RegisterValueChangedCallback(evt => _targetAssetPath = evt.newValue);

            var browseBtn = new Button(() => {
                string path = EditorUtility.OpenFolderPanel("Select Asset Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    _targetAssetPath = "Assets" + path.Substring(Application.dataPath.Length);
                    pathField.value = _targetAssetPath;
                }
            })
            { text = "BROWSE" };

            pathRow.AddChild(c => pathField);
            pathRow.AddChild(c => browseBtn);

            sidebar.AddChild(pathRow);
            sidebar.AddButton("SCAN FOLDER", RunScan);
            sidebar.AddSeparator(Color.gray, 1);

            sidebar.AddChild(new GraphicalUserInterfaceBuilder("ListContainerWrapper")
                .WithScrollable(true, ScrollViewMode.Vertical)
                .WithFlexGrow(1).WithFlexShrink(1)
                .OnBuild(ve => {
                    _listContainer = ve.Q<ScrollView>();
                })
            );

            return sidebar;
        }

        private IGuiProvider CreateMainDetail()
        {
            var main = new GraphicalUserInterfaceBuilder("MainDetail")
                    .WithPadding(20)
                    .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                    .WithFlexGrow(1).WithFlexShrink(1)
                    .OnBuild(ve => {
                        _detailContainer = ve;
                        ve.RegisterCallback<AttachToPanelEvent>(evt => RenderDetailContent());
                    });

            return main;
        }

        private void RunScan()
        {
            if (_listContainer == null) return;
            _listContainer.Clear();

            if (!AssetDatabase.IsValidFolder(_targetAssetPath)) return;

            string[] guids = AssetDatabase.FindAssets("", new[] { _targetAssetPath });
            var groupedAssets = new Dictionary<string, List<string>>();

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(path)) continue;

                Type assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (assetType == null) continue;

                string typeName = assetType.Name;
                if (!groupedAssets.ContainsKey(typeName))
                    groupedAssets[typeName] = new List<string>();

                groupedAssets[typeName].Add(path);
            }

            foreach (var kvp in groupedAssets)
            {
                var foldout = new Foldout { text = $"{kvp.Key} ({kvp.Value.Count})", style = { color = Color.yellow, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 } };
                foldout.value = true;

                foreach (var path in kvp.Value)
                {
                    string assetName = Path.GetFileNameWithoutExtension(path);
                    Texture icon = AssetDatabase.GetCachedIcon(path);

                    var card = new GraphicalUserInterfaceBuilder($"Card_{assetName}")
                        .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f))
                        .WithMarginAll(2).WithPadding(5).WithBorderWidth(1).WithBorderColor(Color.gray)
                        .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                        .WithFlexShrink(0)
                        .OnBuild(ve => {
                            ve.RegisterCallback<PointerDownEvent>(evt => SelectAsset(path, kvp.Key));
                        });

                    if (icon != null)
                    {
                        card.AddChild(new GraphicalUserInterfaceBuilder("Thumb")
                            .OnBuild(ve => {
                                ve.style.width = 30; ve.style.height = 30; ve.style.marginRight = 10;
                                ve.style.backgroundImage = (Texture2D)icon;
                                ve.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                            }));
                    }

                    card.AddChild(new Label(assetName) { style = { color = Color.white, fontSize = 11 } });
                    foldout.Add(card.Build());
                }
                _listContainer.Add(foldout);
            }
        }

        private void SelectAsset(string path, string assetType)
        {
            _selectedAssetPath = path;
            _selectedAssetType = assetType;
            RenderDetailContent();
        }

        private void RenderDetailContent()
        {
            if (_detailContainer == null) return;
            _detailContainer.Clear();

            _inMemoryConfigData = null;
            _activeIntentBuilder = null;

            if (string.IsNullOrEmpty(_selectedAssetPath))
            {
                _detailContainer.Add(new Label("Select an asset to express intent.") { style = { color = Color.gray, alignSelf = Align.Center, marginTop = 100 } });
                return;
            }

            string assetName = Path.GetFileNameWithoutExtension(_selectedAssetPath);
            var contentBuilder = new GraphicalUserInterfaceBuilder("DetailContent").WithFlexGrow(1).WithFlexShrink(1);

            // DYNAMIC PREVIEW
            if (_selectedAssetType == "GameObject")
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_selectedAssetPath);
                contentBuilder.AddChild(new GraphicalUserInterfaceBuilder("PreviewWrapper")
                    .WithHeight(300).WithFlexShrink(0)
                    .AddChild(new LiveModelPreviewBuilder(prefab))
                );
            }
            else if (_selectedAssetType == "Texture2D")
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(_selectedAssetPath);
                contentBuilder.AddChild(new GraphicalUserInterfaceBuilder("PreviewWrapper")
                    .WithHeight(300).WithFlexShrink(0)
                    .OnBuild(ve => {
                        ve.style.backgroundImage = tex;
                        ve.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                        ve.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                        ve.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
                    })
                );
            }

            contentBuilder.AddSeparator(Color.gray, 1);
            contentBuilder.AddChild(new Label($"EXPRESS INTENT: {assetName}") { style = { color = Color.yellow, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10, marginTop = 10 } });

            // DYNAMIC INTENT CONFIGURATION
            if (_selectedAssetType == "GameObject")
            {
                var choices = IntentRegistry.GetAvailableIntents();
                if (choices.Count == 0) choices.Add("NO BUILDERS FOUND");

                var intentSelectionRow = new GraphicalUserInterfaceBuilder("IntentSelectionRow")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                    .WithMarginBottom(10);

                var dropdown = new DropdownField("Workshop Identity:", choices, 0) { style = { flexGrow = 1, marginRight = 10 } };

                var manageBtn = new Button(() => {
                    // Pass the active dropdown intent (e.g., "UNIT") to the CRUD Loader
                    ManageIdentitiesCRUD(dropdown.value);
                })
                {
                    text = "⚙ MANAGE",
                    style = { width = 100, height = 25, backgroundColor = new Color(0.15f, 0.4f, 0.6f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold }
                };

                intentSelectionRow.AddChild(c => dropdown);
                intentSelectionRow.AddChild(c => manageBtn);
                contentBuilder.AddChild(intentSelectionRow);

                var actionContainer = new GraphicalUserInterfaceBuilder("ActionContainer").WithMarginTop(10);
                actionContainer.AddButton("CONFIGURE INTENT", () => { EnterConfigurationMode(dropdown.value, assetName); });
                contentBuilder.AddChild(actionContainer);
            }

            _detailContainer.Add(contentBuilder.Build());
        }

        private void EnterConfigurationMode(string intentName, string assetName)
        {
            _activeIntentBuilder = IntentRegistry.GetBuilder(intentName);
            if (_activeIntentBuilder == null) return;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_selectedAssetPath);
            _inMemoryConfigData = _activeIntentBuilder.CreateInMemoryData(prefab, assetName);

            var actionContainer = _detailContainer.Q<VisualElement>("ActionContainer");
            actionContainer.Clear();

            actionContainer.Add(new Label($"CONFIGURING: {intentName}") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });

            var serializedObj = new SerializedObject(_inMemoryConfigData);
            var inspector = new InspectorElement(serializedObj)
            {
                style = { backgroundColor = new Color(0.15f, 0.15f, 0.18f), paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10,
                    borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1,
                    borderTopColor = Color.gray, borderBottomColor = Color.gray,borderLeftColor = Color.gray,borderRightColor = Color.gray,
                    marginBottom = 15 }
            };
            actionContainer.Add(inspector);

            var saveBtn = new Button(() => CommitDataToDisk(assetName))
            {
                text = "FINALIZE & SAVE TO DATABANK",
                style = { height = 40, backgroundColor = new Color(0, 0.5f, 0.2f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold }
            };
            actionContainer.Add(saveBtn);
        }

        private void CommitDataToDisk(string assetName)
        {
            if (_inMemoryConfigData == null || _activeIntentBuilder == null) return;

            string intentName = _activeIntentBuilder.IntentName;
            string dataPath = "Assets/WorkshopData";

            if (!AssetDatabase.IsValidFolder(dataPath)) AssetDatabase.CreateFolder("Assets", "WorkshopData");
            if (!AssetDatabase.IsValidFolder($"{dataPath}/{intentName}")) AssetDatabase.CreateFolder(dataPath, intentName);

            _activeIntentBuilder.SaveToDisk(_inMemoryConfigData, $"{dataPath}/{intentName}", assetName);
            RenderDetailContent();
        }

        // --- NEW: THE STRATEGY CRUD LAUNCHER ---
        private void ManageIdentitiesCRUD(string intentName)
        {
            if (_detailContainer == null) return;
            _detailContainer.Clear();

            var crudCanvas = new GraphicalUserInterfaceBuilder("CRUD_Canvas")
                .WithFlexGrow(1).WithFlexShrink(1)
                .WithFlexLayout(FlexDirection.Column, Justify.SpaceBetween, Align.Stretch);

            var builder = IntentRegistry.GetBuilder(intentName);
            if (builder != null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_selectedAssetPath);

                // Get the unique CRUD_Builder from the Registry Intent!
                var crudProvider = builder.GetManagerGui(prefab);

                if (crudProvider != null)
                {
                    // Add the CRUD UI directly into the container. It will auto-expand.
                    crudCanvas.AddChild(new GraphicalUserInterfaceBuilder("CrudWrapper")
                        .WithFlexGrow(1).WithFlexShrink(1)
                        .AddChild(crudProvider));
                }
                else
                {
                    crudCanvas.AddChild(new Label($"[System] The {intentName} builder has no CRUD manager implemented yet.") { style = { color = Color.gray, marginTop = 20, marginBottom = 20, marginLeft = 20, marginRight = 20 } });
                }
            }

            // Always provide a way back out!
            crudCanvas.AddChild(new GraphicalUserInterfaceBuilder("Footer")
                .WithPadding(10).WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f))
                .AddButton("<< RETURN TO INGESTOR", RenderDetailContent)
            );

            _detailContainer.Add(crudCanvas.Build());
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string path) { }
        public void ToUIDocument(string path) { }
    }
}
#endif
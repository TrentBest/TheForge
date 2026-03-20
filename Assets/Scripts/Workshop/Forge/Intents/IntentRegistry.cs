#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using TheSingularityWorkshop.Stage;
using TheSingularityWorkshop.Cast;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace TheSingularityWorkshop.Forge.Intents
{
    public interface IAssetIntentBuilder
    {
        string IntentName { get; }
        ScriptableObject CreateInMemoryData(GameObject sourcePrefab, string assetName);
        void SaveToDisk(ScriptableObject data, string folderPath, string assetName);

        // --- NEW: Dynamic CRUD Provider ---
        IGuiProvider GetManagerGui(GameObject targetAsset);
    }

    public static class IntentRegistry
    {
        private static Dictionary<string, IAssetIntentBuilder> _registry;

        public static void Initialize()
        {
            if (_registry != null) return;
            _registry = new Dictionary<string, IAssetIntentBuilder>();

            var types = TypeCache.GetTypesDerivedFrom<IAssetIntentBuilder>()
                                 .Where(t => !t.IsInterface && !t.IsAbstract);

            foreach (var type in types)
            {
                var instance = (IAssetIntentBuilder)Activator.CreateInstance(type);
                _registry[instance.IntentName] = instance;
            }
        }

        public static List<string> GetAvailableIntents() { Initialize(); return _registry.Keys.ToList(); }
        public static IAssetIntentBuilder GetBuilder(string intentName) { Initialize(); return _registry.TryGetValue(intentName, out var builder) ? builder : null; }
    }

    // ====================================================================
    // UNIT BUILDER (Now with Sockets & Components CRUD!)
    // ====================================================================

    // A temporary data model to prove your CRUD Builder works seamlessly
    public class WorkshopSocket
    {
        public string Name { get; set; } = "New Socket";
        public string SocketCategory { get; set; } = "Weapon Hardpoint";
        public Vector3 LocalOffset { get; set; } = Vector3.zero;
    }

    public class ActorIntentBuilder : IAssetIntentBuilder
    {
        public string IntentName => "UNIT";

        // Temporary data store just for the UI showcase (In reality, you'd serialize this to your prefab or SO)
        private List<WorkshopSocket> _mockSockets = new List<WorkshopSocket>();

        public ScriptableObject CreateInMemoryData(GameObject sourcePrefab, string assetName)
        {
            var actorDef = ScriptableObject.CreateInstance<ActorDefinition>();
            actorDef.DisplayName = assetName.Replace("unit_", "").Replace("_A_yup", "");
            actorDef.PrefabResourcePath = AssetDatabase.GetAssetPath(sourcePrefab);
            return actorDef;
        }

        public void SaveToDisk(ScriptableObject data, string folderPath, string assetName)
        {
            string fullPath = $"{folderPath}/{assetName}_Def.asset";
            AssetDatabase.CreateAsset(data, fullPath);
            AssetDatabase.SaveAssets();
        }

        public IGuiProvider GetManagerGui(GameObject targetAsset)
        {
            // Here we utilize YOUR CRUD_Builder, strongly typed to our custom WorkshopSocket class!
            return new CRUD_Builder<WorkshopSocket>(
                title: $"UNIT INTEGRATION: {targetAsset.name}",
                dataSource: () => _mockSockets,
                getDisplayName: item => item.Name,
                buildEditorForm: item =>
                {
                    var form = new VisualElement();

                    form.Add(new Label("SOCKET CONFIGURATION") { style = { color = Color.cyan, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 15 } });

                    var nameField = new TextField("Identity Tag") { value = item.Name };
                    nameField.RegisterValueChangedCallback(e => item.Name = e.newValue);

                    var categoryField = new TextField("Category (e.g. CPU, Armor)") { value = item.SocketCategory };
                    categoryField.RegisterValueChangedCallback(e => item.SocketCategory = e.newValue);

                    var posField = new Vector3Field("Local Offset") { value = item.LocalOffset };
                    posField.RegisterValueChangedCallback(e => item.LocalOffset = e.newValue);

                    form.Add(nameField);
                    form.Add(categoryField);
                    form.Add(posField);

                    return form;
                },
                onSave: item => { if (!_mockSockets.Contains(item)) _mockSockets.Add(item); },
                onDelete: item => { _mockSockets.Remove(item); },
                getGroupCategory: item => item.SocketCategory // Groups the list by Socket Type!
            ).GetGuiProvider();
        }
    }

    // ====================================================================
    // PROP BUILDER
    // ====================================================================

    public class PropIntentBuilder : IAssetIntentBuilder
    {
        public string IntentName => "PROP";

        public ScriptableObject CreateInMemoryData(GameObject sourcePrefab, string assetName)
        {
            var propDef = ScriptableObject.CreateInstance<PropDefinition>();
            propDef.DisplayName = assetName;
            propDef.PrefabResourcePath = AssetDatabase.GetAssetPath(sourcePrefab);
            var bounds = new Bounds(sourcePrefab.transform.position, Vector3.zero);
            foreach (var renderer in sourcePrefab.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(renderer.bounds);
            propDef.BoundingBox = bounds.size;
            return propDef;
        }

        public void SaveToDisk(ScriptableObject data, string folderPath, string assetName)
        {
            string fullPath = $"{folderPath}/{assetName}_Def.asset";
            AssetDatabase.CreateAsset(data, fullPath);
            AssetDatabase.SaveAssets();
        }

        public IGuiProvider GetManagerGui(GameObject targetAsset)
        {
            // For now, Prop just returns a dummy GUI, but you can build a CRUD_Builder<PropFeature> for ladders/destructibles!
            return new GraphicalUserInterfaceBuilder("PropManager")
                .WithPadding(20)
                .AddChild(new Label($"MANAGE PROP: {targetAsset.name}") { style = { color = Color.yellow, fontSize = 20 } })
                .AddChild(new Label("Prop feature manager offline. Awaiting CRUD integration."));
        }
    }
}
#endif
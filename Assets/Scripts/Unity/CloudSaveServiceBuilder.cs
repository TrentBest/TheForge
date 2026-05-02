#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Memory;
using Workshop.GURPS;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Unity
{
    /// <summary>
    /// Advanced Cloud Save Builder with DataWarehouse and DataShelf liaison logic.
    /// Uses the generic ReflectiveGuiBuilder to bridge warehouse state to the UI.
    /// </summary>
    public class CloudSaveServiceBuilder : UnityServiceFluentBuilder<CloudSaveServiceBuilder, UnityServicesConfig>
    {
        public CloudSaveServiceBuilder(UnityServicesConfig config) : base(config) { }

        public override GraphicalUserInterfaceBuilder GetGuiBuilder()
        {
            var serializedConfig = new SerializedObject(_config);

            return new GraphicalUserInterfaceBuilder("CloudSave_FluentGui")
                .WithPadding(20)
                .AddChild(new Label("CLOUD SAVE & DATA SHELF")
                {
                    style = { color = Color.cyan, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 }
                })
                .AddChild(new Label("Link global economy, voxel structures, and player progress to the persistent cloud vault.")
                {
                    style = { color = Color.gray, marginBottom = 20, whiteSpace = WhiteSpace.Normal }
                })
                .AddChild(new PropertyField(serializedConfig.FindProperty("EnableCloudSave")))

                // --- DATAWAREHOUSE LIAISON ---
                .AddChild(new Label("DATA SHELF MAPPING")
                {
                    style = { color = Color.white, marginTop = 15, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 12 }
                })
                .AddChild(new Label("The following shelves are currently synchronized with the cloud vault:")
                {
                    style = { color = Color.gray, fontSize = 9, marginBottom = 10 }
                })

                // FIX: Utilizing the generic ReflectiveGuiBuilder to inspect the DataWarehouse instance
                .AddChild(new ReflectiveGuiBuilder<DataWarehouse>()
                    .WithFilter("CloudSync")
                    .Build())

                .AddChild(new Label("VAULT STATUS: ENCRYPTED")
                {
                    style = { color = new Color(0.4f, 0.8f, 0.4f), marginTop = 20, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold }
                })
                .OnBuild(ve => ve.Bind(serializedConfig));
        }
    }
}
#endif
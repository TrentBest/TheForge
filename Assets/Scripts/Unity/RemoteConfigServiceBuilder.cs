#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Unity
{
    public class RemoteConfigServiceBuilder : UnityServiceFluentBuilder<RemoteConfigServiceBuilder, UnityServicesConfig>
    {
        public RemoteConfigServiceBuilder(UnityServicesConfig config) : base(config) { }

        public RemoteConfigServiceBuilder WithAutoRefresh(bool enabled)
        {
            _config.AutoRefreshRemoteConfig = enabled;
            return Self;
        }

        public RemoteConfigServiceBuilder WithInterval(float seconds)
        {
            _config.RemoteConfigRefreshIntervalSeconds = seconds;
            return Self;
        }

        public override GraphicalUserInterfaceBuilder GetGuiBuilder()
        {
            var serializedConfig = new SerializedObject(_config);

            return new GraphicalUserInterfaceBuilder("RemoteConfig_FluentGui")
                .WithPadding(20)
                .AddChild(new Label("REMOTE CONFIGURATION ENGINE")
                {
                    style = { color = Color.cyan, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 }
                })
                .AddChild(new Label("Configure how the Forge synchronizes with live variables in the cloud.")
                {
                    style = { color = Color.gray, marginBottom = 20, whiteSpace = WhiteSpace.Normal }
                })
                .AddChild(new PropertyField(serializedConfig.FindProperty("AutoRefreshRemoteConfig")))
                .AddChild(new PropertyField(serializedConfig.FindProperty("RemoteConfigRefreshIntervalSeconds")))
                .OnBuild(ve => ve.Bind(serializedConfig));
        }
    }
}
#endif
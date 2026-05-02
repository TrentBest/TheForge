#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Unity
{
    public class LobbyServiceBuilder : UnityServiceFluentBuilder<LobbyServiceBuilder, UnityServicesConfig>
    {
        public LobbyServiceBuilder(UnityServicesConfig config) : base(config) { }

        public LobbyServiceBuilder WithMatchmaking(string region, bool useSkill)
        {
            _config.ActiveLobbyType = LobbyType.PublicMatchmaking;
            _config.MatchmakingRegion = region;
            _config.UseSkillBasedSorting = useSkill;
            return Self;
        }

        public override GraphicalUserInterfaceBuilder GetGuiBuilder()
        {
            var serializedConfig = new SerializedObject(_config);

            var builder = new GraphicalUserInterfaceBuilder("LobbyHandshake_FluentGui")
                .WithPadding(20)
                .AddChild(new Label("MULTIPLAYER & RELAY ORCHESTRATION")
                {
                    style = { color = Color.cyan, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 }
                })
                .AddChild(new Label("Configure the handshake protocols for peer-to-peer and hosted sessions.")
                {
                    style = { color = Color.gray, marginBottom = 20, whiteSpace = WhiteSpace.Normal }
                })
                .AddChild(new PropertyField(serializedConfig.FindProperty("EnableLobbyServices")))
                .AddChild(new PropertyField(serializedConfig.FindProperty("ActiveLobbyType")))
                .AddChild(new PropertyField(serializedConfig.FindProperty("MaxPlayers")));

            // --- CONDITIONAL HANDSHAKE UI ---
            builder.OnBuild(ve => {
                ve.Bind(serializedConfig);

                var matchmakingContainer = new VisualElement { style = { marginTop = 15, paddingLeft = 10, borderLeftWidth = 2, borderLeftColor = Color.magenta } };
                matchmakingContainer.Add(new Label("MATCHMAKING SETTINGS") { style = { color = Color.magenta, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold } });
                matchmakingContainer.Add(new PropertyField(serializedConfig.FindProperty("MatchmakingRegion")));
                matchmakingContainer.Add(new PropertyField(serializedConfig.FindProperty("UseSkillBasedSorting")));

                // --- CRITICAL UI TICK PATTERN ---
                // Only show matchmaking settings if Public Matchmaking is selected
                ve.schedule.Execute(() => {
                    matchmakingContainer.style.display = (_config.ActiveLobbyType == LobbyType.PublicMatchmaking)
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                }).Every(100);

                ve.Add(matchmakingContainer);
            });

            return builder;
        }
    }
}
#endif
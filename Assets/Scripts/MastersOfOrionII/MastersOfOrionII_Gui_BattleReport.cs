using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_BattleReport : IGuiProvider
    {
        public string Title => "AFTER ACTION TACTICAL REPORT";

        private MastersOfOrionII_Game _game;
        private GuiContext _lastCtx;

        // --- UI References ---
        private VisualElement _simulationViewport;
        private Label _playbackStatusLabel;
        private bool _isPlayingReplay = false;

        // --- MOCK BATTLE DATA ---
        private class ShipGroup
        {
            public string ClassName;
            public string HullType;
            public int StartCount;
            public int SurvivedCount;
            public int Kills;
            public Color Theme;
        }

        private class FleetData
        {
            public string EmpireName;
            public string CommanderName;
            public Color FleetColor;
            public List<ShipGroup> Ships;
            public bool IsWinner;
        }

        private string _battleLocation = "Sirius Binary System";
        private FleetData _leftFleet;
        private FleetData _rightFleet;

        public MastersOfOrionII_Gui_BattleReport()
        {
            InitializeMockData();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;
            _game = UnityEngine.Object.FindAnyObjectByType<MastersOfOrionII_Game>();

            var builder = new GraphicalUserInterfaceBuilder("BattleReport_Root")
                .WithBackgroundColor(new Color(0.01f, 0.02f, 0.03f, 0.98f))
                .WithPercentSize(100, 100)
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Stretch);

            // --- LEFT COLUMN: ALLIED FLEET (20%) ---
            builder.AddChild(c => CreateFleetColumn(_leftFleet, true));

            // --- CENTER COLUMN: TACTICAL VIEWPORT & CONTROLS (60%) ---
            builder.AddChild(c =>
            {
                var center = new VisualElement { style = { width = Length.Percent(60), flexDirection = FlexDirection.Column, paddingLeft = 10, paddingRight = 10, paddingTop = 20, paddingBottom = 20 } };

                // Header
                var header = new VisualElement { style = { alignItems = Align.Center, marginBottom = 15 } };
                header.Add(new Label("AFTER ACTION REPORT") { style = { color = Color.white, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold } });
                header.Add(new Label($"Engagement at: {_battleLocation}") { style = { color = Color.gray, fontSize = 14 } });

                var outcomeStr = _leftFleet.IsWinner ? "VICTORY" : "DEFEAT";
                var outcomeCol = _leftFleet.IsWinner ? Color.green : Color.red;
                header.Add(new Label($"TACTICAL OUTCOME: {outcomeStr}") { style = { color = outcomeCol, fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 } });
                center.Add(header);

                // SIMULATION VIEWPORT (The 3D Window)
                _simulationViewport = new VisualElement
                {
                    style = {
                    flexGrow = 1, backgroundColor = new Color(0, 0, 0, 0.8f),
                    borderTopWidth = 2, borderBottomWidth = 2, borderLeftWidth = 2, borderRightWidth = 2,
                    borderTopColor = Color.cyan, borderBottomColor = Color.cyan, borderLeftColor = Color.cyan, borderRightColor = Color.cyan,
                    alignItems = Align.Center, justifyContent = Justify.Center, marginBottom = 10
                }
                };

                // Overlay for the Viewport
                _playbackStatusLabel = new Label("SIMULATION OFFLINE\n\n(3D render space for doctrine-driven ship combat)") { style = { color = new Color(1, 1, 1, 0.3f), fontSize = 14, unityTextAlign = TextAnchor.MiddleCenter } };
                _simulationViewport.Add(_playbackStatusLabel);

                var observeBtn = new Button(() => StartReplay()) { text = "OBSERVE TACTICAL REPLAY", style = { height = 60, width = 250, fontSize = 16, backgroundColor = new Color(0, 0.4f, 0.6f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, position = Position.Absolute } };
                _simulationViewport.Add(observeBtn);

                center.Add(_simulationViewport);

                // CAMERA & PLAYBACK CONTROLS
                var controlsRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, backgroundColor = new Color(0.1f, 0.1f, 0.15f), paddingLeft = 10, paddingTop = 10, paddingRight = 10, paddingBottom = 10 } };

                // Playback
                var playbackBox = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
                playbackBox.Add(new Button(() => Debug.Log("Rewind")) { text = "◄◄", style = { width = 40, height = 30 } });
                playbackBox.Add(new Button(() => Debug.Log("Play/Pause")) { text = "► / II", style = { width = 60, height = 30 } });
                playbackBox.Add(new Button(() => Debug.Log("Fast Forward")) { text = "►►", style = { width = 40, height = 30 } });
                controlsRow.Add(playbackBox);

                // Camera
                var camBox = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
                camBox.Add(new Label("CAMERA:") { style = { color = Color.cyan, fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, marginRight = 10 } });
                camBox.Add(new Button(() => Debug.Log("Camera: Orbit Center")) { text = "ORBIT BATTLESPACE", style = { height = 30, backgroundColor = new Color(0.2f, 0.2f, 0.2f), color = Color.white } });
                camBox.Add(new Button(() => Debug.Log("Camera: Lock Target")) { text = "LOCK SELECTED SHIP", style = { height = 30, backgroundColor = new Color(0.2f, 0.2f, 0.2f), color = Color.white } });
                camBox.Add(new Button(() => Debug.Log("Camera: Zoom In")) { text = "ZOOM +", style = { height = 30, width = 60 } });
                camBox.Add(new Button(() => Debug.Log("Camera: Zoom Out")) { text = "ZOOM -", style = { height = 30, width = 60 } });
                controlsRow.Add(camBox);

                center.Add(controlsRow);

                var exitBtn = new Button(() => _game?.SwitchGui("GalaxyView")) { text = "ACKNOWLEDGE REPORT & RETURN TO GALAXY MAP", style = { height = 40, marginTop = 15, backgroundColor = new Color(0.4f, 0.1f, 0.1f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } };
                center.Add(exitBtn);

                return center;
            });

            // --- RIGHT COLUMN: ENEMY FLEET (20%) ---
            builder.AddChild(c => CreateFleetColumn(_rightFleet, false));

            return builder.Build();
        }

        private VisualElement CreateFleetColumn(FleetData fleet, bool isLeft)
        {
            var col = new VisualElement { style = { width = Length.Percent(20), backgroundColor = new Color(0.05f, 0.05f, 0.08f), paddingTop = 20, paddingBottom = 20, paddingLeft = 15, paddingRight = 15, borderRightWidth = isLeft ? 2 : 0, borderLeftWidth = isLeft ? 0 : 2, borderRightColor = fleet.FleetColor, borderLeftColor = fleet.FleetColor } };

            col.Add(new Label(fleet.EmpireName) { style = { color = fleet.FleetColor, fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, unityTextAlign = isLeft ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight } });
            col.Add(new Label($"Cmdr: {fleet.CommanderName}") { style = { color = Color.white, fontSize = 12, marginBottom = 20, unityTextAlign = isLeft ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight } });

            var scroll = new ScrollView { style = { flexGrow = 1 } };

            foreach (var group in fleet.Ships)
            {
                var card = new VisualElement { style = { backgroundColor = new Color(0.1f, 0.1f, 0.15f), marginBottom = 10, paddingLeft = 10, paddingTop = 10, paddingBottom = 10, paddingRight = 10, borderLeftWidth = isLeft ? 4 : 0, borderRightWidth = isLeft ? 0 : 4, borderLeftColor = fleet.FleetColor, borderRightColor = fleet.FleetColor } };

                var header = new VisualElement { style = { flexDirection = isLeft ? FlexDirection.Row : FlexDirection.RowReverse, justifyContent = Justify.SpaceBetween, marginBottom = 5 } };
                header.Add(new Label(group.ClassName) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });
                header.Add(new Label($"x{group.StartCount}") { style = { color = Color.gray, fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold } });
                card.Add(header);

                card.Add(new Label($"Hull: {group.HullType}") { style = { color = Color.gray, fontSize = 9, unityTextAlign = isLeft ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight, marginBottom = 5 } });

                var statsRow = new VisualElement { style = { flexDirection = isLeft ? FlexDirection.Row : FlexDirection.RowReverse, justifyContent = Justify.SpaceBetween, marginTop = 5, paddingTop = 5, borderTopWidth = 1, borderTopColor = new Color(1, 1, 1, 0.1f) } };

                // Survived logic
                int lost = group.StartCount - group.SurvivedCount;
                Color survCol = group.SurvivedCount == 0 ? Color.red : (lost > 0 ? Color.yellow : Color.green);
                statsRow.Add(new Label($"Survived: {group.SurvivedCount}") { style = { color = survCol, fontSize = 11 } });

                statsRow.Add(new Label($"Kills: {group.Kills}") { style = { color = Color.cyan, fontSize = 11 } });
                card.Add(statsRow);

                // Interactable to "Lock Camera" during playback
                card.RegisterCallback<ClickEvent>(e =>
                {
                    if (_isPlayingReplay) Debug.Log($"Camera Locked to {group.ClassName} squadron.");
                    else Debug.Log($"Selected {group.ClassName} in report.");
                });

                // Hover effect
                card.RegisterCallback<MouseEnterEvent>(e => card.style.backgroundColor = new Color(0.2f, 0.2f, 0.25f));
                card.RegisterCallback<MouseLeaveEvent>(e => card.style.backgroundColor = new Color(0.1f, 0.1f, 0.15f));

                scroll.Add(card);
            }

            col.Add(scroll);

            // Summary at bottom
            int totalStart = fleet.Ships.Sum(s => s.StartCount);
            int totalSurvived = fleet.Ships.Sum(s => s.SurvivedCount);
            float survivalRate = totalStart > 0 ? (float)totalSurvived / totalStart : 0;

            var summary = new VisualElement { style = { marginTop = 20, paddingTop = 10, borderTopWidth = 1, borderTopColor = fleet.FleetColor } };
            summary.Add(new Label($"Total Engaged: {totalStart:N0}") { style = { color = Color.white, unityTextAlign = isLeft ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight } });
            summary.Add(new Label($"Total Survived: {totalSurvived:N0} ({survivalRate:P0})") { style = { color = survivalRate > 0.5f ? Color.green : Color.red, unityTextAlign = isLeft ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight } });
            col.Add(summary);

            return col;
        }

        private void StartReplay()
        {
            _isPlayingReplay = true;

            // Hide the button
            var btn = _simulationViewport.Q<Button>();
            if (btn != null) btn.style.display = DisplayStyle.None;

            // Change the overlay text to simulate the camera feed
            _playbackStatusLabel.text = ">>> TACTICAL FEED ACTIVE <<<\n(Unity Camera rendering the battle here)";
            _playbackStatusLabel.style.color = Color.cyan;
            _simulationViewport.style.backgroundColor = new Color(0.05f, 0.1f, 0.15f); // Make it look "active"

            Debug.Log("Initiating Battle Replay.. (This would activate the 3D battle scene cameras)");
        }

        private void InitializeMockData()
        {
            _leftFleet = new FleetData
            {
                EmpireName = "TERRAN FEDERATION",
                CommanderName = "Admiral Hackett",
                FleetColor = Color.cyan,
                IsWinner = true,
                Ships = new List<ShipGroup>
            {
                new ShipGroup { ClassName = "Vanguard", HullType = "Cruiser", StartCount = 5, SurvivedCount = 3, Kills = 12 },
                new ShipGroup { ClassName = "Aegis", HullType = "Destroyer", StartCount = 15, SurvivedCount = 8, Kills = 24 },
                new ShipGroup { ClassName = "Sparrow", HullType = "Corvette", StartCount = 50, SurvivedCount = 12, Kills = 8 }
            }
            };

            _rightFleet = new FleetData
            {
                EmpireName = "KRAAL HEGEMONY",
                CommanderName = "Warlord Vrok",
                FleetColor = Color.red,
                IsWinner = false,
                Ships = new List<ShipGroup>
            {
                new ShipGroup { ClassName = "Bloodfang", HullType = "Dreadnought", StartCount = 1, SurvivedCount = 0, Kills = 4 },
                new ShipGroup { ClassName = "Ravager", HullType = "Cruiser", StartCount = 8, SurvivedCount = 0, Kills = 18 },
                new ShipGroup { ClassName = "Swarm Rider", HullType = "Fighter Wing", StartCount = 150, SurvivedCount = 0, Kills = 23 }
            }
            };
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
    }
}
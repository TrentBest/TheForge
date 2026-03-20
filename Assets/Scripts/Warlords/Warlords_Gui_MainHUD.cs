using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Warlords
{
    public class Warlords_Gui_MainMenu : IGuiProvider
    {
        public string Title => "WARLORDS: MAIN MENU";

        private VisualElement _chroniclesContent;
        private VisualElement _gameModeOverlay;
        private VisualElement _centerAnchor; // Added to control the center button visibility
        private GuiContext _lastCtx;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var builder = new GraphicalUserInterfaceBuilder("Warlords_MainMenu_Root")
                .WithPercentSize(100, 100)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(Color.black);

            // --- GRID CONTENT ---
            builder.AddChild(context =>
            {
                var grid = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Column } };

                var topRow = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row } };

                // 1. NEW GAME (Red/Crimson Theme)
                topRow.Add(CreateQuadrant("CONQUEST", "BEGIN A NEW CAMPAIGN", new Color(0.2f, 0.05f, 0.05f),
                    () => ShowGameModeOverlay(true)));

                // 2. LOAD GAME (Green/Forest Theme)
                var chroniclesQuad = CreateQuadrant("ARCHIVES", "RESTORE SAVED GAME", new Color(0.05f, 0.15f, 0.05f), null);
                _chroniclesContent = chroniclesQuad;
                RefreshChroniclesState();
                topRow.Add(chroniclesQuad);

                grid.Add(topRow);

                var bottomRow = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row } };

                // 3. SETTINGS (Blue/Royal Theme)
                bottomRow.Add(CreateQuadrant("SETTINGS", "GAME CONFIGURATION", new Color(0.05f, 0.08f, 0.15f),
                    () => Debug.Log("Navigate to Settings Gui..")));

                // 4. QUIT (Purple/Dark Theme)
                bottomRow.Add(CreateQuadrant("SURRENDER", "EXIT TO DESKTOP", new Color(0.15f, 0.05f, 0.15f),
                    () => Application.Quit()));

                grid.Add(bottomRow);
                return grid;
            });

            // --- THE GAME MODE OVERLAY (Hidden by default) ---
            builder.AddChild(context =>
            {
                _gameModeOverlay = new VisualElement
                {
                    style = {
                        position = Position.Absolute,
                        left = 0, top = 0, right = 0, bottom = 0,
                        backgroundColor = new Color(0, 0, 0, 0.95f),
                        justifyContent = Justify.Center, alignItems = Align.Center,
                        display = DisplayStyle.None // HIDDEN INITIALLY
                    }
                };

                _gameModeOverlay.Add(new Label("CHOOSE BATTLE FORMAT") { style = { color = new Color(0.8f, 0.6f, 0.2f), fontSize = 28, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 40 } });

                // 1. Single Player -> Leads to the Warlords_Gui_GameSetup we built
                _gameModeOverlay.Add(new Button(() => Debug.Log("Transition to Warlords_Gui_GameSetup"))
                {
                    text = "SOLO CAMPAIGN (Single Player)",
                    style = { width = 400, height = 45, marginBottom = 15, backgroundColor = new Color(0.2f, 0.05f, 0.05f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 16 }
                });

                // 2. Hotseat Multiplayer
                _gameModeOverlay.Add(new Button(() => Debug.Log("Transition to Hotseat Setup"))
                {
                    text = "LOCAL ALLIANCE (Hotseat Multiplayer)",
                    style = { width = 400, height = 45, marginBottom = 15, backgroundColor = new Color(0.05f, 0.15f, 0.05f), color = Color.white, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 16 }
                });

                // 3. Network Multiplayer (Placeholder)
                var netBtn = new Button()
                {
                    text = "GLOBAL DOMINATION (Network Play)",
                    tooltip = "Authentication Required: Please sign in to the Singularity Workshop to access network features.",
                    style = { width = 400, height = 45, marginBottom = 15, backgroundColor = new Color(0.1f, 0.1f, 0.1f), color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 16 }
                };
                netBtn.SetEnabled(false); // Grays out the button and blocks clicks
                _gameModeOverlay.Add(netBtn);

                // 4. The Modem Nostalgia Button (Placeholder)
                var modemBtn = new Button()
                {
                    text = "DIRECT DIAL-UP (Modem/IPX)",
                    tooltip = "Requires a 14.4k baud modem and ensuring nobody picks up the phone line in the house.",
                    style = { width = 400, height = 45, marginBottom = 30, backgroundColor = new Color(0.1f, 0.1f, 0.1f), color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold, fontSize = 16 }
                };
                modemBtn.SetEnabled(false);
                _gameModeOverlay.Add(modemBtn);

                // Cancel
                _gameModeOverlay.Add(new Button(() => ShowGameModeOverlay(false))
                {
                    text = "RETREAT",
                    style = { width = 400, height = 30, marginTop = 10, backgroundColor = Color.clear, color = Color.gray, unityFontStyleAndWeight = FontStyle.Bold }
                });

                return _gameModeOverlay;
            });

            // --- CENTER LOGO / SIGN IN ANCHOR ---
            builder.AddChild(context =>
            {
                _centerAnchor = new VisualElement
                {
                    style = {
                        position = Position.Absolute, left = Length.Percent(50), top = Length.Percent(50),
                        width = 0, height = 0, justifyContent = Justify.Center, alignItems = Align.Center
                    },
                    pickingMode = PickingMode.Ignore
                };

                // Acting as both the logo and the Sign In button
                var centerSignInBtn = new Button(() => Debug.Log("Sign In Flow Triggered"))
                {
                    tooltip = "Sign in to enable Network Play",
                    style = {
                        width = 140, height = 140, borderBottomLeftRadius = 70, borderTopLeftRadius = 70,
                        borderBottomRightRadius = 70, borderTopRightRadius = 70,
                        backgroundColor = new Color(0.05f, 0.05f, 0.05f, 1f),
                        borderBottomWidth = 4, borderBottomColor = new Color(0.8f, 0.6f, 0.2f),
                        borderTopWidth = 4, borderTopColor = new Color(0.8f, 0.6f, 0.2f),
                        borderLeftWidth = 4, borderLeftColor = new Color(0.8f, 0.6f, 0.2f),
                        borderRightWidth = 4, borderRightColor = new Color(0.8f, 0.6f, 0.2f),
                        justifyContent = Justify.Center, alignItems = Align.Center,
                        flexDirection = FlexDirection.Column
                    }
                };

                centerSignInBtn.Add(new Label("W") { style = { color = new Color(0.8f, 0.6f, 0.2f), fontSize = 56, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = -10 } });
                centerSignInBtn.Add(new Label("SIGN IN") { style = { color = Color.gray, fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold } });

                // Hover animation
                centerSignInBtn.RegisterCallback<MouseEnterEvent>(e => centerSignInBtn.style.scale = new Scale(Vector3.one * 1.05f));
                centerSignInBtn.RegisterCallback<MouseLeaveEvent>(e => centerSignInBtn.style.scale = new Scale(Vector3.one));

                _centerAnchor.Add(centerSignInBtn);
                return _centerAnchor;
            });

            return builder.Build();
        }

        private void ShowGameModeOverlay(bool show)
        {
            if (_gameModeOverlay != null)
                _gameModeOverlay.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

            // Toggle the center button off when the overlay is open so they don't overlap!
            if (_centerAnchor != null)
                _centerAnchor.style.display = show ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void RefreshChroniclesState()
        {
            _chroniclesContent.Clear();
            var saves = GetCachedSaves();

            if (saves.Count > 0)
            {
                var listContainer = new VisualElement { style = { flexGrow = 1, paddingTop = 20, paddingRight = 20, paddingBottom = 20, paddingLeft = 20 } };
                listContainer.Add(new Label("RECENT ARCHIVES") { style = { fontSize = 18, color = new Color(0.8f, 0.6f, 0.2f), alignSelf = Align.Center, marginBottom = 10 } });

                var scroll = new ScrollView();
                foreach (var save in saves)
                {
                    var row = new Button(() => LoadGame(save))
                    {
                        style = {
                            flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween,
                            backgroundColor = new Color(0,0,0,0.5f), marginBottom = 2,
                            borderLeftWidth = 0, borderRightWidth = 0, borderTopWidth = 0, borderBottomWidth = 0,
                            paddingTop = 10, paddingBottom = 10, paddingLeft = 10, paddingRight = 10
                        }
                    };
                    row.Add(new Label(save.Name) { style = { color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } });
                    row.Add(new Label(save.Date) { style = { color = Color.gray, fontSize = 12 } });
                    scroll.Add(row);
                }

                scroll.Add(new Button(() => OpenFileBrowser()) { text = "LOAD FROM DIRECTORY..", style = { marginTop = 15, height = 30, backgroundColor = new Color(0.2f, 0.2f, 0.2f), color = Color.white } });
                listContainer.Add(scroll);
                _chroniclesContent.Add(listContainer);
            }
            else
            {
                var btn = new Button(() => OpenFileBrowser())
                {
                    style = { flexGrow = 1, backgroundColor = Color.clear, borderLeftWidth = 0, borderRightWidth = 0, borderTopWidth = 0, borderBottomWidth = 0,
                        justifyContent = Justify.Center, alignItems = Align.Center }
                };
                btn.Add(new Label("ARCHIVES") { style = { fontSize = 42, color = new Color(1, 1, 1, 0.5f), unityFontStyleAndWeight = FontStyle.Bold, letterSpacing = 8 } });
                btn.Add(new Label("NO HEROIC DEEDS FOUND - LOAD FROM DISK") { style = { fontSize = 14, color = new Color(0.8f, 0.6f, 0.2f), marginTop = 10 } });
                _chroniclesContent.Add(btn);
            }
        }

        private struct SaveFile { public string Name; public string Date; public string Path; }

        private List<SaveFile> GetCachedSaves() => new List<SaveFile>
        {
            new SaveFile { Name = "Sirians_Turn_42", Date = "2026-02-21" },
            new SaveFile { Name = "LordBane_Conquest", Date = "2026-02-20" }
        };

        private void LoadGame(SaveFile save) { Debug.Log($"Loading {save.Name}.."); }
        private void OpenFileBrowser() { Debug.Log("Opening OS File Browser.."); }

        private VisualElement CreateQuadrant(string title, string subtitle, Color baseColor, System.Action onClick)
        {
            var container = new VisualElement { style = { flexGrow = 1, width = Length.Percent(50), backgroundColor = baseColor, overflow = Overflow.Hidden } };

            if (onClick != null)
            {
                var btn = new Button(onClick)
                {
                    style = { flexGrow = 1, backgroundColor = Color.clear, borderLeftWidth = 0, borderRightWidth = 0, borderTopWidth = 0, borderBottomWidth = 0,
                        justifyContent = Justify.Center, alignItems = Align.Center }
                };

                var titleLbl = new Label(title) { style = { fontSize = 42, color = new Color(1, 1, 1, 0.5f), unityFontStyleAndWeight = FontStyle.Bold, letterSpacing = 8 } };
                var subLbl = new Label(subtitle) { style = { fontSize = 14, color = new Color(0.8f, 0.6f, 0.2f), opacity = 0, marginTop = 10, translate = new Translate(0, 20, 0) } };

                btn.Add(titleLbl);
                btn.Add(subLbl);

                btn.RegisterCallback<MouseEnterEvent>(evt => {
                    container.style.backgroundColor = Lighten(baseColor, 0.15f);
                    titleLbl.style.color = Color.white;
                    subLbl.style.opacity = 1; subLbl.style.translate = new Translate(0, 0, 0);
                });
                btn.RegisterCallback<MouseLeaveEvent>(evt => {
                    container.style.backgroundColor = baseColor;
                    titleLbl.style.color = new Color(1, 1, 1, 0.5f);
                    subLbl.style.opacity = 0; subLbl.style.translate = new Translate(0, 20, 0);
                });

                container.Add(btn);
            }
            return container;
        }

        private Color Lighten(Color c, float amount) => new Color(Mathf.Clamp01(c.r + amount), Mathf.Clamp01(c.g + amount), Mathf.Clamp01(c.b + amount));

        public System.Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}
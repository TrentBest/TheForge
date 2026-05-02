using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Warlords
{
    public class Warlords_Gui_MainMenu : IGuiProvider
    {
        public string Title => "WARLORDS: MAIN MENU";

        private VisualElement _chroniclesContent;
        private VisualElement _gameModeOverlay;
        private VisualElement _centerAnchor;
        private GuiContext _lastCtx;
        private IGuiRouter _router;

        // 1. DEFAULT CONSTRUCTOR FOR EDITOR PREVIEWS
        public Warlords_Gui_MainMenu() { }

        // 2. INJECTED CONSTRUCTOR FOR PLAYABLE ALPHA ROUTING
        public Warlords_Gui_MainMenu(IGuiRouter router)
        {
            _router = router;
        }

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
                var grid = new GraphicalUserInterfaceBuilder("GridContainer")
                    .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                    .OnBuild(ve => ve.style.flexGrow = 1)
                    .Build();

                var topRow = new GraphicalUserInterfaceBuilder("TopRow")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                    .OnBuild(ve => ve.style.flexGrow = 1)
                    .Build();

                // 1. NEW GAME (Red/Crimson Theme)
                topRow.Add(CreateQuadrant("CONQUEST", "BEGIN A NEW CAMPAIGN", new Color(0.2f, 0.05f, 0.05f),
                    () => ShowGameModeOverlay(true), ctx));

                // 2. LOAD GAME (Green/Forest Theme)
                var chroniclesQuad = CreateQuadrant("ARCHIVES", "RESTORE SAVED GAME", new Color(0.05f, 0.15f, 0.05f), null, ctx);
                _chroniclesContent = chroniclesQuad;
                RefreshChroniclesState(ctx);
                topRow.Add(chroniclesQuad);

                grid.Add(topRow);

                var bottomRow = new GraphicalUserInterfaceBuilder("BottomRow")
                    .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                    .OnBuild(ve => ve.style.flexGrow = 1)
                    .Build();

                // 3. SETTINGS (Blue/Royal Theme)
                bottomRow.Add(CreateQuadrant("SETTINGS", "GAME CONFIGURATION", new Color(0.05f, 0.08f, 0.15f),
                    () => Debug.Log("Navigate to Settings Gui.."), ctx));

                // 4. QUIT (Purple/Dark Theme)
                //bottomRow.Add(CreateQuadrant("SURRENDER", "EXIT TO DESKTOP", new Color(0.15f, 0.05f, 0.15f),
                //    () => Application.Quit(), ctx));

                grid.Add(bottomRow);
                return grid;
            });

            // --- THE GAME MODE OVERLAY (Hidden by default) ---
            builder.AddChild(context =>
            {
                _gameModeOverlay = new GraphicalUserInterfaceBuilder("GameModeOverlay")
                    .WithBackgroundColor(new Color(0, 0, 0, 0.95f))
                    .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                    .OnBuild(ve => {
                        ve.style.position = Position.Absolute;
                        ve.style.left = 0; ve.style.top = 0; ve.style.right = 0; ve.style.bottom = 0;
                        ve.style.display = DisplayStyle.None; // HIDDEN INITIALLY
                    }).Build();

                var titleLbl = new ForgeLabelBuilder("CHOOSE BATTLE FORMAT")
                    .WithColor(new Color(0.8f, 0.6f, 0.2f))
                    .WithFontSize(28)
                    .WithFontStyle(FontStyle.Bold)
                    .CreateGui(ctx);
                titleLbl.style.marginBottom = 40;
                _gameModeOverlay.Add(titleLbl);

                // 1. Single Player -> Leads to the Warlords_Gui_GameSetup
                var soloBtn = new ForgeButtonBuilder("SOLO CAMPAIGN (Single Player)", () => {
                    ShowGameModeOverlay(false); // Reset overlay so it doesn't block on return
                    Debug.Log("Transition to Warlords_Gui_GameSetup");
                    if (_router != null) _router.NavigateTo("GameSetup");
                })
                .WithWidth(400).WithHeight(45)
                .WithBackgroundColor(new Color(0.2f, 0.05f, 0.05f))
                .WithTextColor(Color.white)
                .WithFontStyle(FontStyle.Bold).WithFontSize(16)
                .CreateGui(ctx);
                soloBtn.style.marginBottom = 15;
                _gameModeOverlay.Add(soloBtn);

                // 2. Hotseat Multiplayer
                var hotseatBtn = new ForgeButtonBuilder("LOCAL ALLIANCE (Hotseat Multiplayer)", () => Debug.Log("Transition to Hotseat Setup"))
                .WithWidth(400).WithHeight(45)
                .WithBackgroundColor(new Color(0.05f, 0.15f, 0.05f))
                .WithTextColor(Color.white)
                .WithFontStyle(FontStyle.Bold).WithFontSize(16)
                .CreateGui(ctx);
                hotseatBtn.style.marginBottom = 15;
                _gameModeOverlay.Add(hotseatBtn);

                // 3. Network Multiplayer (Placeholder)
                var netBtn = new ForgeButtonBuilder("GLOBAL DOMINATION (Network Play)", null)
                .WithWidth(400).WithHeight(45)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f))
                .WithTextColor(Color.gray)
                .WithFontStyle(FontStyle.Bold).WithFontSize(16)
                .CreateGui(ctx);
                netBtn.style.marginBottom = 15;
                netBtn.tooltip = "Authentication Required: Please sign in to the Singularity Workshop to access network features.";
                netBtn.SetEnabled(false); // Grays out the button and blocks clicks
                _gameModeOverlay.Add(netBtn);

                // 4. The Modem Nostalgia Button (Placeholder)
                var modemBtn = new ForgeButtonBuilder("DIRECT DIAL-UP (Modem/IPX)", null)
                .WithWidth(400).WithHeight(45)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f))
                .WithTextColor(Color.gray)
                .WithFontStyle(FontStyle.Bold).WithFontSize(16)
                .CreateGui(ctx);
                modemBtn.style.marginBottom = 30;
                modemBtn.tooltip = "Requires a 14.4k baud modem and ensuring nobody picks up the phone line in the house.";
                modemBtn.SetEnabled(false);
                _gameModeOverlay.Add(modemBtn);

                // Cancel
                var cancelBtn = new ForgeButtonBuilder("RETREAT", () => ShowGameModeOverlay(false))
                .WithWidth(400).WithHeight(30)
                .WithBackgroundColor(Color.clear)
                .WithTextColor(Color.gray)
                .WithFontStyle(FontStyle.Bold)
                .CreateGui(ctx);
                cancelBtn.style.marginTop = 10;
                _gameModeOverlay.Add(cancelBtn);

                return _gameModeOverlay;
            });

            // --- CENTER LOGO / SIGN IN ANCHOR ---
            builder.AddChild(context =>
            {
                _centerAnchor = new GraphicalUserInterfaceBuilder("CenterAnchor")
                    .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                    .OnBuild(ve => {
                        ve.style.position = Position.Absolute;
                        ve.style.left = Length.Percent(50);
                        ve.style.top = Length.Percent(50);
                        ve.style.width = 0; ve.style.height = 0;
                        ve.pickingMode = PickingMode.Ignore;
                    }).Build();

                // Acting as both the logo and the Sign In button
                var centerSignInBtn = new ForgeButtonBuilder("", () => {
                    Debug.Log("Sign In Flow Triggered");
                    if (_router != null) _router.NavigateTo("UnityServices");
                })
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f, 1f))
                .CreateGui(ctx);

                centerSignInBtn.tooltip = "Sign in to enable Network Play";
                centerSignInBtn.style.width = 140; centerSignInBtn.style.height = 140;
                centerSignInBtn.style.borderBottomLeftRadius = 70; centerSignInBtn.style.borderTopLeftRadius = 70;
                centerSignInBtn.style.borderBottomRightRadius = 70; centerSignInBtn.style.borderTopRightRadius = 70;
                centerSignInBtn.style.borderBottomWidth = 4; centerSignInBtn.style.borderBottomColor = new Color(0.8f, 0.6f, 0.2f);
                centerSignInBtn.style.borderTopWidth = 4; centerSignInBtn.style.borderTopColor = new Color(0.8f, 0.6f, 0.2f);
                centerSignInBtn.style.borderLeftWidth = 4; centerSignInBtn.style.borderLeftColor = new Color(0.8f, 0.6f, 0.2f);
                centerSignInBtn.style.borderRightWidth = 4; centerSignInBtn.style.borderRightColor = new Color(0.8f, 0.6f, 0.2f);
                centerSignInBtn.style.justifyContent = Justify.Center; centerSignInBtn.style.alignItems = Align.Center;
                centerSignInBtn.style.flexDirection = FlexDirection.Column;

                var logoLbl = new ForgeLabelBuilder("W").WithColor(new Color(0.8f, 0.6f, 0.2f)).WithFontSize(56).WithFontStyle(FontStyle.Bold).CreateGui(ctx);
                logoLbl.style.marginBottom = -10;

                var signLbl = new ForgeLabelBuilder("SIGN IN").WithColor(Color.gray).WithFontSize(12).WithFontStyle(FontStyle.Bold).CreateGui(ctx);

                centerSignInBtn.Add(logoLbl);
                centerSignInBtn.Add(signLbl);

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

        private void RefreshChroniclesState(GuiContext ctx)
        {
            _chroniclesContent.Clear();
            var saves = GetCachedSaves();

            if (saves.Count > 0)
            {
                var listContainer = new GraphicalUserInterfaceBuilder("ListContainer")
                    .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                    .OnBuild(ve => { ve.style.flexGrow = 1; ve.style.paddingTop = 20; ve.style.paddingRight = 20; ve.style.paddingBottom = 20; ve.style.paddingLeft = 20; })
                    .Build();

                var headerLbl = new ForgeLabelBuilder("RECENT ARCHIVES").WithColor(new Color(0.8f, 0.6f, 0.2f)).WithFontSize(18).CreateGui(ctx);
                headerLbl.style.alignSelf = Align.Center;
                headerLbl.style.marginBottom = 10;
                listContainer.Add(headerLbl);

                var scroll = new ForgeScrollViewBuilder().CreateGui(ctx);

                foreach (var save in saves)
                {
                    var row = new ForgeButtonBuilder("", () => LoadGame(save))
                        .WithBackgroundColor(new Color(0, 0, 0, 0.5f))
                        .CreateGui(ctx);

                    row.style.flexDirection = FlexDirection.Row;
                    row.style.justifyContent = Justify.SpaceBetween;
                    row.style.marginBottom = 2;
                    row.style.borderLeftWidth = 0; row.style.borderRightWidth = 0;
                    row.style.borderTopWidth = 0; row.style.borderBottomWidth = 0;
                    row.style.paddingTop = 10; row.style.paddingBottom = 10;
                    row.style.paddingLeft = 10; row.style.paddingRight = 10;

                    row.Add(new ForgeLabelBuilder(save.Name).WithColor(Color.white).WithFontStyle(FontStyle.Bold).CreateGui(ctx));
                    row.Add(new ForgeLabelBuilder(save.Date).WithColor(Color.gray).WithFontSize(12).CreateGui(ctx));
                    scroll.Add(row);
                }

                var loadDirBtn = new ForgeButtonBuilder("LOAD FROM DIRECTORY..", () => OpenFileBrowser())
                    .WithHeight(30).WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f)).WithTextColor(Color.white)
                    .CreateGui(ctx);
                loadDirBtn.style.marginTop = 15;

                scroll.Add(loadDirBtn);
                listContainer.Add(scroll);
                _chroniclesContent.Add(listContainer);
            }
            else
            {
                var btn = new ForgeButtonBuilder("", () => OpenFileBrowser())
                    .WithBackgroundColor(Color.clear)
                    .CreateGui(ctx);

                btn.style.flexGrow = 1;
                btn.style.borderLeftWidth = 0; btn.style.borderRightWidth = 0;
                btn.style.borderTopWidth = 0; btn.style.borderBottomWidth = 0;
                btn.style.justifyContent = Justify.Center; btn.style.alignItems = Align.Center;

                var titleLbl = new ForgeLabelBuilder("ARCHIVES").WithFontSize(42).WithColor(new Color(1, 1, 1, 0.5f)).WithFontStyle(FontStyle.Bold).CreateGui(ctx);
                titleLbl.style.letterSpacing = 8;
                btn.Add(titleLbl);

                var subLbl = new ForgeLabelBuilder("NO HEROIC DEEDS FOUND - LOAD FROM DISK").WithFontSize(14).WithColor(new Color(0.8f, 0.6f, 0.2f)).CreateGui(ctx);
                subLbl.style.marginTop = 10;
                btn.Add(subLbl);

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

        private VisualElement CreateQuadrant(string title, string subtitle, Color baseColor, System.Action onClick, GuiContext ctx)
        {
            var container = new GraphicalUserInterfaceBuilder($"Quadrant_{title}")
                .WithBackgroundColor(baseColor)
                .OnBuild(ve => {
                    ve.style.flexGrow = 1;
                    ve.style.width = Length.Percent(50);
                    ve.style.overflow = Overflow.Hidden;
                }).Build();

            if (onClick != null)
            {
                var btn = new ForgeButtonBuilder("", onClick)
                    .WithBackgroundColor(Color.clear)
                    .CreateGui(ctx);

                btn.style.flexGrow = 1;
                btn.style.borderLeftWidth = 0; btn.style.borderRightWidth = 0;
                btn.style.borderTopWidth = 0; btn.style.borderBottomWidth = 0;
                btn.style.justifyContent = Justify.Center; btn.style.alignItems = Align.Center;

                var titleLbl = new ForgeLabelBuilder(title)
                    .WithFontSize(42).WithColor(new Color(1, 1, 1, 0.5f)).WithFontStyle(FontStyle.Bold)
                    .CreateGui(ctx);
                titleLbl.style.letterSpacing = 8;

                var subLbl = new ForgeLabelBuilder(subtitle)
                    .WithFontSize(14).WithColor(new Color(0.8f, 0.6f, 0.2f))
                    .CreateGui(ctx);
                subLbl.style.opacity = 0;
                subLbl.style.marginTop = 10;
                subLbl.style.translate = new Translate(0, 20, 0);

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
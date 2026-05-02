using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Showcase.MicroPackages
{
    public enum HermitIntakeState
    {
        Greeting,
        AwaitingIntent, // Modify vs New
        AwaitingPackageName,
        ResolvingCollision,
        HandoffToWizard
    }

    public class ChatLogEntry
    {
        public string Speaker;
        public string Message;
        public Color TextColor;
    }

    /// <summary>
    /// The interactive conversational lobby where Hermit ascertains user intent 
    /// before launching the Micro Package Wizard.
    /// </summary>
    public class MicroPackages_Gui_HermitIntakeConsole : IGuiProvider
    {
        public string Title => "HERMIT INTAKE CONSOLE";

        // State Management
        private HermitIntakeState _currentState = HermitIntakeState.Greeting;
        private List<ChatLogEntry> _chatHistory = new List<ChatLogEntry>();
        private string _proposedPackageName = "";

        // UI Injection Points
        private VisualElement _consoleOutputArea;
        private VisualElement _consoleInputArea;
        private VisualElement _rightContextPane; // Shows package collisions or data

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("HermitIntakeRoot")
                .WithFlexGrow(1)
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f));

            // ==========================================
            // LEFT PANE: Hermit & Terminal
            // ==========================================
            var hermitPane = new GraphicalUserInterfaceBuilder("HermitInteractionPane")
                .WithWidth(350)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithBorderRightWidth(1).WithBorderRightColor(new Color(0.8f, 0.1f, 0.8f)) // Singularity Purple
                .WithFlexLayout(FlexDirection.Column);

            // 1. The LMPB Camera View (Upper Torso)
            var lmpbContainer = new GraphicalUserInterfaceBuilder("HermitLMPB")
                .WithHeight(250)
                .WithBackgroundColor(Color.black)
                .WithBorderBottomWidth(2).WithBorderBottomColor(Color.cyan)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .AddChild(new ForgeLabelBuilder("[LMPB: HERMIT UPPER TORSO CAM]")
                    .WithColor(Color.cyan).WithBold(true).WithFontSize(14));

            hermitPane.AddChild(lmpbContainer);

            // 2. The Dialogue Terminal
            var terminalContainer = new GraphicalUserInterfaceBuilder("TerminalOutput")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.02f))
                .WithPadding(10)
                .WithScrollable(true)
                .OnBuild(ve => {
                    _consoleOutputArea = ve;
                    BootSequence();
                });

            hermitPane.AddChild(terminalContainer);

            // 3. The Input Engine (Dynamic based on state)
            var inputContainer = new GraphicalUserInterfaceBuilder("TerminalInput")
                .WithMinHeight(60)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))
                .WithBorderTopWidth(1).WithBorderTopColor(Color.gray)
                .WithPadding(10)
                .OnBuild(ve => {
                    _consoleInputArea = ve;
                });

            hermitPane.AddChild(inputContainer);
            rootBuilder.AddChild(hermitPane);

            // ==========================================
            // RIGHT PANE: Contextual Data
            // ==========================================
            var contextPane = new GraphicalUserInterfaceBuilder("ContextPane")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.03f, 0.03f, 0.04f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .OnBuild(ve => _rightContextPane = ve);

            rootBuilder.AddChild(contextPane);

            return rootBuilder.Build();
        }

        // ==========================================
        // CONVERSATION LOGIC & ROUTING
        // ==========================================
        private void BootSequence()
        {
            HermitSpeak("System initialized. Welcome back to the Workshop.");
            HermitSpeak("Are you wanting to modify an existing package, or work on a new package?");

            _currentState = HermitIntakeState.AwaitingIntent;
            RefreshInputEngine();
        }

        private void UserSelectsNewPackage()
        {
            UserSpeak("I want to create a new package.");
            HermitSpeak("Oh exciting! This very moment when creativity becomes manifest is quite literally why I exist!");
            HermitSpeak("Do you have a name or working name in mind?");

            _currentState = HermitIntakeState.AwaitingPackageName;
            RefreshInputEngine();
        }

        private void UserProvidesName(string name)
        {
            _proposedPackageName = name;
            UserSpeak($"Let's call it '{name}'.");

            // Mocking the collision detection logic
            if (name.ToLower().Contains("silly package"))
            {
                HermitSpeak($"That's quite a silly idea! Unfortunately, someone else has already had the same or similar idea.");
                HermitSpeak("I can load their package on the right if you want to check it out.");
                HermitSpeak("Just because the name is conflicting, doesn't in fact mean the content is. If their package does something different than yours, you just need to derive a more specific title. For example, if they have 'Attack Package', you could use 'Attack Package with Charge Up Attack!'");

                ShowCollisionDataOnRightPane(name);
                _currentState = HermitIntakeState.ResolvingCollision;
            }
            else
            {
                HermitSpeak($"'{name}' is clear! Registering namespace and preparing the Forge...");
                _currentState = HermitIntakeState.HandoffToWizard;
            }

            RefreshInputEngine();
        }

        // ==========================================
        // UI RENDERING UPDATES
        // ==========================================
        private void HermitSpeak(string text)
        {
            _chatHistory.Add(new ChatLogEntry { Speaker = "HERMIT", Message = text, TextColor = Color.cyan });
            RenderChatLog();
        }

        private void UserSpeak(string text)
        {
            _chatHistory.Add(new ChatLogEntry { Speaker = "OPERATOR", Message = text, TextColor = Color.silver });
            RenderChatLog();
        }

        private void RenderChatLog()
        {
            if (_consoleOutputArea == null) return;
            _consoleOutputArea.Clear();

            foreach (var entry in _chatHistory)
            {
                var msgGui = new GraphicalUserInterfaceBuilder("LogEntry")
                    .WithMarginBottom(10)
                    .WithFlexLayout(FlexDirection.Column)
                    .AddChild(new ForgeLabelBuilder($"[{entry.Speaker}]")
                        .WithColor(entry.TextColor).WithBold(true).WithFontSize(10))
                    .AddChild(new ForgeLabelBuilder(entry.Message)
                        .WithColor(Color.white).WithWordWrap(true).WithMarginTop(2));

                _consoleOutputArea.Add(msgGui.Build());
            }
        }

        private void RefreshInputEngine()
        {
            if (_consoleInputArea == null) return;
            _consoleInputArea.Clear();

            var inputBuilder = new GraphicalUserInterfaceBuilder("CurrentInput");

            switch (_currentState)
            {
                case HermitIntakeState.AwaitingIntent:
                    inputBuilder.WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween)
                        .AddChild(new ForgeButtonBuilder("MODIFY EXISTING")
                            .WithFlexGrow(1).WithMarginRight(5)
                            .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f))
                            .OnClick(() => UserSpeak("I need to modify an existing package.")))
                        .AddChild(new ForgeButtonBuilder("CREATE NEW")
                            .WithFlexGrow(1).WithMarginLeft(5)
                            .WithBackgroundColor(new Color(0.8f, 0.1f, 0.8f)) // Purple emphasis
                            .OnClick(UserSelectsNewPackage));
                    break;

                case HermitIntakeState.AwaitingPackageName:
                    // In your ecosystem, if you have a ForgeTextFieldBuilder, you'd use it here.
                    // For the prototype, we simulate the text input.
                    inputBuilder.WithFlexLayout(FlexDirection.Row)
                        .AddChild(new ForgeButtonBuilder("Simulate Entry: 'Silly Package'")
                            .WithFlexGrow(1).WithBackgroundColor(new Color(0.2f, 0.4f, 0.2f))
                            .OnClick(() => UserProvidesName("Silly Package")));
                    break;

                case HermitIntakeState.ResolvingCollision:
                    inputBuilder.WithFlexLayout(FlexDirection.Column)
                        .AddChild(new ForgeButtonBuilder("RENAME MY PACKAGE")
                            .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f)).WithMarginBottom(5)
                            .OnClick(() => {
                                UserSpeak("Let's pick a different name.");
                                _currentState = HermitIntakeState.AwaitingPackageName;
                                RefreshInputEngine();
                            }))
                        .AddChild(new ForgeButtonBuilder("INSPECT COLLIDING PACKAGE")
                            .WithBackgroundColor(new Color(0.2f, 0.4f, 0.4f)).WithMarginBottom(5));
                    break;

                case HermitIntakeState.HandoffToWizard:
                    inputBuilder.AddChild(new ForgeButtonBuilder("▶ ENTER THE FORGE")
                        .WithHeight(40)
                        .WithBackgroundColor(new Color(0.8f, 0.1f, 0.8f))
                        .WithTextColor(Color.white).WithBold(true)
                        .OnClick(() => Debug.Log("Transitioning to MicroPackages_Gui_MicroPackageWizard...")));
                    break;
            }

            _consoleInputArea.Add(inputBuilder.Build());
        }

        private void ShowCollisionDataOnRightPane(string conflictingName)
        {
            if (_rightContextPane == null) return;
            _rightContextPane.Clear();

            var collisionUI = new GraphicalUserInterfaceBuilder("CollisionData")
                .WithPadding(20)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithBorderWidth(1).WithBorderAllColor(Color.red)
                .AddHeader("NAMESPACE COLLISION DETECTED", Color.red)
                .AddSeparator(Color.red, 1)
                .AddChild(new ForgeLabelBuilder($"Existing Package: {conflictingName}")
                    .WithColor(Color.white).WithBold(true).WithFontSize(18).WithMarginTop(10))
                .AddChild(new ForgeLabelBuilder("Author: Unknown Architect")
                    .WithColor(Color.gray).WithMarginTop(5))
                .AddChild(new ForgeLabelBuilder("Description: A collection of absurdly bouncy physics materials and erratic FSM logic.")
                    .WithColor(Color.silver).WithWordWrap(true).WithMarginTop(15))
                .AddChild(new ForgeButtonBuilder("LOAD PACKAGE INTO LMPB FOR REVIEW")
                    .WithMarginTop(20).WithBackgroundColor(new Color(0.3f, 0.1f, 0.1f)));

            _rightContextPane.Add(collisionUI.Build());
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}
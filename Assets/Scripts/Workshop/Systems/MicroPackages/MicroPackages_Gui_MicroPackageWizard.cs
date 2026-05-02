using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Showcase.MicroPackages
{
    public enum WizardStep
    {
        Welcome,
        Assembly,
        Persona,
        TheGauntlet,
        Deployment
    }

    /// <summary>
    /// A guided, step-by-step workflow for assembling, decorating, and brutally testing 
    /// a MicroPackage, guided by the persona 'Hermit'.
    /// </summary>
    public class MicroPackages_Gui_MicroPackageWizard : IGuiProvider
    {
        public string Title => "MICRO PACKAGE MANAGER";

        private WizardStep _currentStep = WizardStep.Welcome;
        private VisualElement _dynamicContentArea;
        private VisualElement _hermitDialogArea;

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("MicroPackageWizardRoot")
                .WithFlexGrow(1)
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Stretch)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f));

            // 1. LEFT PANE: Hermit's Guide & Progress
            var leftPane = new GraphicalUserInterfaceBuilder("GuidePane")
                .WithWidth(300)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithBorderRightWidth(1).WithBorderRightColor(new Color(0.8f, 0.1f, 0.8f)) // Singularity Purple
                .WithFlexLayout(FlexDirection.Column, Justify.SpaceBetween)
                .WithPadding(15);

            // Hermit Dialog Container (Updates dynamically)
            leftPane.AddChild(new GraphicalUserInterfaceBuilder("HermitContainer")
                .WithFlexGrow(1)
                .OnBuild(ve => {
                    _hermitDialogArea = ve;
                    RefreshHermitDialog();
                }));

            // Step Indicator (Static for now, could be dynamic)
            var stepIndicator = new GraphicalUserInterfaceBuilder("StepIndicator")
                .WithHeight(40)
                .WithBorderTopWidth(1).WithBorderTopColor(Color.gray)
                .WithPaddingTop(10)
                .AddChild(new ForgeLabelBuilder("SYSTEM PROGRESS").WithColor(Color.gray).WithFontSize(10).WithBold(true));

            leftPane.AddChild(stepIndicator);
            rootBuilder.AddChild(leftPane);

            // 2. RIGHT PANE: Dynamic Step Content
            var rightPane = new GraphicalUserInterfaceBuilder("ContentPane")
                .WithFlexGrow(1)
                .WithFlexLayout(FlexDirection.Column)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.03f));

            // Dynamic Injection Area
            rightPane.AddChild(new GraphicalUserInterfaceBuilder("DynamicInjection")
                .WithFlexGrow(1)
                .WithPadding(20)
                .OnBuild(ve => {
                    _dynamicContentArea = ve;
                    RefreshStepContent();
                }));

            // 3. BOTTOM BAR: Navigation Controls
            var navBar = new GraphicalUserInterfaceBuilder("WizardNavigation")
                .WithHeight(60)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.06f))
                .WithBorderTopWidth(1).WithBorderTopColor(Color.gray)
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithPaddingLeft(20).WithPaddingRight(20);

            navBar.AddChild(new ForgeButtonBuilder("◀ ABORT / BACK")
                .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f))
                .WithTextColor(Color.white)
                .OnClick(StepBack));

            navBar.AddChild(new ForgeButtonBuilder("ENGAGE / NEXT ▶")
                .WithBackgroundColor(new Color(0.8f, 0.1f, 0.8f))
                .WithTextColor(Color.white)
                .WithBold(true)
                .OnClick(StepForward));

            rightPane.AddChild(navBar);
            rootBuilder.AddChild(rightPane);

            return rootBuilder.Build();
        }

        // ==========================================
        // STATE MANAGEMENT
        // ==========================================
        private void StepForward()
        {
            if (_currentStep < WizardStep.Deployment)
            {
                _currentStep++;
                RefreshHermitDialog();
                RefreshStepContent();
            }
        }

        private void StepBack()
        {
            if (_currentStep > WizardStep.Welcome)
            {
                _currentStep--;
                RefreshHermitDialog();
                RefreshStepContent();
            }
        }

        // ==========================================
        // HERMIT DIALOGUE LOGIC
        // ==========================================
        private void RefreshHermitDialog()
        {
            if (_hermitDialogArea == null) return;
            _hermitDialogArea.Clear();

            string title = "HERMIT SAYS...";
            string dialog = "";

            switch (_currentStep)
            {
                case WizardStep.Welcome:
                    dialog = "Welcome to The Singularity Workshop's Micro Package Manager!\n\nA micro package is a singular 'Micro' focus bundle of content, not necessarily size. I'll walk you through forging this into a living entity.";
                    break;
                case WizardStep.Assembly:
                    dialog = "Let's gather the materials.\n\nIf you developed in a singular folder, point us there and we'll strip what you don't need. If your content is scattered, use the OS browser on the left and drag it into the hopper on the right.";
                    break;
                case WizardStep.Persona:
                    dialog = "Static packages are dead.\n\nLet's decorate and organize. We want our micro packages to 'live' and have personality by design. Hook up your cover image, ambient tracks, and interaction events.";
                    break;
                case WizardStep.TheGauntlet:
                    title = "WARNING: THE GAUNTLET";
                    dialog = "I cannot afford to donate compute power to ensure your package is 'valid' in my context.\n\nBefore submission, we stream this to your environment and run The Gauntlet. We will stagger brute-force every edge case and skip every n-values to bully this package until it breaks.";
                    break;
                case WizardStep.Deployment:
                    dialog = "It survived.\n\nWe've vaulted this locally for your own testing. You can use it in your environments right now. When you're ready, hit Launch to make it public. You're welcome, for the Earth, the moon, and the sky!";
                    break;
            }

            var builder = new GraphicalUserInterfaceBuilder("HermitMsg")
                .WithFlexLayout(FlexDirection.Column)
                .AddHeader(title, Color.cyan)
                .AddChild(new ForgeLabelBuilder(dialog)
                    .WithColor(Color.silver)
                    .WithFontSize(14)
                    .WithWordWrap(true)
                    .WithMarginTop(15));

            _hermitDialogArea.Add(builder.Build());
        }

        // ==========================================
        // DYNAMIC STEP CONTENT LOGIC
        // ==========================================
        private void RefreshStepContent()
        {
            if (_dynamicContentArea == null) return;
            _dynamicContentArea.Clear();

            IGuiProvider stepContent = null;

            switch (_currentStep)
            {
                case WizardStep.Welcome: stepContent = BuildWelcomeStep(); break;
                case WizardStep.Assembly: stepContent = BuildAssemblyStep(); break;
                case WizardStep.Persona: stepContent = BuildPersonaStep(); break;
                case WizardStep.TheGauntlet: stepContent = BuildGauntletStep(); break;
                case WizardStep.Deployment: stepContent = BuildDeploymentStep(); break;
            }

            if (stepContent != null)
            {
                var ui = stepContent.CreateGui(new GuiContext());
                ui.style.flexGrow = 1;
                _dynamicContentArea.Add(ui);
            }
        }

        #region --- Step Builders ---

        private IGuiProvider BuildWelcomeStep()
        {
            return new GraphicalUserInterfaceBuilder("WelcomeStep")
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .AddChild(new ForgeLabelBuilder("PACKAGE INITIALIZATION SEQUENCE")
                    .WithColor(Color.white).WithFontSize(24).WithBold(true))
                .AddChild(new ForgeLabelBuilder("AWAITING OPERATOR DIRECTIVE...")
                    .WithColor(Color.cyan).WithMarginTop(10));
        }

        private IGuiProvider BuildAssemblyStep()
        {
            var splitBuilder = new GraphicalUserInterfaceBuilder("AssemblySplit")
                .WithFlexLayout(FlexDirection.Row)
                .WithFlexGrow(1);

            // Mock OS Browser
            var osBrowser = new GraphicalUserInterfaceBuilder("OSBrowser")
                .WithFlexGrow(1).WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f))
                .WithMarginRight(10).WithBorderRadius(8).WithPadding(10)
                .AddHeader("OS SOURCE BROWSER", Color.gray)
                .AddChild(new ForgeLabelBuilder("📁 Assets/Physics/AtomBuilder").WithColor(Color.white))
                .AddChild(new ForgeLabelBuilder("📁 Assets/MicroPackages/HelloWorld").WithColor(Color.white));

            // Mock The Hopper (Drag & Drop Target)
            var theHopper = new GraphicalUserInterfaceBuilder("TheHopper")
                .WithFlexGrow(1).WithBackgroundColor(new Color(0.12f, 0.12f, 0.1f))
                .WithBorderWidth(2).WithBorderAllColor(Color.yellow) // Dashed line effect logic would go here
                .WithBorderRadius(8).WithPadding(10)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .AddChild(new ForgeLabelBuilder("📥 DRAG CONTENT HERE")
                    .WithColor(Color.yellow).WithBold(true).WithFontSize(18));

            splitBuilder.AddChild(osBrowser).AddChild(theHopper);
            return splitBuilder;
        }

        private IGuiProvider BuildPersonaStep()
        {
            return new GraphicalUserInterfaceBuilder("PersonaStep")
                .WithFlexLayout(FlexDirection.Column)
                .AddHeader("PACKAGE SENSORY BINDINGS", new Color(0.8f, 0.1f, 0.8f))
                .AddSeparator(new Color(0.8f, 0.1f, 0.8f), 1)

                // Cover Art
                .AddChild(new ForgeLabelBuilder("COVER VISUALIZATION").WithColor(Color.gray).WithMarginTop(15))
                .AddChild(new GraphicalUserInterfaceBuilder("ImageDropArea").WithHeight(100).WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f)).WithBorderWidth(1).WithBorderAllColor(Color.gray))

                // Audio
                .AddChild(new ForgeLabelBuilder("AMBIENT TRACK BINDING").WithColor(Color.gray).WithMarginTop(15))
                .AddChild(new ForgeButtonBuilder("🎧 SELECT BACKGROUND LOOP").WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f)))

                // Interactions
                .AddChild(new ForgeLabelBuilder("HOVER / CLICK EVENTS").WithColor(Color.gray).WithMarginTop(15))
                .AddChild(new ForgeButtonBuilder("⚡ BIND FSM LOGIC TRIGGER").WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f)));
        }

        private IGuiProvider BuildGauntletStep()
        {
            return new GraphicalUserInterfaceBuilder("GauntletStep")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .AddChild(new GraphicalUserInterfaceBuilder("WarningBox")
                    .WithBackgroundColor(new Color(0.2f, 0.0f, 0.0f))
                    .WithBorderWidth(1).WithBorderAllColor(Color.red)
                    .WithPadding(15).WithMarginBottom(20)
                    .AddChild(new ForgeLabelBuilder("INITIALIZING STAGGERED BRUTE FORCE...").WithColor(Color.red).WithBold(true)))

                .AddChild(new GraphicalUserInterfaceBuilder("LogTerminal")
                    .WithFlexGrow(1)
                    .WithBackgroundColor(Color.black)
                    .WithPadding(10)
                    .AddChild(new ForgeLabelBuilder("> Awaiting Gauntlet Execution...").WithColor(Color.green)))

                .AddChild(new ForgeButtonBuilder("🔥 INITIATE GAUNTLET 🔥")
                    .WithHeight(50).WithMarginTop(15)
                    .WithBackgroundColor(Color.red).WithTextColor(Color.white).WithBold(true)
                    .OnClick(() => Debug.Log("Gauntlet Protocol Triggered...")));
        }

        private IGuiProvider BuildDeploymentStep()
        {
            return new GraphicalUserInterfaceBuilder("DeploymentStep")
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .AddChild(new ForgeLabelBuilder("SURVIVAL CONFIRMED")
                    .WithColor(Color.green).WithFontSize(28).WithBold(true))
                .AddChild(new ForgeLabelBuilder("Version 1.0.0 has been securely vaulted to local storage.")
                    .WithColor(Color.silver).WithMarginTop(10).WithMarginBottom(30))

                .AddChild(new ForgeButtonBuilder("🚀 PUBLISH TO PUBLIC NETWORK")
                    .WithHeight(50).WithWidth(300)
                    .WithBackgroundColor(new Color(0.1f, 0.6f, 0.1f)).WithTextColor(Color.white).WithBold(true));
        }

        #endregion

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}
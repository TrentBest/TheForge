using System;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.FSM_API;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Assets.Scripts.CorsairsInSpace;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Corsair
{
    public class CorsairsInSpace_Gui_OpsManager : MonoBehaviour, IGuiProvider, IStateContext
    {
        public string Title => "Operations Manager";

        public bool IsValid { get; set; } = true;
        public string Name { get; set; } = "OpsManager_Context";

        private CorsairLairContext _lairContext;
        private Label _minionLabel;
        private Label _trainingLabel; // Replaces native ProgressBar

        public CorsairsInSpace_Gui_OpsManager(IGuiRouter r)
        {
        }

        public VisualElement CreateGui(GuiContext context)
        {
            _lairContext = FindAnyObjectByType<CorsairLairContext>();

            var rootBuilder = new GraphicalUserInterfaceBuilder("OpsManagerRoot")
                .WithAutoGrow(true)
                .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f, 0.98f))
                .WithPadding(15);

            // 1. Header
            rootBuilder.AddChild(new ForgeLabelBuilder("MINION OPERATIONS & RESEARCH")
                .WithFontSize(22).WithColor(Color.yellow).WithFontStyle(FontStyle.Bold));

            // 2. Minion Breakdown (Need slight wrapper to keep reference for ticking)
            rootBuilder.AddChild(ctx => {
                _minionLabel = new ForgeLabelBuilder("Workers: 0 | Scientists: 0 | Engineers: 0")
                    .WithColor(Color.white).WithMargin(10, 0, 0, 0).CreateGui(ctx) as Label;
                return _minionLabel;
            });

            // 3. Training Section
            var trainingContainer = new GraphicalUserInterfaceBuilder("TrainingContainer")
                .WithMarginTop(20).WithPadding(10).WithBackgroundColor(new Color(0, 0, 0, 0.3f));

            trainingContainer.AddChild(new ForgeLabelBuilder("ACTIVE TRAINING PROGRAM"));

            trainingContainer.AddChild(ctx => {
                _trainingLabel = new ForgeLabelBuilder("[ IDLE ] - 0%").WithColor(Color.cyan).CreateGui(ctx) as Label;
                return _trainingLabel;
            });

            var btnRow = new GraphicalUserInterfaceBuilder("BtnRow")
                .WithFlexLayout(FlexDirection.Row)
                .WithMarginTop(10);

            // Replaced native Buttons with ForgeButtonBuilder
            btnRow.AddChild(new ForgeButtonBuilder("Train Scientist", () => StartTraining("Scientist")));
            btnRow.AddChild(new ForgeButtonBuilder("Train Engineer", () => StartTraining("Engineer")));

            trainingContainer.AddChild(btnRow);
            rootBuilder.AddChild(trainingContainer);

            var root = rootBuilder.Build();

            root.schedule.Execute(() => {
                FSM_API.Interaction.Update("OpsManager_UI");
                RefreshUI();
            }).Every(33);

            InitializeFSM();
            return root;
        }

        private void InitializeFSM()
        {
            if (!FSM_API.Interaction.Exists("OpsManager_OS"))
            {
                FSM_API.Create.CreateFiniteStateMachine("OpsManager_OS", -1, "OpsManager_UI")
                    .State("Idle", null, null, null)
                    .State("Training", OnTrainingTick, null, null)
                    .Transition("Idle", "Training", ctx => _lairContext.ActiveTrainingProgram != "None")
                    .Transition("Training", "Idle", ctx => _lairContext.ActiveTrainingProgram == "None")
                    .BuildDefinition();
            }
            FSM_API.Create.CreateInstance("OpsManager_OS", this, "OpsManager_UI");
        }

        private void StartTraining(string program)
        {
            if (_lairContext != null && _lairContext.Workers > 0)
            {
                _lairContext.ActiveTrainingProgram = program;
                _lairContext.TrainingProgress = 0;
            }
        }

        private void OnTrainingTick(IStateContext ctx)
        {
            if (_lairContext == null) return;

            float speed = 0.5f + (_lairContext.Scientists * 0.05f);
            _lairContext.TrainingProgress += speed;

            if (_lairContext.TrainingProgress >= 100f)
            {
                if (_lairContext.ActiveTrainingProgram == "Scientist") _lairContext.Scientists++;
                else if (_lairContext.ActiveTrainingProgram == "Engineer") _lairContext.Engineers++;

                _lairContext.Workers--;
                _lairContext.ActiveTrainingProgram = "None";
            }
        }

        private void RefreshUI()
        {
            if (_lairContext == null || _minionLabel == null || _trainingLabel == null) return;

            _minionLabel.text = $"Workers: {_lairContext.Workers} | Scientists: {_lairContext.Scientists} | Engineers: {_lairContext.Engineers}";

            if (_lairContext.ActiveTrainingProgram == "None")
                _trainingLabel.text = "[ IDLE ]";
            else
                _trainingLabel.text = $"Training {_lairContext.ActiveTrainingProgram}... {_lairContext.TrainingProgress:F0}%";
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}
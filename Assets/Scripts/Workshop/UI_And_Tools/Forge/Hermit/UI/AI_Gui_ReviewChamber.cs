using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Core.Diagnostics;
using Workshop.UI_And_Tools.Forge.Builders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Core;
using Workshop.UI_And_Tools.Forge.Hermit.Bridge;
using Workshop.UI_And_Tools.Forge.Hermit.Core;
using Workshop.UI_And_Tools.Forge.Hermit.Directives;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.UI_And_Tools.Forge.Hermit.UI
{
    public class AI_Gui_ReviewChamber : MonoBehaviour, IForgeBuilder
    {
        public string ToolName => "Hermit Diagnostic Suite";
        public string Title => "HI";
        public string Category => "Hermit";

        public Type GetProductType() => typeof(object);
        public IGuiProvider GetGuiProvider() => new AI_ReviewChamberGuiProvider();

        private void OnEnable() => BuilderRegistry.Register(this);

        public object Build() => GetGuiProvider();
    }

    [Serializable]
    public class ProposedEdit
    {
        public string EditId;
        public string TargetComponent;
        public string OldStateJson;
        public string NewStateJson;
        public GameObject SimulationPrefab;
    }

    [Serializable]
    public class HermitDiagnosticPayload
    {
        public string Intent;
        public string TargetEditId;
        public string Feedback;
        public string FinalStateJson;
    }

    public class AI_ReviewChamberGuiProvider : IGuiProvider
    {
        public string Title => "Review Chamber";

        private ProposedEdit _activeEdit;
        private string _scopedFeedback = "";
        private string _currentInput = "";
        private VisualElement _rootElement;

        public AI_ReviewChamberGuiProvider()
        {
            _activeEdit = new ProposedEdit
            {
                EditId = "DELTA-A77-9X",
                TargetComponent = "DronePathfinding_FSM",
                OldStateJson = "{\n  \"Speed\": 5.0,\n  \"TurnRadius\": 1.2,\n  \"State\": \"Seek\"\n}",
                NewStateJson = "{\n  \"Speed\": 4.5,\n  \"TurnRadius\": 2.8,\n  \"State\": \"Seek_WideAngle\"\n}",
                SimulationPrefab = GameObject.CreatePrimitive(PrimitiveType.Capsule)
            };
            _activeEdit.SimulationPrefab.hideFlags = HideFlags.HideAndDontSave;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder("DiagnosticRoot")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .WithPadding(10)

                .AddChild(new GraphicalUserInterfaceBuilder("Header")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithMarginBottom(10)
                    .AddHeader($"DIAGNOSTIC REVIEW: {_activeEdit.EditId}", Color.yellow)
                    .AddChild(new ForgeLabelBuilder($"TARGET: {_activeEdit.TargetComponent}").WithColor(Color.cyan))
                )

                .AddChild(new ForgeSplitPanelBuilder(1, 2)
                    .WithSector(0, 0, new GraphicalUserInterfaceBuilder("OldStatePanel")
                        .WithBackgroundColor(new Color(0.15f, 0.05f, 0.05f))
                        .WithPadding(5)
                        .AddHeader("CURRENT STATE", new Color(1f, 0.4f, 0.4f))
                        .AddChild(c => {
                            var sv = new ForgeScrollViewBuilder().WithMode(ScrollViewMode.VerticalAndHorizontal).CreateGui(c) as ScrollView;
                            sv.Add(new ForgeLabelBuilder(_activeEdit.OldStateJson).WithColor(Color.white).CreateGui(c));
                            return sv;
                        })
                    )
                    .WithSector(0, 1, new GraphicalUserInterfaceBuilder("NewStatePanel")
                        .WithBackgroundColor(new Color(0.05f, 0.15f, 0.05f))
                        .WithPadding(5)
                        .AddHeader("PROPOSED DELTA", new Color(0.4f, 1f, 0.4f))
                        .AddChild(c => {
                            var sv = new ForgeScrollViewBuilder().WithMode(ScrollViewMode.VerticalAndHorizontal).CreateGui(c) as ScrollView;
                            sv.Add(new ForgeLabelBuilder(_activeEdit.NewStateJson).WithColor(Color.white).CreateGui(c));
                            return sv;
                        })
                    )
                )

                .AddChild(new GraphicalUserInterfaceBuilder("SandboxArea")
                    .WithHeight(300).WithMarginTop(10).WithMarginBottom(10)
                    .WithBackgroundColor(Color.black).WithBorderRadius(5).WithBorderWidth(1).WithBorderColor(Color.gray)
                    .AddHeader("NESTED PREVIEW PLAYER", Color.white)
                    .AddChild(c => {
                        var lmpb = new LiveModelPreviewBuilder(_activeEdit.SimulationPrefab)
                            .WithOriginalModel(false)
                            .WithAutoRotate(true, 20f);
                        var preview = lmpb.CreateGui(c);
                        preview.style.flexGrow = 1;
                        return preview;
                    })
                )

                .AddChild(new GraphicalUserInterfaceBuilder("ActionControls")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.FlexEnd)
                    .AddChild(new GraphicalUserInterfaceBuilder("FeedbackInput")
                        .WithFlexGrow(1).WithMarginRight(15)
                        .AddChild(new ForgeLabelBuilder("SCOPED DIAGNOSTIC FEEDBACK:").WithColor(Color.gray).WithFontSize(10))
                        .AddChild(new ForgeTextFieldBuilder()
                            .WithValue(_currentInput)
                            .AsMultiline(true)
                            .OnChanged(val => _scopedFeedback = val)
                            .OnSubmit(SendScopedFeedback)
                        )
                    )
                    .AddChild(new GraphicalUserInterfaceBuilder("VerdictButtons")
                        .WithFlexLayout(FlexDirection.Row)
                        .AddButton("❌ REJECT DELTA", TriggerRollback)
                        .AddChild(new GraphicalUserInterfaceBuilder("Spacer").WithWidth(10))
                        .AddButton("✅ APPROVE & MERGE", TriggerCommit)
                    )
                );

            _rootElement = rootBuilder.Build();
            return _rootElement;
        }

        private void SendScopedFeedback()
        {
            if (string.IsNullOrWhiteSpace(_scopedFeedback)) return;

            ForgeLogger.Log($"[Diagnostic] Feedback sent for {_activeEdit.EditId}: {_scopedFeedback}");

            var payload = new HermitDiagnosticPayload
            {
                Intent = "DiagnosticFeedback",
                TargetEditId = _activeEdit.EditId,
                Feedback = _scopedFeedback
            };

            LocalHostClient.SendPrompt(JsonUtility.ToJson(payload));
            _scopedFeedback = "";
        }

        private void TriggerRollback()
        {
            ForgeLogger.Log($"[Diagnostic] Edit {_activeEdit.EditId} REJECTED. Rolling back memory pool.");

            var payload = new HermitDiagnosticPayload
            {
                Intent = "DiagnosticRollback",
                TargetEditId = _activeEdit.EditId
            };
            LocalHostClient.SendPrompt(JsonUtility.ToJson(payload));

            CloseChamber("❌ DELTA REJECTED. Awaiting next manifestation...");
        }

        private void TriggerCommit()
        {
            ForgeLogger.Log($"[Diagnostic] Edit {_activeEdit.EditId} APPROVED. Committing to FSM_Memory.");

            var payload = new HermitDiagnosticPayload
            {
                Intent = "DiagnosticCommit",
                TargetEditId = _activeEdit.EditId,
                FinalStateJson = _activeEdit.NewStateJson
            };
            LocalHostClient.SendPrompt(JsonUtility.ToJson(payload));

            CloseChamber("✅ DELTA APPROVED & MERGED.");
        }

        private void CloseChamber(string finalMessage)
        {
            if (_rootElement == null) return;

            _rootElement.Clear();
            _rootElement.style.justifyContent = Justify.Center;
            _rootElement.style.alignItems = Align.Center;

            _rootElement.Add(new ForgeLabelBuilder(finalMessage)
                .WithColor(finalMessage.Contains("✅") ? Color.green : Color.red)
                .WithFontSize(20)
                .WithFontStyle(FontStyle.Bold)
                .CreateGui(new GuiContext()));
        }

        public void ApproveAction(HermitDirectivePayload payload)
        {
            payload.Status = ProposalStatus.Approved;

            // FIX 1: O(1) Zero-allocation local bus
            SingularityDataBus.Instance.SendLocal("Cortex_Action_Approved", payload);

            if (payload.Intent == "ProposeScript")
            {
                // FIX 2: Unity 6+ fast hierarchy search
                HermitChassis hermit = UnityEngine.Object.FindFirstObjectByType<HermitChassis>();

                if (hermit != null)
                {
                    HermitChassis scribeDrone = hermit.SpawnMinion();
                    IHermitDirective scribeDirective = new Directive_ScribeScript();
                    hermit.StartCoroutine(scribeDirective.Execute(payload, scribeDrone.AgentId));
                }
                else
                {
                    ForgeLogger.LogWarning("[Review Chamber] Could not locate 'HermitChassis' in the scene to dispatch minion.");
                }
            }

            ForgeLogger.Log($"Action '{payload.Intent}' Approved. Minion Dispatched.")
                       .WithHeader("DataBus")
                       .SendToUnity();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

        public void FromUIDocument(string assetPath) { }
        public void ToUIDocument(string assetPath) { }
    }
}
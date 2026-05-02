using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.Systems.MicroPackages;

namespace Workshop.UI_And_Tools.Forge.Tools
{
    public class MicroPackages_ConvergenceDashboard : IGuiProvider
    {
        public string Title => "Ecosystem Convergence Matrix";
        public IGuiProvider CurrentGuiProvider { get; internal set; }

        private ForgeArbitrator _activeArbitrator;

        public MicroPackages_ConvergenceDashboard(ForgeArbitrator arbitrator)
        {
            _activeArbitrator = arbitrator;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("ConvergenceRoot")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.12f));

            // Header
            root.AddChild(new ForgeContainerBuilder("Header")
                .WithHeight(60).WithBackgroundColor(new Color(0.05f, 0.05f, 0.08f))
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                .WithPadding(10)
                .AddChild(new ForgeLabelBuilder("ARBITRATION MATRIX").WithFontSize(24).WithColor(Color.white).WithFontStyle(FontStyle.Bold))
                .AddChild(new ForgeLabelBuilder($"Status: HOMEOSTASIS REACHED").WithColor(Color.green)));

            var splitPanel = new ForgeSplitPanelBuilder("MainSplit");

            // --- LEFT PANEL: Installed Packages ---
            var packageList = new ForgeScrollViewBuilder("PackageList").WithPadding(10);
            packageList.AddChild(new ForgeLabelBuilder("Active Ecosystem").WithFontSize(18).WithColor(Color.gray).WithMarginBottom(10));

            foreach (var pkgId in _activeArbitrator.InstalledPackages)
            {
                packageList.AddChild(new ForgeContainerBuilder($"pkg_{pkgId}")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                    .WithPadding(8).WithMarginBottom(5).WithBorderRadius(4)
                    .AddChild(new ForgeLabelBuilder(pkgId).WithColor(Color.white))
                    .AddChild(new ForgeButtonBuilder("Uninstall")
                        .WithBackgroundColor(new Color(0.6f, 0.2f, 0.2f)).WithColor(Color.white)
                        .OnClick(() => TriggerUninstallation(pkgId))));
            }
            splitPanel.WithLeftPane(packageList);

            // --- RIGHT PANEL: Arbitration Ledger ---
            var ledgerScroll = new ForgeScrollViewBuilder("LedgerScroll").WithPadding(10);
            ledgerScroll.AddChild(new ForgeLabelBuilder("Convergence Ledger").WithFontSize(18).WithColor(Color.gray).WithMarginBottom(10));

            // Mocking the passes based on our previous Sample scenario
            int passNumber = 1;
            ledgerScroll.AddChild(BuildPassBlock(passNumber++, new List<ArbitrationRecord>
            {
                new ArbitrationRecord("Workshop.Sample.Weapons", "Workshop.Sample.Weapons", "Additive", "Singing_Sword_Entity")
            }));

            ledgerScroll.AddChild(BuildPassBlock(passNumber++, new List<ArbitrationRecord>
            {
                new ArbitrationRecord("Workshop.Sample.Magic", "Workshop.Sample.Weapons", "Modification", "Modify_Broadsword_To_Flaming")
            }));

            splitPanel.WithRightPane(ledgerScroll);
            root.AddChild(splitPanel);

            return root.Build();
        }

        private ForgeContainerBuilder BuildPassBlock(int passNum, List<ArbitrationRecord> arbitrations)
        {
            var passContainer = new ForgeContainerBuilder($"Pass_{passNum}").WithMarginBottom(15);
            passContainer.AddChild(new ForgeLabelBuilder($"--- Convergence Pass {passNum} ---").WithColor(Color.yellow).WithMarginBottom(5));

            foreach (var arb in arbitrations)
            {
                passContainer.AddChild(new ForgeContainerBuilder("ArbRecord")
                    .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Center)
                    .WithBackgroundColor(new Color(0.12f, 0.12f, 0.15f)).WithPadding(8).WithMarginBottom(2)
                    .AddChild(new ForgeLabelBuilder($"[{arb.Requester}] targeted [{arb.Target}] -> {arb.Type}: {arb.Payload}").WithColor(Color.white))
                    .AddChild(new ForgeButtonBuilder("Veto")
                        .WithBackgroundColor(Color.gray).WithColor(Color.black)
                        .OnClick(() => VetoArbitration(arb))));
            }
            return passContainer;
        }

        private void VetoArbitration(ArbitrationRecord record)
        {
            Debug.Log($"<b>[Creator Veto]</b> Rejecting {record.Payload}. Dispatching Mini-Hermit to shred the asset.");
            // 1. Add to Arbitrator Blocklist
            // 2. Dispatch Hermit Directive_Trash
            // 3. Trigger Re-Arbitration
        }

        private void TriggerUninstallation(string packageId)
        {
            Debug.Log($"<b>[Creator]</b> Uninstalling {packageId}. Returning to homeostasis.");
            _activeArbitrator.UninstallPackage(packageId);
        }

        // Standard interface compliance
        public System.Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }

        // Mock struct for the UI
        private struct ArbitrationRecord
        {
            public string Requester, Target, Type, Payload;
            public ArbitrationRecord(string r, string t, string ty, string p) { Requester = r; Target = t; Type = ty; Payload = p; }
        }
    }
}
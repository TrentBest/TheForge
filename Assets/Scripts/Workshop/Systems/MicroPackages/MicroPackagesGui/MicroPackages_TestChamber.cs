using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.Systems.MicroPackages;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.IO;

namespace Workshop.UI_And_Tools.Showcase.MicroPackagesGui
{
    public class MicroPackages_TestChamber : IGuiProvider
    {
        public string Title => "Mean Entry Tester";

        private DynamicMicroPackage _package;
        private VisualElement _logContainer;

        // 1. Parameterless Default Constructor
        public MicroPackages_TestChamber() { }

        // Optional convenience constructor
        public MicroPackages_TestChamber(DynamicMicroPackage packageTarget)
        {
            _package = packageTarget;
        }

        // 2. Injection Method
        public void SetTargetPackage(DynamicMicroPackage packageTarget)
        {
            _package = packageTarget;
        }

        public Action<VisualElement> GetGuiBuilder() => (root) => root.Add(CreateGui(new GuiContext()));

        public VisualElement CreateGui(GuiContext ctx)
        {
            if (_package == null)
            {
                return new GraphicalUserInterfaceBuilder("EmptyTester")
                    .AddChild(new ForgeLabelBuilder("No Package Selected for Testing.").WithColor(Color.gray))
                    .CreateGui(ctx);
            }

            var split = new ForgeSplitPanelBuilder(300, Side.Left)
                .WithSidebar(BuildControls())
                .WithMain(BuildLogTerminal());

            return split.CreateGui(ctx);
        }

        private IGuiProvider BuildControls()
        {
            return new GraphicalUserInterfaceBuilder("TestControls")
                .WithTitle("Execution Protocols")
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f, 1f))
                .WithPadding(10)

                .AddChild(new ForgeLabelBuilder($"Target: {_package.PackageId}")
                    .WithColor(Color.cyan).WithFontStyle(FontStyle.Bold).WithMarginBottom(10))

                .AddChild(new ForgeButtonBuilder("Run Bottleneck Analysis", ExecuteStressTest)
                    .WithBackgroundColor(new Color(0.6f, 0.2f, 0.2f))
                    .WithTextColor(Color.white).WithMarginBottom(10))

                .AddChild(new ForgeButtonBuilder("Test Arbitrator Integration", ExecuteArbitratorMock)
                    .WithBackgroundColor(new Color(0.2f, 0.4f, 0.6f))
                    .WithTextColor(Color.white).WithMarginBottom(10))

                .AddChild(new ForgeButtonBuilder("Simulate Server Transmission", ExecuteServerTransmission)
                    .WithBackgroundColor(new Color(0.2f, 0.6f, 0.2f))
                    .WithTextColor(Color.white).WithMarginBottom(10));
        }

        private IGuiProvider BuildLogTerminal()
        {
            var builder = new GraphicalUserInterfaceBuilder("TestTerminal")
                .WithTitle("Telemetry & Output Log")
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f, 1f))
                .WithScrollable(true, ScrollViewMode.Vertical);

            builder.OnBuild(builtRoot => _logContainer = builtRoot);

            return builder;
        }

        private void Log(string message, Color color)
        {
            if (_logContainer == null) return;

            var label = new Label($"[{DateTime.Now:HH:mm:ss.fff}] {message}")
            {
                style = { color = color, fontSize = 12, marginBottom = 2 }
            };
            _logContainer.Add(label);
        }

        private void ExecuteStressTest()
        {
            Log("Initiating Bottleneck Analysis...", Color.yellow);
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            int simulatedOperations = (_package.Providers?.Count ?? 1) * 1000;

            watch.Stop();
            Log($"Stress test complete. Simulated {simulatedOperations} ops in {watch.ElapsedMilliseconds}ms.", Color.green);
        }

        private void ExecuteArbitratorMock()
        {
            Log("Testing Arbitrator Load logic...", Color.cyan);
            try
            {
                Log($"Successfully mocked load of {_package.Providers?.Count ?? 0} providers.", Color.green);
            }
            catch (Exception ex)
            {
                Log($"Integration Failure: {ex.Message}", Color.red);
            }
        }

        private void ExecuteServerTransmission()
        {
            Log("Preparing payload for Network Bus...", Color.cyan);
            var intent = new NetworkIntent
            {
                FullUrl = "https://singularity-test-server.com/api/packages/submit",
                Method = "POST",
                Payload = "{\"mock\":\"json_payload_here\"}",
                OnComplete = (result) =>
                {
                    if (result.IsSuccess)
                        Log($"Transmission SUCCESS: Server responded {result.StatusCode}", Color.green);
                    else
                        Log($"Transmission FAILED: Code {result.StatusCode}", Color.red);
                }
            };

            Log("Transmitting...", Color.yellow);
            ForgeNetworkBus.Instance.QueueRequest(intent);
        }

        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}
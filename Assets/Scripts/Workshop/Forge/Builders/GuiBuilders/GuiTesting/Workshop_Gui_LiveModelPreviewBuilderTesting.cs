using System;
using UnityEngine;
using UnityEngine.UIElements;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Workshop.Forge.Builders.GuiBuilders.GuiTesting
{
    //This is redundant trying to figure out how to get Hermit to show
    class Workshop_Gui_LiveModelPreviewBuilderTesting : IGuiProvider
    {
        public string Title => "I'm Alive!";

        // We move instantiation into CreateGui so we can manage the clone's lifecycle safely
        public VisualElement CreateGui(GuiContext ctx)
        {
            Debug.Log("[Workshop_Gui_LiveModelPreviewBuilderTesting] 1. CreateGui invoked.");

            // 1. SAFE CLONING: Never pass the live scene object into the isolated preview!
            GameObject hermitSource = GameObject.Find("Hermit") ?? GameObject.Find("PlayerArmature");
            if (hermitSource == null)
            {
                Debug.LogError("[Workshop_Gui_LiveModelPreviewBuilderTesting] FAIL: Could not find 'Hermit' in the scene.");
                return new Label("TARGET NOT FOUND") { style = { color = Color.red, fontSize = 20 } };
            }

            Debug.Log($"[Workshop_Gui_LiveModelPreviewBuilderTesting] 2. Cloning {hermitSource.name}...");
            GameObject hermitClone = UnityEngine.Object.Instantiate(hermitSource);
            hermitClone.name = "[PREVIEW] Hermit";

            // Strip physics so gravity doesn't pull him away from the preview camera at Y:-10000
            foreach (var rb in hermitClone.GetComponentsInChildren<Rigidbody>())
            {
                UnityEngine.Object.DestroyImmediate(rb);
            }

            // 2. SETUP THE BUILDER
            var previewBuilder = new LiveModelPreviewBuilder(hermitClone)
                .WithZoom(2.5f)
                .WithAutoOrbit(Vector3.up, 20f)
                .WithMouseControl(true);

            // 3. THE FLEX FIX
            Debug.Log("[Workshop_Gui_LiveModelPreviewBuilderTesting] 3. Applying Strict Flex Wrapper...");
            var wrapper = new GraphicalUserInterfaceBuilder("PreviewWrapper")
                .WithPercentSize(100, 100) // Forces Width 100%, Height 100%
                .WithFlexGrow(1)           // Forces it to fill available space
                .WithBackgroundColor(new Color(0.3f, 0, 0)) // CANARY RED: If you see red, the image failed to fill the box!
                .AddChild(previewBuilder)
                .OnBuild(ve => {
                    // Critical: Destroy the clone when this UI tab is closed
                    ve.RegisterCallback<DetachFromPanelEvent>(e => {
                        Debug.Log("[Workshop_Gui_LiveModelPreviewBuilderTesting] Detached. Destroying Hermit clone.");
                        if (hermitClone != null) UnityEngine.Object.DestroyImmediate(hermitClone);
                    });
                })
                .Build();

            Debug.Log("[Workshop_Gui_LiveModelPreviewBuilderTesting] 4. Success! Yielding UI to Forge.");
            return wrapper;
        }

        public void FromUIDocument(string assetPath) { }
        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
    }
}
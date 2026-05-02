using System;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Core;
using Workshop.UI_And_Tools.Forge.IO;
using Workshop.UI_And_Tools.Forge.Hermit.Core;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ForgeHermitChatPortalBuilder
    {
        private LiveModelPreviewBuilder _lmpb;
        private string _portalName;

        /// <summary>
        /// [FIXED]: New constructor for fluent initialization via HermitSettings.
        /// </summary>
        public ForgeHermitChatPortalBuilder(HermitSettings settings)
        {
            _portalName = "Agent_Primary";

            // Resolve the active chassis in the scene, or fallback to a dummy for the preview
            var activeHermit = GameObject.FindObjectOfType<HermitChassis>();
            GameObject displayObj = activeHermit != null ? activeHermit.gameObject : new GameObject("Preview_Hermit");

            _lmpb = new LiveModelPreviewBuilder(displayObj)
                .WithBackgroundColor(Color.clear)
                .WithZoom(0.6f)
                .WithPitch(10f)
                .WithControls(false);
        }

        public ForgeHermitChatPortalBuilder(string portalName, GameObject hermitChassis)
        {
            _portalName = portalName;
            _lmpb = new LiveModelPreviewBuilder(hermitChassis)
                .WithBackgroundColor(Color.clear)
                .WithZoom(0.6f)
                .WithPitch(10f)
                .WithControls(false);
        }

        public VisualElement Build(GuiContext ctx)
        {
            var root = new ForgeContainerBuilder($"HermitPortal_{_portalName}")
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.05f, 0.8f))
                .WithBorderRadius(12)
                .WithBorderWidth(2).WithBorderColor(new Color(0.2f, 0.8f, 1.0f, 0.5f))
                .OnBuild(ve =>
                {
                    ve.style.position = Position.Absolute;
                    ve.style.top = 20;
                    ve.style.right = 20;
                    ve.style.width = 250;
                    ve.style.height = 250;
                    ve.style.overflow = Overflow.Hidden;

                    ve.style.transitionProperty = new StyleList<StylePropertyName>(new System.Collections.Generic.List<StylePropertyName> { new StylePropertyName("width"), new StylePropertyName("height") });
                    ve.style.transitionDuration = new StyleList<TimeValue>(new System.Collections.Generic.List<TimeValue> { new TimeValue(0.3f, TimeUnit.Second) });
                })
                .Build();

            var modelView = _lmpb.CreateGui(ctx);
            modelView.style.position = Position.Absolute;
            modelView.style.top = modelView.style.bottom = modelView.style.left = modelView.style.right = 0;
            root.Add(modelView);

            var chatLog = new ScrollView { mode = ScrollViewMode.Vertical };
            chatLog.style.position = Position.Absolute;
            chatLog.style.top = 10; chatLog.style.bottom = 40; chatLog.style.left = 10; chatLog.style.right = 10;
            chatLog.style.backgroundColor = new Color(0, 0, 0, 0.4f);

            var logText = new Label("CORTEX LINK ESTABLISHED...") { style = { color = Color.cyan, fontSize = 11 } };
            chatLog.Add(logText);
            root.Add(chatLog);

            var inputField = new TextField { multiline = true };
            inputField.style.position = Position.Absolute;
            inputField.style.bottom = 0; inputField.style.left = inputField.style.right = 0;
            inputField.style.height = 35;
            inputField.style.backgroundColor = new Color(0.1f, 0.1f, 0.12f, 0.9f);

            inputField.RegisterCallback<FocusInEvent>(evt => { root.style.width = 450; root.style.height = 450; inputField.style.height = 120; });
            inputField.RegisterCallback<FocusOutEvent>(evt => { root.style.width = 250; root.style.height = 250; inputField.style.height = 35; });

            root.Add(inputField);
            return root;
        }
    }
}
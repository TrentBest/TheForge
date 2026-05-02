using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.MastersOfOrionII
{
    public class MastersOfOrionII_Gui_ScenarioSetup : IGuiProvider
    {
        public string Title => "SCENARIO SETUP";

        private GuiContext _lastCtx;

        // --- ROUTING ACTIONS (Public for Reflection/Injection) ---
        public Action OnContinueClicked { get; set; }
        public Action OnBackClicked { get; set; }

        // --- CONFIGURATION STATE ---
        private string _galaxySize = "Massive (1,024 Systems)";
        private string _opponents = "7 AI Empires";
        private string _techLevel = "Pre-Warp";

        // Primary Default Constructor - Required for Reflection
        public MastersOfOrionII_Gui_ScenarioSetup() { }

        // Convenience Constructor for Manual Routing Config
        public MastersOfOrionII_Gui_ScenarioSetup(Action onContinueClicked, Action onBackClicked)
        {
            OnContinueClicked = onContinueClicked;
            OnBackClicked = onBackClicked;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            var root = new GraphicalUserInterfaceBuilder("ScenarioSetupRoot")
                .WithPercentSize(100, 100)
                .WithBackgroundColor(new Color(0.04f, 0.04f, 0.06f))
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Center)
                .WithPadding(40)
                .Build();

            // Center Panel
            var panel = new GraphicalUserInterfaceBuilder("ConfigPanel")
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f, 0.9f))
                .WithPadding(40)
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .Build();

            panel.style.width = 500;
            panel.style.borderTopWidth = 3;
            panel.style.borderTopColor = Color.cyan;
            panel.style.borderTopLeftRadius = 10; panel.style.borderTopRightRadius = 10;
            panel.style.borderBottomLeftRadius = 10; panel.style.borderBottomRightRadius = 10;

            panel.Add(new Label("COSMIC SEED CONFIGURATION")
            {
                style = { color = Color.white, fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 30, alignSelf = Align.Center }
            });

            // Master of Orion II style Configuration Options
            var sizes = new List<string> { "Small (64 Systems)", "Medium (256 Systems)", "Large (512 Systems)", "Massive (1,024 Systems)" };
            var opps = new List<string> { "1 AI Empire", "3 AI Empires", "5 AI Empires", "7 AI Empires" };
            var techs = new List<string> { "Pre-Warp", "Average", "Advanced" };

            // Data Bindings using GUI Builder
            var dropdownContainer = new GraphicalUserInterfaceBuilder("Dropdowns")
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch)
                .AddDropdownData("GALAXY SIZE", sizes, sizes.IndexOf(_galaxySize), v => _galaxySize = v)
                .AddDropdownData("OPPONENTS", opps, opps.IndexOf(_opponents), v => _opponents = v)
                .AddDropdownData("STARTING TECH", techs, techs.IndexOf(_techLevel), v => _techLevel = v)
                .Build();

            dropdownContainer.style.marginBottom = 30;
            panel.Add(dropdownContainer);

            // Primary Forward Action
            var startBtn = new Button(() => OnContinueClicked?.Invoke())
            {
                text = "CONTINUE TO GENETIC SEQUENCER",
                style = {
                    backgroundColor = new Color(0, 0.5f, 0.2f), // MoO2 Green
                    color = Color.white,
                    height = 50,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 14,
                    marginBottom = 15
                }
            };
            panel.Add(startBtn);

            // Secondary Backward Action
            var backBtn = new Button(() => OnBackClicked?.Invoke())
            {
                text = "◀ ABORT & RETURN TO MENU",
                style = {
                    backgroundColor = Color.clear,
                    color = Color.gray,
                    height = 30,
                    borderTopWidth = 0, borderBottomWidth = 0, borderLeftWidth = 0, borderRightWidth = 0
                }
            };

            // Hover states for the naked button
            backBtn.RegisterCallback<MouseEnterEvent>(e => backBtn.style.color = Color.white);
            backBtn.RegisterCallback<MouseLeaveEvent>(e => backBtn.style.color = Color.gray);

            panel.Add(backBtn);
            root.Add(panel);

            return root;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));

#if UNITY_EDITOR
        public void ToUIDocument(string assetPath) => GraphicalUserInterfaceBuilder.ConvertToUIDocument(CreateGui(new GuiContext()), assetPath);
#endif
        public void FromUIDocument(string assetPath) => _lastCtx?.OnBuilt?.Invoke(GraphicalUserInterfaceBuilder.ConvertFromUIDocument(assetPath));
    }
}
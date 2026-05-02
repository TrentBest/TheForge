using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Assets.Scripts.Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Assets.Scripts.Builders.GuiBuilders
{
    /// <summary>
    /// Refactored State Property Editor.
    /// Migrated to Forge Builders to ensure theme consistency and systemic content creation.
    /// </summary>
    public class Workshop_Gui_FsmStateGuiBuilder : IGuiProvider
    {
        public string Title { get; set; } = "State Properties";
        private GuiContext _lastCtx;

        public VisualElement CreateGui(GuiContext ctx)
        {
            _lastCtx = ctx;

            // ROOT: Using ForgeContainerBuilder to follow the Granular Padding Protocol
            var root = new ForgeContainerBuilder("StateView_Root")
                .WithPadding(15)
                .WithFlexGrow(1);

            // 1. Identity Field
            root.AddChild(new ForgeTextFieldBuilder("State Identifier")
                .OnChanged(val => Debug.Log($"[FSM] State renamed to: {val}")));

            // 2. Lifecycle Dropdowns using ForgeDropdownBuilder
            // Note: We use OnBuild to apply the specific UI Toolkit formatting callbacks

            // --- ON ENTER ---
            root.AddChild(new ForgeDropdownBuilder("OnEnter:", GetRegisteredOnEnterMethods())
                .OnBuild(ve =>
                {
                    if (ve is DropdownField df)
                    {
                        df.formatSelectedValueCallback = OnEnterFormatSelectedValueCallback;
                        df.formatListItemCallback = OnEnterFormatListItemCallback;
                    }
                }));

            // --- ON UPDATE ---
            root.AddChild(new ForgeDropdownBuilder("OnUpdate:", GetRegisteredOnUpdateMethods())
                .OnBuild(ve =>
                {
                    if (ve is DropdownField df)
                    {
                        df.formatSelectedValueCallback = OnUpdateFormatSelectedValueCallback;
                        df.formatListItemCallback = OnUpdateFormatListItemCallback;
                    }
                }));

            // --- ON EXIT ---
            root.AddChild(new ForgeDropdownBuilder("OnExit:", GetRegisteredOnExitMethods())
                .OnBuild(ve =>
                {
                    if (ve is DropdownField df)
                    {
                        df.formatSelectedValueCallback = OnExitFormatSelectedValueCallback;
                        df.formatListItemCallback = OnExitFormatListItemCallback;
                    }
                }));

            return root.CreateGui(ctx);
        }

        // --------------------------------------------------------------------------------
        // CALLBACK IMPLEMENTATIONS (Experience Model Logic)
        // --------------------------------------------------------------------------------

        private string OnEnterFormatListItemCallback(string arg) => string.IsNullOrEmpty(arg) ? "(None)" : arg;
        private string OnEnterFormatSelectedValueCallback(string arg) => string.IsNullOrEmpty(arg) ? "Select Enter Logic.." : arg;

        private string OnUpdateFormatListItemCallback(string arg) => string.IsNullOrEmpty(arg) ? "(None)" : arg;
        private string OnUpdateFormatSelectedValueCallback(string arg) => string.IsNullOrEmpty(arg) ? "Select Update Logic.." : arg;

        private string OnExitFormatListItemCallback(string arg) => string.IsNullOrEmpty(arg) ? "(None)" : arg;
        private string OnExitFormatSelectedValueCallback(string arg) => string.IsNullOrEmpty(arg) ? "Select Exit Logic.." : arg;

        // --------------------------------------------------------------------------------
        // DATA FETCHING (Stubs)
        // --------------------------------------------------------------------------------

        private List<string> GetRegisteredOnExitMethods() => new List<string> { "" };
        private List<string> GetRegisteredOnUpdateMethods() => new List<string> { "" };
        private List<string> GetRegisteredOnEnterMethods() => new List<string> { "" };

        // --------------------------------------------------------------------------------
        // IGuiProvider Implementation
        // --------------------------------------------------------------------------------

        public Action<VisualElement> GetGuiBuilder()
        {
            return (root) => root.Add(CreateGui(new GuiContext { Name = "FsmState_Inspector" }));
        }

        public void ToUIDocument(string assetPath)
        {
            var root = CreateGui(_lastCtx ?? new GuiContext());
            string fileName = string.IsNullOrEmpty(assetPath) ? "FsmState_Properties_Bake" : System.IO.Path.GetFileNameWithoutExtension(assetPath);
            WorkshopUxmlBaker.Bake(root, fileName);
        }

        public void FromUIDocument(string assetPath)
        {
            // Hydration logic for the State View would go here if loading from static templates
            Debug.Log($"[FSM] State GUI hydration requested from {assetPath}");
        }
    }
}
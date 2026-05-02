using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.UI_And_Tools.Showcase
{

    /// <summary>

    /// The structural definition for any experience featured in the Showcase.

    /// </summary>

    public struct ShowcaseExperienceDef

    {

        public string Title;

        public string Overview;

        public List<string> CoreTechnologies;

        public Action OnLaunchExperience;

        public Action OnInspectArchitecture;

        internal bool IsUnderConstruction;

    }


    public class Showcase_ExperiencePortal : IGuiProvider
    {
        private readonly ShowcaseExperienceDef _def;
        public string Title => _def.Title;

        public Showcase_ExperiencePortal(ShowcaseExperienceDef def)
        {
            _def = def;
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var rootBuilder = new GraphicalUserInterfaceBuilder($"Portal_{_def.Title}")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.04f, 0.04f, 0.05f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexStart, Align.Stretch);

            // --- HERO HEADER ---
            var heroPanel = new GraphicalUserInterfaceBuilder("HeroPanel")
                .WithHeight(150)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.15f))
                .WithPadding(30)
                .WithBorderBottomWidth(2).WithBorderBottomColor(new Color(0.8f, 0.1f, 0.8f))
                .WithFlexLayout(FlexDirection.Column, Justify.FlexEnd, Align.FlexStart);

            heroPanel.AddChild(new ForgeLabelBuilder(_def.Title.ToUpper())
                .WithColor(Color.white).WithFontSize(36).WithFontStyle(FontStyle.Bold));

            rootBuilder.AddChild(heroPanel);

            // --- SPLIT CONTENT AREA ---
            var contentSplit = new GraphicalUserInterfaceBuilder("ContentSplit")
                .WithFlexGrow(1)
                .WithFlexLayout(FlexDirection.Row, Justify.SpaceBetween, Align.Stretch)
                .WithPadding(40);

            // LEFT: The Big Picture Teaching Area
            var bigPicturePanel = new GraphicalUserInterfaceBuilder("BigPicture")
                .WithFlexGrow(1)
                .WithMarginRight(40);

            bigPicturePanel.AddChild(new ForgeLabelBuilder("THE BIG PICTURE")
                .WithColor(Color.cyan).WithFontSize(18).WithFontStyle(FontStyle.Bold).WithMarginBottom(15));

            bigPicturePanel.AddChild(new ForgeLabelBuilder(_def.Overview)
                .WithColor(new Color(0.8f, 0.8f, 0.8f)).WithFontSize(14).WithWhiteSpace(WhiteSpace.Normal));

            // Tech Stack Tags
            var techStackRow = new GraphicalUserInterfaceBuilder("TechStackRow")
                .WithFlexLayout(FlexDirection.Row, Justify.FlexStart, Align.Center)
                .WithFlexWrap(Wrap.Wrap)
                .WithMarginTop(20);

            foreach (var tech in _def.CoreTechnologies)
            {
                techStackRow.AddChild(new GraphicalUserInterfaceBuilder("Tag")
                    .WithBackgroundColor(new Color(0.2f, 0.2f, 0.2f))
                    .WithBorderRadius(10).WithPaddingLeft(10).WithPaddingRight(10).WithPaddingTop(5).WithPaddingBottom(5).WithMarginRight(5)
                    .AddChild(new ForgeLabelBuilder(tech).WithColor(Color.cyan).WithFontSize(11)));
            }
            bigPicturePanel.AddChild(techStackRow);
            contentSplit.AddChild(bigPicturePanel);

            // RIGHT: Action Panel (Launch / Inspect)
            var actionPanel = new GraphicalUserInterfaceBuilder("ActionPanel")
                .WithWidth(350)
                .WithBackgroundColor(new Color(0.06f, 0.06f, 0.08f))
                .WithPadding(20)
                .WithBorderWidth(1).WithBorderColor(Color.gray)
                .WithFlexLayout(FlexDirection.Column, Justify.Center, Align.Stretch);

            actionPanel.AddChild(new ForgeLabelBuilder("INITIATE PROTOCOLS")
                .WithColor(Color.gray).WithFontSize(14).WithFontStyle(FontStyle.Bold).WithMarginBottom(20).WithAlignment(TextAnchor.MiddleCenter));

            // ACTION 1: Play
            actionPanel.AddChild(new ForgeButtonBuilder("LAUNCH EXPERIENCE")
                .WithBackgroundColor(new Color(0.2f, 0.8f, 0.2f)) // Green
                .WithHeight(60).WithFontSize(16).WithFontStyle(FontStyle.Bold).WithMarginBottom(10)
                .OnClick(() => _def.OnLaunchExperience?.Invoke()));

            // ACTION 2: Learn/Deconstruct
            actionPanel.AddChild(new ForgeButtonBuilder("DECONSTRUCT ARCHITECTURE")
                .WithBackgroundColor(new Color(0.8f, 0.1f, 0.8f)) // Purple
                .WithHeight(60).WithFontSize(16).WithFontStyle(FontStyle.Bold)
                .OnClick(() => _def.OnInspectArchitecture?.Invoke()));

            if (_def.IsUnderConstruction)
            {
                actionPanel.AddChild(new ForgeLabelBuilder("SYSTEM UNDER CONSTRUCTION")
                    .WithColor(Color.yellow).WithFontSize(12).WithMarginTop(15).WithAlignment(TextAnchor.MiddleCenter));
            }

            contentSplit.AddChild(actionPanel);
            rootBuilder.AddChild(contentSplit);

            return rootBuilder.Build();
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}
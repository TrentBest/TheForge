using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.Systems.MicroPackages.Literature;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Literature
{
    public class Workshop_Gui_Storyboard : IGuiProvider
    {
        public string Title => "NARRATIVE STORYBOARD";
        private StoryboardSequence _activeSequence;

        public Workshop_Gui_Storyboard(StoryboardSequence sequence = null)
        {
            _activeSequence = sequence ?? new StoryboardSequence();
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new ForgeContainerBuilder("StoryboardRoot")
                .WithFlexGrow(1).WithPadding(20)
                .WithBackgroundColor(new Color(0.08f, 0.08f, 0.1f))
                .AddChild(new ForgeLabelBuilder($"ACTIVE SEQUENCE: {_activeSequence.SequenceName.ToUpper()}")
                    .WithColor(Color.cyan).WithBold().WithFontSize(22).WithMarginBottom(20));

            // The Horizontal Frame Track
            var trackScroll = new ScrollView(ScrollViewMode.Horizontal) { style = { flexGrow = 1 } };
            var trackContainer = new ForgeContainerBuilder("TrackContainer")
                .WithDirection(FlexDirection.Row)
                .WithPadding(10);

            // Add Existing Frames
            foreach (var frame in _activeSequence.Frames.OrderBy(f => f.SequenceNumber))
            {
                trackContainer.AddChild(CreateFrameCard(frame));
            }

            // Add "New Frame" Button at the end
            var addFrameBtn = new ForgeButtonBuilder("+ NEW DELTA EVENT")
                .WithBackgroundColor(new Color(0.1f, 0.4f, 0.2f)).WithTextColor(Color.white)
                .WithWidth(200).WithHeight(300).WithMargin(10).WithBorderRadius(8)
                .WithOnClick(() => CreateNewFrame());

            trackContainer.AddChild(addFrameBtn);
            trackScroll.Add(trackContainer.Build());

            root.OnBuild(ve => ve.Add(trackScroll));

            return root.Build();
        }

        private ForgeContainerBuilder CreateFrameCard(StoryboardFrame frame)
        {
            return new ForgeContainerBuilder($"Frame_{frame.FrameId}")
                .WithWidth(250).WithHeight(350).WithMargin(10)
                .WithBackgroundColor(new Color(0.15f, 0.15f, 0.18f))
                .WithBorderColor(Color.black).WithBorderWidth(2).WithBorderRadius(8)
                .WithPadding(10)
                .AddChild(new ForgeLabelBuilder($"FRAME {frame.SequenceNumber}")
                    .WithColor(Color.gray).WithBold().WithMarginBottom(10))

                // Image Placeholder / Captured Texture
                .OnBuild(ve => {
                    var imageDisplay = new VisualElement { style = { height = 140, backgroundColor = Color.black, marginBottom = 10 } };
                    if (frame.CapturedArtifact != null) imageDisplay.style.backgroundImage = new StyleBackground(frame.CapturedArtifact);
                    else imageDisplay.Add(new Label("NO ARTIFACT SCRIED") { style = { color = Color.red, unityTextAlign = TextAnchor.MiddleCenter, flexGrow = 1 } });
                    ve.Add(imageDisplay);
                })

                // Description Input
                .OnBuild(ve => {
                    var descInput = new TextField { value = frame.SceneDescription, multiline = true };
                    descInput.style.height = 80;
                    descInput.style.whiteSpace = WhiteSpace.Normal;
                    descInput.RegisterValueChangedCallback(evt => frame.SceneDescription = evt.newValue);
                    ve.Add(descInput);
                })

                // The Magic Button: Opens the 3D Portal
                .AddChild(new ForgeButtonBuilder("SCRY THIS MOMENT")
                    .WithBackgroundColor(new Color(0.64f, 0.17f, 0.77f)).WithTextColor(Color.white).WithBold()
                    .WithMarginTop(10)
                    .WithOnClick(() => OpenScryingPool(frame)));
        }

        private void CreateNewFrame()
        {
            int nextIndex = _activeSequence.Frames.Count > 0 ? _activeSequence.Frames.Max(f => f.SequenceNumber) + 1 : 1;
            var newFrame = new StoryboardFrame { SequenceNumber = nextIndex, SceneDescription = "Describe the event..." };
            _activeSequence.Frames.Add(newFrame);
            // Refresh UI logic here
        }

        private void OpenScryingPool(StoryboardFrame frame)
        {
            Debug.Log($"[Storyboard] Opening Scrying Pool for Frame {frame.SequenceNumber}. Anchoring World Chronos to {frame.ChronologicalTime}...");
            // In Production: Tell the SingularityDataBus to manifest the `Workshop_Gui_ScryingPool` diegetic terminal.
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string path) { }
        public void FromUIDocument(string path) { }
    }
}
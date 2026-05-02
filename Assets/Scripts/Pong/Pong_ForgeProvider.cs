using System;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.Pong
{
    public class Pong_ForgeProvider : IGuiProvider
    {
        public string Title => "SINGULARITY PONG - REFORGED";
        private PongContext _ctx;
        private FSMHandle _handle;
        private VisualElement _ball, _leftPaddle, _rightPaddle;

        public Pong_ForgeProvider(PongGameMode mode)
        {
            _ctx = new PongContext(mode);
            // Pillar 2: Assigning to a specific Processing Group
            _handle = FSM_API.Create.CreateInstance(PongFsm.DEF_NAME, _ctx, PongFsm.GROUP);
        }

        public VisualElement CreateGui(GuiContext ctx)
        {
            var root = new GraphicalUserInterfaceBuilder("PongArena")
                .WithFlexGrow(1)
                .WithBackgroundColor(new Color(0.02f, 0.02f, 0.03f))
                .OnBuild(ve => {
                    ve.focusable = true;
                    ve.style.overflow = Overflow.Hidden;
                }).Build();

            // Pillar 1: Using Forge Builders for entities
            _ball = new GraphicalUserInterfaceBuilder("Ball")
                .WithBackgroundColor(Color.white)
                .WithBorderRadius(PongContext.BALL_SIZE / 2)
                .OnBuild(ve => {
                    ve.style.position = Position.Absolute;
                    ve.style.width = PongContext.BALL_SIZE;
                    ve.style.height = PongContext.BALL_SIZE;
                }).Build();

            root.Add(_ball);

            // Tick Logic & Rendering Sync
            root.schedule.Execute(state => {
                // Manually ticking the group as per the UI-driven pattern
                FSM_API.Interaction.Update(PongFsm.GROUP);

                // Sync Visuals
                _ball.style.left = _ctx.BallPos.x;
                _ball.style.top = _ctx.BallPos.y;
            }).Every(16);

            return root;
        }

        public Action<VisualElement> GetGuiBuilder() => root => root.Add(CreateGui(new GuiContext()));
        public void ToUIDocument(string assetPath) { }
        public void FromUIDocument(string assetPath) { }
    }
}
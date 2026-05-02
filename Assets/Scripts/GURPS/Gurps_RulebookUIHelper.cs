using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;

namespace Workshop.GURPS
{
    public static class GURPS_RulebookUIHelper
    {
        private static VisualElement _activeTooltip;

        public static VisualElement CreateBookIcon(string bookName, float size = 24f)
        {
            var book = DigitalGenericUniversalRolePlayingSystem.Books.GetByName(bookName);

            var iconBuilder = new ForgeContainerBuilder("BookIcon")
                .WithWidth(size).WithHeight(size * 1.3f)
                .WithBackgroundColor(new Color(0.1f, 0.1f, 0.1f))
                .WithMarginRight(5).WithBorderWidth(1).WithBorderColor(Color.gray);

            if (book != null)
            {
                iconBuilder.OnBuild(ve => {
                    var tex = book.GetCoverImage();
                    if (tex != null)
                    {
                        ve.style.backgroundImage = new StyleBackground(tex);
                        ve.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                    }

                    // FIXED: position changed to localMousePosition
                    ve.RegisterCallback<MouseEnterEvent>(e => ShowBookTooltip(ve, book, e.localMousePosition));
                    ve.RegisterCallback<MouseLeaveEvent>(e => HideTooltip());
                });
            }

            return iconBuilder.Build();
        }

        public static void ShowBookTooltip(VisualElement trigger, GURPSBook book, Vector2 mousePos)
        {
            if (book == null || trigger == null || trigger.panel == null) return;
            HideTooltip();

            _activeTooltip = new ForgeContainerBuilder("BookTooltip")
                .WithPosition(Position.Absolute)
                .WithPaddingTop(10).WithPaddingBottom(10).WithPaddingLeft(10).WithPaddingRight(10)
                .WithBackgroundColor(new Color(0.05f, 0.05f, 0.07f, 0.95f))
                .WithBorderWidth(1).WithBorderColor(Color.cyan).WithBorderRadius(5)
                .AddChild(new ForgeLabelBuilder(book.Name.ToUpper()).WithBold().WithColor(Color.cyan).WithFontSize(12))
                .AddChild(new ForgeContainerBuilder("TooltipCover").WithWidth(150).WithHeight(210).WithMarginTop(10)
                    .OnBuild(ve => {
                        var tex = book.GetCoverImage();
                        if (tex != null)
                        {
                            ve.style.backgroundImage = new StyleBackground(tex);
                            ve.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                        }
                    }))
                .AddChild(new ForgeLabelBuilder($"Tier {book.SpecificityTier} Ruleset").WithFontSize(10).WithColor(Color.gray))
                .Build();

            Vector2 worldPos = trigger.LocalToWorld(mousePos);
            _activeTooltip.style.left = worldPos.x + 20;
            _activeTooltip.style.top = worldPos.y - 100;

            // FIXED: Using panel visual tree instead of missing UI_Root_Access
            trigger.panel.visualTree.Add(_activeTooltip);
        }

        public static void HideTooltip()
        {
            if (_activeTooltip != null)
            {
                _activeTooltip.RemoveFromHierarchy();
                _activeTooltip = null;
            }
        }
    }
}
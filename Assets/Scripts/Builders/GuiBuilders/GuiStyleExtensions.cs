using TheSingularityWorkshop.Forge.Builders.GuiBuilders;
using TheSingularityWorkshop.Forge.Builders.GuiBuilders.Themes;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Builders.GuiBuilders
{
    public static class GuiStyleExtensions
    {
        public static GraphicalUserInterfaceBuilder ApplySkin(this GraphicalUserInterfaceBuilder builder, GuiElementType type)
        {
            var theme = GuiSkin.Active;
            var profile = theme.GetProfile(type);

            builder.WithBackgroundColor(profile.BackgroundColor)
                   .WithBorderColor(profile.BorderColor)
                   .WithBorderWidth((int)profile.BorderWidth)
                   .WithBorderRadius(profile.CornerRadius);

            

            return builder;
        }
    }
}
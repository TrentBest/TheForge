using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders;
using Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes;

namespace Assets.Scripts.Builders.GuiBuilders
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
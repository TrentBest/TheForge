using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes
{
    public static class GuiSkin
    {
        private static GuiTheme _activeTheme;

        public static GuiTheme Active
        {
            get
            {
                if (_activeTheme == null)
                {
                    _activeTheme = Resources.Load<GuiTheme>("Themes/DefaultHolo");
                    if (_activeTheme == null) _activeTheme = ScriptableObject.CreateInstance<GuiTheme>();
                }
                return _activeTheme;
            }
            set => _activeTheme = value;
        }
    }
}
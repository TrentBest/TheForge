using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheSingularityWorkshop.Forge.Builders.GuiBuilders.Themes
{
    public enum GuiElementType
    {
        GlobalDefault,
        Window,
        Panel,
        ButtonPrimary,
        ButtonSecondary,
        ButtonGhost,
        ButtonAlert,
        InputField,
        Header,
        Label
    }

    [Serializable]
    public class StyleProfile
    {
        public GuiElementType TargetType;

        public Color BackgroundColor = new Color(0, 0, 0, 0);
        public Color BorderColor = new Color(1, 1, 1, 0.1f);
        public Color TextColor = new Color(0.9f, 0.9f, 1f);

        public float BorderWidth = 1f;
        public float CornerRadius = 5f;

        public StyleProfile Clone() => (StyleProfile)this.MemberwiseClone();
    }

    [CreateAssetMenu(menuName = "Singularity/GUI Theme", fileName = "NewGuiTheme")]
    public class GuiTheme : ScriptableObject
    {
        [Header("Global Signal Palette")]
        // These remain global because they are used for logic/highlighting across many elements
        public Color PrimaryAccent = new Color(0f, 0.8f, 1f, 0.8f);
        public Color SecondaryAccent = new Color(0.2f, 1f, 0.4f, 0.6f);
        public Color AlertColor = new Color(1f, 0.2f, 0.2f, 0.7f);

        [Header("Component Styling")]
        public StyleProfile GlobalDefault = new StyleProfile
        {
            TargetType = GuiElementType.GlobalDefault,
            BackgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.9f),
            CornerRadius = 5f
        };

        public List<StyleProfile> Profiles = new List<StyleProfile>();

        // --- API ---
        public StyleProfile GetProfile(GuiElementType type)
        {
            var p = Profiles.FirstOrDefault(x => x.TargetType == type);
            // If specific profile missing, fallback to GlobalDefault
            return p ?? GlobalDefault;
        }

        public StyleProfile GetOrCreateProfile(GuiElementType type)
        {
            var p = Profiles.FirstOrDefault(x => x.TargetType == type);
            if (p == null)
            {
                p = GlobalDefault.Clone();
                p.TargetType = type;
                Profiles.Add(p);
            }
            return p;
        }
    }
}
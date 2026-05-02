using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Themes
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
    public class GuiEffect
    {
        [Tooltip("A recognizable name for this effect, e.g., 'NeonPulse' or 'GlitchHover'")]
        public string EffectName = "New Effect";

        [Tooltip("The exact name of the FSM Definition to create/lookup")]
        public string FsmTemplateName = "UI_Pulse_Template";

        [Tooltip("The execution group for the FSM API")]
        public string ProcessingGroup = "GUI_Effects";

        [Tooltip("The default starting state for this effect")]
        public string InitialState = "Idle";

        // You can add JSON configuration data here later if your FSMs need dynamic variables
        // public string ContextPayloadJson = "{}"; 
    }

    [Serializable]
    public class StyleProfile
    {
        public GuiElementType TargetType;

        public Color BackgroundColor = new Color(0, 0, 0, 0);
        public Color BorderColor = new Color(1, 1, 1, 0.1f); // Sets All Border colors
        public Color BorderTopColor = new Color(1, 1, 1, 0.1f);
        public Color BorderBottomColor = new Color(1, 1, 1, 0.1f);
        public Color BorderLeftColor = new Color(1, 1, 1, .1f);
        public Color BorderRightColor = new Color(1, 1, 1, .1f);

        public Color BorderTopLeftColor = new Color(1, 1, 1, .1f);
        public Color BorderBottomLeftColor = new Color(1, 1, 1, .1f);
        public Color BoderTopRightColor = new Color(1, 1, 1, .1f);
        public Color BorderBottomRightColor = new Color(1, 1, 1, .1f);

        public Color TextColor = new Color(0.9f, 0.9f, 1f);

        public float BorderTopWidth = 1f;
        public float BorderRightWidth = 1f;
        public float BorderBottomWidth = 1f;
        public float BorderLeftWidth = 1f;

        public float BorderWidth = 0f; // Sets All Border Widths

        public float BorderTopLeftRadius = 5f;
        public float BorderTopRightRadius = 5f;
        public float BorderBottomRightRadius = 5f;
        public float BorderBottomLeftRadius = 5f;

        public float CornerRadius = 0f; // Sets All Radius

        [Header("State-Driven Effects")]
        [Tooltip("FSM Effects to attach to this specific component type")]
        public List<GuiEffect> AttachedEffects = new List<GuiEffect>();

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
            BorderBottomLeftRadius = 5f,
            BorderBottomRightRadius = 0,
            BorderTopLeftRadius = 0,
            BorderTopRightRadius = 5f
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
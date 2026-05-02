using UnityEngine;

namespace Workshop.Asteroids
{
    public class HangarTheme
    {
        // Defaulted to the "Singularity Forge" theme (Olive & Copper)
        public Color BaseBackground = new Color(0.12f, 0.16f, 0.10f);
        public Color PanelBackground = new Color(0.20f, 0.25f, 0.18f);
        public Color PrimaryAccent = new Color(0.85f, 0.40f, 0.10f); // Copper

        public Color TextActive = Color.black;
        public Color TextNormal = Color.white;
        public Color TextMuted = Color.gray;
        public Color TitleText = new Color(1.0f, 0.65f, 0.0f); // Ember Glow

        public Color BorderDark = Color.black;
        public Color BorderLight = Color.white;
        public Color ViewerBackground = new Color(0.05f, 0.05f, 0.05f); // Deep space
    }
}
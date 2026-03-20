// File: Assets/Scripts/Builders/GuiBuilders/Themes/SingularityTheme.cs
using UnityEngine;
using UnityEngine.UIElements;

namespace TheSingularityWorkshop.Builders.GuiBuilders.Themes
{
    public static class SingularityTheme
    {
        // The "Default Experience" - A holographic grid
        public static readonly Color VoidBlack = new Color(0.05f, 0.05f, 0.05f, 1f);
        public static readonly Color GridLine = new Color(0f, 0.8f, 1f, 0.1f);
        public static readonly Color HologramBlue = new Color(0f, 0.8f, 1f, 0.8f);
        public static readonly Color NarrativeGold = new Color(1f, 0.7f, 0.2f, 0.9f); // For "Story" elements
        public static readonly Color LogicGreen = new Color(0.2f, 1f, 0.4f, 0.7f);   // For "Mechanics"

        public static void ApplyHolographicStyle(VisualElement root)
        {
            // This simulates the "Gridded Material in a surrounding box"
            root.style.backgroundColor = VoidBlack;
            root.style.backgroundImage = GenerateGridTexture();
            root.style.borderTopWidth = 2;
            root.style.borderTopColor = HologramBlue;
        }

        private static Texture2D GenerateGridTexture()
        {
            // Procedurally generate a simple grid texture for the background
            var tex = new Texture2D(64, 64);
            var cols = new Color[64 * 64];
            for (int i = 0; i < cols.Length; i++) cols[i] = new Color(0, 0, 0, 0);

            // Draw grid lines
            for (int i = 0; i < 64; i++)
            {
                cols[i * 64] = GridLine; // Vertical
                cols[i] = GridLine;      // Horizontal
            }
            tex.SetPixels(cols);
            tex.Apply();
            return tex;
        }
    }
}
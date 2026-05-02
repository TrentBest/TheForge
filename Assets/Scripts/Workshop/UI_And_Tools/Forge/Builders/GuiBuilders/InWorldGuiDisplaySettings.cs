using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    [System.Serializable]
    public class InWorldGuiDisplaySettings
    {
        [Header("1. Readability")]
        [Tooltip("Scales UI elements up. 1.0 = Native. 2.0 = Big Mode (TV style).")]
        [Range(0.5f, 4.0f)]
        public float uiScale = 2.0f;

        [Header("2. The Design (Editor Size)")]
        [Tooltip("The resolution you designed this GUI for. If you built it to look good at 1920x1080, put that here.")]
        public Vector2Int designLayoutSize = new Vector2Int(1024, 768);

        [Header("3. The Reality (In-World Size)")]
        [Tooltip("Physical width/height in Unity Units (Meters). Leave at 0,0 to auto-detect from the Mesh Renderer bounds.")]
        public Vector2 physicalSize = Vector2.zero;

        [Header("4. The Bridge (Quality)")]
        [Tooltip("The actual sharpness of the texture. Higher = crisper text. The script maps the Design Size to this resolution.")]
        public int targetTextureWidth = 2048;


        [Header("Hardware Specs")]
        public InWorldDisplayMode mode = InWorldDisplayMode.RenderTexture;

        [Range(0f, 1f)]
        public float curvature = 0f; // For those fancy sci-fi curved screens
        public LayerMask interactionLayer;

       
    }

    public enum InWorldDisplayMode
    {
        Hologram,
        Projector,
        FlatPanel,
        RenderTexture
    }
}

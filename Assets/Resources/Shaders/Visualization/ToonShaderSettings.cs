using System;
using UnityEngine;

namespace Assets.Resources.Shaders.Visualization
{
    [Serializable]
    public class ToonShaderSettings
    {
        public string ShaderName = "NewToonShader";
        public bool EnableOutlines = true;
        public Color OutlineColor = Color.black;
        public float OutlineWidth = 0.015f;
        public int ColorBands = 3;
    }
}

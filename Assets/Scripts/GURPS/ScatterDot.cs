using UnityEngine;

namespace Workshop.GURPS
{
    public struct ScatterDot
    {
        public float X_Value;
        public float Y_Percent; // 0 to 100 (Height on the graph)
        public Color DotColor;
        public string TooltipTitle;
        public string TooltipData;
    }
}
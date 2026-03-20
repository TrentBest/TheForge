namespace Assets.Scripts.raWWar.Editors.ShaderForge
{
    public static class ShaderBehaviors
    {
        // Action<IStateContext> signature!
        public static void InjectPositionInstancing(ShaderForgeContext ctx) // Replace with IStateContext in actual code
        {
            if (!ctx.IsValid) return; // The Magic of your FSM API!

            ctx.StructData.Add("float3 position;");
            ctx.VertexLogic.Add("// --- Position Instancing ---");
            ctx.VertexLogic.Add("v.vertex.xyz += data.position;");
        }

        public static void InjectRotationInstancing(ShaderForgeContext ctx)
        {
            if (!ctx.IsValid) return;

            ctx.StructData.Add("float facing;");
            ctx.VertexLogic.Add("// --- Y-Axis Rotation ---");
            ctx.VertexLogic.Add("float s, c; sincos(data.facing, s, c);");
            ctx.VertexLogic.Add("float3x3 rotMatrix = float3x3(c, 0, s, 0, 1, 0, -s, 0, c);");
            ctx.VertexLogic.Add("v.vertex.xyz = mul(rotMatrix, v.vertex.xyz);");
        }

        public static void InjectHealthTinting(ShaderForgeContext ctx)
        {
            if (!ctx.IsValid) return;

            ctx.StructData.Add("float health;");
            // Add a property to the material so the user can define the "Dead" color
            ctx.Properties.Add("_WoundColor (\"Wound Color\", Color) = (1,0,0,1)");
            ctx.FragmentLogic.Add("// --- Health Tinting ---");
            ctx.FragmentLogic.Add("col.rgb = lerp(_WoundColor.rgb, col.rgb, data.health / 100.0);");
        }
    }
}
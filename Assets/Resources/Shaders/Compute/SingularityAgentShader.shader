Shader "Singularity/FSM_StructuredBuffer_Agent"
{
    Properties
    {
        // No properties needed here anymore! 
        // Color and Scale are driven entirely by the unmanaged C# memory.
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            // 1. MUST EXACTLY MATCH THE C# STRUCT (32 Bytes total)
            struct FSMAgentData {
                float3 position; // 12 bytes
                float scale;     // 4 bytes
                float4 color;    // 16 bytes
            };

            // 2. The GPU Pointer to your unmanaged C# array
            StructuredBuffer<FSMAgentData> _AgentDataBuffer;

            struct appdata
            {
                float4 vertex : POSITION;
                uint instanceID : SV_InstanceID; // The Agent's Index (0 to 1,000,000)
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                // 3. THE MAGIC: O(1) Memory Lookup. No texture math required!
                FSMAgentData agent = _AgentDataBuffer[v.instanceID];
                
                // 4. Calculate the real world position (Scale first, then Translate)
                float3 worldPos = (v.vertex.xyz * agent.scale) + agent.position;
                
                // 5. Convert to camera clip space
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0f));
                
                // 6. Pass the C# color directly to the screen!
                o.color = agent.color;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Paint the pixel
                return i.color;
            }
            ENDCG
        }
    }
}
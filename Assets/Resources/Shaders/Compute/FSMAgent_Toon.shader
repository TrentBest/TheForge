Shader "Singularity/FSMAgent_Toon"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ShadowTint ("Shadow Tint Color", Color) = (0.2, 0.2, 0.4, 1)
        _MyRenderStyleID ("Render Style ID", Int) = 1 // Defaults to 1 for Toon
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma target 4.5

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 worldNormal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct FSMAgentData
            {
                float3 position;
                float4 rotation;
                float4 color;
                int renderStyleID;
            };

            StructuredBuffer<FSMAgentData> _AgentDataBuffer;
            int _MyRenderStyleID;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ShadowTint;

            float3 RotateVectorByQuaternion(float3 v, float4 q)
            {
                float3 t = 2.0 * cross(q.xyz, v);
                return v + q.w * t + cross(q.xyz, t);
            }

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                FSMAgentData agent = _AgentDataBuffer[instanceID];

                // VERTEX DISCARD
                if (agent.renderStyleID != _MyRenderStyleID)
                {
                    o.pos = float4(0, 0, 0, 0);
                    o.uv = v.uv;
                    o.color = float4(0, 0, 0, 0);
                    o.worldNormal = float3(0,0,0);
                    return o;
                }

                float3 rotatedVertex = RotateVectorByQuaternion(v.vertex.xyz, agent.rotation);
                float3 worldPosition = rotatedVertex + agent.position;
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
                
                // We rotate the normals so the Toon shading respects the agent's facing direction
                o.worldNormal = RotateVectorByQuaternion(v.normal, agent.rotation);
                
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = agent.color;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                
                fixed4 texColor = tex2D(_MainTex, i.uv) * i.color;

                // Simple Cel-Shading Math
                float3 normal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                
                float NdotL = dot(normal, lightDir);
                
                // Creates a harsh 2-step lighting band
                float lightIntensity = smoothstep(0.0, 0.01, NdotL);
                
                // Apply shadow tint to unlit areas
                float4 finalColor = lerp(texColor * _ShadowTint, texColor, lightIntensity);

                return finalColor;
            }
            ENDCG
        }
    }
}
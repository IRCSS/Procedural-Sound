Shader "Unlit/VisualizerShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            StructuredBuffer<float> _WaveForm;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
               
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = float4(0.925, 0.878, 0.823, 1.);
            float waveFormHeight = _WaveForm[(int)(i.uv.x * 128)];

                 col = lerp(col, float4(0.305, 0.223, 0.223, 1. ), 1. - smoothstep(0.01, 0.015, abs(i.uv.y - 0.7 - waveFormHeight) ));
                 return col;
            }
            ENDCG
        }
    }
}

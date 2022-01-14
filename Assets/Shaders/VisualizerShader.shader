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

            StructuredBuffer<float> _whiteKeysPressedState;
            StructuredBuffer<float> _blackKeysPressedState;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
               
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                // Draw the wave forms
                fixed4 col = float4(0.925, 0.878, 0.823, 1.);
                float waveFormHeight = _WaveForm[(int)(i.uv.x * 128)];


                 col = lerp(col, float4(0.305, 0.223, 0.223, 1. ), 1. - smoothstep(0.01, 0.015, abs(i.uv.y - 0.7 - waveFormHeight) ));

                 // Draw the white keys
                int numberOfWhiteKeys = 28;
                 float2 keyboardCoordF = frac (float2(i.uv.x * numberOfWhiteKeys, (i.uv.y - 0.0) / 0.3));
                 float2 keyboardCoordI = floor(float2(i.uv.x * numberOfWhiteKeys, (i.uv.y - 0.0) / 0.3));

               
                 float3 keyBoardCol    = float3(0.925, 0.878, 0.823);
                 float keyIsPressed = _whiteKeysPressedState[keyboardCoordI.x];

                 keyBoardCol = lerp(keyBoardCol, float3(0.172, 0.435, 0.784), keyIsPressed);

                 keyBoardCol = lerp(keyBoardCol, float3(0.305, 0.223, 0.223), step(0.47, abs(keyboardCoordF.y - 0.5))); // vertical borders
                 keyBoardCol = lerp(keyBoardCol, float3(0.305, 0.223, 0.223), step(0.45, abs(keyboardCoordF.x - 0.5))); // horziontal borders
               
                 // Draw the black keys

                 keyboardCoordF = frac (float2((i.uv.x * numberOfWhiteKeys) - 0.5, (i.uv.y - 0.0) / 0.3));
                 keyboardCoordI = floor(float2((i.uv.x * numberOfWhiteKeys) - 0.5, (i.uv.y - 0.0) / 0.3));

                 int blackKeyInOctave = (int)(keyboardCoordI % 7);

                 const float whereBlackKeysExists[7] = { 1., 2., 0., 3., 4., 5., 0. };
                 
                 keyIsPressed = _blackKeysPressedState[(int)(floor(keyboardCoordI.x / 7.) * 5.0) + (int)whereBlackKeysExists[blackKeyInOctave] - 1];
                 float3 blackKeyCol = lerp(float3(0.305, 0.223, 0.223), float3(0.925, 0.352, 0.223), keyIsPressed);

                 keyBoardCol = lerp(keyBoardCol, blackKeyCol, (1.- step(0.3, abs(keyboardCoordF.x - 0.5))) * step(0.4, keyboardCoordF.y) * step(0.5, whereBlackKeysExists[blackKeyInOctave]));

                 col.xyz = lerp(col.xyz, keyBoardCol, (1. - step(0.3, i.uv.y)) * step(0.0, i.uv.y));


                 return col;
            }
            ENDCG
        }
    }
}

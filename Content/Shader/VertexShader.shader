Shader "Hidden/ProBuilder/VertexShader"
{
    Properties
    {
        _Scale("Scale", Range(1,7)) = 3.3
        _Color ("Color", Color) = (1,1,1,1)
        _HandleZTest ("_HandleZTest", Int) = 8
    }

    SubShader
    {
        Tags
        {
            "IgnoreProjector"="True"
            "RenderType"="Geometry"
            "Queue"="Geometry"
        }

        Lighting Off
        ZTest [_HandleZTest]
        ZWrite On
        Cull Off
        Blend Off
        Offset -1,-1

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Scale;
            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                float ortho = (1 - UNITY_MATRIX_P[3][3]);
                v2f o;

                o.pos = float4(UnityObjectToViewPos(v.vertex.xyz), 1);
                o.pos.xyz *= lerp(.99, .95, ortho);
                o.pos = mul(UNITY_MATRIX_P, o.pos);

                // convert vertex to screen space, add pixel-unit xy to vertex, then transform back to clip space.
                float4 clip = o.pos;

                clip.xy /= clip.w;
                clip.xy = clip.xy * .5 + .5;
                clip.xy *= _ScreenParams.xy;

                clip.xy += v.texcoord.xy * _Scale;
                clip.xy /= _ScreenParams.xy;
                clip.xy = (clip.xy - .5) / .5;
                clip.xy *= clip.w;

                o.pos = clip;

                return o;
            }

            half4 frag (v2f i) : COLOR
            {
                return _Color;
            }

            ENDCG
        }
    }
}

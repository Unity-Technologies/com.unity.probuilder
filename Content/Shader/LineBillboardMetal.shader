Shader "Hidden/ProBuilder/LineBillboardMetal"
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
            "RenderType"="Transparent"
            "DisableBatching"="True"
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
            #include "ProBuilderCG.cginc"

            float _Scale;
            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPosWithOffset(v.vertex.xyz);

                // convert vertex to screen space, add pixel-unit xy to vertex, then transform back to clip space.
                float4 clip = o.pos;

                float4 a = ClipToScreen(o.pos);
                float4 b = ClipToScreen(UnityObjectToClipPosWithOffset(v.tangent.xyz));
                float2 d = normalize(b-a).xy;
                float2 p = float2(-d.y, d.x);
                a.xy += p * v.tangent.w * _Scale;

                o.pos = ScreenToClip(a);
                o.color = v.color;
                return o;
            }

            half4 frag (v2f i) : COLOR
            {
                return _Color * i.color;
            }

            ENDCG
        }
    }
}

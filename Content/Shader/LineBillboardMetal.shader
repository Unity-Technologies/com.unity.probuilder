Shader "Hidden/ProBuilder/LineBillboardMetal"
{
    Properties
    {
        _Scale("Scale", Range(1,7)) = 3.3
        _Color ("Color", Color) = (1,1,1,1)
        _HandleZTest ("_HandleZTest", Int) = 8
        _HandleZWrite("_HandleZWrite", Int) = 1
    }

    SubShader
    {
        Tags
        {
            "IgnoreProjector"="True"
            "RenderType"="Geometry"
            "Queue"="Geometry"
            "DisableBatching"="True"
        }

        Lighting Off
        ZTest [_HandleZTest]
        ZWrite [_HandleZWrite]
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
                // next vertex is stored in xyz, and direction to move current vertex is w
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
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

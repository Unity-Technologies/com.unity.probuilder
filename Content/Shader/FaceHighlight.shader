Shader "Hidden/ProBuilder/FaceHighlight"
{
    Properties
    {
        _Color ("Color Tint", Color) = (1,1,1,1)
        _Dither ("Dithering", float) = 0
        _HandleZTest ("_HandleZTest", Int) = 8
        _HandleZWrite("_HandleZWrite", Int) = 0
    }

    SubShader
    {
        Tags { "IgnoreProjector"="True" "Queue"="Transparent" }
        Lighting Off
        ZTest [_HandleZTest]
        ZWrite [_HandleZWrite]
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;
            float _Dither;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = float4(UnityObjectToViewPos(v.vertex.xyz), 1);
                o.pos.xy *= lerp(.99, 1, unity_OrthoParams.w);
                o.pos.z *= .99;
                o.pos = mul(UNITY_MATRIX_P, o.pos);

                return o;
            }

            half4 frag (v2f i) : COLOR
            {
                i.pos.xy = floor(i.pos.xy * 1) * .5;
                float checker = -frac(i.pos.x + i.pos.y);
                clip(lerp(1, checker, _Dither));

                return _Color;
            }

            ENDCG
        }
    }
}

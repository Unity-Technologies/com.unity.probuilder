// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/ProBuilder/FacePicker"
{
    Properties {
        _Tint ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "ProBuilderPicker"="Base" }
        Lighting Off
        ZTest LEqual
        ZWrite On
        Cull Back
        Blend Off

        Pass
        {
            Name "Base"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Tint;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                return o;
            }

            float4 frag (v2f i) : COLOR
            {
                return _Tint * i.color;
            }

            ENDCG
        }
    }
}

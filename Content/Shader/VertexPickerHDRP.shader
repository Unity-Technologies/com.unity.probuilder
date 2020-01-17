Shader "Hidden/ProBuilder/VertexPickerHDRP"
{
    Properties {}

    SubShader
    {
        Tags
        {
            "ProBuilderPicker"="VertexPass"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "DisableBatching"="True"
            "LightMode"="Always"
        }

        Lighting Off
        ZTest LEqual
        ZWrite On
        Cull Off
        Blend Off
        Offset -1, -1

        Pass
        {
            Name "Vertices"

CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "ProBuilderCG.cginc"

            // Is the camera in orthographic mode? (1 yes, 0 no)
            #define ORTHO (1 - UNITY_MATRIX_P[3][3])

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;

                o.pos = float4(UnityObjectToViewPos(v.vertex.xyz), 1);
                o.pos.xyz *= lerp(.99, .95, ORTHO);
                o.pos = mul(UNITY_MATRIX_P, o.pos);

                o.pos = GetPickerColor(o.pos,  v.texcoord1);
                o.uv = v.texcoord.xy;
                o.color = v.color;

                return o;
            }

            float4 frag (v2f i) : COLOR
            {
                return i.color;
            }

ENDCG
        }
    }
}

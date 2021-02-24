Shader "Hidden/ProBuilder/EdgePicker"
{
    Properties {}

    SubShader
    {
        Tags
        {
            "ProBuilderPicker"="EdgePass"
            "IgnoreProjector"="True"
            "DisableBatching"="True"
            "LightMode"="Always"
        }

        Lighting Off
        ZTest LEqual
        ZWrite On
        Cull Off
        Blend Off

        Pass
        {
            Name "Edges"
            AlphaTest Greater .25

CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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
                o.pos = float4(UnityObjectToViewPos(v.vertex.xyz), 1);
                //Offsetting the edges to avoid z-fighting problems occuring with Unity 20.2
                o.pos.xy *= lerp(.99, 1, unity_OrthoParams.w);
                //Moving edges closer when using orthographic camera
                o.pos.z *= lerp(.99, .95, unity_OrthoParams.w);
                o.pos = mul(UNITY_MATRIX_P, o.pos);

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

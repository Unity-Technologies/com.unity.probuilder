Shader "Hidden/ProBuilder/VertexPickerURP"
{
    Properties {}

    SubShader
    {
        Tags
        {
            "ProBuilderPicker"="VertexPass"
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
            "IgnoreProjector"="True"
            "DisableBatching"="True"
        }

        Pass
        {
            Name "Vertices"
            // Tags { "LightMode"="SRPDefaultUnlit" }
            Tags { "LightMode"="ProBuilderPickerA" }

            ZTest LEqual
            ZWrite On
            Cull Off
            Blend Off
            Offset -1, -1

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "ProBuilderCG_URP.cginc"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 positionVS = TransformWorldToView(positionWS);

                output.positionCS = float4(positionVS, 1);
                output.positionCS.xyz *= lerp(0.99, 0.95, ORTHO);
                output.positionCS = mul(UNITY_MATRIX_P, output.positionCS);

                output.positionCS = GetPickerColor(output.positionCS, input.uv1);
                output.uv = input.uv0.xy;
                output.color = input.color;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return input.color;
            }
            ENDHLSL
        }
    }

    Fallback "Hidden/ProBuilder/VertexPicker"
}

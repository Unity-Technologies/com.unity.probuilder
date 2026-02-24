Shader "Hidden/ProBuilder/EdgePickerURP"
{
    Properties {}

    SubShader
    {
        Tags
        {
            "ProBuilderPicker"="EdgePass"
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
            "IgnoreProjector"="True"
            "DisableBatching"="True"
        }

        Pass
        {
            Name "Edges"
            // Tags { "LightMode"="SRPDefaultUnlit" }
            Tags { "LightMode"="ProBuilderPickerA" }
            ZTest LEqual
            ZWrite On
            Cull Off
            Blend Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "ProBuilderCG_URP.cginc"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = UnityObjectToClipPosWithOffset(input.positionOS.xyz);
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                clip(input.color.a - 0.75);
                return input.color;
            }
            ENDHLSL
        }
    }

    Fallback "Hidden/ProBuilder/EdgePicker"
}

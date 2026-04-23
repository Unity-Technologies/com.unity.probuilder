Shader "Hidden/ProBuilder/EdgePickerURP"
{
    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.universal": "17.0"
        }

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
            Tags { "LightMode"="ProBuilderPickerA" }
            ZTest Less
            ZWrite On
            Cull Off
            Blend Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "ProBuilderCG_URP.hlsl"

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
    
    // Fallback to built-in renderer version
    Fallback "Hidden/ProBuilder/EdgePicker"
}

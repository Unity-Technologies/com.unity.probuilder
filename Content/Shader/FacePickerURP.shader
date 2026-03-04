Shader "Hidden/ProBuilder/FacePickerURP"
{
    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.universal": "17.0"
        }

        Tags
        {
            "ProBuilderPicker"="Base"
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
            "DisableBatching"="True"
        }

        Pass
        {
            Name "Base"
            Tags { "LightMode"="ProBuilderPickerA" }
            ZTest LEqual
            ZWrite On
            Cull Back
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

                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
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
    
    // Fallback to built-in renderer version
    Fallback "Hidden/ProBuilder/VertexPicker"
}

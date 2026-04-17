Shader "Hidden/ProBuilder/SelectionPickerURP"
{
    Properties {}

    SubShader
    {
        Tags { "ProBuilderPicker"="EdgePass" }
        Lighting Off
        ZTest LEqual
        ZWrite On
        Cull Off
        Blend Off

        UsePass "Hidden/ProBuilder/EdgePickerURP/EDGES"
    }

    SubShader
    {
        Tags { "ProBuilderPicker"="VertexPass" }
        Lighting Off
        ZTest LEqual
        ZWrite On
        Cull Off
        Blend Off

        UsePass "Hidden/ProBuilder/VertexPickerURP/VERTICES"
    }

    SubShader
    {
        Tags { "ProBuilderPicker"="Base" }
        Lighting Off
        ZTest LEqual
        ZWrite On
        Cull Back
        Blend Off

        UsePass "Hidden/ProBuilder/FacePickerURP/BASE"
    }
}

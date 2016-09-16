Shader "Hidden/ProBuilder/SelectionPicker"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Scale("Scale", Range(1,7)) = 3.3
	}

	SubShader
	{
		Tags { "ProBuilderPicker"="VertexPass" "RenderType"="Transparent" }
		Lighting Off
		ZTest LEqual
		ZWrite On
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha		

		UsePass "Hidden/ProBuilder/VertexPicker/VERTICES"
	}

	SubShader
	{
		Tags { "ProBuilderPicker"="Base" }
		Lighting Off
		ZTest LEqual
		ZWrite On
		Cull Back
		Blend Off

		UsePass "Hidden/ProBuilder/FacePicker/BASE"
	}
}

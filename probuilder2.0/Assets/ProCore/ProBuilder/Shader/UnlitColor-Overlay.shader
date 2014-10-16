Shader "Hidden/ProBuilder/UnlitColor-Overlay" 
{
	Properties
	{
		_Color ("Color Tint", Color) = (1,1,1,1)   
		_MainTex ("Base (RGB) Alpha (A)", 2D) = "white"
	}

	Category
	{
		Lighting Off
		ZWrite On 
		ZTest Always
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		AlphaTest Greater 0.001
//		Tags {"Queue"="Transparent"}
		Tags {"Queue"="Overlay"}

		SubShader
		{
			Pass
			{
				SetTexture [_MainTex]
				{
					ConstantColor [_Color]
					Combine Texture * constant
				}
			}
		}
	}
}
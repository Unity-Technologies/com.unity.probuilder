Shader "Hidden/ProBuilder/UnlitColor" 
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
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		Offset -1, -1
		Fog { Mode Off }
		Tags {"Queue"="Transparent+1" }

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
Shader "Hidden/ProBuilder/UnlitEdgeOffset" 
{
	Properties
	{
		_Color ("Color Tint", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags { "LightMode"="Always" "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Lighting Off
		ZTest LEqual
		ZWrite On
		Offset -1, -1
		Cull Back

		Pass
		{
			AlphaTest Greater 1 //.25

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 _Color;

			struct appdata
			{
				float4 vertex : POSITION;
			//	float4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
			//	float4 color : COLOR;
			};

			v2f vert (appdata v)
			{
				v2f o;

				/// so simple, but oh so effective
				/// https://www.opengl.org/discussion_boards/showthread.php/166719-Clean-Wireframe-Over-Solid-Mesh
				o.pos = mul(UNITY_MATRIX_MV, v.vertex);
				o.pos.xyz *= .99;
				o.pos = mul(UNITY_MATRIX_P, o.pos);

				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				return _Color;
			}

			ENDCG
		}
	}
}
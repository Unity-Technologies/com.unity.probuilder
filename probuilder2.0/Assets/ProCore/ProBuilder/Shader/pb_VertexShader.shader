Shader "Hidden/ProBuilder/pb_VertexShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Scale("Scale", float) = 3
		_Color ("Color Tint", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags { "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="Overlay" }

		// Want to depth test here, using the single point as reference instead of 
		// the four corners of the sprite
		Pass
		{
			Lighting Off
			ZWrite On
			Colormask 0
		}

		Lighting Off
		ZTest LEqual
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass 
		{
			AlphaTest Greater .25

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _Color;
			float _Scale;

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			v2f vert (appdata v)
			{
				v2f o;

				o.pos = mul(UNITY_MATRIX_MV, v.vertex);
				o.pos *= .98;
				o.pos = mul(UNITY_MATRIX_P, o.pos);

				// convert vertex to screen space, add pixel-unit xy to vertex, then transform back to clip space.

				float4 clip = o.pos;

				clip.xy /= clip.w;
				clip.xy = clip.xy * .5 + .5;
				clip.xy *= _ScreenParams.xy;

				clip.xy += v.texcoord1.xy * _Scale;

				clip.xy /= _ScreenParams.xy;
				clip.xy = (clip.xy - .5) / .5;
				clip.xy *= clip.w;

				o.pos = clip;

				o.uv = v.texcoord.xy;
				o.color = v.color;
				o.color.a = 1;

				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				return _Color;
//				return tex2D(_MainTex, i.uv) * i.color;
			}

			ENDCG
		}
	}
}
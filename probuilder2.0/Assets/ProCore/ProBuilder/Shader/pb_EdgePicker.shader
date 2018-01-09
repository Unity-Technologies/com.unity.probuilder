Shader "Hidden/ProBuilder/EdgePicker"
{
	Properties {}

	SubShader
	{
		Tags
		{
			"ProBuilderPicker"="EdgePass"
			"RenderType"="Transparent"
			"IgnoreProjector"="True"
			"DisableBatching"="True"
		}

		Lighting Off
		ZTest LEqual
		ZWrite On
		Cull Off
		Blend Off
		Offset -1, -1

		Pass
		{
			Name "Edges"
			AlphaTest Greater .25

CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color : COLOR;
			};

			v2f vert (appdata v)
			{
				v2f o;

				#if UNITY_VERSION > 550
				o.pos = float4(UnityObjectToViewPos(v.vertex.xyz), 1);
				#else
				o.pos = mul(UNITY_MATRIX_MV, v.vertex);
				#endif
				o.pos.xyz *= .95;
				o.pos = mul(UNITY_MATRIX_P, o.pos);

				// convert vertex to screen space, add pixel-unit xy to vertex, then transform back to clip space.
				float4 clip = o.pos;

				clip.xy /= clip.w;
				clip.xy = clip.xy * .5 + .5;
				clip.xy *= _ScreenParams.xy;

				clip.z -= .0001 * (1 - UNITY_MATRIX_P[3][3]);

				clip.xy /= _ScreenParams.xy;
				clip.xy = (clip.xy - .5) / .5;
				clip.xy *= clip.w;

				o.pos = clip;
				o.color = v.color;

				return o;
			}

			float4 frag (v2f i) : COLOR
			{
				return i.color;
			}

ENDCG
		}
	}
}

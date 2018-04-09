Shader "Hidden/ProBuilder/LineBillboard"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Scale ("Scale", Range(0, 20)) = 7
	}

	SubShader
	{
		Tags
		{
			"ProBuilderPicker"="EdgePass"
			"RenderType"="Geometry"
			"Queue"="Geometry"
			"DisableBatching"="True"
			"ForceNoShadowCasting"="True"
			"IgnoreProjector"="True"
		}

		Lighting Off
		ZTest LEqual
		ZWrite On
		Cull Off
		Blend Off
		Offset -1, -1

		Pass
		{
			Name "EdgePass"

			CGPROGRAM
			#pragma target 4.0
			#pragma vertex vert
			#pragma geometry geo
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
			};

			// Is the camera in orthographic mode? (1 yes, 0 no)
			#define ORTHO (1 - UNITY_MATRIX_P[3][3])

			// How far to pull vertices towards camera in orthographic mode
			const float ORTHO_CAM_OFFSET = .0001;
			float _Scale;
			float4 _Color;

			float4 ClipToScreen(float4 v)
			{
				v.xy /= v.w;
				v.xy = v.xy * .5 + .5;
				v.xy *= _ScreenParams.xy;
				return v;
			}

			float4 ScreenToClip(float4 v)
			{
				v.z -= ORTHO_CAM_OFFSET * ORTHO;
				v.xy /= _ScreenParams.xy;
				v.xy = (v.xy - .5) / .5;
				v.xy *= v.w;
				return v;
			}

			v2f vert (appdata v)
			{
				v2f o;

#if UNITY_VERSION > 550
				o.pos = float4(UnityObjectToViewPos(v.vertex.xyz), 1);
#else
				o.pos = mul(UNITY_MATRIX_MV, v.vertex);
#endif
				// pull closer to camera and into clip space
				o.pos *= .99;
				o.pos = mul(UNITY_MATRIX_P, o.pos);
				// convert clip -> ndc -> screen, build billboards in geo shader, then screen -> ndc -> clip
				o.pos = ClipToScreen(o.pos);
				o.color = v.color;
				return o;
			}

			[maxvertexcount(4)]
			void geo(line v2f p[2], inout TriangleStream<v2f> triStream)
			{
				float2 perp = normalize(float2(-(p[1].pos.y - p[0].pos.y), p[1].pos.x - p[0].pos.x)) * _Scale;

				v2f geo_out;

				geo_out.pos = ScreenToClip( float4(p[1].pos.x + perp.x, p[1].pos.y + perp.y, p[1].pos.z, p[1].pos.w) );
				geo_out.color = p[1].color;
				triStream.Append(geo_out);

				geo_out.pos =  ScreenToClip( float4(p[1].pos.x - perp.x, p[1].pos.y - perp.y, p[1].pos.z, p[1].pos.w) );
				geo_out.color = p[1].color;
				triStream.Append(geo_out);

				geo_out.pos =  ScreenToClip( float4(p[0].pos.x + perp.x, p[0].pos.y + perp.y, p[0].pos.z, p[0].pos.w) );
				geo_out.color = p[0].color;
				triStream.Append(geo_out);

				geo_out.pos =  ScreenToClip( float4(p[0].pos.x - perp.x, p[0].pos.y - perp.y, p[0].pos.z, p[0].pos.w) );
				geo_out.color = p[0].color;
				triStream.Append(geo_out);
			}

			fixed4 frag (v2f i) : COLOR
			{
				return i.color * _Color;
			}

			ENDCG
		}
	}
}

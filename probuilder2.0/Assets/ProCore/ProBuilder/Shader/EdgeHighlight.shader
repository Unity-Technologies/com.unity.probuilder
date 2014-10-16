Shader "Hidden/ProBuilder/Edge Highlight"
{
	Properties
	{
//		_Color ("Color", Color) = (1,1,1,1)
		_Width("Width", float) = 2
		_MaxWidth("MaxWidth", float) = .1
	}

	SubShader
	{
		Tags { "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="Overlay" }
		Lighting Off
		ZTest LEqual
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha
	//	Offset -1, -1

		Pass 
		{
			AlphaTest Greater .25

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

	//		float4 _Color;
			float _Width;
			float _MaxWidth;

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float4 tangent : TANGENT;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			// http://answers.unity3d.com/questions/729881/vertex-position-to-screen-space-and-back-again.html
			float metersPerPixel(float4 v)
			{
				float4x4 projectionMatrix = UNITY_MATRIX_P;
				float d = projectionMatrix[1][1];
				float distanceFromCameraToVertex = mul( UNITY_MATRIX_MV, v ).z;
				//The check here is for wether the camera is orthographic or perspective
				float frustumHeight = projectionMatrix[3][3] == 1 ? 2/d : 2.0*-distanceFromCameraToVertex*(1/d);
				return frustumHeight/_ScreenParams.y;
			}

			/// https://www.mapbox.com/blog/drawing-antialiased-lines/
			v2f vert (appdata v)
			{
				v2f o;

				float4 p0 = mul(UNITY_MATRIX_MVP, v.vertex);	// first vertex
				float4 p1 = mul(UNITY_MATRIX_MVP, float4(v.tangent.xyz, 1.) );

				float4 delta = normalize(p1-p0);
				delta.z = 0.;
				delta.w = 0.;

				float y = delta.y;
				delta.y = -delta.x;
				delta.x = y;

				float size = ((_ScreenParams.z * _ScreenParams.x) * (_Width * 0.001) ) * metersPerPixel(v.vertex);
					size = min( _MaxWidth, size );
				delta *= size;

				// w > 0 means odd, else even
				if( v.tangent.w < 0.5 )
					p0 += delta;
				else
					p0 -= delta;
			
				o.pos = p0;
				o.uv = v.texcoord.xy;
				o.color = v.color;

				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				return i.color;
			}

			ENDCG
		}
	}
}
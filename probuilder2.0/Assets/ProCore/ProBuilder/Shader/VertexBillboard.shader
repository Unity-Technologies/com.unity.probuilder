Shader "Hidden/ProBuilder/VertexBillboard"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Scale("Scale", float) = .02
	}

	SubShader
	{
		Tags { "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="Overlay" }
		Lighting Off
		ZTest Always //LEqual
		ZWrite On

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
				float4 tangent : TANGENT;
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

				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

				float dist = distance(_WorldSpaceCameraPos, mul(_Object2World, v.vertex));

				// prevent oblong shapes
				float2 screenScale = float2( max(_ScreenParams.y/_ScreenParams.x, 1), max(_ScreenParams.x/_ScreenParams.y, 1) );

				// dist from camera - 0, 50
				// appropriate values - .04, .08
				float scale = v.tangent.x;

				if(UNITY_MATRIX_P[3].w < 1)	// If perspective
					o.pos.xy += v.texcoord1.xy * scale * screenScale;
				else
					o.pos.xy += v.texcoord1.xy * (clamp(.08 - (dist/50) * .04, .04, .08)) * _ScreenParams.zw * screenScale;

				o.uv = v.texcoord.xy;
				o.color = v.color;
				o.color.a = 1;

				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				return tex2D(_MainTex, i.uv) * i.color;
			}

			ENDCG
		}
	}
}
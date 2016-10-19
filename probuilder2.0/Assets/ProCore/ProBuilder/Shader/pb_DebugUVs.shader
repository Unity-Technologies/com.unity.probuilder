Shader "Unlit/pb_DebugUVs"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_UVChannel ("UV Channel (0-3)", Int) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float2 uv3 : TEXCOORD3;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;

				UNITY_FOG_COORDS(1)

				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			int _UVChannel;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

				uint c = ((uint)_UVChannel) % 4;
				float2 uv = c == 0 ? v.uv : (c == 1) ? v.uv1 : (c == 2) ? v.uv2 : v.uv3;

				o.uv = TRANSFORM_TEX(uv, _MainTex);

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}

Shader "Hidden/ProBuilder/SmoothingPreview"
{
	Properties { }

	SubShader
	{
		Tags { "IgnoreProjector"="True" "RenderType"="Geometry" }
		Lighting Off
		ZTest LEqual
		ZWrite On
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			AlphaTest Greater .25

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 _Color;

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

				/// https://www.opengl.org/discussion_boards/showthread.php/166719-Clean-Wireframe-Over-Solid-Mesh
				#if UNITY_VERSION > 550
				o.pos = float4(UnityObjectToViewPos(v.vertex.xyz), 1);
				#else
				o.pos = mul(UNITY_MATRIX_MV, v.vertex);
				#endif
				o.pos.xyz *= .99;
				o.pos = mul(UNITY_MATRIX_P, o.pos);
                o.color = v.color;

				return o;
			}

			half4 frag (v2f i) : COLOR
			{
                i.pos.xy = floor(i.pos.xy * 1) * .5;
                float checker = -frac(i.pos.x + i.pos.y);
                clip(checker);

				return i.color;
			}

			ENDCG
		}
	}
}

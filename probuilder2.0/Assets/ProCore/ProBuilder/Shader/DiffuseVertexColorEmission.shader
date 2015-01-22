Shader "ProBuilder/Diffuse Vertex Color Emission" {
  Properties {
    _MainTex ("Texture", 2D) = "gray" {}
  }
  SubShader {
    Tags { "RenderType" = "Opaque" }

    CGPROGRAM
    #pragma surface surf Lambert

    sampler2D _MainTex;

    struct Input {
        float4 color : COLOR; // interpolated vertex color
        float2 uv_MainTex;
    };

    void surf (Input IN, inout SurfaceOutput o) {
        fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
        o.Albedo = c.rgb;
        o.Emission = c.a;
        o.Alpha = c.a; // Alpha used to control glow effect
    }
    ENDCG
  }
  Fallback "Diffuse"
}

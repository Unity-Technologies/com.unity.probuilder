Shader "ProBuilder/Diffuse Vertex Color Transparency" {
  Properties {
    _MainTex ("Texture", 2D) = "white" {}
  }
  SubShader {
    Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}

    //ZWrite Off // on might hide behind pixels, off might miss order
    Blend SrcAlpha OneMinusSrcAlpha
    ColorMask RGB

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
        o.Alpha = c.a;
    }
    ENDCG
  }
  Fallback "Transparent/Diffuse"
}

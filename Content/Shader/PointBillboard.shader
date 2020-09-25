Shader "Hidden/ProBuilder/PointBillboard"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Scale ("Scale", Range(0, 20)) = 7
        _HandleZTest ("_HandleZTest", Int) = 8
        _HandleZWrite("_HandleZWrite", Int) = 0
    }

    SubShader
    {
        Tags
        {
            "ProBuilderPicker"="VertexPass"
            "RenderType"="Geometry"
            "Queue"="Geometry"
            "IgnoreProjector"="True"
            "ForceNoShadowCasting"="True"
            "DisableBatching"="True"
        }

        Pass
        {
            Name "VertexPass"
            Lighting Off
            ZTest [_HandleZTest]
            ZWrite [_HandleZWrite]
            Cull Off
            Blend Off
            Offset 0, -2

            CGPROGRAM
                #pragma target 4.0
                #pragma vertex vert
                #pragma fragment frag
                #pragma geometry geo
                #pragma exclude_renderers metal
                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float4 color : COLOR;
                };

                struct GS_INPUT
                {
                    float4 pos : POSITION;
                    float4 color : COLOR;
                };

                struct FS_INPUT
                {
                    float4 pos : POSITION;
                    float4 color : COLOR;
                };

                float _Scale;
                float4 _Color;

                // Is the camera in orthographic mode? (1 yes, 0 no)
                #define ORTHO (1 - UNITY_MATRIX_P[3][3])

                #define ScreenDepthModifier (lerp(.00001, 0, ORTHO))

                float4 ClipToScreen(float4 v)
                {
                    v.xy /= v.w;
                    v.xy = v.xy * .5 + .5;
                    v.xy *= _ScreenParams.xy;
                    return v;
                }

                float4 ScreenToClip(float4 v)
                {
                    v.xy /= _ScreenParams.xy;
                    v.xy = (v.xy - .5) / .5;
                    v.xy *= v.w;
                    return v;
                }

                GS_INPUT vert(appdata v)
                {
                    GS_INPUT output = (GS_INPUT)0;

                    output.pos = float4(UnityObjectToViewPos(v.vertex.xyz), 1);
                    // perspective rendering applies offset in view coordinates, and orthographic applies offset in the geometry
                    // shader screen space
                    output.pos.xyz *= lerp(1, .95, ORTHO);
                    output.pos = mul(UNITY_MATRIX_P, output.pos);
                    // convert clip -> ndc -> screen, build billboards in geo shader, then screen -> ndc -> clip
                    output.pos = ClipToScreen(output.pos);
                    output.color = v.color;

                    return output;
                }

                [maxvertexcount(4)]
                void geo(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
                {
                    // 3  1
                    // 2  0

                    FS_INPUT geo_out;
                    geo_out.color = p[0].color;

                    geo_out.pos = ScreenToClip( float4(p[0].pos.x + _Scale, p[0].pos.y - _Scale, p[0].pos.z, p[0].pos.w - ScreenDepthModifier) );
                    triStream.Append(geo_out);

                    geo_out.pos =  ScreenToClip( float4(p[0].pos.x + _Scale, p[0].pos.y + _Scale, p[0].pos.z, p[0].pos.w - ScreenDepthModifier) );
                    triStream.Append(geo_out);

                    geo_out.pos =  ScreenToClip( float4(p[0].pos.x - _Scale, p[0].pos.y - _Scale, p[0].pos.z, p[0].pos.w - ScreenDepthModifier) );
                    triStream.Append(geo_out);

                    geo_out.pos =  ScreenToClip( float4(p[0].pos.x - _Scale, p[0].pos.y + _Scale, p[0].pos.z, p[0].pos.w - ScreenDepthModifier) );
                    triStream.Append(geo_out);
                }

                float4 frag(FS_INPUT input) : COLOR
                {
                    return _Color * input.color;
                }

            ENDCG
        }
    }

    Fallback Off
}

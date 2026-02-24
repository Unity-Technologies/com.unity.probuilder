// ProBuilderCG_URP.cginc - URP-compatible version of ProBuilderCG.cginc
// Must be included AFTER: #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
// so that TransformObjectToWorld, TransformWorldToView, UNITY_MATRIX_P, unity_OrthoParams are available.

// Is the camera in orthographic mode? (1 yes, 0 no)
#define ORTHO (1 - UNITY_MATRIX_P[3][3])

// How far to pull vertices towards camera in orthographic mode
const float ORTHO_CAM_OFFSET = .0001;

inline float4 ClipToScreen(float4 v)
{
    v.xy /= v.w;
    v.xy = v.xy * .5 + .5;
    v.xy *= _ScreenParams.xy;
    return v;
}

inline float4 ScreenToClip(float4 v)
{
    v.z -= ORTHO_CAM_OFFSET * ORTHO;
    v.xy /= _ScreenParams.xy;
    v.xy = (v.xy - .5) / .5;
    v.xy *= v.w;
    return v;
}

inline float4 UnityObjectToClipPosWithOffset(float3 positionOS)
{
    VertexPositionInputs positions = GetVertexPositionInputs(positionOS);

    float4 ret = float4(positions.positionVS, 1);
    //Offsetting the edges to avoid z-fighting problems
    //Do not offset when using orthographic camera as XY are
    //screen coords, this would shift the rendering
    ret.xyz *= lerp(0.99, 0.95, ORTHO);
    //Moving edges closer
    //.99 is not sufficient for Orthographic Camera
    ret.w *= lerp(1, 0.95, unity_OrthoParams.w);
    return mul(UNITY_MATRIX_P, ret);
}

inline float4 UnityObjectToClipPosWithOffsetMetal(float3 positionOS)
{
    VertexPositionInputs positions = GetVertexPositionInputs(positionOS);

    float4 ret = float4(positions.positionVS, 1);
    ret *= lerp(.99, .95, ORTHO);
    return mul(UNITY_MATRIX_P, ret);
}

inline float4 GetPickerColor(float4 pos, float2 texcoord1)
{
    // convert vertex to screen space, add pixel-unit xy to vertex, then transform back to clip space.
    float4 clip = pos;

    clip.xy /= clip.w;
    clip.xy = clip.xy * .5 + .5;
    clip.xy *= _ScreenParams.xy;

    clip.xy += texcoord1.xy * 3.5;
    clip.z -= .0001 * (1 - UNITY_MATRIX_P[3][3]);

    clip.xy /= _ScreenParams.xy;
    clip.xy = (clip.xy - .5) / .5;
    clip.xy *= clip.w;

    return clip;
}


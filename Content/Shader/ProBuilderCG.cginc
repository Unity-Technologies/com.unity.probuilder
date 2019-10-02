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

inline float4 UnityObjectToClipPosWithOffset(float3 pos)
{
    float4 ret = float4(UnityObjectToViewPos(pos), 1);
    ret *= lerp(.99, .95, ORTHO);
    return mul(UNITY_MATRIX_P, ret);
}

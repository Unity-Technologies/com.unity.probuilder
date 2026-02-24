#if PB_URP_MODE
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.ProBuilder
{
    class CustomPassFeature : ScriptableRendererFeature
    {
        URPSelectionPickerPass customPass;
        public LayerMask m_LayerMask;


        public override void Create()
        {
            customPass = new URPSelectionPickerPass(m_LayerMask);
            customPass.renderPassEvent = RenderPassEvent.AfterRendering;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(customPass);
        }
    }
}
#endif

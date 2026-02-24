using UnityEditor;
using UnityEngine.Rendering;
using UObject = UnityEngine.Object;
using static UnityEngine.Rendering.RenderPipeline;

#if URP_7_1_0_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif

namespace UnityEngine.ProBuilder
{
    internal partial class SelectionPickerRenderer
    {

        internal class SelectionPickerRendererURP : ISelectionPickerRenderer
        {
            public static Camera temporaryCamera;

            /// <summary>
            /// Render the camera with a replacement shader and return the resulting image.
            /// RenderTexture is always initialized with no gamma conversion (RenderTextureReadWrite.Linear)
            /// </summary>
            /// <param name="camera"></param>
            /// <param name="shader"></param>
            /// <param name="tag"></param>
            /// <param name="width"></param>
            /// <param name="height"></param>
            /// <returns></returns>
            public Texture2D RenderLookupTexture(
                Camera camera,
                Shader shader,
                string tag,
                int width = -1,
                int height = -1)
            {
#if URP_7_1_0_OR_NEWER
                bool autoSize = width < 0 || height < 0;

                int _width = autoSize ? (int)camera.pixelRect.width : width;
                int _height = autoSize ? (int)camera.pixelRect.height : height;

                GameObject go = new GameObject();
                Camera renderCam = go.AddComponent<Camera>();
                temporaryCamera = renderCam;
                renderCam.CopyFrom(camera);

                renderCam.renderingPath = RenderingPath.Forward;
                renderCam.enabled = false;
                renderCam.clearFlags = CameraClearFlags.SolidColor;
                renderCam.backgroundColor = Color.white;
                renderCam.allowHDR = false;
                renderCam.allowMSAA = false;
                renderCam.forceIntoRenderTexture = true;

                renderCam.GetUniversalAdditionalCameraData();

                RenderTextureDescriptor descriptor = new RenderTextureDescriptor()
                {
                    width = _width,
                    height = _height,
                    colorFormat = renderTextureFormat,
                    autoGenerateMips = false,
                    depthBufferBits = 16,
                    dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
                    enableRandomWrite = false,
                    memoryless = RenderTextureMemoryless.None,
                    sRGB = false,
                    useMipMap = false,
                    volumeDepth = 1,
                    msaaSamples = 1
                };


                RenderTexture rt = RenderTexture.GetTemporary(descriptor);
#if PB_DEBUG
            /* Debug.Log(string.Format("antiAliasing {0}\nautoGenerateMips {1}\ncolorBuffer {2}\ndepth {3}\ndepthBuffer {4}\ndimension {5}\nenableRandomWrite {6}\nformat {7}\nheight {8}\nmemorylessMode {9}\nsRGB {10}\nuseMipMap {11}\nvolumeDepth {12}\nwidth {13}",
                RenderTexture.active.antiAliasing,
                RenderTexture.active.autoGenerateMips,
                RenderTexture.active.colorBuffer,
                RenderTexture.active.depth,
                RenderTexture.active.depthBuffer,
                RenderTexture.active.dimension,
                RenderTexture.active.enableRandomWrite,
                RenderTexture.active.format,
                RenderTexture.active.height,
                RenderTexture.active.memorylessMode,
                RenderTexture.active.sRGB,
                RenderTexture.active.useMipMap,
                RenderTexture.active.volumeDepth,
                RenderTexture.active.width));
                */
#endif
                var request = new StandardRequest()
                {
                    destination = rt
                };
                RenderPipelineManager.beginCameraRendering += CustomRenderPass1;

                if (RenderPipeline.SupportsRenderRequest(renderCam, request) == false)
                    Debug.LogWarning("RenderRequest not supported.");

                RenderPipeline.SubmitRenderRequest<StandardRequest>(renderCam, request);
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = rt;

                Texture2D img = new Texture2D(_width, _height, textureFormat, false, false);
                img.ReadPixels(new Rect(0, 0, _width, _height), 0, 0);
                img.Apply();
                //FlipTexture2DVertical(img);

                SaveTexture(img, "Assets/Tmp/SelectionPickerTexture.png");

                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
                RenderPipelineManager.beginCameraRendering -= CustomRenderPass1;
                temporaryCamera = null;
                UObject.DestroyImmediate(go);

                return img;
#else
                return null;
#endif
            }

            /// <summary>Flips the texture vertically in place (swap rows).</summary>
            static void FlipTexture2DVertical(Texture2D tex)
            {
                int w = tex.width;
                int h = tex.height;
                Color[] pixels = tex.GetPixels();
                for (int y = 0; y < h / 2; y++)
                {
                    int top = y * w;
                    int bot = (h - 1 - y) * w;
                    for (int x = 0; x < w; x++)
                    {
                        var t = pixels[top + x];
                        pixels[top + x] = pixels[bot + x];
                        pixels[bot + x] = t;
                    }
                }
                tex.SetPixels(pixels);
                tex.Apply();
            }

            internal static bool SaveTexture(Texture2D texture, string path)
            {
                byte[] bytes = texture.EncodeToPNG();

                if (string.IsNullOrEmpty(path))
                    return false;

                System.IO.File.WriteAllBytes(path, bytes);
                AssetDatabase.Refresh();
                return true;
            }

            static void CustomRenderPass1(ScriptableRenderContext ctx, Camera camera)
            {
                if (camera != temporaryCamera)
                    return;
                var customPass = new URPSelectionPickerPass(-1);
                customPass.renderPassEvent = RenderPassEvent.AfterRendering;

                camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(customPass);
            }
        }
    }
}

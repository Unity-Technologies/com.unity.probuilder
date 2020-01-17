using UObject = UnityEngine.Object;

namespace UnityEngine.ProBuilder
{
    internal partial class SelectionPickerRenderer
    {
        internal class SelectionPickerRendererStandard: ISelectionPickerRenderer
        {
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
                bool autoSize = width < 0 || height < 0;

                int _width = autoSize ? (int)camera.pixelRect.width : width;
                int _height = autoSize ? (int)camera.pixelRect.height : height;

                GameObject go = new GameObject();
                Camera renderCam = go.AddComponent<Camera>();
                renderCam.CopyFrom(camera);

                renderCam.renderingPath = RenderingPath.Forward;
                renderCam.enabled = false;
                renderCam.clearFlags = CameraClearFlags.SolidColor;
                renderCam.backgroundColor = Color.white;
#if UNITY_5_6_OR_NEWER
                renderCam.allowHDR = false;
                renderCam.allowMSAA = false;
                renderCam.forceIntoRenderTexture = true;
#endif

#if UNITY_2017_1_OR_NEWER
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
#else
            RenderTexture rt = RenderTexture.GetTemporary(
                    _width,
                    _height,
                    16,
                    renderTextureFormat,
                    RenderTextureReadWrite.Linear,
                    1);
#endif

                RenderTexture prev = RenderTexture.active;
                renderCam.targetTexture = rt;
                RenderTexture.active = rt;

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
                renderCam.RenderWithShader(shader, tag);

                Texture2D img = new Texture2D(_width, _height, textureFormat, false, false);
                img.ReadPixels(new Rect(0, 0, _width, _height), 0, 0);
                img.Apply();

                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);

                UObject.DestroyImmediate(go);

                return img;
            }
        }
    }
}

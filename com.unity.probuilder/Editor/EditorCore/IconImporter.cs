using System;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Asset post processor for ProBuilder icons.
    /// </summary>
    sealed class IconImporter : AssetPostprocessor
    {
        /// <summary>
        /// Automatically set the importer settings for ProBuilder icons.
        /// </summary>
        public void OnPreprocessTexture()
        {
            // don't try to write to upm dir
            if (!assetPath.StartsWith("Assets"))
                return;

            if (assetPath.IndexOf("ProBuilder/Icons", StringComparison.Ordinal) < 0 &&
                assetPath.IndexOf("ProBuilder/About/Images", StringComparison.Ordinal) < 0)
                return;

            TextureImporter ti = (TextureImporter)assetImporter;

            ti.textureType = TextureImporterType.Default;
            ti.sRGBTexture = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.alphaSource = TextureImporterAlphaSource.FromInput;//.FromGrayScale;
            ti.crunchedCompression = false;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.filterMode = FilterMode.Point;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.mipmapEnabled = false;

            // ti.maxTextureSize = 64;
        }
    }
}

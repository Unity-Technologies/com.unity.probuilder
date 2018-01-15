using System;
using UnityEngine;
using UnityEditor;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Asset post processor for ProBuilder icons.
	/// </summary>
	class pb_ImportIcons : AssetPostprocessor
	{
		/// <summary>
		/// Automatically set the importer settings for ProBuilder icons.
		/// </summary>
		public void OnPreprocessTexture()
		{
			// don't try to write to upm dir
			if (!assetPath.StartsWith("Assets"))
				return;

			if( assetPath.IndexOf("ProBuilder/Icons", StringComparison.Ordinal) < 0 &&
				assetPath.IndexOf("ProBuilder/About/Images", StringComparison.Ordinal) < 0)
				return;

			TextureImporter ti = (TextureImporter) assetImporter;

#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
			ti.textureType = TextureImporterType.Advanced;
			ti.textureFormat = TextureImporterFormat.AutomaticTruecolor;
			ti.linearTexture = true;
#elif UNITY_5_5
			ti.textureType = TextureImporterType.Default;
			ti.sRGBTexture = false;
			ti.textureCompression = TextureImporterCompression.Uncompressed;
#else
			ti.textureType = TextureImporterType.Default;
			ti.sRGBTexture = false;
			ti.textureCompression = TextureImporterCompression.Uncompressed;
			ti.alphaSource = TextureImporterAlphaSource.FromInput;//.FromGrayScale;
			ti.crunchedCompression = false;
#endif
			ti.npotScale = TextureImporterNPOTScale.None;
			ti.filterMode = FilterMode.Point;
			ti.wrapMode = TextureWrapMode.Clamp;
			ti.mipmapEnabled = false;

			// ti.maxTextureSize = 64;
		}
	}
}

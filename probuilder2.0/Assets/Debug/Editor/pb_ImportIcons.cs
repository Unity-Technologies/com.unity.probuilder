using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ProBuilder2.EditorCommon
{
	public class pb_ImportIcons
	{
		public static void ApplyImportSettings(Texture2D icon)
		{
			string path = AssetDatabase.GetAssetPath(icon);
			ApplyImportSettings(path);
		}

		public static void ApplyImportSettings(string path)
		{
			TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

			if(ti == null)
				return;

			ti.textureType = TextureImporterType.Advanced;
			ti.npotScale = TextureImporterNPOTScale.None;
			ti.filterMode = FilterMode.Point;
			ti.wrapMode = TextureWrapMode.Clamp;
			ti.mipmapEnabled = false;
			ti.maxTextureSize = 64;
			ti.textureFormat = TextureImporterFormat.RGBA16;

			ti.SaveAndReimport();
		}
	}
}

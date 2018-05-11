using UnityEditor;
using UnityEngine;

namespace UnityEditor.ProBuilder.DebugUtilities
{
	static class GenerateDesaturatedImage
	{
		[MenuItem("Tools/Debug/ProBuilder/Create Desaturated Icons")]
		static void Init()
		{
			foreach (var o in Selection.objects)
			{
				Texture2D texture = o as Texture2D;

				if(texture == null || string.IsNullOrEmpty(AssetDatabase.GetAssetPath(texture)))
					continue;

				CreateDesaturedImage(texture);
			}
		}

		public static Texture2D CreateDesaturedImage(Texture2D source)
		{
			string path = AssetDatabase.GetAssetPath(source);
			TextureImporter imp = (TextureImporter) AssetImporter.GetAtPath( path );

			if(!imp)
			{
				UnityEngine.Debug.Log("Couldn't find importer : " + source);
				return null;
			}

			imp.isReadable = true;
			imp.SaveAndReimport();

			Color32[] pixels = source.GetPixels32();

			imp.isReadable = false;
			imp.SaveAndReimport();

			for(int i = 0; i < pixels.Length; i++)
			{
				int gray = (System.Math.Min(pixels[i].r, System.Math.Min(pixels[i].g, pixels[i].b)) + System.Math.Max(pixels[i].r, System.Math.Max(pixels[i].g, pixels[i].b))) / 2;

				pixels[i].r = (byte) gray;
				pixels[i].g = (byte) gray;
				pixels[i].b = (byte) gray;
			}

			Texture2D desaturatedIcon = new Texture2D(source.width, source.height);
			desaturatedIcon.hideFlags = HideFlags.HideAndDontSave;
			desaturatedIcon.SetPixels32(pixels);
			desaturatedIcon.Apply();

			byte[] bytes = desaturatedIcon.EncodeToPNG();
			System.IO.File.WriteAllBytes(path.Replace(".png", "_disabled.png"), bytes);

			return desaturatedIcon;
		}
	}
}
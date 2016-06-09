using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	public class pb_AssetPostProcessor : UnityEditor.AssetModificationProcessor
	{
		public static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
		{
			return AssetDeleteResult.DidNotDelete;
		}
	}
}

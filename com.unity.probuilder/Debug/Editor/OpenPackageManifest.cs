using UnityEditor;

namespace UnityEngine.ProBuilder.Debug.Editor
{
	static class OpenPackageManifest
	{
		[MenuItem("Assets/Debug/Open Package Manifest", false, 0)]
		static void MenuOpenPackageManifest()
		{
			SublimeEditor.Open("Packages/manifest.json");
		}
	}
}

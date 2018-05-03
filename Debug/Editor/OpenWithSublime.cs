using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProBuilder.Debug.Editor
{
	static class SublimeEditor
	{
#if UNITY_EDITOR_OSX
		const string k_SublimePath = "/Users/karlh/bin/subl";
	#else
		const string k_SublimePath = "C:\\Program Files\\Sublime Text 3\\sublime_text.exe";
#endif

		[MenuItem("Assets/Open With Sublime Text", false, 400)]
		static void MenuOpenWithSublime()
		{
			Open(string.Join(" ", Selection.objects.Select(AssetDatabase.GetAssetPath).ToArray()));
		}

		public static void Open(string args)
		{
			System.Diagnostics.Process.Start(k_SublimePath, args);
		}
	}
}
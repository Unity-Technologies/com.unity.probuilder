using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class OpenWithSublime : Editor
{
#if UNITY_EDITOR_OSX
	const string k_SublimePath = "/Users/karlh/bin/subl";
#else
	const string k_SublimePath = "C:\\Program Files\\Sublime Text 3\\sublime_text.exe";
#endif

	[MenuItem("Assets/Open With Sublime Text", false, 400)]
	private static void MenuOpenWithSublime()
	{
#if UNITY_EDITOR_OSX
		System.Diagnostics.Process.Start(k_SublimePath, string.Join(" ", Selection.objects.Select(x => AssetDatabase.GetAssetPath(x)).ToArray()));
#else
		System.Diagnostics.Process.Start(k_SublimePath, string.Join(" ", Selection.objects.Select(x => AssetDatabase.GetAssetPath(x)).ToArray()));
#endif
	}
}

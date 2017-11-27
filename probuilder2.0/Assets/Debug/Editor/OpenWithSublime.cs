using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class OpenWithSublime : Editor
{
	private const string SUBL_PATH = "C:\\Program Files\\Sublime Text 3\\sublime_text.exe";

	[MenuItem("Assets/Open With Sublime Text", false, 400)]
	private static void MenuOpenWithSublime()
	{
		System.Diagnostics.Process.Start(SUBL_PATH, string.Join(" ", Selection.objects.Select(x => AssetDatabase.GetAssetPath(x)).ToArray()));
	}
}

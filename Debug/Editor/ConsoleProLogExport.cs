using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FlyingWormConsole3;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditorInternal;
using UObject = UnityEngine.Object;

public static class ConsoleProExtensions
{
	static readonly GUIContent gc_OpenInTextEditor = new GUIContent("Open In Text Editor", "Open a composite of the selected logs in the preferred text editor.");
	static readonly GUIContent gc_OpenEachInTextEditor = new GUIContent("Open Each In Text Editor", "Open each the selected logs in the preferred text editor.");
	static readonly GUIContent gc_OpenDiffTool = new GUIContent("Open Merge | Diff", "Open selected entries in the user set merge / diff tool.");

	// Keep the reference to m_SelectedEntries list.
	static List<ConsoleProEntry> m_SelectedEntries;

	/// <summary>
	/// ConsolePro invokes this function with a reference to the contextMenu object. Here we can add custom entries.
	/// </summary>
	/// <param name="contextMenu"></param>
	/// <param name="entrySelection"></param>
	[ConsoleProLogContextMenuAttribute]
	public static void ContextMenuCallback(GenericMenu contextMenu, List<ConsoleProEntry> entrySelection)
	{
		// Keep reference to the current selection around so that the callback functions know what to work with.
		m_SelectedEntries = entrySelection;

		contextMenu.AddSeparator("");

		// Open a composite of all the selected entries in a single text file.
		contextMenu.AddItem(gc_OpenInTextEditor, false, OpenInTextEditor);

		// Open each selected entry in it's own text file.
		if (m_SelectedEntries != null && m_SelectedEntries.Count > 1)
			contextMenu.AddItem(gc_OpenEachInTextEditor, false, () =>
			{
				foreach (var x in m_SelectedEntries)
				{
					System.Diagnostics.Process.Start(WriteTempFile(x.logText));
				}
			});
		else
			contextMenu.AddDisabledItem(gc_OpenEachInTextEditor);

		contextMenu.AddSeparator("");

		// Open the user-set merge/diff tool with two of the selected entries.
		if (m_SelectedEntries != null && m_SelectedEntries.Count == 2)
			contextMenu.AddItem(gc_OpenDiffTool, false, OpenDiff);
		else
			contextMenu.AddDisabledItem(gc_OpenDiffTool);
	}

	/// <summary>
	/// Open the selected entries in a merge / diff tool.
	/// </summary>
	static void OpenDiff()
	{
		// The menu item shouldn't allow this in the first place, but if somehow it does
		// better to throw a warning than error.
		if (m_SelectedEntries == null || m_SelectedEntries.Count != 2)
		{
			Debug.LogWarning("Diff requires exactly 2 console entries be selected!");
			return;
		}

		string left = WriteTempFile(m_SelectedEntries[0].logText);
		string right = WriteTempFile(m_SelectedEntries[1].logText);

		string m_DiffApp = EditorPrefs.GetString("kDiffsDefaultApp");

		if (string.IsNullOrEmpty(m_DiffApp))
			Debug.LogWarning("No Diff / Merge tool set in Unity Preferences -> External Tools");
		else
			System.Diagnostics.Process.Start(m_DiffApp, string.Format("{0} {1}", left, right));
	}

	/// <summary>
	/// Write the selected entries to a single text file and open in a text editor.
	/// </summary>
	static void OpenInTextEditor()
	{
		System.Diagnostics.Process.Start(WriteTempFile(string.Join("\n", m_SelectedEntries.Select(x => x.logText).ToArray())));
	}

	/// <summary>
	/// Create a new unique temporary file and return it's path.
	/// </summary>
	/// <param name="contents"></param>
	/// <returns></returns>
	static string WriteTempFile(string contents)
	{
		string m_TempFilePath = string.Format("{0}{1}{2}.txt",
			Directory.GetParent(Application.dataPath),
			Path.DirectorySeparatorChar,
			FileUtil.GetUniqueTempPathInProject());

		File.WriteAllText(m_TempFilePath, contents);

		return m_TempFilePath;
	}

	[ConsoleProOpenSourceFileAttribute]
	public static void StackOpenSourceFileCallback(ConsoleProEntry inEntry, ConsoleProStackEntry inStackEntry, int? inLineNumber)
	{
		StringBuilder sb = new StringBuilder();

		for (int i = inEntry.stackEntries.Count - 1; i > -1; i--)
		{
			var entry = inEntry.stackEntries[i];
			sb.AppendLine(entry.fileName + " " + entry.lineNumber);
		}

		// In some cases the stackEntry is null, but we still have a valid consoleEntry.
		var consoleEntry = inStackEntry != null
			? inStackEntry
			: inEntry.stackEntries != null
				? inEntry.stackEntries.FirstOrDefault()
				: null;

		string filePath = consoleEntry.fileName;
		int lineNum = consoleEntry.lineNumber;

		if (string.IsNullOrEmpty(filePath) || filePath == "None")
			return;

		string originalFilePath = filePath;

		if (inLineNumber.HasValue)
			lineNum = inLineNumber.Value;

		if (filePath.StartsWith("Assets/"))
		{
			var scriptObj = AssetDatabase.LoadAssetAtPath<UObject>(filePath);

			if (scriptObj != null)
			{
				AssetDatabase.OpenAsset(scriptObj, lineNum);
				return;
			}
		}

		if (originalFilePath.StartsWith("["))
			originalFilePath = Regex.Replace(originalFilePath, "^\\[[0-9]*\\:[0-9]*\\:[0-9]*\\][ ]*", "");

		if (File.Exists(originalFilePath))
		{
			originalFilePath = originalFilePath.Replace("/", "" + Path.DirectorySeparatorChar);
			InternalEditorUtility.OpenFileAtLineExternal(originalFilePath, lineNum);
		}
	}
}

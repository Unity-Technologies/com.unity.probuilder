#if CONSOLE_PRO_ENABLED

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FlyingWormConsole3;
using System;
using System.IO;
using System.Linq;
using UnityEditorInternal;

public static class ConsoleProCallbacks
{
	private static GUIContent gc_OpenInTextEditor = new GUIContent("Open In Text Editor", "Open a composite of the selected logs in the preferred text editor.");
	private static GUIContent gc_OpenEachInTextEditor = new GUIContent("Open Each In Text Editor", "Open each the selected logs in the preferred text editor.");
	private static GUIContent gc_OpenDiffTool = new GUIContent("Open Merge | Diff", "Open selected entries in the user set merge / diff tool.");

	// Keep the reference to m_SelectedEntries list.
	private static List<ConsoleProEntry> m_SelectedEntries;

	/**
	 *	ConsolePro invokes this function with a reference to the contextMenu object. Here we can
	 *	add custom entries.
	 */
#if UNITY_2017_3_OR_NEWER
	[ConsoleProLogContextMenuAttribute]
#else
	[ConsoleProContextMenuAttribute]
#endif
	public static void ContextMenuCallback(GenericMenu contextMenu, List<ConsoleProEntry> entrySelection)
	{
		// Keep reference to the current selection around so that the callback functions know what to work with.
		m_SelectedEntries = entrySelection;

		contextMenu.AddSeparator("");

		// Open a composite of all the selected entries in a single text file.
		contextMenu.AddItem(gc_OpenInTextEditor, false, OpenInTextEditor);

		// Open each selected entry in it's own text file.
		if(m_SelectedEntries != null && m_SelectedEntries.Count > 1)
			contextMenu.AddItem(gc_OpenEachInTextEditor, false, () =>
				{
					foreach(var x in m_SelectedEntries) {
						System.Diagnostics.Process.Start(WriteTempFile(x.logText));
					}
			});
		else
			contextMenu.AddDisabledItem(gc_OpenEachInTextEditor);

		contextMenu.AddSeparator("");

		// Open the user-set merge/diff tool with two of the selected entries.
		if(m_SelectedEntries != null && m_SelectedEntries.Count == 2)
			contextMenu.AddItem(gc_OpenDiffTool, false, OpenDiff);
		else
			contextMenu.AddDisabledItem(gc_OpenDiffTool);
	}

	/**
	 *	Open the selected entries in a merge / diff tool.
	 */
	private static void OpenDiff()
	{
		// The menu item shouldn't allow this in the first place, but if somehow it does
		// better to throw a warning than error.
		if(m_SelectedEntries == null || m_SelectedEntries.Count != 2)
		{
			Debug.LogWarning("Diff requires exactly 2 console entries be selected!");
			return;
		}

		string left = WriteTempFile(m_SelectedEntries[0].logText);
		string right = WriteTempFile(m_SelectedEntries[1].logText);

		string m_DiffApp = EditorPrefs.GetString("kDiffsDefaultApp");

		if(string.IsNullOrEmpty(m_DiffApp))
			Debug.LogWarning("No Diff / Merge tool set in Unity Preferences -> External Tools");
		else
			System.Diagnostics.Process.Start(m_DiffApp, string.Format("{0} {1}", left, right));
	}

	/**
	 *	Write the selected entries to a single text file and open in a text editor.
	 */
	private static void OpenInTextEditor()
	{
		System.Diagnostics.Process.Start( WriteTempFile(string.Join("\n", m_SelectedEntries.Select(x => x.logText).ToArray())) );
	}

	/**
	 *	Create a new unique temporary file and return it's path.
	 */
	private static string WriteTempFile(string contents)
	{
		string m_TempFilePath = string.Format("{0}{1}{2}.txt",
			Directory.GetParent(Application.dataPath),
			Path.DirectorySeparatorChar,
			FileUtil.GetUniqueTempPathInProject());

		File.WriteAllText(m_TempFilePath, contents);

		return m_TempFilePath;
	}
}
#endif
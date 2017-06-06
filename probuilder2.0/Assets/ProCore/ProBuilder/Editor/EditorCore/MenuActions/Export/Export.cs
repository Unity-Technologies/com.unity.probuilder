using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Linq;
using System.Collections.Generic;
using Parabox.STL;

namespace ProBuilder2.Actions
{
	public class Export : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Object; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return false; } }

		// Options for each export format
		private bool m_ExportRecursive;
		// obj specific
		private bool m_ObjExportRightHanded;
		private bool m_ObjExportAsGroup;
		private bool m_ObjExportCopyTextures;

		public enum ExportFormat
		{
			StlAscii,
			StlBinary,
			Obj,
			Asset
		}

		private ExportFormat m_ExportFormat = ExportFormat.Obj;

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Export",
			"Export the selected ProBuilder objects as a model file."
		);

		public Export()
		{
			m_ExportFormat = (ExportFormat) pb_Preferences_Internal.GetInt("pbDefaultExportFormat", (int) ExportFormat.Obj);

			// Recursively select meshes in selection.
			m_ExportRecursive = pb_Preferences_Internal.GetBool("pbExportRecursive", false);

			// obj options
			m_ObjExportRightHanded = pb_Preferences_Internal.GetBool("pbObjExportRightHanded", true);
			m_ObjExportAsGroup = pb_Preferences_Internal.GetBool("pbObjExportAsGroup", true);
			m_ObjExportCopyTextures = pb_Preferences_Internal.GetBool("pbObjExportCopyTextures", true);
		}

		public override bool IsHidden() { return false; }

		public override bool IsEnabled()
		{
			return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Export Settings", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();
			m_ExportFormat = (ExportFormat) EditorGUILayout.EnumPopup("Format", m_ExportFormat);
			if(EditorGUI.EndChangeCheck())
				pb_Preferences_Internal.SetInt("pbDefaultExportFormat", (int) m_ExportFormat);

			m_ExportRecursive = EditorGUILayout.Toggle("Recursive", m_ExportRecursive);

			if(m_ExportFormat == ExportFormat.Obj)
				ObjExportOptions();

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Export"))
				DoAction();
		}

		private void ObjExportOptions()
		{
			EditorGUI.BeginChangeCheck();

			m_ObjExportRightHanded = EditorGUILayout.Toggle("Right Handed", m_ObjExportRightHanded);
			m_ObjExportAsGroup = EditorGUILayout.Toggle("Export As Group", m_ObjExportAsGroup);
			m_ObjExportCopyTextures = EditorGUILayout.Toggle("Copy Textures", m_ObjExportCopyTextures);

			if(EditorGUI.EndChangeCheck())
			{
				pb_Preferences_Internal.SetBool("pbObjExportRightHanded", m_ObjExportRightHanded);
				pb_Preferences_Internal.SetBool("pbObjExportAsGroup", m_ObjExportAsGroup);
				pb_Preferences_Internal.SetBool("pbObjExportCopyTextures", m_ObjExportCopyTextures);
			}
		}

		public override pb_ActionResult DoAction()
		{
			string res = null;

			IEnumerable<pb_Object> meshes = m_ExportRecursive ? pb_Selection.All() : pb_Selection.Top();

			if(m_ExportFormat == ExportFormat.Obj)
				res = ExportObj.ExportWithFileDialog(meshes);
			else if(m_ExportFormat == ExportFormat.StlAscii)
				res = ExportStlAscii.ExportWithFileDialog(meshes.Select(x => x.gameObject).ToArray(), FileType.Ascii);
			else if(m_ExportFormat == ExportFormat.StlBinary)
				res = ExportStlAscii.ExportWithFileDialog(meshes.Select(x => x.gameObject).ToArray(), FileType.Binary);
			else if(m_ExportFormat == ExportFormat.Asset)
				res = ExportAsset.MakeAsset(meshes);

			if( string.IsNullOrEmpty(res) )
			{
				return new pb_ActionResult(Status.Canceled, "User Canceled");
			}
			else
			{
				if(res.Contains(Application.dataPath))
					AssetDatabase.Refresh();

				return new pb_ActionResult(Status.Success, "Export " + m_ExportFormat);
			}
		}
	}
}

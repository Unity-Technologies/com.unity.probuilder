using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Linq;
using Parabox.STL;

namespace ProBuilder2.Actions
{
	public class Export : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Object; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return false; } }

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

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Export"))
				DoAction();
		}

		private void ObjExportOptions()
		{

		}

		public override pb_ActionResult DoAction()
		{
			string res = null;

			if(m_ExportFormat == ExportFormat.Obj)
				res = ExportObj.ExportWithFileDialog(pb_Selection.Top());
			else if(m_ExportFormat == ExportFormat.StlAscii)
				res = ExportStlAscii.ExportWithFileDialog(Selection.gameObjects, FileType.Ascii);
			else if(m_ExportFormat == ExportFormat.StlBinary)
				res = ExportStlAscii.ExportWithFileDialog(Selection.gameObjects, FileType.Binary);
			else if(m_ExportFormat == ExportFormat.Asset)
				res = ExportAsset.MakeAsset();

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

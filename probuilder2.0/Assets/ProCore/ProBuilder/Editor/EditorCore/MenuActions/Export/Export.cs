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
	/**
	 *	Menu item and options for exporting meshes.
	 */
	public class Export : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Object; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return false; } }
		public override bool hasFileMenuEntry { get { return false; } }

		GUIContent gc_ExportFormat = new GUIContent("Export Format", "The type of file to export the current selection as.");
		GUIContent gc_ExportRecursive = new GUIContent("Include Children", "Should the exporter include children of the current selection when searching for meshes to export?");
		GUIContent gc_ObjExportRightHanded = new GUIContent("Right Handed", "Unity coordinate space is left handed, where most other major 3D modeling softwares are right handed. Usually this option should be left enabled.");
		GUIContent gc_ExportAsGroup = new GUIContent("Export As Group", "If enabled all selected meshes will be combined to a single model. If not, each mesh will be exported individually.");
		GUIContent gc_ObjApplyTransform = new GUIContent("Apply Transforms", "If enabled each mesh will have it's Transform applied prior to export. This is useful when you want to retain the correct placement of objects when re-importing to Unity (just set the imported mesh to { 0, 0, 0 }). If not enabled meshes are exported in local space.");
		GUIContent gc_ObjExportCopyTextures = new GUIContent("Copy Textures", "With Copy Textures enabled the exporter will copy material textures to the destination directory. If false the material library will point to the texture path within the Unity project. If you're exporting models with the intention of editing in an external 3D modeler then re-importing, disable this option to avoid duplicate textures in your project.");
		GUIContent gc_ObjExportVertexColors = new GUIContent("Vertex Colors", "Some 3D modeling applications will read and write vertex colors as an unofficial extension to the OBJ format.\n\nWarning! Enabling this can break compatibility with some other 3D modeling applications.");

		// Options for each export format
		private bool m_ExportRecursive;
		private bool m_ExportAsGroup;

		// obj specific
		private bool m_ObjExportRightHanded;
		private bool m_ObjExportCopyTextures;
		private bool m_ObjApplyTransform;
		private bool m_ObjExportVertexColors;

		// stl specific
		private Parabox.STL.FileType m_StlExportFormat = Parabox.STL.FileType.Ascii;

		// ply specific
		private bool m_PlyExportIsRightHanded;
		private bool m_PlyApplyTransform;
		private bool m_PlyQuads;
		private bool m_PlyNGons;

		public enum ExportFormat
		{
			Obj,
			Stl,
			Ply,
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

			// Recursively select meshes in selection (ie, use GetComponentsInChildren).
			m_ExportRecursive = pb_Preferences_Internal.GetBool("pbExportRecursive", false);
			m_ExportAsGroup = pb_Preferences_Internal.GetBool("pbExportAsGroup", true);

			// obj options
			m_ObjExportRightHanded = pb_Preferences_Internal.GetBool("pbObjExportRightHanded", true);
			m_ObjApplyTransform = pb_Preferences_Internal.GetBool("pbObjApplyTransform", true);
			m_ObjExportCopyTextures = pb_Preferences_Internal.GetBool("pbObjExportCopyTextures", true);
			m_ObjExportVertexColors = pb_Preferences_Internal.GetBool("pbObjExportVertexColors", false);

			// stl options
			m_StlExportFormat = (Parabox.STL.FileType) pb_Preferences_Internal.GetInt("pbStlFormat", (int) Parabox.STL.FileType.Ascii);

			// PLY options
			m_PlyExportIsRightHanded = pb_Preferences_Internal.GetBool("pbPlyExportIsRightHanded", true);
			m_PlyApplyTransform = pb_Preferences_Internal.GetBool("pbPlyApplyTransform", true);
			m_PlyQuads = pb_Preferences_Internal.GetBool("pbPlyQuads", true);
			m_PlyNGons = pb_Preferences_Internal.GetBool("pbPlyNGons", false);
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
			m_ExportFormat = (ExportFormat) EditorGUILayout.EnumPopup(gc_ExportFormat, m_ExportFormat);
			if(EditorGUI.EndChangeCheck())
				pb_Preferences_Internal.SetInt("pbDefaultExportFormat", (int) m_ExportFormat);

			m_ExportRecursive = EditorGUILayout.Toggle(gc_ExportRecursive, m_ExportRecursive);

			if(m_ExportFormat != ExportFormat.Asset && m_ExportFormat != ExportFormat.Stl)
			{
				EditorGUI.BeginChangeCheck();
				m_ExportAsGroup = EditorGUILayout.Toggle(gc_ExportAsGroup, m_ExportAsGroup);
				if(EditorGUI.EndChangeCheck())
					pb_Preferences_Internal.SetBool("pbExportAsGroup", m_ExportAsGroup);
			}

			if(m_ExportFormat == ExportFormat.Obj)
				ObjExportOptions();
			else if(m_ExportFormat == ExportFormat.Stl)
				StlExportOptions();
			else if(m_ExportFormat == ExportFormat.Ply)
				PlyExportOptions();

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Export"))
				DoAction();
		}

		private void ObjExportOptions()
		{
			EditorGUI.BeginChangeCheck();

			EditorGUI.BeginDisabledGroup(m_ExportAsGroup);

			if(m_ExportAsGroup)
				EditorGUILayout.Toggle("Apply Transforms", true);
			else
				m_ObjApplyTransform = EditorGUILayout.Toggle(gc_ObjApplyTransform, m_ObjApplyTransform);
			EditorGUI.EndDisabledGroup();

			m_ObjExportRightHanded = EditorGUILayout.Toggle(gc_ObjExportRightHanded, m_ObjExportRightHanded);
			m_ObjExportCopyTextures = EditorGUILayout.Toggle(gc_ObjExportCopyTextures, m_ObjExportCopyTextures);
			m_ObjExportVertexColors = EditorGUILayout.Toggle(gc_ObjExportVertexColors, m_ObjExportVertexColors);

			if(EditorGUI.EndChangeCheck())
			{
				pb_Preferences_Internal.SetBool("pbObjExportRightHanded", m_ObjExportRightHanded);
				pb_Preferences_Internal.SetBool("pbObjApplyTransform", m_ObjApplyTransform);
				pb_Preferences_Internal.SetBool("pbObjExportCopyTextures", m_ObjExportCopyTextures);
				pb_Preferences_Internal.SetBool("pbObjExportVertexColors", m_ObjExportVertexColors);
			}
		}

		private void StlExportOptions()
		{
			EditorGUI.BeginChangeCheck();

			m_StlExportFormat = (Parabox.STL.FileType) EditorGUILayout.EnumPopup("Stl Format", m_StlExportFormat);

			if(EditorGUI.EndChangeCheck())
				pb_Preferences_Internal.SetInt("pbStlFormat", (int) m_StlExportFormat);
		}

		private void PlyExportOptions()
		{
			EditorGUI.BeginChangeCheck();

			EditorGUI.BeginDisabledGroup(m_ExportAsGroup);
			if(m_ExportAsGroup)
				EditorGUILayout.Toggle("Apply Transforms", true);
			else
				m_PlyApplyTransform = EditorGUILayout.Toggle(gc_ObjApplyTransform, m_PlyApplyTransform);
			EditorGUI.EndDisabledGroup();

			m_PlyExportIsRightHanded = EditorGUILayout.Toggle("Right Handed", m_PlyExportIsRightHanded);
			m_PlyQuads = EditorGUILayout.Toggle("Quads", m_PlyQuads);

			// @todo ProBuilder N-Gon importer
			// m_PlyNGons = EditorGUILayout.Toggle("N-Gons", m_PlyNGons);
			// if(m_PlyNGons)
			// {
			// 	EditorGUILayout.HelpBox("Most 3D modeling programs will not import NGons correctly.", MessageType.Warning);
			// }

			if(EditorGUI.EndChangeCheck())
			{
				pb_Preferences_Internal.SetBool("pbPlyExportIsRightHanded", m_PlyExportIsRightHanded);
				pb_Preferences_Internal.SetBool("pbPlyApplyTransform", m_PlyApplyTransform);
				pb_Preferences_Internal.SetBool("pbPlyQuads", m_PlyQuads);
				pb_Preferences_Internal.SetBool("pbPlyNGons", m_PlyNGons);
			}
		}

		public override pb_ActionResult DoAction()
		{
			string res = null;

			IEnumerable<pb_Object> meshes = m_ExportRecursive ? pb_Selection.All() : pb_Selection.Top();

			if(meshes == null || meshes.Count() < 1)
			{
				return new pb_ActionResult(Status.Canceled, "No Meshes Selected");
			}
			else if(m_ExportFormat == ExportFormat.Obj)
			{
				res = ExportObj.ExportWithFileDialog(meshes, m_ExportAsGroup, new pb_ObjOptions() {
					handedness = m_ObjExportRightHanded ? pb_ObjOptions.Handedness.Right : pb_ObjOptions.Handedness.Left,
					copyTextures = m_ObjExportCopyTextures,
					applyTransforms = m_ExportAsGroup || m_ObjApplyTransform,
					writeVertexColors = m_ObjExportVertexColors
					});
			}
			else if(m_ExportFormat == ExportFormat.Stl)
			{
				res = ExportStlAscii.ExportWithFileDialog(meshes.Select(x => x.gameObject).ToArray(), m_StlExportFormat);
			}
			else if(m_ExportFormat == ExportFormat.Ply)
			{
				res = ExportPly.ExportWithFileDialog(meshes, m_ExportAsGroup, new pb_PlyOptions() { 
					isRightHanded = m_PlyExportIsRightHanded,
					applyTransforms = m_PlyApplyTransform,
					quads = m_PlyQuads,
					ngons = m_PlyNGons
					});
			}
			else if(m_ExportFormat == ExportFormat.Asset)
			{
				res = ExportAsset.ExportWithFileDialog(meshes);
			}

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

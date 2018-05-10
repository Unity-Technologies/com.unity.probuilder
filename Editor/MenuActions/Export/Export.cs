using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using System.Collections.Generic;
using Parabox.STL;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using EditorGUILayout = UnityEditor.EditorGUILayout;
using EditorGUIUtility = UnityEditor.EditorGUIUtility;
using EditorStyles = UnityEditor.EditorStyles;

namespace UnityEditor.ProBuilder.Actions
{
	/**
	 *	Menu item and options for exporting meshes.
	 */
	sealed class Export : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Object; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Object_Export", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		protected override bool hasFileMenuEntry{ get { return false; } }

		GUIContent gc_ExportFormat = new GUIContent("Export Format", "The type of file to export the current selection as.");
		GUIContent gc_ExportRecursive = new GUIContent("Include Children", "Should the exporter include children of the current selection when searching for meshes to export?");
		GUIContent gc_ObjExportRightHanded = new GUIContent("Right Handed", "Unity coordinate space is left handed, where most other major 3D modeling softwares are right handed. Usually this option should be left enabled.");
		GUIContent gc_ExportAsGroup = new GUIContent("Export As Group", "If enabled all selected meshes will be combined to a single model. If not, each mesh will be exported individually.");
		GUIContent gc_ObjApplyTransform = new GUIContent("Apply Transforms", "If enabled each mesh will have it's Transform applied prior to export. This is useful when you want to retain the correct placement of objects when re-importing to Unity (just set the imported mesh to { 0, 0, 0 }). If not enabled meshes are exported in local space.");
		GUIContent gc_ObjExportCopyTextures = new GUIContent("Copy Textures", "With Copy Textures enabled the exporter will copy material textures to the destination directory. If false the material library will point to the texture path within the Unity project. If you're exporting models with the intention of editing in an external 3D modeler then re-importing, disable this option to avoid duplicate textures in your project.");
		GUIContent gc_ObjExportVertexColors = new GUIContent("Vertex Colors", "Some 3D modeling applications will read and write vertex colors as an unofficial extension to the OBJ format.\n\nWarning! Enabling this can break compatibility with some other 3D modeling applications.");
		GUIContent gc_ObjTextureOffsetScale = new GUIContent("Texture Offset, Scale", "Write texture map offset and scale to the material library. Not all 3D modeling applications support this specificiation, and some will fail to load materials that define these values.");
		GUIContent gc_ObjQuads = new GUIContent("Export Quads", "Where possible, faces will be exported as quads instead of triangles. Note that this can result in a larger exported mesh (ProBuilder will not merge shared vertices with this option enabled).");

		// Options for each export format
		private bool m_ExportRecursive;
		private bool m_ExportAsGroup;

		// obj specific
		private bool m_ObjExportRightHanded;
		private bool m_ObjExportCopyTextures;
		private bool m_ObjApplyTransform;
		private bool m_ObjExportVertexColors;
		private bool m_ObjTextureOffsetScale;
		private bool m_ObjQuads;

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

		const ExportFormat DefaultFormat = ExportFormat.Obj;

		private ExportFormat m_ExportFormat = DefaultFormat;

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Export",
			"Export the selected ProBuilder objects as a model file."
		);

		public Export()
		{
			m_ExportFormat = (ExportFormat) PreferencesInternal.GetInt("pbDefaultExportFormat", (int) DefaultFormat);

			// Recursively select meshes in selection (ie, use GetComponentsInChildren).
			m_ExportRecursive = PreferencesInternal.GetBool("pbExportRecursive", false);
			m_ExportAsGroup = PreferencesInternal.GetBool("pbExportAsGroup", true);

			// obj options
			m_ObjExportRightHanded 	= PreferencesInternal.GetBool("pbObjExportRightHanded", true);
			m_ObjApplyTransform 	= PreferencesInternal.GetBool("pbObjApplyTransform", true);
			m_ObjExportCopyTextures = PreferencesInternal.GetBool("pbObjExportCopyTextures", true);
			m_ObjExportVertexColors = PreferencesInternal.GetBool("pbObjExportVertexColors", false);
			m_ObjTextureOffsetScale = PreferencesInternal.GetBool("pbObjTextureOffsetScale", false);
			m_ObjQuads 				= PreferencesInternal.GetBool("pbObjQuads", true);

			// stl options
			m_StlExportFormat = (Parabox.STL.FileType) PreferencesInternal.GetInt("pbStlFormat", (int) Parabox.STL.FileType.Ascii);

			// PLY options
			m_PlyExportIsRightHanded = PreferencesInternal.GetBool("pbPlyExportIsRightHanded", true);
			m_PlyApplyTransform = PreferencesInternal.GetBool("pbPlyApplyTransform", true);
			m_PlyQuads = PreferencesInternal.GetBool("pbPlyQuads", true);
			m_PlyNGons = PreferencesInternal.GetBool("pbPlyNGons", false);
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
				PreferencesInternal.SetInt("pbDefaultExportFormat", (int) m_ExportFormat);

			m_ExportRecursive = EditorGUILayout.Toggle(gc_ExportRecursive, m_ExportRecursive);

			if( m_ExportFormat != ExportFormat.Asset &&
				m_ExportFormat != ExportFormat.Stl )
			{
				EditorGUI.BeginChangeCheck();
				m_ExportAsGroup = EditorGUILayout.Toggle(gc_ExportAsGroup, m_ExportAsGroup);
				if(EditorGUI.EndChangeCheck())
					PreferencesInternal.SetBool("pbExportAsGroup", m_ExportAsGroup);
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
			m_ObjTextureOffsetScale = EditorGUILayout.Toggle(gc_ObjTextureOffsetScale, m_ObjTextureOffsetScale);
			m_ObjQuads = EditorGUILayout.Toggle(gc_ObjQuads, m_ObjQuads);

			if(EditorGUI.EndChangeCheck())
			{
				PreferencesInternal.SetBool("pbObjExportRightHanded", m_ObjExportRightHanded);
				PreferencesInternal.SetBool("pbObjApplyTransform", m_ObjApplyTransform);
				PreferencesInternal.SetBool("pbObjExportCopyTextures", m_ObjExportCopyTextures);
				PreferencesInternal.SetBool("pbObjExportVertexColors", m_ObjExportVertexColors);
				PreferencesInternal.SetBool("pbObjTextureOffsetScale", m_ObjTextureOffsetScale);
				PreferencesInternal.SetBool("pbObjQuads", m_ObjQuads);
			}
		}

		private void StlExportOptions()
		{
			EditorGUI.BeginChangeCheck();

			m_StlExportFormat = (Parabox.STL.FileType) EditorGUILayout.EnumPopup("Stl Format", m_StlExportFormat);

			if(EditorGUI.EndChangeCheck())
				PreferencesInternal.SetInt("pbStlFormat", (int) m_StlExportFormat);
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
				PreferencesInternal.SetBool("pbPlyExportIsRightHanded", m_PlyExportIsRightHanded);
				PreferencesInternal.SetBool("pbPlyApplyTransform", m_PlyApplyTransform);
				PreferencesInternal.SetBool("pbPlyQuads", m_PlyQuads);
				PreferencesInternal.SetBool("pbPlyNGons", m_PlyNGons);
			}
		}

		public override ActionResult DoAction()
		{
			string res = null;

			IEnumerable<ProBuilderMesh> meshes = m_ExportRecursive ? MeshSelection.All() : MeshSelection.Top();

			if(meshes == null || meshes.Count() < 1)
			{
				return new ActionResult(ActionResult.Status.Canceled, "No Meshes Selected");
			}
			else if(m_ExportFormat == ExportFormat.Obj)
			{
				res = ExportObj.ExportWithFileDialog(meshes,
					m_ExportAsGroup,
					m_ObjQuads,
					new ObjOptions() {
						handedness = m_ObjExportRightHanded ? ObjOptions.Handedness.Right : ObjOptions.Handedness.Left,
						copyTextures = m_ObjExportCopyTextures,
						applyTransforms = m_ExportAsGroup || m_ObjApplyTransform,
						vertexColors = m_ObjExportVertexColors,
						textureOffsetScale = m_ObjTextureOffsetScale
					});
			}
			else if(m_ExportFormat == ExportFormat.Stl)
			{
				res = ExportStlAscii.ExportWithFileDialog(meshes.Select(x => x.gameObject).ToArray(), m_StlExportFormat);
			}
			else if(m_ExportFormat == ExportFormat.Ply)
			{
				res = ExportPly.ExportWithFileDialog(meshes, m_ExportAsGroup, new PlyOptions() {
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
				return new ActionResult(ActionResult.Status.Canceled, "User Canceled");
			}
			else
			{
				if(res.Contains(Application.dataPath))
				{
					AssetDatabase.Refresh();
					string projectPath = string.Format("Assets{0}", res.Replace(Application.dataPath, ""));
					Object o = AssetDatabase.LoadAssetAtPath<GameObject>(projectPath);
					if(o != null)
						EditorGUIUtility.PingObject(o);
				}

				return new ActionResult(ActionResult.Status.Success, "Export " + m_ExportFormat);
			}
		}
	}
}

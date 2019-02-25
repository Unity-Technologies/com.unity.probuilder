/**
 *  IMPORTANT
 *
 *  This is a generated file. Any changes will be overwritten.
 *  See Debug/GenerateMenuItems to make modifications.
 */
#if UNITY_2019_1_OR_NEWER
#define SHORTCUT_MANAGER
#endif

using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.Actions;
#if SHORTCUT_MANAGER
using UnityEditor.ShortcutManagement;
#endif

namespace UnityEditor.ProBuilder
{
    static class EditorToolbarMenuItem
    {
        const string k_MenuPrefix = "Tools/ProBuilder/";
        const string k_ShortcutPrefix = "ProBuilder/";

		[MenuItem(k_MenuPrefix + "Editors/New Bezier Shape ", true)]
		static bool MenuVerify_NewBezierShape()
		{
			var instance = EditorToolbarLoader.GetInstance<NewBezierShape>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/New Bezier Shape", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_NewBezierShape()
		{
			var instance = EditorToolbarLoader.GetInstance<NewBezierShape>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Editors/New Poly Shape ", true)]
		static bool MenuVerify_NewPolyShape()
		{
			var instance = EditorToolbarLoader.GetInstance<NewPolyShape>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/New Poly Shape", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_NewPolyShape()
		{
			var instance = EditorToolbarLoader.GetInstance<NewPolyShape>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Lightmap UV Editor ", true)]
		static bool MenuVerify_OpenLightmapUVEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenLightmapUVEditor>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Lightmap UV Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenLightmapUVEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenLightmapUVEditor>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Material Editor ", true)]
		static bool MenuVerify_OpenMaterialEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenMaterialEditor>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Material Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenMaterialEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenMaterialEditor>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Shape Editor ", true)]
		static bool MenuVerify_OpenShapeEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenShapeEditor>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Shape Editor %#k", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenShapeEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenShapeEditor>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Shape Editor Menu Item ", true)]
		static bool MenuVerify_OpenShapeEditorMenuItem()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenShapeEditorMenuItem>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Shape Editor Menu Item %#k", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenShapeEditorMenuItem()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenShapeEditorMenuItem>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Smoothing Editor ", true)]
		static bool MenuVerify_OpenSmoothingEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Smoothing Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenSmoothingEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Editors/Open UV Editor ", true)]
		static bool MenuVerify_OpenUVEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenUVEditor>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open UV Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenUVEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenUVEditor>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Vertex Color Editor ", true)]
		static bool MenuVerify_OpenVertexColorEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Vertex Color Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenVertexColorEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Vertex Position Editor ", true)]
		static bool MenuVerify_OpenVertexPositionEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenVertexPositionEditor>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Vertex Position Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenVertexPositionEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenVertexPositionEditor>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Export/Export ", true)]
		static bool MenuVerify_Export()
		{
			var instance = EditorToolbarLoader.GetInstance<Export>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Export/Export", false, PreferenceKeys.menuExport + 0)]
		static void MenuPerform_Export()
		{
			var instance = EditorToolbarLoader.GetInstance<Export>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Export/Export Asset ", true)]
		static bool MenuVerify_ExportAsset()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportAsset>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Export/Export Asset", false, PreferenceKeys.menuExport + 0)]
		static void MenuPerform_ExportAsset()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportAsset>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Export/Export Obj ", true)]
		static bool MenuVerify_ExportObj()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportObj>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Export/Export Obj", false, PreferenceKeys.menuExport + 0)]
		static void MenuPerform_ExportObj()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportObj>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Export/Export Ply ", true)]
		static bool MenuVerify_ExportPly()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportPly>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Export/Export Ply", false, PreferenceKeys.menuExport + 0)]
		static void MenuPerform_ExportPly()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportPly>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Export/Export Stl Ascii ", true)]
		static bool MenuVerify_ExportStlAscii()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportStlAscii>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Export/Export Stl Ascii", false, PreferenceKeys.menuExport + 0)]
		static void MenuPerform_ExportStlAscii()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportStlAscii>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Export/Export Stl Binary ", true)]
		static bool MenuVerify_ExportStlBinary()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportStlBinary>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Export/Export Stl Binary", false, PreferenceKeys.menuExport + 0)]
		static void MenuPerform_ExportStlBinary()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportStlBinary>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Bevel Edges ", true)]
		static bool MenuVerify_BevelEdges()
		{
			var instance = EditorToolbarLoader.GetInstance<BevelEdges>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Bevel Edges", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_BevelEdges()
		{
			var instance = EditorToolbarLoader.GetInstance<BevelEdges>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Bridge Edges ", true)]
		static bool MenuVerify_BridgeEdges()
		{
			var instance = EditorToolbarLoader.GetInstance<BridgeEdges>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Bridge Edges &b", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_BridgeEdges()
		{
			var instance = EditorToolbarLoader.GetInstance<BridgeEdges>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Collapse Vertices ", true)]
		static bool MenuVerify_CollapseVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<CollapseVertices>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Collapse Vertices &c", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_CollapseVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<CollapseVertices>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Conform Face Normals ", true)]
		static bool MenuVerify_ConformFaceNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<ConformFaceNormals>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Conform Face Normals", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_ConformFaceNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<ConformFaceNormals>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Connect Edges ", true)]
		static bool MenuVerify_ConnectEdges()
		{
			var instance = EditorToolbarLoader.GetInstance<ConnectEdges>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Connect Edges &e", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_ConnectEdges()
		{
			var instance = EditorToolbarLoader.GetInstance<ConnectEdges>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Connect Vertices ", true)]
		static bool MenuVerify_ConnectVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<ConnectVertices>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Connect Vertices &e", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_ConnectVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<ConnectVertices>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Delete Faces ", true)]
		static bool MenuVerify_DeleteFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<DeleteFaces>();
			return instance != null && instance.enabled;
		}

#if SHORTCUT_MANAGER
		[Shortcut(k_ShortcutPrefix + "Geometry/Delete Faces", typeof(UnityEditor.SceneView), (KeyCode) 8, (ShortcutModifiers) 0)]
#endif
		[MenuItem(k_MenuPrefix + "Geometry/Delete Faces [⌫]", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_DeleteFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<DeleteFaces>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Detach Faces ", true)]
		static bool MenuVerify_DetachFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<DetachFaces>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Detach Faces", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_DetachFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<DetachFaces>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Extrude ", true)]
		static bool MenuVerify_Extrude()
		{
			var instance = EditorToolbarLoader.GetInstance<Extrude>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Extrude %e", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_Extrude()
		{
			var instance = EditorToolbarLoader.GetInstance<Extrude>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Extrude Edges ", true)]
		static bool MenuVerify_ExtrudeEdges()
		{
			var instance = EditorToolbarLoader.GetInstance<ExtrudeEdges>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Extrude Edges %e", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_ExtrudeEdges()
		{
			var instance = EditorToolbarLoader.GetInstance<ExtrudeEdges>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Extrude Faces ", true)]
		static bool MenuVerify_ExtrudeFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<ExtrudeFaces>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Extrude Faces %e", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_ExtrudeFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<ExtrudeFaces>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Fill Hole ", true)]
		static bool MenuVerify_FillHole()
		{
			var instance = EditorToolbarLoader.GetInstance<FillHole>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Fill Hole", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_FillHole()
		{
			var instance = EditorToolbarLoader.GetInstance<FillHole>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Flip Face Edge ", true)]
		static bool MenuVerify_FlipFaceEdge()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipFaceEdge>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Flip Face Edge", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_FlipFaceEdge()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipFaceEdge>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Flip Face Normals ", true)]
		static bool MenuVerify_FlipFaceNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipFaceNormals>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Flip Face Normals &n", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_FlipFaceNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipFaceNormals>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Insert Edge Loop ", true)]
		static bool MenuVerify_InsertEdgeLoop()
		{
			var instance = EditorToolbarLoader.GetInstance<InsertEdgeLoop>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Insert Edge Loop &u", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_InsertEdgeLoop()
		{
			var instance = EditorToolbarLoader.GetInstance<InsertEdgeLoop>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Merge Faces ", true)]
		static bool MenuVerify_MergeFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<MergeFaces>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Merge Faces", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_MergeFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<MergeFaces>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Set Pivot To Selection ", true)]
		static bool MenuVerify_SetPivotToSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<SetPivotToSelection>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Set Pivot To Selection %j", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_SetPivotToSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<SetPivotToSelection>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Smart Connect ", true)]
		static bool MenuVerify_SmartConnect()
		{
			var instance = EditorToolbarLoader.GetInstance<SmartConnect>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Smart Connect &e", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_SmartConnect()
		{
			var instance = EditorToolbarLoader.GetInstance<SmartConnect>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Smart Subdivide ", true)]
		static bool MenuVerify_SmartSubdivide()
		{
			var instance = EditorToolbarLoader.GetInstance<SmartSubdivide>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Smart Subdivide &s", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_SmartSubdivide()
		{
			var instance = EditorToolbarLoader.GetInstance<SmartSubdivide>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Split Vertices ", true)]
		static bool MenuVerify_SplitVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<SplitVertices>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Split Vertices &x", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_SplitVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<SplitVertices>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Subdivide Edges ", true)]
		static bool MenuVerify_SubdivideEdges()
		{
			var instance = EditorToolbarLoader.GetInstance<SubdivideEdges>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Subdivide Edges &s", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_SubdivideEdges()
		{
			var instance = EditorToolbarLoader.GetInstance<SubdivideEdges>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Subdivide Faces ", true)]
		static bool MenuVerify_SubdivideFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<SubdivideFaces>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Subdivide Faces &s", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_SubdivideFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<SubdivideFaces>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Triangulate Faces ", true)]
		static bool MenuVerify_TriangulateFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<TriangulateFaces>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Triangulate Faces", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_TriangulateFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<TriangulateFaces>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Geometry/Weld Vertices ", true)]
		static bool MenuVerify_WeldVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<WeldVertices>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Weld Vertices &v", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_WeldVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<WeldVertices>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Drag Rect Mode ", true)]
		static bool MenuVerify_ToggleDragRectMode()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleDragRectMode>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Drag Rect Mode", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleDragRectMode()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleDragRectMode>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Drag Selection Mode ", true)]
		static bool MenuVerify_ToggleDragSelectionMode()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleDragSelectionMode>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Drag Selection Mode", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleDragSelectionMode()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleDragSelectionMode>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Handle Orientation ", true)]
		static bool MenuVerify_ToggleHandleOrientation()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleHandleOrientation>();
			return instance != null && instance.enabled;
		}

#if SHORTCUT_MANAGER
		[Shortcut(k_ShortcutPrefix + "Interaction/Toggle Handle Orientation", typeof(UnityEditor.SceneView), (KeyCode) 112, (ShortcutModifiers) 0)]
#endif
		[MenuItem(k_MenuPrefix + "Interaction/Toggle Handle Orientation [p]", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleHandleOrientation()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleHandleOrientation>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Select Back Faces ", true)]
		static bool MenuVerify_ToggleSelectBackFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Select Back Faces", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleSelectBackFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Object/Center Pivot ", true)]
		static bool MenuVerify_CenterPivot()
		{
			var instance = EditorToolbarLoader.GetInstance<CenterPivot>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Center Pivot", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_CenterPivot()
		{
			var instance = EditorToolbarLoader.GetInstance<CenterPivot>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Object/Conform Object Normals ", true)]
		static bool MenuVerify_ConformObjectNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<ConformObjectNormals>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Conform Object Normals", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_ConformObjectNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<ConformObjectNormals>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Object/Flip Object Normals ", true)]
		static bool MenuVerify_FlipObjectNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipObjectNormals>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Flip Object Normals", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_FlipObjectNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipObjectNormals>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Object/Freeze Transform ", true)]
		static bool MenuVerify_FreezeTransform()
		{
			var instance = EditorToolbarLoader.GetInstance<FreezeTransform>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Freeze Transform", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_FreezeTransform()
		{
			var instance = EditorToolbarLoader.GetInstance<FreezeTransform>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Object/Generate U V2 ", true)]
		static bool MenuVerify_GenerateUV2()
		{
			var instance = EditorToolbarLoader.GetInstance<GenerateUV2>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Generate U V2", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_GenerateUV2()
		{
			var instance = EditorToolbarLoader.GetInstance<GenerateUV2>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Object/Merge Objects ", true)]
		static bool MenuVerify_MergeObjects()
		{
			var instance = EditorToolbarLoader.GetInstance<MergeObjects>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Merge Objects", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_MergeObjects()
		{
			var instance = EditorToolbarLoader.GetInstance<MergeObjects>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Object/Mirror Objects ", true)]
		static bool MenuVerify_MirrorObjects()
		{
			var instance = EditorToolbarLoader.GetInstance<MirrorObjects>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Mirror Objects", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_MirrorObjects()
		{
			var instance = EditorToolbarLoader.GetInstance<MirrorObjects>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Object/Pro Builderize ", true)]
		static bool MenuVerify_ProBuilderize()
		{
			var instance = EditorToolbarLoader.GetInstance<ProBuilderize>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Pro Builderize", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_ProBuilderize()
		{
			var instance = EditorToolbarLoader.GetInstance<ProBuilderize>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Object/Set Collider ", true)]
		static bool MenuVerify_SetCollider()
		{
			var instance = EditorToolbarLoader.GetInstance<SetCollider>();
			return instance != null && instance.enabled;
		}

#if SHORTCUT_MANAGER
		[Shortcut(k_ShortcutPrefix + "Object/Set Collider", typeof(UnityEditor.SceneView), (KeyCode) 99, (ShortcutModifiers) 0)]
#endif
		[MenuItem(k_MenuPrefix + "Object/Set Collider", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_SetCollider()
		{
			var instance = EditorToolbarLoader.GetInstance<SetCollider>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Object/Set Trigger ", true)]
		static bool MenuVerify_SetTrigger()
		{
			var instance = EditorToolbarLoader.GetInstance<SetTrigger>();
			return instance != null && instance.enabled;
		}

#if SHORTCUT_MANAGER
		[Shortcut(k_ShortcutPrefix + "Object/Set Trigger", typeof(UnityEditor.SceneView), (KeyCode) 116, (ShortcutModifiers) 0)]
#endif
		[MenuItem(k_MenuPrefix + "Object/Set Trigger", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_SetTrigger()
		{
			var instance = EditorToolbarLoader.GetInstance<SetTrigger>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Object/Subdivide Object ", true)]
		static bool MenuVerify_SubdivideObject()
		{
			var instance = EditorToolbarLoader.GetInstance<SubdivideObject>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Subdivide Object", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_SubdivideObject()
		{
			var instance = EditorToolbarLoader.GetInstance<SubdivideObject>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Object/Triangulate Object ", true)]
		static bool MenuVerify_TriangulateObject()
		{
			var instance = EditorToolbarLoader.GetInstance<TriangulateObject>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Triangulate Object", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_TriangulateObject()
		{
			var instance = EditorToolbarLoader.GetInstance<TriangulateObject>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Selection/Grow Selection ", true)]
		static bool MenuVerify_GrowSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<GrowSelection>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Grow Selection &g", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_GrowSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<GrowSelection>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Selection/Invert Selection ", true)]
		static bool MenuVerify_InvertSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<InvertSelection>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Invert Selection %#i", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_InvertSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<InvertSelection>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Edge Loop ", true)]
		static bool MenuVerify_SelectEdgeLoop()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectEdgeLoop>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Edge Loop &l", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectEdgeLoop()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectEdgeLoop>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Edge Ring ", true)]
		static bool MenuVerify_SelectEdgeRing()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectEdgeRing>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Edge Ring &r", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectEdgeRing()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectEdgeRing>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Face Loop ", true)]
		static bool MenuVerify_SelectFaceLoop()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectFaceLoop>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Face Loop", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectFaceLoop()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectFaceLoop>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Face Ring ", true)]
		static bool MenuVerify_SelectFaceRing()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectFaceRing>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Face Ring", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectFaceRing()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectFaceRing>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Hole ", true)]
		static bool MenuVerify_SelectHole()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectHole>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Hole", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectHole()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectHole>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Loop ", true)]
		static bool MenuVerify_SelectLoop()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectLoop>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Loop &l", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectLoop()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectLoop>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Material ", true)]
		static bool MenuVerify_SelectMaterial()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectMaterial>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Material", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectMaterial()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectMaterial>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Ring ", true)]
		static bool MenuVerify_SelectRing()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectRing>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Ring &r", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectRing()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectRing>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Smoothing Group ", true)]
		static bool MenuVerify_SelectSmoothingGroup()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectSmoothingGroup>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Smoothing Group", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectSmoothingGroup()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectSmoothingGroup>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Vertex Color ", true)]
		static bool MenuVerify_SelectVertexColor()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectVertexColor>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Vertex Color", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectVertexColor()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectVertexColor>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(k_MenuPrefix + "Selection/Shrink Selection ", true)]
		static bool MenuVerify_ShrinkSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<ShrinkSelection>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Shrink Selection &#g", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_ShrinkSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<ShrinkSelection>();
			if(instance != null && instance.enabled)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}
	}
}

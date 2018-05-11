/**
 *	IMPORTANT
 *
 *	This is a generated file. Any changes will be overwritten.
 *	See pb_GenerateMenuItems to make modifications.
 */
using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.Actions;
using System.Collections.Generic;

namespace UnityEditor.ProBuilder
{
	static class EditorToolbarMenuItems
	{

#if PROTOTYPE
		const string PB_MENU_PREFIX = "Tools/ProBuilder Basic/";
#else
		const string PB_MENU_PREFIX = "Tools/ProBuilder/";
#endif

		[MenuItem(PB_MENU_PREFIX + "Editors/New Bezier Shape ", true)]
		static bool MenuVerifyNewBezierShape()
		{
			NewBezierShape instance = EditorToolbarLoader.GetInstance<NewBezierShape>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Editors/New Bezier Shape ", false, PreferenceKeys.menuEditor + 1)]
		static void MenuDoNewBezierShape()
		{
			NewBezierShape instance = EditorToolbarLoader.GetInstance<NewBezierShape>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/New Poly Shape ", true)]
		static bool MenuVerifyNewPolyShape()
		{
			NewPolyShape instance = EditorToolbarLoader.GetInstance<NewPolyShape>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Editors/New Poly Shape ", false, PreferenceKeys.menuEditor + 1)]
		static void MenuDoNewPolyShape()
		{
			NewPolyShape instance = EditorToolbarLoader.GetInstance<NewPolyShape>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Material Editor ", true)]
		static bool MenuVerifyOpenMaterialEditor()
		{
			OpenMaterialEditor instance = EditorToolbarLoader.GetInstance<OpenMaterialEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Material Editor ", false, PreferenceKeys.menuEditor + 1)]
		static void MenuDoOpenMaterialEditor()
		{
			OpenMaterialEditor instance = EditorToolbarLoader.GetInstance<OpenMaterialEditor>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Shape Editor %#k", true)]
		static bool MenuVerifyOpenShapeEditor()
		{
			OpenShapeEditor instance = EditorToolbarLoader.GetInstance<OpenShapeEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Shape Editor %#k", false, PreferenceKeys.menuEditor + 1)]
		static void MenuDoOpenShapeEditor()
		{
			OpenShapeEditor instance = EditorToolbarLoader.GetInstance<OpenShapeEditor>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Smoothing Editor ", true)]
		static bool MenuVerifyOpenSmoothingEditor()
		{
			OpenSmoothingEditor instance = EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Smoothing Editor ", false, PreferenceKeys.menuEditor + 1)]
		static void MenuDoOpenSmoothingEditor()
		{
			OpenSmoothingEditor instance = EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open UV Editor ", true)]
		static bool MenuVerifyOpenUVEditor()
		{
			OpenUVEditor instance = EditorToolbarLoader.GetInstance<OpenUVEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open UV Editor ", false, PreferenceKeys.menuEditor + 1)]
		static void MenuDoOpenUVEditor()
		{
			OpenUVEditor instance = EditorToolbarLoader.GetInstance<OpenUVEditor>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Vertex Color Editor ", true)]
		static bool MenuVerifyOpenVertexColorEditor()
		{
			OpenVertexColorEditor instance = EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Vertex Color Editor ", false, PreferenceKeys.menuEditor + 1)]
		static void MenuDoOpenVertexColorEditor()
		{
			OpenVertexColorEditor instance = EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Vertex Position Editor ", true)]
		static bool MenuVerifyOpenVertexPositionEditor()
		{
			OpenVertexPositionEditor instance = EditorToolbarLoader.GetInstance<OpenVertexPositionEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Vertex Position Editor ", false, PreferenceKeys.menuEditor + 1)]
		static void MenuDoOpenVertexPositionEditor()
		{
			OpenVertexPositionEditor instance = EditorToolbarLoader.GetInstance<OpenVertexPositionEditor>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}


		[MenuItem(PB_MENU_PREFIX + "Export/Export Asset ", true)]
		static bool MenuVerifyExportAsset()
		{
			ExportAsset instance = EditorToolbarLoader.GetInstance<ExportAsset>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Asset ", false, PreferenceKeys.menuExport + 0)]
		static void MenuDoExportAsset()
		{
			ExportAsset instance = EditorToolbarLoader.GetInstance<ExportAsset>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Obj ", true)]
		static bool MenuVerifyExportObj()
		{
			ExportObj instance = EditorToolbarLoader.GetInstance<ExportObj>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Obj ", false, PreferenceKeys.menuExport + 0)]
		static void MenuDoExportObj()
		{
			ExportObj instance = EditorToolbarLoader.GetInstance<ExportObj>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Ply ", true)]
		static bool MenuVerifyExportPly()
		{
			ExportPly instance = EditorToolbarLoader.GetInstance<ExportPly>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Ply ", false, PreferenceKeys.menuExport + 0)]
		static void MenuDoExportPly()
		{
			ExportPly instance = EditorToolbarLoader.GetInstance<ExportPly>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Stl Ascii ", true)]
		static bool MenuVerifyExportStlAscii()
		{
			ExportStlAscii instance = EditorToolbarLoader.GetInstance<ExportStlAscii>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Stl Ascii ", false, PreferenceKeys.menuExport + 0)]
		static void MenuDoExportStlAscii()
		{
			ExportStlAscii instance = EditorToolbarLoader.GetInstance<ExportStlAscii>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Stl Binary ", true)]
		static bool MenuVerifyExportStlBinary()
		{
			ExportStlBinary instance = EditorToolbarLoader.GetInstance<ExportStlBinary>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Stl Binary ", false, PreferenceKeys.menuExport + 0)]
		static void MenuDoExportStlBinary()
		{
			ExportStlBinary instance = EditorToolbarLoader.GetInstance<ExportStlBinary>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Bevel Edges ", true)]
		static bool MenuVerifyBevelEdges()
		{
			BevelEdges instance = EditorToolbarLoader.GetInstance<BevelEdges>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Bevel Edges ", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoBevelEdges()
		{
			BevelEdges instance = EditorToolbarLoader.GetInstance<BevelEdges>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Bridge Edges &b", true)]
		static bool MenuVerifyBridgeEdges()
		{
			BridgeEdges instance = EditorToolbarLoader.GetInstance<BridgeEdges>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Bridge Edges &b", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoBridgeEdges()
		{
			BridgeEdges instance = EditorToolbarLoader.GetInstance<BridgeEdges>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Collapse Vertices &c", true)]
		static bool MenuVerifyCollapseVertices()
		{
			CollapseVertices instance = EditorToolbarLoader.GetInstance<CollapseVertices>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Collapse Vertices &c", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoCollapseVertices()
		{
			CollapseVertices instance = EditorToolbarLoader.GetInstance<CollapseVertices>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Conform Face Normals ", true)]
		static bool MenuVerifyConformFaceNormals()
		{
			ConformFaceNormals instance = EditorToolbarLoader.GetInstance<ConformFaceNormals>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Conform Face Normals ", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoConformFaceNormals()
		{
			ConformFaceNormals instance = EditorToolbarLoader.GetInstance<ConformFaceNormals>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}



		[MenuItem(PB_MENU_PREFIX + "Geometry/Delete Faces  [delete]", true)]
		static bool MenuVerifyDeleteFaces()
		{
			DeleteFaces instance = EditorToolbarLoader.GetInstance<DeleteFaces>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Delete Faces  [delete]", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoDeleteFaces()
		{
			DeleteFaces instance = EditorToolbarLoader.GetInstance<DeleteFaces>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Detach Faces ", true)]
		static bool MenuVerifyDetachFaces()
		{
			DetachFaces instance = EditorToolbarLoader.GetInstance<DetachFaces>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Detach Faces ", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoDetachFaces()
		{
			DetachFaces instance = EditorToolbarLoader.GetInstance<DetachFaces>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}



		[MenuItem(PB_MENU_PREFIX + "Geometry/Fill Hole ", true)]
		static bool MenuVerifyFillHole()
		{
			FillHole instance = EditorToolbarLoader.GetInstance<FillHole>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Fill Hole ", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoFillHole()
		{
			FillHole instance = EditorToolbarLoader.GetInstance<FillHole>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Flip Face Edge ", true)]
		static bool MenuVerifyFlipFaceEdge()
		{
			FlipFaceEdge instance = EditorToolbarLoader.GetInstance<FlipFaceEdge>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Flip Face Edge ", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoFlipFaceEdge()
		{
			FlipFaceEdge instance = EditorToolbarLoader.GetInstance<FlipFaceEdge>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Flip Face Normals &n", true)]
		static bool MenuVerifyFlipFaceNormals()
		{
			FlipFaceNormals instance = EditorToolbarLoader.GetInstance<FlipFaceNormals>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Flip Face Normals &n", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoFlipFaceNormals()
		{
			FlipFaceNormals instance = EditorToolbarLoader.GetInstance<FlipFaceNormals>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Insert Edge Loop &u", true)]
		static bool MenuVerifyInsertEdgeLoop()
		{
			InsertEdgeLoop instance = EditorToolbarLoader.GetInstance<InsertEdgeLoop>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Insert Edge Loop &u", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoInsertEdgeLoop()
		{
			InsertEdgeLoop instance = EditorToolbarLoader.GetInstance<InsertEdgeLoop>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Merge Faces ", true)]
		static bool MenuVerifyMergeFaces()
		{
			MergeFaces instance = EditorToolbarLoader.GetInstance<MergeFaces>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Merge Faces ", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoMergeFaces()
		{
			MergeFaces instance = EditorToolbarLoader.GetInstance<MergeFaces>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Set Pivot To Selection %j", true)]
		static bool MenuVerifySetPivotToSelection()
		{
			SetPivotToSelection instance = EditorToolbarLoader.GetInstance<SetPivotToSelection>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Set Pivot To Selection %j", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoSetPivotToSelection()
		{
			SetPivotToSelection instance = EditorToolbarLoader.GetInstance<SetPivotToSelection>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Smart Connect &e", true)]
		static bool MenuVerifySmartConnect()
		{
			SmartConnect instance = EditorToolbarLoader.GetInstance<SmartConnect>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Smart Connect &e", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoSmartConnect()
		{
			SmartConnect instance = EditorToolbarLoader.GetInstance<SmartConnect>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Smart Subdivide &s", true)]
		static bool MenuVerifySmartSubdivide()
		{
			SmartSubdivide instance = EditorToolbarLoader.GetInstance<SmartSubdivide>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Smart Subdivide &s", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoSmartSubdivide()
		{
			SmartSubdivide instance = EditorToolbarLoader.GetInstance<SmartSubdivide>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Split Vertices &x", true)]
		static bool MenuVerifySplitVertices()
		{
			SplitVertices instance = EditorToolbarLoader.GetInstance<SplitVertices>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Split Vertices &x", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoSplitVertices()
		{
			SplitVertices instance = EditorToolbarLoader.GetInstance<SplitVertices>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}



		[MenuItem(PB_MENU_PREFIX + "Geometry/Triangulate Faces ", true)]
		static bool MenuVerifyTriangulateFaces()
		{
			TriangulateFaces instance = EditorToolbarLoader.GetInstance<TriangulateFaces>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Triangulate Faces ", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoTriangulateFaces()
		{
			TriangulateFaces instance = EditorToolbarLoader.GetInstance<TriangulateFaces>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Weld Vertices &v", true)]
		static bool MenuVerifyWeldVertices()
		{
			WeldVertices instance = EditorToolbarLoader.GetInstance<WeldVertices>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Weld Vertices &v", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoWeldVertices()
		{
			WeldVertices instance = EditorToolbarLoader.GetInstance<WeldVertices>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Interaction/Toggle Drag Rect Mode ", true)]
		static bool MenuVerifyToggleDragRectMode()
		{
			ToggleDragRectMode instance = EditorToolbarLoader.GetInstance<ToggleDragRectMode>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Interaction/Toggle Drag Rect Mode ", false, PreferenceKeys.menuSelection + 1)]
		static void MenuDoToggleDragRectMode()
		{
			ToggleDragRectMode instance = EditorToolbarLoader.GetInstance<ToggleDragRectMode>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Interaction/Toggle Drag Selection Mode ", true)]
		static bool MenuVerifyToggleDragSelectionMode()
		{
			ToggleDragSelectionMode instance = EditorToolbarLoader.GetInstance<ToggleDragSelectionMode>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Interaction/Toggle Drag Selection Mode ", false, PreferenceKeys.menuSelection + 1)]
		static void MenuDoToggleDragSelectionMode()
		{
			ToggleDragSelectionMode instance = EditorToolbarLoader.GetInstance<ToggleDragSelectionMode>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Interaction/Toggle Handle Alignment  [p]", true)]
		static bool MenuVerifyToggleHandleAlignment()
		{
			ToggleHandleAlignment instance = EditorToolbarLoader.GetInstance<ToggleHandleAlignment>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Interaction/Toggle Handle Alignment  [p]", false, PreferenceKeys.menuSelection + 1)]
		static void MenuDoToggleHandleAlignment()
		{
			ToggleHandleAlignment instance = EditorToolbarLoader.GetInstance<ToggleHandleAlignment>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Interaction/Toggle Select Back Faces ", true)]
		static bool MenuVerifyToggleSelectBackFaces()
		{
			ToggleSelectBackFaces instance = EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Interaction/Toggle Select Back Faces ", false, PreferenceKeys.menuSelection + 1)]
		static void MenuDoToggleSelectBackFaces()
		{
			ToggleSelectBackFaces instance = EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Center Pivot ", true)]
		static bool MenuVerifyCenterPivot()
		{
			CenterPivot instance = EditorToolbarLoader.GetInstance<CenterPivot>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Object/Center Pivot ", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuDoCenterPivot()
		{
			CenterPivot instance = EditorToolbarLoader.GetInstance<CenterPivot>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Conform Object Normals ", true)]
		static bool MenuVerifyConformObjectNormals()
		{
			ConformObjectNormals instance = EditorToolbarLoader.GetInstance<ConformObjectNormals>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Object/Conform Object Normals ", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuDoConformObjectNormals()
		{
			ConformObjectNormals instance = EditorToolbarLoader.GetInstance<ConformObjectNormals>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Flip Object Normals ", true)]
		static bool MenuVerifyFlipObjectNormals()
		{
			FlipObjectNormals instance = EditorToolbarLoader.GetInstance<FlipObjectNormals>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Object/Flip Object Normals ", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuDoFlipObjectNormals()
		{
			FlipObjectNormals instance = EditorToolbarLoader.GetInstance<FlipObjectNormals>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Freeze Transform ", true)]
		static bool MenuVerifyFreezeTransform()
		{
			FreezeTransform instance = EditorToolbarLoader.GetInstance<FreezeTransform>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Object/Freeze Transform ", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuDoFreezeTransform()
		{
			FreezeTransform instance = EditorToolbarLoader.GetInstance<FreezeTransform>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}


		[MenuItem(PB_MENU_PREFIX + "Object/Merge Objects ", true)]
		static bool MenuVerifyMergeObjects()
		{
			MergeObjects instance = EditorToolbarLoader.GetInstance<MergeObjects>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Object/Merge Objects ", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuDoMergeObjects()
		{
			MergeObjects instance = EditorToolbarLoader.GetInstance<MergeObjects>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Mirror Objects ", true)]
		static bool MenuVerifyMirrorObjects()
		{
			MirrorObjects instance = EditorToolbarLoader.GetInstance<MirrorObjects>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Object/Mirror Objects ", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuDoMirrorObjects()
		{
			MirrorObjects instance = EditorToolbarLoader.GetInstance<MirrorObjects>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Pro Builderize ", true)]
		static bool MenuVerifyProBuilderize()
		{
			ProBuilderize instance = EditorToolbarLoader.GetInstance<ProBuilderize>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Object/Pro Builderize ", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuDoProBuilderize()
		{
			ProBuilderize instance = EditorToolbarLoader.GetInstance<ProBuilderize>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Set Collider ", true)]
		static bool MenuVerifySetCollider()
		{
			SetCollider instance = EditorToolbarLoader.GetInstance<SetCollider>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Object/Set Collider ", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuDoSetCollider()
		{
			SetCollider instance = EditorToolbarLoader.GetInstance<SetCollider>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Set Trigger ", true)]
		static bool MenuVerifySetTrigger()
		{
			SetTrigger instance = EditorToolbarLoader.GetInstance<SetTrigger>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Object/Set Trigger ", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuDoSetTrigger()
		{
			SetTrigger instance = EditorToolbarLoader.GetInstance<SetTrigger>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Subdivide Object ", true)]
		static bool MenuVerifySubdivideObject()
		{
			SubdivideObject instance = EditorToolbarLoader.GetInstance<SubdivideObject>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Object/Subdivide Object ", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuDoSubdivideObject()
		{
			SubdivideObject instance = EditorToolbarLoader.GetInstance<SubdivideObject>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Triangulate Object ", true)]
		static bool MenuVerifyTriangulateObject()
		{
			TriangulateObject instance = EditorToolbarLoader.GetInstance<TriangulateObject>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Object/Triangulate Object ", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuDoTriangulateObject()
		{
			TriangulateObject instance = EditorToolbarLoader.GetInstance<TriangulateObject>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Grow Selection &g", true)]
		static bool MenuVerifyGrowSelection()
		{
			GrowSelection instance = EditorToolbarLoader.GetInstance<GrowSelection>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Grow Selection &g", false, PreferenceKeys.menuSelection + 0)]
		static void MenuDoGrowSelection()
		{
			GrowSelection instance = EditorToolbarLoader.GetInstance<GrowSelection>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Invert Selection %#i", true)]
		static bool MenuVerifyInvertSelection()
		{
			InvertSelection instance = EditorToolbarLoader.GetInstance<InvertSelection>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Invert Selection %#i", false, PreferenceKeys.menuSelection + 0)]
		static void MenuDoInvertSelection()
		{
			InvertSelection instance = EditorToolbarLoader.GetInstance<InvertSelection>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}





		[MenuItem(PB_MENU_PREFIX + "Selection/Select Hole ", true)]
		static bool MenuVerifySelectHole()
		{
			SelectHole instance = EditorToolbarLoader.GetInstance<SelectHole>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Select Hole ", false, PreferenceKeys.menuSelection + 0)]
		static void MenuDoSelectHole()
		{
			SelectHole instance = EditorToolbarLoader.GetInstance<SelectHole>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Select Material ", true)]
		static bool MenuVerifySelectMaterial()
		{
			SelectMaterial instance = EditorToolbarLoader.GetInstance<SelectMaterial>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Select Material ", false, PreferenceKeys.menuSelection + 0)]
		static void MenuDoSelectMaterial()
		{
			SelectMaterial instance = EditorToolbarLoader.GetInstance<SelectMaterial>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Select Smoothing Group ", true)]
		static bool MenuVerifySelectSmoothingGroup()
		{
			SelectSmoothingGroup instance = EditorToolbarLoader.GetInstance<SelectSmoothingGroup>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Select Smoothing Group ", false, PreferenceKeys.menuSelection + 0)]
		static void MenuDoSelectSmoothingGroup()
		{
			SelectSmoothingGroup instance = EditorToolbarLoader.GetInstance<SelectSmoothingGroup>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Select Vertex Color ", true)]
		static bool MenuVerifySelectVertexColor()
		{
			SelectVertexColor instance = EditorToolbarLoader.GetInstance<SelectVertexColor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Select Vertex Color ", false, PreferenceKeys.menuSelection + 0)]
		static void MenuDoSelectVertexColor()
		{
			SelectVertexColor instance = EditorToolbarLoader.GetInstance<SelectVertexColor>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Shrink Selection &#g", true)]
		static bool MenuVerifyShrinkSelection()
		{
			ShrinkSelection instance = EditorToolbarLoader.GetInstance<ShrinkSelection>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif

		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Shrink Selection &#g", false, PreferenceKeys.menuSelection + 0)]
		static void MenuDoShrinkSelection()
		{
			ShrinkSelection instance = EditorToolbarLoader.GetInstance<ShrinkSelection>();
			if(instance != null)
				EditorUtility.ShowNotification(instance.DoAction().notification);
		}

	}
}

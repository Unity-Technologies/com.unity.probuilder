/**
 *	IMPORTANT
 *
 *	This is a generated file. Any changes will be overwritten.
 *	See pb_GenerateMenuItems to make modifications.
 */

using UnityEngine;
using UnityEditor;
using ProBuilder.Core;
using ProBuilder.Actions;
using System.Collections.Generic;

namespace ProBuilder.EditorCore
{
	static class pb_EditorToolbarMenuItems
	{

#if PROTOTYPE
		const string PB_MENU_PREFIX = "Tools/ProBuilder Basic/";
#else
		const string PB_MENU_PREFIX = "Tools/ProBuilder/";
#endif

		[MenuItem(PB_MENU_PREFIX + "Editors/New Bezier Shape ", true)]
		static bool MenuVerifyNewBezierShape()
		{
			NewBezierShape instance = pb_EditorToolbarLoader.GetInstance<NewBezierShape>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/New Bezier Shape ", false, pb_Constant.MENU_EDITOR + 1)]
		static void MenuDoNewBezierShape()
		{
			NewBezierShape instance = pb_EditorToolbarLoader.GetInstance<NewBezierShape>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/New Poly Shape ", true)]
		static bool MenuVerifyNewPolyShape()
		{
			NewPolyShape instance = pb_EditorToolbarLoader.GetInstance<NewPolyShape>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/New Poly Shape ", false, pb_Constant.MENU_EDITOR + 1)]
		static void MenuDoNewPolyShape()
		{
			NewPolyShape instance = pb_EditorToolbarLoader.GetInstance<NewPolyShape>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Material Editor ", true)]
		static bool MenuVerifyOpenMaterialEditor()
		{
			OpenMaterialEditor instance = pb_EditorToolbarLoader.GetInstance<OpenMaterialEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Material Editor ", false, pb_Constant.MENU_EDITOR + 1)]
		static void MenuDoOpenMaterialEditor()
		{
			OpenMaterialEditor instance = pb_EditorToolbarLoader.GetInstance<OpenMaterialEditor>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Shape Editor %#k", true)]
		static bool MenuVerifyOpenShapeEditor()
		{
			OpenShapeEditor instance = pb_EditorToolbarLoader.GetInstance<OpenShapeEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Shape Editor %#k", false, pb_Constant.MENU_EDITOR + 1)]
		static void MenuDoOpenShapeEditor()
		{
			OpenShapeEditor instance = pb_EditorToolbarLoader.GetInstance<OpenShapeEditor>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Smoothing Editor ", true)]
		static bool MenuVerifyOpenSmoothingEditor()
		{
			OpenSmoothingEditor instance = pb_EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Smoothing Editor ", false, pb_Constant.MENU_EDITOR + 1)]
		static void MenuDoOpenSmoothingEditor()
		{
			OpenSmoothingEditor instance = pb_EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open UV Editor ", true)]
		static bool MenuVerifyOpenUVEditor()
		{
			OpenUVEditor instance = pb_EditorToolbarLoader.GetInstance<OpenUVEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open UV Editor ", false, pb_Constant.MENU_EDITOR + 1)]
		static void MenuDoOpenUVEditor()
		{
			OpenUVEditor instance = pb_EditorToolbarLoader.GetInstance<OpenUVEditor>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Vertex Color Editor ", true)]
		static bool MenuVerifyOpenVertexColorEditor()
		{
			OpenVertexColorEditor instance = pb_EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Vertex Color Editor ", false, pb_Constant.MENU_EDITOR + 1)]
		static void MenuDoOpenVertexColorEditor()
		{
			OpenVertexColorEditor instance = pb_EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Vertex Position Editor ", true)]
		static bool MenuVerifyOpenVertexPositionEditor()
		{
			OpenVertexPositionEditor instance = pb_EditorToolbarLoader.GetInstance<OpenVertexPositionEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Editors/Open Vertex Position Editor ", false, pb_Constant.MENU_EDITOR + 1)]
		static void MenuDoOpenVertexPositionEditor()
		{
			OpenVertexPositionEditor instance = pb_EditorToolbarLoader.GetInstance<OpenVertexPositionEditor>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}


		[MenuItem(PB_MENU_PREFIX + "Export/Export Asset ", true)]
		static bool MenuVerifyExportAsset()
		{
			ExportAsset instance = pb_EditorToolbarLoader.GetInstance<ExportAsset>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Asset ", false, pb_Constant.MENU_EXPORT + 0)]
		static void MenuDoExportAsset()
		{
			ExportAsset instance = pb_EditorToolbarLoader.GetInstance<ExportAsset>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Obj ", true)]
		static bool MenuVerifyExportObj()
		{
			ExportObj instance = pb_EditorToolbarLoader.GetInstance<ExportObj>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Obj ", false, pb_Constant.MENU_EXPORT + 0)]
		static void MenuDoExportObj()
		{
			ExportObj instance = pb_EditorToolbarLoader.GetInstance<ExportObj>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Ply ", true)]
		static bool MenuVerifyExportPly()
		{
			ExportPly instance = pb_EditorToolbarLoader.GetInstance<ExportPly>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Ply ", false, pb_Constant.MENU_EXPORT + 0)]
		static void MenuDoExportPly()
		{
			ExportPly instance = pb_EditorToolbarLoader.GetInstance<ExportPly>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Stl Ascii ", true)]
		static bool MenuVerifyExportStlAscii()
		{
			ExportStlAscii instance = pb_EditorToolbarLoader.GetInstance<ExportStlAscii>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Stl Ascii ", false, pb_Constant.MENU_EXPORT + 0)]
		static void MenuDoExportStlAscii()
		{
			ExportStlAscii instance = pb_EditorToolbarLoader.GetInstance<ExportStlAscii>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Stl Binary ", true)]
		static bool MenuVerifyExportStlBinary()
		{
			ExportStlBinary instance = pb_EditorToolbarLoader.GetInstance<ExportStlBinary>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Export/Export Stl Binary ", false, pb_Constant.MENU_EXPORT + 0)]
		static void MenuDoExportStlBinary()
		{
			ExportStlBinary instance = pb_EditorToolbarLoader.GetInstance<ExportStlBinary>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Bevel Edges ", true)]
		static bool MenuVerifyBevelEdges()
		{
			BevelEdges instance = pb_EditorToolbarLoader.GetInstance<BevelEdges>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Bevel Edges ", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoBevelEdges()
		{
			BevelEdges instance = pb_EditorToolbarLoader.GetInstance<BevelEdges>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Bridge Edges &b", true)]
		static bool MenuVerifyBridgeEdges()
		{
			BridgeEdges instance = pb_EditorToolbarLoader.GetInstance<BridgeEdges>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Bridge Edges &b", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoBridgeEdges()
		{
			BridgeEdges instance = pb_EditorToolbarLoader.GetInstance<BridgeEdges>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Collapse Vertices &c", true)]
		static bool MenuVerifyCollapseVertices()
		{
			CollapseVertices instance = pb_EditorToolbarLoader.GetInstance<CollapseVertices>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Collapse Vertices &c", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoCollapseVertices()
		{
			CollapseVertices instance = pb_EditorToolbarLoader.GetInstance<CollapseVertices>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Conform Face Normals ", true)]
		static bool MenuVerifyConformFaceNormals()
		{
			ConformFaceNormals instance = pb_EditorToolbarLoader.GetInstance<ConformFaceNormals>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Conform Face Normals ", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoConformFaceNormals()
		{
			ConformFaceNormals instance = pb_EditorToolbarLoader.GetInstance<ConformFaceNormals>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}



		[MenuItem(PB_MENU_PREFIX + "Geometry/Delete Faces  [delete]", true)]
		static bool MenuVerifyDeleteFaces()
		{
			DeleteFaces instance = pb_EditorToolbarLoader.GetInstance<DeleteFaces>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Delete Faces  [delete]", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoDeleteFaces()
		{
			DeleteFaces instance = pb_EditorToolbarLoader.GetInstance<DeleteFaces>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Detach Faces ", true)]
		static bool MenuVerifyDetachFaces()
		{
			DetachFaces instance = pb_EditorToolbarLoader.GetInstance<DetachFaces>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Detach Faces ", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoDetachFaces()
		{
			DetachFaces instance = pb_EditorToolbarLoader.GetInstance<DetachFaces>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}



		[MenuItem(PB_MENU_PREFIX + "Geometry/Fill Hole ", true)]
		static bool MenuVerifyFillHole()
		{
			FillHole instance = pb_EditorToolbarLoader.GetInstance<FillHole>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Fill Hole ", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoFillHole()
		{
			FillHole instance = pb_EditorToolbarLoader.GetInstance<FillHole>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Flip Face Edge ", true)]
		static bool MenuVerifyFlipFaceEdge()
		{
			FlipFaceEdge instance = pb_EditorToolbarLoader.GetInstance<FlipFaceEdge>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Flip Face Edge ", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoFlipFaceEdge()
		{
			FlipFaceEdge instance = pb_EditorToolbarLoader.GetInstance<FlipFaceEdge>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Flip Face Normals &n", true)]
		static bool MenuVerifyFlipFaceNormals()
		{
			FlipFaceNormals instance = pb_EditorToolbarLoader.GetInstance<FlipFaceNormals>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Flip Face Normals &n", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoFlipFaceNormals()
		{
			FlipFaceNormals instance = pb_EditorToolbarLoader.GetInstance<FlipFaceNormals>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Insert Edge Loop &u", true)]
		static bool MenuVerifyInsertEdgeLoop()
		{
			InsertEdgeLoop instance = pb_EditorToolbarLoader.GetInstance<InsertEdgeLoop>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Insert Edge Loop &u", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoInsertEdgeLoop()
		{
			InsertEdgeLoop instance = pb_EditorToolbarLoader.GetInstance<InsertEdgeLoop>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Merge Faces ", true)]
		static bool MenuVerifyMergeFaces()
		{
			MergeFaces instance = pb_EditorToolbarLoader.GetInstance<MergeFaces>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Merge Faces ", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoMergeFaces()
		{
			MergeFaces instance = pb_EditorToolbarLoader.GetInstance<MergeFaces>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Set Pivot To Selection %j", true)]
		static bool MenuVerifySetPivotToSelection()
		{
			SetPivotToSelection instance = pb_EditorToolbarLoader.GetInstance<SetPivotToSelection>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Set Pivot To Selection %j", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoSetPivotToSelection()
		{
			SetPivotToSelection instance = pb_EditorToolbarLoader.GetInstance<SetPivotToSelection>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Smart Connect &e", true)]
		static bool MenuVerifySmartConnect()
		{
			SmartConnect instance = pb_EditorToolbarLoader.GetInstance<SmartConnect>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Smart Connect &e", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoSmartConnect()
		{
			SmartConnect instance = pb_EditorToolbarLoader.GetInstance<SmartConnect>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Smart Subdivide &s", true)]
		static bool MenuVerifySmartSubdivide()
		{
			SmartSubdivide instance = pb_EditorToolbarLoader.GetInstance<SmartSubdivide>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Smart Subdivide &s", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoSmartSubdivide()
		{
			SmartSubdivide instance = pb_EditorToolbarLoader.GetInstance<SmartSubdivide>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Split Vertices &x", true)]
		static bool MenuVerifySplitVertices()
		{
			SplitVertices instance = pb_EditorToolbarLoader.GetInstance<SplitVertices>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Split Vertices &x", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoSplitVertices()
		{
			SplitVertices instance = pb_EditorToolbarLoader.GetInstance<SplitVertices>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}



		[MenuItem(PB_MENU_PREFIX + "Geometry/Triangulate Faces ", true)]
		static bool MenuVerifyTriangulateFaces()
		{
			TriangulateFaces instance = pb_EditorToolbarLoader.GetInstance<TriangulateFaces>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Triangulate Faces ", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoTriangulateFaces()
		{
			TriangulateFaces instance = pb_EditorToolbarLoader.GetInstance<TriangulateFaces>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Weld Vertices &v", true)]
		static bool MenuVerifyWeldVertices()
		{
			WeldVertices instance = pb_EditorToolbarLoader.GetInstance<WeldVertices>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Geometry/Weld Vertices &v", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoWeldVertices()
		{
			WeldVertices instance = pb_EditorToolbarLoader.GetInstance<WeldVertices>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Interaction/Toggle Drag Rect Mode ", true)]
		static bool MenuVerifyToggleDragRectMode()
		{
			ToggleDragRectMode instance = pb_EditorToolbarLoader.GetInstance<ToggleDragRectMode>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Interaction/Toggle Drag Rect Mode ", false, pb_Constant.MENU_SELECTION + 1)]
		static void MenuDoToggleDragRectMode()
		{
			ToggleDragRectMode instance = pb_EditorToolbarLoader.GetInstance<ToggleDragRectMode>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Interaction/Toggle Drag Selection Mode ", true)]
		static bool MenuVerifyToggleDragSelectionMode()
		{
			ToggleDragSelectionMode instance = pb_EditorToolbarLoader.GetInstance<ToggleDragSelectionMode>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Interaction/Toggle Drag Selection Mode ", false, pb_Constant.MENU_SELECTION + 1)]
		static void MenuDoToggleDragSelectionMode()
		{
			ToggleDragSelectionMode instance = pb_EditorToolbarLoader.GetInstance<ToggleDragSelectionMode>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Interaction/Toggle Handle Alignment  [p]", true)]
		static bool MenuVerifyToggleHandleAlignment()
		{
			ToggleHandleAlignment instance = pb_EditorToolbarLoader.GetInstance<ToggleHandleAlignment>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Interaction/Toggle Handle Alignment  [p]", false, pb_Constant.MENU_SELECTION + 1)]
		static void MenuDoToggleHandleAlignment()
		{
			ToggleHandleAlignment instance = pb_EditorToolbarLoader.GetInstance<ToggleHandleAlignment>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Interaction/Toggle Select Back Faces ", true)]
		static bool MenuVerifyToggleSelectBackFaces()
		{
			ToggleSelectBackFaces instance = pb_EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Interaction/Toggle Select Back Faces ", false, pb_Constant.MENU_SELECTION + 1)]
		static void MenuDoToggleSelectBackFaces()
		{
			ToggleSelectBackFaces instance = pb_EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Center Pivot ", true)]
		static bool MenuVerifyCenterPivot()
		{
			CenterPivot instance = pb_EditorToolbarLoader.GetInstance<CenterPivot>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Center Pivot ", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoCenterPivot()
		{
			CenterPivot instance = pb_EditorToolbarLoader.GetInstance<CenterPivot>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Conform Object Normals ", true)]
		static bool MenuVerifyConformObjectNormals()
		{
			ConformObjectNormals instance = pb_EditorToolbarLoader.GetInstance<ConformObjectNormals>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Conform Object Normals ", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoConformObjectNormals()
		{
			ConformObjectNormals instance = pb_EditorToolbarLoader.GetInstance<ConformObjectNormals>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Flip Object Normals ", true)]
		static bool MenuVerifyFlipObjectNormals()
		{
			FlipObjectNormals instance = pb_EditorToolbarLoader.GetInstance<FlipObjectNormals>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Flip Object Normals ", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoFlipObjectNormals()
		{
			FlipObjectNormals instance = pb_EditorToolbarLoader.GetInstance<FlipObjectNormals>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Freeze Transform ", true)]
		static bool MenuVerifyFreezeTransform()
		{
			FreezeTransform instance = pb_EditorToolbarLoader.GetInstance<FreezeTransform>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Freeze Transform ", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoFreezeTransform()
		{
			FreezeTransform instance = pb_EditorToolbarLoader.GetInstance<FreezeTransform>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}


		[MenuItem(PB_MENU_PREFIX + "Object/Merge Objects ", true)]
		static bool MenuVerifyMergeObjects()
		{
			MergeObjects instance = pb_EditorToolbarLoader.GetInstance<MergeObjects>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Merge Objects ", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoMergeObjects()
		{
			MergeObjects instance = pb_EditorToolbarLoader.GetInstance<MergeObjects>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Mirror Objects ", true)]
		static bool MenuVerifyMirrorObjects()
		{
			MirrorObjects instance = pb_EditorToolbarLoader.GetInstance<MirrorObjects>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Mirror Objects ", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoMirrorObjects()
		{
			MirrorObjects instance = pb_EditorToolbarLoader.GetInstance<MirrorObjects>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Pro Builderize ", true)]
		static bool MenuVerifyProBuilderize()
		{
			ProBuilderize instance = pb_EditorToolbarLoader.GetInstance<ProBuilderize>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Pro Builderize ", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoProBuilderize()
		{
			ProBuilderize instance = pb_EditorToolbarLoader.GetInstance<ProBuilderize>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Set Collider ", true)]
		static bool MenuVerifySetCollider()
		{
			SetCollider instance = pb_EditorToolbarLoader.GetInstance<SetCollider>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Set Collider ", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoSetCollider()
		{
			SetCollider instance = pb_EditorToolbarLoader.GetInstance<SetCollider>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Set Trigger ", true)]
		static bool MenuVerifySetTrigger()
		{
			SetTrigger instance = pb_EditorToolbarLoader.GetInstance<SetTrigger>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Set Trigger ", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoSetTrigger()
		{
			SetTrigger instance = pb_EditorToolbarLoader.GetInstance<SetTrigger>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Subdivide Object ", true)]
		static bool MenuVerifySubdivideObject()
		{
			SubdivideObject instance = pb_EditorToolbarLoader.GetInstance<SubdivideObject>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Subdivide Object ", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoSubdivideObject()
		{
			SubdivideObject instance = pb_EditorToolbarLoader.GetInstance<SubdivideObject>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Triangulate Object ", true)]
		static bool MenuVerifyTriangulateObject()
		{
			TriangulateObject instance = pb_EditorToolbarLoader.GetInstance<TriangulateObject>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Object/Triangulate Object ", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoTriangulateObject()
		{
			TriangulateObject instance = pb_EditorToolbarLoader.GetInstance<TriangulateObject>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Grow Selection &g", true)]
		static bool MenuVerifyGrowSelection()
		{
			GrowSelection instance = pb_EditorToolbarLoader.GetInstance<GrowSelection>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Grow Selection &g", false, pb_Constant.MENU_SELECTION + 0)]
		static void MenuDoGrowSelection()
		{
			GrowSelection instance = pb_EditorToolbarLoader.GetInstance<GrowSelection>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Invert Selection %#i", true)]
		static bool MenuVerifyInvertSelection()
		{
			InvertSelection instance = pb_EditorToolbarLoader.GetInstance<InvertSelection>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Invert Selection %#i", false, pb_Constant.MENU_SELECTION + 0)]
		static void MenuDoInvertSelection()
		{
			InvertSelection instance = pb_EditorToolbarLoader.GetInstance<InvertSelection>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}





		[MenuItem(PB_MENU_PREFIX + "Selection/Select Hole ", true)]
		static bool MenuVerifySelectHole()
		{
			SelectHole instance = pb_EditorToolbarLoader.GetInstance<SelectHole>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Select Hole ", false, pb_Constant.MENU_SELECTION + 0)]
		static void MenuDoSelectHole()
		{
			SelectHole instance = pb_EditorToolbarLoader.GetInstance<SelectHole>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Select Material ", true)]
		static bool MenuVerifySelectMaterial()
		{
			SelectMaterial instance = pb_EditorToolbarLoader.GetInstance<SelectMaterial>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Select Material ", false, pb_Constant.MENU_SELECTION + 0)]
		static void MenuDoSelectMaterial()
		{
			SelectMaterial instance = pb_EditorToolbarLoader.GetInstance<SelectMaterial>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Select Smoothing Group ", true)]
		static bool MenuVerifySelectSmoothingGroup()
		{
			SelectSmoothingGroup instance = pb_EditorToolbarLoader.GetInstance<SelectSmoothingGroup>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Select Smoothing Group ", false, pb_Constant.MENU_SELECTION + 0)]
		static void MenuDoSelectSmoothingGroup()
		{
			SelectSmoothingGroup instance = pb_EditorToolbarLoader.GetInstance<SelectSmoothingGroup>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Select Vertex Color ", true)]
		static bool MenuVerifySelectVertexColor()
		{
			SelectVertexColor instance = pb_EditorToolbarLoader.GetInstance<SelectVertexColor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Select Vertex Color ", false, pb_Constant.MENU_SELECTION + 0)]
		static void MenuDoSelectVertexColor()
		{
			SelectVertexColor instance = pb_EditorToolbarLoader.GetInstance<SelectVertexColor>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Shrink Selection &#g", true)]
		static bool MenuVerifyShrinkSelection()
		{
			ShrinkSelection instance = pb_EditorToolbarLoader.GetInstance<ShrinkSelection>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.IsEnabled();
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "Selection/Shrink Selection &#g", false, pb_Constant.MENU_SELECTION + 0)]
		static void MenuDoShrinkSelection()
		{
			ShrinkSelection instance = pb_EditorToolbarLoader.GetInstance<ShrinkSelection>();
			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}

	}
}

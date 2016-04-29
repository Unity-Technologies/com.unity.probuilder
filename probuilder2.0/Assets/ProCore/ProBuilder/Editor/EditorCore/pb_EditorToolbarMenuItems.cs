/**
 *	IMPORTANT
 *
 *	This is a generated file. Any changes will be overwritten.
 *	See pb_GenerateMenuItems.
 */
using UnityEngine;
using UnityEditor;
using ProBuilder2.Actions;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	public class pb_EditorToolbarMenuItems : Editor
	{

		[MenuItem("Tools/ProBuilder/Editors/Open Material Editor", true)]
		static bool MenuVerifyOpenMaterialEditor()
		{
			OpenMaterialEditor instance = pb_EditorToolbarLoader.GetInstance<OpenMaterialEditor>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Editors/Open Material Editor", false, pb_Constant.MENU_EDITOR + 1)]
		static void MenuDoOpenMaterialEditor()
		{
			OpenMaterialEditor instance = pb_EditorToolbarLoader.GetInstance<OpenMaterialEditor>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Editors/Open Mirror Objects Editor", true)]
		static bool MenuVerifyOpenMirrorObjectsEditor()
		{
			OpenMirrorObjectsEditor instance = pb_EditorToolbarLoader.GetInstance<OpenMirrorObjectsEditor>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Editors/Open Mirror Objects Editor", false, pb_Constant.MENU_EDITOR + 1)]
		static void MenuDoOpenMirrorObjectsEditor()
		{
			OpenMirrorObjectsEditor instance = pb_EditorToolbarLoader.GetInstance<OpenMirrorObjectsEditor>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Editors/Open Shape Editor", true)]
		static bool MenuVerifyOpenShapeEditor()
		{
			OpenShapeEditor instance = pb_EditorToolbarLoader.GetInstance<OpenShapeEditor>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Editors/Open Shape Editor", false, pb_Constant.MENU_EDITOR + 1)]
		static void MenuDoOpenShapeEditor()
		{
			OpenShapeEditor instance = pb_EditorToolbarLoader.GetInstance<OpenShapeEditor>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Editors/Open Smoothing Editor", true)]
		static bool MenuVerifyOpenSmoothingEditor()
		{
			OpenSmoothingEditor instance = pb_EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Editors/Open Smoothing Editor", false, pb_Constant.MENU_EDITOR + 1)]
		static void MenuDoOpenSmoothingEditor()
		{
			OpenSmoothingEditor instance = pb_EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Editors/Open UV Editor", true)]
		static bool MenuVerifyOpenUVEditor()
		{
			OpenUVEditor instance = pb_EditorToolbarLoader.GetInstance<OpenUVEditor>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Editors/Open UV Editor", false, pb_Constant.MENU_EDITOR + 1)]
		static void MenuDoOpenUVEditor()
		{
			OpenUVEditor instance = pb_EditorToolbarLoader.GetInstance<OpenUVEditor>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Editors/Open Vertex Color Editor", true)]
		static bool MenuVerifyOpenVertexColorEditor()
		{
			OpenVertexColorEditor instance = pb_EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Editors/Open Vertex Color Editor", false, pb_Constant.MENU_EDITOR + 1)]
		static void MenuDoOpenVertexColorEditor()
		{
			OpenVertexColorEditor instance = pb_EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Bridge Edges", true)]
		static bool MenuVerifyBridgeEdges()
		{
			BridgeEdges instance = pb_EditorToolbarLoader.GetInstance<BridgeEdges>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Bridge Edges", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoBridgeEdges()
		{
			BridgeEdges instance = pb_EditorToolbarLoader.GetInstance<BridgeEdges>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Collapse Vertices", true)]
		static bool MenuVerifyCollapseVertices()
		{
			CollapseVertices instance = pb_EditorToolbarLoader.GetInstance<CollapseVertices>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Collapse Vertices", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoCollapseVertices()
		{
			CollapseVertices instance = pb_EditorToolbarLoader.GetInstance<CollapseVertices>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Conform Face Normals", true)]
		static bool MenuVerifyConformFaceNormals()
		{
			ConformFaceNormals instance = pb_EditorToolbarLoader.GetInstance<ConformFaceNormals>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Conform Face Normals", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoConformFaceNormals()
		{
			ConformFaceNormals instance = pb_EditorToolbarLoader.GetInstance<ConformFaceNormals>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Connect Edges", true)]
		static bool MenuVerifyConnectEdges()
		{
			ConnectEdges instance = pb_EditorToolbarLoader.GetInstance<ConnectEdges>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Connect Edges", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoConnectEdges()
		{
			ConnectEdges instance = pb_EditorToolbarLoader.GetInstance<ConnectEdges>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Connect Vertices", true)]
		static bool MenuVerifyConnectVertices()
		{
			ConnectVertices instance = pb_EditorToolbarLoader.GetInstance<ConnectVertices>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Connect Vertices", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoConnectVertices()
		{
			ConnectVertices instance = pb_EditorToolbarLoader.GetInstance<ConnectVertices>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Delete Faces", true)]
		static bool MenuVerifyDeleteFaces()
		{
			DeleteFaces instance = pb_EditorToolbarLoader.GetInstance<DeleteFaces>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Delete Faces", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoDeleteFaces()
		{
			DeleteFaces instance = pb_EditorToolbarLoader.GetInstance<DeleteFaces>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Detach Faces", true)]
		static bool MenuVerifyDetachFaces()
		{
			DetachFaces instance = pb_EditorToolbarLoader.GetInstance<DetachFaces>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Detach Faces", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoDetachFaces()
		{
			DetachFaces instance = pb_EditorToolbarLoader.GetInstance<DetachFaces>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Extrude Edges", true)]
		static bool MenuVerifyExtrudeEdges()
		{
			ExtrudeEdges instance = pb_EditorToolbarLoader.GetInstance<ExtrudeEdges>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Extrude Edges", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoExtrudeEdges()
		{
			ExtrudeEdges instance = pb_EditorToolbarLoader.GetInstance<ExtrudeEdges>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Extrude Faces", true)]
		static bool MenuVerifyExtrudeFaces()
		{
			ExtrudeFaces instance = pb_EditorToolbarLoader.GetInstance<ExtrudeFaces>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Extrude Faces", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoExtrudeFaces()
		{
			ExtrudeFaces instance = pb_EditorToolbarLoader.GetInstance<ExtrudeFaces>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Flip Face Edge", true)]
		static bool MenuVerifyFlipFaceEdge()
		{
			FlipFaceEdge instance = pb_EditorToolbarLoader.GetInstance<FlipFaceEdge>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Flip Face Edge", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoFlipFaceEdge()
		{
			FlipFaceEdge instance = pb_EditorToolbarLoader.GetInstance<FlipFaceEdge>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Flip Face Normals", true)]
		static bool MenuVerifyFlipFaceNormals()
		{
			FlipFaceNormals instance = pb_EditorToolbarLoader.GetInstance<FlipFaceNormals>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Flip Face Normals", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoFlipFaceNormals()
		{
			FlipFaceNormals instance = pb_EditorToolbarLoader.GetInstance<FlipFaceNormals>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Insert Edge Loop", true)]
		static bool MenuVerifyInsertEdgeLoop()
		{
			InsertEdgeLoop instance = pb_EditorToolbarLoader.GetInstance<InsertEdgeLoop>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Insert Edge Loop", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoInsertEdgeLoop()
		{
			InsertEdgeLoop instance = pb_EditorToolbarLoader.GetInstance<InsertEdgeLoop>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Merge Faces", true)]
		static bool MenuVerifyMergeFaces()
		{
			MergeFaces instance = pb_EditorToolbarLoader.GetInstance<MergeFaces>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Merge Faces", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoMergeFaces()
		{
			MergeFaces instance = pb_EditorToolbarLoader.GetInstance<MergeFaces>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Set Pivot To Selection", true)]
		static bool MenuVerifySetPivotToSelection()
		{
			SetPivotToSelection instance = pb_EditorToolbarLoader.GetInstance<SetPivotToSelection>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Set Pivot To Selection", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoSetPivotToSelection()
		{
			SetPivotToSelection instance = pb_EditorToolbarLoader.GetInstance<SetPivotToSelection>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Split Vertices", true)]
		static bool MenuVerifySplitVertices()
		{
			SplitVertices instance = pb_EditorToolbarLoader.GetInstance<SplitVertices>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Split Vertices", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoSplitVertices()
		{
			SplitVertices instance = pb_EditorToolbarLoader.GetInstance<SplitVertices>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Subdivide Edges", true)]
		static bool MenuVerifySubdivideEdges()
		{
			SubdivideEdges instance = pb_EditorToolbarLoader.GetInstance<SubdivideEdges>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Subdivide Edges", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoSubdivideEdges()
		{
			SubdivideEdges instance = pb_EditorToolbarLoader.GetInstance<SubdivideEdges>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Subdivide Faces", true)]
		static bool MenuVerifySubdivideFaces()
		{
			SubdivideFaces instance = pb_EditorToolbarLoader.GetInstance<SubdivideFaces>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Subdivide Faces", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoSubdivideFaces()
		{
			SubdivideFaces instance = pb_EditorToolbarLoader.GetInstance<SubdivideFaces>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/Weld Vertices", true)]
		static bool MenuVerifyWeldVertices()
		{
			WeldVertices instance = pb_EditorToolbarLoader.GetInstance<WeldVertices>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/Weld Vertices", false, pb_Constant.MENU_GEOMETRY + 3)]
		static void MenuDoWeldVertices()
		{
			WeldVertices instance = pb_EditorToolbarLoader.GetInstance<WeldVertices>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Interaction/Toggle Handle Alignment", true)]
		static bool MenuVerifyToggleHandleAlignment()
		{
			ToggleHandleAlignment instance = pb_EditorToolbarLoader.GetInstance<ToggleHandleAlignment>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Interaction/Toggle Handle Alignment", false, pb_Constant.MENU_SELECTION + 1)]
		static void MenuDoToggleHandleAlignment()
		{
			ToggleHandleAlignment instance = pb_EditorToolbarLoader.GetInstance<ToggleHandleAlignment>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Interaction/Toggle Select Back Faces", true)]
		static bool MenuVerifyToggleSelectBackFaces()
		{
			ToggleSelectBackFaces instance = pb_EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Interaction/Toggle Select Back Faces", false, pb_Constant.MENU_SELECTION + 1)]
		static void MenuDoToggleSelectBackFaces()
		{
			ToggleSelectBackFaces instance = pb_EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Object/Center Pivot", true)]
		static bool MenuVerifyCenterPivot()
		{
			CenterPivot instance = pb_EditorToolbarLoader.GetInstance<CenterPivot>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Object/Center Pivot", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoCenterPivot()
		{
			CenterPivot instance = pb_EditorToolbarLoader.GetInstance<CenterPivot>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Object/Conform Object Normals", true)]
		static bool MenuVerifyConformObjectNormals()
		{
			ConformObjectNormals instance = pb_EditorToolbarLoader.GetInstance<ConformObjectNormals>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Object/Conform Object Normals", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoConformObjectNormals()
		{
			ConformObjectNormals instance = pb_EditorToolbarLoader.GetInstance<ConformObjectNormals>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Object/Flip Object Normals", true)]
		static bool MenuVerifyFlipObjectNormals()
		{
			FlipObjectNormals instance = pb_EditorToolbarLoader.GetInstance<FlipObjectNormals>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Object/Flip Object Normals", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoFlipObjectNormals()
		{
			FlipObjectNormals instance = pb_EditorToolbarLoader.GetInstance<FlipObjectNormals>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Object/Freeze Transform", true)]
		static bool MenuVerifyFreezeTransform()
		{
			FreezeTransform instance = pb_EditorToolbarLoader.GetInstance<FreezeTransform>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Object/Freeze Transform", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoFreezeTransform()
		{
			FreezeTransform instance = pb_EditorToolbarLoader.GetInstance<FreezeTransform>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Object/Merge Objects", true)]
		static bool MenuVerifyMergeObjects()
		{
			MergeObjects instance = pb_EditorToolbarLoader.GetInstance<MergeObjects>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Object/Merge Objects", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoMergeObjects()
		{
			MergeObjects instance = pb_EditorToolbarLoader.GetInstance<MergeObjects>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Object/Subdivide Object", true)]
		static bool MenuVerifySubdivideObject()
		{
			SubdivideObject instance = pb_EditorToolbarLoader.GetInstance<SubdivideObject>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Object/Subdivide Object", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoSubdivideObject()
		{
			SubdivideObject instance = pb_EditorToolbarLoader.GetInstance<SubdivideObject>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Object/Triangulate Object", true)]
		static bool MenuVerifyTriangulateObject()
		{
			TriangulateObject instance = pb_EditorToolbarLoader.GetInstance<TriangulateObject>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Object/Triangulate Object", false, pb_Constant.MENU_GEOMETRY + 2)]
		static void MenuDoTriangulateObject()
		{
			TriangulateObject instance = pb_EditorToolbarLoader.GetInstance<TriangulateObject>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Selection/Grow Selection", true)]
		static bool MenuVerifyGrowSelection()
		{
			GrowSelection instance = pb_EditorToolbarLoader.GetInstance<GrowSelection>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Selection/Grow Selection", false, pb_Constant.MENU_SELECTION + 0)]
		static void MenuDoGrowSelection()
		{
			GrowSelection instance = pb_EditorToolbarLoader.GetInstance<GrowSelection>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Selection/Invert Selection", true)]
		static bool MenuVerifyInvertSelection()
		{
			InvertSelection instance = pb_EditorToolbarLoader.GetInstance<InvertSelection>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Selection/Invert Selection", false, pb_Constant.MENU_SELECTION + 0)]
		static void MenuDoInvertSelection()
		{
			InvertSelection instance = pb_EditorToolbarLoader.GetInstance<InvertSelection>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Selection/Select Edge Loop", true)]
		static bool MenuVerifySelectEdgeLoop()
		{
			SelectEdgeLoop instance = pb_EditorToolbarLoader.GetInstance<SelectEdgeLoop>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Selection/Select Edge Loop", false, pb_Constant.MENU_SELECTION + 0)]
		static void MenuDoSelectEdgeLoop()
		{
			SelectEdgeLoop instance = pb_EditorToolbarLoader.GetInstance<SelectEdgeLoop>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Selection/Select Edge Ring", true)]
		static bool MenuVerifySelectEdgeRing()
		{
			SelectEdgeRing instance = pb_EditorToolbarLoader.GetInstance<SelectEdgeRing>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Selection/Select Edge Ring", false, pb_Constant.MENU_SELECTION + 0)]
		static void MenuDoSelectEdgeRing()
		{
			SelectEdgeRing instance = pb_EditorToolbarLoader.GetInstance<SelectEdgeRing>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Selection/Shrink Selection", true)]
		static bool MenuVerifyShrinkSelection()
		{
			ShrinkSelection instance = pb_EditorToolbarLoader.GetInstance<ShrinkSelection>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Selection/Shrink Selection", false, pb_Constant.MENU_SELECTION + 0)]
		static void MenuDoShrinkSelection()
		{
			ShrinkSelection instance = pb_EditorToolbarLoader.GetInstance<ShrinkSelection>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

	}
}

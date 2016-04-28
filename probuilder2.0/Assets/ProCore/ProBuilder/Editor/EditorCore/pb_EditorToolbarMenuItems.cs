using UnityEngine;
using UnityEditor;
using ProBuilder2.Actions;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	public class pb_EditorToolbarMenuItems : Editor
	{

		[MenuItem("Tools/ProBuilder/Editors/OpenMaterialEditor", true)]
		static bool MenuVerifyOpenMaterialEditor()
		{
			OpenMaterialEditor instance = pb_EditorToolbarLoader.GetInstance<OpenMaterialEditor>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Editors/OpenMaterialEditor", false)]
		static void MenuDoOpenMaterialEditor()
		{
			OpenMaterialEditor instance = pb_EditorToolbarLoader.GetInstance<OpenMaterialEditor>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Editors/OpenMirrorObjectsEditor", true)]
		static bool MenuVerifyOpenMirrorObjectsEditor()
		{
			OpenMirrorObjectsEditor instance = pb_EditorToolbarLoader.GetInstance<OpenMirrorObjectsEditor>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Editors/OpenMirrorObjectsEditor", false)]
		static void MenuDoOpenMirrorObjectsEditor()
		{
			OpenMirrorObjectsEditor instance = pb_EditorToolbarLoader.GetInstance<OpenMirrorObjectsEditor>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Editors/OpenShapeEditor", true)]
		static bool MenuVerifyOpenShapeEditor()
		{
			OpenShapeEditor instance = pb_EditorToolbarLoader.GetInstance<OpenShapeEditor>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Editors/OpenShapeEditor", false)]
		static void MenuDoOpenShapeEditor()
		{
			OpenShapeEditor instance = pb_EditorToolbarLoader.GetInstance<OpenShapeEditor>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Editors/OpenSmoothingEditor", true)]
		static bool MenuVerifyOpenSmoothingEditor()
		{
			OpenSmoothingEditor instance = pb_EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Editors/OpenSmoothingEditor", false)]
		static void MenuDoOpenSmoothingEditor()
		{
			OpenSmoothingEditor instance = pb_EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Editors/OpenUVEditor", true)]
		static bool MenuVerifyOpenUVEditor()
		{
			OpenUVEditor instance = pb_EditorToolbarLoader.GetInstance<OpenUVEditor>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Editors/OpenUVEditor", false)]
		static void MenuDoOpenUVEditor()
		{
			OpenUVEditor instance = pb_EditorToolbarLoader.GetInstance<OpenUVEditor>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Editors/OpenVertexColorEditor", true)]
		static bool MenuVerifyOpenVertexColorEditor()
		{
			OpenVertexColorEditor instance = pb_EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Editors/OpenVertexColorEditor", false)]
		static void MenuDoOpenVertexColorEditor()
		{
			OpenVertexColorEditor instance = pb_EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/BridgeEdges", true)]
		static bool MenuVerifyBridgeEdges()
		{
			BridgeEdges instance = pb_EditorToolbarLoader.GetInstance<BridgeEdges>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/BridgeEdges", false)]
		static void MenuDoBridgeEdges()
		{
			BridgeEdges instance = pb_EditorToolbarLoader.GetInstance<BridgeEdges>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/CollapseVertices", true)]
		static bool MenuVerifyCollapseVertices()
		{
			CollapseVertices instance = pb_EditorToolbarLoader.GetInstance<CollapseVertices>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/CollapseVertices", false)]
		static void MenuDoCollapseVertices()
		{
			CollapseVertices instance = pb_EditorToolbarLoader.GetInstance<CollapseVertices>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/ConformFaceNormals", true)]
		static bool MenuVerifyConformFaceNormals()
		{
			ConformFaceNormals instance = pb_EditorToolbarLoader.GetInstance<ConformFaceNormals>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/ConformFaceNormals", false)]
		static void MenuDoConformFaceNormals()
		{
			ConformFaceNormals instance = pb_EditorToolbarLoader.GetInstance<ConformFaceNormals>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/ConnectEdges", true)]
		static bool MenuVerifyConnectEdges()
		{
			ConnectEdges instance = pb_EditorToolbarLoader.GetInstance<ConnectEdges>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/ConnectEdges", false)]
		static void MenuDoConnectEdges()
		{
			ConnectEdges instance = pb_EditorToolbarLoader.GetInstance<ConnectEdges>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/ConnectVertices", true)]
		static bool MenuVerifyConnectVertices()
		{
			ConnectVertices instance = pb_EditorToolbarLoader.GetInstance<ConnectVertices>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/ConnectVertices", false)]
		static void MenuDoConnectVertices()
		{
			ConnectVertices instance = pb_EditorToolbarLoader.GetInstance<ConnectVertices>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/DeleteFaces", true)]
		static bool MenuVerifyDeleteFaces()
		{
			DeleteFaces instance = pb_EditorToolbarLoader.GetInstance<DeleteFaces>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/DeleteFaces", false)]
		static void MenuDoDeleteFaces()
		{
			DeleteFaces instance = pb_EditorToolbarLoader.GetInstance<DeleteFaces>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/DetachFaces", true)]
		static bool MenuVerifyDetachFaces()
		{
			DetachFaces instance = pb_EditorToolbarLoader.GetInstance<DetachFaces>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/DetachFaces", false)]
		static void MenuDoDetachFaces()
		{
			DetachFaces instance = pb_EditorToolbarLoader.GetInstance<DetachFaces>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/ExtrudeEdges", true)]
		static bool MenuVerifyExtrudeEdges()
		{
			ExtrudeEdges instance = pb_EditorToolbarLoader.GetInstance<ExtrudeEdges>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/ExtrudeEdges", false)]
		static void MenuDoExtrudeEdges()
		{
			ExtrudeEdges instance = pb_EditorToolbarLoader.GetInstance<ExtrudeEdges>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/ExtrudeFaces", true)]
		static bool MenuVerifyExtrudeFaces()
		{
			ExtrudeFaces instance = pb_EditorToolbarLoader.GetInstance<ExtrudeFaces>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/ExtrudeFaces", false)]
		static void MenuDoExtrudeFaces()
		{
			ExtrudeFaces instance = pb_EditorToolbarLoader.GetInstance<ExtrudeFaces>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/FlipFaceEdge", true)]
		static bool MenuVerifyFlipFaceEdge()
		{
			FlipFaceEdge instance = pb_EditorToolbarLoader.GetInstance<FlipFaceEdge>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/FlipFaceEdge", false)]
		static void MenuDoFlipFaceEdge()
		{
			FlipFaceEdge instance = pb_EditorToolbarLoader.GetInstance<FlipFaceEdge>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/FlipFaceNormals", true)]
		static bool MenuVerifyFlipFaceNormals()
		{
			FlipFaceNormals instance = pb_EditorToolbarLoader.GetInstance<FlipFaceNormals>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/FlipFaceNormals", false)]
		static void MenuDoFlipFaceNormals()
		{
			FlipFaceNormals instance = pb_EditorToolbarLoader.GetInstance<FlipFaceNormals>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/InsertEdgeLoop", true)]
		static bool MenuVerifyInsertEdgeLoop()
		{
			InsertEdgeLoop instance = pb_EditorToolbarLoader.GetInstance<InsertEdgeLoop>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/InsertEdgeLoop", false)]
		static void MenuDoInsertEdgeLoop()
		{
			InsertEdgeLoop instance = pb_EditorToolbarLoader.GetInstance<InsertEdgeLoop>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/MergeFaces", true)]
		static bool MenuVerifyMergeFaces()
		{
			MergeFaces instance = pb_EditorToolbarLoader.GetInstance<MergeFaces>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/MergeFaces", false)]
		static void MenuDoMergeFaces()
		{
			MergeFaces instance = pb_EditorToolbarLoader.GetInstance<MergeFaces>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/SetPivotToSelection", true)]
		static bool MenuVerifySetPivotToSelection()
		{
			SetPivotToSelection instance = pb_EditorToolbarLoader.GetInstance<SetPivotToSelection>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/SetPivotToSelection", false)]
		static void MenuDoSetPivotToSelection()
		{
			SetPivotToSelection instance = pb_EditorToolbarLoader.GetInstance<SetPivotToSelection>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/SplitVertices", true)]
		static bool MenuVerifySplitVertices()
		{
			SplitVertices instance = pb_EditorToolbarLoader.GetInstance<SplitVertices>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/SplitVertices", false)]
		static void MenuDoSplitVertices()
		{
			SplitVertices instance = pb_EditorToolbarLoader.GetInstance<SplitVertices>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/SubdivideEdges", true)]
		static bool MenuVerifySubdivideEdges()
		{
			SubdivideEdges instance = pb_EditorToolbarLoader.GetInstance<SubdivideEdges>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/SubdivideEdges", false)]
		static void MenuDoSubdivideEdges()
		{
			SubdivideEdges instance = pb_EditorToolbarLoader.GetInstance<SubdivideEdges>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/SubdivideFaces", true)]
		static bool MenuVerifySubdivideFaces()
		{
			SubdivideFaces instance = pb_EditorToolbarLoader.GetInstance<SubdivideFaces>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/SubdivideFaces", false)]
		static void MenuDoSubdivideFaces()
		{
			SubdivideFaces instance = pb_EditorToolbarLoader.GetInstance<SubdivideFaces>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Geometry/WeldVertices", true)]
		static bool MenuVerifyWeldVertices()
		{
			WeldVertices instance = pb_EditorToolbarLoader.GetInstance<WeldVertices>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Geometry/WeldVertices", false)]
		static void MenuDoWeldVertices()
		{
			WeldVertices instance = pb_EditorToolbarLoader.GetInstance<WeldVertices>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Interaction/ToggleHandleAlignment", true)]
		static bool MenuVerifyToggleHandleAlignment()
		{
			ToggleHandleAlignment instance = pb_EditorToolbarLoader.GetInstance<ToggleHandleAlignment>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Interaction/ToggleHandleAlignment", false)]
		static void MenuDoToggleHandleAlignment()
		{
			ToggleHandleAlignment instance = pb_EditorToolbarLoader.GetInstance<ToggleHandleAlignment>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Interaction/ToggleSelectBackFaces", true)]
		static bool MenuVerifyToggleSelectBackFaces()
		{
			ToggleSelectBackFaces instance = pb_EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Interaction/ToggleSelectBackFaces", false)]
		static void MenuDoToggleSelectBackFaces()
		{
			ToggleSelectBackFaces instance = pb_EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Object/CenterPivot", true)]
		static bool MenuVerifyCenterPivot()
		{
			CenterPivot instance = pb_EditorToolbarLoader.GetInstance<CenterPivot>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Object/CenterPivot", false)]
		static void MenuDoCenterPivot()
		{
			CenterPivot instance = pb_EditorToolbarLoader.GetInstance<CenterPivot>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Object/ConformObjectNormals", true)]
		static bool MenuVerifyConformObjectNormals()
		{
			ConformObjectNormals instance = pb_EditorToolbarLoader.GetInstance<ConformObjectNormals>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Object/ConformObjectNormals", false)]
		static void MenuDoConformObjectNormals()
		{
			ConformObjectNormals instance = pb_EditorToolbarLoader.GetInstance<ConformObjectNormals>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Object/FlipObjectNormals", true)]
		static bool MenuVerifyFlipObjectNormals()
		{
			FlipObjectNormals instance = pb_EditorToolbarLoader.GetInstance<FlipObjectNormals>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Object/FlipObjectNormals", false)]
		static void MenuDoFlipObjectNormals()
		{
			FlipObjectNormals instance = pb_EditorToolbarLoader.GetInstance<FlipObjectNormals>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Object/FreezeTransform", true)]
		static bool MenuVerifyFreezeTransform()
		{
			FreezeTransform instance = pb_EditorToolbarLoader.GetInstance<FreezeTransform>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Object/FreezeTransform", false)]
		static void MenuDoFreezeTransform()
		{
			FreezeTransform instance = pb_EditorToolbarLoader.GetInstance<FreezeTransform>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Object/MergeObjects", true)]
		static bool MenuVerifyMergeObjects()
		{
			MergeObjects instance = pb_EditorToolbarLoader.GetInstance<MergeObjects>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Object/MergeObjects", false)]
		static void MenuDoMergeObjects()
		{
			MergeObjects instance = pb_EditorToolbarLoader.GetInstance<MergeObjects>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Object/SubdivideObject", true)]
		static bool MenuVerifySubdivideObject()
		{
			SubdivideObject instance = pb_EditorToolbarLoader.GetInstance<SubdivideObject>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Object/SubdivideObject", false)]
		static void MenuDoSubdivideObject()
		{
			SubdivideObject instance = pb_EditorToolbarLoader.GetInstance<SubdivideObject>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Object/TriangulateObject", true)]
		static bool MenuVerifyTriangulateObject()
		{
			TriangulateObject instance = pb_EditorToolbarLoader.GetInstance<TriangulateObject>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Object/TriangulateObject", false)]
		static void MenuDoTriangulateObject()
		{
			TriangulateObject instance = pb_EditorToolbarLoader.GetInstance<TriangulateObject>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Selection/GrowSelection", true)]
		static bool MenuVerifyGrowSelection()
		{
			GrowSelection instance = pb_EditorToolbarLoader.GetInstance<GrowSelection>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Selection/GrowSelection", false)]
		static void MenuDoGrowSelection()
		{
			GrowSelection instance = pb_EditorToolbarLoader.GetInstance<GrowSelection>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Selection/InvertSelection", true)]
		static bool MenuVerifyInvertSelection()
		{
			InvertSelection instance = pb_EditorToolbarLoader.GetInstance<InvertSelection>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Selection/InvertSelection", false)]
		static void MenuDoInvertSelection()
		{
			InvertSelection instance = pb_EditorToolbarLoader.GetInstance<InvertSelection>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Selection/SelectEdgeLoop", true)]
		static bool MenuVerifySelectEdgeLoop()
		{
			SelectEdgeLoop instance = pb_EditorToolbarLoader.GetInstance<SelectEdgeLoop>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Selection/SelectEdgeLoop", false)]
		static void MenuDoSelectEdgeLoop()
		{
			SelectEdgeLoop instance = pb_EditorToolbarLoader.GetInstance<SelectEdgeLoop>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Selection/SelectEdgeRing", true)]
		static bool MenuVerifySelectEdgeRing()
		{
			SelectEdgeRing instance = pb_EditorToolbarLoader.GetInstance<SelectEdgeRing>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Selection/SelectEdgeRing", false)]
		static void MenuDoSelectEdgeRing()
		{
			SelectEdgeRing instance = pb_EditorToolbarLoader.GetInstance<SelectEdgeRing>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem("Tools/ProBuilder/Selection/ShrinkSelection", true)]
		static bool MenuVerifyShrinkSelection()
		{
			ShrinkSelection instance = pb_EditorToolbarLoader.GetInstance<ShrinkSelection>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Selection/ShrinkSelection", false)]
		static void MenuDoShrinkSelection()
		{
			ShrinkSelection instance = pb_EditorToolbarLoader.GetInstance<ShrinkSelection>();
			if(instance != null)
				pb_Editor_Utility.ShowNotification(instance.DoAction().notification);
		}

	}
}

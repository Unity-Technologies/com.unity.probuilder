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
	static class EditorToolbarMenuItem
	{
		const string PB_MENU_PREFIX = "Tools/ProBuilder/";

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Editors/New Bezier Shape ", true)]
		static bool MenuVerifyNewBezierShape()
		{
			NewBezierShape instance = EditorToolbarLoader.GetInstance<NewBezierShape>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Editors/New Bezier Shape ", false, 0)]
		static void MenuDoNewBezierShape()
		{
			NewBezierShape instance = EditorToolbarLoader.GetInstance<NewBezierShape>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Editors/New Poly Shape ", true)]
		static bool MenuVerifyNewPolyShape()
		{
			NewPolyShape instance = EditorToolbarLoader.GetInstance<NewPolyShape>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Editors/New Poly Shape ", false, 0)]
		static void MenuDoNewPolyShape()
		{
			NewPolyShape instance = EditorToolbarLoader.GetInstance<NewPolyShape>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Editors/Open Material Editor ", true)]
		static bool MenuVerifyOpenMaterialEditor()
		{
			OpenMaterialEditor instance = EditorToolbarLoader.GetInstance<OpenMaterialEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Editors/Open Material Editor ", false, 0)]
		static void MenuDoOpenMaterialEditor()
		{
			OpenMaterialEditor instance = EditorToolbarLoader.GetInstance<OpenMaterialEditor>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Editors/Open Shape Editor %#k", true)]
		static bool MenuVerifyOpenShapeEditor()
		{
			OpenShapeEditor instance = EditorToolbarLoader.GetInstance<OpenShapeEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Editors/Open Shape Editor %#k", false, 0)]
		static void MenuDoOpenShapeEditor()
		{
			OpenShapeEditor instance = EditorToolbarLoader.GetInstance<OpenShapeEditor>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Editors/Open Smoothing Editor ", true)]
		static bool MenuVerifyOpenSmoothingEditor()
		{
			OpenSmoothingEditor instance = EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Editors/Open Smoothing Editor ", false, 0)]
		static void MenuDoOpenSmoothingEditor()
		{
			OpenSmoothingEditor instance = EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Editors/Open UV Editor ", true)]
		static bool MenuVerifyOpenUVEditor()
		{
			OpenUVEditor instance = EditorToolbarLoader.GetInstance<OpenUVEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Editors/Open UV Editor ", false, 0)]
		static void MenuDoOpenUVEditor()
		{
			OpenUVEditor instance = EditorToolbarLoader.GetInstance<OpenUVEditor>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Editors/Open Vertex Color Editor ", true)]
		static bool MenuVerifyOpenVertexColorEditor()
		{
			OpenVertexColorEditor instance = EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Editors/Open Vertex Color Editor ", false, 0)]
		static void MenuDoOpenVertexColorEditor()
		{
			OpenVertexColorEditor instance = EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Editors/Open Vertex Position Editor ", true)]
		static bool MenuVerifyOpenVertexPositionEditor()
		{
			OpenVertexPositionEditor instance = EditorToolbarLoader.GetInstance<OpenVertexPositionEditor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Editors/Open Vertex Position Editor ", false, 0)]
		static void MenuDoOpenVertexPositionEditor()
		{
			OpenVertexPositionEditor instance = EditorToolbarLoader.GetInstance<OpenVertexPositionEditor>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}


		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Export/Export Asset ", true)]
		static bool MenuVerifyExportAsset()
		{
			ExportAsset instance = EditorToolbarLoader.GetInstance<ExportAsset>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Export/Export Asset ", false, 0)]
		static void MenuDoExportAsset()
		{
			ExportAsset instance = EditorToolbarLoader.GetInstance<ExportAsset>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Export/Export Obj ", true)]
		static bool MenuVerifyExportObj()
		{
			ExportObj instance = EditorToolbarLoader.GetInstance<ExportObj>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Export/Export Obj ", false, 0)]
		static void MenuDoExportObj()
		{
			ExportObj instance = EditorToolbarLoader.GetInstance<ExportObj>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Export/Export Ply ", true)]
		static bool MenuVerifyExportPly()
		{
			ExportPly instance = EditorToolbarLoader.GetInstance<ExportPly>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Export/Export Ply ", false, 0)]
		static void MenuDoExportPly()
		{
			ExportPly instance = EditorToolbarLoader.GetInstance<ExportPly>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Export/Export Stl Ascii ", true)]
		static bool MenuVerifyExportStlAscii()
		{
			ExportStlAscii instance = EditorToolbarLoader.GetInstance<ExportStlAscii>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Export/Export Stl Ascii ", false, 0)]
		static void MenuDoExportStlAscii()
		{
			ExportStlAscii instance = EditorToolbarLoader.GetInstance<ExportStlAscii>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Export/Export Stl Binary ", true)]
		static bool MenuVerifyExportStlBinary()
		{
			ExportStlBinary instance = EditorToolbarLoader.GetInstance<ExportStlBinary>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Export/Export Stl Binary ", false, 0)]
		static void MenuDoExportStlBinary()
		{
			ExportStlBinary instance = EditorToolbarLoader.GetInstance<ExportStlBinary>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Bevel Edges ", true)]
		static bool MenuVerifyBevelEdges()
		{
			BevelEdges instance = EditorToolbarLoader.GetInstance<BevelEdges>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Bevel Edges ", false, 0)]
		static void MenuDoBevelEdges()
		{
			BevelEdges instance = EditorToolbarLoader.GetInstance<BevelEdges>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Bridge Edges &b", true)]
		static bool MenuVerifyBridgeEdges()
		{
			BridgeEdges instance = EditorToolbarLoader.GetInstance<BridgeEdges>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Bridge Edges &b", false, 0)]
		static void MenuDoBridgeEdges()
		{
			BridgeEdges instance = EditorToolbarLoader.GetInstance<BridgeEdges>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Collapse Vertexes &c", true)]
		static bool MenuVerifyCollapseVertexes()
		{
			CollapseVertexes instance = EditorToolbarLoader.GetInstance<CollapseVertexes>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Collapse Vertexes &c", false, 0)]
		static void MenuDoCollapseVertexes()
		{
			CollapseVertexes instance = EditorToolbarLoader.GetInstance<CollapseVertexes>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Conform Face Normals ", true)]
		static bool MenuVerifyConformFaceNormals()
		{
			ConformFaceNormals instance = EditorToolbarLoader.GetInstance<ConformFaceNormals>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Conform Face Normals ", false, 0)]
		static void MenuDoConformFaceNormals()
		{
			ConformFaceNormals instance = EditorToolbarLoader.GetInstance<ConformFaceNormals>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}



		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Delete Faces  [delete]", true)]
		static bool MenuVerifyDeleteFaces()
		{
			DeleteFaces instance = EditorToolbarLoader.GetInstance<DeleteFaces>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Delete Faces  [delete]", false, 0)]
		static void MenuDoDeleteFaces()
		{
			DeleteFaces instance = EditorToolbarLoader.GetInstance<DeleteFaces>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Detach Faces ", true)]
		static bool MenuVerifyDetachFaces()
		{
			DetachFaces instance = EditorToolbarLoader.GetInstance<DetachFaces>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Detach Faces ", false, 0)]
		static void MenuDoDetachFaces()
		{
			DetachFaces instance = EditorToolbarLoader.GetInstance<DetachFaces>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}



		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Fill Hole ", true)]
		static bool MenuVerifyFillHole()
		{
			FillHole instance = EditorToolbarLoader.GetInstance<FillHole>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Fill Hole ", false, 0)]
		static void MenuDoFillHole()
		{
			FillHole instance = EditorToolbarLoader.GetInstance<FillHole>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Flip Face Edge ", true)]
		static bool MenuVerifyFlipFaceEdge()
		{
			FlipFaceEdge instance = EditorToolbarLoader.GetInstance<FlipFaceEdge>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Flip Face Edge ", false, 0)]
		static void MenuDoFlipFaceEdge()
		{
			FlipFaceEdge instance = EditorToolbarLoader.GetInstance<FlipFaceEdge>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Flip Face Normals &n", true)]
		static bool MenuVerifyFlipFaceNormals()
		{
			FlipFaceNormals instance = EditorToolbarLoader.GetInstance<FlipFaceNormals>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Flip Face Normals &n", false, 0)]
		static void MenuDoFlipFaceNormals()
		{
			FlipFaceNormals instance = EditorToolbarLoader.GetInstance<FlipFaceNormals>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Insert Edge Loop &u", true)]
		static bool MenuVerifyInsertEdgeLoop()
		{
			InsertEdgeLoop instance = EditorToolbarLoader.GetInstance<InsertEdgeLoop>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Insert Edge Loop &u", false, 0)]
		static void MenuDoInsertEdgeLoop()
		{
			InsertEdgeLoop instance = EditorToolbarLoader.GetInstance<InsertEdgeLoop>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Merge Faces ", true)]
		static bool MenuVerifyMergeFaces()
		{
			MergeFaces instance = EditorToolbarLoader.GetInstance<MergeFaces>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Merge Faces ", false, 0)]
		static void MenuDoMergeFaces()
		{
			MergeFaces instance = EditorToolbarLoader.GetInstance<MergeFaces>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Set Pivot To Selection %j", true)]
		static bool MenuVerifySetPivotToSelection()
		{
			SetPivotToSelection instance = EditorToolbarLoader.GetInstance<SetPivotToSelection>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Set Pivot To Selection %j", false, 0)]
		static void MenuDoSetPivotToSelection()
		{
			SetPivotToSelection instance = EditorToolbarLoader.GetInstance<SetPivotToSelection>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Smart Connect &e", true)]
		static bool MenuVerifySmartConnect()
		{
			SmartConnect instance = EditorToolbarLoader.GetInstance<SmartConnect>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Smart Connect &e", false, 0)]
		static void MenuDoSmartConnect()
		{
			SmartConnect instance = EditorToolbarLoader.GetInstance<SmartConnect>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Smart Subdivide &s", true)]
		static bool MenuVerifySmartSubdivide()
		{
			SmartSubdivide instance = EditorToolbarLoader.GetInstance<SmartSubdivide>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Smart Subdivide &s", false, 0)]
		static void MenuDoSmartSubdivide()
		{
			SmartSubdivide instance = EditorToolbarLoader.GetInstance<SmartSubdivide>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Split Vertexes &x", true)]
		static bool MenuVerifySplitVertexes()
		{
			SplitVertexes instance = EditorToolbarLoader.GetInstance<SplitVertexes>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Split Vertexes &x", false, 0)]
		static void MenuDoSplitVertexes()
		{
			SplitVertexes instance = EditorToolbarLoader.GetInstance<SplitVertexes>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}



		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Triangulate Faces ", true)]
		static bool MenuVerifyTriangulateFaces()
		{
			TriangulateFaces instance = EditorToolbarLoader.GetInstance<TriangulateFaces>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Triangulate Faces ", false, 0)]
		static void MenuDoTriangulateFaces()
		{
			TriangulateFaces instance = EditorToolbarLoader.GetInstance<TriangulateFaces>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Weld Vertexes &v", true)]
		static bool MenuVerifyWeldVertexes()
		{
			WeldVertexes instance = EditorToolbarLoader.GetInstance<WeldVertexes>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Geometry/Weld Vertexes &v", false, 0)]
		static void MenuDoWeldVertexes()
		{
			WeldVertexes instance = EditorToolbarLoader.GetInstance<WeldVertexes>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Interaction/Toggle Drag Rect Mode ", true)]
		static bool MenuVerifyToggleDragRectMode()
		{
			ToggleDragRectMode instance = EditorToolbarLoader.GetInstance<ToggleDragRectMode>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Interaction/Toggle Drag Rect Mode ", false, 0)]
		static void MenuDoToggleDragRectMode()
		{
			ToggleDragRectMode instance = EditorToolbarLoader.GetInstance<ToggleDragRectMode>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Interaction/Toggle Drag Selection Mode ", true)]
		static bool MenuVerifyToggleDragSelectionMode()
		{
			ToggleDragSelectionMode instance = EditorToolbarLoader.GetInstance<ToggleDragSelectionMode>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Interaction/Toggle Drag Selection Mode ", false, 0)]
		static void MenuDoToggleDragSelectionMode()
		{
			ToggleDragSelectionMode instance = EditorToolbarLoader.GetInstance<ToggleDragSelectionMode>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Interaction/Toggle Handle Alignment  [p]", true)]
		static bool MenuVerifyToggleHandleAlignment()
		{
			ToggleHandleAlignment instance = EditorToolbarLoader.GetInstance<ToggleHandleAlignment>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Interaction/Toggle Handle Alignment  [p]", false, 0)]
		static void MenuDoToggleHandleAlignment()
		{
			ToggleHandleAlignment instance = EditorToolbarLoader.GetInstance<ToggleHandleAlignment>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Interaction/Toggle Select Back Faces ", true)]
		static bool MenuVerifyToggleSelectBackFaces()
		{
			ToggleSelectBackFaces instance = EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Interaction/Toggle Select Back Faces ", false, 0)]
		static void MenuDoToggleSelectBackFaces()
		{
			ToggleSelectBackFaces instance = EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Center Pivot ", true)]
		static bool MenuVerifyCenterPivot()
		{
			CenterPivot instance = EditorToolbarLoader.GetInstance<CenterPivot>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Center Pivot ", false, 0)]
		static void MenuDoCenterPivot()
		{
			CenterPivot instance = EditorToolbarLoader.GetInstance<CenterPivot>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Conform Object Normals ", true)]
		static bool MenuVerifyConformObjectNormals()
		{
			ConformObjectNormals instance = EditorToolbarLoader.GetInstance<ConformObjectNormals>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Conform Object Normals ", false, 0)]
		static void MenuDoConformObjectNormals()
		{
			ConformObjectNormals instance = EditorToolbarLoader.GetInstance<ConformObjectNormals>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Flip Object Normals ", true)]
		static bool MenuVerifyFlipObjectNormals()
		{
			FlipObjectNormals instance = EditorToolbarLoader.GetInstance<FlipObjectNormals>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Flip Object Normals ", false, 0)]
		static void MenuDoFlipObjectNormals()
		{
			FlipObjectNormals instance = EditorToolbarLoader.GetInstance<FlipObjectNormals>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Freeze Transform ", true)]
		static bool MenuVerifyFreezeTransform()
		{
			FreezeTransform instance = EditorToolbarLoader.GetInstance<FreezeTransform>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Freeze Transform ", false, 0)]
		static void MenuDoFreezeTransform()
		{
			FreezeTransform instance = EditorToolbarLoader.GetInstance<FreezeTransform>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}


		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Merge Objects ", true)]
		static bool MenuVerifyMergeObjects()
		{
			MergeObjects instance = EditorToolbarLoader.GetInstance<MergeObjects>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Merge Objects ", false, 0)]
		static void MenuDoMergeObjects()
		{
			MergeObjects instance = EditorToolbarLoader.GetInstance<MergeObjects>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Mirror Objects ", true)]
		static bool MenuVerifyMirrorObjects()
		{
			MirrorObjects instance = EditorToolbarLoader.GetInstance<MirrorObjects>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Mirror Objects ", false, 0)]
		static void MenuDoMirrorObjects()
		{
			MirrorObjects instance = EditorToolbarLoader.GetInstance<MirrorObjects>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Pro Builderize ", true)]
		static bool MenuVerifyProBuilderize()
		{
			ProBuilderize instance = EditorToolbarLoader.GetInstance<ProBuilderize>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Pro Builderize ", false, 0)]
		static void MenuDoProBuilderize()
		{
			ProBuilderize instance = EditorToolbarLoader.GetInstance<ProBuilderize>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Set Collider ", true)]
		static bool MenuVerifySetCollider()
		{
			SetCollider instance = EditorToolbarLoader.GetInstance<SetCollider>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Set Collider ", false, 0)]
		static void MenuDoSetCollider()
		{
			SetCollider instance = EditorToolbarLoader.GetInstance<SetCollider>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Set Trigger ", true)]
		static bool MenuVerifySetTrigger()
		{
			SetTrigger instance = EditorToolbarLoader.GetInstance<SetTrigger>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Set Trigger ", false, 0)]
		static void MenuDoSetTrigger()
		{
			SetTrigger instance = EditorToolbarLoader.GetInstance<SetTrigger>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Subdivide Object ", true)]
		static bool MenuVerifySubdivideObject()
		{
			SubdivideObject instance = EditorToolbarLoader.GetInstance<SubdivideObject>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Subdivide Object ", false, 0)]
		static void MenuDoSubdivideObject()
		{
			SubdivideObject instance = EditorToolbarLoader.GetInstance<SubdivideObject>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Triangulate Object ", true)]
		static bool MenuVerifyTriangulateObject()
		{
			TriangulateObject instance = EditorToolbarLoader.GetInstance<TriangulateObject>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Object/Triangulate Object ", false, 0)]
		static void MenuDoTriangulateObject()
		{
			TriangulateObject instance = EditorToolbarLoader.GetInstance<TriangulateObject>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Selection/Grow Selection &g", true)]
		static bool MenuVerifyGrowSelection()
		{
			GrowSelection instance = EditorToolbarLoader.GetInstance<GrowSelection>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Selection/Grow Selection &g", false, 0)]
		static void MenuDoGrowSelection()
		{
			GrowSelection instance = EditorToolbarLoader.GetInstance<GrowSelection>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Selection/Invert Selection %#i", true)]
		static bool MenuVerifyInvertSelection()
		{
			InvertSelection instance = EditorToolbarLoader.GetInstance<InvertSelection>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Selection/Invert Selection %#i", false, 0)]
		static void MenuDoInvertSelection()
		{
			InvertSelection instance = EditorToolbarLoader.GetInstance<InvertSelection>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}





		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Selection/Select Hole ", true)]
		static bool MenuVerifySelectHole()
		{
			SelectHole instance = EditorToolbarLoader.GetInstance<SelectHole>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Selection/Select Hole ", false, 0)]
		static void MenuDoSelectHole()
		{
			SelectHole instance = EditorToolbarLoader.GetInstance<SelectHole>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Selection/Select Material ", true)]
		static bool MenuVerifySelectMaterial()
		{
			SelectMaterial instance = EditorToolbarLoader.GetInstance<SelectMaterial>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Selection/Select Material ", false, 0)]
		static void MenuDoSelectMaterial()
		{
			SelectMaterial instance = EditorToolbarLoader.GetInstance<SelectMaterial>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Selection/Select Smoothing Group ", true)]
		static bool MenuVerifySelectSmoothingGroup()
		{
			SelectSmoothingGroup instance = EditorToolbarLoader.GetInstance<SelectSmoothingGroup>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Selection/Select Smoothing Group ", false, 0)]
		static void MenuDoSelectSmoothingGroup()
		{
			SelectSmoothingGroup instance = EditorToolbarLoader.GetInstance<SelectSmoothingGroup>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Selection/Select Vertex Color ", true)]
		static bool MenuVerifySelectVertexColor()
		{
			SelectVertexColor instance = EditorToolbarLoader.GetInstance<SelectVertexColor>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Selection/Select Vertex Color ", false, 0)]
		static void MenuDoSelectVertexColor()
		{
			SelectVertexColor instance = EditorToolbarLoader.GetInstance<SelectVertexColor>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Selection/Shrink Selection &#g", true)]
		static bool MenuVerifyShrinkSelection()
		{
			ShrinkSelection instance = EditorToolbarLoader.GetInstance<ShrinkSelection>();

#if PROTOTYPE
			return instance != null && !instance.isProOnly && instance.IsEnabled();
#else
			return instance != null && instance.enabled;
#endif
	
		}

		[MenuItem(PB_MENU_PREFIX + "C:/Users/karlh/procore/probuilder/com.unity.probuilder/Editor/Menu Actions/Selection/Shrink Selection &#g", false, 0)]
		static void MenuDoShrinkSelection()
		{
			ShrinkSelection instance = EditorToolbarLoader.GetInstance<ShrinkSelection>();
			if(instance != null)
				UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
		}

	}
}

using UnityEngine;
using System.Collections.Generic;
using ProBuilder2.Common;
using System.Linq;

namespace ProBuilder2.EditorCommon
{
	public class pb_EditorToolbarLoader
	{
		private static readonly pb_TooltipContent tt_ShapeEditor = new pb_TooltipContent(
			"Shape Editor",
			"Opens the Shape Editor window.\n\nThe Shape Editor is a window that allows you to interactively create new 3d primitves."
			);

		private static readonly pb_TooltipContent tt_MaterialEditor = new pb_TooltipContent(
			"Material Editor",
			"Opens the Material Editor window.\n\nThe Material Editor window applies materials to selected faces or objects."
			);

		private static readonly pb_TooltipContent tt_UVEditor = new pb_TooltipContent(
			"UV Editor",
			"Opens the UV Editor window.\n\nThe UV Editor allows you to change how textures are rendered on this mesh."
			);

		private static readonly pb_TooltipContent tt_VertexColors = new pb_TooltipContent(
			"Vertex Colors",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_SmoothingEditor = new pb_TooltipContent(
			"Smoothing Editor",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_MirrorObjects = new pb_TooltipContent(
			"Mirror Objects",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_SelectBackFaces = new pb_TooltipContent(
			"Select Hidden Faces",
			"When drag selecting or clicking, this will ignore faces that are either not facing the camera or behind other geometery."
			);
		private static readonly pb_TooltipContent tt_SetHandleAlignment = new pb_TooltipContent(
			"Set Handle Alignment",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_GrowSelection = new pb_TooltipContent(
			"Grow Selection",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_ShrinkSelection = new pb_TooltipContent(
			"Shrink Selection",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_InvertSelection = new pb_TooltipContent(
			"Invert Selection",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_SelectEdgeRing = new pb_TooltipContent(
			"Select Edge Ring",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_SelectEdgeLoop = new pb_TooltipContent(
			"Select Edge Loop",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_MergeObjects = new pb_TooltipContent(
			"Merge Objects",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_FlipObjectNormals = new pb_TooltipContent(
			"Flip Object Normals",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_SubdivideObject = new pb_TooltipContent(
			"Subdivide Object",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_FreezeTransform = new pb_TooltipContent(
			"Freeze Transform",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_ConformObjectNormals = new pb_TooltipContent(
			"Conform Object Normals",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_TriangulateObject = new pb_TooltipContent(
			"Triangulate Object",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_SetPivotToSelection = new pb_TooltipContent(
			"Set Pivot to Center of Selection",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_ExtrudeFaces = new pb_TooltipContent(
			"Extrude Faces",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_ConformFaceNormals = new pb_TooltipContent(
			"Conform Face Normals",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_FlipFaceNormals = new pb_TooltipContent(
			"Flip Face Normals",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_FlipFaceEdge = new pb_TooltipContent(
			"Flip Face Edge",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_DeleteFaces = new pb_TooltipContent(
			"Delete Faces",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_DetachFaces = new pb_TooltipContent(
			"Detach Faces",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_MergeFaces = new pb_TooltipContent(
			"Merge Faces",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_SubdivideFaces = new pb_TooltipContent(
			"Subdivide Faces",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_ExtrudeEdges = new pb_TooltipContent(
			"Extrude Edges",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_BridgeEdges = new pb_TooltipContent(
			"Bridge Edges",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_ConnectEdges = new pb_TooltipContent(
			"Connect Edges",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_InsertEdgeLoop = new pb_TooltipContent(
			"Insert Edge Loop",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_ConnectVertices = new pb_TooltipContent(
			"Connect Vertices",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_WeldVertices = new pb_TooltipContent(
			"Weld Vertices",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_CollapseVertices = new pb_TooltipContent(
			"Collapse Vertices",
			"DESCRIPTION GOES HERE"
			);

		private static readonly pb_TooltipContent tt_SplitVertices = new pb_TooltipContent(
			"Split Vertices",
			"DESCRIPTION GOES HERE"
			);



		private static Texture2D Icon(string path)
		{
			return pb_IconUtility.GetIcon(path);
		}

		private static pb_MenuAction_Element CreateFaceAction(
			string icon,
			string tooltip,
			System.Func<pb_Object[], pb_ActionResult> action,
			bool enforceMode = false,
			int minFaceCount = 1)
		{
			return new pb_MenuAction_Element(
				Icon(icon),
				tooltip,
				action,
				SelectMode.Face,
				false,
				(x) => { return x.Sum(y => y.SelectedFaceCount) >= minFaceCount; });
		}

		private static pb_MenuAction_Element CreateEdgeAction(
			string icon,
			string tooltip,
			System.Func<pb_Object[], pb_ActionResult> action,
			bool enforceMode = false,
			int minEdgeCount = 1)
		{
			return new pb_MenuAction_Element( 
				Icon(icon),
				tooltip,
				action,
				SelectMode.Edge,
				false,
				(x) => { return x.Sum(y => y.SelectedEdges.Length) >= minEdgeCount; });
		}

		private static pb_MenuAction_Element CreateVertexAction(
			string icon,
			string tooltip,
			System.Func<pb_Object[], pb_ActionResult> action,
			bool enforceMode = false,
			int minVertCount = 1)
		{
			return new pb_MenuAction_Element( 
				Icon(icon),
				tooltip,
				action,
				SelectMode.Vertex,
				false,
				(x) => { return x.Sum(y => y.SelectedTriangleCount) >= minVertCount; });
		}

		public static List<pb_MenuAction> GetActions()
		{
			return new List<pb_MenuAction>()
			{
				// tools
				new pb_MenuAction_Tool( Icon("Panel_Shapes"), tt_ShapeEditor, pb_Geometry_Interface.MenuOpenShapeCreator ),
				new pb_MenuAction_Tool( Icon("Panel_Materials"), tt_MaterialEditor, pb_Material_Editor.MenuOpenMaterialEditor ),
				new pb_MenuAction_Tool( Icon("Panel_UVEditor"), tt_UVEditor, pb_UV_Editor.MenuOpenUVEditor ),
				new pb_MenuAction_Tool( Icon("Panel_VertColors"), tt_VertexColors, pb_Menu_Commands.MenuOpenVertexColorsEditor ),
				new pb_MenuAction_Tool( Icon("Panel_Smoothing"), tt_SmoothingEditor, pb_Smoothing_Editor.MenuOpenSmoothingEditor ),
				new pb_MenuAction_Tool( Icon("Object_Mirror"), tt_MirrorObjects, pb_Mirror_Tool.MenuOpenMirrorEditor ),

				// interaction
				new pb_MenuAction_Toggle( 
					new Texture2D[] { Icon("Selection_SelectHidden-ON"), Icon("Selection_SelectHidden-OFF") },
					tt_SelectBackFaces,
					(x) => { pb_Editor.instance.SetSelectHiddenEnabled(x == 0); }),

				new pb_MenuAction_Toggle( 
					new Texture2D[] { Icon("HandleAlign_World"), Icon("HandleAlign_Local"), Icon("HandleAlign_Plane") },
					tt_SetHandleAlignment,
					(x) => { pb_Editor.instance.SetHandleAlignment((HandleAlignment)x); }),

				// selection
				new pb_MenuAction_Element(Icon("Selection_Grow"), tt_GrowSelection, pb_Menu_Commands.MenuGrowSelection, SelectMode.Vertex, false, pb_Menu_Commands.VerifyGrowSelection),
				new pb_MenuAction_Element(Icon("Selection_Shrink"), tt_ShrinkSelection, pb_Menu_Commands.MenuShrinkSelection, SelectMode.Vertex, false, pb_Menu_Commands.VerifyShrinkSelection),
				new pb_MenuAction_Element(Icon("Selection_Invert"), tt_InvertSelection, pb_Menu_Commands.MenuInvertSelection, SelectMode.Vertex, false, pb_Menu_Commands.VerifyInvertSelection),
				new pb_MenuAction_Element(Icon("Selection_Ring"), tt_SelectEdgeRing, pb_Menu_Commands.MenuRingSelection, SelectMode.Edge, true, pb_Menu_Commands.VerifyEdgeRingLoop),
				new pb_MenuAction_Element(Icon("Selection_Loop"), tt_SelectEdgeLoop, pb_Menu_Commands.MenuLoopSelection, SelectMode.Edge, true, pb_Menu_Commands.VerifyEdgeRingLoop),

					
				// object
				new pb_MenuAction_Object( Icon("Object_Merge"), tt_MergeObjects, pb_Menu_Commands.MenuMergeObjects, (x) => { return x != null && x.Length >= 2;} ),
				new pb_MenuAction_Object( Icon("Object_FlipNormals"), tt_FlipObjectNormals, pb_Menu_Commands.MenuFlipObjectNormals ),
				new pb_MenuAction_Object( Icon("Object_Subdivide"), tt_SubdivideObject, pb_Menu_Commands.MenuSubdivide ),
				new pb_MenuAction_Object( Icon("Pivot_Reset"), tt_FreezeTransform, pb_Menu_Commands.MenuFreezeTransforms ),
				new pb_MenuAction_Object( Icon("null"), tt_ConformObjectNormals, pb_Menu_Commands.MenuConformObjectNormals ),
				new pb_MenuAction_Object( Icon("null"), tt_TriangulateObject, pb_Menu_Commands.MenuTriangulateObject ),
				
				// // elements all
				CreateVertexAction("Pivot_MoveToCenter", tt_SetPivotToSelection, pb_Menu_Commands.MenuSetPivot, false, 1),	// @todo

				// // elements face
				CreateFaceAction("Face_Extrude", tt_ExtrudeFaces, pb_Menu_Commands.MenuExtrude),							// @todo
				CreateFaceAction("null", tt_ConformFaceNormals, pb_Menu_Commands.MenuConformNormals, false, 3),
				CreateFaceAction("Face_FlipNormals", tt_FlipFaceNormals, pb_Menu_Commands.MenuFlipNormals ),
				CreateFaceAction("null", tt_FlipFaceEdge, pb_Menu_Commands.MenuFlipEdges),
				CreateFaceAction("Face_Delete", tt_DeleteFaces, pb_Menu_Commands.MenuDeleteFace),
				CreateFaceAction("Face_Detach", tt_DetachFaces, pb_Menu_Commands.MenuDetachFaces),
				CreateFaceAction("Face_Merge", tt_MergeFaces, pb_Menu_Commands.MenuMergeFaces, false, 2),
				CreateFaceAction("Face_Subdivide", tt_SubdivideFaces, pb_Menu_Commands.MenuSubdivideFace),
				
				// elements edge
				CreateEdgeAction("Edge_Extrude", tt_ExtrudeEdges, pb_Menu_Commands.MenuExtrude),							// @todo
				CreateEdgeAction("Edge_Bridge", tt_BridgeEdges, pb_Menu_Commands.MenuBridgeEdges, true, 2),
				CreateEdgeAction("Edge_Connect", tt_ConnectEdges, pb_Menu_Commands.MenuConnectEdges, true, 2),
				CreateEdgeAction("Edge_InsertLoop", tt_InsertEdgeLoop, pb_Menu_Commands.MenuInsertEdgeLoop),

				CreateVertexAction("Vert_Connect", tt_ConnectVertices, pb_Menu_Commands.MenuConnectVertices, false, 2),
				CreateVertexAction("Vert_Weld", tt_WeldVertices, pb_Menu_Commands.MenuWeldVertices, false, 2),				// @todo
				CreateVertexAction("Vert_Collapse", tt_CollapseVertices, pb_Menu_Commands.MenuCollapseVertices, false, 2),	// @todo
				CreateVertexAction("Vert_Split", tt_SplitVertices, pb_Menu_Commands.MenuSplitVertices)						// @todo
			};
		}
	}
}

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
			"Vertex Color Editor",
			"Opens either the Vertex Color Palette or the Vertex Color Painter.\n\nThe Palette is useful for applying colors to selected faces with hard edges, where the Painter is good for brush strokes and soft edges."
			);

		private static readonly pb_TooltipContent tt_SmoothingEditor = new pb_TooltipContent(
			"Smoothing Editor",
			"opens the Smoothing Groups Editor.\n\nSmoothing groups are how you mark edges as being soft or hard, which means the light will either blend between two faces or create a seam."
			);

		private static readonly pb_TooltipContent tt_MirrorObjects = new pb_TooltipContent(
			"Mirror Objects Editor",
			"Opens the Mirror Editor.\n\nMirroring objects will duplicate an flip objects on the specified axes."
			);

		private static readonly pb_TooltipContent tt_SelectBackFaces = new pb_TooltipContent(
			"Select Hidden Faces",
			"Toggles back-face selection on or off.\n\nWhen drag selecting or clicking, back-face selection off will ignore faces that are either not facing the camera or behind other geometery."
			);

		private static readonly pb_TooltipContent tt_SetHandleAlignment = new pb_TooltipContent(
			"Set Handle Alignment",
			"Toggles the transform gizmo's coordinate space."
			);

		private static readonly pb_TooltipContent tt_GrowSelection = new pb_TooltipContent(
			"Grow Selection",
			"Expands the current element selection to include nearby elements."
			);

		private static readonly pb_TooltipContent tt_ShrinkSelection = new pb_TooltipContent(
			"Shrink Selection",
			"Removes the elements on the outer edges of the current selection."
			);

		private static readonly pb_TooltipContent tt_InvertSelection = new pb_TooltipContent(
			"Invert Selection",
			"Set the element selection to the opposite of what is currently selected."
			);

		private static readonly pb_TooltipContent tt_SelectEdgeRing = new pb_TooltipContent(
			"Select Edge Ring",
			"Select all edges that are opposite the selected."
			);

		private static readonly pb_TooltipContent tt_SelectEdgeLoop = new pb_TooltipContent(
			"Select Edge Loop",
			"Select a loop of connected edges."
			);

		private static readonly pb_TooltipContent tt_MergeObjects = new pb_TooltipContent(
			"Merge Objects",
			"Merges all selected ProBuilder objects to a single mesh."
			);

		private static readonly pb_TooltipContent tt_FlipObjectNormals = new pb_TooltipContent(
			"Flip Object Normals",
			"Reverse the direction of all faces on the selected objects."
			);

		private static readonly pb_TooltipContent tt_SubdivideObject = new pb_TooltipContent(
			"Subdivide Object",
			"Increase the number of edges and vertices on this object by creating 4 new quads in every face."
			);

		private static readonly pb_TooltipContent tt_FreezeTransform = new pb_TooltipContent(
			"Freeze Transform",
			"Set the pivot point to world coordinates (0,0,0) and clear all Transform values while keeping the mesh in place."
			);

		private static readonly pb_TooltipContent tt_ConformObjectNormals = new pb_TooltipContent(
			"Conform Object Normals",
			"Check the object for faces that are flipped in the opposite direction of most other faces, then reverses any dissenters."
			);

		private static readonly pb_TooltipContent tt_TriangulateObject = new pb_TooltipContent(
			"Triangulate Object",
			"Removes all quads and n-gons on the mesh and inserts triangles instead.  Use this and a hard smoothing group to achieve a low-poly facetized look."
			);

		private static readonly pb_TooltipContent tt_SetPivotToSelection = new pb_TooltipContent(
			"Set Pivot to Center of Selection",
			"Sets the pivot point of the selected mesh to the position the transform gizmo is currently in."
			);

		private static readonly pb_TooltipContent tt_ExtrudeFaces = new pb_TooltipContent(
			"Extrude Faces",
			"Extrude all selected faces."
			);

		private static readonly pb_TooltipContent tt_ConformFaceNormals = new pb_TooltipContent(
			"Conform Face Normals",
			"Orients all selected faces to face the same direction."
			);

		private static readonly pb_TooltipContent tt_FlipFaceNormals = new pb_TooltipContent(
			"Flip Face Normals",
			"Reverse the direction that the selected faces are pointing."
			);

		private static readonly pb_TooltipContent tt_FlipFaceEdge = new pb_TooltipContent(
			"Flip Face Edge",
			"Also called \"turning\" and edge, this action changes the vertices that a quad's connecting edge points to.  This is useful if a quad has one vertex that is raised or lowered and the \"hump\" of the face is in the wrong direction."
			);

		private static readonly pb_TooltipContent tt_DeleteFaces = new pb_TooltipContent(
			"Delete Faces",
			"Deletes the selected faces."
			);

		private static readonly pb_TooltipContent tt_DetachFaces = new pb_TooltipContent(
			"Detach Faces",
			"Detach selected faces to a new object or submesh."
			);

		private static readonly pb_TooltipContent tt_MergeFaces = new pb_TooltipContent(
			"Merge Faces",
			"Tells ProBuilder to treat the selected faces as if they were a single face.  Be careful not to use this with unconnected faces!"
			);

		private static readonly pb_TooltipContent tt_SubdivideFaces = new pb_TooltipContent(
			"Subdivide Faces",
			"Adds extra vertices and edges to the selected faces by inserting a new vertex in the middle and connecting the middle of all edges to it."
			);

		// private static readonly pb_TooltipContent tt_ExtrudeEdges = new pb_TooltipContent(
		// 	"Extrude Edges",
		// 	"Extrudes the selected edges."
		// 	);

		private static readonly pb_TooltipContent tt_BridgeEdges = new pb_TooltipContent(
			"Bridge Edges",
			"Add a new face connecting two edges."
			);

		private static readonly pb_TooltipContent tt_ConnectEdges = new pb_TooltipContent(
			"Connect Edges",
			"Insert a new edge from the centers of each selected edge to the others."
			);

		private static readonly pb_TooltipContent tt_InsertEdgeLoop = new pb_TooltipContent(
			"Insert Edge Loop",
			"Creates a series of new edges from the center of the selected edge to it's ringed edges."
			);

		private static readonly pb_TooltipContent tt_ConnectVertices = new pb_TooltipContent(
			"Connect Vertices",
			"Creates a new edge connecting two or more vertices."
			);

		private static readonly pb_TooltipContent tt_WeldVertices = new pb_TooltipContent(
			"Weld Vertices",
			"Checks all selected vertices for neighbors within a certain distance of one another, and if they are close enough they will be collapsed to a single vertex."
			);

		private static readonly pb_TooltipContent tt_CollapseVertices = new pb_TooltipContent(
			"Collapse Vertices",
			"Merges selected vertices to a single vertex."
			);

		private static readonly pb_TooltipContent tt_SplitVertices = new pb_TooltipContent(
			"Split Vertices",
			"Splits a vertex into separate vertices for each attached edge."
			);


		private static Texture2D Icon(string path)
		{
			return pb_IconUtility.GetIcon(path);
		}

		private static pb_MenuAction_Element CreateFaceAction(
			string icon,
			pb_TooltipContent tooltip,
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
			pb_TooltipContent tooltip,
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
			pb_TooltipContent tooltip,
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
				new pb_MenuAction_Tool( Icon("Panel_VertColors"), tt_VertexColors, pb_Menu_Commands.MenuOpenVertexColorsEditor, typeof(pb_MenuOption_VertexColors) ),
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
				new pb_MenuAction_Element(Icon("Selection_Grow"),
					tt_GrowSelection,
					pb_Menu_Commands.MenuGrowSelection,
					SelectMode.Vertex,
					false,
					pb_Menu_Commands.VerifyGrowSelection,
					typeof(pb_MenuOption_GrowSelection),
					(x, y, z) => { return x == EditLevel.Geometry && y == SelectMode.Face; }
					),
				new pb_MenuAction_Element(Icon("Selection_Shrink"), tt_ShrinkSelection, pb_Menu_Commands.MenuShrinkSelection, SelectMode.Vertex, false, pb_Menu_Commands.VerifyShrinkSelection),
				new pb_MenuAction_Element(Icon("Selection_Invert"), tt_InvertSelection, pb_Menu_Commands.MenuInvertSelection, SelectMode.Vertex, false, pb_Menu_Commands.VerifyInvertSelection),
				new pb_MenuAction_Element(Icon("Selection_Ring"), tt_SelectEdgeRing, pb_Menu_Commands.MenuRingSelection, SelectMode.Edge, true, pb_Menu_Commands.VerifyEdgeRingLoop),
				new pb_MenuAction_Element(Icon("Selection_Loop"), tt_SelectEdgeLoop, pb_Menu_Commands.MenuLoopSelection, SelectMode.Edge, true, pb_Menu_Commands.VerifyEdgeRingLoop),

				// object
				new pb_MenuAction_Object( Icon("Object_Merge"), tt_MergeObjects, pb_Menu_Commands.MenuMergeObjects, (x) => { return x != null && x.Length >= 2;} ),
				new pb_MenuAction_Object( Icon("Object_FlipNormals"), tt_FlipObjectNormals, pb_Menu_Commands.MenuFlipObjectNormals ),
				new pb_MenuAction_Object( Icon("Object_Subdivide"), tt_SubdivideObject, pb_Menu_Commands.MenuSubdivide ),
				new pb_MenuAction_Object( Icon("Pivot_Reset"), tt_FreezeTransform, pb_Menu_Commands.MenuFreezeTransforms ),
				new pb_MenuAction_Object( Icon("Object_ConformNormals"), tt_ConformObjectNormals, pb_Menu_Commands.MenuConformObjectNormals ),
				new pb_MenuAction_Object( Icon("Object_Triangulate"), tt_TriangulateObject, pb_Menu_Commands.MenuTriangulateObject ),

				// elements all
				CreateVertexAction("Pivot_MoveToCenter", tt_SetPivotToSelection, pb_Menu_Commands.MenuSetPivot, false, 1),	// @todo

				// elements face
				new pb_MenuAction_Element(
					Icon("Face_Extrude"),
					tt_ExtrudeFaces,
					pb_Menu_Commands.MenuExtrude,
					SelectMode.Edge,
					false,
					(x) => { return x.Sum(y => y.SelectedFaceCount) >= 1 || x.Sum(z => z.SelectedEdgeCount) >= 1; },
					typeof(pb_MenuOption_Extrude)),

				CreateFaceAction("Face_ConformNormals", tt_ConformFaceNormals, pb_Menu_Commands.MenuConformNormals, false, 3),
				CreateFaceAction("Face_FlipNormals", tt_FlipFaceNormals, pb_Menu_Commands.MenuFlipNormals ),
				CreateFaceAction("Face_FlipEdge", tt_FlipFaceEdge, pb_Menu_Commands.MenuFlipEdges),
				CreateFaceAction("Face_Delete", tt_DeleteFaces, pb_Menu_Commands.MenuDeleteFace),
				CreateFaceAction("Face_Detach", tt_DetachFaces, pb_Menu_Commands.MenuDetachFaces),
				CreateFaceAction("Face_Merge", tt_MergeFaces, pb_Menu_Commands.MenuMergeFaces, false, 2),
				CreateFaceAction("Face_Subdivide", tt_SubdivideFaces, pb_Menu_Commands.MenuSubdivideFace),

				// elements edge
				// CreateEdgeAction("Edge_Extrude", tt_ExtrudeEdges, pb_Menu_Commands.MenuExtrude),							// @todo
				CreateEdgeAction("Edge_Bridge", tt_BridgeEdges, pb_Menu_Commands.MenuBridgeEdges, true, 2),
				CreateEdgeAction("Edge_Connect", tt_ConnectEdges, pb_Menu_Commands.MenuConnectEdges, true, 2),
				CreateEdgeAction("Edge_InsertLoop", tt_InsertEdgeLoop, pb_Menu_Commands.MenuInsertEdgeLoop),

				CreateVertexAction("Vert_Connect", tt_ConnectVertices, pb_Menu_Commands.MenuConnectVertices, false, 2),
				new pb_MenuAction_Element(
					Icon("Vert_Weld"),
					tt_WeldVertices,
					pb_Menu_Commands.MenuWeldVertices,
					SelectMode.Vertex,
					false,
					(x) => { return x.Sum(y => y.SelectedTriangleCount) >= 2; },
					typeof(pb_MenuOption_Weld)),
				CreateVertexAction("Vert_Collapse", tt_CollapseVertices, pb_Menu_Commands.MenuCollapseVertices, false, 2),	// @todo
				CreateVertexAction("Vert_Split", tt_SplitVertices, pb_Menu_Commands.MenuSplitVertices)						// @todo

			};
		}
	}
}

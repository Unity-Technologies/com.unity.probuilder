using UnityEngine;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.Actions;
using System.Linq;

namespace ProBuilder2.EditorCommon
{
	public class pb_EditorToolbarLoader
	{
		// private static readonly pb_TooltipContent tt_SetPivotToSelection = new pb_TooltipContent(
		// 	"Set Pivot to Center of Selection",
		// 	"Sets the pivot point of the selected mesh to the position the transform gizmo is currently in."
		// 	);

		// private static readonly pb_TooltipContent tt_ConformFaceNormals = new pb_TooltipContent(
		// 	"Conform Face Normals",
		// 	"Orients all selected faces to face the same direction."
		// 	);

		// private static readonly pb_TooltipContent tt_FlipFaceNormals = new pb_TooltipContent(
		// 	"Flip Face Normals",
		// 	"Reverse the direction that the selected faces are pointing."
		// 	);

		// private static readonly pb_TooltipContent tt_FlipFaceEdge = new pb_TooltipContent(
		// 	"Flip Face Edge",
		// 	"Also called \"turning\" and edge, this action changes the vertices that a quad's connecting edge points to.  This is useful if a quad has one vertex that is raised or lowered and the \"hump\" of the face is in the wrong direction."
		// 	);

		// private static readonly pb_TooltipContent tt_DeleteFaces = new pb_TooltipContent(
		// 	"Delete Faces",
		// 	"Deletes the selected faces."
		// 	);

		// private static readonly pb_TooltipContent tt_DetachFaces = new pb_TooltipContent(
		// 	"Detach Faces",
		// 	"Detach selected faces to a new object or submesh."
		// 	);

		// private static readonly pb_TooltipContent tt_MergeFaces = new pb_TooltipContent(
		// 	"Merge Faces",
		// 	"Tells ProBuilder to treat the selected faces as if they were a single face.  Be careful not to use this with unconnected faces!"
		// 	);

		// private static readonly pb_TooltipContent tt_SubdivideFaces = new pb_TooltipContent(
		// 	"Subdivide Faces",
		// 	"Adds extra vertices and edges to the selected faces by inserting a new vertex in the middle and connecting the middle of all edges to it."
		// 	);

		// // private static readonly pb_TooltipContent tt_ExtrudeEdges = new pb_TooltipContent(
		// // 	"Extrude Edges",
		// // 	"Extrudes the selected edges."
		// // 	);


		// private static readonly pb_TooltipContent tt_ConnectEdges = new pb_TooltipContent(
		// 	"Connect Edges",
		// 	"Insert a new edge from the centers of each selected edge to the others."
		// 	);

		// private static readonly pb_TooltipContent tt_InsertEdgeLoop = new pb_TooltipContent(
		// 	"Insert Edge Loop",
		// 	"Creates a series of new edges from the center of the selected edge to it's ringed edges."
		// 	);

		// private static readonly pb_TooltipContent tt_ConnectVertices = new pb_TooltipContent(
		// 	"Connect Vertices",
		// 	"Creates a new edge connecting two or more vertices."
		// 	);

		// private static readonly pb_TooltipContent tt_WeldVertices = new pb_TooltipContent(
		// 	"Weld Vertices",
		// 	"Checks all selected vertices for neighbors within a certain distance of one another, and if they are close enough they will be collapsed to a single vertex."
		// 	);

		// private static readonly pb_TooltipContent tt_CollapseVertices = new pb_TooltipContent(
		// 	"Collapse Vertices",
		// 	"Merges selected vertices to a single vertex."
		// 	);

		// private static readonly pb_TooltipContent tt_SplitVertices = new pb_TooltipContent(
		// 	"Split Vertices",
		// 	"Splits a vertex into separate vertices for each attached edge."
		// 	);

		public static List<pb_MenuAction> GetActions()
		{
			return new List<pb_MenuAction>()
			{
				// tools
				new OpenShapeEditor(),
				new OpenMaterialEditor(),
				new OpenUVEditor(),
				new OpenVertexColorEditor(),
				new OpenSmoothingEditor(),
				new OpenMirrorObjectsEditor(),

				new ToggleSelectBackFaces(),
				new ToggleHandleAlignment(),

				new GrowSelection(),
				new ShrinkSelection(),
				new InvertSelection(),
				new SelectEdgeLoop(),
				new SelectEdgeRing(),

				// object
				new MergeObjects(),
				new FlipObjectNormals(),
				new SubdivideObject(),
				new FreezeTransform(),
				new ConformObjectNormals(),
				new TriangulateObject(),

				// All
				new SetPivotToSelection(),

				// Faces (All)
				new DeleteFaces(),
				new DetachFaces(),
				new ExtrudeFaces(),

				// Face
				new ConformFaceNormals(),
				new FlipFaceEdge(),
				new FlipFaceNormals(),
				new MergeFaces(),
				new SubdivideFaces(),
				
				// Edge
				new BridgeEdges(),
				new ConnectEdges(),
				new ExtrudeEdges(),
				new InsertEdgeLoop(),

				// Vertex
				new CollapseVertices(),
				new WeldVertices(),
				new ConnectVertices(),
				new SplitVertices(),
			};
		}
	}
}

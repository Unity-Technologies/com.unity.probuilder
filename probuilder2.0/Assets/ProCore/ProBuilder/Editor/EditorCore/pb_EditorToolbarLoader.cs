using UnityEngine;
using System.Collections.Generic;
using ProBuilder2.Common;
using System.Linq;

namespace ProBuilder2.EditorCommon
{
	public class pb_EditorToolbarLoader
	{
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
				new pb_MenuAction_Tool( Icon("Panel_Shapes"), "Shape Editor", pb_Geometry_Interface.MenuOpenShapeCreator ),
				new pb_MenuAction_Tool( Icon("Panel_Materials"), "Material Editor", pb_Material_Editor.MenuOpenMaterialEditor ),
				new pb_MenuAction_Tool( Icon("Panel_UVEditor"), "UV Editor", pb_UV_Editor.MenuOpenUVEditor ),
				new pb_MenuAction_Tool( Icon("Panel_VertColors"), "Vertex Colors", pb_Menu_Commands.MenuOpenVertexColorsEditor ),
				new pb_MenuAction_Tool( Icon("Panel_Smoothing"), "Smoothing Editor", pb_Smoothing_Editor.MenuOpenSmoothingEditor ),
				new pb_MenuAction_Tool( Icon("Object_Mirror"), "Mirror Objects", pb_Mirror_Tool.MenuOpenMirrorEditor ),

				// interaction
				// new pb_MenuAction_Simple( Icon("HandleAlign_World"), "Handle Alignment World", (x) => { return new pb_ActionResult(Status.Failure, "Handle Alignment World"); }),
				// new pb_MenuAction_Simple( Icon("HandleAlign_Local"), "Handle Alignment Object", (x) => { return new pb_ActionResult(Status.Failure, "Handle Alignment Object"); }),
				// new pb_MenuAction_Simple( Icon("HandleAlign_Plane"), "Handle Alignment Plane", (x) => { return new pb_ActionResult(Status.Failure, "Handle Alignment Plane"); }),
				new pb_MenuAction_Toggle( 
					new Texture2D[] { Icon("Selection_SelectHidden-ON"), Icon("Selection_SelectHidden-OFF") },
					"Select All | Select Visible",
					(x) => { pb_Editor.instance.SetSelectHiddenEnabled(x == 0); }
					),

				// selection
				new pb_MenuAction_Element(Icon("Selection_Grow"), "Grow Selection", pb_Menu_Commands.MenuGrowSelection, SelectMode.Vertex, false, pb_Menu_Commands.VerifyGrowSelection),
				// new pb_MenuAction_Simple( Icon("Selection_Grow"), "Grow Selection", pb_Menu_Commands.MenuGrowSelection ),
				// new pb_MenuAction_Simple( Icon("Selection_Shrink"), "Shrink Selection", pb_Menu_Commands.MenuShrinkSelection ),
				// new pb_MenuAction_Simple( Icon("Selection_Invert"), "Invert Selection", pb_Menu_Commands.MenuInvertSelection ),
				// new pb_MenuAction_Simple( Icon("Selection_Ring"), "Select Edge Ring", pb_Menu_Commands.MenuRingSelection ),
				// new pb_MenuAction_Simple( Icon("Selection_Loop"), "Select Edge Loop", pb_Menu_Commands.MenuLoopSelection ),
					
				// object
				new pb_MenuAction_Object( Icon("Object_Merge"), "Merge Objects", pb_Menu_Commands.MenuMergeObjects, (x) => { return x != null && x.Length >= 2;} ),
				new pb_MenuAction_Object( Icon("Object_FlipNormals"), "Flip Object Normals", pb_Menu_Commands.MenuFlipObjectNormals ),
				new pb_MenuAction_Object( Icon("Object_Subdivide"), "Subdivide Object", pb_Menu_Commands.MenuSubdivide ),
				new pb_MenuAction_Object( Icon("Pivot_Reset"), "Freeze Transform", pb_Menu_Commands.MenuFreezeTransforms ),
				new pb_MenuAction_Object( Icon("null"), "Conform Object Normals", pb_Menu_Commands.MenuConformObjectNormals ),
				new pb_MenuAction_Object( Icon("null"), "Triangulate Object", pb_Menu_Commands.MenuTriangulateObject ),
				
				// // elements all
				CreateVertexAction("Pivot_MoveToCenter", "Set Pivot to Center of Selection", pb_Menu_Commands.MenuSetPivot, false, 1),	// @todo

				// // elements face
				CreateFaceAction("Face_Extrude", "Extrude Faces", pb_Menu_Commands.MenuExtrude),							// @todo
				CreateFaceAction("null", "Conform Face Normals", pb_Menu_Commands.MenuConformNormals, false, 3),
				CreateFaceAction("Face_FlipNormals", "Flip Face Normals", pb_Menu_Commands.MenuFlipNormals ),
				CreateFaceAction("null", "Flip Face Edge", pb_Menu_Commands.MenuFlipEdges),
				CreateFaceAction("Face_Delete", "Delete Faces", pb_Menu_Commands.MenuDeleteFace),
				CreateFaceAction("Face_Detach", "Detach Faces", pb_Menu_Commands.MenuDetachFaces),
				CreateFaceAction("Face_Merge", "Merge Faces", pb_Menu_Commands.MenuMergeFaces, false, 2),
				CreateFaceAction("Face_Subdivide", "Subdivide Faces", pb_Menu_Commands.MenuSubdivideFace),
				
				// elements edge
				CreateEdgeAction("Edge_Extrude", "Extrude Edges", pb_Menu_Commands.MenuExtrude),							// @todo
				CreateEdgeAction("Edge_Bridge", "Bridge Edges", pb_Menu_Commands.MenuBridgeEdges, true, 2),
				CreateEdgeAction("Edge_Connect", "Connect Edges", pb_Menu_Commands.MenuConnectEdges, true, 2),
				CreateEdgeAction("Edge_InsertLoop", "Insert Edge Loop", pb_Menu_Commands.MenuInsertEdgeLoop),

				CreateVertexAction("Vert_Connect", "Connect Vertices", pb_Menu_Commands.MenuConnectVertices, false, 2),
				CreateVertexAction("Vert_Weld", "Weld Vertices", pb_Menu_Commands.MenuWeldVertices, false, 2),				// @todo
				CreateVertexAction("Vert_Collapse", "Collapse Vertices", pb_Menu_Commands.MenuCollapseVertices, false, 2),	// @todo
				CreateVertexAction("Vert_Split", "Split Vertices", pb_Menu_Commands.MenuSplitVertices)						// @todo
			};
		}
	}
}

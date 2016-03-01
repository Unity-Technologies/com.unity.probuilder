using UnityEngine;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	public class pb_EditorToolbarLoader
	{
		private static Texture2D Icon(string path)
		{
			return pb_IconUtility.GetIcon(path);
		}

		public static List<pb_MenuAction> GetActions()
		{
			return new List<pb_MenuAction>()
			{
				new pb_MenuAction_Simple( Icon("Panel_Shapes"), "Shape", (x) => { return new pb_ActionResult(Status.Success, "Shape"); }),
				new pb_MenuAction_Simple( Icon("Panel_Materials"), "Material Editor", (x) => { return new pb_ActionResult(Status.Success, "Material Editor"); }),
				new pb_MenuAction_Simple( Icon("Panel_UVEditor"), "UV Editor", (x) => { return new pb_ActionResult(Status.Success, "UV Editor"); }),
				new pb_MenuAction_Simple( Icon("Panel_VertColors"), "Vertex Colors", (x) => { return new pb_ActionResult(Status.Success, "Vertex Colors"); }),
				new pb_MenuAction_Simple( Icon("Panel_Smoothing"), "Smoothing Editor", (x) => { return new pb_ActionResult(Status.Success, "Smoothing Editor"); }),
				new pb_MenuAction_Simple( Icon("HandleAlign_World"), "Handle Alignment World", (x) => { return new pb_ActionResult(Status.Success, "Handle Alignment World"); }),
				new pb_MenuAction_Simple( Icon("HandleAlign_Local"), "Handle Alignment Object", (x) => { return new pb_ActionResult(Status.Success, "Handle Alignment Object"); }),
				new pb_MenuAction_Simple( Icon("HandleAlign_Plane"), "Handle Alignment Plane", (x) => { return new pb_ActionResult(Status.Success, "Handle Alignment Plane"); }),
				new pb_MenuAction_Simple( Icon("Selection_SelectHidden-ON"), "Select All | Select Visible", (x) => { return new pb_ActionResult(Status.Success, "Select All | Select Visible"); }),
				new pb_MenuAction_Simple( Icon("Selection_Grow"), "Grow Selection", (x) => { return new pb_ActionResult(Status.Success, "Grow Selection"); }),
				new pb_MenuAction_Simple( Icon("Selection_Shrink"), "Shrink Selection", (x) => { return new pb_ActionResult(Status.Success, "Shrink Selection"); }),
				new pb_MenuAction_Simple( Icon("Selection_Invert"), "Invert Selection", (x) => { return new pb_ActionResult(Status.Success, "Invert Selection"); }),
				new pb_MenuAction_Simple( Icon("Selection_Ring"), "Select Edge Ring", (x) => { return new pb_ActionResult(Status.Success, "Select Edge Ring"); }),
				new pb_MenuAction_Simple( Icon("Selection_Loop"), "Select Edge Loop", (x) => { return new pb_ActionResult(Status.Success, "Select Edge Loop"); }),
				new pb_MenuAction_Simple( Icon("Object_Merge"), "Merge Objects", (x) => { return new pb_ActionResult(Status.Success, "Merge Objects"); }),
				new pb_MenuAction_Simple( Icon("Object_Mirror"), "Mirror Objects", (x) => { return new pb_ActionResult(Status.Success, "Mirror Objects"); }),
				new pb_MenuAction_Simple( Icon("Object_FlipNormals"), "Flip Object Normals", (x) => { return new pb_ActionResult(Status.Success, "Flip Object Normals"); }),
				new pb_MenuAction_Simple( Icon("Object_Subdivide"), "Subdivide Object", (x) => { return new pb_ActionResult(Status.Success, "Subdivide Object"); }),
				new pb_MenuAction_Simple( Icon("Pivot_Reset"), "Freeze Transform", (x) => { return new pb_ActionResult(Status.Success, "Freeze Transform"); }),
				new pb_MenuAction_Simple( Icon("null"), "Conform Object Normals", (x) => { return new pb_ActionResult(Status.Success, "Conform Object Normals"); }),
				new pb_MenuAction_Simple( Icon("null"), "Triangulate Object", (x) => { return new pb_ActionResult(Status.Success, "Triangulate Object"); }),
				new pb_MenuAction_Simple( Icon("Pivot_MoveToCenter"), "Set Pivot to Center of Selection", (x) => { return new pb_ActionResult(Status.Success, "Set Pivot to Center of Selection"); }),
				new pb_MenuAction_Simple( Icon("Face_Extrude"), "Extrude Face", (x) => { return new pb_ActionResult(Status.Success, "Extrude Face"); }),
				new pb_MenuAction_Simple( Icon("Edge_Extrude"), "Extrude Edge", (x) => { return new pb_ActionResult(Status.Success, "Extrude Edge"); }),
				new pb_MenuAction_Simple( Icon("null"), "Conform Face Normals", (x) => { return new pb_ActionResult(Status.Success, "Conform Face Normals"); }),
				new pb_MenuAction_Simple( Icon("Face_FlipNormals"), "Flip Face Normals", (x) => { return new pb_ActionResult(Status.Success, "Flip Face Normals"); }),
				new pb_MenuAction_Simple( Icon("null"), "Flip Edge", (x) => { return new pb_ActionResult(Status.Success, "Flip Edge"); }),
				new pb_MenuAction_Simple( Icon("Face_Delete"), "Delete Faces", (x) => { return new pb_ActionResult(Status.Success, "Delete Faces"); }),
				new pb_MenuAction_Simple( Icon("Face_Detach"), "Detach Faces", (x) => { return new pb_ActionResult(Status.Success, "Detach Faces"); }),
				new pb_MenuAction_Simple( Icon("Face_Merge"), "Merge Faces", (x) => { return new pb_ActionResult(Status.Success, "Merge Faces"); }),
				new pb_MenuAction_Simple( Icon("Edge_Bridge"), "Bridge Edges", (x) => { return new pb_ActionResult(Status.Success, "Bridge Edges"); }),
				new pb_MenuAction_Simple( Icon("Edge_Connect"), "Connect Edges", (x) => { return new pb_ActionResult(Status.Success, "Connect Edges"); }),
				new pb_MenuAction_Simple( Icon("Vert_Connect"), "Connect Vertices", (x) => { return new pb_ActionResult(Status.Success, "Connect Vertices"); }),
				new pb_MenuAction_Simple( Icon("Face_Subdivide"), "Subdivide Faces", (x) => { return new pb_ActionResult(Status.Success, "Subdivide Faces"); }),
				new pb_MenuAction_Simple( Icon("Vert_Weld"), "Weld Vertices", (x) => { return new pb_ActionResult(Status.Success, "Weld Vertices"); }),
				new pb_MenuAction_Simple( Icon("Vert_Collapse"), "Collapse Vertices", (x) => { return new pb_ActionResult(Status.Success, "Collapse Vertices"); }),
				new pb_MenuAction_Simple( Icon("Vert_Split"), "Split Vertices", (x) => { return new pb_ActionResult(Status.Success, "Split Vertices"); }),
				new pb_MenuAction_Simple( Icon("Edge_InsertLoop"), "Insert Edge Loop", (x) => { return new pb_ActionResult(Status.Success, "Insert Edge Loop"); })
			};
		}
	}
}

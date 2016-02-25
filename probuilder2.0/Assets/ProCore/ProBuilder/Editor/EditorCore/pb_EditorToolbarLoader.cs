using UnityEngine;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	public class pb_EditorToolbarLoader
	{

		public static List<pb_MenuAction> GetActions()
		{

			return new List<pb_MenuAction>()
			{
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("New3DShape"), 			(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Shape"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("MaterialToolsPanel"), 	(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Material Editor"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("UV"), 					(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "UV Editor"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("VertexColorToolsPanel"), 	(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Vertex Colors"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("SmoothingToolsPanel"), 	(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Smoothing Editor"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("HandleAlignment-World"), 	(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Handle Alignment World"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("HandleAlignment-Local"), 	(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Handle Alignment Object"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("HandleAlignment-Plane"), 	(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Handle Alignment Plane"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("SelectVisibleOnly"), 		(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Select All | Select Visible"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("Selection-Grow"),  		(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Grow Selection"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("Selection-Shrink"),  		(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Shrink Selection"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("Selection-Invert"),  		(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Invert Selection"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("null"), 					(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Select Edge Ring"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("null"), 					(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Select Edge Loop"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("MergeObjects"), 			(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Merge Objects"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("MirrorObject"), 			(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Mirror Objects"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("FlipObjectNormals"), 		(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Flip Object Normals"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("null"), 					(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Subdivide Object"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("ResetPivot"), 			(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Freeze Transform"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("null"), 					(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Conform Object Normals"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("null"), 					(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Triangulate Object"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("EditPivot"), 				(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Set Pivot to Center of Selection"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("ExtrudeFace"), 			(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Extrude Face"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("null"), 					(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Extrude Edge"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("null"), 					(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Conform Face Normals"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("null"), 					(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Flip Face Normals"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("null"), 					(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Flip Edge"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("DeleteSelectedFaces"), 	(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Delete Faces"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("DetachFaces"), 			(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Detach Faces"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("MergeFaces"), 			(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Merge Faces"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("null"), 					(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Bridge Edges"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("null"), 					(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Connect Edges"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("ConnectVerts"), 			(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Connect Vertices"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("SubdivideFace"), 			(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Subdivide Faces"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("WeldVerts"), 				(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Weld Vertices"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("CollapseVerts"), 			(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Collapse Vertices"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("null"), 					(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Split Vertices"); }),
				new pb_MenuAction_Simple( pb_IconUtility.GetIcon("null"), 					(x) => { return new pb_ActionResult(pb_ActionResult.Status.Success, "Insert Edge Loop"); })
			};
		}
	}
}

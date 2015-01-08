using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.Math;
using ProBuilder2.MeshOperations;

public class pb_DeleteVertices : MonoBehaviour {

	[MenuItem("Tools/ProBuilder/Geometry/Delete Vertices")]
	public static void MenuDeleteVertices()
	{
		pb_Object[] sel = pbUtil.GetComponents<pb_Object>(Selection.transforms);

		pbUndo.RecordObjects(sel, "Delete Vertices");

		foreach(pb_Object pb in sel)
		{
			pb_Face face = DeleteSelectedVertices(pb);

			if(face != null)
			{
				pb.Refresh();
				pb.GenerateUV2();

				pb.SetSelectedFaces( new pb_Face[] { face } );
			}
		}

		if(pb_Editor.instance)
			pb_Editor.instance.UpdateSelection();
	}

	static pb_Face DeleteSelectedVertices(pb_Object pb)
	{
		int[] selected = pb.sharedIndices.AllIndicesWithValues(pb.SelectedTriangles);

		pb_Face[] selected_faces = pbMeshUtils.GetNeighborFaces(pb, selected);

		if(selected_faces.Length < 1)
			return null;

		Vector3 nrm = pb_Math.Normal(pb, selected_faces[0]);

		pb.DeleteVerticesWithIndices(selected);

		pb_Face composite = pb.MergeFaces(selected_faces);

		pb.TriangulateFace(composite, nrm);

		int[] removed;
		pb.RemoveDegenerateTriangles(out removed);

		return composite;
	}
}

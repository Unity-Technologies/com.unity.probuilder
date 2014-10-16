using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;

/**
 * Triangulates a ProBuilder object.
 *
 * MenuItem: Tools -> ProBuilder -> Actions -> Triangulate Selection 
 */
public class TriangulatePbObject : Editor
{
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/Triangulate Selection", true)]
	public static bool MenuVerifyTriangulateSelection()
	{
		return pbUtil.GetComponents<pb_Object>(Selection.transforms).Length > 0;
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/Triangulate Selection")]
	public static void MenuTriangulatePbObjects()
	{
		pb_Object[] selection = pbUtil.GetComponents<pb_Object>(Selection.transforms);

		pbUndo.RecordObjects(selection, "Triangulate Objects");

		for(int i = 0; i < selection.Length; i++)
		{
			Triangulate(selection[i]);

			selection[i].GenerateUV2(true);
			selection[i].Refresh();
		}

		pb_Editor_Utility.ShowNotification(selection.Length > 0 ? "Triangulate" : "Nothing Selected");
	}

	static void Triangulate(pb_Object pb)
	{
		Vector3[] 	v = pb.vertices;
		Vector2[] 	u = pb.msh.uv;

		int triangleCount = pb.msh.triangles.Length;

		if(triangleCount == v.Length)
		{
			Debug.LogWarning("We can't pull over any further!\npb_Object: " + pb.name + " is already triangulated.");
		}

		int vertexCount = triangleCount;
		int faceCount = vertexCount / 3;

		Vector3[]	tri_vertices = new Vector3[vertexCount];
		Vector2[]	tri_uvs = new Vector2[vertexCount];
		pb_Face[]	tri_faces = new pb_Face[faceCount];

		int n = 0, f = 0;
		foreach(pb_Face face in pb.faces)
		{
			int[] indices = face.indices;

			for(int i = 0; i < indices.Length; i+=3)
			{
				tri_vertices[n+0] = v[indices[i+0]];
				tri_vertices[n+1] = v[indices[i+1]];
				tri_vertices[n+2] = v[indices[i+2]];

				tri_uvs[n+0] = u[indices[i+0]];
				tri_uvs[n+1] = u[indices[i+1]];
				tri_uvs[n+2] = u[indices[i+2]];
	
				tri_faces[f++] = new pb_Face( new int[] { n+0, n+1, n+2 },
											face.material,
											face.uv,
											face.smoothingGroup,
											face.textureGroup,		// textureGroup -> force to manual uv mode
											face.elementGroup,
											face.manualUV,			// manualUV
											face.color
										);	
				n += 3;
			}

		}

		pb.SetVertices(tri_vertices);
		pb.SetUV(tri_uvs);
		pb.SetFaces(tri_faces);

		pb.SetSharedIndices( pb_IntArrayUtility.ExtractSharedIndices(tri_vertices) );
		pb.SetSharedIndicesUV( new pb_IntArray[0] );
	}
}

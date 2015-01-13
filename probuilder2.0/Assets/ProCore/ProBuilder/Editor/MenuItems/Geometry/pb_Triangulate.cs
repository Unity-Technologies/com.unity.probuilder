using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;

/**
 * Triangulates a ProBuilder object.
 *
 * MenuItem: Tools -> ProBuilder -> Geometry -> Triangulate Selection 
 */
public class pb_Triangulate : Editor
{
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Triangulate Object", true, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_OBJECT)]
	public static bool MenuVerifyTriangulateSelection()
	{
		return pbUtil.GetComponents<pb_Object>(Selection.transforms).Length > 0;
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Triangulate Object", false, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_OBJECT)]
	public static void MenuTriangulatePbObjects()
	{
		pb_Object[] selection = pbUtil.GetComponents<pb_Object>(Selection.transforms);

		pbUndo.RecordObjects(selection, "Triangulate Objects");

		for(int i = 0; i < selection.Length; i++)
		{

			Triangulate(selection[i]);

			selection[i].ToMesh();
			selection[i].Refresh();
			selection[i].GenerateUV2();
		}

		if(pb_Editor.instance)
		{
			pb_Editor.instance.UpdateSelection();
		}

		pb_Editor_Utility.ShowNotification(selection.Length > 0 ? "Triangulate" : "Nothing Selected");
	}

	static void Triangulate(pb_Object pb)
	{
		Vector3[] 	v = pb.vertices;
		Color[] 	c = pb.colors;
		Vector2[] 	u = pb.uv;

		int triangleCount = pb.TriangleCount();
		// int triangleCount = pb_Face.AllTriangles(pb.faces).Length; // pb.msh.triangles.Length;

		if(triangleCount == v.Length)
		{
			Debug.LogWarning("We can't pull over any further!\npb_Object: " + pb.name + " is already triangulated.");
		}

		int vertexCount = triangleCount;
		int faceCount = vertexCount / 3;

		Vector3[]	tri_vertices = new Vector3[vertexCount];
		Color[] 	tri_colors = new Color[vertexCount];
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

				tri_colors[n+0] = c[indices[i+0]];
				tri_colors[n+1] = c[indices[i+1]];
				tri_colors[n+2] = c[indices[i+2]];

				tri_uvs[n+0] = u[indices[i+0]];
				tri_uvs[n+1] = u[indices[i+1]];
				tri_uvs[n+2] = u[indices[i+2]];
	
				tri_faces[f++] = new pb_Face( new int[] { n+0, n+1, n+2 },
											face.material,
											face.uv,
											face.smoothingGroup,
											face.textureGroup,		// textureGroup -> force to manual uv mode
											face.elementGroup,
											face.manualUV
										);	
				n += 3;
			}

		}

		pb.SetVertices(tri_vertices);
		pb.SetColors(tri_colors);
		pb.SetUV(tri_uvs);
		pb.SetFaces(tri_faces);

		pb.SetSharedIndices( pb_IntArrayUtility.ExtractSharedIndices(tri_vertices) );
		pb.SetSharedIndicesUV( new pb_IntArray[0] );
	}
}

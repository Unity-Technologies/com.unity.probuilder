using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

namespace ProBuilder2.Actions
{
public class ProBuilderizeMesh : Editor
{
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/ProBuilderize Selection", true, pb_Constant.MENU_ACTIONS + 1)]
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/ProBuilderize Selection (Preserve Faces)", true, pb_Constant.MENU_ACTIONS + 2)]
	public static bool VerifyProBuilderize()
	{
		return Selection.transforms.Length - pbUtil.GetComponents<pb_Object>(Selection.transforms).Length > 0;
	}	

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/ProBuilderize Selection", false, pb_Constant.MENU_ACTIONS + 1)]
	public static void MenuProBuilderizeTris()
	{
		ProBuilderizeObjects(false);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/ProBuilderize Selection (Preserve Faces)", false, pb_Constant.MENU_ACTIONS + 2)]
	public static void MenuProBuilderizeQuads()
	{
		ProBuilderizeObjects(true);
	}

	public static void ProBuilderizeObjects(bool preserveFaces)
	{
		foreach(Transform t in Selection.transforms)
		{
			if(t.GetComponent<MeshFilter>())
			{
				pb_Object pb = ProBuilderize(t, preserveFaces);

				if(pb.GetComponent<MeshCollider>())	
					DestroyImmediate(pb.GetComponent<MeshCollider>());
			}
		}
	}

	public static pb_Object ProBuilderize(Transform t, bool preserveFaces)
	{
		Mesh m = t.GetComponent<MeshFilter>().sharedMesh;
		Vector3[] m_vertices = m.vertices;
		Vector2[] m_uvs = m.uv;

		List<Vector3> verts = preserveFaces ? new List<Vector3>(m.vertices) : new List<Vector3>();
		List<Vector2> uvs = preserveFaces ? new List<Vector2>(m.uv) : new List<Vector2>();
		List<pb_Face> faces = new List<pb_Face>();

		for(int n = 0; n < m.subMeshCount; n++)
		{
			int[] tris = m.GetTriangles(n);
			for(int i = 0; i < tris.Length; i+=3)
			{
				int index = -1;
				if(preserveFaces)
				{
					for(int j = 0; j < faces.Count; j++)
					{
						if(	faces[j].distinctIndices.Contains(tris[i+0]) ||
							faces[j].distinctIndices.Contains(tris[i+1]) ||
							faces[j].distinctIndices.Contains(tris[i+2]))
						{
							index = j;
							break;
						}
					}
				}

				if(index > -1 && preserveFaces)
				{
					int len = faces[index].indices.Length;
					int[] arr = new int[len + 3];
					System.Array.Copy(faces[index].indices, 0, arr, 0, len);
					arr[len+0] = tris[i+0];
					arr[len+1] = tris[i+1];
					arr[len+2] = tris[i+2];
					faces[index].SetIndices(arr);
					faces[index].RebuildCaches();
				}
				else
				{
					int[] faceTris;

					if(preserveFaces)
					{
						faceTris = new int[3]
						{
							tris[i+0],
							tris[i+1],
							tris[i+2]	
						};
					}
					else
					{
						verts.Add(m_vertices[tris[i+0]]);
						verts.Add(m_vertices[tris[i+1]]);
						verts.Add(m_vertices[tris[i+2]]);

						uvs.Add(m_uvs[tris[i+0]]);
						uvs.Add(m_uvs[tris[i+1]]);
						uvs.Add(m_uvs[tris[i+2]]);
						faceTris = new int[3] { i+0, i+1, i+2 };
					}

					faces.Add( 
						new pb_Face(
							faceTris,
							t.GetComponent<MeshRenderer>().sharedMaterials[n],
							new pb_UV(),
							0,		// smoothing group
							-1,		// texture group
							-1,		// element group
							true, 	// manualUV 
							Color.white
						));					
				}
			}
		}

		GameObject go = (GameObject)GameObject.Instantiate(t.gameObject);
		go.GetComponent<MeshFilter>().sharedMesh = null;

		pb_Object pb = go.AddComponent<pb_Object>();
		pb.GeometryWithVerticesFaces(verts.ToArray(), faces.ToArray());

		pb.SetUV(uvs.ToArray());

		pb.SetName(t.name);
		
		pb_Editor_Utility.SetEntityType(EntityType.Detail, pb.gameObject);
		
		go.transform.position = t.position;
		go.transform.localRotation = t.localRotation;
		go.transform.localScale = t.localScale;
		
		// pb.FreezeScaleTransform();

#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5	
		t.gameObject.active = false;
#else
		t.gameObject.SetActive(false);
#endif

		return pb;
	}
}
}
using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;

public class VertexRender : Editor
{
	[MenuItem("Window/Render Verts")]
	public static void Init()
	{
		// foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
		// {
		// 	Vector3[] v = new Vector3[pb.uniqueIndices.Length];
		// 	for(int i = 0; i < v.Length; i++)	
		// 		v[i] = pb.vertices[pb.uniqueIndices[i]];

		// 	Vector3[] billboards 	= new Vector3[v.Length*4];
		// 	Vector3[] nrm 			= new Vector3[v.Length*4];
		// 	Vector2[] uvs 			= new Vector2[v.Length*4];
		// 	Vector2[] uv2s 			= new Vector2[v.Length*4];
		// 	Vector4[] tan 			= new Vector4[v.Length*4];
		// 	Color[] col 			= new Color[v.Length*4];

		// 	// Vector3 camPos = SceneView.lastActiveSceneView.camera.transform.position;
		// 	int[] tris = new int[v.Length*6];
		// 	int n = 0;
		// 	int t = 0;

		// 	Vector3 up = Vector3.up;// * .1f;
		// 	Vector3 right = Vector3.right;// * .1f;

		// 	for(int i = 0; i < v.Length; i++)
		// 	{

		// 		billboards[t+0] = pb.transform.TransformPoint(v[i]);//-up-right;
		// 		billboards[t+1] = pb.transform.TransformPoint(v[i]);//-up+right;
		// 		billboards[t+2] = pb.transform.TransformPoint(v[i]);//+up-right;
		// 		billboards[t+3] = pb.transform.TransformPoint(v[i]);//+up+right;

		// 		uvs[t+0] = Vector3.zero;
		// 		uvs[t+1] = Vector3.right;
		// 		uvs[t+2] = Vector3.up;
		// 		uvs[t+3] = Vector3.one;

		// 		tan[t+0] = pb.transform.position;
		// 		tan[t+1] = pb.transform.position;
		// 		tan[t+2] = pb.transform.position;
		// 		tan[t+3] = pb.transform.position;

		// 		uv2s[t+0] = -up-right;
		// 		uv2s[t+1] = -up+right;
		// 		uv2s[t+2] =  up-right;
		// 		uv2s[t+3] =  up+right;

		// 		nrm[t+0] = Vector3.forward;
		// 		nrm[t+1] = Vector3.forward;
		// 		nrm[t+2] = Vector3.forward;
		// 		nrm[t+3] = Vector3.forward;

		// 		tris[n+0] = t+2;
		// 		tris[n+1] = t+1;
		// 		tris[n+2] = t+0;
		// 		tris[n+3] = t+2;
		// 		tris[n+4] = t+3;
		// 		tris[n+5] = t+1;

		// 		col[t+0] = new Color(0f, 0f, 1f, 1f);
		// 		col[t+1] = new Color(0f, 0f, 1f, 1f);
		// 		col[t+2] = new Color(0f, 0f, 1f, 1f);
		// 		col[t+3] = new Color(0f, 0f, 1f, 1f);
				
		// 		t+=4;
		// 		n+=6;
		// 	}

		// 	Mesh m = new Mesh();
		// 	m.vertices = billboards;
		// 	m.uv = uvs;
		// 	m.uv2 = uv2s;
		// 	m.tangents = tan;
		// 	m.colors = col;
		// 	m.triangles = tris;
		// 	GameObject go = new GameObject();
		// 	// go.transform.position = pb.transform.position;
		// 	// go.transform.localRotation = pb.transform.localRotation;
		// 	go.AddComponent<MeshFilter>().sharedMesh = m;
		// 	go.AddComponent<MeshRenderer>().sharedMaterial = Resources.Load("Test", typeof(Material)) as Material;
		// }
	}
}

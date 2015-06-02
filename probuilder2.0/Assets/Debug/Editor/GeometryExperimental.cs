using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

namespace ProBuilder2.EditorCommon

{
	public class SharedVerticesGen : Editor
	{

		[MenuItem("Tools/ProBuilder/Create TORUS %T")]
		static void INtsdt()
		{
			GameObject go = GameObject.Find("TORUS_DEBUG");
			if(go != null)
				GameObject.DestroyImmediate(go);

			pb_Object pb = pb_ShapeGenerator.TorusGenerator(16, 32, 2f, .5f, true, 360f, 360f);
			pb.gameObject.name = "TORUS_DEBUG";
		}


		[MenuItem("Tools/ProBuilder/Create Vertex Billboard")]
		static void initdsafd()
		{
			Vector3[] v = new Vector3[4]
			{
				Vector3.zero,
				Vector3.zero,
				Vector3.zero,
				Vector3.zero
			};

			Vector3[] n = new Vector3[]
			{
				Vector3.forward,
				Vector3.forward,
				Vector3.forward,
				Vector3.forward
			};

			Vector2[] u = new Vector2[]
			{
				Vector2.zero,
				Vector2.right,
				Vector2.up,
				Vector2.one
			};

			Vector2[] u2 = new Vector2[]
			{
				new Vector2(-1, -1),
				new Vector2( 1, -1),
				new Vector2(-1,  1),
				new Vector2( 1,  1)
			};

			Mesh m = new Mesh();
			m.vertices = v;
			m.triangles = new int[] { 	2, 1, 0, 2, 3, 1,
										0, 1, 2, 1, 3, 2 };
			m.normals = n;
			m.uv = u;
			m.uv2 = u2;

			GameObject go = new GameObject();
			go.AddComponent<MeshRenderer>().sharedMaterial = (Material)Resources.Load("VertexMaterial");
			go.AddComponent<MeshFilter>().sharedMesh = m;
		}


		[MenuItem("Tools/ProBuilder/Create Shared Plane")]
		static void Init()
		{
			Vector3[] v = new Vector3[]
			{
				new Vector3(-.5f, 0f, 0f),
				new Vector3( .5f, 0f, 0f),
				new Vector3(-.5f, 1f, 0f),
				new Vector3( .5f, 1f, 0f),

				new Vector3(-.5f, 2f, 0f),
				new Vector3( .5f, 2f, 0f)
			};

			// 4, 5
			// 2, 3
			// 0, 1
			pb_Face[] f = new pb_Face[] 
			{
				new pb_Face(new int[] { 0, 1, 2, 1, 3, 2} ),
				new pb_Face(new int[] { 2, 3, 4, 3, 5, 4} )
			};

			pb_Object pb = pb_Object.CreateInstanceWithVerticesFaces(v, f);

			pb_Editor_Utility.InitObjectFlags(pb, ColliderType.None, EntityType.Detail);
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	[ExecuteInEditMode]
	public class pb_LineRenderer : MonoBehaviour
	{
		// HideFlags.DontSaveInEditor isn't exposed for whatever reason, so do the bit math on ints 
		// and just cast to HideFlags.
		// HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable
		HideFlags SceneCameraHideFlags = (HideFlags) (1 | 4 | 8);

		[HideInInspector] pb_ObjectPool pool = new pb_ObjectPool(1, 16, MeshConstructor);

		static Mesh MeshConstructor()
		{
			return new Mesh();
		}

		[HideInInspector]
		public List<Mesh> gizmos = new List<Mesh>();

		[HideInInspector]
		public Material mat;

		void OnEnable()
		{
			// mat = new Material(Shader.Find("Hidden/ProBuilder/FaceHighlight"));
			mat = new Material(Shader.Find("ProBuilder/UnlitVertexColor"));
			mat.name = "pb_LineRenderer_Material";
			mat.SetColor("_Color", Color.white);
			mat.hideFlags = pb_Constant.EDITOR_OBJECT_HIDE_FLAGS;
		}

		public void AddLineSegments(Vector3[] segments, Color[] colors)
		{
			Mesh m = (Mesh) pool.Get();

			m.Clear();
			m.MarkDynamic();

			int vc = segments.Length;
			int cc = colors.Length;

			m.vertices = segments;

			int[] tris = new int[vc];
			Color[] col = new Color[vc];

			int n = 0;
			for(int i = 0; i < vc; i++)
			{
				tris[i] = i;
				col[i] = colors[n%cc];
				if(i % 2 == 1) n++;
			}


			m.subMeshCount = 1;
			m.SetIndices(tris, MeshTopology.Lines, 0);
			m.uv = new Vector2[m.vertexCount];
			m.colors = col;

			m.hideFlags = pb_Constant.EDITOR_OBJECT_HIDE_FLAGS;

			gizmos.Add(m);
		}

		void OnDestroy()
		{
			foreach(Mesh m in gizmos)
				DestroyImmediate(m);

			DestroyImmediate(mat);

			pool.Empty();
		}

		void OnRenderObject()
		{
			// instead of relying on 'SceneCamera' string comparison, check if the hideflags match.
			// this could probably even just check for one bit match, since chances are that any 
			// game view camera isn't going to have hideflags set.
			if( (Camera.current.gameObject.hideFlags & SceneCameraHideFlags) != SceneCameraHideFlags || Camera.current.name != "SceneCamera" )
				return;

			mat.SetPass(0);

			for(int i = 0; i < gizmos.Count; i++) {
				Graphics.DrawMeshNow(gizmos[i], Vector3.zero, Quaternion.identity, 0);
				pool.Put(gizmos[i]);
			}

			gizmos.Clear();
		}
	}
}
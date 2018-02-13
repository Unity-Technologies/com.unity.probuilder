using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder.Core
{
	/// <summary>
	/// Renders lines to the scene view.  Handles material and mesh generation and management.
	/// </summary>
	/// <remarks>For general purpose rendering to the sceneview, use pb_MeshRenderer.</remarks>
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	class pb_LineRenderer : pb_MonoBehaviourSingleton<pb_LineRenderer>
	{
		// HideFlags.DontSaveInEditor isn't exposed for whatever reason, so do the bit math on ints
		// and just cast to HideFlags.
		// HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable
		HideFlags SceneCameraHideFlags = (HideFlags) (1 | 4 | 8);

		pb_ObjectPool<Mesh> pool;

		static Mesh MeshConstructor()
		{
			Mesh m = new Mesh();
			m.hideFlags = pb_Constant.k_EditorHideFlags;
			m.name = "pb_LineRenderer::Mesh";
			return m;
		}

		[HideInInspector]
		public List<Mesh> gizmos = new List<Mesh>();

		[HideInInspector]
		public Material mat;

		public override void OnEnable()
		{
			base.OnEnable();
			pool = new pb_ObjectPool<Mesh>(1, 8, MeshConstructor, null);
		}

		void OnDisable()
		{
			pool.Empty();
		}

		public override void Awake()
		{
			base.Awake();

			gameObject.hideFlags = HideFlags.HideAndDontSave;

			mat = new Material(Shader.Find("ProBuilder/UnlitVertexColor"));
			mat.name = "pb_LineRenderer_Material";
			mat.SetColor("_Color", Color.white);
			mat.hideFlags = pb_Constant.k_EditorHideFlags;
		}

		void OnDestroy()
		{
			foreach(Mesh m in gizmos)
			{
				if(m != null)
					DestroyImmediate(m);
			}

			DestroyImmediate(mat);
		}

		public void AddLineSegments(Vector3[] segments, Color[] colors)
		{
			// Because editor scripts are enabled first, pool could still be null
			// when first request comes in.
			if(pool == null)
				pool = new pb_ObjectPool<Mesh>(1, 4, MeshConstructor, null);

			Mesh m = pool.Get();

			m.Clear();
			m.name = "pb_LineRenderer::Mesh_" + m.GetInstanceID();
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

			m.hideFlags = pb_Constant.k_EditorHideFlags;

			gizmos.Add(m);
		}

		/**
		 * Clear the queue of line segments to render.
		 */
		public void Clear()
		{
			for(int i = 0; i < gizmos.Count; i++)
				pool.Put(gizmos[i]);

			gizmos.Clear();
		}

		void OnRenderObject()
		{
			// instead of relying on 'SceneCamera' string comparison, check if the hideflags match.
			// this could probably even just check for one bit match, since chances are that any
			// game view camera isn't going to have hideflags set.
			if( mat == null || (Camera.current.gameObject.hideFlags & SceneCameraHideFlags) != SceneCameraHideFlags || Camera.current.name != "SceneCamera" )
				return;

			mat.SetPass(0);

			for(int i = 0; i < gizmos.Count && gizmos[i] != null; i++) {
				Graphics.DrawMeshNow(gizmos[i], Vector3.zero, Quaternion.identity, 0);
			}
		}
	}
}
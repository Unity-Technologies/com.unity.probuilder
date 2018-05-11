using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;

namespace UnityEngine.ProBuilder
{
	/// <inheritdoc />
	/// <summary>
	/// Renders lines to the scene view. Handles material and mesh generation and management.
	/// </summary>
	sealed class SceneViewLineRenderer : IDisposable
	{
		ObjectPool<Mesh> m_Pool;
		bool m_IsDisposed;

		static Mesh MeshConstructor()
		{
			Mesh m = new Mesh();
			m.hideFlags = PreferenceKeys.k_EditorHideFlags;
			m.name = "pb_LineRenderer::Mesh";
			return m;
		}

		public List<Mesh> gizmos = new List<Mesh>();
		public Material mat;

		public SceneViewLineRenderer()
		{
			m_Pool = new ObjectPool<Mesh>(1, 8, MeshConstructor, null);
			mat = new Material(Shader.Find("ProBuilder/UnlitVertexColor"));
			mat.name = "pb_LineRenderer_Material";
			mat.SetColor("_Color", Color.white);
			mat.hideFlags = PreferenceKeys.k_EditorHideFlags;
			SceneView.onSceneGUIDelegate += Render;
		}

		~SceneViewLineRenderer()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (m_IsDisposed)
				return;

			SceneView.onSceneGUIDelegate -= Render;
			m_IsDisposed = true;
			m_Pool.Empty();

			foreach(Mesh m in gizmos)
			{
				if(m != null)
					Object.DestroyImmediate(m);
			}

			Object.DestroyImmediate(mat);
		}

		public void AddLineSegments(Vector3[] segments, Color[] colors)
		{
			// Because editor scripts are enabled first, pool could still be null
			// when first request comes in.
			if(m_Pool == null)
				m_Pool = new ObjectPool<Mesh>(1, 4, MeshConstructor, null);

			Mesh m = m_Pool.Get();

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

			m.hideFlags = PreferenceKeys.k_EditorHideFlags;

			gizmos.Add(m);
		}

		/// <summary>
		/// Clear the queue of line segments to render.
		/// </summary>
		public void Clear()
		{
			for(int i = 0; i < gizmos.Count; i++)
				m_Pool.Put(gizmos[i]);

			gizmos.Clear();
		}

		public void Render(SceneView view)
		{
			mat.SetPass(0);

			for(int i = 0; i < gizmos.Count && gizmos[i] != null; i++)
				Graphics.DrawMeshNow(gizmos[i], Vector3.zero, Quaternion.identity, 0);
		}

	}
}

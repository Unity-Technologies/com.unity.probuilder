using System;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.ProBuilder;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

namespace UnityEditor.ProBuilder
{
	class EditorMeshHandles : IDisposable, IHasPreferences
	{
		const HideFlags k_MeshHideFlags = (HideFlags) (1 | 2 | 4 | 8);

		bool m_IsDisposed;
		ObjectPool<Mesh> m_MeshPool;

		Dictionary<ProBuilderMesh, MeshHandle> m_WireHandles;
		Dictionary<ProBuilderMesh, MeshHandle> m_VertexHandles;

		Dictionary<ProBuilderMesh, MeshHandle> m_FaceHandles;
		Dictionary<ProBuilderMesh, MeshHandle> m_VertHandles;
		Dictionary<ProBuilderMesh, MeshHandle> m_EdgeHandles;

		static readonly Color k_VertexUnselectedDefault = new Color(.7f, .7f, .7f, 1f);
		static readonly Color k_WireframeDefault = new Color(94.0f / 255.0f, 119.0f / 255.0f, 155.0f / 255.0f, 1f);

		static Color s_FaceSelectedColor;
		static Color s_WireframeColor;
		static Color s_PreselectionColor;
		static Color s_EdgeSelectedColor;
		static Color s_EdgeUnselectedColor;
		static Color s_VertexSelectedColor;
		static Color s_VertexUnselectedColor;

		static bool s_EnableFaceDither;
		static float s_WireframeSize;
		static float s_EdgeSize;

		Material m_EdgeMaterial;
		Material m_FaceMaterial;
		Material m_VertMaterial;
		Material m_WireMaterial;

		public static Color faceSelectedColor
		{
			get { return s_FaceSelectedColor; }
		}

		public static Color wireframeColor
		{
			get { return s_WireframeColor; }
		}

		public static Color preselectionColor
		{
			get { return s_PreselectionColor; }
		}

		public static Color edgeSelectedColor
		{
			get { return s_EdgeSelectedColor; }
		}

		public static Color edgeUnselectedColor
		{
			get { return s_EdgeUnselectedColor; }
		}

		public static Color vertexSelectedColor
		{
			get { return s_VertexSelectedColor; }
		}

		public static Color vertexUnselectedColor
		{
			get { return s_VertexUnselectedColor; }
		}

		public EditorMeshHandles()
		{
			m_MeshPool = new ObjectPool<Mesh>( 0, 8, CreateMesh, DestroyMesh);
			m_WireHandles = new Dictionary<ProBuilderMesh, MeshHandle>();
			m_VertexHandles = new Dictionary<ProBuilderMesh, MeshHandle>();
			m_FaceHandles = new Dictionary<ProBuilderMesh, MeshHandle>();
			m_EdgeHandles = new Dictionary<ProBuilderMesh, MeshHandle>();
			m_VertHandles = new Dictionary<ProBuilderMesh, MeshHandle>();

			var lineShader = BuiltinMaterials.geometryShadersSupported ? BuiltinMaterials.lineShader : BuiltinMaterials.wireShader;
			var vertShader = BuiltinMaterials.geometryShadersSupported ? BuiltinMaterials.pointShader : BuiltinMaterials.dotShader;

			m_EdgeMaterial = CreateMaterial(Shader.Find(lineShader), "ProBuilder::LineMaterial");
			m_WireMaterial = CreateMaterial(Shader.Find(lineShader), "ProBuilder::WireMaterial");
			m_VertMaterial = CreateMaterial(Shader.Find(vertShader), "ProBuilder::VertexMaterial");
			m_FaceMaterial = CreateMaterial(Shader.Find(BuiltinMaterials.faceShader), "ProBuilder::FaceMaterial");

			ReloadPreferences();
		}

		public void Dispose()
		{
			if (m_IsDisposed)
				return;

			m_IsDisposed = true;

			ClearHandles();

			m_MeshPool.Dispose();

			UObject.DestroyImmediate(m_EdgeMaterial);
			UObject.DestroyImmediate(m_WireMaterial);
			UObject.DestroyImmediate(m_VertMaterial);
			UObject.DestroyImmediate(m_FaceMaterial);
		}

		public void ReloadPreferences()
		{
			s_WireframeSize = PreferencesInternal.GetFloat(PreferenceKeys.pbWireframeSize) * EditorGUIUtility.pixelsPerPoint;
			s_WireframeColor = PreferencesInternal.GetColor(PreferenceKeys.pbWireframeColor);
			s_EdgeSize = PreferencesInternal.GetFloat(PreferenceKeys.pbLineHandleSize) * EditorGUIUtility.pixelsPerPoint;

			if (PreferencesInternal.GetBool(PreferenceKeys.pbUseUnityColors))
			{
				s_FaceSelectedColor = Handles.selectedColor;
				s_EnableFaceDither = true;

				s_EdgeSelectedColor = Handles.selectedColor;
				s_EdgeUnselectedColor = k_WireframeDefault;

				s_VertexSelectedColor = Handles.selectedColor;
				s_VertexUnselectedColor = k_VertexUnselectedDefault;

				s_PreselectionColor = Handles.preselectionColor;
			}
			else
			{
				s_FaceSelectedColor = PreferencesInternal.GetColor(PreferenceKeys.pbSelectedFaceColor);
				s_EnableFaceDither = PreferencesInternal.GetBool(PreferenceKeys.pbSelectedFaceDither);

				s_EdgeSelectedColor = PreferencesInternal.GetColor(PreferenceKeys.pbSelectedEdgeColor);
				s_EdgeUnselectedColor = PreferencesInternal.GetColor(PreferenceKeys.pbUnselectedEdgeColor);

				s_VertexSelectedColor = PreferencesInternal.GetColor(PreferenceKeys.pbSelectedVertexColor);
				s_VertexUnselectedColor = PreferencesInternal.GetColor(PreferenceKeys.pbUnselectedVertexColor);

				s_PreselectionColor = PreferencesInternal.GetColor(PreferenceKeys.pbPreselectionColor);
			}

			m_WireMaterial.SetColor("_Color", s_WireframeColor);
			m_FaceMaterial.SetFloat("_Dither", s_EnableFaceDither ? 1f : 0f);

			m_VertMaterial.SetFloat("_Scale", PreferencesInternal.GetFloat(PreferenceKeys.pbVertexHandleSize) * EditorGUIUtility.pixelsPerPoint);

			if (BuiltinMaterials.geometryShadersSupported)
			{
				m_WireMaterial.SetFloat("_Scale", s_WireframeSize);
				m_EdgeMaterial.SetFloat("_Scale", s_EdgeSize);
			}
		}

		static Material CreateMaterial(Shader shader, string materialName)
		{
			Material mat = new Material(shader);
			mat.name = materialName;
			mat.hideFlags = k_MeshHideFlags;
			return mat;
		}

		Mesh CreateMesh()
		{
			var mesh = new Mesh();
			mesh.name = "EditorMeshHandles.MeshHandle" + mesh.GetInstanceID();
			mesh.hideFlags = HideFlags.HideAndDontSave;
			return mesh;
		}

		void DestroyMesh(Mesh mesh)
		{
			if (mesh == null)
				throw new ArgumentNullException("mesh");

			UObject.DestroyImmediate(mesh);
		}

		static MethodInfo s_ApplyWireMaterial = null;

		static object[] s_ApplyWireMaterialArgs = new object[]
		{
			CompareFunction.Always
		};

		internal bool BeginDrawingLines(CompareFunction zTest)
		{
			if (Event.current.type != EventType.Repaint)
				return false;

			if (!BuiltinMaterials.geometryShadersSupported ||
				!m_EdgeMaterial.SetPass(0))
			{
				if (s_ApplyWireMaterial == null)
				{
					s_ApplyWireMaterial = typeof(HandleUtility).GetMethod(
						"ApplyWireMaterial",
						BindingFlags.Static | BindingFlags.NonPublic,
						null,
						new System.Type[] { typeof(CompareFunction) },
						null);

					if (s_ApplyWireMaterial == null)
					{
						Log.Info("Failed to find wire material, stopping draw lines.");
						return false;
					}
				}

				s_ApplyWireMaterialArgs[0] = zTest;
				s_ApplyWireMaterial.Invoke(null, s_ApplyWireMaterialArgs);
			}

			GL.PushMatrix();
			GL.Begin(GL.LINES);

			return true;
		}

		internal void EndDrawingLines()
		{
			GL.End();
			GL.PopMatrix();
		}

		internal void DrawSceneSelection(SceneSelection selection)
		{
			var mesh = selection.mesh;

			if (mesh == null)
				return;

			var positions = mesh.positionsInternal;

			// Draw nearest edge
			if (selection.face != null)
			{
				m_FaceMaterial.SetColor("_Color", preselectionColor);

				if (!m_FaceMaterial.SetPass(0))
					return;

				GL.PushMatrix();
				GL.Begin(GL.TRIANGLES);
				GL.MultMatrix(mesh.transform.localToWorldMatrix);

				var face = selection.face;
				var ind = face.indexes;

				for (int i = 0, c = ind.Count; i < c; i += 3)
				{
					GL.Vertex(positions[ind[i]]);
					GL.Vertex(positions[ind[i+1]]);
					GL.Vertex(positions[ind[i+2]]);
				}

				GL.End();
				GL.PopMatrix();
			}
			else if (selection.edge != Edge.Empty)
			{
				m_EdgeMaterial.SetColor("_Color", preselectionColor);

				if (BeginDrawingLines(Handles.zTest))
				{
					GL.MultMatrix(mesh.transform.localToWorldMatrix);
					GL.Vertex(positions[selection.edge.a]);
					GL.Vertex(positions[selection.edge.b]);
					EndDrawingLines();
				}
			}
			else if (selection.vertex > -1)
			{
				var size = PreferencesInternal.GetFloat(PreferenceKeys.pbVertexHandleSize) * .0125f;

				using (new Handles.DrawingScope(preselectionColor, mesh.transform.localToWorldMatrix))
				{
					var pos = positions[selection.vertex];
					Handles.DotHandleCap(-1, pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * size, Event.current.type);
				}
			}
		}

		public void DrawSceneHandles(SelectMode mode)
		{
			if (Event.current.type != EventType.Repaint)
				return;

			switch (mode)
			{
				case SelectMode.Edge:
				{
					// render wireframe with edge material in edge mode so that the size change is reflected
					RenderWithColor(m_WireHandles, m_EdgeMaterial, s_EdgeUnselectedColor);
					RenderWithColor(m_EdgeHandles, m_EdgeMaterial, s_EdgeSelectedColor);
					break;
				}
				case SelectMode.Face:
				{
					RenderWithColor(m_WireHandles, m_WireMaterial, s_WireframeColor);
					RenderWithColor(m_FaceHandles, m_FaceMaterial, s_FaceSelectedColor);
					break;
				}
				case SelectMode.Vertex:
				{
					RenderWithColor(m_WireHandles, m_WireMaterial, s_WireframeColor);
					RenderWithColor(m_VertexHandles, m_VertMaterial, s_VertexUnselectedColor);
					RenderWithColor(m_VertHandles, m_VertMaterial, s_VertexSelectedColor);
					break;
				}
				default:
				{
					RenderWithColor(m_WireHandles, m_WireMaterial, s_WireframeColor);
					break;
				}
			}
		}

		static void RenderWithColor(Dictionary<ProBuilderMesh, MeshHandle> handles, Material material, Color color)
		{
			material.SetColor("_Color", color);

			if (material.SetPass(0))
			{
				foreach (var kvp in handles)
					kvp.Value.DrawMeshNow(0);
			}
		}

		public void ClearHandles()
		{
			ClearHandlesInternal(m_WireHandles);
			ClearHandlesInternal(m_VertexHandles);
			ClearHandlesInternal(m_FaceHandles);
			ClearHandlesInternal(m_EdgeHandles);
			ClearHandlesInternal(m_VertHandles);
		}

		static List<int> s_VertexList = new List<int>();

		public void RebuildSelectedHandles(IEnumerable<ProBuilderMesh> meshes, ComponentMode selectionMode)
		{
			ClearHandles();

			foreach (var mesh in meshes)
			{
				// always do wireframe
				RebuildMeshHandle(mesh, m_WireHandles, MeshHandles.CreateEdgeMesh);

				switch (selectionMode)
				{
					case ComponentMode.Vertex:
					{
						RebuildMeshHandle(mesh, m_VertexHandles, (x,y) =>
						{
							s_VertexList.Clear();
							for (int i = 0, c = mesh.sharedVerticesInternal.Length; i < c; i++)
								s_VertexList.Add(mesh.sharedVerticesInternal[i][0]);
							MeshHandles.CreateVertexMesh(x, y, s_VertexList);
						});

						RebuildMeshHandle(mesh, m_VertHandles, (x,y) =>
						{
							MeshHandles.CreateVertexMesh(x, y, x.selectedIndexesInternal);
						});
						break;
					}

					case ComponentMode.Edge:
					{
						RebuildMeshHandle(mesh, m_EdgeHandles, (x, y) =>
						{
							MeshHandles.CreateEdgeMesh(x, y, x.selectedEdgesInternal);
						});
						break;
					}

					case ComponentMode.Face:
					{
						RebuildMeshHandle(mesh, m_FaceHandles, MeshHandles.CreateFaceMesh);
						break;
					}
				}
			}
		}

		void RebuildMeshHandle(ProBuilderMesh mesh, Dictionary<ProBuilderMesh, MeshHandle> list, Action<ProBuilderMesh, Mesh> ctor)
		{
			MeshHandle handle;

			if (!list.TryGetValue(mesh, out handle))
			{
				var m = m_MeshPool.Get();
				handle = new MeshHandle(mesh.transform, m);
				list.Add(mesh, handle);
			}

			ctor(mesh, handle.mesh);
		}

		void ClearHandlesInternal(Dictionary<ProBuilderMesh, MeshHandle> handles)
		{
			foreach(var kvp in handles)
				m_MeshPool.Put(kvp.Value.mesh);
			handles.Clear();
		}
	}
}

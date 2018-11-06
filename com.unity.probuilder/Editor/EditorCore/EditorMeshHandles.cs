using System;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.ProBuilder;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEditor.SettingsManagement;

namespace UnityEditor.ProBuilder
{
	partial class EditorMeshHandles : IDisposable, IHasPreferences
	{
		const HideFlags k_MeshHideFlags = (HideFlags) (1 | 2 | 4 | 8);

		bool m_IsDisposed;
		ObjectPool<Mesh> m_MeshPool;

		Dictionary<ProBuilderMesh, MeshHandle> m_WireHandles;
		Dictionary<ProBuilderMesh, MeshHandle> m_VertexHandles;

		Dictionary<ProBuilderMesh, MeshHandle> m_SelectedFaceHandles;
		Dictionary<ProBuilderMesh, MeshHandle> m_SelectedVertexHandles;
		Dictionary<ProBuilderMesh, MeshHandle> m_SelectedEdgeHandles;

		static readonly Color k_VertexUnselectedDefault = new Color(.7f, .7f, .7f, 1f);
		static readonly Color k_WireframeDefault = new Color(94.0f / 255.0f, 119.0f / 255.0f, 155.0f / 255.0f, 1f);

		[UserSetting]
		static Pref<bool> s_UseUnityColors = new Pref<bool>("graphics.handlesUseUnityColors", true, SettingsScope.User);
		[UserSetting]
		static Pref<bool> s_DitherFaceHandle = new Pref<bool>("graphics.ditherFaceHandles", true, SettingsScope.User);
		[UserSetting]
		static Pref<Color> s_SelectedFaceColorPref = new Pref<Color>("graphics.userSelectedFaceColor", new Color(0f, 210f / 255f, 239f / 255f, 1f), SettingsScope.User);
		[UserSetting]
		static Pref<Color> s_WireframeColorPref = new Pref<Color>("graphics.userWireframeColor", new Color(125f / 255f, 155f / 255f, 185f / 255f, 1f), SettingsScope.User);
		[UserSetting]
		static Pref<Color> s_UnselectedEdgeColorPref = new Pref<Color>("graphics.userUnselectedEdgeColor", new Color(44f / 255f, 44f / 255f, 44f / 255f, 1f), SettingsScope.User);
		[UserSetting]
		static Pref<Color> s_SelectedEdgeColorPref = new Pref<Color>("graphics.userSelectedEdgeColor", new Color(0f, 210f / 255f, 239f / 255f, 1f), SettingsScope.User);
		[UserSetting]
		static Pref<Color> s_UnselectedVertexColorPref = new Pref<Color>("graphics.userUnselectedVertexColor", new Color(44f / 255f, 44f / 255f, 44f / 255f, 1f), SettingsScope.User);
		[UserSetting]
		static Pref<Color> s_SelectedVertexColorPref = new Pref<Color>("graphics.userSelectedVertexColor", new Color(0f, 210f / 255f, 239f / 255f, 1f), SettingsScope.User);
		[UserSetting]
		static Pref<Color> s_PreselectionColorPref = new Pref<Color>("graphics.userPreselectionColor", new Color(179f / 255f, 246f / 255f, 255f / 255f, 1f), SettingsScope.User);

		[UserSetting]
		static Pref<float> s_WireframeLineSize = new Pref<float>("graphics.wireframeLineSize", .5f, SettingsScope.User);
		[UserSetting]
		static Pref<float> s_EdgeLineSize = new Pref<float>("graphics.edgeLineSize", 1f, SettingsScope.User);
		[UserSetting]
		static Pref<float> s_VertexPointSize = new Pref<float>("graphics.vertexPointSize", 3f, SettingsScope.User);

		[UserSetting("Graphics", "Handle Z Test", "The compare function to use when drawing mesh handles.")]
		static Pref<CompareFunction> s_HandleCompareFunction = new Pref<CompareFunction>("graphics.handleZTest", CompareFunction.Always, SettingsScope.User);

		[UserSettingBlock("Graphics")]
		static void HandleColorPreferences(string searchContext)
		{
			s_UseUnityColors.value = SettingsGUILayout.SettingsToggle("Use Unity Colors", s_UseUnityColors, searchContext);

			if (!s_UseUnityColors.value)
			{
				using (new SettingsGUILayout.IndentedGroup())
				{
					s_DitherFaceHandle.value = SettingsGUILayout.SettingsToggle("Dither Face Overlay", s_DitherFaceHandle, searchContext);
					s_WireframeColorPref.value = SettingsGUILayout.SettingsColorField("Wireframe", s_WireframeColorPref, searchContext);
					s_PreselectionColorPref.value = SettingsGUILayout.SettingsColorField("Preselection", s_PreselectionColorPref, searchContext);
					s_SelectedFaceColorPref.value = SettingsGUILayout.SettingsColorField("Selected Face Color", s_SelectedFaceColorPref, searchContext);
					s_UnselectedEdgeColorPref.value = SettingsGUILayout.SettingsColorField("Unselected Edge Color", s_UnselectedEdgeColorPref, searchContext);
					s_SelectedEdgeColorPref.value = SettingsGUILayout.SettingsColorField("Selected Edge Color", s_SelectedEdgeColorPref, searchContext);
					s_UnselectedVertexColorPref.value = SettingsGUILayout.SettingsColorField("Unselected Vertex Color", s_UnselectedVertexColorPref, searchContext);
					s_SelectedVertexColorPref.value = SettingsGUILayout.SettingsColorField("Selected Vertex Color", s_SelectedVertexColorPref, searchContext);
				}
			}

			s_VertexPointSize.value = SettingsGUILayout.SettingsSlider("Vertex Size", s_VertexPointSize, 1f, 10f, searchContext);

			bool geoLine = BuiltinMaterials.geometryShadersSupported;

			if (geoLine)
			{
				s_EdgeLineSize.value = SettingsGUILayout.SettingsSlider("Line Size", s_EdgeLineSize, 0f, 3f, searchContext);
				s_WireframeLineSize.value = SettingsGUILayout.SettingsSlider("Wireframe Size", s_WireframeLineSize, 0f, 3f, searchContext);
			}
			else
			{
				GUI.enabled = false;
				SettingsGUILayout.SearchableSlider("Line Size", 0f, 0f, 3f, searchContext);
				SettingsGUILayout.SearchableSlider("Wireframe Size", 0f, 0f, 3f, searchContext);
				GUI.enabled = true;
			}

		}

		static Color s_FaceSelectedColor;
		static Color s_WireframeColor;
		static Color s_PreselectionColor;
		static Color s_EdgeSelectedColor;
		static Color s_EdgeUnselectedColor;
		static Color s_VertexSelectedColor;
		static Color s_VertexUnselectedColor;

		Material m_EdgeMaterial;
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
			Init();

			m_MeshPool = new ObjectPool<Mesh>( 0, 8, CreateMesh, DestroyMesh);
			m_WireHandles = new Dictionary<ProBuilderMesh, MeshHandle>();
			m_VertexHandles = new Dictionary<ProBuilderMesh, MeshHandle>();
			m_SelectedFaceHandles = new Dictionary<ProBuilderMesh, MeshHandle>();
			m_SelectedEdgeHandles = new Dictionary<ProBuilderMesh, MeshHandle>();
			m_SelectedVertexHandles = new Dictionary<ProBuilderMesh, MeshHandle>();

			var lineShader = BuiltinMaterials.geometryShadersSupported ? BuiltinMaterials.lineShader : BuiltinMaterials.wireShader;
			var vertShader = BuiltinMaterials.geometryShadersSupported ? BuiltinMaterials.pointShader : BuiltinMaterials.dotShader;

			m_EdgeMaterial = CreateMaterial(Shader.Find(lineShader), "ProBuilder::LineMaterial");
			m_WireMaterial = CreateMaterial(Shader.Find(lineShader), "ProBuilder::WireMaterial");
			m_VertMaterial = CreateMaterial(Shader.Find(vertShader), "ProBuilder::VertexMaterial");

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
		}

		public void ReloadPreferences()
		{
			if (s_UseUnityColors)
			{
				s_WireframeColor = k_WireframeDefault;

				s_FaceSelectedColor = Handles.selectedColor;

				s_EdgeSelectedColor = Handles.selectedColor;
				s_EdgeUnselectedColor = k_WireframeDefault;

				s_VertexSelectedColor = Handles.selectedColor;
				s_VertexUnselectedColor = k_VertexUnselectedDefault;

				s_PreselectionColor = Handles.preselectionColor;
			}
			else
			{
				s_WireframeColor = s_WireframeColorPref;

				s_FaceSelectedColor = s_SelectedFaceColorPref;
				s_PreselectionColor = s_PreselectionColorPref;

				s_EdgeSelectedColor = s_SelectedEdgeColorPref;
				s_EdgeUnselectedColor = s_UnselectedEdgeColorPref;

				s_VertexSelectedColor = s_SelectedVertexColorPref;
				s_VertexUnselectedColor = s_UnselectedVertexColorPref;
			}

			m_WireMaterial.SetColor("_Color", s_WireframeColor);
			s_FaceMaterial.SetFloat("_Dither", (s_UseUnityColors || s_DitherFaceHandle) ? 1f : 0f);

			m_VertMaterial.SetFloat("_Scale", s_VertexPointSize * EditorGUIUtility.pixelsPerPoint);

			m_WireMaterial.SetInt("_HandleZTest", (int) CompareFunction.LessEqual);
			m_EdgeMaterial.SetInt("_HandleZTest", (int) CompareFunction.LessEqual);

			if (BuiltinMaterials.geometryShadersSupported)
			{
				m_WireMaterial.SetFloat("_Scale", s_WireframeLineSize * EditorGUIUtility.pixelsPerPoint);
				m_EdgeMaterial.SetFloat("_Scale", s_EdgeLineSize * EditorGUIUtility.pixelsPerPoint);
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

		internal void DrawSceneSelection(SceneSelection selection)
		{
			var mesh = selection.mesh;

			if (mesh == null)
				return;

			var positions = mesh.positionsInternal;

			// Draw nearest edge
			if (selection.face != null)
			{
				using (new TriangleDrawingScope(preselectionColor, s_HandleCompareFunction))
				{
					GL.MultMatrix(mesh.transform.localToWorldMatrix);

					var face = selection.face;
					var ind = face.indexes;

					for (int i = 0, c = ind.Count; i < c; i += 3)
					{
						GL.Vertex(positions[ind[i]]);
						GL.Vertex(positions[ind[i+1]]);
						GL.Vertex(positions[ind[i+2]]);
					}
				}
			}
			else if (selection.edge != Edge.Empty)
			{
				using (new LineDrawingScope(preselectionColor, -1f, s_HandleCompareFunction))
				{
					GL.MultMatrix(mesh.transform.localToWorldMatrix);
					GL.Vertex(positions[selection.edge.a]);
					GL.Vertex(positions[selection.edge.b]);
				}
			}
			else if (selection.vertex > -1)
			{
				var size = s_VertexPointSize * .0125f;

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
				case SelectMode.TextureEdge:
				{
					// render wireframe with edge material in edge mode so that the size change is reflected
					RenderWithColor(m_WireHandles, m_EdgeMaterial, s_EdgeUnselectedColor);
					RenderWithColor(m_SelectedEdgeHandles, m_EdgeMaterial, s_EdgeSelectedColor);
					break;
				}
				case SelectMode.Face:
				case SelectMode.TextureFace:
				{
					RenderWithColor(m_WireHandles, m_WireMaterial, s_WireframeColor);
					RenderWithColor(m_SelectedFaceHandles, s_FaceMaterial, s_FaceSelectedColor);
					break;
				}
				case SelectMode.Vertex:
				case SelectMode.TextureVertex:
				{
					RenderWithColor(m_WireHandles, m_WireMaterial, s_WireframeColor);
					RenderWithColor(m_VertexHandles, m_VertMaterial, s_VertexUnselectedColor);
					RenderWithColor(m_SelectedVertexHandles, m_VertMaterial, s_VertexSelectedColor);
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
			ClearHandlesInternal(m_SelectedFaceHandles);
			ClearHandlesInternal(m_SelectedEdgeHandles);
			ClearHandlesInternal(m_SelectedVertexHandles);
		}

		public void RebuildSelectedHandles(IEnumerable<ProBuilderMesh> meshes, SelectMode selectionMode, bool selectionOrVertexCountChanged = true)
		{
			if (!selectionOrVertexCountChanged)
			{
				foreach (var handle in m_WireHandles)
					handle.Value.mesh.vertices = handle.Key.positionsInternal;

				switch (selectionMode)
				{
					case SelectMode.Vertex:
					case SelectMode.TextureVertex:
					{
						foreach (var handle in m_VertexHandles)
							handle.Value.mesh.vertices = handle.Key.positionsInternal;
						foreach (var handle in m_SelectedVertexHandles)
							handle.Value.mesh.vertices = handle.Key.positionsInternal;
						break;
					}

					case SelectMode.Edge:
					case SelectMode.TextureEdge:
					{
						foreach (var handle in m_SelectedEdgeHandles)
							handle.Value.mesh.vertices = handle.Key.positionsInternal;
						break;
					}

					case SelectMode.Face:
					case SelectMode.TextureFace:
					{
						foreach (var handle in m_SelectedFaceHandles)
							handle.Value.mesh.vertices = handle.Key.positionsInternal;
						break;
					}
				}

				return;
			}

			ClearHandles();

			foreach (var mesh in meshes)
			{
				// always do wireframe
				RebuildMeshHandle(mesh, m_WireHandles, MeshHandles.CreateEdgeMesh);

				switch (selectionMode)
				{
					case SelectMode.Vertex:
					case SelectMode.TextureVertex:
					{
						RebuildMeshHandle(mesh, m_VertexHandles, MeshHandles.CreateVertexMesh);

						RebuildMeshHandle(mesh, m_SelectedVertexHandles, (x, y) =>
						{
							MeshHandles.CreateVertexMesh(x, y, x.selectedIndexesInternal);
						});
						break;
					}

					case SelectMode.Edge:
					case SelectMode.TextureEdge:
					{
						RebuildMeshHandle(mesh, m_SelectedEdgeHandles, (x, y) =>
						{
							MeshHandles.CreateEdgeMesh(x, y, x.selectedEdgesInternal);
						});
						break;
					}

					case SelectMode.Face:
					case SelectMode.TextureFace:
					{
						RebuildMeshHandle(mesh, m_SelectedFaceHandles, MeshHandles.CreateFaceMesh);
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

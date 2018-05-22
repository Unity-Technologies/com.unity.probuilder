using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor;

namespace UnityEditor.ProBuilder
{
	static class MeshHandles
	{
		const string k_FaceShader = "Hidden/ProBuilder/FaceHighlight";

		// used when gpu doesn't support geometry shaders (metal, for example)
		const string k_EdgeShader = "Hidden/ProBuilder/FaceHighlight";

		// used when gpu doesn't support geometry shaders (metal, for example)
		const string k_VertexShader = "Hidden/ProBuilder/VertexShader";

		// geometry shader expands lines to billboards
		const string k_LineBillboardShader = "Hidden/ProBuilder/LineBillboard";

		// geometry shader expands points to billboards
		const string k_PointBillboardShader = "Hidden/ProBuilder/PointBillboard";

		static bool s_GeometryShadersSupported;
		static bool s_EnableFaceDither = false;

		static Material s_FaceMaterial;
		static Material s_VertexMaterial;
		static Material s_WireframeMaterial;
		static Material s_LineMaterial;

		static readonly Color k_VertexUnselectedDefault = new Color(.7f, .7f, .7f, 1f);
		static readonly Color k_WireframeDefault = new Color(94.0f / 255.0f, 119.0f / 255.0f, 155.0f / 255.0f, 1f);

		static Color s_FaceSelectedColor;
		static Color s_WireframeColor;
		static Color s_PreselectionColor;
		static Color s_EdgeSelectedColor;
		static Color s_EdgeUnselectedColor;
		static Color s_VertexSelectedColor;
		static Color s_VertexUnselectedColor;

		public static Color preselectionColor
		{
			get { return s_PreselectionColor; }
		}

		const HideFlags k_MeshHideFlags = (HideFlags) (1 | 2 | 4 | 8);

		static ObjectPool<Renderable> s_RenderablePool;
		static List<Renderable> s_ActiveRenderables;
		static List<Renderable> s_WireframeRenderables;

		static bool s_IsInitialized;
		static bool s_IsGuiInitialized;

		public static bool geometryShadersSupported
		{
			get { return s_GeometryShadersSupported; }
		}

		public static Material lineMaterial
		{
			get { return s_LineMaterial; }
		}

		public static Material vertexMaterial
		{
			get { return s_VertexMaterial; }
		}

		public static void Initialize()
		{
			if (!s_IsInitialized)
			{
				s_RenderablePool = new ObjectPool<Renderable>(0, 8, Renderable.CreateInstance, Renderable.DestroyInstance);
				s_ActiveRenderables = new List<Renderable>();
				s_WireframeRenderables = new List<Renderable>();

				float wireframeSize = PreferencesInternal.GetFloat(PreferenceKeys.pbWireframeSize);
				float edgeSize = PreferencesInternal.GetFloat(PreferenceKeys.pbLineHandleSize);

				s_LineMaterial = CreateMaterial(Shader.Find(edgeSize <= 0f ? k_EdgeShader : k_LineBillboardShader), "pb_ElementGraphics::LineMaterial");
				s_WireframeMaterial = CreateMaterial(Shader.Find(wireframeSize <= 0f ? k_EdgeShader : k_LineBillboardShader), "pb_ElementGraphics::WireMaterial");
				s_VertexMaterial = CreateMaterial(Shader.Find(k_PointBillboardShader), "pb_ElementGraphics::VertexBillboardMaterial");
				s_FaceMaterial = CreateMaterial(Shader.Find(k_FaceShader), "pb_ElementGraphics::FaceSelectionMaterial");

				s_GeometryShadersSupported = s_WireframeMaterial.shader.isSupported && s_VertexMaterial.shader.isSupported;

				if (!s_GeometryShadersSupported)
				{
					s_LineMaterial.shader = Shader.Find(k_EdgeShader);
					s_WireframeMaterial.shader = Shader.Find(k_EdgeShader);
					s_VertexMaterial.shader = Shader.Find(k_VertexShader);
				}

				s_IsGuiInitialized = false;
				s_IsInitialized = true;
			}
		}

		public static void Destroy()
		{
			ClearAllRenderables();
			s_RenderablePool.Empty();
			Object.DestroyImmediate(s_FaceMaterial);
			Object.DestroyImmediate(s_VertexMaterial);
			Object.DestroyImmediate(s_WireframeMaterial);
			Object.DestroyImmediate(s_LineMaterial);
			s_IsInitialized = false;
		}

		/// <summary>
		/// Reload colors for edge and face highlights from editor prefs.
		/// </summary>
		public static void InitializeStyles()
		{
			if (s_IsGuiInitialized)
				return;

			s_IsGuiInitialized = true;

			s_WireframeColor = PreferencesInternal.GetColor(PreferenceKeys.pbWireframeColor);

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

			s_WireframeMaterial.SetColor("_Color", s_WireframeColor);
			s_FaceMaterial.SetColor("_Color", s_FaceSelectedColor);
			s_FaceMaterial.SetFloat("_Dither", s_EnableFaceDither ? 1f : 0f);

			if (geometryShadersSupported)
			{
				s_WireframeMaterial.SetFloat("_Scale", PreferencesInternal.GetFloat(PreferenceKeys.pbWireframeSize) * EditorGUIUtility.pixelsPerPoint);
				s_LineMaterial.SetFloat("_Scale", PreferencesInternal.GetFloat(PreferenceKeys.pbLineHandleSize) * EditorGUIUtility.pixelsPerPoint);
			}

			s_VertexMaterial.SetFloat("_Scale", PreferencesInternal.GetFloat(PreferenceKeys.pbVertexHandleSize) * EditorGUIUtility.pixelsPerPoint);
		}

		public static void DoGUI(SelectMode selectionMode)
		{
			if (Event.current.type != EventType.Repaint)
				return;

			InitializeStyles();

			s_VertexMaterial.SetColor("_Color", s_VertexUnselectedColor);
			s_LineMaterial.SetColor("_Color", s_EdgeUnselectedColor);

			foreach (var r in s_WireframeRenderables)
				r.Render();

			// don't render overlays when drag and drop is active so that the user can see the material preview
			if (!SceneDragAndDropListener.IsDragging())
			{
				if (selectionMode == SelectMode.Vertex)
				{
					s_VertexMaterial.SetColor("_Color", s_VertexSelectedColor);
					foreach(var r in s_ActiveRenderables)
						r.Render();
				}
				else if (selectionMode == SelectMode.Face)
				{
					foreach(var r in s_ActiveRenderables)
						r.Render();
				}
				else if (selectionMode == SelectMode.Edge)
				{
					Handles.lighting = false;

					var selection = MeshSelection.Top();

					lineMaterial.SetColor("_Color", Color.white);

					for (int i = 0; i < selection.Length; i++)
					{
						if (EditorHandleUtility.BeginDrawingLines(Handles.zTest))
						{
							ProBuilderMesh pb = selection[i];
							Edge[] edges = pb.selectedEdges.ToArray();
							GL.Color(s_EdgeSelectedColor);

							GL.MultMatrix(pb.transform.localToWorldMatrix);

							for (int j = 0, c = selection[i].selectedEdgeCount; j < c; j++)
							{
								GL.Vertex(pb.positionsInternal[edges[j].x]);
								GL.Vertex(pb.positionsInternal[edges[j].y]);
							}

							EditorHandleUtility.EndDrawingLines();
						}
						else
						{
							break;
						}
					}

					Handles.lighting = true;
				}
			}
		}

		static void ClearAllRenderables()
		{
			foreach (var ren in s_ActiveRenderables)
				s_RenderablePool.Put(ren);

			foreach (var ren in s_WireframeRenderables)
				s_RenderablePool.Put(ren);

			s_ActiveRenderables.Clear();
			s_WireframeRenderables.Clear();
		}

		static Material CreateMaterial(Shader shader, string materialName)
		{
			Material mat = new Material(shader);
			mat.name = materialName;
			mat.hideFlags = k_MeshHideFlags;
			return mat;
		}

		/// <summary>
		/// Update the highlight and wireframe graphics.
		/// </summary>
		/// <param name="selection"></param>
		/// <param name="commonIndicesLookup"></param>
		/// <param name="editLevel"></param>
		/// <param name="selectionMode"></param>
		public static void RebuildGraphics(ProBuilderMesh[] selection, Dictionary<int, int>[] commonIndicesLookup, EditLevel editLevel, SelectMode selectionMode)
		{
			// in the event that the editor starts calling UpdateGraphics before the object has run OnEnable() (which happens on script reloads)
			if(s_RenderablePool == null)
				return;

			// clear the current renderables
			ClearAllRenderables();

			for (int i = 0; i < selection.Length; i++)
				s_WireframeRenderables.Add(BuildEdgeMesh(selection[i], (editLevel == EditLevel.Geometry && selectionMode == SelectMode.Edge) ? s_LineMaterial : s_WireframeMaterial));

			if(editLevel == EditLevel.Geometry)
			{
				// update vert / edge / face
				switch(selectionMode)
				{
					case SelectMode.Face:
						foreach(ProBuilderMesh pb in selection)
							s_ActiveRenderables.Add(BuildFaceMesh(pb));
						break;

					case SelectMode.Vertex:
					{
						if (geometryShadersSupported)
						{
							for (int i = 0, c = selection.Length; i < c; i++)
								s_WireframeRenderables.Add(BuildVertexPoints(selection[i]));
						}

						for (int i = 0, c = selection.Length; i < c; i++)
							s_ActiveRenderables.Add(geometryShadersSupported
								? BuildVertexPoints(selection[i], selection[i].selectedIndicesInternal)
								: BuildVertexMesh(selection[i], commonIndicesLookup[i]));
						break;
					}

					default:
						break;
				}
			}
		}

		/// <summary>
		/// Populate a renderable's mesh with a face highlight mesh matching the selected triangles array.
		/// </summary>
		/// <param name="pb"></param>
		/// <returns></returns>
		static Renderable BuildFaceMesh(ProBuilderMesh pb)
		{
			Renderable ren = s_RenderablePool.Get();

			ren.name = "pb_ElementGraphics::FacesRenderable";
			ren.transform = pb.transform;
			ren.material = s_FaceMaterial;
			ren.mesh.Clear();
			ren.mesh.vertices = pb.positionsInternal;
			ren.mesh.triangles = pb.selectedFacesInternal.SelectMany(x => x.ToTriangles()).ToArray();

			return ren;
		}

		/// <summary>
		/// Populate a rendereble's mesh with a spattering of vertices representing both selected and not selected.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="lookup"></param>
		/// <returns></returns>
		static Renderable BuildVertexMesh(ProBuilderMesh pb, Dictionary<int, int> lookup)
		{
			ushort maxBillboardCount = ushort.MaxValue / 4;

			int billboardCount = pb.sharedIndicesInternal.Length;

			if(billboardCount > maxBillboardCount)
				billboardCount = maxBillboardCount;

			Vector3[] v = new Vector3[pb.sharedIndicesInternal.Length];
			HashSet<int> selected = new HashSet<int>(IntArrayUtility.GetCommonIndices(lookup, pb.selectedIndicesInternal));

			for(int i = 0; i < billboardCount; i++)
				v[i] = pb.positionsInternal[pb.sharedIndicesInternal[i][0]];

			Vector3[] 	t_billboards 		= new Vector3[billboardCount*4];
			Vector3[] 	t_nrm 				= new Vector3[billboardCount*4];
			Vector2[] 	t_uvs 				= new Vector2[billboardCount*4];
			Vector2[] 	t_uv2 				= new Vector2[billboardCount*4];
			Color[]   	t_col 				= new Color[billboardCount*4];
			int[] 		t_tris 				= new int[billboardCount*6];

			int n = 0;
			int t = 0;

			Vector3 up = Vector3.up;
			Vector3 right = Vector3.right;

			for(int i = 0; i < billboardCount; i++)
			{
				t_billboards[t+0] = v[i];
				t_billboards[t+1] = v[i];
				t_billboards[t+2] = v[i];
				t_billboards[t+3] = v[i];

				t_uvs[t+0] = Vector3.zero;
				t_uvs[t+1] = Vector3.right;
				t_uvs[t+2] = Vector3.up;
				t_uvs[t+3] = Vector3.one;

				t_uv2[t+0] = -up-right;
				t_uv2[t+1] = -up+right;
				t_uv2[t+2] =  up-right;
				t_uv2[t+3] =  up+right;

				t_nrm[t+0] = Vector3.forward;
				t_nrm[t+1] = Vector3.forward;
				t_nrm[t+2] = Vector3.forward;
				t_nrm[t+3] = Vector3.forward;

				t_tris[n+0] = t + 0;
				t_tris[n+1] = t + 1;
				t_tris[n+2] = t + 2;
				t_tris[n+3] = t + 1;
				t_tris[n+4] = t + 3;
				t_tris[n+5] = t + 2;

				if( selected.Contains(i) )
				{
					t_col[t+0] = s_VertexSelectedColor;
					t_col[t+1] = s_VertexSelectedColor;
					t_col[t+2] = s_VertexSelectedColor;
					t_col[t+3] = s_VertexSelectedColor;

					// t_nrm[t].x = .1f;
					// t_nrm[t+1].x = .1f;
					// t_nrm[t+2].x = .1f;
					// t_nrm[t+3].x = .1f;
				}
				else
				{
					t_col[t+0] = s_VertexUnselectedColor;
					t_col[t+1] = s_VertexUnselectedColor;
					t_col[t+2] = s_VertexUnselectedColor;
					t_col[t+3] = s_VertexUnselectedColor;
				}

				t+=4;
				n+=6;
			}

			Renderable ren = s_RenderablePool.Get();

			ren.name = "pb_ElementGraphics::VertexRenderable";
			ren.transform = pb.transform;
			ren.material = s_VertexMaterial;
			ren.mesh.Clear();
			ren.mesh.vertices = t_billboards;
			ren.mesh.normals = t_nrm;
			ren.mesh.uv = t_uvs;
			ren.mesh.uv2 = t_uv2;
			ren.mesh.colors = t_col;
			ren.mesh.triangles = t_tris;

			return ren;
		}

		static Renderable BuildEdgeMesh(ProBuilderMesh pb, Material material)
		{
			int edgeCount = 0;
			int faceCount = pb.faceCount;

			for (int i = 0; i < faceCount; i++)
				edgeCount += pb.facesInternal[i].edgesInternal.Length;

			int elementCount = System.Math.Min(edgeCount, ushort.MaxValue / 2 - 1);
			int[] tris = new int[ elementCount * 2 ];

			int edgeIndex = 0;

			for(int i = 0; i < faceCount && edgeIndex < elementCount; i++)
			{
				for (int n = 0; n < pb.facesInternal[i].edgesInternal.Length && edgeIndex < elementCount; n++)
				{
					var edge = pb.facesInternal[i].edgesInternal[n];

					int positionIndex = edgeIndex * 2;

					tris[positionIndex + 0] = edge.x;
					tris[positionIndex + 1] = edge.y;

					edgeIndex++;
				}
			}

			Renderable ren = s_RenderablePool.Get();
			ren.material = material;
			ren.name = "pb_ElementGraphics::WireframeRenderable";
			ren.transform = pb.transform;
			ren.mesh.Clear();
			ren.mesh.name = "pb_ElementGraphics::WireframeMesh";
			ren.mesh.vertices = pb.positionsInternal;
			ren.mesh.subMeshCount = 1;
			ren.mesh.SetIndices(tris, MeshTopology.Lines, 0);

			return ren;
		}

		/// <summary>
		/// Draw a set of vertices.
		/// </summary>
		/// <param name="pb"></param>
		static Renderable BuildVertexPoints(ProBuilderMesh pb)
		{
			int[] indices = new int[pb.sharedIndicesInternal.Length];
			for (int i = 0; i < pb.sharedIndicesInternal.Length; i++)
				indices[i] = pb.sharedIndicesInternal[i][0];
			return BuildVertexPoints(pb, indices);
		}

		/// <summary>
		/// Draw a set of vertices.
		/// </summary>
		/// <param name="pb"></param>
		static Renderable BuildVertexPoints(ProBuilderMesh pb, int[] indices)
		{
			var renderable = s_RenderablePool.Get();
			renderable.material = s_VertexMaterial;
			renderable.transform = pb.transform;
			var mesh = renderable.mesh;
			mesh.Clear();
			mesh.name = "pb_ElementGraphics::PointMesh";
			mesh.vertices = pb.positionsInternal;
			mesh.subMeshCount = 1;
			mesh.SetIndices(indices, MeshTopology.Points, 0);
			return renderable;
		}
	}
}

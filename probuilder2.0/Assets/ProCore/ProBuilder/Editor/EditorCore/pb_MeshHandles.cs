using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;
using UnityEditor;

namespace ProBuilder.EditorCore
{
	public static class pb_MeshHandles
	{
		const string k_FaceShader = "Hidden/ProBuilder/FaceHighlight";

		// used when gpu doesn't support geometry shaders (metal, for example)
		const string k_EdgeShader = "Hidden/ProBuilder/FaceHighlight";

		// used when gpu doesn't support geometry shaders (metal, for example)
		const string k_VertexShader = "Hidden/ProBuilder/pb_VertexShader";

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

		static pb_ObjectPool<pb_Renderable> s_RenderablePool;
		static List<pb_Renderable> s_ActiveRenderables;
		static List<pb_Renderable> s_WireframeRenderables;

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
				s_RenderablePool = new pb_ObjectPool<pb_Renderable>(0, 8, pb_Renderable.CreateInstance, pb_Renderable.DestroyInstance);
				s_ActiveRenderables = new List<pb_Renderable>();
				s_WireframeRenderables = new List<pb_Renderable>();

				float wireframeSize = pb_PreferencesInternal.GetFloat(pb_Constant.pbWireframeSize);
				float edgeSize = pb_PreferencesInternal.GetFloat(pb_Constant.pbLineHandleSize);

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

			s_WireframeColor = pb_PreferencesInternal.GetColor(pb_Constant.pbWireframeColor);

			if (pb_PreferencesInternal.GetBool(pb_Constant.pbUseUnityColors))
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
				s_FaceSelectedColor = pb_PreferencesInternal.GetColor(pb_Constant.pbSelectedFaceColor);
				s_EnableFaceDither = pb_PreferencesInternal.GetBool(pb_Constant.pbSelectedFaceDither);

				s_EdgeSelectedColor = pb_PreferencesInternal.GetColor(pb_Constant.pbSelectedEdgeColor);
				s_EdgeUnselectedColor = pb_PreferencesInternal.GetColor(pb_Constant.pbUnselectedEdgeColor);

				s_VertexSelectedColor = pb_PreferencesInternal.GetColor(pb_Constant.pbSelectedVertexColor);
				s_VertexUnselectedColor = pb_PreferencesInternal.GetColor(pb_Constant.pbUnselectedVertexColor);

				s_PreselectionColor = pb_PreferencesInternal.GetColor(pb_Constant.pbPreselectionColor);
			}

			s_WireframeMaterial.SetColor("_Color", s_WireframeColor);
			s_FaceMaterial.SetColor("_Color", s_FaceSelectedColor);
			s_FaceMaterial.SetFloat("_Dither", s_EnableFaceDither ? 1f : 0f);

			if (geometryShadersSupported)
			{
				s_WireframeMaterial.SetFloat("_Scale", pb_PreferencesInternal.GetFloat(pb_Constant.pbWireframeSize) * EditorGUIUtility.pixelsPerPoint);
				s_LineMaterial.SetFloat("_Scale", pb_PreferencesInternal.GetFloat(pb_Constant.pbLineHandleSize) * EditorGUIUtility.pixelsPerPoint);
			}

			s_VertexMaterial.SetFloat("_Scale", pb_PreferencesInternal.GetFloat(pb_Constant.pbVertexHandleSize) * EditorGUIUtility.pixelsPerPoint);
		}

		public static void DoGUI(EditLevel editLevel, SelectMode selectionMode)
		{
			if (Event.current.type != EventType.Repaint)
				return;

			InitializeStyles();

			s_VertexMaterial.SetColor("_Color", s_VertexUnselectedColor);
			s_LineMaterial.SetColor("_Color", s_EdgeUnselectedColor);

			foreach (var r in s_WireframeRenderables)
				r.Render();

			// don't render overlays when drag and drop is active so that the user can see the material preview
			if (!pb_DragAndDropListener.IsDragging())
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

					var selection = pb_Selection.Top();

					lineMaterial.SetColor("_Color", Color.white);

					for (int i = 0; i < selection.Length; i++)
					{
						if (pb_EditorHandleUtility.BeginDrawingLines(Handles.zTest))
						{
							pb_Object pb = selection[i];
							pb_Edge[] edges = pb.SelectedEdges;
							GL.Color(s_EdgeSelectedColor);

							GL.MultMatrix(pb.transform.localToWorldMatrix);

							for (int j = 0, c = selection[i].SelectedEdgeCount; j < c; j++)
							{
								GL.Vertex(pb.vertices[edges[j].x]);
								GL.Vertex(pb.vertices[edges[j].y]);
							}

							pb_EditorHandleUtility.EndDrawingLines();
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
		public static void RebuildGraphics(pb_Object[] selection, Dictionary<int, int>[] commonIndicesLookup, EditLevel editLevel, SelectMode selectionMode)
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
						foreach(pb_Object pb in selection)
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
								? BuildVertexPoints(selection[i], selection[i].SelectedTriangles)
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
		static pb_Renderable BuildFaceMesh(pb_Object pb)
		{
			pb_Renderable ren = s_RenderablePool.Get();

			ren.name = "pb_ElementGraphics::FacesRenderable";
			ren.transform = pb.transform;
			ren.material = s_FaceMaterial;
			ren.mesh.Clear();
			ren.mesh.vertices = pb.vertices;
			ren.mesh.triangles = pb_Face.AllTriangles(pb.SelectedFaces);

			return ren;
		}

		/// <summary>
		/// Populate a rendereble's mesh with a spattering of vertices representing both selected and not selected.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="lookup"></param>
		/// <returns></returns>
		static pb_Renderable BuildVertexMesh(pb_Object pb, Dictionary<int, int> lookup)
		{
			ushort maxBillboardCount = ushort.MaxValue / 4;

			int billboardCount = pb.sharedIndices.Length;

			if(billboardCount > maxBillboardCount)
				billboardCount = maxBillboardCount;

			Vector3[] v = new Vector3[pb.sharedIndices.Length];
			HashSet<int> selected = new HashSet<int>(pb_IntArrayUtility.GetCommonIndices(lookup, pb.SelectedTriangles));

			for(int i = 0; i < billboardCount; i++)
				v[i] = pb.vertices[pb.sharedIndices[i][0]];

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

			pb_Renderable ren = s_RenderablePool.Get();

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

		static pb_Renderable BuildEdgeMesh(pb_Object pb, Material material)
		{
			int edgeCount = 0;
			int faceCount = pb.faceCount;

			for (int i = 0; i < faceCount; i++)
				edgeCount += pb.faces[i].edges.Length;

			int elementCount = System.Math.Min(edgeCount, ushort.MaxValue / 2 - 1);
			int[] tris = new int[ elementCount * 2 ];

			int edgeIndex = 0;

			for(int i = 0; i < faceCount && edgeIndex < elementCount; i++)
			{
				for (int n = 0; n < pb.faces[i].edges.Length && edgeIndex < elementCount; n++)
				{
					var edge = pb.faces[i].edges[n];

					int positionIndex = edgeIndex * 2;

					tris[positionIndex + 0] = edge.x;
					tris[positionIndex + 1] = edge.y;

					edgeIndex++;
				}
			}

			pb_Renderable ren = s_RenderablePool.Get();
			ren.material = material;
			ren.name = "pb_ElementGraphics::WireframeRenderable";
			ren.transform = pb.transform;
			ren.mesh.Clear();
			ren.mesh.name = "pb_ElementGraphics::WireframeMesh";
			ren.mesh.vertices = pb.vertices;
			ren.mesh.subMeshCount = 1;
			ren.mesh.SetIndices(tris, MeshTopology.Lines, 0);

			return ren;
		}

		/// <summary>
		/// Draw a set of vertices.
		/// </summary>
		/// <param name="pb"></param>
		static pb_Renderable BuildVertexPoints(pb_Object pb)
		{
			int[] indices = new int[pb.sharedIndices.Length];
			for (int i = 0; i < pb.sharedIndices.Length; i++)
				indices[i] = pb.sharedIndices[i][0];
			return BuildVertexPoints(pb, indices);
		}

		/// <summary>
		/// Draw a set of vertices.
		/// </summary>
		/// <param name="pb"></param>
		static pb_Renderable BuildVertexPoints(pb_Object pb, int[] indices)
		{
			var renderable = s_RenderablePool.Get();
			renderable.material = s_VertexMaterial;
			renderable.transform = pb.transform;
			var mesh = renderable.mesh;
			mesh.Clear();
			mesh.name = "pb_ElementGraphics::PointMesh";
			mesh.vertices = pb.vertices;
			mesh.subMeshCount = 1;
			mesh.SetIndices(indices, MeshTopology.Points, 0);
			return renderable;
		}
	}
}

using UnityEngine;
using System.Collections.Generic;
using System.Net.Configuration;
using ProBuilder.EditorCore;
using UnityEditor;

namespace ProBuilder.Core
{
	static class pb_ElementGraphics
	{
		const string k_FaceShader = "Hidden/ProBuilder/FaceHighlight";
		const string k_EdgeShader = "Hidden/ProBuilder/FaceHighlight";
		const string k_VertexShader = "Hidden/ProBuilder/pb_VertexShader";

		static float s_VertexHandleSize = .03f;
		static bool s_EnableFaceDither = false;
		const bool k_UseUnityColors = false;

		static Material s_FaceMaterial;
		static Material s_VertexMaterial;
		static Material s_WireframeMaterial;

		static Color s_FaceSelectedColor = new Color(0f, 1f, 1f, .275f);
		static Color s_WireframeColor = new Color(94.0f / 255.0f, 119.0f / 255.0f, 155.0f / 255.0f, 1f);
		static Color s_EdgeSelectedColor = new Color(0f, .6f, .7f, 1f);
		static Color s_EdgeUnselectedColor = new Color(0f, .6f, .7f, 1f);

		static Color s_VertexSelectedColor = new Color(1f, .2f, .2f, 1f);
		static Color s_VertexUnselectedColor = new Color(.8f, .8f, .8f, 1f);

		const HideFlags k_MeshHideFlags = (HideFlags) (1 | 2 | 4 | 8);
		static readonly Material[] k_WireframeMaterials = new Material[1];

		static pb_ObjectPool<pb_Renderable> s_RenderablePool;
		static List<pb_Renderable> s_ActiveRenderables;

		static bool s_IsInitialized;

		public static void Initialize()
		{
			if (!s_IsInitialized)
			{
				s_RenderablePool =
					new pb_ObjectPool<pb_Renderable>(0, 8, pb_Renderable.CreateInstance, pb_Renderable.DestroyInstance);
				s_ActiveRenderables = new List<pb_Renderable>();

				s_WireframeMaterial = CreateMaterial(Shader.Find(k_EdgeShader), "pb_ElementGraphics::WireframeMaterial");
				k_WireframeMaterials[0] = s_WireframeMaterial;

				s_FaceMaterial = CreateMaterial(Shader.Find(k_FaceShader), "pb_ElementGraphics::FaceSelectionMaterial");
				s_VertexMaterial = CreateMaterial(Shader.Find(k_VertexShader), "pb_ElementGraphics::VertexBillboardMaterial");

				s_IsInitialized = true;
			}

			LoadPrefs();
		}

		public static void Destroy()
		{
			s_RenderablePool.Empty();
			Object.DestroyImmediate(s_FaceMaterial);
			Object.DestroyImmediate(s_VertexMaterial);
			Object.DestroyImmediate(s_WireframeMaterial);
			s_IsInitialized = false;
		}

		/// <summary>
		/// Reload colors for edge and face highlights from editor prefs.
		/// </summary>
		public static void LoadPrefs()
		{
			s_WireframeColor = pb_PreferencesInternal.GetColor(pb_Constant.pbWireframeColor);
			s_VertexHandleSize = pb_PreferencesInternal.GetFloat(pb_Constant.pbVertexHandleSize);

			if (k_UseUnityColors)
			{
				s_FaceSelectedColor = Handles.selectedColor;
				s_EdgeSelectedColor = Handles.selectedColor;
				s_VertexSelectedColor = Handles.selectedColor;

				s_EdgeUnselectedColor = s_WireframeColor;
				s_VertexUnselectedColor = s_WireframeColor;

				s_EnableFaceDither = true;
			}
			else
			{
				s_FaceSelectedColor = pb_PreferencesInternal.GetColor(pb_Constant.pbSelectedFaceColor);
				s_EnableFaceDither = pb_PreferencesInternal.GetBool(pb_Constant.pbSelectedFaceDither);

				s_EdgeSelectedColor = pb_PreferencesInternal.GetColor(pb_Constant.pbSelectedEdgeColor);
				s_EdgeUnselectedColor = pb_PreferencesInternal.GetColor(pb_Constant.pbUnselectedEdgeColor);

				s_VertexSelectedColor = pb_PreferencesInternal.GetColor(pb_Constant.pbSelectedVertexColor);
				s_VertexUnselectedColor = pb_PreferencesInternal.GetColor(pb_Constant.pbUnselectedVertexColor);
			}

			s_WireframeMaterial.SetColor("_Color", s_WireframeColor);
			s_FaceMaterial.SetColor("_Color", s_FaceSelectedColor);
			s_FaceMaterial.SetFloat("_Dither", s_EnableFaceDither ? 1f : 0f);
			s_VertexMaterial.SetColor("_Color", s_VertexUnselectedColor);
			s_VertexMaterial.SetFloat("_Scale", s_VertexHandleSize * 4f);
		}

		static int Clamp(int val, int min, int max)
		{
			return val < min ? min :
				val > max ? max : val;
		}

		public static void DoGUI(EditLevel editLevel, SelectMode selectionMode)
		{
			if (Event.current.type != EventType.Repaint)
				return;

			foreach(pb_Renderable renderable in s_ActiveRenderables)
			{
				Material[] mats = renderable.materials;

				if( renderable.mesh == null )
					continue;

				for(int n = 0; n < renderable.mesh.subMeshCount; n++)
				{
					int materialIndex = Clamp(n, 0, mats.Length-1);

					if (mats[materialIndex] == null || !mats[materialIndex].SetPass(0) )
					{
						pb_Log.Debug("ProBuilder mesh handle material is null.");
						continue;
					}

					Graphics.DrawMeshNow(renderable.mesh, renderable.transform != null ? renderable.transform.localToWorldMatrix : Matrix4x4.identity, n);
				}
			}

			Handles.lighting = false;

			// Edge wireframe and selected faces are drawn in pb_ElementGraphics, selected edges & vertices are drawn here.
			if(selectionMode == SelectMode.Edge)
			{
				var selection = pb_Selection.Top();

				for (int i = 0; i < selection.Length; i++)
				{
					pb_EditorHandleUtility.BeginDrawingLines(Handles.zTest);
					pb_Object pb = selection[i];
					pb_Edge[] edges = pb.SelectedEdges;

					GL.MultMatrix(pb.transform.localToWorldMatrix);
					GL.Color(s_EdgeSelectedColor);

					for (int j = 0; j < selection[i].SelectedEdges.Length; j++)
					{
						GL.Vertex(pb.vertices[edges[j].x]);
						GL.Vertex(pb.vertices[edges[j].y]);
					}
					pb_EditorHandleUtility.EndDrawingLines();
				}
			}

			Handles.lighting = true;
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
			foreach(pb_Renderable ren in s_ActiveRenderables)
				s_RenderablePool.Put(ren);

			s_ActiveRenderables.Clear();

			// update wireframe
			s_WireframeMaterial.SetColor("_Color", (selectionMode == SelectMode.Edge && editLevel == EditLevel.Geometry) ? s_EdgeUnselectedColor : s_WireframeColor);

			for(int i = 0; i < selection.Length; i++)
				s_ActiveRenderables.Add(BuildEdgeMesh(selection[i]));

			if(editLevel == EditLevel.Geometry)
			{
				// update vert / edge / face
				switch(selectionMode)
				{
					case SelectMode.Face:
						foreach(pb_Object pb in selection)
							s_ActiveRenderables.Add( BuildFaceMesh(pb) );
						break;

					case SelectMode.Vertex:
						for(int i = 0, c = selection.Length; i < c; i++)
							s_ActiveRenderables.Add( BuildVertexMesh(selection[i], commonIndicesLookup[i]) );
						break;

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
			ren.materials = new Material[] { s_FaceMaterial };
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
			ren.materials = new Material[] { s_VertexMaterial };
			ren.mesh.Clear();
			ren.mesh.vertices = t_billboards;
			ren.mesh.normals = t_nrm;
			ren.mesh.uv = t_uvs;
			ren.mesh.uv2 = t_uv2;
			ren.mesh.colors = t_col;
			ren.mesh.triangles = t_tris;

			return ren;
		}

		static pb_Renderable BuildEdgeMesh(pb_Object pb)
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
			ren.materials = k_WireframeMaterials;
			ren.name = "pb_ElementGraphics::WireframeRenderable";
			ren.transform = pb.transform;
			ren.mesh.Clear();
			ren.mesh.name = "Edge Billboard";
			ren.mesh.vertices = pb.vertices;
			ren.mesh.subMeshCount = 1;
			ren.mesh.SetIndices(tris, MeshTopology.Lines, 0);

			return ren;
		}
	}
}

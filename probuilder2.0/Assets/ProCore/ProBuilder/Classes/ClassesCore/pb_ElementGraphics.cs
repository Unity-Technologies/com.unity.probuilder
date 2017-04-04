using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.Common
{
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	[System.Serializable]
	public class pb_ElementGraphics : pb_MonoBehaviourSingleton<pb_ElementGraphics>
	{
		const string FACE_SHADER = "Hidden/ProBuilder/FaceHighlight";
		const string EDGE_SHADER = "Hidden/ProBuilder/FaceHighlight";
		const string VERT_SHADER = "Hidden/ProBuilder/pb_VertexShader";

		const string PREVIEW_OBJECT_NAME = "ProBuilderSelectionGameObject";
		const string WIREFRAME_OBJECT_NAME = "ProBuilderWireframeGameObject";
		const string SELECTION_MESH_NAME = "ProBuilderEditorSelectionMesh";
		const string WIREFRAME_MESH_NAME = "ProBuilderEditorWireframeMesh";

		static float vertexHandleSize = .03f;

		[SerializeField] Material faceMaterial;
		[SerializeField] Material vertexMaterial;
		[SerializeField] Material wireframeMaterial;

		[SerializeField] Color faceSelectionColor 	= new Color(0f, 1f, 1f, .275f);
		[SerializeField] Color edgeSelectionColor 	= new Color(0f, .6f, .7f, 1f);
		[SerializeField] Color vertSelectionColor 	= new Color(1f, .2f, .2f, 1f);
		[SerializeField] Color wireframeColor 		= new Color(0.53f, 0.65f, 0.84f, 1f);	///< Unity's wireframe color (approximately)
		[SerializeField] Color vertexDotColor 		= new Color(.8f, .8f, .8f, 1f);

		static readonly HideFlags PB_EDITOR_GRAPHIC_HIDE_FLAGS = (HideFlags) (1 | 2 | 4 | 8);

		pb_ObjectPool<pb_Renderable> pool;
		List<pb_Renderable> activeRenderables = new List<pb_Renderable>();

		public override void Awake()
		{
			base.Awake();

			gameObject.hideFlags = HideFlags.HideAndDontSave;

			if(pb_MeshRenderer.nullableInstance == null)
				gameObject.AddComponent<pb_MeshRenderer>();

			// Initialize materials
			wireframeMaterial = CreateMaterial(Shader.Find(EDGE_SHADER), "WIREFRAME_MATERIAL");
			wireframeMaterial.SetColor("_Color", wireframeColor);

			faceMaterial = CreateMaterial(Shader.Find(FACE_SHADER), "FACE_SELECTION_MATERIAL");
			faceMaterial.SetColor("_Color", faceSelectionColor);

			vertexMaterial = CreateMaterial(Shader.Find(VERT_SHADER), "VERTEX_BILLBOARD_MATERIAL");
			vertexMaterial.SetColor("_Color", vertexDotColor);
			vertexMaterial.SetFloat("_Scale", vertexHandleSize * 4f);
		}

		void OnDestroy()
		{
			GameObject.DestroyImmediate(faceMaterial);
			GameObject.DestroyImmediate(vertexMaterial);
			GameObject.DestroyImmediate(wireframeMaterial);
		}

		public override void OnEnable()
		{
			base.OnEnable();
			pool = new pb_ObjectPool<pb_Renderable>(0, 8, pb_Renderable.CreateInstance, pb_Renderable.DestroyInstance);
		}

		void OnDisable()
		{
			pool.Empty();
		}

		Material CreateMaterial(Shader shader, string materialName)
		{
			Material mat = new Material(shader);
			mat.name = materialName;
			mat.hideFlags = PB_EDITOR_GRAPHIC_HIDE_FLAGS;
			return mat;
		}

		/**
		 * Reload colors for edge and face highlights from editor prefs.
		 */
		public void LoadPrefs(Hashtable prefs)
		{
			faceSelectionColor 	= (Color) prefs[pb_Constant.pbDefaultFaceColor];
			edgeSelectionColor 	= (Color) prefs[pb_Constant.pbDefaultEdgeColor];
			vertSelectionColor 	= (Color) prefs[pb_Constant.pbDefaultSelectedVertexColor];
			vertexDotColor 		= (Color) prefs[pb_Constant.pbDefaultVertexColor];

			vertexHandleSize 	= (float) prefs[pb_Constant.pbVertexHandleSize];

			wireframeMaterial.SetColor("_Color", wireframeColor);// (_selectMode == SelectMode.Edge && _editLevel == EditLevel.Geometry) ? edgeSelectionColor : wireframeColor);
			faceMaterial.SetColor("_Color", faceSelectionColor);
			vertexMaterial.SetColor("_Color", vertexDotColor);
			vertexMaterial.SetFloat("_Scale", vertexHandleSize * 4f);
		}

		void AddRenderable(pb_Renderable ren)
		{
			activeRenderables.Add(ren);
			pb_MeshRenderer.Add(ren);
		}

		/**
		 * Update the highlight and wireframe graphics.
		 */
		public void RebuildGraphics(pb_Object[] selection, pb_Edge[][] universalEdgesDistinct, EditLevel editLevel, SelectMode selectionMode)
		{
			// in the event that the editor starts calling UpdateGraphics before the object has run OnEnable() (which happens on script reloads)
			if(pool == null) return;

			// clear the current renderables
			foreach(pb_Renderable ren in activeRenderables)
			{
				pool.Put(ren);
				pb_MeshRenderer.Remove(ren);
			}

			// update wireframe
			wireframeMaterial.SetColor("_Color", (selectionMode == SelectMode.Edge && editLevel == EditLevel.Geometry) ? edgeSelectionColor : wireframeColor);

			for(int i = 0; i < selection.Length; i++)
				AddRenderable( BuildEdgeMesh(selection[i], universalEdgesDistinct[i]) );

			if(editLevel == EditLevel.Geometry)
			{
				// update vert / edge / face
				switch(selectionMode)
				{
					case SelectMode.Face:
						foreach(pb_Object pb in selection)
							AddRenderable( BuildFaceMesh(pb) );
						break;

					case SelectMode.Vertex:
						foreach(pb_Object pb in selection)
							AddRenderable( BuildVertexMesh(pb) );
						break;

					default:
						break;
				}
			}
		}

		/**
		 * Populate a renderable's mesh with a face highlight mesh matching the selected triangles array.
		 */
		private pb_Renderable BuildFaceMesh(pb_Object pb)
		{
			pb_Renderable ren = pool.Get();

			ren.name = "Faces Renderable";
			ren.transform = pb.transform;
			ren.materials = new Material[] { faceMaterial };

			ren.mesh.Clear();
			ren.mesh.vertices = pb.vertices;

#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7
			// Unity 5.0 and up is okay with null normals & uvs, but lower versions
			// log a warning to the console even if the shader does not require them
			ren.mesh.normals = pb.vertices;
			ren.mesh.uv = new Vector2[pb.vertexCount];
#endif

			ren.mesh.triangles = pb_Face.AllTriangles(pb.SelectedFaces);

			return ren;
		}

		/**
		  * Populate a rendereble's mesh with a spattering of vertices representing both selected and not selected.
		  */
		private pb_Renderable BuildVertexMesh(pb_Object pb)
		{
			ushort maxBillboardCount = ushort.MaxValue / 4;

			int billboardCount = pb.sharedIndices.Length;

			if(billboardCount > maxBillboardCount)
				billboardCount = maxBillboardCount;

			Vector3[] v = new Vector3[pb.sharedIndices.Length];
			HashSet<int> selected = new HashSet<int>(pb.sharedIndices.GetCommonIndices(pb.SelectedTriangles));

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
					t_col[t+0] = vertSelectionColor;
					t_col[t+1] = vertSelectionColor;
					t_col[t+2] = vertSelectionColor;
					t_col[t+3] = vertSelectionColor;

					// t_nrm[t].x = .1f;
					// t_nrm[t+1].x = .1f;
					// t_nrm[t+2].x = .1f;
					// t_nrm[t+3].x = .1f;
				}
				else
				{
					t_col[t+0] = vertexDotColor;
					t_col[t+1] = vertexDotColor;
					t_col[t+2] = vertexDotColor;
					t_col[t+3] = vertexDotColor;
				}

				t+=4;
				n+=6;
			}

			pb_Renderable ren = pool.Get();

			ren.name = "Vertex Renderable";
			ren.transform = pb.transform;
			ren.materials = new Material[] { vertexMaterial };
			ren.mesh.Clear();
			ren.mesh.vertices = t_billboards;
			ren.mesh.normals = t_nrm;
			ren.mesh.uv = t_uvs;
			ren.mesh.uv2 = t_uv2;
			ren.mesh.colors = t_col;
			ren.mesh.triangles = t_tris;

			return ren;
		}

		private pb_Renderable BuildEdgeMesh(pb_Object pb, pb_Edge[] universalEdgesDistinct)
		{
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			int segmentCount = universalEdgesDistinct.Length;
			int[] lineSegments = new int[segmentCount * 2];

			int n = 0;

			for(int i = 0; i < segmentCount; i++)
			{
				lineSegments[n++] = sharedIndices[universalEdgesDistinct[i].x][0];
				lineSegments[n++] = sharedIndices[universalEdgesDistinct[i].y][0];
			}

			pb_Renderable ren = pool.Get();

			ren.name = "Wireframe Renderable";
			ren.materials = new Material[] { wireframeMaterial };
			ren.transform = pb.transform;
			ren.mesh.name = "Wireframe Mesh";
			ren.mesh.Clear();
			ren.mesh.vertices = pb.vertices;
#if !UNITY_5
			// appease unity 4
			ren.mesh.normals = pb.vertices;
			ren.mesh.uv = new Vector2[pb.vertexCount];
#endif
			ren.mesh.subMeshCount = 1;
			ren.mesh.SetIndices(lineSegments, MeshTopology.Lines, 0);

			return ren;
		}
	}
}

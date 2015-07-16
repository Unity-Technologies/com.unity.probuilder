using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if PB_DEBUG
using Parabox.Debug;
#endif

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

		[SerializeField] pb_MeshRenderer pbRenderer;

		[SerializeField] Color faceSelectionColor 	= new Color(0f, 1f, 1f, .275f);
		[SerializeField] Color edgeSelectionColor 	= new Color(0f, .6f, .7f, 1f);
		[SerializeField] Color vertSelectionColor 	= new Color(1f, .2f, .2f, 1f);
		[SerializeField] Color wireframeColor 		= new Color(0.53f, 0.65f, 0.84f, 1f);	///< Unity's wireframe color (approximately)
		[SerializeField] Color vertexDotColor 		= new Color(.8f, .8f, .8f, 1f);

		static readonly HideFlags PB_EDITOR_GRAPHIC_HIDE_FLAGS = (HideFlags) (1 | 2 | 4 | 8);

		pb_ObjectPool<pb_Renderable> pool;

		public override void Awake()
		{
			base.Awake();

			gameObject.hideFlags = HideFlags.HideAndDontSave;
			pbRenderer = gameObject.AddComponent<pb_MeshRenderer>();

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

		/**
		 * Update the highlight and wireframe graphics.
		 */
		public void RebuildGraphics(pb_Object[] selection, pb_Edge[][] universalEdgesDistinct, EditLevel editLevel, SelectMode selectionMode)
		{
			// in the event that the editor starts calling UpdateGraphics before the object has run OnEnable() (which happens on script reloads)
			if(pool == null) return;

			// clear t he current renderables
			foreach(pb_Renderable ren in pbRenderer.renderables)
				pool.Put(ren);

			pbRenderer.renderables.Clear();

			// update wireframe
			wireframeMaterial.SetColor("_Color", (selectionMode == SelectMode.Edge && editLevel == EditLevel.Geometry) ? edgeSelectionColor : wireframeColor);

			for(int i = 0; i < selection.Length; i++)
				pbRenderer.renderables.Add( BuildEdgeMesh(selection[i], universalEdgesDistinct[i]) );

			if(editLevel == EditLevel.Geometry)
			{
				// update vert / edge / face
				switch(selectionMode)
				{
					case SelectMode.Face:
						foreach(pb_Object pb in selection)
							pbRenderer.renderables.Add( BuildFaceMesh(pb) );
						break;

					case SelectMode.Vertex:
						foreach(pb_Object pb in selection)
							pbRenderer.renderables.Add( BuildVertexMesh(pb) );
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
			int[] selectedTriangles = pb_Face.AllTriangles(pb.SelectedFaces);

			Vector3[] 	v = pbUtil.ValuesWithIndices(pb.vertices, selectedTriangles);

			pb_Renderable ren = pool.Get();

			ren.name = "Faces Renderable";
			ren.transform = pb.transform;
			ren.materials = new Material[] { faceMaterial };

			ren.mesh.Clear();
			ren.mesh.vertices = v;
			ren.mesh.normals = v;
#if UNITY_5
			ren.mesh.uv = null;
#else
			ren.mesh.uv = new Vector2[v.Length];
#endif
			ren.mesh.triangles = SequentialTriangles(v.Length);

			return ren;
		}

		/**
		  * Populate a rendereble's mesh with a spattering of vertices representing both selected and not selected.
		  */
		private pb_Renderable BuildVertexMesh(pb_Object pb)
		{
			int vcount = 0;

			Vector3[] v = new Vector3[pb.sharedIndices.Length];
			HashSet<int> selected = new HashSet<int>(pb.sharedIndices.GetUniversalIndices(pb.SelectedTriangles));

			for(int i = 0; i < v.Length; i++)	
				v[i] = pb.vertices[pb.sharedIndices[i][0]];

			Vector3[] 	t_billboards 		= new Vector3[v.Length*4];
			Vector3[] 	t_nrm 				= new Vector3[v.Length*4];
			Vector2[] 	t_uvs 				= new Vector2[v.Length*4];
			Vector2[] 	t_uv2 				= new Vector2[v.Length*4];
			Color[]   	t_col 				= new Color[v.Length*4];
			int[] 		t_tris 				= new int[v.Length*6];

			int n = 0;
			int t = 0;

			Vector3 up = Vector3.up;// * .1f;
			Vector3 right = Vector3.right;// * .1f;
			
			for(int i = 0; i < v.Length; i++)
			{
				t_billboards[t+0] = v[i];//-up-right;
				t_billboards[t+1] = v[i];//-up+right;
				t_billboards[t+2] = v[i];//+up-right;
				t_billboards[t+3] = v[i];//+up+right;

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

				t_tris[n+0] = t+0+vcount;
				t_tris[n+1] = t+1+vcount;
				t_tris[n+2] = t+2+vcount;
				t_tris[n+3] = t+1+vcount;
				t_tris[n+4] = t+3+vcount;
				t_tris[n+5] = t+2+vcount;

				if( selected.Contains(i) )
				{
					t_col[t+0] = vertSelectionColor;
					t_col[t+1] = vertSelectionColor;
					t_col[t+2] = vertSelectionColor;
					t_col[t+3] = vertSelectionColor;

					t_nrm[t].x = .1f;
					t_nrm[t+1].x = .1f;
					t_nrm[t+2].x = .1f;
					t_nrm[t+3].x = .1f;
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

			Vector3[] pbverts = pb.vertices;
			pb_IntArray[] sharedIndices = pb.sharedIndices;

			int vertexCount = System.Math.Min(universalEdgesDistinct.Count() * 2, pb_Constant.MAX_VERTEX_COUNT);
			Vector3[] edge_verts = new Vector3[vertexCount];

			int n = 0;
			for(int i = 0; i < vertexCount / 2; i++) // (pb_Edge e in universalEdgesDistinct)
			{
				edge_verts[n++] = pbverts[sharedIndices[universalEdgesDistinct[i].x][0]];
				edge_verts[n++] = pbverts[sharedIndices[universalEdgesDistinct[i].y][0]];
			}

			pb_Renderable ren = pool.Get();

			ren.name = "Wireframe Renderable";
			ren.materials = new Material[] { wireframeMaterial };
			ren.transform = pb.transform;
			ren.mesh.name = "Wireframe Mesh";
			ren.mesh.Clear();
			ren.mesh.vertices = edge_verts;
#if !UNITY_5
			ren.mesh.normals = edge_verts;	// appease unity 4
			ren.mesh.uv = new Vector2[edge_verts.Length];
#endif
			ren.mesh.subMeshCount = 1;
			ren.mesh.SetIndices(SequentialTriangles(edge_verts.Length), MeshTopology.Lines, 0);
			
			return ren;
		}

		static int[] SequentialTriangles(int len)
		{
			int[] tris = new int[len];
			for(int i = 0; i < len; i++) {
				tris[i] = i;
			}
			return tris;
		}
	}
}
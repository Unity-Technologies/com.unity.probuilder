using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using System.Linq;

#if PB_DEBUG
using Parabox.Debug;
#endif

[ExecuteInEditMode]
[System.Serializable]
public class pb_Editor_Graphics : MonoBehaviour
{
	public static pb_Editor_Graphics instance
	{
		get
		{
			if(_instance == null)
			{
				pb_Editor_Graphics[] danglers = Resources.FindObjectsOfTypeAll<pb_Editor_Graphics>();

				if(danglers == null || danglers.Length < 1)
				{
					GameObject go = new GameObject();
					go.name = "pb_Editor_Graphics";
					_instance = go.AddComponent<pb_Editor_Graphics>();
					_instance.gameObject.hideFlags = HideFlags.DontSave;
				}
				else
				{
					// shouldn't ever have dangling instances, but just in case...
					_instance = danglers[0];
					for(int i = 1; i < danglers.Length; i++)
						GameObject.DestroyImmediate(danglers[i]);
				}
			}

			return _instance;
		}
	}

	private static pb_Editor_Graphics _instance;

	const string FACE_SHADER = "Hidden/ProBuilder/FaceHighlight";
	const string EDGE_SHADER = "Hidden/ProBuilder/FaceHighlight";
	const string VERT_SHADER = "Hidden/ProBuilder/pb_VertexShader";

	const string PREVIEW_OBJECT_NAME = "ProBuilderSelectionGameObject";
	const string WIREFRAME_OBJECT_NAME = "ProBuilderWireframeGameObject";
	const string SELECTION_MESH_NAME = "ProBuilderEditorSelectionMesh";
	const string WIREFRAME_MESH_NAME = "ProBuilderEditorWireframeMesh";

	static float vertexHandleSize = .03f;

	[SerializeField] Material 			faceMaterial;
	[SerializeField] Material 			vertexMaterial;
	[SerializeField] Material 			wireframeMaterial;

	[SerializeField] pb_MeshRenderer renderer;

	[SerializeField] Color faceSelectionColor 	= new Color(0f, 1f, 1f, .275f);
	[SerializeField] Color edgeSelectionColor 	= new Color(0f, .6f, .7f, 1f);
	[SerializeField] Color vertSelectionColor 	= new Color(1f, .2f, .2f, 1f);
	[SerializeField] Color wireframeColor 		= new Color(0.53f, 0.65f, 0.84f, 1f);	///< Unity's wireframe color (approximately)
	[SerializeField] Color vertexDotColor 		= new Color(.8f, .8f, .8f, 1f);

	static readonly HideFlags PB_EDITOR_GRAPHIC_HIDE_FLAGS = (HideFlags) (1 | 2 | 4 | 8);

	pb_ObjectPool<pb_Renderable> pool;

	void Awake()
	{
		_instance = this;

		renderer = gameObject.AddComponent<pb_MeshRenderer>();

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

	void OnEnable()
	{
		_instance = this;
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
		vertexHandleSize 	= (float) prefs[pb_Constant.pbVertexHandleSize];

		wireframeMaterial.SetColor("_Color", wireframeColor);// (_selectMode == SelectMode.Edge && _editLevel == EditLevel.Geometry) ? edgeSelectionColor : wireframeColor);
		faceMaterial.SetColor("_Color", faceSelectionColor);
		vertexMaterial.SetColor("_Color", vertexDotColor);
		vertexMaterial.SetFloat("_Scale", vertexHandleSize * 4f);
	}

	/**
	 * Update the highlight and wireframe graphics.
	 */
	public void UpdateGraphics(pb_Object[] selection, EditLevel editLevel, SelectMode selectionMode)
	{
		// clear the current renderables
		foreach(pb_Renderable ren in renderer.renderables)
			pool.Put(ren);

		renderer.renderables.Clear();

		// update wireframe
		wireframeMaterial.SetColor("_Color", (selectionMode == SelectMode.Edge && editLevel == EditLevel.Geometry) ? edgeSelectionColor : wireframeColor);

		foreach(pb_Object pb in selection)
			renderer.renderables.Add( BuildEdgeMesh(pb) );

		// update vert / edge / face
		switch(selectionMode)
		{
			case SelectMode.Face:
				foreach(pb_Object pb in selection)
					renderer.renderables.Add( BuildFaceMesh(pb) );
				break;

			case SelectMode.Vertex:
				foreach(pb_Object pb in selection)
					renderer.renderables.Add( BuildVertexMesh(pb) );
				break;

			default:
				break;
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
		ren.matrix = pb.transform.localToWorldMatrix;
		ren.materials = new Material[] { faceMaterial };

		ren.mesh.Clear();
		ren.mesh.vertices = v;
		ren.mesh.normals = v;
		ren.mesh.uv = null;
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
		ren.matrix = pb.transform.localToWorldMatrix;
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

	private pb_Renderable BuildEdgeMesh(pb_Object pb)
	{
		Vector3[] pbverts = pb.vertices;
		pb_IntArray[] sharedIndices = pb.sharedIndices;

		// not exactly loosely coupled, but GetUniversal edges is ~40ms on a 2000 vertex object
		pb_Edge[] universalEdges = pb_Edge.GetUniversalEdges(pb_Edge.AllEdges(pb.faces), pb.sharedIndices).Distinct().ToArray();
		Vector3[] edge_verts = new Vector3[universalEdges.Length*2];

		int n = 0;
		foreach(pb_Edge e in universalEdges)
		{
			edge_verts[n++] = pbverts[sharedIndices[e.x][0]];
			edge_verts[n++] = pbverts[sharedIndices[e.y][0]];
		}

		pb_Renderable ren = pool.Get();

		ren.name = "Wireframe Renderable";
		ren.materials = new Material[] { wireframeMaterial };
		ren.matrix = pb.transform.localToWorldMatrix;
		ren.mesh.name = "Wireframe Mesh";
		ren.mesh.Clear();
		ren.mesh.vertices = edge_verts;
		ren.mesh.normals = edge_verts;	// appease unity 4
		ren.mesh.uv = new Vector2[edge_verts.Length];
		ren.mesh.subMeshCount = 1;
		ren.mesh.SetIndices(SequentialTriangles(edge_verts.Length), MeshTopology.Lines, 0);
		
		return ren;
	}

	// /**
	//  * Generate a mesh composed of all universal edges in an array of pb_Object.
	//  */
	// internal static void UpdateWireframeMeshes(pb_Object[] selection)
	// {
	// 	for(int i = 0; i < wireframeRenderer.renderables.Count; i++)
	// 		pool.Put(wireframeRenderer.renderables[i]);

	// 	wireframeRenderer.renderables.Clear();

	// 	if( (editor == null || selection == null || editor.SelectedUniversalEdges == null) || selection.Length != editor.SelectedUniversalEdges.Length )
	// 		return;

	// 	try
	// 	{
	// 		for(int i = 0; i < selection.Length; i++)
	// 		{
	// 			pb_Renderable ren = pool.Get();
	// 			ren.name = "Wireframe Renderable";
	// 			Mesh mesh = ren.mesh;
	// 			ren.materials[0] = WireframeMaterial;
	// 			ren.matrix = selection[i].transform.localToWorldMatrix;
	// 			pb_Object pb = selection[i];

	// 			Vector3[] pbverts = pb.vertices;
	// 			pb_IntArray[] sharedIndices = pb.sharedIndices;

	// 			// not exactly loosely coupled, but GetUniversal edges is ~40ms on a 2000 vertex object
	// 			pb_Edge[] universalEdges = editor.SelectedUniversalEdges[i];
	// 			Vector3[] edge_verts = new Vector3[universalEdges.Length*2];
			
	// 			int n = 0;
	// 			foreach(pb_Edge e in universalEdges)
	// 			{
	// 				edge_verts[n++] = pbverts[sharedIndices[e.x][0]];
	// 				edge_verts[n++] = pbverts[sharedIndices[e.y][0]];
	// 			}
				
	// 			mesh.Clear();
	// 			mesh.vertices = edge_verts;
	// 			mesh.normals = edge_verts;	// appease unity 4
	// 			mesh.uv = new Vector2[edge_verts.Length];
	// 			mesh.subMeshCount = 1;
	// 			mesh.SetIndices(SequentialTriangles(edge_verts.Length), MeshTopology.Lines, 0);
	// 			mesh.hideFlags = PB_EDITOR_GRAPHIC_HIDE_FLAGS;

	// 			wireframeRenderer.renderables.Add(ren);
	// 		}
	// 	}
	// 	catch
	// 	{
	// 	}
	// }

	static int[] SequentialTriangles(int len)
	{
		int[] tris = new int[len];
		for(int i = 0; i < len; i++) {
			tris[i] = i;
		}
		return tris;
	}
}
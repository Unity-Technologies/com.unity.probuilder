#pragma warning disable 0168	///< Disable unused var (that exception hack)

#define FORCE_MESH_GRAPHICS

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using System.Linq;

#if PB_DEBUG
using Parabox.Debug;
#endif

public class pb_Editor_Graphics
{
	const string FACE_SHADER = "Hidden/ProBuilder/FaceHighlight";// "Hidden/ProBuilder/UnlitColor";
	const string EDGE_SHADER = "Hidden/ProBuilder/UnlitEdgeOffset";

	// "Hidden/ProBuilder/VertexBillboard"
	const string PREVIEW_OBJECT_NAME = "ProBuilderSelectionMeshObject";
	const string WIREFRAME_OBJECT_NAME = "ProBuilderWireframeMeshObject";

	static float vertexHandleSize = .04f;
	const float SELECTION_MESH_OFFSET = 0.0001f;//.005f;
	
	public static GameObject 	selectionObject { get; private set; }	// allow get so that pb_Editor can check that the user hasn't 
	public static GameObject 	wireframeObject { get; private set; }	// selected the graphic objects on accident.

	static Mesh 				selectionMesh;
	static Mesh 				wireframeMesh;
	static Material 			selectionMaterial;
	static Material 			wireframeMaterial;

	static Color 				faceSelectionColor = new Color(0f, 1f, 1f, .275f);
	static Color 				edgeSelectionColor = new Color(0f, .6f, .7f, 1f);

	static Color 				wireframeColor = new Color(0.53f, 0.65f, 0.84f, 1f);	///< Unity's wireframe color (approximately)

	public static SelectMode 	_selectMode = SelectMode.Face;

	static pb_Editor editor { get { return pb_Editor.instance; } }

	/**
	 * Reload colors for edge and face highlights from editor prefs.
	 */
	public static void LoadColors()
	{
		faceSelectionColor = pb_Preferences_Internal.GetColor(pb_Constant.pbDefaultFaceColor);
		edgeSelectionColor = pb_Preferences_Internal.GetColor(pb_Constant.pbDefaultEdgeColor);

		SetMaterial(_selectMode);
	}

	private static void Init()
	{
		DestroyTempObjects();

		HideFlags hide = (HideFlags)(1 | 2 | 4 | 8);	/// I'll just have eeeverything on the menu.

		selectionObject = EditorUtility.CreateGameObjectWithHideFlags(PREVIEW_OBJECT_NAME, hide, new System.Type[2]{typeof(MeshFilter), typeof(MeshRenderer)});
		wireframeObject = EditorUtility.CreateGameObjectWithHideFlags(WIREFRAME_OBJECT_NAME, hide, new System.Type[2]{typeof(MeshFilter), typeof(MeshRenderer)});
		
		selectionObject.GetComponent<MeshFilter>().sharedMesh = new Mesh();
		wireframeObject.GetComponent<MeshFilter>().sharedMesh = new Mesh();

		selectionObject.GetComponent<MeshRenderer>().enabled = false;
		wireframeObject.GetComponent<MeshRenderer>().enabled = false;

		// Force the mesh to only render in SceneView
		selectionObject.AddComponent<pb_SceneMeshRender>();
		wireframeObject.AddComponent<pb_SceneMeshRender>();

		selectionMesh = selectionObject.GetComponent<MeshFilter>().sharedMesh;
		wireframeMesh = wireframeObject.GetComponent<MeshFilter>().sharedMesh;

		LoadColors();
		vertexHandleSize = pb_Preferences_Internal.GetFloat(pb_Constant.pbVertexHandleSize);

		SetMaterial(_selectMode);
	}

	/**
	 * If Materials are null, initialize them.
	 */
	static void SetMaterial(SelectMode sm)
	{
		if(selectionMaterial != null) GameObject.DestroyImmediate(selectionMaterial);
		if(wireframeMaterial != null) GameObject.DestroyImmediate(wireframeMaterial);

		// Always generate the wireframe
		wireframeMaterial = new Material(Shader.Find(EDGE_SHADER));

		switch(sm)
		{
			// case SelectMode.Vertex:
			// 	vertexHandleSize = pb_Preferences_Internal.GetFloat(pb_Constant.pbVertexHandleSize);
			// 	selectionMaterial = new Material(Shader.Find( GetRenderingPath() == RenderingPath.DeferredLighting ? OVERLAY_SHADER : HIGHLIGHT_SHADER ));
			// 	selectionMaterial.SetTexture("_MainTex", (Texture2D)Resources.Load("Textures/VertOff", typeof(Texture2D)));
			// 	break;

			default:
				selectionMaterial = new Material(Shader.Find(FACE_SHADER));
				break;
		}

		wireframeMaterial.SetColor("_Color", sm == SelectMode.Edge ? edgeSelectionColor : wireframeColor);
		selectionMaterial.SetColor("_Color", faceSelectionColor);
	
		selectionObject.GetComponent<MeshRenderer>().sharedMaterial = selectionMaterial;
		wireframeObject.GetComponent<MeshRenderer>().sharedMaterial = wireframeMaterial;
	}

	static internal void OnDisable()
	{
		DestroyTempObjects();
	}

	static internal void DestroyTempObjects()
	{
		DestroyObjectsWithName(PREVIEW_OBJECT_NAME);
		DestroyObjectsWithName(WIREFRAME_OBJECT_NAME);
	}

	static private void DestroyObjectsWithName(string InName)
	{
		GameObject go = GameObject.Find(InName);

		while(go != null)
		{
			Mesh msh = go.GetComponent<MeshFilter>().sharedMesh;
			Material mat = go.GetComponent<MeshRenderer>().sharedMaterial;

			if(msh != null) GameObject.DestroyImmediate(msh);

			if(mat != null) GameObject.DestroyImmediate(mat);

			GameObject.DestroyImmediate(go);

			go = GameObject.Find(InName);
		}
	}

	// static internal void Draw()
	// {
	// 	// selectionMaterial.SetPass(0);
	// 	// Graphics.DrawMeshNow(selectionMesh, Vector3.zero, Quaternion.identity/*, selectionMaterial*/, 0);
	// }

	/**
	 * Draw the wireframe with the regular mesh rendering pipeline, which has the effect of being
	 * much lighter and more akin to Unity's default wireframe.
	 */
	// static internal void DrawWireframe()
	// {
	// 	wireframeMaterial.SetPass(0);
	// 	Graphics.DrawMeshNow(wireframeMesh, Vector3.zero, Quaternion.identity/*, selectionMaterial*/, 0);
	// 	Graphics.DrawMesh(wireframeMesh, Vector3.zero, Quaternion.identity, wireframeMaterial, 0);
	// }

	/**
	 * Draw vertex handles using UnityEngine.Handles.
	 */
	public static void DrawVertexHandles(int selectionLength, int[][] indices, Vector3[][] allVerticesInWorldSpace, Color col)
	{
		Color t = Handles.color;
		Handles.color = col;
		try {
			for(int i = 0; i < selectionLength; i++)
			{
				foreach(int j in indices[i])
				{
					Vector3 pos = allVerticesInWorldSpace[i][j];
					Handles.DotCap(-1, pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * vertexHandleSize);
				}
			}
		} catch (System.Exception e) {}

		Handles.color = t;
	}

	/**
	 * Refresh the selection and wireframe mesh with _selection.
	 */
	static internal void UpdateSelectionMesh(pb_Object[] _selection, SelectMode selectionMode)
	{
		// Debug.Log("UpdateSelectionMesh");
		// Always clear the mesh whenever updating, even if selection is null.
		if(selectionObject == null || wireframeObject == null || selectionMesh == null || wireframeMesh == null)
			Init();

		selectionMesh.Clear();
		wireframeMesh.Clear();

		if(_selection == null || _selection.Length < 1)
		{
			return;
		}

		if(_selectMode != selectionMode)
		{	
			SetMaterial(selectionMode);
			_selectMode = selectionMode;
		}

		selectionMesh.name = "EDITOR_SELECTION_MESH";
		wireframeMesh.name = "EDITOR_WIREFRAME_MESH";

		List<Vector3> verts = new List<Vector3>();
		List<Vector4> tan = new List<Vector4>();
		List<Vector2> uvs 	= new List<Vector2>();
		List<Vector2> uv2s 	= new List<Vector2>();
		List<Color> col = new List<Color>();
		List<int> tris = new List<int>();

		MakeEdgeMesh(_selection, ref wireframeMesh);

		switch( selectionMode )
		{
			case SelectMode.Vertex:

				// int vcount = 0;
				// foreach(pb_Object pb in _selection)
				// {
				// 	Vector3[] v = new Vector3[pb.uniqueIndices.Length];
				// 	for(int i = 0; i < v.Length; i++)	
				// 		v[i] = pb.vertices[pb.uniqueIndices[i]];

				// 	int[] sel = pb.SelectedTriangles;

				// 	Vector3[] 	t_billboards 		= new Vector3[v.Length*4];
				// 	Vector3[] 	t_nrm 				= new Vector3[v.Length*4];
				// 	Vector2[] 	t_uvs 				= new Vector2[v.Length*4];
				// 	Vector2[] 	t_uv2 				= new Vector2[v.Length*4];
				// 	Vector4[] 	t_tan 				= new Vector4[v.Length*4];
				// 	Color[]   	t_col 				= new Color[v.Length*4];
				// 	int[] 		t_tris 				= new int[v.Length*6];

				// 	int n = 0;
				// 	int t = 0;

				// 	Vector3 up = Vector3.up;// * .1f;
				// 	Vector3 right = Vector3.right;// * .1f;

				// 	for(int i = 0; i < v.Length; i++)
				// 	{
				// 		Vector3 vpoint = pb.transform.TransformPoint(v[i]);
				// 		float handleSize = HandleUtility.GetHandleSize(vpoint) * .04f;
						
				// 		t_billboards[t+0] = vpoint;//-up-right;
				// 		t_billboards[t+1] = vpoint;//-up+right;
				// 		t_billboards[t+2] = vpoint;//+up-right;
				// 		t_billboards[t+3] = vpoint;//+up+right;

				// 		t_uvs[t+0] = Vector3.zero;
				// 		t_uvs[t+1] = Vector3.right;
				// 		t_uvs[t+2] = Vector3.up;
				// 		t_uvs[t+3] = Vector3.one;

				// 		t_tan[t+0] = new Vector4(handleSize, 0f, 0f, 0f);
				// 		t_tan[t+1] = new Vector4(handleSize, 0f, 0f, 0f);
				// 		t_tan[t+2] = new Vector4(handleSize, 0f, 0f, 0f);
				// 		t_tan[t+3] = new Vector4(handleSize, 0f, 0f, 0f);
	
				// 		t_uv2[t+0] = -up-right;
				// 		t_uv2[t+1] = -up+right;
				// 		t_uv2[t+2] =  up-right;
				// 		t_uv2[t+3] =  up+right;
	
				// 		t_nrm[t+0] = Vector3.forward;
				// 		t_nrm[t+1] = Vector3.forward;
				// 		t_nrm[t+2] = Vector3.forward;
				// 		t_nrm[t+3] = Vector3.forward;

				// 		t_tris[n+0] = t+2+vcount;
				// 		t_tris[n+1] = t+1+vcount;
				// 		t_tris[n+2] = t+0+vcount;
				// 		t_tris[n+3] = t+2+vcount;
				// 		t_tris[n+4] = t+3+vcount;
				// 		t_tris[n+5] = t+1+vcount;

				// 		if(System.Array.IndexOf(sel, pb.uniqueIndices[i]) > -1)
				// 		{
				// 			t_col[t+0] = Color.green;
				// 			t_col[t+1] = Color.green;
				// 			t_col[t+2] = Color.green;
				// 			t_col[t+3] = Color.green;
				// 		}
				// 		else
				// 		{
				// 			t_col[t+0] = faceSelectionColor;
				// 			t_col[t+1] = faceSelectionColor;
				// 			t_col[t+2] = faceSelectionColor;
				// 			t_col[t+3] = faceSelectionColor;
				// 		}

				// 		t+=4;
				// 		n+=6;				
				// 	}

				// 	verts.AddRange(t_billboards);
				// 	vcount += t_billboards.Length;
				// 	uvs.AddRange(t_uvs);
				// 	uv2s.AddRange(t_uv2);
				// 	tan.AddRange(t_tan);
				// 	col.AddRange(t_col);
				// 	tris.AddRange(t_tris);
				// }

				break;

			case SelectMode.Face:

				foreach(pb_Object pb in _selection)			
				{
					int[] selectedTriangles = pb_Face.AllTriangles(pb.SelectedFaces);

					Vector3[] 	v = pb.VerticesInWorldSpace(selectedTriangles);
					Vector2[] 	u = pbUtil.ValuesWithIndices(pb.uv, selectedTriangles);

					verts.AddRange(v);
					uvs.AddRange  (u);
				}

				tris = new List<int>(verts.Count);			// because ValuesWithIndices returns in wound order, just fill
				tan = new List<Vector4>(verts.Count);
				for(int p = 0; p < verts.Count; p++)		// triangles with 0, 1, 2, 3, etc
				{
					tan.Add(Vector4.one);
					tris.Add(p);
				}
				
				break;
		}

		selectionMesh.vertices = verts.ToArray();	// it is assigned here because we need to get normals
		selectionMesh.triangles = tris.ToArray();
		selectionMesh.uv = uvs.ToArray();
		selectionMesh.uv2 = uv2s.ToArray();
		selectionMesh.tangents = tan.ToArray();
		selectionMesh.colors = col.ToArray();

		// if(selectionMode == SelectMode.Face)
		// {
		// 	selectionMesh.RecalculateNormals();

		// 	Vector3[] nrmls = selectionMesh.normals;

		// 	for(int i = 0; i < verts.Count; i++)
		// 		verts[i] += SELECTION_MESH_OFFSET * nrmls[i].normalized;

		// 	selectionMesh.vertices = verts.ToArray();
		// }
	}

	/**
	 * Returns the actual rendering path - (will not return UsePlayerSettings).
	 */
	public static RenderingPath GetRenderingPath()
	{
		SceneView scn = SceneView.lastActiveSceneView;
		Camera cam;

		if(scn != null)
		{
			cam = scn.camera;
		}
		else
		{
			cam = Camera.main;
		}

		if(cam == null)
			return RenderingPath.Forward;

		return cam.actualRenderingPath == RenderingPath.UsePlayerSettings ? PlayerSettings.renderingPath : cam.actualRenderingPath;
	}

	/**
	 * Generate a mesh composed of all universal edges in an array of pb_Object.
	 */
	static void MakeEdgeMesh(pb_Object[] selection, ref Mesh mesh)
	{
		List<Vector3> all_verts = new List<Vector3>();

		for(int i = 0; i < selection.Length; i++)
		{
			pb_Object pb = selection[i];

			Vector3[] pbverts = pb.vertices;
			pb_IntArray[] sharedIndices = pb.sharedIndices;

			// not exactly loosely coupled, but GetUniversal edges is ~40ms on a 2000 vertex object
			pb_Edge[] universalEdges = editor.Selected_Universal_Edges_All[i]; // new List<pb_Edge>(pb_Edge.GetUniversalEdges(pb_Edge.AllEdges(pb.faces), sharedIndices).Distinct());
			
			Vector3[] edge_verts = new Vector3[universalEdges.Length*2];
			
			int n = 0;
			foreach(pb_Edge e in universalEdges)
			{
				edge_verts[n++] = pb.transform.TransformPoint(pbverts[sharedIndices[e.x][0]]);
				edge_verts[n++] = pb.transform.TransformPoint(pbverts[sharedIndices[e.y][0]]);
			}

			all_verts.AddRange(edge_verts);
		}

		int[] tris = new int[all_verts.Count * 2];
		Vector2[] uvs = new Vector2[all_verts.Count];

		for(int i = 0; i < all_verts.Count; i+=2)
		{
			tris[i] = i;
			tris[i+1] = i+1;

			uvs[i] = Vector2.zero;
			uvs[i+1] = Vector2.zero;
		}

		mesh.Clear();
		mesh.vertices = all_verts.ToArray();
		mesh.uv = uvs;
		mesh.subMeshCount = 1;
		mesh.SetIndices(tris.ToArray(), MeshTopology.Lines, 0);
	}
}
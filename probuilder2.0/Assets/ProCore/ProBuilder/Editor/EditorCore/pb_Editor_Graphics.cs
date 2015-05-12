#pragma warning disable 0168	///< Disable unused var (that exception hack)

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

[InitializeOnLoad]
public class pb_Editor_Graphics
{
	const string FACE_SHADER = "Hidden/ProBuilder/FaceHighlight";// "Hidden/ProBuilder/UnlitColor";
	const string EDGE_SHADER = "Hidden/ProBuilder/FaceHighlight";
	const string VERT_SHADER = "Hidden/ProBuilder/pb_VertexShader";

	// "Hidden/ProBuilder/VertexBillboard"
	const string PREVIEW_OBJECT_NAME = "ProBuilderSelectionGameObject";
	const string WIREFRAME_OBJECT_NAME = "ProBuilderWireframeGameObject";
	const string SELECTION_MESH_NAME = "ProBuilderEditorSelectionMesh";
	const string WIREFRAME_MESH_NAME = "ProBuilderEditorWireframeMesh";

	static float vertexHandleSize = .03f;
	
	public static GameObject 	selectionObject { get; private set; }	// allow get so that pb_Editor can check that the user hasn't 
	public static GameObject 	wireframeObject { get; private set; }	// selected the graphic objects on accident.

	static Material 			faceMaterial;
	static Material 			vertexMaterial;
	static Material 			wireframeMaterial;
	static pb_MeshRenderer 		wireframeRenderer;
	static pb_MeshRenderer 		selectionRenderer;

	private static pb_ObjectPool<pb_Renderable> renderablePool;

	static pb_Editor_Graphics()
	{
		renderablePool = new pb_ObjectPool<pb_Renderable>(2, 8, CreateEditorRenderable, DestroyEditorRenderable);
	}

	static Color 				faceSelectionColor = new Color(0f, 1f, 1f, .275f);
	static Color 				edgeSelectionColor = new Color(0f, .6f, .7f, 1f);
	static Color 				vertSelectionColor = new Color(1f, .2f, .2f, 1f);

	static Color 				wireframeColor = new Color(0.53f, 0.65f, 0.84f, 1f);	///< Unity's wireframe color (approximately)
	static Color 				vertexDotColor = new Color(.8f, .8f, .8f, 1f);

	private static EditLevel 	_editLevel = EditLevel.Geometry;
	private static SelectMode 	_selectMode = SelectMode.Face;

	static pb_Editor editor { get { return pb_Editor.instance; } }

	static readonly HideFlags PB_EDITOR_GRAPHIC_HIDE_FLAGS = (HideFlags) (1 | 2 | 4 | 8);

	static Material WireframeMaterial
	{
		get
		{
			if(wireframeMaterial == null)
			{
				wireframeMaterial = new Material(Shader.Find(EDGE_SHADER));
				wireframeMaterial.name = "WIREFRAME_MATERIAL";
				wireframeMaterial.SetColor("_Color", (_selectMode == SelectMode.Edge && _editLevel == EditLevel.Geometry) ? edgeSelectionColor : wireframeColor);
				wireframeMaterial.hideFlags = PB_EDITOR_GRAPHIC_HIDE_FLAGS;
			}
			return wireframeMaterial;
		}
	}

	static Material FaceMaterial
	{
		get
		{
			if( faceMaterial == null )	
			{
				faceMaterial = new Material(Shader.Find(FACE_SHADER));
				faceMaterial.name = "FACE_SELECTION_MATERIAL";
				faceMaterial.SetColor("_Color", faceSelectionColor);
				faceMaterial.hideFlags = PB_EDITOR_GRAPHIC_HIDE_FLAGS;
			}

			return faceMaterial;
		}
	}

	static Material VertexMaterial
	{
		get
		{
			if( vertexMaterial == null )	
			{
				vertexHandleSize = pb_Preferences_Internal.GetFloat(pb_Constant.pbVertexHandleSize);
				vertexMaterial = new Material(Shader.Find(VERT_SHADER));
				vertexMaterial.name = "VERTEX_BILLBOARD_MATERIAL";
				Texture2D dot = (Texture2D)Resources.Load("Textures/VertOff");
				vertexMaterial.mainTexture = dot;
				vertexMaterial.SetColor("_Color", vertexDotColor);
				vertexMaterial.SetFloat("_Scale", vertexHandleSize * (dot == null ? 4f : 6f));
				vertexMaterial.hideFlags = PB_EDITOR_GRAPHIC_HIDE_FLAGS;
			}

			return vertexMaterial;
		}
	}

	/**
	 * Reload colors for edge and face highlights from editor prefs.
	 */
	public static void LoadPrefs()
	{
		faceSelectionColor 	= pb_Preferences_Internal.GetColor(pb_Constant.pbDefaultFaceColor);
		edgeSelectionColor 	= pb_Preferences_Internal.GetColor(pb_Constant.pbDefaultEdgeColor);
		vertSelectionColor 	= pb_Preferences_Internal.GetColor(pb_Constant.pbDefaultSelectedVertexColor);

		if(!selectionObject || !wireframeObject)
			Init(_editLevel, _selectMode);

		vertexHandleSize = pb_Preferences_Internal.GetFloat(pb_Constant.pbVertexHandleSize);
	}

	/**
	 * Create selection and wireframe render objects, and set their hideflasgs.
	 */
	private static void Init(EditLevel el, SelectMode sm)
	{
		DestroyTempObjects();

		selectionObject = EditorUtility.CreateGameObjectWithHideFlags(PREVIEW_OBJECT_NAME, PB_EDITOR_GRAPHIC_HIDE_FLAGS, new System.Type[1] { typeof(pb_MeshRenderer) });
		wireframeObject = EditorUtility.CreateGameObjectWithHideFlags(WIREFRAME_OBJECT_NAME, PB_EDITOR_GRAPHIC_HIDE_FLAGS, new System.Type[1] { typeof(pb_MeshRenderer) });	

		selectionRenderer = selectionObject.GetComponent<pb_MeshRenderer>();
		wireframeRenderer = wireframeObject.GetComponent<pb_MeshRenderer>();

		_editLevel = el;
		_selectMode = sm;

		LoadPrefs();
	}

	static internal void OnDisable()
	{
		DestroyTempObjects();
	}

	static internal void DestroyTempObjects()
	{
		DestroyObjectsWithName(PREVIEW_OBJECT_NAME);
		DestroyObjectsWithName(WIREFRAME_OBJECT_NAME);

		// renderablePool.Empty();
	}

	/**
	 * Search scene and destroy all PB rendering objects.
	 */
	static private void DestroyObjectsWithName(string InName)
	{
		GameObject go = GameObject.Find(InName);

		while(go != null)
		{
			pb_MeshRenderer mr = go.GetComponent<pb_MeshRenderer>();

			if(mr != null)
			{
				GameObject.DestroyImmediate(mr);
			}

			GameObject.DestroyImmediate(go);

			go = GameObject.Find(InName);
		}
	}

	/**
	 * Refresh the selection and wireframe mesh with _selection.
	 */
	static internal void UpdateSelectionMesh(pb_Object[] _selection, EditLevel editLevel, SelectMode selectionMode)
	{
		// Always clear the mesh whenever updating, even if selection is null.
		if(selectionObject == null || wireframeObject == null)
		{
			Init(editLevel, selectionMode);
		}

		if(_selectMode != selectionMode || _editLevel != editLevel)
		{	
			if(wireframeMaterial != null)
				GameObject.DestroyImmediate(wireframeMaterial);	
	
			_editLevel = editLevel;
			_selectMode = selectionMode;
		}

		foreach(pb_Renderable ren in selectionRenderer.renderables)
			renderablePool.Put(ren);

		selectionRenderer.renderables.Clear();

		UpdateWireframeMeshes(_selection);

		if(_selection == null || _selection.Length < 1)
		{
			return;
		}

		if(editLevel == EditLevel.Geometry && selectionMode != SelectMode.Edge)
		{
			switch( selectionMode )
			{
				case SelectMode.Vertex:

					int vcount = 0;
					foreach(pb_Object pb in _selection)
					{
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

						pb_Renderable ren = renderablePool.Get();
						ren.name = "Selection Renderable";
						ren.matrix = pb.transform.localToWorldMatrix;
						ren.materials[0] = VertexMaterial;
						ren.mesh.Clear();

						ren.mesh.vertices = t_billboards;
						ren.mesh.normals = t_nrm;
						ren.mesh.uv = t_uvs;
						ren.mesh.uv2 = t_uv2;
						ren.mesh.colors = t_col;
						ren.mesh.triangles = t_tris;
						
						selectionRenderer.renderables.Add(ren);
					}

					break;

				case SelectMode.Face:

					foreach(pb_Object pb in _selection)			
					{
						int[] selectedTriangles = pb_Face.AllTriangles(pb.SelectedFaces);

						Vector3[] 	v = pbUtil.ValuesWithIndices(pb.vertices, selectedTriangles);
						Vector2[] 	u = pbUtil.ValuesWithIndices(pb.uv, selectedTriangles);

						pb_Renderable ren = renderablePool.Get();
						ren.name = "Selection Renderable";
						ren.matrix = pb.transform.localToWorldMatrix;
						ren.materials[0] = FaceMaterial;

						ren.mesh.Clear();
						ren.mesh.vertices = v;
						ren.mesh.normals = v;
						ren.mesh.uv = u;
						ren.mesh.triangles = SequentialTriangles(v.Length);
					
						selectionRenderer.renderables.Add(ren);
					}
					break;
			}
		}
	}

	/**
	 * Create a new pb_Renderable with the wireframe material.
	 */
	static pb_Renderable CreateEditorRenderable()
	{
		pb_Renderable ren = pb_Renderable.CreateInstance(new Mesh(), (Material)null);
		ren.hideFlags = PB_EDITOR_GRAPHIC_HIDE_FLAGS;
		ren.mesh.hideFlags = PB_EDITOR_GRAPHIC_HIDE_FLAGS;
		return ren;
	}

	/**
	 * Destructor for wireframe pb_Renderables.
	 */
	static void DestroyEditorRenderable(pb_Renderable ren)
	{
		// Don't destroy material since it's used elsewhere
		ren.materials = (Material[])null;

		GameObject.DestroyImmediate(ren);
	}

	/**
	 * Generate a mesh composed of all universal edges in an array of pb_Object.
	 */
	internal static void UpdateWireframeMeshes(pb_Object[] selection)
	{
		for(int i = 0; i < wireframeRenderer.renderables.Count; i++)
			renderablePool.Put(wireframeRenderer.renderables[i]);

		wireframeRenderer.renderables.Clear();

		if( (editor == null || selection == null || editor.SelectedUniversalEdges == null) || selection.Length != editor.SelectedUniversalEdges.Length )
			return;

		try
		{
			for(int i = 0; i < selection.Length; i++)
			{
				pb_Renderable ren = renderablePool.Get();
				ren.name = "Wireframe Renderable";
				Mesh mesh = ren.mesh;
				ren.materials[0] = WireframeMaterial;
				ren.matrix = selection[i].transform.localToWorldMatrix;
				pb_Object pb = selection[i];

				Vector3[] pbverts = pb.vertices;
				pb_IntArray[] sharedIndices = pb.sharedIndices;

				// not exactly loosely coupled, but GetUniversal edges is ~40ms on a 2000 vertex object
				pb_Edge[] universalEdges = editor.SelectedUniversalEdges[i];
				Vector3[] edge_verts = new Vector3[universalEdges.Length*2];
			
				int n = 0;
				foreach(pb_Edge e in universalEdges)
				{
					edge_verts[n++] = pbverts[sharedIndices[e.x][0]];
					edge_verts[n++] = pbverts[sharedIndices[e.y][0]];
				}
				
				mesh.Clear();
				mesh.vertices = edge_verts;
				mesh.normals = edge_verts;	// appease unity 4
				mesh.uv = new Vector2[edge_verts.Length];
				mesh.subMeshCount = 1;
				mesh.SetIndices(SequentialTriangles(edge_verts.Length), MeshTopology.Lines, 0);
				mesh.hideFlags = PB_EDITOR_GRAPHIC_HIDE_FLAGS;

				wireframeRenderer.renderables.Add(ren);
			}
		}
		catch(System.Exception e)
		{
			// Don't care.
		}
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
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ProBuilder2.Common;
using ProBuilder2.Math;
using ProBuilder2.MeshOperations;

#if PB_DEBUG
using Parabox.Debug;
#endif

public class pb_DebugWindow : EditorWindow 
{
	static pb_Editor editor { get { return pb_Editor.instance; } }

	[MenuItem("Tools/ProBuilder/Debug/2 Quad Plane &p")]
	public static void TwoSidedPlane()
	{
		pb_Object pb = pb_Shape_Generator.PlaneGenerator(
				 	4,
				 	2,
				 	2,
				 	0,
				 	Axis.Up,
				 	false);

		pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);
		pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);
	}

	[MenuItem("Tools/ProBuilder/Debug/Test Function &d")]
	public static void run()
	{
		pb_Object[] sel = pbUtil.GetComponents<pb_Object>(Selection.transforms);

		if(sel.Length < 2) return;

		Mesh c = Parabox.CSG.CSG.Union(sel[0].gameObject, sel[1].gameObject);

		GameObject go = new GameObject();

		go.AddComponent<MeshRenderer>().sharedMaterial = pb_Constant.DefaultMaterial;
		go.AddComponent<MeshFilter>().sharedMesh = c;

		// Vector3[] vertices = new Vector3[] {
		// 	new Vector3(-.5f, -.5f, 0f),
		// 	new Vector3( .5f, -.5f, 0f),
		// 	new Vector3(-.5f,  .5f, 0f),
		// 	new Vector3( .5f,  .5f, 0f)
		// };

		// pb_Face[] faces = new pb_Face[] {
		// 	new pb_Face( new int[] { 0, 1, 2, 1, 3, 2 } )
		// };

		// pb_IntArray[] sharedIndices = new pb_IntArray[] { 
		// 	(pb_IntArray)new int[] { 0 },
		// 	(pb_IntArray)new int[] { 1 },
		// 	(pb_IntArray)new int[] { 2 },
		// 	(pb_IntArray)new int[] { 3 }
		// };

		// pb_Object pb = pb_Object.CreateInstanceWithElements(vertices, null, faces, sharedIndices, null);
		// // pb_Object pb = pb_Shape_Generator.PlaneGenerator(
		// //  	1,
		// //  	1,
		// //  	0,
		// //  	0,
		// //  	Axis.Up,
		// //  	false);

		// pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);
		// pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);
	}

	static void AddToList(List<int> n)
	{
		n.Add(4);
	}

	[MenuItem("Tools/ProBuilder/Debug/ProBuilder Debug Window")]
	public static void MenuSceneViewDebug()
	{
		EditorWindow.GetWindow<pb_DebugWindow>();
	}

	void OnEnable()
	{
		HookSceneViewDelegate();
	}

	private void HookSceneViewDelegate()
	{
		if(SceneView.onSceneGUIDelegate != this.OnSceneGUI)
		{
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;	// fuuuuck yooou lightmapping window
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
		}

	}

	void OnDisable()
	{
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
	}

	public bool edgeInfo = false;
	public bool faceInfo = false;
	public bool elementGroupInfo = false;
	public bool vertexInfo = true;
	public bool autoUVInfo = false;
	Vector2 scroll = Vector2.zero;

	class ParamView
	{
		public bool showObject;
		public bool showVertices;
		public bool showUv;
		public bool showUv2;
		public bool showAutoUV;

		public ParamView()
		{
			this.showObject = true;
			this.showVertices = false;
			this.showUv = false;
			this.showUv2 = false;
			this.showAutoUV = false;
		}
	}
	Hashtable showParams = new Hashtable();
	
	pb_Object[] selection = new pb_Object[0];

	void OnGUI()
	{
		selection = editor != null ? editor.selection : new pb_Object[0];

		edgeInfo = EditorGUILayout.Toggle("Edge Info", edgeInfo);
		faceInfo = EditorGUILayout.Toggle("Face Info", faceInfo);
		elementGroupInfo = EditorGUILayout.Toggle("Element Group Info", elementGroupInfo);
		vertexInfo = EditorGUILayout.Toggle("Vertex Info", vertexInfo);

		GUILayout.Label("Active Selection", EditorStyles.boldLabel);
		if(selection.Length > 0)
		{
			if(selection[0].SelectedTriangles.Length < 256)
			{
				GUILayout.Label("Faces: [" + selection[0].SelectedFaceIndices.Length + "/" + selection[0].faces.Length + "]  " + selection[0].SelectedFaceIndices.ToFormattedString(", "));
				GUILayout.Label("Edges: [" + selection[0].SelectedEdges.Length + "]  " + selection[0].SelectedEdges.ToFormattedString(", "));
				GUILayout.Label("Triangles: [" + selection[0].SelectedTriangles.Length + "]  " + selection[0].SelectedTriangles.ToFormattedString(", "));
			}
		}

		GUILayout.Space(8);

		scroll = GUILayout.BeginScrollView(scroll);

			foreach(pb_Object pb in selection)
			{
				Mesh m = pb.msh;
				Renderer ren = pb.GetComponent<MeshRenderer>();

				ParamView pv;
				int id = pb.gameObject.GetInstanceID();

				if(showParams.ContainsKey(id))
				{
					pv = (ParamView)showParams[id];
				}
				else
				{
					showParams.Add(id, new ParamView());
					pv = (ParamView)showParams[id];
				}

				pv.showObject = EditorGUILayout.Foldout(pv.showObject, pb.name + "(" + pb.id +")");
				if(pv.showObject)
				{
					/* VERTICES */			
					GUILayout.BeginHorizontal();
						GUILayout.Space(24);
						pv.showVertices = EditorGUILayout.Foldout(pv.showVertices, "Vertices: " + pb.vertexCount);
					GUILayout.EndHorizontal();
		
					GUILayout.BeginHorizontal();
					GUILayout.Space(48);
						if(pv.showVertices)
						{
							if(m == null)
							{
								GUILayout.Label("" + pb.vertices.ToFormattedString("\n"));						
							}
							else
							{
								GUILayout.BeginVertical();
								for(int i = 0; i < m.subMeshCount; i++)
								{
									GUILayout.Label("Mat: " + ren.sharedMaterials[i].name + "\n" + pb.GetVertices( m.GetTriangles(i) ).ToFormattedString("\n") + "\n");
								}
								GUILayout.EndVertical();
							}
						}
					GUILayout.EndHorizontal();
					
					/* UV  */			
					GUILayout.BeginHorizontal();
						GUILayout.Space(24);
						pv.showUv = EditorGUILayout.Foldout(pv.showUv, "UVs: " + pb.uv.Length);
					GUILayout.EndHorizontal();
		
					GUILayout.BeginHorizontal();
					GUILayout.Space(48);
						if(pv.showUv)
							GUILayout.Label("" + pb.uv.ToFormattedString("\n"));
					GUILayout.EndHorizontal();

					/* UV 2 */			
					GUILayout.BeginHorizontal();
						GUILayout.Space(24);
						pv.showUv2 = EditorGUILayout.Foldout(pv.showUv2, "UV2: " + (m ? m.uv2.Length.ToString() : "NULL"));
					GUILayout.EndHorizontal();
		
					GUILayout.BeginHorizontal();
					GUILayout.Space(48);
						if(pv.showUv2 && m != null)
							GUILayout.Label("" + m.uv2.ToFormattedString("\n"));
					GUILayout.EndHorizontal();

					/* Auto UV params */
					GUILayout.BeginHorizontal();
						GUILayout.Space(24);
						pv.showAutoUV = EditorGUILayout.Foldout(pv.showAutoUV, "Auto-UV Params");
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Space(48);
						if(pv.showAutoUV)
							GUILayout.Label("" + pb.SelectedFaces.Select(x => x.uv).ToArray().ToFormattedString("\n"));
					GUILayout.EndHorizontal();

				}
			}
		GUILayout.EndScrollView();
	}

	void OnSceneGUI(SceneView scn)
	{
		foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
			DrawStats(pb);

		Repaint();
	}

	void DrawStats(pb_Object pb)
	{
		StringBuilder sb = new StringBuilder();

		Handles.BeginGUI();

		if(edgeInfo)
		foreach(pb_Edge f in pb.SelectedEdges)
		{
			Vector2 cen = HandleUtility.WorldToGUIPoint( pb.transform.TransformPoint((pb.vertices[f.x] + pb.vertices[f.y])/ 2f) );
			GUI.Box(new Rect(cen.x, cen.y, 60, 40), f.ToString() + "\n");
			// GUI.Label(new Rect(cen.x, cen.y, 200, 200), f.ToString());
		}

		/**
		 * SHARED INDICES
		 */
		// foreach(pb_IntArray arr in pb.sharedIndices)
		// {
		// 	Vector2 cen = HandleUtility.WorldToGUIPoint( pb.transform.TransformPoint(pb.vertices[arr[0]]) );
						
		// 	GUI.Label(new Rect(cen.x, cen.y, 200, 200), ((int[])arr).ToFormattedString("\n"));
		// }

		if(faceInfo)
		foreach(pb_Face f in pb.SelectedFaces)
		{
			Vector2 cen = HandleUtility.WorldToGUIPoint( pb.transform.TransformPoint( pb_Math.Average( pb.GetVertices(f.distinctIndices) ) ) );
			
				GUI.Box( new Rect(cen.x, cen.y, 300, 100), "Face: " + f.ToString() + "Element Group: " + f.elementGroup);			

		}

		if(elementGroupInfo)
		foreach(pb_Face f in pb.faces)
		{
			Vector2 cen = HandleUtility.WorldToGUIPoint( pb.transform.TransformPoint( pb_Math.Average( pb.GetVertices(f.distinctIndices) ) ) );
			GUI.Label( new Rect(cen.x, cen.y, 300, 100), f.elementGroup.ToString(), EditorStyles.boldLabel);			
		}

			// sb.AppendLine(f.ToString() + ", ");


		// foreach(pb_Face face in pb.SelectedFaces)
		// 	sb.AppendLine(face.colors.ToFormattedString("\n") + "\n");

		// sb.AppendLine("\n");

		// foreach(pb_IntArray si in pb.sharedIndices)
		// {
		// 	sb.AppendLine(si.array.ToFormattedString(", "));
		// }

		// sb.AppendLine("\n");

		if(vertexInfo)
		{
			foreach(pb_IntArray arr in pb.sharedIndices)
			{
				Vector3 v = pb.vertices[arr[0]];

				Vector2 cen = HandleUtility.WorldToGUIPoint( pb.transform.TransformPoint( v ) );
				GUI.Label(new Rect(cen.x, cen.y, 500, 64), arr.array.ToFormattedString(", "), EditorStyles.boldLabel);
			}
		}

		Handles.EndGUI();

		Handles.BeginGUI();
		{
			GUI.Label(new Rect(10, 10, 400, 800), sb.ToString());
		}
		Handles.EndGUI();
	}

	[MenuItem("Tools/ProBuilder/Debug/Make All Scene Objects Editable")]
	public static void clearflags()
	{
		foreach(GameObject go in FindObjectsOfType(typeof(GameObject))) {
			
			go.hideFlags = (HideFlags)0;
		}
	}	

	[MenuItem("Tools/ProBuilder/Debug/2.5k cubes")]
	public static void Cubeit()
	{
		int rows = 50, columns = 50;
		for(int n = 0; n < rows; n++)
		{
			for(int i = 0; i < columns; i++)
			{
				if(EditorUtility.DisplayCancelableProgressBar(
					"2.5k Cubes",
					"Cube " + ((n*((float)rows))+i),
					((float)((n*rows)+i))/(rows*columns)))
				{
					EditorUtility.ClearProgressBar();
					return;
				}

				// GameObject pb = GameObject.CreatePrimitive(PrimitiveType.Cube);
				// pb.GetComponent<MeshRenderer>().sharedMaterial = pb_Constant.DefaultMaterial;
				// pb.AddComponent<BoxCollider>();
				pb_Object pb = ProBuilder.CreatePrimitive(Shape.Cube);
				pb_Editor_Utility.InitObjectFlags(pb, ColliderType.BoxCollider, pb.GetComponent<pb_Entity>().entityType);
				
				pb.transform.position = new Vector3( (i*2f) - (columns/2f), 0f, (n*2) - (rows/2f) );
			}
		}

		EditorUtility.ClearProgressBar();
	}

	[MenuItem("Tools/ProBuilder/Debug/Dump Object Info (Detailed)")]
	public static void Dump()
	{
		foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
		{
			#if PB_DEBUG
			Bugger.Log(pb.ToStringDetailed());
			#else
			Debug.Log(pb.ToStringDetailed());
			#endif
		}
	}

	[MenuItem("Tools/ProBuilder/Debug/Mesh Information (Actual Mesh - not pb_Object) %#m")]
	public static void MenuDumpMeshInfo()
	{
		foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
		{
			Mesh m = pb.msh;
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(m.name);
			sb.AppendLine("Vertex Count:" + m.vertices.Length);
			sb.AppendLine("Submesh Count:" + m.subMeshCount);
			sb.AppendLine("Triangle Count:" + m.triangles.Length);
			sb.AppendLine("Normal Count:" + m.normals.Length);
		//	Bugger.Log(m.uv.ToFormattedString("\n"));
			Debug.Log(sb.ToString());
		}
	}

	[MenuItem("Tools/ProBuilder/Debug/Dump Selected faces")]
	public static void Dumpfaces()
	{
		foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
		{
			foreach(pb_Face face in pb.SelectedFaces)
			#if PB_DEBUG
				Bugger.Log(face.ToString());
			#else
				Debug.Log(face.ToString());
			#endif
		}
	}
}
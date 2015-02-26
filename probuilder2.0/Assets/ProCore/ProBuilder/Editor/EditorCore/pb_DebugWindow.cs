using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ProBuilder2.Common;
using ProBuilder2.Math;
using ProBuilder2.MeshOperations;
using ProBuilder2.GUI;

/**
 * Debugging menu items for ProBuilder.
 */
public class pb_DebugWindow : EditorWindow 
{
	float elementLength = .15f;
	float elementOffset = .01f;

	static readonly Color SceneLabelBackgroundColor = new Color(.12f, .12f, .12f, 1f);

	static pb_Editor editor { get { return pb_Editor.instance; } }

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
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
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
	public bool textureGroupInfo = false;
	public bool vertexInfo = true;
	public bool autoUVInfo = false;
	Vector2 scroll = Vector2.zero;
	public bool ntbSelectedOnly = false;

	class ParamView
	{
		public bool showObject;
		public bool showVertices;
		public bool showColors;
		public bool showUv;
		public bool showUv2;
		public bool showAutoUV;
		public bool showSharedUV;
		public bool showSharedTris;

		public ParamView()
		{
			this.showObject = true;
			this.showVertices = false;
			this.showColors = false;
			this.showUv = false;
			this.showUv2 = false;
			this.showAutoUV = false;
			this.showSharedUV = false;
			this.showSharedTris = false;
		}
	}
	Hashtable showParams = new Hashtable();
	
	pb_Object[] selection = new pb_Object[0];

	void OnGUI()
	{
		selection = editor != null ? editor.selection : new pb_Object[0];

		EditorGUI.BeginChangeCheck();
			edgeInfo = EditorGUILayout.Toggle("Edge Info", edgeInfo);
			faceInfo = EditorGUILayout.Toggle("Face Info", faceInfo);
			elementGroupInfo = EditorGUILayout.Toggle("Element Group Info", elementGroupInfo);
			textureGroupInfo = EditorGUILayout.Toggle("Texture Group Info", textureGroupInfo);
			vertexInfo = EditorGUILayout.Toggle("Vertex Info", vertexInfo);

			GUILayout.BeginHorizontal();
				Color pop = GUI.color;
				GUI.color = Color.green;
				GUILayout.Label("Normals", EditorStyles.boldLabel);
				GUI.color = pop;
				GUILayout.Label(" / ", EditorStyles.boldLabel);
				GUI.color = Color.red;
				GUILayout.Label("Tangents", EditorStyles.boldLabel);
				GUI.color = pop;
				GUILayout.Label(" / ", EditorStyles.boldLabel);
				GUI.color = Color.blue;
				GUILayout.Label("Bitangents", EditorStyles.boldLabel);
				GUI.color = pop;

				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			elementLength = EditorGUILayout.Slider("Line Length", elementLength, 0f, 1f);
			elementOffset = EditorGUILayout.Slider("Vertex Offset", elementOffset, 0f, .1f);
			ntbSelectedOnly = EditorGUILayout.Toggle("Selection Only Selected", ntbSelectedOnly);

		if(EditorGUI.EndChangeCheck())
		{
			SceneView.RepaintAll();
		}


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
					{
						GUILayout.BeginHorizontal();
							GUILayout.Space(24);
							pv.showVertices = EditorGUILayout.Foldout(pv.showVertices, "Vertices: " + pb.vertexCount + " / " + pb.msh.vertexCount);
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
					}
					
					/* Colors */			
					{
						GUILayout.BeginHorizontal();
							GUILayout.Space(24);
							pv.showColors = EditorGUILayout.Foldout(pv.showColors, "colors: " + (pb.colors != null ? pb.colors.Length : 0).ToString());
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						GUILayout.Space(48);
							if(pv.showColors)
							{
								GUILayout.Label("" + pb.colors.ToFormattedString("\n"));						
							}
						GUILayout.EndHorizontal();
					}
					
					/* UV  */	
					{		
						GUILayout.BeginHorizontal();
							GUILayout.Space(24);
							pv.showUv = EditorGUILayout.Foldout(pv.showUv, "UVs: " + pb.uv.Length);
						GUILayout.EndHorizontal();
			
						GUILayout.BeginHorizontal();
						GUILayout.Space(48);
							if(pv.showUv)
								GUILayout.Label("" + pb.uv.ToFormattedString("\n"));
						GUILayout.EndHorizontal();
					}

					/* UV 2 */			
					{
						GUILayout.BeginHorizontal();
							GUILayout.Space(24);
							pv.showUv2 = EditorGUILayout.Foldout(pv.showUv2, "UV2: " + (m ? m.uv2.Length.ToString() : "NULL"));
						GUILayout.EndHorizontal();
			
						GUILayout.BeginHorizontal();
						GUILayout.Space(48);
							if(pv.showUv2 && m != null)
								GUILayout.Label("" + m.uv2.ToFormattedString("\n"));
						GUILayout.EndHorizontal();
					}

					/* Auto UV params */
					{
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

					/* Shared UVs */
					{
						GUILayout.BeginHorizontal();
							GUILayout.Space(24);
							pv.showSharedUV = EditorGUILayout.Foldout(pv.showSharedUV, "Shared UV");
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						GUILayout.Space(48);
							if(pv.showSharedUV)
							{
								GUILayout.BeginVertical();
								for(int i = 0; i < pb.sharedIndicesUV.Length; i++)
								{
									if(GUILayout.Button("" + pb.sharedIndicesUV[i].array.ToFormattedString(", "), EditorStyles.label))
									{
										pb.SetSelectedTriangles(pb.sharedIndicesUV[i]);

										if(pb_Editor.instance)
										{
											pb_Editor.instance.UpdateSelection();
											SceneView.RepaintAll();
										}
									}
								}
								GUILayout.EndVertical();
								// GUILayout.Label("" + pb.sharedIndicesUV.ToFormattedString("\n"));
							}
						GUILayout.EndHorizontal();
					}

					/* Shared Triangle */
					{
						GUILayout.BeginHorizontal();
							GUILayout.Space(24);
							pv.showSharedTris = EditorGUILayout.Foldout(pv.showSharedTris, "Shared Indices");
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						GUILayout.Space(48);
							if(pv.showSharedTris)
								GUILayout.Label("" + pb.sharedIndices.ToFormattedString("\n"));
						GUILayout.EndHorizontal();
					}
				}
			}
		GUILayout.EndScrollView();
	}

	void OnSceneGUI(SceneView scn)
	{
		foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
		{
			DrawStats(pb);

			if(elementLength > 0.01f)
				DrawElements(pb);
		}

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
			GUIContent gc = new GUIContent(f.ToString(), "");
			DrawSceneLabel(gc, cen);
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
			
			GUIContent gc = new GUIContent("Face: " + f.ToString() + "\nElement Group: " + f.elementGroup, "");

			DrawSceneLabel(gc, cen);
		}

		if(elementGroupInfo || textureGroupInfo)
		{
			foreach(pb_Face f in pb.faces)
			{
				Vector3 v = pb_Math.Average( pb.GetVertices(f.distinctIndices) );
				v += pb_Math.Normal(pb, f) * .01f;
				v = pb.transform.TransformPoint(v);

				if(!PointIsOccluded(v))
				{
					Vector2 cen = HandleUtility.WorldToGUIPoint( v );
					GUIContent gc;
					
					if( elementGroupInfo && textureGroupInfo)
						gc = new GUIContent("E: " + f.elementGroup + "\nT: " + f.textureGroup, "");
					else if(elementGroupInfo)
						gc = new GUIContent("E: " + f.elementGroup, "");
					else
						gc = new GUIContent("T: " + f.textureGroup, "");

					DrawSceneLabel(gc, cen);
				}
			}
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
			Vector3[] normals = pb.msh.normals;
			foreach(pb_IntArray arr in pb.sharedIndices)
			{
				Vector3 v = pb.transform.TransformPoint(pb.vertices[arr[0]] + normals[arr[0]] * .01f);

				if(!PointIsOccluded(v))
				{
					Vector2 cen = HandleUtility.WorldToGUIPoint( v );
					GUIContent gc = new GUIContent(arr.array.ToFormattedString(", "), "");

					DrawSceneLabel(gc, cen);
				}
			}
		}

		Handles.EndGUI();

		Handles.BeginGUI();
		{
			GUI.Label(new Rect(10, 10, 400, 800), sb.ToString());
		}
		Handles.EndGUI();
	}

	void DrawSceneLabel(GUIContent content, Vector2 position)
	{
		float width = EditorStyles.boldLabel.CalcSize(content).x;
		float height = EditorStyles.label.CalcHeight(content, width) + 4;

		pb_GUI_Utility.DrawSolidColor( new Rect(position.x, position.y, width, height), SceneLabelBackgroundColor);
		GUI.Label( new Rect(position.x, position.y, width, height), content, EditorStyles.boldLabel );
	}

	bool PointIsOccluded(Vector3 worldPoint)
	{
		Camera cam = SceneView.lastActiveSceneView.camera;

		Ray ray = new Ray(worldPoint, (cam.transform.position - worldPoint).normalized * 1000f );

		pb_RaycastHit hit;

		foreach(pb_Object pb in selection)
		{
			if( pb_Handle_Utility.MeshRaycast(ray, pb, out hit, false) )
			{
				return true;
			}
		}

		return false;

	//	pb_Handle.MeshRaycast(Ray InWorldRay, pb_Object pb, out pb_RaycastHit hit, bool ignoreBackfaces)

	}

	/**
	 * Draw the normals, tangents, and bitangets associated with this mesh.
	 * Green = normals
	 * Blue = tangents
	 * Red = bitangents
	 */
	void DrawElements(pb_Object pb)
	{
		if( ntbSelectedOnly && pb.vertexCount != pb.msh.vertices.Length )
			return;

		int vertexCount = ntbSelectedOnly ? pb.SelectedTriangleCount : pb.msh.vertexCount;

		Vector3[] vertices = ntbSelectedOnly ?  pbUtil.ValuesWithIndices<Vector3>(pb.msh.vertices, pb.SelectedTriangles) : pb.msh.vertices;
		Vector3[] normals  = ntbSelectedOnly ?  pbUtil.ValuesWithIndices<Vector3>(pb.msh.normals, pb.SelectedTriangles) : pb.msh.normals;
		Vector4[] tangents = ntbSelectedOnly ?  pbUtil.ValuesWithIndices<Vector4>(pb.msh.tangents, pb.SelectedTriangles) : pb.msh.tangents;

		Handles.matrix = pb.transform.localToWorldMatrix;

		Handles.color = Color.green;
		for(int i = 0; i < vertexCount; i++)
			Handles.DrawLine( (vertices[i] + normals[i] * elementOffset),  (vertices[i] + normals[i] * elementOffset) + normals[i] * elementLength);

		Handles.color = Color.blue;
		for(int i = 0; i < vertexCount; i++)
			Handles.DrawLine( (vertices[i] + normals[i] * elementOffset),  (vertices[i] + normals[i] * elementOffset) + (Vector3)tangents[i] * elementLength);

		Handles.color = Color.red;
		for(int i = 0; i < vertexCount; i++)
			Handles.DrawLine( (vertices[i] + normals[i] * elementOffset),  (vertices[i] + normals[i] * elementOffset) + (Vector3.Cross(normals[i], (Vector3)tangents[i]) * tangents[i].w) * elementLength);

		Handles.matrix = Matrix4x4.identity;
	}
}
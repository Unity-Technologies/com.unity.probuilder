#pragma warning disable 0168

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ProBuilder2.Common;
using ProBuilder2.Interface;
using ProBuilder2.MeshOperations;

namespace ProBuilder2.EditorCommon
{
	/**
	 * Debugging menu items for ProBuilder.
	 */
	public class pb_Debug_Window : EditorWindow 
	{
		float elementLength = .15f;
		float elementOffset = .01f;

		static readonly Color SceneLabelBackgroundColor = new Color(.12f, .12f, .12f, 1f);

		static pb_Editor editor { get { return pb_Editor.instance; } }

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Debug/ProBuilder Debug Window", false, pb_Constant.MENU_REPAIR)]
		public static void MenuSceneViewDebug()
		{
			EditorWindow.GetWindow<pb_Debug_Window>();
		}

		void OnEnable()
		{
			HookSceneViewDelegate();
		}

		private void HookSceneViewDelegate()
		{
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;

			pb_Editor.OnSelectionUpdate += OnSelectionUpdate;
			pb_Editor.OnVertexMovementFinish += OnVertexMovementFinish;
		}

		void OnDisable()
		{
			if(pb_LineRenderer.Valid())
				pb_LineRenderer.instance.Clear();

			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			pb_Editor.OnSelectionUpdate -= OnSelectionUpdate;
			pb_Editor.OnVertexMovementFinish -= OnVertexMovementFinish;
		}

		void OnSelectionUpdate(pb_Object[] selection)
		{
			try
			{
				foreach(pb_Object pb in selection)
					DrawElements(pb);
			} catch {}
		}

		void OnVertexMovementFinish(pb_Object[] selection)
		{
			foreach(pb_Object pb in selection)
				DrawElements(pb);
		}

		public bool edgeInfo = false;
		public bool faceInfo = false;
		public bool elementGroupInfo = false;
		public bool textureGroupInfo = false;
		public bool smoothingGroupInfo = false;
		// public bool vertexInfo = false;
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
			public bool showTriangles;

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
				this.showTriangles = false;
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

				GUI.enabled = faceInfo;
				{
					elementGroupInfo = EditorGUILayout.Toggle("Element Group Info", elementGroupInfo);
					textureGroupInfo = EditorGUILayout.Toggle("Texture Group Info", textureGroupInfo);
					smoothingGroupInfo = EditorGUILayout.Toggle("Smoothing Group Info", smoothingGroupInfo);
				}
				GUI.enabled = true;

				// vertexInfo = EditorGUILayout.Toggle("Vertex Info", vertexInfo);

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
				ntbSelectedOnly = EditorGUILayout.Toggle("Selection Only", ntbSelectedOnly);

			if(EditorGUI.EndChangeCheck())
			{
				foreach(pb_Object pb in selection)
					DrawElements(pb);

				SceneView.RepaintAll();
			}


			GUILayout.Label("Active Selection", EditorStyles.boldLabel);
			if(selection.Length > 0)
			{
				if(selection[0].SelectedTriangles.Length < 256)
				{
					GUILayout.Label("Faces: [" + selection[0].SelectedFaceIndices.Length + "/" + selection[0].faces.Length + "]  " + selection[0].SelectedFaceIndices.ToString(", "));
					GUILayout.Label("Edges: [" + selection[0].SelectedEdges.Length + "]  " + selection[0].SelectedEdges.ToString(", "));
					GUILayout.Label("Triangles: [" + selection[0].SelectedTriangles.Length + "]  " + selection[0].SelectedTriangles.ToString(", "));
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
										GUILayout.Label("" + pb.vertices.ToString("\n"));						
									}
									else
									{
										GUILayout.BeginVertical();
										for(int i = 0; i < m.subMeshCount; i++)
										{
											GUILayout.Label("Mat: " + ren.sharedMaterials[i].name + "\n" + pb.vertices.ValuesWithIndices( m.GetTriangles(i) ).ToString("\n") + "\n");
										}
										GUILayout.EndVertical();
									}
								}
							GUILayout.EndHorizontal();						
						}

						/* Triangles */			
						{
							GUILayout.BeginHorizontal();
								GUILayout.Space(24);
								pv.showTriangles = EditorGUILayout.Foldout(pv.showTriangles, "Triangles: " + pb.msh.triangles.Length);
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal();
							GUILayout.Space(48);
								if(pv.showTriangles)
								{
									if(m == null)
									{
										GUILayout.Label("Faces: " + pb.faces.Length);
									}
									else
									{
										GUILayout.BeginVertical();
										for(int i = 0; i < m.subMeshCount; i++)
										{
											int[] tris = pb.msh.GetTriangles(i);
											GUILayout.Label("Mat: " + ren.sharedMaterials[i].name + " : " + tris.Length);
		
											GUILayout.BeginHorizontal();
												GUILayout.Space(16);
		
												GUILayout.BeginVertical();
													if(tris.Length > 256)
													{
														int[] dup = new int[256];
														System.Array.Copy(tris, 0, dup, 0, 256);
														GUILayout.Label( dup.ToString("\n") + "\n" );
													}
													else
													{
														GUILayout.Label( tris.ToString("\n") + "\n" );
													}
												GUILayout.EndVertical();
											GUILayout.EndHorizontal();

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
									GUILayout.Label("" + pb.colors.ToString("\n"));						
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
									GUILayout.Label("" + pb.uv.ToString("\n"));
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
									GUILayout.Label("" + m.uv2.ToString("\n"));
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
									GUILayout.Label("" + pb.SelectedFaces.Select(x => x.uv).ToArray().ToString("\n"));
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
										if(GUILayout.Button("" + pb.sharedIndicesUV[i].array.ToString(", "), EditorStyles.label))
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
									// GUILayout.Label("" + pb.sharedIndicesUV.ToString("\n"));
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
									GUILayout.Label("" + pb.sharedIndices.ToString("\n"));
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
							
			// 	GUI.Label(new Rect(cen.x, cen.y, 200, 200), ((int[])arr).ToString("\n"));
			// }

			if(faceInfo)
			foreach(pb_Face f in pb.SelectedFaces)
			{
				Vector2 cen = HandleUtility.WorldToGUIPoint( pb.transform.TransformPoint( pb_Math.Average( pb.vertices.ValuesWithIndices(f.distinctIndices) ) ) );
				
				GUIContent gc = new GUIContent("Face: " + f.ToString(), "");

				if(smoothingGroupInfo || elementGroupInfo || textureGroupInfo)
					gc.text += "\nGroups:";

				if(smoothingGroupInfo)
					gc.text += "\nSmoothing: " + f.smoothingGroup;
				if(elementGroupInfo)
					gc.text += "\nElement: " + f.elementGroup;
				if(textureGroupInfo)
					gc.text += "\nTexture: " + f.textureGroup;

				DrawSceneLabel(gc, cen);
			}

			// sb.AppendLine(f.ToString() + ", ");


			// foreach(pb_Face face in pb.SelectedFaces)
			// 	sb.AppendLine(face.colors.ToString("\n") + "\n");

			// sb.AppendLine("\n");

			// foreach(pb_IntArray si in pb.sharedIndices)
			// {
			// 	sb.AppendLine(si.array.ToString(", "));
			// }

			// sb.AppendLine("\n");

			// if(vertexInfo)
			// {
			// 	try
			// 	{
			// 		Camera cam = SceneView.lastActiveSceneView.camera;
			// 		Vector3[] normals = pb.msh.normals;
			// 		int index = 0;
			// 		foreach(pb_IntArray arr in pb.sharedIndices)
			// 		{
			// 			Vector3 v = pb.transform.TransformPoint(pb.vertices[arr[0]] + normals[arr[0]] * .01f);

			// 			if(!pb_HandleUtility.PointIsOccluded(cam, pb, v))
			// 			{
			// 				Vector2 cen = HandleUtility.WorldToGUIPoint( v );
							
			// 				GUIContent gc = new GUIContent(index++ + ": " + arr.array.ToString(", "), "");

			// 				DrawSceneLabel(gc, cen);
			// 			}
			// 		}
			// 	} catch { /* do not care */; }
			// }

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

		readonly Color[] ElementColors = new Color[] { Color.green, Color.blue, Color.red };

		/**
		 * Draw the normals, tangents, and bitangets associated with this mesh.
		 * Green = normals
		 * Blue = tangents
		 * Red = bitangents
		 */
		void DrawElements(pb_Object pb)
		{
			pb_LineRenderer.instance.Clear();

			if( ntbSelectedOnly && pb.vertexCount != pb.msh.vertices.Length || elementLength <= 0f)
				return;

			int vertexCount = ntbSelectedOnly ? pb.SelectedTriangleCount : pb.msh.vertexCount;

			Vector3[] vertices = ntbSelectedOnly ?  pbUtil.ValuesWithIndices<Vector3>(pb.msh.vertices, pb.SelectedTriangles) : pb.msh.vertices;
			Vector3[] normals  = ntbSelectedOnly ?  pbUtil.ValuesWithIndices<Vector3>(pb.msh.normals, pb.SelectedTriangles) : pb.msh.normals;
			Vector4[] tangents = ntbSelectedOnly ?  pbUtil.ValuesWithIndices<Vector4>(pb.msh.tangents, pb.SelectedTriangles) : pb.msh.tangents;

			Matrix4x4 matrix = pb.transform.localToWorldMatrix;

			Vector3[] segments = new Vector3[vertexCount * 3 * 2];

			int n = 0;
			Vector3 pivot = Vector3.zero;

			for(int i = 0; i < vertexCount; i++)
			{
				pivot = vertices[i] + normals[i] * elementOffset;

				segments[n+0] = matrix.MultiplyPoint3x4( pivot );
				segments[n+1] = matrix.MultiplyPoint3x4( (pivot + normals[i] * elementLength) );

				segments[n+2] = segments[n];
				segments[n+3] = matrix.MultiplyPoint3x4( (pivot + (Vector3)tangents[i] * elementLength) );

				segments[n+4] = segments[n];
				segments[n+5] = matrix.MultiplyPoint3x4( (pivot + (Vector3.Cross(normals[i], (Vector3)tangents[i]) * tangents[i].w) * elementLength) );

				n += 6;
			}

			pb_LineRenderer.instance.AddLineSegments(segments, ElementColors);
		}
	}
}

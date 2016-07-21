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
	public class pb_DebugWindow : EditorWindow
	{
		float elementLength = .15f;
		float elementOffset = .01f;

		static readonly Color SceneLabelBackgroundColor = new Color(.12f, .12f, .12f, 1f);
		static readonly Color SplitterColor = new Color(.3f, .3f, .3f, .75f);

		GUIStyle boldLabel {
			get {
				if(_boldLabel == null) {
					_boldLabel = new GUIStyle(EditorStyles.boldLabel);
					_boldLabel.normal.textColor = Color.white;
				}
				return _boldLabel;
			}
		}

		GUIStyle _boldLabel = null;

		static pb_Editor editor { get { return pb_Editor.instance; } }

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Debug/ProBuilder Debug Window", false, pb_Constant.MENU_REPAIR)]
		public static void MenuSceneViewDebug()
		{
			EditorWindow.GetWindow<pb_DebugWindow>();
		}

		void OnEnable()
		{
			elementLength = EditorPrefs.GetFloat("pb_Debug_elementLength", .1f);
			elementOffset = EditorPrefs.GetFloat("pb_Debug_elementOffset", .01f);
			testOcclusion = EditorPrefs.GetBool("pb_Debug_testOcclusion", false);

			HookSceneViewDelegate();
		}

		private void HookSceneViewDelegate()
		{
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;

			pb_Editor.OnSelectionUpdate += OnSelectionUpdate;
			pb_Editor.OnVertexMovementFinish += OnSelectionUpdate;
		}

		void OnDisable()
		{
			if(pb_LineRenderer.Valid())
				pb_LineRenderer.instance.Clear();

			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			pb_Editor.OnSelectionUpdate -= OnSelectionUpdate;
			pb_Editor.OnVertexMovementFinish -= OnSelectionUpdate;
		}

		void OnSelectionUpdate(pb_Object[] selection)
		{
			try
			{
				pb_LineRenderer.instance.Clear();

				foreach(pb_Object pb in selection)
					DrawElements(pb);

			} catch {}
		}

		public bool edgeInfo = false;
		public IndexFormat edgeIndexFormat = IndexFormat.Local;
		public bool faceInfo = false;
		public IndexFormat faceIndexFormat = IndexFormat.Local;
		public bool triInfo = false;
		public IndexFormat triIndexFormat = IndexFormat.Local;
		public bool elementGroupInfo = false;
		public bool textureGroupInfo = false;
		public bool smoothingGroupInfo = false;
		public bool autoUVInfo = false;
		Vector2 scroll = Vector2.zero;
		public bool selectedOnly = false;
		public bool testOcclusion = false;

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

		Dictionary<int, ParamView> showParams = new Dictionary<int, ParamView>();
		pb_Object[] selection = new pb_Object[0];

		void OnGUI()
		{
			selection = editor != null ? editor.selection : new pb_Object[0];

			scroll = GUILayout.BeginScrollView(scroll);

			EditorGUI.BeginChangeCheck();

			triInfo = EditorGUILayout.BeginToggleGroup("Index Info", triInfo);
			if(triInfo)
			{
				triIndexFormat = (IndexFormat) EditorGUILayout.EnumPopup(triIndexFormat);
			}
			EditorGUILayout.EndToggleGroup();

			edgeInfo = EditorGUILayout.BeginToggleGroup("Edge Info", edgeInfo);
			if(edgeInfo)
			{
				edgeIndexFormat = (IndexFormat) EditorGUILayout.EnumPopup(edgeIndexFormat);
			}
			EditorGUILayout.EndToggleGroup();

			faceInfo = EditorGUILayout.BeginToggleGroup("Face Info", faceInfo);
			if(faceInfo)
			{
				faceIndexFormat = (IndexFormat) EditorGUILayout.EnumPopup(faceIndexFormat);
				elementGroupInfo = EditorGUILayout.Toggle("Element Group Info", elementGroupInfo);
				textureGroupInfo = EditorGUILayout.Toggle("Texture Group Info", textureGroupInfo);
				smoothingGroupInfo = EditorGUILayout.Toggle("Smoothing Group Info", smoothingGroupInfo);
			}
			EditorGUILayout.EndToggleGroup();

			GUILayout.Label("Scene Label Settings", EditorStyles.boldLabel);

			selectedOnly = EditorGUILayout.Toggle("Selection Only", selectedOnly);
			testOcclusion = EditorGUILayout.Toggle("Depth Test", testOcclusion);

			GUILayout.Space(4);

			pb_GUI_Utility.DrawSeparator(1, SplitterColor);

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

			if(EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetFloat("pb_Debug_elementLength", elementLength);
				EditorPrefs.SetFloat("pb_Debug_elementOffset", elementOffset);
				EditorPrefs.SetBool("pb_Debug_testOcclusion", testOcclusion);

				foreach(pb_Object pb in selection)
					DrawElements(pb);

				SceneView.RepaintAll();
			}

			GUILayout.Space(8);
			pb_GUI_Utility.DrawSeparator(1, SplitterColor);

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

			foreach(pb_Object pb in selection)
			{
				Mesh m = pb.msh;
				Renderer ren = pb.GetComponent<MeshRenderer>();

				ParamView pv;
				int id = pb.gameObject.GetInstanceID();

				if(!showParams.TryGetValue(id, out pv))
				{
					pv = new ParamView();
					showParams.Add(id, pv);
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

											StringBuilder sb = new StringBuilder();

											for(int n = 0; n < tris.Length && n < 300; n+=3)
												sb.AppendLine(string.Format("{0}, {1}, {2}", tris[n], tris[n+1], tris[n+2]));

											GUILayout.Label(sb.ToString());

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
			Handles.BeginGUI();

			if(edgeInfo)
				DrawEdgeInfo(pb);

			if(triInfo)
				DrawTriangleInfo(pb);

			Handles.EndGUI();

			if(faceInfo)
				DrawFaceInfo(pb);
		}

		void DrawTriangleInfo(pb_Object pb)
		{
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			Dictionary<int, int> lookup = sharedIndices.ToDictionary();
			Vector3[] vertices = pb.vertices;

			HashSet<int> common = new HashSet<int>();

			if(selectedOnly)
			{
				foreach(int i in pb.SelectedTriangles)
					common.Add(lookup[i]);
			}
			else
			{
				for(int i = 0; i < sharedIndices.Length; i++)
					common.Add(i);
			}

			foreach(int i in common)
			{
				int[] indices = sharedIndices[i];
				Vector2 cen = HandleUtility.WorldToGUIPoint( pb.transform.TransformPoint(vertices[indices[0]]) );

				StringBuilder sb = new StringBuilder();

				if(triIndexFormat == IndexFormat.Common || triIndexFormat == IndexFormat.Both)
					sb.Append(i);

				if(triIndexFormat == IndexFormat.Both)
					sb.Append(": ");

				if(triIndexFormat == IndexFormat.Local || triIndexFormat == IndexFormat.Both)
				{

					sb.Append(indices[0].ToString());

					for(int n = 1; n < indices.Length; n++)
					{
						sb.Append(", ");
						sb.Append(indices[n].ToString());
					}
				}

				DrawSceneLabel(sb.ToString(), cen);
			}
		}

		void DrawEdgeInfo(pb_Object pb)
		{
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			pb_Edge[] source = selectedOnly ? pb.SelectedEdges : pb.faces.SelectMany(x => x.edges).ToArray();
			IEnumerable<pb_EdgeLookup> edges = pb_EdgeLookup.GetEdgeLookup(source, lookup);

			foreach(pb_EdgeLookup edge in edges)
			{
				Vector2 cen = HandleUtility.WorldToGUIPoint( pb.transform.TransformPoint((pb.vertices[edge.local.x] + pb.vertices[edge.local.y])/ 2f) );

				switch(edgeIndexFormat)
				{
					case IndexFormat.Both:
						DrawSceneLabel(string.Format("common: [{0}, {1}]", edge.common.x, edge.common.y), cen);
						break;
					case IndexFormat.Local:
						DrawSceneLabel(string.Format("local: [{0}, {1}]", edge.local.x, edge.local.y), cen);
						break;
					case IndexFormat.Common:
						DrawSceneLabel(string.Format("local: [{0}, {1}]\ncommon: [{0}, {1}]", edge.local.x, edge.local.y, edge.common.x, edge.common.y), cen);
						break;
				}
			}
		}

		void DrawFaceInfo(pb_Object pb)
		{
			pb_Face[] faces = selectedOnly ? pb.SelectedFaces : pb.faces;
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			Camera cam = SceneView.lastActiveSceneView.camera;

			foreach(pb_Face f in faces)
			{
				Vector3 point = pb.transform.TransformPoint( pb_Math.Average(pb.vertices, f.distinctIndices) );

				if( testOcclusion && pb_HandleUtility.PointIsOccluded(cam, pb, point) )
					continue;

				Vector3 normal = pb.transform.TransformDirection( pb_Math.Normal(pb, f) );

				StringBuilder sb = new StringBuilder();

				if(faceIndexFormat == IndexFormat.Local || faceIndexFormat == IndexFormat.Both)
				{
					if(faceIndexFormat == IndexFormat.Both) sb.Append("local: ");

					for(int i = 0; i < f.indices.Length; i+=3)
					{
						sb.Append("[");
						sb.Append(f.indices[i+0]);
						sb.Append(", ");
						sb.Append(f.indices[i+1]);
						sb.Append(", ");
						sb.Append(f.indices[i+2]);
						sb.Append("] ");
					}
				}

				if(faceIndexFormat == IndexFormat.Both)
					sb.AppendLine("");

				if(faceIndexFormat == IndexFormat.Common || faceIndexFormat == IndexFormat.Both)
				{
					if(faceIndexFormat == IndexFormat.Both) sb.Append("common: ");

					for(int i = 0; i < f.indices.Length; i+=3)
					{
						sb.Append("[");
						sb.Append(lookup[f.indices[i+0]]);
						sb.Append(", ");
						sb.Append(lookup[f.indices[i+1]]);
						sb.Append(", ");
						sb.Append(lookup[f.indices[i+2]]);
						sb.Append("] ");
					}
				}

				if(smoothingGroupInfo || elementGroupInfo || textureGroupInfo)
					sb.AppendLine("");

				if(smoothingGroupInfo)
				{
					sb.Append("Smoothing: ");
					sb.Append(f.smoothingGroup.ToString());
					if(elementGroupInfo || textureGroupInfo) sb.AppendLine("");
				}

				if(elementGroupInfo)
				{
					sb.Append("Element: ");
					sb.Append(f.elementGroup.ToString());
					if(textureGroupInfo) sb.AppendLine("");
				}

				if(textureGroupInfo)
				{
					sb.Append("Texture: ");
					sb.Append(f.textureGroup.ToString());
				}

				Vector3 labelPos = point + (normal.normalized + cam.transform.up.normalized) * .2f;

				Handles.DrawLine(point, labelPos);

				Vector2 cen = HandleUtility.WorldToGUIPoint(labelPos);
				cen.y -= 5f;

				Handles.BeginGUI();
				DrawSceneLabel(sb.ToString(), cen);
				Handles.EndGUI();
			}
		}

		void DrawSceneLabel(string text, Vector2 position)
		{
			GUIContent gc = pb_GUI_Utility.TempGUIContent(text);

			float width = EditorStyles.boldLabel.CalcSize(gc).x;
			float height = EditorStyles.label.CalcHeight(gc, width) + 4;

			pb_GUI_Utility.DrawSolidColor(new Rect(position.x, position.y, width, height), SceneLabelBackgroundColor);

			GUI.Label( new Rect(position.x, position.y, width, height), gc, EditorStyles.boldLabel );
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

			if( selectedOnly && pb.vertexCount != pb.msh.vertices.Length || elementLength <= 0f)
				return;

			int vertexCount = selectedOnly ? pb.SelectedTriangleCount : pb.msh.vertexCount;

			Vector3[] vertices = selectedOnly ?  pbUtil.ValuesWithIndices<Vector3>(pb.msh.vertices, pb.SelectedTriangles) : pb.msh.vertices;
			Vector3[] normals  = selectedOnly ?  pbUtil.ValuesWithIndices<Vector3>(pb.msh.normals, pb.SelectedTriangles) : pb.msh.normals;
			Vector4[] tangents = selectedOnly ?  pbUtil.ValuesWithIndices<Vector4>(pb.msh.tangents, pb.SelectedTriangles) : pb.msh.tangents;

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

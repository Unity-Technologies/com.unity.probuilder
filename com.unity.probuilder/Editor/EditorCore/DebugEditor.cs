#pragma warning disable 0168

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Debugging menu items for ProBuilder.
	/// </summary>
	/// <inheritdoc cref="EditorWindow"/>
	/// <inheritdoc cref="System.IDisposable"/>
	sealed class DebugEditor : EditorWindow, System.IDisposable
	{
		float elementLength = .15f;
		float elementOffset = .01f;
		float faceInfoOffset = .05f;

		const int k_MaxSceneLabels = 64;
		static readonly Color SplitterColor = new Color(.3f, .3f, .3f, .75f);
		static ProBuilderEditor editor { get { return ProBuilderEditor.instance; } }
		SceneViewLineRenderer m_LineRenderer;

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Debug/ProBuilder Debug Window", false, PreferenceKeys.menuRepair)]
		public static void MenuSceneViewDebug()
		{
			GetWindow<DebugEditor>();
		}

		void OnEnable()
		{
			elementLength = PreferencesInternal.GetFloat("pb_Debug_elementLength", .1f);
			elementOffset = PreferencesInternal.GetFloat("pb_Debug_elementOffset", .01f);
			faceInfoOffset = PreferencesInternal.GetFloat("pb_Debug_faceInfoOffset", .05f);
			testOcclusion = PreferencesInternal.GetBool("pb_Debug_testOcclusion", false);

			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			SceneView.onSceneGUIDelegate += OnSceneGUI;
			ProBuilderEditor.onSelectionUpdate += OnSelectionUpdate;
			ProBuilderEditor.onVertexMovementFinish += OnSelectionUpdate;

			m_LineRenderer = new SceneViewLineRenderer();
		}

		void OnDisable()
		{
			m_LineRenderer.Dispose();
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			ProBuilderEditor.onSelectionUpdate -= OnSelectionUpdate;
			ProBuilderEditor.onVertexMovementFinish -= OnSelectionUpdate;
		}

		void OnSelectionUpdate(ProBuilderMesh[] selection)
		{
			try
			{
				m_LineRenderer.Clear();

				foreach(ProBuilderMesh pb in selection)
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
				showObject = true;
				showVertices = false;
				showColors = false;
				showUv = false;
				showUv2 = false;
				showAutoUV = false;
				showSharedUV = false;
				showSharedTris = false;
				showTriangles = false;
			}
		}

		Dictionary<int, ParamView> showParams = new Dictionary<int, ParamView>();
		ProBuilderMesh[] selection = new ProBuilderMesh[0];

		void OnGUI()
		{
			selection = editor != null ? editor.selection : new ProBuilderMesh[0];

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
				faceInfoOffset = EditorGUILayout.Slider("Label Offset", faceInfoOffset, 0f, .5f);

			}
			EditorGUILayout.EndToggleGroup();

			GUILayout.Label("Scene Label Settings", EditorStyles.boldLabel);

			selectedOnly = EditorGUILayout.Toggle("Selection Only", selectedOnly);
			testOcclusion = EditorGUILayout.Toggle("Depth Test", testOcclusion);

			GUILayout.Space(4);

			UI.EditorGUIUtility.DrawSeparator(1, SplitterColor);

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
				PreferencesInternal.SetFloat("pb_Debug_elementLength", elementLength);
				PreferencesInternal.SetFloat("pb_Debug_elementOffset", elementOffset);
				PreferencesInternal.SetBool("pb_Debug_testOcclusion", testOcclusion);

				foreach(ProBuilderMesh pb in selection)
					DrawElements(pb);

				SceneView.RepaintAll();
			}

			GUILayout.Space(8);
			UI.EditorGUIUtility.DrawSeparator(1, SplitterColor);

			GUILayout.Label("Active Selection", EditorStyles.boldLabel);

			if(selection.Length > 0)
			{
				if(selection[0].selectedVertexCount < 256)
				{
					GUILayout.Label("Faces: [" + selection[0].selectedFaceCount + "/" + selection[0].facesInternal.Length + "]  ");
					GUILayout.Label("Edges: [" + selection[0].selectedEdgeCount + "]");
					GUILayout.Label("Triangles: [" + selection[0].selectedVertexCount + "]");
				}
			}

			foreach(ProBuilderMesh pb in selection)
			{
				Mesh m = pb.mesh;
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
							pv.showVertices = EditorGUILayout.Foldout(pv.showVertices, "Vertices: " + pb.vertexCount + " / " + pb.mesh.vertexCount);
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						GUILayout.Space(48);
							if(pv.showVertices)
							{
								if(m == null)
								{
									GUILayout.Label("" + pb.positionsInternal.ToString("\n"));
								}
								else
								{
									GUILayout.BeginVertical();
									for(int i = 0; i < m.subMeshCount; i++)
									{
										GUILayout.Label("Mat: " + ren.sharedMaterials[i].name + "\n" + pb.positionsInternal.ValuesWithIndices( m.GetTriangles(i) ).ToString("\n") + "\n");
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
							pv.showTriangles = EditorGUILayout.Foldout(pv.showTriangles, "Triangles: " + pb.mesh.triangles.Length);
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						GUILayout.Space(48);
							if(pv.showTriangles)
							{
								if(m == null)
								{
									GUILayout.Label("Faces: " + pb.facesInternal.Length);
								}
								else
								{
									GUILayout.BeginVertical();
									for(int i = 0; i < m.subMeshCount; i++)
									{
										int[] tris = pb.mesh.GetTriangles(i);
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
							pv.showColors = EditorGUILayout.Foldout(pv.showColors, "colors: " + (pb.colorsInternal != null ? pb.colorsInternal.Length : 0).ToString());
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						GUILayout.Space(48);
							if(pv.showColors)
							{
								GUILayout.Label("" + pb.colorsInternal.ToString("\n"));
							}
						GUILayout.EndHorizontal();
					}

					/* UV  */
					{
						GUILayout.BeginHorizontal();
							GUILayout.Space(24);
							pv.showUv = EditorGUILayout.Foldout(pv.showUv, "UVs: " + pb.texturesInternal.Length);
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						GUILayout.Space(48);
							if(pv.showUv)
								GUILayout.Label("" + pb.texturesInternal.ToString("\n"));
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
								GUILayout.Label("" + pb.GetSelectedFaces().Select(x => x.uv).ToArray().ToString("\n"));
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
								for(int i = 0; i < pb.sharedIndicesUVInternal.Length; i++)
								{
									if(GUILayout.Button("" + pb.sharedIndicesUVInternal[i].array.ToString(", "), EditorStyles.label))
									{
										pb.SetSelectedVertices(pb.sharedIndicesUVInternal[i]);

										if(ProBuilderEditor.instance)
										{
											ProBuilderEditor.Refresh();
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
								GUILayout.Label("" + pb.sharedIndicesInternal.ToString("\n"));
						GUILayout.EndHorizontal();
					}
				}
			}
			GUILayout.EndScrollView();
		}

		void OnSceneGUI(SceneView scn)
		{
			foreach(ProBuilderMesh pb in InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms))
			{
				DrawStats(pb);
			}

			Repaint();
		}

		void DrawStats(ProBuilderMesh pb)
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

		void DrawTriangleInfo(ProBuilderMesh pb)
		{
			IntArray[] sharedIndices = pb.sharedIndicesInternal;
			Dictionary<int, int> lookup = sharedIndices.ToDictionary();
			Vector3[] vertices = pb.positionsInternal;
			Camera cam = SceneView.lastActiveSceneView.camera;

			HashSet<int> common = new HashSet<int>();

			if(selectedOnly)
			{
				foreach(int i in pb.selectedVertices)
					common.Add(lookup[i]);
			}
			else
			{
				for(int i = 0; i < sharedIndices.Length; i++)
					common.Add(i);
			}

			int labelCount = 0;

			foreach(int i in common)
			{
				int[] indices = sharedIndices[i];

				Vector3 point = pb.transform.TransformPoint(vertices[indices[0]]);

				if( testOcclusion && UnityEngine.ProBuilder.HandleUtility.PointIsOccluded(cam, pb, point) )
					continue;

				Vector2 cen = HandleUtility.WorldToGUIPoint(point);

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

				UI.EditorGUIUtility.SceneLabel(sb.ToString(), cen);

				if(++labelCount > k_MaxSceneLabels) break;
			}
		}

		void DrawEdgeInfo(ProBuilderMesh pb)
		{
			Dictionary<int, int> lookup = pb.sharedIndicesInternal.ToDictionary();
			Edge[] source = selectedOnly ? pb.selectedEdges.ToArray() : pb.facesInternal.SelectMany(x => x.edgesInternal).ToArray();
			IEnumerable<EdgeLookup> edges = EdgeLookup.GetEdgeLookup(source, lookup);
			Camera cam = SceneView.lastActiveSceneView.camera;

			int labelCount = 0;

			foreach(EdgeLookup edge in edges)
			{
				Vector3 point = pb.transform.TransformPoint((pb.positionsInternal[edge.local.x] + pb.positionsInternal[edge.local.y])/ 2f);

				if( testOcclusion && UnityEngine.ProBuilder.HandleUtility.PointIsOccluded(cam, pb, point) )
					continue;

				Vector2 cen = HandleUtility.WorldToGUIPoint(point);

				switch(edgeIndexFormat)
				{
					case IndexFormat.Common:
						UI.EditorGUIUtility.SceneLabel(string.Format("[{0}, {1}]", edge.common.x, edge.common.y), cen);
						break;
					case IndexFormat.Local:
						UI.EditorGUIUtility.SceneLabel(string.Format("[{0}, {1}]", edge.local.x, edge.local.y), cen);
						break;
					case IndexFormat.Both:
						UI.EditorGUIUtility.SceneLabel(string.Format("local: [{0}, {1}]\ncommon: [{2}, {3}]",
                            edge.local.x,
                            edge.local.y,
                            edge.common.x,
                            edge.common.y), cen);
						break;
				}

				if(++labelCount > k_MaxSceneLabels) break;
			}
		}

		void DrawFaceInfo(ProBuilderMesh pb)
		{
			Face[] faces = selectedOnly ? pb.GetSelectedFaces() : pb.facesInternal;
			Dictionary<int, int> lookup = pb.sharedIndicesInternal.ToDictionary();
			Camera cam = SceneView.lastActiveSceneView.camera;

			int labelCount = 0;

			foreach(Face f in faces)
			{
				Vector3 point = pb.transform.TransformPoint( Math.Average(pb.positionsInternal, f.distinctIndices) );

				if( testOcclusion && UnityEngine.ProBuilder.HandleUtility.PointIsOccluded(cam, pb, point) )
					continue;

				Vector3 normal = pb.transform.TransformDirection( Math.Normal(pb, f) );

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

				Vector3 labelPos = point + (normal.normalized + cam.transform.up.normalized) * faceInfoOffset;

				if(faceInfoOffset > .001f)
					Handles.DrawLine(point, labelPos);

				Vector2 cen = HandleUtility.WorldToGUIPoint(labelPos);
				cen.y -= 5f;

				Handles.BeginGUI();
				UI.EditorGUIUtility.SceneLabel(sb.ToString(), cen);
				Handles.EndGUI();

				if(++labelCount > k_MaxSceneLabels) break;
			}
		}

		readonly Color[] ElementColors = new Color[] { Color.green, Color.blue, Color.red };

		/**
		 * Draw the normals, tangents, and bitangets associated with this mesh.
		 * Green = normals
		 * Blue = tangents
		 * Red = bitangents
		 */
		void DrawElements(ProBuilderMesh pb)
		{
			m_LineRenderer.Clear();

			if( selectedOnly && pb.vertexCount != pb.mesh.vertices.Length || elementLength <= 0f)
				return;

			int vertexCount = selectedOnly ? pb.selectedVertexCount : pb.mesh.vertexCount;

			var indices = pb.selectedVertices.ToArray();
			Vector3[] vertices = selectedOnly ? UnityEngine.ProBuilder.ArrayUtility.ValuesWithIndices<Vector3>(pb.mesh.vertices, indices) : pb.mesh.vertices;
			Vector3[] normals  = selectedOnly ? UnityEngine.ProBuilder.ArrayUtility.ValuesWithIndices<Vector3>(pb.mesh.normals, indices) : pb.mesh.normals;
			Vector4[] tangents = selectedOnly ? UnityEngine.ProBuilder.ArrayUtility.ValuesWithIndices<Vector4>(pb.mesh.tangents, indices) : pb.mesh.tangents;

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

			m_LineRenderer.AddLineSegments(segments, ElementColors);
		}

        public void Dispose()
        {
            if (m_LineRenderer != null)
                m_LineRenderer.Dispose();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;
using PHandleUtility = UnityEngine.ProBuilder.HandleUtility;
using UHandleUtility = UnityEditor.HandleUtility;
using MaterialEditor = UnityEditor.ProBuilder.MaterialEditor;
using RaycastHit = UnityEngine.ProBuilder.RaycastHit;

namespace UnityEditor.ProBuilder
{
	struct ScenePickerPreferences
	{
		public const float maxPointerDistanceFuzzy = 128f;
		public const float maxPointerDistancePrecise = 12f;
		public const CullingMode defaultCullingMode = CullingMode.Back;
		public const SelectionModifierBehavior defaultSelectionModifierBehavior = SelectionModifierBehavior.Difference;

		public float maxPointerDistance;
		public CullingMode cullMode;
		public SelectionModifierBehavior selectionModifierBehavior;
	}

	class SceneSelection
	{
		public GameObject gameObject;
		public ProBuilderMesh mesh;
		public int vertex;
		public Edge edge;
		public Face face;

		public SceneSelection(GameObject gameObject)
		{
			this.gameObject = gameObject;
		}

		public SceneSelection(ProBuilderMesh mesh, int vertex) : this(mesh != null ? mesh.gameObject : null)
		{
			this.mesh = mesh;
			this.vertex = vertex;
			edge = Edge.Empty;
			face = null;
		}

		public SceneSelection(ProBuilderMesh mesh, Edge edge) : this(mesh != null ? mesh.gameObject : null)
		{
			this.mesh = mesh;
			vertex = -1;
			this.edge = edge;
			face = null;
		}

		public SceneSelection(ProBuilderMesh mesh, Face face) : this(mesh != null ? mesh.gameObject : null)
		{
			this.mesh = mesh;
			vertex = -1;
			edge = Edge.Empty;
			this.face = face;
		}
	}

	static class EditorSceneViewPicker
	{
		static int s_DeepSelectionPrevious = 0x0;
		static Rect s_MouseClickRect = new Rect(0f, 0f, 20, 20);

		public static ProBuilderMesh DoMouseClick(Event evt, SelectMode selectionMode, ScenePickerPreferences pickerPreferences)
		{
			bool appendModifier = EditorHandleUtility.IsAppendModifier(evt.modifiers);

			if (!appendModifier)
				MeshSelection.SetSelection((GameObject) null);

			var selection = MouseRayHitTest(evt.mousePosition, selectionMode, pickerPreferences, evt.clickCount > 1 ? -1 : 0);

			if (selection == null)
				return null;

			MeshSelection.AddToSelection(selection.gameObject);

			if (selection.mesh != null)
			{
				var mesh = selection.mesh;

				if (selection.face != null)
				{
					var ind = mesh.faces.IndexOf(selection.face);
					var sel = mesh.selectedFaceIndexes.IndexOf(ind);

					UndoUtility.RecordSelection(mesh, "Select Face");

					if(sel > -1)
						mesh.RemoveFromFaceSelectionAtIndex(sel);
					else
						mesh.AddToFaceSelection(ind);
				}
				else if(selection.edge != Edge.Empty)
				{
					int ind = mesh.selectedEdges.IndexOf(selection.edge, mesh.sharedIndicesInternal.ToDictionary());

					UndoUtility.RecordSelection(mesh, "Select Edge");

					if (ind > -1)
						mesh.SetSelectedEdges(mesh.selectedEdges.ToArray().RemoveAt(ind));
					else
						mesh.SetSelectedEdges(mesh.selectedEdges.ToArray().Add(selection.edge));

				}
				else if (selection.vertex > -1)
				{
					int ind = System.Array.IndexOf(mesh.selectedIndicesInternal, selection.vertex);

					UndoUtility.RecordSelection(mesh, "Select Vertex");

					if (ind > -1)
						mesh.SetSelectedVertices(mesh.selectedIndicesInternal.RemoveAt(ind));
					else
						mesh.SetSelectedVertices(mesh.selectedIndicesInternal.Add(selection.vertex));
				}

				return mesh;
			}

			return null;
		}

		// Returns the pb_Object modified by this action.  If no action taken, or action is eaten by texture window, return null.
		// A pb_Object is returned because double click actions need to know what the last selected pb_Object was.
		// If deepClickOffset is specified, the object + deepClickOffset in the deep select stack will be returned (instead of next).
		public static SceneSelection MouseRayHitTest(Vector3 mousePosition, SelectMode selectionMode, ScenePickerPreferences pickerOptions, int deepClickOffset = 0)
		{
			// Since Edge or Vertex selection may be valid even if clicking off a gameObject, check them
			// first. If no hits, move on to face selection or object change.
			SceneSelection selected;

			if ( (selectionMode == SelectMode.Edge && (selected = EdgeClickCheck(mousePosition, pickerOptions)) != null)
				|| (selectionMode == SelectMode.Vertex && (selected = VertexClickCheck(mousePosition, pickerOptions)) != null))
				return selected;

			return FaceClickCheck(mousePosition, pickerOptions, deepClickOffset);
		}

		internal static SceneSelection FaceClickCheck(Vector3 mousePosition, ScenePickerPreferences pickerOptions, int deepClickOffset = 0)
		{
			GameObject pickedGo = null;
			ProBuilderMesh pickedPb = null;
			Face pickedFace = null;
			int newHash = 0;
			List<GameObject> picked = EditorHandleUtility.GetAllOverlapping(mousePosition);
			EventModifiers em = Event.current.modifiers;

			// If any event modifiers are engaged don't cycle the deep click
			int pickedCount = em != EventModifiers.None ? System.Math.Min(1, picked.Count) : picked.Count;

			for (int i = 0, next = 0; i < pickedCount; i++)
			{
				GameObject go = picked[i];
				var mesh = go.GetComponent<ProBuilderMesh>();
				Face face = null;

				if (mesh != null)
				{
					Ray ray = UHandleUtility.GUIPointToWorldRay(mousePosition);
					RaycastHit hit;

					if (UnityEngine.ProBuilder.HandleUtility.FaceRaycast(ray,
						mesh,
						out hit,
						Mathf.Infinity,
						pickerOptions.cullMode))
					{
						face = mesh.facesInternal[hit.face];
					}
				}

				// pb_Face doesn't define GetHashCode, meaning it falls to object.GetHashCode (reference comparison)
				int hash = face == null ? go.GetHashCode() : face.GetHashCode();

				if (s_DeepSelectionPrevious == hash)
					next = (i + (1 + deepClickOffset)) % pickedCount;

				if (next == i)
				{
					pickedGo = go;
					pickedPb = mesh;
					pickedFace = face;

					newHash = hash;

					// a prior hash was matched, this is the next. if
					// it's just the first iteration don't break (but do
					// set the default).
					if (next != 0)
						break;
				}
			}

			s_DeepSelectionPrevious = newHash;

			if (pickedGo != null)
			{
				Event.current.Use();

				if (pickedPb != null)
				{
					if (pickedPb.isSelectable)
					{
						MeshSelection.AddToSelection(pickedGo);

						// Check for other editor mouse shortcuts first
						MaterialEditor matEditor = MaterialEditor.instance;
						if (matEditor != null && matEditor.ClickShortcutCheck(Event.current.modifiers, pickedPb, pickedFace))
							return null;

						UVEditor uvEditor = UVEditor.instance;
						if (uvEditor != null && uvEditor.ClickShortcutCheck(pickedPb, pickedFace))
							return null;

						return new SceneSelection(pickedPb, pickedFace);
					}

					return null;
				}

				if (!PreferencesInternal.GetBool(PreferenceKeys.pbPBOSelectionOnly))
				{
					// If clicked off a pb_Object but onto another gameobject, set the selection
					// and dip out.
					return new SceneSelection(pickedGo);
				}

				// clicked on something that isn't allowed at all (ex, pboSelectionOnly on and clicked a camera)
				return null;
			}

			return null;
		}

		internal static SceneSelection VertexClickCheck(Vector3 mousePosition, ScenePickerPreferences pickerOptions)
		{
			Camera cam = SceneView.lastActiveSceneView.camera;
			var nearest = new List<SimpleTuple<float, Vector3, ProBuilderMesh, int>>();
			var selection = MeshSelection.Top();
			float minAllowableDistance = pickerOptions.maxPointerDistance * pickerOptions.maxPointerDistance;

			for (int i = 0; i < selection.Length; i++)
			{
				ProBuilderMesh pb = selection[i];

				if (!pb.isSelectable)
					continue;

				var positions = pb.positionsInternal;
				var common = pb.sharedIndicesInternal;

				for (int n = 0, c = common.Length; n < c; n++)
				{
					int index = common[n][0];
					Vector3 v = pb.transform.TransformPoint(positions[index]);
					Vector3 p = UHandleUtility.WorldToGUIPoint(v);

					float dist = (p - mousePosition).sqrMagnitude;

					if (dist < minAllowableDistance)
						nearest.Add(new SimpleTuple<float, Vector3, ProBuilderMesh, int>(dist, v, pb, index));
				}
			}

			nearest.Sort((x, y) => x.item1.CompareTo(y.item1));

			for (int i = 0; i < nearest.Count; i++)
			{
				if (!UnityEngine.ProBuilder.HandleUtility.PointIsOccluded(cam, nearest[i].item3, nearest[i].item2))
					return new SceneSelection(nearest[i].item3, nearest[i].item4);
			}

			return null;
		}

		public static SceneSelection EdgeClickCheck(Vector3 mousePosition, ScenePickerPreferences pickerPrefs)
		{
			var selection = MeshSelection.Top();

			if (selection.Length < 1)
				return null;

			GameObject go = UHandleUtility.PickGameObject(mousePosition, false);
			ProBuilderMesh nearestMesh = go != null ? go.GetComponent<ProBuilderMesh>() : null;
			Edge nearestEdge = Edge.Empty;

			// If mouse isn't over a pb object, it still may be near enough to an edge.
			if (nearestMesh == null)
			{
				float bestDistance = pickerPrefs.maxPointerDistance;

				foreach (var mesh in selection)
				{
					var trs = mesh.transform;
					var positions = mesh.positionsInternal;

					foreach (var face in mesh.faces)
					{
						foreach (var edge in face.edges)
						{
							int x = edge.x;
							int y = edge.y;

							float d = UHandleUtility.DistanceToLine(
								trs.TransformPoint(positions[x]),
								trs.TransformPoint(positions[y]));

							if (d < bestDistance)
							{
								nearestMesh = mesh;
								nearestEdge = new Edge(x, y);
								bestDistance = d;
							}
						}
					}
				}
			}
			else
			{
				// Test culling
				List<RaycastHit> hits;
				Ray ray = UHandleUtility.GUIPointToWorldRay(mousePosition);

				if (PHandleUtility.FaceRaycast(ray, nearestMesh, out hits, CullingMode.Back))
				{
					Camera cam = SceneView.lastActiveSceneView.camera;

					// Sort from nearest hit to farthest
					hits.Sort((x, y) => x.distance.CompareTo(y.distance));

					// Find the nearest edge in the hit faces

					float bestDistance = Mathf.Infinity;
					Vector3[] v = nearestMesh.positionsInternal;

					for (int i = 0; i < hits.Count; i++)
					{
						if (UnityEngine.ProBuilder.HandleUtility.PointIsOccluded(cam, nearestMesh, nearestMesh.transform.TransformPoint(hits[i].point)))
							continue;

						foreach (Edge edge in nearestMesh.facesInternal[hits[i].face].edgesInternal)
						{
							float d = UHandleUtility.DistancePointLine(hits[i].point, v[edge.x], v[edge.y]);

							if (d < bestDistance)
							{
								bestDistance = d;
								nearestEdge = edge;
							}
						}

						if (Vector3.Dot(ray.direction, nearestMesh.transform.TransformDirection(hits[i].normal)) < 0f)
							break;
					}

					if (nearestEdge.IsValid() &&
						UHandleUtility.DistanceToLine(nearestMesh.transform.TransformPoint(v[nearestEdge.x]),
							nearestMesh.transform.TransformPoint(v[nearestEdge.y])) >
						pickerPrefs.maxPointerDistance)
						nearestEdge = Edge.Empty;
				}
			}

			return nearestMesh != null && nearestEdge != Edge.Empty ? new SceneSelection(nearestMesh, nearestEdge) : null;
		}

		public static void DragCheck()
		{}
//		{
//			SceneView sceneView = SceneView.lastActiveSceneView;
//			Camera cam = sceneView.camera;
//
//			UndoUtility.RecordSelection(selection, "Drag Select");
//			bool selectHidden = selectHiddenEnabled;
//
//			var pickingOptions = new PickerOptions()
//			{
//				depthTest = !selectHidden,
//				rectSelectMode = PreferencesInternal.GetEnum<RectSelectMode>(PreferenceKeys.pbRectSelectMode)
//			};
//
//			switch (selectionMode)
//			{
//				case SelectMode.Vertex:
//				{
//					if (!m_CurrentEvent.shift && !(m_CurrentEvent.command || m_CurrentEvent.control))
//						ClearElementSelection();
//
//					Dictionary<ProBuilderMesh, HashSet<int>> selected = Picking.PickVerticesInRect(
//						SceneView.lastActiveSceneView.camera,
//						m_MouseDragRect,
//						selection,
//						pickingOptions,
//						EditorGUIUtility.pixelsPerPoint);
//
//					foreach (var kvp in selected)
//					{
//						IntArray[] sharedIndices = kvp.Key.sharedIndicesInternal;
//						HashSet<int> common;
//
//						if (m_CurrentEvent.shift || (m_CurrentEvent.command || m_CurrentEvent.control))
//						{
//							common = sharedIndices.GetCommonIndices(kvp.Key.selectedIndicesInternal);
//
//							if (m_DragSelectMode == DragSelectMode.Add)
//								common.UnionWith(kvp.Value);
//							else if (m_DragSelectMode == DragSelectMode.Subtract)
//								common.RemoveWhere(x => kvp.Value.Contains(x));
//							else if (m_DragSelectMode == DragSelectMode.Difference)
//								common.SymmetricExceptWith(kvp.Value);
//						}
//						else
//						{
//							common = kvp.Value;
//						}
//
//						kvp.Key.SetSelectedVertices(common.SelectMany(x => sharedIndices[x].array).ToArray());
//					}
//
//					ProBuilderEditor.Refresh(false);
//				}
//					break;
//
//				case SelectMode.Face:
//				{
//					if (!m_CurrentEvent.shift && !(m_CurrentEvent.command || m_CurrentEvent.control))
//						ClearElementSelection();
//
//					Dictionary<ProBuilderMesh, HashSet<Face>> selected = Picking.PickFacesInRect(
//						SceneView.lastActiveSceneView.camera,
//						m_MouseDragRect,
//						selection,
//						pickingOptions,
//						EditorGUIUtility.pixelsPerPoint);
//
//					foreach (var kvp in selected)
//					{
//						HashSet<Face> current;
//
//						if (m_CurrentEvent.shift || (m_CurrentEvent.command || m_CurrentEvent.control))
//						{
//							current = new HashSet<Face>(kvp.Key.selectedFacesInternal);
//
//							if (m_DragSelectMode == DragSelectMode.Add)
//								current.UnionWith(kvp.Value);
//							else if (m_DragSelectMode == DragSelectMode.Subtract)
//								current.RemoveWhere(x => kvp.Value.Contains(x));
//							else if (m_DragSelectMode == DragSelectMode.Difference)
//								current.SymmetricExceptWith(kvp.Value);
//						}
//						else
//						{
//							current = kvp.Value;
//						}
//
//						kvp.Key.SetSelectedFaces(current);
//					}
//
//					ProBuilderEditor.Refresh(false);
//				}
//					break;
//
//				case SelectMode.Edge:
//				{
//					if (!m_CurrentEvent.shift && !(m_CurrentEvent.command || m_CurrentEvent.control))
//						ClearElementSelection();
//
//					var selected = Picking.PickEdgesInRect(
//						SceneView.lastActiveSceneView.camera,
//						m_MouseDragRect,
//						selection,
//						pickingOptions,
//						EditorGUIUtility.pixelsPerPoint);
//
//					foreach (var kvp in selected)
//					{
//						ProBuilderMesh pb = kvp.Key;
//						Dictionary<int, int> commonIndices = pb.sharedIndicesInternal.ToDictionary();
//						HashSet<EdgeLookup> selectedEdges = EdgeLookup.GetEdgeLookupHashSet(kvp.Value, commonIndices);
//
//						HashSet<EdgeLookup> current;
//
//						if (m_CurrentEvent.shift || (m_CurrentEvent.command || m_CurrentEvent.control))
//						{
//							current = EdgeLookup.GetEdgeLookupHashSet(pb.selectedEdges, commonIndices);
//
//							if (m_DragSelectMode == DragSelectMode.Add)
//								current.UnionWith(selectedEdges);
//							else if (m_DragSelectMode == DragSelectMode.Subtract)
//								current.RemoveWhere(x => selectedEdges.Contains(x));
//							else if (m_DragSelectMode == DragSelectMode.Difference)
//								current.SymmetricExceptWith(selectedEdges);
//						}
//						else
//						{
//							current = selectedEdges;
//						}
//
//						pb.SetSelectedEdges(current.Select(x => x.local));
//					}
//
//					ProBuilderEditor.Refresh(false);
//				}
//					break;
//
//				default:
//					DragObjectCheck();
//					break;
//			}
//
//			SceneView.RepaintAll();
//		}
//
//		// Emulates the usual Unity drag to select objects functionality
//		public static void DragObjectCheck()
//		{
//			// if we're in vertex selection mode, only add to selection if shift key is held,
//			// and don't clear the selection if shift isn't held.
//			// if not, behave regularly (clear selection if shift isn't held)
//			if (editLevel == EditLevel.Geometry && selectionMode == SelectMode.Vertex)
//			{
//				if (!m_CurrentEvent.shift && m_SelectedVertexCount > 0) return;
//			}
//			else
//			{
//				if (!m_CurrentEvent.shift) MeshSelection.ClearElementAndObjectSelection();
//			}
//
//			// scan for new selected objects
//			// if mode based, don't allow selection of non-probuilder objects
//			foreach (ProBuilderMesh g in HandleUtility.PickRectObjects(m_MouseDragRect).GetComponents<ProBuilderMesh>())
//				if (!Selection.Contains(g.gameObject))
//					MeshSelection.AddToSelection(g.gameObject);
//		}

	}
}

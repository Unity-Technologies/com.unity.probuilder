using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using MeshOps = UnityEngine.ProBuilder.MeshOperations;
using Object = UnityEngine.Object;
using UnityEngine.ProBuilder.Experimental.CSG;

namespace UnityEditor.ProBuilder
{
	/// <inheritdoc />
	/// <summary>
	/// Contains Menu commands for most ProBuilder operations. Will also attempt to Update the pb_Editor.
	/// </summary>
	sealed class MenuCommands : UnityEditor.Editor
	{
		private static ProBuilderEditor editor { get { return ProBuilderEditor.instance; } }

#region Object Level

#if !PROTOTYPE
		/**
		 * Combine selected pb_Objects to a single object.
		 * ProBuilder only.
		 */
		public static ActionResult MenuMergeObjects(ProBuilderMesh[] selected)
		{
			if(selected.Length < 2)
				return new ActionResult(ActionResult.Status.Canceled, "Must Select 2+ Objects");

			ProBuilderMesh pb = null;

			if( InternalMeshUtility.CombineObjects(selected, out pb) )
			{
//				pb_EntityUtility.SetEntityType(selected[0].GetComponent<pb_Entity>().entityType, pb.gameObject);

				pb.Optimize();

				pb.gameObject.name = "pb-MergedObject" + pb.id;

				// Delete donor objects
				for(int i = 0; i < selected.Length; i++)
				{
					if(selected[i] != null)
						UndoUtility.DestroyImmediate(selected[i].gameObject);
				}

				UndoUtility.RegisterCreatedObjectUndo(pb.gameObject, "Merge Objects");

				Selection.activeTransform = pb.transform;
			}

			if(editor)
				ProBuilderEditor.Refresh();

			return new ActionResult(ActionResult.Status.Success, "Merged Objects");
		}
#endif

		/**
		 * Set the pivot to the center of the current element selection.
		 * ProBuilder only.
		 */
		public static ActionResult MenuCenterPivot(ProBuilderMesh[] selection)
		{
			return _SetPivot(selection, null);
		}

		public static ActionResult MenuSetPivot(ProBuilderMesh[] selection)
		{
			int[][] tri = new int[selection.Length][];

			for(int i = 0; i < tri.Length; i++)
				tri[i] = selection[i].selectedVertices.ToArray();

			return _SetPivot(selection, tri);
		}

		private static ActionResult _SetPivot(ProBuilderMesh[] selection, int[][] triangles = null)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			Object[] objects = new Object[selection.Length * 2];
			System.Array.Copy(selection, 0, objects, 0, selection.Length);
			for(int i = selection.Length; i < objects.Length; i++)
				objects[i] = selection[i-selection.Length].transform;

			UndoUtility.RegisterCompleteObjectUndo(objects, "Set Pivot");

			for(int i = 0; i < selection.Length; i++)
			{
				TransformUtility.UnparentChildren(selection[i].transform);

				if(triangles != null)
					selection[i].CenterPivot(triangles[i]);
				else
					selection[i].CenterPivot(null);

				selection[i].Optimize();

				TransformUtility.ReparentChildren(selection[i].transform);
			}

			SceneView.RepaintAll();

			if(editor != null)
				ProBuilderEditor.Refresh();

			return new ActionResult(ActionResult.Status.Success, "Set Pivot");
		}

		public static ActionResult MenuFreezeTransforms(ProBuilderMesh[] selection)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			List<Object> undoables = new List<Object>( selection.Select(x => (Object) x.transform) );
			undoables.AddRange(selection);
			UndoUtility.RecordObjects(undoables.ToArray(), "Freeze Transforms");

			Vector3[][] vertices = new Vector3[selection.Length][];

			for(int i = 0; i < selection.Length; i++)
				vertices[i] = selection[i].VerticesInWorldSpace();

			for(int i = 0; i < selection.Length; i++)
			{
				ProBuilderMesh pb = selection[i];

				pb.transform.position = Vector3.zero;
				pb.transform.rotation = Quaternion.identity;
				pb.transform.localScale = Vector3.one;

				foreach(Face face in pb.facesInternal)
					face.manualUV = true;

				pb.SetPositions(vertices[i]);

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			ProBuilderEditor.Refresh();

			SceneView.RepaintAll();

			return new ActionResult(ActionResult.Status.Success, "Freeze Transforms");
		}

		/// <summary>
		/// Set the pb_Entity entityType on selection.
		/// </summary>
		/// <param name="selection"></param>
		/// <param name="entityType"></param>
		/// <returns></returns>
		[Obsolete("pb_Entity is obsolete.")]
		public static ActionResult MenuSetEntityType(ProBuilderMesh[] selection, EntityType entityType)
		{
			if(selection.Length < 1)
				return ActionResult.NoSelection;

			Object[] undoObjects = selection.SelectMany(x => x.GetComponents<Component>()).ToArray();

			UndoUtility.RecordObjects(undoObjects, "Set Entity Type");

			foreach(ProBuilderMesh pb in selection)
			{
				EntityUtility.SetEntityType(entityType, pb.gameObject);
				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			return new ActionResult(ActionResult.Status.Success, "Set " + entityType);
		}

		/**
		 *	Open the vertex color editor (palette or painter) based on prefs.
		 */
		public static void MenuOpenVertexColorsEditor()
		{
			switch( PreferencesInternal.GetEnum<VertexColorTool>(PreferenceKeys.pbVertexColorTool) )
			{
				case VertexColorTool.Palette:
					VertexColorPalette.MenuOpenWindow();
					break;

				default:
					VertexColorPainter.MenuOpenWindow();
					break;
			}
		}

        /// <summary>
        /// Open the vertex coloring editor as stored by user prefs.
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        public static ActionResult MenuOpenVertexColorsEditor2()
		{
			switch( PreferencesInternal.GetEnum<VertexColorTool>(PreferenceKeys.pbVertexColorTool) )
			{
				case VertexColorTool.Palette:
					VertexColorPalette.MenuOpenWindow();
					break;

				default:
					VertexColorPainter.MenuOpenWindow();
					break;
			}

			return new ActionResult(ActionResult.Status.Success, "Open Vertex Colors Editor");
		}

		public static void VertexColorsGUI(int width)
		{
			VertexColorTool tool = PreferencesInternal.GetEnum<VertexColorTool>(PreferenceKeys.pbVertexColorTool);
			VertexColorTool prev = tool;

			GUILayout.Label("Color Editor");

			tool = (VertexColorTool) EditorGUILayout.EnumPopup("", tool, GUILayout.MaxWidth(width-14));

			if(prev != tool)
				PreferencesInternal.SetInt(PreferenceKeys.pbVertexColorTool, (int)tool);
		}
#if !PROTOTYPE

		public static ActionResult MenuFacetizeObject(ProBuilderMesh[] selection)
		{
			if(!editor || selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RegisterCompleteObjectUndo(selection, "Triangulate Objects");

			for(int i = 0; i < selection.Length; i++)
			{
				selection[i].ToMesh();
				selection[i].ToTriangles(selection[i].facesInternal);
				selection[i].Refresh();
				selection[i].Optimize();
			}

			ProBuilderEditor.Refresh();

			return new ActionResult(ActionResult.Status.Success, "Triangulate " + selection.Length + (selection.Length > 1 ? " Objects" : " Object"));
		}

		enum BooleanOperation
		{
			Union,
			Subtract,
			Intersect
		}

		static ActionResult MenuBooleanOperation(BooleanOperation operation, ProBuilderMesh lhs, ProBuilderMesh rhs)
		{
			if(lhs == null || rhs == null)
				return new ActionResult(ActionResult.Status.Failure, "Must Select 2 Objects");

			string op_string = operation == BooleanOperation.Union ? "Union" : (operation == BooleanOperation.Subtract ? "Subtract" : "Intersect");

			ProBuilderMesh[] sel = new ProBuilderMesh[] { lhs, rhs };

			UndoUtility.RecordSelection(sel, op_string);

			Mesh c;

			switch(operation)
			{
				case BooleanOperation.Union:
					c = CSG.Union(lhs.gameObject, rhs.gameObject);
					break;

				case BooleanOperation.Subtract:
					c = CSG.Subtract(lhs.gameObject, rhs.gameObject);
					break;

				default:
					c = CSG.Intersect(lhs.gameObject, rhs.gameObject);
					break;
			}

			GameObject go = new GameObject();

			go.AddComponent<MeshRenderer>().sharedMaterial = PreferencesInternal.GetMaterial(PreferenceKeys.pbDefaultMaterial);
			go.AddComponent<MeshFilter>().sharedMesh = c;

			ProBuilderMesh pb = InternalMeshUtility.CreatePbObjectWithTransform(go.transform, false);
			DestroyImmediate(go);

			Selection.objects = new Object[] { pb.gameObject };

			return new ActionResult(ActionResult.Status.Success, op_string);
		}

		/**
		 * Union operation between two ProBuilder objects.
		 */
		public static ActionResult MenuUnion(ProBuilderMesh lhs, ProBuilderMesh rhs)
		{
			return MenuBooleanOperation(BooleanOperation.Union, lhs, rhs);
		}

		/**
		 * Subtract boolean operation between two pb_Objects.
		 */
		public static ActionResult MenuSubtract(ProBuilderMesh lhs, ProBuilderMesh rhs)
		{
			return MenuBooleanOperation(BooleanOperation.Subtract, lhs, rhs);
		}

		/**
		 * Intersect boolean operation between two pb_Objects.
		 */
		public static ActionResult MenuIntersect(ProBuilderMesh lhs, ProBuilderMesh rhs)
		{
			return MenuBooleanOperation(BooleanOperation.Intersect, lhs, rhs);
		}
#endif

#endregion

#region Normals

		/**
		 *	Reverse the direction of all faces on each object.
		 */
		public static ActionResult MenuFlipObjectNormals(ProBuilderMesh[] selected)
		{
			if(selected == null || selected.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RecordSelection(InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms), "Flip Object Normals");

			foreach(ProBuilderMesh pb in selected)
			{
				foreach(var face in pb.facesInternal)
					face.Reverse();
				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			return new ActionResult(ActionResult.Status.Success, "Flip Object Normals");
		}

		/**
		 * Flips all face normals if editLevel == EditLevel.Top, else flips only pb_Object->SelectedFaces
		 */
		public static ActionResult MenuFlipNormals(ProBuilderMesh[] selected)
		{
			if(selected == null || selected.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RecordSelection(InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms), "Flip Face Normals");

			int c = 0;
			int faceCount = ProBuilderEditor.instance.selectedFaceCount;

			foreach(ProBuilderMesh pb in InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms))
			{
				if( pb.selectedFaceCount < 1 && faceCount < 1 )
				{
					foreach(var face in pb.facesInternal)
						face.Reverse();

					c += pb.facesInternal.Length;
				}
				else
				{
					foreach(var face in pb.GetSelectedFaces())
						face.Reverse();

					c += pb.selectedFaceCount;
				}


				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			if(c > 0)
				return new ActionResult(ActionResult.Status.Success, "Flip " + c + (c > 1 ? " Face Normals" : " Face Normal"));
			else
				return new ActionResult(ActionResult.Status.Canceled, "Flip Normals\nNo Faces Selected");
		}

		/**
		 * Attempt to make face normals uniform.
		 */
		public static ActionResult MenuConformObjectNormals(ProBuilderMesh[] selection)
		{
			return DoConformNormals(selection, false);
		}

		public static ActionResult MenuConformNormals(ProBuilderMesh[] selection)
		{
			return DoConformNormals(selection, true);
		}

		public static ActionResult DoConformNormals(ProBuilderMesh[] selection, bool perFace = true)
		{
			if(!editor || selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RecordSelection(selection, "Conform " + (editor.selectedFaceCount > 0 ? "Face" : "Object") + " Normals.");

			ActionResult res = ActionResult.NoSelection;

			foreach(ProBuilderMesh pb in selection)
			{
				Face[] faces = perFace ? pb.GetSelectedFaces() : pb.facesInternal;

				if(faces == null)
					continue;

				res = UnityEngine.ProBuilder.MeshOperations.SurfaceTopology.ConformNormals(pb, faces);

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			ProBuilderEditor.Refresh();

			return res;
		}
#endregion

#region Extrude / Bridge

		public static void ExtrudeButtonGUI(int width)
		{
			float extrudeAmount = PreferencesInternal.HasKey(PreferenceKeys.pbExtrudeDistance) ? PreferencesInternal.GetFloat(PreferenceKeys.pbExtrudeDistance) : .5f;
			bool extrudeAsGroup = PreferencesInternal.GetBool(PreferenceKeys.pbExtrudeAsGroup);

			EditorGUI.BeginChangeCheck();

			EditorGUIUtility.labelWidth = width - 28;
			extrudeAsGroup = EditorGUILayout.Toggle("As Group", extrudeAsGroup);

			EditorGUIUtility.labelWidth = width - 68;
			extrudeAmount = EditorGUILayout.FloatField("Dist", extrudeAmount, GUILayout.MaxWidth(width-12));

			if(EditorGUI.EndChangeCheck())
			{
				PreferencesInternal.SetFloat(PreferenceKeys.pbExtrudeDistance, extrudeAmount);
				PreferencesInternal.SetBool(PreferenceKeys.pbExtrudeAsGroup, extrudeAsGroup);
			}
		}

		/**
		 * Infers the correct context and extrudes the selected elements.
		 */
		public static ActionResult MenuExtrude(ProBuilderMesh[] selection, bool enforceCurrentSelectionMode = false)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RegisterCompleteObjectUndo(selection, "Extrude");

			int extrudedFaceCount = 0;
			bool success = false;

			foreach(ProBuilderMesh pb in selection)
			{
				pb.ToMesh();
				pb.Refresh(RefreshMask.Normals);

				if(editor && editor.selectionMode == SelectMode.Edge)
				{
					if(pb.selectedEdgeCount < 1 || (!enforceCurrentSelectionMode && pb.selectedFaceCount > 0))
					{
						success = false;
					}
					else
					{
						extrudedFaceCount += pb.selectedEdgeCount;
						Edge[] newEdges = pb.Extrude(	pb.selectedEdges.ToArray(),
												PreferencesInternal.GetFloat(PreferenceKeys.pbExtrudeDistance),
												PreferencesInternal.GetBool(PreferenceKeys.pbExtrudeAsGroup),
												PreferencesInternal.GetBool(PreferenceKeys.pbManifoldEdgeExtrusion));
						success = newEdges != null;

						if(success)
							pb.SetSelectedEdges(newEdges);
						else
							extrudedFaceCount -= pb.selectedEdgeCount;
					}

					pb.ToMesh();
				}

				if((editor.selectionMode == SelectMode.Face || (!enforceCurrentSelectionMode && !success)) && pb.selectedFaceCount > 0)
				{
					extrudedFaceCount += pb.selectedFaceCount;
					var selectedFaces = pb.GetSelectedFaces();
					pb.Extrude(	selectedFaces,
								PreferencesInternal.GetEnum<ExtrudeMethod>(PreferenceKeys.pbExtrudeMethod),
								PreferencesInternal.GetFloat(PreferenceKeys.pbExtrudeDistance));
					pb.SetSelectedFaces(selectedFaces);
					pb.ToMesh();
				}

				pb.Refresh();
				pb.Optimize();
			}

			if(editor != null)
				ProBuilderEditor.Refresh();

			SceneView.RepaintAll();

			if( extrudedFaceCount > 0 )
				return new ActionResult(ActionResult.Status.Success, "Extrude");
			else
				return new ActionResult(ActionResult.Status.Canceled, "Extrude\nEmpty Selection");
		}

#if !PROTOTYPE
		/**
		 * Create a face between two edges.
		 */
		public static ActionResult MenuBridgeEdges(ProBuilderMesh[] selection)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RecordSelection(selection, "Bridge Edges");

			bool success = false;
			bool limitToPerimeterEdges = PreferencesInternal.GetBool(PreferenceKeys.pbPerimeterEdgeBridgeOnly);

			foreach(ProBuilderMesh pb in selection)
			{
				if(pb.selectedEdgeCount == 2)
				{
					if(pb.Bridge(pb.selectedEdges[0], pb.selectedEdges[1], limitToPerimeterEdges) != null)
					{
						success = true;
						pb.ToMesh();
						pb.Refresh();
						pb.Optimize();
					}
				}
			}

			if(success)
			{
				ProBuilderEditor.Refresh();
				return new ActionResult(ActionResult.Status.Success, "Bridge Edges");
			}
			else
			{
				Debug.LogWarning("Failed Bridge Edges.  Bridge Edges requires that only 2 edges be selected, and they must both only have one connecting face (non-manifold).");
				return new ActionResult(ActionResult.Status.Failure, "Bridge Edges requires that only 2 edges be selected, and they must both only have one connecting face (non-manifold).");
			}
		}

		/**
		 * Bevel selected edges.
		 */
		public static ActionResult MenuBevelEdges(ProBuilderMesh[] selection)
		{
			ActionResult res = ActionResult.NoSelection;
			UndoUtility.RecordSelection(selection, "Bevel Edges");

			float amount = PreferencesInternal.GetFloat(PreferenceKeys.pbBevelAmount);

			foreach(ProBuilderMesh pb in selection)
			{
				pb.ToMesh();

				List<Face> faces = Bevel.BevelEdges(pb, pb.selectedEdges, amount);
				res = faces != null ? new ActionResult(ActionResult.Status.Success, "Bevel Edges") : new ActionResult(ActionResult.Status.Failure, "Failed Bevel Edges");

				if(res)
					pb.SetSelectedFaces(faces);

				pb.Refresh();
				pb.Optimize();
			}

			ProBuilderEditor.Refresh();

			return res;
		}
#endif
#endregion

#region Selection

		private static void GetSelectedElementCount(ProBuilderMesh[] selection, out int sel, out int max)
		{
			switch(editor.selectionMode)
			{
				case SelectMode.Face:
					sel = selection.Sum(x => x.selectedFaceCount);
					max = selection.Sum(x => x.faceCount);
					break;

				case SelectMode.Edge:
					sel = selection.Sum(x => x.selectedEdgeCount);
					max = selection.Sum(x => x.facesInternal.Sum(y=>y.edgesInternal.Length));
					break;

				default:
					sel = selection.Sum(x => x.selectedVertexCount);
					max = selection.Sum(x => x.triangleCount);
					break;
			}
		}

		public static bool VerifyGrowSelection(ProBuilderMesh[] selection)
		{
			if(	!editor ||
				selection == null ||
				selection.Length < 1 ||
				editor.editLevel == EditLevel.Top
				)
				return false;

			int sel, max;
			GetSelectedElementCount(selection, out sel, out max);

			return sel > 0 && sel < max;
		}

		/**
		 * Grow selection to plane using max angle diff.
		 */
		public static ActionResult MenuGrowSelection(ProBuilderMesh[] selection)
		{
			if(!editor || selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RecordSelection(selection, "Grow Selection");

			int grown = 0;
			bool angleGrow = PreferencesInternal.GetBool(PreferenceKeys.pbGrowSelectionUsingAngle);
			bool iterative = PreferencesInternal.GetBool(PreferenceKeys.pbGrowSelectionAngleIterative);
			float growSelectionAngle = PreferencesInternal.GetFloat(PreferenceKeys.pbGrowSelectionAngle);

			if(!angleGrow && !iterative)
				iterative = true;

			foreach(ProBuilderMesh pb in InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms))
			{
				int previousTriCount = pb.selectedVertexCount;

				switch( editor != null ? editor.selectionMode : (SelectMode)0 )
				{
					case SelectMode.Vertex:
						pb.SetSelectedEdges(ElementSelection.GetConnectedEdges(pb, pb.selectedIndicesInternal));
						break;

					case SelectMode.Edge:
						pb.SetSelectedEdges(ElementSelection.GetConnectedEdges(pb, pb.selectedIndicesInternal));
						break;

					case SelectMode.Face:

						Face[] selectedFaces = pb.GetSelectedFaces();

						HashSet<Face> sel;

						if(iterative)
						{
							sel = ElementSelection.GrowSelection(pb, selectedFaces, angleGrow ? growSelectionAngle : -1f);
							sel.UnionWith(selectedFaces);
						}
						else
						{
							sel = ElementSelection.FloodSelection(pb, selectedFaces, angleGrow ? growSelectionAngle : -1f);
						}

						pb.SetSelectedFaces( sel.ToArray() );

						break;
				}

				grown += pb.selectedVertexCount - previousTriCount;
			}

			ProBuilderEditor.Refresh();
			SceneView.RepaintAll();

			if(grown > 0)
				return new ActionResult(ActionResult.Status.Success, "Grow Selection");
			else
				return new ActionResult(ActionResult.Status.Failure, "Nothing to Grow");
		}

		public static bool VerifyShrinkSelection(ProBuilderMesh[] selection)
		{
			if(	!editor ||
				selection == null ||
				selection.Length < 1 ||
				editor.editLevel == EditLevel.Top)
				return false;

			int sel = 0, max = 0;
			GetSelectedElementCount(selection, out sel, out max);

			return sel > 1 && sel < max;
		}

		public static ActionResult MenuShrinkSelection(ProBuilderMesh[] selection)
		{
			// @TODO
			if(editor == null)
				return new ActionResult(ActionResult.Status.Canceled, "ProBuilder Editor Not Open!");
			else if (selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RecordSelection(selection, "Shrink Selection");

			// find perimeter edges
			int rc = 0;
			for(int i = 0; i < selection.Length; i++)
			{
				ProBuilderMesh pb = selection[i];

				switch(editor.selectionMode)
				{
					case SelectMode.Edge:
					{
						int[] perimeter = ElementSelection.GetPerimeterEdges(pb, pb.selectedEdges);
						pb.SetSelectedEdges( pb.selectedEdges.RemoveAt(perimeter) );
						rc += perimeter != null ? perimeter.Length : 0;
						break;
					}

					case SelectMode.Face:
					{
						Face[] perimeter = ElementSelection.GetPerimeterFaces(pb, pb.selectedFacesInternal).ToArray();
						pb.SetSelectedFaces( pb.selectedFacesInternal.Except(perimeter).ToArray() );
						rc += perimeter.Length;
						break;
					}

					case SelectMode.Vertex:
					{
						int[] perimeter = ElementSelection.GetPerimeterVertices(pb, pb.selectedIndicesInternal, editor.selectedUniversalEdges[i]);
						pb.SetSelectedVertices( pb.selectedIndicesInternal.RemoveAt(perimeter) );
						rc += perimeter != null ? perimeter.Length : 0;
						break;
					}
				}

			}

			ProBuilderEditor.Refresh(false);

			if( rc > 0 )
				return new ActionResult(ActionResult.Status.Success, "Shrink Selection");
			else
				return new ActionResult(ActionResult.Status.Canceled, "Nothing to Shrink");
		}

		public static bool VerifyInvertSelection(ProBuilderMesh[] selection)
		{
			if(	!editor ||
				selection == null ||
				selection.Length < 1 ||
				editor.editLevel == EditLevel.Top)
				return false;
			return true;
		}

		/**
		 * Invert the current selection.
		 */
		public static ActionResult MenuInvertSelection(ProBuilderMesh[] selection)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RecordSelection(selection, "Invert Selection");

			switch( editor != null ? editor.selectionMode : (SelectMode)0 )
			{
				case SelectMode.Vertex:
					foreach(ProBuilderMesh pb in selection)
					{
						IntArray[] sharedIndices = pb.sharedIndicesInternal;
						List<int> selSharedIndices = new List<int>();

						foreach(int i in pb.selectedIndicesInternal)
							selSharedIndices.Add( sharedIndices.IndexOf(i) );

						List<int> inverse = new List<int>();

						for(int i = 0; i < sharedIndices.Length; i++)
						{
							if(!selSharedIndices.Contains(i))
								inverse.Add(sharedIndices[i][0]);
						}

						pb.SetSelectedVertices(inverse.ToArray());
					}
					break;

				case SelectMode.Face:
					foreach(ProBuilderMesh pb in selection)
					{
						IEnumerable<Face> inverse = pb.facesInternal.Where( x => !pb.selectedFacesInternal.Contains(x) );
						pb.SetSelectedFaces(inverse.ToArray());
					}
					break;

				case SelectMode.Edge:

					if(!editor) break;

					for(int i = 0; i < selection.Length; i++)
					{
						Edge[] universal_selected_edges = EdgeExtension.GetUniversalEdges(selection[i].selectedEdges, selection[i].sharedIndicesInternal).Distinct().ToArray();
						Edge[] inverse_universal = System.Array.FindAll(editor.selectedUniversalEdges[i], x => !universal_selected_edges.Contains(x));
						Edge[] inverse = new Edge[inverse_universal.Length];

						for(int n = 0; n < inverse_universal.Length; n++)
							inverse[n] = new Edge( selection[i].sharedIndicesInternal[inverse_universal[n].x][0], selection[i].sharedIndicesInternal[inverse_universal[n].y][0] );

						selection[i].SetSelectedEdges(inverse);
					}
					break;
			}

			ProBuilderEditor.Refresh();
			SceneView.RepaintAll();

			return new ActionResult(ActionResult.Status.Success, "Invert Selection");
		}

		public static bool VerifyEdgeRingLoop(ProBuilderMesh[] selection)
		{
			if(	!editor ||
				selection == null ||
				selection.Length < 1 ||
				editor.editLevel == EditLevel.Top)
				return false;

			int sel, max;
			GetSelectedElementCount(selection, out sel, out max);
			return sel > 0 && sel < max;
		}

		/**
		 * Expands the current selection using a "Ring" method.
		 */
		public static ActionResult MenuRingSelection(ProBuilderMesh[] selection)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RecordSelection(selection, "Select Edge Ring");

			bool success = false;

			foreach(ProBuilderMesh pb in InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms))
			{
				Edge[] edges = ElementSelection.GetEdgeRing(pb, pb.selectedEdges).ToArray();

				if(edges.Length > pb.selectedEdgeCount)
					success = true;

				pb.SetSelectedEdges( edges );
			}

			ProBuilderEditor.Refresh(false);

			SceneView.RepaintAll();

			if(success)
				return new ActionResult(ActionResult.Status.Success, "Select Edge Ring");
			else
				return new ActionResult(ActionResult.Status.Failure, "Nothing to Ring");
		}

		/**
		 * Selects an edge loop.
		 */
		public static ActionResult MenuLoopSelection(ProBuilderMesh[] selection)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RecordSelection(selection, "Select Edge Loop");

			bool foundLoop = false;

			foreach(ProBuilderMesh pb in selection)
			{
				Edge[] loop;
				bool success = ElementSelection.GetEdgeLoop(pb, pb.selectedEdges, out loop);
				if(success)
				{
					if(loop.Length > pb.selectedEdgeCount)
						foundLoop = true;

					pb.SetSelectedEdges(loop);
				}
			}

			ProBuilderEditor.Refresh(false);

			SceneView.RepaintAll();

			if(foundLoop)
				return new ActionResult(ActionResult.Status.Success, "Select Edge Loop");
			else
				return new ActionResult(ActionResult.Status.Failure, "Nothing to Loop");
		}

		public static ActionResult MenuLoopFaces(ProBuilderMesh[] selection)
		{
			UndoUtility.RecordSelection(selection, "Select Face Loop");

			foreach (ProBuilderMesh pb in selection)
			{
				HashSet<Face> loop = ElementSelection.GetFaceLoop(pb, pb.selectedFacesInternal);
				pb.SetSelectedFaces(loop);
			}

			ProBuilderEditor.Refresh();

			return new ActionResult(ActionResult.Status.Success, "Select Face Loop");
		}

		public static ActionResult MenuRingFaces(ProBuilderMesh[] selection)
		{
			UndoUtility.RecordSelection(selection, "Select Face Ring");

			foreach (ProBuilderMesh pb in selection)
			{
				HashSet<Face> loop = ElementSelection.GetFaceLoop(pb, pb.selectedFacesInternal, true);
				pb.SetSelectedFaces(loop);
			}

			ProBuilderEditor.Refresh();

			return new ActionResult(ActionResult.Status.Success, "Select Face Ring");
		}

		public static ActionResult MenuRingAndLoopFaces(ProBuilderMesh[] selection)
		{
			UndoUtility.RecordSelection(selection, "Select Face Ring and Loop");

			foreach (ProBuilderMesh pb in selection)
			{
				HashSet<Face> loop = ElementSelection.GetFaceRingAndLoop(pb, pb.selectedFacesInternal);
				pb.SetSelectedFaces(loop);
			}

			ProBuilderEditor.Refresh();

			return new ActionResult(ActionResult.Status.Success, "Select Face Ring and Loop");
		}
#endregion

#region Delete / Detach

		/**
		 * Delete selected faces.
		 * ProBuilder only.
		 */
		public static ActionResult MenuDeleteFace(ProBuilderMesh[] selection)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RegisterCompleteObjectUndo(selection, "Delete Face");

			int count = 0;

			foreach(ProBuilderMesh pb in selection)
			{
				if(pb.selectedFaceCount == pb.facesInternal.Length)
				{
					Debug.LogWarning("Attempting to delete all faces on this mesh...  I'm afraid I can't let you do that.");
					continue;
				}

				pb.DeleteFaces(pb.selectedFacesInternal);
				count += pb.selectedFaceCount;

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			if(editor)
			{
				editor.ClearElementSelection();
				ProBuilderEditor.Refresh();
			}

			if(count > 0)
				return new ActionResult(ActionResult.Status.Success, "Delete " + count + " Faces");
			else
				return new ActionResult(ActionResult.Status.Failure, "No Faces Selected");
		}

#if !PROTOTYPE

		public static ActionResult MenuDetachFaces(ProBuilderMesh[] selection)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			bool detachToNewObject = PreferencesInternal.GetBool(PreferenceKeys.pbDetachToNewObject);

			if(detachToNewObject)
				return MenuDetachFacesToObject(selection);
			else
				return MenuDetachFacesToSubmesh(selection);
		}

		/**
		 * Detach selected faces to submesh.
		 * ProBuilder only.
		 */
		public static ActionResult MenuDetachFacesToSubmesh(ProBuilderMesh[] selection)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RegisterCompleteObjectUndo(selection, "Detach Face(s)");

			int count = 0;

			foreach(ProBuilderMesh pb in selection)
			{
				pb.ToMesh();
				List<Face> res = pb.DetachFaces(pb.selectedFacesInternal);
				pb.Refresh();
				pb.Optimize();

				pb.SetSelectedFaces(res.ToArray());

				count += pb.selectedFaceCount;
			}

			if(editor)
				ProBuilderEditor.Refresh();

			if(count > 0)
				return new ActionResult(ActionResult.Status.Success, "Detach " + count + (count > 1 ? " Faces" : " Face"));
			else
				return new ActionResult(ActionResult.Status.Success, "Detach Faces");
		}

		/**
		 * Detaches currently selected faces to a new ProBuilder object.
		 * ProBuilder only.
		 */
		public static ActionResult MenuDetachFacesToObject(ProBuilderMesh[] selection)
		{
			if(!editor || selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RegisterCompleteObjectUndo(selection, "Detach Selection to PBO");

			int detachedFaceCount = 0;
			List<GameObject> detached = new List<GameObject>();

			foreach(ProBuilderMesh pb in selection)
			{
				if(pb.selectedFaceCount < 1 || pb.selectedFaceCount == pb.facesInternal.Length)
					continue;

				// work with face indices here 'cause copying breaks the face ref
				int[] primary = pb.selectedFacesInternal.Select(x => System.Array.IndexOf((Array) pb.facesInternal, x)).ToArray();

				detachedFaceCount += primary.Length;

				List<int> inverse_list = new List<int>();
				for(int i = 0; i < pb.facesInternal.Length; i++)
					if(System.Array.IndexOf(primary, i) < 0)
						inverse_list.Add(i);

				int[] inverse = inverse_list.ToArray();

				ProBuilderMesh copy = ((GameObject)GameObject.Instantiate(pb.gameObject)).GetComponent<ProBuilderMesh>();
				copy.MakeUnique();

				// if is prefab, break connection and destroy children
				if( EditorUtility.IsPrefabInstance(copy.gameObject) || EditorUtility.IsPrefabRoot(copy.gameObject) )
					PrefabUtility.DisconnectPrefabInstance(copy.gameObject);

				if(copy.transform.childCount > 0)
				{
					for(int i = 0; i < copy.transform.childCount; ++i)
						GameObject.DestroyImmediate(copy.transform.GetChild(i).gameObject);

					foreach(ProBuilderMesh pb_child in pb.transform.GetComponentsInChildren<ProBuilderMesh>())
						EditorUtility.EnsureMeshSyncState(pb_child);
				}

				Undo.RegisterCreatedObjectUndo(copy.gameObject, "Detach Selection");

				copy.transform.position = pb.transform.position;
				copy.transform.localScale = pb.transform.localScale;
				copy.transform.localRotation = pb.transform.localRotation;

				pb.DeleteFaces(primary);
				copy.DeleteFaces(inverse);

				pb.ToMesh();
				copy.ToMesh();

				// copy.CenterPivot(null);

				pb.Refresh();
				copy.Refresh();

				pb.Optimize();
				copy.Optimize();

				pb.ClearSelection();
				copy.ClearSelection();

				copy.gameObject.name = pb.gameObject.name + "-detach";
				detached.Add(copy.gameObject);
			}

			if(editor)
			{
				MeshSelection.SetSelection(detached.ToArray());
				ProBuilderEditor.Refresh();
			}

			if(detachedFaceCount > 0)
				return new ActionResult(ActionResult.Status.Success, "Detach " + detachedFaceCount + " faces to new Object");
			else
				return new ActionResult(ActionResult.Status.Failure, "No Faces Selected");
		}

#endif
#endregion

#region Face / Triangles

		/**
		 *	Treat selected faces as a single face.
		 */
		public static ActionResult MenuMergeFaces(ProBuilderMesh[] selection)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RecordObjects(selection, "Merge Faces");

			int success = 0;

			foreach(ProBuilderMesh pb in selection)
			{
				if(pb.selectedFaceCount > 1)
				{
					success += pb.selectedFaceCount;

					Face face = MergeElements.Merge(pb, pb.selectedFacesInternal);

					pb.ToMesh();
					pb.Refresh();
					pb.Optimize();

					pb.SetSelectedFaces( new Face[] { face } );
				}
			}

			if(editor)
				ProBuilderEditor.Refresh();

			if(success > 0)
				return new ActionResult(ActionResult.Status.Success, "Merged " + success + " Faces");
			else
				return new ActionResult(ActionResult.Status.Failure, "Merge Faces\nNo Faces Selected");
		}

		/**
		 * Turn / flip / swap a quad connecting edge.
		 */
		public static ActionResult MenuFlipEdges(ProBuilderMesh[] selection)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RecordSelection(selection, "Flip Face Edges");
			int success = 0;
			int attempts = 0;

			foreach(ProBuilderMesh pb in selection)
			{
				foreach(Face face in pb.selectedFacesInternal)
				{
					if( pb.FlipEdge(face) )
						success++;
				}

				attempts++;

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			if(editor)
				ProBuilderEditor.Refresh();

			if(success > 0)
				return new ActionResult(ActionResult.Status.Success, "Flipped " + success + " Edges");
			else
				return new ActionResult(ActionResult.Status.Failure, string.Format("Flip Edges\n{0}", attempts > 0 ? "Faces Must Be Quads" : "No Faces Selected"));
		}
#endregion

#region Vertex Operations

#if !PROTOTYPE

		/**
		 * Collapse selected vertices
		 * ProBuilder only.
		 */
		public static ActionResult MenuCollapseVertices(ProBuilderMesh[] selection)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			bool success = false;

			bool collapseToFirst = PreferencesInternal.GetBool(PreferenceKeys.pbCollapseVertexToFirst);

			UndoUtility.RegisterCompleteObjectUndo(selection, "Collapse Vertices");

			foreach(ProBuilderMesh pb in selection)
			{
				if(pb.selectedIndicesInternal.Length > 1)
				{
					int newIndex = pb.MergeVertices(pb.selectedIndicesInternal, collapseToFirst);
					success = newIndex > -1;

					if(success)
					{
						pb.RemoveDegenerateTriangles();
						pb.SetSelectedVertices(new int[] { newIndex });
					}

					pb.ToMesh();
					pb.Refresh();
					pb.Optimize();
				}
			}


			if(editor)
				ProBuilderEditor.Refresh();

			if(success)
				return new ActionResult(ActionResult.Status.Success, "Collapse Vertices");
			else
				return new ActionResult(ActionResult.Status.Failure, "Collapse Vertices\nNo Vertices Selected");
		}

		/**
		 * Weld all selected vertices.
		 * ProBuilder only.
		 */
		public static ActionResult MenuWeldVertices(ProBuilderMesh[] selection)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			ActionResult res = ActionResult.NoSelection;

			UndoUtility.RegisterCompleteObjectUndo(selection, "Weld Vertices");
			float weld = PreferencesInternal.GetFloat(PreferenceKeys.pbWeldDistance);
			int weldCount = 0;

			foreach(ProBuilderMesh pb in selection)
			{
				weldCount += pb.sharedIndicesInternal.Length;

				if(pb.selectedIndicesInternal.Length > 1)
				{
					pb.ToMesh();

					int[] welds = pb.WeldVertices(pb.selectedIndicesInternal, weld);
					res = welds != null ? new ActionResult(ActionResult.Status.Success, "Weld Vertices") : new ActionResult(ActionResult.Status.Failure, "Failed Weld Vertices");

					if(res)
					{
						if(pb.RemoveDegenerateTriangles() != null)
						{
							pb.ToMesh();
							welds = new int[0];	// @todo
						}

						pb.SetSelectedVertices(welds ?? new int[0] {});
					}

					pb.Refresh();
					pb.Optimize();
				}

				weldCount -= pb.sharedIndicesInternal.Length;
			}

			ProBuilderEditor.Refresh();

			if(res && weldCount > 0)
				return new ActionResult(ActionResult.Status.Success, "Weld " + weldCount + (weldCount > 1 ? " Vertices" : " Vertex"));
			else
				return new ActionResult(ActionResult.Status.Failure, "Nothing to Weld");
		}

		const float k_MinWeldDistance = .0001f;

		/**
		 * Expose the distance parameter used in Weld operations.
		 * ProBuilder only.
		 */
		public static void WeldButtonGUI(int width)
		{
			EditorGUI.BeginChangeCheck();

			float weldDistance = PreferencesInternal.GetFloat(PreferenceKeys.pbWeldDistance);

			if(weldDistance <= k_MinWeldDistance)
				weldDistance = k_MinWeldDistance;

			EditorGUIUtility.labelWidth = width - 68;
			weldDistance = EditorGUILayout.FloatField(new GUIContent("Max", "The maximum distance between two vertices in order to be welded together."), weldDistance);

			if( EditorGUI.EndChangeCheck() )
			{
				if(weldDistance < k_MinWeldDistance)
					weldDistance = k_MinWeldDistance;
				PreferencesInternal.SetFloat(PreferenceKeys.pbWeldDistance, weldDistance);
			}
		}

		public static ActionResult MenuSplitVertices(ProBuilderMesh[] selection)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			int splitCount = 0;
			UndoUtility.RecordSelection(selection, "Split Vertices");

			foreach(ProBuilderMesh pb in selection)
			{
				List<int> tris = new List<int>(pb.selectedIndicesInternal);			// loose verts to split

				if(pb.selectedFacesInternal.Length > 0)
				{
					IntArray[] sharedIndices = pb.sharedIndicesInternal;

					int[] selTrisIndices = new int[pb.selectedIndicesInternal.Length];

					// Get sharedIndices index for each vert in selection
					for(int i = 0; i < pb.selectedIndicesInternal.Length; i++)
						selTrisIndices[i] = sharedIndices.IndexOf(pb.selectedIndicesInternal[i]);

					// cycle through selected faces and remove the tris that compose full faces.
					foreach(Face face in pb.selectedFacesInternal)
					{
						List<int> faceSharedIndices = new List<int>();

						for(int j = 0; j < face.distinctIndices.Length; j++)
							faceSharedIndices.Add( sharedIndices.IndexOf(face.distinctIndices[j]) );

						List<int> usedTris = new List<int>();
						for(int i = 0; i < selTrisIndices.Length; i++)
							if( faceSharedIndices.Contains(selTrisIndices[i]) )
								usedTris.Add(pb.selectedIndicesInternal[i]);

						// This face *is* composed of selected tris.  Remove these tris from the loose index list
						foreach(int i in usedTris)
							if(tris.Contains(i))
								tris.Remove(i);
					}
				}

				// Now split the faces, and any loose vertices
				pb.DetachFaces(pb.selectedFacesInternal);

				splitCount += pb.selectedIndicesInternal.Length;
				pb.SplitVertices(pb.selectedIndicesInternal);

				// Reattach detached face vertices (if any are to be had)
				if(pb.selectedFacesInternal.Length > 0)
					pb.WeldVertices(pb.selectedFacesInternal.SelectMany(x => x.ToTriangles()), Mathf.Epsilon);

				// And set the selected triangles to the newly split
				List<int> newTriSelection = new List<int>(pb.selectedFacesInternal.SelectMany(x => x.ToTriangles()));
				newTriSelection.AddRange(tris);
				pb.SetSelectedVertices(newTriSelection.ToArray());

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			ProBuilderEditor.Refresh();

			if(splitCount > 0)
				return new ActionResult(ActionResult.Status.Success, "Split " + splitCount + (splitCount > 1 ? " Vertices" : " Vertex"));
			else
				return new ActionResult(ActionResult.Status.Failure, "Split Vertices\nInsuffient Vertices Selected");
		}

		/**
		 *	Attempt to create polygons bridging any gaps in geometry.
		 */
		public static ActionResult MenuFillHole(ProBuilderMesh[] selection)
		{
			if(editor == null)
				return ActionResult.NoSelection;

			UndoUtility.RecordSelection(selection, "Fill Hole");

			ActionResult res = new ActionResult(ActionResult.Status.NoChange, "No Holes Found");
			int filled = 0;
			bool wholePath = PreferencesInternal.GetBool(PreferenceKeys.pbFillHoleSelectsEntirePath);

			foreach(ProBuilderMesh pb in selection)
			{
				bool selectAll = pb.selectedIndicesInternal == null || pb.selectedIndicesInternal.Length < 1;
				IEnumerable<int> indices = selectAll ? pb.facesInternal.SelectMany(x => x.ToTriangles()) : pb.selectedIndicesInternal;

				pb.ToMesh();

				Dictionary<int, int> lookup = pb.sharedIndicesInternal.ToDictionary();
				List<WingedEdge> wings = WingedEdge.GetWingedEdges(pb);
				HashSet<int> common = IntArrayUtility.GetCommonIndices(lookup, indices);
				List<List<WingedEdge>> holes = ElementSelection.FindHoles(wings, common);

				HashSet<Face> faces = new HashSet<Face>();
				List<Face> adjacent = new List<Face>();

				foreach(List<WingedEdge> hole in holes)
				{
					List<int> holeIndices;
					Face face;

					if(wholePath)
					{
						// if selecting whole path and in edge mode, make sure the path contains
						// at least one complete edge from the selection.
						if(	editor.selectionMode == SelectMode.Edge &&
							!hole.Any(x => common.Contains(x.edge.common.x) &&
							common.Contains(x.edge.common.y)))
							continue;

						holeIndices = hole.Select(x => x.edge.local.x).ToList();
						face = AppendElements.CreatePolygon(pb, holeIndices, false);
						adjacent.AddRange(hole.Select(x => x.face));
					}
					else
					{
						IEnumerable<WingedEdge> selected = hole.Where(x => common.Contains(x.edge.common.x));
						holeIndices = selected.Select(x => x.edge.local.x).ToList();
						face = AppendElements.CreatePolygon(pb, holeIndices, true);

						if(res)
							adjacent.AddRange(selected.Select(x => x.face));
					}

					if(face != null)
					{
						filled++;
						adjacent.Add(face);
						faces.Add(face);
					}
				}

				pb.SetSelectedFaces(faces);

				wings = WingedEdge.GetWingedEdges(pb, adjacent);

				// make sure the appended faces match the first adjacent face found
				// both in winding and face properties
				foreach(WingedEdge wing in wings)
				{
					if( faces.Contains(wing.face) )
					{
						faces.Remove(wing.face);

						foreach(WingedEdge p in wing)
						{
							if(p.opposite != null)
							{
								p.face.material = p.opposite.face.material;
								p.face.uv = new AutoUnwrapSettings(p.opposite.face.uv);
								MeshOps.SurfaceTopology.ConformOppositeNormal(p.opposite);
								break;
							}
						}
					}
				}

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			ProBuilderEditor.Refresh();

			if(filled > 0)
                res = new ActionResult(ActionResult.Status.Success, filled > 1 ? string.Format("Filled {0} Holes", filled) : "Fill Hole");
			return res;
		}
#endif
#endregion

#region Subdivide / Split

#if !PROTOTYPE

		/**
		 * Attempts to subdivide the selected objects.  If Edge or Face selection mode, splits at the
		 * center of the edge.  Otherwise from Vertex.
		 * ProBuilder only.
		 */
		public static ActionResult MenuSubdivide(ProBuilderMesh[] selection)
		{
			if(!editor || selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RegisterCompleteObjectUndo(selection, "Subdivide Selection");

			int success = 0;

			foreach(ProBuilderMesh pb in selection)
			{
				pb.ToMesh();

				if( pb.Subdivide() )
					success++;

				pb.Refresh();
				pb.Optimize();

				pb.SetSelectedVertices(new int[0]);
			}

			ProBuilderEditor.Refresh();

			return new ActionResult(ActionResult.Status.Success, "Subdivide " + selection.Length + " Objects");
		}

		/**
		 * Attempts to subdivide the selected object edges.
		 * ProBuilder only.
		 */
		public static ActionResult MenuSubdivideEdge(ProBuilderMesh[] selection)
		{
			if(!editor || selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			int subdivisions = PreferencesInternal.GetInt(PreferenceKeys.pbEdgeSubdivisions, 1);

			UndoUtility.RegisterCompleteObjectUndo(selection, "Subdivide Edges");

			ActionResult result = ActionResult.NoSelection;

			foreach(ProBuilderMesh pb in selection)
			{
				List<Edge> newEdgeSelection = AppendElements.AppendVerticesToEdge(pb, pb.selectedEdges, subdivisions);

				if (newEdgeSelection != null)
				{
					pb.SetSelectedEdges(newEdgeSelection);
					pb.ToMesh();
					pb.Refresh();
					pb.Optimize();
					result = new ActionResult(ActionResult.Status.Success, "Subdivide Edge");
				}
				else
				{
					result = new ActionResult(ActionResult.Status.Failure, "Failed Subdivide Edge");
				}
			}

			ProBuilderEditor.Refresh(true);

			return result;
		}

		/**
		 * Subdivides all currently selected faces.
		 * ProBuilder only.
		 */
		public static ActionResult MenuSubdivideFace(ProBuilderMesh[] selection)
		{
			if(!editor || selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			int success = 0;

			foreach(ProBuilderMesh pb in selection)
			{
				UndoUtility.RegisterCompleteObjectUndo(selection, "Subdivide Faces");

				Face[] faces = pb.Subdivide(pb.selectedFacesInternal);
				pb.ToMesh();

				if(faces != null)
				{
					success += pb.selectedFacesInternal.Length;
					pb.SetSelectedFaces(faces);

					pb.Refresh();
					pb.Optimize();
				}
			}

			if(success > 0)
			{
				ProBuilderEditor.Refresh();

				return new ActionResult(ActionResult.Status.Success, "Subdivide " + success + ((success > 1) ? " faces" : " face"));
			}
			else
			{
				Debug.LogWarning("Subdivide faces failed - did you not have any faces selected?");
				return new ActionResult(ActionResult.Status.Failure, "Subdivide Faces\nNo faces selected");
			}
		}

		public static ActionResult MenuConnectEdges(ProBuilderMesh[] selection)
		{
			ActionResult res = ActionResult.NoSelection;

			UndoUtility.RegisterCompleteObjectUndo(selection, "Connect Edges");

			foreach(ProBuilderMesh pb in selection)
			{
				Edge[] connections = pb.Connect(pb.selectedEdges).item2;

				if (connections != null)
				{
					pb.SetSelectedEdges(connections);
					pb.Refresh();
					pb.Optimize();

					res = new ActionResult(ActionResult.Status.Success, "Connected " + connections.Length + " Edges");
				}
			}

			ProBuilderEditor.Refresh();
			return res;
		}

		/**
		 * Connects all currently selected vertices.
		 * ProBuilder only.
		 */
		public static ActionResult MenuConnectVertices(ProBuilderMesh[] selection)
		{
			ActionResult res = ActionResult.NoSelection;

			UndoUtility.RegisterCompleteObjectUndo(selection, "Connect Vertices");

			foreach(ProBuilderMesh pb in selection)
			{
				pb.ToMesh();
				int[] splits = pb.Connect(pb.selectedIndicesInternal);

				if(splits != null)
				{
					pb.Refresh();
					pb.Optimize();
					pb.SetSelectedVertices(splits);
					res = new ActionResult(ActionResult.Status.Success, "Connect Edges");
				}
				else
				{
					res = new ActionResult(ActionResult.Status.Failure, "Failed Connecting Edges");
				}
			}
			ProBuilderEditor.Refresh();

			return res;
		}

		/**
		 * Inserts an edge loop along currently selected Edges.
		 * ProBuilder only.
		 */
		public static ActionResult MenuInsertEdgeLoop(ProBuilderMesh[] selection)
		{
			if(!editor || selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			int success = 0;
			UndoUtility.RegisterCompleteObjectUndo(selection, "Insert Edge Loop");

			foreach(ProBuilderMesh pb in selection)
			{
				Edge[] edges = pb.Connect(ElementSelection.GetEdgeRing(pb, pb.selectedEdges)).item2;

				if(edges != null)
				{
					pb.SetSelectedEdges(edges);
					pb.ToMesh();
					pb.Refresh();
					pb.Optimize();
					success++;
				}
			}

			ProBuilderEditor.Refresh();

			if(success > 0)
				return new ActionResult(ActionResult.Status.Success, "Insert Edge Loop");
			else
				return new ActionResult(ActionResult.Status.Success, "Insert Edge Loop");
		}

#endif
#endregion
	}
}

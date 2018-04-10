using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.MeshOperations;
using Object = UnityEngine.Object;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Contains Menu commands for most ProBuilder operations. Will also attempt to Update the pb_Editor.
	/// </summary>
	class pb_MenuCommands : UnityEditor.Editor
	{
		private static pb_Editor editor { get { return pb_Editor.instance; } }

#if PROTOTYPE
		public static pb_ActionResult MenuMergeObjects(pb_Object[] selection) { Debug.LogWarning("MenuMergeObjects is a ProBuilder Advanced feature.");  return new pb_ActionResult(Status.Failure, "ProBuilder Advanced Feature"); }
		public static pb_ActionResult MenuFacetizeObject(pb_Object[] selection) { Debug.LogWarning("MenuFacetizeObject is a ProBuilder Advanced feature.");  return new pb_ActionResult(Status.Failure, "ProBuilder Advanced Feature"); }
		public static pb_ActionResult MenuSubdivide(pb_Object[] selection) { Debug.LogWarning("MenuSubdivide is a ProBuilder Advanced feature.");  return new pb_ActionResult(Status.Failure, "ProBuilder Advanced Feature"); }
		public static pb_ActionResult MenuDetachFaces(pb_Object[] selection) { Debug.LogWarning("MenuDetachFaces is a ProBuilder Advanced feature.");  return new pb_ActionResult(Status.Failure, "ProBuilder Advanced Feature"); }
		public static pb_ActionResult MenuSubdivideFace(pb_Object[] selection) { Debug.LogWarning("MenuSubdivideFace is a ProBuilder Advanced feature.");  return new pb_ActionResult(Status.Failure, "ProBuilder Advanced Feature"); }
		public static pb_ActionResult MenuSubdivideEdge(pb_Object[] selection) { Debug.LogWarning("MenuSubdivideEdge is a ProBuilder Advanced feature.");  return new pb_ActionResult(Status.Failure, "ProBuilder Advanced Feature"); }
		public static pb_ActionResult MenuBridgeEdges(pb_Object[] selection) { Debug.LogWarning("MenuBridgeEdges is a ProBuilder Advanced feature.");  return new pb_ActionResult(Status.Failure, "ProBuilder Advanced Feature"); }
		public static pb_ActionResult MenuConnectEdges(pb_Object[] selection) { Debug.LogWarning("MenuConnectEdges is a ProBuilder Advanced feature.");  return new pb_ActionResult(Status.Failure, "ProBuilder Advanced Feature"); }
		public static pb_ActionResult MenuConnectVertices(pb_Object[] selection) { Debug.LogWarning("MenuConnectVertices is a ProBuilder Advanced feature.");  return new pb_ActionResult(Status.Failure, "ProBuilder Advanced Feature"); }
		public static pb_ActionResult MenuInsertEdgeLoop(pb_Object[] selection) { Debug.LogWarning("MenuInsertEdgeLoop is a ProBuilder Advanced feature.");  return new pb_ActionResult(Status.Failure, "ProBuilder Advanced Feature"); }
		public static pb_ActionResult MenuWeldVertices(pb_Object[] selection) { Debug.LogWarning("MenuWeldVertices is a ProBuilder Advanced feature.");  return new pb_ActionResult(Status.Failure, "ProBuilder Advanced Feature"); }
		public static pb_ActionResult MenuCollapseVertices(pb_Object[] selection) { Debug.LogWarning("MenuCollapseVertices is a ProBuilder Advanced feature.");  return new pb_ActionResult(Status.Failure, "ProBuilder Advanced Feature"); }
		public static pb_ActionResult MenuSplitVertices(pb_Object[] selection) { Debug.LogWarning("MenuSplitVertices is a ProBuilder Advanced feature.");  return new pb_ActionResult(Status.Failure, "ProBuilder Advanced Feature"); }
		public static pb_ActionResult MenuBevelEdges(pb_Object[] selection) { Debug.LogWarning("MenuBevelEdges is a ProBuilder Advanced feature.");  return new pb_ActionResult(Status.Failure, "ProBuilder Advanced Feature"); }
		public static pb_ActionResult MenuFillHole(pb_Object[] selection) { Debug.LogWarning("MenuFillHole is a ProBuilder Advanced feature.");  return new pb_ActionResult(Status.Failure, "ProBuilder Advanced Feature"); }
		public static pb_ActionResult WeldButtonGUI(int width) { Debug.LogWarning("WeldButtonGUI is a ProBuilder Advanced feature.");  return new pb_ActionResult(Status.Failure, "ProBuilder Advanced Feature"); }
#endif
#region Object Level

#if !PROTOTYPE
		/**
		 * Combine selected pb_Objects to a single object.
		 * ProBuilder only.
		 */
		public static pb_ActionResult MenuMergeObjects(pb_Object[] selected)
		{
			if(selected.Length < 2)
				return new pb_ActionResult(Status.Canceled, "Must Select 2+ Objects");

			pb_Object pb = null;

			if( pb_MeshOps.CombineObjects(selected, out pb) )
			{
//				pb_EntityUtility.SetEntityType(selected[0].GetComponent<pb_Entity>().entityType, pb.gameObject);

				pb.Optimize();

				pb.gameObject.name = "pb-MergedObject" + pb.id;

				// Delete donor objects
				for(int i = 0; i < selected.Length; i++)
				{
					if(selected[i] != null)
						pb_Undo.DestroyImmediate(selected[i].gameObject, "Delete Merged Objects");
				}

				pb_Undo.RegisterCreatedObjectUndo(pb.gameObject, "Merge Objects");

				Selection.activeTransform = pb.transform;
			}

			if(editor)
				editor.UpdateSelection();

			return new pb_ActionResult(Status.Success, "Merged Objects");
		}
#endif

		/**
		 * Set the pivot to the center of the current element selection.
		 * ProBuilder only.
		 */
		public static pb_ActionResult MenuCenterPivot(pb_Object[] selection)
		{
			return _SetPivot(selection, null);
		}

		public static pb_ActionResult MenuSetPivot(pb_Object[] selection)
		{
			int[][] tri = new int[selection.Length][];

			for(int i = 0; i < tri.Length; i++)
				tri[i] = selection[i].SelectedTriangles;

			return _SetPivot(selection, tri);
		}

		private static pb_ActionResult _SetPivot(pb_Object[] selection, int[][] triangles = null)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			Object[] objects = new Object[selection.Length * 2];
			System.Array.Copy(selection, 0, objects, 0, selection.Length);
			for(int i = selection.Length; i < objects.Length; i++)
				objects[i] = selection[i-selection.Length].transform;

			pb_Undo.RegisterCompleteObjectUndo(objects, "Set Pivot");

			for(int i = 0; i < selection.Length; i++)
			{
				pb_TransformUtil.UnparentChildren(selection[i].transform);

				if(triangles != null)
					selection[i].CenterPivot(triangles[i]);
				else
					selection[i].CenterPivot(null);

				selection[i].Optimize();

				pb_TransformUtil.ReparentChildren(selection[i].transform);
			}

			SceneView.RepaintAll();

			if(editor != null)
				editor.UpdateSelection();

			return new pb_ActionResult(Status.Success, "Set Pivot");
		}

		public static pb_ActionResult MenuFreezeTransforms(pb_Object[] selection)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			List<Object> undoables = new List<Object>( selection.Select(x => (Object) x.transform) );
			undoables.AddRange(selection);
			pb_Undo.RecordObjects(undoables.ToArray(), "Freeze Transforms");

			Vector3[][] vertices = new Vector3[selection.Length][];

			for(int i = 0; i < selection.Length; i++)
				vertices[i] = selection[i].VerticesInWorldSpace();

			for(int i = 0; i < selection.Length; i++)
			{
				pb_Object pb = selection[i];

				pb.transform.position = Vector3.zero;
				pb.transform.rotation = Quaternion.identity;
				pb.transform.localScale = Vector3.one;

				foreach(pb_Face face in pb.faces)
					face.manualUV = true;

				pb.SetVertices(vertices[i]);

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			pb_Editor.Refresh();

			SceneView.RepaintAll();

			return new pb_ActionResult(Status.Success, "Freeze Transforms");
		}

		/// <summary>
		/// Set the pb_Entity entityType on selection.
		/// </summary>
		/// <param name="selection"></param>
		/// <param name="entityType"></param>
		/// <returns></returns>
		[Obsolete("pb_Entity is obsolete.")]
		public static pb_ActionResult MenuSetEntityType(pb_Object[] selection, EntityType entityType)
		{
			if(selection.Length < 1)
				return pb_ActionResult.NoSelection;

			Object[] undoObjects = selection.SelectMany(x => x.GetComponents<Component>()).ToArray();

			pb_Undo.RecordObjects(undoObjects, "Set Entity Type");

			foreach(pb_Object pb in selection)
			{
				pb_EntityUtility.SetEntityType(entityType, pb.gameObject);
				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			return new pb_ActionResult(Status.Success, "Set " + entityType);
		}

		/**
		 *	Open the vertex color editor (palette or painter) based on prefs.
		 */
		public static void MenuOpenVertexColorsEditor()
		{
			switch( pb_PreferencesInternal.GetEnum<VertexColorTool>(pb_Constant.pbVertexColorTool) )
			{
				case VertexColorTool.Palette:
					pb_VertexColorPalette.MenuOpenWindow();
					break;

				default:
					pb_VertexColorPainter.MenuOpenWindow();
					break;
			}
		}

		/**
		 *	Open the vertex coloring editor as stored by user prefs.
		 */
		public static pb_ActionResult MenuOpenVertexColorsEditor2(pb_Object[] selection)
		{
			switch( pb_PreferencesInternal.GetEnum<VertexColorTool>(pb_Constant.pbVertexColorTool) )
			{
				case VertexColorTool.Palette:
					pb_VertexColorPalette.MenuOpenWindow();
					break;

				default:
					pb_VertexColorPainter.MenuOpenWindow();
					break;
			}

			return new pb_ActionResult(Status.Success, "Open Vertex Colors Editor");
		}

		public static void VertexColorsGUI(int width)
		{
			VertexColorTool tool = pb_PreferencesInternal.GetEnum<VertexColorTool>(pb_Constant.pbVertexColorTool);
			VertexColorTool prev = tool;

			GUILayout.Label("Color Editor");

			tool = (VertexColorTool) EditorGUILayout.EnumPopup("", tool, GUILayout.MaxWidth(width-14));

			if(prev != tool)
				pb_PreferencesInternal.SetInt(pb_Constant.pbVertexColorTool, (int)tool);
		}
#if !PROTOTYPE

		public static pb_ActionResult MenuFacetizeObject(pb_Object[] selection)
		{
			if(!editor || selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_Undo.RegisterCompleteObjectUndo(selection, "Triangulate Objects");

			for(int i = 0; i < selection.Length; i++)
			{
				pb_Face[] splits = null;
				selection[i].ToMesh();
				selection[i].ToTriangles(selection[i].faces, out splits);
				selection[i].Refresh();
				selection[i].Optimize();
			}

			editor.UpdateSelection();

			return new pb_ActionResult(Status.Success, "Triangulate " + selection.Length + (selection.Length > 1 ? " Objects" : " Object"));
		}

		enum BooleanOperation
		{
			Union,
			Subtract,
			Intersect
		}

		static pb_ActionResult MenuBooleanOperation(BooleanOperation operation, pb_Object lhs, pb_Object rhs)
		{
			if(lhs == null || rhs == null)
				return new pb_ActionResult(Status.Failure, "Must Select 2 Objects");

			string op_string = operation == BooleanOperation.Union ? "Union" : (operation == BooleanOperation.Subtract ? "Subtract" : "Intersect");

			pb_Object[] sel = new pb_Object[] { lhs, rhs };

			pb_Undo.RecordSelection(sel, op_string);

			Mesh c;

			switch(operation)
			{
				case BooleanOperation.Union:
					c = Parabox.CSG.CSG.Union(lhs.gameObject, rhs.gameObject);
					break;

				case BooleanOperation.Subtract:
					c = Parabox.CSG.CSG.Subtract(lhs.gameObject, rhs.gameObject);
					break;

				default:
					c = Parabox.CSG.CSG.Intersect(lhs.gameObject, rhs.gameObject);
					break;
			}

			GameObject go = new GameObject();

			go.AddComponent<MeshRenderer>().sharedMaterial = pb_PreferencesInternal.GetMaterial(pb_Constant.pbDefaultMaterial);
			go.AddComponent<MeshFilter>().sharedMesh = c;

			pb_Object pb = pb_MeshOps.CreatePbObjectWithTransform(go.transform, false);
			DestroyImmediate(go);

			Selection.objects = new Object[] { pb.gameObject };

			return new pb_ActionResult(Status.Success, op_string);
		}

		/**
		 * Union operation between two ProBuilder objects.
		 */
		public static pb_ActionResult MenuUnion(pb_Object lhs, pb_Object rhs)
		{
			return MenuBooleanOperation(BooleanOperation.Union, lhs, rhs);
		}

		/**
		 * Subtract boolean operation between two pb_Objects.
		 */
		public static pb_ActionResult MenuSubtract(pb_Object lhs, pb_Object rhs)
		{
			return MenuBooleanOperation(BooleanOperation.Subtract, lhs, rhs);
		}

		/**
		 * Intersect boolean operation between two pb_Objects.
		 */
		public static pb_ActionResult MenuIntersect(pb_Object lhs, pb_Object rhs)
		{
			return MenuBooleanOperation(BooleanOperation.Intersect, lhs, rhs);
		}
#endif

#endregion

#region Normals

		/**
		 *	Reverse the direction of all faces on each object.
		 */
		public static pb_ActionResult MenuFlipObjectNormals(pb_Object[] selected)
		{
			if(selected == null || selected.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_Undo.RecordSelection(pb_Util.GetComponents<pb_Object>(Selection.transforms), "Flip Object Normals");

			foreach(pb_Object pb in selected)
			{
				pb.ReverseWindingOrder(pb.faces);
				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			return new pb_ActionResult(Status.Success, "Flip Object Normals");
		}

		/**
		 * Flips all face normals if editLevel == EditLevel.Top, else flips only pb_Object->SelectedFaces
		 */
		public static pb_ActionResult MenuFlipNormals(pb_Object[] selected)
		{
			if(selected == null || selected.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_Undo.RecordSelection(pb_Util.GetComponents<pb_Object>(Selection.transforms), "Flip Face Normals");

			int c = 0;
			int faceCount = pb_Editor.instance.selectedFaceCount;

			foreach(pb_Object pb in pb_Util.GetComponents<pb_Object>(Selection.transforms))
			{
				if( pb.SelectedFaceCount < 1 && faceCount < 1 )
				{
					pb.ReverseWindingOrder(pb.faces);
					c += pb.faces.Length;
				}
				else
				{
					pb.ReverseWindingOrder(pb.SelectedFaces);
					c += pb.SelectedFaceCount;
				}


				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			if(c > 0)
				return new pb_ActionResult(Status.Success, "Flip " + c + (c > 1 ? " Face Normals" : " Face Normal"));
			else
				return new pb_ActionResult(Status.Canceled, "Flip Normals\nNo Faces Selected");
		}

		/**
		 * Attempt to make face normals uniform.
		 */
		public static pb_ActionResult MenuConformObjectNormals(pb_Object[] selection)
		{
			return DoConformNormals(selection, false);
		}

		public static pb_ActionResult MenuConformNormals(pb_Object[] selection)
		{
			return DoConformNormals(selection, true);
		}

		public static pb_ActionResult DoConformNormals(pb_Object[] selection, bool perFace = true)
		{
			if(!editor || selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_Undo.RecordSelection(selection, "Conform " + (editor.selectedFaceCount > 0 ? "Face" : "Object") + " Normals.");

			pb_ActionResult res = pb_ActionResult.NoSelection;

			foreach(pb_Object pb in selection)
			{
				pb_Face[] faces = perFace ? pb.SelectedFaces : pb.faces;

				if(faces == null)
					continue;

				res = pb_ConformNormals.ConformNormals(pb, faces);

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			pb_Editor.Refresh();

			return res;
		}
#endregion

#region Extrude / Bridge

		public static void ExtrudeButtonGUI(int width)
		{
			float extrudeAmount = pb_PreferencesInternal.HasKey(pb_Constant.pbExtrudeDistance) ? pb_PreferencesInternal.GetFloat(pb_Constant.pbExtrudeDistance) : .5f;
			bool extrudeAsGroup = pb_PreferencesInternal.GetBool(pb_Constant.pbExtrudeAsGroup);

			EditorGUI.BeginChangeCheck();

			EditorGUIUtility.labelWidth = width - 28;
			extrudeAsGroup = EditorGUILayout.Toggle("As Group", extrudeAsGroup);

			EditorGUIUtility.labelWidth = width - 68;
			extrudeAmount = EditorGUILayout.FloatField("Dist", extrudeAmount, GUILayout.MaxWidth(width-12));

			if(EditorGUI.EndChangeCheck())
			{
				pb_PreferencesInternal.SetFloat(pb_Constant.pbExtrudeDistance, extrudeAmount);
				pb_PreferencesInternal.SetBool(pb_Constant.pbExtrudeAsGroup, extrudeAsGroup);
			}
		}

		/**
		 * Infers the correct context and extrudes the selected elements.
		 */
		public static pb_ActionResult MenuExtrude(pb_Object[] selection, bool enforceCurrentSelectionMode = false)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_Undo.RegisterCompleteObjectUndo(selection, "Extrude");

			int extrudedFaceCount = 0;
			bool success = false;

			foreach(pb_Object pb in selection)
			{
				pb.ToMesh();
				pb.Refresh(RefreshMask.Normals);

				if(editor && editor.selectionMode == SelectMode.Edge)
				{
					if(pb.SelectedEdges.Length < 1 || (!enforceCurrentSelectionMode && pb.SelectedFaces.Length > 0))
					{
						success = false;
					}
					else
					{
						extrudedFaceCount += pb.SelectedEdges.Length;
						pb_Edge[] newEdges;

						success = pb.Extrude(	pb.SelectedEdges,
												pb_PreferencesInternal.GetFloat(pb_Constant.pbExtrudeDistance),
												pb_PreferencesInternal.GetBool(pb_Constant.pbExtrudeAsGroup),
												pb_PreferencesInternal.GetBool(pb_Constant.pbManifoldEdgeExtrusion),
												out newEdges);

						if(success)
							pb.SetSelectedEdges(newEdges);
						else
							extrudedFaceCount -= pb.SelectedEdges.Length;
					}

					pb.ToMesh();
				}

				if((editor.selectionMode == SelectMode.Face || (!enforceCurrentSelectionMode && !success)) && pb.SelectedFaces.Length > 0)
				{
					extrudedFaceCount += pb.SelectedFaces.Length;

					pb.Extrude(	pb.SelectedFaces,
								pb_PreferencesInternal.GetEnum<ExtrudeMethod>(pb_Constant.pbExtrudeMethod),
								pb_PreferencesInternal.GetFloat(pb_Constant.pbExtrudeDistance));

					pb.SetSelectedFaces(pb.SelectedFaces);

					pb.ToMesh();
				}

				pb.Refresh();
				pb.Optimize();
			}

			if(editor != null)
				editor.UpdateSelection();

			SceneView.RepaintAll();

			if( extrudedFaceCount > 0 )
				return new pb_ActionResult(Status.Success, "Extrude");
			else
				return new pb_ActionResult(Status.Canceled, "Extrude\nEmpty Selection");
		}

#if !PROTOTYPE
		/**
		 * Create a face between two edges.
		 */
		public static pb_ActionResult MenuBridgeEdges(pb_Object[] selection)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_Undo.RecordSelection(selection, "Bridge Edges");

			bool success = false;
			bool limitToPerimeterEdges = pb_PreferencesInternal.GetBool(pb_Constant.pbPerimeterEdgeBridgeOnly);

			foreach(pb_Object pb in selection)
			{
				if(pb.SelectedEdges.Length == 2)
				{
					if(pb.Bridge(pb.SelectedEdges[0], pb.SelectedEdges[1], limitToPerimeterEdges))
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
				pb_Editor.Refresh();
				return new pb_ActionResult(Status.Success, "Bridge Edges");
			}
			else
			{
				Debug.LogWarning("Failed Bridge Edges.  Bridge Edges requires that only 2 edges be selected, and they must both only have one connecting face (non-manifold).");
				return new pb_ActionResult(Status.Failure, "Bridge Edges requires that only 2 edges be selected, and they must both only have one connecting face (non-manifold).");
			}
		}

		/**
		 * Bevel selected edges.
		 */
		public static pb_ActionResult MenuBevelEdges(pb_Object[] selection)
		{
			pb_ActionResult res = pb_ActionResult.NoSelection;
			pb_Undo.RecordSelection(selection, "Bevel Edges");

			float amount = pb_PreferencesInternal.GetFloat(pb_Constant.pbBevelAmount);

			foreach(pb_Object pb in selection)
			{
				pb.ToMesh();

				List<pb_Face> faces;
				res = pb_Bevel.BevelEdges(pb, pb.SelectedEdges, amount, out faces);

				if(res)
					pb.SetSelectedFaces(faces);

				pb.Refresh();
				pb.Optimize();
			}

			pb_Editor.Refresh();

			return res;
		}
#endif
#endregion

#region Selection

		private static void GetSelectedElementCount(pb_Object[] selection, out int sel, out int max)
		{
			switch(editor.selectionMode)
			{
				case SelectMode.Face:
					sel = selection.Sum(x => x.SelectedFaceCount);
					max = selection.Sum(x => x.faceCount);
					break;

				case SelectMode.Edge:
					sel = selection.Sum(x => x.SelectedEdgeCount);
					max = selection.Sum(x => x.faces.Sum(y=>y.edges.Length));
					break;

				default:
					sel = selection.Sum(x => x.SelectedTriangleCount);
					max = selection.Sum(x => x.triangleCount);
					break;
			}
		}

		public static bool VerifyGrowSelection(pb_Object[] selection)
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
		public static pb_ActionResult MenuGrowSelection(pb_Object[] selection)
		{
			if(!editor || selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_Undo.RecordSelection(selection, "Grow Selection");

			int grown = 0;
			bool angleGrow = pb_PreferencesInternal.GetBool(pb_Constant.pbGrowSelectionUsingAngle);
			bool iterative = pb_PreferencesInternal.GetBool(pb_Constant.pbGrowSelectionAngleIterative);
			float growSelectionAngle = pb_PreferencesInternal.GetFloat(pb_Constant.pbGrowSelectionAngle);

			if(!angleGrow && !iterative)
				iterative = true;

			foreach(pb_Object pb in pb_Util.GetComponents<pb_Object>(Selection.transforms))
			{
				int previousTriCount = pb.SelectedTriangleCount;

				switch( editor != null ? editor.selectionMode : (SelectMode)0 )
				{
					case SelectMode.Vertex:
						pb.SetSelectedEdges(pb_MeshUtils.GetConnectedEdges(pb, pb.SelectedTriangles));
						break;

					case SelectMode.Edge:
						pb.SetSelectedEdges(pb_MeshUtils.GetConnectedEdges(pb, pb.SelectedTriangles));
						break;

					case SelectMode.Face:

						pb_Face[] selectedFaces = pb.SelectedFaces;

						HashSet<pb_Face> sel;

						if(iterative)
						{
							sel = pb_GrowShrink.GrowSelection(pb, selectedFaces, angleGrow ? growSelectionAngle : -1f);
							sel.UnionWith(selectedFaces);
						}
						else
						{
							sel = pb_GrowShrink.FloodSelection(pb, selectedFaces, angleGrow ? growSelectionAngle : -1f);
						}

						pb.SetSelectedFaces( sel.ToArray() );

						break;
				}

				grown += pb.SelectedTriangleCount - previousTriCount;
			}

			pb_Editor.Refresh();
			SceneView.RepaintAll();

			if(grown > 0)
				return new pb_ActionResult(Status.Success, "Grow Selection");
			else
				return new pb_ActionResult(Status.Failure, "Nothing to Grow");
		}

		public static bool VerifyShrinkSelection(pb_Object[] selection)
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

		/**
		 * Shrink selection.
		 */
		public static pb_ActionResult MenuShrinkSelection(pb_Object[] selection)
		{
			// @TODO
			if(editor == null)
				return new pb_ActionResult(Status.Canceled, "ProBuilder Editor Not Open!");
			else if (selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_Undo.RecordSelection(selection, "Shrink Selection");

			// find perimeter edges
			int rc = 0;
			for(int i = 0; i < selection.Length; i++)
			{
				pb_Object pb = selection[i];

				switch(editor.selectionMode)
				{
					case SelectMode.Edge:
					{
						int[] perimeter = pb_MeshUtils.GetPerimeterEdges(pb, pb.SelectedEdges);
						pb.SetSelectedEdges( pb.SelectedEdges.RemoveAt(perimeter) );
						rc += perimeter != null ? perimeter.Length : 0;
						break;
					}

					case SelectMode.Face:
					{
						pb_Face[] perimeter = pb_MeshUtils.GetPerimeterFaces(pb, pb.SelectedFaces).ToArray();
						pb.SetSelectedFaces( pb.SelectedFaces.Except(perimeter).ToArray() );
						rc += perimeter != null ? perimeter.Length : 0;
						break;
					}

					case SelectMode.Vertex:
					{
						int[] perimeter = pb_MeshUtils.GetPerimeterVertices(pb, pb.SelectedTriangles, editor.SelectedUniversalEdges[i]);
						pb.SetSelectedTriangles( pb.SelectedTriangles.RemoveAt(perimeter) );
						rc += perimeter != null ? perimeter.Length : 0;
						break;
					}
				}

			}

			if(editor)
				editor.UpdateSelection(false);

			if( rc > 0 )
				return new pb_ActionResult(Status.Success, "Shrink Selection");
			else
				return new pb_ActionResult(Status.Canceled, "Nothing to Shrink");
		}

		public static bool VerifyInvertSelection(pb_Object[] selection)
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
		public static pb_ActionResult MenuInvertSelection(pb_Object[] selection)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_Undo.RecordSelection(selection, "Invert Selection");

			switch( editor != null ? editor.selectionMode : (SelectMode)0 )
			{
				case SelectMode.Vertex:
					foreach(pb_Object pb in selection)
					{
						pb_IntArray[] sharedIndices = pb.sharedIndices;
						List<int> selSharedIndices = new List<int>();

						foreach(int i in pb.SelectedTriangles)
							selSharedIndices.Add( sharedIndices.IndexOf(i) );

						List<int> inverse = new List<int>();

						for(int i = 0; i < sharedIndices.Length; i++)
						{
							if(!selSharedIndices.Contains(i))
								inverse.Add(sharedIndices[i][0]);
						}

						pb.SetSelectedTriangles(inverse.ToArray());
					}
					break;

				case SelectMode.Face:
					foreach(pb_Object pb in selection)
					{
						IEnumerable<pb_Face> inverse = pb.faces.Where( x => !pb.SelectedFaces.Contains(x) );
						pb.SetSelectedFaces(inverse.ToArray());
					}
					break;

				case SelectMode.Edge:

					if(!editor) break;

					for(int i = 0; i < selection.Length; i++)
					{
						pb_Edge[] universal_selected_edges = pb_EdgeExtension.GetUniversalEdges(selection[i].SelectedEdges, selection[i].sharedIndices).Distinct().ToArray();
						pb_Edge[] inverse_universal = System.Array.FindAll(editor.SelectedUniversalEdges[i], x => !universal_selected_edges.Contains(x));
						pb_Edge[] inverse = new pb_Edge[inverse_universal.Length];

						for(int n = 0; n < inverse_universal.Length; n++)
							inverse[n] = new pb_Edge( selection[i].sharedIndices[inverse_universal[n].x][0], selection[i].sharedIndices[inverse_universal[n].y][0] );

						selection[i].SetSelectedEdges(inverse);
					}
					break;
			}

			pb_Editor.Refresh();
			SceneView.RepaintAll();

			return new pb_ActionResult(Status.Success, "Invert Selection");
		}

		public static bool VerifyEdgeRingLoop(pb_Object[] selection)
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
		public static pb_ActionResult MenuRingSelection(pb_Object[] selection)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_Undo.RecordSelection(selection, "Select Edge Ring");

			bool success = false;

			foreach(pb_Object pb in pb_Util.GetComponents<pb_Object>(Selection.transforms))
			{
				pb_Edge[] edges = pb_MeshUtils.GetEdgeRing(pb, pb.SelectedEdges).ToArray();

				if(edges.Length > pb.SelectedEdges.Length)
					success = true;

				pb.SetSelectedEdges( edges );
			}

			if(editor)
				editor.UpdateSelection(false);

			SceneView.RepaintAll();

			if(success)
				return new pb_ActionResult(Status.Success, "Select Edge Ring");
			else
				return new pb_ActionResult(Status.Failure, "Nothing to Ring");
		}

		/**
		 * Selects an edge loop.
		 */
		public static pb_ActionResult MenuLoopSelection(pb_Object[] selection)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_Undo.RecordSelection(selection, "Select Edge Loop");

			bool foundLoop = false;

			foreach(pb_Object pb in selection)
			{
				pb_Edge[] loop;
				bool success = pb_MeshUtils.GetEdgeLoop(pb, pb.SelectedEdges, out loop);
				if(success)
				{
					if(loop.Length > pb.SelectedEdges.Length)
						foundLoop = true;

					pb.SetSelectedEdges(loop);
				}
			}

			if(editor)
				editor.UpdateSelection(false);

			SceneView.RepaintAll();

			if(foundLoop)
				return new pb_ActionResult(Status.Success, "Select Edge Loop");
			else
				return new pb_ActionResult(Status.Failure, "Nothing to Loop");
		}

		public static pb_ActionResult MenuLoopFaces(pb_Object[] selection)
		{
			pb_Undo.RecordSelection(selection, "Select Face Loop");

			foreach (pb_Object pb in selection)
			{
				HashSet<pb_Face> loop = pb_FaceLoop.GetFaceLoop(pb, pb.SelectedFaces);
				pb.SetSelectedFaces(loop);
			}

			pb_Editor.Refresh();

			return new pb_ActionResult(Status.Success, "Select Face Loop");
		}

		public static pb_ActionResult MenuRingFaces(pb_Object[] selection)
		{
			pb_Undo.RecordSelection(selection, "Select Face Ring");

			foreach (pb_Object pb in selection)
			{
				HashSet<pb_Face> loop = pb_FaceLoop.GetFaceLoop(pb, pb.SelectedFaces, true);
				pb.SetSelectedFaces(loop);
			}

			pb_Editor.Refresh();

			return new pb_ActionResult(Status.Success, "Select Face Ring");
		}

		public static pb_ActionResult MenuRingAndLoopFaces(pb_Object[] selection)
		{
			pb_Undo.RecordSelection(selection, "Select Face Ring and Loop");

			foreach (pb_Object pb in selection)
			{
				HashSet<pb_Face> loop = pb_FaceLoop.GetFaceRingAndLoop(pb, pb.SelectedFaces);
				pb.SetSelectedFaces(loop);
			}

			pb_Editor.Refresh();

			return new pb_ActionResult(Status.Success, "Select Face Ring and Loop");
		}
#endregion

#region Delete / Detach

		/**
		 * Delete selected faces.
		 * ProBuilder only.
		 */
		public static pb_ActionResult MenuDeleteFace(pb_Object[] selection)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_Undo.RegisterCompleteObjectUndo(selection, "Delete Face");

			int count = 0;

			foreach(pb_Object pb in selection)
			{
				if(pb.SelectedFaceCount == pb.faces.Length)
				{
					Debug.LogWarning("Attempting to delete all faces on this mesh...  I'm afraid I can't let you do that.");
					continue;
				}

				pb.DeleteFaces(pb.SelectedFaces);
				count += pb.SelectedFaceCount;

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			if(editor)
			{
				editor.ClearElementSelection();
				editor.UpdateSelection();
			}

			if(count > 0)
				return new pb_ActionResult(Status.Success, "Delete " + count + " Faces");
			else
				return new pb_ActionResult(Status.Failure, "No Faces Selected");
		}

#if !PROTOTYPE

		public static pb_ActionResult MenuDetachFaces(pb_Object[] selection)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			bool detachToNewObject = pb_PreferencesInternal.GetBool(pb_Constant.pbDetachToNewObject);

			if(detachToNewObject)
				return MenuDetachFacesToObject(selection);
			else
				return MenuDetachFacesToSubmesh(selection);
		}

		/**
		 * Detach selected faces to submesh.
		 * ProBuilder only.
		 */
		public static pb_ActionResult MenuDetachFacesToSubmesh(pb_Object[] selection)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_Undo.RegisterCompleteObjectUndo(selection, "Detach Face(s)");

			int count = 0;

			foreach(pb_Object pb in selection)
			{
				pb.ToMesh();
				List<pb_Face> res = pb.DetachFaces(pb.SelectedFaces);
				pb.Refresh();
				pb.Optimize();

				pb.SetSelectedFaces(res.ToArray());

				count += pb.SelectedFaceCount;
			}

			if(editor)
				editor.UpdateSelection();

			if(count > 0)
				return new pb_ActionResult(Status.Success, "Detach " + count + (count > 1 ? " Faces" : " Face"));
			else
				return new pb_ActionResult(Status.Success, "Detach Faces");
		}

		/**
		 * Detaches currently selected faces to a new ProBuilder object.
		 * ProBuilder only.
		 */
		public static pb_ActionResult MenuDetachFacesToObject(pb_Object[] selection)
		{
			if(!editor || selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_Undo.RegisterCompleteObjectUndo(selection, "Detach Selection to PBO");

			int detachedFaceCount = 0;
			List<GameObject> detached = new List<GameObject>();

			foreach(pb_Object pb in selection)
			{
				if(pb.SelectedFaceCount < 1 || pb.SelectedFaceCount == pb.faces.Length)
					continue;

				// work with face indices here 'cause copying breaks the face ref
				int[] primary = pb.SelectedFaces.Select(x => System.Array.IndexOf((Array) pb.faces, x)).ToArray();

				detachedFaceCount += primary.Length;

				List<int> inverse_list = new List<int>();
				for(int i = 0; i < pb.faces.Length; i++)
					if(System.Array.IndexOf(primary, i) < 0)
						inverse_list.Add(i);

				int[] inverse = inverse_list.ToArray();

				pb_Object copy = ((GameObject)GameObject.Instantiate(pb.gameObject)).GetComponent<pb_Object>();
				copy.MakeUnique();

				// if is prefab, break connection and destroy children
				if( pb_EditorUtility.IsPrefabInstance(copy.gameObject) || pb_EditorUtility.IsPrefabRoot(copy.gameObject) )
					PrefabUtility.DisconnectPrefabInstance(copy.gameObject);

				if(copy.transform.childCount > 0)
				{
					for(int i = 0; i < copy.transform.childCount; ++i)
						GameObject.DestroyImmediate(copy.transform.GetChild(i).gameObject);

					foreach(pb_Object pb_child in pb.transform.GetComponentsInChildren<pb_Object>())
						pb_EditorUtility.VerifyMesh(pb_child);
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
				pb_Selection.SetSelection(detached.ToArray());
				editor.UpdateSelection();
			}

			if(detachedFaceCount > 0)
				return new pb_ActionResult(Status.Success, "Detach " + detachedFaceCount + " faces to new Object");
			else
				return new pb_ActionResult(Status.Failure, "No Faces Selected");
		}

#endif
#endregion

#region Face / Triangles

		/**
		 *	Treat selected faces as a single face.
		 */
		public static pb_ActionResult MenuMergeFaces(pb_Object[] selection)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_Undo.RecordObjects(selection, "Merge Faces");

			int success = 0;

			foreach(pb_Object pb in selection)
			{
				if(pb.SelectedFaceCount > 1)
				{
					success += pb.SelectedFaceCount;

					pb_Face face = pb_MergeFaces.Merge(pb, pb.SelectedFaces);

					pb.ToMesh();
					pb.Refresh();
					pb.Optimize();

					pb.SetSelectedFaces( new pb_Face[] { face } );
				}
			}

			if(editor)
				editor.UpdateSelection();

			if(success > 0)
				return new pb_ActionResult(Status.Success, "Merged " + success + " Faces");
			else
				return new pb_ActionResult(Status.Failure, "Merge Faces\nNo Faces Selected");
		}

		/**
		 * Turn / flip / swap a quad connecting edge.
		 */
		public static pb_ActionResult MenuFlipEdges(pb_Object[] selection)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_Undo.RecordSelection(selection, "Flip Face Edges");
			int success = 0;
			int attempts = 0;

			foreach(pb_Object pb in selection)
			{
				foreach(pb_Face face in pb.SelectedFaces)
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
				editor.UpdateSelection();

			if(success > 0)
				return new pb_ActionResult(Status.Success, "Flipped " + success + " Edges");
			else
				return new pb_ActionResult(Status.Failure, string.Format("Flip Edges\n{0}", attempts > 0 ? "Faces Must Be Quads" : "No Faces Selected"));
		}
#endregion

#region Vertex Operations

#if !PROTOTYPE

		/**
		 * Collapse selected vertices
		 * ProBuilder only.
		 */
		public static pb_ActionResult MenuCollapseVertices(pb_Object[] selection)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			bool success = false;

			bool collapseToFirst = pb_PreferencesInternal.GetBool(pb_Constant.pbCollapseVertexToFirst);

			pb_Undo.RegisterCompleteObjectUndo(selection, "Collapse Vertices");

			foreach(pb_Object pb in selection)
			{
				if(pb.SelectedTriangles.Length > 1)
				{
					int newIndex = -1;
					success = pb.MergeVertices(pb.SelectedTriangles, out newIndex, collapseToFirst);

					if(success)
					{
						int[] removed;
						pb.RemoveDegenerateTriangles(out removed);
						pb.SetSelectedTriangles(new int[] { newIndex });
					}

					pb.ToMesh();
					pb.Refresh();
					pb.Optimize();
				}
			}


			if(editor)
				editor.UpdateSelection();

			if(success)
				return new pb_ActionResult(Status.Success, "Collapse Vertices");
			else
				return new pb_ActionResult(Status.Failure, "Collapse Vertices\nNo Vertices Selected");
		}

		/**
		 * Weld all selected vertices.
		 * ProBuilder only.
		 */
		public static pb_ActionResult MenuWeldVertices(pb_Object[] selection)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_ActionResult res = pb_ActionResult.NoSelection;

			pb_Undo.RegisterCompleteObjectUndo(selection, "Weld Vertices");
			float weld = pb_PreferencesInternal.GetFloat(pb_Constant.pbWeldDistance);
			int weldCount = 0;

			foreach(pb_Object pb in selection)
			{
				weldCount += pb.sharedIndices.Length;

				if(pb.SelectedTriangles.Length > 1)
				{
					pb.ToMesh();

					int[] welds;
					res = pb.WeldVertices(pb.SelectedTriangles, weld, out welds);

					if(res)
					{
						int[] removed;

						if( pb.RemoveDegenerateTriangles(out removed) )
						{
							pb.ToMesh();
							welds = new int[0];	// @todo
						}

						pb.SetSelectedTriangles(welds ?? new int[0] {});
					}

					pb.Refresh();
					pb.Optimize();
				}

				weldCount -= pb.sharedIndices.Length;
			}

			if(editor)
				editor.UpdateSelection(true);

			if(res && weldCount > 0)
				return new pb_ActionResult(Status.Success, "Weld " + weldCount + (weldCount > 1 ? " Vertices" : " Vertex"));
			else
				return new pb_ActionResult(Status.Failure, "Nothing to Weld");
		}

		const float MIN_WELD_DISTANCE = .0001f;

		/**
		 * Expose the distance parameter used in Weld operations.
		 * ProBuilder only.
		 */
		public static void WeldButtonGUI(int width)
		{
			EditorGUI.BeginChangeCheck();

			float weldDistance = pb_PreferencesInternal.GetFloat(pb_Constant.pbWeldDistance);

			if(weldDistance <= MIN_WELD_DISTANCE)
				weldDistance = MIN_WELD_DISTANCE;

			EditorGUIUtility.labelWidth = width - 68;
			weldDistance = EditorGUILayout.FloatField(new GUIContent("Max", "The maximum distance between two vertices in order to be welded together."), weldDistance);

			if( EditorGUI.EndChangeCheck() )
			{
				if(weldDistance < MIN_WELD_DISTANCE)
					weldDistance = MIN_WELD_DISTANCE;
				pb_PreferencesInternal.SetFloat(pb_Constant.pbWeldDistance, weldDistance);
			}
		}

		/**
		 * Split selected vertices from shared vertices.
		 * ProBuilder only.
		 */
		public static pb_ActionResult MenuSplitVertices(pb_Object[] selection)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			int splitCount = 0;
			pb_Undo.RecordSelection(selection, "Split Vertices");

			foreach(pb_Object pb in selection)
			{
				List<int> tris = new List<int>(pb.SelectedTriangles);			// loose verts to split

				if(pb.SelectedFaces.Length > 0)
				{
					pb_IntArray[] sharedIndices = pb.sharedIndices;

					int[] selTrisIndices = new int[pb.SelectedTriangles.Length];

					// Get sharedIndices index for each vert in selection
					for(int i = 0; i < pb.SelectedTriangles.Length; i++)
						selTrisIndices[i] = sharedIndices.IndexOf(pb.SelectedTriangles[i]);

					// cycle through selected faces and remove the tris that compose full faces.
					foreach(pb_Face face in pb.SelectedFaces)
					{
						List<int> faceSharedIndices = new List<int>();

						for(int j = 0; j < face.distinctIndices.Length; j++)
							faceSharedIndices.Add( sharedIndices.IndexOf(face.distinctIndices[j]) );

						List<int> usedTris = new List<int>();
						for(int i = 0; i < selTrisIndices.Length; i++)
							if( faceSharedIndices.Contains(selTrisIndices[i]) )
								usedTris.Add(pb.SelectedTriangles[i]);

						// This face *is* composed of selected tris.  Remove these tris from the loose index list
						foreach(int i in usedTris)
							if(tris.Contains(i))
								tris.Remove(i);
					}
				}

				// Now split the faces, and any loose vertices
				pb.DetachFaces(pb.SelectedFaces);

				splitCount += pb.SelectedTriangles.Length;
				pb.SplitCommonVertices(pb.SelectedTriangles);

				// Reattach detached face vertices (if any are to be had)
				if(pb.SelectedFaces.Length > 0)
				{
					int[] welds;
					pb.WeldVertices( pb_Face.AllTriangles(pb.SelectedFaces), Mathf.Epsilon, out welds);
				}

				// And set the selected triangles to the newly split
				List<int> newTriSelection = new List<int>(pb_Face.AllTriangles(pb.SelectedFaces));
				newTriSelection.AddRange(tris);
				pb.SetSelectedTriangles(newTriSelection.ToArray());

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			pb_Editor.Refresh();

			if(splitCount > 0)
				return new pb_ActionResult(Status.Success, "Split " + splitCount + (splitCount > 1 ? " Vertices" : " Vertex"));
			else
				return new pb_ActionResult(Status.Failure, "Split Vertices\nInsuffient Vertices Selected");
		}

		/**
		 *	Attempt to create polygons bridging any gaps in geometry.
		 */
		public static pb_ActionResult MenuFillHole(pb_Object[] selection)
		{
			if(editor == null)
				return pb_ActionResult.NoSelection;

			pb_Undo.RecordSelection(selection, "Fill Hole");

			pb_ActionResult res = new pb_ActionResult(Status.NoChange, "No Holes Found");
			int filled = 0;
			bool wholePath = pb_PreferencesInternal.GetBool(pb_Constant.pbFillHoleSelectsEntirePath);

			foreach(pb_Object pb in selection)
			{
				bool selectAll = pb.SelectedTriangles == null || pb.SelectedTriangles.Length < 1;
				int[] indices = selectAll ? pb_Face.AllTriangles(pb.faces) : pb.SelectedTriangles;

				pb.ToMesh();

				Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
				List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb);
				HashSet<int> common = pb_IntArrayUtility.GetCommonIndices(lookup, indices);
				List<List<pb_WingedEdge>> holes = pb_AppendPolygon.FindHoles(wings, common);

				HashSet<pb_Face> faces = new HashSet<pb_Face>();
				List<pb_Face> adjacent = new List<pb_Face>();

				foreach(List<pb_WingedEdge> hole in holes)
				{
					List<int> holeIndices;

					pb_Face face;

					if(wholePath)
					{
						// if selecting whole path and in edge mode, make sure the path contains
						// at least one complete edge from the selection.
						if(	editor.selectionMode == SelectMode.Edge &&
							!hole.Any(x => common.Contains(x.edge.common.x) &&
							common.Contains(x.edge.common.y)))
							continue;

						holeIndices = hole.Select(x => x.edge.local.x).ToList();
						res = pb_AppendPolygon.CreatePolygon(pb, holeIndices, false, out face);
						adjacent.AddRange(hole.Select(x => x.face));
					}
					else
					{
						IEnumerable<pb_WingedEdge> selected = hole.Where(x => common.Contains(x.edge.common.x));
						holeIndices = selected.Select(x => x.edge.local.x).ToList();
						res = pb_AppendPolygon.CreatePolygon(pb, holeIndices, true, out face);

						if(res)
							adjacent.AddRange(selected.Select(x => x.face));
					}

					if(res)
					{
						filled++;
						adjacent.Add(face);
						faces.Add(face);
					}
				}

				pb.SetSelectedFaces(faces);

				wings = pb_WingedEdge.GetWingedEdges(pb, adjacent);

				// make sure the appended faces match the first adjacent face found
				// both in winding and face properties
				foreach(pb_WingedEdge wing in wings)
				{
					if( faces.Contains(wing.face) )
					{
						faces.Remove(wing.face);

						foreach(pb_WingedEdge p in wing)
						{
							if(p.opposite != null)
							{
								p.face.material = p.opposite.face.material;
								p.face.uv = new pb_UV(p.opposite.face.uv);
								pb_ConformNormals.ConformOppositeNormal(p.opposite);
								break;
							}
						}
					}
				}

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			pb_Editor.Refresh();

			if(filled > 0)
			{
				res.status = Status.Success;
				res.notification = filled > 1 ? string.Format("Filled {0} Holes", filled) : "Fill Hole";
			}

			return res;
		}

		public static pb_ActionResult MenuCreatePolygon(pb_Object[] selection)
		{
			pb_Undo.RecordSelection(selection, "Create Polygon");

			pb_ActionResult res = pb_ActionResult.NoSelection;

			foreach(pb_Object pb in selection)
			{
				int[] indices = pb.SelectedTriangles;

				if(indices == null || indices.Length < 2)
					continue;

				pb.ToMesh();

				pb_Face face;

				res = pb_AppendPolygon.CreatePolygon(pb, indices, false, out face);

				pb.Refresh();
				pb.Optimize();

				if(res)
					pb.SetSelectedFaces(new pb_Face[] { face });
			}

			pb_Editor.Refresh();

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
		public static pb_ActionResult MenuSubdivide(pb_Object[] selection)
		{
			if(!editor || selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pb_Undo.RegisterCompleteObjectUndo(selection, "Subdivide Selection");

			int success = 0;

			foreach(pb_Object pb in selection)
			{
				pb.ToMesh();

				if( pb.Subdivide() )
					success++;

				pb.Refresh();
				pb.Optimize();

				pb.SetSelectedTriangles(new int[0]);
			}

			if(editor)
				editor.UpdateSelection(true);

			return new pb_ActionResult(Status.Success, "Subdivide " + selection.Length + " Objects");
		}

		/**
		 * Attempts to subdivide the selected object edges.
		 * ProBuilder only.
		 */
		public static pb_ActionResult MenuSubdivideEdge(pb_Object[] selection)
		{
			if(!editor || selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			int subdivisions = pb_PreferencesInternal.GetInt(pb_Constant.pbEdgeSubdivisions, 1);

			pb_Undo.RegisterCompleteObjectUndo(selection, "Subdivide Edges");

			pb_ActionResult result = pb_ActionResult.NoSelection;

			foreach(pb_Object pb in selection)
			{
				List<pb_Edge> newEdgeSelection;

				result = pb_VertexOps.AppendVerticesToEdge(pb, pb.SelectedEdges, subdivisions, out newEdgeSelection);

				if(result.status == Status.Success)
					pb.SetSelectedEdges(newEdgeSelection);

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			pb_Editor.Refresh(true);

			return result;
		}

		/**
		 * Subdivides all currently selected faces.
		 * ProBuilder only.
		 */
		public static pb_ActionResult MenuSubdivideFace(pb_Object[] selection)
		{
			if(!editor || selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			int success = 0;

			foreach(pb_Object pb in selection)
			{
				pb_Undo.RegisterCompleteObjectUndo(selection, "Subdivide Faces");

				pb_Face[] faces;

				pb.ToMesh();

				if(pb.Subdivide(pb.SelectedFaces, out faces))
				{
					success += pb.SelectedFaces.Length;
					pb.SetSelectedFaces(faces);

					pb.Refresh();
					pb.Optimize();
				}
			}

			if(success > 0)
			{
				if(editor)
					editor.UpdateSelection(true);

				return new pb_ActionResult(Status.Success, "Subdivide " + success + ((success > 1) ? " faces" : " face"));
			}
			else
			{
				Debug.LogWarning("Subdivide faces failed - did you not have any faces selected?");
				return new pb_ActionResult(Status.Failure, "Subdivide Faces\nNo faces selected");
			}
		}

		/**
		 * Connects all currently selected edges.
		 * ProBuilder only.
		 */
		public static pb_ActionResult MenuConnectEdges(pb_Object[] selection, bool useOld = false)
		{
			pb_ActionResult res = pb_ActionResult.NoSelection;

			pb_Undo.RegisterCompleteObjectUndo(selection, "Connect Edges");

			foreach(pb_Object pb in selection)
			{
				pb_Edge[] connections;
				res = pb.Connect(pb.SelectedEdges, out connections);
				pb.SetSelectedEdges(connections);
				pb.Refresh();
				pb.Optimize();
			}

			pb_Editor.Refresh();
			return res;
		}

		/**
		 * Connects all currently selected vertices.
		 * ProBuilder only.
		 */
		public static pb_ActionResult MenuConnectVertices(pb_Object[] selection)
		{
			pb_ActionResult res = pb_ActionResult.NoSelection;

			pb_Undo.RegisterCompleteObjectUndo(selection, "Connect Vertices");

			foreach(pb_Object pb in selection)
			{
				int[] splits;

				pb.ToMesh();

				res = pb.Connect(pb.SelectedTriangles, out splits);

				if(res)
				{
					pb.Refresh();
					pb.Optimize();
					pb.SetSelectedTriangles(splits);
				}
			}
			pb_Editor.Refresh();

			return res;
		}

		/**
		 * Inserts an edge loop along currently selected Edges.
		 * ProBuilder only.
		 */
		public static pb_ActionResult MenuInsertEdgeLoop(pb_Object[] selection)
		{
			if(!editor || selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			int success = 0;
			pb_Undo.RegisterCompleteObjectUndo(selection, "Insert Edge Loop");

			foreach(pb_Object pb in selection)
			{
				pb_Edge[] edges;

				if( pb.Connect(pb_MeshUtils.GetEdgeRing(pb, pb.SelectedEdges), out edges) )
				{
					pb.SetSelectedEdges(edges);
					pb.ToMesh();
					pb.Refresh();
					pb.Optimize();
					success++;
				}
			}

			if(editor)
				editor.UpdateSelection(true);

			if(success > 0)
				return new pb_ActionResult(Status.Success, "Insert Edge Loop");
			else
				return new pb_ActionResult(Status.Success, "Insert Edge Loop");
		}

#endif
#endregion
	}
}

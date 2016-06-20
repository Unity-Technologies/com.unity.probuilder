using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using System.Reflection;
using ProBuilder2.MeshOperations;
using System.Linq;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.EditorCommon
{
	/**
	 * Contains Menu commands for most ProBuilder operations.  Will
	 * also attempt to Update the pb_Editor.
	 */
	public class pb_Menu_Commands : Editor
	{
		private static pb_Editor editor { get { return pb_Editor.instance; } }

		/**
		 *	Define "headers" for pro only functions.
		 */
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
				return new pb_ActionResult(Status.Failure, "Must Select 2+ Objects");

			int option = EditorUtility.DisplayDialogComplex(
				"Save or Delete Originals?",
				"Saved originals will be deactivated and hidden from the Scene, but available in the Hierarchy.",
				"Merge Delete",		// 0
				"Merge Save",		// 1
				"Cancel");			// 2

			pb_Object pb = null;

			if(option == 2) return new pb_ActionResult(Status.Canceled, "Merge Canceled");

			if( pbMeshOps.CombineObjects(selected, out pb) )
			{
				pb_EditorUtility.SetEntityType(selected[0].GetComponent<pb_Entity>().entityType, pb.gameObject);

				pb.Optimize();

				pb.gameObject.name = "pb-MergedObject" + pb.id;

				switch(option)
				{
					case 0: 	// Delete donor objects
						for(int i = 0; i < selected.Length; i++)
						{
							if(selected[i] != null)
								pbUndo.DestroyImmediate(selected[i].gameObject, "Delete Merged Objects");
						}

						break;

					case 1:
						foreach(pb_Object sel in selected)
							sel.gameObject.SetActive(false);
						break;
				}

				pbUndo.RegisterCreatedObjectUndo(pb.gameObject, "Merge Objects");

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

			pbUndo.RegisterCompleteObjectUndo(objects, "Set Pivot");

			for(int i = 0; i < selection.Length; i++)
			{
				selection[i].ToMesh();

				if(triangles != null)
					selection[i].CenterPivot(triangles[i]);
				else
					selection[i].CenterPivot(null);

				selection[i].Refresh();
				selection[i].Optimize();
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

			pbUndo.RecordObjects(Selection.transforms, "Freeze Transforms");
			pbUndo.RecordObjects(selection, "Freeze Transforms");

			Vector3[][] vertices = new Vector3[selection.Length][];

			for(int i = 0; i < selection.Length; i++)
				vertices[i] = selection[i].VerticesInWorldSpace();

			for(int i = 0; i < selection.Length; i++)
			{
				pb_Object pb = selection[i];

				pb.transform.position = Vector3.zero;
				pb.transform.localRotation = Quaternion.identity;
				pb.transform.localScale = Vector3.one;

				foreach(pb_Face face in pb.faces)
					face.manualUV = true;

				pb.SetVertices(vertices[i]);

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			if(pb_Editor.instance)
				pb_Editor.instance.UpdateSelection();

			SceneView.RepaintAll();

			return new pb_ActionResult(Status.Success, "Freeze Transforms");
		}

		/**
		 * Adds pb_Object and pb_Entity to object without duplicating the objcet.  Is undo-able.
		 */
		public static pb_ActionResult ProBuilderize(IEnumerable<MeshFilter> selected, bool preserveFaces)
		{
			if(selected.Count() < 1)
				return new pb_ActionResult(Status.Canceled, "Nothing Selected");

			int i = 0;

			foreach(MeshFilter mf in selected)
			{
				if(mf.sharedMesh == null)
					continue;

				GameObject go = mf.gameObject;
				MeshRenderer mr = go.GetComponent<MeshRenderer>();

				pb_Object pb = Undo.AddComponent<pb_Object>(go);
				pbMeshOps.ResetPbObjectWithMeshFilter(pb, preserveFaces);

				EntityType entityType = EntityType.Detail;

				if(mr != null && mr.sharedMaterials != null && mr.sharedMaterials.Any(x => x != null && x.name.Contains("Collider")))
					entityType = EntityType.Collider;
				else
				if(mr != null && mr.sharedMaterials != null && mr.sharedMaterials.Any(x => x != null && x.name.Contains("Trigger")))
					entityType = EntityType.Trigger;

				// if this was previously a pb_Object, or similarly any other instance asset, destroy it.
				// if it is backed by saved asset, leave the mesh asset alone but assign a new mesh to the
				// renderer so that we don't modify the asset.
				if(AssetDatabase.GetAssetPath(mf.sharedMesh) == "" )
					Undo.DestroyObjectImmediate(mf.sharedMesh);
				else if(mf != null)
					go.GetComponent<MeshFilter>().sharedMesh = new Mesh();

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();

				i++;

				// Don't call the editor version of SetEntityType because that will
				// reset convexity and trigger settings, which we can assume are user
				// set already.
				if( !pb.gameObject.GetComponent<pb_Entity>() )
					Undo.AddComponent<pb_Entity>(pb.gameObject).SetEntity(entityType);
				else
					Undo.AddComponent<pb_Entity>(pb.gameObject).SetEntity(entityType);
				// pb_EditorUtility.SetEntityType(entityType, t.gameObject);
			}

			if(pb_Editor.instance != null)
				pb_Editor.instance.UpdateSelection();

			return new pb_ActionResult(Status.Success, "ProBuilderize " + i + (i > 1 ? " Objects" : " Object").ToString());
		}

		/**
		 * Set the pb_Entity entityType on selection.
		 */
		public static pb_ActionResult MenuSetEntityType(pb_Object[] selection, EntityType entityType)
		{
			if(selection.Length < 1)
				return pb_ActionResult.NoSelection;

			Object[] undoObjects = selection.SelectMany(x => x.GetComponents<Component>()).ToArray();

			pbUndo.RecordObjects(undoObjects, "Set Entity Type");

			foreach(pb_Object pb in selection)
			{
				pb_EditorUtility.SetEntityType(entityType, pb.gameObject);
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
			switch( pb_Preferences_Internal.GetEnum<VertexColorTool>(pb_Constant.pbVertexColorTool) )
			{
				case VertexColorTool.Palette:
					pb_Vertex_Color_Toolbar.MenuOpenWindow();
					break;

				default:
					pb_VertexColor_Editor.MenuOpenWindow();
					break;
			}
		}

		/**
		 *	Open the vertex coloring editor as stored by user prefs.
		 */
		public static pb_ActionResult MenuOpenVertexColorsEditor2(pb_Object[] selection)
		{
			switch( pb_Preferences_Internal.GetEnum<VertexColorTool>(pb_Constant.pbVertexColorTool) )
			{
				case VertexColorTool.Palette:
					pb_Vertex_Color_Toolbar.MenuOpenWindow();
					break;

				default:
					pb_VertexColor_Editor.MenuOpenWindow();
					break;
			}

			return new pb_ActionResult(Status.Success, "Open Vertex Colors Editor");
		}

		public static void VertexColorsGUI(int width)
		{
			VertexColorTool tool = pb_Preferences_Internal.GetEnum<VertexColorTool>(pb_Constant.pbVertexColorTool);
			VertexColorTool prev = tool;

			GUILayout.Label("Color Editor");

			tool = (VertexColorTool) EditorGUILayout.EnumPopup("", tool, GUILayout.MaxWidth(width-14));

			if(prev != tool)
				EditorPrefs.SetInt(pb_Constant.pbVertexColorTool, (int)tool);
		}
#if !PROTOTYPE

		public static pb_ActionResult MenuFacetizeObject(pb_Object[] selection)
		{
			if(!editor || selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pbUndo.RegisterCompleteObjectUndo(selection, "Triangulate Objects");

			for(int i = 0; i < selection.Length; i++)
			{
				pbTriangleOps.Facetize(selection[i]);
				selection[i].ToMesh();
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

			pbUndo.RecordObjects(sel, op_string);

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

			go.AddComponent<MeshRenderer>().sharedMaterial = pb_Constant.DefaultMaterial;
			go.AddComponent<MeshFilter>().sharedMesh = c;

			pb_Object pb = pbMeshOps.CreatePbObjectWithTransform(go.transform, false);
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

			pbUndo.RecordObjects(pbUtil.GetComponents<pb_Object>(Selection.transforms), "Flip Object Normals");

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

			pbUndo.RecordObjects(pbUtil.GetComponents<pb_Object>(Selection.transforms), "Flip Face Normals");
			int c = 0;
			int faceCount = pb_Editor.instance.selectedFaceCount;

			foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
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

			pbUndo.RecordObjects(selection, "Conform " + (editor.selectedFaceCount > 0 ? "Face" : "Object") + " Normals.");

			int flipped = 0;

			foreach(pb_Object pb in selection)
			{
				pb_Face[] faces = perFace ? pb.SelectedFaces : pb.faces;

				if(faces == null)
					continue;

				int len = faces.Length;

				int toggle = 0;
				WindingOrder[] winding = new WindingOrder[len];

				// First figure out what the majority of the faces' winding order is
				for(int i = 0; i < len; i++)
				{
					winding[i] = pb.GetWindingOrder( faces[i] );
					toggle += (winding[i] == WindingOrder.Unknown ? 0 : (winding[i] == WindingOrder.Clockwise ? 1 : -1));
				}

				// if toggle >= 0 wind clockwise, else ccw
				for(int i = 0; i < len; i++)
				{
					if( (toggle >= 0 && winding[i] == WindingOrder.CounterClockwise) ||
						(toggle < 0 && winding[i] == WindingOrder.Clockwise) )
					{
						faces[i].ReverseIndices();
						flipped++;
					}
				}

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			editor.UpdateSelection();

			if(flipped > 0)
				return new pb_ActionResult(Status.Success, "Reversed " + flipped + " Faces");
			else
				return new pb_ActionResult(Status.Canceled, "Normals Already Uniform");
		}
#endregion

#region Extrude / Bridge

		public static void ExtrudeButtonGUI(int width)
		{
			float extrudeAmount = EditorPrefs.HasKey(pb_Constant.pbExtrudeDistance) ? EditorPrefs.GetFloat(pb_Constant.pbExtrudeDistance) : .5f;
			bool extrudeAsGroup = pb_Preferences_Internal.GetBool(pb_Constant.pbExtrudeAsGroup);

			EditorGUI.BeginChangeCheck();

			EditorGUIUtility.labelWidth = width - 28;
			extrudeAsGroup = EditorGUILayout.Toggle("As Group", extrudeAsGroup);

			EditorGUIUtility.labelWidth = width - 68;
			extrudeAmount = EditorGUILayout.FloatField("Dist", extrudeAmount, GUILayout.MaxWidth(width-12));

			if(EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetFloat(pb_Constant.pbExtrudeDistance, extrudeAmount);
				EditorPrefs.SetBool(pb_Constant.pbExtrudeAsGroup, extrudeAsGroup);
			}
		}

		/**
		 * Infers the correct context and extrudes the selected elements.
		 */
		public static pb_ActionResult MenuExtrude(pb_Object[] selection, bool forceMode = false)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pbUndo.RegisterCompleteObjectUndo(selection, "Extrude");

			int extrudedFaceCount = 0;
			bool success = false;

			foreach(pb_Object pb in selection)
			{
				pb.ToMesh();
				pb.RefreshNormals();

				if(editor && editor.selectionMode == SelectMode.Edge)
				{
					if(pb.SelectedEdges.Length < 1 || (!forceMode && pb.SelectedFaces.Length > 0))
					{
						success = false;
					}
					else
					{
						extrudedFaceCount += pb.SelectedEdges.Length;
						pb_Edge[] newEdges;

						success = pb.Extrude(	pb.SelectedEdges,
												pb_Preferences_Internal.GetFloat(pb_Constant.pbExtrudeDistance),
												pb_Preferences_Internal.GetBool(pb_Constant.pbExtrudeAsGroup),
												pb_Preferences_Internal.GetBool(pb_Constant.pbManifoldEdgeExtrusion),
												out newEdges);

						if(success)
							pb.SetSelectedEdges(newEdges);
						else
							extrudedFaceCount -= pb.SelectedEdges.Length;
					}

					pb.ToMesh();
				}

				if((editor.selectionMode == SelectMode.Face || (!forceMode && !success)) && pb.SelectedFaces.Length > 0)
				{
					extrudedFaceCount += pb.SelectedFaces.Length;

					pb_Face[] result;
					pb.Extrude(	pb.SelectedFaces,
								pb_Preferences_Internal.GetFloat(pb_Constant.pbExtrudeDistance),
								pb_Preferences_Internal.GetBool(pb_Constant.pbExtrudeAsGroup),
								out result);

					pb.SetSelectedFaces(pb.SelectedFaces);

					pb.ToMesh();
				}

				pb.Refresh();
				pb.Optimize();
			}

			if(editor != null)
				editor.UpdateSelection();

			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));

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

			pbUndo.RecordObjects(selection, "Bridge Edges");

			bool success = false;
			bool limitToPerimeterEdges = pb_Preferences_Internal.GetBool(pb_Constant.pbPerimeterEdgeBridgeOnly);

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

			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));

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
			pbUndo.RecordSelection(selection, "Bevel Edges");
			pb_ActionResult res = pb_ActionResult.NoSelection;

			float amount = pb_Preferences_Internal.GetFloat(pb_Constant.pbBevelAmount);

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

			pbUndo.RecordSelection(selection, "Grow Selection");

			// profiler.BeginSample("MenuGrowSelection");

			int grown = 0;

			foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
			{
				int sel = pb.SelectedTriangleCount;

				switch( editor != null ? editor.selectionMode : (SelectMode)0 )
				{
					case SelectMode.Vertex:
						pb.SetSelectedEdges(pbMeshUtils.GetConnectedEdges(pb, pb.SelectedTriangles));
						break;

					case SelectMode.Edge:
						pb.SetSelectedEdges(pbMeshUtils.GetConnectedEdges(pb, pb.SelectedTriangles));
						break;

					case SelectMode.Face:

						if( pb_Preferences_Internal.GetBool(pb_Constant.pbGrowSelectionUsingAngle) )
						{
							bool iterative = pb_Preferences_Internal.GetBool(pb_Constant.pbGrowSelectionAngleIterative);
							float growSelectionAngle = pb_Preferences_Internal.GetFloat("pbGrowSelectionAngle");
							HashSet<pb_Face> selected = new HashSet<pb_Face>( pb.SelectedFaces );
							List<pb_Face> perimeterFaces = pbMeshUtils.GetPerimeterFaces(pb, pb.SelectedFaces).ToList();

							if(!iterative)
							{
								/**
								 * Grow by angle flood fill
								 */
								// profiler.BeginSample("GenerateNeighborLookup");
								Dictionary<pb_Face, List<pb_Face>> faceLookup = pbMeshUtils.GenerateNeighborLookup(pb, pb.faces);
								// profiler.EndSample();

								bool facesAdded = true;

								List<pb_Face> newFaces = new List<pb_Face>();
								Vector3[] v = pb.vertices;
								Vector3 a, b, c;

								// profiler.BeginSample("while(facesAdded");
								while(facesAdded)
								{
									facesAdded = false;

									// profiler.BeginSample("Walk Perimeter");
									foreach(pb_Face f in perimeterFaces)
									{
										// profiler.BeginSample("Face Normal");
										Vector3 nrm = pb_Math.Normal( pb.vertices.ValuesWithIndices(f.indices) );
										// profiler.EndSample();

										// profiler.BeginSample("Face Lookup Contains");
										IEnumerable<pb_Face> adjacent = faceLookup[f].Where(x => !selected.Contains(x));
										// profiler.EndSample();

										// profiler.BeginSample("Add Faces");
										foreach(pb_Face connectedFace in adjacent)
										{
											a = v[connectedFace.indices[0]];
											b = v[connectedFace.indices[1]];
											c = v[connectedFace.indices[2]];
											float angle = Vector3.Angle(nrm, Vector3.Cross(b-a, c-a));

											if( angle < growSelectionAngle )
											{
												selected.Add(connectedFace);
												newFaces.Add(connectedFace);
												facesAdded = true;
											}
										}
										// profiler.EndSample();
									}
									// profiler.EndSample();

									perimeterFaces = new List<pb_Face>(newFaces);
									newFaces.Clear();
								}
								// profiler.EndSample();

								pb.SetSelectedFaces(selected.ToArray());
							}
							else
							{
								/**
								 * Grow with angle iterative
								 */
								Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();

								for(int i = 0; i < perimeterFaces.Count; i++)
								{
									List<pb_Face> adjacent = pbMeshUtils.GetNeighborFaces(pb, perimeterFaces[i], lookup, perimeterFaces);

									Vector3 perim_normal = pb_Math.Normal(pb.vertices.ValuesWithIndices(perimeterFaces[i].indices));

									for(int n = 0; n < adjacent.Count; n++)
									{
										Vector3 adjacent_normal = pb_Math.Normal( pb.vertices.ValuesWithIndices(adjacent[n].indices) );

										float angle = Vector3.Angle( perim_normal, adjacent_normal );

										if( angle < growSelectionAngle )
										{
											selected.Add(adjacent[n]);
										}
									}
								}

								pb.SetSelectedFaces(selected.ToArray());
							}
						}
						else
						{
							/**
							 * Grow by proximity
							 */
							pb_Face[] perimeter = pbMeshUtils.GetNeighborFaces(pb, pb.sharedIndices.ToDictionary(), pb.SelectedFaces);

							perimeter = pbUtil.Concat(perimeter, pb.SelectedFaces);

							pb.SetSelectedFaces(perimeter);
						}

						break;
				}

				grown += pb.SelectedTriangleCount - sel;
			}

			// profiler.EndSample();

			if(editor != null)
				editor.UpdateSelection(false);

			SceneView.RepaintAll();

			if(grown > 0)
				return new pb_ActionResult(Status.Success, "Grow Selection");
			else
				return new pb_ActionResult(Status.Failure, "Nothing to Grow");
		}

		public static void GrowSelectionGUI(int width)
		{
			bool angleGrow = pb_Preferences_Internal.GetBool(pb_Constant.pbGrowSelectionUsingAngle);

			EditorGUI.BeginChangeCheck();

			EditorGUIUtility.labelWidth = width - 28;
			angleGrow = EditorGUILayout.Toggle("Angle", angleGrow);

			float angleVal = pb_Preferences_Internal.GetFloat(pb_Constant.pbGrowSelectionAngle);

			bool te = GUI.enabled;

			GUI.enabled = te ? angleGrow : te;

			EditorGUIUtility.labelWidth = width - 68;
			angleVal = EditorGUILayout.FloatField("Max", angleVal, GUILayout.MaxWidth(width-12));

			EditorGUIUtility.labelWidth = width - 28;
			bool iterative = pb_Preferences_Internal.GetBool(pb_Constant.pbGrowSelectionAngleIterative);
			iterative = EditorGUILayout.Toggle("Iterative", iterative);

			GUI.enabled = te;

			if( EditorGUI.EndChangeCheck() )
			{
				EditorPrefs.SetBool(pb_Constant.pbGrowSelectionUsingAngle, angleGrow);
				EditorPrefs.SetBool(pb_Constant.pbGrowSelectionAngleIterative, iterative);
				EditorPrefs.SetFloat(pb_Constant.pbGrowSelectionAngle, angleVal);
			}
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
		 * Note - requires a reference to an open pb_Editor be passed.  This is because shrink
		 * vertices requires access to the Selected_Universal_Edges_All array.
		 */
		public static pb_ActionResult MenuShrinkSelection(pb_Object[] selection)
		{
			// @TODO
			if(editor == null)
				return new pb_ActionResult(Status.Canceled, "ProBuilder Editor Not Open!");
			else if (selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pbUndo.RecordSelection(selection, "Shrink Selection");

			// find perimeter edges
			int rc = 0;
			for(int i = 0; i < selection.Length; i++)
			{
				pb_Object pb = selection[i];

				switch(editor.selectionMode)
				{
					case SelectMode.Edge:
					{
						int[] perimeter = pbMeshUtils.GetPerimeterEdges(pb, pb.SelectedEdges);
						pb.SetSelectedEdges( pb.SelectedEdges.RemoveAt(perimeter) );
						rc += perimeter != null ? perimeter.Length : 0;
						break;
					}

					case SelectMode.Face:
					{
						pb_Face[] perimeter = pbMeshUtils.GetPerimeterFaces(pb, pb.SelectedFaces).ToArray();
						pb.SetSelectedFaces( pb.SelectedFaces.Except(perimeter).ToArray() );
						rc += perimeter != null ? perimeter.Length : 0;
						break;
					}

					case SelectMode.Vertex:
					{
						int[] perimeter = pbMeshUtils.GetPerimeterVertices(pb, pb.SelectedTriangles, editor.SelectedUniversalEdges[i]);
						pb.SetSelectedTriangles( pb.SelectedTriangles.RemoveAt(perimeter) );
						rc += perimeter != null ? perimeter.Length : 0;
						break;
					}
				}

			}

			if(editor)
				editor.UpdateSelection(false);

			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));

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

			pbUndo.RecordSelection(selection, "Invert Selection");

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
						List<pb_Face> inverse = new List<pb_Face>();

						for(int i = 0; i < pb.faces.Length; i++)
							if( System.Array.IndexOf(pb.SelectedFaceIndices, i) < 0 )
								inverse.Add(pb.faces[i]);

						pb.SetSelectedFaces(inverse.ToArray());
					}
					break;

				case SelectMode.Edge:

					if(!editor) break;

					for(int i = 0; i < selection.Length; i++)
					{
						pb_Edge[] universal_selected_edges = pb_Edge.GetUniversalEdges(selection[i].SelectedEdges, selection[i].sharedIndices).Distinct().ToArray();
						pb_Edge[] inverse_universal = System.Array.FindAll(editor.SelectedUniversalEdges[i], x => !universal_selected_edges.Contains(x));
						pb_Edge[] inverse = new pb_Edge[inverse_universal.Length];

						for(int n = 0; n < inverse_universal.Length; n++)
							inverse[n] = new pb_Edge( selection[i].sharedIndices[inverse_universal[n].x][0], selection[i].sharedIndices[inverse_universal[n].y][0] );

						selection[i].SetSelectedEdges(inverse);
					}
					break;
			}

			if(editor)
				editor.UpdateSelection();

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

			pbUndo.RecordSelection(selection, "Select Edge Ring");

			bool success = false;

			foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
			{
				pb_Edge[] edges = pbMeshUtils.GetEdgeRing(pb, pb.SelectedEdges).ToArray();

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
		 * Selects an Edge loop. Todo - support for face loops.
		 */
		public static pb_ActionResult MenuLoopSelection(pb_Object[] selection)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pbUndo.RecordSelection(selection, "Select Edge Loop");

			bool foundLoop = false;

			foreach(pb_Object pb in selection)
			{
				pb_Edge[] loop;
				bool success = pbMeshUtils.GetEdgeLoop(pb, pb.SelectedEdges, out loop);
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

			pbUndo.RegisterCompleteObjectUndo(selection, "Delete Face");

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
				editor.ClearFaceSelection();
				editor.UpdateSelection();
			}

			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));

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

			int option = EditorUtility.DisplayDialogComplex(
				"Rules of Detachment",
				"Detach face selection to submesh or new ProBuilder object?",
				"New Object",		// 0
				"Submesh",			// 1
				"Cancel");			// 2

			switch(option)
			{
				case 0:
					return MenuDetachFacesToObject(selection);

				case 1:
					return MenuDetachFacesToSubmesh(selection);

				default:
					return pb_ActionResult.UserCanceled;
			}
		}

		/**
		 * Detach selected faces to submesh.
		 * ProBuilder only.
		 */
		public static pb_ActionResult MenuDetachFacesToSubmesh(pb_Object[] selection)
		{
			if(selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pbUndo.RegisterCompleteObjectUndo(selection, "Detach Face(s)");

			int count = 0;

			foreach(pb_Object pb in selection)
			{
				foreach(pb_Face face in pb.SelectedFaces)
					pb.DetachFace(face);

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();

				pb.SetSelectedFaces(pb.SelectedFaces);

				count += pb.SelectedFaceCount;
			}

			if(editor)
				editor.UpdateSelection();

			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));

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

			pbUndo.RegisterCompleteObjectUndo(selection, "Detach Selection to PBO");

			int detachedFaceCount = 0;
			List<GameObject> detached = new List<GameObject>();

			foreach(pb_Object pb in selection)
			{
				if(pb.SelectedFaceIndices.Length < 1 || pb.SelectedFaceIndices.Length == pb.faces.Length) continue;

				int[] primary = pb.SelectedFaceIndices;

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

				Undo.RegisterCreatedObjectUndo(copy.gameObject, "Detach Face");

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
				editor.SetSelection(detached.ToArray());
				editor.UpdateSelection();
			}

			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));

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

			pbUndo.RegisterCompleteObjectUndo(selection, "Merge Faces");

			int success = 0;

			foreach(pb_Object pb in selection)
			{
				if(pb.SelectedFaceCount > 1)
				{
					success += pb.SelectedFaceCount;

					pb_Face face = pb.MergeFaces(pb.SelectedFaces);

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

			pbUndo.RecordSelection(selection, "Flip Face Edges");
			int success = 0;

			foreach(pb_Object pb in selection)
			{

				foreach(pb_Face face in pb.SelectedFaces)
				{
					if( pb.FlipEdge(face) )
						success++;
				}

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			if(editor)
				editor.UpdateSelection();

			if(success > 0)
				return new pb_ActionResult(Status.Success, "Flipped " + success + " Edges");
			else
				return new pb_ActionResult(Status.Failure, "Flip Edges\nNo Faces Selected");
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

			bool collapseToFirst = pb_Preferences_Internal.GetBool(pb_Constant.pbCollapseVertexToFirst);

			pbUndo.RegisterCompleteObjectUndo(selection, "Collapse Vertices");

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

			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));

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

			pbUndo.RegisterCompleteObjectUndo(selection, "Weld Vertices");
			float weld = pb_Preferences_Internal.GetFloat(pb_Constant.pbWeldDistance);
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
							welds = new int[0];	// @todo

						pb.SetSelectedTriangles(welds ?? new int[0] {});
					}

					pb.Refresh();
					pb.Optimize();
				}

				weldCount -= pb.sharedIndices.Length;
			}

			if(editor)
				editor.UpdateSelection(true);

			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));

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

			float weldDistance = pb_Preferences_Internal.GetFloat(pb_Constant.pbWeldDistance);

			if(weldDistance <= MIN_WELD_DISTANCE)
				weldDistance = MIN_WELD_DISTANCE;

			EditorGUIUtility.labelWidth = width - 68;
			weldDistance = EditorGUILayout.FloatField(new GUIContent("Max", "The maximum distance between two vertices in order to be welded together."), weldDistance);

			if( EditorGUI.EndChangeCheck() )
			{
				if(weldDistance < MIN_WELD_DISTANCE)
					weldDistance = MIN_WELD_DISTANCE;
				EditorPrefs.SetFloat(pb_Constant.pbWeldDistance, weldDistance);
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
			pbUndo.RecordObjects(selection, "Split Vertices");

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
				foreach(pb_Face f in pb.SelectedFaces)
					pb.DetachFace(f);

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

			if(editor)
				editor.UpdateSelection();

			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));

			if(splitCount > 0)
				return new pb_ActionResult(Status.Success, "Split " + splitCount + (splitCount > 1 ? " Vertices" : " Vertex"));
			else
				return new pb_ActionResult(Status.Failure, "Split Vertices\nInsuffient Vertices Selected");
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

			pbUndo.RegisterCompleteObjectUndo(selection, "Subdivide Selection");

			int success = 0;

			foreach(pb_Object pb in selection)
			{
				if( pbSubdivideSplit.Subdivide(pb) )
					success++;

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
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

			int subdivisions = EditorPrefs.GetInt(pb_Constant.pbEdgeSubdivisions, 1);

			pbUndo.RegisterCompleteObjectUndo(selection, "Subdivide Edges");

			pb_ActionResult result = pb_ActionResult.NoSelection;

			foreach(pb_Object pb in selection)
			{
				List<pb_Edge> newEdgeSelection;

				result = pbVertexOps.AppendVerticesToEdge(pb, pb.SelectedEdges, subdivisions, out newEdgeSelection);

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
				pbUndo.RegisterCompleteObjectUndo(selection, "Subdivide Faces");

				pb_Face[] faces;

				if(pb.SubdivideFace(pb.SelectedFaces, out faces))
				{
					success += pb.SelectedFaces.Length;
					pb.SetSelectedFaces(faces);

					pb.ToMesh();
					pb.Refresh();
					pb.Optimize();
				}
			}

			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));

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
		public static pb_ActionResult MenuConnectEdges(pb_Object[] selection)
		{
			if(!editor || selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			pbUndo.RegisterCompleteObjectUndo(selection, "Connect Edges");

			int success = 0;

			foreach(pb_Object pb in selection)
			{
				pb_Edge[] edges;
				if(pb.ConnectEdges(pb.SelectedEdges, out edges))
				{
					pb.SetSelectedEdges(edges);

					pb.ToMesh();
					pb.Refresh();
					pb.Optimize();

					success++;
				}
			}

			if(success > 0)
			{
				if(editor)
					editor.UpdateSelection(true);

				return new pb_ActionResult(Status.Success, "Connect Edges");
			}
			else
			{
				Debug.LogWarning("No valid split paths found.  This is most likely because you are attempting to split edges that do belong to the same face, or do not have more than one edge selected.");
				return new pb_ActionResult(Status.Failure, "Connect Edges\nNo Edges Selected");
			}
		}

		/**
		 * Connects all currently selected vertices.
		 * ProBuilder only.
		 */
		public static pb_ActionResult MenuConnectVertices(pb_Object[] selection)
		{
			if(!editor || selection == null || selection.Length < 1)
				return pb_ActionResult.NoSelection;

			int success = 0;

			pbUndo.RegisterCompleteObjectUndo(selection, "Connect Vertices");

			foreach(pb_Object pb in selection)
			{
				int[] selectedTriangles = pb.SelectedTriangles.Distinct().ToArray();
				int len = selectedTriangles.Length;

				List<pb_VertexConnection> splits = new List<pb_VertexConnection>();
				List<pb_Face>[] connectedFaces = new List<pb_Face>[len];

				// For each vertex, get all it's connected faces
				for(int i = 0; i < len; i++)
					connectedFaces[i] = pbMeshUtils.GetNeighborFaces(pb, selectedTriangles[i]);

				for(int i = 0; i < len; i++)
				{
					foreach(pb_Face face in connectedFaces[i])
					{
						int index = splits.IndexOf((pb_VertexConnection)face);	// pb_VertexConnection only compares face property
						if(index < 0)
							splits.Add( new pb_VertexConnection(face, new List<int>(1) { selectedTriangles[i] } ) );
						else
							splits[index].indices.Add(selectedTriangles[i]);
					}
				}

				for(int i = 0; i < splits.Count; i++)
					splits[i] = splits[i].Distinct(pb.sharedIndices);

				int[] f;
				if(pb.ConnectVertices(splits, out f))
				{
					success++;
					pb.SetSelectedTriangles(f);
				}
			}

			foreach(pb_Object pb in selection)
			{
				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			if(success > 0)
			{
				if(editor)
					editor.UpdateSelection(true);

				return new pb_ActionResult(Status.Success, "Connect Vertices");
			}
			else
			{
				Debug.LogWarning("No valid split paths found.  This is could be because you are attempting to split between vertices that do not belong to the same face, or the split function can't find a good plane to re-triangulate from.");
				return new pb_ActionResult(Status.Failure, "Connect Vertices\nNo Valid Split Paths Found");
			}
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
			pbUndo.RegisterCompleteObjectUndo(selection, "Insert Edge Loop");

			foreach(pb_Object pb in selection)
			{
				pb_Edge[] edges;
				if( pb.ConnectEdges( pbMeshUtils.GetEdgeRing(pb, pb.SelectedEdges).ToArray(), out edges) )
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

			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));

			if(success > 0)
				return new pb_ActionResult(Status.Success, "Insert Edge Loop");
			else
				return new pb_ActionResult(Status.Success, "Insert Edge Loop");
		}

#endif
#endregion
	}
}

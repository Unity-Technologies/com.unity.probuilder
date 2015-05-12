using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using System.Reflection;
using ProBuilder2.MeshOperations;
using System.Linq;
using ProBuilder2.Math;

#if PB_DEBUG
using Parabox.Debug;
#endif

/**
 * Contains Menu commands for most ProBuilder operations.  Will 
 * also attempt to Update the pb_Editor if possible.
 */
public class pb_Menu_Commands : Editor
{
	private static pb_Editor editor { get { return pb_Editor.instance; } }

#region Object Level

#if !PROTOTYPE
	/**
	 * Combine selected pb_Objects to a single object.
	 * ProBuilder only.
	 */
	public static void MenuMergeObjects(pb_Object[] selected)	
	{
		if(selected.Length < 2)
		{
			pb_Editor_Utility.ShowNotification("Must Select 2+ Objects");
			return;
		}

		int option = EditorUtility.DisplayDialogComplex(
			"Save or Delete Originals?",
			"Saved originals will be deactivated and hidden from the Scene, but available in the Hierarchy.",
			"Merge Delete",		// 0
			"Merge Save",		// 1
			"Cancel");			// 2

		pb_Object pb = null;

		if(option == 2) return;

		if( pbMeshOps.CombineObjects(selected, out pb) )
		{
			pb_Editor_Utility.SetEntityType(selected[0].GetComponent<pb_Entity>().entityType, pb.gameObject);
			pb_Lightmap_Editor.SetObjectUnwrapParamsToDefault(pb);			
			pb.gameObject.AddComponent<MeshCollider>().convex = false;

			pb.Optimize();

			pb.gameObject.name = "pb-MergedObject" + pb.id;

			switch(option)
			{
				case 0: 	// Delete donor objects
					for(int i = 0; i < selected.Length; i++)
					{
						pbUndo.DestroyImmediate(selected[i].gameObject, "Delete Merged Objects");
					}

					break;

				case 1:
					foreach(pb_Object sel in selected)
						sel.gameObject.SetActive(false);
					break;
			}

			Undo.RegisterCreatedObjectUndo(pb.gameObject, "Merge Objects");

			Selection.activeTransform = pb.transform;
		}

		if(editor)
			editor.UpdateSelection();
	}
#endif

	/**
	 * Adds pb_Object and pb_Entity to object without duplicating the objcet.  Is undo-able.
	 */
	public static void ProBuilderize(GameObject[] selected, bool preserveFaces)
	{
		foreach(GameObject go in selected)
		{
			Undo.RegisterFullObjectHierarchyUndo(go, "ProBuilderize");

			MeshRenderer mr = go.GetComponent<MeshRenderer>();

			pb_Object pb = pbMeshOps.AddPbObjectToObject(go, preserveFaces);

			EntityType entityType = EntityType.Detail;

			if(mr != null && mr.sharedMaterials != null && mr.sharedMaterials.Any(x => x != null && x.name.Contains("Collider")))
				entityType = EntityType.Collider;
			else
			if(mr != null && mr.sharedMaterials != null && mr.sharedMaterials.Any(x => x != null && x.name.Contains("Trigger")))
				entityType = EntityType.Trigger;

			// if this was previously a pb_Object, or similarly any other instance asset, destroy it.
			// if it is backed by saved asset, leave the mesh asset alone but assign a new mesh to the 
			// renderer so that we don't modify the asset.
			if( AssetDatabase.GetAssetPath(go.GetComponent<MeshFilter>().sharedMesh) == "" )
				Undo.DestroyObjectImmediate(go.GetComponent<MeshFilter>().sharedMesh);
			else
				go.GetComponent<MeshFilter>().sharedMesh = new Mesh();

			pb.ToMesh();
			pb.Refresh(); 
			pb.Optimize();

			// Don't call the editor version of SetEntityType because that will
			// reset convexity and trigger settings, which we can assume are user
			// set already.
			if( !pb.gameObject.GetComponent<pb_Entity>() )
				pb.gameObject.AddComponent<pb_Entity>().SetEntity(entityType);
			else
				pb.gameObject.GetComponent<pb_Entity>().SetEntity(entityType);
			// pb_Editor_Utility.SetEntityType(entityType, t.gameObject);
		}

		if(pb_Editor.instance != null)
			pb_Editor.instance.UpdateSelection();
	}

	/**
	 * Set the pivot to the center of the current element selection.
	 * ProBuilder only.
	 */
	public static void MenuSetPivot(pb_Object[] selection)
	{
		if(selection.Length > 0)
			pb_Editor_Utility.ShowNotification("Set Pivot");
		else
			pb_Editor_Utility.ShowNotification("Nothing Selected");

		Object[] objects = new Object[selection.Length * 2];
		System.Array.Copy(selection, 0, objects, 0, selection.Length);
		for(int i = selection.Length; i < objects.Length; i++)
			objects[i] = selection[i-selection.Length].transform;

		pbUndo.RecordObjects(objects, "Set Pivot");

		foreach (pb_Object pb in selection)
		{
			pb.ToMesh();

			if (pb.SelectedTriangles.Length > 0)
				pb.CenterPivot(pb.SelectedTriangles);
			else
				pb.CenterPivot(null);
	
			pb.Refresh();
			pb.Optimize();
		}
		
		SceneView.RepaintAll();

		if(editor != null)
			editor.UpdateSelection();
	}	

	/**
	 * Set the pb_Entity entityType on selection.
	 */
	public static void MenuSetEntityType(pb_Object[] selection, EntityType entityType)
	{
		if(selection.Length < 1)
		{
			pb_Editor_Utility.ShowNotification("Nothing Selected");
			return;
		}
		
		Object[] undoObjects = selection.SelectMany(x => x.GetComponents<Component>()).ToArray();

		pbUndo.RecordObjects(undoObjects, "Set Entity Type");

		foreach(pb_Object pb in selection)
		{
			pb_Editor_Utility.SetEntityType(entityType, pb.gameObject);
			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
		}

		pb_Editor_Utility.ShowNotification("Set " + entityType);
	}


#if !PROTOTYPE

	enum BooleanOperation
	{
		Union,
		Subtract,
		Intersect
	}

	static void MenuBooleanOperation(BooleanOperation operation, pb_Object lhs, pb_Object rhs)
	{
		if(lhs == null || rhs == null)
		{
			pb_Editor_Utility.ShowNotification("Must Select 2 Objects");	
			return;
		}

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

		pb_Editor_Utility.ShowNotification(op_string);
	}

	/**
	 * Union operation between two ProBuilder objects.
	 */
	public static void MenuUnion(pb_Object lhs, pb_Object rhs)
	{
		MenuBooleanOperation(BooleanOperation.Union, lhs, rhs);
	}

	/**
	 * Subtract boolean operation between two pb_Objects.
	 */
	public static void MenuSubtract(pb_Object lhs, pb_Object rhs)
	{
		MenuBooleanOperation(BooleanOperation.Subtract, lhs, rhs);
	}
	
	/**
	 * Intersect boolean operation between two pb_Objects.
	 */
	public static void MenuIntersect(pb_Object lhs, pb_Object rhs)
	{
		MenuBooleanOperation(BooleanOperation.Intersect, lhs, rhs);
	}
#endif

#endregion

#region Normals

	/**
	 * Flips all face normals if editLevel == EditLevel.Top, else flips only pb_Object->SelectedFaces
	 */
	public static void MenuFlipNormals(pb_Object[] selected)
	{
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
			pb_Editor_Utility.ShowNotification("Flip " + c + (c > 1 ? " Faces" : " Face"));
		else
			pb_Editor_Utility.ShowNotification("Flip Normals\nNo Faces Selected!");
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
	public static void MenuExtrude(pb_Object[] selection)
	{
		pb_Object[] pbs = pbUtil.GetComponents<pb_Object>(Selection.transforms);

		pbUndo.RecordObjects(pbUtil.GetComponents<pb_Object>(Selection.transforms), "Extrude selected.");

		int extrudedFaceCount = 0;
		bool success = false;

		foreach(pb_Object pb in pbs)
		{
			pb.ToMesh();
			pb.RefreshNormals();

			if(editor && editor.selectionMode == SelectMode.Edge)
			{
				if(pb.SelectedEdges.Length < 1 || pb.SelectedFaceCount > 0)
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

			}

			if(!success)
			{
				if(pb.SelectedFaces.Length < 1)
					continue;
				
				extrudedFaceCount += pb.SelectedFaces.Length;

				pb_Face[] result;
				pb.Extrude(	pb.SelectedFaces,
							pb_Preferences_Internal.GetFloat(pb_Constant.pbExtrudeDistance),
							pb_Preferences_Internal.GetBool(pb_Constant.pbExtrudeAsGroup),
							out result);
				
				pb.SetSelectedFaces(pb.SelectedFaces);
			}

			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
		}

		if(extrudedFaceCount > 0)
			pb_Editor_Utility.ShowNotification("Extrude");// + val, "Extrudes the selected faces / edges.");

		if(editor != null)
			editor.UpdateSelection();
			
		EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));

		SceneView.RepaintAll();
	}

	/**
	 * Create a face between two edges.
	 */
	#if !PROTOTYPE
	public static void MenuBridgeEdges(pb_Object[] selection)
	{
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

		if(success)
		{
			pb_Editor.instance.UpdateSelection();
			pb_Editor_Utility.ShowNotification("Bridge Edges");
		}
		else
		{
			Debug.LogWarning("Failed Bridge Edges.  Bridge Edges requires that only 2 edges be selected, and they must both only have one connecting face (non-manifold).");
		}
		
		EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
	}
	#endif
#endregion

#region Selection

	/**
	 * Grow selection to plane using max angle diff.
	 */
	public static void MenuGrowSelection(pb_Object[] selection)
	{
		pbUndo.RecordObjects(selection, "Grow Selection");

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
							Dictionary<pb_Face, List<pb_Face>> faceLookup = pbMeshUtils.GenerateNeighborLookup(pb, pb.faces);

							bool facesAdded = true;

							List<pb_Face> newFaces = new List<pb_Face>();

							while(facesAdded)
							{
								facesAdded = false;

								foreach(pb_Face f in perimeterFaces)
								{
									Vector3 nrm = pb_Math.Normal( pb.GetVertices(f.indices) );

									List<pb_Face> adjacent = faceLookup[f].Where(x => !selected.Contains(x)).ToList();

									foreach(pb_Face connectedFace in adjacent)
									{
										float angle = Vector3.Angle( nrm, pb_Math.Normal( pb.GetVertices(connectedFace.indices)) );

										if( angle < growSelectionAngle )
										{
											selected.Add(connectedFace);
											newFaces.Add(connectedFace);
											facesAdded = true;
										}
									}
								}

								perimeterFaces = new List<pb_Face>(newFaces);
								newFaces.Clear();
							}

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
								List<pb_Face> adjacent = pbMeshUtils.GetNeighborFaces(pb, lookup, perimeterFaces, perimeterFaces[i]);

								Vector3 perim_normal = pb_Math.Normal(pb.GetVertices(perimeterFaces[i].indices));

								for(int n = 0; n < adjacent.Count; n++)
								{
									Vector3 adjacent_normal = pb_Math.Normal( pb.GetVertices(adjacent[n].indices) );

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

		if(editor != null)
			editor.UpdateSelection(false);

		pb_Editor_Utility.ShowNotification(grown > 0 ? "Grow Selection" : "Nothing to Grow");

		SceneView.RepaintAll();
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

	/**
	 * Shrink selection.
	 * Note - requires a reference to an open pb_Editor be passed.  This is because shrink
	 * vertices requires access to the Selected_Universal_Edges_All array.
	 */
	public static void MenuShrinkSelection(pb_Object[] selection)
	{
		// @TODO
		if(editor == null)
		{
			pb_Editor_Utility.ShowNotification("ProBuilder Editor Not Open!");
			return;
		}

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

		if(selection.Length > 0 && rc > 0)
			pb_Editor_Utility.ShowNotification("Shrink Selection");
		else
			pb_Editor_Utility.ShowNotification("Unable to Shrink");

		if(editor)
			editor.UpdateSelection(false);

		EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
	}

	/**
	 * Invert the current selection.
	 */
	public static void MenuInvertSelection(pb_Object[] selection)
	{
		pbUndo.RecordObjects(selection, "Invert Selection");

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
		
		pb_Editor_Utility.ShowNotification("Invert Selection");

		SceneView.RepaintAll();
	}

	/**
	 * Expands the current selection using a "Ring" method.
	 */
	public static void MenuRingSelection(pb_Object[] selection)
	{
		pbUndo.RecordObjects(selection, "Select Edge Ring");

		#if PB_DEBUG
		pb_Profiler profiler = new pb_Profiler();
		profiler.BeginSample("MenuRingSelection");
		#endif

		bool success = false;

		foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
		{
			pb_Edge[] edges = pbMeshUtils.GetEdgeRing(pb, pb.SelectedEdges);

			if(edges.Length > pb.SelectedEdges.Length)
				success = true;

			pb.SetSelectedEdges( edges );
		}

		#if PB_DEBUG
		profiler.EndSample();
		Bugger.Log( profiler.ToString() );
		#endif

		if(editor)
			editor.UpdateSelection(false);

		pb_Editor_Utility.ShowNotification(success ? "Select Edge Ring" : "Nothing to Ring");

		SceneView.RepaintAll();
	}
		
	/**
	 * Selects an Edge loop. Todo - support for face loops.
	 */
	public static void MenuLoopSelection(pb_Object[] selection)
	{
		pbUndo.RecordObjects(selection, "Select Edge Loop");

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

		pb_Editor_Utility.ShowNotification(foundLoop ? "Select Edge Loop" : "Nothing to Loop");

		SceneView.RepaintAll();
	}
#endregion

#region Delete / Detach

	#if !PROTOTYPE

	/**
	 * Delete selected faces.
	 * ProBuilder only.
	 */
	public static void MenuDeleteFace(pb_Object[] selection)
	{
		pbUndo.RecordObjects(selection as Object[], "Delete Face(s)");

		foreach(pb_Object pb in selection)
		{
			pb.DeleteFaces(pb.SelectedFaces);

			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
		}

		if(editor)
		{
			editor.ClearFaceSelection();
			editor.UpdateSelection();
		}
		
		pb_Editor_Utility.ShowNotification("Delete Elements");

		EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
	}

	/**
	 * Delete selected vertices and attempt to retriangulate a super face.
	 */
	public static void MenuDeleteVertices(pb_Object[] selection)
	{
		pbUndo.RecordObjects(selection, "Delete Vertices");

		foreach(pb_Object pb in selection)
		{
			int[] selected = pb.sharedIndices.AllIndicesWithValues(pb.SelectedTriangles).ToArray();

			pb_Face[] selected_faces = pbMeshUtils.GetNeighborFaces(pb, selected).ToArray();

			if(selected_faces.Length < 1)
				continue;

			Vector3 nrm = pb_Math.Normal(pb, selected_faces[0]);

			pb.DeleteVerticesWithIndices(selected);

			pb_Face composite = pb.MergeFaces(selected_faces);

			pb.TriangulateFace(composite, nrm);

			int[] removed;
			pb.RemoveDegenerateTriangles(out removed);

			if(composite != null)
			{
				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();

				pb.SetSelectedFaces( new pb_Face[] { composite } );
			}
		}

		if(pb_Editor.instance)
			pb_Editor.instance.UpdateSelection();
	}

	public static void MenuDetachFacesContext(pb_Object[] selection)
	{
		int option = EditorUtility.DisplayDialogComplex(
			"Rules of Detachment",
			"Detach face selection to submesh or new ProBuilder object?",
			"New Object",		// 0
			"Submesh",			// 1
			"Cancel");			// 2

		switch(option)
		{
			case 0:
				MenuDetachFacesToObject(selection);
				break;
			case 1:
				MenuDetachFacesToSubmesh(selection);
				break;
			case 2:
				break;
		}
	}

	/**
	 * Detach selected faces to submesh.
	 * ProBuilder only.
	 */
	public static void MenuDetachFacesToSubmesh(pb_Object[] selection)
	{
		pbUndo.RecordObjects(selection, "Detach Face(s)");

		foreach(pb_Object pb in selection)
		{
			foreach(pb_Face face in pb.SelectedFaces)
				pb.DetachFace(face);

			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
			
			pb.SetSelectedFaces(pb.SelectedFaces);
		}

		if(editor)
			editor.UpdateSelection();

		pb_Editor_Utility.ShowNotification("Detach Face");

		EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
	}

	/**
	 * Detaches currently selected faces to a new ProBuilder object.
	 * ProBuilder only.
	 */
	public static void MenuDetachFacesToObject(pb_Object[] selection)
	{
		if(!editor) return;

		pbUndo.RecordObjects(selection, "Detach Selection to PBO");

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
			if( pb_Editor_Utility.IsPrefabInstance(copy.gameObject) || pb_Editor_Utility.IsPrefabRoot(copy.gameObject) )
				PrefabUtility.DisconnectPrefabInstance(copy.gameObject);

			if(copy.transform.childCount > 0)
			{
				for(int i = 0; i < copy.transform.childCount; ++i)
					GameObject.DestroyImmediate(copy.transform.GetChild(i).gameObject);

				foreach(pb_Object pb_child in pb.transform.GetComponentsInChildren<pb_Object>())
					pb_child.Verify();
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

		if(detachedFaceCount > 0)
			pb_Editor_Utility.ShowNotification("Detach " + detachedFaceCount + " faces to new Object");
		else
			Debug.LogWarning("No faces selected! Please select some faces and try again.");

		EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
	}

	#endif
#endregion

#region Face / Triangles

	public static void MenuMergeFaces(pb_Object[] selection)
	{
		pbUndo.RecordObjects(selection, "Merge Faces");

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

		if(success > 0)
		{
			pb_Editor_Utility.ShowNotification("Merged " + success + " Faces");
		}

		if(editor)
		{
			editor.UpdateSelection();
		}
	}
#endregion

#region Vertex Operations

	#if !PROTOTYPE

	/**
	 * Collapse selected vertices
	 * ProBuilder only.
	 */
	public static void MenuCollapseVertices(pb_Object[] selection)
	{
		bool success = false;
		
		pbUndo.RecordObjects(selection, "Collapse Vertices");
		
		foreach(pb_Object pb in selection)
		{
			if(pb.SelectedTriangles.Length > 1)
			{
				pb.ToMesh();

				int newIndex = -1;
				success = pb.MergeVertices(pb.SelectedTriangles, out newIndex);
					
				if(success)
				{
					int[] removed;
					pb.RemoveDegenerateTriangles(out removed);
					pb.SetSelectedTriangles(new int[] { newIndex });
				}
				
				pb.Refresh();
				pb.Optimize();
			}
		}

		if(success)
			pb_Editor_Utility.ShowNotification("Collapse Vertices");

		if(editor)
			editor.UpdateSelection();

		EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
	}

	/**
	 * Weld all selected vertices.
	 * ProBuilder only.
	 */
	public static void MenuWeldVertices(pb_Object[] selection)
	{
		bool success = false;

		pbUndo.RecordObjects(selection, "Weld Vertices");
		float weld = pb_Preferences_Internal.GetFloat(pb_Constant.pbWeldDistance);
		int weldCount = 0;

		foreach(pb_Object pb in selection)
		{
			weldCount += pb.sharedIndices.Length;

			if(pb.SelectedTriangles.Length > 1)
			{
				pb.ToMesh();
				
				int[] welds;
				success = pb.WeldVertices(pb.SelectedTriangles, weld, out welds);

				if(success)
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


		if(success && weldCount > 0)
			pb_Editor_Utility.ShowNotification("Weld " + weldCount + (weldCount > 1 ? " Vertices" : " Vertex"));
		else
			pb_Editor_Utility.ShowNotification("Nothing to Weld");

		if(editor)
			editor.UpdateSelection(true);

		EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
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
	public static void MenuSplitVertices(pb_Object[] selection)
	{
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
			pb.SplitVertices(pb.SelectedTriangles);
	
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

		pb_Editor_Utility.ShowNotification("Split " + splitCount + (splitCount > 1 ? " Vertices" : " Vertex"));

		if(editor)
			editor.UpdateSelection();
		EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
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
	public static void MenuSubdivide(pb_Object[] selection)
	{
		if(selection.Length < 1)
		{
			pb_Editor_Utility.ShowNotification("Nothing Selected");
			return;
		}

		pbUndo.RecordObjects(selection, "Subdivide Selection");
		int success = 0;

		foreach(pb_Object pb in selection)
		{
			if( pbSubdivideSplit.Subdivide(pb) )
				success++;

			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
		}

		/*
			// Vertex subdivision
			foreach(pb_Object pb in selection)
			{
				List<pb_VertexConnection> vertexConnections = new List<pb_VertexConnection>();

				foreach(pb_Face f in pb.faces)
					vertexConnections.Add( new pb_VertexConnection(f, new List<int>(f.distinctIndices) ) );

				pb_Face[] faces;

				if(pb.ConnectVertices(vertexConnections, out faces))	
				{
					pb.SetSelectedFaces(faces);
					success++;

					pb.GenerateUV2(pb_Editor.show_NoDraw);
					pb.Refresh();
				}
			}
		h*/

		pb_Editor_Utility.ShowNotification("Subdivide Object");

		if(editor)
			editor.UpdateSelection(true);
	}

	/**
	 * Subdivides all currently selected faces.
	 * ProBuilder only.
	 */
	public static void MenuSubdivideFace(pb_Object[] selection)
	{
		int success = 0;

		foreach(pb_Object pb in selection)
		{
			pbUndo.RecordObject(pb, "Subdivide Face");
			
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

		if(success > 0)
		{
	        pb_Editor_Utility.ShowNotification("Subdivide " + success + ((success > 1) ? " faces" : " face"));
			
			if(editor)
				editor.UpdateSelection(true);
		}
		else
		{
			Debug.LogWarning("Subdivide faces failed - did you not have any faces selected?");
		}

		EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
	}

	/**
	 * Connects all currently selected edges.
	 * ProBuilder only.
	 */
	public static void MenuConnectEdges(pb_Object[] selection)
	{
		pbUndo.RecordObjects(selection, "Connect Edges");
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
			{
				editor.UpdateSelection(true);
			}

			pb_Editor_Utility.ShowNotification("Connect Edges");
		}
		else
		{
			Debug.LogWarning("No valid split paths found.  This is most likely because you are attempting to split edges that do belong to the same face, or do not have more than one edge selected.");
		}
	}

	/**
	 * Connects all currently selected vertices.
	 * ProBuilder only.
	 */
	public static void MenuConnectVertices(pb_Object[] selection)
	{
		int success = 0;

		pbUndo.RecordObjects(selection, "Connect Vertices");
		
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
			pb_Editor_Utility.ShowNotification("Connect Vertices");

			if(editor)
			{
				editor.UpdateSelection(true);
			}
		}
		else
		{
			Debug.LogWarning("No valid split paths found.  This is most likely because you are attempting to split vertices that do not belong to the same face.  This is not currently supported, sorry!");
		}
	}

	/**
	 * Inserts an edge loop along currently selected Edges.
	 * ProBuilder only.
	 */
	public static void MenuInsertEdgeLoop(pb_Object[] selection)
	{
		int success = 0;
		pbUndo.RecordObjects(selection, "Insert Edge Loop");

		foreach(pb_Object pb in selection)
		{
			pb_Edge[] edges;
			if( pb.ConnectEdges( pbMeshUtils.GetEdgeRing(pb, pb.SelectedEdges), out edges) )
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
			pb_Editor_Utility.ShowNotification("Insert Edge Loop");
		}

		if(editor)
			editor.UpdateSelection(true);

		EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
	}

	#endif
#endregion
}

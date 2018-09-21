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
		static ProBuilderEditor editor { get { return ProBuilderEditor.instance; } }

#region Object Level

		public static ActionResult MenuMergeObjects(ProBuilderMesh[] selected)
		{
			if(selected.Length < 2)
				return new ActionResult(ActionResult.Status.Canceled, "Must Select 2+ Objects");

			List<ProBuilderMesh> res = InternalMeshUtility.CombineObjects(selected);

			if (res != null)
			{
				foreach (var mesh in res)
				{
					mesh.Optimize();
					mesh.gameObject.name = "pb-MergedObject" + mesh.id;
					UndoUtility.RegisterCreatedObjectUndo(mesh.gameObject, "Merge Objects");
					Selection.objects = res.Select(x => x.gameObject).ToArray();
				}

				// Delete donor objects
				for(int i = 0; i < selected.Length; i++)
				{
					if(selected[i] != null)
						UndoUtility.DestroyImmediate(selected[i].gameObject);
				}
			}

			ProBuilderEditor.Refresh();

			return new ActionResult(ActionResult.Status.Success, "Merged Objects");
		}

		/**
		 * Set the pivot to the center of the current element selection.
		 * ProBuilder only.
		 */
		public static ActionResult MenuCenterPivot(ProBuilderMesh[] selection)
		{
			return SetPivotInternal(selection, null);
		}

		public static ActionResult MenuSetPivot(ProBuilderMesh[] selection)
		{
			int[][] tri = new int[selection.Length][];

			for(int i = 0; i < tri.Length; i++)
				tri[i] = selection[i].selectedVertices.ToArray();

			return SetPivotInternal(selection, tri);
		}

		static ActionResult SetPivotInternal(ProBuilderMesh[] selection, int[][] triangles = null)
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

			Vector3[][] positions = new Vector3[selection.Length][];

			for(int i = 0; i < selection.Length; i++)
				positions[i] = selection[i].VerticesInWorldSpace();

			for(int i = 0; i < selection.Length; i++)
			{
				ProBuilderMesh pb = selection[i];

				pb.transform.position = Vector3.zero;
				pb.transform.rotation = Quaternion.identity;
				pb.transform.localScale = Vector3.one;

				foreach(Face face in pb.facesInternal)
					face.manualUV = true;

				pb.positions = positions[i];

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

		public static void MenuOpenVertexColorsEditor()
		{
			VertexColorPalette.MenuOpenWindow();
		}

		public static ActionResult MenuTriangulateObject(ProBuilderMesh[] selection)
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
				selection[i].ClearSelection();
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

			go.AddComponent<MeshRenderer>().sharedMaterial = EditorUtility.GetUserMaterial();
			go.AddComponent<MeshFilter>().sharedMesh = c;

			ProBuilderMesh pb = InternalMeshUtility.CreateMeshWithTransform(go.transform, false);
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
			int faceCount = MeshSelection.selectedFaceCount;

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

			UndoUtility.RecordSelection(selection, "Conform " + (MeshSelection.selectedFaceCount > 0 ? "Face" : "Object") + " Normals.");

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

		public static ActionResult MenuBridgeEdges(ProBuilderMesh[] selection)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RecordSelection(selection, "Bridge Edges");

			bool success = false;

			foreach(ProBuilderMesh pb in selection)
			{
				if(pb.selectedEdgeCount == 2)
				{
					if(pb.Bridge(pb.selectedEdges[0], pb.selectedEdges[1], ProBuilderEditor.s_AllowNonManifoldActions) != null)
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

#endregion

#region Selection


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

		public static ActionResult MenuSplitVertices(ProBuilderMesh[] selection)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			int splitCount = 0;
			UndoUtility.RecordSelection(selection, "Split Vertices");

			foreach(ProBuilderMesh mesh in selection)
			{
				// loose verts to split
				List<int> tris = new List<int>(mesh.selectedIndexesInternal);

				if (mesh.selectedFacesInternal.Length > 0)
				{
					int[] sharedVertexHandles = new int[mesh.selectedIndexesInternal.Length];

					// Get shared index index for each vert in selection
					for (int i = 0; i < mesh.selectedIndexesInternal.Length; i++)
						sharedVertexHandles[i] = mesh.GetSharedVertexHandle(mesh.selectedIndexesInternal[i]);

					// cycle through selected faces and remove the tris that compose full faces.
					foreach (Face face in mesh.selectedFacesInternal)
					{
						List<int> faceSharedIndexes = new List<int>();

						for (int j = 0; j < face.distinctIndexesInternal.Length; j++)
							faceSharedIndexes.Add(mesh.GetSharedVertexHandle(face.distinctIndexesInternal[j]));

						List<int> usedTris = new List<int>();
						for (int i = 0; i < sharedVertexHandles.Length; i++)
							if (faceSharedIndexes.Contains(sharedVertexHandles[i]))
								usedTris.Add(mesh.selectedIndexesInternal[i]);

						// This face *is* composed of selected tris.  Remove these tris from the loose index list
						foreach (int i in usedTris)
							if (tris.Contains(i))
								tris.Remove(i);
					}
				}

				// Now split the faces, and any loose vertices
				mesh.DetachFaces(mesh.selectedFacesInternal);

				splitCount += mesh.selectedIndexesInternal.Length;
				mesh.SplitVertices(mesh.selectedIndexesInternal);

				// Reattach detached face vertices (if any are to be had)
				if(mesh.selectedFacesInternal.Length > 0)
					mesh.WeldVertices(mesh.selectedFacesInternal.SelectMany(x => x.indexes), Mathf.Epsilon);

				// And set the selected triangles to the newly split
				List<int> newTriSelection = new List<int>(mesh.selectedFacesInternal.SelectMany(x => x.indexes));
				newTriSelection.AddRange(tris);
				mesh.SetSelectedVertices(newTriSelection.ToArray());

				mesh.ToMesh();
				mesh.Refresh();
				mesh.Optimize();
			}

			ProBuilderEditor.Refresh();

			if(splitCount > 0)
				return new ActionResult(ActionResult.Status.Success, "Split " + splitCount + (splitCount > 1 ? " Vertices" : " Vertex"));
			else
				return new ActionResult(ActionResult.Status.Failure, "Split Vertices\nInsuffient Vertices Selected");
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
				Edge[] connections;
				Face[] faces;

				res = ConnectElements.Connect(pb, pb.selectedEdges, out faces, out connections, true, true);

				if (connections != null)
				{
					pb.SetSelectedEdges(connections);
					pb.Refresh();
					pb.Optimize();
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
				int[] splits = pb.Connect(pb.selectedIndexesInternal);

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

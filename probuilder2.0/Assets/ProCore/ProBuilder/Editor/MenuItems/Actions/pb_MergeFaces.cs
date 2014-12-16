using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;

/**
 * Merge 2 or more faces into a single face.  Merged face
 * retains the properties of the first selected face in the
 * event of conflicts.
 *
 * Note!! This is included by default in ProBuilder 2.4+
 */
public class pb_MergeFaces : Editor
{
	[MenuItem("Tools/ProBuilder/Geometry/Delete Edge", true, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_EDGE)]
	public static bool MenuVerifyDeleteEdge()
	{
		pb_Editor editor = pb_Editor.instance;
		return editor && editor.editLevel == EditLevel.Geometry && editor.selectionMode == SelectMode.Edge && editor.selectedEdgeCount > 0;
	}

	[MenuItem("Tools/ProBuilder/Geometry/Delete Edge", false, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_EDGE)]
	public static void MenuMergeFaces()
	{
		pbUndo.RecordObjects(Selection.transforms, "Merge Faces");

		foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
		{
			if(pb.SelectedFaceCount > 1)
			{
				
				MergeFaces(pb, pb.SelectedFaces);
			}
		}

		pb_Editor_Utility.ShowNotification("Merged Faces");

		if(pb_Editor.instance)
			pb_Editor.instance.UpdateSelection(true);
	}

	static void MergeFaces(pb_Object pb, pb_Face[] faces)
	{
		List<int> collectedIndices = new List<int>(faces[0].indices);
		
		for(int i = 1; i < faces.Length; i++)
		{
			collectedIndices.AddRange(faces[i].indices);
		}

		pb_Face mergedFace = new pb_Face(collectedIndices.ToArray(),
		                                 faces[0].material,
		                                 faces[0].uv,
		                                 faces[0].smoothingGroup,
		                                 faces[0].textureGroup,
		                                 faces[0].elementGroup,
		                                 faces[0].manualUV);

		pb_Face[] rebuiltFaces = new pb_Face[pb.faces.Length - faces.Length + 1];

		int n = 0;
		foreach(pb_Face f in pb.faces)
		{
			if(System.Array.IndexOf(faces, f) < 0)
			{
				rebuiltFaces[n++] = f;
			}
		}
		
		rebuiltFaces[n] = mergedFace;

		pb.SetFaces(rebuiltFaces);

		// merge vertices that are on top of one another now that they share a face
		Dictionary<int, int> shared = new Dictionary<int, int>();

		for(int i = 0; i < mergedFace.indices.Length; i++)
		{
			int sharedIndex = pb.sharedIndices.IndexOf(mergedFace.indices[i]);

			if(shared.ContainsKey(sharedIndex))
			{
				mergedFace.indices[i] = shared[sharedIndex];
			}
			else
			{
				shared.Add(sharedIndex, mergedFace.indices[i]);
			}
		}

		pb.RemoveUnusedVertices();

		pb.Refresh();
	}
}

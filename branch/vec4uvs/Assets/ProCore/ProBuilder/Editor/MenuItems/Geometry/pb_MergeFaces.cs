using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;

#if !PROTOTYPE

/**
 * Merge 2 or more faces into a single face.  Merged face
 * retains the properties of the first selected face in the
 * event of conflicts.
 *
 * Note!! This is included by default in ProBuilder 2.4+
 */
public class pb_MergeFaces : Editor
{
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Merge Faces", true, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE)]
	public static bool MenuVerifyDeleteEdge()
	{
		pb_Editor editor = pb_Editor.instance;
		return editor && editor.editLevel == EditLevel.Geometry && editor.selectedFaceCount > 0;
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Merge Faces", false, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE)]
	public static void MenuMergeFaces()
	{
		pbUndo.RecordObjects(Selection.transforms, "Merge Faces");

		foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
		{
			if(pb.SelectedFaceCount > 1)
			{		
				pb.SetSelectedFaces( new pb_Face[] { pb.MergeFaces(pb.SelectedFaces) } );
			}
		}

		pb_Editor_Utility.ShowNotification("Merged Faces");

		if(pb_Editor.instance)
		{
			pb_Editor.instance.UpdateSelection(true);
		}
	}
}

#endif
using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

public class pb_ConformNormals : Editor
{
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Conform Normals", true,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 2)]
	public static bool MenuVerifyConformNormals()
	{
		return pb_Editor.instance != null && pb_Editor.instance.selection.Length > 0;
	}	

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Conform Normals", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 2)]
	public static void MenuConformNormals()
	{
		pb_Editor editor = pb_Editor.instance;
		if(editor == null) return;	// this should be redundant, but y'know... safety first?

		pb_Object[] selection = pbUtil.GetComponents<pb_Object>(Selection.transforms);

		pbUndo.RecordObjects(selection, "Conform " + (editor.selectedFaceCount > 0 ? "Face" : "Object") + " Normals.");

		foreach(pb_Object pb in selection)
		{
			pb_Face[] faces = pb.SelectedFaceCount > 0 ? pb.SelectedFaces : pb.faces;
			int len = faces.Length;

			int toggle = 0;
			WindingOrder[] winding = new WindingOrder[len];

			// First figure out what the majority of the faces' winding order is
			for(int i = 0; i < len; i++)
			{
				winding[i] = pb.GetWindingOrder( faces[i] );
				toggle += (winding[i] == WindingOrder.Unknown ? 0: (winding[i] == WindingOrder.Clockwise ? 1 : -1));
			}

			int flipped = 0;

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

			if(pb_Editor.instance != null)
				pb_Editor.instance.UpdateSelection();

			pb_Editor_Utility.ShowNotification(flipped > 0 ? "Reversed " + flipped + " Faces" : "Normals Already Uniform");
		}
	}
}

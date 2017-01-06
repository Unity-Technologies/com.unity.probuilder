using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
using System.Linq;
using System.Text;
using System;
using System.Reflection;

public class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		pb_Object[] selection = Selection.transforms.GetComponents<pb_Object>();

		pbUndo.RecordObjects(selection, "Extrude Faces (Experimental)");

		foreach(pb_Object pb in selection)
		{
			pb.Extrude(pb.SelectedFaces);

			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
		}

		pb_Editor.Refresh(true);
	}

}

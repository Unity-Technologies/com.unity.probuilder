using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
using System.Linq;
using System.Text;
using System.Reflection;

using Parabox.Debug;

public class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		pb_Object[] selection = Selection.transforms.GetComponents<pb_Object>();

		pbUndo.RecordSelection(selection, "Bevel Edges");

		foreach(pb_Object pb in selection)
		{
			profiler.Begin("bevel edges");

			pb.ToMesh();
			
			pb_ActionResult result = pb_Bevel.BevelEdges(pb, pb.SelectedEdges, .05f);
			pb_EditorUtility.ShowNotification(result.notification);

			pb.Refresh();
			pb.Optimize();

			profiler.End();
		}

		pb_Editor.Refresh();
	}
}

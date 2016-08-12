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

		// foreach(pb_Object pb in selection)
		// {
		// 	// profiler.Begin("Grow Selection (Old)");
		// 	// pb_Menu_Commands.MenuGrowSelection(selection);
		// 	// profiler.End();
			
			profiler.Begin("Grow Selection (New)");
			MenuGrowSelection(selection);
			profiler.End();
		// }

		pb_Editor.Refresh();
	}

	public static pb_ActionResult MenuGrowSelection(pb_Object[] selection)
	{
		pbUndo.RecordSelection(selection, "Grow Selection");

		bool iterative = pb_Preferences_Internal.GetBool(pb_Constant.pbGrowSelectionAngleIterative);
		float growSelectionAngle = pb_Preferences_Internal.GetFloat("pbGrowSelectionAngle");

		foreach(pb_Object pb in selection)
		{
			pb_Face[] selectedFaces = pb.SelectedFaces;

			HashSet<pb_Face> sel;

			if(iterative)
			{
				sel = GrowSelection(pb, selectedFaces, growSelectionAngle);
				sel.UnionWith(selectedFaces);
			}
			else
			{
				sel = FloodSelection(pb, selectedFaces, growSelectionAngle);
			}

			pb.SetSelectedFaces( sel.ToArray() );
		}

		pb_Editor.Refresh();

		return new pb_ActionResult(Status.Success, "Grow Selection");
	}

	private static HashSet<pb_Face> GrowSelection(pb_Object pb, IList<pb_Face> faces, float maxAngleDiff = -1f)
	{
		List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb, true);
		HashSet<pb_Face> source = new HashSet<pb_Face>(faces);
		HashSet<pb_Face> neighboring = new HashSet<pb_Face>();

		Vector3 srcNormal = Vector3.zero;
		bool checkAngle = maxAngleDiff > 0f;

		for(int i = 0; i < wings.Count; i++)
		{
			if(!source.Contains(wings[i].face))
				continue;

			if(checkAngle)
				srcNormal = pb_Math.Normal(pb, wings[i].face);

			foreach(pb_WingedEdge w in wings[i])
			{
				if(w.opposite != null && !source.Contains(w.opposite.face))
				{
					if(checkAngle)
					{
						Vector3 oppNormal = pb_Math.Normal(pb, w.opposite.face);

						if(Vector3.Angle(srcNormal, oppNormal) < maxAngleDiff)
							neighboring.Add(w.opposite.face);
					}
					else
					{
						neighboring.Add(w.opposite.face);
					}
				}
			}
		}

		return neighboring;
	}

	private static void Flood(pb_Object pb, pb_WingedEdge wing, Vector3 wingNrm, float maxAngle, HashSet<pb_Face> selection)
	{
		pb_WingedEdge next = wing.next;

		while(next != wing)
		{
			pb_WingedEdge opp = next.opposite;

			if(opp != null && !selection.Contains(opp.face))
			{
				
				Vector3 oppNormal = pb_Math.Normal(pb, opp.face);

				if(Vector3.Angle(wingNrm, oppNormal) < maxAngle)
				{
					selection.Add(opp.face);
					Flood(pb, opp, oppNormal, maxAngle, selection);
				}
			}

			next = next.next;
		}
	}

	private static HashSet<pb_Face> FloodSelection(pb_Object pb, IList<pb_Face> faces, float maxAngleDiff)
	{
		List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb, true);
		HashSet<pb_Face> source = new HashSet<pb_Face>(faces);
		HashSet<pb_Face> flood = new HashSet<pb_Face>();

		for(int i = 0; i < wings.Count; i++)
		{
			if(!flood.Contains(wings[i].face) && source.Contains(wings[i].face))
			{
				flood.Add(wings[i].face);
				Flood(pb, wings[i], pb_Math.Normal(pb, wings[i].face), maxAngleDiff, flood);
			}
		}
		return flood;
	}
}

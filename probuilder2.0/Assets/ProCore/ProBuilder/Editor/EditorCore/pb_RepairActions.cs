using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProBuilder.Core;
using ProBuilder.MeshOperations;
using UnityEditor;
using UnityEngine;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Common troubleshooting actions for repairing ProBuilder meshes.
	/// </summary>
	static class pb_RepairActions
	{
		/// <summary>
		/// Menu interface for manually re-generating all ProBuilder geometry in scene.
		/// </summary>
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Rebuild All ProBuilder Objects", false, pb_Constant.MENU_REPAIR)]
		public static void MenuForceSceneRefresh()
		{
			StringBuilder sb = new StringBuilder();
			pb_Object[] all = Object.FindObjectsOfType<pb_Object>();

			for (int i = 0, l = all.Length; i < l; i++)
			{
				EditorUtility.DisplayProgressBar(
					"Refreshing ProBuilder Objects",
					"Reshaping pb_Object " + all[i].id + ".",
					((float) i / all.Length));

				try
				{
					all[i].ToMesh();
					all[i].Refresh();
					all[i].Optimize();
				}
				catch(System.Exception e)
				{
					if(!ReProBuilderize(all[i]))
						sb.AppendLine("Failed rebuilding: " + all[i].ToString() + "\n\t" + e.ToString());
				}
			}

			if(sb.Length > 0)
				pb_Log.Error(sb.ToString());

			EditorUtility.ClearProgressBar();
			EditorUtility.DisplayDialog("Refresh ProBuilder Objects",
				"Successfully refreshed all ProBuilder objects in scene.",
				"Okay");
		}

		static bool ReProBuilderize(pb_Object pb)
		{
			try
			{
				GameObject go = pb.gameObject;
				pb.dontDestroyMeshOnDelete = true;
				Undo.DestroyObjectImmediate(pb);

				// don't delete pb_Entity here because it won't
				// actually get removed till the next frame, and
				// probuilderize wants to add it if it's missing
				// (which it looks like it is from c# side but
				// is not)

				pb = Undo.AddComponent<pb_Object>(go);
				pb_MeshOps.ResetPbObjectWithMeshFilter(pb, true);

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();

				return true;
			}
			catch
			{
				return false;
			}
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Rebuild Shared Indices Cache", true, pb_Constant.MENU_REPAIR)]
		static bool VertifyRebuildMeshes()
		{
			return pb_Util.GetComponents<pb_Object>(Selection.transforms).Length > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Rebuild Shared Indices Cache", false, pb_Constant.MENU_REPAIR)]
		public static void DoRebuildMeshes()
		{
			RebuildSharedIndices( pb_Util.GetComponents<pb_Object>(Selection.transforms) );
		}

		/// <summary>
		/// Rebuild targets if they can't be refreshed.
		/// </summary>
		/// <param name="targets"></param>
		static void RebuildSharedIndices(pb_Object[] targets)
		{
			StringBuilder sb = new StringBuilder();

			for(int i = 0; i < targets.Length; i++)
			{
				EditorUtility.DisplayProgressBar(
					"Refreshing ProBuilder Objects",
					"Reshaping pb_Object " + targets[i].id + ".",
					((float)i / targets.Length));

				pb_Object pb = targets[i];

				try
				{
					pb.SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(pb.vertices));

					pb.ToMesh();
					pb.Refresh();
					pb.Optimize();
				}
				catch(System.Exception e)
				{
					sb.AppendLine("Failed rebuilding " + pb.name + " shared indices cache.\n" + e.ToString());
				}
			}

			if(sb.Length > 0)
				pb_Log.Error(sb.ToString());

			EditorUtility.ClearProgressBar();
			EditorUtility.DisplayDialog("Rebuild Shared Index Cache", "Successfully rebuilt " + targets.Length + " shared index caches", "Okay");
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Remove Degenerate Triangles", false, pb_Constant.MENU_REPAIR)]
		public static void MenuRemoveDegenerateTriangles()
		{
			int count = 0;

			foreach(pb_Object pb in pb_Util.GetComponents<pb_Object>(Selection.transforms))
			{
				pb.ToMesh();

				int[] rm;
				pb.RemoveDegenerateTriangles(out rm);
				count += rm.Length;

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			pb_EditorUtility.ShowNotification("Removed " + (count/3) + " degenerate triangles.");
		}
	}
}
using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using System.Text;

public class pb_PrintDebugInfo : Editor
{
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Print Debug Information", false, pb_Constant.MENU_REPAIR + 200)]
	public static void MenuPrintDebugInfo()
	{
		StringBuilder sb = new StringBuilder();

		sb.AppendLine("Build Target: " + EditorUserBuildSettings.activeBuildTarget);
		sb.AppendLine("Pixel Light Count:  " + QualitySettings.pixelLightCount);

		sb.AppendLine("---");

		sb.AppendLine("ProBuilder: " + pb_Constant.pbVersion);

		foreach(pb_Object pb in FindObjectsOfType(typeof(pb_Object)))
		{
			sb.AppendLine(pb.ToStringDetailed() + "\n");
		}

		Debug.Log(sb.ToString());
	}
}

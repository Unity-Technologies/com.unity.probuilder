// A set of UI methods used by pb_Editor
using UnityEditor;
using UnityEngine;

public static class pb_Editor_GUI
{
	/**
	 * \brief Draws dimensions of passed pb_Object in screen space.  Must be called from OnSceneGUI
	 */
	public static void DrawDimensions(pb_Object pb)
	{
		Rect r = pb_Editor_Utility.GUIRectWithObject(pb.gameObject);
		Rect info = new Rect(r.x+r.width, r.y, 400, 300);

		Handles.BeginGUI();
			// Handles.DrawLine( new Vector2(r.x, r.y), new Vector2(r.x+r.width, r.y) );
			// Handles.DrawLine( new Vector2(r.x+r.width, r.y), new Vector2(r.x+r.width, r.y+r.height) );
			GUI.Label(info, "Size: " + pb.gameObject.GetComponent<Renderer>().bounds.size);
		Handles.EndGUI();
	}
}
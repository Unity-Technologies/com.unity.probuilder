using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

public class pb_VertexColor_Editor : EditorWindow
{
#region Initialization

	/**
	 * Public call to init.
	 */
	public static void Init()
	{
		EditorWindow.GetWindow<pb_VertexColor_Editor>(true, "ProBuilder Vertex Painter", true).Show();
	}

 	void OnEnable()
	{
		pb_Lightmapping.PushGIWorkflowMode();

		if(SceneView.onSceneGUIDelegate != this.OnSceneGUI)
		{
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
		}

		if(editor)
			editor.SetEditLevel(EditLevel.Plugin);

		colorName = pb_ColorUtil.GetColorName(color);
	}

	void OnDisable()
	{
		pb_Lightmapping.PopGIWorkflowMode();

		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;

		foreach(pb_Object pb in modified)
		{
			pb.ToMesh();
			pb.Refresh();
			pb.GenerateUV2();
		}

		if(editor)
			editor.PopEditLevel();
	}
#endregion

#region Members

	public pb_Editor editor { get { return pb_Editor.instance; } }		///< Convenience getter for pb_Editor.instance

	static readonly Color OuterRingColor = new Color(1f, 1f, 1f, .3f);
	static readonly Color MiddleRingColor = new Color(1f, 1f, 1f, .5f);
	static readonly Color InnerRingColor = new Color(1f, 1f, 1f, .7f);

	Color color = Color.green;											///< The color currently being painted.
	string colorName = "Green";											///< Human readable color name.
	bool enabled = true;												///< Is the tool enabled?
	float brushSize = .5f;												///< The brush size to use.

	Event currentEvent;													///< Cache the current event at start of OnSceneGUI.
	Camera sceneCamera;													///< Cache the sceneview camera at start of OnSceneGUI.
 
	///< Used to store changes to mesh color array for live preview.
	Dictionary<pb_Object, Color[]> hovering = new Dictionary<pb_Object, Color[]>();
 
	Vector2 mpos = Vector2.zero;
	pb_Object pb;										// The object currently gettin' paintered
	bool mouseMoveEvent = false;
	Vector3 handlePosition = Vector3.zero;
	Quaternion handleRotation = Quaternion.identity;
	float handleDistance = 10f;
	bool lockhandleToCenter = false;
	Vector2 screenCenter = Vector2.zero;

	HashSet<pb_Object> modified = new HashSet<pb_Object>();	// list of all objects that have been modified by the painter, stored so that we can regenerate UV2 on disable
#endregion
 
#region OnGUI

	void OnGUI()
	{
		lockhandleToCenter = EditorWindow.focusedWindow == this;

		enabled = EditorGUILayout.Toggle("Enabled", enabled);
 
 		EditorGUI.BeginChangeCheck();
		color = EditorGUILayout.ColorField("Color", color);
		if(EditorGUI.EndChangeCheck())
		{
			colorName = pb_ColorUtil.GetColorName(color);
		}

		GUILayout.Label(colorName);

		EditorGUI.BeginChangeCheck();
			brushSize = Mathf.Max(.01f, EditorGUILayout.FloatField("Brush Size", brushSize));
		if(EditorGUI.EndChangeCheck())
			SceneView.RepaintAll();
	}
#endregion

#region OnSceneGUI

	void OnSceneGUI(SceneView scnview)
	{
		if(!enabled || (EditorWindow.focusedWindow != scnview && !lockhandleToCenter))
			return;
 
		if(editor && editor.editLevel != EditLevel.Plugin)
			editor.SetEditLevel(EditLevel.Plugin);
 
#if UNITY_5
		if( Lightmapping.giWorkflowMode == Lightmapping.GIWorkflowMode.Iterative )
		{
			pb_Lightmapping.PushGIWorkflowMode();
			Lightmapping.Cancel();
			Debug.LogWarning("Vertex Painter requires Continuous Baking to be Off.  When you close the Vertex Painter tool, Continuous Baking will returned to it's previous state automatically.\nIf you toggle Continuous Baking On while the Vertex Painter is open, you may lose all mesh vertex colors.");
		}
#endif

		currentEvent = Event.current;
		sceneCamera = scnview.camera;

		screenCenter.x = Screen.width/2f;
		screenCenter.y = Screen.height/2f;

		mouseMoveEvent = currentEvent.type == EventType.MouseMove;
		
		if( mouseMoveEvent )
		{
			GameObject go = HandleUtility.PickGameObject(Event.current.mousePosition, false);

			if( go != null && (pb == null || go != pb.gameObject) )
			{
				pb = go.GetComponent<pb_Object>();

				if(pb != null)
				{
					modified.Add(pb);
					
					pb.ToMesh();
					pb.Refresh();
				}
			}
		}

		/**
		*    Draw the handles
		*/
		if(!lockhandleToCenter && !pb_Handle_Utility.SceneViewInUse(currentEvent))
		{
			if(pb != null)
			{
				if(!hovering.ContainsKey(pb))
				{
					hovering.Add(pb, pb.msh.colors ?? new Color[pb.vertexCount]);
				}
				else
				{
					pb.msh.colors = hovering[pb];
				}
 
				Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
				RaycastHit hit;
 
				if ( pb_Handle_Utility.Raycast(ray, pb, out hit) )
				{
					handlePosition = hit.point;
					handleDistance = Vector3.Distance(handlePosition, sceneCamera.transform.position);
					handleRotation = Quaternion.LookRotation(hit.normal, Vector3.up);
 
 					Transform t = pb.transform;
					Color[] colors = pb.msh.colors;

					int[][] sharedIndices = pb.sharedIndices.ToArray();
					for(int i = 0; i < sharedIndices.Length; i++)
					{
						if( Vector3.Distance(hit.point, t.TransformPoint(pb.vertices[sharedIndices[i][0]])) < brushSize)
						{
							for(int n = 0; n < sharedIndices[i].Length; n++)
								colors[sharedIndices[i][n]] = (Color)color;
						}
					}
 
					pb.msh.colors = colors;
				}
				else
				{
					// Clear
					foreach(KeyValuePair<pb_Object, Color[]> kvp in hovering)
						kvp.Key.msh.colors = kvp.Value;
 
					hovering.Clear();

					ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
					handleRotation = Quaternion.LookRotation(sceneCamera.transform.forward, Vector3.up);
					handlePosition = ray.origin + ray.direction * handleDistance;
				}
			}
		}
		else
		{
			// No longer focusing object
			foreach(KeyValuePair<pb_Object, Color[]> kvp in hovering)
			{
				kvp.Key.msh.colors = kvp.Value;
			}
 
			hovering.Clear();
 	
			Ray ray = HandleUtility.GUIPointToWorldRay(lockhandleToCenter ? screenCenter : currentEvent.mousePosition);
			handleRotation = Quaternion.LookRotation(sceneCamera.transform.forward, Vector3.up);
			handlePosition = ray.origin + ray.direction * handleDistance;
 		}

 		Handles.color = InnerRingColor;
			Handles.CircleCap(0, handlePosition, handleRotation, brushSize * .2f);
 		Handles.color = MiddleRingColor;
			Handles.CircleCap(0, handlePosition, handleRotation, brushSize * .5f);
 		Handles.color = OuterRingColor;		
			Handles.CircleCap(0, handlePosition, handleRotation, brushSize);
 		Handles.color = Color.white;
 
		// This prevents us from selecting other objects in the scene,
		// and allows for the selection of faces / vertices.
		int controlID = GUIUtility.GetControlID(FocusType.Passive);
		HandleUtility.AddDefaultControl(controlID);
 
		if( (currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag) )
		{
			Dictionary<pb_Object, Color[]> sticky = new Dictionary<pb_Object, Color[]>();
 
 			// Apply colors
			foreach(KeyValuePair<pb_Object, Color[]> kvp in hovering)
			{
				Color[] colors = kvp.Key.msh.colors;

				sticky.Add(kvp.Key, colors);
 
				kvp.Key.msh.colors = kvp.Value;

				pbUndo.RecordObjects(new Object[] {kvp.Key, kvp.Key.msh}, "Apply Vertex Colors");

				kvp.Key.SetColors(colors);

				kvp.Key.msh.colors = colors;
			}
 
			hovering = sticky;
		}
 
		if(mpos != currentEvent.mousePosition && currentEvent.type == EventType.Repaint)
		{
			mpos = currentEvent.mousePosition;
			SceneView.RepaintAll();
		}
	}
#endregion
}
 
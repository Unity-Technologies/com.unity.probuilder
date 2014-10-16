using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

public class pb_VertexPainter : EditorWindow
{
#region Initialization

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Tools/Vertex Painter Tool")]
	public static void MenuOpenVertexPainterWindow()
	{
		EditorWindow.GetWindow<pb_VertexPainter>(true, "ProBuilder Vertex Painter", true).Show();
	}
 

 	void OnEnable()
	{
		if(SceneView.onSceneGUIDelegate != this.OnSceneGUI)
		{
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
		}

		if(editor)
			editor.SetEditLevel(EditLevel.Plugin);
	}

	void OnDisable()
	{
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;

		if(editor)
			editor.PopEditLevel();
	}
#endregion

#region Members

	public pb_Editor editor { get { return pb_Editor.instance; } }		///< Convenience getter for pb_Editor.instance

	Color color = Color.green;											///< The color currently being painted.
	bool enabled = true;												///< Is the tool enabled?
	float brushSize = 1f;												///< The brush size to use.

	Event currentEvent;													///< Cache the current event at start of OnSceneGUI.
	Camera sceneCamera;													///< Cache the sceneview camera at start of OnSceneGUI.
 
	///< Used to store changes to mesh color array for live preview.
	Dictionary<pb_Object, Color32[]> hovering = new Dictionary<pb_Object, Color32[]>();
 
	Vector2 mpos = Vector2.zero;
	GameObject go;
	bool mouseMoveEvent = false;
	Vector3 handlePosition = Vector3.zero;
	Quaternion handleRotation = Quaternion.identity;
	float handleDistance = 10f;
	bool lockhandleToCenter = false;
	Vector2 screenCenter = Vector2.zero;
#endregion
 
#region OnGUI

	void OnGUI()
	{
		lockhandleToCenter = EditorWindow.focusedWindow == this;

		enabled = EditorGUILayout.Toggle("Enabled", enabled);
 
		color = EditorGUILayout.ColorField("Color", color);

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
 
		currentEvent = Event.current;
		sceneCamera = scnview.camera;

		screenCenter.x = Screen.width/2f;
		screenCenter.y = Screen.height/2f;

		mouseMoveEvent = currentEvent.type == EventType.MouseMove;
		
		if( mouseMoveEvent )
			go = HandleUtility.PickGameObject(Event.current.mousePosition, false);

		/**
		*    Draw the handles
		*/
		if(go != null && !lockhandleToCenter && !pb_Handle_Utility.SceneViewInUse(currentEvent))
		{
			pb_Object pb = go.GetComponent<pb_Object>();
 
			if(pb != null)
			{
				if(!hovering.ContainsKey(pb))
					hovering.Add(pb, pb.msh.colors32 ?? new Color32[pb.vertexCount]);
				else
					pb.msh.colors32 = hovering[pb];
 
				Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
				RaycastHit hit;
 
				if ( pb_Handle_Utility.Raycast(ray, pb, out hit) )
				{
					handlePosition = hit.point;
					handleDistance = Vector3.Distance(handlePosition, sceneCamera.transform.position);
					handleRotation = Quaternion.LookRotation(hit.normal, Vector3.up);
 
 					Transform t = pb.transform;
					Color32[] colors = pb.msh.colors32;

					int[][] sharedIndices = pb.sharedIndices.ToArray();
					for(int i = 0; i < sharedIndices.Length; i++)
					{
						if( Vector3.Distance(hit.point, t.TransformPoint(pb.vertices[sharedIndices[i][0]])) < brushSize)
						{
							for(int n = 0; n < sharedIndices[i].Length; n++)
								colors[sharedIndices[i][n]] = (Color32)color;
						}
					}
 
					pb.msh.colors32 = colors;
				}
				else
				{
					// Clear
					foreach(KeyValuePair<pb_Object, Color32[]> kvp in hovering)
						kvp.Key.msh.colors32 = kvp.Value;
 
					hovering.Clear();

					ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
					handleRotation = Quaternion.LookRotation(sceneCamera.transform.forward, Vector3.up);
					handlePosition = ray.origin + ray.direction * handleDistance;
				}
			}
		}
		else
		{
			foreach(KeyValuePair<pb_Object, Color32[]> kvp in hovering)
				kvp.Key.msh.colors32 = kvp.Value;
 
			hovering.Clear();
 	
			Ray ray = HandleUtility.GUIPointToWorldRay(lockhandleToCenter ? screenCenter : currentEvent.mousePosition);
			handleRotation = Quaternion.LookRotation(sceneCamera.transform.forward, Vector3.up);
			handlePosition = ray.origin + ray.direction * handleDistance;
 		}

		Handles.CircleCap(0, handlePosition, handleRotation, brushSize);
 
		// This prevents us from selecting other objects in the scene,
		// and allows for the selection of faces / vertices.
		int controlID = GUIUtility.GetControlID(FocusType.Passive);
		HandleUtility.AddDefaultControl(controlID);
 
		if( (currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag) )
		{
			Dictionary<pb_Object, Color32[]> sticky = new Dictionary<pb_Object, Color32[]>();
 
			foreach(KeyValuePair<pb_Object, Color32[]> kvp in hovering)
			{
				Color32[] colors = kvp.Key.msh.colors32;

				sticky.Add(kvp.Key, colors);
 
				kvp.Key.msh.colors32 = kvp.Value;

				pbUndo.RecordObjects(new Object[] {kvp.Key, kvp.Key.msh}, "Apply Vertex Colors");

				// This is terrible, and is currently being re-written -
				// the whole vertex color API is overhauled for next release!
				foreach(pb_Face face in kvp.Key.faces)
				{
					face.SetColors( pbUtil.ValuesWithIndices(colors, face.indices) );
				}

				kvp.Key.msh.colors32 = colors;
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
 
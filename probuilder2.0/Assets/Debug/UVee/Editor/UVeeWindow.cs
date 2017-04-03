#pragma warning disable 0618

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parabox.UVee
{
public class UVeeWindow : EditorWindow {

#region ENUM / CLASS

	public enum UVChannel 
	{
		UV,
		UV2,
#if UNITY_5
		UV3,
		UV4
#endif
	}

	/**
	 * Generic MeshFilter / SkinnedMeshRenderer type, implemented because
	 * we need to access similar properties in both, and can't treat them
	 * both as the same type.
	 */
	public class MeshSelection
	{
		public MeshSelection(MeshFilter mf)
		{
			meshFilter = mf;
			skinnedMeshRenderer = null;
		}

		public MeshSelection(SkinnedMeshRenderer smr)
		{
			meshFilter = null;
			skinnedMeshRenderer = smr;
		}

		public MeshSelection(Transform t)
		{
			if(t.GetComponent<MeshFilter>())
			{
				meshFilter = t.GetComponent<MeshFilter>();
			}
			else if (t.GetComponent<SkinnedMeshRenderer>())
			{
				skinnedMeshRenderer = t.GetComponent<SkinnedMeshRenderer>();
			}
			else
			{
				meshFilter = null;
				skinnedMeshRenderer = null;
			}
		}

		MeshFilter meshFilter;
		SkinnedMeshRenderer skinnedMeshRenderer;

		public GameObject gameObject
		{
			get
			{
				return meshFilter != null ? meshFilter.gameObject : (skinnedMeshRenderer != null ? skinnedMeshRenderer.gameObject : null);
			}
		}

		public Renderer renderer
		{
			get
			{
				return meshFilter != null ? (Renderer)meshFilter.GetComponent<MeshRenderer>() : (Renderer)skinnedMeshRenderer;
			}
		}

		public Mesh sharedMesh
		{
			get
			{
				return meshFilter != null ? meshFilter.sharedMesh : (skinnedMeshRenderer == null ? null : skinnedMeshRenderer.sharedMesh);
			}

			set
			{
				if(meshFilter != null)
					meshFilter.sharedMesh = value;
				else if(skinnedMeshRenderer != null)
					skinnedMeshRenderer.sharedMesh = value;
				else
					UnityEngine.Debug.LogError("MeshSelection is null");
			}
		}

		public Object rawObject
		{
			get
			{
				return (Object)meshFilter ?? (Object)skinnedMeshRenderer;
			}
		}
	}
#endregion

#region SETTINGS

	bool showPreferences = true;
	UVChannel uvChannel = UVChannel.UV;
	bool showCoordinates = false;
	bool showTex = true;
	bool drawTriangles = true;

	// bool maintainSpacing = true;
	// bool drawNonSelectedPoints = true;
#endregion

#region DATA

	MeshSelection[] selection = new MeshSelection[0];
	int submesh = 0;
	HashSet<int>[] selected_triangles = new HashSet<int>[0];
	Texture tex;

	// GUI draw caches
	Vector2[][] 	uv_points 		= new Vector2[0][];			// all uv points
	Vector2[][] 	user_points 	= new Vector2[0][];			// UV pooints in GUI space
	Vector2[][]		triangle_points = new Vector2[0][];			// wound in twos - so a triangle is { p0, p1, p1, p1, p2, p0 }
	Vector2[][]		user_triangle_points = new Vector2[0][];	// contains only selected triangles
	Vector2 		uv_center 		= Vector2.zero;				// GUI point

	// selection caches
	int[][]			distinct_triangle_selection = new int[0][];	///< Guarantees that only one index per vertex is present
	bool[] 			validChannel	= new bool[0]{};
#endregion

#region CONSTANT

	const int MIN_ZOOM = 1;
	const int MAX_ZOOM = 2000;

	const int LINE_WIDTH = 1;
	Color LINE_COLOR;

	const int UV_DOT_SIZE = 4;

	const int TOOL_SIZE = 128;

	Color[] COLOR_ARRAY = new Color[5];

	Color DRAG_BOX_COLOR_BASIC = new Color(0f, .7f, 1f, .2f);
	Color DRAG_BOX_COLOR_PRO = new Color(0f, .7f, 1f, 1f);
	Color DRAG_BOX_COLOR;

	Color TRIANGLE_COLOR_BASIC = new Color(.2f, .2f, .2f, .2f);
	Color TRIANGLE_COLOR_PRO = new Color(1f, 1f, 1f, .5f);
	Color TRIANGLE_LINE_COLOR;

	const float ALT_SCROLL_MODIFIER = .07f;
	const float SCROLL_MODIFIER = .5f;
#endregion

#region GUI MEMBERS

	Texture2D dot;

	Vector2 center = new Vector2(0f, 0f);			// actual center
	Vector2 workspace_origin = new Vector2(0f, 0f);	// where to start drawing in GUI space
	Vector2 workspace = new Vector2(0f, 0f);		// max allowed size, padding inclusive
	int max = 0;

	int pad = 10;
	int workspace_scale = 100;

	float scale = 0f;

	const int SETTINGS_BOX_EXPANDED = 114;
	const int SETTINGS_BOX_COMPACT = 50;
	private int settingsBoxHeight;

	int settingsBoxPad = 10;
	int PREFERENCES_MAX_WIDTH = 0;
	Rect settingsBoxRect = new Rect();
	
	Vector2 offset = new Vector2(0f, 0f);
	Vector2 start = new Vector2(0f, 0f);
	bool dragging = false;

	bool scrolling = false;

	// Vector2 setPosition = Vector2.zero;
	// float setRotation = 90f;
#endregion

#region UV MODIFICATION MEMBERS

	const int UVTOOL_LENGTH = 4;
	public enum UVTool {
		None,
		Move,
		Rotate,
		Scale
	}
	private UVTool tool = UVTool.None;
#endregion

#region INITIALIZATION
	
	[MenuItem("Window/UVee Window")]
	public static void Init()
	{
		EditorWindow.GetWindow(typeof(UVeeWindow), true, "UVee Viewer", true).autoRepaintOnSceneChange = true;
	}

	public void OnEnable()
	{	
		LINE_COLOR = EditorGUIUtility.isProSkin ? Color.gray : Color.gray;
		DRAG_BOX_COLOR = EditorGUIUtility.isProSkin ? DRAG_BOX_COLOR_PRO : DRAG_BOX_COLOR_BASIC;
		TRIANGLE_LINE_COLOR = EditorGUIUtility.isProSkin ? TRIANGLE_COLOR_PRO : TRIANGLE_COLOR_BASIC;
		
		dot = EditorGUIUtility.whiteTexture;// (Texture2D)Resources.Load("dot", typeof(Texture2D));

		PopulateColorArray();
		OnSelectionChange();

		Undo.undoRedoPerformed += this.UndoRedoPerformed;

		Repaint();
	}
	
	public void OnDisable()
	{
		if(SceneView.onSceneGUIDelegate == this.OnSceneGUI)
		{
			ClearAll();
			SceneView.RepaintAll();

			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		}
	}

	public void HookSceneView()
	{
		if(SceneView.onSceneGUIDelegate != this.OnSceneGUI)
		{
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
		}
	}
#endregion

#region EVENT

	public bool UndoRedoPerformedEvent { get { return Event.current.type == EventType.ValidateCommand; } }// && Event.current.commandName == "UndoRedoPerformed"; } }
	void UndoRedoPerformed()
	{
		Repaint();
	}

	int screenWidth = 0;
	int screenHeight = 0;
	public void OnWindowResize()
	{
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		UpdateGUIPointCache();
		Repaint();
	}

	public void OnFocus()
	{
		OnSelectionChange();
	}
	
	public void OnHierarchyChange()
	{
		if(selection.Length != Selection.transforms.Length)
			OnSelectionChange();
	}

	public void OnSelectionChange()
	{
		List<Transform> validMeshTransforms = Selection.transforms.Where(x => x.GetComponent<MeshFilter>() || x.GetComponent<SkinnedMeshRenderer>()).ToList();
		selection = new MeshSelection[validMeshTransforms.Count];
		for(int i = 0; i < validMeshTransforms.Count; i++)
		{
			selection[i] = new MeshSelection(validMeshTransforms[i]);
		}

		// I'm not sure why this is necessary, as the sharedMesh is never directly modified.
		for(int i = 0; i < selection.Length; i++)
		{
			if(!selection[i].sharedMesh.isReadable)
				selection[i].sharedMesh.SetIsReadable(true);
		}

		selected_triangles = new HashSet<int>[selection.Length];

		for(int i = 0; i < selection.Length; i++)
			selected_triangles[i] = new HashSet<int>();

		if(selection != null && selection.Length > 0)
		{

			if(selection[0].renderer != null && selection[0].renderer.sharedMaterial != null)
			{
				Object t = selection[0].renderer.sharedMaterial.mainTexture;
				if( t is Texture2D)
					tex = (Texture2D)t;
			}
		}
		
		UpdateGUIPointCache();
		Repaint();
	}

	private void UpdateSelectionWithGUIRect(Rect rect, bool shift)
	{
		bool pointSelected = false;
		// avoid if checks if shift isn't held - (this loop is already slow, so take speed improvements where we can)
		if(!shift)
		{
			for(int i = 0; i < selection.Length; i++)
			{
				if(!validChannel[i])
					continue;

				selected_triangles[i].Clear();
				int[] tris = selection[i].sharedMesh.triangles;
				for(int n = 0; n < tris.Length; n++)
				{
					if(rect.Contains(uv_points[i][tris[n]]))
					{
						pointSelected = true;
						selected_triangles[i].Add( tris[n] );
					}
				}
			}
		}
		else
		{
			for(int i = 0; i < selection.Length; i++)
			{
				if(!validChannel[i])
					continue;
				
				int[] tris = selection[i].sharedMesh.triangles;

				for(int n = 0; n < tris.Length; n++)
				{
					if(rect.Contains(uv_points[i][tris[n]]))
					{
						pointSelected = true;

						if(distinct_triangle_selection[i].Contains(tris[n]))
							selected_triangles[i].Remove(tris[n]);
						else
							selected_triangles[i].Add( tris[n] );
					}
				}
			}
		}

		if(!pointSelected && !shift)
			OnSelectionChange();

		UpdateGUIPointCache();
	}

	// Call after UVs are selected, or the GUI space has been modified
	private void UpdateGUIPointCache()
	{	
		// LogStart("UpdateGUIPointCache");

		uv_points 					= new Vector2[selection.Length][];
		user_points 				= new Vector2[selection.Length][];
		triangle_points 			= new Vector2[selection.Length][];
		user_triangle_points 		= new Vector2[selection.Length][];
		distinct_triangle_selection = new int 	 [selection.Length][];
		validChannel				= new bool   [selection.Length];

		for(int i = 0; i < selection.Length; i++)
		{
			uv_points[i] = UVToGUIPoint( GetUVChannel(selection[i].sharedMesh, uvChannel) );
			if(uv_points[i] == null || uv_points[i].Length < 1)
			{
				user_points[i]					= new Vector2[0]{};
				triangle_points[i]				= new Vector2[0]{};
				user_triangle_points[i] 		= new Vector2[0]{};
				distinct_triangle_selection[i] 	= new int[0]{};
				validChannel[i] = false;
				continue;
			}

			distinct_triangle_selection[i] = selected_triangles[i].Distinct().ToArray();
			user_points[i] = UVToGUIPoint(UVArrayWithTriangles(selection[i], distinct_triangle_selection[i]));
			validChannel[i] = true;

			int[] tris = (submesh == 0) ? selection[i].sharedMesh.triangles : selection[i].sharedMesh.GetTriangles(submesh-1);
			List<Vector2> lines = new List<Vector2>();
			List<Vector2> u_lines = new List<Vector2>();
			for(int n = 0; n < tris.Length; n+=3)
			{
				Vector2 p0 = uv_points[i][tris[n+0]];
				Vector2 p1 = uv_points[i][tris[n+1]];
				Vector2 p2 = uv_points[i][tris[n+2]];

				// HashSet.Contains() is about a gazillion times faster than List.Contains()
				bool p0_s = selected_triangles[i].Contains(tris[n+0]);
				bool p1_s = selected_triangles[i].Contains(tris[n+1]);
				bool p2_s = selected_triangles[i].Contains(tris[n+2]);

				if(p0_s && p1_s) { u_lines.Add(p0); u_lines.Add(p1); }
				if(p1_s && p2_s) { u_lines.Add(p1); u_lines.Add(p2); }
				if(p0_s && p2_s) { u_lines.Add(p2); u_lines.Add(p0); }

				lines.AddRange(new Vector2[6] {p0, p1, p1, p2, p2, p0});
			}

			triangle_points[i] = lines.ToArray();
			user_triangle_points[i] = u_lines.ToArray();
			// UnityEngine.Debug.Log(distinct_triangle_selection[i].ToFormattedString(", ") + "\n" + selected_triangles[i].ToArray().ToFormattedString(", "));
		}

		uv_center = Average(user_points);

		SceneView.RepaintAll();

		// LogFinish("UpdateGUIPointCache");
	}

 	Vector2[] GetUVChannel(Mesh m, UVChannel channel)
 	{
 		switch(channel)
 		{		
 			case UVChannel.UV2:
 				return m.uv2;

#if UNITY_5
 			case UVChannel.UV3:
 				return m.uv3;

 			case UVChannel.UV4:
 				return m.uv4;
#endif

 			default:
 				return m.uv;
 		}
 	}

 	void SetUVChannel(Mesh m, UVChannel channel, Vector2[] uvs)
 	{
 		switch(channel)
 		{		
 			case UVChannel.UV2:
 				m.uv2 = uvs;
 				break;

#if UNITY_5
 			case UVChannel.UV3:
 				m.uv3 = uvs;
 				break;

 			case UVChannel.UV4:
 				m.uv4 = uvs;
 				break;
#endif

 			default:
 				m.uv = uvs;
 				break;
 		}
 	}

	private void ClearAll()
	{
		uv_points 					= new Vector2[0][];
		user_points 				= new Vector2[0][];
		triangle_points 			= new Vector2[0][];
		user_triangle_points 		= new Vector2[0][];
		distinct_triangle_selection = new int 	 [0][];
		validChannel				= new bool   [0];
	}
#endregion

#region GUI

	Vector2 drag_start; 
	bool mouseDragging = false;
	bool zoom_dragging = false;
	Vector2 zoom_dragging_start = Vector2.zero;
	bool needsRepaint = false;

	void CopyUVsToChannel(object channel)
	{
		for(int i = 0; i < selection.Length; i++)
		{	
			Vector2[] uvs = GetUVChannel(selection[i].sharedMesh, uvChannel);
			// unity bug requires we copy mesh data like this
			// Vector3[] v = selection[i].sharedMesh.vertices;
			// Vector3[] n = selection[i].sharedMesh.normals;
			int[][] t = new int[selection[i].sharedMesh.subMeshCount][];
			for(int iter = 0; iter < t.Length; iter++)
				t[i] = selection[i].sharedMesh.GetIndices(iter);

			SetUVChannel(selection[i].sharedMesh, (UVChannel)channel, uvs);
			// selection[i].sharedMesh.vertices = v;
			// selection[i].sharedMesh.normals = n;
			for(int iter = 0; iter < t.Length; iter++)
				selection[i].sharedMesh.SetIndices(t[iter], selection[i].sharedMesh.GetTopology(iter), iter);
		}
	}

	void OnGUI()
	{
		if(Screen.width != screenWidth || Screen.height != screenHeight)
			OnWindowResize();

		//** Handle events **//
		Event e = Event.current;

		if(e.type == EventType.ContextClick)
		{
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Copy UVs To Channel/" + UVChannel.UV, "Copies the currently displayed UV channel to another channel."), false, CopyUVsToChannel, UVChannel.UV);
			menu.AddItem(new GUIContent("Copy UVs To Channel/" + UVChannel.UV2, "Copies the currently displayed UV channel to another channel."), false, CopyUVsToChannel, UVChannel.UV2);
#if UNITY_5
			menu.AddItem(new GUIContent("Copy UVs To Channel/" + UVChannel.UV3, "Copies the currently displayed UV channel to another channel."), false, CopyUVsToChannel, UVChannel.UV3);
			menu.AddItem(new GUIContent("Copy UVs To Channel/" + UVChannel.UV4, "Copies the currently displayed UV channel to another channel."), false, CopyUVsToChannel, UVChannel.UV4);
#endif
			menu.ShowAsContext();
		}

		// shortcut listining
		if(e.isKey && e.type == EventType.KeyUp)
		{
			needsRepaint = true;

			switch(e.keyCode)
			{
				case KeyCode.W:
					tool = UVTool.Move;
					break;
				case KeyCode.E:
					tool = UVTool.Rotate;
					break;
				case KeyCode.R:
					tool = UVTool.Scale;
					break;
				case KeyCode.Q:
					tool = UVTool.None;
					break;
			
				default:	
					needsRepaint = false;
					break;
			}

			if(needsRepaint)	
				e.Use();
		}

		// drag selection
		if( UVee_HandleUtility.CurrentID < 0 )
		{
			if(e.isMouse && !settingsBoxRect.Contains(e.mousePosition) )
			{
				if(e.button == 2 || (e.modifiers == EventModifiers.Alt && e.button == 0))
				{
					if(e.type == EventType.MouseDown) {
						start = e.mousePosition;
						dragging = true;
					}

					// pan canvas
					if(dragging) {
						offset = offset + (e.mousePosition - start);
						start = e.mousePosition;
					}

					if(e.type == EventType.MouseUp || e.type == EventType.Ignore) {
						dragging = false;
					}
				}

				// alt right click and drag == zoom
				if(e.button == 1 && e.modifiers == EventModifiers.Alt)
				{
					if(e.type == EventType.MouseDown) {
						zoom_dragging = true;
						zoom_dragging_start = e.mousePosition;
					}

					if(zoom_dragging) {
						float modifier = -1f;
						Vector2 delta = zoom_dragging_start - e.mousePosition;
						zoom_dragging_start = e.mousePosition;
						workspace_scale = (int)Mathf.Clamp(workspace_scale + ( (delta.x - delta.y) * modifier), MIN_ZOOM, MAX_ZOOM);
					}

					if( (e.type == EventType.MouseUp || e.type == EventType.Ignore) && zoom_dragging )
					{
						zoom_dragging = false;
					}
				}

				// USER INPUT THAT CAN BE DRAWN
				if(e.type == EventType.MouseDown && e.button == 0 && e.modifiers != EventModifiers.Alt) {
					drag_start = e.mousePosition;
					mouseDragging = true;
				}
			}
			
			if(e.type == EventType.MouseUp && e.button == 0 && mouseDragging) {
				mouseDragging = false;
				UpdateSelectionWithGUIRect(GUIRectWithPoints(drag_start, e.mousePosition), e.modifiers == EventModifiers.Shift);
			}

			// workspace scale
			if(e.type == EventType.ScrollWheel)
			{
				float modifier = -1f;
				offset += new Vector2(e.delta.y, e.delta.y);
				workspace_scale = (int)Mathf.Clamp(workspace_scale + (e.delta.y * modifier), MIN_ZOOM, MAX_ZOOM);
				scrolling = true;
			}

			if(e.isKey && e.keyCode == KeyCode.Alpha0) {
				offset = Vector2.zero;
				workspace_scale = 100;
				needsRepaint = true;
				UpdateGUIPointCache();
			}
		}
		else
		{
			mouseDragging = false;
		}

		/**
		 * Draw Background items
		 */
		DrawGraphBase();

		/**
		 * UV Tools (position, rotate, scale)
		 */
		switch(tool)
		{
			case UVTool.Move:
				MoveTool();
				break;

			case UVTool.Rotate:
				RotateTool();
				break;

			case UVTool.Scale:
				ScaleTool();
				break;
		
			default:
			case UVTool.None:
				break;
		}

		/**
		 * Always catch MouseUps
		 */
		if(e.type == EventType.MouseUp || e.type == EventType.Ignore)
		{
			movingUVs = false;
			
			uv_scale = Vector2.one;
			uv_rotation = 0f;

			UpdateGUIPointCache();
			needsRepaint = true;
		}

		if(drawTriangles)
		{
			for(int i = 0; i < selected_triangles.Length; i++)
				DrawLines(triangle_points[i], TRIANGLE_LINE_COLOR);

			for(int i = 0; i < selected_triangles.Length; i++)
				DrawLines(user_triangle_points[i], COLOR_ARRAY[i%COLOR_ARRAY.Length]);
		}

		for(int i = 0; i < selection.Length; i++)
			DrawPoints( user_points[i] );//, COLOR_ARRAY[i%COLOR_ARRAY.Length]);

		//** Draw Preferences Pane **//
		DrawPreferencesPane();

		if(mouseDragging) {
			if(Vector2.Distance(drag_start, e.mousePosition) > 10)
				DrawBox(drag_start, e.mousePosition, DRAG_BOX_COLOR);
			needsRepaint = true;
		}

		if(dragging || zoom_dragging) {
			needsRepaint = true;
		}

		if(scrolling) {
			scrolling = false;
			needsRepaint = true;
		}

		if(UndoRedoPerformedEvent) {
			needsRepaint = true;
		}

		if(needsRepaint)
		{
			UpdateGUIPointCache();
			
			Repaint();
			needsRepaint = false;
		}
	}
#endregion

#region ONSCENEGUI

	public void OnSceneGUI(SceneView sceneView)
	{
		if(Selection.transforms.Length < 1 || selection.Length != selected_triangles.Length) return;

		for(int i = 0; i < selected_triangles.Length; i++)
		{
			Vector3[] v = TransformExtensions.VerticesInWorldSpace(selection[i]);
			int[] tris 	= selected_triangles[i].ToArray();

			Handles.color = COLOR_ARRAY[i%COLOR_ARRAY.Length];
			for(int n = 0; n < tris.Length; n++)
			{
				Handles.DotCap(0,
					v[tris[n]],
					Quaternion.identity,
					HandleUtility.GetHandleSize(v[tris[n]]) * .05f);
			}
			Handles.color = Color.white;
		}
	}
#endregion

#region DRAWING

	public void DrawGraphBase()
	{
		max = (Screen.width > Screen.height-settingsBoxHeight) ? (Screen.height-settingsBoxHeight) - (pad*2) : Screen.width - (pad*2);
		workspace = new Vector2(max, max);
		workspace *= (workspace_scale/100f);

		scale = workspace.x/2f;

		center = new Vector2(Screen.width / 2, (Screen.height + settingsBoxHeight) / 2 );

		center += offset;

		workspace_origin = new Vector2(center.x-workspace.x/2, center.y-workspace.y/2);

		// Draw the background gray workspace
		GUI.Box(new Rect(workspace_origin.x, workspace_origin.y, workspace.x, workspace.y), "");

		// Draw texture (if it exists)
		if(showTex && tex)
			GUI.DrawTexture(new Rect(center.x, center.y-workspace.y/2, workspace.x/2, workspace.y/2), tex, ScaleMode.ScaleToFit);

		GUI.color = LINE_COLOR;
		// Draw vertical line
		GUI.DrawTexture(new Rect( center.x, workspace_origin.y, LINE_WIDTH, workspace.y), dot);

		// Draw horizontal line
		GUI.DrawTexture(new Rect(workspace_origin.x, center.y, workspace.x, LINE_WIDTH), dot);
		GUI.color = Color.white;
	}
	
	int halfDot = 1;
	public void DrawPoints(Vector2[] points)
	{
		DrawPoints(points, Color.blue);
	}

	public void DrawPoints(Vector2[] points, Color col)
	{
		halfDot = UV_DOT_SIZE / 2;

		foreach(Vector2 guiPoint in points)
		{
			// Vector2 guiPoint = UVToGUIPoint(uv_coord);
			GUI.color = col;
				GUI.DrawTexture(new Rect(guiPoint.x-halfDot, guiPoint.y-halfDot, UV_DOT_SIZE, UV_DOT_SIZE), dot, ScaleMode.ScaleToFit);
			GUI.color = Color.white;

			if(showCoordinates)
				GUI.Label(new Rect(guiPoint.x, guiPoint.y, 100, 40), "" + GUIToUVPoint(guiPoint).ToString("F3") );
		}
	}

	public void DrawBox(Vector2 p0, Vector2 p1, Color col)
	{
		GUI.backgroundColor = col;
		GUI.Box(GUIRectWithPoints(p0, p1), "");
		GUI.backgroundColor = Color.white;
	}

	public void DrawLines(Vector2[] points, Color col)
	{
		Handles.BeginGUI();
		Handles.color = col;

			for(int i = 0; i < points.Length; i+=2)
				Handles.DrawLine(
					points[i+0],
					points[i+1]);

		Handles.color = Color.white;
		Handles.EndGUI();
	}

	public void DrawBoundingBox(Vector2[] points)
	{
		Vector2 min = Vector2ArrayMin(points);
		Vector2 max = Vector2ArrayMax(points);
		
		GUI.color = new Color(.2f, .2f, .2f, .2f);
			GUI.Box(GUIRectWithPoints( min, max), "");
		GUI.color = Color.white;
	}

	private void DrawPreferencesPane()
	{
		settingsBoxRect = new Rect(settingsBoxPad, settingsBoxPad, Screen.width-settingsBoxPad*2, settingsBoxHeight-settingsBoxPad);
		PREFERENCES_MAX_WIDTH = ((int)settingsBoxRect.width-settingsBoxPad*2) / 2 - settingsBoxPad;
		Rect revertRect = new Rect(Screen.width-95-settingsBoxPad*2-10, 10, 90, 20);
		Rect exportRect = new Rect(Screen.width-190-settingsBoxPad*2-10, 10, 90, 20);
		Rect foldoutRect = new Rect(7, 10, 20, 20);
		
		int SEG_WIDTH = PREFERENCES_MAX_WIDTH/2;

		GUI.Box(settingsBoxRect, "");
		GUI.BeginGroup(settingsBoxRect);

			showPreferences = EditorGUI.Foldout(foldoutRect, showPreferences, "Preferences");

			EditorGUIUtility.labelWidth = 100;

			if(GUI.Button(revertRect, new GUIContent("Revert", "Reverts all changes made to the UV channel back to the original Mesh UV")))
				Revert(selection);

			if(GUI.Button(exportRect, new GUIContent("Export", "Exports a new Mesh copy and saves it to your project folder so that you can use this mesh with the modified UVs anywhere.")))
				Export();

			GUILayout.Space(foldoutRect.height + 20);

			if(showPreferences)
			{
				GUILayout.BeginHorizontal();
				
				GUILayout.BeginVertical();
					settingsBoxHeight = SETTINGS_BOX_EXPANDED;
					workspace_scale = EditorGUILayout.IntSlider("Scale", workspace_scale, MIN_ZOOM, MAX_ZOOM, GUILayout.MaxWidth(SEG_WIDTH), GUILayout.MinWidth(SEG_WIDTH));
					
					GUILayout.Space(3);
					EditorGUI.BeginChangeCheck();
		
						uvChannel = (UVChannel)EditorGUILayout.EnumPopup("UV Channel", uvChannel, GUILayout.MaxWidth(SEG_WIDTH), GUILayout.MinWidth(SEG_WIDTH));

				GUILayout.EndVertical();

				GUILayout.BeginVertical();
						string[] submeshes = new string[ (selection != null && selection.Length > 0) ? selection[0].sharedMesh.subMeshCount+1 : 1];

						submeshes[0] = "All";

						for(int i = 1; i < submeshes.Length; i++)
							submeshes[i] = (i-1).ToString();

						submesh = EditorGUILayout.Popup("Submesh", submesh, submeshes, GUILayout.MaxWidth(SEG_WIDTH), GUILayout.MinWidth(SEG_WIDTH));
						
						if(GUILayout.Button("Generate UV2", GUILayout.MaxWidth(SEG_WIDTH), GUILayout.MinWidth(SEG_WIDTH)))
						{
							GenerateUV2(selection);
							UpdateGUIPointCache();
						}

					if(EditorGUI.EndChangeCheck())
					{
						UpdateGUIPointCache();
						Repaint();
					}
				GUILayout.EndVertical();

				GUILayout.Space(10);

				GUILayout.BeginVertical();
				
					showCoordinates = EditorGUILayout.Toggle("Coordinates", showCoordinates, GUILayout.MaxWidth(SEG_WIDTH), GUILayout.MinWidth(SEG_WIDTH));
					drawTriangles = EditorGUILayout.Toggle("Triangles", drawTriangles, GUILayout.MaxWidth(SEG_WIDTH), GUILayout.MinWidth(SEG_WIDTH));

				GUILayout.EndVertical();

				GUILayout.BeginVertical();
					GUI.changed = false;
					showTex = EditorGUILayout.Toggle("Display Texture", showTex, GUILayout.MaxWidth(SEG_WIDTH), GUILayout.MinWidth(SEG_WIDTH));
					if(GUI.changed) OnSelectionChange();

					GUI.enabled = showTex;				
					tex = TexturePopup(Selection.transforms, SEG_WIDTH);
					GUI.enabled = true;				
				GUILayout.EndVertical();

				// GUILayout.BeginVertical();	

				// 	// ABSOLUTE POSITIONING
				// 	// switch(tool)
				// 	// {
				// 	// 	case UVTool.Rotate:
							
				// 	// 		setRotation = EditorGUILayout.FloatField("Rotation", setRotation, GUILayout.MaxWidth(SEG_WIDTH), GUILayout.MinWidth(SEG_WIDTH));

				// 	// 		if(GUILayout.Button("Set Rotation", GUILayout.MaxWidth((int)(SEG_WIDTH))))
				// 	// 		{
				// 	// 			BeginModifyUVs();
				// 	// 			RotateUVs(setRotation);
				// 	// 			UpdateGUIPointCache();
				// 	// 		}
				// 	// 		break;

				// 	// 	case UVTool.Scale:
				// 	// 		setPosition = EditorGUILayout.Vector2Field("Position", setPosition, GUILayout.MaxWidth(SEG_WIDTH), GUILayout.MinWidth(SEG_WIDTH));

				// 	// 		if(GUILayout.Button("Set Scale", GUILayout.MaxWidth((int)(SEG_WIDTH))))
				// 	// 		{
				// 	// 			BeginModifyUVs();
				// 	// 			ScaleUVs(setPosition);
				// 	// 			UpdateGUIPointCache();
				// 	// 		}
				// 	// 		break;

				// 	// 	default:
				// 	// 		setPosition = EditorGUILayout.Vector2Field("Position", setPosition, GUILayout.MaxWidth(SEG_WIDTH), GUILayout.MinWidth(SEG_WIDTH));

				// 	// 		maintainSpacing = EditorGUILayout.Toggle("Maintain Spacing", maintainSpacing, GUILayout.MaxWidth(SEG_WIDTH), GUILayout.MinWidth(SEG_WIDTH));

				// 	// 		if(GUILayout.Button("Set Position", GUILayout.MaxWidth((int)(SEG_WIDTH))))
				// 	// 		{
				// 	// 			BeginModifyUVs();
				// 	// 			SetUVPosition(setPosition, maintainSpacing);
				// 	// 			UpdateGUIPointCache();
				// 	// 		}
				// 	// 		break;
				// 	// }

				// 	// DRAG_BOX_COLOR = EditorGUILayout.ColorField("Drag Box", DRAG_BOX_COLOR);

				// GUILayout.EndVertical();
				
				GUILayout.FlexibleSpace();

				GUILayout.EndHorizontal();

				GUILayout.Space(8);

				// Toolbar
				GUILayout.BeginHorizontal(GUILayout.MaxWidth(settingsBoxRect.width-2));
					GUILayout.Space(1);
					for(int i = 0; i < UVTOOL_LENGTH; i++)
					{
						if((int)tool == i) GUI.backgroundColor = Color.gray;
						if(GUILayout.Button(((UVTool)i).ToString(), EditorStyles.toolbarButton))
							tool = (UVTool)i;
						GUI.backgroundColor = Color.white;
					}

				GUILayout.EndHorizontal();
			}
			else
				settingsBoxHeight = SETTINGS_BOX_COMPACT;
		GUI.EndGroup();
	}
#endregion

#region TOOLS

	bool movingUVs = false;

	float SnapValue(float val, float snpVal)
	{
		return snpVal * Mathf.Round(val / snpVal);
	}
	
	void MoveTool()
	{
		Vector2 pos = uv_center;
		Vector2 t_pos = pos;

		pos = UVee_HandleUtility.PositionHandle2d(1, pos, TOOL_SIZE);

		if(pos != t_pos)
		{
			if(!movingUVs)
				BeginModifyUVs();
				
			Vector2 delta = GUIToUVDirection( pos - center_origin );
			
			if(Event.current.modifiers == EventModifiers.Command || Event.current.modifiers == EventModifiers.Control)
			{
				delta.x = SnapValue(delta.x, .1f);
				delta.y = SnapValue(delta.y, .1f);
			}

			TranslateUVs( delta );
			
			UpdateGUIPointCache();

			needsRepaint = true;
		}
	}

	void RotateTool()
	{
		float t_rot = uv_rotation;

		uv_rotation = UVee_HandleUtility.RotationHandle2d(2, uv_center, uv_rotation, TOOL_SIZE) ;

		if(uv_rotation != t_rot)
		{
			if(!movingUVs)
				BeginModifyUVs();
			
			if(Event.current.modifiers == EventModifiers.Command || Event.current.modifiers == EventModifiers.Control)
				uv_rotation = SnapValue(uv_rotation, 15f);

			RotateUVs( uv_rotation );
			
			UpdateGUIPointCache();

			needsRepaint = true;
		}
	}

	void ScaleTool()
	{
		Vector2 t_ska = uv_scale;

		uv_scale = UVee_HandleUtility.ScaleHandle2d(3, uv_center, uv_scale, TOOL_SIZE) ;

		if(uv_scale != t_ska)
		{
			if(!movingUVs)
				BeginModifyUVs();
			
			if(Event.current.modifiers == EventModifiers.Command || Event.current.modifiers == EventModifiers.Control)
			{
				uv_scale.x = SnapValue(uv_scale.x, .1f);
				uv_scale.y = SnapValue(uv_scale.y, .1f);
			}

			ScaleUVs( uv_scale );
			
			UpdateGUIPointCache();

			needsRepaint = true;
		}
	}

	/**
	 * Copies instances of mesh, registers Undo, etc.
	 */
	Vector2[][] uv_origins = new Vector2[0][];
	Vector2 center_origin = Vector2.zero;	// in GUI space
	Vector2 uv_center_origin = Vector2.zero;
	float uv_rotation = 0f;
	Vector2 uv_scale = Vector2.one;
	void BeginModifyUVs()
	{
		movingUVs = true;
		center_origin = uv_center;
		uv_center_origin = GUIToUVPoint(center_origin);		
		uv_origins = new Vector2[selection.Length][];

		for(int i = 0; i < selection.Length; i++)
		{

			if(!selection[i].sharedMesh.name.Contains("uvee-"))
			{
				Undo.RecordObject(selection[i].rawObject, "Modify UVs");
				CreateMeshInstance(selection[i]);
			}
			else
			{
				Undo.RecordObject(selection[i].rawObject, "Modify UVs");
				// Mesh old = selection[i].sharedMesh;
				// CreateMeshInstance(selection[i]);
				// Undo.DestroyObjectImmediate(old);
			}			

			// copy origin uvs
			Vector2[] uvs = GetUVChannel(selection[i].sharedMesh, uvChannel);
			uv_origins[i] = new Vector2[uvs.Length];
			System.Array.Copy(uvs, uv_origins[i], uvs.Length);
		}

		for(int i = 0; i < Selection.transforms.Length; i++)
			EditorUtility.SetDirty(Selection.transforms[i]);
	}
#endregion

#region UV WRANGLING

	private void TranslateUVs(Vector2 uvDelta)
	{
		for(int i = 0; i < selection.Length; i++)
		{	
			Vector2[] uvs = GetUVChannel(selection[i].sharedMesh, uvChannel);

			foreach(int n in selected_triangles[i])
				uvs[n] = uv_origins[i][n] + uvDelta;

			SetUVChannel(selection[i].sharedMesh, uvChannel, uvs);

			PropertyModification[] propmods = PrefabUtility.GetPropertyModifications(selection[i].rawObject);
			PrefabUtility.SetPropertyModifications(selection[i].rawObject, propmods);
		}
	}

	/**
	 * Set the UV position of all selected Uvs exactly to this point.  If maintainSpacing is true,
	 * the relative positions of UV coordinates will be preserved.  If false, each point will be
	 * collapsed to `pos`.
	 */
	private void SetUVPosition(Vector2 pos, bool maintainSpacing)
	{
		for(int i = 0; i < selection.Length; i++)
		{	
			Vector2[] uvs = GetUVChannel(selection[i].sharedMesh, uvChannel);

			Vector2 cen = Vector2.zero;

			int[] dist = selected_triangles[i].Distinct().ToArray();
			foreach(int n in dist)
				cen += uvs[n];

			cen /= (float)dist.Length;

			foreach(int n in selected_triangles[i])
				uvs[n] = maintainSpacing ? (pos + (cen- uvs[n])) : pos;

			SetUVChannel(selection[i].sharedMesh, uvChannel, uvs);

			PropertyModification[] propmods = PrefabUtility.GetPropertyModifications(selection[i].rawObject);
			PrefabUtility.SetPropertyModifications(selection[i].rawObject, propmods);
		}
	}

	public void ScaleUVs(Vector2 uvDelta)
	{

		for(int i = 0; i < selection.Length; i++)
		{
			Vector2[] uvs = GetUVChannel(selection[i].sharedMesh, uvChannel);

			foreach(int n in selected_triangles[i])
			{
				Vector2 p = uv_origins[i][n];

				p -= uv_center_origin;
				p.Scale(uvDelta);
				p += uv_center_origin;

				uvs[n] = p;
			}

			SetUVChannel(selection[i].sharedMesh, uvChannel, uvs);

			PropertyModification[] propmods = PrefabUtility.GetPropertyModifications(selection[i].rawObject);
			PrefabUtility.SetPropertyModifications(selection[i].rawObject, propmods);
		}
	}

	public void RotateUVs(float deg)
	{
		float theta = Mathf.Deg2Rad * deg;

		for(int i = 0; i < selection.Length; i++)
		{
			Vector2[] uvs = GetUVChannel(selection[i].sharedMesh, uvChannel);
			
			foreach(int n in selected_triangles[i])
			{
				Vector2 p = uv_origins[i][n];
				uvs[n] = p.RotateAroundPoint(uv_center_origin, theta);			
			}

			SetUVChannel(selection[i].sharedMesh, uvChannel, uvs);

			PropertyModification[] propmods = PrefabUtility.GetPropertyModifications(selection[i].rawObject);
			PrefabUtility.SetPropertyModifications(selection[i].rawObject, propmods);
		}
	}

	public void GenerateUV2(MeshSelection[] selection)
	{
		for(int i = 0; i < selection.Length; i++)
		{
			Unwrapping.GenerateSecondaryUVSet(selection[i].sharedMesh);
		}
	}
#endregion

#region UTILITY

	int mat_selected = 0;
	List<Texture> mat_textures = new List<Texture>();
	string[] mat_names = new string[0]{};
	int[] mat_values = new int[0] {};
	string[] EMPTY_STRING_ARRAY = new string[0] {};
	int[] EMPTY_INT_ARRAY = new int[0] {};

	Texture TexturePopup(Transform[] transforms, int maxWidth)
	{
		Material[] materials = selection.SelectMany(x => x.renderer.sharedMaterials).ToArray();

		if(materials == null || materials.Length < 1)
		{
			EditorGUILayout.IntPopup(0, EMPTY_STRING_ARRAY, EMPTY_INT_ARRAY, GUILayout.MaxWidth(maxWidth));
			return null;
		}

		mat_textures = materials.SelectMany(x => GetTextures(x)).ToList();

		if(mat_textures.Count < 1)
		{
			EditorGUILayout.IntPopup(0, EMPTY_STRING_ARRAY, EMPTY_INT_ARRAY, GUILayout.MaxWidth(maxWidth));
			return null;
		}

		mat_names = mat_textures.Select(x => x.name).ToArray();

		mat_values = new int[mat_names.Length];

		for(int i = 0; i < mat_names.Length; i++)
			mat_values[i] = i;

		mat_selected = EditorGUILayout.IntPopup(mat_selected, mat_names, mat_values, GUILayout.MaxWidth(maxWidth));

		return mat_textures[ (int)Mathf.Clamp(mat_selected, 0, mat_names.Length-1) ];
	}

	List<Texture> GetTextures(Material material)
	{
		List<Texture> list = new List<Texture>();

		var count = ShaderUtil.GetPropertyCount(material.shader);
		for(var i = 0; i < count; i++)
		{
			if(ShaderUtil.GetPropertyType(material.shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
			{
				string name = ShaderUtil.GetPropertyName(material.shader, i);
				Texture t = (Texture)material.GetTexture(name);

				if(t != null)
					list.Add( t );
			}
		}
		return list;
	}

	private Color RandomColor()
	{
		return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
	}

	private void Revert(MeshSelection[] mfs)
	{
		foreach(MeshSelection mf in mfs)
		{
			PrefabUtility.ReconnectToLastPrefab(mf.gameObject);
			PrefabUtility.ResetToPrefabState(mf.rawObject);
		}
		#if UNITY_5
		EditorUtility.UnloadUnusedAssetsImmediate();
		#else
		EditorUtility.UnloadUnusedAssets();
		#endif
	}

	
	private void Export()
	{
		if(selection == null || selection.Length < 1) return;


		string path = "Assets/UVee/Exported Meshes/";
		if(!System.IO.Directory.Exists(path))
			System.IO.Directory.CreateDirectory(path);

		foreach(MeshSelection mf in selection)
		{
			if(mf.sharedMesh == null) continue;

			AssetDatabase.CreateAsset( MeshInstance(mf), AssetDatabase.GenerateUniqueAssetPath(path + mf.rawObject.name + ".asset"));
		}

		AssetDatabase.Refresh();
	}

	private void CreateMeshInstance(MeshSelection mf)
	{
		// Mesh m = new Mesh();
		// m.vertices = mf.sharedMesh.vertices;
		// m.subMeshCount = mf.sharedMesh.subMeshCount;
		// for(int i = 0; i < m.subMeshCount; i++)
		// 	m.SetTriangles(mf.sharedMesh.GetTriangles(i), i);
		// m.normals = mf.sharedMesh.normals;
		// m.uv = mf.sharedMesh.uv;
		// m.uv2 = mf.sharedMesh.uv2;
		// m.tangents = mf.sharedMesh.tangents;
		// m.colors = mf.sharedMesh.colors;
		// m.colors32 = mf.sharedMesh.colors32;
		// m.boneWeights = mf.sharedMesh.boneWeights;
		// m.bindposes = mf.sharedMesh.bindposes;
		// m.bounds = mf.sharedMesh.bounds;

		// m.name = "uvee-" + mf.sharedMesh.name;

		Mesh m = MeshInstance(mf);
		PrefabUtility.DisconnectPrefabInstance(mf.rawObject);
		Undo.RegisterCreatedObjectUndo(m, "Modify UVs");
		mf.sharedMesh = m;
	}

	private Mesh MeshInstance(MeshSelection mf)
	{
		Mesh m = new Mesh();

		m.vertices = mf.sharedMesh.vertices;
		m.subMeshCount = mf.sharedMesh.subMeshCount;
		for(int i = 0; i < m.subMeshCount; i++)
			m.SetTriangles(mf.sharedMesh.GetTriangles(i), i);
		m.normals = mf.sharedMesh.normals;
		m.uv = mf.sharedMesh.uv;
		m.uv2 = mf.sharedMesh.uv2;
		m.tangents = mf.sharedMesh.tangents;
		m.colors = mf.sharedMesh.colors;
		m.colors32 = mf.sharedMesh.colors32;
		m.boneWeights = mf.sharedMesh.boneWeights;
		m.bindposes = mf.sharedMesh.bindposes;
		m.bounds = mf.sharedMesh.bounds;

		m.name = "uvee-" + mf.sharedMesh.name;

		return m;
	}

	public void PopulateColorArray()
	{
		// for(int i = 0; i < COLOR_ARRAY.Length; i++)
		// 	COLOR_ARRAY[i] = RandomColor();

		COLOR_ARRAY[0] = Color.green;
		COLOR_ARRAY[1] = Color.cyan;
		COLOR_ARRAY[2] = Color.blue;
		COLOR_ARRAY[3] = Color.black;
		COLOR_ARRAY[4] = Color.magenta;
	}

	public Vector2[] UVArrayWithTriangles(MeshSelection mf, int[] tris)
	{
		List<Vector2> uvs = new List<Vector2>();

		Vector2[] mf_uv = GetUVChannel(mf.sharedMesh, uvChannel);
		
		if(mf_uv == null)
			return new Vector2[0]{};

		foreach(int tri in tris)
			uvs.Add(mf_uv[tri]);
		return uvs.ToArray();
	}

	public bool ValidUVPoints(MeshFilter mf, Vector2[] uvs)
	{
		return(uvs != null && uvs.Length == mf.sharedMesh.vertices.Length);
	}

	// Returns a rect in GUI coordinates
	public Rect GUIRectWithPoints(Vector2 p0, Vector2 p1)
	{
		float minX = p0.x < p1.x ? p0.x : p1.x;
		float minY = p0.y < p1.y ? p0.y : p1.y;

		float maxX = p0.x > p1.x ? p0.x : p1.x;
		float maxY = p0.y > p1.y ? p0.y : p1.y;

		return new Rect(minX, minY, maxX - minX, maxY - minY);
	}

	public Vector2 Center(int[][] selected_uv_indices)
	{
		Vector2 v = Vector2.zero;
		float count = 0f;

		for(int i = 0; i < selected_uv_indices.Length; i++)
		{
			Vector2[] uvs = GetUVChannel(selection[i].sharedMesh, uvChannel);

			int[] sel = selected_uv_indices[i];

			count += sel.Length;
			for(int x = 0; x < sel.Length; x++)
				v += uvs[sel[x]];
		}
		return v/count;
	}

	public Vector2 Vector2ArrayMin(Vector2[] val)
	{
		if(val == null || val.Length < 1)
			return Vector2.zero;

		float x = val[0].x, y = val[0].y;
		
		foreach(Vector2 v in val)
		{
			if(v.x < x)
				x = v.x;
			if(v.y < y)
				y = v.y;
		}
		return new Vector2(x, y);
	}

	public Vector2 Vector2ArrayMax(Vector2[] val)
	{
		if(val == null || val.Length < 1)
			return Vector2.zero;

		float x = val[0].x, y = val[0].y;
		
		foreach(Vector2 v in val)
		{
			if(v.x > x)
				x = v.x;
			if(v.y > y)
				y = v.y;
		}
		return new Vector2(x, y);
	}
#endregion

#region SCREEN TO UV SPACE CONVERSION AND CHECKS
	
	Vector2[] UVToGUIPoint(Vector2[] uvs)
	{
		Vector2[] uv = new Vector2[uvs.Length];
		for(int i = 0; i < uv.Length; i++)
			uv[i] = UVToGUIPoint(uvs[i]);
		return uv;
	}

	Vector2 UVToGUIPoint(Vector2 uv)
	{
		// flip y
		Vector2 u = new Vector2(uv.x, -uv.y);

		// offset
		u *= scale;
		u += center;
		// u -= new Vector2(buttonSize/2f, buttonSize/2f);
		u = new Vector2(Mathf.Round(u.x), Mathf.Round(u.y));
		return u;
	}

	Vector2 GUIToUVPoint(Vector2 gui)
	{
		gui -= center;
		gui /= scale;
		Vector2 u = new Vector2(gui.x, -gui.y);

		return u;
	}

	/**
	 * Flip Y axis and scale appropriately, but don't apply offset.
	 */
	Vector2 GUIToUVDirection(Vector2 dir)
	{
		return new Vector2(dir.x, -dir.y) / scale;
	}
#endregion

#region DEBUG
		
	Dictionary<string, List<float>> methodExecutionTimes = new Dictionary<string, List<float>>();
	private void LogMethodTime(string methodName, float time)
	{
		if(methodExecutionTimes.ContainsKey(methodName))
			methodExecutionTimes[methodName].Add(time);
		else
			methodExecutionTimes.Add(methodName, new List<float>(new float[1]{time}));
	}

	Dictionary<string, float> timer = new Dictionary<string, float>();
	private void LogStart(string methodName)
	{
		if(methodExecutionTimes.ContainsKey(methodName))
			timer[methodName] = (float)EditorApplication.timeSinceStartup;
		else
			timer.Add(methodName, (float)EditorApplication.timeSinceStartup);
	}

	private void LogFinish(string methodName)
	{
		LogMethodTime(methodName, (float)EditorApplication.timeSinceStartup - timer[methodName]);
	}

	private void DumpTimes()
	{
		foreach(KeyValuePair<string, List<float>> kvp in methodExecutionTimes)
		{
			UnityEngine.Debug.Log("Method: " + kvp.Key + "\nAvg. Time: " + Average(kvp.Value).ToString("F7") + "\nSamples: " + kvp.Value.Count);
		}
	}

	private float Average(List<float> list)
	{
		float avg = 0f;
		for(int i = 0; i < list.Count; i++)
			avg += list[i];
		return avg/(float)list.Count;
	}

	private Vector2 Average(List<Vector2> list)
	{
		Vector2 avg = Vector2.zero;
		for(int i = 0; i < list.Count; i++)
			avg += list[i];
		return avg/(float)list.Count;
	}	

	private Vector2 Average(Vector2[][] uvs)
	{
		float c = 0;
		Vector2 avg = Vector2.zero;
		foreach(Vector2[] varr in uvs)
		{
			foreach(Vector2 v in varr)
			{
				avg += v;
				c++;
			}
		}
		return avg/c;
	}
#endregion
}

#region EXTENSION

	public static class TransformExtensions
	{
		public static T[] GetComponents<T>(Transform[] t_arr) where T : Component
		{
			List<T> c = new List<T>();
			foreach(Transform t in t_arr)
			{
				c.AddRange(t.GetComponentsInChildren<T>());
			}
			return c.ToArray() as T[];
		}

		public static Mesh[] GetMeshesUVee(this Transform[] t_arr)
		{
			MeshFilter[] mfs = GetComponents<MeshFilter>(t_arr);
			Mesh[] m = new Mesh[mfs.Length];
			for(int i = 0; i < mfs.Length; i++)
				m[i] = mfs[i].sharedMesh;
			return m;
		}

		public static GameObject[] GetGameObjectsWithComponent<T>(Transform[] t_arr) where T : Component
		{
			List<GameObject> c = new List<GameObject>();
			foreach(Transform t in t_arr)
			{
				if(t.GetComponent<T>())	
					c.Add(t.gameObject);
			}
			return c.ToArray() as GameObject[];
		}

		public static void SetIsReadable(this Mesh m, bool readable)
		{
			AssetImporter ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(m));
			if(ai.GetType() == typeof(ModelImporter))
				((ModelImporter)ai).isReadable = readable;

			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
		}	

		public static string ToFormattedString(this int[] arr, string seperator)
		{
			if(arr == null || arr.Length < 1)
				return "";

			string str = "";
			for(int i = 0; i < arr.Length-1; i++)
				str += arr[i].ToString() + seperator;
			str += arr[arr.Length-1];
			return str;
		}

		public static Vector3[] VerticesInWorldSpace(UVeeWindow.MeshSelection mf)
		{
			if(mf == null || mf.gameObject == null || mf.sharedMesh == null)
				return new Vector3[0];

			Vector3[] v = mf.sharedMesh.vertices;
			for(int i = 0; i < v.Length; i++)
				v[i] = mf.gameObject.transform.TransformPoint(v[i]);
			return v;
		}
	}

	public static class VectorExtensions
	{
		public static Vector3 RotateAroundPoint(this Vector2 v, Vector2 origin, float theta)
		{
			// discard y val
			float cx = origin.x, cy = origin.y;	// origin
			float px = v.x, py = v.y;			// point

			float s = Mathf.Sin(theta);
			float c = Mathf.Cos(theta);

			// translate point back to origin:
			px -= cx;
			py -= cy;

			// rotate point
			float xnew = px * c + py * s;
			float ynew = -px * s + py * c;

			// translate point back:
			px = xnew + cx;
			py = ynew + cy;
			
			return new Vector2(px, py);
		}
	}
#endregion
}

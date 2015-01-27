#pragma warning disable 0168	///< Disable unused var (that exception hack)

using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.GUI;
using ProBuilder2.Math;

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

		pb_Object[] sel = Selection.transforms.GetComponents<pb_Object>();

		if(sel != null && sel.Length > 0)
			textures = GetTextures( sel[0].transform.GetComponent<MeshRenderer>().sharedMaterial ).ToArray();

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

	static readonly Color OuterRingColor = new Color(.4f, .7f, .4f, .5f);
	static readonly Color MiddleRingColor = new Color(.3f, 7f, .3f, .5f);
	static readonly Color InnerRingColor = new Color(.2f, 9f, .2f, .8f);

	const int MOUSE_BUTTON_LEFT = 0;
	const float BRUSH_STRENGTH_MAX = 24f;								///< Max brush applications per-second
	const float BRUSH_SIZE_MAX = 5f;

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

	Texture[] textures = new Texture[0];

	public enum VertexPainterMode
	{
		Color,
		Texture
	}

	private VertexPainterMode mode = VertexPainterMode.Color;

	private float brushStrength = 10f;			///< How many brush strokes should be registered per-second.
	private float brushOpacity = 1f;
	private double lastBrushApplication = 0f;	///< The last second that a brush stroke was applied.
	private double CurTime
	{
		get
		{
			return EditorApplication.timeSinceStartup;
		}
	}

	private Vector2 scroll = Vector2.zero;
	private bool helpFoldout = false;
	GUIContent gc_BrushSize = new GUIContent("Brush Size", "How big the brush is.  Use Ctrl + Scroll Wheel to adjust this in the Scene View.");
	GUIContent gc_BrushOpacity = new GUIContent("Opacity", "The opacity that this brush will paint.  Large value means fully opaque, low values are more transparent.");
	GUIContent gc_BrushStrength = new GUIContent("Strength", "How fast your brush affects the mesh colors.  High values mean changes happen quickly, low values mean colors have to be applied for longer to show.");
#endregion
 
#region OnGUI

	Vector3 nonzero(Vector3 vec)
	{
		if(vec.x == 0f && vec.y == 0f && vec.z == 0f)
			return Vector3.up;
		return vec;
	}

	void OnGUI()
	{		
		scroll = EditorGUILayout.BeginScrollView(scroll);

		GUILayout.BeginHorizontal(EditorStyles.toolbar);

			if(mode == VertexPainterMode.Color)	GUI.backgroundColor = Color.gray;
			if(GUILayout.Button("Colors", EditorStyles.toolbarButton))
				mode = VertexPainterMode.Color;

			GUI.backgroundColor = mode == VertexPainterMode.Texture ? Color.gray : Color.white;
			if(GUILayout.Button("Textures", EditorStyles.toolbarButton))
				mode = VertexPainterMode.Texture;

			GUI.backgroundColor = Color.white;
		GUILayout.EndHorizontal();

		lockhandleToCenter = EditorWindow.focusedWindow == this;

		/**
		 * BRUSH SETTINGS
		 */

		enabled = EditorGUILayout.Toggle("Enabled", enabled);
	
		EditorGUI.BeginChangeCheck();
			
			brushSize = EditorGUILayout.Slider(gc_BrushSize, brushSize, .01f, BRUSH_SIZE_MAX);
			brushOpacity = EditorGUILayout.Slider(gc_BrushOpacity, brushOpacity, .01f, 1f);
			brushStrength = EditorGUILayout.Slider(gc_BrushStrength, brushStrength, .01f, 1f);

		if(EditorGUI.EndChangeCheck())
			SceneView.RepaintAll();


		GUILayout.Space(6);
		pb_GUI_Utility.DrawSeparator(2, pb_Constant.ProBuilderLightGray);
		GUILayout.Space(6);

		/**
		 * COLOR / TEXTURE SPECIFIC SETTINGS
		 */

		if(mode == VertexPainterMode.Color)
		{
	 		EditorGUI.BeginChangeCheck();
			color = EditorGUILayout.ColorField("Color", color);
			if(EditorGUI.EndChangeCheck())
			{
				colorName = pb_ColorUtil.GetColorName(color);
			}

			GUILayout.Label(colorName, EditorStyles.boldLabel);
		}
		else
		{
			GUILayout.BeginHorizontal();
			int max = (Screen.width - 20) / 4;

			// Only allow 4
			for(int i = 0; i < (int)Mathf.Min(textures.Length, 4); i++)
			{
				if( GUILayout.Button(i < textures.Length ? textures[i] : null, EditorStyles.label, GUILayout.MaxWidth(max), GUILayout.MaxHeight(max)) )
				{
					color.r = i == 0 ? 1f : 0f;
					color.g = i == 1 ? 1f : 0f;
					color.b = i == 2 ? 1f : 0f;
					color.a = i == 3 ? 1f : 0f;
				}

				if( i < textures.Length && i == GetIndex(color) )
				{
					Rect r = GUILayoutUtility.GetLastRect();

					r.y += r.height + 4;
					r.width -= 4f;
					r.x += 2f;
					r.height = 6f;

					pb_GUI_Utility.DrawSolidColor(r, Color.green);
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(6);
		}

		GUILayout.Space(6);

		helpFoldout = EditorGUILayout.Foldout(helpFoldout, "Help!");

		if( helpFoldout )
		{
			EditorGUILayout.HelpBox(@"If you're not seeing anything happen on your ProBuilder object, make sure that you're using a shader that supports vertex colors.  The ProBuilder default material provides support for colored vertices, and the included `Diffuse Texture Blend` material supports blending multiple textures together.  You can use the shaders from either of these on new Materials that you create to enable vertex color support.", MessageType.Info);

			if(GUILayout.Button("Show me the Vertex Color Shader"))
			{
				Shader shader = Shader.Find("ProBuilder/Diffuse Vertex Color");
				if(shader != null)
					EditorGUIUtility.PingObject(shader);
				else
					Debug.LogWarning("Couldn't find default ProBuilder shader: \"Diffuse Vertex Color\"");
			}

			if(GUILayout.Button("Show me the Texture Blend Shader"))
			{
				Shader shader = Shader.Find("ProBuilder/Diffuse Texture Blend");
				if(shader != null)
					EditorGUIUtility.PingObject(shader);
				else
					Debug.LogWarning("Couldn't find default ProBuilder shader: \"Diffuse Texture Blend\"");
			}
		}

		EditorGUILayout.EndScrollView();
	}
#endregion

#region OnSceneGUI

	void OnSceneGUI(SceneView scnview)
	{
		if(!enabled)// || (EditorWindow.focusedWindow != scnview && !lockhandleToCenter))
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
		
		/**
		 * Check if a new object is under the mouse.
		 */
		if( mouseMoveEvent )
		{
			GameObject go = HandleUtility.PickGameObject(Event.current.mousePosition, false);

			if( go != null && (pb == null || go != pb.gameObject) )
			{
				pb = go.GetComponent<pb_Object>();

				if(pb != null)
				{
					textures = GetTextures( pb.transform.GetComponent<MeshRenderer>().sharedMaterial ).ToArray();
					Repaint();

					modified.Add(pb);
					
					pb.ToMesh();
					pb.Refresh();
				}
			}
		}

		/**
		 * Hit test scene
		 */
		if(!lockhandleToCenter && !pb_Handle_Utility.SceneViewInUse(currentEvent))
		{
			if(pb != null)
			{
				if(!hovering.ContainsKey(pb))
				{
					hovering.Add(pb, pb.colors ?? new Color[pb.vertexCount]);
				}
				else
				{
					if(pb.msh.vertexCount != pb.vertexCount)
					{
						// script reload can make this happen
						pb.ToMesh();
						pb.Refresh();
					}

					pb.msh.colors = hovering[pb];
				}
 
				Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
				pb_RaycastHit hit;

				if ( pb_Handle_Utility.MeshRaycast(ray, pb, out hit) )
				{
					handlePosition = pb.transform.TransformPoint(hit.Point);
					handleDistance = Vector3.Distance(handlePosition, sceneCamera.transform.position);					
					handleRotation = Quaternion.LookRotation(nonzero(pb.transform.TransformDirection(hit.Normal)), Vector3.up);
 
					Color[] colors = pb.msh.colors;

					int[][] sharedIndices = pb.sharedIndices.ToArray();

					// wrapped in try/catch because a script reload can cause the mesh
					// to re-unwrap itself in some crazy configuration, throwing off the 
					// vertex count sync.
					try
					{
						for(int i = 0; i < sharedIndices.Length; i++)
						{
							float dist = Vector3.Distance(hit.Point, pb.vertices[sharedIndices[i][0]]);

							if(dist < brushSize)
							{
								for(int n = 0; n < sharedIndices[i].Length; n++)
								{
									colors[sharedIndices[i][n]] = Lerp(hovering[pb][sharedIndices[i][n]], color, (1f-(dist/brushSize)) * brushOpacity );
								}
							}
						}
	 				} catch (System.Exception e) { /* shhhhh */ }

					// show a preview
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

		/**
		*    Draw the handles
		*/
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
 
		/**
		 * Apply colors to mesh
		 */
		if( (currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag) &&
		   	(currentEvent.button == MOUSE_BUTTON_LEFT) &&
		   	currentEvent.modifiers == (EventModifiers)0 &&
		   	((CurTime - lastBrushApplication) > 1f/(brushStrength * BRUSH_STRENGTH_MAX)) )
		{
			lastBrushApplication = CurTime;

			Dictionary<pb_Object, Color[]> sticky = new Dictionary<pb_Object, Color[]>();
 	
 			// Apply colors
			foreach(KeyValuePair<pb_Object, Color[]> kvp in hovering)
			{
				Color[] colors = kvp.Key.msh.colors;

				sticky.Add(kvp.Key, colors);
 
				// kvp.Key.msh.colors = kvp.Value;

				pbUndo.RecordObjects(new Object[] {kvp.Key}, "Apply Vertex Colors");

				kvp.Key.SetColors(colors);

				// kvp.Key.msh.colors = colors;
			}
 
			hovering = sticky;
		}

		if(currentEvent.control && currentEvent.type == EventType.ScrollWheel)
		{
			currentEvent.Use();
			brushSize += (currentEvent.delta.y > 0f ? -1f : 1f) * (brushSize * .1f);
			brushSize = Mathf.Clamp(brushSize, .01f, BRUSH_SIZE_MAX);

			Repaint();
		}
 
		if(mpos != currentEvent.mousePosition && currentEvent.type == EventType.Repaint)
		{
			mpos = currentEvent.mousePosition;
			SceneView.RepaintAll();
		}
	}
#endregion

#region Utility

	static Color Lerp(Color lhs, Color rhs, float alpha)
	{
		return new Color(lhs.r * (1f-alpha) + rhs.r * alpha,
		                 lhs.g * (1f-alpha) + rhs.g * alpha,
		                 lhs.b * (1f-alpha) + rhs.b * alpha,
		                 lhs.a * (1f-alpha) + rhs.a * alpha );
	}

	private static List<Texture> GetTextures(Material mat)
	{
		List<Texture> textures = new List<Texture>();

		for(int i = 0; i < ShaderUtil.GetPropertyCount(mat.shader); i++)
		{
			if( ShaderUtil.GetPropertyType(mat.shader, i) == ShaderUtil.ShaderPropertyType.TexEnv )
			{
				Texture t = mat.GetTexture( ShaderUtil.GetPropertyName(mat.shader, i));
				
				if(t != null)
					textures.Add(t);
			}
		}

		return textures;
	}

	int GetIndex(Color col)
	{
		if(col.r > col.g && col.r > col.b && col.r > col.a)
			return 0;
		else if(col.g > col.r && col.g > col.b && col.g > col.a)
			return 1;
		else if(col.b > col.r && col.b > col.g && col.b > col.a)
			return 2;
		else if(col.a > col.r && col.a > col.g && col.a > col.b)
			return 3;

		return 0;
	}
#endregion
}
 

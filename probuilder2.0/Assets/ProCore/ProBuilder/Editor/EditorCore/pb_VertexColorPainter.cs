using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using ProBuilder.EditorCore;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.Interface;

namespace ProBuilder.EditorCore
{
	class pb_VertexColorPainter : EditorWindow
	{
		public static void MenuOpenWindow()
		{
			EditorWindow.GetWindow<pb_VertexColorPainter>(true, "ProBuilder Vertex Painter", true).Show();
		}

		void OnEnable()
		{
			// pb_Lightmapping.PushGIWorkflowMode();

			if (SceneView.onSceneGUIDelegate != this.OnSceneGUI)
			{
				SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
				SceneView.onSceneGUIDelegate += this.OnSceneGUI;
			}

			pb_Object[] sel = Selection.transforms.GetComponents<pb_Object>();

			if (sel != null && sel.Length > 0)
				textures = GetTextures(sel[0].transform.GetComponent<MeshRenderer>().sharedMaterial).ToArray();

			if (editor)
				editor.SetEditLevel(EditLevel.Plugin);

			colorName = pb_ColorUtil.GetColorName(color);

			// load the users custom palette
			UserColors = new Color[10];
			for (int i = 0; i < DEFAULT_COLORS.Length; i++)
			{
				if (!pb_Util.TryParseColor(pb_PreferencesInternal.GetString(pb_Constant.pbVertexColorPrefs + i), ref UserColors[i]))
					UserColors[i] = DEFAULT_COLORS[i];
			}
		}

		void OnDisable()
		{
			// pb_Lightmapping.PopGIWorkflowMode();

			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;

			foreach (pb_Object pb in modified)
			{
				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			if (editor)
				editor.PopEditLevel();
		}

		private readonly Color[] DEFAULT_COLORS = new Color[10]
		{
			Color.white,
			Color.red,
			Color.blue,
			Color.yellow,
			Color.green,
			Color.cyan,
			Color.black,
			Color.magenta,
			Color.gray,
			new Color(.4f, 0f, 1f, 1f)
		};

		private Color[] UserColors;

		public pb_Editor editor
		{
			get { return pb_Editor.instance; }
		}

		// Convenience getter for pb_Editor.instance
		static readonly Color OuterRingColor = new Color(.4f, .7f, .4f, .5f);

		static readonly Color MiddleRingColor = new Color(.3f, 7f, .3f, .5f);
		static readonly Color InnerRingColor = new Color(.2f, 9f, .2f, .8f);

		const int MOUSE_BUTTON_LEFT = 0;
		const float BRUSH_STRENGTH_MAX = 24f;

		///< Max brush applications per-second
		const float BRUSH_SIZE_MAX = 5f;

		Color color = Color.green;

		///< The color currently being painted.
		string colorName = "Green";

		///< Human readable color name.
		bool enabled = true;

		///< Is the tool enabled?
		float brushSize = .5f;

		///< The brush size to use.

		ColorMask colorMask = new ColorMask(true, true, true, true);

		///< Apply all colors by default.

		Event currentEvent;

		///< Cache the current event at start of OnSceneGUI.
		Camera sceneCamera;

		///< Cache the sceneview camera at start of OnSceneGUI.

		///< Used to store changes to mesh color array for live preview.
		Dictionary<pb_Object, Color[]> hovering = new Dictionary<pb_Object, Color[]>();

		Vector2 mpos = Vector2.zero;
		pb_Object pb; // The object currently gettin' paintered
		bool mouseMoveEvent = false;
		Vector3 handlePosition = Vector3.zero;
		Quaternion handleRotation = Quaternion.identity;
		float handleDistance = 10f;
		bool lockhandleToCenter = false;
		Vector2 screenCenter = Vector2.zero;
		bool isPainting = false;

		HashSet<pb_Object> modified = new HashSet<pb_Object>()
			; // list of all objects that have been modified by the painter, stored so that we can regenerate UV2 on disable

		Texture[] textures = new Texture[0];

		public enum VertexPainterMode
		{
			Color,
			Texture
		}

		private VertexPainterMode mode = VertexPainterMode.Color;

		private float brushStrength = 10f;

		///< How many brush strokes should be registered per-second.
		private float brushOpacity = 1f;

		private double lastBrushApplication = 0f;

		///< The last second that a brush stroke was applied.
		private double CurTime
		{
			get { return EditorApplication.timeSinceStartup; }
		}

		private Vector2 scroll = Vector2.zero;
		private bool helpFoldout = false;

		GUIContent gc_BrushSize = new GUIContent("Brush Size",
			"How big the brush is.  Use Ctrl + Scroll Wheel to adjust this in the Scene View.");

		GUIContent gc_BrushOpacity = new GUIContent("Opacity",
			"The opacity that this brush will paint.  Large value means fully opaque, low values are more transparent.");

		GUIContent gc_BrushStrength = new GUIContent("Strength",
				"How fast your brush affects the mesh colors.  High values mean changes happen quickly, low values mean colors have to be applied for longer to show.")
			;

		Vector3 nonzero(Vector3 vec)
		{
			if (vec.x == 0f && vec.y == 0f && vec.z == 0f)
				return Vector3.up;
			return vec;
		}

		void OnGUI()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);

			if (mode == VertexPainterMode.Color) GUI.backgroundColor = Color.gray;

			if (GUILayout.Button("Colors", EditorStyles.toolbarButton))
			{
				colorName = pb_ColorUtil.GetColorName(color);
				mode = VertexPainterMode.Color;
			}

			GUI.backgroundColor = mode == VertexPainterMode.Texture ? Color.gray : Color.white;
			if (GUILayout.Button("Textures", EditorStyles.toolbarButton))
			{
				switch (GetIndex(color))
				{
					case 0:
						color = Color.red;
						break;
					case 1:
						color = Color.green;
						break;
					case 2:
						color = Color.blue;
						break;
					case 3:
						color = Color.black;
						break;
				}
				mode = VertexPainterMode.Texture;
			}

			GUI.backgroundColor = Color.white;
			GUILayout.EndHorizontal();

			scroll = EditorGUILayout.BeginScrollView(scroll);


			lockhandleToCenter = EditorWindow.focusedWindow == this;

			/**
			 * BRUSH SETTINGS
			 */

			enabled = EditorGUILayout.Toggle("Enabled", enabled);

			EditorGUI.BeginChangeCheck();

			brushSize = EditorGUILayout.Slider(gc_BrushSize, brushSize, .01f, BRUSH_SIZE_MAX);
			brushOpacity = EditorGUILayout.Slider(gc_BrushOpacity, brushOpacity, .01f, 1f);
			brushStrength = EditorGUILayout.Slider(gc_BrushStrength, brushStrength, .01f, 1f);

			if (EditorGUI.EndChangeCheck())
				SceneView.RepaintAll();

			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Channel Mask");
			EditorGUIUtility.labelWidth = 18;
			colorMask.r = EditorGUILayout.Toggle("R", colorMask.r);
			colorMask.g = EditorGUILayout.Toggle("G", colorMask.g);
			colorMask.b = EditorGUILayout.Toggle("B", colorMask.b);
			colorMask.a = EditorGUILayout.Toggle("A", colorMask.a);
			EditorGUIUtility.labelWidth = 0;
			GUILayout.EndHorizontal();

			GUILayout.Space(6);
			pb_EditorGUIUtility.DrawSeparator(2, pb_Constant.ProBuilderLightGray);
			GUILayout.Space(6);

			/**
			 * COLOR / TEXTURE SPECIFIC SETTINGS
			 */

			if (mode == VertexPainterMode.Color)
			{
				ColorGUI();
			}
			else
			{
				GUILayout.BeginHorizontal();
				int max = (Screen.width - 20) / 4;

				// Only allow 4
				for (int i = 0; i < (int) Mathf.Min(textures.Length, 4); i++)
				{
					if (GUILayout.Button(i < textures.Length ? textures[i] : null, EditorStyles.label, GUILayout.MaxWidth(max),
						GUILayout.MaxHeight(max)))
					{
						color.r = i == 0 ? 1f : 0f;
						color.g = i == 1 ? 1f : 0f;
						color.b = i == 2 ? 1f : 0f;
						color.a = i == 3 ? 1f : 0f;
					}

					if (i < textures.Length && i == GetIndex(color))
					{
						Rect r = GUILayoutUtility.GetLastRect();

						r.y += r.height + 4;
						r.width -= 4f;
						r.x += 2f;
						r.height = 6f;

						pb_EditorGUIUtility.DrawSolidColor(r, Color.green);
					}
				}
				GUILayout.EndHorizontal();

				GUILayout.Space(6);
			}

			GUILayout.Space(6);

			helpFoldout = EditorGUILayout.Foldout(helpFoldout, "Help!");

			if (helpFoldout)
			{
				EditorGUILayout.HelpBox(
					@"If you're not seeing anything happen on your ProBuilder object, make sure that you're using a shader that supports vertex colors.  The ProBuilder default material provides support for colored vertices, and the included `Diffuse Texture Blend` material supports blending multiple textures together.  You can use the shaders from either of these on new Materials that you create to enable vertex color support.",
					MessageType.Info);

				if (GUILayout.Button("Show me the Vertex Color Shader"))
				{
					Shader shader = Shader.Find("ProBuilder/Diffuse Vertex Color");
					if (shader != null)
						EditorGUIUtility.PingObject(shader);
					else
						Debug.LogWarning("Couldn't find default ProBuilder shader: \"Diffuse Vertex Color\"");
				}

				if (GUILayout.Button("Show me the Texture Blend Shader"))
				{
					Shader shader = Shader.Find("ProBuilder/Diffuse Texture Blend");
					if (shader != null)
						EditorGUIUtility.PingObject(shader);
					else
						Debug.LogWarning("Couldn't find default ProBuilder shader: \"Diffuse Texture Blend\"");
				}
			}

			EditorGUILayout.EndScrollView();
		}

		// Color[] colors = new Color[12];
		Vector2 colorScroll = Vector2.zero;

		int pad = 4;
		int ButtonWidth = 50;

		void ColorGUI()
		{
			Rect r = GUILayoutUtility.GetLastRect();
			r.x = 6;
			r.y += r.height;
			r.width = Screen.width - 12;
			r.height = 26;

			pb_EditorGUIUtility.DrawSolidColor(r, color);

			GUILayout.Label("  Brush Color: " + colorName, EditorStyles.boldLabel);

			GUILayout.Space(2);

			colorScroll =
				EditorGUILayout.BeginScrollView(colorScroll, GUILayout.MinHeight(82), GUILayout.MaxHeight(Screen.height));

			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();

			int curRow = 0, rowSize = Screen.width / (ButtonWidth + 5);

			for (int i = 0; i < UserColors.Length; i++)
			{
				if ((i - (curRow * rowSize)) >= rowSize)
				{
					curRow++;
					GUILayout.FlexibleSpace();

					GUILayout.EndHorizontal();

					GUILayout.Space(6);

					GUILayout.BeginHorizontal();
				}

				GUILayout.BeginVertical();

				GUI.color = Color.white;

				if (GUILayout.Button(EditorGUIUtility.whiteTexture, GUILayout.Width(ButtonWidth), GUILayout.Height(42)))
				{
					color = UserColors[i];
					colorName = pb_ColorUtil.GetColorName(color);
				}

				GUI.color = UserColors[i];
				Rect layoutRect = GUILayoutUtility.GetLastRect();
				layoutRect.x += pad;
				layoutRect.y += pad;
				layoutRect.width -= pad * 2;
				layoutRect.height -= pad * 2;
				EditorGUI.DrawPreviewTexture(layoutRect, EditorGUIUtility.whiteTexture, null, ScaleMode.StretchToFill, 0);

				GUI.changed = false;
				UserColors[i] = EditorGUILayout.ColorField(UserColors[i], GUILayout.Width(ButtonWidth));

				if (GUI.changed)
					pb_PreferencesInternal.SetString(pb_Constant.pbVertexColorPrefs + i, UserColors[i].ToString());

				GUILayout.EndVertical();

				if (i == UserColors.Length - 1)
					GUILayout.FlexibleSpace();
			}

			GUI.color = Color.white;

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			EditorGUILayout.EndScrollView();
		}

		void OnSceneGUI(SceneView scnview)
		{
			if (!enabled) // || (EditorWindow.focusedWindow != scnview && !lockhandleToCenter))
				return;

			if (editor && editor.editLevel != EditLevel.Plugin)
				editor.SetEditLevel(EditLevel.Plugin);

			// #if UNITY_5
			// 		if( Lightmapping.giWorkflowMode == Lightmapping.GIWorkflowMode.Iterative )
			// 		{
			// 			pb_Lightmapping.PushGIWorkflowMode();
			// 			Lightmapping.Cancel();
			// 			Debug.LogWarning("Vertex Painter requires Continuous Baking to be Off.  When you close the Vertex Painter tool, Continuous Baking will returned to it's previous state automatically.\nIf you toggle Continuous Baking On while the Vertex Painter is open, you may lose all mesh vertex colors.");
			// 		}
			// #endif

			currentEvent = Event.current;
			sceneCamera = scnview.camera;

			screenCenter.x = Screen.width / 2f;
			screenCenter.y = Screen.height / 2f;

			mouseMoveEvent = currentEvent.type == EventType.MouseMove;

			/**
			 * Check if a new object is under the mouse.
			 */
			if (mouseMoveEvent)
			{
				GameObject go = HandleUtility.PickGameObject(Event.current.mousePosition, false);

				if (go != null && (pb == null || go != pb.gameObject))
				{
					pb = go.GetComponent<pb_Object>();

					if (pb != null)
					{
						textures = GetTextures(pb.transform.GetComponent<MeshRenderer>().sharedMaterial).ToArray();
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
			if (!lockhandleToCenter && !pb_EditorHandleUtility.SceneViewInUse(currentEvent))
			{
				if (pb != null)
				{
					if (!hovering.ContainsKey(pb))
					{
						hovering.Add(pb, pb.colors ?? new Color[pb.vertexCount]);
					}
					else
					{
						if (pb.msh.vertexCount != pb.vertexCount)
						{
							// script reload can make this happen
							pb.ToMesh();
							pb.Refresh();
						}

						pb.msh.colors = hovering[pb];
					}

					Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
					pb_RaycastHit hit;

					if (pb_HandleUtility.FaceRaycast(ray, pb, out hit))
					{
						handlePosition = pb.transform.TransformPoint(hit.point);
						handleDistance = Vector3.Distance(handlePosition, sceneCamera.transform.position);
						handleRotation = Quaternion.LookRotation(nonzero(pb.transform.TransformDirection(hit.normal)), Vector3.up);

						Color[] colors = pb.msh.colors;

						int[][] sharedIndices = pb.sharedIndices.ToArray();

						// wrapped in try/catch because a script reload can cause the mesh
						// to re-unwrap itself in some crazy configuration, throwing off the
						// vertex count sync.
						try
						{
							for (int i = 0; i < sharedIndices.Length; i++)
							{
								float dist = Vector3.Distance(hit.point, pb.vertices[sharedIndices[i][0]]);

								if (dist < brushSize)
								{
									for (int n = 0; n < sharedIndices[i].Length; n++)
									{
										colors[sharedIndices[i][n]] = Lerp(hovering[pb][sharedIndices[i][n]], color,
											(1f - (dist / brushSize)) * brushOpacity, colorMask);
									}
								}
							}
						}
						catch
						{
							/* shhhhh */
						}

						// show a preview
						pb.msh.colors = colors;
					}
					else
					{
						// Clear
						foreach (KeyValuePair<pb_Object, Color[]> kvp in hovering)
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
				foreach (KeyValuePair<pb_Object, Color[]> kvp in hovering)
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
			pb_Handles.CircleCap(0, handlePosition, handleRotation, brushSize * .2f);
			Handles.color = MiddleRingColor;
			pb_Handles.CircleCap(0, handlePosition, handleRotation, brushSize * .5f);
			Handles.color = OuterRingColor;
			pb_Handles.CircleCap(0, handlePosition, handleRotation, brushSize);
			Handles.color = Color.white;

			// This prevents us from selecting other objects in the scene,
			// and allows for the selection of faces / vertices.
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			HandleUtility.AddDefaultControl(controlID);

			/**
			 * Apply colors to mesh
			 */
			if ((currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag) &&
			    (currentEvent.button == MOUSE_BUTTON_LEFT) &&
			    currentEvent.modifiers == (EventModifiers) 0 &&
			    ((CurTime - lastBrushApplication) > 1f / (brushStrength * BRUSH_STRENGTH_MAX)))
			{
				lastBrushApplication = CurTime;

				Dictionary<pb_Object, Color[]> sticky = new Dictionary<pb_Object, Color[]>();

				if (!isPainting)
				{
					Undo.RegisterCompleteObjectUndo(hovering.Keys.ToArray(), "Apply Vertex Colors");
					isPainting = true;
				}


				// Apply colors
				foreach (KeyValuePair<pb_Object, Color[]> kvp in hovering)
				{
					Color[] colors = kvp.Key.msh.colors;

					sticky.Add(kvp.Key, colors);

					kvp.Key.SetColors(colors);
				}

				hovering = sticky;
			}

			if (currentEvent.control && currentEvent.type == EventType.ScrollWheel)
			{
				currentEvent.Use();
				brushSize += (currentEvent.delta.y > 0f ? -1f : 1f) * (brushSize * .1f);
				brushSize = Mathf.Clamp(brushSize, .01f, BRUSH_SIZE_MAX);

				Repaint();
			}

			if (currentEvent.type == EventType.MouseUp)
				isPainting = false;

			if (mpos != currentEvent.mousePosition && currentEvent.type == EventType.Repaint)
			{
				mpos = currentEvent.mousePosition;
				SceneView.RepaintAll();
			}
		}

		struct ColorMask
		{
			public bool r;
			public bool g;
			public bool b;
			public bool a;

			public ColorMask(bool r, bool g, bool b, bool a)
			{
				this.r = r;
				this.g = g;
				this.b = b;
				this.a = a;
			}
		}

		static Color Lerp(Color lhs, Color rhs, float alpha, ColorMask mask)
		{
			return new Color(mask.r ? (lhs.r * (1f - alpha) + rhs.r * alpha) : lhs.r,
				mask.g ? (lhs.g * (1f - alpha) + rhs.g * alpha) : lhs.g,
				mask.b ? (lhs.b * (1f - alpha) + rhs.b * alpha) : lhs.b,
				mask.a ? (lhs.a * (1f - alpha) + rhs.a * alpha) : lhs.a);
		}

		private static List<Texture> GetTextures(Material mat)
		{
			List<Texture> textures = new List<Texture>();

			for (int i = 0; i < ShaderUtil.GetPropertyCount(mat.shader); i++)
			{
				if (ShaderUtil.GetPropertyType(mat.shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
				{
					Texture t = mat.GetTexture(ShaderUtil.GetPropertyName(mat.shader, i));

					if (t != null)
						textures.Add(t);
				}
			}

			return textures;
		}

		int GetIndex(Color col)
		{
			if (col.r > col.g && col.r > col.b && col.r > col.a)
				return 0;
			else if (col.g > col.r && col.g > col.b && col.g > col.a)
				return 1;
			else if (col.b > col.r && col.b > col.g && col.b > col.a)
				return 2;
			else if (col.a > col.r && col.a > col.g && col.a > col.b)
				return 3;

			return 0;
		}
	}
}
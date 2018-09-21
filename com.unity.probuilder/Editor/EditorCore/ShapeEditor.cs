using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.UI;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Shape creation panel implementation.
	/// </summary>
	sealed class ShapeEditor : ConfigurableWindow
	{
		enum ShapeType
		{
			Cube,
			Stair,
			Prism,
			Cylinder,
			Plane,
			Door,
			Pipe,
			Cone,
			Sprite,
			Arch,
			Icosahedron,
			Torus,
			Custom
		}

		public static void MenuOpenShapeCreator()
		{
			GetWindow<ShapeEditor>("Shape Tool");
		}

		static readonly Color k_ColorGreen = new Color(0f, .8f, 0f, .8f);

		[SerializeField]
		ShapeType m_CurrentShape = ShapeType.Cube;

		GameObject m_PreviewObject;
		bool m_ShowPreview = true;

		// used to toggle preview on and off from class OnGUI
		bool m_DoInitPreview = false;
		Material m_DefaultMaterial = null;
		Vector2 m_Scroll = Vector2.zero;
		static readonly Color k_PreviewColor = new Color(.5f, .9f, 1f, .56f);
		Material m_ShapePreviewMaterial;

		[UserSetting("Toolbar", "Close Shape Window after Build", "If true the shape window will close after hitting the build button.")]
		static Pref<bool> s_CloseWindowAfterCreateShape = new Pref<bool>("closeWindowAfterShapeCreation", false);

		void OnEnable()
		{
			m_DefaultMaterial = EditorUtility.GetUserMaterial();
			m_DoInitPreview = true;

			if (m_ShapePreviewMaterial == null)
			{
				m_ShapePreviewMaterial = new Material(BuiltinMaterials.defaultMaterial.shader);
				m_ShapePreviewMaterial.hideFlags = HideFlags.HideAndDontSave;

				if (m_ShapePreviewMaterial.HasProperty("_MainTex"))
					m_ShapePreviewMaterial.mainTexture = (Texture2D)Resources.Load("Textures/GridBox_Default");

				if (m_ShapePreviewMaterial.HasProperty("_Color"))
					m_ShapePreviewMaterial.SetColor("_Color", k_PreviewColor);
			}
		}

		void OnDisable()
		{
			DestroyImmediate(m_ShapePreviewMaterial);
		}

		void OnDestroy()
		{
			DestroyPreviewObject();
		}

		[MenuItem("GameObject/3D Object/" + PreferenceKeys.pluginTitle + " Cube _%k")]
		public static void MenuCreateCube()
		{
			ProBuilderMesh pb = ShapeGenerator.GenerateCube(Vector3.one);
			UndoUtility.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

			Material mat = EditorUtility.GetUserMaterial();
			SetFaceMaterial(pb.facesInternal, mat);
			pb.GetComponent<MeshRenderer>().sharedMaterial = mat;

			EditorUtility.InitObject(pb);
		}

		void OnGUI()
		{
			DoContextMenu();

			GUILayout.BeginHorizontal();
				bool sp = m_ShowPreview;
				m_ShowPreview = GUILayout.Toggle(m_ShowPreview, "Show Preview");

				if(sp != m_ShowPreview)
				{
					if(m_ShowPreview)
						m_DoInitPreview = true;
					else
						DestroyPreviewObject();
				}

				if(GUILayout.Button("Center Preview"))
				{
					if(m_PreviewObject == null) return;

					EditorUtility.ScreenCenter(m_PreviewObject.gameObject);
					Selection.activeTransform = m_PreviewObject.transform;
					Selection.activeObject = m_PreviewObject;
					RegisterPreviewObjectTransform();

					SceneView.RepaintAll();
				}
			GUILayout.EndHorizontal();

			GUILayout.Space(7);

			GUILayout.Label("Shape Selector", EditorStyles.boldLabel);

			ShapeType oldShape = m_CurrentShape;
			m_CurrentShape = (ShapeType) EditorGUILayout.EnumPopup(m_CurrentShape);

			if(m_CurrentShape != oldShape) m_DoInitPreview = true;

			m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

			switch(m_CurrentShape)
			{
				case ShapeType.Cube:
					CubeGUI();
					break;
				case ShapeType.Prism:
					PrismGUI();
					break;
				case ShapeType.Stair:
					StairGUI();
					break;
				case ShapeType.Cylinder:
					CylinderGUI();
					break;
				case ShapeType.Plane:
					PlaneGUI();
					break;
				case ShapeType.Door:
					DoorGUI();
					break;
				case ShapeType.Pipe:
					PipeGUI();
					break;
				case ShapeType.Cone:
					ConeGUI();
					break;
				case ShapeType.Sprite:
					SpriteGUI();
					break;
				case ShapeType.Arch:
					ArchGUI();
					break;
				case ShapeType.Icosahedron:
					IcosahedronGUI();
					break;
				case ShapeType.Torus:
					TorusGUI();
					break;
				case ShapeType.Custom:
					CustomGUI();
					break;

				default:
					EditorGUILayout.EndScrollView();
					return;
			}

			// if( !pb_Preferences_Internal.GetBool(pb_Constant.pbShapeWindowFloating) )
			GUILayout.FlexibleSpace();
		}

		/**
		 *	\brief Creates a cube.
		 *	\returns The cube.
		 */
		static Vector3 cubeSize = Vector3.one;

		void CubeGUI()
		{
			cubeSize = EditorGUILayout.Vector3Field("Dimensions", cubeSize);

			if(cubeSize.x <= 0) cubeSize.x = .01f;
			if(cubeSize.y <= 0) cubeSize.y = .01f;
			if(cubeSize.z <= 0) cubeSize.z = .01f;

			if( m_ShowPreview && (GUI.changed || m_DoInitPreview) ) SetPreviewObject(ShapeGenerator.GenerateCube(cubeSize));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = k_ColorGreen;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + m_CurrentShape, GUILayout.MinHeight(28)))
			{
				ProBuilderMesh pb = ShapeGenerator.GenerateCube(cubeSize);
				UndoUtility.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				if( m_DefaultMaterial ) SetFaceMaterial(pb.facesInternal, m_DefaultMaterial );

				EditorUtility.InitObject(pb);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				m_ShowPreview = false;

				if(s_CloseWindowAfterCreateShape)
				{
					this.Close();
				}
			}

			GUI.backgroundColor = oldColor;
		}

		/**
		 *	\brief Creates a sprite.
		 *	\returns The sprite.
		 */
		void SpriteGUI()
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Initial Orientation");
			plane_axis = (Axis)EditorGUILayout.EnumPopup(plane_axis);
			GUILayout.EndHorizontal();

			if( m_ShowPreview && (GUI.changed || m_DoInitPreview) )
				SetPreviewObject(
					 ShapeGenerator.GeneratePlane(
					 	1,
					 	1,
					 	0,
					 	0,
					 	plane_axis));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = k_ColorGreen;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + m_CurrentShape, GUILayout.MinHeight(28)))
			{
				ProBuilderMesh pb = ShapeGenerator.GeneratePlane(
					 	1,
					 	1,
					 	0,
					 	0,
					 	plane_axis);

				UndoUtility.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				if( m_DefaultMaterial ) SetFaceMaterial(pb.facesInternal, m_DefaultMaterial );

				EditorUtility.InitObject(pb);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				m_ShowPreview = false;

				if (s_CloseWindowAfterCreateShape)
				{
					this.Close();
				}
			}
			GUI.backgroundColor = oldColor;
		}

		/**
		 *	\brief Creates a prism.
		 *	...that's it.
		 *	\returns The prism.
		 */
		static Vector3 prismSize = Vector3.one;
		void PrismGUI()
		{
			prismSize = EditorGUILayout.Vector3Field("Dimensions", prismSize);

			if(prismSize.x < 0) prismSize.x = 0.01f;
			if(prismSize.y < 0) prismSize.y = 0.01f;
			if(prismSize.z < 0) prismSize.z = 0.01f;

			if( m_ShowPreview && (GUI.changed || m_DoInitPreview) ) SetPreviewObject(ShapeGenerator.GeneratePrism(prismSize));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = k_ColorGreen;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + m_CurrentShape, GUILayout.MinHeight(28)))
			{
				ProBuilderMesh pb = ShapeGenerator.GeneratePrism(prismSize);

				UndoUtility.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				if( m_DefaultMaterial ) SetFaceMaterial(pb.facesInternal, m_DefaultMaterial );

				EditorUtility.InitObject(pb);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				m_ShowPreview = false;

				if (s_CloseWindowAfterCreateShape)
				{
					this.Close();
				}
			}

			GUI.backgroundColor = oldColor;
		}

		/**** Stair Generator ***/
		static int stair_steps = 6;
		static Vector3 stair_size = new Vector3(2f, 2.5f, 4f);
		static float stair_cirumference = 0f;
		static bool stair_sides = true;
		static bool stair_mirror = false;

		void StairGUI()
		{
			EditorGUI.BeginChangeCheck();

			stair_steps = (int) Mathf.Max(UI.EditorGUIUtility.FreeSlider("Steps", stair_steps, 2, 64), 2);

			stair_sides = EditorGUILayout.Toggle("Build Sides", stair_sides);

			stair_cirumference = EditorGUILayout.Slider("Curvature", stair_cirumference, 0f, 360f);

			if(stair_cirumference > 0f)
			{
				stair_mirror = EditorGUILayout.Toggle("Mirror", stair_mirror);

				stair_size.x = Mathf.Max(UI.EditorGUIUtility.FreeSlider(new GUIContent("Stair Width", "The width of an individual stair step."), stair_size.x, .01f, 10f), .01f);
				stair_size.y = Mathf.Max(UI.EditorGUIUtility.FreeSlider(new GUIContent("Stair Height", "The total height of this staircase.  You may enter any value in the float field."), stair_size.y, .01f, 10f), .01f);
				stair_size.z = Mathf.Max(UI.EditorGUIUtility.FreeSlider(new GUIContent("Inner Radius", "The distance from the center that stairs begin."), stair_size.z, 0f, 10f), 0f);
			}
			else
			{
				stair_size = EditorGUILayout.Vector3Field("Width, Height, Depth", stair_size);

				stair_size.x = UI.EditorGUIUtility.FreeSlider("Width", stair_size.x, 0.01f, 10f);
				stair_size.y = UI.EditorGUIUtility.FreeSlider("Height", stair_size.y, 0.01f, 10f);
				stair_size.z = UI.EditorGUIUtility.FreeSlider("Depth", stair_size.z, 0.01f, 10f);
			}

			if( m_ShowPreview && (EditorGUI.EndChangeCheck() || m_DoInitPreview) )
			{
				if(stair_cirumference > 0f)
				{
					SetPreviewObject(ShapeGenerator.GenerateCurvedStair(
						stair_size.x,
						stair_size.y,
						stair_size.z,
						stair_mirror ? -stair_cirumference : stair_cirumference,
						stair_steps,
						stair_sides));
				}
				else
				{
					SetPreviewObject(ShapeGenerator.GenerateStair(
						stair_size,
						stair_steps,
						stair_sides));
				}
			}

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = k_ColorGreen;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + m_CurrentShape, GUILayout.MinHeight(28)))
			{
				ProBuilderMesh pb = stair_cirumference > 0f ?
					ShapeGenerator.GenerateCurvedStair(
						stair_size.x,
						stair_size.y,
						stair_size.z,
						stair_mirror ? -stair_cirumference : stair_cirumference,
						stair_steps,
						stair_sides) :
					ShapeGenerator.GenerateStair(
						stair_size,
						stair_steps,
						stair_sides);

				UndoUtility.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				if( m_DefaultMaterial ) SetFaceMaterial(pb.facesInternal, m_DefaultMaterial );

				EditorUtility.InitObject(pb);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				m_ShowPreview = false;

				if (s_CloseWindowAfterCreateShape)
				{
					this.Close();
				}
			}

			GUI.backgroundColor = oldColor;
		}

		/**** Cylinder Generator ***/
		static int cyl_axisCuts = 8;
		static float cyl_radius = .5f;
		static float cyl_height = 1f;
		static int cyl_heightCuts = 0;
		static bool cyl_smoothing = true;
		void CylinderGUI()
		{
			// Store old values
			cyl_radius = EditorGUILayout.FloatField("Radius", cyl_radius);
			cyl_radius = Mathf.Clamp(cyl_radius, .01f, Mathf.Infinity);

			cyl_axisCuts = EditorGUILayout.IntField("Number of Sides", cyl_axisCuts);
			cyl_axisCuts = Clamp(cyl_axisCuts, 4, 48);

			cyl_height = EditorGUILayout.FloatField("Height", cyl_height);

			cyl_heightCuts = EditorGUILayout.IntField("Height Segments", cyl_heightCuts);
			cyl_heightCuts = Clamp(cyl_heightCuts, 0, 48);

			cyl_smoothing = EditorGUILayout.Toggle("Smooth", cyl_smoothing);

			if(cyl_axisCuts % 2 != 0)
				cyl_axisCuts++;

			if(cyl_heightCuts < 0)
				cyl_heightCuts = 0;

			if( m_ShowPreview && (GUI.changed || m_DoInitPreview) )
			{
				SetPreviewObject(
					ShapeGenerator.GenerateCylinder(
					cyl_axisCuts,
					cyl_radius,
					cyl_height,
					cyl_heightCuts,
					cyl_smoothing ? 1 : -1),
					new int[1] { (cyl_axisCuts*(cyl_heightCuts+1)*4)+1 });
			}

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = k_ColorGreen;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + m_CurrentShape, GUILayout.MinHeight(28)))
			{
				ProBuilderMesh pb = ShapeGenerator.GenerateCylinder(cyl_axisCuts, cyl_radius, cyl_height, cyl_heightCuts, cyl_smoothing ? 1 : -1);
				UndoUtility.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				int centerIndex = (cyl_axisCuts*(cyl_heightCuts+1)*4)+1;

				if( m_DefaultMaterial ) SetFaceMaterial(pb.facesInternal, m_DefaultMaterial );

				EditorUtility.InitObject(pb);

				AlignWithPreviewObject(pb.gameObject);

				DestroyPreviewObject();
				m_ShowPreview = false;

				if (s_CloseWindowAfterCreateShape)
				{
					this.Close();
				}
			}

			GUI.backgroundColor = oldColor;
		}

		/**** Door Generator ***/
		static float door_totalWidth  = 4.0f;
		static float door_totalHeight = 4.0f;
		static float door_ledgeHeight = 1.0f;
		static float door_legWidth	  = 1.0f;
		static float door_depth		  = 0.5f;
		void DoorGUI()
		{

			door_totalWidth = EditorGUILayout.FloatField("Total Width", door_totalWidth);
			door_totalWidth = Mathf.Clamp(door_totalWidth, 1.0f, 500.0f);

			door_totalHeight = EditorGUILayout.FloatField("Total Height", door_totalHeight);
			door_totalHeight = Mathf.Clamp(door_totalHeight, 1.0f, 500.0f);

			door_depth = EditorGUILayout.FloatField("Total Depth", door_depth);
			door_depth = Mathf.Clamp(door_depth, 0.01f, 500.0f);

			door_ledgeHeight = EditorGUILayout.FloatField("Door Height", door_ledgeHeight);
			door_ledgeHeight = Mathf.Clamp(door_ledgeHeight, 0.01f, 500.0f);

			door_legWidth = EditorGUILayout.FloatField("Leg Width", door_legWidth);
			door_legWidth = Mathf.Clamp(door_legWidth, 0.01f, 2.0f);

			if (m_ShowPreview && (GUI.changed || m_DoInitPreview))
				SetPreviewObject(ShapeGenerator.GenerateDoor(door_totalWidth, door_totalHeight, door_ledgeHeight, door_legWidth, door_depth));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = k_ColorGreen;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + m_CurrentShape, GUILayout.MinHeight(28)))
			{
				ProBuilderMesh pb = ShapeGenerator.GenerateDoor(door_totalWidth, door_totalHeight, door_ledgeHeight, door_legWidth, door_depth);
				UndoUtility.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				if( m_DefaultMaterial ) SetFaceMaterial(pb.facesInternal, m_DefaultMaterial );

				EditorUtility.InitObject(pb);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				m_ShowPreview = false;

				if (s_CloseWindowAfterCreateShape)
				{
					this.Close();
				}
			}

			GUI.backgroundColor = oldColor;
		}

		static float plane_height = 10, plane_width = 10;
		static int plane_height_cuts = 3, plane_width_cuts = 3;
		static Axis plane_axis = Axis.Up;

		void PlaneGUI()
		{
			plane_axis = (Axis)EditorGUILayout.EnumPopup("Initial Orientation", plane_axis);

			plane_width = EditorGUILayout.FloatField("Width", plane_width);
			plane_height = EditorGUILayout.FloatField("Length", plane_height);

			if(plane_height < 1f)
				plane_height = 1f;

			if(plane_width < 1f)
				plane_width = 1f;

			plane_width_cuts= EditorGUILayout.IntField("Width Segments", plane_width_cuts);
			plane_height_cuts = EditorGUILayout.IntField("Length Segments", plane_height_cuts);

			if(plane_width_cuts < 0)
				plane_width_cuts = 0;

			if(plane_height_cuts < 0)
				plane_height_cuts = 0;

			if( m_ShowPreview && (GUI.changed || m_DoInitPreview) )
				SetPreviewObject(
					 ShapeGenerator.GeneratePlane(
					 	plane_height,
					 	plane_width,
					 	plane_height_cuts,
					 	plane_width_cuts,
					 	plane_axis));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = k_ColorGreen;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + m_CurrentShape, GUILayout.MinHeight(28)))
			{
				ProBuilderMesh pb = ShapeGenerator.GeneratePlane(plane_height, plane_width, plane_height_cuts, plane_width_cuts, plane_axis);
				UndoUtility.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				if( m_DefaultMaterial ) SetFaceMaterial(pb.facesInternal, m_DefaultMaterial );

				EditorUtility.InitObject(pb);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				m_ShowPreview = false;

				if (s_CloseWindowAfterCreateShape)
				{
					this.Close();
				}
			}

			GUI.backgroundColor = oldColor;
		}

		static float pipe_radius = 1f;
		static float pipe_height = 2f;
		static float pipe_thickness = .2f;
		static int pipe_subdivAxis = 6;
		static int pipe_subdivHeight = 1;
		void PipeGUI()
		{
			pipe_radius = EditorGUILayout.FloatField("Radius", pipe_radius);
			pipe_height = EditorGUILayout.FloatField("Height", pipe_height);
			pipe_thickness = EditorGUILayout.FloatField("Thickness", pipe_thickness);
			pipe_subdivAxis = EditorGUILayout.IntField("Number of Sides", pipe_subdivAxis);
			pipe_subdivHeight = EditorGUILayout.IntField("Height Segments", pipe_subdivHeight);

			if(pipe_radius < .1f)
				pipe_radius = .1f;

			if(pipe_height < .1f)
				pipe_height = .1f;

			pipe_subdivHeight = (int)Mathf.Clamp(pipe_subdivHeight, 0f, 32f);
			pipe_thickness = Mathf.Clamp(pipe_thickness, .01f, pipe_radius-.01f);
			pipe_subdivAxis = (int)Mathf.Clamp(pipe_subdivAxis, 3f, 32f);

			if( m_ShowPreview && (GUI.changed || m_DoInitPreview) )
				SetPreviewObject(
					 ShapeGenerator.GeneratePipe(
					 	pipe_radius,
						pipe_height,
						pipe_thickness,
						pipe_subdivAxis,
						pipe_subdivHeight
					 	));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = k_ColorGreen;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + m_CurrentShape, GUILayout.MinHeight(28)))
			{
				ProBuilderMesh pb = ShapeGenerator.GeneratePipe(
					 	pipe_radius,
						pipe_height,
						pipe_thickness,
						pipe_subdivAxis,
						pipe_subdivHeight
					 	);
				UndoUtility.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				if( m_DefaultMaterial ) SetFaceMaterial(pb.facesInternal, m_DefaultMaterial );

				EditorUtility.InitObject(pb);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				m_ShowPreview = false;

				if (s_CloseWindowAfterCreateShape)
				{
					this.Close();
				}
			}

			GUI.backgroundColor = oldColor;
		}

		static float 	cone_radius = 1f;
		static float 	cone_height = 2f;
		static int 		cone_subdivAxis = 6;
		void ConeGUI()
		{
			cone_radius = EditorGUILayout.FloatField("Radius", cone_radius);
			cone_height = EditorGUILayout.FloatField("Height", cone_height);
			cone_subdivAxis = EditorGUILayout.IntField("Number of Sides", cone_subdivAxis);

			if(cone_radius < .1f)
				cone_radius = .1f;

			if(cone_height < .1f)
				cone_height = .1f;

			pipe_subdivHeight = (int)Mathf.Clamp(pipe_subdivHeight, 1f, 32f);
			pipe_thickness = Mathf.Clamp(pipe_thickness, .01f, cone_radius-.01f);
			cone_subdivAxis = (int)Mathf.Clamp(cone_subdivAxis, 3f, 32f);

			if( m_ShowPreview && (GUI.changed || m_DoInitPreview) )
				SetPreviewObject(
					 ShapeGenerator.GenerateCone(
					 	cone_radius,
						cone_height,
						cone_subdivAxis
					 	));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = k_ColorGreen;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + m_CurrentShape, GUILayout.MinHeight(28)))
			{
				ProBuilderMesh pb = ShapeGenerator.GenerateCone(
					 	cone_radius,
						cone_height,
						cone_subdivAxis
					 	);
				UndoUtility.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				if( m_DefaultMaterial ) SetFaceMaterial(pb.facesInternal, m_DefaultMaterial );

				EditorUtility.InitObject(pb);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				m_ShowPreview = false;

				if (s_CloseWindowAfterCreateShape)
				{
					this.Close();
				}
			}

			GUI.backgroundColor = oldColor;

		}

		/**** Arch Generator ***/
		static float arch_angle		= 180.0f;
		static float arch_radius	= 3.0f;
		static float arch_width		= 0.50f;
		static float arch_depth		= 1f;
		static int arch_radialCuts	= 6;
		static bool arch_insideFaces 	= true;	///< Generate inside faces of arch?
		static bool arch_outsideFaces 	= true;	///< Generate outside faces of arch?
		static bool arch_frontFaces 	= true;	///< Generate front faces of arch?
		static bool arch_backFaces 		= true;	///< Generate back faces of arch?
		static bool arch_endCaps 		= true;	///< Generate end cap faces of arch?

		void ArchGUI()
		{
			arch_radius = EditorGUILayout.FloatField("Radius", arch_radius);
			arch_radius = arch_radius <= 0f ? .01f : arch_radius;

			arch_width = EditorGUILayout.FloatField("Thickness", arch_width);
			arch_width = Mathf.Clamp(arch_width, 0.01f, 100f);

			arch_depth = EditorGUILayout.FloatField("Depth", arch_depth);
			arch_depth = Mathf.Clamp(arch_depth, 0.1f, 500.0f);

			arch_radialCuts = EditorGUILayout.IntField("Number of Sides", arch_radialCuts);
			arch_radialCuts = Mathf.Clamp(arch_radialCuts, 2, 200);

			arch_angle = EditorGUILayout.FloatField("Arch Degrees", arch_angle);
			arch_angle = Mathf.Clamp(arch_angle, 0.0f, 360.0f);

			// arch_insideFaces = EditorGUILayout.Toggle("Inner Faces", arch_insideFaces);
			// arch_outsideFaces = EditorGUILayout.Toggle("Outer Faces", arch_outsideFaces);
			// arch_frontFaces = EditorGUILayout.Toggle("Front Faces", arch_frontFaces);
			// arch_backFaces = EditorGUILayout.Toggle("Rear Faces", arch_backFaces);

			if(arch_angle < 360f)
				arch_endCaps = EditorGUILayout.Toggle("End Caps", arch_endCaps);

			if(arch_angle > 180f)
				arch_radialCuts = System.Math.Max(3, arch_radialCuts);

		  	if (m_ShowPreview && (GUI.changed || m_DoInitPreview))
				SetPreviewObject( ShapeGenerator.GenerateArch(	arch_angle,
																	arch_radius,
																	Mathf.Clamp(arch_width, 0.01f, arch_radius),
																	arch_depth,
																	arch_radialCuts + 1,
																	arch_insideFaces,
																	arch_outsideFaces,
																	arch_frontFaces,
																	arch_backFaces,
																	arch_endCaps));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = k_ColorGreen;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + m_CurrentShape, GUILayout.MinHeight(28)))
			{
				ProBuilderMesh pb = ShapeGenerator.GenerateArch(
					arch_angle,
					arch_radius,
					Mathf.Clamp(arch_width, 0.01f, arch_radius),
					arch_depth,
					arch_radialCuts + 1,
					arch_insideFaces,
					arch_outsideFaces,
					arch_frontFaces,
					arch_backFaces,
					arch_endCaps);

				pb.RemoveDegenerateTriangles();

				UndoUtility.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				if (m_DefaultMaterial) SetFaceMaterial(pb.facesInternal,m_DefaultMaterial);

				EditorUtility.InitObject(pb);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				m_ShowPreview = false;

				if (s_CloseWindowAfterCreateShape)
				{
					this.Close();
				}
			}

			GUI.backgroundColor = oldColor;
		}

		static float ico_radius = 1f;
		static int ico_subdivisions = 1;

		void IcosahedronGUI()
		{
			float t_ico_radius = ico_radius;
			int t_ico_subdivisions = ico_subdivisions;

			ico_radius = EditorGUILayout.Slider("Radius", ico_radius, 0.01f, 10f);

			ico_subdivisions = (int) EditorGUILayout.Slider("Subdivisions", ico_subdivisions, 0, 4);

			if (m_ShowPreview && ((t_ico_subdivisions != ico_subdivisions || t_ico_radius != ico_radius) || m_DoInitPreview))
				SetPreviewObject(ShapeGenerator.GenerateIcosahedron(ico_radius, ico_subdivisions, false));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = k_ColorGreen;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + m_CurrentShape, GUILayout.MinHeight(28)))
			{
				ProBuilderMesh pb = ShapeGenerator.GenerateIcosahedron(ico_radius, ico_subdivisions);
				UndoUtility.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				// To keep the preview snappy, shared indexes aren't built in IcosahadreonGenerator
				UVEditing.ProjectFacesBox(pb, pb.facesInternal);

				for(int i = 0; i < pb.facesInternal.Length; i++)
					pb.facesInternal[i].manualUV = true;

				if (m_DefaultMaterial) SetFaceMaterial(pb.facesInternal,m_DefaultMaterial);

				EditorUtility.InitObject(pb);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				m_ShowPreview = false;

				if (s_CloseWindowAfterCreateShape)
				{
					this.Close();
				}
			}

			GUI.backgroundColor = oldColor;

		}

		static float torus_radius = 1f;
		static float torus_tubeRadius = .3f;
		static int torus_rows = 16;
		static int torus_colums = 24;
		static bool torus_smooth = true;
		static float torus_horizontalCircumference = 360f;
		static float torus_verticalCircumference = 360f;
		static Vector2 torus_innerOuter = new Vector2(1f, .7f);
		static Pref<bool> torus_useInnerOuterMethod = new Pref<bool>("shape.torusDefinesInnerOuter", false, Settings.Scope.User);

		void TorusGUI()
		{
			EditorGUI.BeginChangeCheck();

			torus_rows = (int) EditorGUILayout.IntSlider(new GUIContent("Rows", "How many rows the torus will have.  More equates to smoother geometry."), torus_rows, 3, 32);
			torus_colums = (int) EditorGUILayout.IntSlider(new GUIContent("Columns", "How many columns the torus will have.  More equates to smoother geometry."), torus_colums, 3, 64);

			torus_useInnerOuterMethod.value = EditorGUILayout.Toggle("Define Inner / Out Radius", torus_useInnerOuterMethod);

			if(!torus_useInnerOuterMethod)
			{
				torus_radius = EditorGUILayout.FloatField("Radius", torus_radius);

				if(torus_radius < .001f)
					torus_radius = .001f;

				torus_tubeRadius = UI.EditorGUIUtility.Slider(new GUIContent("Tube Radius", "How thick the donut will be."), torus_tubeRadius, .01f, torus_radius);
			}
			else
			{
				torus_innerOuter.x = torus_radius;
				torus_innerOuter.y = torus_radius - (torus_tubeRadius * 2f);

				torus_innerOuter.x = EditorGUILayout.FloatField("Outer Radius", torus_innerOuter.x);
				torus_innerOuter.y = UI.EditorGUIUtility.Slider(new GUIContent("Inner Radius", "Distance from center to inside of donut ring."), torus_innerOuter.y, .001f, torus_innerOuter.x);

				torus_radius = torus_innerOuter.x;
				torus_tubeRadius = (torus_innerOuter.x - torus_innerOuter.y) * .5f;
			}

			torus_horizontalCircumference = EditorGUILayout.Slider("Horizontal Circumference", torus_horizontalCircumference, .01f, 360f);
			torus_verticalCircumference = EditorGUILayout.Slider("Vertical Circumference", torus_verticalCircumference, .01f, 360f);

			torus_smooth = EditorGUILayout.Toggle("Smooth", torus_smooth);

			if (m_ShowPreview && (EditorGUI.EndChangeCheck() || m_DoInitPreview))
				SetPreviewObject(ShapeGenerator.GenerateTorus(
					torus_rows,
					torus_colums,
					torus_radius,
					torus_tubeRadius,
					torus_smooth,
					torus_horizontalCircumference,
					torus_verticalCircumference,
					true));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = k_ColorGreen;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + m_CurrentShape, GUILayout.MinHeight(28)))
			{
				ProBuilderMesh pb = ShapeGenerator.GenerateTorus(
					torus_rows,
					torus_colums,
					torus_radius,
					torus_tubeRadius,
					torus_smooth,
					torus_horizontalCircumference,
					torus_verticalCircumference,
					true);
				UndoUtility.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				UVEditing.ProjectFacesBox(pb, pb.facesInternal);

				if (m_DefaultMaterial) SetFaceMaterial(pb.facesInternal,m_DefaultMaterial);

				EditorUtility.InitObject(pb);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				m_ShowPreview = false;

				if (s_CloseWindowAfterCreateShape)
				{
					this.Close();
				}
			}

			GUI.backgroundColor = oldColor;
		}

		static string verts = "//Vertical Plane\n0, 0, 0\n1, 0, 0\n0, 1, 0\n1, 1, 0\n";
		static Vector2 scrollbar = new Vector2(0f, 0f);
		void CustomGUI()
		{
			GUILayout.Label("Custom Geometry", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox("Vertices must be wound in faces, and counter-clockwise.\n(Think horizontally reversed Z)", MessageType.Info);

			scrollbar = GUILayout.BeginScrollView(scrollbar);
				verts = EditorGUILayout.TextArea(verts, GUILayout.MinHeight(160));
			GUILayout.EndScrollView();

			if( m_ShowPreview && (GUI.changed || m_DoInitPreview) )
			{
				Vector3[] v = InternalUtility.StringToVector3Array(verts);
				if(v.Length % 4 == 0)
					SetPreviewObject(ProBuilderMesh.CreateInstanceWithPoints(v));
			}

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = k_ColorGreen;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + m_CurrentShape, GUILayout.MinHeight(28)))
			{
				ProBuilderMesh pb = ProBuilderMesh.CreateInstanceWithPoints(InternalUtility.StringToVector3Array(verts));
				UndoUtility.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				if( m_DefaultMaterial ) SetFaceMaterial(pb.facesInternal, m_DefaultMaterial );

				EditorUtility.InitObject(pb);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				m_ShowPreview = false;

				if (s_CloseWindowAfterCreateShape)
				{
					this.Close();
				}
			}

			GUI.backgroundColor = oldColor;
		}

		static int Clamp(int val, int min, int max)
		{
			if(val > max) val = max;
			if(val < min) val = min;
			return val;
		}

		public void DestroyPreviewObject()
		{
			if(m_PreviewObject != null)
			{
				if(m_PreviewObject.GetComponent<MeshFilter>().sharedMesh != null)
					DestroyImmediate(m_PreviewObject.GetComponent<MeshFilter>().sharedMesh);

				DestroyImmediate(m_PreviewObject);
			}
		}

		void SetPreviewObject(ProBuilderMesh pb, int[] vertices = null)
		{
			pb.selectable = false;

			m_DoInitPreview = false;
			bool prevTransform = false;

			if(m_PreviewObject != null)
			{
				prevTransform = true;
				RegisterPreviewObjectTransform();
			}

			DestroyPreviewObject();

			m_PreviewObject = pb.gameObject;

			if(prevTransform)
			{
				m_PreviewObject.transform.position = m_pos;
				m_PreviewObject.transform.rotation = m_rot;
				m_PreviewObject.transform.localScale = m_scale;
			}
			else
			{
				EditorUtility.ScreenCenter(m_PreviewObject.gameObject);
			}

			EditorUtility.SetPivotLocationAndSnap(pb);

			// Remove pb_Object
			Mesh m = UnityEngine.ProBuilder.MeshUtility.DeepCopy( pb.mesh );

			Object.DestroyImmediate(pb.mesh);
			Object.DestroyImmediate(pb);

			if(m_PreviewObject.GetComponent<Entity>())
				Object.DestroyImmediate(m_PreviewObject.GetComponent<Entity>());

			m.hideFlags = HideFlags.DontSave;
			m_PreviewObject.hideFlags = HideFlags.DontSave;

			m_PreviewObject.GetComponent<MeshFilter>().sharedMesh = m;
			m_PreviewObject.GetComponent<MeshRenderer>().sharedMaterial = m_ShapePreviewMaterial;

			Selection.activeTransform = m_PreviewObject.transform;
		}

		Vector3 m_pos = Vector3.zero;
		Quaternion m_rot = Quaternion.identity;
		Vector3 m_scale = Vector3.zero;

		void RegisterPreviewObjectTransform()
		{
			m_pos 	= m_PreviewObject.transform.position;
			m_rot 	= m_PreviewObject.transform.rotation;
			m_scale = m_PreviewObject.transform.localScale;
		}

		bool PreviewObjectHasMoved()
		{
			if(m_pos != m_PreviewObject.transform.position)
				return true;
			if(m_rot != m_PreviewObject.transform.rotation)
				return true;
			if(m_scale != m_PreviewObject.transform.localScale)
				return true;
			return false;
		}

		void AlignWithPreviewObject(GameObject go)
		{
			if(go == null || m_PreviewObject == null) return;
			go.transform.position 	= m_PreviewObject.transform.position;
			go.transform.rotation 	= m_PreviewObject.transform.rotation;
			go.transform.localScale = m_PreviewObject.transform.localScale;

			ProBuilderMesh pb = go.GetComponent<ProBuilderMesh>();

			pb.FreezeScaleTransform();
			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
		}

		static void SetFaceMaterial(Face[] faces, Material material)
		{
			foreach (var face in faces)
				face.material = material;
		}
	}
}

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.EditorCommon
{
	/**
	 * Shape creation panel implementation.
	 */
	public class pb_Geometry_Interface : EditorWindow
	{	

		public static void MenuOpenShapeCreator()
		{
			EditorWindow.GetWindow(typeof(pb_Geometry_Interface), true, "Shape Tool", true);
		}

		static Color COLOR_GREEN = new Color(0f, .8f, 0f, .8f);
		static Color PREVIEW_COLOR = new Color(.5f, .9f, 1f, .56f);
		public Shape shape = Shape.Cube;

		private GameObject previewObject;
		private bool showPreview = true;
		private Material _prevMat;

		public Material previewMat
		{
			get
			{
				if(_prevMat == null)
				{
					_prevMat = new Material(Shader.Find("Diffuse"));
					_prevMat.mainTexture = (Texture2D)Resources.Load("Textures/GridBox_Default");
					_prevMat.SetColor("_Color", PREVIEW_COLOR);
				}
				return _prevMat;
			}
		}
		private bool initPreview = false; // used to toggle preview on and off from class OnGUI

		private bool prefClose	  // toogle for closing the window after shape creation from the prefrences window
		{
			get
			{
			  	return EditorPrefs.HasKey(pb_Constant.pbCloseShapeWindow) ? EditorPrefs.GetBool(pb_Constant.pbCloseShapeWindow) : false;
			}
		}

		Material userMaterial = null;
		void OnEnable()
		{
			userMaterial = pb_Preferences_Internal.GetMaterial(pb_Constant.pbDefaultMaterial);

			initPreview = true;
		}

		void OnDisable()
		{
			if(previewMat != null)
				DestroyImmediate(previewObject);

			DestroyPreviewObject();
		}

		[MenuItem("GameObject/Create Other/" + pb_Constant.PRODUCT_NAME + " Cube _%k")]
		public static void MenuCreateCube()
		{
			pb_Object pb = pb_Shape_Generator.CubeGenerator(Vector3.one);
			pbUndo.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");
			
			Material mat = null;

			if(EditorPrefs.HasKey(pb_Constant.pbDefaultMaterial))
				mat = (Material)AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString(pb_Constant.pbDefaultMaterial), typeof(Material));

			if(mat != null) pb.SetFaceMaterial(pb.faces, mat);

			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);
			pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);
		}

		Vector2 scroll = Vector2.zero;

		void OnGUI()
		{	
			GUILayout.BeginHorizontal();
				bool sp = showPreview;
				showPreview = GUILayout.Toggle(showPreview, "Show Preview");
				if(sp != showPreview && !showPreview) DestroyPreviewObject();

				if(GUILayout.Button("Center Preview"))
				{
					if(previewObject == null) return;

					pb_Editor_Utility.ScreenCenter(previewObject.gameObject);
					Selection.activeTransform = previewObject.transform;
					Selection.activeObject = previewObject;
					RegisterPreviewObjectTransform();
				}
			GUILayout.EndHorizontal();

			GUILayout.Space(7);

			GUILayout.Label("Shape Selector", EditorStyles.boldLabel);
			
			Shape oldShape = shape;
			shape = (Shape)EditorGUILayout.EnumPopup(shape);
				
			if(shape != oldShape) initPreview = true;

			scroll = EditorGUILayout.BeginScrollView(scroll);
			switch(shape)
			{
				case Shape.Cube:
					CubeGUI();
					break;
				case Shape.Prism:
					PrismGUI();
					break;
				case Shape.Stair:
					StairGUI();
					break;
				case Shape.Cylinder:
					CylinderGUI();
					break;
				case Shape.Plane:
					PlaneGUI();
					break;
				case Shape.Door:
					DoorGUI();
					break;
				case Shape.Pipe:
					PipeGUI();
					break;
				case Shape.Cone:
					ConeGUI();
					break;
				case Shape.Sprite:
					SpriteGUI();
					break;
				case Shape.Arch:
					ArchGUI();
					break;
				case Shape.Icosahedron:
					IcosahedronGUI();
					break;
				case Shape.Custom:
					CustomGUI();
					break;

				default:
					EditorGUILayout.EndScrollView();
					return;
			}

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

			if( showPreview && (GUI.changed || initPreview) ) SetPreviewObject(pb_Shape_Generator.CubeGenerator(cubeSize));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = COLOR_GREEN;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + shape, GUILayout.MinHeight(28)))
			{
				pb_Object pb = pb_Shape_Generator.CubeGenerator(cubeSize);
				pbUndo.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );

				pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);
				pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				showPreview = false;

				if(prefClose)
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

			if( showPreview && (GUI.changed || initPreview) ) 
				SetPreviewObject(
					 pb_Shape_Generator.PlaneGenerator(
					 	1,
					 	1,
					 	0,
					 	0,
					 	plane_axis,
					 	false));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = COLOR_GREEN;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + shape, GUILayout.MinHeight(28)))
			{
				pb_Object pb = pb_Shape_Generator.PlaneGenerator(
					 	1,
					 	1,
					 	0,
					 	0,
					 	plane_axis,
					 	false);

				pbUndo.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );
				
				pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);
				pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				showPreview = false;

				if (prefClose)
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

			if( showPreview && (GUI.changed || initPreview) ) SetPreviewObject(pb_Shape_Generator.PrismGenerator(prismSize));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = COLOR_GREEN;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + shape, GUILayout.MinHeight(28)))
			{
				pb_Object pb = pb_Shape_Generator.PrismGenerator(prismSize);
				
				pbUndo.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );
		
				pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);
				pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				showPreview = false;

				if (prefClose)
				{
					this.Close();
				}
			}

			GUI.backgroundColor = oldColor;
		}

		/**** Stair Generator ***/
		static bool extendSidesToFloor = true;
		static bool generateBack = true;
		static int stair_steps = 6;
		static float stair_width = 4f, stair_height = 5f, stair_depth = 8f;
		static bool stair_platformsOnly = false;
		void StairGUI()
		{
			stair_steps = EditorGUILayout.IntField("Number of Steps", stair_steps);
			stair_steps = Clamp(stair_steps, 2, 50);

			stair_width = EditorGUILayout.FloatField("Width", stair_width);
			stair_width = Mathf.Clamp(stair_width, 0.01f, 500f);

			stair_height = EditorGUILayout.FloatField("Height", stair_height);
			stair_height = Mathf.Clamp(stair_height, .01f, 500f);

			stair_depth = EditorGUILayout.FloatField("Depth", stair_depth);
			stair_depth = Mathf.Clamp(stair_depth, .01f, 500f);

			stair_platformsOnly = EditorGUILayout.Toggle("Platforms Only", stair_platformsOnly);
			if(stair_platformsOnly) { GUI.enabled = false; extendSidesToFloor = false; generateBack = false; }
			extendSidesToFloor = EditorGUILayout.Toggle("Extend sides to floor", extendSidesToFloor);
			generateBack = EditorGUILayout.Toggle("Generate Back", generateBack);
			GUI.enabled = true;

			if( showPreview && (GUI.changed || initPreview) ) 
				SetPreviewObject(pb_Shape_Generator.StairGenerator(
					stair_steps, 
					stair_width,
					stair_height,
					stair_depth,
					extendSidesToFloor,
					generateBack,
					stair_platformsOnly));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = COLOR_GREEN;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + shape, GUILayout.MinHeight(28)))
			{
				pb_Object pb = pb_Shape_Generator.StairGenerator(stair_steps, stair_width, stair_height, stair_depth, extendSidesToFloor, generateBack, stair_platformsOnly);
				pbUndo.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );
				
				pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);
				pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				showPreview = false;
		
				if (prefClose)
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
		static int cyl_heightCuts = 1;
		void CylinderGUI()
		{
			// Store old values	
			cyl_radius = EditorGUILayout.FloatField("Radius", cyl_radius);
			cyl_radius = Mathf.Clamp(cyl_radius, .01f, Mathf.Infinity);

			cyl_axisCuts = EditorGUILayout.IntField("Number of Sides", cyl_axisCuts);
			cyl_axisCuts = Clamp(cyl_axisCuts, 2, 48);

			cyl_height = EditorGUILayout.FloatField("Height", cyl_height);

			cyl_heightCuts = EditorGUILayout.IntField("Height Segments", cyl_heightCuts);
			cyl_heightCuts = Clamp(cyl_heightCuts, 0, 48);

			if(cyl_axisCuts % 2 != 0)
				cyl_axisCuts++;

			if(cyl_heightCuts < 0)
				cyl_heightCuts = 0;

			if( showPreview && (GUI.changed || initPreview) ) 
			{
				SetPreviewObject(
					pb_Shape_Generator.CylinderGenerator(
					cyl_axisCuts,
					cyl_radius,
					cyl_height,
					cyl_heightCuts),
					new int[1] { (cyl_axisCuts*(cyl_heightCuts+1)*4)+1 } );
			}

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = COLOR_GREEN;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + shape, GUILayout.MinHeight(28)))
			{
				pb_Object pb = pb_Shape_Generator.CylinderGenerator(cyl_axisCuts, cyl_radius, cyl_height, cyl_heightCuts);
				pbUndo.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");
				
				int centerIndex = (cyl_axisCuts*(cyl_heightCuts+1)*4)+1;
				
				if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );

				pb_Editor_Utility.SetPivotAndSnapWithPref(pb, new int[1] {centerIndex});
				pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);

				AlignWithPreviewObject(pb.gameObject);
				
				DestroyPreviewObject();
				showPreview = false;
			
				if (prefClose)
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

			if (showPreview && (GUI.changed || initPreview))
				SetPreviewObject(pb_Shape_Generator.DoorGenerator(door_totalWidth, door_totalHeight, door_ledgeHeight, door_legWidth, door_depth));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = COLOR_GREEN;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + shape, GUILayout.MinHeight(28)))
			{
				pb_Object pb = pb_Shape_Generator.DoorGenerator(door_totalWidth, door_totalHeight, door_ledgeHeight, door_legWidth, door_depth);
				pbUndo.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");
				 
				if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );

				pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);
				pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				showPreview = false;
			
				if (prefClose)
				{
					this.Close();
				}
			}

			GUI.backgroundColor = oldColor;
		}

		static float plane_height = 10, plane_width = 10;
		static int plane_height_cuts = 3, plane_width_cuts = 3;
		static Axis plane_axis = Axis.Up;
		static bool plane_smooth = false;
		void PlaneGUI()
		{
			plane_axis = (Axis)EditorGUILayout.EnumPopup("Initial Orientation", plane_axis);

			plane_width = EditorGUILayout.FloatField("Width", plane_width);
			plane_height = EditorGUILayout.FloatField("Height", plane_height);

			if(plane_height < 1f)
				plane_height = 1f;

			if(plane_width < 1f)
				plane_width = 1f;

			plane_height_cuts = EditorGUILayout.IntField("Width Segments", plane_height_cuts);
			
			if(plane_height_cuts < 0)
				plane_height_cuts = 0;

			plane_width_cuts = EditorGUILayout.IntField("Length Segments", plane_width_cuts);
			
			if(plane_width_cuts < 0)
				plane_width_cuts = 0;

			if( showPreview && (GUI.changed || initPreview) ) 
				SetPreviewObject(
					 pb_Shape_Generator.PlaneGenerator(
					 	plane_height,
					 	plane_width,
					 	plane_height_cuts,
					 	plane_width_cuts,
					 	plane_axis,
					 	plane_smooth));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = COLOR_GREEN;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + shape, GUILayout.MinHeight(28)))
			{
				pb_Object pb = pb_Shape_Generator.PlaneGenerator(plane_height, plane_width, plane_height_cuts, plane_width_cuts, plane_axis, plane_smooth);
				pbUndo.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");
				
				if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );
				
				pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);
				pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				showPreview = false;

				if (prefClose)
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

			if( showPreview && (GUI.changed || initPreview) ) 
				SetPreviewObject(
					 pb_Shape_Generator.PipeGenerator(	
					 	pipe_radius,
						pipe_height,
						pipe_thickness,
						pipe_subdivAxis,
						pipe_subdivHeight
					 	));	 	

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = COLOR_GREEN;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + shape, GUILayout.MinHeight(28)))
			{
				pb_Object pb = pb_Shape_Generator.PipeGenerator(	
					 	pipe_radius,
						pipe_height,
						pipe_thickness,
						pipe_subdivAxis,
						pipe_subdivHeight
					 	);
				pbUndo.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );

				pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);
				pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				showPreview = false;

				if (prefClose)
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

			if( showPreview && (GUI.changed || initPreview) ) 
				SetPreviewObject(
					 pb_Shape_Generator.ConeGenerator(	
					 	cone_radius,
						cone_height,
						cone_subdivAxis
					 	));	 	

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = COLOR_GREEN;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + shape, GUILayout.MinHeight(28)))
			{
				pb_Object pb = pb_Shape_Generator.ConeGenerator(	
					 	cone_radius,
						cone_height,
						cone_subdivAxis
					 	);
				pbUndo.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );

				pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);
				pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				showPreview = false;

				if (prefClose)
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
			arch_radialCuts = Mathf.Clamp(arch_radialCuts, 3, 200);

			arch_angle = EditorGUILayout.FloatField("Arch Degrees", arch_angle);
			arch_angle = Mathf.Clamp(arch_angle, 0.0f, 360.0f);

			// arch_insideFaces = EditorGUILayout.Toggle("Inner Faces", arch_insideFaces);

			// arch_outsideFaces = EditorGUILayout.Toggle("Outer Faces", arch_outsideFaces);

			// arch_frontFaces = EditorGUILayout.Toggle("Front Faces", arch_frontFaces);

			// arch_backFaces = EditorGUILayout.Toggle("Rear Faces", arch_backFaces);

			if(arch_angle < 360f)
				arch_endCaps = EditorGUILayout.Toggle("End Caps", arch_endCaps);

		  	if (showPreview && (GUI.changed || initPreview))
				SetPreviewObject( pb_Shape_Generator.ArchGenerator(	arch_angle,
																	arch_radius,
																	Mathf.Clamp(arch_width, 0.01f, arch_radius),
																	arch_depth,
																	arch_radialCuts,
																	arch_insideFaces,
																	arch_outsideFaces,
																	arch_frontFaces,
																	arch_backFaces,
																	arch_endCaps));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = COLOR_GREEN;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + shape, GUILayout.MinHeight(28)))
			{
				pb_Object pb = pb_Shape_Generator.ArchGenerator(
					arch_angle,
					arch_radius,
					Mathf.Clamp(arch_width, 0.01f, arch_radius),
					arch_depth,
					arch_radialCuts,
					arch_insideFaces,
					arch_outsideFaces,
					arch_frontFaces,
					arch_backFaces,
					arch_endCaps);

				int[] removed;
				// happens when radius and width are the same :/
				pb.RemoveDegenerateTriangles(out removed);

				pbUndo.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				if (userMaterial) pb.SetFaceMaterial(pb.faces,userMaterial);

				pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);
				pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				showPreview = false;

				if (prefClose)
				{
					this.Close();
				}
			}

			GUI.backgroundColor = oldColor;
		}

		float ico_radius = 1f;
		int ico_subdivisions = 1;

		void IcosahedronGUI()
		{
			float t_ico_radius = ico_radius;
			int t_ico_subdivisions = ico_subdivisions;

			ico_radius = EditorGUILayout.Slider("Radius", ico_radius, 0.01f, 10f);

			ico_subdivisions = (int) EditorGUILayout.Slider("Subdivisions", ico_subdivisions, 0, 4);

			if (showPreview && ((t_ico_subdivisions != ico_subdivisions || t_ico_radius != ico_radius) || initPreview))
				SetPreviewObject(pb_Shape_Generator.IcosahedronGenerator(ico_radius, ico_subdivisions));

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = COLOR_GREEN;
			
			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + shape, GUILayout.MinHeight(28)))
			{
				pb_Object pb = pb_Shape_Generator.IcosahedronGenerator(ico_radius, ico_subdivisions);
				pbUndo.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");

				// To keep the preview snappy, shared indices aren't built in IcosahadreonGenerator 
				int[] welds;
				pb.WeldVertices(pb_Face.AllTriangles(pb.faces), Mathf.Epsilon, out welds);
				
				pbUVOps.ProjectFacesBox(pb, pb.faces);

				for(int i = 0; i < pb.faces.Length; i++)
					pb.faces[i].manualUV = true;

				if (userMaterial) pb.SetFaceMaterial(pb.faces,userMaterial);
				
				pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);
				pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);
				
				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				showPreview = false;
				
				if (prefClose)
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

			if( showPreview && (GUI.changed || initPreview) ) 
			{
				Vector3[] v = pbUtil.StringToVector3Array(verts);
				if(v.Length % 4 == 0)
					SetPreviewObject(pb_Object.CreateInstanceWithPoints(v));
			}

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = COLOR_GREEN;

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Build " + shape, GUILayout.MinHeight(28)))
			{
				pb_Object pb = pb_Object.CreateInstanceWithPoints(pbUtil.StringToVector3Array(verts));
				pbUndo.RegisterCreatedObjectUndo(pb.gameObject, "Create Shape");
				
				if( userMaterial ) pb.SetFaceMaterial(pb.faces, userMaterial );

				pb_Editor_Utility.SetPivotAndSnapWithPref(pb, null);
				pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);

				AlignWithPreviewObject(pb.gameObject);
				DestroyPreviewObject();
				showPreview = false;

				if (prefClose)
				{
					this.Close();
				}
			}
			
			GUI.backgroundColor = oldColor;
		}

		private int Clamp(int val, int min, int max)
		{
			if(val > max) val = max;
			if(val < min) val = min;
			return val;
		}

	#region PREVIEW OBJECT

		public void DestroyPreviewObject()
		{
			if(previewObject != null)
			{
				if(previewObject.GetComponent<MeshFilter>().sharedMesh != null)
					DestroyImmediate(previewObject.GetComponent<MeshFilter>().sharedMesh);

				GameObject.DestroyImmediate(previewObject);
			}
			if(_prevMat != null) DestroyImmediate(_prevMat);
		}

		private void SetPreviewObject(pb_Object pb)
		{
			SetPreviewObject(pb, null);
		}

		private void SetPreviewObject(pb_Object pb, int[] indicesToCenterPivotOn)
		{		
			pb.isSelectable = false;

			initPreview = false;
			bool prevTransform = false;
			

			if(previewObject != null)
			{
				prevTransform = true;
				RegisterPreviewObjectTransform();
			}
			
			DestroyPreviewObject();
			
			previewObject = pb.gameObject;

			if(pb_Preferences_Internal.GetBool(pb_Constant.pbForceGridPivot))
				pb.CenterPivot(indicesToCenterPivotOn == null ? new int[1]{0} : indicesToCenterPivotOn);

			if(prevTransform)
			{
				previewObject.transform.position = m_pos;
				previewObject.transform.rotation = m_rot;
				previewObject.transform.localScale = m_scale;
			}
			else
			{
				pb_Editor_Utility.ScreenCenter(previewObject.gameObject);
			}

			if(pb_ProGrids_Interface.SnapEnabled())
				pb.transform.position = pbUtil.SnapValue(pb.transform.position, pb_ProGrids_Interface.SnapValue());
			else
			if(pb_Preferences_Internal.GetBool(pb_Constant.pbForceVertexPivot))
				pb.transform.position = pbUtil.SnapValue(pb.transform.position, 1f);

			// Remove pb_Object
			Mesh m = pbUtil.DeepCopyMesh( pb.msh );
			
			GameObject.DestroyImmediate(pb.msh);
			GameObject.DestroyImmediate(pb);

			if(previewObject.GetComponent<pb_Entity>())
				GameObject.DestroyImmediate(previewObject.GetComponent<pb_Entity>());

			m.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave; // pb_Constant.EDITOR_OBJECT_HIDE_FLAGS;// HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
			previewMat.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave; // pb_Constant.EDITOR_OBJECT_HIDE_FLAGS;// HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
			previewObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave; // pb_Constant.EDITOR_OBJECT_HIDE_FLAGS;// HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

			previewObject.GetComponent<MeshFilter>().sharedMesh = m;
			previewObject.GetComponent<MeshRenderer>().sharedMaterial = previewMat;

			Selection.activeTransform = previewObject.transform;//pb.transform;
		}

		Vector3 m_pos = Vector3.zero;
		Quaternion m_rot = Quaternion.identity;
		Vector3 m_scale = Vector3.zero;
		private void RegisterPreviewObjectTransform()
		{
			m_pos 	= previewObject.transform.position;
			m_rot 	= previewObject.transform.rotation;
			m_scale = previewObject.transform.localScale;
		}	

		private bool PreviewObjectHasMoved()
		{
			if(m_pos != previewObject.transform.position)
				return true;
			if(m_rot != previewObject.transform.rotation)
				return true;
			if(m_scale != previewObject.transform.localScale)
				return true;	
			return false;
		}

		private void AlignWithPreviewObject(GameObject go)
		{
			if(go == null || previewObject == null) return;
			go.transform.position 	= previewObject.transform.position;
			go.transform.rotation 	= previewObject.transform.rotation;
			go.transform.localScale = previewObject.transform.localScale;

			pb_Object pb = go.GetComponent<pb_Object>();

			pb.ToMesh();
			pb.FreezeScaleTransform();
			pb.Refresh();
		}
	#endregion
	}
}
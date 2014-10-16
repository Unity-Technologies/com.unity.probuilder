using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;

/**
 *	\brief A simple example showing how to instantiate ProBuilder built prefabs and ProBuilder primitives
 */
public class ProBuilderInstantiate : MonoBehaviour
{
	// A ProBuilder built prefab
	public GameObject probuilderPrefab;

	// Object selection
	const int SHAPE_ENUM_LENGTH = 4;
	enum ObjectSelectionOptions
	{
		Prefab,
		Cube,
		Cylinder,
		Pipe
	}

	// Determines which object type to instantiate
	ObjectSelectionOptions objectToInstantiate = 0;

	// boring gui stuff
	private Rect prefWindow = new Rect(10, 10, 300, 300);
	private List<GameObject> generatedObjects = new List<GameObject>();

	public void OnEnable()
	{
		foreach(Component c in probuilderPrefab.GetComponents<Component>())
		{
			if(c is pb_Object)
				return;
		}
		objectToInstantiate = (ObjectSelectionOptions)1;
	}

	// nothing exciting going on here
	void OnGUI()
	{
		prefWindow = GUI.Window( 0, prefWindow, SelectionWindow, "Object To Instantiate" );
	}

	void SelectionWindow(int id)
	{
		GUI.DragWindow(new Rect(0,0, 20000, 20));

		GUILayout.BeginVertical();
			// Cycle through the available objects to instantiate
			for(int i = 0; i < SHAPE_ENUM_LENGTH; i++)
			{
				if(i == (int)objectToInstantiate)
					GUI.color = Color.green;
				if(GUILayout.Button( ((ObjectSelectionOptions)i).ToString() ))
					objectToInstantiate = (ObjectSelectionOptions) i;
				GUI.color = Color.white;
			}

			GUI.color = Color.red;
				if(GUILayout.Button("Clear Screen"))
				{
					foreach(GameObject go in generatedObjects)	
						GameObject.Destroy(go);
					
					generatedObjects.Clear();
				}
			GUI.color = Color.white;

			GUILayout.Label("Click to instantiate the selected object.");

		GUILayout.EndVertical();
	}

	Vector2 mPos_screen = Vector2.zero;
	void Update()
	{
		mPos_screen = Input.mousePosition;

		// If mouse is in the menu, ignore any clicks
		Vector2 mPos_gui = new Vector2(mPos_screen.x, Screen.height - mPos_screen.y);
		if(prefWindow.Contains( mPos_gui ))
			return;

		// If left click detected, instantiate the selected prefab where the mouse clicked (lock Z to 0)
		if(Input.GetMouseButtonUp(0))
		{
			// grab mouse position in world space
			Vector3 mPos_world = Camera.main.ScreenToWorldPoint( new Vector3(
				mPos_screen.x,
				mPos_screen.y,
				Camera.main.transform.position.x));	// for some reason I decided to set the cube up on the x axis?

			GameObject myInstantiatedObject;

			switch(objectToInstantiate)
			{
				case ObjectSelectionOptions.Prefab:
					
					// Use ProBuilder.Instantiate(GameObject) to build cube.  This force rebuilds the mesh
					// immediately.  It is also safe to use with non-ProBuilder objects, and will behave
					// exactly like GameObject.Instantiate() if no pb_Object type is found.
					myInstantiatedObject = ProBuilder.Instantiate(probuilderPrefab, Vector3.zero, Quaternion.identity);

					break;

				///* These demonstrate how to instantiate ProBuilder Primitive Types
				///* (called 'Shapes' to differentiate from UnityEngine.PrimitiveType).

				case ObjectSelectionOptions.Cube:
					
					// Using the basic Shape instantiation, build a cube.  CreatePrimitive() returns a 
					// pb_Object, so get the GameObject after instantiation.
					pb_Object pb = ProBuilder.CreatePrimitive(Shape.Cube);

					// Cubes alone are a little boring, so let's add some color to it
					Color[] cubeColors = new Color[6]
					{
						Color.green,
						Color.red,
						Color.cyan,
						Color.blue,
						Color.yellow,
						Color.magenta
					};

					int i = 0;
					foreach(pb_Face face in pb.faces)
						pb.SetFaceColor(face, cubeColors[i++]);

					myInstantiatedObject = pb.gameObject;
					
					// Cube gets a BoxCollider
					myInstantiatedObject.gameObject.AddComponent<BoxCollider>();

					break;

				case ObjectSelectionOptions.Cylinder:

					// ProBuilder also offers an extended interface for creating objects with
					// parameters. "ProBuilder.CreatePrimitive(ProBuilder.Shape.Cylinder);"
					// would also work here.  See the pb_Shape docs for full lists with params.
				
					// axisDivisions, radius, height, heightCuts
					myInstantiatedObject = pb_Shape_Generator.CylinderGenerator(12, .7f, .5f, 0).gameObject;

					// A convex MeshCollider suits a Cylinder nicely
					myInstantiatedObject.gameObject.AddComponent<MeshCollider>().convex = true;

					break;

				case ObjectSelectionOptions.Pipe:

					// ProBuilder also offers an extended interface for creating objects with
					// parameters. "ProBuilder.CreatePrimitive(ProBuilder.Shape.Cylinder);"
					// would also work here.  See the pb_Shape docs for full lists with params.
				
					// axisDivisions, radius, height, heightCuts
					// float radius, float height, float thickness, int subdivAxis, int subdivHeight) 
					myInstantiatedObject = pb_Shape_Generator.PipeGenerator(1f, 1f, .3f, 8, 0).gameObject;

					// A convex MeshCollider suits a Pipe nicely
					myInstantiatedObject.gameObject.AddComponent<MeshCollider>().convex = true;

					break;

				default:
					return;
			}
			
			// Move instantiated object to mouse position
			myInstantiatedObject.transform.position = mPos_world;
			
			// Add some rotation, cause that's fun
			myInstantiatedObject.transform.localRotation = Quaternion.Euler(
				Random.Range(0f, 360f),
				Random.Range(0f, 360f),
				Random.Range(0f, 360f));
			
			// Get some physics up in here
			myInstantiatedObject.AddComponent<Rigidbody>();
		
			// Add this to the list of instantiated gameObjects so we can remove it later
			generatedObjects.Add(myInstantiatedObject);
		}
	}
}

using UnityEngine;
using System.Collections;

/**
 *	\brief Contains ProBuilder types and general use calls.
 *	Use this class for creating ProBuilder objects.  You should not need 
 *	to access #pb_Object directly for anything other than direct mesh 
 *	manipulation.
 */
namespace ProBuilder2.Common
{
public class ProBuilder
{

#region PUBLIC_METHODS

	/**
	 *	\brief Instantiates the passed GameObject exactly as GameObject.Instantiate() would.
	 *	If the passed object is detected to be a pb_Object, it will forcibly reconstruct the Mesh
	 *	property.
	 */
	public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		GameObject go = (GameObject)GameObject.Instantiate(prefab, position, rotation);

		ReconstructMeshRecursive(go.transform);			
		
		return go;
	}

	public static GameObject Instantiate(GameObject prefab)
	{
		GameObject go = (GameObject)GameObject.Instantiate(prefab);

		ReconstructMeshRecursive(go.transform);			

		return go;
	}

	private static void ReconstructMeshRecursive(Transform t)
	{
		if(t.GetComponent<pb_Object>())
			t.GetComponent<pb_Object>().Verify();

		foreach(Transform child in t)
			ReconstructMeshRecursive(child);
	}

	/**
	 *	\brief If passed a valid ProBuilder::Shape, create it!  Non-valid shape is Shape.Custom.
	 *	\code{.cs}
	 *	// Example usage:
	 *	pb_Object myNewPrism = ProBuilder.CreatePrimitive(ProBuilder.Shape.Prism);
	 *	\endcode
	 *	@param shape The primitive type to create.  Always creates the shape with default parameters (if applicable).
	 *	\returns The new pb_Object.
	 */
	public static pb_Object CreatePrimitive(Shape shape)
	{
		pb_Object pb;
		switch(shape)
		{
			case Shape.Cube:
				pb = pb_Shape_Generator.CubeGenerator(Vector3.one);
				break;

			case Shape.Prism:
				pb = pb_Shape_Generator.PrismGenerator(Vector3.one);
				break;

			case Shape.Stair:
				// steps, width, height, depth, extend sides to floor, generate back, platforms only
				pb = pb_Shape_Generator.StairGenerator(5, 2f, 5f, 7f, true, true, false);
				break;

			case Shape.Cylinder:
				pb = pb_Shape_Generator.CylinderGenerator(6, 1.5f, 4f, 2);
				break;

			case Shape.Plane:
				pb = pb_Shape_Generator.PlaneGenerator(2f, 2f, 3, 3, Axis.Up, false);
				break;

			default:
				return null;
		}

		// pb.Refresh();

		return pb;
	}

	/**
	 *	\brief Creates a new pb_Object via duplicating the passed pb_Object.
	 *	@param pb The pb_Object to clone.
	 *	\returns The new pb_Object.
	 */
	public static pb_Object CreateObjectWithObject(pb_Object pb)
	{
		return pb_Object.InitWithObject(pb);
	}

	/**
	 *	\brief Creates a new #pb_Object using passed vertices to construct geometry.
	 *	@param vertices A vertex array (Vector3[]) containing the points to be used in 
	 *	the construction of the #pb_Object.  Vertices must be wound in counter-clockise
	 *	order.  Triangles will be wound in vertex groups of 4, with the winding order
	 *	0,1,2 1,3,2.
	 *	\code{.cs}
	 *	// Creates a pb_Object plane
	 *	ProBuilder.CreateInstanceWithPoints(new Vector3[4]{
	 *		new Vector3(-.5f, -.5f, 0f),
	 *		new Vector3(.5f, -.5f, 0f),
	 *		new Vector3(-.5f, .5f, 0f),
	 *		new Vector3(.5f, .5f, 0f)
	 *		});
	 *	\endcode
	 *	\sa pbUtil::StringWithArray<T>(T[] t) pbUtil::StringToVector3Array
	 *	\note Calling method is responsible for setting pb_Entity data.
	 *	\returns The resulting #pb_Object.
	 */
	public static pb_Object CreateObjectWithPoints(Vector3[] vertices)
	{
		return pb_Object.CreateInstanceWithPoints(vertices);
	}

	/**
	 *	\brief Creates a pb_Object with passed vertices and pb_Face array.  Allows a 
	 *	great amount of control when creating pb_Objects, and allows for non-quad geometry 
	 *	to be created.
	 *	@param _vertices The vertex array containing all points to be used.
	 *	@param _faces An array of pb_Face objects containing all triangle data material information for creating the new pb_Object.  May contain shared vertices, or not.
	 *	\sa pb_Object::SmoothIndices pb_Object::HardIndices
 	 *	\note Calling method is responsible for setting pb_Entity data.
	 *	\returns The newly created pb_Object.
	 */
	public static pb_Object CreateObjectWithVerticesFaces(Vector3[] _vertices, pb_Face[] _faces)
	{
		return pb_Object.CreateInstanceWithVerticesFaces(_vertices, _faces);
	}
#endregion
}
}
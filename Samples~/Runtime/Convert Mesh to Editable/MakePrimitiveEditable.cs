// Demonstrates how to convert a UnityEngine.Mesh object to an editable ProBuilderMesh.

using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace ProBuilder.Examples
{
	[RequireComponent(typeof(MeshFilter))]
	public class MakePrimitiveEditable : MonoBehaviour
	{
		void Start()
		{
			// Import from a GameObject. In this case we're loading and assigning to the same GameObject, but you may
			// load and apply to different Objects as well.

			// Create a new MeshImporter
			var importer = new MeshImporter(gameObject);
			importer.Import();

			// Since we're loading and setting from the same object, it is necessary to create a new mesh to avoid
			// overwriting the mesh that is being read from.
			var filter = GetComponent<MeshFilter>();
			filter.sharedMesh = new Mesh();

			//Retrieve the create PB Mesh
			var mesh = gameObject.GetComponent<ProBuilderMesh>();

			// Do something with the pb_Object. Here we're extruding every face on the object by .25.
			mesh.Extrude(mesh.faces, ExtrudeMethod.IndividualFaces, .25f);

			// Apply the imported geometry to the pb_Object
			mesh.ToMesh();

			// Rebuild UVs, Collisions, Tangents, etc.
			mesh.Refresh();
		}
	}
}

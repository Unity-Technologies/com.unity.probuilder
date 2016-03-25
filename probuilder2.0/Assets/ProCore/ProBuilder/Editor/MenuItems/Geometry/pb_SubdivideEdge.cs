using UnityEditor;
using ProBuilder2.EditorCommon;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using System.Linq;

namespace ProBuilder2.Actions
{
	static class pb_SubdivideEdge
	{
		[MenuItem("Tools/ProBuilder/Geometry/Subdivide Edges #&c")]
		static void Doit()
		{
			pb_ActionResult result = pb_Menu_Commands.MenuSubdivideEdge(Selection.transforms.GetComponents<pb_Object>());
		}
	}
}


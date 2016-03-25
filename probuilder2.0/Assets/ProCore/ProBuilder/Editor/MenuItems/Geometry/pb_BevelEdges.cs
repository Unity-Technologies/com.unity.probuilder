using UnityEditor;
using ProBuilder2.EditorCommon;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using System.Linq;

namespace ProBuilder2.Actions
{
	static class pb_BevelEdges
	{
		[MenuItem("Tools/ProBuilder/Geometry/Bevel Edges #&b")]
		static void Doit()
		{
			pb_Object pb = Selection.transforms.GetComponents<pb_Object>().FirstOrDefault();
			pb_Edge edge = pb.SelectedEdges.FirstOrDefault();

			if(pb != null && edge != null)
			{
				if( pb_Bevel.Bevel(pb, edge, .2f) )
				{
					pb.ToMesh();
					pb.Refresh();
					pb_Editor.Refresh();
				}
			}
		}
	}
}


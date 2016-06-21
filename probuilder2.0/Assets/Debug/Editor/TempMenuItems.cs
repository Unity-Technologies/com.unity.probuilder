using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
using System.Linq;
using System.Text;
using System.Reflection;

using Parabox.Debug;

public class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		pb_Object[] selection = Selection.transforms.GetComponents<pb_Object>();

		pbUndo.RecordSelection(selection, "Bevel Edges");

		foreach(pb_Object pb in selection)
		{
			pb_Face[] faces = pb.SelectedFaces;

			if(faces == null || faces.Length != 2)
				continue;

			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb);
			
			pb_WingedEdge wing = wings.FirstOrDefault(x => x.face == faces[0]);
			pb_WingedEdge first = wing;

			while( wing.opposite == null || wing.opposite.face != faces[1] )
			{
				wing = wing.next;

				if(wing == first)
					break;
			}

			pb_Edge cea = GetCommonEdgeInWindingOrder(wing);
			pb_Edge ceb = GetCommonEdgeInWindingOrder(wing.opposite);

			if( cea.x == ceb.x )
				Debug.Log("BAD NORMAL");
			else
				Debug.Log("GOOD NORMAL");
		}

		pb_Editor.Refresh();
	}

	static pb_Edge GetCommonEdgeInWindingOrder(pb_WingedEdge wing)
	{
		int[] indices = wing.face.indices;
		int len = indices.Length;

		for(int i = 0; i < len; i += 3)
		{
			pb_Edge e = wing.edge.local;
			int a = indices[i], b = indices[i+1], c = indices[i+2];

			if(e.x == a && e.y == b)
				return new pb_Edge(wing.edge.common);
			else if(e.x == b && e.y == a)
				return new pb_Edge(wing.edge.common.y, wing.edge.common.x);
			else if(e.x == b && e.y == c)
				return new pb_Edge(wing.edge.common);
			else if(e.x == c && e.y == b)
				return new pb_Edge(wing.edge.common.y, wing.edge.common.x);
			else if(e.x == c && e.y == a)
				return new pb_Edge(wing.edge.common);
			else if(e.x == a && e.y == c)
				return new pb_Edge(wing.edge.common.y, wing.edge.common.x);
		}
		return null;
	}

	static pb_ActionResult WeldVertices(pb_Object pb, int[] indices, out int[] welds, float distance)
	{
		welds = null;

		return pb_ActionResult.Success;
	}

	static pb_ActionResult SplitVertices(pb_Object pb, int[] indices, float distance)
	{
		HashSet<int> common = pb_IntArrayUtility.GetCommonIndices(pb.sharedIndices, indices);

		List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb);
		Dictionary<pb_Face, List<pb_Tuple<pb_WingedEdge, int>>> sorted = new Dictionary<pb_Face, List<pb_Tuple<pb_WingedEdge, int>>>();

		foreach(int c in common)
		{
			IEnumerable<pb_WingedEdge> matches = wings.Where(x => x.edge.common.Contains(c));
			HashSet<pb_Face> used = new HashSet<pb_Face>();

			foreach(pb_WingedEdge match in matches)
			{
				if(!used.Add(match.face))
					continue;
 
				sorted.AddOrAppend(match.face, new pb_Tuple<pb_WingedEdge, int>(match, c));
			}
		}

		List<pb_Face> faces = new List<pb_Face>(pb.faces);
		List<pb_Vertex> vertices = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
		List<pb_FaceRebuildData> newFaces = new List<pb_FaceRebuildData>();

		foreach(var kvp in sorted)
		{
			Dictionary<int, List<pb_Vertex>> dgaf = new Dictionary<int, List<pb_Vertex>>();
			pb_FaceRebuildData f = pbVertexOps.ExplodeVertex(vertices, kvp.Value, .2f, out dgaf);	
			newFaces.Add(f);
		}

		Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
		Dictionary<int, int> lookupUV = pb.sharedIndicesUV.ToDictionary();

		pb_FaceRebuildData.Apply(
			newFaces,
			vertices,
			faces,
			lookup,
			lookupUV);

		pb.SetVertices(vertices);
		pb.SetFaces(faces.ToArray());
		pb.SetSharedIndices(lookup);
		pb.SetSharedIndicesUV(lookupUV);
		pb.DeleteFaces( sorted.Keys );
		pb.ToMesh();

		return new pb_ActionResult(Status.Success, "Magic?");
	}
}

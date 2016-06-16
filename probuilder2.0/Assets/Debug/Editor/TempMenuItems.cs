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
			profiler.Begin("bevel edges");

			pb.ToMesh();

			pb_ActionResult result = pb_Bevel.BevelEdges(pb, pb.SelectedEdges, .05f);
			pb_EditorUtility.ShowNotification(result.notification);

			// pb_EditorUtility.ShowNotification(SplitVertices(pb, pb.SelectedTriangles, .2f).notification);
			pb.SetSelectedTriangles(null);

			pb.Refresh();
			pb.Optimize();

			profiler.End();
		}

		pb_Editor.Refresh();
	}

	static pb_ActionResult WeldVertices(pb_Object pb, int[] indices, out int[] welds, float distance)
	{
		welds = null;

		return pb_ActionResult.Success;
	}

	static pb_ActionResult SplitVertices(pb_Object pb, int[] indices, float distance)
	{
		HashSet<int> common = pb_IntArrayUtility.GetCommonIndices(pb.sharedIndices, indices);

		List<pb_WingedEdge> wings = pb_WingedEdge.GenerateWingedEdges(pb);
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
			pb_FaceRebuildData f = pbVertexOps.ExplodeVertex(vertices, kvp.Value, .2f);	
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

	// static void SplitVertex(pb_Object pb, int commonIndex)
	// {
	// 	List<pb_WingedEdge> wings = pb_WingedEdge.GenerateWingedEdges(pb);
	// 	pb_WingedEdge wing = wings.FirstOrDefault(x => x.edge.local.Contains(commonIndex));

	// 	if(wing == null)
	// 		return;

	// 	List<pb_Vertex> vertices = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));

	// 	pb_Edge ae = AlignEdgeWithDirection(wing.edge, commonIndex);
	// 	pb_WingedEdge next = wing.next.edge.common.Contains(commonIndex) ? wing.next : wing.previous;
	// 	pb_Edge an = AlignEdgeWithDirection(next.edge, commonIndex);
	// 	Debug.Log(ae + " : " + an);

	// 	int[] fi = wing.face.indices;
	// 	List<int> indices = new List<int>(wing.face.distinctIndices);

	// 	Vector3 normal = pb_Math.Normal(vertices[fi[0]].position, vertices[fi[1]].position, vertices[fi[2]].position);
	// 	Vector3 adir = vertices[ae.y].position - vertices[ae.x].position;
	// 	Vector3 bdir = vertices[an.y].position - vertices[an.x].position;

	// 	if(ae.x == an.x)
	// 	{
	// 		Debug.Log("append vertex");
	// 		indices.Add(vertices.Count);
	// 		an.x = vertices.Count;
	// 		vertices.Add(new pb_Vertex(vertices[ae.x]));
	// 	}

	// 	vertices[ae.x].position += adir.normalized * .2f;
	// 	vertices[an.x].position += bdir.normalized * .2f;

	// 	Vector3[] facePoints = new Vector3[indices.Count];
	// 	for(int i = 0; i < indices.Count; ++i)
	// 		facePoints[i] = vertices[indices[i]].position;

	// 	Vector2[] points2d = pb_Projection.PlanarProject(facePoints, normal);
	// 	List<int> triangles;
	// 	Debug.Log(points2d.ToString("\n"));

	// 	if(pb_Triangulation.SortAndTriangulate(points2d, out triangles))
	// 	{
	// 		int[] faceTris = new int[triangles.Count];

	// 		for(int i = 0; i < triangles.Count; i++)
	// 			faceTris[i] = indices[triangles[i]];

	// 		wing.face.SetIndices(faceTris);
	// 	}

	// 	pb.SetVertices(vertices);
	// 	pb.SetSharedIndices( pb_IntArrayUtility.ExtractSharedIndices(pb.vertices) );
	// 	pb.ToMesh();
	// }

	// static pb_Edge AlignEdgeWithDirection(pb_EdgeLookup edge, int commonIndex)
	// {
	// 	if(edge.common.x == commonIndex)
	// 		return new pb_Edge(edge.local.x, edge.local.y);
	// 	else
	// 		return new pb_Edge(edge.local.y, edge.local.x);
	// }
}

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

		EditorWindow.GetWindow<TempMenuItems>().Show();
		return;

		pb_Object[] selection = Selection.transforms.GetComponents<pb_Object>();

		pbUndo.RecordSelection(selection, "Bevel Edges");

		foreach(pb_Object pb in selection)
		{
			profiler.Begin("bevel edges");

			pb.ToMesh();
			
			int[] welds;
			pb_ActionResult result = pb.WeldVertices(pb.SelectedTriangles, .001f, out welds);
			pb_EditorUtility.ShowNotification(result.notification);
						
			// pb_ActionResult result = pb_Bevel.BevelEdges(pb, pb.SelectedEdges, .05f);
			// pb_EditorUtility.ShowNotification(result.notification);

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

	static void SplitVertex(pb_Object pb, int commonIndex)
	{
		List<pb_WingedEdge> wings = pb_WingedEdge.GenerateWingedEdges(pb);
		pb_WingedEdge wing = wings.FirstOrDefault(x => x.edge.local.Contains(commonIndex));

		if(wing == null)
			return;

		List<pb_Vertex> vertices = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));

		pb_Edge ae = AlignEdgeWithDirection(wing.edge, commonIndex);
		pb_WingedEdge next = wing.next.edge.common.Contains(commonIndex) ? wing.next : wing.previous;
		pb_Edge an = AlignEdgeWithDirection(next.edge, commonIndex);
		Debug.Log(ae + " : " + an);

		int[] fi = wing.face.indices;
		List<int> indices = new List<int>(wing.face.distinctIndices);

		Vector3 normal = pb_Math.Normal(vertices[fi[0]].position, vertices[fi[1]].position, vertices[fi[2]].position);
		Vector3 adir = vertices[ae.y].position - vertices[ae.x].position;
		Vector3 bdir = vertices[an.y].position - vertices[an.x].position;

		if(ae.x == an.x)
		{
			Debug.Log("append vertex");
			indices.Add(vertices.Count);
			an.x = vertices.Count;
			vertices.Add(new pb_Vertex(vertices[ae.x]));
		}

		vertices[ae.x].position += adir.normalized * .2f;
		vertices[an.x].position += bdir.normalized * .2f;

		Vector3[] facePoints = new Vector3[indices.Count];
		for(int i = 0; i < indices.Count; ++i)
			facePoints[i] = vertices[indices[i]].position;

		Vector2[] points2d = pb_Projection.PlanarProject(facePoints, normal);
		List<int> triangles;
		Debug.Log(points2d.ToString("\n"));

		if(pb_Triangulation.SortAndTriangulate(points2d, out triangles))
		{
			int[] faceTris = new int[triangles.Count];

			for(int i = 0; i < triangles.Count; i++)
				faceTris[i] = indices[triangles[i]];

			wing.face.SetIndices(faceTris);
		}

		pb.SetVertices(vertices);
		pb.SetSharedIndices( pb_IntArrayUtility.ExtractSharedIndices(pb.vertices) );
		pb.ToMesh();
	}

	static pb_Edge AlignEdgeWithDirection(pb_EdgeLookup edge, int commonIndex)
	{
		if(edge.common.x == commonIndex)
			return new pb_Edge(edge.local.x, edge.local.y);
		else
			return new pb_Edge(edge.local.y, edge.local.x);
	}
}

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Reflection;
using KdTree;
using KdTree.Math;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using ProBuilder2.EditorCommon;

/**
 *	KdTree DLL compiled from https://github.com/procore3d/KdTree
 */
public class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		// O = acos(dot(a, b))
		// cos(o)

		pb_Object[] selection = Selection.transforms.GetComponents<pb_Object>();
		pb_Undo.RecordObjects(selection, "sdlafk");
		foreach(pb_Object pb in selection)
		{
			pb.ToMesh();
			pb_Smoothing.ApplySmoothingGroups(pb, pb.SelectedFaces, 5f);
			pb.Refresh();
			pb.Optimize();
		}

		// System.Text.StringBuilder sb = new System.Text.StringBuilder();

		// foreach(pb_Object pb in Selection.transforms.GetComponents<pb_Object>())
		// {
		// 	List<pb_WingedEdge> faces = pb_WingedEdge.GetWingedEdges(pb, pb.SelectedFaces, true);
		// 	m_Vertices = pb_Vertex.GetVertices(pb);

		// 	foreach(pb_WingedEdge edge in faces[0])
		// 	{
		// 		if(edge.opposite != null)
		// 		{
		// 			sb.AppendLine("quad score: " + GetQuadScore(edge, edge.opposite));
		// 		}
		// 	}
		// }

		// pb_Log.Info(sb.ToString());
	}

	private static pb_Vertex[] m_Vertices = null;

	private static float GetQuadScore(pb_WingedEdge left, pb_WingedEdge right, float normalThreshold = .9f)
	{
		int[] quad = pb_WingedEdge.MakeQuad(left, right);

		if(quad == null)
			return 0f;

		// first check normals
		Vector3 leftNormal = pb_Math.Normal(m_Vertices[quad[0]].position, m_Vertices[quad[1]].position, m_Vertices[quad[2]].position);
		Vector3 rightNormal = pb_Math.Normal(m_Vertices[quad[2]].position, m_Vertices[quad[3]].position, m_Vertices[quad[0]].position);

		float score = Vector3.Dot(leftNormal, rightNormal);

		if(score < normalThreshold)
			return 0f;

		// next is right-angle-ness check
		Vector3 a = (m_Vertices[quad[1]].position - m_Vertices[quad[0]].position);
		Vector3 b = (m_Vertices[quad[2]].position - m_Vertices[quad[1]].position);
		Vector3 c = (m_Vertices[quad[3]].position - m_Vertices[quad[2]].position);
		Vector3 d = (m_Vertices[quad[0]].position - m_Vertices[quad[3]].position);

		a.Normalize();
		b.Normalize();
		c.Normalize();
		d.Normalize();

		float da = Mathf.Abs(Vector3.Dot(a, b));
		float db = Mathf.Abs(Vector3.Dot(b, c));
		float dc = Mathf.Abs(Vector3.Dot(c, d));
		float dd = Mathf.Abs(Vector3.Dot(d, a));

		score += 1f - ((da + db + dc + dd) * .25f);

		// and how close to parallel the opposite sides area
		score += Mathf.Abs(Vector3.Dot(a, c)) * .5f;
		score += Mathf.Abs(Vector3.Dot(b, d)) * .5f;

		return score * .33f;
	}

}


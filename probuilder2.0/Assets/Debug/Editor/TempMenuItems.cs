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
		System.Text.StringBuilder sb = new System.Text.StringBuilder();

		foreach(pb_Object pb in Selection.transforms.GetComponents<pb_Object>())
		{
			List<pb_WingedEdge> faces = pb_WingedEdge.GetWingedEdges(pb, pb.SelectedFaces, true);
			m_Vertices = pb_Vertex.GetVertices(pb);

			foreach(pb_WingedEdge edge in faces[0])
			{
				if(edge.opposite != null)
				{
					sb.AppendLine("quad score: " + GetQuadScore(edge, edge.opposite));
				}
			}
		}

		pb_Log.Info(sb.ToString());
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

		Vector3 a = (m_Vertices[quad[1]].position - m_Vertices[quad[0]].position).normalized;
		Vector3 b = (m_Vertices[quad[2]].position - m_Vertices[quad[1]].position).normalized;
		Vector3 c = (m_Vertices[quad[3]].position - m_Vertices[quad[2]].position).normalized;
		Vector3 d = (m_Vertices[quad[0]].position - m_Vertices[quad[3]].position).normalized;

		score += 1f - ((Mathf.Abs(Vector3.Dot(a, b)) +
			Mathf.Abs(Vector3.Dot(b, c)) +
			Mathf.Abs(Vector3.Dot(c, d)) +
			Mathf.Abs(Vector3.Dot(d, a))) * .25f);

		return score * .5f;
	}

}

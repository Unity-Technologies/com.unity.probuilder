using UnityEngine;
using ProBuilder2.Common;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder2.MeshOperations
{
	public class pb_MeshImporter
	{
		private pb_Object m_Mesh;
		private pb_Vertex[] m_Vertices;

		public pb_MeshImporter(pb_Object target)
		{
			m_Mesh = target;
		}

		/**
		 * Import a mesh onto an empty pb_Object.
		 */
		public bool Import(MeshFilter meshFilter)
		{
			Mesh mesh = meshFilter.sharedMesh;
			m_Vertices = pb_Vertex.GetVertices(mesh);
			List<pb_Face> faces = new List<pb_Face>();

			// Fill in Faces array with just the position indices. In the next step we'll
			// figure out smoothing groups & merging
			for(int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
			{
				switch(mesh.GetTopology(subMeshIndex))
				{
					case MeshTopology.Triangles:
					{
						int[] indices = mesh.GetIndices(subMeshIndex);

						for(int tri = 0; tri < indices.Length; tri += 3)
							faces.Add(new pb_Face(new int[] { indices[tri], indices[tri+1], indices[tri+2] } ));
					}
					break;

					case MeshTopology.Quads:
					{
						int[] indices = mesh.GetIndices(subMeshIndex);

						for(int quad = 0; quad < indices.Length; quad += 4)
							faces.Add(new pb_Face(new int[] {
								indices[quad  ], indices[quad+1], indices[quad+2],
								indices[quad+1], indices[quad+2], indices[quad+3] } ));
					}
					break;

					default:
						throw new System.NotImplementedException("ProBuilder only supports importing triangle and quad meshes.");
				}
			}

			m_Mesh.Clear();
			m_Mesh.SetVertices(m_Vertices);
			m_Mesh.SetFaces(faces);
			m_Mesh.SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(m_Mesh.vertices));

			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(m_Mesh, m_Mesh.faces, true);
			HashSet<pb_Face> processed = new HashSet<pb_Face>();
			int smoothingGroup = 1;

			for(int i = 0; i < wings.Count; i++)
			{
				if(processed.Contains(wings[i].face))
					continue;
				wings[i].face.smoothingGroup = smoothingGroup++;
				FindSoftEdgesRecursive(wings[i], processed);
			}

			Dictionary<pb_EdgeLookup, float> connections = new Dictionary<pb_EdgeLookup, float>();

			for(int i = 0; i < wings.Count; i++)
			{
				foreach(pb_WingedEdge border in wings[i])
				{
					if(border.opposite != null && !connections.ContainsKey(border.edge))
					{
						float score = GetQuadScore(border, border.opposite);
						connections.Add(border.edge, score);
					}
				}
			}

			Debug.Log("connections: " + connections.Count);

			foreach(var kvp in connections)
				Debug.Log(kvp.Key + "  " + kvp.Value);

			return false;
		}

		/**
		 * Get a weighted value for the quality of a quad composed of two triangles. 0 is terrible, 1 is perfect.
		 * normalThreshold will discard any quads where the dot product of their normals is less than the threshold.
		 */
		private float GetQuadScore(pb_WingedEdge left, pb_WingedEdge right, float normalThreshold = .9f)
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

		private void FindSoftEdgesRecursive(pb_WingedEdge wing, HashSet<pb_Face> processed)
		{
			foreach(pb_WingedEdge border in wing)
			{
				if(border.opposite == null)
					continue;

				if( !processed.Contains(border.opposite.face) && IsSoftEdge(border.edge, border.opposite.edge) )
				{
					border.opposite.face.smoothingGroup = wing.face.smoothingGroup;
					processed.Add(border.opposite.face);
					FindSoftEdgesRecursive(border.opposite, processed);
				}
			}
		}

		private bool IsSoftEdge(pb_EdgeLookup left, pb_EdgeLookup right)
		{
			pb_Vertex lx = m_Vertices[left.local.x];
			pb_Vertex ly = m_Vertices[left.local.y];
			pb_Vertex rx = m_Vertices[right.common.x == left.common.x ? right.local.x : right.local.y];
			pb_Vertex ry = m_Vertices[right.common.y == left.common.y ? right.local.y : right.local.x];

			if( lx.hasNormal && ly.hasNormal && rx.hasNormal && ry.hasNormal )
				return pb_Math.Approx3(lx.normal, rx.normal) && pb_Math.Approx3(ly.normal, ry.normal);

			return false;
		}
	}
}

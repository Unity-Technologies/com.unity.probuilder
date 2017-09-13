using UnityEngine;
using ProBuilder2.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.MeshOperations
{
	public class pb_MeshImporter
	{
		public class Settings
		{
			public bool quads = true;
			// Generate smoothing groups based on mesh normals.
			public bool smoothing = true;
			// Degree of difference between face normals to allow when determining smoothing groups.
			public float smoothingThreshold = 1f;
		}

		private static readonly Settings DEFAULT_IMPORT_SETTINGS = new Settings()
		{
			quads = true,
			smoothing = true,
			smoothingThreshold = 1f
		};

		private pb_Object m_Mesh;
		private pb_Vertex[] m_Vertices;

		public pb_MeshImporter(pb_Object target)
		{
			m_Mesh = target;
		}

		/**
		 * Import a mesh onto an empty pb_Object.
		 */
		public bool Import(MeshFilter meshFilter, Settings importSettings = null)
		{
			if(importSettings == null)
				importSettings = DEFAULT_IMPORT_SETTINGS;

			Mesh originalMesh = meshFilter.sharedMesh;

			// When importing the mesh is always split into triangles with no vertices shared
			// between faces. In a later step co-incident vertices are collapsed (eg, before
			// leaving the Import function).
			pb_Vertex[] sourceVertices = pb_Vertex.GetVertices(originalMesh);
			List<pb_Vertex> splitVertices = new List<pb_Vertex>();
			List<pb_Face> faces = new List<pb_Face>();

			// Fill in Faces array with just the position indices. In the next step we'll
			// figure out smoothing groups & merging
			int vertexIndex = 0;

			for(int subMeshIndex = 0; subMeshIndex < originalMesh.subMeshCount; subMeshIndex++)
			{
				switch(originalMesh.GetTopology(subMeshIndex))
				{
					case MeshTopology.Triangles:
					{
						int[] indices = originalMesh.GetIndices(subMeshIndex);

						for(int tri = 0; tri < indices.Length; tri += 3)
						{
							faces.Add(new pb_Face(new int[] { vertexIndex, vertexIndex + 1, vertexIndex + 2 } ));
							splitVertices.Add(sourceVertices[indices[tri  ]]);
							splitVertices.Add(sourceVertices[indices[tri+1]]);
							splitVertices.Add(sourceVertices[indices[tri+2]]);

							vertexIndex += 3;
						}
					}
					break;

					case MeshTopology.Quads:
					{
						int[] indices = originalMesh.GetIndices(subMeshIndex);

						for(int quad = 0; quad < indices.Length; quad += 4)
						{
							faces.Add(new pb_Face(new int[] {
								vertexIndex    , vertexIndex + 1, vertexIndex + 2,
								vertexIndex + 1, vertexIndex + 2, vertexIndex + 3
							}));

							splitVertices.Add(sourceVertices[indices[quad  ]]);
							splitVertices.Add(sourceVertices[indices[quad+1]]);
							splitVertices.Add(sourceVertices[indices[quad+2]]);
							splitVertices.Add(sourceVertices[indices[quad+3]]);

							vertexIndex += 4;
						}
				}
					break;

					default:
						throw new System.NotImplementedException("ProBuilder only supports importing triangle and quad meshes.");
				}
			}

			m_Vertices = splitVertices.ToArray();

			m_Mesh.Clear();
			m_Mesh.SetVertices(m_Vertices);
			m_Mesh.SetFaces(faces);
			m_Mesh.SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(m_Mesh.vertices));
			m_Mesh.SetSharedIndicesUV(new pb_IntArray[0]);

			HashSet<pb_Face> processed = new HashSet<pb_Face>();

			if(importSettings.quads)
			{
				List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(m_Mesh, m_Mesh.faces, true);

				// build a lookup of the strength of edge connections between triangle faces
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

				List<pb_Tuple<pb_Face, pb_Face>> quads = new List<pb_Tuple<pb_Face, pb_Face>>();

				// move through each face and find it's best quad neighbor
				foreach(pb_WingedEdge face in wings)
				{
					if(!processed.Add(face.face))
						continue;

					float bestScore = 0f;
					pb_Face buddy = null;

					foreach(pb_WingedEdge border in face)
					{
						if(processed.Contains(border.opposite.face))
							continue;

						float borderScore = connections[border.edge];

						// only add it if the opposite face's best score is also this face
						if(borderScore > bestScore && face.face == GetBestQuadConnection(border.opposite, connections))
						{
							bestScore = borderScore;
							buddy = border.opposite.face;
						}
					}

					if(buddy != null)
					{
						processed.Add(buddy);
						quads.Add(new pb_Tuple<pb_Face, pb_Face>(face.face, buddy));
					}
				}

				// don't collapse coincident vertices if smoothing is enabled, we need the original
				// normals intact
				pb_MergeFaces.MergePairs(m_Mesh, quads, !importSettings.smoothing);
			}

			if(importSettings.smoothing)
			{
				pb_Smoothing.ApplySmoothingGroups(m_Mesh, m_Mesh.faces, importSettings.smoothingThreshold, m_Vertices.Select(x => x.normal).ToArray());
				pb_MergeFaces.CollapseCoincidentVertices(m_Mesh, m_Mesh.faces);
			}

			return false;
		}

		private pb_Face GetBestQuadConnection(pb_WingedEdge wing, Dictionary<pb_EdgeLookup, float> connections)
		{
			float score = 0f;
			pb_Face face = null;

			foreach(pb_WingedEdge border in wing)
			{
				if( connections[border.edge] > score )
				{
					score = connections[border.edge];
					face = border.opposite.face;
				}
			}

			return face;
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

			// the three tests each contribute 1
			return score * .33f;
		}
	}
}

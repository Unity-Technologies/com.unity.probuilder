using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// A collection of settings used when importing models to the ProBuilderMesh component.
    /// </summary>
	[Serializable]
    public sealed class MeshImportSettings
    {
        [SerializeField]
        bool m_Quads = true;

        [SerializeField]
        bool m_Smoothing = true;

        [SerializeField]
        float m_SmoothingThreshold = 1f;

        /// <value>
        /// Try to quadrangilize triangle meshes.
        /// </value>
        public bool quads
        {
            get { return m_Quads; }
            set { m_Quads = value; }
        }

        // Allow ngons when importing meshes. @todo
        // public bool ngons = false;

        /// <value>
        /// Generate smoothing groups based on mesh normals.
        /// </value>
        public bool smoothing
        {
            get { return m_Smoothing; }
            set { m_Smoothing = value; }
        }

        /// <value>
        /// Degree of difference between face normals to allow when determining smoothing groups.
        /// </value>
        public float smoothingAngle
        {
            get { return m_SmoothingThreshold; }
            set { m_SmoothingThreshold = value; }
        }

        /// <value>
        /// Basic mesh import settings. Imports quads, and smoothes faces with a threshold of 1 degree.
        /// </value>
        public static MeshImportSettings Default
        {
            get
            {
                return new MeshImportSettings()
                {
                    m_Quads = true,
                    m_Smoothing = true,
                    m_SmoothingThreshold = 1f
                };
            }
        }

        public override string ToString()
        {
            return string.Format("quads: {0}\nsmoothing: {1}\nthreshold: {2}",
                quads,
                smoothing,
                smoothingAngle);
        }
    }

    /// <summary>
    /// Responsible for importing UnityEngine.Mesh data to a ProBuilderMesh component.
    /// </summary>
    public sealed class MeshImporter
	{
		static readonly MeshImportSettings k_DefaultImportSettings = new MeshImportSettings()
		{
			quads = true,
			smoothing = true,
			smoothingAngle= 1f
		};

		ProBuilderMesh m_Mesh;
		Vertex[] m_Vertices;

		/// <summary>
		/// Create a new MeshImporter instance.
		/// </summary>
		/// <param name="target">The ProBuilderMesh component that will be initialized with the imported mesh attributes.</param>
		public MeshImporter(ProBuilderMesh target)
		{
			m_Mesh = target;
		}

		/// <summary>
		/// Import mesh data from a GameObject's MeshFilter.sharedMesh and MeshRenderer.sharedMaterials.
		/// </summary>
		/// <param name="gameObject">The GameObject to search for MeshFilter and MeshRenderer data.</param>
		/// <param name="importSettings">Optional settings parameter defines import customization properties.</param>
		/// <returns>True if the mesh data was successfully translated to the ProBuilderMesh target, false if something went wrong.</returns>
		public bool Import(GameObject gameObject, MeshImportSettings importSettings = null)
		{
            if (gameObject == null)
                throw new ArgumentNullException("gameObject");

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
			MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();

			if (meshFilter == null || meshFilter.sharedMesh == null)
			{
				Log.Error("GameObject does not contain a valid MeshFilter or sharedMesh.");
				return false;
			}

			return Import(meshFilter.sharedMesh, mr ? mr.sharedMaterials : null, importSettings);
		}

		/// <summary>
		/// Import mesh data from a GameObject's MeshFilter.sharedMesh and MeshRenderer.sharedMaterials.
		/// </summary>
		/// <param name="originalMesh">The UnityEngine.Mesh to extract attributes from.</param>
		/// <param name="materials">The materials array corresponding to the originalMesh submeshes.</param>
		/// <param name="importSettings">Optional settings parameter defines import customization properties.</param>
		/// <returns>True if the mesh data was successfully translated to the ProBuilderMesh target, false if something went wrong.</returns>
		/// <exception cref="NotImplementedException">Import only supports triangle and quad mesh topologies.</exception>
		public bool Import(Mesh originalMesh, Material[] materials, MeshImportSettings importSettings = null)
		{
            if (originalMesh == null)
                throw new ArgumentNullException("originalMesh");

            if (materials == null)
                throw new ArgumentNullException("materials");

            if (importSettings == null)
				importSettings = k_DefaultImportSettings;

			// When importing the mesh is always split into triangles with no vertices shared
			// between faces. In a later step co-incident vertices are collapsed (eg, before
			// leaving the Import function).
			Vertex[] sourceVertices = Vertex.GetVertices(originalMesh);
			List<Vertex> splitVertices = new List<Vertex>();
			List<Face> faces = new List<Face>();

			// Fill in Faces array with just the position indices. In the next step we'll
			// figure out smoothing groups & merging
			int vertexIndex = 0;
			int materialCount = materials != null ? materials.Length : 0;

			for(int subMeshIndex = 0; subMeshIndex < originalMesh.subMeshCount; subMeshIndex++)
			{
				Material material = materialCount > 0 ? materials[subMeshIndex % materialCount] : BuiltinMaterials.DefaultMaterial;

				switch(originalMesh.GetTopology(subMeshIndex))
				{
					case UnityEngine.MeshTopology.Triangles:
					{
						int[] indices = originalMesh.GetIndices(subMeshIndex);

						for(int tri = 0; tri < indices.Length; tri += 3)
						{
							faces.Add(new Face(
								new int[] { vertexIndex, vertexIndex + 1, vertexIndex + 2 },
								material,
								new AutoUnwrapSettings(),
								Smoothing.smoothingGroupNone,
								-1,
								-1,
								true));

							splitVertices.Add(sourceVertices[indices[tri  ]]);
							splitVertices.Add(sourceVertices[indices[tri+1]]);
							splitVertices.Add(sourceVertices[indices[tri+2]]);

							vertexIndex += 3;
						}
					}
					break;

					case UnityEngine.MeshTopology.Quads:
					{
						int[] indices = originalMesh.GetIndices(subMeshIndex);

						for(int quad = 0; quad < indices.Length; quad += 4)
						{
							faces.Add(new Face(new int[] {
								vertexIndex    , vertexIndex + 1, vertexIndex + 2,
								vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 },
								material,
								new AutoUnwrapSettings(),
								Smoothing.smoothingGroupNone,
								-1,
								-1,
								true));

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
			m_Mesh.SetSharedIndexes(IntArrayUtility.GetSharedIndexesWithPositions(m_Mesh.positionsInternal));
			m_Mesh.SetSharedIndexesUV(new IntArray[0]);

			HashSet<Face> processed = new HashSet<Face>();

			if(importSettings.quads)
			{
				List<WingedEdge> wings = WingedEdge.GetWingedEdges(m_Mesh, m_Mesh.facesInternal, true);

				// build a lookup of the strength of edge connections between triangle faces
				Dictionary<EdgeLookup, float> connections = new Dictionary<EdgeLookup, float>();

				for(int i = 0; i < wings.Count; i++)
				{
					foreach(WingedEdge border in wings[i])
					{
						if(border.opposite != null && !connections.ContainsKey(border.edge))
						{
							float score = GetQuadScore(border, border.opposite);
							connections.Add(border.edge, score);
						}
					}
				}

				List<SimpleTuple<Face, Face>> quads = new List<SimpleTuple<Face, Face>>();

				// move through each face and find it's best quad neighbor
				foreach(WingedEdge face in wings)
				{
					if(!processed.Add(face.face))
						continue;

					float bestScore = 0f;
					Face buddy = null;

					foreach(WingedEdge border in face)
					{
						if(border.opposite != null && processed.Contains(border.opposite.face))
							continue;

						float borderScore;

						// only add it if the opposite face's best score is also this face
						if( connections.TryGetValue(border.edge, out borderScore) &&
							borderScore > bestScore &&
							face.face == GetBestQuadConnection(border.opposite, connections))
						{
							bestScore = borderScore;
							buddy = border.opposite.face;
						}
					}

					if(buddy != null)
					{
						processed.Add(buddy);
						quads.Add(new SimpleTuple<Face, Face>(face.face, buddy));
					}
				}

				// don't collapse coincident vertices if smoothing is enabled, we need the original normals intact
				MergeElements.MergePairs(m_Mesh, quads, !importSettings.smoothing);
			}

			if(importSettings.smoothing)
			{
				Smoothing.ApplySmoothingGroups(m_Mesh, m_Mesh.facesInternal, importSettings.smoothingAngle, m_Vertices.Select(x => x.normal).ToArray());
				// After smoothing has been applied go back and weld coincident vertices created by MergePairs.
				MergeElements.CollapseCoincidentVertices(m_Mesh, m_Mesh.facesInternal);
			}

			return false;
		}

		Face GetBestQuadConnection(WingedEdge wing, Dictionary<EdgeLookup, float> connections)
		{
			float score = 0f;
			Face face = null;

			foreach(WingedEdge border in wing)
			{
				float s = 0f;

				if(connections.TryGetValue(border.edge, out s) && s > score)
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
		 * @todo Abstract the quad detection to a separate class so it can be applied to pb_Objects.
		 */
		float GetQuadScore(WingedEdge left, WingedEdge right, float normalThreshold = .9f)
		{
			int[] quad = WingedEdge.MakeQuad(left, right);

			if(quad == null)
				return 0f;

			// first check normals
			Vector3 leftNormal = Math.Normal(m_Vertices[quad[0]].position, m_Vertices[quad[1]].position, m_Vertices[quad[2]].position);
			Vector3 rightNormal = Math.Normal(m_Vertices[quad[2]].position, m_Vertices[quad[3]].position, m_Vertices[quad[0]].position);

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

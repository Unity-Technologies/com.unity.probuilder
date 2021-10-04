using UnityEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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

        /// <summary>
        /// Gets or sets whether to quadrangilize meshes (convert them to quads if possible).
        /// </summary>
        public bool quads
        {
            get { return m_Quads; }
            set { m_Quads = value; }
        }

        // Allow ngons when importing meshes. @todo
        // public bool ngons = false;

        /// <summary>
        /// Gets or sets whether to generate smoothing groups based on mesh normals.
        /// </summary>
        public bool smoothing
        {
            get { return m_Smoothing; }
            set { m_Smoothing = value; }
        }

        /// <summary>
        /// Gets or sets the allowable degree of difference between face normals when determining smoothing groups.
        /// </summary>
        public float smoothingAngle
        {
            get { return m_SmoothingThreshold; }
            set { m_SmoothingThreshold = value; }
        }

        /// <summary>
        /// Returns a string representation of the options.
        /// </summary>
        /// <returns>String formatted as `quads: [quads]\nsmoothing: [smoothing]\nthreshold: [threshold]`.</returns>
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
            smoothingAngle = 1f
        };

        Mesh m_SourceMesh;
        Material[] m_SourceMaterials;
        ProBuilderMesh m_Destination;
        Vertex[] m_Vertices;

        /// <summary>
        /// Creates a new ProBuilderMesh importer instance from the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to write vertex data to.</param>
        public MeshImporter(GameObject gameObject)
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            m_SourceMesh = meshFilter.sharedMesh;
            if(m_SourceMesh == null)
                throw new ArgumentNullException("gameObject", "GameObject does not contain a valid MeshFilter.sharedMesh.");
            m_Destination = gameObject.DemandComponent<ProBuilderMesh>();
            m_SourceMaterials = gameObject.GetComponent<MeshRenderer>()?.sharedMaterials;
        }

        /// <summary>
        /// Creates a new ProBuilderMesh importer instance from the specified mesh and materials.
        /// </summary>
        /// <param name="sourceMesh">The Mesh asset to import vertex data from.</param>
        /// <param name="sourceMaterials">The materials to assign to the ProBuilderMesh renderer.</param>
        /// <param name="destination">The ProBuilderMesh asset to write vertex data to.</param>
        public MeshImporter(Mesh sourceMesh, Material[] sourceMaterials, ProBuilderMesh destination)
        {
            if(sourceMesh == null)
                throw new ArgumentNullException("sourceMesh");
            if(destination == null)
                throw new ArgumentNullException("destination");
            m_SourceMesh = sourceMesh;
            m_SourceMaterials = sourceMaterials;
            m_Destination = destination;
        }

        /// <summary>Obsolete.</summary>
        /// <param name="destination">The ProBuilderMesh asset.</param>
        [Obsolete, EditorBrowsable(EditorBrowsableState.Never)]
        public MeshImporter(ProBuilderMesh destination)
        {
            m_Destination = destination;
        }

        /// <summary>Obsolete.</summary>
        /// <param name="go">The GameObject asset.</param>
        /// <param name="importSettings">The import settings.</param>
        /// <returns>Success/failure</returns>
        [Obsolete, EditorBrowsable(EditorBrowsableState.Never)]
        public bool Import(GameObject go, MeshImportSettings importSettings = null)
        {
            try
            {
                m_SourceMesh = go.GetComponent<MeshFilter>().sharedMesh;
                m_SourceMaterials = go.GetComponent<MeshRenderer>()?.sharedMaterials;
                Import(importSettings);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Imports mesh data from a GameObject's <see cref="UnityEngine.MeshFilter.sharedMesh"/> and
        /// <see cref="UnityEngine.Renderer.sharedMaterials"/> properties.
        /// </summary>
        /// <param name="importSettings">Optional import customization settings.</param>
        /// <exception cref="NotSupportedException">Import only supports triangle and quad mesh topologies.</exception>
        public void Import(MeshImportSettings importSettings = null)
        {
            if (importSettings == null)
                importSettings = k_DefaultImportSettings;

            // When importing the mesh is always split into triangles with no vertices shared
            // between faces. In a later step co-incident vertices are collapsed (eg, before
            // leaving the Import function).
            Vertex[] sourceVertices = m_SourceMesh.GetVertices();
            List<Vertex> splitVertices = new List<Vertex>();
            List<Face> faces = new List<Face>();

            // Fill in Faces array with just the position indexes. In the next step we'll
            // figure out smoothing groups & merging
            int vertexIndex = 0;
            int materialCount = m_SourceMaterials != null ? m_SourceMaterials.Length : 0;

            for (int submeshIndex = 0; submeshIndex < m_SourceMesh.subMeshCount; submeshIndex++)
            {
                switch (m_SourceMesh.GetTopology(submeshIndex))
                {
                    case MeshTopology.Triangles:
                    {
                        int[] indexes = m_SourceMesh.GetIndices(submeshIndex);

                        for (int tri = 0; tri < indexes.Length; tri += 3)
                        {
                            faces.Add(new Face(
                                    new int[] { vertexIndex, vertexIndex + 1, vertexIndex + 2 },
                                    Math.Clamp(submeshIndex, 0, materialCount - 1),
                                    AutoUnwrapSettings.tile,
                                    Smoothing.smoothingGroupNone,
                                    -1,
                                    -1,
                                    true));

                            splitVertices.Add(sourceVertices[indexes[tri]]);
                            splitVertices.Add(sourceVertices[indexes[tri + 1]]);
                            splitVertices.Add(sourceVertices[indexes[tri + 2]]);

                            vertexIndex += 3;
                        }
                    }
                    break;

                    case MeshTopology.Quads:
                    {
                        int[] indexes = m_SourceMesh.GetIndices(submeshIndex);

                        for (int quad = 0; quad < indexes.Length; quad += 4)
                        {
                            faces.Add(new Face(new int[]
                                {
                                    vertexIndex, vertexIndex + 1, vertexIndex + 2,
                                    vertexIndex + 2, vertexIndex + 3, vertexIndex + 0
                                },
                                Math.Clamp(submeshIndex, 0, materialCount - 1),
                                AutoUnwrapSettings.tile,
                                Smoothing.smoothingGroupNone,
                                -1,
                                -1,
                                true));

                            splitVertices.Add(sourceVertices[indexes[quad]]);
                            splitVertices.Add(sourceVertices[indexes[quad + 1]]);
                            splitVertices.Add(sourceVertices[indexes[quad + 2]]);
                            splitVertices.Add(sourceVertices[indexes[quad + 3]]);

                            vertexIndex += 4;
                        }
                    }
                    break;

                    default:
                        throw new NotSupportedException("ProBuilder only supports importing triangle and quad meshes.");
                }
            }

            m_Vertices = splitVertices.ToArray();

            m_Destination.Clear();
            m_Destination.SetVertices(m_Vertices);
            m_Destination.faces = faces;
            m_Destination.sharedVertices = SharedVertex.GetSharedVerticesWithPositions(m_Destination.positionsInternal);
            m_Destination.sharedTextures = new SharedVertex[0];

            if (importSettings.quads)
            {
                var newFaces = m_Destination.ToQuads(m_Destination.facesInternal, !importSettings.smoothing);
            }

            if (importSettings.smoothing)
            {
                Smoothing.ApplySmoothingGroups(m_Destination, m_Destination.facesInternal, importSettings.smoothingAngle, m_Vertices.Select(x => x.normal).ToArray());
                // After smoothing has been applied go back and weld coincident vertices created by MergePairs.
                MergeElements.CollapseCoincidentVertices(m_Destination, m_Destination.facesInternal);
            }
        }

    }
}

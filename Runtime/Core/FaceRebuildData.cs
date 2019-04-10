using System;
using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Information required to append a face to a pb_Object.
    /// </summary>
    sealed class FaceRebuildData
    {
#pragma warning disable 0649
        // new pb_Face
        public Face face;
        // new vertices (all vertices required to rebuild, not just new)
        public List<Vertex> vertices;
        // shared indexes pointers (must match vertices length)
        public List<int> sharedIndexes;
        // shared UV indexes pointers (must match vertices length)
        public List<int> sharedIndexesUV;
        // The offset applied to this face via Apply() call.
        private int _appliedOffset = 0;
#pragma warning restore 0649

        /**
         * If this face has been applied to a pb_Object via Apply() this returns the index offset applied.
         */
        public int Offset()
        {
            return _appliedOffset;
        }

        public override string ToString()
        {
            return string.Format("{0}\n{1}", vertices.ToString(", "), sharedIndexes.ToString(", "));
        }

        public static void Apply(
            IEnumerable<FaceRebuildData> newFaces,
            ProBuilderMesh mesh,
            List<Vertex> vertices = null,
            List<Face> faces = null)
        {
            if (faces == null)
                faces = new List<Face>(mesh.facesInternal);

            if (vertices == null)
                vertices = new List<Vertex>(mesh.GetVertices());

            var lookup = mesh.sharedVertexLookup;
            var lookupUV = mesh.sharedTextureLookup;

            Apply(newFaces, vertices, faces, lookup, lookupUV);

            mesh.SetVertices(vertices);
            mesh.faces = faces;
            mesh.SetSharedVertices(lookup);
            mesh.SetSharedTextures(lookupUV);
        }

        /// <summary>
        /// Shift face rebuild data to appropriate positions and update the vertex, face, and shared indexes arrays.
        /// </summary>
        /// <param name="newFaces"></param>
        /// <param name="vertices"></param>
        /// <param name="faces"></param>
        /// <param name="sharedVertexLookup"></param>
        /// <param name="sharedTextureLookup"></param>
        public static void Apply(
            IEnumerable<FaceRebuildData> newFaces,
            List<Vertex> vertices,
            List<Face> faces,
            Dictionary<int, int> sharedVertexLookup,
            Dictionary<int, int> sharedTextureLookup = null)
        {
            int index = vertices.Count;

            foreach (FaceRebuildData rd in newFaces)
            {
                Face face = rd.face;
                int faceVertexCount = rd.vertices.Count;

                bool hasSharedIndexes = sharedVertexLookup != null && rd.sharedIndexes != null && rd.sharedIndexes.Count == faceVertexCount;
                bool hasSharedIndexesUV = sharedTextureLookup != null && rd.sharedIndexesUV != null && rd.sharedIndexesUV.Count == faceVertexCount;

                for (int n = 0; n < faceVertexCount; n++)
                {
                    int localIndex = n;

                    if (sharedVertexLookup != null)
                        sharedVertexLookup.Add(localIndex + index, hasSharedIndexes ? rd.sharedIndexes[localIndex] : -1);

                    if (sharedTextureLookup != null && hasSharedIndexesUV)
                        sharedTextureLookup.Add(localIndex + index, rd.sharedIndexesUV[localIndex]);
                }

                rd._appliedOffset = index;
                int[] faceIndexes = face.indexesInternal;

                for (int n = 0, c = faceIndexes.Length; n < c; n++)
                    faceIndexes[n] += index;

                index += rd.vertices.Count;
                face.indexesInternal = faceIndexes;
                faces.Add(face);
                vertices.AddRange(rd.vertices);
            }
        }
    }
}

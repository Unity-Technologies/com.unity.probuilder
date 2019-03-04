using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations
{
	public static class CombineMeshes
	{
        /// <summary>
        /// Given an array of "donors", this method returns a merged ProBuilderMesh.
        /// </summary>
        /// <param name="meshes"></param>
        /// <returns></returns>
        public static List<ProBuilderMesh> Combine(IEnumerable<ProBuilderMesh> meshes)
        {
            if (meshes == null)
                throw new ArgumentNullException("meshes");

            if (!meshes.Any() || meshes.Count() < 2)
                return null;

            var vertices = new List<Vertex>();
            var faces = new List<Face>();
            var autoUvFaces = new List<Face>();
            var sharedVertices = new List<SharedVertex>();
            var sharedTextures = new List<SharedVertex>();
            int offset = 0;
            var materialMap = new List<Material>();

            foreach (var mesh in meshes)
            {
                var meshVertexCount = mesh.vertexCount;
                var transform = mesh.transform;
                var meshVertices = mesh.GetVertices();
                var meshFaces = mesh.facesInternal;
                var meshSharedVertices = mesh.sharedVertices;
                var meshSharedTextures = mesh.sharedTextures;
                var materials = mesh.renderer.sharedMaterials;
                var materialCount = materials.Length;

                for (int i = 0; i < meshVertexCount; i++)
                    vertices.Add(transform.TransformVertex(meshVertices[i]));

                foreach (var face in meshFaces)
                {
                    var newFace = new Face(face);
                    newFace.ShiftIndexes(offset);

                    // prevents uvs from shifting when being converted from local coords to world space
                    if (!newFace.manualUV && !newFace.uv.useWorldSpace)
                    {
                        newFace.manualUV = true;
                        autoUvFaces.Add(newFace);
                    }
                    var material = materials[Math.Clamp(face.submeshIndex, 0, materialCount - 1)];
                    var submeshIndex = materialMap.IndexOf(material);

                    if (submeshIndex > -1)
                    {
                        newFace.submeshIndex = submeshIndex;
                    }
                    else
                    {
                        if (material == null)
                        {
                            newFace.submeshIndex = 0;
                        }
                        else
                        {
                            newFace.submeshIndex = materialMap.Count;
                            materialMap.Add(material);
                        }
                    }

                    faces.Add(newFace);
                }

                foreach (var sv in meshSharedVertices)
                {
                    var nsv = new SharedVertex(sv);
                    nsv.ShiftIndexes(offset);
                    sharedVertices.Add(nsv);
                }

                foreach (var st in meshSharedTextures)
                {
                    var nst = new SharedVertex(st);
                    nst.ShiftIndexes(offset);
                    sharedTextures.Add(nst);
                }

                offset += meshVertexCount;
            }

            var res = InternalMeshUtility.SplitByMaxVertexCount(vertices, faces, sharedVertices, sharedTextures);
            var pivot = meshes.LastOrDefault().transform.position;

            foreach (var m in res)
            {
                m.renderer.sharedMaterials = materialMap.ToArray();
                InternalMeshUtility.FilterUnusedSubmeshIndexes(m);
                m.SetPivot(pivot);
                UVEditing.SetAutoAndAlignUnwrapParamsToUVs(m, autoUvFaces);

            }
            return res;
        }
	}
}

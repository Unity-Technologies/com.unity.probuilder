using UnityEngine;
using ProBuilder2.Common;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder2.MeshOperations
{
	public static class pb_MeshImporter
	{
		/**
		 * Import a mesh onto an empty pb_Object.
		 */
		public static bool Import(MeshFilter meshFilter, pb_Object target)
		{
			Mesh mesh = meshFilter.sharedMesh;
			pb_Vertex[] vertices = pb_Vertex.GetVertices(mesh);
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
						break;
				}
			}

			target.Clear();
			target.SetVertices(vertices);
			target.SetFaces(faces);
			target.SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(target.vertices));

			return false;
		}
	}
}

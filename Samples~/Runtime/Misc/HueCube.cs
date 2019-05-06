#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine;
using UnityEngine.ProBuilder;

namespace ProBuilder.Examples
{
	/// <summary>
	/// Creates a cube on start and colors it's vertices programatically.
	/// </summary>
	sealed class HueCube : MonoBehaviour
	{
		ProBuilderMesh m_Mesh;

		void Start()
		{
			// Create a new ProBuilder cube to work with.
			m_Mesh = ShapeGenerator.CreateShape(ShapeType.Cube);

			// Cycle through each unique vertex in the cube (8 total), and assign a color
			// to the index in the sharedIndices array.
			int sharedVertexCount = m_Mesh.sharedVertices.Count;

			Color[] vertexColors = new Color[sharedVertexCount];

			for(int i = 0; i < sharedVertexCount; i++)
			{
				vertexColors[i] = Color.HSVToRGB((i/(float)sharedVertexCount) * 360f, 1f, 1f);
			}

			// Now go through each face (vertex colors are stored the pb_Face class) and
			// assign the pre-calculated index color to each index in the triangles array.
			var colors = m_Mesh.colors;

			for(int sharedIndex = 0; sharedIndex < m_Mesh.sharedVertices.Count; sharedIndex++)
			{
				foreach(int index in m_Mesh.sharedVertices[sharedIndex])
				{
					colors[index] = vertexColors[sharedIndex];
				}
			}

			m_Mesh.colors = colors;

			// In order for these changes to take effect, you must refresh the mesh
			// object.
			m_Mesh.Refresh();
		}
	}
}
#endif

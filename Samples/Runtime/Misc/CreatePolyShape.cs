using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace ProBuilder.Examples
{
	public class CreatePolyShape : MonoBehaviour
	{
		public float m_RadiusMin = 1.5f;
		public float m_RadiusMax = 2f;
		public float m_Height = 1f;
		public bool m_FlipNormals = false;

		ProBuilderMesh m_Mesh;

		void Start()
		{
			// Create a new GameObject
			var go = new GameObject();

			// Add a ProBuilderMesh component (ProBuilder mesh data is stored here)
			m_Mesh = go.gameObject.AddComponent<ProBuilderMesh>();

			InvokeRepeating("Rebuild", 0f, .1f);
		}

		void Rebuild()
		{
			// Create a circle of points with randomized distance from origin.
			Vector3[] points = new Vector3[32];

			for (int i = 0, c = points.Length; i < c; i++)
			{
				float angle = Mathf.Deg2Rad * ((i / (float)c) * 360f);
				points[i] = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * Random.Range(m_RadiusMin, m_RadiusMax);
			}

			// CreateShapeFromPolygon is an extension method that sets the pb_Object mesh data with vertices and faces
			// generated from a polygon path.
			m_Mesh.CreateShapeFromPolygon(points, m_Height, m_FlipNormals);
		}
	}
}

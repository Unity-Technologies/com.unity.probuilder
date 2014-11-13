using UnityEngine;

namespace Parabox.CSG
{
	struct CSG_Vertex
	{
		public Vector3 position;
		public Vector3 normal;
		public Vector2 uv;

		public CSG_Vertex(Vector3 position, Vector3 normal, Vector2 uv)
		{
			this.position = position;
			this.normal = normal;
			this.uv = uv;
		}

		public void Flip()
		{
			normal *= -1f;
		}

		// Create a new vertex between this vertex and `other` by linearly
		// interpolating all properties using a parameter of `t`. Subclasses should
		// override this to interpolate additional properties.
		public static CSG_Vertex Interpolate(CSG_Vertex a, CSG_Vertex b, float t)
		{
			CSG_Vertex ret = new CSG_Vertex();

			ret.position = Vector3.Lerp(a.position, b.position, t);
			ret.normal = Vector3.Lerp(a.normal, b.normal, t);
			ret.uv = Vector2.Lerp(a.uv, b.uv, t);

			return ret;
		}
	}
}
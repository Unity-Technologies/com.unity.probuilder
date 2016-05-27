using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.Common
{
	/**
	 *	Holds information about a single vertex, and provides methods for averaging between many.
	 */
	public class pb_Vertex : System.IEquatable<pb_Vertex>
	{
		public Vector3 position;
		public Color color;
		public Vector3 normal;
		public Vector4 tangent;
		public Vector2 uv0;
		public Vector2 uv2;
		public Vector4 uv3;
		public Vector4 uv4;

		public bool hasPosition	= false;
		public bool hasColor	= false;
		public bool hasNormal	= false;
		public bool hasTangent	= false;
		public bool hasUv0		= false;
		public bool hasUv2		= false;
		public bool hasUv3		= false;
		public bool hasUv4		= false;

		public pb_Vertex() {}

		/**
		 *	New pb_Vertex from a vertex index in pb_Object.
		 */
		[System.Obsolete]
		public pb_Vertex(pb_Object pb, int index)
		{
			int vertexCount = pb.vertexCount;
			Mesh m = pb.msh;
			bool hasMesh = m != null;

			this.position = pb.vertices[index];
			this.color = pb.colors[index];
			this.uv0 = pb.uv[index];
			this.hasPosition = true;
			this.hasColor = true;
			this.hasUv0 = true;

			if(hasMesh && m.normals != null && m.normals.Length == vertexCount)
			{
				this.hasNormal = true;
				this.normal = m.normals[index];
			}

			if(hasMesh && m.tangents != null && m.tangents.Length == vertexCount)
			{
				this.hasTangent = true;
				this.tangent = m.tangents[index];
			}

			if(hasMesh && m.uv2 != null && m.uv2.Length == vertexCount)
			{
				this.hasUv2 = true;
				this.uv2 = m.uv2[index];
			}

			if(pb.hasUv3)
			{
				this.hasUv3 = true;
				this.uv3 = pb.uv3[index];
			}

			if(pb.hasUv4)
			{
				this.hasUv4 = true;
				this.uv4 = pb.uv4[index];
			}
		}

		public override bool Equals(object other)
		{
			return other is pb_Vertex && this.Equals(other as pb_Vertex);
		}

		public bool Equals(pb_Vertex other)
		{
			return other != null && this.position == other.position;
		}

		public override int GetHashCode()
		{
			return -1;
		}

		/**
		 *	Copy constructor.
		 */
		public pb_Vertex(pb_Vertex v)
		{
			this.position 	= v.position;
			this.hasPosition = v.hasPosition;
			this.color 		= v.color;
			this.hasColor 	= v.hasColor;
			this.uv0 		= v.uv0;
			this.hasUv0 	= v.hasUv0;
			this.normal 	= v.normal;
			this.hasNormal 	= v.hasNormal;
			this.tangent 	= v.tangent;
			this.hasTangent = v.hasTangent;
			this.uv2 		= v.uv2;
			this.hasUv2 	= v.hasUv2;
			this.uv3 		= v.uv3;
			this.hasUv3 	= v.hasUv3;
			this.uv4 		= v.uv4;
			this.hasUv4 	= v.hasUv4;
		}

		public override string ToString()
		{
			return this.position.ToString();
		}

		/**
		 *	Creates a new array of pb_Vertex with the provide pb_Object data.
		 */
		public static pb_Vertex[] GetVertices(pb_Object pb)
		{
			int vertexCount = pb.vertexCount;

			pb_Vertex[] v = new pb_Vertex[vertexCount];

			Vector3[] positions = pb.vertices;
			Color[] colors 		= pb.colors;
			Vector2[] uv0s 		= pb.uv;

			Vector3[] normals 	= pb.msh.normals;
			Vector4[] tangents 	= pb.msh.tangents;
			Vector2[] uv2s 		= pb.msh.uv2;

			List<Vector4> uv3s = new List<Vector4>();
			List<Vector4> uv4s = new List<Vector4>();

			pb.GetUVs(2, uv3s);
			pb.GetUVs(3, uv4s);

			bool _hasPositions	= positions != null && positions.Count() == vertexCount;
			bool _hasColors		= colors != null && colors.Count() == vertexCount;
			bool _hasNormals	= normals != null && normals.Count() == vertexCount;
			bool _hasTangents	= tangents != null && tangents.Count() == vertexCount;
			bool _hasUv0		= uv0s != null && uv0s.Count() == vertexCount;
			bool _hasUv2		= uv2s != null && uv2s.Count() == vertexCount;
			bool _hasUv3		= uv3s != null && uv3s.Count() == vertexCount;
			bool _hasUv4		= uv4s != null && uv4s.Count() == vertexCount;

			for(int i = 0; i < vertexCount; i++)
			{
				if( _hasPositions )	{ v[i].hasPosition = true; v[i].position = positions[i]; }
				if( _hasColors ) 	{ v[i].hasColor = true; v[i].color = colors[i]; }
				if( _hasNormals ) 	{ v[i].hasNormal = true; v[i].normal = normals[i]; }
				if( _hasTangents ) 	{ v[i].hasTangent = true; v[i].tangent = tangents[i]; }
				if( _hasUv0 ) 		{ v[i].hasUv0 = true; v[i].uv0 = uv0s[i]; }
				if( _hasUv2 ) 		{ v[i].hasUv2 = true; v[i].uv2 = uv2s[i]; }
				if( _hasUv3 ) 		{ v[i].hasUv3 = true; v[i].uv3 = uv3s[i]; }
				if( _hasUv4 ) 		{ v[i].hasUv4 = true; v[i].uv4 = uv4s[i]; }
			}

			return v;
		}

		public static pb_Vertex[] GetVertices(Mesh m)
		{
			if(m == null)
				return null;

			int vertexCount = m.vertexCount;
			pb_Vertex[] v = new pb_Vertex[vertexCount];

			Vector3[] positions = m.vertices;
			Color[] colors 		= m.colors;
			Vector3[] normals 	= m.normals;
			Vector4[] tangents 	= m.tangents;
			Vector2[] uv0s 		= m.uv;
			Vector2[] uv2s 		= m.uv2;
			List<Vector4> uv3s = new List<Vector4>();
			List<Vector4> uv4s = new List<Vector4>();
			m.GetUVs(2, uv3s);
			m.GetUVs(3, uv4s);

			bool _hasPositions	= positions != null && positions.Count() == vertexCount;
			bool _hasColors		= colors != null && colors.Count() == vertexCount;
			bool _hasNormals	= normals != null && normals.Count() == vertexCount;
			bool _hasTangents	= tangents != null && tangents.Count() == vertexCount;
			bool _hasUv0		= uv0s != null && uv0s.Count() == vertexCount;
			bool _hasUv2		= uv2s != null && uv2s.Count() == vertexCount;
			bool _hasUv3		= uv3s != null && uv3s.Count() == vertexCount;
			bool _hasUv4		= uv4s != null && uv4s.Count() == vertexCount;

			for(int i = 0; i < vertexCount; i++)
			{
				v[i] = new pb_Vertex();

				if( _hasPositions )	{ v[i].hasPosition = true; v[i].position = positions[i]; }
				if( _hasColors ) 	{ v[i].hasColor = true; v[i].color = colors[i]; }
				if( _hasNormals ) 	{ v[i].hasNormal = true; v[i].normal = normals[i]; }
				if( _hasTangents ) 	{ v[i].hasTangent = true; v[i].tangent = tangents[i]; }
				if( _hasUv0 ) 		{ v[i].hasUv0 = true; v[i].uv0 = uv0s[i]; }
				if( _hasUv2 ) 		{ v[i].hasUv2 = true; v[i].uv2 = uv2s[i]; }
				if( _hasUv3 ) 		{ v[i].hasUv3 = true; v[i].uv3 = uv3s[i]; }
				if( _hasUv4 ) 		{ v[i].hasUv4 = true; v[i].uv4 = uv4s[i]; }
			}

			return v;
		}

		public static void GetArrays(	IList<pb_Vertex> vertices,
										out Vector3[] position,
										out Color[] color,
										out Vector2[] uv0,
										out Vector3[] normal,
										out Vector4[] tangent,
										out Vector2[] uv2,
										out List<Vector4> uv3,
										out List<Vector4> uv4)
		{
			position 	= vertices.Select(x => x.position).ToArray();
			color 		= vertices.Select(x => x.color).ToArray();
			uv0 		= vertices.Select(x => x.uv0).ToArray();

			// normal		= vertices.All(x => x.hasNormal) ? vertices.Select(x => (Vector3) x.normal).ToArray() : null;
			// tangent		= vertices.All(x => x.hasTangent) ? vertices.Select(x => (Vector4) x.tangent).ToArray() : null;
			// uv2			= vertices.All(x => x.hasUv2) ? vertices.Select(x => (Vector2) x.uv2).ToArray() : null;
			// uv3			= vertices.All(x => x.hasUv3) ? vertices.Select(x => (Vector4) x.uv3).ToList() : null;
			// uv4			= vertices.All(x => x.hasUv4) ? vertices.Select(x => (Vector4) x.uv4).ToList() : null;

			normal		= vertices[0].hasNormal ? vertices.Select(x => (Vector3) x.normal).ToArray() : null;
			tangent		= vertices[0].hasTangent ? vertices.Select(x => (Vector4) x.tangent).ToArray() : null;
			uv2			= vertices[0].hasUv2 ? vertices.Select(x => (Vector2) x.uv2).ToArray() : null;
			uv3			= vertices[0].hasUv3 ? vertices.Select(x => (Vector4) x.uv3).ToList() : null;
			uv4			= vertices[0].hasUv4 ? vertices.Select(x => (Vector4) x.uv4).ToList() : null;
		}

		/**
		 *	Replace mesh values with vertex array.  This function clears the mesh, so be sure to set triangles after.
		 */
		public static void SetMesh(Mesh m, pb_Vertex[] vertices)
		{
			Vector3[] positions	= null;
 			Color[] colors		= null;
 			Vector2[] uv0s		= null;
 			Vector3[] normals	= null;
 			Vector4[] tangents	= null;
 			Vector2[] uv2s		= null;
 			List<Vector4> uv3s	= null;
 			List<Vector4> uv4s	= null;

			GetArrays(vertices,	out positions,
								out colors,
								out uv0s,
								out normals,
								out tangents,
								out uv2s,
								out uv3s,
								out uv4s);

			m.Clear();
			m.vertices = positions;
			m.colors = colors;
			m.uv = uv0s;
			m.normals = normals;
			m.tangents = tangents;
			m.uv2 = uv2s;

			if(uv3s != null) m.SetUVs(2, uv3s);
			if(uv4s != null) m.SetUVs(3, uv4s);
		}

		/**
		 *	Average all vertices to a single vertex.
		 */
		public static pb_Vertex Average(IList<pb_Vertex> vertices, IList<int> indices = null)
		{
			pb_Vertex v = new pb_Vertex();

			int vertexCount = indices != null ? indices.Count : vertices.Count;

			int normalCount = 0,
				tangentCount = 0,
				uv2Count = 0,
				uv3Count = 0,
				uv4Count = 0;

			for(int i = 0; i < vertexCount; i++)
			{
				int index = indices == null ? i : indices[i];

				v.position 	+= vertices[index].position;
				v.color 	+= vertices[index].color;
				v.uv0 		+= vertices[index].uv0;

				if(vertices[index].hasNormal) {
					normalCount++;
					v.normal += vertices[index].normal;
				}

				if(vertices[index].hasTangent) {
					tangentCount++;
					v.tangent += vertices[index].tangent;
				}

				if(vertices[index].hasUv2) {
					uv2Count++;
					v.uv2 += vertices[index].uv2;
				}

				if(vertices[index].hasUv3) {
					uv3Count++;
					v.uv3 += vertices[index].uv3;
				}

				if(vertices[index].hasUv4) {
					uv4Count++;
					v.uv4 += vertices[index].uv4;
				}
			}

			v.position 	*= (1f/vertexCount);
			v.color 	*= (1f/vertexCount);
			v.uv0 		*= (1f/vertexCount);

			v.normal 	*= (1f/normalCount);
			v.tangent 	*= (1f/tangentCount);
			v.uv2 		*= (1f/uv2Count);
			v.uv3 		*= (1f/uv3Count);
			v.uv4 		*= (1f/uv4Count);

			return v;
		}

		/**
		 *	Returns a new vertex mixed between x and y.  1 is fully y, 0 is fully x.
		 */
		public static pb_Vertex Mix(pb_Vertex x, pb_Vertex y, float a)
		{
			float i = 1f - a;

			pb_Vertex v = new pb_Vertex();

			v.position 	= x.position * i + y.position * a;
			v.color 	= x.color * i + y.color * a;
			v.uv0 		= x.uv0 * i + y.uv0 * a;

			if(x.hasNormal && y.hasNormal)
				v.normal = x.normal * i + y.normal * a;
			else if(x.hasNormal)
				v.normal = x.normal;
			else if(y.hasNormal)
				v.normal = y.normal;

			if(x.hasTangent && y.hasTangent)
				v.tangent = x.tangent * i + y.tangent * a;
			else if(x.hasTangent)
				v.tangent = x.tangent;
			else if(y.hasTangent)
				v.tangent = y.tangent;

			if(x.hasUv2 && y.hasUv2)
				v.uv2 = x.uv2 * i + y.uv2 * a;
			else if(x.hasUv2)
				v.uv2 = x.uv2;
			else if(y.hasUv2)
				v.uv2 = y.uv2;

			if(x.hasUv3 && y.hasUv3)
				v.uv3 = x.uv3 * i + y.uv3 * a;
			else if(x.hasUv3)
				v.uv3 = x.uv3;
			else if(y.hasUv3)
				v.uv3 = y.uv3;

			if(x.hasUv4 && y.hasUv4)
				v.uv4 = x.uv4 * i + y.uv4 * a;
			else if(x.hasUv4)
				v.uv4 = x.uv4;
			else if(y.hasUv4)
				v.uv4 = y.uv4;

			return v;
		}
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.Common
{
	/**
	 *	Holds information about a single vertex, and provides methods for averaging between many.
	 */
	public class pb_Vertex
	{
		public Vector3 	position 	= Vector3.zero;
		public Color	color 		= Color.white;
		public Vector2	uv0 		= Vector2.zero;

		public Vector3? normal 		= null;
		public Vector4?	tangent 	= null;
		public Vector2?	uv2 		= null;
		public Vector4?	uv3 		= null;
		public Vector4?	uv4 		= null;

		public pb_Vertex() {}

		/**
		 *	New pb_Vertex from a vertex index in pb_Object.
		 */
		public pb_Vertex(pb_Object pb, int index)
		{
			int vertexCount = pb.vertexCount;
			Mesh m = pb.msh;
			bool hasMesh = m != null;

			this.position = pb.vertices[index];
			this.color = pb.colors[index];
			this.uv0 = pb.uv[index];

			if(hasMesh && m.normals != null && m.normals.Length == vertexCount)
				this.normal = m.normals[index];

			if(hasMesh && m.tangents != null && m.tangents.Length == vertexCount)
				this.tangent = m.tangents[index];

			if(hasMesh && m.uv2 != null && m.uv2.Length == vertexCount)
				this.uv2 = m.uv2[index];

			if(pb.hasUv3) this.uv3 = pb.uv3[index];

			if(pb.hasUv4) this.uv4 = pb.uv4[index];
		}

		public static pb_Vertex[] CreateArray(pb_Object pb)
		{
			pb_Vertex[] v = new pb_Vertex[pb.vertexCount];
			for(int i = 0; i < pb.vertexCount; i++)
				v[i] = new pb_Vertex(pb, i);
			return v;
		}

		public static void GetArrays(	pb_Vertex[] vertices,
										out Vector3[] position,
										out Color[] color,
										out Vector2[] uv0,
										out Vector3[] normal,
										out Vector4[] tangent,
										out Vector2[] uv2,
										out Vector4[] uv3,
										out Vector4[] uv4)
		{
			position 	= vertices.Select(x => x.position).ToArray();
			color 		= vertices.Select(x => x.color).ToArray();
			uv0 		= vertices.Select(x => x.uv0).ToArray();

			normal		= vertices.Any(x => x.normal != null) ? vertices.Select(x => (Vector3) x.normal).ToArray() : null;
			tangent		= vertices.Any(x => x.tangent != null) ? vertices.Select(x => (Vector4) x.tangent).ToArray() : null;
			uv2			= vertices.Any(x => x.uv2 != null) ? vertices.Select(x => (Vector2) x.uv2).ToArray() : null;
			uv3			= vertices.Any(x => x.uv3 != null) ? vertices.Select(x => (Vector4) x.uv3).ToArray() : null;
			uv4			= vertices.Any(x => x.uv4 != null) ? vertices.Select(x => (Vector4) x.uv4).ToArray() : null;
		}

		/**
		 *	Average all vertices to a single vertices.
		 */
		public static pb_Vertex Average(IList<pb_Vertex> vertices)
		{
			pb_Vertex v = new pb_Vertex();

			int vertexCount = vertices.Count;

			int normalCount = 0,
				tangentCount = 0,
				uv2Count = 0,
				uv3Count = 0,
				uv4Count = 0;

			for(int i = 0; i < vertexCount; i++)
			{
				v.position += vertices[i].position;
				v.color += vertices[i].color;
				v.uv0 += vertices[i].uv0;

				if(vertices[i].normal != null)
				{
					normalCount++;
					v.normal += vertices[i].normal;
				}

				if(vertices[i].tangent != null)
				{
					tangentCount++;
					v.tangent += vertices[i].tangent;
				}

				if(vertices[i].uv2 != null)
				{
					uv2Count++;
					v.uv2 += vertices[i].uv2;
				}

				if(vertices[i].uv3 != null)
				{
					uv3Count++;
					v.uv3 += vertices[i].uv3;
				}

				if(vertices[i].uv4 != null)
				{
					uv4Count++;
					v.uv4 += vertices[i].uv4;
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
	}
}

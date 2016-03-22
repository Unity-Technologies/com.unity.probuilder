using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	/**
	 *	description
	 */
	public class pb_Vertex
	{
		public Vector3 	position 	= Vector3.zero;
		public Color	color 		= Color.white;
		public Vector4	uv0 		= Vector4.zero;

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
			bool hasMesh = pb.msh != null;

			this.position = pb.vertices[index];
			this.color = pb.colors[index];
			this.uv0 = pb.uv0[index];

			if(hasMesh && pb.msh.normals != null && pb.msh.normals.Length == vertexCount)
				this.normal = pb.msh.normals[index];

			if(hasMesh && pb.msh.tangents != null && pb.msh.tangents.Length == vertexCount)
				this.tangent = pb.msh.tangents[index];

			if(hasMesh && pb.msh.uv2 != null && pb.msh.uv2.Length == vertexCount)
				this.uv2 = pb.msh.uv2[index];

			if(pb.uv3 != null && pb.uv3.Count == vertexCount)
				this.uv3 = pb.uv3[index];

			if(pb.uv4 != null && pb.uv4.Count == vertexCount)
				this.uv4 = pb.uv4[index];
		}

		public static pb_Vertex[] CreateArray(pb_Object pb)
		{
			pb_Vertex[] v = new pb_Vertex[pb.vertexCount];
			for(int i = 0; i < pb.vertexCount; i++)
				v[i] = new pb_Vertex(pb, i);
			return v;
		}

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

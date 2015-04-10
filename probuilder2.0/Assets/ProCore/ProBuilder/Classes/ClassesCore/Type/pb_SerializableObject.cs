using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

using ProBuilder2.Common;

namespace ProBuilder2.Serialization
{
	[Serializable()]		
	public class pb_SerializableObject : ISerializable
	{
		// pb_Object
		public Vector3[] 	vertices;
		public Vector2[] 	uv;
		public Color[]		color;
		public pb_Face[] 	faces;
		public int[][] 		sharedIndices;
		public int[][] 		sharedIndicesUV;

		// transform
		public Vector3 		t_position;
		public Quaternion 	t_rotation;
		public Vector3 		t_scale;

		/**
		 * Getters
		 */
		public Vector3[]	GetVertices() { return vertices; }
		public Vector2[]	GetUvs() { return uv; }
		public Color[]		GetColors() { return color; }
		public pb_Face[]	GetFaces() { return faces; }
		public int[][]		GetSharedIndices() { return sharedIndices; }
		public int[][]		GetSharedIndicesUV() { return sharedIndicesUV; }

		/**
		 * Default constructor to appease serializers
		 */
		public pb_SerializableObject() {}

		public pb_SerializableObject(pb_Object pb)
		{
			this.vertices = pb.vertices;
			this.uv = pb.uv;
			this.color = pb.colors;
			this.faces = pb.faces;
			this.sharedIndices = (int[][])pb.GetSharedIndices().ToArray();
			this.sharedIndicesUV = (int[][])pb.GetSharedIndicesUV().ToArray();

			// Transform
			this.t_position = pb.transform.position;
			this.t_rotation = pb.transform.localRotation;
			this.t_scale = pb.transform.localScale;
		}

		public void Print()
		{
			Debug.Log(	"vertices: " + vertices.ToFormattedString(", ") +
						"\nuv: " + uv.ToFormattedString(", ") +
						"\nsharedIndices: " + ((pb_IntArray[])sharedIndices.ToPbIntArray()).ToFormattedString(", ") +
						"\nfaces: " + faces.ToFormattedString(", ")
						);
		}

		// OnSerialize
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			// pb_object
			info.AddValue("vertices", 			System.Array.ConvertAll(vertices, x => (pb_Vector3)x),	typeof(pb_Vector3[]));
			info.AddValue("uv", 				System.Array.ConvertAll(uv, x => (pb_Vector2)x), 		typeof(pb_Vector2[]));
			info.AddValue("color", 				System.Array.ConvertAll(color, x => (pb_Color)x), 		typeof(pb_Color[]));
			info.AddValue("faces", 				faces, 													typeof(pb_Face[]));
			info.AddValue("sharedIndices", 		sharedIndices, 											typeof(int[][]));
			info.AddValue("sharedIndicesUV",	sharedIndicesUV, 										typeof(int[][]));

			// transform
			info.AddValue("t_position", 		(pb_Vector3)t_position,									typeof(pb_Vector3));
			info.AddValue("t_rotation", 		(pb_Vector4)t_rotation,									typeof(pb_Vector4));
			info.AddValue("t_scale", 			(pb_Vector3)t_scale, 									typeof(pb_Vector3));
		}

		// The pb_SerializableObject constructor is used to deserialize values. 
		public pb_SerializableObject(SerializationInfo info, StreamingContext context)
		{
			/// Vertices
			pb_Vector3[] pb_vertices = (pb_Vector3[]) info.GetValue("vertices", typeof(pb_Vector3[]));
			this.vertices = System.Array.ConvertAll(pb_vertices, x => (Vector3)x);
			
			/// UVs
			pb_Vector2[] pb_uv = (pb_Vector2[]) info.GetValue("uv", typeof(pb_Vector2[]));
			this.uv = System.Array.ConvertAll(pb_uv, x => (Vector2)x);
			
			/// Colors
			pb_Color[] pb_color = (pb_Color[]) info.GetValue("color", typeof(pb_Color[]));
			this.color = System.Array.ConvertAll(pb_color, x => (Color)x);

			/// Faces
			this.faces = (pb_Face[]) info.GetValue("faces", typeof(pb_Face[]));

			// Shared Indices
			this.sharedIndices = (int[][]) info.GetValue("sharedIndices", typeof(int[][]));

			// Shared Indices UV
			this.sharedIndicesUV = (int[][]) info.GetValue("sharedIndicesUV", typeof(int[][]));

			this.t_position = (pb_Vector3) info.GetValue("t_position", typeof(pb_Vector3));
			this.t_rotation = (pb_Vector4) info.GetValue("t_rotation", typeof(pb_Vector4));
			this.t_scale = (pb_Vector3) info.GetValue("t_scale", typeof(pb_Vector3));
		}
	}
}
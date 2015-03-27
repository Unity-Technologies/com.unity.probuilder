using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using ProBuilder2.Common;

using UnityEngine;

/**
 *
 */
namespace ProBuilder2.UpgradeKit
{
	#if UNITY_WP8
	public class pb_SerializableObject
	{
		// pb_Object
		public Vector3[] vertices;
		public Vector2[] uv;
		public pb_Face[] faces;
		public int[][] sharedIndices;
		public int[][] sharedIndicesUV;
	}
	#else
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

		public pb_SerializableObject(pb_Object pb)
		{
			this.vertices = pb.vertices;
			this.uv = pb.uv;
			if(pb.msh != null && pb.msh.colors != null && pb.msh.colors.Length == pb.vertexCount)
			{
				this.color = pb.msh.colors;
			}
			else
			{
				this.color = new Color[pb.vertexCount];
				for(int i = 0; i < this.color.Length; i++)
					this.color[i] = Color.white;
			}
			this.faces = pb.faces;
			this.sharedIndices = (int[][])pb.GetSharedIndices().ToArray();
			this.sharedIndicesUV = (int[][])pb.GetSharedIndicesUV().ToArray();
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
		}
	}
	#endif
}
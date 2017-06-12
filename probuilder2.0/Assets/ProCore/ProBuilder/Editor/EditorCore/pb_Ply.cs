using UnityEngine;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Export options for Ply format.
	 */
	public class pb_PlyOptions
	{
		// Should the mesh be exported with a right handed coordinate system?
		public bool isRightHanded = true;
	}

	/**
	 *	Import and export of Ply files in Unity.
	 */
	public static class pb_Ply
	{
		public static bool Export(IEnumerable<pb_Object> models, out string contents, pb_PlyOptions options = null)
		{
			foreach(pb_Object pb in models)
			{
				pb.ToMesh();
				pb.Refresh();
			}

			int modelCount = models.Count();

			Vector3[] positions = models.SelectMany(x => x.vertices).ToArray();
			Vector3[] normals = models.SelectMany(x => x.msh.normals).ToArray();
			Color[] colors = models.SelectMany(x => x.colors).ToArray();

			List<int[]> faces = new List<int[]>(modelCount);
			int vertexOffset = 0;

			foreach(pb_Object pb in models)
			{
				IEnumerable<int[]> indices = pb.faces.Select(y => y.ToQuad() ?? y.indices);

				foreach(int[] face in indices)
					for(int y = 0; y < face.Length; y++)
						face[y] += vertexOffset;

				vertexOffset += pb.vertexCount;

				faces.AddRange(indices);
			}

			bool res = Export(positions, faces.ToArray(), out contents, normals, colors, options);

			foreach(pb_Object pb in models)
				pb.Optimize();

			return res;
		}

		/**
		 *	Create the contents of an ASCII formatted PLY file.
		 */
		public static bool Export(
			Vector3[] positions,
			int[][] faces, out string contents,
			Vector3[] normals = null,
			Color[] colors = null,
			pb_PlyOptions options = null)
		{
			if(options == null)
				options = new pb_PlyOptions();

			int faceCount = faces != null ? faces.Length : 0;
			int vertexCount = positions != null ? positions.Length : 0;

			if(vertexCount < 1 || faceCount < 1)
			{
				contents = null;
				return false;
			}

			bool hasNormals = normals != null && normals.Length == vertexCount;
			bool hasColors = colors != null && colors.Length == vertexCount;

			StringBuilder sb = new StringBuilder();

			WriteHeader(vertexCount, faceCount, hasNormals, hasColors, ref sb);

			for(int i = 0; i < vertexCount; i++)
			{
				sb.Append(string.Format("{0} {1} {2}", options.isRightHanded ? -positions[i].x : positions[i].x, positions[i].y, positions[i].z));

				if(hasNormals)
					sb.Append(string.Format(" {0} {1} {2}", options.isRightHanded ? -normals[i].x : -normals[i].x, normals[i].y, normals[i].z));

				if(hasColors)
					sb.Append(string.Format(" {0} {1} {2} {3}",
						System.Math.Min(System.Math.Max(0, (int)(colors[i].r * 255)), 255),
						System.Math.Min(System.Math.Max(0, (int)(colors[i].g * 255)), 255),
						System.Math.Min(System.Math.Max(0, (int)(colors[i].b * 255)), 255),
						System.Math.Min(System.Math.Max(0, (int)(colors[i].a * 255)), 255)));

				sb.AppendLine();
			}

			for(int i = 0; i < faceCount; i++)
			{
				int faceLength = faces[i] != null ? faces[i].Length : 0;
				sb.Append(faceLength.ToString());
				for(int n = 0; n < faceLength; n++)
					sb.Append(string.Format(" {0}", faces[i][ options.isRightHanded ? faceLength - n - 1 : n]));
				sb.AppendLine();
			}

			contents = sb.ToString();

			return true;
		}

		private static void WriteHeader(int vertexCount, int faceCount, bool hasNormals, bool hasColors, ref StringBuilder sb)
		{
			sb.AppendLine("ply");
			sb.AppendLine("format ascii 1.0");
			sb.AppendLine("comment Exported by [ProBuilder](http://www.procore3d.com/probuilder)");
			sb.AppendLine("comment " + System.DateTime.Now);
			sb.AppendLine(string.Format("element vertex {0}", vertexCount));
			sb.AppendLine("property float32 x");
			sb.AppendLine("property float32 y");
			sb.AppendLine("property float32 z");
			if(hasNormals)
			{
				sb.AppendLine("property float32 nx");
				sb.AppendLine("property float32 ny");
				sb.AppendLine("property float32 nz");
			}
			if(hasColors)
			{
				sb.AppendLine("property uint8 red");
				sb.AppendLine("property uint8 green");
				sb.AppendLine("property uint8 blue");
				sb.AppendLine("property uint8 alpha");
			}
			sb.AppendLine(string.Format("element face {0}", faceCount));
			sb.AppendLine("property list uint8 int32 vertex_index");
			sb.AppendLine("end_header");
		}
	}
}

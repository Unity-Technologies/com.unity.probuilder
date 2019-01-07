#pragma warning disable 0219

using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UnityEngine.ProBuilder.Stl
{
	/**
	 * Import methods for STL files.
	 */
	static class pb_Stl_Importer
	{
		const int MAX_FACETS_PER_MESH = 65535 / 3;

		class Facet
		{
			public Vector3 normal;
			public Vector3 a, b, c;

			public override string ToString()
			{
				return string.Format("{0:F2}: {1:F2}, {2:F2}, {3:F2}", normal, a, b, c);
			}
		}

		/**
		 * Import an STL file at path.
		 */
		public static Mesh[] Import(string path)
		{
			if( IsBinary(path) )
			{
				try
				{
					return ImportBinary(path);
				}
				catch(System.Exception e)
				{
					UnityEngine.Debug.LogWarning(string.Format("Failed importing mesh at path {0}.\n{1}", path, e.ToString()));
					return null;
				}
			}
			else
			{
				return ImportAscii(path);
			}
		}

		private static Mesh[] ImportBinary(string path)
		{
			Facet[] facets;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                {
                    // read header
                    byte[] header = br.ReadBytes(80);
                    uint facetCount = br.ReadUInt32();
                    facets = new Facet[facetCount];

                    for(uint i = 0; i < facetCount; i++)
                        facets[i] = br.GetFacet();
                }
            }

			return CreateMeshWithFacets(facets);
		}

        private static Facet GetFacet(this BinaryReader binaryReader)
        {
            Facet facet = new Facet();
            facet.normal = binaryReader.GetVector3();

            // maintain counter-clockwise orientation of vertices:
            facet.a = binaryReader.GetVector3();
            facet.c = binaryReader.GetVector3();
            facet.b = binaryReader.GetVector3();
            binaryReader.ReadUInt16(); // padding

            return facet;
        }
        private static Vector3 GetVector3(this BinaryReader binaryReader)
        {
            Vector3 vector3 = new Vector3();
            for (int i = 0; i < 3; i++)
                vector3[i] = binaryReader.ReadSingle();
            return vector3.UnityCoordTrafo();
        }

        private static Vector3 UnityCoordTrafo(this Vector3 vector3)
        {
            return new Vector3(-vector3.y, vector3.z, vector3.x);
        }

		const int SOLID = 1;
		const int FACET = 2;
		const int OUTER = 3;
		const int VERTEX = 4;
		const int ENDLOOP = 5;
		const int ENDFACET = 6;
		const int ENDSOLID = 7;
		const int EMPTY = 0;

		private static int ReadState(string line)
		{
			if(line.StartsWith("solid"))
				return SOLID;
			else if(line.StartsWith("facet"))
				return FACET;
			else if(line.StartsWith("outer"))
				return OUTER;
			else if(line.StartsWith("vertex"))
				return VERTEX;
			else if(line.StartsWith("endloop"))
				return ENDLOOP;
			else if(line.StartsWith("endfacet"))
				return ENDFACET;
			else if(line.StartsWith("endsolid"))
				return ENDSOLID;
			else
				return EMPTY;
		}

		private static Mesh[] ImportAscii(string path)
		{
			List<Facet> facets = new List<Facet>();

			using(StreamReader sr = new StreamReader(path))
			{
				string line;
				int state = EMPTY, vertex = 0;
				Facet f = null;
				bool exit = false;

				while(sr.Peek() > 0 && !exit)
				{
					line = sr.ReadLine().Trim();
					int previousState = state;
					state = ReadState(line);

					switch(state)
					{
						case SOLID:
							continue;

						case FACET:
							f = new Facet();
							f.normal = StringToVec3(line.Replace("facet normal ", ""));
						break;

						case OUTER:
							vertex = 0;
						break;

						case VERTEX:
                            // maintain counter-clockwise orientation of vertices:
                            if (vertex == 0) f.a = StringToVec3(line.Replace("vertex ", ""));
							else if(vertex == 2) f.c = StringToVec3(line.Replace("vertex ", ""));
                            else if (vertex == 1) f.b = StringToVec3(line.Replace("vertex ", ""));
                            vertex++;
						break;

						case ENDLOOP:
						break;

						case ENDFACET:
							facets.Add(f);
						break;

						case ENDSOLID:
							exit = true;
						break;

						case EMPTY:
						default:
						break;

					}
				}
			}

			return CreateMeshWithFacets(facets);
		}

		private static Vector3 StringToVec3(string str)
		{
			string[] split = str.Trim().Split(null);
			Vector3 v = new Vector3();

			float.TryParse(split[0], out v.x);
			float.TryParse(split[1], out v.y);
			float.TryParse(split[2], out v.z);

            return v.UnityCoordTrafo();
		}

		/**
		 * Read the first 80 bytes of a file and if they are all 0x0 it's likely
		 * that this file is binary.
		 */
		private static bool IsBinary(string path)
		{
			// http://stackoverflow.com/questions/968935/compare-binary-files-in-c-sharp
			FileInfo file = new FileInfo(path);

			if(file.Length < 130)
				return false;

			var isBinary = false;

			using(FileStream f0 = file.OpenRead())
			{
				using(BufferedStream bs0 = new BufferedStream(f0))
				{
					for(long i = 0; i < 80; i++)
					{
					    var readByte = bs0.ReadByte();
					    if (readByte == 0x0)
					    {
					        isBinary = true;
					        break;
					    }
					}
				}
			}

            if (!isBinary)
            {
                using (FileStream f0 = file.OpenRead())
                {
                    using (BufferedStream bs0 = new BufferedStream(f0))
                    {
                        var byteArray = new byte[6];

                        for (var i = 0; i < 6; i++)
                        {
                            byteArray[i] = (byte)bs0.ReadByte();
                        }

                        var text = Encoding.UTF8.GetString(byteArray);
                        isBinary = text != "solid ";
                    }
                }
            }

			return isBinary;
		}

		/**
		 * @todo test with > USHORT_MAX vertex count meshes
		 */
		private static Mesh[] CreateMeshWithFacets(IList<Facet> facets)
		{
			int fl = facets.Count, f = 0, mvc = MAX_FACETS_PER_MESH * 3;
			Mesh[] meshes = new Mesh[fl / MAX_FACETS_PER_MESH + 1];

			for(int i = 0; i < meshes.Length; i++)
			{
				int len = System.Math.Min(mvc, (fl - f) * 3);
				Vector3[] v = new Vector3[len];
				Vector3[] n = new Vector3[len];
				int[] t = new int[len];

				for(int it = 0; it < len; it += 3)
				{
					v[it  ] = facets[f].a;
					v[it+1] = facets[f].b;
					v[it+2] = facets[f].c;

					n[it  ] = facets[f].normal;
					n[it+1] = facets[f].normal;
					n[it+2] = facets[f].normal;

					t[it  ] = it;
					t[it+1] = it+1;
					t[it+2] = it+2;

					f++;
				}

				meshes[i] = new Mesh();
				meshes[i].vertices = v;
				meshes[i].normals = n;
				meshes[i].triangles = t;
			}

			return meshes;
		}
	}
}

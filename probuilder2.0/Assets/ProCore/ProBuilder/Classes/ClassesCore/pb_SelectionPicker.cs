// #define PB_DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.Common
{
	public static class pb_SelectionPicker
	{
		// RenderTextureFormat.Default
		private static RenderTextureFormat m_RenderTextureFormat = RenderTextureFormat.ARGBFloat;

		// Render formats that will work for selection picking.
		private static RenderTextureFormat[] m_AvailableFormats = new RenderTextureFormat[]
		{
			RenderTextureFormat.ARGB32,
			// RenderTextureFormat.Depth,
			RenderTextureFormat.ARGBHalf,
			// RenderTextureFormat.Shadowmap,
			// RenderTextureFormat.RGB565,
			// RenderTextureFormat.ARGB4444,
			// RenderTextureFormat.ARGB1555,
			RenderTextureFormat.Default,
			RenderTextureFormat.ARGB2101010,
			// RenderTextureFormat.DefaultHDR,
			RenderTextureFormat.ARGB64,
			RenderTextureFormat.ARGBFloat,
			// RenderTextureFormat.RGFloat,
			// RenderTextureFormat.RGHalf,
			// RenderTextureFormat.RFloat,
			// RenderTextureFormat.RHalf,
			// RenderTextureFormat.R8,
			// RenderTextureFormat.ARGBInt,
			// RenderTextureFormat.RGInt,
			// RenderTextureFormat.RInt,
			// RenderTextureFormat.BGRA32,
			RenderTextureFormat.RGB111110Float,
			// RenderTextureFormat.RG32,
#if UNITY_2017_1_OR_NEWER
			// RenderTextureFormat.RGBAUShort,
			// RenderTextureFormat.RG16
#endif
		};

		/**
		 *	Given a camera and selection rect (in screen space) return a Dictionary containing the number of faces touched
		 *	by the rect.

		 */
		public static Dictionary<pb_Object, HashSet<pb_Face>> PickFacesInRect(
			Camera camera,
			Rect pickerRect,
			IEnumerable<pb_Object> selection,
			int renderTextureWidth = -1,
			int renderTextureHeight = -1)
		{
			Dictionary<uint, pb_Tuple<pb_Object, pb_Face>> map;
			Texture2D tex = RenderSelectionPickerTexture(camera, selection, out map, renderTextureWidth, renderTextureHeight);

			#if PB_DEBUG
			System.IO.File.WriteAllBytes("Assets/scene.png", tex.EncodeToPNG());
			#endif

			Color32[] pix = tex.GetPixels32();

			int ox = System.Math.Max(0, Mathf.FloorToInt(pickerRect.x));
			int oy = System.Math.Max(0, Mathf.FloorToInt((tex.height - pickerRect.y) - pickerRect.height));
			int imageWidth = tex.width;
			int imageHeight = tex.height;
			int width = Mathf.FloorToInt(pickerRect.width);
			int height = Mathf.FloorToInt(pickerRect.height);
			GameObject.DestroyImmediate(tex);

			Dictionary<pb_Object, HashSet<pb_Face>> selected = new Dictionary<pb_Object, HashSet<pb_Face>>();
			pb_Tuple<pb_Object, pb_Face> hit;
			HashSet<pb_Face> faces = null;
			HashSet<uint> used = new HashSet<uint>();

			#if PB_DEBUG
			List<Color> rectImg = new List<Color>();
			#endif

			for(int y = oy; y < System.Math.Min(oy + height, imageHeight); y++)
			{
				for(int x = ox; x < System.Math.Min(ox + width, imageWidth); x++)
				{
					#if PB_DEBUG
					rectImg.Add(pix[y * imageWidth + x]);
					#endif

					uint v = pb_SelectionPicker.DecodeRGBA( pix[y * imageWidth + x] );

					if( used.Add(v) && map.TryGetValue(v, out hit) )
					{
						if(selected.TryGetValue(hit.Item1, out faces))
							faces.Add(hit.Item2);
						else
							selected.Add(hit.Item1, new HashSet<pb_Face>() { hit.Item2 });
					}
				}
			}

			#if PB_DEBUG
			if(width > 0 && height > 0)
			{
				Debug.Log("used: \n" + used.Select(x => string.Format("{0} ({1})", x, EncodeRGBA(x))).ToString("\n"));
				Texture2D img = new Texture2D(width, height);
				img.SetPixels(rectImg.ToArray());
				img.Apply();
				byte[] bytes = img.EncodeToPNG();
				System.IO.File.WriteAllBytes("Assets/rect.png", bytes);
				UnityEditor.AssetDatabase.Refresh();
				GameObject.DestroyImmediate(img);
			}
			#endif

			return selected;
		}

		/**
		 *	Given a camera and selection rect (in screen space) return a Dictionary containing the number of vertices touched
		 *	by the rect.
		 */
		public static Dictionary<pb_Object, HashSet<int>> PickVerticesInRect(
			Camera camera,
			Rect pickerRect,
			IEnumerable<pb_Object> selection,
			int renderTextureWidth = -1,
			int renderTextureHeight = -1)
		{
			Dictionary<uint, pb_Tuple<pb_Object, int>> map;
			Dictionary<pb_Object, HashSet<int>> selected = new Dictionary<pb_Object, HashSet<int>>();

#if PB_DEBUG
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			foreach(RenderTextureFormat tf in m_AvailableFormats)
			{
				if( !SystemInfo.SupportsRenderTextureFormat(tf) )
					continue;

				sb.AppendLine(tf.ToString());
				m_RenderTextureFormat = tf;
				List<Color> rectImg = new List<Color>();
				selected.Clear();
#endif

			Texture2D tex = RenderSelectionPickerTexture(camera, selection, out map, renderTextureWidth, renderTextureHeight);
			Color32[] pix = tex.GetPixels32();

			#if PB_DEBUG
			// System.IO.File.WriteAllBytes("Assets/scene.png", tex.EncodeToPNG());
			#endif

			int ox = System.Math.Max(0, Mathf.FloorToInt(pickerRect.x));
			int oy = System.Math.Max(0, Mathf.FloorToInt((tex.height - pickerRect.y) - pickerRect.height));
			int imageWidth = tex.width;
			int imageHeight = tex.height;
			int width = Mathf.FloorToInt(pickerRect.width);
			int height = Mathf.FloorToInt(pickerRect.height);
			GameObject.DestroyImmediate(tex);

			pb_Tuple<pb_Object, int> hit;
			HashSet<int> indices = null;
			HashSet<uint> used = new HashSet<uint>();

			for(int y = oy; y < System.Math.Min(oy + height, imageHeight); y++)
			{
				for(int x = ox; x < System.Math.Min(ox + width, imageWidth); x++)
				{
					uint v = pb_SelectionPicker.DecodeRGBA( pix[y * imageWidth + x] );

					#if PB_DEBUG
					rectImg.Add(pix[y * imageWidth + x]);
					#endif

					if( used.Add(v) && map.TryGetValue(v, out hit) )
					{
						if(selected.TryGetValue(hit.Item1, out indices))
							indices.Add(hit.Item2);
						else
							selected.Add(hit.Item1, new HashSet<int>() { hit.Item2 });
					}
				}
			}

			#if PB_DEBUG
				if(width > 0 && height > 0)
				{
					sb.AppendLine("  in rect: \n" + used.Select(x => string.Format("   {0:X6} ({1})", x, EncodeRGBA(x))).ToString("\n"));
					Texture2D img = new Texture2D(width, height);
					img.SetPixels(rectImg.ToArray());
					img.Apply();
					System.IO.File.WriteAllBytes("Assets/rect_" + tf.ToString() + ".png", img.EncodeToPNG());
					UnityEditor.AssetDatabase.Refresh();
					GameObject.DestroyImmediate(img);
				}
			}
			Debug.Log(sb.ToString());

			#endif

			return selected;
		}

		/**
		 *	Render the pb_Object selection with the special selection picker shader and return a texture and color -> {object, face} dictionary.
		 */
		public static Texture2D RenderSelectionPickerTexture(Camera camera, IEnumerable<pb_Object> selection, out Dictionary<uint, pb_Tuple<pb_Object, pb_Face>> map, int width = -1, int height = -1)
		{
			List<GameObject> depthGameObjects = GenerateFaceDepthTestMeshes(selection, out map);

			Texture2D tex = RenderWithReplacementShader(camera, pb_Constant.SelectionPickerShader, "ProBuilderPicker", width, height);

			foreach(GameObject go in depthGameObjects)
			{
				GameObject.DestroyImmediate(go.GetComponent<MeshFilter>().sharedMesh);
				GameObject.DestroyImmediate(go);
			}

			return tex;
		}

		/**
		 *	Render the pb_Object selection with the special selection picker shader and return a texture and color -> {object, sharedIndex} dictionary.
		 */
		public static Texture2D RenderSelectionPickerTexture(Camera camera, IEnumerable<pb_Object> selection, out Dictionary<uint, pb_Tuple<pb_Object, int>> map, int width = -1, int height = -1)
		{
			List<GameObject> depthGameObjects = GenerateVertexDepthTestMeshes(selection, out map);

			Texture2D tex = RenderWithReplacementShader(camera, pb_Constant.SelectionPickerShader, "ProBuilderPicker", width, height);

			foreach(GameObject go in depthGameObjects)
			{
				GameObject.DestroyImmediate(go.GetComponent<MeshFilter>().sharedMesh);
				GameObject.DestroyImmediate(go);
			}

			return tex;
		}

		/**
		 *	Generate a set of meshes and gameObjects that can be rendered for depth testing faces.
		 */
		public static List<GameObject> GenerateFaceDepthTestMeshes(IEnumerable<pb_Object> selection, out Dictionary<uint, pb_Tuple<pb_Object, pb_Face>> map)
		{
			List<GameObject> meshes = new List<GameObject>();
			map = new Dictionary<uint, pb_Tuple<pb_Object, pb_Face>>();

			uint index = 0;

			foreach(pb_Object pb in selection)
			{
				GameObject go = new GameObject();
				go.name = pb.name + " (Face Depth Test)";
				go.transform.position = pb.transform.position;
				go.transform.localRotation = pb.transform.localRotation;
				go.transform.localScale = pb.transform.localScale;

				Mesh m = new Mesh();
				m.vertices = pb.vertices;
				m.triangles = pb.faces.SelectMany(x => x.indices).ToArray();
				#if UNITY_4_7
				// avoid incorrect unity warning about missing uv channel on 4.7
				m.uv = new Vector2[pb.vertexCount];
				m.uv2 = new Vector2[pb.vertexCount];
				#endif

				Color32[] colors = new Color32[m.vertexCount];

				foreach(pb_Face f in pb.faces)
				{
					Color32 color = EncodeRGBA(index++);
					map.Add(DecodeRGBA(color), new pb_Tuple<pb_Object, pb_Face>(pb, f));

					for(int i = 0; i < f.distinctIndices.Length; i++)
						colors[f.distinctIndices[i]] = color;
				}

				m.colors32 = colors;

				// FacePickerMaterial may fail, and in that case we still want to clean up our mess
				try
				{
					go.AddComponent<MeshFilter>().sharedMesh = m;
					go.AddComponent<MeshRenderer>().sharedMaterial = pb_Constant.FacePickerMaterial;
				}
				catch
				{
					Debug.LogWarning("Could not find shader `pb_FacePicker.shader`.  Please re-import ProBuilder to fix!");
				}

				meshes.Add(go);
			}

			return meshes;
		}

		/**
		 *	Generate a set of meshes and gameObjects that can be rendered for depth testing vertex positions.
		 */
		private static List<GameObject> GenerateVertexDepthTestMeshes(
			IEnumerable<pb_Object> selection,
			out Dictionary<uint, pb_Tuple<pb_Object, int>> map)
		{
			List<GameObject> meshes = new List<GameObject>();
			map = new Dictionary<uint, pb_Tuple<pb_Object, int>>();
			Color32 BLACK = new Color32(0,0,0,255);

			// don't start at 0 because that means one vertex would be black, matching
			// the color used to cull hidden vertices.
			uint index = 0x02;

			foreach(pb_Object pb in selection)
			{
				// copy the select gameobject just for z-write
				GameObject go = pbUtil.EmptyGameObjectWithTransform(pb.transform);
				go.name = pb.name + "  (Depth Mask)";

				Mesh m = new Mesh();
				m.vertices = pb.vertices;
				m.triangles = pb.faces.SelectMany(x => x.indices).ToArray();
				m.colors32 = pbUtil.Fill<Color32>(BLACK, pb.vertexCount);
				#if UNITY_4_7
				// avoid incorrect unity warning about missing uv channel on 4.7
				m.uv = new Vector2[pb.vertexCount];
				m.uv2 = new Vector2[pb.vertexCount];
				#endif
				go.AddComponent<MeshFilter>().sharedMesh = m;
				go.AddComponent<MeshRenderer>().sharedMaterial = pb_Constant.FacePickerMaterial;
				meshes.Add(go);

				// build vertex billboards
				GameObject go2 = pbUtil.EmptyGameObjectWithTransform(pb.transform);
				go2.name = pb.name + "  (Vertex Billboards)";
				go2.AddComponent<MeshFilter>().sharedMesh = BuildVertexMesh(pb, map, ref index);
				go2.AddComponent<MeshRenderer>().sharedMaterial = pb_Constant.VertexPickerMaterial;
				meshes.Add(go2);
			}

			return meshes;
		}

		private static Mesh BuildVertexMesh(pb_Object pb, Dictionary<uint, pb_Tuple<pb_Object, int>> map, ref uint index)
		{
			int length = System.Math.Min(pb.sharedIndices.Length, ushort.MaxValue / 4 - 1);

			Vector3[] 	t_billboards 		= new Vector3[ length * 4 ];
			Vector2[] 	t_uvs 				= new Vector2[ length * 4 ];
			Vector2[] 	t_uv2 				= new Vector2[ length * 4 ];
			Color[]   	t_col 				= new Color[ length * 4 ];
			int[] 		t_tris 				= new int[ length * 6 ];

			int n = 0;
			int t = 0;

			Vector3 up = Vector3.up;
			Vector3 right = Vector3.right;

			for(int i = 0; i < length; i++)
			{
				Vector3 v = pb.vertices[pb.sharedIndices[i][0]];

				t_billboards[t+0] = v;
				t_billboards[t+1] = v;
				t_billboards[t+2] = v;
				t_billboards[t+3] = v;

				t_uvs[t+0] = Vector3.zero;
				t_uvs[t+1] = Vector3.right;
				t_uvs[t+2] = Vector3.up;
				t_uvs[t+3] = Vector3.one;

				t_uv2[t+0] = -up-right;
				t_uv2[t+1] = -up+right;
				t_uv2[t+2] =  up-right;
				t_uv2[t+3] =  up+right;

				t_tris[n+0] = t + 0;
				t_tris[n+1] = t + 1;
				t_tris[n+2] = t + 2;
				t_tris[n+3] = t + 1;
				t_tris[n+4] = t + 3;
				t_tris[n+5] = t + 2;

				Color32 color = EncodeRGBA(index);
				map.Add(index++, new pb_Tuple<pb_Object, int>(pb, i));

				t_col[t+0] = color;
				t_col[t+1] = color;
				t_col[t+2] = color;
				t_col[t+3] = color;

				t+=4;
				n+=6;
			}

			Mesh mesh = new Mesh();
			mesh.name = "Vertex Billboard";
			mesh.vertices = t_billboards;
			mesh.uv = t_uvs;
			mesh.uv2 = t_uv2;
			mesh.colors = t_col;
			mesh.triangles = t_tris;

			return mesh;
		}

		/**
		 *	Decode Color32.RGB values to a 32 bit unsigned int, using the RGB as the little
		 *	bytes.  Discards the hi byte (alpha)
		 */
		public static uint DecodeRGBA(Color32 color)
		{
			uint r = (uint)color.r;
			uint g = (uint)color.g;
			uint b = (uint)color.b;

			if(System.BitConverter.IsLittleEndian)
				return r << 16 | g << 8 | b;
			else
				return r << 24 | g << 16 | b << 8;
		}

		/**
		 *	Encode the low 24 bits of a UInt32 to RGB of Color32, using 255 for A.
		 */
		public static Color32 EncodeRGBA(uint hash)
		{
			// skip using BitConverter.GetBytes since this is super simple
			// bit math, and allocating arrays for each conversion is expensive
			if( System.BitConverter.IsLittleEndian)
				return new Color32(
					(byte) (hash >> 16 & 0xFF),
					(byte) (hash >>  8 & 0xFF),
					(byte) (hash       & 0xFF),
					(byte) (			  255) );
			else
				return new Color32(
					(byte) (hash >> 24 & 0xFF),
					(byte) (hash >> 16 & 0xFF),
					(byte) (hash >>  8 & 0xFF),
					(byte) (			  255) );
		}

		/**
		 *	Render the camera with a replacement shader and return the resulting image.
		 *	RenderTexture is always initialized with no gamma conversion (RenderTextureReadWrite.Linear)
		 */
		public static Texture2D RenderWithReplacementShader(Camera camera, Shader shader, string tag, int width = -1, int height = -1)
		{
			bool autoSize = width < 0 || height < 0;

			int _width = autoSize ? (int) camera.pixelRect.width : width;
			int _height = autoSize ? (int) camera.pixelRect.height : height;

			GameObject go = new GameObject();
			Camera renderCam = go.AddComponent<Camera>();
			renderCam.CopyFrom(camera);
#if UNITY_5_6_OR_NEWER
			renderCam.allowMSAA = false;
#endif
			// Deferred path doesn't play nice with RenderWithShader
			renderCam.renderingPath = RenderingPath.Forward;
			renderCam.enabled = false;
			renderCam.clearFlags = CameraClearFlags.SolidColor;
			renderCam.backgroundColor = Color.white;

#if UNITY_2017_1_OR_NEWER
			RenderTextureDescriptor descriptor = new RenderTextureDescriptor() {
				width = _width,
				height = _height,
				colorFormat = m_RenderTextureFormat,
				autoGenerateMips = false,
				depthBufferBits = 16,
				dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
				enableRandomWrite = false,
				memoryless = RenderTextureMemoryless.None,
				sRGB = false,
				useMipMap = false,
				volumeDepth = 1,
				msaaSamples = 8
			};
			RenderTexture rt = RenderTexture.GetTemporary(descriptor);
#else
			RenderTexture rt = RenderTexture.GetTemporary(
				_width,
				_height,
				16,
				m_RenderTextureFormat,
				RenderTextureReadWrite.Linear,
				1);
#endif
			rt.antiAliasing = 1;

			renderCam.targetTexture = rt;
			renderCam.RenderWithShader(shader, tag);

			RenderTexture prev = RenderTexture.active;
			RenderTexture.active = rt;

			Texture2D img = new Texture2D(_width, _height);
			img.ReadPixels(new Rect(0, 0, _width, _height), 0, 0);
			img.Apply();

			RenderTexture.active = prev;
			RenderTexture.ReleaseTemporary(rt);

			GameObject.DestroyImmediate(go);

			return img;
		}
	}
}

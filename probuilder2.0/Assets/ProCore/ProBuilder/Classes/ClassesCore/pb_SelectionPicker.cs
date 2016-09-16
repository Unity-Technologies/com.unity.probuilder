using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.Common
{
	public static class pb_SelectionPicker
	{

		/**
		 *	Render the pb_Object selection with the special selection picker shader and return a texture and color -> {object, face} dictionary.
		 */
		public static Texture2D RenderSelectionPickerTexture(Camera camera, IEnumerable<pb_Object> selection, out Dictionary<uint, pb_Tuple<pb_Object, pb_Face>> map)
		{
			List<GameObject> depthGameObjects = GenerateFaceDepthTestMeshes(selection, out map);

			Shader shader = pb_Constant.SelectionPickerShader;

			Texture2D tex = RenderWithReplacementShader(camera, shader, "ProBuilderPicker");

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
		public static Texture2D RenderSelectionPickerTexture(Camera camera, IEnumerable<pb_Object> selection, out Dictionary<uint, pb_Tuple<pb_Object, int>> map)
		{
			List<GameObject> depthGameObjects = GenerateVertexDepthTestMeshes(selection, out map);

			Shader shader = pb_Constant.VertexPickerShader;

			Texture2D tex = RenderWithReplacementShader(camera, shader, "ProBuilderPicker");

			// foreach(GameObject go in depthGameObjects)
			// {
			// 	GameObject.DestroyImmediate(go.GetComponent<MeshFilter>().sharedMesh);
			// 	GameObject.DestroyImmediate(go);
			// }

			return tex;
		}

		/**
		 *	Generate a set of meshes and gameObjects that can be rendered for depth testing faces.
		 */
		private static List<GameObject> GenerateFaceDepthTestMeshes(IEnumerable<pb_Object> selection, out Dictionary<uint, pb_Tuple<pb_Object, pb_Face>> map)
		{
			List<GameObject> meshes = new List<GameObject>();
			map = new Dictionary<uint, pb_Tuple<pb_Object, pb_Face>>();

			uint index = 0;

			foreach(pb_Object pb in selection)
			{
				GameObject go = new GameObject();
				go.transform.position = pb.transform.position;
				go.transform.localRotation = pb.transform.localRotation;
				go.transform.localScale = pb.transform.localScale;

				Mesh m = new Mesh();
				m.vertices = pb.vertices;
				m.triangles = pb.faces.SelectMany(x => x.indices).ToArray();
				Color32[] colors = new Color32[m.vertexCount];

				foreach(pb_Face f in pb.faces)
				{
					Color32 color = EncodeRGBA(index++);
					map.Add(DecodeRGBA(color), new pb_Tuple<pb_Object, pb_Face>(pb, f));

					for(int i = 0; i < f.distinctIndices.Length; i++)
						colors[f.distinctIndices[i]] = color;
				}
				m.colors32 = colors;

				go.AddComponent<MeshFilter>().sharedMesh = m;
				go.AddComponent<MeshRenderer>().sharedMaterial = pb_Constant.FacePickerMaterial;

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
			uint index = 0;

			foreach(pb_Object pb in selection)
			{
				// copy the select gameobject just for z-write
				GameObject go = pbUtil.EmptyGameObjectWithTransform(pb.transform);
				go.name = pb.name + "  (Depth Mask)";

				Mesh m = new Mesh();
				m.vertices = pb.vertices;
				m.triangles = pb.faces.SelectMany(x => x.indices).ToArray();
				m.colors32 = pbUtil.Fill<Color32>(new Color32(0,0,0,255), pb.vertexCount);
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
				int sharedIndex = pb.sharedIndices[i][0];
				Vector3 v = pb.vertices[sharedIndex];

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
				map.Add(index++, new pb_Tuple<pb_Object, int>(pb, sharedIndex));

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

		public static Texture2D RenderWithReplacementShader(Camera camera, Shader shader, string tag)
		{
			int width = (int) camera.pixelRect.width;
			int height = (int) camera.pixelRect.height;

			GameObject go = new GameObject();
			Camera renderCam = go.AddComponent<Camera>();
			renderCam.CopyFrom(camera);
			renderCam.enabled = false;
			renderCam.clearFlags = CameraClearFlags.SolidColor;
			renderCam.backgroundColor = Color.white;
			
			RenderTexture rt = RenderTexture.GetTemporary(
				width,
				height,
				16,
				RenderTextureFormat.Default,
				RenderTextureReadWrite.Default,
				1);

			renderCam.targetTexture = rt;
			renderCam.RenderWithShader(shader, tag);

			RenderTexture prev = RenderTexture.active;
			RenderTexture.active = rt;

			Texture2D img = new Texture2D(width, height);

			img.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			img.Apply();

			RenderTexture.active = prev;
			RenderTexture.ReleaseTemporary(rt);

			GameObject.DestroyImmediate(go);

			return img;
		}
	}
}

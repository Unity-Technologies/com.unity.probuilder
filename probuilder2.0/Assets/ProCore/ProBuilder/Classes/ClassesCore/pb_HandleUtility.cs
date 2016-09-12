using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.Common
{

	/**
	 * Static methods for working with pb_Objects in an editor.
	 */
	public class pb_HandleUtility
	{
		/**
		 * Find a triangle intersected by InRay on InMesh.  InRay is in world space.
		 * Returns the index in mesh.faces of the hit face, or -1.  Optionally can ignore
		 * backfaces.
		 */
		public static bool FaceRaycast(Ray InWorldRay, pb_Object mesh, out pb_RaycastHit hit)
		{
			return FaceRaycast(InWorldRay, mesh, out hit, Mathf.Infinity, Culling.Front);
		}

		/**
		 * Find the nearest triangle intersected by InWorldRay on this pb_Object.  InWorldRay is in world space.
		 * @hit contains information about the hit point.  @distance limits how far from @InWorldRay.origin the hit
		 * point may be.  @cullingMode determines what face orientations are tested (Culling.Front only tests front 
		 * faces, Culling.Back only tests back faces, and Culling.FrontBack tests both).
		 */
		public static bool FaceRaycast(Ray InWorldRay, pb_Object mesh, out pb_RaycastHit hit, float distance, Culling cullingMode)
		{
			/**
			 * Transform ray into model space
			 */

			InWorldRay.origin 		-= mesh.transform.position;  // Why doesn't worldToLocalMatrix apply translation?
			InWorldRay.origin 		= mesh.transform.worldToLocalMatrix * InWorldRay.origin;
			InWorldRay.direction 	= mesh.transform.worldToLocalMatrix * InWorldRay.direction;

			Vector3[] vertices = mesh.vertices;

			float dist = 0f;
			Vector3 point = Vector3.zero;

			float OutHitPoint = Mathf.Infinity;
			float dot; 		// vars used in loop
			Vector3 nrm;	// vars used in loop
			int OutHitFace = -1;
			Vector3 OutNrm = Vector3.zero;

			/**
			 * Iterate faces, testing for nearest hit to ray origin.  Optionally ignores backfaces.
			 */
			for(int CurFace = 0; CurFace < mesh.faces.Length; ++CurFace)
			{
				int[] Indices = mesh.faces[CurFace].indices;

				for(int CurTriangle = 0; CurTriangle < Indices.Length; CurTriangle += 3)
				{
					Vector3 a = vertices[Indices[CurTriangle+0]];
					Vector3 b = vertices[Indices[CurTriangle+1]];
					Vector3 c = vertices[Indices[CurTriangle+2]];

					nrm = Vector3.Cross(b-a, c-a);
					dot = Vector3.Dot(InWorldRay.direction, nrm);

					bool ignore = false;

					switch(cullingMode)
					{
						case Culling.Front:
							if(dot > 0f) ignore = true;
							break;

						case Culling.Back:
							if(dot < 0f) ignore = true;
							break;
					}

					if(!ignore && pb_Math.RayIntersectsTriangle(InWorldRay, a, b, c, out dist, out point))
					{
						if(dist > OutHitPoint || dist > distance)
							continue;

						OutNrm = nrm;
						OutHitFace = CurFace;
						OutHitPoint = dist;

						continue;
					}
				}
			}

			hit = new pb_RaycastHit(OutHitPoint,
									InWorldRay.GetPoint(OutHitPoint),
									OutNrm,
									OutHitFace);

			return OutHitFace > -1;
		}

		/**
		 * Find the all triangles intersected by InWorldRay on this pb_Object.  InWorldRay is in world space.
		 * @hit contains information about the hit point.  @distance limits how far from @InWorldRay.origin the hit
		 * point may be.  @cullingMode determines what face orientations are tested (Culling.Front only tests front 
		 * faces, Culling.Back only tests back faces, and Culling.FrontBack tests both).
		 */
		public static bool FaceRaycast(Ray InWorldRay, pb_Object mesh, out List<pb_RaycastHit> hits, float distance, Culling cullingMode)
		{
			/**
			 * Transform ray into model space
			 */
			InWorldRay.origin -= mesh.transform.position;  // Why doesn't worldToLocalMatrix apply translation?
			
			InWorldRay.origin 		= mesh.transform.worldToLocalMatrix * InWorldRay.origin;
			InWorldRay.direction 	= mesh.transform.worldToLocalMatrix * InWorldRay.direction;

			Vector3[] vertices = mesh.vertices;

			float dist = 0f;
			Vector3 point = Vector3.zero;

			float dot; // vars used in loop
			Vector3 nrm;	// vars used in loop
			hits = new List<pb_RaycastHit>();

			/**
			 * Iterate faces, testing for nearest hit to ray origin.  Optionally ignores backfaces.
			 */
			for(int CurFace = 0; CurFace < mesh.faces.Length; ++CurFace)
			{
				int[] Indices = mesh.faces[CurFace].indices;

				for(int CurTriangle = 0; CurTriangle < Indices.Length; CurTriangle += 3)
				{
					Vector3 a = vertices[Indices[CurTriangle+0]];
					Vector3 b = vertices[Indices[CurTriangle+1]];
					Vector3 c = vertices[Indices[CurTriangle+2]];

					if(pb_Math.RayIntersectsTriangle(InWorldRay, a, b, c, out dist, out point))
					{
						nrm = Vector3.Cross(b-a, c-a);

						switch(cullingMode)
						{
							case Culling.Front:
								dot = Vector3.Dot(InWorldRay.direction, -nrm);

								if(dot > 0f)
									goto case Culling.FrontBack;
								break;

							case Culling.Back:
								dot = Vector3.Dot(InWorldRay.direction, nrm);

								if(dot > 0f)
									goto case Culling.FrontBack;
								break;

							case Culling.FrontBack:
								hits.Add( new pb_RaycastHit(dist,
															InWorldRay.GetPoint(dist),
															nrm,
															CurFace));
								break;
						}

						continue;
					}
				}
			}

			return hits.Count > 0;
		}

		const float MAX_EDGE_SELECT_DISTANCE = 20f;

		/**
		 * Checks if mouse is over an edge, and if so, returns true setting @edge.
		 */
		public static bool EdgeRaycast(Camera cam, Vector2 mousePosition, pb_Object mesh, pb_Edge[] edges, Vector3[] verticesInWorldSpace, out pb_Edge edge)
		{
			Vector3 v0, v1;
			float bestDistance = Mathf.Infinity;
			float distance = 0f;
			edge = null;

			GameObject go = ObjectRaycast(cam, mousePosition, (GameObject[]) Resources.FindObjectsOfTypeAll(typeof(GameObject)));

			if( go == null || go != mesh.gameObject)
			{
				int width = Screen.width;
				int height = Screen.height;

				for(int i = 0; i < edges.Length; i++)
				{
					v0 = verticesInWorldSpace[edges[i].x];
					v1 = verticesInWorldSpace[edges[i].y];
					
					distance = pb_HandleUtility.DistancePoint2DToLine(cam, mousePosition, v0, v1);

					if ( distance < bestDistance && distance < MAX_EDGE_SELECT_DISTANCE )// && !PointIsOccluded(mesh, (v0+v1)*.5f) )
					{
						Vector3 vs0 = cam.WorldToScreenPoint(v0);
						
						// really simple frustum check (will fail on edges that have vertices outside the frustum but is visible)
						if( vs0.z <= 0 || vs0.x < 0 || vs0.y < 0 || vs0.x > width || vs0.y > height )
							continue;

						Vector3 vs1 = cam.WorldToScreenPoint(v1);

						if( vs1.z <= 0 || vs1.x < 0 || vs1.y < 0 || vs1.x > width || vs1.y > height )
							continue;


						bestDistance = distance;
						edge = edges[i];
					}
				}
			}
			else
			{
				// Test culling
				List<pb_RaycastHit> hits;
				Ray ray = cam.ScreenPointToRay(mousePosition);// HandleUtility.GUIPointToWorldRay(mousePosition);

				if( FaceRaycast(ray, mesh, out hits, Mathf.Infinity, Culling.FrontBack) )
				{
					// Sort from nearest hit to farthest
					hits.Sort( (x, y) => x.distance.CompareTo(y.distance) );
					
					// Find the nearest edge in the hit faces
					Vector3[] v = mesh.vertices;

					for(int i = 0; i < hits.Count; i++)
					{
						if( pb_HandleUtility.PointIsOccluded(cam, mesh, mesh.transform.TransformPoint(hits[i].point)) )
							continue;

						foreach(pb_Edge e in mesh.faces[hits[i].face].GetAllEdges())
						{
							float d = pb_Math.DistancePointLineSegment(hits[i].point, v[e.x], v[e.y]);

							if(d < bestDistance)
							{
								bestDistance = d;
								edge = e;
							}
						}

						if( Vector3.Dot(ray.direction, mesh.transform.TransformDirection(hits[i].normal)) < 0f )
							break;
					}

					if(edge != null && pb_HandleUtility.DistancePoint2DToLine(cam, mousePosition, mesh.transform.TransformPoint(v[edge.x]), mesh.transform.TransformPoint(v[edge.y])) > MAX_EDGE_SELECT_DISTANCE)
					{
						edge = null;
					}
				}
			}

			return edge != null;
		}

		/**
		 * Returns the nearest gameobject to the @mousePosition.
		 */
		public static GameObject ObjectRaycast(Camera cam, Vector2 mousePosition, GameObject[] objects)
		{

			return null;
		}

		public static float DistancePoint2DToLine(Camera cam, Vector2 mousePosition, Vector3 worldPosition1, Vector3 worldPosition2)
		{
			Vector2 v0 = cam.WorldToScreenPoint(worldPosition1);
			Vector2 v1 = cam.WorldToScreenPoint(worldPosition2);

			return pb_Math.DistancePointLineSegment(mousePosition, v0, v1);
		}

		/**
		 * Returns true if this point in world space is occluded by a triangle on this object.
		 */
		public static bool PointIsOccluded(Camera cam, pb_Object pb, Vector3 worldPoint)
		{
			Vector3 dir = (cam.transform.position - worldPoint).normalized;

			// move the point slightly towards the camera to avoid colliding with its own triangle
			Ray ray = new Ray(worldPoint + dir * .0001f, dir);
			
			pb_RaycastHit hit;

			return pb_HandleUtility.FaceRaycast(ray, pb, out hit, Vector3.Distance(cam.transform.position, worldPoint), Culling.Back);
		}

		/**
		 * Returns true if this point in world space is occluded by a triangle on this object.
		 */
		public static bool IsOccluded(Camera cam, pb_Object pb, pb_Face face)
		{
			Vector3 point = Vector3.zero;
			int len = face.distinctIndices.Length;

			for(int i = 0;i < len; i++)
				point += pb.vertices[face.distinctIndices[i]];

			point *= (1f/len);

			return PointIsOccluded(cam, pb, pb.transform.TransformPoint(point));
		}

		/**
		 *	Render the pb_Object selection with the special selection picker shader and return a texture and color -> {object, face} dictionary.
		 */
		public static Texture2D RenderSelectionPickerTexture(Camera camera, pb_Object[] selection, out Dictionary<Color32, pb_Tuple<pb_Object, pb_Face>> map)
		{
			List<GameObject> depthGameObjects = GenerateDepthTestMeshes(selection, out map);

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
		 *	Generate a set of meshes and gameObjects that can be rendered for depth testing faces.
		 */
		private static List<GameObject> GenerateDepthTestMeshes(IEnumerable<pb_Object> selection, out Dictionary<Color32, pb_Tuple<pb_Object, pb_Face>> map)
		{
			List<GameObject> meshes = new List<GameObject>();
			map = new Dictionary<Color32, pb_Tuple<pb_Object, pb_Face>>();

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
					map.Add(color, new pb_Tuple<pb_Object, pb_Face>(pb, f));

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

		public static uint DecodeRGBA(Color32 color)
		{
			if(System.BitConverter.IsLittleEndian)
				return System.BitConverter.ToUInt32( new byte[] {
					color.b,
					color.g,
					color.r,
					(byte) 0}, 
					0);
				else
					return System.BitConverter.ToUInt32( new byte[] {
						(byte) 0,
						color.b,
						color.g,
						color.r },
						0);
		}

		public static Color32 EncodeRGBA(uint hash)
		{
			byte[] bytes = System.BitConverter.GetBytes(hash);

			// Debug.Log(string.Format("encode {0:X8} to {1:X}, {2:X}, {3:X}, {4:X}  {5}", 
			// 	hash,
			// 	bytes[0],
			// 	bytes[1],
			// 	bytes[2],
			// 	bytes[3],
			// 	System.BitConverter.IsLittleEndian ? "little endian" : "big endian"));

			// msdn - "The order of bytes in the array returned by the GetBytes method depends on whether the computer architecture is little-endian or big-endian."
			// since we're restricted to 24bit depth, lop off the hi byte
			if( System.BitConverter.IsLittleEndian )
				return new Color32( bytes[2],			
									bytes[1],			
									bytes[0],			
									(byte) 255);
			else
				return new Color32( bytes[1],			
									bytes[2],			
									bytes[3],			
									(byte) 255);
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

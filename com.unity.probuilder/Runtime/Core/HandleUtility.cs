using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Static methods for working with pb_Objects in an editor.
	/// </summary>
	static class HandleUtility
	{
		const float k_MaxEdgeSelectDistance = 20f;

		/// <summary>
		/// Convert a screen point (0,0 bottom left, in pixels) to a GUI point (0,0 top left, in points).
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="point"></param>
		/// <param name="pixelsPerPoint"></param>
		/// <returns></returns>
		public static Vector3 ScreenToGuiPoint(this Camera camera, Vector3 point, float pixelsPerPoint)
		{
			return new Vector3(point.x / pixelsPerPoint, (camera.pixelHeight - point.y) / pixelsPerPoint, point.z);
		}

		/// <summary>
		/// Find a triangle intersected by InRay on InMesh.  InRay is in world space.
		/// Returns the index in mesh.faces of the hit face, or -1.  Optionally can ignore
		/// backfaces.
		/// </summary>
		/// <param name="worldRay"></param>
		/// <param name="mesh"></param>
		/// <param name="hit"></param>
		/// <param name="ignore"></param>
		/// <returns></returns>
		public static bool FaceRaycast(Ray worldRay, ProBuilderMesh mesh, out RaycastHit hit, HashSet<Face> ignore = null)
		{
			return FaceRaycast(worldRay, mesh, out hit, Mathf.Infinity, CullingMode.Back, ignore);
		}

		/// <summary>
		/// Find the nearest face intersected by InWorldRay on this pb_Object.
		/// </summary>
		/// <param name="worldRay">A ray in world space.</param>
		/// <param name="mesh">The ProBuilder object to raycast against.</param>
		/// <param name="hit">If the mesh was intersected, hit contains information about the intersect point.</param>
		/// <param name="distance">The distance from the ray origin to the intersection point.</param>
		/// <param name="cullingMode">Which sides of a face are culled when hit testing. Default is back faces are culled.</param>
		/// <param name="ignore">Optional collection of faces to ignore when raycasting.</param>
		/// <returns>True if the ray intersects with the mesh, false if not.</returns>
		public static bool FaceRaycast(Ray worldRay, ProBuilderMesh mesh, out RaycastHit hit, float distance, CullingMode cullingMode, HashSet<Face> ignore = null)
		{
			// Transform ray into model space
			worldRay.origin -= mesh.transform.position; // Why doesn't worldToLocalMatrix apply translation?
			worldRay.origin = mesh.transform.worldToLocalMatrix * worldRay.origin;
			worldRay.direction = mesh.transform.worldToLocalMatrix * worldRay.direction;

			var positions = mesh.positionsInternal;
			var faces = mesh.facesInternal;

			float OutHitPoint = Mathf.Infinity;
			int OutHitFace = -1;
			Vector3 OutNrm = Vector3.zero;

			// Iterate faces, testing for nearest hit to ray origin. Optionally ignores backfaces.
			for(int i = 0, fc = faces.Length; i < fc; ++i)
			{
				if(ignore != null && ignore.Contains(faces[i]))
					continue;

				int[] indexes = mesh.facesInternal[i].indexesInternal;

				for(int j = 0, ic = indexes.Length; j < ic; j += 3)
				{
					Vector3 a = positions[indexes[j+0]];
					Vector3 b = positions[indexes[j+1]];
					Vector3 c = positions[indexes[j+2]];

					Vector3 nrm = Vector3.Cross(b-a, c-a);
					float dot = Vector3.Dot(worldRay.direction, nrm);

					bool skip = false;

					switch(cullingMode)
					{
						case CullingMode.Front:
							if(dot < 0f) skip = true;
							break;

						case CullingMode.Back:
							if(dot > 0f) skip = true;
							break;
					}

					var dist = 0f;

					Vector3 point;
					if(!skip && Math.RayIntersectsTriangle(worldRay, a, b, c, out dist, out point))
					{
						if(dist > OutHitPoint || dist > distance)
							continue;

						OutNrm = nrm;
						OutHitFace = i;
						OutHitPoint = dist;
					}
				}
			}

			hit = new RaycastHit(OutHitPoint,
									worldRay.GetPoint(OutHitPoint),
									OutNrm,
									OutHitFace);

			return OutHitFace > -1;
		}

		public static bool FaceRaycastBothCullModes(Ray worldRay, ProBuilderMesh mesh, ref SimpleTuple<Face, Vector3> back, ref SimpleTuple<Face, Vector3> front)
		{
			// Transform ray into model space
			worldRay.origin -= mesh.transform.position; // Why doesn't worldToLocalMatrix apply translation?
			worldRay.origin = mesh.transform.worldToLocalMatrix * worldRay.origin;
			worldRay.direction = mesh.transform.worldToLocalMatrix * worldRay.direction;

			var positions = mesh.positionsInternal;
			var faces = mesh.facesInternal;

			back.item1 = null;
			front.item1 = null;

			float backDistance = Mathf.Infinity;
			float frontDistance = Mathf.Infinity;

			// Iterate faces, testing for nearest hit to ray origin. Optionally ignores backfaces.
			for(int i = 0, fc = faces.Length; i < fc; ++i)
			{
				int[] indexes = mesh.facesInternal[i].indexesInternal;

				for(int j = 0, ic = indexes.Length; j < ic; j += 3)
				{
					Vector3 a = positions[indexes[j+0]];
					Vector3 b = positions[indexes[j+1]];
					Vector3 c = positions[indexes[j+2]];

					float dist;
					Vector3 point;

					if(Math.RayIntersectsTriangle(worldRay, a, b, c, out dist, out point))
					{
						if (dist < backDistance || dist < frontDistance)
						{
							Vector3 nrm = Vector3.Cross(b - a, c - a);
							float dot = Vector3.Dot(worldRay.direction, nrm);

							if (dot < 0f)
							{
								if (dist < backDistance)
								{
									backDistance = dist;
									back.item1 = faces[i];
								}
							}
							else
							{
								if (dist < frontDistance)
								{
									frontDistance = dist;
									front.item1 = faces[i];
								}
							}
						}
					}
				}
			}

			if (back.item1 != null)
				back.item2 = worldRay.GetPoint(backDistance);

			if (front.item1 != null)
				front.item2 = worldRay.GetPoint(frontDistance);

			return back.item1 != null || front.item1 != null;
		}

		/// <summary>
		/// Find the all faces intersected by InWorldRay on this pb_Object.
		/// </summary>
		/// <param name="InWorldRay">A ray in world space.</param>
		/// <param name="mesh">The ProBuilder object to raycast against.</param>
		/// <param name="hits">If the mesh was intersected, hits contains all intersection point RaycastHit information.</param>
		/// <param name="cullingMode">What sides of triangles does the ray intersect with.</param>
		/// <param name="ignore">Optional collection of faces to ignore when raycasting.</param>
		/// <returns>True if the ray intersects with the mesh, false if not.</returns>
		public static bool FaceRaycast(
			Ray InWorldRay,
			ProBuilderMesh mesh,
			out List<RaycastHit> hits,
			CullingMode cullingMode,
			HashSet<Face> ignore = null)
		{
			// Transform ray into model space
			InWorldRay.origin -= mesh.transform.position;  // Why doesn't worldToLocalMatrix apply translation?

			InWorldRay.origin 		= mesh.transform.worldToLocalMatrix * InWorldRay.origin;
			InWorldRay.direction 	= mesh.transform.worldToLocalMatrix * InWorldRay.direction;

			Vector3[] vertexes = mesh.positionsInternal;

			hits = new List<RaycastHit>();

            // Iterate faces, testing for nearest hit to ray origin.  Optionally ignores backfaces.
            for (int CurFace = 0; CurFace < mesh.facesInternal.Length; ++CurFace)
			{
				if(ignore != null && ignore.Contains(mesh.facesInternal[CurFace]))
					continue;

				int[] indexes = mesh.facesInternal[CurFace].indexesInternal;

				for(int CurTriangle = 0; CurTriangle < indexes.Length; CurTriangle += 3)
				{
					Vector3 a = vertexes[indexes[CurTriangle+0]];
					Vector3 b = vertexes[indexes[CurTriangle+1]];
					Vector3 c = vertexes[indexes[CurTriangle+2]];

					var dist = 0f;
					Vector3 point;

					if(Math.RayIntersectsTriangle(InWorldRay, a, b, c, out dist, out point))
					{
						Vector3 nrm = Vector3.Cross(b-a, c-a);

						float dot; // vars used in loop
						switch(cullingMode)
						{
							case CullingMode.Front:
								dot = Vector3.Dot(InWorldRay.direction, nrm);

								if(dot > 0f)
									goto case CullingMode.FrontBack;
								break;

							case CullingMode.Back:
								dot = Vector3.Dot(InWorldRay.direction, nrm);

								if(dot < 0f)
									goto case CullingMode.FrontBack;
								break;

							case CullingMode.FrontBack:
								hits.Add( new RaycastHit(dist,
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

		/// <summary>
		/// Transform a ray from world space to a transform local space.
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="InWorldRay"></param>
		/// <returns></returns>
		public static Ray InverseTransformRay(this Transform transform, Ray InWorldRay)
		{
			Vector3 o = InWorldRay.origin;
			o -= transform.position;
			o = transform.worldToLocalMatrix * o;
			Vector3 d = transform.worldToLocalMatrix.MultiplyVector(InWorldRay.direction);
			return new Ray(o, d);
		}

		/// <summary>
		/// Find the nearest triangle intersected by InWorldRay on this mesh.
		/// </summary>
		/// <param name="InWorldRay"></param>
		/// <param name="transform"></param>
		/// <param name="vertexes"></param>
		/// <param name="triangles"></param>
		/// <param name="hit"></param>
		/// <param name="distance"></param>
		/// <param name="cullingMode"></param>
		/// <returns></returns>
		public static bool WorldRaycast(Ray InWorldRay, Transform transform, Vector3[] vertexes, int[] triangles, out RaycastHit hit, float distance = Mathf.Infinity)
		{
			Ray ray = transform.InverseTransformRay(InWorldRay);
			return MeshRaycast(ray, vertexes, triangles, out hit, distance);
		}

		/// <summary>
		/// Cast a ray (in model space) against a mesh.
		/// </summary>
		/// <param name="InRay"></param>
		/// <param name="mesh"></param>
		/// <param name="triangles"></param>
		/// <param name="hit"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		public static bool MeshRaycast(Ray InRay, Vector3[] mesh, int[] triangles, out RaycastHit hit, float distance = Mathf.Infinity)
		{
			// float dot; 		// vars used in loop
			float hitDistance = Mathf.Infinity;
			Vector3 hitNormal = new Vector3(0f, 0f, 0f);	// vars used in loop
			Vector3 a, b, c;
			int hitFace = -1;
			Vector3 o = InRay.origin, d = InRay.direction;

            // Iterate faces, testing for nearest hit to ray origin.
            for (int CurTri = 0; CurTri < triangles.Length; CurTri += 3)
			{
				a = mesh[triangles[CurTri+0]];
				b = mesh[triangles[CurTri+1]];
				c = mesh[triangles[CurTri+2]];

				if(Math.RayIntersectsTriangle2(o, d, a, b, c, ref distance, ref hitNormal))
				{
					hitFace = CurTri / 3;
					hitDistance = distance;
					break;
				}
			}

			hit = new RaycastHit( hitDistance,
									InRay.GetPoint(hitDistance),
									hitNormal,
									hitFace);

			return hitFace > -1;
		}

		/// <summary>
		/// Returns true if this point in world space is occluded by a triangle on this object.
		/// </summary>
		/// <remarks>This is very slow, do not use.</remarks>
		/// <param name="cam"></param>
		/// <param name="pb"></param>
		/// <param name="worldPoint"></param>
		/// <returns></returns>
		internal static bool PointIsOccluded(Camera cam, ProBuilderMesh pb, Vector3 worldPoint)
		{
			Vector3 dir = (cam.transform.position - worldPoint).normalized;

			// move the point slightly towards the camera to avoid colliding with its own triangle
			Ray ray = new Ray(worldPoint + dir * .0001f, dir);

			RaycastHit hit;

			return FaceRaycast(ray, pb, out hit, Vector3.Distance(cam.transform.position, worldPoint), CullingMode.Front);
		}
	}
}

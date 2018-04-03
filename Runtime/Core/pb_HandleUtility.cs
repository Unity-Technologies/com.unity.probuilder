using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder.Core
{
	/// <summary>
	/// Static methods for working with pb_Objects in an editor.
	/// </summary>
	static class pb_HandleUtility
	{
		const float MAX_EDGE_SELECT_DISTANCE = 20f;

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
		/// <param name="InWorldRay"></param>
		/// <param name="mesh"></param>
		/// <param name="hit"></param>
		/// <param name="ignore"></param>
		/// <returns></returns>
		public static bool FaceRaycast(Ray InWorldRay, pb_Object mesh, out pb_RaycastHit hit, HashSet<pb_Face> ignore = null)
		{
			return FaceRaycast(InWorldRay, mesh, out hit, Mathf.Infinity, pb_Culling.Front, ignore);
		}

		/// <summary>
		/// Find the nearest face intersected by InWorldRay on this pb_Object.
		/// </summary>
		/// <param name="InWorldRay">A ray in world space.</param>
		/// <param name="mesh">The ProBuilder object to raycast against.</param>
		/// <param name="hit">If the mesh was intersected, hit contains information about the intersect point.</param>
		/// <param name="distance">The distance from the ray origin to the intersection point.</param>
		/// <param name="cullingMode">What sides of triangles does the ray intersect with.</param>
		/// <param name="ignore">Optional collection of faces to ignore when raycasting.</param>
		/// <returns>True if the ray intersects with the mesh, false if not.</returns>
		public static bool FaceRaycast(Ray InWorldRay, pb_Object mesh, out pb_RaycastHit hit, float distance, pb_Culling cullingMode, HashSet<pb_Face> ignore = null)
		{
			// Transform ray into model space
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

			// Iterate faces, testing for nearest hit to ray origin. Optionally ignores backfaces.
			for(int CurFace = 0; CurFace < mesh.faces.Length; ++CurFace)
			{
				if(ignore != null && ignore.Contains(mesh.faces[CurFace]))
					continue;

				int[] Indices = mesh.faces[CurFace].indices;

				for(int CurTriangle = 0; CurTriangle < Indices.Length; CurTriangle += 3)
				{
					Vector3 a = vertices[Indices[CurTriangle+0]];
					Vector3 b = vertices[Indices[CurTriangle+1]];
					Vector3 c = vertices[Indices[CurTriangle+2]];

					nrm = Vector3.Cross(b-a, c-a);
					dot = Vector3.Dot(InWorldRay.direction, nrm);

					bool skip = false;

					switch(cullingMode)
					{
						case pb_Culling.Front:
							if(dot > 0f) skip = true;
							break;

						case pb_Culling.Back:
							if(dot < 0f) skip = true;
							break;
					}

					if(!skip && pb_Math.RayIntersectsTriangle(InWorldRay, a, b, c, out dist, out point))
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

		/// <summary>
		/// Find the all faces intersected by InWorldRay on this pb_Object.
		/// </summary>
		/// <param name="InWorldRay">A ray in world space.</param>
		/// <param name="mesh">The ProBuilder object to raycast against.</param>
		/// <param name="hits">If the mesh was intersected, hits contains all intersection point RaycastHit information.</param>
		/// <param name="distance">The distance from the ray origin to the intersection point.</param>
		/// <param name="cullingMode">What sides of triangles does the ray intersect with.</param>
		/// <param name="ignore">Optional collection of faces to ignore when raycasting.</param>
		/// <returns>True if the ray intersects with the mesh, false if not.</returns>
		public static bool FaceRaycast(
			Ray InWorldRay,
			pb_Object mesh,
			out List<pb_RaycastHit> hits,
			float distance,
			pb_Culling cullingMode,
			HashSet<pb_Face> ignore = null)
		{
			// Transform ray into model space
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
				if(ignore != null && ignore.Contains(mesh.faces[CurFace]))
					continue;

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
							case pb_Culling.Front:
								dot = Vector3.Dot(InWorldRay.direction, -nrm);

								if(dot > 0f)
									goto case pb_Culling.FrontBack;
								break;

							case pb_Culling.Back:
								dot = Vector3.Dot(InWorldRay.direction, nrm);

								if(dot > 0f)
									goto case pb_Culling.FrontBack;
								break;

							case pb_Culling.FrontBack:
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
		/// <param name="vertices"></param>
		/// <param name="triangles"></param>
		/// <param name="hit"></param>
		/// <param name="distance"></param>
		/// <param name="cullingMode"></param>
		/// <returns></returns>
		public static bool WorldRaycast(Ray InWorldRay, Transform transform, Vector3[] vertices, int[] triangles, out pb_RaycastHit hit, float distance = Mathf.Infinity, pb_Culling cullingMode = pb_Culling.Front)
		{
			Ray ray = transform.InverseTransformRay(InWorldRay);
			return MeshRaycast(ray, vertices, triangles, out hit, distance, cullingMode);
		}

		/// <summary>
		/// Cast a ray (in model space) against a mesh.
		/// </summary>
		/// <param name="InRay"></param>
		/// <param name="vertices"></param>
		/// <param name="triangles"></param>
		/// <param name="hit"></param>
		/// <param name="distance"></param>
		/// <param name="cullingMode"></param>
		/// <returns></returns>
		public static bool MeshRaycast(Ray InRay, Vector3[] vertices, int[] triangles, out pb_RaycastHit hit, float distance = Mathf.Infinity, pb_Culling cullingMode = pb_Culling.Front)
		{
			// float dot; 		// vars used in loop
			float hitDistance = Mathf.Infinity;
			Vector3 hitNormal = new Vector3(0f, 0f, 0f);	// vars used in loop
			Vector3 a, b, c;
			int hitFace = -1;
			Vector3 o = InRay.origin, d = InRay.direction;

			/**
			 * Iterate faces, testing for nearest hit to ray origin.
			 */
			for(int CurTri = 0; CurTri < triangles.Length; CurTri += 3)
			{
				a = vertices[triangles[CurTri+0]];
				b = vertices[triangles[CurTri+1]];
				c = vertices[triangles[CurTri+2]];

				if(pb_Math.RayIntersectsTriangle2(o, d, a, b, c, ref distance, ref hitNormal))
				{
					hitFace = CurTri / 3;
					hitDistance = distance;
					break;
				}
			}

			hit = new pb_RaycastHit( hitDistance,
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
		internal static bool PointIsOccluded(Camera cam, pb_Object pb, Vector3 worldPoint)
		{
			Vector3 dir = (cam.transform.position - worldPoint).normalized;

			// move the point slightly towards the camera to avoid colliding with its own triangle
			Ray ray = new Ray(worldPoint + dir * .0001f, dir);

			pb_RaycastHit hit;

			return pb_HandleUtility.FaceRaycast(ray, pb, out hit, Vector3.Distance(cam.transform.position, worldPoint), pb_Culling.Back);
		}

		/// <summary>
		/// Returns true if this point in world space is occluded by a triangle on this object.
		/// </summary>
		/// <remarks>This is very slow, do not use.</remarks>
		/// <param name="cam"></param>
		/// <param name="pb"></param>
		/// <param name="face"></param>
		/// <returns></returns>
		internal static bool IsOccluded(Camera cam, pb_Object pb, pb_Face face)
		{
			Vector3 point = Vector3.zero;
			int len = face.distinctIndices.Length;

			for(int i = 0;i < len; i++)
				point += pb.vertices[face.distinctIndices[i]];

			point *= (1f/len);

			return PointIsOccluded(cam, pb, pb.transform.TransformPoint(point));
		}

	}

}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Math;
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

						foreach(pb_Edge e in mesh.faces[hits[i].face].GetEdges())
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
					// else
					// {
					// 	edge.x = mesh.ToUserIndex(edge.x);
					// 	edge.y = mesh.ToUserIndex(edge.y);
					// }
				}
			}

			return edge != null;
		}

		// public static bool VertexRaycast(Camera cam, Vector2 mousePosition, int rectSize, pb_Object mesh, Vector3[] verticesInWorldSpace, int[] indices, out int index)
		// {
		// 	float bestDistance = Mathf.Infinity;
		// 	float distance = 0f;
		// 	index = -1;

		// 	GameObject go = pb_HandleUtility.ObjectRaycast(cam, mousePosition, (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject)) );// HandleUtility.PickGameObject(mousePosition, false);

		// 	if( go == null || go != mesh.gameObject )
		// 	{
		// 		int width = Screen.width;
		// 		int height = Screen.height;

		// 		Rect mouseRect = new Rect(mousePosition.x - (rectSize/2f), mousePosition.y - (rectSize/2f), rectSize, rectSize);
		// 		List<int> user = (List<int>) selection.mesh.GetUserIndices();

		// 		for(int i = 0; i < user.Count; i++)
		// 		{
		// 			if( mouseRect.Contains( HandleUtility.WorldToGUIPoint(selection.verticesInWorldSpace[user[i]]) ) )
		// 			{				
		// 				Vector3 v = cam.WorldToScreenPoint(selection.verticesInWorldSpace[user[i]]);

		// 				distance = Vector2.Distance(mousePosition, v);

		// 				if( distance < bestDistance )
		// 				{
		// 					if( v.z <= 0 || v.x < 0 || v.y < 0 || v.x > width || v.y > height )
		// 						continue;

		// 					if( PointIsOccluded(mesh, selection.verticesInWorldSpace[user[i]]) )
		// 						continue;

		// 					index = user[i];
		// 					bestDistance = Vector2.Distance(v, mousePosition);
		// 				}
		// 			}
		// 		}
		// 	}
		// 	else
		// 	{
		// 		// Test culling
		// 		List<pb_RaycastHit> hits;
		// 		Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

		// 		if( MeshRaycast(ray, mesh, out hits, Mathf.Infinity, Culling.FrontBack) )
		// 		{
		// 			// Sort from nearest hit to farthest
		// 			hits.Sort( (x, y) => x.Distance.CompareTo(y.Distance) );
					
		// 			// Find the nearest edge in the hit faces
		// 			Vector3[] v = mesh.vertices;

		// 			for(int i = 0; i < hits.Count; i++)
		// 			{
		// 				if(  PointIsOccluded(mesh, mesh.transform.TransformPoint(hits[i].Point)) )
		// 					continue;

		// 				foreach(int tri in mesh.faces[hits[i].FaceIndex].indices)
		// 				{
		// 					float d = Vector3.Distance(hits[i].Point, v[tri]);

		// 					if(d < bestDistance)
		// 					{
		// 						bestDistance = d;
		// 						index = tri;
		// 					}
		// 				}

		// 				if( Vector3.Dot(ray.direction, mesh.transform.TransformDirection(hits[i].Normal)) < 0f )
		// 					break;
		// 			}

		// 			if(index > -1 && Vector2.Distance(mousePosition, HandleUtility.WorldToGUIPoint(selection.verticesInWorldSpace[index])) > rectSize * 1.3f)
		// 			{
		// 				index = -1;
		// 			}
		// 		}
		// 	}

		// 	if( index > -1 )
		// 		index = mesh.ToUserIndex(index);

		// 	return index > -1;
		// }

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
	}

}

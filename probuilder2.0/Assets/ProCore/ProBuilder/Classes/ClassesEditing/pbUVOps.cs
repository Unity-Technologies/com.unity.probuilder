using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.Math;
using System.Linq;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.MeshOperations {

public static class pbUVOps
{

#region Sew / Split

	/**
	 * Sews a UV seam using delta to determine which UVs are close enough to be merged.
	 * \sa pbVertexOps::WeldVertices
	 */
	public static bool SewUVs(this pb_Object pb, int[] indices, float delta)
	{
		int[] si = new int[indices.Length];
		Vector2[] uvs = pb.msh.uv;

		// set the shared indices cache to a unique non-used index
		for(int i = 0; i < indices.Length; i++)
			si[i] = -(i+1);
		
		pb_IntArray[] sharedIndices = pb.sharedIndicesUV;

		for(int i = 0; i < indices.Length-1; i++)
		{
			for(int n = i+1; n < indices.Length; n++)
			{
				if(si[i] == si[n])
					continue;	// they already share a vertex
				
				if(Vector2.Distance(uvs[indices[i]], uvs[indices[n]]) < delta)
				{
					Vector3 cen = (uvs[indices[i]] + uvs[indices[n]]) / 2f;
					uvs[indices[i]] = cen;
					uvs[indices[n]] = cen;
					int newIndex = pb_IntArrayUtility.MergeSharedIndices(ref sharedIndices, new int[2] {indices[i], indices[n]});
					si[i] = newIndex;
					si[n] = newIndex;
				}
			}
		}

		pb.SetUV(uvs);
		pb.SetSharedIndicesUV(sharedIndices);

		return true;
	}

	/**
	 * Similar to Sew, except Collapse just flattens all UVs to the center point no matter the distance.
	 */
	public static void CollapseUVs(this pb_Object pb, int[] indices)
	{
		Vector2[] uvs = pb.uv;

		// set the shared indices cache to a unique non-used index
		Vector2 cen = pb_Math.Average(pbUtil.ValuesWithIndices(uvs, indices) );

		foreach(int i in indices)
			uvs[i] = cen;
			
		pb_IntArray[] sharedIndices = pb.sharedIndicesUV;
		pb_IntArrayUtility.MergeSharedIndices(ref sharedIndices, indices);
		
		pb.SetUV(uvs);
		pb.SetSharedIndicesUV(sharedIndices);
	}

	/**
	 * Creates separate entries in sharedIndices cache for all passed indices.
	 * If indices are not present in pb_IntArray[], don't do anything with them.
	 */
	public static bool SplitUVs(this pb_Object pb, int[] indices)
	{
		pb_IntArray[] sharedIndices = pb.sharedIndicesUV;

		if( sharedIndices == null )
			return false;

		List<int> distInd = indices.Distinct().ToList();

		/**
		 * remove indices from sharedIndices
		 */
		for(int i = 0; i < distInd.Count; i++)
		{
			int index = sharedIndices.IndexOf(distInd[i]);

			if(index < 0) continue;

			// can't use ArrayUtility.RemoveAt on account of it being Editor only
			sharedIndices[index].array = sharedIndices[index].array.Remove(distInd[i]);
		}

		/**
		 * and add 'em back in as loners
		 */
		foreach(int i in distInd)	
			pb_IntArrayUtility.AddValueAtIndex(ref sharedIndices, -1, i);

		pb.SetSharedIndicesUV(sharedIndices);

		return true;
	}
#endregion

#region Projection

	/**
	 * Projects UVs on all passed faces, automatically updating the sharedIndicesUV table
	 * as required (only associates vertices that share a seam).
	 */
	public static void ProjectFacesAuto(pb_Object pb, pb_Face[] faces)
	{
		int[] ind = pb_Face.AllTrianglesDistinct(faces);
		Vector3[] verts = pbUtil.ValuesWithIndices(pb.vertices, ind);
		
		/* get average face normal */
		Vector3 nrm = Vector3.zero;
		foreach(pb_Face face in faces)
			nrm += pb_Math.Normal(pb, face);
		nrm /= (float)faces.Length;

		/* project uv coordinates */
		Vector2[] uvs = pb_Math.PlanarProject(verts, nrm);

		/* re-assign new projected coords back into full uv array */
		Vector2[] rebuiltUVs = pb.uv;
		for(int i = 0; i < ind.Length; i++)
			rebuiltUVs[ind[i]] = uvs[i];

		/* and set the msh uv array using the new coordintaes */
		pb.SetUV(rebuiltUVs);
		pb.msh.uv = rebuiltUVs;
		
		/* now go trhough and set all adjacent face groups to use matching element groups */
		foreach(pb_Face f in faces)
		{
			f.elementGroup = -1;
			SplitUVs(pb, f.distinctIndices);
		}

		// pb_IntArray[] sharedIndices = pb.sharedIndices;

		pb.SewUVs(pb_Face.AllTrianglesDistinct(faces), .001f);

		// foreach(pb_Face f in faces)
		// {
		// 	foreach(pb_Edge e in f.edges)
		// 	{
		// 		foreach(pb_Face f2 in faces)
		// 		{
		// 			if(f2 == f) continue;
						
		// 			int index = f2.edges.IndexOf(e, sharedIndices);

		// 			// Found an aligned edge
		// 			if( index > -1 )
		// 			{
		// 				if(f.elementGroup < 0)
		// 				{
		// 					if(f2.elementGroup < 0)
		// 					{
		// 						f.elementGroup = pb.UnusedElementGroup(0);
		// 						f2.elementGroup = f.elementGroup;
		// 					}
		// 					else
		// 					{
		// 						f.elementGroup = f2.elementGroup;
		// 					}
		// 				}
		// 				else
		// 				{
		// 					if(f2.elementGroup < 0)
		// 						f2.elementGroup = f.elementGroup;
		// 					else
		// 					{
		// 						foreach(pb_Face iter in System.Array.FindAll(faces, element => element.elementGroup == f2.elementGroup))
		// 							iter.elementGroup = f.elementGroup;
		// 					}
		// 				}
		// 			}
		// 		}
		// 	}
		// }
	}

	/**
	 * Projects UVs for each face using the closest normal on a box.
	 */
	public  static void ProjectFacesBox(pb_Object pb, pb_Face[] faces)
	{
		Vector2[] uv = pb.uv;

		Dictionary<ProjectionAxis, List<pb_Face>> sorted = new Dictionary<ProjectionAxis, List<pb_Face>>();

		for(int i = 0; i < faces.Length; i++)
		{
			Vector3 nrm = pb_Math.Normal(pb, faces[i]);
			ProjectionAxis axis = pb_Math.VectorToProjectionAxis(nrm);

			if(sorted.ContainsKey(axis))
			{
				sorted[axis].Add(faces[i]);
			}
			else
			{
				sorted.Add(axis, new List<pb_Face>() { faces[i] });
			}

			// clean up UV stuff - no shared UV indices and remove element group
			faces[i].elementGroup = -1;
		}

		foreach(KeyValuePair<ProjectionAxis, List<pb_Face>> kvp in sorted)
		{
			int[] distinct = pb_Face.AllTrianglesDistinct(kvp.Value.ToArray());

			Vector2[] uvs = pb_Math.PlanarProject( pb.GetVertices(distinct), pb_Math.ProjectionAxisToVector(kvp.Key), kvp.Key );

			for(int n = 0; n < distinct.Length; n++)
				uv[distinct[n]] = uvs[n];
				
			SplitUVs(pb, distinct);
		}

		/* and set the msh uv array using the new coordintaes */
		pb.SetUV(uv);
		
		pb.ToMesh();
		pb.Refresh();
	}
#endregion

#region Fill Modes

	/*
	 *	Returns normalized UV values for a mesh uvs (0,0) - (1,1)
	 */
	public static Vector2[] FitUVs(Vector2[] uvs)
	{
		// shift UVs to zeroed coordinates
		Vector2 smallestVector2 = pb_Math.SmallestVector2(uvs);

		int i;
		for(i = 0; i < uvs.Length; i++)
		{
			uvs[i] -= smallestVector2;
		}

		float scale = pb_Math.LargestValue( pb_Math.LargestVector2(uvs) );

		for(i = 0; i < uvs.Length; i++)
		{
			uvs[i] /= scale;
		}

		return uvs;
	}
#endregion

#region Alignment

	/**
	 * Provided two faces, this method will attempt to project @f2 and align its size, rotation, and position
	 * to match the shared edge on f1.  Returns true on success, false otherwise.
	 */
	public static bool AutoStitch(pb_Object pb, pb_Face f1, pb_Face f2)
	{
		// Cache shared indices (we gon' use 'em a lot)
		pb_IntArray[] sharedIndices = pb.sharedIndices;

		for(int i = 0; i < f1.edges.Length; i++)
		{
			// find a matching edge
			int ind = f2.edges.IndexOf(f1.edges[i], sharedIndices);
			if( ind > -1 )
			{
				// First, project the second face
				pbUVOps.ProjectFacesAuto(pb, new pb_Face[] { f2 } );

				// Use the first first projected as the starting point
				// and match the vertices
				f1.manualUV = true;
				f2.manualUV = true;

				f1.textureGroup = -1;
				f2.textureGroup = -1;

				AlignEdges(pb, f1, f2, f1.edges[i], f2.edges[ind]);
				return true;
			}
		}

		// no matching edge found
		return false;
	}

	/**
	 * move the UVs to where the edges passed meet
	 */
	static bool AlignEdges(pb_Object pb, pb_Face f1, pb_Face f2, pb_Edge edge1, pb_Edge edge2)
	{
		Vector2[] uvs = pb.uv;
		pb_IntArray[] sharedIndices = pb.sharedIndices;
		pb_IntArray[] sharedIndicesUV = pb.sharedIndicesUV;

		/**
		 * Match each edge vertex to the other
		 */
		int[] matchX = new int[2] { edge1.x, -1 };
		int[] matchY = new int[2] { edge1.y, -1 };

		int siIndex = sharedIndices.IndexOf(edge1.x);
		if(siIndex < 0) 
			return false;

		if(sharedIndices[siIndex].array.Contains(edge2.x))
		{
			matchX[1] = edge2.x;
			matchY[1] = edge2.y;
		}
		else
		{
			matchX[1] = edge2.y;
			matchY[1] = edge2.x;
		}

		// scale face 2 to match the edge size of f1
		float dist_e1 = Vector2.Distance(uvs[edge1.x], uvs[edge1.y]);
		float dist_e2 = Vector2.Distance(uvs[edge2.x], uvs[edge2.y]);
		
		float scale = dist_e1/dist_e2;
		
		// doesn't matter what point we scale around because we'll move it in the next step anyways
		foreach(int i in f2.distinctIndices)
			uvs[i] = uvs[i].ScaleAroundPoint(Vector2.zero, Vector2.one * scale);

		/**
		 * Figure out where the center of each edge is so that we can move the f2 edge to match f1's origin 
		 */
		Vector2 f1_center = (uvs[edge1.x] + uvs[edge1.y]) / 2f;
		Vector2 f2_center = (uvs[edge2.x] + uvs[edge2.y]) / 2f;

		Vector2 diff = f1_center - f2_center;

		/**
		 * Move f2 face to where it's matching edge center is on top of f1's center
		 */
		foreach(int i in f2.distinctIndices)
			uvs[i] += diff;

		/**
		 * Now that the edge's centers are matching, rotate f2 to match f1's angle
		 */
		Vector2 angle1 = uvs[matchY[0]] - uvs[matchX[0]];
		Vector2 angle2 = uvs[matchY[1]] - uvs[matchX[1]];

		float angle = Vector2.Angle(angle1, angle2);
		if(Vector3.Cross(angle1, angle2).z < 0)
			angle = 360f - angle;
	
		foreach(int i in f2.distinctIndices)
			uvs[i] = pb_Math.RotateAroundPoint(uvs[i], f1_center, angle);

		float error = Mathf.Abs( Vector2.Distance(uvs[matchX[0]], uvs[matchX[1]]) ) + Mathf.Abs( Vector2.Distance(uvs[matchY[0]], uvs[matchY[1]]) );

		// now check that the matched UVs are on top of one another if the error allowance is greater than some small value
		if(error > .02)
		{
			// first try rotating 180 degrees
			foreach(int i in f2.distinctIndices)
				uvs[i] = pb_Math.RotateAroundPoint(uvs[i], f1_center, 180f);

			float e2 = Mathf.Abs( Vector2.Distance(uvs[matchX[0]], uvs[matchX[1]]) ) + Mathf.Abs( Vector2.Distance(uvs[matchY[0]], uvs[matchY[1]]) );
			if(e2 < error)
				error = e2;
			else
			{
				// flip 'em back around
				foreach(int i in f2.distinctIndices)
					uvs[i] = pb_Math.RotateAroundPoint(uvs[i], f1_center, 180f);
			}
		}

		// If successfully aligned, merge the sharedIndicesUV
		pbUVOps.SplitUVs(pb, f2.distinctIndices);

		pb_IntArrayUtility.MergeSharedIndices(ref sharedIndicesUV, matchX);
		pb_IntArrayUtility.MergeSharedIndices(ref sharedIndicesUV, matchY);

		pb_IntArray.RemoveEmptyOrNull(ref sharedIndicesUV);

		pb.SetSharedIndicesUV(sharedIndicesUV);

		// @todo Update Element Groups here?

		pb.SetUV(uvs);

		return true;
	}

	/**
	 * Attempts to translate, rotate, and scale @points to match @target as closely as possible.
	 * Only points[0, target.Length] coordinates are used in the matching process - points[target.Length, points.Length]
	 * are just along for the ride.
	 */
	public static pb_Transform2D MatchCoordinates(Vector2[] points, Vector2[] target)
	{
		int length = points.Length < target.Length ? points.Length : target.Length;

		pb_Bounds2D t_bounds = new pb_Bounds2D(target, length); // only match the bounds of known matching points

		// move points to the center of target
		Vector2 translation = t_bounds.center - pb_Bounds2D.Center(points, length);

		Vector2[] transformed = new Vector2[points.Length];
		for(int i = 0; i < points.Length; i++)
			transformed[i] = points[i] + translation;

		// rotate to match target points
		Vector2 target_angle = target[1]-target[0], transform_angle = transformed[1]-transformed[0];

		float angle = Vector2.Angle(target_angle, transform_angle);
		float dot = Vector2.Dot( pb_Math.Perpendicular(target_angle), transform_angle);

		if(dot < 0) angle = 360f - angle;

		for(int i = 0; i < points.Length; i++)
			transformed[i] = transformed[i].RotateAroundPoint(t_bounds.center, angle);

		// and lastly scale
		pb_Bounds2D p_bounds = new pb_Bounds2D(transformed, length);
		Vector2 scale = t_bounds.size.DivideBy(p_bounds.size);

		// for(int i = 0; i < points.Length; i++)
		// 	transformed[i] = transformed[i].ScaleAroundPoint(t_bounds.center, scale);

		return new pb_Transform2D(translation, angle, scale);
	}
#endregion

#region Auto or User

	/**
	 * Sets the passed faces to use Auto or Manual UVs, and (if previously manual) splits any vertex connections.
	 */
	public static void SetAutoUV(pb_Object pb, pb_Face[] faces, bool auto)
	{
		if(auto)
		{
			faces = System.Array.FindAll(faces, x => x.manualUV).ToArray();	// only operate on faces that were previously manual

			pb.SplitUVs( pb_Face.AllTriangles(faces) );

			Vector2[][] uv_origins = new Vector2[faces.Length][];
			for(int i = 0; i < faces.Length; i++)
				uv_origins[i] = pb.GetUVs(faces[i].distinctIndices);

			for(int f = 0; f < faces.Length; f++)
			{
				faces[f].uv.Reset();
				faces[f].manualUV = !auto;
				faces[f].elementGroup = -1;
			}

			pb.RefreshUV(faces);

			for(int i = 0; i < faces.Length; i++)
			{
				pb_Transform2D transform = MatchCoordinates(pb.GetUVs(faces[i].distinctIndices), uv_origins[i]);

				faces[i].uv.offset = -transform.position;
				faces[i].uv.rotation = transform.rotation;
	
				if( Mathf.Abs(transform.scale.sqrMagnitude - 2f) > .1f )
					faces[i].uv.scale = transform.scale;
			}
		}
		else
		{
			foreach(pb_Face f in faces)
			{
				f.textureGroup = -1;
				f.manualUV = !auto;
			}
		}
	}
#endregion

#region Info

	/**
	 * Iterates through uvs and returns the nearest Vector2 to pos.  If uvs lenght is < 1, return pos.
	 */
	public static Vector2 NearestVector2(Vector2 pos, Vector2[] uvs)
	{
		if(uvs.Length < 1) return pos;

		Vector2 nearest = uvs[0];
		float best = Vector2.Distance(pos, nearest);

		for(int i = 1; i < uvs.Length; i++)
		{
			float dist = Vector2.Distance(pos, uvs[i]);

			if(dist < best)
			{
				best = dist;
				nearest = uvs[i];
			}
		}

		return nearest;
	}
#endregion
}
}
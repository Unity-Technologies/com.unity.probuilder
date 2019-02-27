using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations
{
	/// <summary>
	/// UV actions.
	/// </summary>
	static partial class UVEditing
	{
		/// <summary>
		/// Sets the passed faces to use Auto or Manual UVs, and (if previously manual) splits any vertex connections.
		/// </summary>
		internal static void SetAutoUV(ProBuilderMesh mesh, Face[] faces, bool auto)
		{
			if (auto)
			{
				SetAutoAndAlignUnwrapParamsToUVs(mesh, faces.Where(x => x.manualUV));
			}
			else
			{
				foreach (Face f in faces)
				{
					f.textureGroup = -1;
					f.manualUV = true;
				}
			}
		}

        /// <summary>
        /// Reset the AutoUnwrapParameters of a set of faces to best match their current UV coordinates.
        /// </summary>
        /// <remarks>
        /// Auto UVs do not support distortion, so this conversion process cannot be loss-less. However as long as there
        /// is minimal skewing the results are usually very close.
        /// </remarks>
        internal static void SetAutoAndAlignUnwrapParamsToUVs(ProBuilderMesh mesh, IEnumerable<Face> facesToConvert)
        {
            var destinationTextures = mesh.textures.ToArray();
            var faces = facesToConvert as Face[] ?? facesToConvert.ToArray();

            foreach (var face in faces)
            {
	            face.uv = AutoUnwrapSettings.defaultAutoUnwrapSettings;
				face.elementGroup = -1;
				face.textureGroup = -1;
	            face.manualUV = false;
            }

	        mesh.RefreshUV(faces);
            var unmodifiedProjection = mesh.texturesInternal;

            foreach (var face in faces)
            {
	            var indices = face.indexesInternal;
	            var distinct = face.distinctIndexesInternal;

	            // rotate to match target points by comparing the angle between old UV and new auto projection
	            Vector2 dstAngle = destinationTextures[indices[1]] - destinationTextures[indices[0]];
	            Vector2 srcAngle = unmodifiedProjection[indices[1]] - unmodifiedProjection[indices[0]];
	            float rotation = Vector2.Angle(dstAngle, srcAngle);
	            if (Vector2.Dot(Vector2.Perpendicular(dstAngle), srcAngle) < 0)
		            rotation = 360f - rotation;

				// inverse the rotation to get an axis-aligned scale
	            Vector2 dstCenter = Bounds2D.Center(destinationTextures, indices);

	            for (int i = 0, c = distinct.Length; i < c; i++)
		            destinationTextures[distinct[i]] = destinationTextures[distinct[i]].RotateAroundPoint(dstCenter, -rotation);

	            var dstBounds = new Bounds2D(destinationTextures, indices);
	            var srcBounds = new Bounds2D(unmodifiedProjection, indices);

	            Vector2 scale = dstBounds.size.DivideBy(srcBounds.size);
	            Vector2 translation = dstCenter - srcBounds.center;

	            var uv = face.uv;
	            uv.offset = -translation;
	            uv.rotation = rotation;
	            uv.scale = scale;
	            face.uv = uv;
            }

            mesh.RefreshUV(faces);
        }

        internal struct UVTransform
        {
	        public Vector2 translation;
	        public float rotation;
	        public Vector2 scale;
        }

//        internal static UVTransform CalculateTransform(ProBuilderMesh mesh, Face face)
//        {
//			var unmodifiedProjection = Projection.PlanarProject(mesh.positions, face.distinctIndexesInternal, Math.Normal(mesh, face));
//
//	        return new UVTransform()
//		        { };
//        }

        internal static UVTransform CalculateDelta(IList<Vector2> src, IList<Vector2> dst, IList<int> indices)
        {
	        // rotate to match target points by comparing the angle between old UV and new auto projection
	        Vector2 dstAngle = dst[indices[1]] - dst[indices[0]];
	        Vector2 srcAngle = src[indices[1]] - src[indices[0]];
	        float rotation = Vector2.Angle(dstAngle, srcAngle);
	        if (Vector2.Dot(Vector2.Perpendicular(dstAngle), srcAngle) < 0)
		        rotation = 360f - rotation;

	        Vector2 dstCenter = Bounds2D.Center(dst, indices);

	        // inverse the rotation to get an axis-aligned scale
	        Vector2 dstSize = GetRotatedSize(dst, indices, dstCenter, -rotation);

	        var srcBounds = new Bounds2D(src, indices);

			return new UVTransform()
			{
				translation = dstCenter - srcBounds.center,
				rotation = rotation,
				scale = dstSize.DivideBy(srcBounds.size)
			};
        }

        static Vector2 GetRotatedSize(IList<Vector2> points, IList<int> indices, Vector2 center, float rotation)
        {
	        float xMin = 0f,
		        xMax = 0f,
		        yMin = 0f,
		        yMax = 0f;

	        int size = indices.Count;

	        Vector2 point = points[indices[0]].RotateAroundPoint(center, rotation);

	        xMin = point.x;
	        yMin = point.y;
	        xMax = xMin;
	        yMax = yMin;

	        for (int i = 1; i < size; i++)
	        {
		        point = points[indices[i]].RotateAroundPoint(center, rotation);

		        float x = point.x;
		        float y = point.y;

		        if (x < xMin) xMin = x;
		        if (x > xMax) xMax = x;

		        if (y < yMin) yMin = y;
		        if (y > yMax) yMax = y;
	        }

	        return new Vector2(xMax - xMin, yMax - yMin);
        }
	}
}

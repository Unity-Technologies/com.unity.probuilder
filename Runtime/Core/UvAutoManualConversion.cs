using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// UV actions.
	/// </summary>
	static partial class UvUnwrapping
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
				var trs = CalculateDelta(unmodifiedProjection, face.indexesInternal, destinationTextures, face.indexesInternal);
				var uv = face.uv;

				// offset is flipped for legacy reasons. people were confused that positive offsets moved the texture
				// in negative directions in the scene-view, but positive in UV window. if changed we would need to
				// write some way of upgrading the unwrap settings to account for this.
				uv.offset = -trs.translation;
				uv.rotation = trs.rotation;
				uv.scale = trs.scale;
				face.uv = uv;
			}

			mesh.RefreshUV(faces);
		}

		internal struct UVTransform
		{
			public Vector2 translation;
			public float rotation;
			public Vector2 scale;

			public override string ToString()
			{
				return translation.ToString("F2") + ", " + rotation + ", " + scale.ToString("F2");
			}
		}

		static List<Vector2> s_UVTransformProjectionBuffer = new List<Vector2>(8);

        /// <summary>
        /// Returns the auto unwrap settings for a face. In cases where the face is auto unwrapped (manualUV = false),
        /// this returns an unmodified copy of the AutoUnwrapSettings. If the face is manually unwrapped, it returns
        /// the auto unwrap settings computed from GetUVTransform.
        /// </summary>
        /// <returns></returns>
        internal static AutoUnwrapSettings GetAutoUnwrapSettings(ProBuilderMesh mesh, Face face)
        {
            if (!face.manualUV)
                return new AutoUnwrapSettings(face.uv);

            var trs = GetUVTransform(mesh, face);
            var uvSettings = AutoUnwrapSettings.defaultAutoUnwrapSettings;
            uvSettings.offset = trs.translation;
            uvSettings.rotation = 360 - trs.rotation;
            uvSettings.scale = uvSettings.scale / trs.scale;

            return uvSettings;
        }

        /// <summary>
        /// Attempt to calculate the UV transform for a face. In cases where the face is auto unwrapped
        /// (manualUV = false), this returns the offset, rotation, and scale from <see cref="Face.uv"/>. If the face is
        /// manually unwrapped, a transform will be calculated by trying to match an unmodified planar projection to the
        /// current UVs. The results
        /// </summary>
        /// <returns></returns>
        internal static UVTransform GetUVTransform(ProBuilderMesh mesh, Face face)
		{
			Projection.PlanarProject(mesh.positionsInternal, face.indexesInternal, Math.Normal(mesh, face), s_UVTransformProjectionBuffer);
			return CalculateDelta(mesh.texturesInternal, face.indexesInternal, s_UVTransformProjectionBuffer, null);
		}

		// messy hack to support cases where you want to iterate a collection of values with an optional collection of
		// indices. do not make public.
		static int GetIndex(IList<int> collection, int index)
		{
			return collection == null ? index : collection[index];
		}

		// indices arrays are optional - if null is passed the index will be 0, 1, 2... up to values array length.
		// this is done to avoid allocating a separate array just to pass linear indices
		internal static UVTransform CalculateDelta(IList<Vector2> src, IList<int> srcIndices, IList<Vector2> dst, IList<int> dstIndices)
        {
	        // rotate to match target points by comparing the angle between old UV and new auto projection
	        Vector2 srcAngle = src[GetIndex(srcIndices, 1)] - src[GetIndex(srcIndices, 0)];
	        Vector2 dstAngle = dst[GetIndex(dstIndices, 1)] - dst[GetIndex(dstIndices, 0)];

	        float rotation = Vector2.Angle(dstAngle, srcAngle);

	        if (Vector2.Dot(Vector2.Perpendicular(dstAngle), srcAngle) < 0)
		        rotation = 360f - rotation;

	        Vector2 dstCenter = dstIndices == null ? Bounds2D.Center(dst) : Bounds2D.Center(dst, dstIndices);

	        // inverse the rotation to get an axis-aligned scale
	        Vector2 dstSize = GetRotatedSize(dst, dstIndices, dstCenter, -rotation);
	        Bounds2D srcBounds = srcIndices == null ? new Bounds2D(src) : new Bounds2D(src, srcIndices);
            Vector2 scale = dstSize.DivideBy(srcBounds.size);

            // Calculate new bounds after rotation
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            int count = srcIndices?.Count ?? src.Count;
            for (int i = 0; i < count; i++)
            {
                int index = GetIndex(srcIndices, i);
                Vector2 rotated = Math.RotateAroundPoint(src[index], srcBounds.center, rotation);
                min.x = Mathf.Min(min.x, rotated.x);
                min.y = Mathf.Min(min.y, rotated.y);
                max.x = Mathf.Max(max.x, rotated.x);
                max.y = Mathf.Max(max.y, rotated.y);
            }

            // Calculate center of rotated bounds, and apply the calculated scale afterwards
            Vector2 rotatedCenter = (min + max) * 0.5f;
            Vector2 srcTransformedCenter = rotatedCenter * scale;

			return new UVTransform()
			{
				translation =  dstCenter - srcTransformedCenter,
				rotation = rotation,
				scale = dstSize.DivideBy(srcBounds.size)
			};
        }

        static Vector2 GetRotatedSize(IList<Vector2> points, IList<int> indices, Vector2 center, float rotation)
        {
	        int size = indices == null ? points.Count : indices.Count;

	        Vector2 point = points[GetIndex(indices, 0)].RotateAroundPoint(center, rotation);

	        float xMin = point.x;
	        float yMin = point.y;
	        float xMax = xMin;
	        float yMax = yMin;

	        for (int i = 1; i < size; i++)
	        {
		        point = points[GetIndex(indices, i)].RotateAroundPoint(center, rotation);

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

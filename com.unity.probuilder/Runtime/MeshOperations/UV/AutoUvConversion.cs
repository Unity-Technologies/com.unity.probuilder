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
            var origins = mesh.textures.ToArray();
            var faces = facesToConvert as Face[] ?? facesToConvert.ToArray();

            foreach (var face in faces)
            {
	            face.uv = AutoUnwrapSettings.defaultAutoUnwrapSettings;
				face.elementGroup = -1;
				face.textureGroup = -1;
	            face.manualUV = false;
            }

	        mesh.RefreshUV(faces);
            var textures = mesh.texturesInternal;

            foreach (var face in faces)
            {
	            var indices = face.indexesInternal;
	            var distinct = face.distinctIndexesInternal;

	            // rotate to match target points by comparing the angle between old UV and new auto projection
	            Vector2 dstAngle = origins[indices[1]] - origins[indices[0]];
	            Vector2 srcAngle = textures[indices[1]] - textures[indices[0]];
	            float rotation = Vector2.Angle(dstAngle, srcAngle);
	            if (Vector2.Dot(Vector2.Perpendicular(dstAngle), srcAngle) < 0)
		            rotation = 360f - rotation;

				// inverse the rotation to get an axis-aligned scale
	            Vector2 dstCenter = Bounds2D.Center(origins, indices);

	            for (int i = 0, c = distinct.Length; i < c; i++)
		            origins[distinct[i]] = origins[distinct[i]].RotateAroundPoint(dstCenter, -rotation);

	            var dstBounds = new Bounds2D(origins, indices);
	            var srcBounds = new Bounds2D(textures, indices);

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
	}
}

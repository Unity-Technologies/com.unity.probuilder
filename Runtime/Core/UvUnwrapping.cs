using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder
{
    static partial class UvUnwrapping
    {
        static Vector2 s_TempVector2 = Vector2.zero;
        static readonly List<int> s_IndexBuffer = new List<int>(64);

        internal static void Unwrap(ProBuilderMesh mesh, Face face, Vector3 projection = default)
        {
            Projection.PlanarProject(mesh, face, projection != Vector3.zero ? projection : Vector3.zero);
            ApplyUVSettings(mesh.texturesInternal, face.distinctIndexesInternal, face.uv);
        }

        /// <summary>
        /// Copy UVs from source to dest for the given mesh.
        /// </summary>
        /// <param name="mesh">ProbuilderMesh</param>
        /// <param name="source">face to copy UVs from</param>
        /// <param name="dest">face to copy UVs to</param>
        internal static void CopyUVs(ProBuilderMesh mesh, Face source, Face dest)
        {
            var uvs = mesh.texturesInternal;
            var sourceIndexes = source.distinctIndexesInternal;
            var destIndexes = dest.distinctIndexesInternal;
            for (int i = 0; i < sourceIndexes.Length; i++)
            {
                uvs[destIndexes[i]].x = uvs[sourceIndexes[i]].x;
                uvs[destIndexes[i]].y = uvs[sourceIndexes[i]].y;
            }
        }

        internal static void ProjectTextureGroup(ProBuilderMesh mesh, int group, AutoUnwrapSettings unwrapSettings)
        {
            Projection.PlanarProject(mesh, group, unwrapSettings);

            s_IndexBuffer.Clear();
            foreach (var face in mesh.facesInternal)
            {
                if (face.textureGroup == group)
                {
                    s_IndexBuffer.AddRange(face.distinctIndexesInternal);
                }
            }

            ApplyUVSettings(mesh.texturesInternal, s_IndexBuffer, unwrapSettings);
        }

        static void ApplyUVSettings(Vector2[] uvs, IList<int> indexes, AutoUnwrapSettings uvSettings)
        {
            int len = indexes.Count;
            Bounds2D bounds = new Bounds2D(uvs, indexes);

            switch (uvSettings.fill)
            {
                case AutoUnwrapSettings.Fill.Tile:
                    break;
                case AutoUnwrapSettings.Fill.Fit:
                    var max = Mathf.Max(bounds.size.x, bounds.size.y);
                    ScaleUVs(uvs, indexes, new Vector2(max, max), bounds);
                    bounds.center /= max;
                    break;
                case AutoUnwrapSettings.Fill.Stretch:
                    ScaleUVs(uvs, indexes, bounds.size, bounds);
                    bounds.center /= bounds.size;
                    break;
            }

            // Apply transform last, so that fill and justify don't override it.
            if (uvSettings.scale.x != 1f || uvSettings.scale.y != 1f || uvSettings.rotation != 0f)
            {
                // apply an offset to the positions relative to UV scale before rotation or scale is applied so that
                // UVs remain static in UV space
                Vector2 scaledCenter = bounds.center * uvSettings.scale;
                Vector2 delta = bounds.center - scaledCenter;
                Vector2 center = scaledCenter;

                for (int i = 0; i < len; i++)
                {
                    uvs[indexes[i]] -= delta;
                    uvs[indexes[i]] = uvs[indexes[i]].ScaleAroundPoint(center, uvSettings.scale);
                    uvs[indexes[i]] = uvs[indexes[i]].RotateAroundPoint(center, uvSettings.rotation);
                }
            }

            if (!uvSettings.useWorldSpace && uvSettings.anchor != AutoUnwrapSettings.Anchor.None)
                ApplyUVAnchor(uvs, indexes, uvSettings.anchor);

            if (uvSettings.flipU || uvSettings.flipV || uvSettings.swapUV)
            {
                for (int i = 0; i < len; i++)
                {
                    float   u = uvs[indexes[i]].x,
                            v = uvs[indexes[i]].y;

                    if (uvSettings.flipU)
                        u = -u;

                    if (uvSettings.flipV)
                        v = -v;

                    if (!uvSettings.swapUV)
                    {
                        uvs[indexes[i]].x = u;
                        uvs[indexes[i]].y = v;
                    }
                    else
                    {
                        uvs[indexes[i]].x = v;
                        uvs[indexes[i]].y = u;
                    }
                }
            }

            for (int i = 0; i < indexes.Count; i++)
            {
                uvs[indexes[i]].x -= uvSettings.offset.x;
                uvs[indexes[i]].y -= uvSettings.offset.y;
            }
        }

        static void ScaleUVs(Vector2[] uvs, IList<int> indexes, Vector2 scale, Bounds2D bounds)
        {
            var center = bounds.center;
            Vector2 scaledCenter = center / scale;
            Vector2 delta = center - scaledCenter;
            center = scaledCenter;

            for (int i = 0; i < indexes.Count; i++)
            {
                var uv = uvs[indexes[i]] - delta;
                uv.x = ((uv.x - center.x) / scale.x) + center.x;
                uv.y = ((uv.y - center.y) / scale.y) + center.y;
                uvs[indexes[i]] = uv;
            }
        }

        static void ApplyUVAnchor(Vector2[] uvs, IList<int> indexes, AutoUnwrapSettings.Anchor anchor)
        {
            s_TempVector2.x = 0f;
            s_TempVector2.y = 0f;

            Vector2 min = Math.SmallestVector2(uvs, indexes);
            Vector2 max = Math.LargestVector2(uvs, indexes);

            if (anchor == AutoUnwrapSettings.Anchor.UpperLeft || anchor == AutoUnwrapSettings.Anchor.MiddleLeft || anchor == AutoUnwrapSettings.Anchor.LowerLeft)
                s_TempVector2.x = min.x;
            else if (anchor == AutoUnwrapSettings.Anchor.UpperRight || anchor == AutoUnwrapSettings.Anchor.MiddleRight || anchor == AutoUnwrapSettings.Anchor.LowerRight)
                s_TempVector2.x = max.x - 1f;
            else
                s_TempVector2.x = (min.x + ((max.x - min.x) * .5f)) - .5f;

            if (anchor == AutoUnwrapSettings.Anchor.UpperLeft || anchor == AutoUnwrapSettings.Anchor.UpperCenter || anchor == AutoUnwrapSettings.Anchor.UpperRight)
                s_TempVector2.y = max.y - 1f;
            else if (anchor == AutoUnwrapSettings.Anchor.MiddleLeft || anchor == AutoUnwrapSettings.Anchor.MiddleCenter || anchor == AutoUnwrapSettings.Anchor.MiddleRight)
                s_TempVector2.y = (min.y + ((max.y - min.y) * .5f)) - .5f;
            else
                s_TempVector2.y = min.y;

            int len = indexes.Count;

            for (int i = 0; i < len; i++)
            {
                uvs[indexes[i]].x -= s_TempVector2.x;
                uvs[indexes[i]].y -= s_TempVector2.y;
            }
        }

        // 2020/8/23 - scaled auto UV faces now have an offset applied to their projected coordinates so that they
        // remain static in UV space when the mesh geometry is modified
        internal static void UpgradeAutoUVScaleOffset(ProBuilderMesh mesh)
        {
            var original = mesh.textures.ToArray();
            mesh.RefreshUV(mesh.facesInternal);
            var textures = mesh.texturesInternal;

            foreach (var face in mesh.facesInternal)
            {
                if (face.manualUV)
                    continue;
                var utrs = CalculateDelta(original, face.indexesInternal, textures, face.indexesInternal);
                var auto = face.uv;
                auto.offset += utrs.translation;
                face.uv = auto;
            }
        }
    }
}

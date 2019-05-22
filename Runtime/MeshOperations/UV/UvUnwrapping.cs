using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    static class UvUnwrapping
    {
        static Vector2 s_TempVector2 = Vector2.zero;
        static readonly List<int> s_IndexBuffer = new List<int>(64);

        internal static void Unwrap(ProBuilderMesh mesh, Face face, Vector3 projection = default)
        {
            Projection.PlanarProject(mesh, face, projection != Vector3.zero? projection : Vector3.zero);
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

            switch (uvSettings.fill)
            {
                case AutoUnwrapSettings.Fill.Tile:
                    break;
                case AutoUnwrapSettings.Fill.Fit:
                    FitUVs(uvs, indexes);
                    break;
                case AutoUnwrapSettings.Fill.Stretch:
                    StretchUVs(uvs, indexes);
                    break;
            }

            // Apply transform last, so that fill and justify don't override it.
            if (uvSettings.scale.x != 1f ||
                uvSettings.scale.y != 1f ||
                uvSettings.rotation != 0f)
            {
                Vector2 center = Bounds2D.Center(uvs, indexes);

                for (int i = 0; i < len; i++)
                {
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

        static void StretchUVs(Vector2[] uvs, IList<int> indexes)
        {
            var bounds = new Bounds2D();
            bounds.SetWithPoints(uvs, indexes);
            var c = bounds.center;
            var s = bounds.size;

            for (int i = 0; i < indexes.Count; i++)
            {
                var uv = uvs[indexes[i]];

                uv.x = ((uv.x - c.x) / s.x) + c.x;
                uv.y = ((uv.y - c.y) / s.y) + c.y;

                uvs[indexes[i]] = uv;
            }
        }

        static void FitUVs(Vector2[] uvs, IList<int> indexes)
        {
            var bounds = new Bounds2D();
            bounds.SetWithPoints(uvs, indexes);
            var c = bounds.center;
            var s = Mathf.Max(bounds.size.x, bounds.size.y);

            for (int i = 0; i < indexes.Count; i++)
            {
                var uv = uvs[indexes[i]];

                uv.x = ((uv.x - c.x) / s) + c.x;
                uv.y = ((uv.y - c.y) / s) + c.y;

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
    }
}

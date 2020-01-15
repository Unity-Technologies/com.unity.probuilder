using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Helper functions for working with transforms.
    /// </summary>
    public static class TransformUtility
    {
        static Dictionary<Transform, Transform[]> s_ChildStack = new Dictionary<Transform, Transform[]>();

        /// <summary>
        /// Unparent all children from a transform, saving them for later re-parenting (see ReparentChildren).
        /// </summary>
        /// <param name="t"></param>
        internal static void UnparentChildren(Transform t)
        {
            Transform[] children = new Transform[t.childCount];
            
            for (int i = t.childCount - 1; i >= 0; --i)
            {
                Transform child = t.GetChild(i);
                children[i] = child;
                child.SetParent(null, true);
            }

            s_ChildStack.Add(t, children);
        }

        /// <summary>
        /// Re-parent all children to a transform.  Must have called UnparentChildren prior.
        /// </summary>
        /// <param name="t"></param>
        internal static void ReparentChildren(Transform t)
        {
            Transform[] children;

            if (s_ChildStack.TryGetValue(t, out children)) 
            { 
                foreach (Transform c in children)
                    c.SetParent(t, true);

                s_ChildStack.Remove(t);
            }
        }

        /// <summary>
        /// Transform a vertex into world space.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        /// <param name="vertex">A model space vertex.</param>
        /// <returns>A new vertex in world coordinate space.</returns>
        public static Vertex TransformVertex(this Transform transform, Vertex vertex)
        {
            var v = new Vertex();

            if (vertex.HasArrays(MeshArrays.Position))
                v.position = transform.TransformPoint(vertex.position);

            if (vertex.HasArrays(MeshArrays.Color))
                v.color = vertex.color;

            if (vertex.HasArrays(MeshArrays.Normal))
                v.normal = transform.TransformDirection(vertex.normal);

            if (vertex.HasArrays(MeshArrays.Tangent))
                v.tangent = transform.rotation * vertex.tangent;

            if (vertex.HasArrays(MeshArrays.Texture0))
                v.uv0 = vertex.uv0;

            if (vertex.HasArrays(MeshArrays.Texture1))
                v.uv2 = vertex.uv2;

            if (vertex.HasArrays(MeshArrays.Texture2))
                v.uv3 = vertex.uv3;

            if (vertex.HasArrays(MeshArrays.Texture3))
                v.uv4 = vertex.uv4;

            return v;
        }

        /// <summary>
        /// Transform a vertex from world space to local space.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        /// <param name="vertex">A world space vertex.</param>
        /// <returns>A new vertex in transform coordinate space.</returns>
        public static Vertex InverseTransformVertex(this Transform transform, Vertex vertex)
        {
            var v = new Vertex();

            if (vertex.HasArrays(MeshArrays.Position))
                v.position = transform.InverseTransformPoint(vertex.position);

            if (vertex.HasArrays(MeshArrays.Color))
                v.color = vertex.color;

            if (vertex.HasArrays(MeshArrays.Normal))
                v.normal = transform.InverseTransformDirection(vertex.normal);

            if (vertex.HasArrays(MeshArrays.Tangent))
                v.tangent = transform.InverseTransformDirection(vertex.tangent);

            if (vertex.HasArrays(MeshArrays.Texture0))
                v.uv0 = vertex.uv0;

            if (vertex.HasArrays(MeshArrays.Texture1))
                v.uv2 = vertex.uv2;

            if (vertex.HasArrays(MeshArrays.Texture2))
                v.uv3 = vertex.uv3;

            if (vertex.HasArrays(MeshArrays.Texture3))
                v.uv4 = vertex.uv4;

            return v;
        }
    }
}

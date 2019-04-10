using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
    static class MaterialUtility
    {
#if UNITY_2018_2_OR_NEWER
        static List<Material> s_MaterialArray = new List<Material>();
#endif

        internal static int GetMaterialCount(Renderer renderer)
        {
#if UNITY_2018_2_OR_NEWER
            s_MaterialArray.Clear();
            renderer.GetSharedMaterials(s_MaterialArray);
            return s_MaterialArray.Count;
#else
            return renderer.sharedMaterials.Length;
#endif
        }

        internal static Material GetSharedMaterial(Renderer renderer, int index)
        {
#if UNITY_2018_2_OR_NEWER
            s_MaterialArray.Clear();
            renderer.GetSharedMaterials(s_MaterialArray);
            var count = s_MaterialArray.Count;
            if (count < 1)
                return null;
            return s_MaterialArray[Math.Clamp(index, 0, count - 1)];
#else
            var array = renderer.sharedMaterials;
            var count = array == null ? 0 : array.Length;
            if (count < 1)
                return null;
            return array[Math.Clamp(index, 0, count - 1)];
#endif
        }
    }
}

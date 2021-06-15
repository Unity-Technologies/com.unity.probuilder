using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
    static class MaterialUtility
    {
        static List<Material> s_MaterialArray = new List<Material>();

        internal static int GetMaterialCount(Renderer renderer)
        {
            s_MaterialArray.Clear();
            renderer.GetSharedMaterials(s_MaterialArray);
            return s_MaterialArray.Count;
        }

        internal static Material GetSharedMaterial(Renderer renderer, int index)
        {
            s_MaterialArray.Clear();
            renderer.GetSharedMaterials(s_MaterialArray);
            var count = s_MaterialArray.Count;
            if (count < 1)
                return null;
            return s_MaterialArray[Math.Clamp(index, 0, count - 1)];
        }

        internal static void RemoveMaterialsAndTrimExcess(Renderer renderer, List<int> indicesToRemove, int submeshCount)
        {
            indicesToRemove.Sort();
            s_MaterialArray.Clear();
            renderer.GetSharedMaterials(s_MaterialArray);
            int startLength = s_MaterialArray.Count;

            for (int i = indicesToRemove.Count - 1; i >= 0; --i)
            {
                int indexToRemove = indicesToRemove[i];
                if (indexToRemove < s_MaterialArray.Count)
                    s_MaterialArray.RemoveAt(indexToRemove);
            }

            if (submeshCount < s_MaterialArray.Count)
                s_MaterialArray.RemoveRange(submeshCount, s_MaterialArray.Count - submeshCount);

            if (startLength != s_MaterialArray.Count)
                renderer.materials = s_MaterialArray.ToArray();
        }
    }
}

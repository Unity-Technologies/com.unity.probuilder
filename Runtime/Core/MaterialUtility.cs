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

        /// <summary>
        /// Given a sorted collection of material indices, removes renderer's shared materials
        /// </summary>
        /// <param name="renderer">Target renderer</param>
        /// <param name="materialIndices">Sorted collection of material indices</param>
        internal static void RemoveSharedMaterials(Renderer renderer, ICollection<int> materialIndices)
        {
            s_MaterialArray.Clear();
            renderer.GetSharedMaterials(s_MaterialArray);

            var materialsRemoved = 0;
            foreach (var materialIndex in materialIndices)
            {
                var adjustedIndex = materialIndex - materialsRemoved;

                if (adjustedIndex >= 0 && adjustedIndex < s_MaterialArray.Count)
                {
                    s_MaterialArray.RemoveAt(adjustedIndex);
                    materialsRemoved++;
                }
            }

            if (materialsRemoved > 0)
                renderer.sharedMaterials = s_MaterialArray.ToArray();
        }
    }
}

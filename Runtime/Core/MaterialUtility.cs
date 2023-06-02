using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
    static class MaterialUtility
    {
        internal static List<Material> s_MaterialArray = new List<Material>();

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
    }
}

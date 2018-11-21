using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Utility methods for working with UnityEngine.Selection.
    /// </summary>
    static class SelectionUtility
    {
        /// <summary>
        /// Remove a pb_Object (or it's GameObject) from the current Selection.
        /// </summary>
        /// <param name="pb"></param>
        public static void Remove(ProBuilderMesh pb)
        {
            if (pb == null)
                return;

            Selection.objects = Selection.objects.Where(x => !ObjectMatchesComponent(x, pb)).ToArray();
        }

        static bool ObjectMatchesComponent(Object o, Component c)
        {
            if (o is GameObject)
            {
                GameObject g = (GameObject)o;
                return g.GetComponent(c.GetType()) == c;
            }

            if (o is Component)
                return o as Component == c;

            return false;
        }
    }
}

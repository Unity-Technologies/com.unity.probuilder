using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    internal static class ShapeParameters
    {
        static Dictionary<string, Shape> s_Prefs = new Dictionary<string, Shape>();

        static ShapeParameters()
        {
            var types = typeof(Shape).Assembly.GetTypes();

            foreach (var type in types)
            {
                if (typeof(Shape).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    var name = "ShapeBuilder." + type.Name;
                    var pref = ProBuilderSettings.Get(name, SettingsScope.Project, (Shape)Activator.CreateInstance(type));
                    s_Prefs.Add(name, pref);
                }
            }
        }

        public static void SaveParams<T>(T shape) where T : Shape
        {
            var name = "ShapeBuilder." + shape.GetType().Name;
            if (s_Prefs.TryGetValue(name, out var data))
            {
                data = shape;
                ProBuilderSettings.Set(name, data);
            }
        }

        public static void SetToLastParams<T>(ref T shape) where T : Shape
        {
            var name = "ShapeBuilder." + shape.GetType().Name;
            if (s_Prefs.TryGetValue(name, out var data))
            {
                if (data != null && data != null)
                    shape = (T)data;
            }  
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    internal static class EditorShapeUtility
    {
        static Dictionary<string, Shape> s_Prefs = new Dictionary<string, Shape>();

        static EditorShapeUtility()
        {
            var types = TypeCache.GetTypesDerivedFrom<Shape>();

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
                s_Prefs[name] = data;
                ProBuilderSettings.Set(name, data);
            }
        }

        public static Shape GetLastParams(Type type)
        {
            if (!typeof(Shape).IsAssignableFrom(type))
            {
                throw new ArgumentException(nameof(type));
            }
            var name = "ShapeBuilder." + type.Name;
            if (s_Prefs.TryGetValue(name, out var data))
            {
                if (data != null)
                    return (Shape)data;
            }
            try
            {
                return Activator.CreateInstance(type) as Shape;
            }
            catch (Exception e)
            {
                Debug.LogError($"Cannot create shape of type { type.ToString() } because it doesn't have a default constructor.");
            }
            return default;
        }

        public static Shape CreateShape(Type type)
        {
            Shape shape = null;
            try
            {
                shape = Activator.CreateInstance(type) as Shape;
            }
            catch (Exception e)
            {
                Debug.LogError($"Cannot create shape of type { type.ToString() } because it doesn't have a default constructor.");
            }
            shape = GetLastParams(shape.GetType());
            return shape;
        }
    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    internal static class ShapeParameters
    {
        static Pref<ShapeSave[]> s_PrefList = new Pref<ShapeSave[]>("ShapeBuilder.ShapesSettings", new ShapeSave[]{ });
        static Dictionary<string, ShapeSave> s_Prefs = new Dictionary<string, ShapeSave>();

        static ShapeParameters()
        {
            //ProBuilderSettings.
            var list = new List<Type>();
            var types = typeof(Shape).Assembly.GetTypes();
            s_Prefs = new Dictionary<string, ShapeSave>();

            foreach (var type in types)
            {
                if (typeof(Shape).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    var name = "ShapeBuilder." + type.Name;
                    var pref = new ShapeSave { name = type.Name, shape = Activator.CreateInstance(type) as Shape };
                    s_Prefs.Add(type.Name, pref);
                }
            }
            foreach (var save in s_PrefList.value) 
            {
                if (save.name != null && s_Prefs.ContainsKey(save.name))
                {
                    s_Prefs[save.name] = save;
                }
            }
            s_PrefList.value = s_Prefs.Values.ToArray();
        }

        public static void SaveParams<T>(T shape) where T : Shape
        {
            if (s_Prefs.TryGetValue(shape.GetType().Name, out var data))
            {
                data.shape = shape;
                s_PrefList.value = s_PrefList.value;
            }
        }

        public static void SetToLastParams<T>(ref T shape) where T : Shape
        {
            if (s_Prefs.TryGetValue(shape.GetType().Name, out var data))
            {
                if (data != null && data.shape != null)
                    shape = (T)data.shape;
            }  
        }
    }

    [Serializable]
    class ShapeSave
    {
        [SerializeField]
        public string name;

        [SerializeReference]
        public Shape shape;
    }
}

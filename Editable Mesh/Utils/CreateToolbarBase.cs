#if !UNITY_2022_1_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;


abstract class CreateToolbarBase : UnityEditor.Editor
{
    protected abstract IEnumerable<string> toolbarElements { get; }

    const string k_ElementClassName = "unity-editor-toolbar-element";
    const string k_StyleSheetsPath = "StyleSheets/Toolbars/";

    protected static VisualElement CreateToolbar()
    {
        var target = new VisualElement();
        var path = k_StyleSheetsPath + "EditorToolbar";

        var common = EditorGUIUtility.Load($"{path}Common.uss") as StyleSheet;
        if (common != null)
            target.styleSheets.Add(common);

        var themeSpecificName = EditorGUIUtility.isProSkin ? "Dark" : "Light";
        var themeSpecific = EditorGUIUtility.Load($"{path}{themeSpecificName}.uss") as StyleSheet;
        if (themeSpecific != null)
            target.styleSheets.Add(themeSpecific);

        target.AddToClassList("unity-toolbar-overlay");
        target.style.flexDirection = FlexDirection.Row;
        return target;
    }

    public override VisualElement CreateInspectorGUI()
    {
        var root = CreateToolbar();

        var elements = TypeCache.GetTypesWithAttribute(typeof(EditorToolbarElementAttribute));

        foreach (var element in toolbarElements)
        {
            var type = elements.FirstOrDefault(x =>
            {
                var attrib = x.GetCustomAttribute<EditorToolbarElementAttribute>();
                return attrib != null && attrib.id == element;
            });

            if (type != null)
            {
                try
                {
                    const BindingFlags flags =  BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.CreateInstance;

                    var ve = (VisualElement)Activator.CreateInstance(type, flags, null, null, null, null);
                    ve.AddToClassList(k_ElementClassName);
                    root.Add(ve);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed creating toolbar element from ID \"{element}\".\n{e}");
                }
            }
        }

        EditorToolbarUtility.SetupChildrenAsButtonStrip(root);

        return root;
    }
}

#endif
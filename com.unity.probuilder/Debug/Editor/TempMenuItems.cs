using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder;
using UObject = UnityEngine.Object;

class TempMenuItems : EditorWindow
{
    [MenuItem("Tools/Temp Menu Item &d", false, 1000)]
    static void MenuInit()
    {
        List<string> textures;
        string obj, mat;

        ObjOptions options = new ObjOptions()
        {
            applyTransforms = false
        };

        if (ObjExporter.Export("probuilder cube", MeshSelection.top.Select(x => new Model("Cube", x)), out obj, out mat, out textures, options))
            System.IO.File.WriteAllText("/Users/karlh/Desktop/cube.obj", obj);
    }

    [MenuItem("Tools/Recompile")]
    static void Recompile()
    {
        if (ScriptingSymbolManager.ContainsDefine("PROBUILDER_RECOMPILE_FLAG"))
            ScriptingSymbolManager.RemoveScriptingDefine("PROBUILDER_RECOMPILE_FLAG");
        else
            ScriptingSymbolManager.AddScriptingDefine("PROBUILDER_RECOMPILE_FLAG");
    }
}

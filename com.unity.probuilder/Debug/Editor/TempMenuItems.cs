using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UObject = UnityEngine.Object;

class TempMenuItems : EditorWindow
{

    [MenuItem("Tools/Temp Menu Item &d", false, 1000)]
    static void MenuInit()
    {
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

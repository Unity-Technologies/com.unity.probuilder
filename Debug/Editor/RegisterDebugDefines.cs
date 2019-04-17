using UnityEditor;
using UnityEditor.ProBuilder;

[InitializeOnLoad]
public class RegisterDebugDefines
{
    static RegisterDebugDefines()
    {
        if (EditorPrefs.GetBool("DeveloperMode", false))
            ScriptingSymbolManager.AddScriptingDefine("DEVELOPER_MODE");
        else
            ScriptingSymbolManager.RemoveScriptingDefine("DEVELOPER_MODE");

        var progridsType = ProGridsInterface.GetProGridsType();

        if (progridsType != null)
            ScriptingSymbolManager.AddScriptingDefine("PROGRIDS_ENABLED");
        else
            ScriptingSymbolManager.RemoveScriptingDefine("PROGRIDS_ENABLED");
    }
}

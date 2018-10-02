using UnityEditor;
using UnityEditor.ProBuilder;

[InitializeOnLoad]
public class RegisterDefineInDeveloperMode
{
	static RegisterDefineInDeveloperMode()
	{
		if (EditorPrefs.GetBool("DeveloperMode", false))
			ScriptingSymbolManager.AddScriptingDefine("DEVELOPER_MODE");
		else
			ScriptingSymbolManager.RemoveScriptingDefine("DEVELOPER_MODE");
	}
}

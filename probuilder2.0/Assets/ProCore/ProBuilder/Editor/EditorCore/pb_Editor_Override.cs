using UnityEditor;
using UnityEngine;

public static class pb_Editor_Override
{
	public static string ToString(this PropertyModification mod)
	{
		return mod.propertyPath + ": " + mod.target.ToString() + " = " + mod.value.ToString();
	}
}
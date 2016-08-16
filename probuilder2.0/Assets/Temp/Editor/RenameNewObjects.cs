using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

[InitializeOnLoad]
public class RenameNewObjects : Editor
{
	static RenameNewObjects()
	{
		pb_EditorUtility.AddOnObjectCreatedListener(OnProBuilderObjectCreated);
	}

	~RenameNewObjects()
	{
		pb_EditorUtility.RemoveOnObjectCreatedListener(OnProBuilderObjectCreated);
	}

	static void OnProBuilderObjectCreated(pb_Object pb)
	{
		pb.gameObject.name = string.Format("pb_{0}{1}", pb.gameObject.name, pb.GetInstanceID());
	}
}

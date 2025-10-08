/**
 *	This script demonstrates how one might use the OnproBuilderObjectCreated delegate.
 */

// Uncomment this line to enable this script.
// #define PROBUILDER_API_EXAMPLE

#if PROBUILDER_API_EXAMPLE

using UnityEngine;
using UnityEditor;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.EditorExamples
{
	[InitializeOnLoad]
	public class RenameNewObjects : Editor
	{
		// Static constructor is called and subscribes to the OnProBuilderObjectCreated delegate.
		static RenameNewObjects()
		{
			pb_EditorApi.AddOnObjectCreatedListener(OnProBuilderObjectCreated);
		}

		~RenameNewObjects()
		{
			pb_EditorApi.RemoveOnObjectCreatedListener(OnProBuilderObjectCreated);
		}

		/// <summary>
		/// When a new object is created this function is called with a reference to the pb_Object last built.
		/// </summary>
		/// <param name="pb"></param>
		static void OnProBuilderObjectCreated(pb_Object pb)
		{
			pb.gameObject.name = string.Format("pb_{0}{1}", pb.gameObject.name, pb.GetObjectId());
		}
	}
}

#endif

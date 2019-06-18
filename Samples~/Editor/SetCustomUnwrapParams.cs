// Set new ProBuilder objects to use special UV2 unwrap params.
// Uncomment this line to enable this script.
// #define PROBUILDER_API_EXAMPLE

#if PROBUILDER_API_EXAMPLE

using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.EditorExamples
{
	[InitializeOnLoad]
	public class SetUnwrapParams : Editor
	{
		/// <summary>
		/// Static constructor is called and subscribes to the OnProBuilderObjectCreated delegate.
		/// </summary>
		static SetUnwrapParams()
		{
			pb_EditorApi.AddOnObjectCreatedListener(OnProBuilderObjectCreated);
		}

		~SetUnwrapParams()
		{
			pb_EditorApi.RemoveOnObjectCreatedListener(OnProBuilderObjectCreated);
		}

		/// <summary>
		/// When a new object is created this function is called with a reference to the pb_Object last built.
		/// </summary>
		/// <param name="pb"></param>
		static void OnProBuilderObjectCreated(pb_Object pb)
		{
			pb_UnwrapParameters up = pb.unwrapParameters;
			up.hardAngle = 88f; // range: 1f, 180f
			up.packMargin = 15f; // range: 1f, 64f
			up.angleError = 30f; // range: 1f, 75f
			up.areaError = 15f; // range: 1f, 75f
		}
	}
}

#endif

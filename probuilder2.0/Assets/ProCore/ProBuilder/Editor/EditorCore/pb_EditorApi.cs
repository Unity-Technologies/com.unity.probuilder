using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Register for ProBuilder editor callbacks.
	/// </summary>
	/// <remarks>
	/// This exists as a temporary measure to expose some of the more useful callbacks in the editor while I sort out
	/// the editor-side public API.
	/// </remarks>
	public static class pb_EditorApi
	{
		/// <summary>
		/// Delegate for Top/Geometry/Texture mode changes.
		/// </summary>
		public static void AddOnEditLevelChangedListener(Action<int> func)
		{
			pb_Editor.AddOnEditLevelChangedListener(func);
		}

		/// <summary>
		/// note - this was added in ProBuilder 2.5.1
		/// </summary>
		public static void AddOnObjectCreatedListener(OnObjectCreated func)
		{
			pb_EditorUtility.AddOnObjectCreatedListener(func);
		}

		/// <summary>
		/// Also called when the geometry is modified by ProBuilder.
		/// </summary>
		public static void AddOnSelectionUpdateListener(OnSelectionUpdateEventHandler func)
		{
			pb_Editor.OnSelectionUpdate += func;
		}

		/// <summary>
		/// Called when vertices are about to be modified.
		/// </summary>
		public static void AddOnVertexMovementBeginListener(OnVertexMovementBeginEventHandler func)
		{
			pb_Editor.OnVertexMovementBegin += func;
		}

		/// <summary>
		/// Called when vertices have been moved by ProBuilder.
		/// </summary>
		public static void AddOnVertexMovementFinishListener(OnVertexMovementFinishedEventHandler func)
		{
			pb_Editor.OnVertexMovementFinish += func;
		}

		/// <summary>
		/// Called when the Unity mesh is rebuilt from ProBuilder mesh data.
		/// </summary>
		public static void AddOnMeshCompiledListener(OnMeshCompiled func)
		{
			pb_EditorUtility.AddOnMeshCompiledListener(func);
		}

		public static void RemoveOnEditLevelChangedListener(Action<int> func)
		{
			pb_Editor.RemoveOnEditLevelChangedListener(func);
		}

		public static void RemoveOnObjectCreatedListener(OnObjectCreated func)
		{
			pb_EditorUtility.RemoveOnObjectCreatedListener(func);
		}

		public static void RemoveOnSelectionUpdateListener(OnSelectionUpdateEventHandler func)
		{
			pb_Editor.OnSelectionUpdate -= func;
		}

		public static void RemoveOnVertexMovementBeginListener(OnVertexMovementBeginEventHandler func)
		{
			pb_Editor.OnVertexMovementBegin -= func;
		}

		public static void RemoveOnVertexMovementFinishListener(OnVertexMovementFinishedEventHandler func)
		{
			pb_Editor.OnVertexMovementFinish -= func;
		}

		public static void RemoveOnMeshCompiledListener(OnMeshCompiled func)
		{
			pb_EditorUtility.RemoveOnMeshCompiledListener(func);
		}
	}
}

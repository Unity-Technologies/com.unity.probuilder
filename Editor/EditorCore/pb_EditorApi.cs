using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Register for ProBuilder editor callbacks.
	/// </summary>
	public static class pb_EditorApi
	{
		/// <summary>
		/// Delegate for Top/Geometry/Texture mode changes.
		/// </summary>
		public static void AddOnEditLevelChangedListener(Action<int> func)
		{
			ProBuilderEditor.AddOnEditLevelChangedListener(func);
		}

		/// <summary>
		/// note - this was added in ProBuilder 2.5.1
		/// </summary>
		public static void AddOnObjectCreatedListener(OnObjectCreated func)
		{
			EditorUtility.AddOnObjectCreatedListener(func);
		}

		/// <summary>
		/// Also called when the geometry is modified by ProBuilder.
		/// </summary>
		public static void AddOnSelectionUpdateListener(OnSelectionUpdateEventHandler func)
		{
			ProBuilderEditor.onSelectionUpdate += func;
		}

		/// <summary>
		/// Called when vertices are about to be modified.
		/// </summary>
		public static void AddOnVertexMovementBeginListener(OnVertexMovementBeginEventHandler func)
		{
			ProBuilderEditor.onVertexMovementBegin += func;
		}

		/// <summary>
		/// Called when vertices have been moved by ProBuilder.
		/// </summary>
		public static void AddOnVertexMovementFinishListener(OnVertexMovementFinishedEventHandler func)
		{
			ProBuilderEditor.onVertexMovementFinish += func;
		}

		/// <summary>
		/// Called when the Unity mesh is rebuilt from ProBuilder mesh data.
		/// </summary>
		public static void AddOnMeshCompiledListener(OnMeshCompiled func)
		{
			EditorMeshUtility.onMeshCompiled += func;
		}

		public static void RemoveOnEditLevelChangedListener(Action<int> func)
		{
			ProBuilderEditor.RemoveOnEditLevelChangedListener(func);
		}

		public static void RemoveOnObjectCreatedListener(OnObjectCreated func)
		{
			EditorUtility.RemoveOnObjectCreatedListener(func);
		}

		public static void RemoveOnSelectionUpdateListener(OnSelectionUpdateEventHandler func)
		{
			ProBuilderEditor.onSelectionUpdate -= func;
		}

		public static void RemoveOnVertexMovementBeginListener(OnVertexMovementBeginEventHandler func)
		{
			ProBuilderEditor.onVertexMovementBegin -= func;
		}

		public static void RemoveOnVertexMovementFinishListener(OnVertexMovementFinishedEventHandler func)
		{
			ProBuilderEditor.onVertexMovementFinish -= func;
		}

		public static void RemoveOnMeshCompiledListener(OnMeshCompiled func)
		{
			EditorMeshUtility.onMeshCompiled -= func;
		}
	}
}

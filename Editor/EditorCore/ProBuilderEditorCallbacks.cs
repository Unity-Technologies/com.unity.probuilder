using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Register for ProBuilder editor callbacks.
	/// </summary>
	public static class ProBuilderEditorCallbacks
	{
		/// <summary>
		/// Receive a callback when the ProBuilder edit level is changed.
		/// </summary>
		/// <see cref="ProBuilder.Core.EditLevel"/>
		public static void AddOnEditLevelChangedListener(Action<int> func)
		{
			ProBuilderEditor.AddOnEditLevelChangedListener(func);
		}

		/// <summary>
		/// Register a callback when a ProBuilder shape is created.
		/// </summary>
		public static void AddOnObjectCreatedListener(Action<pb_Object> func)
		{
			EditorUtility.AddOnObjectCreatedListener(func);
		}

		/// <summary>
		/// Called when the geometry is modified by ProBuilder.
		/// </summary>
		public static void AddOnSelectionUpdateListener(Action<pb_Object[]> func)
		{
			ProBuilderEditor.OnSelectionUpdate += func;
		}

		/// <summary>
		/// Called prior to mesh modification.
		/// </summary>
		public static void AddOnVertexMovementBeginListener(Action<pb_Object[]> func)
		{
			ProBuilderEditor.OnVertexMovementBegin += func;
		}

		/// <summary>
		/// Called after mesh modification.
		/// </summary>
		public static void AddOnVertexMovementFinishListener(Action<pb_Object[]> func)
		{
			ProBuilderEditor.OnVertexMovementFinish += func;
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

		public static void RemoveOnObjectCreatedListener(Action<pb_Object> func)
		{
			EditorUtility.RemoveOnObjectCreatedListener(func);
		}

		public static void RemoveOnSelectionUpdateListener(Action<pb_Object[]> func)
		{
			ProBuilderEditor.OnSelectionUpdate -= func;
		}

		public static void RemoveOnVertexMovementBeginListener(Action<pb_Object[]> func)
		{
			ProBuilderEditor.OnVertexMovementBegin -= func;
		}

		public static void RemoveOnVertexMovementFinishListener(Action<pb_Object[]> func)
		{
			ProBuilderEditor.OnVertexMovementFinish -= func;
		}

		public static void RemoveOnMeshCompiledListener(OnMeshCompiled func)
		{
			EditorMeshUtility.onMeshCompiled -= func;
		}
	}
}

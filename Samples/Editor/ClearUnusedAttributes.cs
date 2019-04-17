// Uncomment this line to enable this script.
// #define PROBUILDER_API_EXAMPLE

using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;

namespace ProBuilder.EditorExamples
{
	/// <summary>
	/// This script demonstrates one use case for the pb_EditorUtility.onMeshCompiled delegate.
	/// Whenever ProBuilder compiles a mesh it removes the colors, tangents, and uv attributes.
	/// </summary>
	[InitializeOnLoad]
	sealed class ClearUnusedAttributes : Editor
	{
		/// <summary>
		/// Static constructor is called and subscribes to the OnMeshCompiled delegate.
		/// </summary>
		static ClearUnusedAttributes()
		{
			EditorMeshUtility.meshOptimized += OnMeshCompiled;
		}

		/// <summary>
		/// When a ProBuilder object is compiled to UnityEngine.Mesh this is called.
		/// </summary>
		/// <param name="probuilderMesh"></param>
		/// <param name="mesh"></param>
		static void OnMeshCompiled(ProBuilderMesh probuilderMesh, Mesh mesh)
		{
#if PROBUILDER_API_EXAMPLE
			mesh.uv = null;
			mesh.colors32 = null;
			mesh.tangents = null;
#endif
		}
	}
}

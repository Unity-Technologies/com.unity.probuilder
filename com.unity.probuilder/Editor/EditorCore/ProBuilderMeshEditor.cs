using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	/// <inheritdoc />
	/// <summary>
	/// Custom editor for pb_Object type.
	/// </summary>
	[CustomEditor(typeof(ProBuilderMesh))]
	[CanEditMultipleObjects]
	sealed class ProBuilderMeshEditor : Editor
	{
		static class Styles
		{
			static bool s_Initialized;
			public static GUIStyle miniButton;

			public static void Init()
			{
				if (s_Initialized)
					return;
				s_Initialized = true;
				miniButton = new GUIStyle(GUI.skin.button);
				miniButton.stretchHeight = false;
				miniButton.stretchWidth = false;
				miniButton.padding = new RectOffset(6, 6, 3, 3);
			}
		}

		internal static event System.Action onGetFrameBoundsEvent;

		ProBuilderMesh m_Mesh;

		SerializedProperty m_GenerateUV2;
		SerializedProperty m_UnwrapParameters;

		GUIContent m_LightmapUVsContent = new GUIContent("Generate Lightmap UVs");

		ProBuilderEditor editor
		{
			get { return ProBuilderEditor.instance; }
		}

		Renderer m_MeshRenderer = null;
		Vector3 offset = Vector3.zero;

		public void OnEnable()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			m_Mesh = (ProBuilderMesh)target;

			if (!m_Mesh)
				return;

			m_GenerateUV2 = serializedObject.FindProperty("m_GenerateUV2");
			m_UnwrapParameters = serializedObject.FindProperty("m_UnwrapParameters");

			m_MeshRenderer = m_Mesh.gameObject.GetComponent<Renderer>();
			SelectionRenderState s = EditorUtility.GetSelectionRenderState();
			EditorUtility.SetSelectionRenderState(m_MeshRenderer, editor != null ? s & SelectionRenderState.Outline : s);

			foreach (var mesh in Selection.transforms.GetComponents<ProBuilderMesh>())
				EditorUtility.SynchronizeWithMeshFilter(mesh);
		}

		public override void OnInspectorGUI()
		{
			Styles.Init();

			Vector3 bounds = m_MeshRenderer != null ? m_MeshRenderer.bounds.size : Vector3.zero;
			EditorGUILayout.Vector3Field("Object Size (read only)", bounds);

#if PB_DEBUG
			GUILayout.TextField( string.IsNullOrEmpty(pb.asset_guid) ? "null" : pb.asset_guid );
#endif

			serializedObject.Update();

			EditorGUILayout.PropertyField(m_GenerateUV2, m_LightmapUVsContent);

			if (m_GenerateUV2.boolValue)
			{
				EditorGUILayout.PropertyField(m_UnwrapParameters, true);

				if (m_UnwrapParameters.isExpanded)
				{
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();

					if (GUILayout.Button("Reset", Styles.miniButton))
						ResetUnwrapParams(m_UnwrapParameters);

					if (GUILayout.Button("Apply", Styles.miniButton))
						RebuildLightmapUVs();

					GUILayout.EndHorizontal();
					GUILayout.Space(4);
				}
			}

			serializedObject.ApplyModifiedProperties();
		}

		void RebuildLightmapUVs()
		{
			foreach (var obj in targets)
			{
				if(obj is ProBuilderMesh)
					((ProBuilderMesh)obj).Optimize(true);
			}
		}

		void ResetUnwrapParams(SerializedProperty prop)
		{
			var hardAngle = prop.FindPropertyRelative("m_HardAngle");
			var packMargin = prop.FindPropertyRelative("m_PackMargin");
			var angleError = prop.FindPropertyRelative("m_AngleError");
			var areaError = prop.FindPropertyRelative("m_AreaError");

			hardAngle.floatValue = UnwrapParameters.k_HardAngle;
			packMargin.floatValue = UnwrapParameters.k_PackMargin;
			angleError.floatValue = UnwrapParameters.k_AngleError;
			areaError.floatValue = UnwrapParameters.k_AreaError;

			RebuildLightmapUVs();
		}

		bool HasFrameBounds()
		{
			if (m_Mesh == null)
				m_Mesh = (ProBuilderMesh)target;

			return ProBuilderEditor.instance != null &&
				InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms).Sum(x => x.selectedIndexesInternal.Length) > 0;
		}

		Bounds OnGetFrameBounds()
		{
			if (onGetFrameBoundsEvent != null)
				onGetFrameBoundsEvent();

			Vector3 min = Vector3.zero, max = Vector3.zero;
			bool init = false;

			foreach (ProBuilderMesh mesh in InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms))
			{
				int[] tris = mesh.selectedIndexesInternal;

				if (tris == null || tris.Length < 1)
					continue;

				Vector3[] verts = mesh.positionsInternal;
				var trs = mesh.transform;

				if (!init)
				{
					init = true;
					min = trs.TransformPoint(verts[tris[0]]);
					max = trs.TransformPoint(verts[tris[0]]);
				}

				for (int i = 0, c = tris.Length; i < c; i++)
				{
					Vector3 p = trs.TransformPoint(verts[tris[i]]);

					min.x = Mathf.Min(p.x, min.x);
					max.x = Mathf.Max(p.x, max.x);

					min.y = Mathf.Min(p.y, min.y);
					max.y = Mathf.Max(p.y, max.y);

					min.z = Mathf.Min(p.z, min.z);
					max.z = Mathf.Max(p.z, max.z);
				}
			}

			return new Bounds((min + max) / 2f, max != min ? max - min : Vector3.one * .1f);
		}
	}
}

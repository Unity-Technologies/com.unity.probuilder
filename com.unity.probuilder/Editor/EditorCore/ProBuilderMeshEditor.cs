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

			public static readonly GUIContent lightmapStatic = EditorGUIUtility.TrTextContent("Lightmap Static", "Controls whether the geometry will be marked as Static for lightmapping purposes. When enabled, this mesh will be present in lightmap calculations.");
			public static readonly GUIContent lightmapUVs = new GUIContent("Generate Lightmap UVs");

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

		SerializedObject m_GameObjectsSerializedObject;
		SerializedProperty m_GenerateUV2;
		SerializedProperty m_UnwrapParameters;
		SerializedProperty m_StaticEditorFlags;

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

			m_GameObjectsSerializedObject = new SerializedObject(serializedObject.targetObjects.Select(t => ((Component)t).gameObject).ToArray());

			m_GenerateUV2 = serializedObject.FindProperty("m_GenerateUV2");
			m_UnwrapParameters = serializedObject.FindProperty("m_UnwrapParameters");
			m_StaticEditorFlags = m_GameObjectsSerializedObject.FindProperty("m_StaticEditorFlags");

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

			LightmapStaticSettings();

			bool showLightmapSettings = (m_StaticEditorFlags.intValue & (int)StaticEditorFlags.LightmapStatic) != 0;

			if (showLightmapSettings)
			{
				EditorGUILayout.PropertyField(m_GenerateUV2, Styles.lightmapUVs);

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

				bool anyMissingLightmapUVs = targets.Any(x =>
				{
					if (x is ProBuilderMesh)
					{
						var uv2 = ((ProBuilderMesh)x).mesh.uv2;
						return uv2 == null || uv2.Length < 3;
					}

					return false;
				});

				if (anyMissingLightmapUVs)
				{
					EditorGUILayout.HelpBox("Lightmap UVs are missing, please generate Lightmap UVs.", MessageType.Warning);

					if (GUILayout.Button("Generate Lightmap UVs"))
						RebuildLightmapUVs();
				}
			}
			else
			{
				EditorGUILayout.HelpBox("To enable generation of lightmap UVs for this Mesh, please enable the 'Lightmap Static' property.", MessageType.Info);
			}

			serializedObject.ApplyModifiedProperties();
		}

		void LightmapStaticSettings()
		{
			bool lightmapStatic = (m_StaticEditorFlags.intValue & (int)StaticEditorFlags.LightmapStatic) != 0;

			EditorGUI.BeginChangeCheck();
			lightmapStatic = EditorGUILayout.Toggle(Styles.lightmapStatic, lightmapStatic);

			if (EditorGUI.EndChangeCheck())
			{
				SceneModeUtility.SetStaticFlags(m_GameObjectsSerializedObject.targetObjects, (int)StaticEditorFlags.LightmapStatic, lightmapStatic);
				m_GameObjectsSerializedObject.Update();
			}
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

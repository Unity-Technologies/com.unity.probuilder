// This script demonstrates how to create a new action that can be accessed from the ProBuilder toolbar.
// A new menu item is registered under "Geometry" actions called "Gen. Shadows".

using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.Rendering;

// When creating your own actions use your own namespace.
namespace ProBuilder.ExampleActions
{
	[ProBuilderMenuAction]
	sealed class CreateShadowObject : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Object; } }
		public override Texture2D icon { get { return null; } }
		public override TooltipContent tooltip { get { return k_Tooltip; } }

		static readonly GUIContent k_VolumeSize = new GUIContent("Volume Size", "How far the shadow volume extends from " +
			"the base mesh.  To visualize, imagine the width of walls.\n\nYou can also select the child ShadowVolume " +
			"object and turn the Shadow Casting Mode to \"One\" or \"Two\" sided to see the resulting mesh.");

		// What to show in the hover tooltip window.  TooltipContent is similar to GUIContent, with the exception
		// that it also includes an optional params[] char list in the constructor to define shortcut keys
		// (ex, CMD_CONTROL, K).
		static readonly TooltipContent k_Tooltip = new TooltipContent(
			"Gen Shadow Obj",
			"Creates a new ProBuilder mesh child with inverted normals that only exists to cast shadows. Use to " +
			"create lit interior scenes with shadows from directional lights.\n\nNote that this exists largely as " +
			"a workaround for real-time shadow light leaks. Baked shadows do not require this workaround.",
			""
		);

		static bool showPreview
		{
			get { return EditorPrefs.GetBool("pb_shadowVolumePreview", true); }
			set { EditorPrefs.SetBool("pb_shadowVolumePreview", value); }
		}

		// Determines if the action should be enabled or shown as disabled in the menu.
		public override bool enabled
		{
			get { return MeshSelection.selectedObjectCount > 0; }
		}

		/// <summary>
		/// Determines if the action should be loaded in the menu (ex, face actions shouldn't be shown when in vertex editing mode).
		/// </summary>
		/// <returns></returns>
		public override bool hidden
		{
			get { return false; }
		}

		protected override void OnSettingsEnable()
		{
			if( showPreview )
				DoAction();
		}

		protected override void OnSettingsGUI()
		{
			GUILayout.Label("Create Shadow Volume Options", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();

			EditorGUI.BeginChangeCheck();
			float volumeSize = EditorPrefs.GetFloat("pb_CreateShadowObject_volumeSize", .07f);
			volumeSize = EditorGUILayout.Slider(k_VolumeSize, volumeSize, 0.001f, 1f);
			if( EditorGUI.EndChangeCheck() )
				EditorPrefs.SetFloat("pb_CreateShadowObject_volumeSize", volumeSize);

			#if !UNITY_4_6 && !UNITY_4_7
			EditorGUI.BeginChangeCheck();
			ShadowCastingMode shadowMode = (ShadowCastingMode) EditorPrefs.GetInt("pb_CreateShadowObject_shadowMode", (int) ShadowCastingMode.ShadowsOnly);
			shadowMode = (ShadowCastingMode) EditorGUILayout.EnumPopup("Shadow Casting Mode", shadowMode);
			if(EditorGUI.EndChangeCheck())
				EditorPrefs.SetInt("pb_CreateShadowObject_shadowMode", (int) shadowMode);
			#endif

			EditorGUI.BeginChangeCheck();
			ExtrudeMethod extrudeMethod = (ExtrudeMethod) EditorPrefs.GetInt("pb_CreateShadowObject_extrudeMethod", (int) ExtrudeMethod.FaceNormal);
			extrudeMethod = (ExtrudeMethod) EditorGUILayout.EnumPopup("Extrude Method", extrudeMethod);
			if(EditorGUI.EndChangeCheck())
				EditorPrefs.SetInt("pb_CreateShadowObject_extrudeMethod", (int) extrudeMethod);

			if(EditorGUI.EndChangeCheck())
				DoAction();

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Create Shadow Volume"))
			{
				DoAction();
				SceneView.RepaintAll();
//				MenuOption.CloseAll();
			}
		}

		/// <summary>
		/// Perform the action.
		/// </summary>
		/// <returns>Return a pb_ActionResult indicating the success/failure of action.</returns>
		public override ActionResult DoAction()
		{
			ShadowCastingMode shadowMode = (ShadowCastingMode) EditorPrefs.GetInt("pb_CreateShadowObject_shadowMode", (int) ShadowCastingMode.ShadowsOnly);
			float extrudeDistance = EditorPrefs.GetFloat("pb_CreateShadowObject_volumeSize", .08f);
			ExtrudeMethod extrudeMethod = (ExtrudeMethod) EditorPrefs.GetInt("pb_CreateShadowObject_extrudeMethod", (int) ExtrudeMethod.FaceNormal);

			foreach(ProBuilderMesh mesh in MeshSelection.top)
			{
				ProBuilderMesh shadow = GetShadowObject(mesh);

				if(shadow == null)
					continue;

				foreach (Face f in shadow.faces)
				{
					f.SetIndexes(f.indexes.Reverse().ToArray());
					f.manualUV = true;
				}
				shadow.Extrude(shadow.faces, extrudeMethod, extrudeDistance);
				shadow.ToMesh();
				shadow.Refresh();
				shadow.Optimize();

				#if !UNITY_4_6 && !UNITY_4_7
				MeshRenderer mr = shadow.gameObject.GetComponent<MeshRenderer>();
				mr.shadowCastingMode = shadowMode;
				if(shadowMode == ShadowCastingMode.ShadowsOnly)
					mr.receiveShadows = false;
				#endif

				Collider collider = shadow.GetComponent<Collider>();

				while(collider != null)
				{
					Object.DestroyImmediate(collider);
					collider = shadow.GetComponent<Collider>();
				}
			}

			// Refresh the Editor wireframe and working caches.
			ProBuilderEditor.Refresh();

			return new ActionResult(ActionResult.Status.Success, "Create Shadow Object");
		}

		private ProBuilderMesh GetShadowObject(ProBuilderMesh mesh)
		{
			if(mesh == null || mesh.name.Contains("-ShadowVolume"))
				return null;

			for(int i = 0; i < mesh.transform.childCount; i++)
			{
				Transform t = mesh.transform.GetChild(i);

				if(t.name.Equals(string.Format("{0}-ShadowVolume", mesh.name)))
				{
					ProBuilderMesh shadow = t.GetComponent<ProBuilderMesh>();

					if(shadow != null)
					{
						Undo.RecordObject(shadow, "Update Shadow Object");

						Face[] faces = new Face[mesh.faceCount];

						for(int nn = 0; nn < mesh.faceCount; nn++)
							faces[nn] = new Face(mesh.faces[nn]);

						shadow.RebuildWithPositionsAndFaces(mesh.positions, faces);

						return shadow;
					}
				}
			}

			ProBuilderMesh newShadowMesh = ProBuilderMesh.Create();
			newShadowMesh.CopyFrom(mesh);
			newShadowMesh.name = string.Format("{0}-ShadowVolume", mesh.name);
			newShadowMesh.transform.SetParent(mesh.transform, false);
			Undo.RegisterCreatedObjectUndo(newShadowMesh.gameObject, "Create Shadow Object");
			return newShadowMesh;
		}
	}
}


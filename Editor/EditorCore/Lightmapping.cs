using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UL = UnityEditor.Lightmapping;
using UnityEditor.SettingsManagement;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Methods used in manipulating or creating Lightmaps.
    /// </summary>
    [InitializeOnLoad]
    static class Lightmapping
    {
#if UNITY_2019_2_OR_NEWER
        const StaticEditorFlags k_ContributeGI = StaticEditorFlags.ContributeGI;
#else
        const StaticEditorFlags k_ContributeGI = StaticEditorFlags.LightmapStatic;
#endif

        const string k_StaticEditorFlagsProperty = "m_StaticEditorFlags";

        [UserSetting("General", "Auto Lightmap UVs", "Automatically build the lightmap UV array when editing ProBuilder meshes. If this feature is disabled, you will need to use the 'Generate UV2' action to build lightmap UVs for meshes prior to baking lightmaps.")]
        static Pref<bool> s_AutoUnwrapLightmapUV = new Pref<bool>("lightmapping.autoUnwrapLightmapUV", true);

        [UserSetting("General", "Show Missing Lightmap UVs Warning", "Enable or disable a warning log if lightmaps are baked while ProBuilder shapes are missing a valid UV2 channel.")]
        static Pref<bool> s_ShowMissingLightmapUVWarning = new Pref<bool>("lightmapping.showMissingLightmapWarning", true, SettingsScope.User);

        [UserSetting]
        internal static Pref<UnwrapParameters> s_UnwrapParameters = new Pref<UnwrapParameters>("lightmapping.defaultLightmapUnwrapParameters", new UnwrapParameters());

        static Pref<UL.GIWorkflowMode> s_GiWorkflowMode = new Pref<UL.GIWorkflowMode>("lightmapping.giWorkflowMode", UL.GIWorkflowMode.Iterative, SettingsScope.User);

        static class Styles
        {
            public static readonly GUIContent hardAngle = new GUIContent("Hard Angle", "Angle between neighbor triangles that will generate seam.");
            public static readonly GUIContent packMargin = new GUIContent("Pack Margin", "Measured in pixels, assuming mesh will cover an entire 1024x1024 lightmap.");
            public static readonly GUIContent angleError = new GUIContent("Angle Error", "Measured in percents. Angle error measures deviation of UV angles from geometry angles.");
            public static readonly GUIContent areaError = new GUIContent("Area Error", "");

            static bool s_Initialized;
            public static GUIStyle miniButton;
            public static bool unwrapSettingsFoldout;

            public static void Init()
            {
                if (s_Initialized)
                    return;

                s_Initialized = true;

                miniButton = new GUIStyle(GUI.skin.button);
                miniButton.stretchHeight = false;
                miniButton.stretchWidth = false;
                miniButton.padding = new RectOffset(6, 6, 3, 3);
                miniButton.margin = new RectOffset(4, 4, 4, 0);
            }
        }

        [UserSettingBlock("Mesh Settings")]
        static void UnwrapSettingDefaults(string searchContext)
        {
            Styles.Init();
            var isSearching = !string.IsNullOrEmpty(searchContext);

            if (!isSearching)
                Styles.unwrapSettingsFoldout = EditorGUILayout.Foldout(Styles.unwrapSettingsFoldout, "Lightmap UVs Settings");

            if (isSearching || Styles.unwrapSettingsFoldout)
            {
                EditorGUI.BeginChangeCheck();

                var unwrap = (UnwrapParameters)s_UnwrapParameters;

                using (new SettingsGUILayout.IndentedGroup())
                {
                    unwrap.hardAngle = SettingsGUILayout.SearchableSlider(Styles.hardAngle, unwrap.hardAngle, 1f, 180f, searchContext);
                    unwrap.packMargin = SettingsGUILayout.SearchableSlider(Styles.packMargin, unwrap.packMargin, 1f, 64f, searchContext);
                    unwrap.angleError = SettingsGUILayout.SearchableSlider(Styles.angleError, unwrap.angleError, 1f, 75f, searchContext);
                    unwrap.areaError = SettingsGUILayout.SearchableSlider(Styles.areaError, unwrap.areaError, 1f, 75f, searchContext);

                    if (!isSearching)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Reset", Styles.miniButton))
                            unwrap.Reset();
                        GUILayout.EndHorizontal();
                    }
                }

                SettingsGUILayout.DoResetContextMenuForLastRect(s_UnwrapParameters);

                if (EditorGUI.EndChangeCheck())
                    s_UnwrapParameters.value = unwrap;
            }
        }

        public static bool autoUnwrapLightmapUV
        {
            get { return (bool)s_AutoUnwrapLightmapUV; }
            set { s_AutoUnwrapLightmapUV.value = value; }
        }

        static Lightmapping()
        {
#if UNITY_2019_2_OR_NEWER
            UL.bakeCompleted += OnLightmappingCompleted;
#else
            UL.completed += OnLightmappingCompleted;
#endif
            Undo.postprocessModifications += PostprocessModifications;
        }

        /// <summary>
        /// Toggles the LightmapStatic bit of an objects Static flags.
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="isEnabled"></param>
        public static void SetLightmapStaticFlagEnabled(ProBuilderMesh pb, bool isEnabled)
        {
            Entity ent = pb.GetComponent<Entity>();

            if (ent != null && ent.entityType == EntityType.Detail)
            {
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(pb.gameObject);

                if (isEnabled != (flags & k_ContributeGI) > 0)
                {
                    flags ^= k_ContributeGI;
                    GameObjectUtility.SetStaticEditorFlags(pb.gameObject, flags);
                }
            }
        }

        static UndoPropertyModification[] PostprocessModifications(UndoPropertyModification[] modifications)
        {
            if (!autoUnwrapLightmapUV)
                return modifications;

            foreach (var modification in modifications)
            {
                string property = modification.currentValue == null ? null : modification.currentValue.propertyPath;

                if (string.IsNullOrEmpty(property)
                    || !property.Equals(k_StaticEditorFlagsProperty)
                    || string.IsNullOrEmpty(modification.currentValue.value))
                    continue;

                var staticFlags = uint.Parse(modification.currentValue.value);
                var lightmapStatic = (staticFlags & (uint) k_ContributeGI) != 0;

                if (lightmapStatic)
                {
                    var gameObject = modification.currentValue.target as GameObject;

                    if (gameObject != null)
                    {
                        var mesh = gameObject.GetComponent<ProBuilderMesh>();

                        if (mesh != null)
                            mesh.Optimize();
                    }
                }
            }

            return modifications;
        }

        static void OnLightmappingCompleted()
        {
            if (!s_ShowMissingLightmapUVWarning)
                return;

            var missingUv2 = Object.FindObjectsOfType<ProBuilderMesh>().Where(x => !x.HasArrays(MeshArrays.Lightmap) && x.gameObject.HasStaticFlag(k_ContributeGI));

            int count = missingUv2.Count();

            if (count > 0)
                Log.Warning("{0} ProBuilder {1} included in lightmap bake with missing UV2. Use the Lightmap + options to find missing UV2s.\n(You can turn off this warning in Preferences/ProBuilder).", count, count == 1 ? "mesh" : "meshes");
        }

        /// <summary>
        /// Build Lightmap UVs for each mesh in the selection that is missing the UV2 array.
        /// </summary>
        /// <param name="selection"></param>
        /// <param name="showProgressBar"></param>
        public static int RebuildMissingLightmapUVs(IEnumerable<ProBuilderMesh> selection, bool showProgressBar = false)
        {
            int count = 0;
            float total = selection.Count(x => x.gameObject.HasStaticFlag(k_ContributeGI) && !x.HasArrays(MeshArrays.Lightmap));

            foreach (var mesh in selection)
            {
                if (!mesh.gameObject.HasStaticFlag(k_ContributeGI) || mesh.HasArrays(MeshArrays.Texture1))
                    continue;

                if (showProgressBar)
                {
                    if (UnityEditor.EditorUtility.DisplayCancelableProgressBar("Generate Lightmap UVs", "Unwrapping UVs for mesh: " + mesh.name, count / total))
                        break;
                }

                count++;
                mesh.Optimize(true);
            }

            UnityEditor.EditorUtility.ClearProgressBar();

            return count;
        }

        /**
         *  Get the UnwrapParam values from a pb_UnwrapParameters object.
         *  Not in pb_UnwrapParameters because UnwrapParam is an Editor class.
         */
        public static UnwrapParam GetUnwrapParam(UnwrapParameters parameters)
        {
            UnwrapParam param = new UnwrapParam();

            if (parameters != null)
            {
                param.angleError = Mathf.Clamp(parameters.angleError, 1f, 75f) * .01f;
                param.areaError  = Mathf.Clamp(parameters.areaError , 1f, 75f) * .01f;
                param.hardAngle  = Mathf.Clamp(parameters.hardAngle , 0f, 180f);
                param.packMargin = Mathf.Clamp(parameters.packMargin, 1f, 64) * .001f;
            }
            else
            {
                param.angleError = Mathf.Clamp(UnwrapParameters.k_AngleError, 1f, 75f) * .01f;
                param.areaError  = Mathf.Clamp(UnwrapParameters.k_AreaError , 1f, 75f) * .01f;
                param.hardAngle  = Mathf.Clamp(UnwrapParameters.k_HardAngle , 0f, 180f);
                param.packMargin = Mathf.Clamp(UnwrapParameters.k_PackMargin, 1f, 64) * .001f;
            }

            return param;
        }

        internal static void PushGIWorkflowMode()
        {
            s_GiWorkflowMode.SetValue(UL.giWorkflowMode, true);

            if (UL.giWorkflowMode != UL.GIWorkflowMode.Legacy)
                UL.giWorkflowMode = UL.GIWorkflowMode.OnDemand;
        }

        internal static void PopGIWorkflowMode()
        {
            UL.giWorkflowMode = s_GiWorkflowMode;
        }
    }
}

using UnityEditor;
using UnityEditor.SettingsManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProBuilder.Debug.Editor
{
    static class GuidesSettingsProvider
    {
        const string k_PreferencesPath = "Preferences/Scene Guides";
        static Settings s_Settings;
        public static Settings settings => s_Settings ?? (s_Settings = new Settings("com.unity.scene-guides"));

        [SettingsProvider]
        static SettingsProvider CreateSettingsProvider()
        {
            var provider = new UserSettingsProvider(k_PreferencesPath,
                settings,
                new[] { typeof(GuidesSettingsProvider).Assembly });

            settings.afterSettingsSaved += HandleUtility.Repaint;

            return provider;
        }
    }

    class Pref<T> : UserSetting<T>
    {
        public Pref(string key, T value, SettingsScope scope = SettingsScope.Project)
            : base(GuidesSettingsProvider.settings, key, value, scope) { }
    }

    static class Guides
    {
        [UserSetting("World Space", "Origin Axes", "Shows 3 axis guides from the scene view origin.")]
        static Pref<bool> s_SceneOrigin = new Pref<bool>("Guides.s_SceneOrigin", false);

        [UserSetting("Selection", "Transform Pivot", "Draw a gizmo at the origin of each selected transform.")]
        static Pref<bool> s_SelectionPivot = new Pref<bool>("Guides.s_SelectionPivot", false);

        static Vector3  zero = Vector3.zero,
                        up = new Vector3(0f, 1f, 0f),
                        right = new Vector3(1f, 0f, 0f),
                        forward = new Vector3(0f, 0f, 1f);

        [InitializeOnLoadMethod]
        static void Init()
        {
            SceneView.duringSceneGui += view =>
            {
                var evt = Event.current;

                if (evt.type != EventType.Repaint)
                    return;

                if (s_SceneOrigin)
                {
                    var dist = view.camera.farClipPlane / 10f;

                    DrawLine(zero, right * dist, Handles.xAxisColor);
                    DrawLine(zero, right * -dist, Handles.xAxisColor * .7f);

                    DrawLine(zero, up * dist, Handles.yAxisColor);
                    DrawLine(zero, up * -dist, Handles.yAxisColor * .7f);

                    DrawLine(zero, forward * dist, Handles.zAxisColor);
                    DrawLine(zero, forward * -dist, Handles.zAxisColor * .7f);
                }

                if (s_SelectionPivot)
                {
                    foreach (var transform in Selection.transforms)
                        DrawPivot(transform);
                }
            };
        }

        static void DrawPivot(Transform transform)
        {
            using (new Handles.DrawingScope(transform.localToWorldMatrix))
            {
                DrawLine(zero, right * .2f, Handles.xAxisColor, 1f, 3f);
                DrawLine(zero, up * .2f, Handles.yAxisColor, 1f, 3f);
                DrawLine(zero, forward * .2f, Handles.zAxisColor, 1f, 3f);
            }
        }

        static void DrawLine(Vector3 from, Vector3 to, Color color, float occludedTint = .7f, float thickness = 0f)
        {
            Handles.color = color;
            Handles.zTest = CompareFunction.LessEqual;
            Handles.DrawLine(from, to, thickness);

            Handles.color = color * occludedTint;
            Handles.zTest = CompareFunction.Greater;
            Handles.DrawLine(from, to, thickness);
        }
    }
}

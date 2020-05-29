using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using UnityEditor;
using UnityEngine;

public class EditorPrefBrowser : EditorWindow, IHasCustomMenu
{
    private static class Styles
    {
        public static readonly GUIStyle ToolbarSearchField = "ToolbarSeachTextField";
        public static readonly GUIStyle ToolbarSearchFieldCancel = "ToolbarSeachCancelButton";
        public static readonly GUIStyle ToolbarSearchFieldCancelEmpty = "ToolbarSeachCancelButtonEmpty";

        public static readonly GUIStyle HeaderBackground = new GUIStyle(GUI.skin.box);

        static Styles()
        {
            // Zero out margin to go to edges of window
            HeaderBackground.margin = new RectOffset();

            //
            HeaderBackground.overflow = new RectOffset(1, 1, 0, 0);
        }
    }

    private const string kUnityRootSubKey = "Software\\Unity Technologies\\Unity Editor 5.x\\";

    [NonSerialized]
    private readonly SortedDictionary<string, object> m_EditorPrefsLookup = new SortedDictionary<string, object>();

    [SerializeField]
    private Vector2 m_ScrollPosition = new Vector2(0f, 0f);

    [SerializeField]
    private string m_Filter = "";

    private bool IsFiltering
    {
        get { return !string.IsNullOrEmpty(m_Filter); }
    }

    [MenuItem("Tools/Debug/Editor Pref Browser")]
    public static void ShowWindow()
    {
        GetWindow<EditorPrefBrowser>().titleContent = new GUIContent("Editor Pref");
    }

    public void OnEnable()
    {
        FetchKeyValues();
    }

    public void OnGUI()
    {
        EditorGUIUtility.labelWidth = 400f;
        DoToolbar();
        DoHeader();
        DoList();
        EditorGUIUtility.labelWidth = 0f;
    }

    private void DoHeader()
    {
        using (new EditorGUILayout.HorizontalScope(Styles.HeaderBackground, GUILayout.ExpandHeight(false)))
        {
            GUILayout.Label("Name", GUILayout.Width(EditorGUIUtility.labelWidth));
            GUILayout.Label("Value");
        }
    }

    private void DoList()
    {
        using (var scrollView = new EditorGUILayout.ScrollViewScope(m_ScrollPosition))
        {
            m_ScrollPosition = scrollView.scrollPosition;

            EditorGUI.BeginChangeCheck();
            string valueName = null;
            object value = null;

            foreach (var kvp in m_EditorPrefsLookup)
            {
                valueName = kvp.Key;
                value = kvp.Value;

                if (IsFiltering && !valueName.ToLower().Contains(m_Filter.ToLower()))
                    continue;

                // Strings are encoded as utf8 bytes
                var bytes = value as byte[];
                if (bytes != null)
                {
                    string valueAsString = Encoding.UTF8.GetString(bytes);
                    EditorGUI.BeginChangeCheck();
                    string newString = EditorGUILayout.DelayedTextField(StripValueNameHash(valueName), valueAsString);
                    if (EditorGUI.EndChangeCheck())
                    {
                        value = Encoding.UTF8.GetBytes(newString);
                        break;
                    }
                }
                else if (value is int)
                {
                    int valueAsInt = (int)value;
                    EditorGUI.BeginChangeCheck();
                    int newInt = EditorGUILayout.DelayedIntField(StripValueNameHash(valueName), valueAsInt);
                    if (EditorGUI.EndChangeCheck())
                    {
                        value = newInt;
                        break;
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(StripValueNameHash(valueName), string.Format("Unhandled Type {0}", value.GetType()));
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                SetKeyValue(valueName, value);
            }
        }
    }

    private void SetKeyValue(string valueName, object newValue)
    {
        if (valueName == null)
            throw new ArgumentNullException("valueName");

        if (newValue == null)
            throw new ArgumentNullException("newValue");

        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(kUnityRootSubKey, true))
        {
            if (key == null)
                throw new KeyNotFoundException(string.Format("Failed to open sub key {0}.", kUnityRootSubKey));

            // Unity caches values, so it doesn't dip into the registry for every EditorPrefs.Get* call.
            // This means we need to tell Unity to delete this value to remove it from the cache and force Unity to look into registry for value.
            EditorPrefs.DeleteKey(StripValueNameHash(valueName));

            key.SetValue(valueName, newValue);
            m_EditorPrefsLookup[valueName] = key.GetValue(valueName);
        }
    }

    private void DoToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            // Refresh Button
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
                FetchKeyValues();

            GUILayout.FlexibleSpace();

            // Filter Field
            m_Filter = EditorGUILayout.TextField(m_Filter, Styles.ToolbarSearchField, GUILayout.Width(250f));
            if (GUILayout.Button(GUIContent.none, IsFiltering ? Styles.ToolbarSearchFieldCancel : Styles.ToolbarSearchFieldCancelEmpty))
            {
                m_Filter = "";
                GUIUtility.keyboardControl = 0;
            }
        }
    }

    private void FetchKeyValues()
    {
        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(kUnityRootSubKey, false))
        {
            if (key == null)
                throw new KeyNotFoundException(string.Format("Failed to open sub key {0}.", kUnityRootSubKey));

            m_EditorPrefsLookup.Clear();

            foreach (string keyValueName in key.GetValueNames())
            {
                var value = key.GetValue(keyValueName);
                m_EditorPrefsLookup.Add(keyValueName, value);
            }
        }
    }

    private string StripValueNameHash(string keyValueName)
    {
        return keyValueName.Split(new[] { "_h" }, StringSplitOptions.None).First();
    }

    public void AddItemsToMenu(GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Delete All"), false, EditorPrefs.DeleteAll);
    }
}

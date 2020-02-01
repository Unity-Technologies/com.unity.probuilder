#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
#if UNITY_EDITOR
    class SelectionManager : ScriptableSingleton<SelectionManager>, ISerializationCallbackReceiver
#else
    class SelectionManager : ISerializationCallbackReceiver
#endif
    {
        // todo implement elementSelectionChanged event

#if !UNITY_EDITOR
        // todo Proper runtime support
        static SelectionManager s_Instance;

        public SelectionManager instance
        {
            get
            {
                if(s_Instance == null)
                    s_Instance = new SelectionManager();
                return s_Instance;
            }
        }
#endif

        [SerializeField]
        ProBuilderMesh[] m_SelectionKeys;

        [SerializeField]
        ElementCollection[] m_SelectionValues;

        Dictionary<ProBuilderMesh, ElementCollection> m_Selection = new Dictionary<ProBuilderMesh, ElementCollection>();

        internal Dictionary<ProBuilderMesh, ElementCollection> selection
        {
            get { return m_Selection; }
        }

        public void OnBeforeSerialize()
        {
            m_SelectionKeys = m_Selection.Keys.ToArray();
            m_SelectionValues = m_Selection.Values.ToArray();
        }

        public void OnAfterDeserialize()
        {
            if (m_SelectionKeys == null || m_SelectionValues == null)
                return;

            for (int i = 0, c = System.Math.Min(m_SelectionKeys.Length, m_SelectionValues.Length); i < c; i++)
                m_Selection.Add(m_SelectionKeys[i], m_SelectionValues[i]);
        }

        public ElementCollection GetOrCreateSelection(ProBuilderMesh mesh)
        {
            ElementCollection collection;
            if (!m_Selection.TryGetValue(mesh, out collection))
            {
                Debug.Log($"CreateSelection({mesh.id})");
                m_Selection.Add(mesh, collection = new ElementCollection(mesh));
            }

            return collection;
        }

        public void SetSelection(ProBuilderMesh mesh, ElementCollection collection)
        {
            if (m_Selection.ContainsKey(mesh))
                m_Selection[mesh] = collection;
            else
                m_Selection.Add(mesh, collection);
        }

        public void Remove(ProBuilderMesh mesh)
        {
            m_Selection.Remove(mesh);
        }

        public void Clear()
        {
            selection.Clear();
        }
    }
}
#endif

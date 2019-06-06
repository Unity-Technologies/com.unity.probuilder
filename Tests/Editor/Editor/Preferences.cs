using System.Linq;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEditor.ProBuilder;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;


namespace UnityEngine.ProBuilder.EditorTests
{
    public class PreferencesTest
    {
        bool m_OpenedWindow = false;
        bool m_ShowIconsInitialValue = false;

        [SetUp]
        public void Setup()
        {
            // make sure the ProBuilder window is open
            if (ProBuilderEditor.instance == null)
            {
                ProBuilderEditor.MenuOpenWindow();
                m_OpenedWindow = true;
            }

            m_ShowIconsInitialValue = ProBuilderEditor.s_IsIconGui.value;
        }

        [TearDown]
        public void Cleanup()
        {
            ProBuilderEditor.s_IsIconGui.value = m_ShowIconsInitialValue;

            // close editor window if we had to open it
            if (m_OpenedWindow && ProBuilderEditor.instance != null)
            {
                ProBuilderEditor.instance.Close();
            }
        }

        [UnityTest]
        public IEnumerator Preferences_IconWindowChanged()
        {
            Assert.That(ProBuilderEditor.s_EditorToolbar, Is.Not.Null);

            Assert.That(ProBuilderEditor.s_EditorToolbar, Is.Not.Null);
            Assert.That(ProBuilderEditor.s_EditorToolbar.isIconMode, Is.EqualTo(m_ShowIconsInitialValue));

            ProBuilderEditor.s_IsIconGui.value = !m_ShowIconsInitialValue;
            ProBuilderEditor.instance.Repaint();
            ProBuilderEditor.Refresh();
            yield return null;
            Assert.That(ProBuilderEditor.s_EditorToolbar.isIconMode, Is.EqualTo(!m_ShowIconsInitialValue));
        }
    }
}

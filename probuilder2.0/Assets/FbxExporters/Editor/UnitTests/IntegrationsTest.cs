using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;

namespace FbxExporters.UnitTests
{
    public class IntegrationsTest {
        string m_oldMayaLocation;

        [SetUp]
        public void ClearEnv() {
            m_oldMayaLocation = System.Environment.GetEnvironmentVariable ("MAYA_LOCATION");
            System.Environment.SetEnvironmentVariable("MAYA_LOCATION", null);
        }

        [TearDown]
        public void ResetEnv() {
            System.Environment.SetEnvironmentVariable("MAYA_LOCATION", m_oldMayaLocation);
        }

        void LogNonEmptyString(string name, string str) {
            Debug.Log(name + ": " + str);
            Assert.IsFalse(string.IsNullOrEmpty(str));
        }

        [Test]
        public void BasicTest() {
            // Note: This test assumes that Maya is actually installed in a default location.
            Assert.IsTrue(Directory.Exists(Editor.Integrations.MayaVersion.AdskRoot));

            var maya = new Editor.Integrations.MayaVersion();

            LogNonEmptyString("location", maya.Location);
            LogNonEmptyString("binary  ", maya.MayaExe);
            LogNonEmptyString("version ", maya.Version);

            Assert.IsFalse(Editor.Integrations.IsHeadlessInstall());

            LogNonEmptyString("module path (2017)", Editor.Integrations.GetModulePath("2017"));
            LogNonEmptyString("module template path (2017)", Editor.Integrations.GetModuleTemplatePath("2017"));
            Assert.That( () => Editor.Integrations.GetModuleTemplatePath("bad version"),
                    Throws.Exception.TypeOf<Editor.Integrations.MayaException>());

            LogNonEmptyString("app path", Editor.Integrations.GetAppPath());
            LogNonEmptyString("project path", Editor.Integrations.GetProjectPath());
            LogNonEmptyString("package path", Editor.Integrations.GetPackagePath());
            LogNonEmptyString("package version", Editor.Integrations.GetPackageVersion());
            LogNonEmptyString("temp path", Editor.Integrations.GetTempSavePath());
        }
    }
}

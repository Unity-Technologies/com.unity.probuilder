using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using FbxExporters.EditorTools;
using NUnit.Framework;

namespace FbxExporters.UnitTests
{
    public class FbxExportSettingsTest {
        ExportSettings m_originalSettings;

        // We read two private fields for the test.
        static System.Reflection.FieldInfo s_InstanceField; // static
        static System.Reflection.FieldInfo s_SavePathField; // member

        static FbxExportSettingsTest() {
            // all names, private or public, instance or static
            var privates = System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.Instance;
            var t = typeof(ExportSettings);

            s_SavePathField = t.GetField("convertToModelSavePath", privates);
            Assert.IsNotNull(s_SavePathField, "convertToModelSavePath");

            // static fields can't be found through inheritance with GetField.
            // if we change the inheritance diagram, we have to change t.BaseType here.
            s_InstanceField = t.BaseType.GetField("s_Instance", privates);
            Assert.IsNotNull(s_InstanceField, "s_Instance");
        }

        [NUnit.Framework.SetUp]
        public void SetUp()
        {
            var settings = (ExportSettings)s_InstanceField.GetValue(null);
            m_originalSettings = settings;

            // Clear out the current instance and create a new one (but keep the original around).
            s_InstanceField.SetValue(null, null);
            s_InstanceField.SetValue(null, ScriptableObject.CreateInstance<ExportSettings>());
        }

        [NUnit.Framework.TearDown]
        public void TearDown()
        {
            // Destroy the test settings and restore the original.
            // The original might be null -- not a problem.
            var settings = (ExportSettings)s_InstanceField.GetValue(null);
            ScriptableObject.DestroyImmediate(settings);

            s_InstanceField.SetValue(null, m_originalSettings);
        }


        [Test]
        public void TestNormalizePath()
        {
            // Test slashes in both directions, and leading and trailing slashes.
            var path = "/a\\b/c/\\";
            var norm = ExportSettings.NormalizePath(path, isRelative: true);
            Assert.AreEqual("a/b/c", norm);
            norm = ExportSettings.NormalizePath(path, isRelative: false);
            Assert.AreEqual("/a/b/c", norm);

            // Test empty path. Not actually absolute, so it's treated as a relative path.
            path = "";
            norm = ExportSettings.NormalizePath(path, isRelative: true);
            Assert.AreEqual(".", norm);
            norm = ExportSettings.NormalizePath(path, isRelative: false);
            Assert.AreEqual(".", norm);

            // Test just a bunch of slashes. Root or . depending on whether it's abs or rel.
            path = "///";
            norm = ExportSettings.NormalizePath(path, isRelative: true);
            Assert.AreEqual(".", norm);
            norm = ExportSettings.NormalizePath(path, isRelative: false);
            Assert.AreEqual("/", norm);

            // Test handling of .
            path = "/a/./b/././c/.";
            norm = ExportSettings.NormalizePath(path, isRelative: true);
            Assert.AreEqual("a/b/c", norm);

            // Test handling of leading ..
            path = "..";
            norm = ExportSettings.NormalizePath(path, isRelative: true);
            Assert.AreEqual("..", norm);

            path = "../a";
            norm = ExportSettings.NormalizePath(path, isRelative: true);
            Assert.AreEqual("../a", norm);

            // Test two leading ..
            path = "../../a";
            norm = ExportSettings.NormalizePath(path, isRelative: true);
            Assert.AreEqual("../../a", norm);

            // Test .. in the middle and effect on leading /
            path = "/a/../b";
            norm = ExportSettings.NormalizePath(path, isRelative: true);
            Assert.AreEqual("b", norm);
            norm = ExportSettings.NormalizePath(path, isRelative: false);
            Assert.AreEqual("/b", norm);

            // Test that we can change the separator
            norm = ExportSettings.NormalizePath(path, isRelative: false, separator: '\\');
            Assert.AreEqual("\\b", norm);
        }

        [Test]
        public void TestGetRelativePath()
        {
            var from = "//a/b/c";
            var to = "///a/b/c/d/e";
            var relative = ExportSettings.GetRelativePath(from, to);
            Assert.AreEqual("d/e", relative);

            from = "///a/b/c/";
            to = "///a/b/c/d/e/";
            relative = ExportSettings.GetRelativePath(from, to);
            Assert.AreEqual("d/e", relative);

            from = "///aa/bb/cc/dd/ee";
            to = "///aa/bb/cc";
            relative = ExportSettings.GetRelativePath(from, to);
            Assert.AreEqual("../..", relative);

            from = "///a/b/c/d/e/";
            to = "///a/b/c/";
            relative = ExportSettings.GetRelativePath(from, to);
            Assert.AreEqual("../..", relative);

            from = "///a/b/c/d/e/";
            to = "///a/b/c/";
            relative = ExportSettings.GetRelativePath(from, to, separator: ':');
            Assert.AreEqual("..:..", relative);

            from = Path.Combine(Application.dataPath, "foo");
            to = Application.dataPath;
            relative = ExportSettings.GetRelativePath(from, to);
            Assert.AreEqual("..", relative);

            to = Path.Combine(Application.dataPath, "foo");
            relative = ExportSettings.ConvertToAssetRelativePath(to);
            Assert.AreEqual("foo", relative);

            relative = ExportSettings.ConvertToAssetRelativePath("/path/to/somewhere/else");
            Assert.AreEqual("", relative);

            relative = ExportSettings.ConvertToAssetRelativePath("/path/to/somewhere/else", requireSubdirectory: false);
            Assert.IsTrue(relative.StartsWith("../"));
            Assert.IsFalse(relative.Contains("\\"));
        }

        [Test]
        public void TestGetSetFields()
        {
            var defaultRelativePath = ExportSettings.GetRelativeSavePath();
            Assert.AreEqual(ExportSettings.kDefaultSavePath, defaultRelativePath);

            // the path to Assets but with platform-dependent separators
            var appDataPath = Application.dataPath.Replace(Path.AltDirectorySeparatorChar,
                    Path.DirectorySeparatorChar);

            var defaultAbsolutePath = ExportSettings.GetAbsoluteSavePath();
            var dataPath = Path.Combine(appDataPath, ExportSettings.kDefaultSavePath);
            Assert.AreEqual(dataPath, defaultAbsolutePath);

            // set; check that the saved value is platform-independent,
            // that the relative path uses / like in unity,
            // and that the absolute path is platform-specific
            ExportSettings.SetRelativeSavePath("/a\\b/c/\\");
            var convertToModelSavePath = s_SavePathField.GetValue(ExportSettings.instance);
            Assert.AreEqual("a/b/c", convertToModelSavePath);
            Assert.AreEqual("a/b/c", ExportSettings.GetRelativeSavePath());
            var platformPath = Path.Combine("a", Path.Combine("b", "c"));
            Assert.AreEqual(Path.Combine(appDataPath, platformPath), ExportSettings.GetAbsoluteSavePath());
        }
    }
}

#if UNITY_2019_2_OR_NEWER
using System.Reflection;
#else
using System.IO;
#endif
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    static class Version
    {
#if !UNITY_2019_2_OR_NEWER
        struct PackageInfo
        {
#pragma warning disable 649
            public string version;
#pragma warning restore 649
        }
#endif

        internal static bool TryGetPackageVersion(out SemVer version)
        {
            version = new SemVer();

#if UNITY_2019_2_OR_NEWER
            var assembly = Assembly.GetExecutingAssembly();
            var info = PackageManager.PackageInfo.FindForAssembly(assembly);
            return SemVer.TryGetVersionInfo(info.version, out version);
#else
            try
            {
                var packageInfo = FileUtility.GetProBuilderInstallDirectory() + "/package.json";
                var contents = File.ReadAllText(packageInfo);
                var info = JsonUtility.FromJson<PackageInfo>(contents);
                return SemVer.TryGetVersionInfo(info.version, out version);
            }
            catch
            {
                return false;
            }
#endif
        }
    }
}

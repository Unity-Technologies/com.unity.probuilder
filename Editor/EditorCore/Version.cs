using System.Reflection;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    static class Version
    {
        internal static bool TryGetPackageVersion(out SemVer version)
        {
            version = new SemVer();

            var assembly = Assembly.GetExecutingAssembly();
            var info = PackageManager.PackageInfo.FindForAssembly(assembly);
            return SemVer.TryGetVersionInfo(info.version, out version);
        }
    }
}

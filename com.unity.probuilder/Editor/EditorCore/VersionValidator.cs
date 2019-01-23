using UnityEngine.ProBuilder.AssetIdRemapUtility;
using UnityEngine.ProBuilder;
using Version = UnityEngine.ProBuilder.Version;

namespace UnityEditor.ProBuilder
{
    [InitializeOnLoad]
    class VersionValidator
    {
        static readonly SemVer k_ProBuilder4_0_0 = new SemVer(4, 0, 0);

        const string k_UpgradeDialog = "ProBuilder 2.x and 3.x assets are incompatible with 4.0.0+ and need to be upgraded.* Would you like to convert your project to the new version of ProBuilder?\n\n*Future updates will not require this conversion.";
        const string k_UpgradeLaterText = "\n\nIf you choose \"No\" this dialog may be accessed again at any time through the \"Tools/ProBuilder/Repair/Convert to ProBuilder 4\" menu item.";
        const string k_AssetStoreUpgradeTitle = "Old ProBuilder Install Found";
        const string k_DeprecatedGuidsTitle = "Broken ProBuilder References Found in Project";

        static Pref<SemVer> s_StoredVersionInfo = new Pref<SemVer>("about.identifier", new SemVer(), SettingsScope.Project);

        static VersionValidator()
        {
            EditorApplication.delayCall += ValidateVersion;
        }

        [MenuItem("Tools/ProBuilder/Repair/Check for Broken ProBuilder References")]
        internal static void OpenConversionEditor()
        {
            CheckForUpgradeableAssets(true, true);
        }

        static void ValidateVersion()
        {
            var currentVersion = Version.currentInfo;
            var oldVersion = (SemVer) s_StoredVersionInfo;
            bool isNewVersion = currentVersion != oldVersion;

            if (isNewVersion)
            {
                PreferencesUpdater.CheckEditorPrefsVersion();
                s_StoredVersionInfo.SetValue(currentVersion, true);

                if (oldVersion < k_ProBuilder4_0_0)
                    CheckForUpgradeableAssets(true);
            }
        }

        static void CheckForUpgradeableAssets(bool checkForDeprecatedGuids, bool calledFromMenu = false)
        {
            bool pre4PackageFound = PackageImporter.IsPreProBuilder4InProject();
            bool deprecatedGuidsFound = !pre4PackageFound && checkForDeprecatedGuids && PackageImporter.DoesProjectContainDeprecatedGUIDs();

            if (pre4PackageFound || deprecatedGuidsFound)
            {
                if (UnityEditor.EditorUtility.DisplayDialog(
                    pre4PackageFound ? k_AssetStoreUpgradeTitle : k_DeprecatedGuidsTitle,
                    k_UpgradeDialog + k_UpgradeLaterText,
                    "Yes", "No"))
                    EditorApplication.delayCall += AssetIdRemapEditor.OpenConversionEditor;
            }
            else if(calledFromMenu)
            {
                UnityEditor.EditorUtility.DisplayDialog(
                    "Project is up to date",
                    "No missing or broken references found.",
                    "Okay"
                );
            }
        }
    }
}

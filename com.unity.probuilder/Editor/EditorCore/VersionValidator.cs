using UnityEngine.ProBuilder.AssetIdRemapUtility;
using UnityEngine.ProBuilder;
using Version = UnityEngine.ProBuilder.Version;
using UnityEditor;

namespace UnityEditor.ProBuilder
{
    [InitializeOnLoad]
    class VersionValidator
    {
        static readonly SemVer k_ProBuilder4_0_0 = new SemVer(4, 0, 0);
        static readonly SemVer k_EmptyVersion = new SemVer(0, 0, 0);

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

                // When upgrading, skip the expensive scan for old GUIDs in scene and prefab files. It is only necessary
                // in the case where a user has deleted the old version prior to updating (which should be an edge case
                // with Package Manager). The full check is still available through a menu item.
                if (oldVersion < k_ProBuilder4_0_0)
                    CheckForUpgradeableAssets(false);
            }
        }

        static void CheckForUpgradeableAssets(bool checkForDeprecatedGuids, bool calledFromMenu = false)
        {
            bool pre4PackageFound = PackageImporter.IsPreProBuilder4InProject();
            bool deprecatedGuidsFound = checkForDeprecatedGuids && PackageImporter.DoesProjectContainDeprecatedGUIDs();

            const string k_AssetStoreUpgradeTitle = "Old ProBuilder Install Found";
            const string k_UpgradeDialog = "ProBuilder 2.x and 3.x assets are incompatible with 4.0.0+ and need to be upgraded.* Would you like to convert your project to the new version of ProBuilder?\n\n*Future updates will not require this conversion.";
            const string k_DeprecatedGuidsTitle = "Broken ProBuilder References Found in Project";
            const string k_DeprecatedGuidsDialog = "ProBuilder has found some mesh components that are missing references. To keep these models editable by ProBuilder, they need to be repaired. Would you like to perform the repair action now?";

            if (pre4PackageFound || deprecatedGuidsFound)
            {
                if (UnityEditor.EditorUtility.DisplayDialog(
                    pre4PackageFound ? k_AssetStoreUpgradeTitle : k_DeprecatedGuidsTitle,
                    pre4PackageFound
                        ? k_UpgradeDialog
                        : k_DeprecatedGuidsDialog +
                          "\n\nIf you choose \"No\" this dialog may be accessed again at any time through the \"Tools/ProBuilder/Repair/Convert to ProBuilder 4\" menu item.",
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

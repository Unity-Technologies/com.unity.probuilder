using UnityEngine.ProBuilder.AssetIdRemapUtility;
using UnityEngine.ProBuilder;
using Version = UnityEngine.ProBuilder.Version;

namespace UnityEditor.ProBuilder
{
    [InitializeOnLoad]
    class VersionValidator
    {
        static Pref<SemVer> s_StoredVersionInfo = new Pref<SemVer>("about.identifier", new SemVer(), SettingsScope.Project);
        
        static VersionValidator()
        {
            EditorApplication.delayCall += ValidateVersion;
        }

        static void ValidateVersion()
        {
            var currentVersion = Version.currentInfo;
            var oldVersion = (SemVer)s_StoredVersionInfo;

            bool isNewVersion = currentVersion != oldVersion;

            if (isNewVersion)
            {
                PreferencesUpdater.CheckEditorPrefsVersion();
                s_StoredVersionInfo.SetValue(currentVersion, true);
            }

            bool assetStoreInstallFound = isNewVersion && PackageImporter.IsPreProBuilder4InProject();
            bool deprecatedGuidsFound = isNewVersion && PackageImporter.DoesProjectContainDeprecatedGUIDs();

            const string k_AssetStoreUpgradeTitle = "Old ProBuilder Install Found in Assets";
            const string k_AssetStoreUpgradeDialog = "The Asset Store version of ProBuilder is incompatible with Package Manager. Would you like to convert your project to the Package Manager version of ProBuilder?";
            const string k_DeprecatedGuidsTitle = "Broken ProBuilder References Found in Project";
            const string k_DeprecatedGuidsDialog = "ProBuilder has found some mesh components that are missing references. To keep these models editable by ProBuilder, they need to be repaired. Would you like to perform the repair action now?";

            if (isNewVersion && (assetStoreInstallFound || deprecatedGuidsFound))
                if (UnityEditor.EditorUtility.DisplayDialog(assetStoreInstallFound ? k_AssetStoreUpgradeTitle : k_DeprecatedGuidsTitle,
                        assetStoreInstallFound ? k_AssetStoreUpgradeDialog : k_DeprecatedGuidsDialog +
                        "\n\nIf you choose \"No\" this dialog may be accessed again at any time through the \"Tools/ProBuilder/Repair/Convert to ProBuilder 4\" menu item.",
                        "Yes", "No"))
                    EditorApplication.delayCall += AssetIdRemapEditor.OpenConversionEditor;
        }
    }
}

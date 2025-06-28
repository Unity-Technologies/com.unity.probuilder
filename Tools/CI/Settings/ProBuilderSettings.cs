using RecipeEngine.Api.Settings;
using RecipeEngine.Modules.Wrench.Models;
using RecipeEngine.Modules.Wrench.Settings;

namespace ProBuilder.Cookbook.Settings;

public class ProBuilderSettings : AnnotatedSettingsBase
{
    // Path from the root of the repository where packages are located.
    readonly string[] PackagesRootPaths = {"."};

    // Environment variables
    public static readonly string ProBuilderPackageName = "com.unity.probuilder";
    readonly string _excludeAssembliesCodeCovCommand = "generateAdditionalMetrics;generateHtmlReport;assemblyFilters:ASSEMBLY_NAME,-*Tests*,-*Examples*,-*Debug*;pathFilters:-**External/**;pathReplacePatterns:@*,,**/PackageCache/,;sourcePaths:YAMATO_SOURCE_DIR/Packages;";

    // update this to list all packages in this repo that you want to release.
    Dictionary<string, PackageOptions> PackageOptions = new()
    {
        {
            ProBuilderPackageName,
            new PackageOptions() { ReleaseOptions = new ReleaseOptions() { IsReleasing = true } }
        }
    };

    public ProBuilderSettings()
    {
        Wrench = new WrenchSettings(
            PackagesRootPaths,
            PackageOptions
        );

        Wrench.Packages[ProBuilderPackageName].CoverageCommands.Enabled = true;
        Wrench.Packages[ProBuilderPackageName].CoverageCommands.Commands = [_excludeAssembliesCodeCovCommand];
    }

    public WrenchSettings Wrench { get; private set; }
}

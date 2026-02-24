using RecipeEngine.Api.Platforms;
using RecipeEngine.Api.Settings;
using RecipeEngine.Modules.Wrench.Models;
using RecipeEngine.Modules.Wrench.Settings;
using RecipeEngine.Platforms;

namespace ProBuilder.Cookbook.Settings;

public class ProBuilderSettings : AnnotatedSettingsBase
{
    static ProBuilderSettings? _instance;

    // Path from the root of the repository where packages are located.
    readonly string[] PackagesRootPaths = {"."};

    // Environment variables
    public static readonly string ProBuilderPackageName = "com.unity.probuilder";
    readonly string _excludeAssembliesCodeCovCommand = "generateAdditionalMetrics;generateHtmlReport;assemblyFilters:ASSEMBLY_NAME,-*Tests*,-*Examples*,-*Debug*;pathFilters:-**External/**;pathReplacePatterns:@*,,**/PackageCache/,;sourcePaths:YAMATO_SOURCE_DIR/Packages;";

    public static ProBuilderSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ProBuilderSettings();
            }
            return _instance;
        }
    }

    // update this to list all packages in this repo that you want to release.
    Dictionary<string, PackageOptions> PackageOptions = new()
    {
        {
            ProBuilderPackageName,
            new PackageOptions()
            {
                ReleaseOptions = new ReleaseOptions() { IsReleasing = true },
                ValidationOptions = new ValidationOptions()
                {
                    AdditionalUtrArguments = ["--fail-on-assert"]
                }
            }
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

        var defaultMacPlatform = WrenchPackage.DefaultEditorPlatforms[SystemType.MacOS];
        Wrench.Packages["com.unity.probuilder"].EditorPlatforms[SystemType.MacOS] = new Platform(new Agent("package-ci/macos-13-arm64:v4", FlavorType.MacDefault, defaultMacPlatform.Agent.Resource, "M1"), defaultMacPlatform.System);
    }

    public WrenchSettings Wrench { get; private set; }
}

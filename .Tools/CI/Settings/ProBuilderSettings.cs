using RecipeEngine.Api.Platforms;
using RecipeEngine.Api.Settings;
using RecipeEngine.Modules.Wrench.Helpers;
using RecipeEngine.Modules.Wrench.Models;
using RecipeEngine.Modules.Wrench.Platforms;
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
                MaximumEditorVersion =  "6000.5",
                ValidationOptions = new ValidationOptions()
                {
                    AdditionalUtrArguments = ["--fail-on-assert --coverage-pkg-version=1.3.0"]
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
    }

    public WrenchSettings Wrench { get; private set; }
}

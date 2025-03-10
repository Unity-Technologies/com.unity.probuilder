using RecipeEngine.Api.Settings;
using RecipeEngine.Modules.Wrench.Models;
using RecipeEngine.Modules.Wrench.Settings;

namespace ProBuilder.Cookbook.Settings;

public class ProBuilderSettings : AnnotatedSettingsBase
{
    // Path from the root of the repository where packages are located.
    readonly string[] PackagesRootPaths = {"."};

    // update this to list all packages in this repo that you want to release.
    Dictionary<string, PackageOptions> PackageOptions = new()
    {
        {
            "com.unity.probuilder",
            new PackageOptions() { ReleaseOptions = new ReleaseOptions() { IsReleasing = true } }
        }
    };

    public ProBuilderSettings()
    {
        Wrench = new WrenchSettings(
            PackagesRootPaths,
            PackageOptions
        );      
    }

    public WrenchSettings Wrench { get; private set; }
}

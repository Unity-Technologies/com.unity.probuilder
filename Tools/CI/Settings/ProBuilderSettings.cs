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

    // You can either use a platform.json file or specify custom yamato VM images for each package in code.
    private readonly Dictionary<SystemType, Platform> ImageOverrides = new()
    {
        {
            SystemType.Ubuntu,
            new Platform(new Agent("package-ci/ubuntu-18.04:v4", FlavorType.BuildLarge, ResourceType.Vm),
                SystemType.Ubuntu)
        }
    };
    
    public ProBuilderSettings()
    {
        Wrench = new WrenchSettings(
            PackagesRootPaths,
            PackageOptions
        );      
        
        // change default images as per Dictionary above.
        Wrench.Packages["com.unity.probuilder"].EditorPlatforms = ImageOverrides;
    }

    public WrenchSettings Wrench { get; private set; }
}

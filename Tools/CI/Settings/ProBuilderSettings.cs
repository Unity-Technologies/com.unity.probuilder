using RecipeEngine.Api.Settings;
using RecipeEngine.Modules.Wrench.Models;
using RecipeEngine.Modules.Wrench.Settings;

namespace ProBuilder.Cookbook.Settings;

public class ProBuilderSettings : AnnotatedSettingsBase
{
    // Path from the root of the repository where packages are located.
    readonly string[] PackagesRootPaths = {"."};

    static ProBuilderSettings _instance;

    // Environment variables
    internal static readonly string ProBuilderPackageName = "com.unity.probuilder";
    internal readonly string EditorVersion = "trunk";

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
    }

    public WrenchSettings Wrench { get; private set; }

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
}

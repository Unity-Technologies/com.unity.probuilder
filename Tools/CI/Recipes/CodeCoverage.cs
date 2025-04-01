using ProBuilder.Cookbook.Settings;
using RecipeEngine.Api.Dependencies;
using RecipeEngine.Api.Extensions;
using RecipeEngine.Api.Jobs;
using RecipeEngine.Api.Platforms;
using RecipeEngine.Api.Recipes;
using RecipeEngine.Modules.UnifiedTestRunner;
using RecipeEngine.Modules.UpmCi;
using RecipeEngine.Modules.UpmPvp;
using RecipeEngine.Modules.Wrench.Models;
using RecipeEngine.Platforms;
using Semver;

namespace ProBuilder.Cookbook.Recipes;

public class CodeCoverage : RecipeBase
{
    protected override ISet<Job> LoadJobs()
        => Combine.Collections(GetJobs()).SelectJobs();

    public IEnumerable<Dependency> AsDependencies()
        => this.Jobs.ToDependencies(this);

    private string GetJobName(string editorVersion, SystemType systemType)
        => $"Code Coverage - {editorVersion}  - {systemType}";

    public List<IJobBuilder> GetJobs()
    {
        List<IJobBuilder> builders = new();
        ProBuilderSettings settings = ProBuilderSettings.Instance;

        foreach (var package in settings.Wrench.PackagesToRelease)
        {
            var platform = settings.Wrench.Packages[ProBuilderSettings.ProBuilderPackageName].EditorPlatforms[SystemType.Ubuntu];
            var packageAssemblyName = settings.Wrench.Packages[package].Name.Replace("com.", string.Empty);
            var sourceDir = platform.System.FormatEnvironmentVariable("YAMATO_SOURCE_DIR");

            var job = JobBuilder.Create(GetJobName(settings.EditorVersion, platform.System))
                .WithDescription($"Generate code coverage data for {settings.Wrench.Packages[package].DisplayName} on {platform.System}")
                .WithPlatform(platform)
                .WithCommands("npm install upm-ci-utils@stable -g --registry https://artifactory.prd.cds.internal.unity3d.com/artifactory/api/npm/upm-npm")
                .WithCommands($"upm-ci package test -u trunk --package-path {package} --type package-tests --enable-code-coverage --code-coverage-options \"generateAdditionalMetrics;generateHtmlReport;assemblyFilters:+{packageAssemblyName}*,-*Tests*,-*Examples*;pathFilters:-**Tests**;\" --extra-utr-arg=--coverage-results-path=${sourceDir}/upm-ci~/test-results/CoverageResults")
                .WithUpmCiArtifacts()
                .WithDependencies(settings.Wrench.WrenchJobs[package][JobTypes.Pack]);

            builders.Add(job);
        }

        return builders;
    }
}

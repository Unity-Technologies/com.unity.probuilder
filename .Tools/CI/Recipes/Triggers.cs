using ProBuilder.Cookbook.Settings;
using RecipeEngine.Api.Dependencies;
using RecipeEngine.Api.Extensions;
using RecipeEngine.Api.Jobs;
using RecipeEngine.Api.Recipes;
using RecipeEngine.Api.Triggers;
using RecipeEngine.Modules.Wrench.Models;

namespace ProBuilder.Cookbook.Recipes;

public class Triggers : RecipeBase
{
    ProBuilderSettings Settings = ProBuilderSettings.Instance;

    protected override ISet<Job> LoadJobs()
        => Combine.Collections(GetTriggers()).SelectJobs();

    private ISet<IJobBuilder> GetTriggers()
    {
        var allValidationJobs = Settings.Wrench.WrenchJobs[ProBuilderSettings.ProBuilderPackageName][JobTypes.Validation];

        HashSet<IJobBuilder> builders =
        [
            JobBuilder.Create("Pull Request Trigger")
                .WithDependencies(allValidationJobs.Where(d => d.JobId.Contains("macos")))
                .WithPullRequestTrigger(pr => pr.ExcludeDraft().And().WithTargetBranch("master"),
                    true, cancelLeftoverJobs: CancelLeftoverJobs.Always)
        ];
        return builders;
    }
}

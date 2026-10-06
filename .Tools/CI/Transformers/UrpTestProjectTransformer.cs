using RecipeEngine.Api.Commands;
using RecipeEngine.Api.Jobs;

namespace ProBuilder.Cookbook.Transformers;

public class UrpTestProjectTransformer : IJobTransformer
{
    const string k_ValidationJobPrefix = "Validate - probuilder - ";
    const string k_GeneratedProject = "test-probuilder";
    const string k_UrpProject = "TestProjects~/URP";
    const int k_FirstUrpOnlyMajorVersion = 7000;

    public bool AppliesTo(Job job)
        => job.Name.StartsWith(k_ValidationJobPrefix)
           && int.TryParse(job.Name.Substring(k_ValidationJobPrefix.Length).Split('.')[0], out var major)
           && major >= k_FirstUrpOnlyMajorVersion;

    public IJobBuilder Transform(IJobBuilder builder)
    {
        var job = builder.Build();
        return JobBuilder.Create(job with { Commands = job.Commands.Select(UseUrpProject).ToArray() });
    }

    static Command UseUrpProject(Command command)
    {
        if (command.Line == null)
            return command;

        var line = command.Line
            .Replace($"create-test-project {k_GeneratedProject} ", $"create-test-project {k_UrpProject} ")
            .Replace($"--testproject={k_GeneratedProject} ", $"--testproject={k_UrpProject} ");

        if (line == command.Line)
            return command;

        return command is RetryCommand retry
            ? new RetryCommand(line, retry.RetryCount, retry.TimeoutInMinutes, retry.Name, retry.Labels)
            : new Command(command.Name, line, command.Labels);
    }
}

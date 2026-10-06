using ProBuilder.Cookbook.Settings;
using ProBuilder.Cookbook.Transformers;
using RecipeEngine;
using RecipeEngine.Modules.Wrench.Helpers;


// ReSharper disable once CheckNamespace
public static class Program
{
    public static int Main(string[] args)
    {
        var settings = new ProBuilderSettings();

        // ReSharper disable once UnusedVariable
        var engine = EngineFactory
            .Create()
            .ScanAll()
            .WithWrenchModule(settings.Wrench)
            .WithJobTransformer<UrpTestProjectTransformer>()
            .GenerateAsync().Result;
        return engine;
    }
}

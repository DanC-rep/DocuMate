using DocumentationGenerator;
using DocumentationGenerator.CodeParsing.Analyzers;
using DocumentationGenerator.Documentation;
using DocumentationGenerator.DocumentationTemplates;
using DocumentationGenerator.Interfaces;
using Domain.Constants;
using Domain.Options;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ConsoleApp;

public static class DependencyInjection
{
    public static IServiceCollection Inject(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IConfiguration>(configuration);
        
        services.AddLogging(configuration);
        services.AddScoped<IProjectAnalyzer, DotnetProjectAnalyzer>();
        services.AddScoped<IDocumentationGenerator, LlamaDocsGenerator>();
        services.AddScoped<IPromptGenerator, PromptGenerator>();
        services.AddScoped<IDocumentationTemplate, DotnetDocumentationTemplate>();
        services.AddScoped<IFilesProcessor, FilesProcessor>();

        services.AddInfrastructure(configuration);

        return services;
    }

    private static IServiceCollection AddLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Seq(configuration.GetConnectionString(ConnectionStrings.Seq)
                         ?? throw new ArgumentException(ConnectionStrings.Seq))
            .MinimumLevel.Information()
            .CreateLogger();

        services.AddLogging(builder => builder.AddSerilog(Log.Logger));

        return services;
    }
}
using ConsoleApp;
using DocumentationGenerator;
using DocumentationGenerator.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var provider = new ServiceCollection()
    .Inject(config)
    .BuildServiceProvider();

Console.Write("Enter your project path: ");
var path = Console.ReadLine();

while (string.IsNullOrWhiteSpace(path))
    path = Console.ReadLine();

var documentationGenerator = new Processor(
    provider.GetRequiredService<IProjectAnalyzer>(),
    provider.GetRequiredService<IDocumentationGenerator>());

var result = await documentationGenerator.Process(path);

if (result.IsFailure)
    Console.WriteLine($"В процессе генерации документации произошла ошибка: {result.Error.Message}");
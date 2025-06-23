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

var documentationGenerator = new Generator(provider.GetRequiredService<IProjectAnalyzer>());

var result = await documentationGenerator.GenerateDocumentation(path);

if (result.IsFailure)
{
    Console.WriteLine($"В процессе генерации документации произошла ошибка: {result.Error.Message}");
}
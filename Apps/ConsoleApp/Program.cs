using DocumentationGenerator;
using DocumentationGenerator.CodeParsing;
using DocumentationGenerator.CodeParsing.Analyzers;

Console.Write("Enter your project path: ");
var path = Console.ReadLine();

while (string.IsNullOrWhiteSpace(path))
    path = Console.ReadLine();

var dotnetAnalyzer = new DotnetProjectAnalyzer();
var parser = new ProjectAnalyzer(dotnetAnalyzer);
var documentationGenerator = new Generator();

var projectInfoResult = parser.AnalyzeProject(path);

if (projectInfoResult.IsFailure)
{
    Console.WriteLine($"{projectInfoResult.Error.Code} {projectInfoResult.Error.Message}");
    return;
}

var documentationResult = documentationGenerator.GenerateDocumentation(projectInfoResult.Value);
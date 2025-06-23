using CSharpFunctionalExtensions;
using DocumentationGenerator.Interfaces;
using Domain.Errors;

namespace DocumentationGenerator;

public class Generator
{
    private readonly IProjectAnalyzer _projectAnalyzer;
    
    public Generator(IProjectAnalyzer projectAnalyzer)
    {
        _projectAnalyzer = projectAnalyzer;
    }
    
    public async Task<Result<string, Error>> GenerateDocumentation(string projectPath)
    {
        var analyzeResult = _projectAnalyzer.AnalyzeProject(projectPath);

        return string.Empty;

        // Весь функционал по генерации в другой класс

        // if (analyzeResult.IsFailure)
        //     return analyzeResult.Error;
        //
        // var uri = new Uri("http://localhost:11434"); // load from appsettings.json
        //
        // var ollama = new OllamaApiClient(uri)
        // {
        //     SelectedModel = "llama3"
        // };
        //
        // string code = File.ReadAllText("E:\\Projects\\DocuMate\\Core\\DocumentationGenerator\\ExpClass.cs");
        //
        // var prompt = "Сгенерируй документацию для этого файла в формате md: " + code;
        // List<string> response = new List<string>();
        //
        // await foreach (var stream in ollama.GenerateAsync(prompt))
        //     response.Add(stream.Response);
        //
        // return string.Join("", response);

        // Вызов инфраструктуры и схранение информации о файлах в бд
    }
}
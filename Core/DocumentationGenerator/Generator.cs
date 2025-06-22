using Domain;
using OllamaSharp;

namespace DocumentationGenerator;

public class Generator
{
    public async Task<string> GenerateDocumentation(ProjectAnalysisResult projectInfo)
    {
        var uri = new Uri("http://localhost:11434"); // load from appsettings.json

        var ollama = new OllamaApiClient(uri)
        {
            SelectedModel = "llama3"
        };

        string code = File.ReadAllText("E:\\Projects\\DocuMate\\Core\\DocumentationGenerator\\ExpClass.cs");

        var prompt = "Сгенерируй документацию для этого файла в формате md: " + code;
        List<string> response = new List<string>();

        await foreach (var stream in ollama.GenerateAsync(prompt))
            response.Add(stream.Response);
        
        return string.Join("", response);
        
        // Вызов инфраструктуры и схранение информации о файлах в бд
    }
}
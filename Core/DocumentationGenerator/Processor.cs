using CSharpFunctionalExtensions;
using DocumentationGenerator.Interfaces;
using Domain.Errors;

namespace DocumentationGenerator;

public class Processor
{
    private readonly IProjectAnalyzer _projectAnalyzer;
    private readonly IDocumentationGenerator _documentationGenerator;
    
    public Processor(
        IProjectAnalyzer projectAnalyzer,
        IDocumentationGenerator documentationGenerator)
    {
        _projectAnalyzer = projectAnalyzer;
        _documentationGenerator = documentationGenerator;
    }
    
    public async Task<Result<string, Error>> Process(string projectPath)
    {
        var analyzeResult = _projectAnalyzer.AnalyzeProject(projectPath);

        if (analyzeResult.IsFailure)
            return analyzeResult.Error;

        var projectDocumentation = await _documentationGenerator.GenerateDocumentation(analyzeResult.Value);

        return string.Empty;
    }
}
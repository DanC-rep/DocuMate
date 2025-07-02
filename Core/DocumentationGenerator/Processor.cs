using CSharpFunctionalExtensions;
using DocumentationGenerator.Interfaces;
using Domain.Errors;

namespace DocumentationGenerator;

public class Processor
{
    private readonly IProjectAnalyzer _projectAnalyzer;
    private readonly IDocumentationGenerator _documentationGenerator;
    private readonly IFilesProcessor _filesProcessor;
    
    public Processor(
        IProjectAnalyzer projectAnalyzer,
        IDocumentationGenerator documentationGenerator,
        IFilesProcessor filesProcessor)
    {
        _projectAnalyzer = projectAnalyzer;
        _documentationGenerator = documentationGenerator;
        _filesProcessor = filesProcessor;
    }
    
    public async Task<UnitResult<Error>> Process(string projectPath, CancellationToken cancellationToken = default!)
    {
        var analyzeResult = _projectAnalyzer.AnalyzeProject(projectPath);

        if (analyzeResult.IsFailure)
            return analyzeResult.Error;

        var projectDocumentation = await _documentationGenerator.GenerateDocumentation(
            analyzeResult.Value,
            cancellationToken);
        
        if (projectDocumentation.IsFailure)
            return projectDocumentation.Error;
        
        var uploadResult = await _filesProcessor.UploadFiles(projectDocumentation.Value, Path.GetDirectoryName(projectPath)!, cancellationToken);

        if (uploadResult.IsFailure)
            return uploadResult.Error;

        return Result.Success<Error>();
    }
}
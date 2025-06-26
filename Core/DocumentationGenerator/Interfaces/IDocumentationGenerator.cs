using CSharpFunctionalExtensions;
using Domain;
using Domain.Errors;

namespace DocumentationGenerator.Interfaces;

public interface IDocumentationGenerator
{
    Task<Result<List<string>, Error>> GenerateDocumentation(ProjectAnalysisResult projectInfo);
}
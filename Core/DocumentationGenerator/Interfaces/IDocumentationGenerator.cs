using CSharpFunctionalExtensions;
using Domain;
using Domain.Errors;

namespace DocumentationGenerator.Interfaces;

public interface IDocumentationGenerator
{
    Task<Result<Dictionary<string, string>, Error>> GenerateDocumentation(
        ProjectAnalysisResult projectInfo,
        CancellationToken cancellation = default!);
}
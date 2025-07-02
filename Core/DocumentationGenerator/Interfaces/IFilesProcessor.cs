using CSharpFunctionalExtensions;
using Domain.Errors;

namespace DocumentationGenerator.Interfaces;

public interface IFilesProcessor
{
    Task<UnitResult<Error>> UploadFiles(
        Dictionary<string, string> filesDocumentation,
        string projectName,
        CancellationToken cancellationToken = default);
}
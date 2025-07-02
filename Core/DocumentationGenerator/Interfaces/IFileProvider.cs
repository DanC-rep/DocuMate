using CSharpFunctionalExtensions;
using Domain;
using Domain.Errors;

namespace DocumentationGenerator.Interfaces;

public interface IFileProvider
{
    Task<Result<string, Error>> UploadFile(
        UploadFileData uploadFileData,
        CancellationToken cancellationToken = default!);
    
    Task<UnitResult<Error>> RemoveBucketIfExists(
        string bucketName,
        List<string> filesToDelete,
        CancellationToken cancellationToken = default!);
}
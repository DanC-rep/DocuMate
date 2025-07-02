using CSharpFunctionalExtensions;
using Domain;
using Domain.Errors;

namespace DocumentationGenerator.Interfaces;

public interface IFilesRepository
{
    Task<UnitResult<Error>> AddRange(IEnumerable<FileData> filesData, CancellationToken cancellationToken = default!);
    
    Task<UnitResult<Error>> DeleteByBucket(string bucketName, CancellationToken cancellationToken = default!);
    
    Task<Result<List<string>, Error>> GetFileNamesByBucket(string bucketName, CancellationToken cancellationToken = default!);
}
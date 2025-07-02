using CSharpFunctionalExtensions;
using DocumentationGenerator.Interfaces;
using Domain;
using Domain.Errors;
using Microsoft.Extensions.Logging;
using Minio.Credentials;
using MongoDB.Driver;

namespace Infrastructure.MongoDataAccess;

public class FilesRepository : IFilesRepository
{
    private readonly FileMongoDbContext _dbContext;
    private readonly ILogger<FilesRepository> _logger;

    public FilesRepository(
        FileMongoDbContext dbContext,
        ILogger<FilesRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<UnitResult<Error>> AddRange(IEnumerable<FileData> filesData, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Files.InsertManyAsync(filesData, cancellationToken: cancellationToken);

            return Result.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fail to upload files to mongodb");

            return Error.Failure("files.upload.mongo", "Fail to upload files to mongodb");
        }
    }

    public async Task<UnitResult<Error>> DeleteByBucket(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Files.DeleteManyAsync(f => f.BucketName == bucketName, cancellationToken);
            
            return Result.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fail to delete files from mongodb");
            
            return Error.Failure("delete.files.mongo", "Fail to delete files from mongodb");
        }
    }

    public async Task<Result<List<string>, Error>> GetFileNamesByBucket(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            var files = await _dbContext.Files.Find(f => f.BucketName == bucketName).ToListAsync(cancellationToken);

            return files.Select(f => f.FilePath).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fail to get files from mongodb");
            
            return Error.Failure("get.files.mongo", "Fail to get files from mongodb");
        }
    }
}
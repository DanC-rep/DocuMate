using CSharpFunctionalExtensions;
using DocumentationGenerator.Interfaces;
using Domain;
using Domain.Errors;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace Infrastructure;

public class MinioProvider : IFileProvider
{
    private readonly IMinioClient _minioClient;
    private readonly ILogger<MinioProvider> _logger;

    public MinioProvider(
        IMinioClient minioClient,
        ILogger<MinioProvider> logger)
    {
        _minioClient = minioClient;
        _logger = logger;
    }
    
    public async Task<Result<string, Error>> UploadFile(
        UploadFileData uploadFileData, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bucketExists = await IsBucketExists(uploadFileData.BucketName, cancellationToken);

            if (!bucketExists)
            {
                await CreateBucket(uploadFileData.BucketName, cancellationToken);
            }

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(uploadFileData.BucketName)
                .WithStreamData(uploadFileData.Stream)
                .WithObjectSize(uploadFileData.Stream.Length)
                .WithObject(uploadFileData.ObjectName);

            var result = await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

            return result.ObjectName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fail to upload file {fileName} in minio", uploadFileData.ObjectName);

            return Error.Failure("file.upload.minio", "Fail to upload file in minio");
        }
    }

    public async Task<UnitResult<Error>> RemoveBucketIfExists(
        string bucketName, 
        List<string> filesToDelete,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bucketExists = await IsBucketExists(bucketName, cancellationToken);

            if (!bucketExists)
                return Result.Success<Error>();

            if (filesToDelete.Count > 0)
                await RemoveBucketObjects(bucketName, filesToDelete, cancellationToken);
            
            var removeBucketArgs = new RemoveBucketArgs()
                .WithBucket(bucketName);
            
            await _minioClient.RemoveBucketAsync(removeBucketArgs, cancellationToken);

            return Result.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fail to remove bucket {bucketName} in minio", bucketName);

            return Error.Failure("remove.bucket", "Fail to remove bucket in minio");
        }
    }

    private async Task<bool> IsBucketExists(string bucketName, CancellationToken cancellationToken = default)
    {
        var bucketExistsArgs = new BucketExistsArgs()
            .WithBucket(bucketName);
        
        return await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);
    }

    private async Task CreateBucket(string bucketName, CancellationToken cancellationToken = default)
    {
        var makeBucketArgs = new MakeBucketArgs()
            .WithBucket(bucketName);
        
        await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
    }

    private async Task RemoveBucketObjects(string bucketName, List<string> filesToDelete, CancellationToken cancellation = default)
    {
        var removeObjectsArgs = new RemoveObjectsArgs()
            .WithObjects(filesToDelete)
            .WithBucket(bucketName);
        
        await _minioClient.RemoveObjectsAsync(removeObjectsArgs, cancellation);
    }
}
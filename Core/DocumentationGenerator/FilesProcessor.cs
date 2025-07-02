using CSharpFunctionalExtensions;
using DocumentationGenerator.Interfaces;
using Domain;
using Domain.Errors;

namespace DocumentationGenerator;

public class FilesProcessor : IFilesProcessor
{
    private readonly IFileProvider _fileProvider;
    private readonly IFilesRepository _filesRepository;

    public FilesProcessor(
        IFileProvider fileProvider,
        IFilesRepository filesRepository)
    {
        _fileProvider = fileProvider;
        _filesRepository = filesRepository;
    }
    
    public async Task<UnitResult<Error>> UploadFiles(
        Dictionary<string, string> filesDocumentation, 
        string projectName,
        CancellationToken cancellationToken = default)
    {
        var bucketName = Path.GetFileName(projectName)!;

        var filesToDelete = await _filesRepository.GetFileNamesByBucket(bucketName, cancellationToken);

        if (filesToDelete.IsFailure)
            return filesToDelete.Error;
        
        var removeBucketResult = await _fileProvider.RemoveBucketIfExists(bucketName, filesToDelete.Value, cancellationToken);

        if (removeBucketResult.IsFailure)
            return removeBucketResult.Error;

        var deleteFilesResult = await _filesRepository.DeleteByBucket(bucketName, cancellationToken);

        if (deleteFilesResult.IsFailure)
            return deleteFilesResult.Error;

        foreach (var file in filesDocumentation)
        {
            var fileName = Path.GetFileName(file.Key).Replace(".cs", ".md");
            
            using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);
            
            var marker = "# File Overview";
            var documentation = file.Value.Contains(marker) 
                ? file.Value.Substring(file.Value.IndexOf(marker, StringComparison.Ordinal)) : file.Value;
            
            await writer.WriteAsync(documentation);
            await writer.FlushAsync(cancellationToken);
            stream.Position = 0;
            
            var uploadData = new UploadFileData(stream, bucketName, fileName);
            
            var uploadMinioResult = await _fileProvider.UploadFile(uploadData, cancellationToken);
            
            if (uploadMinioResult.IsFailure)
                return uploadMinioResult.Error;

            var fileData = new FileData
            {
                Id = Guid.NewGuid(),
                FileSize = stream.Length,
                ContentType = "text/markdown",
                FilePath = fileName,
                BucketName = bucketName,
                UploadDate = DateTime.UtcNow
            };

            var uploadMongoResult = await _filesRepository.AddRange([fileData], cancellationToken);
            
            if (uploadMongoResult.IsFailure)
                return uploadMongoResult.Error;
        }

        return Result.Success<Error>();
    }
}
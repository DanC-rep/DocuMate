namespace Domain;

public record UploadFileData(Stream Stream, string BucketName, string ObjectName);
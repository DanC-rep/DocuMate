using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain;

public class FileData
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; init; }
    
    public required DateTime UploadDate { get; init; }
    
    public required long FileSize { get; init; }
    
    public required string ContentType { get; init; }
    
    public required string FilePath { get; init; }
    
    public required string BucketName { get; init; }
}
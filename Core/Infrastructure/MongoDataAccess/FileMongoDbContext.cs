using Domain;
using MongoDB.Driver;

namespace Infrastructure.MongoDataAccess;

public class FileMongoDbContext (IMongoClient mongoClient)
{
    private readonly IMongoDatabase _database = mongoClient.GetDatabase("file_service");
    
    public IMongoCollection<FileData> Files => _database.GetCollection<FileData>("files");
}
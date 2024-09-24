using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;

namespace Backend.Api.Utils.Mongo;

[ExcludeFromCodeCoverage]

public class MongoDbClientFactory : IMongoDbClientFactory
{
    private readonly IMongoDatabase _mongoDatabase;
   private readonly MongoClient _client;

    public MongoDbClientFactory(string? connectionString, string databaseName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("MongoDB connection string cannot be empty");

        var settings = MongoClientSettings.FromConnectionString(connectionString);
        _client = new MongoClient(settings);

        var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
        // convention must be registered before initialising collection
        ConventionRegistry.Register("CamelCase", camelCaseConvention, _ => true);

        _mongoDatabase = _client.GetDatabase(databaseName);
    }

    public IMongoClient CreateClient()
    {

        return _client;
    }

    public IMongoCollection<T> GetCollection<T>(string collection)
    {
        return _mongoDatabase.GetCollection<T>(collection);
    }

    public IMongoClient GetClient()
    {
        return _client;
    }

}

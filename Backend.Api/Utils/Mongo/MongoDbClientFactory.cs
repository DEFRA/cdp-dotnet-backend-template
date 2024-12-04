using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;
using Backend.Api.Config;
using Microsoft.Extensions.Options;

namespace Backend.Api.Utils.Mongo;

[ExcludeFromCodeCoverage]
public class MongoDbClientFactory : IMongoDbClientFactory
{
    private readonly IMongoDatabase _mongoDatabase;
    private readonly MongoClient _client;

    public MongoDbClientFactory(IOptions<MongoConfig> config)
   {
        var uri = config.Value.DatabaseUri;
        var databaseName = config.Value.DatabaseName;
        
        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException("MongoDB uri string cannot be empty");

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new ArgumentException("MongoDB database name cannot be empty");
        
        var settings = MongoClientSettings.FromConnectionString(uri);
        _client = new MongoClient(settings);

        var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
        // convention must be registered before initialising collection
        ConventionRegistry.Register("CamelCase", camelCaseConvention, _ => true);

        _mongoDatabase = _client.GetDatabase(databaseName);
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
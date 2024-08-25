﻿using MongoDB.Driver;

namespace Backend.Api.Utils.Mongo;

public abstract class MongoService<T>
{
    protected readonly IMongoClient Client;
    protected readonly IMongoCollection<T> Collection;

    protected readonly ILogger _logger;

    protected MongoService(IMongoDbClientFactory connectionFactory, string collectionName, ILoggerFactory loggerFactory)
    {
        Client = connectionFactory.GetClient();
        Collection = connectionFactory.GetCollection<T>(collectionName);
        var loggerName = GetType().FullName ?? GetType().Name;
        _logger = loggerFactory.CreateLogger(loggerName);
        EnsureIndexes();
    }

    protected abstract List<CreateIndexModel<T>> DefineIndexes(IndexKeysDefinitionBuilder<T> builder);

    private void EnsureIndexes()
    {
        var builder = Builders<T>.IndexKeys;
        var indexes = DefineIndexes(builder);
        if (indexes.Count == 0) return;

        _logger.LogInformation(
            "Ensuring index is created if it does not exist for collection {CollectionNamespaceCollectionName} in DB {DatabaseDatabaseNamespace}",
            Collection.CollectionNamespace.CollectionName,
            Collection.Database.DatabaseNamespace);
        Collection.Indexes.CreateMany(indexes);
    }
}
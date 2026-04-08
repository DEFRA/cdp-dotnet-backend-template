using Backend.Api.Example.Models;
using Backend.Api.Utils.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Backend.Api.Utils.Auditing;

namespace Backend.Api.Example.Services;

public interface IExamplePersistence
{
    Task CreateAsync(ExampleModel example, CancellationToken cancellationToken = default);

    Task<ExampleModel?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ExampleModel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ExampleModel>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    Task<ExampleModel?> UpdateAsync(ExampleModel example, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string name, CancellationToken cancellationToken = default);
}


[ExcludeFromCodeCoverage]
public class ExamplePersistence(IMongoDbClientFactory connectionFactory, ILoggerFactory loggerFactory)
    : MongoService<ExampleModel>(connectionFactory, "example", loggerFactory), IExamplePersistence
{
    public async Task CreateAsync(ExampleModel example, CancellationToken cancellationToken = default)
    {
        try
        {
            await Collection.InsertOneAsync(example, cancellationToken: cancellationToken);
            Logger.Audit("Created new record: {Name}", example.Name);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new ExampleConflictException(example.Name, ex);
        }
    }

    [ExcludeFromCodeCoverage]
    public async Task<ExampleModel?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var result = await Collection.Find(b => b.Name == name).FirstOrDefaultAsync(cancellationToken);
        Logger.LogInformation("Searching for {Name}, found {Result}", name, result);
        return result;
    }

    [ExcludeFromCodeCoverage]
    public async Task<IReadOnlyCollection<ExampleModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Collection.Find(_ => true).ToListAsync(cancellationToken);
    }


    [ExcludeFromCodeCoverage]
    public async Task<IReadOnlyCollection<ExampleModel>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var escapedSearchTerm = Regex.Escape(searchTerm.Trim());
        var filter = Builders<ExampleModel>.Filter.Regex(
            model => model.Value,
            new BsonRegularExpression(escapedSearchTerm, "i"));
        var result = await Collection.Find(filter).ToListAsync(cancellationToken);
        return result;
    }


    [ExcludeFromCodeCoverage]
    public async Task<ExampleModel?> UpdateAsync(ExampleModel example, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ExampleModel>.Filter.Eq(e => e.Name, example.Name);
        var update = Builders<ExampleModel>.Update
            .Set(e => e.Value, example.Value)
            .Set(e => e.Counter, example.Counter);

        return await Collection.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<ExampleModel>
            {
                ReturnDocument = ReturnDocument.After
            },
            cancellationToken);
    }

    [ExcludeFromCodeCoverage]
    public async Task<bool> DeleteAsync(string name, CancellationToken cancellationToken = default)
    {
        var result = await Collection.DeleteOneAsync(e => e.Name == name, cancellationToken);
        return result.DeletedCount > 0;
    }

    [ExcludeFromCodeCoverage]
    protected override List<CreateIndexModel<ExampleModel>> DefineIndexes(
        IndexKeysDefinitionBuilder<ExampleModel> builder)
    {
        var options = new CreateIndexOptions { Unique = true };
        var nameIndex = new CreateIndexModel<ExampleModel>(builder.Ascending(e => e.Name), options);
        return [nameIndex];
    }
}
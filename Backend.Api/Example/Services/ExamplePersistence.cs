using Backend.Api.Example.Models;
using Backend.Api.Utils.Mongo;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;

namespace Backend.Api.Example.Services;

public interface IExamplePersistence
{
    public Task<bool> CreateAsync(ExampleModel example);

    public Task<ExampleModel?> GetByExampleName(string name);

    public Task<IEnumerable<ExampleModel>> GetAllAsync();

    public Task<IEnumerable<ExampleModel>> SearchByValueAsync(string searchTerm);

    public Task<bool> UpdateAsync(ExampleModel example);

    public Task<bool> DeleteAsync(string name);
}

/**
 * An example of how to persist data in MongoDB.
 * The base class `MongoService` provides access to the db collection as well as providing helpers to
 * ensure the indexes for this collection are created on startup.
 */
public class ExamplePersistence(IMongoDbClientFactory connectionFactory, ILoggerFactory loggerFactory)
    : MongoService<ExampleModel>(connectionFactory, "example", loggerFactory), IExamplePersistence
{
    public async Task<bool> CreateAsync(ExampleModel example)
    {
        try
        {
            await Collection.InsertOneAsync(example);
            return true;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to insert {example}", example);
            return false;
        }
    }

    [ExcludeFromCodeCoverage]
    public async Task<ExampleModel?> GetByExampleName(string name)
    {
        var result = await Collection.Find(b => b.Name == name).FirstOrDefaultAsync();
        Logger.LogInformation("Searching for {Name}, found {Result}", name, result);
        return result;
    }

    [ExcludeFromCodeCoverage]
    public async Task<IEnumerable<ExampleModel>> GetAllAsync()
    {
        return await Collection.Find(_ => true).ToListAsync();
    }


    [ExcludeFromCodeCoverage]
    public async Task<IEnumerable<ExampleModel>> SearchByValueAsync(string searchTerm)
    {
        var searchOptions = new TextSearchOptions { CaseSensitive = false, DiacriticSensitive = false };
        var filter = Builders<ExampleModel>.Filter.Text(searchTerm, searchOptions);
        var result = await Collection.Find(filter).ToListAsync();
        return result;
    }

    /**
     * Updates the value field for a given name and increments the counter.
     * Rather than replacing the whole record we selectively $set and $inc fields while leaving others
     * unchanged.
     */
    [ExcludeFromCodeCoverage]
    public async Task<bool> UpdateAsync(ExampleModel example)
    {
        var filter = Builders<ExampleModel>.Filter.Eq(e => e.Name, example.Name);
        var update = Builders<ExampleModel>.Update
            .Inc(e => e.Counter, 1)
            .Set(e => e.Value, example.Value);

        var result = await Collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    [ExcludeFromCodeCoverage]
    public async Task<bool> DeleteAsync(string name)
    {
        var result = await Collection.DeleteOneAsync(e => e.Name == name);
        return result.DeletedCount > 0;
    }

    /**
     * Ensure indexes are created for this collection.
     * In this example it creates a single index on the `name` field. The Unique flag is set preventing duplicate names
     * being inserted.
     */
    [ExcludeFromCodeCoverage]
    protected override List<CreateIndexModel<ExampleModel>> DefineIndexes(
        IndexKeysDefinitionBuilder<ExampleModel> builder)
    {
        var options = new CreateIndexOptions { Unique = true };
        var nameIndex = new CreateIndexModel<ExampleModel>(builder.Ascending(e => e.Name), options);
        return [nameIndex];
    }
}
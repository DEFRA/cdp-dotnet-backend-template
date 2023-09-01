using Backend.Api.Data;
using Backend.Api.Models;
using Backend.Api.Services.DBO;
using MongoDB.Driver;

namespace Backend.Api.Services;

public class BookService : MongoService<MongoBook>, IBookService
{

    public BookService(IMongoDbClientFactory connectionFactory, ILoggerFactory loggerFactory) : base(connectionFactory, "books", loggerFactory)
    {
    }

    public async Task<bool> CreateAsync(Book book)
    {
        var existingBook = await GetByIsbnAsync(book.Isbn);
        if (existingBook is not null) return false;

        await Collection.InsertOneAsync(new MongoBook(book));
        return true;
    }

    public async Task<Book?> GetByIsbnAsync(string isbn)
    {
        var result = await Collection.Find(b => b.Isbn == isbn).ToListAsync();
        return result?.FirstOrDefault()?.ToBook();
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        var result = await Collection.Find(_ => true).ToListAsync();
        return result.Select(mongoBook => mongoBook.ToBook());
    }

    public async Task<IEnumerable<Book>> SearchByTitleAsync(string searchTerm)
    {
        var searchOptions = new TextSearchOptions { CaseSensitive = false, DiacriticSensitive = false };
        var filter = Builders<MongoBook>.Filter.Text(searchTerm, searchOptions);
        var result = await Collection.Find(filter).ToListAsync();
        return result.Select(mongoBook => mongoBook.ToBook());
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        var result = await Collection.ReplaceOneAsync(b => b.Isbn == book.Isbn, new MongoBook(book));
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string isbn)
    {
        var result = await Collection.DeleteOneAsync(b => b.Isbn == isbn);
        return result.DeletedCount > 0;
    }

    protected override List<CreateIndexModel<MongoBook>> DefineIndexes(IndexKeysDefinitionBuilder<MongoBook> builder)
    {
        var options = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<MongoBook>(builder.Ascending(b => b.Isbn), options);
        var titleModel = new CreateIndexModel<MongoBook>(builder.Text(b => b.Title));
        return new List<CreateIndexModel<MongoBook>>() { indexModel, titleModel };
    }
}
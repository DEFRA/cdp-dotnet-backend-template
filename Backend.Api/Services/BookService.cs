using Backend.Api.Data;
using Backend.Api.Models;
using MongoDB.Driver;

namespace Backend.Api.Services;

public class BookService : MongoService<Book>, IBookService
{
    public BookService(IMongoDbClientFactory connectionFactory, ILoggerFactory loggerFactory) : base(connectionFactory,
        "books", loggerFactory)
    {
    }

    public async Task<bool> CreateAsync(Book book)
    {
        var existingBook = await GetByIsbnAsync(book.Isbn);
        if (existingBook is not null) return false;

        await Collection.InsertOneAsync(book);
        return true;
    }

    public async Task<Book?> GetByIsbnAsync(string isbn)
    {
        var result = await Collection.Find(b => b.Isbn == isbn).ToListAsync();
        return result?.FirstOrDefault();
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        var result = await Collection.Find(_ => true).ToListAsync();
        return result.Select(Book => Book);
    }

    public async Task<IEnumerable<Book>> SearchByTitleAsync(string searchTerm)
    {
        var searchOptions = new TextSearchOptions { CaseSensitive = false, DiacriticSensitive = false };
        var filter = Builders<Book>.Filter.Text(searchTerm, searchOptions);
        var result = await Collection.Find(filter).ToListAsync();
        return result;
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        var result = await Collection.ReplaceOneAsync(b => b.Isbn == book.Isbn, book);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string isbn)
    {
        var result = await Collection.DeleteOneAsync(b => b.Isbn == isbn);
        return result.DeletedCount > 0;
    }

    protected override List<CreateIndexModel<Book>> DefineIndexes(IndexKeysDefinitionBuilder<Book> builder)
    {
        var options = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<Book>(builder.Ascending(b => b.Isbn), options);
        var titleModel = new CreateIndexModel<Book>(builder.Text(b => b.Title));
        return new List<CreateIndexModel<Book>> { indexModel, titleModel };
    }
}
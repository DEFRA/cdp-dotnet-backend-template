using Backend.Api.Data;
using Backend.Api.Models;
using Backend.Api.Services.DBO;
using MongoDB.Driver;

namespace Backend.Api.Services;

public class BookService : IBookService
{
    private readonly IMongoCollection<MongoBook> _booksCollection;

    public BookService(IMongoDbClientFactory connectionFactory)
    {
        _booksCollection = connectionFactory.GetCollection<MongoBook>("books");
        Task.Run(() => EnsureIndexes()).Wait(); // Figure out a better way of doing this 
    }

    public async Task<bool> CreateAsync(Book book)
    {
        var existingBook = await GetByIsbnAsync(book.Isbn);
        if (existingBook is not null) return false;

        await _booksCollection.InsertOneAsync(new MongoBook(book));
        return true;
    }

    public async Task<Book?> GetByIsbnAsync(string isbn)
    {
        var result = await _booksCollection.Find(b => b.Isbn == isbn).ToListAsync();
        return result?.FirstOrDefault()?.ToBook();
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        var result = await _booksCollection.Find(_ => true).ToListAsync();
        return result.Select(mongoBook => mongoBook.ToBook());
    }

    public async Task<IEnumerable<Book>> SearchByTitleAsync(string searchTerm)
    {
        var searchOptions = new TextSearchOptions { CaseSensitive = false, DiacriticSensitive = false };
        var filter = Builders<MongoBook>.Filter.Text(searchTerm, searchOptions);
        var result = await _booksCollection.Find(filter).ToListAsync();
        return result.Select(mongoBook => mongoBook.ToBook());
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        var result = await _booksCollection.ReplaceOneAsync(b => b.Isbn == book.Isbn, new MongoBook(book));
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string isbn)
    {
        var result = await _booksCollection.DeleteOneAsync(b => b.Isbn == isbn);
        return result.DeletedCount > 0;
    }

    private IEnumerable<string?> EnsureIndexes()
    {
        var options = new CreateIndexOptions { Unique = true };
        var bookBuilder = Builders<MongoBook>.IndexKeys;
        var indexModel = new CreateIndexModel<MongoBook>(bookBuilder.Ascending(b => b.Isbn), options);
        var titleModel = new CreateIndexModel<MongoBook>(bookBuilder.Text(b => b.Title));
        var result = _booksCollection.Indexes.CreateMany(new[] { indexModel, titleModel });
        return result;
    }
}
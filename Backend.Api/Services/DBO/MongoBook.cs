using Backend.Api.Models;
using MongoDB.Bson;

namespace Backend.Api.Services.DBO;

public class MongoBook
{
    public MongoBook(Book book)
    {
        Isbn = book.Isbn;
        Title = book.Title;
        Author = book.Author;
        ShortDescription = book.ShortDescription;
        PageCount = book.PageCount;
        ReleaseDate = book.ReleaseDate;
    }

    public ObjectId Id { get; init; } = default!;
    public string Isbn { get; init; } = default!;
    public string Title { get; init; } = default!;
    public string Author { get; init; } = default!;
    public string ShortDescription { get; init; } = default!;
    public int PageCount { get; init; }
    public DateTime ReleaseDate { get; init; }

    public Book ToBook()
    {
        return new Book
        {
            Isbn = Isbn,
            Title = Title,
            Author = Author,
            ShortDescription = ShortDescription,
            PageCount = PageCount,
            ReleaseDate = ReleaseDate
        };
    }
}
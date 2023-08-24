using Backend.Api.Models;
using Backend.Api.Services;
using FluentValidation;
using FluentValidation.Results;

namespace Backend.Api.Endpoints;

public static class LibraryEndpoints
{
    private const string ContentType = "application/json";
    private const string Tag = "Books";
    private const string BaseRoute = "books";

    public static void UseLibraryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(BaseRoute, CreateBookAsync)
            .WithName("CreateBook")
            .Accepts<Book>(ContentType)
            .Produces<Book>(201).Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags(Tag);

        app.MapGet(BaseRoute, GetAllBooksAsync)
            .WithName("GetBooks")
            .Produces<IEnumerable<Book>>()
            .WithTags(Tag);

        app.MapGet($"{BaseRoute}/{{isbn}}", GetBookByIsbnAsync)
            .WithName("GetBook")
            .Produces<Book>().Produces(404)
            .WithTags(Tag);

        app.MapPut($"{BaseRoute}/{{isbn}}", UpdateBookAsync)
            .WithName("UpdateBook")
            .Accepts<Book>(ContentType)
            .Produces<Book>().Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags(Tag);

        app.MapDelete($"{BaseRoute}/{{isbn}}", DeleteBookAsync)
            .WithName("DeleteBook")
            .Produces(204).Produces(404)
            .WithTags(Tag);
    }

    private static async Task<IResult> CreateBookAsync(
        Book book, IBookService bookService, IValidator<Book> validator)
    {
        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid) return Results.BadRequest(validationResult.Errors);

        var created = await bookService.CreateAsync(book);
        if (!created)
            return Results.BadRequest(new List<ValidationFailure>
            {
                new("Isbn", "A book with this ISBN-13 already exists")
            });

        return Results.Created($"/{BaseRoute}/{book.Isbn}", book);
    }

    private static async Task<IResult> GetAllBooksAsync(
        IBookService bookService, string? searchTerm)
    {
        if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
        {
            var matchedBooks = await bookService.SearchByTitleAsync(searchTerm);
            return Results.Ok(matchedBooks);
        }

        var books = await bookService.GetAllAsync();
        return Results.Ok(books);
    }

    private static async Task<IResult> GetBookByIsbnAsync(
        string isbn, IBookService bookService)
    {
        var book = await bookService.GetByIsbnAsync(isbn);
        return book is not null ? Results.Ok(book) : Results.NotFound();
    }

    private static async Task<IResult> UpdateBookAsync(
        string isbn, Book book, IBookService bookService,
        IValidator<Book> validator)
    {
        book.Isbn = isbn;
        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid) return Results.BadRequest(validationResult.Errors);

        var updated = await bookService.UpdateAsync(book);
        return updated ? Results.Ok(book) : Results.NotFound();
    }

    private static async Task<IResult> DeleteBookAsync(
        string isbn, IBookService bookService)
    {
        var deleted = await bookService.DeleteAsync(isbn);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
}
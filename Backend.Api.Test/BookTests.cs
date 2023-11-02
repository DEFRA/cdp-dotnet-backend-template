using Backend.Api.Models;
using Backend.Api.Validators;
using MongoDB.Bson;

namespace Backend.Api.Test;

using FluentValidation.TestHelper;

public class BookTests
{
    private BookValidator _validator = new BookValidator();
    
    [Fact]
    public void BookValidateIsbn()
    {
        var book = new Book()
        {
            Author = "Some Author",
            Id = new ObjectId(),
            Isbn = "invalid",
            PageCount = 200,
            ReleaseDate = DateTime.Now,
            ShortDescription = "This is the best book ever",
            Title = "Best Book Ever"
        };
        var result = _validator.TestValidate(book);
        result.ShouldHaveValidationErrorFor(b => b.Isbn);
    }
}
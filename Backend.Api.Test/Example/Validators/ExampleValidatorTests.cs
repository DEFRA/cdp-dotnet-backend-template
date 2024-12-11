using Backend.Api.Example.Models;
using Backend.Api.Example.Validators;
using FluentValidation.TestHelper;
using MongoDB.Bson;

namespace Backend.Api.Test.Example.Validators;

public class ExampleValidatorTests
{
    private readonly ExampleValidator _validator = new();

    [Fact]
    public void ValidModel()
    {
        var model = new ExampleModel
        {
            Id = new ObjectId(),
            Value = "some value",
            Name = "Test",
            Counter = 0
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void InvalidName()
    {
        var model = new ExampleModel
        {
            Id = new ObjectId(),
            Value = "Some value",
            Name = "Test $FOO someName" // letters/numbers/spaces only
        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(b => b.Name);
    }

    [Fact]
    public void InvalidCounter()
    {
        var model = new ExampleModel
        {
            Id = new ObjectId(),
            Value = "Some value",
            Name = "Test",
            Counter = -1

        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(b => b.Counter);
    }

    [Fact]
    public void EmptyValue()
    {
        var model = new ExampleModel
        {
            Id = new ObjectId(),
            Value = "",
            Name = "Test",
            Counter = 0

        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(b => b.Value);
    }

}

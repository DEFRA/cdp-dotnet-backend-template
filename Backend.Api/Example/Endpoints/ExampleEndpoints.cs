using Backend.Api.Example.Models;
using Backend.Api.Example.Services;
using FluentValidation;
using FluentValidation.Results;
using System.Diagnostics.CodeAnalysis;

namespace Backend.Api.Example.Endpoints;

[ExcludeFromCodeCoverage]
public static class ExampleEndpoints
{
    public static void UseExampleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("example", Create);

        app.MapGet("example", GetAll);

        app.MapGet("example/{name}", GetByName);

        app.MapPut("example/{name}", Update);

        app.MapDelete("example/{name}", Delete);
    }

    private static async Task<IResult> Create(
        ExampleModel example, IExamplePersistence examplePersistence, IValidator<ExampleModel> validator)
    {
        var validationResult = await validator.ValidateAsync(example);
        if (!validationResult.IsValid) return Results.BadRequest(validationResult.Errors);

        var created = await examplePersistence.CreateAsync(example);
        if (!created)
            return Results.BadRequest(new List<ValidationFailure>
            {
                new("Example", "An example record with this name already exists")
            });

        return Results.Created($"/example/{example.Name}", example);
    }

    private static async Task<IResult> GetAll(
        IExamplePersistence examplePersistence, string? searchTerm)
    {
        if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
        {
            var matched = await examplePersistence.SearchByValueAsync(searchTerm);
            return Results.Ok(matched);
        }

        var matches = await examplePersistence.GetAllAsync();
        return Results.Ok(matches);
    }

    private static async Task<IResult> GetByName(
        string name, IExamplePersistence examplePersistence)
    {
        var example = await examplePersistence.GetByExampleName(name);
        return example is not null ? Results.Ok(example) : Results.NotFound();
    }

    private static async Task<IResult> Update(
        string name, ExampleModel example, IExamplePersistence examplePersistence,
        IValidator<ExampleModel> validator)
    {
        example.Name = name;
        var validationResult = await validator.ValidateAsync(example);
        if (!validationResult.IsValid) return Results.BadRequest(validationResult.Errors);

        var updated = await examplePersistence.UpdateAsync(example);
        return updated ? Results.Ok(example) : Results.NotFound();
    }

    private static async Task<IResult> Delete(
        string name, IExamplePersistence examplePersistence)
    {
        var deleted = await examplePersistence.DeleteAsync(name);
        return deleted ? Results.Ok() : Results.NotFound();
    }
}
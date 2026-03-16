using System.ComponentModel.DataAnnotations;
using Backend.Api.Example.Models;
using Backend.Api.Example.Services;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Example.Endpoints;

[ExcludeFromCodeCoverage]
public static class ExampleEndpoints
{
    public static void UseExampleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("example", Create);
        app.MapGet("example", GetAll);
        app.MapGet("example/{name}", GetByName).WithName("GetByName");
        app.MapPut("example/{name}", Update);
        app.MapDelete("example/{name}", Delete);
    }

    private static async Task<Results<BadRequest<List<ValidationResult>>, Conflict<string>, CreatedAtRoute>> Create(
        [FromBody] ExampleModel example, 
        [FromServices] IExamplePersistence examplePersistence
        )
    {
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(example, new ValidationContext(example), results, validateAllProperties: true);
        if (!isValid)
        {
            return TypedResults.BadRequest(results);
        }

        var created = await examplePersistence.CreateAsync(example);
        if (!created)
        {
            return TypedResults.Conflict("An example record with this name already exists");
        }

        return TypedResults.CreatedAtRoute(
            routeName: "GetByName",
            routeValues: new { entityId = example.Name });
    }

    private static async Task<Ok<IEnumerable<ExampleModel>>> GetAll(
        [FromQuery]  string? searchTerm,
        [FromServices] IExamplePersistence examplePersistence)
    {
        if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
        {
            var matched = await examplePersistence.SearchByValueAsync(searchTerm);
            return TypedResults.Ok(matched);
        }

        var matches = await examplePersistence.GetAllAsync();
        return TypedResults.Ok(matches);
    }

    private static async Task<Results<Ok<ExampleModel>, NotFound>> GetByName(
        [FromRoute] string name, 
        [FromServices] IExamplePersistence examplePersistence)
    {
        var example = await examplePersistence.GetByExampleName(name);
        return example is not null ? TypedResults.Ok(example) : TypedResults.NotFound();
    }

    private static async Task<Results<Ok<ExampleModel>, BadRequest<List<ValidationResult>>, NotFound>> Update(
        [FromRoute] string name, 
        [FromBody] ExampleModel example,
        [FromServices] IExamplePersistence examplePersistence)
    {
        example.Name = name;
        
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(example, new ValidationContext(example), results, validateAllProperties: true);
        if (!isValid)
        {
            return TypedResults.BadRequest(results);
        }

        var updated = await examplePersistence.UpdateAsync(example);
        return updated ? TypedResults.Ok(example) : TypedResults.NotFound();
    }

    private static async Task<IResult> Delete(
        [FromRoute] string name, 
        [FromServices] IExamplePersistence examplePersistence)
    {
        var deleted = await examplePersistence.DeleteAsync(name);
        return deleted ? Results.Ok() : Results.NotFound();
    }
}
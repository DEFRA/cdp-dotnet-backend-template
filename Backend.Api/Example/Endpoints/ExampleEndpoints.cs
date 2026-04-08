using Backend.Api.Example.Models;
using Backend.Api.Example.Services;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Example.Endpoints;

[ExcludeFromCodeCoverage]
public static class ExampleEndpoints
{
    public static RouteGroupBuilder MapExampleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/example")
            .WithTags("Example");

        group.MapPost(string.Empty, Create);
        group.MapGet(string.Empty, GetAll);
        group.MapGet("/{name}", GetByName).WithName("GetByName");
        group.MapPut("/{name}", Update);
        group.MapDelete("/{name}", Delete);

        return group;
    }

    private static async Task<Results<CreatedAtRoute<ExampleModel>, Conflict<ProblemDetails>>> Create(
        CreateExampleRequest request,
        [FromServices] IExamplePersistence examplePersistence,
        CancellationToken cancellationToken)
    {
        var example = new ExampleModel
        {
            Name = request.Name,
            Value = request.Value,
            Counter = request.Counter
        };

        try
        {
            await examplePersistence.CreateAsync(example, cancellationToken);
        }
        catch (ExampleConflictException ex)
        {
            return TypedResults.Conflict(new ProblemDetails
            {
                Title = "Example already exists",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }

        return TypedResults.CreatedAtRoute(example, "GetByName", new { name = example.Name });
    }

    private static async Task<Ok<IReadOnlyCollection<ExampleModel>>> GetAll(
        [FromQuery] string? searchTerm,
        [FromServices] IExamplePersistence examplePersistence,
        CancellationToken cancellationToken)
    {
        if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
        {
            var matched = await examplePersistence.SearchAsync(searchTerm, cancellationToken);
            return TypedResults.Ok(matched);
        }

        var matches = await examplePersistence.GetAllAsync(cancellationToken);
        return TypedResults.Ok(matches);
    }

    private static async Task<Results<Ok<ExampleModel>, NotFound>> GetByName(
        [FromRoute] string name,
        [FromServices] IExamplePersistence examplePersistence,
        CancellationToken cancellationToken)
    {
        var example = await examplePersistence.GetByNameAsync(name, cancellationToken);
        return example is not null ? TypedResults.Ok(example) : TypedResults.NotFound();
    }

    private static async Task<Results<Ok<ExampleModel>, NotFound>> Update(
        [FromRoute] string name,
        UpdateExampleRequest request,
        [FromServices] IExamplePersistence examplePersistence,
        CancellationToken cancellationToken)
    {
        var example = new ExampleModel
        {
            Name = name,
            Value = request.Value,
            Counter = request.Counter
        };

        var updated = await examplePersistence.UpdateAsync(example, cancellationToken);
        return updated is not null ? TypedResults.Ok(updated) : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound>> Delete(
        [FromRoute] string name,
        [FromServices] IExamplePersistence examplePersistence,
        CancellationToken cancellationToken)
    {
        var deleted = await examplePersistence.DeleteAsync(name, cancellationToken);
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
using System.Net;
using System.Net.Http.Json;
using Backend.Api.Example.Models;
using Backend.Api.Example.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Backend.Api.Test.Example.Endpoints;

public class ExampleEndpointsTest
{
    [Fact]
    public async Task Post_returns_created_and_location_for_valid_request()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new TestApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/example", new CreateExampleRequest
        {
            Name = "alpha",
            Value = "first value",
            Counter = 2
        }, cancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/example/alpha", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Post_returns_validation_problem_details_for_invalid_request()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new TestApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/example", new CreateExampleRequest
        {
            Name = string.Empty,
            Value = string.Empty,
            Counter = -1
        }, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken);
        Assert.NotNull(problem);
        Assert.Contains(nameof(CreateExampleRequest.Name), problem.Errors.Keys);
        Assert.Contains(nameof(CreateExampleRequest.Value), problem.Errors.Keys);
        Assert.Contains(nameof(CreateExampleRequest.Counter), problem.Errors.Keys);
    }

    [Fact]
    public async Task Post_returns_conflict_when_name_already_exists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new TestApplicationFactory();
        using var client = factory.CreateClient();

        factory.MockPersistence
            .CreateAsync(Arg.Any<ExampleModel>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ExampleConflictException("alpha", new Exception()));


        var response = await client.PostAsJsonAsync("/example", new CreateExampleRequest
        {
            Name = "alpha",
            Value = "second value",
            Counter = 5
        }, cancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        Assert.NotNull(problem);
        Assert.Equal("Example already exists", problem.Title);
    }

    [Fact]
    public async Task Put_uses_route_name_and_returns_updated_model()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new TestApplicationFactory();
        using var client = factory.CreateClient();

        factory.MockPersistence
            .UpdateAsync(Arg.Any<ExampleModel>(), Arg.Any<CancellationToken>())
            .Returns(new ExampleModel { Name = "alpha", Counter = 1, Value = "first value" });

        var response = await client.PutAsJsonAsync("/example/alpha", new UpdateExampleRequest
        {
            Value = "updated value",
            Counter = 9
        }, cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_supports_search_term_filter()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new TestApplicationFactory();
        using var client = factory.CreateClient();

        await client.GetFromJsonAsync<List<ExampleModel>>("/example?searchTerm=starter", cancellationToken);
        await factory.MockPersistence.Received().SearchAsync(Arg.Is("starter"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_returns_no_content_for_existing_record()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new TestApplicationFactory();
        using var client = factory.CreateClient();

        factory.MockPersistence
            .DeleteAsync(Arg.Is("alpha"), Arg.Any<CancellationToken>())
            .Returns(true);

        var response = await client.DeleteAsync("/example/alpha", cancellationToken);
        await factory.MockPersistence.Received().DeleteAsync(Arg.Is("alpha"), Arg.Any<CancellationToken>());

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Health_endpoint_is_available()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new TestApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed class TestApplicationFactory : WebApplicationFactory<Program>
    {
        public readonly IExamplePersistence MockPersistence = Substitute.For<IExamplePersistence>();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IExamplePersistence>();
                services.AddSingleton(MockPersistence);
            });
        }
    }
}
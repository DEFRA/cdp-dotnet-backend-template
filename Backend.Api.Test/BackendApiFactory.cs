using Backend.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MongoDb;

namespace Backend.Api.Test;

public sealed class BackendApiFactory : WebApplicationFactory<IApiMarker>
{
    private readonly MongoDbContainer _mongoDbContainer = new MongoDbBuilder().WithImage("mongo:6.0")
        .WithPortBinding(27017, true).Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _mongoDbContainer.StartAsync().Wait();

        builder.ConfigureServices(collection =>
        {
            collection.RemoveAll(typeof(IMongoDbClientFactory));
            collection.AddSingleton<IMongoDbClientFactory>(_ =>
                new MongoDbClientFactory(_mongoDbContainer.GetConnectionString(), "books"));
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await _mongoDbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
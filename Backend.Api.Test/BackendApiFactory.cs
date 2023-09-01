using Backend.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MongoDb;

namespace Backend.Api.Test;

public sealed class BackendApiFactory : WebApplicationFactory<IApiMarker>
{
    // private readonly MongoRunnerOptions _options = new()
    // {
    //     UseSingleNodeReplicaSet = true, // Default: false
    //     StandardOuputLogger = line => Console.WriteLine(line), // Default: null
    //     StandardErrorLogger = line => Console.WriteLine(line), // Default: null
    //     AdditionalArguments = "--quiet", // Default: null
    //     MongoPort = 27777, // Default: random available port
    //     KillMongoProcessesWhenCurrentProcessExits = true // Default: false
    // };
    //
    // private IMongoRunner _runner = null!;

    private readonly MongoDbContainer _mongoDbContainer = new MongoDbBuilder().WithPortBinding(27017, true).Build();
    
    protected override async void ConfigureWebHost(IWebHostBuilder builder)
    {
        await _mongoDbContainer.StartAsync();

        builder.ConfigureServices(collection =>
        {
            collection.RemoveAll(typeof(IMongoDbClientFactory));
            collection.AddSingleton<IMongoDbClientFactory>(_ =>
                new MongoDbClientFactory($"mongodb://127.0.0.1:{_mongoDbContainer.GetMappedPublicPort(27017)}", "books"));
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await _mongoDbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
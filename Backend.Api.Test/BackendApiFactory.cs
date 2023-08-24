using Backend.Api.Data;
using EphemeralMongo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Backend.Api.Test;

public sealed class BackendApiFactory : WebApplicationFactory<IApiMarker>
{
    private readonly MongoRunnerOptions _options = new()
    {
        UseSingleNodeReplicaSet = true, // Default: false
        StandardOuputLogger = line => Console.WriteLine(line), // Default: null
        StandardErrorLogger = line => Console.WriteLine(line), // Default: null
        AdditionalArguments = "--quiet", // Default: null
        MongoPort = 27777, // Default: random available port
        KillMongoProcessesWhenCurrentProcessExits = true // Default: false
    };

    private IMongoRunner _runner = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _runner = MongoRunner.Run(_options);

        builder.ConfigureServices(collection =>
        {
            collection.RemoveAll(typeof(IMongoDbClientFactory));
            collection.AddSingleton<IMongoDbClientFactory>(_ =>
                new MongoDbClientFactory("mongodb://127.0.0.1:27777", "books"));
        });
    }

    public override ValueTask DisposeAsync()
    {
        _runner.Dispose();
        return base.DisposeAsync();
    }
}
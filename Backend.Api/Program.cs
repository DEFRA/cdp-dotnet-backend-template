using Backend.Api.Example.Endpoints;
using Backend.Api.Example.Services;
using Backend.Api.Utils;
using Backend.Api.Utils.Http;
using Backend.Api.Utils.Mongo;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;
using Backend.Api.Config;
using Backend.Api.Utils.Logging;
using Serilog;

var app = CreateWebApplication(args);
await app.RunAsync();
return;

[ExcludeFromCodeCoverage]
static WebApplication CreateWebApplication(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    ConfigureBuilder(builder);

    var app = builder.Build();
    return SetupApplication(app);
}

[ExcludeFromCodeCoverage]
static void ConfigureBuilder(WebApplicationBuilder builder)
{
    builder.Configuration.AddEnvironmentVariables();

    // Load certificates into Trust Store - Note must happen before Mongo and Http client connections.
    builder.Services.AddCustomTrustStore();
    
    // Configure logging to use the CDP Platform standards.
    builder.Services.AddHttpContextAccessor();
    builder.Host.UseSerilog(CdpLogging.Configuration);
    
    // Default HTTP Client
    builder.Services
        .AddHttpClient("DefaultClient")
        .AddHeaderPropagation();

    // Proxy HTTP Client
    builder.Services.AddTransient<ProxyHttpMessageHandler>();
    builder.Services
        .AddHttpClient("proxy")
        .ConfigurePrimaryHttpMessageHandler<ProxyHttpMessageHandler>();

    // Propagate trace header.
    builder.Services.AddHeaderPropagation(options =>
    {
        var traceHeader = builder.Configuration.GetValue<string>("TraceHeader");
        if (!string.IsNullOrWhiteSpace(traceHeader))
        {
            options.Headers.Add(traceHeader);
        }
    });
    
    
    // Set up the MongoDB client. Config and credentials are injected automatically at runtime.
    builder.Services.Configure<MongoConfig>(builder.Configuration.GetSection("Mongo"));
    builder.Services.AddSingleton<IMongoDbClientFactory, MongoDbClientFactory>();
    
    builder.Services.AddHealthChecks();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    
    // Set up the endpoints and their dependencies
    builder.Services.AddSingleton<IExamplePersistence, ExamplePersistence>();
}

[ExcludeFromCodeCoverage]
static WebApplication SetupApplication(WebApplication app)
{
    app.UseHeaderPropagation();
    app.UseRouting();
    app.MapHealthChecks("/health");

    // Example module, remove before deploying!
    app.UseExampleEndpoints();

    return app;
}

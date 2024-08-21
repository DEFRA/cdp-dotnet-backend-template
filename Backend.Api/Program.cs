using Backend.Api.Example.Endpoints;
using Backend.Api.Example.Services;
using Backend.Api.Utils;
using Backend.Api.Utils.Http;
using Backend.Api.Utils.Logging;
using Backend.Api.Utils.Mongo;
using FluentValidation;
using Serilog;

//-------- Configure the WebApplication builder------------------//

var builder = WebApplication.CreateBuilder(args);

// Grab environment variables
builder.Configuration.AddEnvironmentVariables();

// Serilog
builder.Logging.ClearProviders();
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.With<LogLevelMapper>()
    .CreateLogger();
builder.Logging.AddSerilog(logger);

logger.Information("Starting application");

// Load certificates into Trust Store - Note must happen before Mongo and Http client connections 
builder.Services.AddCustomTruststore(logger);

// Mongo
builder.Services.AddSingleton<IMongoDbClientFactory>(_ =>
    new MongoDbClientFactory(builder.Configuration.GetValue<string>("Mongo:DatabaseUri")!,
        builder.Configuration.GetValue<string>("Mongo:DatabaseName")!));

// our service
builder.Services.AddSingleton<IExamplePersistence, ExamplePersistence>();

// health checks
builder.Services.AddHealthChecks();

// http client
builder.Services.AddHttpClient();

// calls outside the platform should be done using the named 'proxy' http client.
builder.Services.AddHttpProxyClient(logger);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseRouting();
app.MapHealthChecks("/health");

// Example module, remove before deploying!
app.UseExampleEndpoints();

app.Run();

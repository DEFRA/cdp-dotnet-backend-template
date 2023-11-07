using Backend.Api.Config;
using Backend.Api.Data;
using Backend.Api.Endpoints;
using Backend.Api.Services;
using FluentValidation;
using Serilog;

//-------- Configure the WebApplication builder------------------//

var builder = WebApplication.CreateBuilder(args);

// Grab environment variables
builder.Configuration.AddEnvironmentVariables("CDP");
builder.Configuration.AddEnvironmentVariables();

// Serilog
builder.Logging.ClearProviders();
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Logging.AddSerilog(logger);

logger.Information("Starting application");

// Mongo
builder.Services.AddSingleton<IMongoDbClientFactory>(_ =>
    new MongoDbClientFactory(builder.Configuration.GetValue<string>("Mongo:DatabaseUri")!,
        builder.Configuration.GetValue<string>("Mongo:DatabaseName")!));

// our service
builder.Services.AddSingleton<IBookService, BookService>();

// health checks
builder.Services.AddHealthChecks();

// swagger endpoints
if (builder.IsSwaggerEnabled())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

if (builder.IsSwaggerEnabled())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UsePathBase("/cdp-dotnet-backend-template");
app.UseRouting();
app.UseLibraryEndpoints();
app.MapHealthChecks("/health");

app.Run();
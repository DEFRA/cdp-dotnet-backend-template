using Backend.Api.Example.Endpoints;
using Backend.Api.Example.Services;
using Backend.Api.Utils;
using Backend.Api.Utils.Http;
using Backend.Api.Utils.Logging;
using Backend.Api.Utils.Mongo;
using FluentValidation;
using Serilog;
using Serilog.Core;
using System.Diagnostics.CodeAnalysis;

//-------- Configure the WebApplication builder------------------//

var app = CreateWebApplication(args);
await app.RunAsync();


[ExcludeFromCodeCoverage]
static WebApplication CreateWebApplication(string[] args)
{
   var _builder = WebApplication.CreateBuilder(args);

   ConfigureWebApplication(_builder);

   var _app = BuildWebApplication(_builder);

   return _app;
}

[ExcludeFromCodeCoverage]
static void ConfigureWebApplication(WebApplicationBuilder _builder)
{
   _builder.Configuration.AddEnvironmentVariables();

   var logger = ConfigureLogging(_builder);

   // Load certificates into Trust Store - Note must happen before Mongo and Http client connections
   _builder.Services.AddCustomTrustStore(logger);

   ConfigureMongoDb(_builder);

   ConfigureEndpoints(_builder);

   _builder.Services.AddHttpClient();

   // calls outside the platform should be done using the named 'proxy' http client.
   _builder.Services.AddHttpProxyClient(logger);

   _builder.Services.AddValidatorsFromAssemblyContaining<Program>();
}

[ExcludeFromCodeCoverage]
static Logger ConfigureLogging(WebApplicationBuilder _builder)
{
   _builder.Logging.ClearProviders();
   var logger = new LoggerConfiguration()
       .ReadFrom.Configuration(_builder.Configuration)
       .Enrich.With<LogLevelMapper>()
       .CreateLogger();
   _builder.Logging.AddSerilog(logger);
   logger.Information("Starting application");
   return logger;
}

[ExcludeFromCodeCoverage]
static void ConfigureMongoDb(WebApplicationBuilder _builder)
{
   _builder.Services.AddSingleton<IMongoDbClientFactory>(_ =>
       new MongoDbClientFactory(_builder.Configuration.GetValue<string>("Mongo:DatabaseUri")!,
           _builder.Configuration.GetValue<string>("Mongo:DatabaseName")!));
}

[ExcludeFromCodeCoverage]
static void ConfigureEndpoints(WebApplicationBuilder _builder)
{
   // our Example service, remove before deploying!
   _builder.Services.AddSingleton<IExamplePersistence, ExamplePersistence>();

   _builder.Services.AddHealthChecks();
}

[ExcludeFromCodeCoverage]
static WebApplication BuildWebApplication(WebApplicationBuilder _builder)
{
   var app = _builder.Build();

   app.UseRouting();
   app.MapHealthChecks("/health");

   // Example module, remove before deploying!
   app.UseExampleEndpoints();

   return app;
}

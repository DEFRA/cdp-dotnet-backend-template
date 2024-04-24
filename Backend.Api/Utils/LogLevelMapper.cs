using Serilog.Core;
using Serilog.Events;

namespace Backend.Api.Utils;


/**
 * Maps log levels from the C# default 'Information' etc to the node style 'info'.
 */
public class LogLevelMapper : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var logLevel = string.Empty;

        switch (logEvent.Level)
        {
            case LogEventLevel.Debug:
                logLevel = "debug";
                break;

            case LogEventLevel.Error:
                logLevel = "error";
                break;

            case LogEventLevel.Fatal:
                logLevel = "fatal";
                break;

            case LogEventLevel.Information:
                logLevel = "info";
                break;

            case LogEventLevel.Verbose:
                logLevel = "all";
                break;

            case LogEventLevel.Warning:
                logLevel = "warn";
                break;
        }

        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("log.level", logLevel));
    }
}
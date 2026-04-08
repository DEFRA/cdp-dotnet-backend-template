using System.Diagnostics.CodeAnalysis;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Backend.Api.Utils.Auditing;

/// <summary>
/// Adds a custom AUDIT log level to the default logger.
/// Messages logged at AUDIT level are routed to the CDP Audit store.
/// </summary>
[ExcludeFromCodeCoverage]
public static class AuditLogger
{
    public const string AuditPropertyName = "IsAudit";

    public static Logger CreateAuditLogger()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .Filter.With<Filters.OnlyAuditEvents>()
            .Enrich.With<EnrichAuditLog>()
            .WriteTo.Console(new CompactJsonFormatter())
            .CreateLogger();
    }

    [ExcludeFromCodeCoverage]
    class EnrichAuditLog : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("log.level", "audit"));
        }
    }

    public static class Filters
    {
        [ExcludeFromCodeCoverage]
        public class OnlyAuditEvents : ILogEventFilter
        {
            public bool IsEnabled(LogEvent logEvent)
            {
                return logEvent.Properties.ContainsKey(AuditPropertyName);
            }
        }

        [ExcludeFromCodeCoverage]
        public class ExcludeAuditEvents : ILogEventFilter
        {
            public bool IsEnabled(LogEvent logEvent)
            {
                return !logEvent.Properties.ContainsKey(AuditPropertyName);
            }
        }
    }
}
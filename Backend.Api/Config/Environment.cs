using System.Diagnostics.CodeAnalysis;

namespace Backend.Api.Config;

[ExcludeFromCodeCoverage]
public static class Environment
{
    public static bool IsDevMode(this WebApplicationBuilder builder)
    {
        return !builder.Environment.IsProduction();
    }
}

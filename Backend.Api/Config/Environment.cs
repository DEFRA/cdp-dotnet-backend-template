namespace Backend.Api.Config;

public static class Environment
{
    public static bool IsDevMode(this WebApplicationBuilder builder)
    {
        return !builder.Environment.IsProduction();
    }
}
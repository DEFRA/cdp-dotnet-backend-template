namespace Backend.Api.Config;

public static class Swagger
{
    public static bool IsSwaggerEnabled(this WebApplicationBuilder builder)
    {
        return builder.IsDevMode() || builder.Configuration.GetValue<bool>("EnableSwagger");
    }
}
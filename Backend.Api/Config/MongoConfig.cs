namespace Backend.Api.Config;

using System.ComponentModel.DataAnnotations;

public class MongoConfig
{
    [Required]
    public required string DatabaseUri { get; init; }

    [Required]
    public required string DatabaseName { get; init; }
}
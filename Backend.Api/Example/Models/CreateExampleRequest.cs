using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Example.Models;

public sealed class CreateExampleRequest
{
    [Required]
    [MinLength(1)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string Value { get; init; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Counter { get; init; }
}
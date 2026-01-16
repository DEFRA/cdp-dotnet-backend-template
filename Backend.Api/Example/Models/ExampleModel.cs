using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Backend.Api.Example.Models;

[ExcludeFromCodeCoverage]
public class ExampleModel
{
    [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public ObjectId? Id { get; init; }

    public required string Name { get; set; }

    public required string Value { get; set; }

    public int? Counter { get; set; } = 0;

    public DateTime? Created { get; set; } = DateTime.UtcNow;
}
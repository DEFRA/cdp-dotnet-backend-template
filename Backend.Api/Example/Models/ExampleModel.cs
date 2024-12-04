using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Backend.Api.Example.Models;

public class ExampleModel
{
    [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public ObjectId Id { get; init; } = default!;

    public string Name { get; set; } = default!;

    public string Value { get; set; } = default!;

    public int? Counter { get; set; } = 0;

    public DateTime? Created { get; set; } = DateTime.UtcNow;
}
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Backend.Api.Models;

public class Book
{
    [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public ObjectId Id { get; init; } = default!;

    public string Isbn { get; set; } = default!;

    public string Title { get; set; } = default!;

    public string Author { get; set; } = default!;

    public string ShortDescription { get; set; } = default!;

    public int PageCount { get; set; }

    public DateTime ReleaseDate { get; set; }
}
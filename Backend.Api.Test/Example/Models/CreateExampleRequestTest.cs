using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Backend.Api.Example.Models;

namespace Backend.Api.Test.Example.Models;

public class CreateExampleRequestTest
{
    [Fact]
    public void Test_validation_on_valid_request()
    {
        var req = new CreateExampleRequest { Name = "Foo", Counter = 1, Value = "test" };
        var ctx = new ValidationContext(req);
        var results = new List<ValidationResult>();
        Assert.True(Validator.TryValidateObject(req, ctx, results));
    }

    [Fact]
    public void Test_validation_fails_on_invalid_request()
    {
        var req = new CreateExampleRequest { Name = "", Counter = -1, Value = null };
        var ctx = new ValidationContext(req);
        var results = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(req, ctx, results));
    }

    [Fact]
    public void Test_json_serialization()
    {
        var req = new CreateExampleRequest { Name = "Foo", Counter = 1, Value = "test" };
        var json = JsonSerializer.Serialize(req);
        Assert.Equal("{\"Name\":\"Foo\",\"Value\":\"test\",\"Counter\":1}", json);
    }

    [Fact]
    public void Test_json_deserialization()
    {
        var json = "{\"Name\":\"Foo\",\"Value\":\"test\",\"Counter\":1}";
        var req = JsonSerializer.Deserialize<CreateExampleRequest>(json);
        Assert.Equivalent(new CreateExampleRequest { Name = "Foo", Counter = 1, Value = "test" }, req);
    }
}
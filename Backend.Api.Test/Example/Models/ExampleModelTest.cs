using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Backend.Api.Example.Models;

namespace Backend.Api.Test.Example.Models;

public class ExampleModelTest
{
    [Fact]
    public void Test_model_can_serialize_to_json()
    {
        var model = new ExampleModel
        {
            Name = "testName", Value = "testValue", Counter = 10, Created = new DateTime(2026, 3, 16)
        };

        var json = JsonSerializer.Serialize(model);
        Assert.DoesNotContain("ObjectId", json);

        var modelFromJson = JsonSerializer.Deserialize<ExampleModel>(json);
        Assert.Equivalent(model, modelFromJson);
    }
    
    
    [Fact]
    public void Test_validation()
    {
        var model = new ExampleModel
        {
            Name = "testName", Value = "testValue", Counter = 10, Created = new DateTime(2026, 3, 16)
        };

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        Assert.True(isValid);
    }


        
    [Fact]
    public void Test_validation_with_invalid_model()
    {
        var model = new ExampleModel
        {
            Name = "", Value = "", Counter = -10, Created = new DateTime(2026, 3, 16)
        };

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        Assert.False(isValid);
    }
}
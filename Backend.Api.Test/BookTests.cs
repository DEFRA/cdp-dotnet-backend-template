namespace Backend.Api.Test;

public class BookTests
{
    [Fact]
    public void Book_Json_Serialization_No_Id()
    {
    }

    private string GenerateIsbn()
    {
        return $"{Random.Shared.Next(100, 999)}-" +
               $"{Random.Shared.Next(1000000000, 2100999999)}";
    }
}
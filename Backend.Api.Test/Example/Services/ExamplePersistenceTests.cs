using MongoDB.Driver;
using Backend.Api.Utils.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Backend.Api.Example.Models;
using Backend.Api.Example.Services;
using FluentAssertions;
using MongoDB.Bson;

namespace Backend.Api.Test.Example.Services;

public class ExamplePersistenceTests
{

   private readonly IMongoDbClientFactory _conFactoryMock = Substitute.For<IMongoDbClientFactory>();
   private readonly IMongoCollection<ExampleModel> _collectionMock = Substitute.For<IMongoCollection<ExampleModel>>();
   private readonly IMongoDatabase _databaseMock = Substitute.For<IMongoDatabase>();
   private readonly CollectionNamespace _collectionNamespace = new("test", "example");

   private readonly ExamplePersistence _persistence;

   public ExamplePersistenceTests()
   {
      _collectionMock
            .CollectionNamespace
            .Returns(_collectionNamespace);
      _collectionMock
            .Database
            .Returns(_databaseMock);
      _databaseMock
         .DatabaseNamespace
         .Returns(new DatabaseNamespace("test"));
      _conFactoryMock
         .GetClient()
         .Returns(Substitute.For<IMongoClient>());
      _conFactoryMock
         .GetCollection<ExampleModel>("example")
         .Returns(_collectionMock);

      _persistence = new ExamplePersistence(_conFactoryMock, NullLoggerFactory.Instance);
   }

   [Fact]
   public async Task CreateAsyncOk()
   {
      _collectionMock
          .InsertOneAsync(Arg.Any<ExampleModel>())
          .Returns(Task.CompletedTask);

      var example = new ExampleModel
      {
         Id = new ObjectId(),
         Value = "some value",
         Name = "Test",
         Counter = 0
      };
      var result = await _persistence.CreateAsync(example);
      result.Should().BeTrue();
   }

   [Fact]
   public async Task CreateAsyncLogError()
   {

      var loggerFactoryMock = Substitute.For<ILoggerFactory>();
      var logMock = Substitute.For<ILogger<ExamplePersistence>>();
      loggerFactoryMock.CreateLogger<ExamplePersistence>().Returns(logMock);

      _collectionMock
          .InsertOneAsync(Arg.Any<ExampleModel>())
          .Returns(Task.FromException<ExampleModel>(new Exception()));

      var persistence = new ExamplePersistence(_conFactoryMock, loggerFactoryMock);

      var example = new ExampleModel()
      {
         Id = new ObjectId(),
         Value = "some value",
         Name = "Test",
         Counter = 0
      };

      var result = await persistence.CreateAsync(example);

      result.Should().BeFalse();
   }

}

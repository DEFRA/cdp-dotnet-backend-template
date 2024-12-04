using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NSubstitute;
using Microsoft.Extensions.Logging.Abstractions;
using Backend.Api.Utils.Mongo;

namespace Backend.Api.Test.Utils.Mongo
{
   public class MongoServiceTests
   {
      private readonly IMongoDbClientFactory _connectionFactoryMock;
      private readonly ILoggerFactory _loggerFactoryMock;
      private readonly IMongoClient _clientMock;
      private readonly IMongoCollection<TestModel> _collectionMock;

      private readonly TestMongoService _service;

      public MongoServiceTests()
      {
         _connectionFactoryMock = Substitute.For<IMongoDbClientFactory>();
         _loggerFactoryMock = Substitute.For<ILoggerFactory>();
         _clientMock = Substitute.For<IMongoClient>();
         _collectionMock = Substitute.For<IMongoCollection<TestModel>>();

         _connectionFactoryMock
            .GetClient()
            .Returns(Substitute.For<IMongoClient>());

         _connectionFactoryMock
            .GetCollection<TestModel>(Arg.Any<string>())
            .Returns(_collectionMock);

         _collectionMock.CollectionNamespace.Returns(new CollectionNamespace("test", "example"));
         _collectionMock.Database.DatabaseNamespace.Returns(new DatabaseNamespace("test"));


         _service = new TestMongoService(_connectionFactoryMock, "testCollection", NullLoggerFactory.Instance);

         _collectionMock.DidNotReceive().Indexes.CreateMany(Arg.Any<IEnumerable<CreateIndexModel<TestModel>>>());
      }

      [Fact]
      public void EnsureIndexes_CreatesIndexes_WhenIndexesAreDefined()
      {
         var _indexes = new List<CreateIndexModel<TestModel>>()
            {
                new CreateIndexModel<TestModel>(Builders<TestModel>.IndexKeys.Ascending(x => x.Name))
            };
         _service.setIndexes(_indexes);
         _service.RunEnsureIndexes();

         _collectionMock.Received(1).Indexes.CreateMany(_indexes);
      }

      [Fact]
      public void EnsureIndexes_DoesNotCreateIndexes_WhenIndexesAreNotDefined()
      {
         _service.setIndexes(new List<CreateIndexModel<TestModel>>());
         _service.RunEnsureIndexes();

         _collectionMock.DidNotReceive().Indexes.CreateMany(Arg.Any<IEnumerable<CreateIndexModel<TestModel>>>());

      }

      public class TestModel
      {
         public string? Name { get; set; }
      }

      public interface ITestMongoService
      {
         public List<CreateIndexModel<TestModel>> getIndexes();
         public void setIndexes(List<CreateIndexModel<TestModel>> indexes);
      }
      private class TestMongoService : MongoService<TestModel>, ITestMongoService
      {
         protected List<CreateIndexModel<TestModel>> _indexes = new List<CreateIndexModel<TestModel>>();

         public TestMongoService(IMongoDbClientFactory connectionFactory, string collectionName, ILoggerFactory loggerFactory)
             : base(connectionFactory, collectionName, loggerFactory)
         {
         }

         public List<CreateIndexModel<TestModel>> getIndexes()
         {
            return _indexes;
         }

         public void setIndexes(List<CreateIndexModel<TestModel>> indexes)
         {
            this._indexes = indexes;
         }

         protected override List<CreateIndexModel<TestModel>> DefineIndexes(IndexKeysDefinitionBuilder<TestModel> builder)
         {
            if (getIndexes() == null)
            {
               throw new System.Exception("Indexes not defined");
            }
            return getIndexes();
         }

         public void RunEnsureIndexes()
         {
            base.EnsureIndexes();
         }


      }

   }
}

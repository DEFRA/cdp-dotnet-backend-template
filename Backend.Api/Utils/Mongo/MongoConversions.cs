using MongoDB.Bson.Serialization.Conventions;

namespace Backend.Api.Utils.Mongo;

public static class MongoConventions
{
    private static int s_initialized;

    public static void Register()
    {
        if (Interlocked.Exchange(ref s_initialized, 1) == 1)
        {
            return;
        }

        var conversions = new ConventionPack
        {
            new CamelCaseElementNameConvention()
        };

        ConventionRegistry.Register("CamelCase", conversions, _ => true);
    }
}
using MongoDB.Driver;
using MongoDB.Driver.Authentication.AWS;

namespace Backend.Api.Utils.Mongo;

public static class MongoExtensions
{
    private static int s_initialized;

    public static void Register()
    {
        if (Interlocked.Exchange(ref s_initialized, 1) == 1)
        {
            return;
        }

        MongoClientSettings.Extensions.AddAWSAuthentication();
    }
}
using Serilog.Core;

namespace Backend.Api.Utils;

public static class Proxy
{
   public const string ProxyClient = "proxy";

   public static void AddHttpProxyClient(this IServiceCollection services, Logger logger)
   {
      services.AddHttpClient(ProxyClient).ConfigurePrimaryHttpMessageHandler(() =>
      {
         // Note: HTTPS proxy isn't support in dotnet until dotnet 8
         var cdpHttpProxy = Environment.GetEnvironmentVariable("CDP_HTTP_PROXY");
         if (cdpHttpProxy != null)
         {
            var proxy = new System.Net.WebProxy
            {
               BypassProxyOnLocal = true,
               Address = new Uri(cdpHttpProxy)
            };
            logger.Information("Creating proxy http client, {proxyUri}", cdpHttpProxy);
            return new HttpClientHandler { Proxy = proxy, UseProxy = true };
         }
         else
         {
            logger.Warning("CDP_HTTP_PROXY is NOT set, proxy client will be disabled");
            return new HttpClientHandler { UseProxy = false };
         }
      });
   }
}

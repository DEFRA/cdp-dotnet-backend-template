using System.Net;
using Serilog.Core;

namespace Backend.Api.Utils.Http;

public static class Proxy
{
    public const string ProxyClient = "proxy";

    /**
     * A preconfigured HTTP Client that uses the Platform's outbound proxy.
     * 
     * Usage:
     *  1. inject an `IHttpClientFactory` into your class.
     *  2. Use the IHttpClientFactory to create a named instance of HttpClient:
     *     `clientFactory.CreateClient(Proxy.ProxyClient);`
     */
    public static void AddHttpProxyClient(this IServiceCollection services, Logger logger)
    {
        services.AddHttpClient(ProxyClient).ConfigurePrimaryHttpMessageHandler(() =>
        {
            var proxyUri = Environment.GetEnvironmentVariable("CDP_HTTPS_PROXY");
            var proxy = new WebProxy
            {
                BypassProxyOnLocal = true
            };
            if (proxyUri != null)
            {
                logger.Debug("Creating proxy http client");
                var uri = new UriBuilder(proxyUri);

                var credentials = GetCredentialsFromUri(uri);
                if (credentials != null)
                {
                    logger.Debug("Setting proxy credentials");
                    proxy.Credentials = credentials;    
                }

                // Remove credentials from URI to so they don't get logged.
                uri.UserName = "";
                uri.Password = "";
                proxy.Address = uri.Uri;
            }
            else
            {
                logger.Warning("CDP_HTTP_PROXY is NOT set, proxy client will be disabled");
            }
            return new HttpClientHandler { Proxy = proxy, UseProxy = proxyUri != null};
        });
    }
    
    private static NetworkCredential? GetCredentialsFromUri(UriBuilder uri)
    {
        var username = uri.UserName;
        var password = uri.Password;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return null;
        return new NetworkCredential(username, password);
    }

}


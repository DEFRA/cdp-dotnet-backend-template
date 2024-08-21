using System.Net;
using Serilog.Core;

namespace Backend.Api.Utils.Http;

public static class Proxy
{
    public const string ProxyClient = "proxy";

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
                var uri = new Uri(proxyUri);
                logger.Debug("Creating proxy http client");
                proxy.Address = uri;

                var credentials = GetCredentialsFromUri(uri) ?? GetCredentialsFromEnv();
                if (credentials == null) return new HttpClientHandler { Proxy = proxy, UseProxy = true };
                logger.Debug("Setting proxy credentials");
                proxy.Credentials = credentials;
            }
            else
            {
                logger.Warning("CDP_HTTP_PROXY is NOT set, proxy client will be disabled");
            }
            return new HttpClientHandler { Proxy = proxy, UseProxy = proxyUri != null};
        });
    }

    private static NetworkCredential? GetCredentialsFromUri(Uri uri)
    {
        var split = uri.UserInfo.Split(':');
        return split.Length == 2 ? new NetworkCredential(split[0], split[1]) : null;
    }
    
    private static NetworkCredential? GetCredentialsFromEnv()
    {
        var proxyUsername = Environment.GetEnvironmentVariable("SQUID_USERNAME");
        var proxyPassword = Environment.GetEnvironmentVariable("SQUID_PASSWORD");
        if (proxyUsername != null && proxyPassword != null)
        {
            return new NetworkCredential(proxyUsername, proxyPassword);
        }

        return null;
    }
}


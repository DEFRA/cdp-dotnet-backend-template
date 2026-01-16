using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Backend.Api.Utils.Http;

public class ProxyHttpMessageHandler : HttpClientHandler
{
    [ExcludeFromCodeCoverage]
    public ProxyHttpMessageHandler(ILogger<ProxyHttpMessageHandler> logger)
    {
        var proxyUri = Environment.GetEnvironmentVariable("HTTP_PROXY");
        var proxy = new WebProxy { BypassProxyOnLocal = true };
        if (proxyUri != null)
        {
            logger.LogDebug("Creating proxy http client");
            var uri = new UriBuilder(proxyUri).Uri;
            proxy.Address = uri;
        }
        else
        {
            logger.LogWarning("HTTP_PROXY is NOT set, proxy client will be disabled");
        }

        Proxy = proxy;
        UseProxy = proxyUri != null;
    }
}
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Backend.Api.Utils.Http;

public class ProxyHttpMessageHandler : HttpClientHandler
{
    [ExcludeFromCodeCoverage]
    public ProxyHttpMessageHandler()
    {
        var proxyUri = Environment.GetEnvironmentVariable("CDP_HTTPS_PROXY");
        var proxy = new WebProxy { BypassProxyOnLocal = true };
        if (proxyUri != null)
        {
            var uri = new UriBuilder(proxyUri);

            var credentials = GetCredentialsFromUri(uri);
            if (credentials != null)
            {
                proxy.Credentials = credentials;
            }

            // Remove credentials from URI to so they don't get logged.
            uri.UserName = "";
            uri.Password = "";
            proxy.Address = uri.Uri;
        }

        Proxy = proxy;
        UseProxy = proxyUri != null;
    }

    public static NetworkCredential? GetCredentialsFromUri(UriBuilder uri)
    {
        var username = uri.UserName;
        var password = uri.Password;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return null;
        return new NetworkCredential(username, password);
    }
}
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Serilog.Core;

namespace Backend.Api.Utils;

public static class TrustStore
{
    private const string ConfigKey = "TrustStore";

    public static void SetupTrustStore(this WebApplicationBuilder builder, Logger logger)
    {
        logger.Information("Loading Certificates into Trust store");
        var certificates = GetCertificates(builder.Configuration, logger);
        AddCertificates(certificates);
    }

    private static List<string> GetCertificates(IConfiguration configuration, Logger logger)
    {
        return configuration.GetSection(ConfigKey).GetChildren()
            .Where(config => config.Value != null && IsBase64String(config.Value))
            .Select((config, _) =>
            {
                var configValue = config.Value;
                var data = Convert.FromBase64String(configValue!);
                logger.Information($"{ConfigKey}.{config.Key} certificate decoded");
                return Encoding.UTF8.GetString(data);
            }).ToList();
    }

    private static void AddCertificates(IReadOnlyCollection<string> certificates)
    {
        if (!certificates.Any()) return; // to stop trust store access denied issues on Macs
        var x509Certificate2S = certificates.Select(
            cert => new X509Certificate2(Encoding.ASCII.GetBytes(cert)));
        var certificateCollection = new X509Certificate2Collection();

        foreach (var certificate2 in x509Certificate2S)
        {
            certificateCollection.Add(certificate2);
        }

        var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
        try
        {
            store.Open(OpenFlags.ReadWrite);
            store.AddRange(certificateCollection);
        }
        catch (Exception ex)
        {
            throw new FileLoadException("Root certificate import failed: " + ex.Message, ex);
        }
        finally
        {
            store.Close();
        }
    }

    private static bool IsBase64String(string str)
    {
        var buffer = new Span<byte>(new byte[str.Length]);
        return Convert.TryFromBase64String(str, buffer, out _);
    }
}
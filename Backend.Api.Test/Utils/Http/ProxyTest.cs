using Backend.Api.Utils.Http;
using NSubstitute;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;
using Serilog.Core;
using Elastic.CommonSchema;
using Serilog;

namespace Backend.Api.Test.Utils.Http;

public class ProxyTest
{

   private readonly Logger logger = new LoggerConfiguration().CreateLogger();

   private readonly string proxyUri = "http://user:password@localhost:8080";
   private readonly string localProxy = "http://localhost:8080/";
   private readonly string localhost = "http://localhost/";

   public ProxyTest()
   {
   }

   [Fact]
   public void ExtractProxyCredentials()
   {

      var proxy = new System.Net.WebProxy
      {
         BypassProxyOnLocal = true
      };

      Proxy.ConfigureProxy(proxy, proxyUri, logger);

      var credentials = proxy.Credentials?.GetCredential(new System.Uri(proxyUri), "Basic");

      credentials?.UserName.Should().Be("user");
      credentials?.Password.Should().Be("password");
   }

   [Fact]
   public void ExtractProxyEmptyCredentials()
   {
      var noPasswordUri = "http://user@localhost:8080";

      var proxy = new System.Net.WebProxy
      {
         BypassProxyOnLocal = true
      };

      Proxy.ConfigureProxy(proxy, noPasswordUri, logger);

      proxy.Credentials.Should().BeNull();
   }

   [Fact]
   public void ExtractProxyUri()
   {

      var proxy = new System.Net.WebProxy
      {
         BypassProxyOnLocal = true
      };

      Proxy.ConfigureProxy(proxy, proxyUri, logger);
      proxy.Address.Should().NotBeNull();
      proxy.Address?.AbsoluteUri.Should().Be(localProxy);
   }

   [Fact]
   public void CreateProxyFromUri()
   {

      var proxy = Proxy.CreateProxy(proxyUri, logger);

      proxy.Address.Should().NotBeNull();
      proxy.Address?.AbsoluteUri.Should().Be(localProxy);
   }

   [Fact]
   public void CreateNoProxyFromEmptyUri()
   {
      var proxy = Proxy.CreateProxy(null, logger);

      proxy.Address.Should().BeNull();
   }

   [Fact]
   public void ProxyShouldBypassLocal()
   {

      var proxy = Proxy.CreateProxy(proxyUri, logger);

      proxy.BypassProxyOnLocal.Should().BeTrue();
      proxy.IsBypassed(new Uri(localhost)).Should().BeTrue();
      proxy.IsBypassed(new Uri("https://defra.gov.uk")).Should().BeFalse();
   }

   [Fact]
   public void HandlerShouldHaveProxy()
   {
      var handler = Proxy.CreateHttpClientHandler(proxyUri, logger);

      handler.Proxy.Should().NotBeNull();
      handler.UseProxy.Should().BeTrue();
      handler.Proxy?.Credentials.Should().NotBeNull();
      handler.Proxy?.GetProxy(new Uri(localhost)).Should().NotBeNull();
      handler.Proxy?.GetProxy(new Uri("http://google.com")).Should().NotBeNull();
      handler.Proxy?.GetProxy(new Uri(localhost))?.AbsoluteUri.Should().Be(localhost);
      handler.Proxy?.GetProxy(new Uri("http://google.com"))?.AbsoluteUri.Should().Be(localProxy);
   }


}

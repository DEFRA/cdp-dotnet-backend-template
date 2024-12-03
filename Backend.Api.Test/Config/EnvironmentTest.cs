using Microsoft.AspNetCore.Builder;

namespace Backend.Api.Test.Config;

public class EnvironmentTest
{

   [Fact]
   public void IsNotDevModeByDefault()
   {
      var builder = WebApplication.CreateBuilder();

      var isDev = Backend.Api.Config.Environment.IsDevMode(builder);

      Assert.False(isDev);
   }
}

using Microsoft.AspNetCore.Builder;

namespace Backend.Api.Test.Config;

public class EnvironmentTest
{

   [Fact]
   public void IsNotDevModeByDefault()
   {
      var _builder = WebApplication.CreateBuilder();

      var isDev = Backend.Api.Config.Environment.IsDevMode(_builder);

      Assert.False(isDev);
   }
}

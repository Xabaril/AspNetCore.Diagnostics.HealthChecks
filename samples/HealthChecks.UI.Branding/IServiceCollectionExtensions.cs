using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddDemoAuthentication(this IServiceCollection services)
        {
            return services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://demo.identityserver.io";
                    options.Audience = "api";

                }).Services
                  .AddAuthorization(configure =>
                  {
                      configure.AddPolicy("AuthUserPolicy", config => config.RequireAuthenticatedUser());
                  });
        }
    }
}

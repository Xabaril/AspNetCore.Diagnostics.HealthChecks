using System;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HealthChecks.UI.Oidc
{
    public class Startup
    {
        private const string HealthChecksUIPolicy = nameof(HealthChecksUIPolicy);

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddHealthChecksUI()
                .AddInMemoryStorage()
                .Services
                .AddAuthorization(cfg =>
                {
                    cfg.AddPolicy(name: HealthChecksUIPolicy, cfgPolicy =>
                    {
                        cfgPolicy.AddRequirements().RequireAuthenticatedUser();
                        cfgPolicy.AddAuthenticationSchemes(OpenIdConnectDefaults.AuthenticationScheme);
                    });
                })
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.SlidingExpiration = true;

                })
                .AddOpenIdConnect(options =>
                {
                    options.Authority = "https://demo.identityserver.io";
                    options.ClientId = "interactive.confidential";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";
                    options.SaveTokens = true;
                })
                .Services
                .AddHealthChecks()
                .AddUrlGroup(new Uri("https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks"))
                .Services
                .AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
             {
                 endpoints.MapHealthChecks("healthz", new HealthCheckOptions()
                 {
                     Predicate = _ => true,
                     ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                 });
                 endpoints.MapHealthChecksUI().RequireAuthorization(HealthChecksUIPolicy);
                 endpoints.MapDefaultControllerRoute();
             });
        }
    }
}

using HealthChecks.UI.Image.Configuration;
using HealthChecks.UI.Image.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Builder
{
    public static class IEndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapHealthChecksUI(this IEndpointRouteBuilder builder, IConfiguration configuration)
       {
            return builder.MapHealthChecksUI(setup =>
            {
                setup.ConfigureStylesheet(configuration);
                setup.ConfigurePaths(configuration);
            });
        }
    }
}

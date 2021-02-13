using HealthChecks.Network;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DnsResolveOptionsExtensions
    {
        /// <summary>
        /// Add a host for dns resolution
        /// </summary>
        /// <remarks>Usage: ResolveHost(host).To(registrations);</remarks>
        /// <param name="options">DNS Resolve options</param>
        /// <param name="host">Target host</param>

        public static Func<(DnsResolveOptions, DnsRegistration)> ResolveHost(this DnsResolveOptions options, string host)
        {
            return () => (options, new DnsRegistration(host));
        }

        public static DnsResolveOptions To(this Func<(DnsResolveOptions, DnsRegistration)> registrationFunc, params string[] resolutions)
        {
            var (options, registration) = registrationFunc();
            registration.Resolutions = resolutions;

            options.AddHost(registration.Host, registration);

            return options;
        }
    }
}

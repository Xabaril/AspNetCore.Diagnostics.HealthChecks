using Microsoft.AspNetCore.Hosting.Server.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HealthChecks.UI.Core
{
    internal class ServerAddressesService
    {
        internal IServerAddressesFeature _serverAddressesFeature;

        internal void SetFeature(IServerAddressesFeature feature) => _serverAddressesFeature = feature;

        internal ICollection<string> Addresses => _serverAddressesFeature.Addresses;

        internal bool HasAddresses => Addresses.Any();

        internal string AbsoluteUriFromRelative(string relativeUrl)
        {
            var targetAddress = Addresses.First();

            if (targetAddress.EndsWith("/"))
            {
                targetAddress = targetAddress[0..^1];
            }

            if (!relativeUrl.StartsWith("/"))
            {
                relativeUrl = $"/{relativeUrl}";
            }

            return $"{targetAddress}{relativeUrl}";
        }
    }
}

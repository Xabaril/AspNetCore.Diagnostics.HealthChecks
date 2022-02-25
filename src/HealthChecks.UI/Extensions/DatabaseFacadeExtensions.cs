using System;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore
{
    public static class DatabaseFacadeExtensions
    {
        private const string IN_MEMORY_DATABASE_PROVIDER = "Microsoft.EntityFrameworkCore.InMemory";

        public static bool IsInMemory(this DatabaseFacade database)
        {
            return string.Equals(database.ProviderName, IN_MEMORY_DATABASE_PROVIDER, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}

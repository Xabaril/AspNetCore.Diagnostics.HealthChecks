using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace Microsoft.EntityFrameworkCore
{
    public static class DatabaseFacadeExtensions
    {
        private static string InMemoryDatabaseProvider = "Microsoft.EntityFrameworkCore.InMemory";
        public static bool IsInMemory(this DatabaseFacade database)
        {
            return string.Equals(database.ProviderName, InMemoryDatabaseProvider, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}

using System;

namespace Tellma.Repository.Common.Tests
{
    public static class EntitySources
    {
        public static string Sources(Type t)
        {
            return t.Name switch
            {
                nameof(TestEntity) => "[map].[TestEntities]()", // Fake
                _ => throw new InvalidOperationException($"The requested type {t.Name} is not supported in test queries."),
            };
        }
    }
}

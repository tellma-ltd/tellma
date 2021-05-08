using System.Collections.Generic;

namespace Tellma.Utilities.Sharding
{
    public class ShardResolverOptions
    {
        /// <summary>
        /// How many minutes to keep the connection string in the cache.
        /// </summary>
        public int? ShardResolverCacheExpirationMinutes { get; set; }

        /// <summary>
        /// Dictionary mapping keywords to tenant sql server passwords, this way passwords
        /// can be stored in a secure configuration provider and only the keywords can be
        /// stored in the admin database.
        /// </summary>
        public Dictionary<string, string> Passwords { get; set; }
    }
}

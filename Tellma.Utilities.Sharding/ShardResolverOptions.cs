namespace Tellma.Utilities.Sharding
{    
     /// <summary>
     /// Contains options for the sharding infrastructure.
     /// </summary>
    public class ShardResolverOptions
    {
        /// <summary>
        /// How many minutes to keep the connection string in the cache.
        /// </summary>
        public int? ShardResolverCacheExpirationMinutes { get; set; }
    }
}

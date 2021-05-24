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

        /// <summary>
        /// Value in seconds of all application databases connection string ConnectTimeout property
        /// </summary>
        public int? ConnectionStringsTimeoutInSeconds { get; set; }
    }
}

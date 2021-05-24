using System.Collections.Generic;

namespace Tellma.Services.Sharding
{
    public class ShardResolverOptions
    {
        public int? ShardResolverCacheExpirationMinutes { get; set; }
        public Dictionary<string, string> Passwords { get; set; }
    }
}
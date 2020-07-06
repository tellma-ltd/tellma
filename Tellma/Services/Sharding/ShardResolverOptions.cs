using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Services.Sharding
{
    public class ShardResolverOptions
    {
        public int? ShardResolverCacheExpirationMinutes { get; set; }
        public Dictionary<string, string> Passwords { get; set; }
    }
}

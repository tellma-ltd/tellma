using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.Sharding
{
    public interface IShardResolver
    {
        string GetConnectionString();
    }
}

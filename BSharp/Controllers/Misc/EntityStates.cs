using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Misc
{
    public static class EntityStates
    {
        public static readonly string Inserted = nameof(Inserted);
        public static readonly string Updated = nameof(Updated);
        public static readonly string Deleted = nameof(Deleted);
    }
}

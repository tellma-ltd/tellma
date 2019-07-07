using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.OData
{
    public static class Ops
    {
        public const string gt = nameof(gt);
        public const string ge = nameof(ge);
        public const string lt = nameof(lt);
        public const string le = nameof(le);
        public const string eq = nameof(eq);
        public const string ne = nameof(ne);
        public const string contains = nameof(contains);
        public const string ncontains = nameof(ncontains);
        public const string startsw = nameof(startsw);
        public const string nstartsw = nameof(nstartsw);
        public const string endsw = nameof(endsw);
        public const string nendsw = nameof(nendsw);
        public const string childof = nameof(childof);
        public const string descof = nameof(descof);
    }
}

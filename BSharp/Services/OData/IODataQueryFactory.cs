using BSharp.Controllers.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.OData
{
    public interface IODataQueryFactory
    {
        ODataQuery<T, TKey> MakeODataQuery<T, TKey>(DbConnection conn, Func<Type, string> sources) where T : DtoKeyBase<TKey>;
    }
}

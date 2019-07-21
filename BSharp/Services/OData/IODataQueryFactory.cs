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
        ODataQuery<T> MakeODataQuery<T>(DbConnection conn, Func<Type, string> sources) where T : DtoBase;
        ODataAggregateQuery<T> MakeODataAggregateQuery<T>(DbConnection conn, Func<Type, string> sources) where T : DtoBase;
    }
}

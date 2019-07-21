using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.OData
{
    public interface IQueryInternal
    {
        ODataQueryInternal PrincipalQuery { get; set; } // Need to populate these 3 properties and use them below

        string ForeignKeyToPrincipalQuery { get; set; }

        ArraySegment<string> PathToCollectionPropertyInPrincipal { get; set; }

        Type ResultType { get; set; }

        bool IsAncestorExpand { get; set; }

        SqlStatement PrepareStatement(
            Func<Type, string> sources,
            SqlStatementParameters ps,
            int currentUserId,
            TimeZoneInfo currentUserTimeZone);
    }
}

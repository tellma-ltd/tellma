using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data.Queries
{
    /// <summary>
    /// Interface for both <see cref="QueryInternal"/> internal and <see cref="AggregateQueryInternal"/>
    /// </summary>
    public interface IQueryInternal
    {
        /// <summary>
        /// If this query A is to retrieve the navigation collection of another query B, then B is the principal query of A
        /// </summary>
        QueryInternal PrincipalQuery { get; set; } // Need to populate these 3 properties and use them below

        /// <summary>
        /// Every navigation collection will have a foreign key in the child collection pointing to the parents, this is the name of that foreign key
        /// </summary>
        string ForeignKeyToPrincipalQuery { get; set; }

        /// <summary>
        /// The path to the collection navigation property in the principal query
        /// </summary>
        ArraySegment<string> PathToCollectionPropertyInPrincipal { get; set; }

        /// <summary>
        /// The type of the result of this query
        /// </summary>
        Type ResultType { get; set; }

        /// <summary>
        /// True if this query is to retrieve all the ancestors of some tree entities
        /// </summary>
        bool IsAncestorExpand { get; set; }

        /// <summary>
        /// Prepares the <see cref="SqlStatement"/> containing all the information needed to execute
        /// the query as SQL, load the results and connect them with results from other queries
        /// </summary>
        SqlStatement PrepareStatement(
            Func<Type, string> sources,
            SqlStatementParameters ps,
            int currentUserId,
            TimeZoneInfo currentUserTimeZone);
    }
}

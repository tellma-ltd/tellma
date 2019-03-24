using System;

namespace BSharp.Data.DbModel
{
    /// <summary>
    /// A Query type https://docs.microsoft.com/en-us/ef/core/modeling/query-types
    /// Used for returning a flat list of GUIDs
    /// </summary>
    public class DbGuid
    {
        public Guid Value { get; set; }
    }
}

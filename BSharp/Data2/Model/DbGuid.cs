using System;

namespace BSharp.Data.Model
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

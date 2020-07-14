using System;

namespace Tellma.Entities
{
    /// <summary>
    /// Indicates a property that corresponds to a non-nullable column in the SQL table, useful for using the more performant INNER JOIN when querying data
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotNullAttribute : Attribute
    {
    }
}

using System;

namespace Tellma.Entities
{
    /// <summary>
    /// For properties that map to NOT NULL columns in the DB, allows for more optimized SQL query generation.
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NotNullAttribute : Attribute
    {
    }
}

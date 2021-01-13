using System;

namespace Tellma.Entities
{
    /// <summary>
    /// Properties of type <see cref="DateTime"/> are assumed to map to an SQL column of type DATE, 
    /// unless they are adorned with this attribute then they are mapped to a DATETIME.
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class IncludesTimeAttribute : Attribute
    {
    }
}

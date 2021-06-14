using System;

namespace Tellma.Data
{
    /// <summary>
    /// Indicates that the requested database delete operation will violate a foreign key constraint
    /// </summary>
    public class ForeignKeyViolationException : Exception
    {
    }
}

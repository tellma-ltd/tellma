using System;

namespace Tellma.Repository.Common
{
    /// <summary>.
    /// The exception that is thrown when the requested database delete operation violates a foreign key constraint.
    /// </summary>
    public class ForeignKeyViolationException : Exception
    {
    }
}

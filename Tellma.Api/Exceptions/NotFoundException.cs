using System;
using System.Collections.Generic;

namespace Tellma.Api
{
    /// <summary>
    /// Exception that signifies that the requested entity/entities was not found,
    /// web controllers should translate it to a status code 404 Not Found.
    /// </summary>
    public class NotFoundException<TKey> : Exception
    {
        public NotFoundException(IEnumerable<TKey> ids)
        {
            Ids = ids;
        }

        public NotFoundException(TKey id) : this(new List<TKey> { id })
        {
        }

        /// <summary>
        /// The resource keys that were not found.
        /// </summary>
        public IEnumerable<TKey> Ids { get; private set; }
    }
}

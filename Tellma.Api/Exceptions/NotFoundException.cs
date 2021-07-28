using System;
using System.Collections.Generic;

namespace Tellma.Api
{
    /// <summary>
    /// Exception that signifies that the requested entity/entities was not found,
    /// web controllers should translate it to a status code 404 Not Found.
    /// </summary>
    public class NotFoundException<TKey> : NotFoundException
    {
        public NotFoundException(IEnumerable<TKey> ids)
        {
            Ids = ids ?? throw new ArgumentNullException(nameof(ids));
        }

        public NotFoundException(TKey id) : this(new List<TKey> { id })
        {
        }

        /// <summary>
        /// The resource keys that were not found.
        /// </summary>
        public IEnumerable<TKey> Ids { get; private set; }

        public override IEnumerable<object> GetIds()
        {
            foreach (var id in Ids)
            {
                yield return id;
            }
        }
    }

    /// <summary>
    /// Generic base class for <see cref="NotFoundException{TKey}"/>.
    /// </summary>
    public abstract class NotFoundException : Exception
    {
        /// <summary>
        /// Returns the Ids as objects.
        /// </summary>
        public abstract IEnumerable<object> GetIds();
    }
}

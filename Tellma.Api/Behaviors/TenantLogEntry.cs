using System;
using System.Collections.Generic;

namespace Tellma.Api.Behaviors
{
    /// <summary>
    /// Represents an event that the tenant administrator would like to know about.
    /// </summary>
    public abstract class TenantLogEntry
    {
        public TenantLogEntry(TenantLogLevel level)
        {
            Level = level;
        }

        /// <summary>
        /// A unique idenifier of the log entry.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Log level (Error, Warning, etc...)
        /// </summary>
        public virtual TenantLogLevel Level { get; private set; }

        /// <summary>
        /// The tenant ID where the event occurred
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// The name of the tenant
        /// </summary>
        public string TenantName { get; set; }

        /// <summary>
        /// The support emails configured for this tenant
        /// </summary>
        public IEnumerable<string> TenantSupportEmails { get; set; }
    }
}

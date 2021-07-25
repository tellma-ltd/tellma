using System;

namespace Tellma.Api.Dto
{
    /// <summary>
    /// Base class for all status update DTOs that pertain to a specific tenant.
    /// </summary>
    public abstract class TenantStatusToSend
    {
        /// <summary>
        /// The time on the server when this event was generated,
        /// used to resolve race conditions on the client.
        /// </summary>
        public DateTimeOffset ServerTime { get; set; }

        /// <summary>
        /// The Tenant Id associated with this server notification.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// The external user Id.
        /// </summary>
        public string ExternalId { get; set; }
    }
}

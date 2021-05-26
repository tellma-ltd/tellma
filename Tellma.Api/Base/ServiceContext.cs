using System;
using System.Threading;

namespace Tellma.Api
{
    /// <summary>
    /// Contains identity and contextual information needed by services to execute requests.
    /// </summary>
    public class ServiceContext
    {
        /// <summary>
        /// Creates an instance of <see cref="ServiceContext"/>.
        /// </summary>
        /// <param name="externalUserId">The external Id of the authenticated user on behalf of which the service is used.</param>
        /// <param name="externalEmail">The external email of the authenticated user on behalf of which the service is used.</param>
        /// <param name="tenantId">The id of the database accessed by the request.</param>
        /// <param name="definitionId">The definition Id of the principal entity of the service.</param>
        /// <param name="today">The current date at the client's time zone.</param>
        /// <param name="cancellation">The cancellation instruction for the service request.</param>
        public ServiceContext(
            string externalUserId, 
            string externalEmail, 
            int? tenantId = null, 
            int? definitionId = null, 
            DateTime today = default, 
            CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                throw new ArgumentException($"'{nameof(externalUserId)}' cannot be null or whitespace.", nameof(externalUserId));
            }

            if (string.IsNullOrWhiteSpace(externalEmail))
            {
                throw new ArgumentException($"'{nameof(externalEmail)}' cannot be null or whitespace.", nameof(externalEmail));
            }

            // Make sure today is within a reasonable range
            today = today.Date;
            if (today < DateTime.Today.AddDays(-1) || today > DateTime.Today.AddDays(1))
            {
                today = DateTime.Today;
            }

            ExternalUserId = externalUserId;
            ExternalEmail = externalEmail;
            TenantId = tenantId;
            DefinitionId = definitionId;
            Today = today;
            Cancellation = cancellation;
        }

        /// <summary>
        /// The external Id of the authenticated user on behalf of which the service is used.
        /// </summary>
        public string ExternalUserId { get; }

        /// <summary>
        /// The external email of the authenticated user on behalf of which the service is used.
        /// </summary>
        public string ExternalEmail { get; }

        /// <summary>
        /// The id of the database accessed by the request.
        /// </summary>
        public int? TenantId { get; }

        /// <summary>
        /// The definition Id of the principal entity of the service.
        /// </summary>
        public int? DefinitionId { get; set; }

        /// <summary>
        /// The current date at the client's time zone.
        /// </summary>
        public DateTime Today { get; set; }

        /// <summary>
        /// The cancellation instruction for the service request.
        /// </summary>
        public CancellationToken Cancellation { get; set; }
    }
}

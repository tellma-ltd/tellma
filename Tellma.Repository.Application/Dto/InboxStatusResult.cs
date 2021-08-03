using System.Collections.Generic;
using Tellma.Repository.Common;

namespace Tellma.Repository.Application
{
    /// <summary>
    /// Base class for all results of operations that affect the statuses of users' inboxes. <br/>
    /// This object contains: <br/>
    ///  - Errors. <br/>
    ///  - InboxStatuses. <br/>
    /// </summary>
    public class InboxStatusResult : OperationResult
    {
        public InboxStatusResult(IEnumerable<ValidationError> errors, IEnumerable<InboxStatus> inboxStatuses) : base(errors)
        {
            InboxStatuses = inboxStatuses;
        }

        /// <summary>
        /// Every inbox that was affected by this transaction.
        /// </summary>
        public IEnumerable<InboxStatus> InboxStatuses { get; }
    }

    public class InboxStatus
    {
        public InboxStatus(string externalId, int count, int unknownCount)
        {
            ExternalId = externalId;
            Count = count;
            UnknownCount = unknownCount;
        }

        /// <summary>
        /// The external Id of the user who owns the inbox.
        /// </summary>
        public string ExternalId { get; }

        /// <summary>
        /// The current count of the documents in the inbox.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// The current count of the documents in the inbox since the last user check.
        /// </summary>
        public int UnknownCount { get; }
    }
}


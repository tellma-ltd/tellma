using System.Collections.Generic;
using Tellma.Model.Application;
using Tellma.Repository.Common;

namespace Tellma.Repository.Application
{
    public class AssignOutput : InboxStatusOutput
    {
        public AssignOutput(IEnumerable<ValidationError> errors, IEnumerable<InboxStatus> inboxStatuses, User assigneeInfo, int docSerial) : base(errors, inboxStatuses)
        {
            AssigneeInfo = assigneeInfo;
            DocumentSerial = docSerial;
        }

        /// <summary>
        /// Information of the user to whom the document was assigned, 
        /// in order to create a personalized notification.
        /// </summary>
        public User AssigneeInfo { get; }

        /// <summary>
        /// The serial number of the first assigned document.
        /// </summary>
        public int DocumentSerial { get; }
    }
}


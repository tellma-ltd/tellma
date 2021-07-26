using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Repository.Application;

namespace Tellma.Api
{
    /// <summary>
    /// The implementation allows the API to: <br/>
    /// 1 - Send real-time notifications to the client app. <br/>
    /// 2 - (Optional) Send SMS messages containing links to the client app. <br/>
    /// 3 - (Optional) Send emails branded and themed like the client app and containing links to the client app. <br/>
    /// </summary>
    /// <remarks>An implementation of <see cref="IClientProxy"/> is requird in order to use the API.</remarks>
    public interface IClientProxy
    {
        /// <summary>
        /// True if email is enabled in this installation, false otherwise.
        /// </summary>
        public bool EmailEnabled { get; }

        /// <summary>
        /// True if SMS is enabled in this installation, false otherwise.
        /// </summary>
        public bool SmsEnabled { get; }

        /// <summary>
        /// Sends a test email to the given email address.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant performing the test.</param>
        /// <param name="emailAddress">The email address to test.</param>
        /// <returns>The subject of the email.</returns>
        public Task<string> TestEmailAddress(int tenantId, string emailAddress);

        /// <summary>
        /// Sends a test SMS message to the given phone number.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant performing the test.</param>
        /// <param name="phoneNumber">The phone number to test.</param>
        /// <returns>The body of the test SMS.</returns>
        public Task<string> TestPhoneNumber(int tenantId, string phoneNumber);

        /// <summary>
        /// Send invitation email containing a link to the company main menu on the client.
        /// </summary>
        /// <param name="tenantId">The Id of the inviting company.</param>
        /// <param name="infos">The information of the invited users.</param>
        /// <returns>The asynchronous operation.</returns>
        public Task InviteConfirmedUsersToTenant(int tenantId, IEnumerable<ConfirmedEmailInvitation> infos);

        /// <summary>
        /// Send invitation email containing an email confirmation and password reset
        /// links and a return url to the company main menu on the client.
        /// </summary>
        /// <param name="tenantId">The Id of the inviting company.</param>
        /// <param name="infos">The information of the invited users.</param>
        /// <returns>The asynchronous operation.</returns>
        public Task InviteUnconfirmedUsersToTenant(int tenantId, IEnumerable<UnconfirmedEmailInvitation> infos);

        /// <summary>
        /// Send a real-time status update to the client app indicating that the inbox has changed.
        /// </summary>
        /// <param name="tenantId">The Id of the company where the inbox has changed.</param>
        /// <param name="statuses">The changed inboxes.</param>
        /// <param name="updateInboxList">Instructs the client to refresh the inbox records.</param>
        public void UpdateInboxStatuses(int tenantId, IEnumerable<InboxStatus> statuses, bool updateInboxList = true);

        /// <summary>
        /// Send email and SMS notifications to the user that they have a new documents in their inbox.
        /// </summary>
        /// <param name="tenantId">The Id of the company where the assignment happened.</param>
        /// <param name="args">All the information needed to dispatch the assignment notifications.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task NotifyDocumentsAssignment(int tenantId, NotifyDocumentAssignmentArguments args);
    }
}

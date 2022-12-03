using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Api.Notifications;
using Tellma.Repository.Application;
using Tellma.Utilities.Email;
using Tellma.Utilities.Sms;

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
        #region User Invitation

        /// <summary>
        /// Create an invitation email containing a link to the company main menu on the client.
        /// </summary>
        /// <param name="tenantId">The Id of the inviting company.</param>
        /// <param name="infos">The information of the invited users.</param>
        /// <returns>The email to send.</returns>
        IEnumerable<EmailToSend> MakeEmailsForConfirmedUsers(int tenantId, IEnumerable<ConfirmedEmailInvitation> infos);

        /// <summary>
        /// Create an invitation email containing an email confirmation and password reset
        /// links and a return url to the company main menu on the client.
        /// </summary>
        /// <param name="tenantId">The Id of the inviting company.</param>
        /// <param name="infos">The information of the invited users.</param>
        /// <returns>The email to send.</returns>
        IEnumerable<EmailToSend> MakeEmailsForUnconfirmedUsers(int tenantId, IEnumerable<UnconfirmedEmailInvitation> infos);

        /// <summary>
        /// Create an invitation email containing a link to the admin console.
        /// </summary>
        /// <param name="infos">The information of the invited admin users.</param>
        /// <returns>The email to send.</returns>
        IEnumerable<EmailToSend> MakeEmailsForConfirmedAdminUsers(IEnumerable<ConfirmedAdminEmailInvitation> infos);

        /// <summary>
        /// Create an invitation email containing an email confirmation and password reset
        /// links and a return url to the admin console.
        /// </summary>
        /// <param name="infos">The information of the invited admin users.</param>
        /// <returns>The email to send.</returns>
        IEnumerable<EmailToSend> MakeEmailsForUnconfirmedAdminUsers(IEnumerable<UnconfirmedAdminEmailInvitation> infos);

        #endregion

        #region Document Assignments

        /// <summary>
        /// Send a real-time status update to the client apps indicating that the inbox statuses 
        /// (count, unknown count) have changed.
        /// </summary>
        /// <param name="tenantId">The Id of the company where the inbox has changed.</param>
        /// <param name="statuses">The changed inboxes.</param>
        /// <param name="updateInboxList">Instructs the client to refresh the inbox records.</param>
        public void UpdateInboxStatuses(int tenantId, IEnumerable<InboxStatus> statuses, bool updateInboxList = true);

        /// <summary>
        /// Creates an <see cref="EmailToSend"/> notifying the user about new documents in their inbox.
        /// </summary>
        /// <param name="tenantId">The Id of the company where the document assignment happened.</param>
        /// <param name="args">All the information needed to create the <see cref="EmailToSend"/>.</param>
        public EmailToSend MakeDocumentAssignmentEmail(int tenantId, string contactEmail, NotifyDocumentAssignmentArguments args);

        /// <summary>
        /// Creates an <see cref="SmsToSend"/> notifying the user about new documents in their inbox.
        /// </summary>
        /// <param name="tenantId">The Id of the company where the document assignment happened.</param>
        /// <param name="args">All the information needed to create the <see cref="SmsToSend"/>.</param>
        public SmsToSend MakeDocumentAssignmentSms(int tenantId, string contactMobile, NotifyDocumentAssignmentArguments args);

        /// <summary>
        /// Creates an <see cref="PushToSend"/> notifying the user about new documents in their inbox.
        /// </summary>
        /// <param name="tenantId">The Id of the company where the document assignment happened.</param>
        /// <param name="args">All the information needed to create the <see cref="PushToSend"/>.</param>
        public PushToSend MakeDocumentAssignmentPush(int tenantId, NotifyDocumentAssignmentArguments args);

        #endregion

        #region Custom Script Logging

        /// <summary>
        /// Create an email that notifies the tenant admin about an error in 
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public EmailToSend MakeTenantNotificationEmail(TenantLogEntry ex);

        #endregion
    }
}

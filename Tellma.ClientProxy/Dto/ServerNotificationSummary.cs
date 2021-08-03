namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// When a client connects for the first time, or re-connects after an Internet disconnection,
    /// it needs a summary of all the information that it missed out on, this information is delivered
    /// in this DTO.
    /// </summary>
    public class ServerNotificationSummary
    {
        public InboxStatusToSend Inbox { get; set; }
    }
}

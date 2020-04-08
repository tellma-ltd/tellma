namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// When the number of user notifications changes, or the known number of thereof
    /// </summary>
    public class NotificationsNotification : TenantNotification
    {
        public int Count { get; set; }
        public int UnknownCount { get; set; }
    }
}

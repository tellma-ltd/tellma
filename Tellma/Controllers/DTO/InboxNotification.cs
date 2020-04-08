using Tellma.Entities;

namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// When the number of user assigned <see cref="Document"/>s changes, or the known number thereof
    /// </summary>
    public class InboxNotification : TenantNotification
    {
        public int Count { get; set; }
        public int UnknownCount { get; set; }
        public bool UpdateInboxList { get; set; }
    }
}

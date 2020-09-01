using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Entities
{
    public class NotificationSms : EntityWithKey<int>
    {
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public int? State { get; set; }
        public DateTimeOffset? StateAt { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}

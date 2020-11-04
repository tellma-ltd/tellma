using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Entities
{
    public class NotificationEmail : EntityWithKey<int>
    {
        public string ToEmail { get; set; } // Required
        public string FromEmail { get; set; } // Nullable
        public string Subject { get; set; } // Nullable
        public string Body { get; set; } // Required, should we delete this once Status = Opened
        public int? Status { get; set; } // Required
        public DateTimeOffset? StatusAt { get; set; } // Required
        public DateTimeOffset? CreatedAt { get; set; } // Required
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Controllers.Jobs
{
    public class EmailQueue : BackgroundQueue<EmailQueueItem>
    {
    }
}

using Tellma.Services.Sms;

namespace Tellma.Controllers.Jobs
{
    public class SmsQueue : BackgroundQueue<SmsMessage>
    {
    }
}

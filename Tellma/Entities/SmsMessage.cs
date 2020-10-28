using System.ComponentModel.DataAnnotations;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "SmsMessage", Plural = "SmsMessages")]
    public class SmsMessageForSave : EntityWithKey<int>
    {
        [Display(Name = "SmsMessage_ToPhoneNumber")]
        public string ToPhoneNumber { get; set; }

        [Display(Name = "SmsMessage_Message")]
        public string Message { get; set; }
    }

    public class SmsMessage : SmsMessageForSave
    {

        // TODO: State and timestamps
    }
}

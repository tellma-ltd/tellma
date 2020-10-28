//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Tellma.Entities;
//using Tellma.Services.Sms;

//namespace Tellma.Controllers.Jobs
//{
//    public class SmsQueueItem
//    {
//        /// <summary>
//        /// The phone number to send the SMS to
//        /// </summary>
//        public string ToPhoneNumber { get; set; }

//        /// <summary>
//        /// The content of the SMS
//        /// </summary>
//        public string Message { get; set; }

//        /// <summary>
//        /// The message id in the tenant database
//        /// </summary>
//        public int MessageId { get; set; }

//        /// <summary>
//        /// The Id of the tenant where the message is stored
//        /// </summary>
//        public int TenantId { get; set; }

//        public static Sms ToForSender(SmsQueueItem e)
//        {
//            return new Sms(e.ToPhoneNumber, e.Message)
//            {
//                MessageId = e.MessageId,
//                TenantId = e.TenantId,
//            };
//        }
//    }
//}

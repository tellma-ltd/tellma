using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Services.Email
{
    public class Email
    {
        public Email(string toEmail)
        {
            ToEmail = toEmail;
        }

        /// <summary>
        /// The email address to send the email to
        /// </summary>
        public string ToEmail { get; }

        /// <summary>
        /// The subject (title) of the email
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The content of the email
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The email id in the tenant database or 0 if there is no tenant ID
        /// </summary>
        public int EmailId { get; set; }

        /// <summary>
        /// The Id of the tenant where the email is stored or 0 if there is no tenant ID
        /// </summary>
        public int TenantId { get; set; }
    }
}

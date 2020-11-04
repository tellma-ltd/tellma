using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Services.EmailLogger
{
    public class EmailLoggerOptions
    {
        /// <summary>
        /// The email to send to when an error happens
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// In case the same email receives errors from multiple instances
        /// </summary>
        public string InstanceIdentifier { get; set; }
    }
}

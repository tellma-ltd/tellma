using System;

namespace Tellma.Services.ClientInfo
{
    /// <summary>
    /// A class containing information about the client, extracted from the client request.
    /// IMPORTANT: None of the information contained here can be trusted for anything security-sensitive
    /// </summary>
    public class ClientInfo
    {
        /// <summary>
        /// The current date at the client's time zone
        /// </summary>
        public DateTime? Today { get; set; }
    }
}

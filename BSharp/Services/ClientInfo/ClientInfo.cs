using System;

namespace BSharp.Services.ClientInfo
{
    /// <summary>
    /// A class containing information about the client, extracted from the client request.
    /// IMPORTANT: None of the information contained here can be trusted for anything security-sensitive
    /// </summary>
    public class ClientInfo
    {
        /// <summary>
        /// The time zone of the calling client
        /// </summary>
        public TimeZoneInfo TimeZone { get; set; }
    }
}

using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// Binds to the all the configuratiosn needed by the client store of the embedded instance of 
    /// IdentityServer this is following the options pattern recommended by microsoft: https://bit.ly/2GiJ19F
    /// </summary>
    public class ClientStoreConfiguration
    {
        public string WebClientUri { get; set; }
        public string MobileClientUri { get; set; }
    }
}

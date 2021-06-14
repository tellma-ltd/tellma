using Tellma.Controllers;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Tellma.Services.ClientInfo
{
    public class ClientInfoAccessor : IClientInfoAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientInfoAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClientInfo GetInfo()
        {
            // Get today at the client time zone
            DateTime? userToday = null;
            {
                var suppliedString = _httpContextAccessor
                    .HttpContext.Request.Headers["X-Today"].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(suppliedString))
                {
                    // Parse the header value
                    DateTime suppliedDate;
                    try
                    {
                        suppliedDate = DateTime.Parse(suppliedString);
                    }
                    catch
                    {
                        throw new BadRequestException($"The value '{suppliedString}' of the header X-Today could not be parsed into a valid date");
                    }

                    userToday = suppliedDate;
                }
            }

            string calendar = _httpContextAccessor
                    .HttpContext.Request.Headers["X-Calendar"].FirstOrDefault();

            // Return client info
            return new ClientInfo
            {
                Today = userToday,
                Calendar = calendar,
            };
        }
    }
}

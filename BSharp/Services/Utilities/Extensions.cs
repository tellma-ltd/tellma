using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Security.Claims;

namespace BSharp.Services.Utilities
{
    public static class Extensions
    {
        /// <summary>
        /// Checks whether a certain type has a certain property name defined
        /// </summary>
        public static bool HasProperty(this Type type, string propertyName)
        {
            return type.GetProperty(propertyName) != null;
        }

        /// <summary>
        /// Retrieves the username of the authenticated claims principal
        /// </summary>
        public static string UserId(this ClaimsPrincipal user)
        {
            return "4F7785F2-5942-4CFB-B5AD-85AB72F7EB35"; // TODO
        }

        /// <summary>
        /// Extracts all errors inside an IdentityResult and concatenates them together, 
        /// falling back to a default message if no errors were found in the IdentityResult object
        /// </summary>
        public static string ErrorMessage(this IdentityResult result, string defaultMessage)
        {
            string errorMessage = defaultMessage;
            if (result.Errors.Any())
                errorMessage = string.Join(" ", result.Errors.Select(e => e.Description));

            return errorMessage;
        }
    }
}

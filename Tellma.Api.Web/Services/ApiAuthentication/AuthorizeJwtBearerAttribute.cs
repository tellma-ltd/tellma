using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Tellma.Services.ApiAuthentication
{
    /// <summary>
    /// Since the default Authentication scheme is already taken by the embedded instance of identityserver
    /// we have to use a different scheme which is <see cref="JwtBearerDefaults.AuthenticationScheme"/>, this
    /// attribute makes it less tedious to annotate controllers with this scheme.
    /// </summary>
    public class AuthorizeJwtBearerAttribute : AuthorizeAttribute
    {
        public AuthorizeJwtBearerAttribute() : base()
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
        }

        public AuthorizeJwtBearerAttribute(string policy) : base(policy)
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
        }
    }
}

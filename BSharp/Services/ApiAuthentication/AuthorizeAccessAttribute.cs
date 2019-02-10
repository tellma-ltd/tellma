using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace BSharp.Services.ApiAuthentication
{
    /// <summary>
    /// Since the default Authentication scheme is already taken by the embedded instance of identityserver
    /// we have to use a different scheme which is <see cref="JwtBearerDefaults.AuthenticationScheme"/>, this
    /// attribute makes it less tedious to annotate controllers with this authentication scheme
    /// </summary>
    public class AuthorizeAccessAttribute : AuthorizeAttribute
    {
        public AuthorizeAccessAttribute() : base()
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
        }

        public AuthorizeAccessAttribute(string policy) : base(policy)
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
        }
    }
}

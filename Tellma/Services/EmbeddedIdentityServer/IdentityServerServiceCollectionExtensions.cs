using Tellma.Services.EmbeddedIdentityServer;
using Tellma.Services.Utilities;
using IdentityModel;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServerServiceCollectionExtensions
    {
        /// <summary>
        /// For small and simple installations of the system, it would be too tedious to setup a separate identity server
        /// by enabling this feature through the options, the system runs an embedded instance of Identity server that 
        /// only authenticates a single web client and a single mobile client, the technician need only provide a valid
        /// signing certificate in the environment and set its thumbprint in a configuration provider
        /// </summary>
        public static IServiceCollection AddEmbeddedIdentityServer(this IServiceCollection services,
            IConfiguration configSection, IConfiguration clientsConfigSection, IMvcBuilder mvcBuilder, bool isDevelopment)
        {
            // basic sanity checks
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configSection == null)
            {
                throw new ArgumentNullException(nameof(configSection));
            }

            // Extract the configuration section of Identity Server into a strongly typed object that is easier to deal with
            services.Configure<EmbeddedIdentityServerOptions>(configSection);
            var config = configSection.Get<EmbeddedIdentityServerOptions>();

            // Register the identity context
            string connString = config?.ConnectionString ?? throw new InvalidOperationException("To enable the embedded IdentityServer, the connection string to the database of IdentityServer must be specified in a configuration provider");
            services.AddDbContext<EmbeddedIdentityServerContext>(opt =>
                    opt.UseSqlServer(connString));

            // Setup the identity options (password requirements, lockout, etc)
            services.Configure<IdentityOptions>(configSection);

            // Required dependency
            services.AddClientAppAddressResolver();

            // Increase the default email and password reset tokens lifespan from 1 day to 3 days
            services.Configure<DataProtectionTokenProviderOptions>(opt =>
                    opt.TokenLifespan = TimeSpan.FromDays(Constants.TokenExpiryInDays));

            // Add default identity setup for the embedded IdentityServer instance
            services.AddIdentityCore<EmbeddedIdentityServerUser>(opt =>
            {
                opt.SignIn.RequireConfirmedEmail = true;
                opt.User.RequireUniqueEmail = true;
            })
                .AddSignInManager()
                .AddErrorDescriber<LocalizedIdentityErrorDescriptor>()
                .AddDefaultTokenProviders()
                .AddUserStore<ClaimlessUserStore>();

            // Add authentication and cookie schemes (the section below is copied from GitHub: https://bit.ly/2W8GXaN)
            var authBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddCookie(IdentityConstants.ApplicationScheme, o =>
            {
                o.LoginPath = new PathString("/Account/Login");
                o.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
                };
            })
            .AddCookie(IdentityConstants.ExternalScheme, o =>
            {
                o.Cookie.Name = IdentityConstants.ExternalScheme;
                o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddCookie(IdentityConstants.TwoFactorRememberMeScheme, o =>
            {
                o.Cookie.Name = IdentityConstants.TwoFactorRememberMeScheme;
                o.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = SecurityStampValidator.ValidateAsync<ITwoFactorSecurityStampValidator>
                };
            })
            .AddCookie(IdentityConstants.TwoFactorUserIdScheme, o =>
            {
                o.Cookie.Name = IdentityConstants.TwoFactorUserIdScheme;
                o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            });

            // add external providers
            if (config?.Google != null && config.Google.ClientId != null)
            {
                authBuilder.AddGoogle("Google", "Google", opt =>
                {
                    opt.ClientId = config.Google.ClientId;
                    opt.ClientSecret = config.Google.ClientSecret;
                });
            }

            if (config?.Microsoft != null && config.Microsoft.ClientId != null)
            {
                authBuilder.AddMicrosoftAccount("Microsoft", "Microsoft", opt =>
                {
                    opt.ClientId = config.Microsoft.ClientId;
                    opt.ClientSecret = config.Microsoft.ClientSecret;
                });
            }

            // For windows authentication
            services.Configure<IISOptions>(opt =>
            {
                opt.AuthenticationDisplayName = "Windows";
                opt.AutomaticAuthentication = false;
            });

            // Add identity server
            services.Configure<ClientApplicationsOptions>(clientsConfigSection);

            var builder = services.AddIdentityServer(opt =>
            {
                opt.UserInteraction.LoginUrl = "/identity/sign-in";
                opt.UserInteraction.LogoutUrl = "/identity/sign-out";
                opt.UserInteraction.ErrorUrl = "/server-error";
            })
                .AddInMemoryIdentityResources(GetIdentityResources())
                .AddInMemoryApiScopes(GetApiScopes())

                // This one uses the ClientsConfiguration configured earlier
                .AddClientStore<DefaultsToSameOriginClientStore>()
                .AddAspNetIdentity<EmbeddedIdentityServerUser>();

            // Add signing credentials
            if (isDevelopment)
            {
                // Not secure, good for development only
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                var certThumbprint = config?.X509Certificate2Thumbprint ??
                    throw new Exception("To enable the embedded IdentityServer in production, a valid X509 certificate thumbprint must be specified in a configuration provider");

                using X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);

                certStore.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = certStore.Certificates.Find(
                                           X509FindType.FindByThumbprint, certThumbprint, validOnly: false);

                // Get the first cert with the thumbprint
                if (certCollection.Count > 0)
                {
                    X509Certificate2 cert = certCollection[0];
                    builder.AddSigningCredential(cert);
                }
                else
                {
                    throw new Exception($"The specified X509 certificate thumbprint '{certThumbprint}' was not found");
                }
            }

            // Configure cookie authentication for the embedded identity server
            services.ConfigureApplicationCookie(opt =>
            {
                opt.ExpireTimeSpan = TimeSpan.FromDays(config.CookieSessionLifetimeInDays);
                opt.SlidingExpiration = true;
                opt.LoginPath = $"/identity/sign-in";
                opt.LogoutPath = $"/identity/sign-out";
                opt.AccessDeniedPath = $"/identity/access-denied";
            });

            // Add the Identity Server web pages (sign-in, change password, etc...)
            mvcBuilder.AddViewLocalization()
                    .AddRazorPagesOptions(opt =>
            {
                opt.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                opt.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
            });

            return services;
        }

        /// <summary>
        /// Embedded IdentityServer middleware
        /// </summary>
        public static IApplicationBuilder UseEmbeddedIdentityServer(this IApplicationBuilder app)
        {
            return app.UseIdentityServer();
        }

        /// <summary>
        /// The identity resources supported by the embedded IdentityServer instance
        /// </summary>
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            yield return new IdentityResources.OpenId();
            yield return new IdentityResources.Email();
            yield return new IdentityResources.Profile();
        }

        /// <summary>
        /// The API resources supported by the embedded IdentityServer instance
        /// </summary>
        public static IEnumerable<ApiScope> GetApiScopes()
        {
            yield return new ApiScope(Constants.ApiResourceName)
            {
                UserClaims = { JwtClaimTypes.Email, JwtClaimTypes.EmailVerified }
            };
        }
    }
}

using Tellma.Services.EmbeddedIdentityServer;
using Tellma.Services.Utilities;
using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Tellma.Api;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.DataProtection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServerExtensions
    {
        private const string EmbeddedIdentityServerSection = "EmbeddedIdentityServer";
        private const string ClientApplicationsSection = "ClientApplications";

        /// <summary>
        /// For small and simple installations of the system, it would be too tedious to setup a separate identity server
        /// by enabling this feature through the options, the system runs an embedded instance of Identity server that 
        /// only authenticates a single web client, the technician need only provide a valid signing certificate in the
        /// and set its thumbprint in a configuration provider, as well as a connection string to the identity database
        /// (could be the same as the admin database).
        /// </summary>
        /// <remarks>
        /// This requires implementations of <see cref="IClientProxy"/> to be available in the DI as well the 
        /// <see cref="Tellma.Repository.Admin.AdminRepository"/>.
        /// </remarks>
        public static IServiceCollection AddEmbeddedIdentityServer(this IServiceCollection services,
            IConfiguration config,
            IMvcBuilder mvcBuilder,
            bool isDevelopment)
        {
            // basic sanity checks
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            // Configure options
            var idConfig = config.GetSection(EmbeddedIdentityServerSection);
            var clientAppsConfig = config.GetSection(ClientApplicationsSection);

            services.Configure<EmbeddedIdentityServerOptions>(idConfig); // Certificate thumbprint, db connection string, etc
            services.Configure<IdentityOptions>(idConfig); // password requirements, lockout, etc
            services.Configure<ClientApplicationsOptions>(clientAppsConfig); // URLs

            // Get the identity server options
            var idOptions = idConfig.Get<EmbeddedIdentityServerOptions>();

            // When the Angular client is served from a different origin than this server (the default in
            // development: http://localhost:4200 against https://localhost:5001) the browser treats the two
            // as separate sites - under "schemeful same-site" a differing scheme alone is enough, the port
            // never mattered. OIDC session management and silent token refresh both work by pointing a hidden
            // iframe at this server, and a SameSite=Lax cookie is withheld from those cross-site iframe
            // requests. IdentityServer then sees an anonymous prompt=none request, answers 'login_required',
            // and the client signs the user out a few seconds after sign-in. SameSite=None keeps the session
            // cookies flowing in that case; the browser only honours it on Secure cookies, hence SecurePolicy.
            var webClientIsCrossSite = !config.GetValue<bool>("EmbeddedClientApplicationEnabled");

            // Register the identity context
            var connString = config.GetConnectionString("AdminConnection");
            services.AddAdminRepository(connString);
            services.AddDbContext<EmbeddedIdentityServerContext>(opt => 
                opt.UseSqlServer(connString));

            // Required dependency
            services.AddClientAppAddressResolver(config);

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
            if (!string.IsNullOrWhiteSpace(idOptions.Google?.ClientId))
            {
                authBuilder.AddGoogle("Google", "Google", opt =>
                {
                    opt.ClientId = idOptions.Google.ClientId;
                    opt.ClientSecret = idOptions.Google.ClientSecret;
                });
            }

            if (!string.IsNullOrWhiteSpace(idOptions.Microsoft?.ClientId))
            {
                authBuilder.AddMicrosoftAccount("Microsoft", "Microsoft", opt =>
                {
                    opt.ClientId = idOptions.Microsoft.ClientId;
                    opt.ClientSecret = idOptions.Microsoft.ClientSecret;
                });
            }

            // For windows authentication
            services.Configure<IISOptions>(opt =>
            {
                opt.AuthenticationDisplayName = "Windows";
                opt.AutomaticAuthentication = false;
            });

            // Add identity server
            var builder = services.AddIdentityServer(opt =>
            {
                opt.UserInteraction.LoginUrl = "/identity/sign-in";
                opt.UserInteraction.LogoutUrl = "/identity/sign-out";
                opt.UserInteraction.ErrorUrl = "/server-error";
                opt.KeyManagement.Enabled = false;

                if (webClientIsCrossSite)
                {
                    opt.Authentication.CheckSessionCookieSameSiteMode = SameSiteMode.None;
                }

                if (!string.IsNullOrWhiteSpace(idOptions.Duende.LicenseKey))
                {
                    opt.LicenseKey = idOptions.Duende.LicenseKey;
                }
            })
                .AddInMemoryIdentityResources(GetIdentityResources())
                .AddInMemoryApiScopes(GetApiScopes())

                // This one uses the ClientsConfiguration configured earlier
                .AddClientStore<ClientStore>()
                .AddPersistedGrantStore<PersistedGrantStore>()
                .AddAspNetIdentity<EmbeddedIdentityServerUser>();

            // CORS for identity server requests
            services.AddSingleton<ICorsPolicyService, EmbeddedIdentityCorsPolicyService>();

            // Add signing credentials
            if (isDevelopment)
            {
                // Not secure, good for development only
                builder.AddDeveloperSigningCredential();
            }
            else if (string.IsNullOrWhiteSpace(idOptions.X509Certificate2Thumbprint))
            {
                throw new InvalidOperationException(
                    "To enable the embedded IdentityServer in production, a valid X509 certificate thumbprint must be specified in a configuration provider.");
            }
            else
            {
                var certThumbprint = idOptions.X509Certificate2Thumbprint;

                using X509Store certStore = new(StoreName.My, StoreLocation.CurrentUser);
                certStore.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection certCollection = certStore.Certificates.Find(
                                           X509FindType.FindByThumbprint, certThumbprint, validOnly: false);

                // Get the first cert with the thumbprint
                if (certCollection.Count > 0)
                {
                    X509Certificate2 cert = certCollection[0];
                    builder.AddSigningCredential(cert);

                    // This allows multiple deployment slots (e.g staging and production)
                    // to share the same keys for protecting authentication cookies
                    services.AddDataProtection()
                        .PersistKeysToDbContext<EmbeddedIdentityServerContext>()
                        .ProtectKeysWithCertificate(cert);
                }
                else
                {
                    throw new InvalidOperationException($"The specified X509 certificate thumbprint '{certThumbprint}' was not found.");
                }
            }

            // Configure cookie authentication for the embedded identity server
            services.ConfigureApplicationCookie(opt =>
            {
                opt.ExpireTimeSpan = TimeSpan.FromDays(idOptions.CookieSessionLifetimeInDays);
                opt.SlidingExpiration = true;
                opt.LoginPath = $"/identity/sign-in";
                opt.LogoutPath = $"/identity/sign-out";
                opt.AccessDeniedPath = $"/identity/access-denied";

                if (webClientIsCrossSite)
                {
                    opt.Cookie.SameSite = SameSiteMode.None;
                    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                }
            });

            // Add the Identity Server web pages (sign-in, change password, etc...)
            mvcBuilder.AddViewLocalization()
                    .AddRazorPagesOptions(opt =>
            {
                opt.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                opt.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
            });


            // So the API can talk to the embedded identity server (Scoped because UserManager is scoped
            services.AddScoped<IIdentityProxy, EmbeddedIdentityProxy>();

            // Add the identity service for accessing users and the behavior
            services
                .AddScoped<IdentityServerUsersService>();

            // Clients collection used by ClientStore (and overridden by the integration test project
            services
                .AddSingleton<IUserClientsProvider, DefaultsToSameOriginClientsProvider>();

            return services;
        }

        /// <summary>
        /// Embedded IdentityServer middleware.
        /// </summary>
        public static IApplicationBuilder UseEmbeddedIdentityServer(this IApplicationBuilder app, bool isDevelopment, bool webClientIsCrossSite)
        {
            if (isDevelopment)
            {
                // This allows us to authenticate over HTTP in debug mode. The minimum policy only ever raises
                // a cookie's SameSite, so it would quietly promote the SameSite=None session cookies back to
                // Lax and break silent refresh - leave them alone when the client app is on another site.
                var minimumSameSite = webClientIsCrossSite ? SameSiteMode.Unspecified : SameSiteMode.Lax;
                app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = minimumSameSite });
            }

            return app.UseIdentityServer();
        }

        /// <summary>
        /// The identity resources supported by the embedded IdentityServer instance.
        /// </summary>
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            yield return new IdentityResources.OpenId();
            yield return new IdentityResources.Email();
            yield return new IdentityResources.Profile();
        }

        /// <summary>
        /// The API resources supported by the embedded IdentityServer instance.
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

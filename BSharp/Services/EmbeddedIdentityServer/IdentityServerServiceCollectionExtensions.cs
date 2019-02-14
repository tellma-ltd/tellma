using BSharp.Data;
using BSharp.Data.Model;
using BSharp.Services.EmbeddedIdentityServer;
using BSharp.Services.Utilities;
using IdentityModel;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

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
        public static IServiceCollection AddEmbeddedIdentityServerIfEnabled(
            this IServiceCollection services, IConfiguration config, IHostingEnvironment env)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            // Check that the embedded identity server is enabled
            var embeddedIdentitySection = config.GetSection("EmbeddedIdentityServer");
            var enabled = embeddedIdentitySection["Enabled"];
            if (enabled?.ToLower() != "true")
            {
                return services;
            }

            // Register the identity context
            services.AddDbContext<IdentityContext>(opt =>
                opt.UseSqlServer(config.GetConnectionString(Constants.IdentityConnection)));

            // Setup configurations
            services.Configure<IdentityOptions>(embeddedIdentitySection);

            // Add default identity setup for the embedded IdentityServer instance
            services.AddIdentity<User, IdentityRole>(opt =>
            {
                // When the server is online, the system should require a a valid email
                // opt.SignIn.RequireConfirmedEmail = config["IsOnline"]?.ToLower() == "true";
                opt.SignIn.RequireConfirmedEmail = true;
            })
                .AddDefaultTokenProviders()
                // Use the identity context database
                .AddEntityFrameworkStores<IdentityContext>();


            // For windows authentication
            services.Configure<IISOptions>(opt =>
            {
                opt.AuthenticationDisplayName = "Windows";
                opt.AutomaticAuthentication = false;
            });

            // Add identity server
            services.Configure<ClientStoreConfiguration>(embeddedIdentitySection.GetSection("ClientStore"));
            var builder = services.AddIdentityServer(opt =>
            {
                opt.UserInteraction.LoginUrl = "/identity/sign-in";
                opt.UserInteraction.LogoutUrl = "/identity/sign-out";
                opt.UserInteraction.ErrorUrl = "/error";
            })
                .AddInMemoryIdentityResources(GetIdentityResources())
                .AddInMemoryApiResources(GetApiResources())

                // This one uses the ClientStoreConfiguration configured earlier
                .AddClientStore<DefaultsToSameOriginClientStore>()
                .AddAspNetIdentity<User>();


            // Add signing credentials
            if (env.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                var certThumbprint = embeddedIdentitySection["X509Certificate2Thumbprint"];
                X509Certificate2 cert = null;
                if (!string.IsNullOrWhiteSpace(certThumbprint))
                {
                    using (X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                    {
                        certStore.Open(OpenFlags.ReadOnly);
                        X509Certificate2Collection certCollection = certStore.Certificates.Find(
                                                   X509FindType.FindByThumbprint, certThumbprint, validOnly: false);

                        // Get the first cert with the thumbprint
                        if (certCollection.Count > 0)
                        {
                            cert = certCollection[0];
                            builder.AddSigningCredential(cert);
                        }
                        else
                        {
                            throw new Exception("Specified X509 certificate thumbprint was not found");
                        }
                    }
                }
                else
                {
                    throw new Exception("To enable the embedded Identity Server, a valid X509 certificate thumbprint must be specified in a configuration provider");
                }
            }

            // add authentication schemes
            var authBuilder = services.AddAuthentication();

            var googleSection = embeddedIdentitySection.GetSection("Google");
            var googleClientId = googleSection["ClientId"];
            var googleClientSecret = googleSection["ClientSecret"];
            if (!string.IsNullOrWhiteSpace(googleClientId))
            {
                authBuilder.AddGoogle("Google", "Google", opt =>
                {
                    opt.ClientId = googleClientId;
                    opt.ClientSecret = googleClientSecret;
                });
            }

            var microsoftSection = embeddedIdentitySection.GetSection("Microsoft");
            var microsoftClientId = microsoftSection["ClientId"];
            var microsoftClientSecret = microsoftSection["ClientSecret"];
            if (!string.IsNullOrWhiteSpace(microsoftClientId))
            {
                authBuilder.AddMicrosoftAccount("Microsoft", "Microsoft", opt =>
                {
                    opt.ClientId = microsoftClientId;
                    opt.ClientSecret = microsoftClientSecret;
                });
            }

            return services;
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            yield return new IdentityResources.OpenId();
            yield return new IdentityResources.Email();
            yield return new IdentityResources.Profile();
        }

        /// <summary>
        /// The API resources supported by the embedded IdentityServer instance
        /// </summary>
        public static IEnumerable<ApiResource> GetApiResources()
        {
            yield return new ApiResource(Constants.ApiResourceName)
            {
                UserClaims = { JwtClaimTypes.Email, JwtClaimTypes.EmailVerified }
            };
        }
    }
}

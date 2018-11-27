//using BSharp.Data;
//using BSharp.Data.Model.Identity;
//using BSharp.Services.Identity;
//using BSharp.Services.Utilities;
//using IdentityServer4;
//using IdentityServer4.Models;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Cryptography.X509Certificates;
//using System.Threading.Tasks;

//namespace Microsoft.Extensions.DependencyInjection
//{
//    public static class IdentityServiceCollectionExtensions
//    {
//        public static IServiceCollection AddApplicationIdentity(this IServiceCollection services, IConfiguration _config)
//        {
//            // This adds infrastructure for claims but not for roles
//            services.AddIdentityCore<ApplicationUser>(opt =>
//            {
//                // Make the password requirement less annoying, and compensate by increasing required length
//                opt.Password.RequireLowercase = false;
//                opt.Password.RequireUppercase = false;
//                opt.Password.RequireNonAlphanumeric = false;
//                opt.Password.RequiredLength = 7;
//            })
//            .AddEntityFrameworkStores<IdentityContext>()
//            .AddDefaultTokenProviders();

//            services.Configure<IISOptions>(iis =>
//            {
//                iis.AuthenticationDisplayName = "Windows";
//                iis.AutomaticAuthentication = false;
//            });

//            //X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
//            //certStore.Open(OpenFlags.ReadOnly);
//            //X509Certificate2Collection certCollection = certStore.Certificates.Find(
//            //                           X509FindType.FindByThumbprint,
//            //                     // Replace below with your cert's thumbprint
//            //                     "E661583E8FABEF4C0BEF694CBC41C28FB81CD870",
//            //                           false);
//            //// Get the first cert with the thumbprint
//            //if (certCollection.Count > 0)
//            //{
//            //    X509Certificate2 cert = certCollection[0];
//            //    // Use certificate
//            //    Console.WriteLine(cert.FriendlyName);
//            //}
//            //certStore.Close();



//            services.AddIdentityServer(opt =>
//            {
//                opt.UserInteraction.LoginUrl = "/identity/sign-in";
//                opt.UserInteraction.LogoutUrl = "/identity/sign-out";
//            })
//                .AddDeveloperSigningCredential()
//                .AddInMemoryIdentityResources(Config.GetIdentityResources())
//                .AddInMemoryApiResources(Config.GetApiResources())
//                .AddClientStore<DefaultsToSameOriginClientStore>()
//                .AddAspNetIdentity<IdentityUser>();


//            services.AddAuthentication()
//                .AddGoogle("Google", "Google", options =>
//                {
//                    options.ClientId = "295873087951-l1tfcidsa29sisqltr1f4a06msn2pg8a.apps.googleusercontent.com";
//                    options.ClientSecret = "3fPnwOrg6RRUMyjXeW_-aucG";
//                })
//                .AddIdentityServerAuthentication(JwtBearerDefaults.AuthenticationScheme, options =>
//                {
//                    options.Authority = "https://localhost:44339"; // TODO
//                    options.ApiName = Constants.BSharpAPI;
//                });


//            return services;
//        }
//    }

//    public static class Config
//    {
//        // scopes define the resources in your system
//        public static IEnumerable<IdentityResource> GetIdentityResources()
//        {
//            return new List<IdentityResource>
//            {
//                new IdentityResources.OpenId(),
//                new IdentityResources.Profile(),
//            };
//        }

//        public static IEnumerable<ApiResource> GetApiResources()
//        {
//            return new List<ApiResource>
//            {
//                new ApiResource(Constants.BSharpAPI)
//            };
//        }
//    }
//}
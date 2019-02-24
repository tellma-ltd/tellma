using AutoMapper;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.EmbeddedIdentityServer;
using BSharp.Services.Migrations;
using BSharp.Services.ModelMetadata;
using BSharp.Services.SqlLocalization;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Globalization;

namespace BSharp
{
    public class Startup
    {
        private readonly IConfiguration _config;
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration config, IHostingEnvironment env)
        {
            _config = config;
            _env = env;
        }

        private static bool _alreadyConfigured = false;

        public void ConfigureServices(IServiceCollection services)
        {
            // For some reason the integration tests are calling configure services 
            // twice, which is causing exceptions, this is a workaround until we
            // figure out the reason
            if (_alreadyConfigured)
            {
                return;
            }
            else
            {
                _alreadyConfigured = true;
            }

            // Global configurations maybe used in many places
            services.Configure<GlobalConfiguration>(_config);

            // Register the admin context
            services.AddDbContext<AdminContext>(builder =>
                builder.UseSqlServer(_config.GetConnectionString(Constants.AdminConnection))
                .ReplaceService<IMigrationsSqlGenerator, CustomSqlServerMigrationsSqlGenerator>());


            // The application context contains the shardlets, and unlike the other contexts it acquires its connection
            // string dynamically using IShardResolver when it is constructed, therefore this context cannot be
            // be registered in the DI the usual way with AddDbContext<T>()
            services.AddScoped<ApplicationContext>();

            // Add all our custom services
            services.AddMultiTenancy();
            services.AddSharding();
            services.AddBlobService(_config);
            services.AddSqlLocalization(_config);
            services.AddDynamicModelMetadata();
            services.AddGlobalSettingsCache(_config);

            // Setup an embedded instance of identity server in the same domain as the API if it is enabled in the configuration
            services.AddEmbeddedIdentityServerIfEnabled(_config, _env);

            // Add services for authenticating API calls against an OIDC authority, and helper services for accessing claims
            services.AddApiAuthentication(_config);

            // Register MVC using the most up to date version
            services.AddMvc(opt =>
            {
                // This filter checks version headers (e.g. x-translations-version) supplied by the client and efficiently
                // sets a response header to 'Fresh' or 'Stale' to prompt the client to refresh its settings if necessary
                opt.Filters.Add(typeof(CheckGlobalVersionsFilter));
            })
                .AddViewLocalization()
                .AddDataAnnotationsLocalization()
                .AddJsonOptions(options =>
                {
                    // The JSON options below instruct the serializer to keep property names in PascalCase, 
                    // even though this violates convention, it makes a few things easier since both client and server
                    // sides get to see and communicate identical property names, for example 'api/customers?orderby='Name'
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new DefaultNamingStrategy()
                    };
                    // To reduce response size, some of the DTOs we use are humongously wide
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)

                // TODO: Only when using embedded identity
                .AddRazorPagesOptions(opt =>
                {
                    opt.AllowAreas = true;
                    opt.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                    opt.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");

                });

            // TODO: Only when using embedded identity
            services.ConfigureApplicationCookie(opt =>
            {
                opt.ExpireTimeSpan = TimeSpan.FromDays(Constants.TokenExpiryInDays);
                opt.SlidingExpiration = true;
                opt.LoginPath = $"/identity/sign-in";
                opt.LogoutPath = $"/identity/sign-out";
                opt.AccessDeniedPath = $"/identity/access-denied";
            });

            // TODO: Only when using embedded identity
            services.AddEmail(_config.GetSection("Email"));

            // To allow a client that is hosted on another server
            services.AddCors();

            // Configure some custom behavior for API controllers
            services.Configure<ApiBehaviorOptions>(opt =>
            {
                // This overrides the default behavior, when there are validation
                // errors we return a 422 unprocessable entity, instead of the default
                // 400 bad request, this makes it easier for clients to distinguish 
                // such kind of errors and handle them in a special way, for example:
                // by showing them on the fields with a red color
                opt.InvalidModelStateResponseFactory = ctx =>
                {
                    return new UnprocessableEntityObjectResult(ctx.ModelState);
                };
            });

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(opt =>
            {
                opt.RootPath = "ClientApp/dist";
            });

            // AutoMapper https://automapper.org/
            services.AddAutoMapper();

        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> logger, IServiceProvider services)
        {
            var globalConfig = services.GetService<IOptions<GlobalConfiguration>>()?.Value;
            var localizationConfig = services.GetService<IOptions<SqlLocalizationConfiguration>>()?.Value;
            var clientStoreConfig = services.GetService<IOptions<ClientStoreConfiguration>>()?.Value;

            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // Picks out the culture from the request string and sets it in the current thread
            app.UseRequestLocalization(opt =>
            {
                var defaultUICulture = localizationConfig?.DefaultUICulture ?? "en";
                var defaultCulture = localizationConfig?.DefaultCulture ?? "en-GB";

                opt.DefaultRequestCulture = new RequestCulture(defaultCulture, defaultUICulture);

                // Formatting numbers, dates, etc.
                opt.AddSupportedCultures(defaultCulture);

                // UI strings that we have localized.
                opt.SupportedUICultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            // CORS
            string webClientUri = clientStoreConfig?.WebClientUri.WithoutTrailingSlash();
            if (!string.IsNullOrWhiteSpace(webClientUri))
            {
                app.UseCors(builder =>
                {
                    builder.WithOrigins(webClientUri)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("x-image-id")
                    .WithExposedHeaders("x-settings-version")
                    .WithExposedHeaders("x-permissions-version")
                    .WithExposedHeaders("x-user-settings-version")
                    .WithExposedHeaders("x-translations-version")
                    .WithExposedHeaders("x-global-settings-version");
                });
            }

            // Serves the identity server
            if (globalConfig.EmbeddedIdentityServerEnabled)
            {
                app.UseIdentityServer();
            }

            // Serves the API
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            // Serves the Angular client
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                if (_env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}

using BSharp.Controllers;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace BSharp
{
    public class Startup
    {
        // The UI cultures currently supported by the system
        public static readonly string[] SUPPORTED_CULTURES = new string[] { "en", "ar" };

        private readonly IConfiguration _config;
        private readonly IHostingEnvironment _env;

        /// <summary>
        /// If there is an error in <see cref="ConfigureServices(IServiceCollection)"/>, usually
        /// due to a required configuration value that was not provided, the error message is recorded
        /// here. If the middlewhere finds this error it returns it immediately as plaintext and ignores
        /// everything else. This is a convenient way to report configuration errors
        /// </summary>
        public string ConfigurationError { get; private set; }

        /// <summary>
        /// If there is an error when starting up the application, or seeding the database etc.
        /// It is added here, and served as plain text to any web request
        /// </summary>
        public static string GlobalError { get; set; }

        /// <summary>
        /// Used in both <see cref="ConfigureServices(IServiceCollection)"/> and <see cref="Configure(IApplicationBuilder)"/>
        /// </summary>
        public GlobalOptions GlobalOptions { get; private set; }

        /// <summary>
        /// Create a new instance of <see cref="Startup"/>
        /// </summary>
        public Startup(IConfiguration config, IHostingEnvironment env)
        {
            _config = config;
            _env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                // Global configurations maybe used in many places
                services.Configure<GlobalOptions>(_config);
                GlobalOptions = _config.Get<GlobalOptions>();

                // Access to caller information
                services.AddClientInfo();

                // Register the admin repo
                var connString = _config.GetConnectionString(Constants.AdminConnection);
                services.AddAdminRepository(connString);

                // Custom services
                services.AddMultiTenancy();
                services.AddSharding();

                // The application repository contains the tenant specific data, it acquires the
                // connection string dynamically, therefore it depends on multitenancy and sharding
                services.AddApplicationRepository();

                // More custom services
                services.AddBlobService(_config);
                services.AddDefinitionsModelMetadata();
                services.AddGlobalSettingsCache(_config.GetSection("GlobalSettingsCache"));

                // Add the default localization that relies on resource files in /Resources
                services.AddLocalization(opt =>
                {
                    opt.ResourcesPath = "Resources";
                });

                // Register MVC using the most up to date version
                var mvcBuilder = services.AddMvc(opt =>
                {
                    // This filter checks version headers (e.g. x-translations-version) supplied by the client and efficiently
                    // sets a response header to 'Fresh' or 'Stale' to prompt the client to refresh its settings if necessary
                    opt.Filters.Add(typeof(GlobalFilter));
                })
                    .AddDataAnnotationsLocalization(opt =>
                    {
                        // This allows us to have a single RESX file for all classes and namespaces
                        opt.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(Strings));
                    })
                    .AddJsonOptions(opt =>
                    {
                        // The JSON options below instruct the serializer to keep property names in PascalCase, 
                        // even though this violates convention, it makes a few things easier since both client and server
                        // sides get to see and communicate identical property names, for example 'api/customers?orderby='Name'
                        opt.SerializerSettings.ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new DefaultNamingStrategy(),
                        };

                        // To reduce response size, since some of the Entities we use are humongously wide
                        // and the API allows selecting a small subset of the columns
                        opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    })
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

                // Setup an embedded instance of identity server in the same domain as the API if it is enabled in the configuration
                if (GlobalOptions.EmbeddedIdentityServerEnabled)
                {
                    var idServerConfig = _config.GetSection("EmbeddedIdentityServer");
                    var clientAppsConfig = _config.GetSection(nameof(Services.Utilities.GlobalOptions.ClientApplications));

                    services.AddEmbeddedIdentityServer(
                        configSection: idServerConfig,
                        clientsConfigSection: clientAppsConfig,
                        mvcBuilder: mvcBuilder,
                        isDevelopment: _env.IsDevelopment());
                }

                // Add services for authenticating API calls against an OIDC authority, and helper services for accessing claims
                var apiAuthConfig = _config.GetSection("ApiAuthentication");
                services.AddApiAuthentication(apiAuthConfig);

                // Add Email
                services.AddEmail(_config.GetSection("Email"));

                // Configure some custom behavior for API controllers
                services.Configure<ApiBehaviorOptions>(opt =>
                {
                    // This overrides the default behavior, when there are validation
                    // errors we return a 422 unprocessable entity, instead of the default
                    // 400 bad request, this makes it easier for clients to distinguish 
                    // such kinds of errors and handle them in a special way, for example:
                    // by showing them on the fields with a red color
                    opt.InvalidModelStateResponseFactory = ctx =>
                        {
                            return new UnprocessableEntityObjectResult(ctx.ModelState);
                        };
                });

                // Embedded Client Application
                if (GlobalOptions.EmbeddedClientApplicationEnabled)
                {
                    services.AddSpaStaticFiles(opt =>
                    {
                        // In production, the Angular files will be served from this directory
                        opt.RootPath = "ClientApp/dist";
                    });
                }

                // Giving access to clients that are hosted on another domain
                services.AddCors();
            }
            catch (Exception ex)
            {
                // The configuration encountered a fatal error, usually a required yet missing configuration
                // Setting this property instructs the middleware to short-circuit and just return this error in plain text                
                ConfigurationError = ex.Message;

            }
        }

        public void Configure(IApplicationBuilder app)
        {
            try
            {
                // Configuration Errors
                app.Use(async (context, next) =>
                {
                    string error = ConfigurationError ?? GlobalError;
                    if (error != null)
                    {
                    // This means the application was not configured correctly and should not be running
                    // We cut the pipeline short and report the error message in plain text
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync(error);
                    }
                    else
                    {
                    // All is good, continue the normal pipeline
                    await next.Invoke();
                    }
                });

                // Regular Errors
                if (_env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Error");
                    app.UseHsts();
                }

                // Localization
                // Extract the culture from the request string and set it in the execution thread
                var defaultUiCulture = GlobalOptions.Localization?.DefaultUICulture ?? "en";
                var defaultCulture = GlobalOptions.Localization?.DefaultCulture ?? "en-GB";
                app.UseRequestLocalization(opt =>
                {
                // When no culture is specified in the request, use these
                opt.DefaultRequestCulture = new RequestCulture(defaultCulture, defaultUiCulture);

                // Formatting numbers, dates, etc.
                opt.AddSupportedCultures(defaultCulture);

                // UI strings that we have localized
                opt.AddSupportedUICultures(SUPPORTED_CULTURES);
                });

                app.UseHttpsRedirection();
                app.UseStaticFiles();

                if (GlobalOptions.EmbeddedClientApplicationEnabled)
                {
                    app.UseSpaStaticFiles();
                }

                // CORS
                string webClientUri = GlobalOptions.ClientApplications?.WebClientUri.WithoutTrailingSlash();
                if (!string.IsNullOrWhiteSpace(webClientUri))
                {
                    // If a web client is listed in the configurations, add it to CORS
                    app.UseCors(builder =>
                    {
                        builder.WithOrigins(webClientUri)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("x-image-id")
                        .WithExposedHeaders("x-settings-version")
                        .WithExposedHeaders("x-permissions-version")
                        .WithExposedHeaders("x-definitions-version")
                        .WithExposedHeaders("x-user-settings-version")
                        .WithExposedHeaders("x-global-settings-version");
                    });
                }

                // IdentityServer
                if (GlobalOptions.EmbeddedIdentityServerEnabled)
                {
                    app.UseEmbeddedIdentityServer();
                }

                // The API
                app.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller}/{action=Index}/{id?}");
                });

                // The Angular client
                if (GlobalOptions.EmbeddedClientApplicationEnabled)
                {
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
            catch (Exception ex)
            {
                // The configuration encountered a fatal error, usually a required yet missing configuration
                // Setting this property instructs the middleware to short-circuit and just return this error in plain text
                ConfigurationError = ex.Message;
            }
        }
    }

    /// <summary>
    /// Only here to allow us to have a single shared resource file, as per the official docs https://bit.ly/2Z1fH0k
    /// </summary>
    public class Strings { }
}

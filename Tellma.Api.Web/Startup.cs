using Tellma.Controllers;
using Tellma.Services.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using Microsoft.Extensions.Azure;
using Azure.Storage.Queues;
using Azure.Storage.Blobs;
using Azure.Core.Extensions;
using Newtonsoft.Json.Converters;

namespace Tellma
{
    public class Startup
    {
        // The UI cultures currently supported by the system
        public static readonly string[] SUPPORTED_CULTURES = new string[] { "en", "ar", "zh", "am", "om" };

        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

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
        public Startup(IConfiguration config, IWebHostEnvironment env)
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

                // Adds a service that can resolve the client URI
                services.AddClientAppAddressResolver();

                // Azure Application Insights
                services.AddApplicationInsightsTelemetry(_config["APPINSIGHTS_INSTRUMENTATIONKEY"]);
                services.AddInstrumentation(GlobalOptions.InstrumentationEnabled, _config.GetSection("Instrumentation"));

                // Access to caller information
                services.AddClientInfo();

                // Register the admin repo
                var connString = _config.GetConnectionString(Constants.AdminConnection);
                services.AddAdminRepository(connString);

                // Custom services
                services.AddMultiTenancy();
                services.AddSharding(_config.GetSection("Sharding"));

                // The application repository contains the tenant specific data, it acquires the
                // connection string dynamically, therefore it depends on multitenancy and sharding
                services.AddApplicationRepository();

                // More custom services
                services.AddBlobService(_config);

                // Add the default localization that relies on resource files in /Resources
                services.AddLocalization(opt =>
                {
                    opt.ResourcesPath = "Resources";
                });


                // Register MVC
                services
                    .AddControllersWithViews(opt =>
                    {
                        // This filter checks version headers (e.g. x-translations-version) supplied by the client and efficiently
                        // sets a response header to 'Fresh' or 'Stale' to prompt the client to refresh its settings if necessary
                        opt.Filters.Add(typeof(GlobalFilter));
                    })
                    .ConfigureApiBehaviorOptions(opt =>
                    {
                        // Validation is performed at the business service layer, not at the Controller layer
                        // Controllers are very thin, their only purpose is to translate from web world to C# world
                        opt.SuppressModelStateInvalidFilter = true;
                    })
                    .AddDataAnnotationsLocalization(opt =>
                    {
                        // This allows us to have a single RESX file for all classes and namespaces
                        opt.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(Strings));
                    })
                    .AddNewtonsoftJson(opt =>
                    {
                        // The JSON options below instruct the serializer to keep property names in PascalCase, 
                        // even though this violates convention, it makes a few things easier since both client and server
                        // sides get to see and communicate identical property names, for example 'api/customers?orderby='Name'
                        opt.SerializerSettings.ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new DefaultNamingStrategy(),
                        };

                        // This converts all datetimeoffsets to UTC in the ISO format terminating with Z
                        //   opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                        opt.SerializerSettings.Converters.Add(new CustomDateTimeConverter());

                        // To reduce response size, since some of the Entities we use are humongously wide
                        // and the API allows selecting a small subset of the columns
                        opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    })
                    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

                // Setup an embedded instance of identity server in the same domain as the API if it is enabled in the configuration
                if (GlobalOptions.EmbeddedIdentityServerEnabled)
                {
                    // Tp support the authentication pages
                    var mvcBuilder = services.AddRazorPages();

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
                services.AddEmail(GlobalOptions.EmailEnabled, _config);

                // Add SMS
                services.AddSms(GlobalOptions.SmsEnabled, _config);

                // Configure some custom behavior for API controllers
                services.Configure<ApiBehaviorOptions>(opt =>
                {
                    // This overrides the default behavior, when there are validation
                    // errors we return a 422 unprocessable entity, instead of the default
                    // 400 bad request, this makes it easier for clients to distinguish 
                    // such kinds of errors and handle them in a special way, for example:
                    // by showing them on the fields with a red color
                    opt.InvalidModelStateResponseFactory = ctx => new UnprocessableEntityObjectResult(ctx.ModelState);
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

                services.AddHttpsRedirection(opt =>
                {
                    // This is a required configuration since ASP.NET Core 2.1
                    opt.HttpsPort = 443;
                });

                // Adds and configures SignalR
                services.AddSignalRImplementation(_config, _env);

                // Add service for generating markup from templates
                services.AddMarkupTemplates();

                // For better management of HttpClients
                services.AddHttpClient();

                // Add the business logic services (DocumentsService, ResourcesService, etc...)
                services.AddBusinessServices(_config);
            }
            catch (Exception ex)
            {
                // The configuration encountered a fatal error, usually a required yet missing configuration
                // Setting this property instructs the middleware to short-circuit and just return this error in plain text                
                ConfigurationError = ex.Message;
            }

            //services.AddAzureClients(builder =>
            //{
            //    builder.AddBlobServiceClient(Configuration["ConnectionStrings:AzureBlobStorage:ConnectionString:blob"], preferMsi: true);
            //    builder.AddQueueServiceClient(Configuration["ConnectionStrings:AzureBlobStorage:ConnectionString:queue"], preferMsi: true);
            //});
        }

        public void Configure(IApplicationBuilder app)
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

            // If there is a configuration/global error already, don't configure the remaining
            // middleware, they may overrwrite the error message causing the above trick to fail
            if ((ConfigurationError ?? GlobalError) != null)
            {
                return;
            }

            try
            {
                // Built-in instrumentation
                app.UseInstrumentation();

                // Regular Errors
                if (_env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseMiddlewareInstrumentation("Developer Exception Page");
                }
                else
                {
                    app.UseExceptionHandler("/Error");
                    app.UseMiddlewareInstrumentation("Exception Handler");

                    app.UseHsts();
                    app.UseMiddlewareInstrumentation("HSTS");
                }

                app.UseHttpsRedirection();
                app.UseMiddlewareInstrumentation("Https Redirection");

                // Adds the SMS event webhook Callback
                app.UseSmsCallback(_config);

                // Adds the email event webhook Callback
                app.UseEmailCallback(_config);

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
                app.UseMiddlewareInstrumentation("Localization");

                app.UseStaticFiles();
                app.UseMiddlewareInstrumentation("Static Files");

                if (GlobalOptions.EmbeddedClientApplicationEnabled)
                {
                    app.UseSpaStaticFiles();
                    app.UseMiddlewareInstrumentation("SPA Static Files");
                }

                // The API
                app.UseRouting();
                app.UseMiddlewareInstrumentation("Routing");

                // CORS
                if (!GlobalOptions.EmbeddedClientApplicationEnabled)
                {
                    string webClientUri = GlobalOptions.ClientApplications?.WebClientUri.WithoutTrailingSlash();
                    if (string.IsNullOrWhiteSpace(webClientUri))
                    {
                        throw new Exception($"The configuration value {nameof(GlobalOptions.ClientApplications)}:{nameof(WebClientOptions.WebClientUri)} is required when {nameof(GlobalOptions.EmbeddedClientApplicationEnabled)} is not set to true");
                    }

                    // If a web client is listed in the configurations, add it to CORS
                    app.UseCors(builder =>
                    {
                        builder.WithOrigins(webClientUri)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .WithExposedHeaders(
                            "x-image-id",
                            "x-settings-version",
                            "x-permissions-version",
                            "x-definitions-version",
                            "x-user-settings-version",
                            "x-admin-settings-version",
                            "x-admin-permissions-version",
                            "x-admin-user-settings-version",
                            "x-global-settings-version",
                            "x-instrumentation"
                        );
                    });
                    app.UseMiddlewareInstrumentation("CORS");
                }

                // Moves the access token from the query string to the Authorization header, for SignalR
                app.UseQueryStringToken();
                app.UseMiddlewareInstrumentation("Query String Token");

                // IdentityServer
                if (GlobalOptions.EmbeddedIdentityServerEnabled)
                {
                    // Note: this already includes a call to app.UseAuthentication()
                    app.UseEmbeddedIdentityServer();
                    app.UseMiddlewareInstrumentation("Embedded Identity Server");
                }
                else
                {
                    app.UseAuthentication();
                    app.UseMiddlewareInstrumentation("Authentication");
                }

                app.UseAuthorization();
                app.UseMiddlewareInstrumentation("Authorization");

                // The API
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHub<ServerNotificationsHub>("api/hubs/notifications");

                    

                    // For the API
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller}/{action=Index}/{id?}");

                    // For authentication Razor pages
                    if (GlobalOptions.EmbeddedIdentityServerEnabled)
                    {
                        endpoints.MapRazorPages();
                    }
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
    /// Converts all DateTime values to the following format: "2021-02-15T01:17:13.286".
    /// Converts all DateTimeOffset values to the following format: "2021-02-15T01:17:13.2865330Z".
    /// </summary>
    public class CustomDateTimeConverter : IsoDateTimeConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string text;

            if (value is DateTime dateTime)
            {
                text = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fff", Culture);
            }
            else if (value is DateTimeOffset dateTimeOffset)
            {
                dateTimeOffset = dateTimeOffset.ToUniversalTime();
                text = dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", Culture);
            }
            else
            {
                throw new JsonSerializationException($"Unexpected value when converting date. Expected DateTime or DateTimeOffset, got {value?.GetType()}.");
            }

            writer.WriteValue(text);
        }
    }

    internal static class StartupExtensions
    {
        public static IAzureClientBuilder<BlobServiceClient, BlobClientOptions> AddBlobServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
        {
            if (preferMsi && Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri serviceUri))
            {
                return builder.AddBlobServiceClient(serviceUri);
            }
            else
            {
                return builder.AddBlobServiceClient(serviceUriOrConnectionString);
            }
        }
        public static IAzureClientBuilder<QueueServiceClient, QueueClientOptions> AddQueueServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
        {
            if (preferMsi && Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri serviceUri))
            {
                return builder.AddQueueServiceClient(serviceUri);
            }
            else
            {
                return builder.AddQueueServiceClient(serviceUriOrConnectionString);
            }
        }
    }
}

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
using Tellma.Api.Base;
using Tellma.Api.Web.Controllers;
using Tellma.Controllers;
using Tellma.Services.ClientProxy;
using Tellma.Services.Utilities;

namespace Tellma
{
    public class Startup
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly GlobalOptions _opt;

        /// <summary>
        /// If there is an error when starting up the web server (usually due to a required configuration 
        /// value that was not provided, the error message is set here. <br/>
        /// If the middlewhere finds this error it returns it immediately as plaintext and ignores
        /// everything else. This is a convenient way to debug configuration errors when setting up the 
        /// system for the first time.
        /// </summary>
        public static string StartupError { get; set; }

        public Startup(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
            _opt = _config.Get<GlobalOptions>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                // Global configurations maybe used in many places
                services.Configure<GlobalOptions>(_config);
                services.Configure<AdminOptions>(_config.GetSection("Admin"));

                // Adds a service that can resolve the client URI
                services.AddClientAppAddressResolver(_config);

                // Azure Application Insights
                services.AddApplicationInsightsTelemetry(_config["APPINSIGHTS_INSTRUMENTATIONKEY"]);

                // Register the API
                services.AddTellmaApi(_config)
                    .AddSingleton<IServiceContextAccessor, WebServiceContextAccessor>()
                    .AddClientAppProxy(_config);

                // Dependency for the GlobalController and the GlobalFilter
                services.AddScoped<GlobalSettingsProvider>();

                // Add optoinal services
                if (_opt.EmailEnabled)
                {
                    services.AddSendGrid(_config);
                }

                if (_opt.SmsEnabled)
                {
                    services.AddTwilio(_config);
                }

                if (_opt.AzureBlobStorageEnabled)
                {
                    // This overrides the SQL implementation added by default
                    services.AddAzureBlobStorage(_config);
                }

                // Add the default localization that relies on resource files in /Resources
                services.AddLocalization();

                // Register MVC
                services
                    .AddControllersWithViews(opt =>
                    {
                        // This filter checks version headers (e.g. x-translations-version) supplied by the client and efficiently
                        // sets a response header to 'Fresh' or 'Stale' to prompt the client to refresh its settings if necessary
                        opt.Filters.Add<GlobalFilter>();

                        // This filters traps any exception in the action execution and turns it into a proper response
                        opt.Filters.Add<ExceptionsFilter>();
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
                if (_opt.EmbeddedIdentityServerEnabled)
                {
                    // To support the authentication pages
                    var mvcBuilder = services.AddRazorPages();

                    services.AddEmbeddedIdentityServer(config: _config, mvcBuilder: mvcBuilder,
                        isDevelopment: _env.IsDevelopment());
                }

                // Add services for authenticating API calls against an OIDC authority, and helper services for accessing claims
                services.AddApiAuthentication(_config);

                // Configure some custom behavior for API controllers
                services.Configure<ApiBehaviorOptions>(opt =>
                {
                    // This overrides the default behavior, when there are validation
                    // errors we return a 422 unprocessable entity, instead of the default
                    // 400 bad request, this makes it easier for clients to distinguish 
                    // such kinds of errors and handle them in a special way, for example:
                    // by showing them on the relevant fields with a red highlight
                    opt.InvalidModelStateResponseFactory = ctx => new UnprocessableEntityObjectResult(ctx.ModelState);
                });

                // Embedded Client Application
                if (_opt.EmbeddedClientApplicationEnabled)
                {
                    services.AddSpaStaticFiles(opt =>
                    {
                        // In production, the Angular files will be served from this directory
                        opt.RootPath = "ClientApp/dist";
                    });
                }

                // Giving access to clients that are hosted on another domain
                services.AddCors();

                // This is a required configuration since ASP.NET Core 2.1
                services.AddHttpsRedirection(opt =>
                {
                    opt.HttpsPort = 443;
                });

                // Adds and configures SignalR
                services.AddSignalRImplementation(_config, _env);
            }
            catch (Exception ex)
            {
                // The configuration encountered a fatal error, usually a required yet missing configuration
                // Setting this property instructs the middleware to short-circuit and just return this error in plain text                
                StartupError = ex.Message;
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            #region Startup Error Handling

            // Configuration Errors
            app.Use(async (context, next) =>
            {
                if (StartupError != null)
                {
                    // This means the application was not configured correctly and should not be running
                    // We cut the pipeline short and report the error message in plain text to make debugging easier
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync(StartupError);
                }
                else
                {
                    // All is good, continue the normal pipeline
                    await next.Invoke();
                }
            });

            // If there is a configuration/global error already, don't configure the remaining
            // middleware, they may overrwrite the error message causing the above trick to fail
            if (StartupError != null)
            {
                return;
            }

            #endregion

            try
            {
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

                // Only HTTPS allowed
                app.UseHttpsRedirection();

                // Twilio event webhook callback
                if (_opt.SmsEnabled)
                {
                    app.UseTwilioCallback(_config);
                }

                // SendGrid event webhook callback
                if (_opt.EmailEnabled)
                {
                    app.UseSendGridCallback(_config);
                }

                // Localization
                var l10nOpt = _config.GetSection("Localization").Get<LocalizationOptions>();
                var defaultUiCulture = l10nOpt.DefaultUICulture ?? "en";
                var defaultCulture = l10nOpt.DefaultCulture ?? "en-GB";

                // Extract the culture from the request string and set it in the execution thread
                app.UseRequestLocalization(opt =>
                {
                    // When no culture is specified in the request, use these
                    opt.DefaultRequestCulture = new RequestCulture(defaultCulture, defaultUiCulture);

                    // Formatting numbers, dates, etc.
                    opt.AddSupportedCultures(defaultCulture);

                    // UI strings that we have localized
                    opt.AddSupportedUICultures(Strings.SUPPORTED_CULTURES);
                });

                // wwwroot folder
                app.UseStaticFiles();

                if (_opt.EmbeddedClientApplicationEnabled)
                {
                    // Angular assets folder
                    app.UseSpaStaticFiles();
                }

                // The API
                app.UseRouting();

                // CORS
                if (!_opt.EmbeddedClientApplicationEnabled)
                {
                    app.UseCorsForNonEmbeddedClientApp(_config);
                }

                // Moves the access token from the query string to the Authorization header, for SignalR
                app.UseQueryStringToken();

                // IdentityServer
                if (_opt.EmbeddedIdentityServerEnabled)
                {
                    // Note: this already includes a call to app.UseAuthentication()
                    app.UseEmbeddedIdentityServer();
                }
                else
                {
                    app.UseAuthentication();
                }

                app.UseAuthorization();

                // The API
                app.UseEndpoints(endpoints =>
                {
                    // Signal R
                    endpoints.MapHub<ServerNotificationsHub>("api/hubs/notifications");

                    // For the API
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller}/{action=Index}/{id?}");

                    // For authentication Razor pages
                    if (_opt.EmbeddedIdentityServerEnabled)
                    {
                        endpoints.MapRazorPages();
                    }
                });

                // The Angular client
                if (_opt.EmbeddedClientApplicationEnabled)
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
                StartupError = ex.Message;
            }
        }
    }
}

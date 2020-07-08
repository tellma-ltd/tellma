using Tellma.Data;
using Tellma.Services.ApiAuthentication;
using Tellma.Services.Identity;
using Tellma.Services.MultiTenancy;
using Tellma.Services.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Services.Sharding;
using Microsoft.Extensions.Logging;
using Tellma.Services;

namespace Tellma.Controllers
{
    /// <summary>
    /// 1. Ensures that the API caller has supplied a valid tenantId value, otherwise the request is aborted with a 400
    /// 2. Ensures that the authenticated user has an active membership in that tenantId otherwise the request is aborted with a 403
    /// 3. If the user is new it updates his/her ExternalId in the tenant database as well as the centralized admin database
    /// 4. If the user has a new email it updates his/her Email in the app database
    /// 5. Add the tenant info in the HTTP context, making it accessible to our model metadata provider
    /// 6. Ensures that the <see cref="IDefinitionsCache"/> is nice and fresh
    /// 7. If the version headers are provided, it also checks their freshness and adds appropriate response headers
    /// IMPORTANT: This attribute should always be precedede with another attribute <see cref="AuthorizeJwtBearerAttribute"/>
    /// </summary>
    public class ApplicationControllerAttribute : TypeFilterAttribute
    {
        public ApplicationControllerAttribute(bool allowUnobtrusive = false) :
            base(allowUnobtrusive ? typeof(UnobtrusiveApplicationApiFilter) : typeof(ObtrusiveApplicationApiFilter))
        { }

        /// <summary>
        /// An implementation of the method described here https://bit.ly/2MKwY7A
        /// </summary>
        private abstract class ApplicationApiFilter : IAsyncResourceFilter
        {
            private readonly ApplicationRepository _appRepo;
            private readonly ITenantIdAccessor _tenantIdAccessor;
            private readonly ITenantInfoAccessor _tenantInfoAccessor;
            private readonly IExternalUserAccessor _externalUserAccessor;
            private readonly IServiceProvider _serviceProvider;
            private readonly IDefinitionsCache _definitionsCache;
            private readonly ISettingsCache _settingsCache;
            private readonly IInstrumentationService _instrumentation;

            public ApplicationApiFilter(IServiceProvider sp)
            {
                _appRepo = sp.GetRequiredService<ApplicationRepository>();
                _tenantIdAccessor = sp.GetRequiredService<ITenantIdAccessor>();
                _tenantInfoAccessor = sp.GetRequiredService<ITenantInfoAccessor>();
                _externalUserAccessor = sp.GetRequiredService<IExternalUserAccessor>();
                _definitionsCache = sp.GetRequiredService<IDefinitionsCache>();
                _settingsCache = sp.GetRequiredService<ISettingsCache>();
                _instrumentation = sp.GetRequiredService<IInstrumentationService>();
                _serviceProvider = sp;
            }

            protected abstract bool AllowUnobtrusive { get; }

            public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
            {
                IDisposable block;

                // (1) Make sure the API caller have provided a tenantId, and extract it
                try
                {
                    var cancellation = context.HttpContext.RequestAborted;
                    int tenantId = _tenantIdAccessor.GetTenantId();

                    // Init the database connection...
                    // The client sometimes makes ambient API calls, not in response to user interaction
                    // Such calls should not update LastAccess of that user
                    bool unobtrusive = AllowUnobtrusive && context.HttpContext.Request.Query["unobtrusive"].FirstOrDefault()?.ToString()?.ToLower() == "true";
                    await _appRepo.InitConnectionAsync(tenantId, setLastActive: !unobtrusive, cancellation);

                    // (2) Make sure the user is a member of this tenant
                    UserInfo userInfo = await _appRepo.GetUserInfoAsync(cancellation);

                    if (userInfo.UserId == null)
                    {
                        // If there is no user cut the pipeline short and return a Forbidden 403
                        context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);

                        // This indicates to the client to discard all cached information about this
                        // company since the user is no longer a member of it
                        context.HttpContext.Response.Headers.Add("x-settings-version", Constants.Unauthorized);
                        context.HttpContext.Response.Headers.Add("x-definitions-version", Constants.Unauthorized);
                        context.HttpContext.Response.Headers.Add("x-permissions-version", Constants.Unauthorized);
                        context.HttpContext.Response.Headers.Add("x-user-settings-version", Constants.Unauthorized);

                        return;
                    }

                    var userId = userInfo.UserId.Value;
                    var externalId = _externalUserAccessor.GetUserId();
                    var externalEmail = _externalUserAccessor.GetUserEmail();

                    // (3) If the user exists but new, set the External Id
                    if (userInfo.ExternalId == null)
                    {
                        // Update external Id in this tenant database
                        await _appRepo.Users__SetExternalIdByUserId(userId, externalId);

                        // Update external Id in the central Admin database too (To avoid an awkward situation
                        // where a user exists on the tenant but not on the Admin db, if they change their email in between)
                        var adminRepo = _serviceProvider.GetRequiredService<AdminRepository>();
                        await adminRepo.DirectoryUsers__SetExternalIdByEmail(externalEmail, externalId);
                    }

                    else if (userInfo.ExternalId != externalId)
                    {
                        // Note: there is the edge case of identity providers who allow email recycling. I.e. we can get the same email twice with 
                        // two different external Ids. This issue is so unlikely to naturally occur and cause problems here that we are not going
                        // to handle it for now. It can however happen artificually if the application is re-configured to a new identity provider,
                        // or if someone messed with the identity database directly, but again out of scope for now.
                        context.Result = new BadRequestObjectResult("The sign-in email already exists but with a different external Id");
                        return;
                    }

                    // (4) If the user's email address has changed at the identity server, update it locally
                    else if (userInfo.Email != externalEmail)
                    {
                        await _appRepo.Users__SetEmailByUserId(userId, externalEmail);
                    }

                    // (5) Set the tenant info in the context, to make it accessible for model metadata providers
                    var tenantInfo = await _appRepo.GetTenantInfoAsync(cancellation);
                    _tenantInfoAccessor.SetInfo(tenantId, tenantInfo);

                    // (6) Ensure the freshness of the definitions and settings caches
                    {
                        var databaseVersion = tenantInfo.DefinitionsVersion;
                        var serverVersion = _definitionsCache.GetDefinitionsIfCached(tenantId)?.Version;

                        if (serverVersion == null || serverVersion != databaseVersion)
                        {
                            // Update the cache
                            var definitions = await DefinitionsService.LoadDefinitionsForClient(_appRepo, cancellation);
                            if (!cancellation.IsCancellationRequested)
                            {
                                _definitionsCache.SetDefinitions(tenantId, definitions);
                            }
                        }
                    }
                    {
                        var databaseVersion = tenantInfo.SettingsVersion;
                        var serverVersion = _settingsCache.GetSettingsIfCached(tenantId)?.Version;

                        if (serverVersion == null || serverVersion != databaseVersion)
                        {
                            // Update the cache
                            var settings = await SettingsService.LoadSettingsForClient(_appRepo, cancellation);
                            if (!cancellation.IsCancellationRequested)
                            {
                                _settingsCache.SetSettings(tenantId, settings);
                            }
                        }
                    }

                    // (7) If any version headers are supplied: examine their freshness
                    {
                        // Permissions
                        var clientVersion = context.HttpContext.Request.Headers["X-Permissions-Version"].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(clientVersion))
                        {
                            var databaseVersion = userInfo.PermissionsVersion;
                            context.HttpContext.Response.Headers.Add("x-permissions-version",
                                clientVersion == databaseVersion ? Constants.Fresh : Constants.Stale);
                        }
                    }

                    {
                        // User Settings
                        var clientVersion = context.HttpContext.Request.Headers["X-User-Settings-Version"].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(clientVersion))
                        {
                            var databaseVersion = userInfo.UserSettingsVersion;
                            context.HttpContext.Response.Headers.Add("x-user-settings-version",
                                clientVersion == databaseVersion ? Constants.Fresh : Constants.Stale);
                        }
                    }

                    {
                        // Definitions
                        var clientVersion = context.HttpContext.Request.Headers["X-Definitions-Version"].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(clientVersion))
                        {
                            var databaseVersion = tenantInfo.DefinitionsVersion;
                            context.HttpContext.Response.Headers.Add("x-definitions-version",
                                clientVersion == databaseVersion ? Constants.Fresh : Constants.Stale);
                        }
                    }
                    {
                        // Settings
                        var clientVersion = context.HttpContext.Request.Headers["X-Settings-Version"].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(clientVersion))
                        {
                            var databaseVersion = tenantInfo.SettingsVersion;
                            context.HttpContext.Response.Headers.Add("x-settings-version",
                                clientVersion == databaseVersion ? Constants.Fresh : Constants.Stale);
                        }
                    }

                    // Call the Action itself
                    await next();
                }
                catch (TaskCanceledException)
                {
                    context.Result = new OkResult();
                    return;
                }
                catch (MultitenancyException ex)
                {
                    // If the tenant Id is not provided cut the pipeline short and return a Bad Request 400
                    context.Result = new BadRequestObjectResult(ex.Message);
                    return;
                }
                catch (BadRequestException ex)
                {
                    // If the tenant Id is not provided cut the pipeline short and return a Bad Request 400
                    context.Result = new BadRequestObjectResult(ex.Message);
                    return;
                }
                catch (Exception ex)
                {
                    // TODO: Return to logging and 500 status code
                    context.Result = new BadRequestObjectResult(ex.GetType().Name + ": " + ex.Message);
                    //_logger.LogError(ex.Message);
                    //context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);

                    return;
                }
            }
        }

        #region Implementations

        /// <summary>
        /// An implementation of <see cref="ApplicationApiFilter"/> that allows the client to bypass setting LastAccess of the user
        /// </summary>
        private class UnobtrusiveApplicationApiFilter : ApplicationApiFilter
        {
            public UnobtrusiveApplicationApiFilter(IServiceProvider sp) : base(sp)
            {
            }

            protected override bool AllowUnobtrusive => true;
        }

        /// <summary>
        /// An implementation of <see cref="ApplicationApiFilter"/> that forces the setting of LastAccess of the user
        /// </summary>
        private class ObtrusiveApplicationApiFilter : ApplicationApiFilter
        {
            public ObtrusiveApplicationApiFilter(IServiceProvider sp) : base(sp)
            {
            }

            protected override bool AllowUnobtrusive => false;
        }

        #endregion
    };
}

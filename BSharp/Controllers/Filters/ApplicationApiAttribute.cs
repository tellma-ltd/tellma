using BSharp.Controllers.Dto;
using BSharp.Data;
using BSharp.Services.ApiAuthentication;
using BSharp.Services.Identity;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    /// <summary>
    /// 1. Ensures that the API caller has supplied a valid tenantId value, otherwise the request is aborted with a 400
    /// 2. Ensures that the authenticated user has an active membership in that tenantId otherwise the request is aborted with a 403
    /// 3. If the user is new it updates his/her ExternalId in the tenant database as well as the centralized admin database
    /// 4. If the user has a new email it updates his/her Email in the app database
    /// 5. Add the tenant info in the HTTP context, making it accessible to our model metadata provider
    /// 6. Ensures that the <see cref="IDefinitionsCache"/> is nice and fresh
    /// 7. If the version headers are provided, it also checks their freshness and adds appropriate response headers
    /// IMPORTANT: This attribute should always be precedede with another attribute <see cref="AuthorizeAccessAttribute"/>
    /// </summary>
    public class ApplicationApiAttribute : TypeFilterAttribute
    {
        public ApplicationApiAttribute() : base(typeof(ApplicationApiFilter)) { }

        /// <summary>
        /// An implementation of the method described here https://bit.ly/2MKwY7A
        /// </summary>
        private class ApplicationApiFilter : IAsyncResourceFilter
        {
            private readonly ApplicationRepository _appRepo;
            private readonly ITenantIdAccessor _tenantIdAccessor;
            private readonly ITenantInfoAccessor _tenantInfoAccessor;
            private readonly IExternalUserAccessor _externalUserAccessor;
            private readonly IServiceProvider _serviceProvider;
            private readonly IDefinitionsCache _definitionsProvider;

            public ApplicationApiFilter(ITenantIdAccessor tenantIdAccessor, ApplicationRepository appRepo, ITenantInfoAccessor tenantInfoAccessor,
                IExternalUserAccessor externalUserAccessor, IServiceProvider serviceProvider, IDefinitionsCache definitionsProvider)
            {
                _appRepo = appRepo;
                _tenantIdAccessor = tenantIdAccessor;
                _tenantInfoAccessor = tenantInfoAccessor;
                _externalUserAccessor = externalUserAccessor;
                _serviceProvider = serviceProvider;
                _definitionsProvider = definitionsProvider;
            }

            public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
            {
                // (1) Make sure the API caller have provided a tenantId, and extract it
                int tenantId;
                try
                {
                    tenantId = _tenantIdAccessor.GetTenantId();
                }
                catch (MultitenancyException ex)
                {
                    // If the tenant Id is not provided cut the pipeline short and return a Bad Request 400
                    context.Result = new BadRequestObjectResult(ex.Message);
                    return;
                }

                // (2) Make sure the user is a member of this tenant
                UserInfo userInfo = await _appRepo.GetUserInfoAsync();

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
                    await adminRepo.GlobalUsers__SetExternalIdByEmail(externalEmail, externalId);
                }

                else if (userInfo.ExternalId != externalId)
                {
                    // Note: there is the edge case of identity providers who allow email recycling. I.e. we can get the same email twice with 
                    // two different external Ids. This issue is so unlikely to naturally occur and cause problems here that we are not going
                    // to handle it for now. It can however happen artificually if the application is re-configured to a new identity provider,
                    // or if someone messed with the database directly, but again out of scope for now.
                    context.Result = new BadRequestObjectResult("The sign-in email already exists but with a different external Id");
                    return;
                }

                // (4) If the user's email address has changed at the identity server, update it locally
                else if (userInfo.Email != externalEmail)
                {
                    await _appRepo.Users__SetEmailByUserId(userId, externalEmail);
                }

                // (5) Set the tenant info in the context, to make accessible for model metadata providers
                var tenantInfo = await _appRepo.GetTenantInfoAsync();
                _tenantInfoAccessor.SetInfo(tenantId, tenantInfo);

                // (6) Ensure the freshness of the definitions cache
                {
                    var databaseVersion = tenantInfo.DefinitionsVersion;
                    var serverVersion = _definitionsProvider.GetDefinitionsIfCached(tenantId)?.Version;

                    if (serverVersion == null || serverVersion != databaseVersion)
                    {
                        // Update the cache
                        var definitions = await LoadDefinitions(_appRepo);
                        _definitionsProvider.SetDefinitions(tenantId, definitions);
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

            private async Task<DataWithVersion<DefinitionsForClient>> LoadDefinitions(ApplicationRepository appRepo)
            {
                // TODO: Replace mock with real
                var result = new DefinitionsForClient
                {
                    Documents = new Dictionary<string, DocumentDefinitionForClient>
                    {
                        //["journal-vouchers"] = new DocumentDefinitionForClient
                        //{
                        //    IsSourceDocument = true,
                        //    FinalState = "Posted",


                        //    // TODO: implement mock
                        //}
                    },

                    Resources = new Dictionary<string, ResourceDefinitionForClient>
                    {
                        ["raw-materials"] = new ResourceDefinitionForClient
                        {
                            TitlePlural = "Raw Materials",
                            TitlePlural2 = "مواد خام",
                            TitlePlural3 = "原料",
                            TitleSingular = "Raw Material",
                            TitleSingular2 = "مادة خام",
                            TitleSingular3 = "原料",
                            MainMenuIcon = "clipboard",
                            MainMenuSection = "Financials",
                            MainMenuSortKey = 203m,

                            ResourceLookup1_Label = "Thickness",
                            ResourceLookup1_Label2 = "السماكة",
                            ResourceLookup1_Visibility = Visibility.Visible,
                            ResourceLookup1_DefinitionId = "thicknesses",

                            MassUnit_Visibility = Visibility.Visible,
                            VolumeUnit_Visibility = Visibility.Visible,
                            AreaUnit_Visibility = Visibility.Visible,
                            LengthUnit_Visibility = Visibility.Visible,
                            TimeUnit_Visibility = Visibility.Visible,
                            CountUnit_Visibility = Visibility.Visible,
                            Memo_Visibility = Visibility.Visible,
                            Memo_DefaultValue = "My default memo",
                        },
                        ["finished-goods"] = new ResourceDefinitionForClient
                        {
                            TitlePlural = "Finished Goods",
                            TitlePlural2 = "سلع مصنعة",
                            TitleSingular = "Finished Good",
                            TitleSingular2 = "سلعة مصنعة",
                            TitlePlural3 = "完成品",
                            TitleSingular3 = "成品",
                            MainMenuIcon = "truck",
                            MainMenuSection = "Financials",
                            MainMenuSortKey = 204m,
                            ResourceLookup1_Label = "Color",
                            ResourceLookup1_Label2 = "اللون",
                            ResourceLookup1_Visibility = Visibility.Visible,
                            ResourceLookup1_DefinitionId = "colors"
                        }
                    },

                    Lines = new Dictionary<string, LineTypeForClient>
                    {
                        //["bla"] = new LineDefinitionForClient
                        //{
                        //    // TODO: implement mock
                        //}
                    },

                    Accounts = new Dictionary<string, AccountDefinitionForClient>
                    {
                        ["gl-accounts"] = new AccountDefinitionForClient
                        {
                            TitleSingular = "G/L Account",
                            TitleSingular2 = "حساب عام",
                            TitleSingular3 = "基本账户",
                            TitlePlural = "G/L Accounts",
                            TitlePlural2 = "حسابات عامة",
                            TitlePlural3 = "基本账户",
                            MainMenuIcon = "coins",
                            MainMenuSection = "Financials",
                            MainMenuSortKey = 206m,
                            Custodian_Visibility = AccountVisibility.RequiredInAccounts,
                            //Custodian_Label = "My Custodian",
                            Location_Visibility = AccountVisibility.RequiredInAccounts,
                            Location_Label = "My Location",
                            Resource_Visibility = AccountVisibility.RequiredInAccounts,
                            //Resource_Label = "My Resource",
                            Resource_DefinitionList = "finished-goods",
                            PartyReference_Visibility = AccountVisibility.RequiredInAccounts,
                            PartyReference_Label = "Barcode",
                            PartyReference_Label2 = "الباركود",
                            PartyReference_Label3 = "条码",
                            ResponsibilityCenter_Visibility = AccountVisibility.RequiredInAccounts,
                            ResponsibilityCenter_Label = "My Responsibility Center",
                        }
                    },

                    Lookups = new Dictionary<string, LookupDefinitionForClient>
                    {
                        ["colors"] = new LookupDefinitionForClient
                        {
                            TitleSingular = "Color",
                            TitleSingular2 = "لون",
                            TitleSingular3 = "颜色",
                            TitlePlural = "Colors",
                            TitlePlural2 = "ألوان",
                            TitlePlural3 = "颜色",
                            MainMenuIcon = "list",
                            MainMenuSection = "Administration",
                            MainMenuSortKey = 202m
                        },
                        ["thicknesses"] = new LookupDefinitionForClient
                        {
                            TitleSingular = "Thickness",
                            TitleSingular2 = "سماكة",
                            TitleSingular3 = "厚度",
                            TitlePlural = "Thicknesses",
                            TitlePlural2 = "سماكات",
                            TitlePlural3 = "厚度",
                            MainMenuIcon = "list",
                            MainMenuSection = "Administration",
                            MainMenuSortKey = 101m
                        }
                    },

                    Reports = new Dictionary<string, ReportDefinitionForClient>
                    {
                        ["my-amazing-report"] = new ReportDefinitionForClient
                        {
                            Title = "My Amazing Report",
                            Title2 = "تقريري المذهل",
                            Title3 = "我的惊人报告",
                            MainMenuIcon = "chart-pie",
                            MainMenuSection = "Financials",
                            MainMenuSortKey = 202m,

                            Type = ReportType.Summary,
                            Chart = "Line",
                            DefaultsToChart = false,
                            Collection = "MeasurementUnit",
                            Filter = "UnitType eq @UnitType and (Name contains @Name or Name2 contains @Name or Name3 contains @Name)",
                            Parameters = new List<ReportParameterDefinitionForClient>
                            {
                                new ReportParameterDefinitionForClient
                                {
                                    Key = "Name", // "FromDate"
                                    Label = "Name Contains",
                                    Label2 = "الإسم يحتوي",
                                    Label3 = "我的密",
                                    IsRequired = false
                                },
                            },
                            Columns = new List<ReportDimensionDefinitionForClient>
                            {
                                //new ReportDimensionDefinition
                                //{
                                //    Path = "ModifiedBy",
                                //    Label = "Modified By",
                                //    Label2 = "آخر تعديل",
                                //    Label3 = "我的密",
                                //    AutoExpand = true,
                                //},
                                //new ReportDimensionDefinition
                                //{
                                //    Path = "UnitType",
                                //    Label = "Unit Type",
                                //    Label2 = "نوع الوحدة",
                                //    Label3 = "我的密",
                                //    OrderDirection = "desc",
                                //    AutoExpand =true
                                //},
                            },
                            Rows = new List<ReportDimensionDefinitionForClient>
                            {
                                new ReportDimensionDefinitionForClient
                                {
                                    Path = "CreatedBy",
                                    Label = "Created By",
                                    Label2 = "إنشاء من قبل",
                                    Label3 = "我的密",
                                    AutoExpand = true,
                                },
                                new ReportDimensionDefinitionForClient
                                {
                                    Path = "UnitType",
                                    Label = "Unit Type",
                                    Label2 = "نوع الوحدة",
                                    Label3 = "我的密",
                                    OrderDirection = "desc",
                                    AutoExpand = true
                                },
                            },
                            Measures = new List<ReportMeasureDefinitionForClient>
                            {
                                new ReportMeasureDefinitionForClient
                                {
                                    Path = "Id",
                                    Aggregation = "count",
                                    Label = "Count",
                                    Label2 = "العدد",
                                    Label3 = "我的密"
                                },
                                new ReportMeasureDefinitionForClient
                                {
                                    Path = "Id",
                                    Aggregation = "avg",
                                    Label = "Average",
                                    Label2 = "المعدل",
                                    Label3 = "我的密"
                                },
                                //new ReportMeasureDefinition
                                //{
                                //    Path = "Name",
                                //    Aggregation = "max",
                                //    Label = "Max",
                                //    Label2 = "الأقصى",
                                //    Label3 = "我的密"
                                //},
                            },
                            ShowColumnsTotal = true,
                            ShowRowsTotal = true,
                        },
                        ["my-incredible-report"] = new ReportDefinitionForClient
                        {
                            Title = "My Incredible Report",
                            Title2 = "تقريري المدهش",
                            Title3 = "我的惊人报告",
                            MainMenuIcon = "chart-pie",
                            MainMenuSection = "Financials",
                            MainMenuSortKey = 202m,

                            Type = ReportType.Summary,
                            Chart = "Card",
                            DefaultsToChart = true,
                            Collection = "Resource",
                            DefinitionId = "finished-goods",
                            // Filter = "Memo contains @Memo",
                            Parameters = new List<ReportParameterDefinitionForClient>
                            {
                                new ReportParameterDefinitionForClient
                                {
                                    Key = "Memo", // "FromDate"
                                    Label = "Memo Contains",
                                    Label2 = "الملاحظات تحتوي",
                                    Label3 = "我的密",
                                    IsRequired = false
                                }
                            },
                            Columns = new List<ReportDimensionDefinitionForClient>
                            {

                            },
                            Rows = new List<ReportDimensionDefinitionForClient>
                            {
                                //new ReportDimensionDefinition
                                //{
                                //    Path = "ModifiedBy",
                                //    Label = "Modified By",
                                //    Label2 = "آخر تعديل",
                                //    Label3 = "我的密",
                                //    AutoExpand =false,
                                //},
                                //new ReportDimensionDefinition
                                //{
                                //    Path = "ResourceLookup1",
                                //    //Label = "Modified By State",
                                //    //Label2 = "آخر تعديل",
                                //    //Label3 = "我的密",
                                //    OrderDirection = "desc",
                                //    AutoExpand = true,
                                //},
                                //new ReportDimensionDefinition
                                //{
                                //    Path = "ResourceClassification",
                                //    //Label = "Unit Type",
                                //    //Label2 = "نوع الوحدة",
                                //    //Label3 = "我的密",
                                //   //  OrderDirection = "desc",
                                //    AutoExpand =true
                                //},

                            },
                            Measures = new List<ReportMeasureDefinitionForClient>
                            {
                                new ReportMeasureDefinitionForClient
                                {
                                    Path = "Id",
                                    Aggregation = "count",
                                    Label = "Count",
                                    Label2 = "العدد",
                                    Label3 = "我的密"
                                },
                                //new ReportMeasureDefinition
                                //{
                                //    Path = "Id",
                                //    Aggregation = "avg",
                                //    Label = "Average",
                                //    Label2 = "المعدل",
                                //    Label3 = "我的密"
                                //}
                            },
                            ShowColumnsTotal = false,
                            ShowRowsTotal = true,
                        },
                        ["my-awesome-report"] = new ReportDefinitionForClient
                        {
                            Title = "My Awesome Report",
                            Title2 = "تقريري العجيب",
                            Title3 = "我的惊人报告",
                            MainMenuIcon = "chart-pie",
                            MainMenuSection = "Financials",
                            MainMenuSortKey = 203m,
                            // Top = 10,
                            Type = ReportType.Details,
                            Collection = "MeasurementUnit",
                            Filter = "UnitType eq @UnitType",
                            OrderBy = "BaseAmount desc",
                            Parameters = new List<ReportParameterDefinitionForClient>
                            {
                            },
                            Select = new List<ReportSelectDefinitionForClient>
                            {
                                new ReportSelectDefinitionForClient
                                {
                                    Path = ""
                                },
                                new ReportSelectDefinitionForClient
                                {
                                    Path = "Description"
                                },
                                new ReportSelectDefinitionForClient
                                {
                                    Path = "UnitType"
                                },
                                new ReportSelectDefinitionForClient
                                {
                                    Path = "CreatedBy"
                                },
                                new ReportSelectDefinitionForClient
                                {
                                    Path = "CreatedBy/State"
                                },
                                new ReportSelectDefinitionForClient
                                {
                                    Path = "BaseAmount",
                                    Label = "My Base Amount",
                                    Label2 = "مقداري الأساسي",
                                    Label3 = "我的惊人报告"
                                }
                            }
                        }
                    }
                };

                await Task.Delay(5); // To simulate database communication

                return new DataWithVersion<DefinitionsForClient>
                {
                    Data = result,
                    Version = appRepo.GetTenantInfo().DefinitionsVersion
                };
            }
        }
    };
}

using Tellma.Controllers.Dto;
using Tellma.Data;
using Tellma.Entities;
using Tellma.Services.ApiAuthentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Controllers
{
    [Route("api/definitions")]
    [AuthorizeAccess]
    [ApplicationApi]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class DefinitionsController : ControllerBase
    {
        // Private fields

        private readonly IDefinitionsCache _definitionsCache;
        private readonly ILogger<SettingsController> _logger;

        public DefinitionsController(IDefinitionsCache definitionsCache,
            ILogger<SettingsController> logger)
        {
            _definitionsCache = definitionsCache;
            _logger = logger;
        }


        [HttpGet("client")]
        public ActionResult<DataWithVersion<DefinitionsForClient>> DefinitionsForClient()
        {
            try
            {
                // Simply retrieves the cached definitions, which were refreshed by ApiController
                var result = _definitionsCache.GetCurrentDefinitionsIfCached();
                if (result == null)
                {
                    throw new InvalidOperationException("The definitions were missing from the cache");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        private static string MapVisibility(string visibility)
        {
            if (visibility == Visibility.None)
            {
                return null;
            }

            return visibility;
        }

        private static LookupDefinitionForClient MapLookupDefinition(LookupDefinition def)
        {
            return new LookupDefinitionForClient
            {
                MainMenuIcon = def.MainMenuIcon,
                MainMenuSortKey = def.MainMenuSortKey,
                MainMenuSection = def.MainMenuSection,
                TitlePlural = def.TitlePlural,
                TitlePlural2 = def.TitlePlural2,
                TitlePlural3 = def.TitlePlural3,
                TitleSingular = def.TitleSingular,
                TitleSingular2 = def.TitleSingular2,
                TitleSingular3 = def.TitleSingular3,
            };
        }

        private static AgentDefinitionForClient MapAgentDefinition(AgentDefinition def)
        {
            return new AgentDefinitionForClient
            {
                MainMenuIcon = def.MainMenuIcon,
                MainMenuSortKey = def.MainMenuSortKey,
                MainMenuSection = def.MainMenuSection,
                TitlePlural = def.TitlePlural,
                TitlePlural2 = def.TitlePlural2,
                TitlePlural3 = def.TitlePlural3,
                TitleSingular = def.TitleSingular,
                TitleSingular2 = def.TitleSingular2,
                TitleSingular3 = def.TitleSingular3,

                BankAccountNumberVisibility = MapVisibility(def.BankAccountNumberVisibility),
                BasicSalaryVisibility = MapVisibility(def.BasicSalaryVisibility),
                JobVisibility = MapVisibility(def.JobVisibility),
                OvertimeRateVisibility = MapVisibility(def.OvertimeRateVisibility),
                StartDateLabel = def.StartDateLabel,
                StartDateLabel2 = def.StartDateLabel2,
                StartDateLabel3 = def.StartDateLabel3,
                StartDateVisibility = MapVisibility(def.StartDateVisibility),
                TaxIdentificationNumberVisibility = MapVisibility(def.TaxIdentificationNumberVisibility),
                TransportationAllowanceVisibility = MapVisibility(def.TransportationAllowanceVisibility),
            };
        }

        private static ResourceDefinitionForClient MapResourceDefinition(ResourceDefinition def)
        {
            return new ResourceDefinitionForClient
            {
                MainMenuIcon = def.MainMenuIcon,
                MainMenuSortKey = def.MainMenuSortKey,
                MainMenuSection = def.MainMenuSection,
                TitlePlural = def.TitlePlural,
                TitlePlural2 = def.TitlePlural2,
                TitlePlural3 = def.TitlePlural3,
                TitleSingular = def.TitleSingular,
                TitleSingular2 = def.TitleSingular2,
                TitleSingular3 = def.TitleSingular3,

                IdentifierLabel = def.IdentifierLabel,
                IdentifierLabel2 = def.IdentifierLabel2,
                IdentifierLabel3 = def.IdentifierLabel3,
                IdentifierVisibility = MapVisibility(def.IdentifierVisibility),
                CurrencyVisibility = MapVisibility(def.CurrencyVisibility),
                CountUnitVisibility = MapVisibility(def.CountUnitVisibility),
                MassUnitVisibility = MapVisibility(def.MassUnitVisibility),
                VolumeUnitVisibility = MapVisibility(def.VolumeUnitVisibility),
                TimeUnitVisibility = MapVisibility(def.TimeUnitVisibility),
                DescriptionVisibility = MapVisibility(def.DescriptionVisibility),
                AvailableSinceLabel = def.AvailableSinceLabel,
                AvailableSinceLabel2 = def.AvailableSinceLabel2,
                AvailableSinceLabel3 = def.AvailableSinceLabel3,
                AvailableSinceVisibility = MapVisibility(def.AvailableSinceVisibility),
                AvailableTillLabel = def.AvailableTillLabel,
                AvailableTillLabel2 = def.AvailableTillLabel2,
                AvailableTillLabel3 = def.AvailableTillLabel3,
                AvailableTillVisibility = MapVisibility(def.AvailableTillVisibility),
                Lookup1Label = def.Lookup1Label,
                Lookup1Label2 = def.Lookup1Label2,
                Lookup1Label3 = def.Lookup1Label3,
                Lookup1Visibility = MapVisibility(def.Lookup1Visibility),
                Lookup1DefinitionId = def.Lookup1DefinitionId,
                Lookup2Label = def.Lookup2Label,
                Lookup2Label2 = def.Lookup2Label2,
                Lookup2Label3 = def.Lookup2Label3,
                Lookup2Visibility = MapVisibility(def.Lookup2Visibility),
                Lookup2DefinitionId = def.Lookup2DefinitionId,
                DueDateLabel = def.DueDateLabel,
                DueDateLabel2 = def.DueDateLabel2,
                DueDateLabel3 = def.DueDateLabel3,
                DueDateVisibility = MapVisibility(def.DueDateVisibility),
            };
        }

        public static async Task<DataWithVersion<DefinitionsForClient>> LoadDefinitionsForClient(ApplicationRepository repo)
        {
            var (version, lookupDefs, agentDefs, resourceDefs) = await repo.Definitions__Load();

            var result = new DefinitionsForClient
            {
                Lookups = lookupDefs.ToDictionary(def => def.Id, def => MapLookupDefinition(def)),

                Agents = agentDefs.ToDictionary(def => def.Id, def => MapAgentDefinition(def)),

                Resources = resourceDefs.ToDictionary(def => def.Id, def => MapResourceDefinition(def)),

                // TODO: Load these from the database as well

                Documents = new Dictionary<string, DocumentDefinitionForClient>
                {
                    ["manual-journal-vouchers"] = new DocumentDefinitionForClient
                    {
                        Prefix = "JV",
                        TitlePlural = "Manual Journal Vouchers",
                        TitlePlural2 = "قيود تسوية يدوية",
                        TitlePlural3 = "手动日记帐凭单",
                        TitleSingular = "Manual Journal Voucher",
                        TitleSingular2 = "قيد تسوية يدوي",
                        TitleSingular3 = "手动日记帐凭证",
                        MainMenuIcon = "exchange-alt",
                        MainMenuSection = "Financials",
                        MainMenuSortKey = 50m,

                        // TODO: implement mock
                    }
                },

                Lines = new Dictionary<string, LineTypeForClient>
                {
                    //["bla"] = new LineDefinitionForClient
                    //{
                    //    // TODO: implement mock
                    //}
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
                                    Visibility = Visibility.Optional
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
                                    Visibility = Visibility.Optional
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
                            //    Path = "Lookup1",
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

            return new DataWithVersion<DefinitionsForClient>
            {
                Data = result,
                Version = version.ToString()
            };
        }
    }
}

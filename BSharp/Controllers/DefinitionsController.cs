using BSharp.Controllers.Dto;
using BSharp.Data;
using BSharp.Services.ApiAuthentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BSharp.Controllers
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
        public ActionResult<DataWithVersion<SettingsForClient>> GetForClient()
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

        public static async Task<DataWithVersion<DefinitionsForClient>> LoadDefinitionsForClient(ApplicationRepository repo)
        {
            // TODO: Replace mock with real
            var result = new DefinitionsForClient
            {
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

                Agents = new Dictionary<string, AgentDefinitionForClient>
                {
                    ["customers"] = new AgentDefinitionForClient
                    {
                        TitlePlural = "Customers",
                        TitlePlural2 = "عملاء",
                        TitlePlural3 = "个人",
                        TitleSingular = "Customer",
                        TitleSingular2 = "فرد",
                        TitleSingular3 = "个人",
                        MainMenuIcon = "clipboard",
                        MainMenuSection = "Financials",
                        MainMenuSortKey = 300m,

                        OperatingSegmentVisibility = Visibility.Optional,
                        TaxIdentificationNumberVisibility = Visibility.Optional,
                        StartDateVisibility = Visibility.Optional,
                        StartDateLabel = "Birth Date",
                        StartDateLabel2 = "تاريخ الميلاد",
                    },
                    ["employees"] = new AgentDefinitionForClient
                    {
                        TitlePlural = "Employees",
                        TitlePlural2 = "موظفون",
                        TitlePlural3 = "组织机构",
                        TitleSingular = "Employee",
                        TitleSingular2 = "مؤسسة",
                        TitleSingular3 = "组织",
                        MainMenuIcon = "user-friends",
                        MainMenuSection = "Financials",
                        MainMenuSortKey = 400m,

                        OperatingSegmentVisibility = Visibility.Optional,
                        TaxIdentificationNumberVisibility = Visibility.Optional,
                        StartDateVisibility = Visibility.Optional,
                        StartDateLabel = "Birth Date",
                        StartDateLabel2 = "تاريخ الميلاد",
                        JobVisibility = Visibility.Optional,
                        BasicSalaryVisibility = Visibility.Optional,
                        TransportationAllowanceVisibility = Visibility.Optional,
                        OvertimeRateVisibility = Visibility.Optional,
                        BankAccountNumberVisibility = Visibility.Optional,
                    },
                    ["suppliers"] = new AgentDefinitionForClient
                    {
                        TitlePlural = "Suppliers",
                        TitlePlural2 = "موردون",
                        TitlePlural3 = "组织机构",
                        TitleSingular = "Supplier",
                        TitleSingular2 = "مورد",
                        TitleSingular3 = "组织",
                        MainMenuIcon = "clipboard",
                        MainMenuSection = "Financials",
                        MainMenuSortKey = 500m,

                        OperatingSegmentVisibility = Visibility.Optional,
                        TaxIdentificationNumberVisibility = Visibility.Required,
                        BankAccountNumberVisibility = Visibility.Optional,
                    }
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

                        IdentifierVisibility = Visibility.Optional,
                        IdentifierLabel = "Tag",

                        CurrencyVisibility = Visibility.Optional,
                        // CurrencyLabel = "My Currency",

                        MonetaryValueVisibility = Visibility.Optional,
                        MonetaryValueLabel = "Price",

                        MassUnitVisibility = Visibility.Optional,
                        MassVisibility = Visibility.Optional,
                        CountUnitVisibility = Visibility.Optional,
                        CountVisibility = Visibility.Optional,
                        TimeUnitVisibility = Visibility.Optional,
                        TimeVisibility = Visibility.Optional,
                        VolumeUnitVisibility = Visibility.Optional,
                        VolumeVisibility = Visibility.Optional,

                        OperatingSegmentVisibility = Visibility.Optional,
                        AvailableSinceVisibility = Visibility.Optional,
                        AvailableTillVisibility = Visibility.Optional,
                        DescriptionVisibility = Visibility.Optional,


                        Lookup1Label = "Thickness",
                        Lookup1Label2 = "السماكة",
                        Lookup1Visibility = Visibility.Optional,
                        Lookup1DefinitionId = "steel-thicknesses",

                        Lookup2Label = "Color",
                        Lookup2Label2 = "اللون",
                        Lookup2Visibility = Visibility.Optional,
                        Lookup2DefinitionId = "body-colors",

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
                        Lookup1Label = "Color",
                        Lookup1Label2 = "اللون",
                        Lookup1Visibility = Visibility.Optional,
                        Lookup1DefinitionId = "colors"
                    },
                    ["currencies"] = new ResourceDefinitionForClient
                    {
                        TitlePlural = "Currencies",
                        TitlePlural2 = "عملات",
                        TitleSingular = "Currency",
                        TitleSingular2 = "عملة",
                        TitlePlural3 = "完成品",
                        TitleSingular3 = "成品",
                        MainMenuIcon = "euro-sign",
                        MainMenuSection = "Financials",
                        MainMenuSortKey = 205m,
                    }
                },

                Lines = new Dictionary<string, LineTypeForClient>
                {
                    //["bla"] = new LineDefinitionForClient
                    //{
                    //    // TODO: implement mock
                    //}
                },

                Lookups = new Dictionary<string, LookupDefinitionForClient>
                {
                    ["body-colors"] = new LookupDefinitionForClient
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
                    ["steel-thicknesses"] = new LookupDefinitionForClient
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

            await Task.Delay(5); // To simulate database communication

            return new DataWithVersion<DefinitionsForClient>
            {
                Data = result,
                Version = repo.GetTenantInfo().DefinitionsVersion
            };
        }
    }
}

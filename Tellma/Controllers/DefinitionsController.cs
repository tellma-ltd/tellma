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
                MainMenuSortKey = def.MainMenuSortKey ?? 0m,
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
                MainMenuSortKey = def.MainMenuSortKey ?? 0m,
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
                MainMenuSortKey = def.MainMenuSortKey ?? 0m,
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

        private static ReportDefinitionForClient MapReportDefinition(ReportDefinition def)
        {
            return new ReportDefinitionForClient
            {
                // Basics
                Collection = def.Collection,
                DefinitionId = def.DefinitionId,
                Type = def.Type,

                // Data
                Rows = def.Rows?.Select(r => new ReportDimensionDefinitionForClient
                {
                    Path = r.Path,
                    Label = r.Label,
                    Label2 = r.Label2,
                    Label3 = r.Label3,
                    AutoExpand = r.AutoExpand ?? false,
                    Modifier = r.Modifier,
                    OrderDirection = r.OrderDirection, 
                }).ToList(),
                ShowRowsTotal = def.ShowRowsTotal ?? false,

                Columns = def.Columns?.Select(c => new ReportDimensionDefinitionForClient
                {
                    Path = c.Path,
                    Label = c.Label,
                    Label2 = c.Label2,
                    Label3 = c.Label3,
                    AutoExpand = c.AutoExpand ?? false,
                    Modifier = c.Modifier,
                    OrderDirection = c.OrderDirection,
                }).ToList(),
                ShowColumnsTotal = def.ShowColumnsTotal ?? false,

                Measures = def.Measures?.Select(m => new ReportMeasureDefinitionForClient
                {
                    Path = m.Path,
                    Label = m.Label,
                    Label2 = m.Label2,
                    Label3 = m.Label3,
                    OrderDirection = m.OrderDirection,
                    Aggregation = m.Aggregation,
                }).ToList(),

                Select = def.Select?.Select(s => new ReportSelectDefinitionForClient
                {
                    Path = s.Path,
                    Label = s.Label,
                    Label2 = s.Label2,
                    Label3 = s.Label3,
                }).ToList(),

                OrderBy = def.OrderBy,
                Top = def.Top ?? 0,

                // Filter
                Filter = def.Filter,
                Parameters = def.Parameters?.Select(p => new ReportParameterDefinitionForClient
                {
                    Key = p.Key,
                    Label = p.Label,
                    Label2 = p.Label2,
                    Label3 = p.Label3,
                    Visibility = p.Visibility,
                    Value = p.Value,
                }).ToList(),

                // Chart
                Chart = def.Chart,
                DefaultsToChart = def.DefaultsToChart ?? false,

                // Title
                Title = def.Title,
                Title2 = def.Title2,
                Title3 = def.Title3,
                Description = def.Description,
                Description2 = def.Description2,
                Description3 = def.Description3,

                // Main Menu
                ShowInMainMenu = def.ShowInMainMenu ?? false,
                MainMenuIcon = def.MainMenuIcon,
                MainMenuSortKey = def.MainMenuSortKey ?? 0m,
                MainMenuSection = def.MainMenuSection,
            };
        }

        public static async Task<DataWithVersion<DefinitionsForClient>> LoadDefinitionsForClient(ApplicationRepository repo)
        {
            var (version, lookupDefs, agentDefs, resourceDefs, reportDefs) = await repo.Definitions__Load();

            var result = new DefinitionsForClient
            {
                Lookups = lookupDefs.ToDictionary(def => def.Id, def => MapLookupDefinition(def)),

                Agents = agentDefs.ToDictionary(def => def.Id, def => MapAgentDefinition(def)),

                Resources = resourceDefs.ToDictionary(def => def.Id, def => MapResourceDefinition(def)),

                Reports = reportDefs.ToDictionary(def => def.Id, def => MapReportDefinition(def)),

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
            };

            return new DataWithVersion<DefinitionsForClient>
            {
                Data = result,
                Version = version.ToString()
            };
        }

    }
}

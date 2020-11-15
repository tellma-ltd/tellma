using Tellma.Controllers.Dto;
using Tellma.Data;
using Tellma.Entities;
using Tellma.Services.ApiAuthentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Azure;

namespace Tellma.Controllers
{
    [Route("api/definitions")]
    [AuthorizeJwtBearer]
    [ApplicationController(allowUnobtrusive: true)]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class DefinitionsController : ControllerBase
    {
        private readonly IDefinitionsCache _definitionsCache;
        private readonly ILogger<SettingsController> _logger;

        public DefinitionsController(IDefinitionsCache definitionsCache, ILogger<SettingsController> logger)
        {
            _definitionsCache = definitionsCache;
            _logger = logger;
        }

        [HttpGet("client")]
        public ActionResult<Versioned<DefinitionsForClient>> DefinitionsForClient()
        {
            try
            {
                // Simply retrieves the cached definitions, which were refreshed by ApiController
                // We ask for the latest cached version, not the one at the beginning of the request which may have changed
                var result = _definitionsCache.GetCurrentDefinitionsIfCached(forceFresh: true);
                if (result == null)
                {
                    throw new InvalidOperationException("The definitions were missing from the cache");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error caught in {nameof(DefinitionsForClient)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }
    }

    public class DefinitionsService : ServiceBase
    {
        private const string ManualLine = nameof(ManualLine);
        private const string ManualJournalVoucher = nameof(ManualJournalVoucher);

        private static string MapVisibility(string visibility)
        {
            if (visibility == Visibility.None)
            {
                return null;
            }

            return visibility;
        }

        private static string MapCardinality(string cardinality)
        {
            if (cardinality == Cardinality.None)
            {
                return null;
            }

            return cardinality;
        }

        private static LookupDefinitionForClient MapLookupDefinition(LookupDefinition def)
        {
            return new LookupDefinitionForClient
            {
                State = def.State,
                Code = def.Code,
                MainMenuIcon = def.MainMenuIcon,
                MainMenuSortKey = def.MainMenuSortKey ?? 0m,
                MainMenuSection = def.MainMenuSection,
                TitlePlural = def.TitlePlural,
                TitlePlural2 = def.TitlePlural2,
                TitlePlural3 = def.TitlePlural3,
                TitleSingular = def.TitleSingular,
                TitleSingular2 = def.TitleSingular2,
                TitleSingular3 = def.TitleSingular3,

                ReportDefinitions = def.ReportDefinitions?.Select(e => new DefinitionReportDefinitionForClient
                {
                    ReportDefinitionId = e.ReportDefinitionId.Value,
                    Name = e.Name,
                    Name2 = e.Name2,
                    Name3 = e.Name3,

                })?.ToList() ?? new List<DefinitionReportDefinitionForClient>()
            };
        }

        private static RelationDefinitionForClient MapRelationDefinition(RelationDefinition def)
        {
            return new RelationDefinitionForClient
            {
                State = def.State,
                Code = def.Code,
                MainMenuIcon = def.MainMenuIcon,
                MainMenuSortKey = def.MainMenuSortKey ?? 0m,
                MainMenuSection = def.MainMenuSection,
                TitlePlural = def.TitlePlural,
                TitlePlural2 = def.TitlePlural2,
                TitlePlural3 = def.TitlePlural3,
                TitleSingular = def.TitleSingular,
                TitleSingular2 = def.TitleSingular2,
                TitleSingular3 = def.TitleSingular3,

                CurrencyVisibility = MapVisibility(def.CurrencyVisibility),
                DescriptionVisibility = MapVisibility(def.DescriptionVisibility),
                LocationVisibility = MapVisibility(def.LocationVisibility),
                ImageVisibility = MapVisibility(def.ImageVisibility),
                CenterVisibility = MapVisibility(def.CenterVisibility),

                FromDateLabel = def.FromDateLabel,
                FromDateLabel2 = def.FromDateLabel2,
                FromDateLabel3 = def.FromDateLabel3,
                FromDateVisibility = MapVisibility(def.FromDateVisibility),
                ToDateLabel = def.ToDateLabel,
                ToDateLabel2 = def.ToDateLabel2,
                ToDateLabel3 = def.ToDateLabel3,
                ToDateVisibility = MapVisibility(def.ToDateVisibility),


                DateOfBirthVisibility = MapVisibility(def.DateOfBirthVisibility),
                ContactEmailVisibility = MapVisibility(def.ContactEmailVisibility),
                ContactMobileVisibility = MapVisibility(def.ContactMobileVisibility),
                ContactAddressVisibility = MapVisibility(def.ContactAddressVisibility),

                Date1Label = def.Date1Label,
                Date1Label2 = def.Date1Label2,
                Date1Label3 = def.Date1Label3,
                Date1Visibility = MapVisibility(def.Date1Visibility),
                Date2Label = def.Date2Label,
                Date2Label2 = def.Date2Label2,
                Date2Label3 = def.Date2Label3,
                Date2Visibility = MapVisibility(def.Date2Visibility),
                Date3Label = def.Date3Label,
                Date3Label2 = def.Date3Label2,
                Date3Label3 = def.Date3Label3,
                Date3Visibility = MapVisibility(def.Date3Visibility),
                Date4Label = def.Date4Label,
                Date4Label2 = def.Date4Label2,
                Date4Label3 = def.Date4Label3,
                Date4Visibility = MapVisibility(def.Date4Visibility),



                Decimal1Label = def.Decimal1Label,
                Decimal1Label2 = def.Decimal1Label2,
                Decimal1Label3 = def.Decimal1Label3,
                Decimal1Visibility = MapVisibility(def.Decimal1Visibility),

                Decimal2Label = def.Decimal2Label,
                Decimal2Label2 = def.Decimal2Label2,
                Decimal2Label3 = def.Decimal2Label3,
                Decimal2Visibility = MapVisibility(def.Decimal2Visibility),

                Int1Label = def.Int1Label,
                Int1Label2 = def.Int1Label2,
                Int1Label3 = def.Int1Label3,
                Int1Visibility = MapVisibility(def.Int1Visibility),

                Int2Label = def.Int2Label,
                Int2Label2 = def.Int2Label2,
                Int2Label3 = def.Int2Label3,
                Int2Visibility = MapVisibility(def.Int2Visibility),

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

                Lookup3Label = def.Lookup3Label,
                Lookup3Label2 = def.Lookup3Label2,
                Lookup3Label3 = def.Lookup3Label3,
                Lookup3Visibility = MapVisibility(def.Lookup3Visibility),
                Lookup3DefinitionId = def.Lookup3DefinitionId,

                Lookup4Label = def.Lookup4Label,
                Lookup4Label2 = def.Lookup4Label2,
                Lookup4Label3 = def.Lookup4Label3,
                Lookup4Visibility = MapVisibility(def.Lookup4Visibility),
                Lookup4DefinitionId = def.Lookup4DefinitionId,

                Lookup5Label = def.Lookup5Label,
                Lookup5Label2 = def.Lookup5Label2,
                Lookup5Label3 = def.Lookup5Label3,
                Lookup5Visibility = MapVisibility(def.Lookup5Visibility),
                Lookup5DefinitionId = def.Lookup5DefinitionId,

                Lookup6Label = def.Lookup6Label,
                Lookup6Label2 = def.Lookup6Label2,
                Lookup6Label3 = def.Lookup6Label3,
                Lookup6Visibility = MapVisibility(def.Lookup6Visibility),
                Lookup6DefinitionId = def.Lookup6DefinitionId,

                Lookup7Label = def.Lookup7Label,
                Lookup7Label2 = def.Lookup7Label2,
                Lookup7Label3 = def.Lookup7Label3,
                Lookup7Visibility = MapVisibility(def.Lookup7Visibility),
                Lookup7DefinitionId = def.Lookup7DefinitionId,

                Lookup8Label = def.Lookup8Label,
                Lookup8Label2 = def.Lookup8Label2,
                Lookup8Label3 = def.Lookup8Label3,
                Lookup8Visibility = MapVisibility(def.Lookup8Visibility),
                Lookup8DefinitionId = def.Lookup8DefinitionId,

                Text1Label = def.Text1Label,
                Text1Label2 = def.Text1Label2,
                Text1Label3 = def.Text1Label3,
                Text1Visibility = MapVisibility(def.Text1Visibility),

                Text2Label = def.Text2Label,
                Text2Label2 = def.Text2Label2,
                Text2Label3 = def.Text2Label3,
                Text2Visibility = MapVisibility(def.Text2Visibility),

                Text3Label = def.Text3Label,
                Text3Label2 = def.Text3Label2,
                Text3Label3 = def.Text3Label3,
                Text3Visibility = MapVisibility(def.Text3Visibility),

                Text4Label = def.Text4Label,
                Text4Label2 = def.Text4Label2,
                Text4Label3 = def.Text4Label3,
                Text4Visibility = MapVisibility(def.Text4Visibility),

                // Relation Only
                Relation1Label = def.Relation1Label,
                Relation1Label2 = def.Relation1Label2,
                Relation1Label3 = def.Relation1Label3,
                Relation1Visibility = MapVisibility(def.Relation1Visibility),
                Relation1DefinitionId = def.Relation1DefinitionId,

                AgentVisibility = MapVisibility(def.AgentVisibility),
                TaxIdentificationNumberVisibility = MapVisibility(def.TaxIdentificationNumberVisibility),
                JobVisibility = MapVisibility(def.JobVisibility),
                BankAccountNumberVisibility = MapVisibility(def.BankAccountNumberVisibility),
                UserCardinality = MapCardinality(def.UserCardinality),

                ReportDefinitions = def.ReportDefinitions?.Select(e => new DefinitionReportDefinitionForClient
                {
                    ReportDefinitionId = e.ReportDefinitionId.Value,
                    Name = e.Name,
                    Name2 = e.Name2,
                    Name3 = e.Name3,

                })?.ToList() ?? new List<DefinitionReportDefinitionForClient>()
            };
        }

        private static CustodyDefinitionForClient MapCustodyDefinition(CustodyDefinition def)
        {
            return new CustodyDefinitionForClient
            {
                State = def.State,
                Code = def.Code,
                MainMenuIcon = def.MainMenuIcon,
                MainMenuSortKey = def.MainMenuSortKey ?? 0m,
                MainMenuSection = def.MainMenuSection,
                TitlePlural = def.TitlePlural,
                TitlePlural2 = def.TitlePlural2,
                TitlePlural3 = def.TitlePlural3,
                TitleSingular = def.TitleSingular,
                TitleSingular2 = def.TitleSingular2,
                TitleSingular3 = def.TitleSingular3,

                CurrencyVisibility = MapVisibility(def.CurrencyVisibility),
                DescriptionVisibility = MapVisibility(def.DescriptionVisibility),
                LocationVisibility = MapVisibility(def.LocationVisibility),
                ImageVisibility = MapVisibility(def.ImageVisibility),
                CenterVisibility = MapVisibility(def.CenterVisibility),

                FromDateLabel = def.FromDateLabel,
                FromDateLabel2 = def.FromDateLabel2,
                FromDateLabel3 = def.FromDateLabel3,
                FromDateVisibility = MapVisibility(def.FromDateVisibility),
                ToDateLabel = def.ToDateLabel,
                ToDateLabel2 = def.ToDateLabel2,
                ToDateLabel3 = def.ToDateLabel3,
                ToDateVisibility = MapVisibility(def.ToDateVisibility),

                Decimal1Label = def.Decimal1Label,
                Decimal1Label2 = def.Decimal1Label2,
                Decimal1Label3 = def.Decimal1Label3,
                Decimal1Visibility = MapVisibility(def.Decimal1Visibility),

                Decimal2Label = def.Decimal2Label,
                Decimal2Label2 = def.Decimal2Label2,
                Decimal2Label3 = def.Decimal2Label3,
                Decimal2Visibility = MapVisibility(def.Decimal2Visibility),

                Int1Label = def.Int1Label,
                Int1Label2 = def.Int1Label2,
                Int1Label3 = def.Int1Label3,
                Int1Visibility = MapVisibility(def.Int1Visibility),

                Int2Label = def.Int2Label,
                Int2Label2 = def.Int2Label2,
                Int2Label3 = def.Int2Label3,
                Int2Visibility = MapVisibility(def.Int2Visibility),

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

                Lookup3Label = def.Lookup3Label,
                Lookup3Label2 = def.Lookup3Label2,
                Lookup3Label3 = def.Lookup3Label3,
                Lookup3Visibility = MapVisibility(def.Lookup3Visibility),
                Lookup3DefinitionId = def.Lookup3DefinitionId,

                Lookup4Label = def.Lookup4Label,
                Lookup4Label2 = def.Lookup4Label2,
                Lookup4Label3 = def.Lookup4Label3,
                Lookup4Visibility = MapVisibility(def.Lookup4Visibility),
                Lookup4DefinitionId = def.Lookup4DefinitionId,

                Text1Label = def.Text1Label,
                Text1Label2 = def.Text1Label2,
                Text1Label3 = def.Text1Label3,
                Text1Visibility = MapVisibility(def.Text1Visibility),

                Text2Label = def.Text2Label,
                Text2Label2 = def.Text2Label2,
                Text2Label3 = def.Text2Label3,
                Text2Visibility = MapVisibility(def.Text2Visibility),

                // Custody Only

                CustodianVisibility = MapVisibility(def.CustodianVisibility),
                CustodianDefinitionId = def.CustodianDefinitionId,

                ExternalReferenceLabel = def.ExternalReferenceLabel,
                ExternalReferenceLabel2 = def.ExternalReferenceLabel2,
                ExternalReferenceLabel3 = def.ExternalReferenceLabel3,
                ExternalReferenceVisibility = MapVisibility(def.ExternalReferenceVisibility),

                ReportDefinitions = def.ReportDefinitions?.Select(e => new DefinitionReportDefinitionForClient
                {
                    ReportDefinitionId = e.ReportDefinitionId.Value,
                    Name = e.Name,
                    Name2 = e.Name2,
                    Name3 = e.Name3,

                })?.ToList() ?? new List<DefinitionReportDefinitionForClient>()
            };
        }

        private static ResourceDefinitionForClient MapResourceDefinition(ResourceDefinition def)
        {
            return new ResourceDefinitionForClient
            {
                State = def.State,
                Code = def.Code,
                MainMenuIcon = def.MainMenuIcon,
                MainMenuSortKey = def.MainMenuSortKey ?? 0m,
                MainMenuSection = def.MainMenuSection,
                TitlePlural = def.TitlePlural,
                TitlePlural2 = def.TitlePlural2,
                TitlePlural3 = def.TitlePlural3,
                TitleSingular = def.TitleSingular,
                TitleSingular2 = def.TitleSingular2,
                TitleSingular3 = def.TitleSingular3,

                CurrencyVisibility = MapVisibility(def.CurrencyVisibility),
                DescriptionVisibility = MapVisibility(def.DescriptionVisibility),
                LocationVisibility = MapVisibility(def.LocationVisibility),
                ImageVisibility = MapVisibility(def.ImageVisibility),
                CenterVisibility = MapVisibility(def.CenterVisibility),
                CostCenterVisibility = MapVisibility(def.CostCenterVisibility),

                FromDateLabel = def.FromDateLabel,
                FromDateLabel2 = def.FromDateLabel2,
                FromDateLabel3 = def.FromDateLabel3,
                FromDateVisibility = MapVisibility(def.FromDateVisibility),
                ToDateLabel = def.ToDateLabel,
                ToDateLabel2 = def.ToDateLabel2,
                ToDateLabel3 = def.ToDateLabel3,
                ToDateVisibility = MapVisibility(def.ToDateVisibility),

                Decimal1Label = def.Decimal1Label,
                Decimal1Label2 = def.Decimal1Label2,
                Decimal1Label3 = def.Decimal1Label3,
                Decimal1Visibility = MapVisibility(def.Decimal1Visibility),

                Decimal2Label = def.Decimal2Label,
                Decimal2Label2 = def.Decimal2Label2,
                Decimal2Label3 = def.Decimal2Label3,
                Decimal2Visibility = MapVisibility(def.Decimal2Visibility),

                Int1Label = def.Int1Label,
                Int1Label2 = def.Int1Label2,
                Int1Label3 = def.Int1Label3,
                Int1Visibility = MapVisibility(def.Int1Visibility),

                Int2Label = def.Int2Label,
                Int2Label2 = def.Int2Label2,
                Int2Label3 = def.Int2Label3,
                Int2Visibility = MapVisibility(def.Int2Visibility),

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

                Lookup3Label = def.Lookup3Label,
                Lookup3Label2 = def.Lookup3Label2,
                Lookup3Label3 = def.Lookup3Label3,
                Lookup3Visibility = MapVisibility(def.Lookup3Visibility),
                Lookup3DefinitionId = def.Lookup3DefinitionId,

                Lookup4Label = def.Lookup4Label,
                Lookup4Label2 = def.Lookup4Label2,
                Lookup4Label3 = def.Lookup4Label3,
                Lookup4Visibility = MapVisibility(def.Lookup4Visibility),
                Lookup4DefinitionId = def.Lookup4DefinitionId,

                Text1Label = def.Text1Label,
                Text1Label2 = def.Text1Label2,
                Text1Label3 = def.Text1Label3,
                Text1Visibility = MapVisibility(def.Text1Visibility),

                Text2Label = def.Text2Label,
                Text2Label2 = def.Text2Label2,
                Text2Label3 = def.Text2Label3,
                Text2Visibility = MapVisibility(def.Text2Visibility),

                // Resource Only

                IdentifierLabel = def.IdentifierLabel,
                IdentifierLabel2 = def.IdentifierLabel2,
                IdentifierLabel3 = def.IdentifierLabel3,
                IdentifierVisibility = MapVisibility(def.IdentifierVisibility),

                VatRateVisibility = MapVisibility(def.VatRateVisibility),
                DefaultVatRate = def.DefaultVatRate,

                ReorderLevelVisibility = MapVisibility(def.ReorderLevelVisibility),
                EconomicOrderQuantityVisibility = MapVisibility(def.EconomicOrderQuantityVisibility),
                UnitCardinality = MapCardinality(def.UnitCardinality),
                DefaultUnitId = def.DefaultUnitId,
                UnitMassVisibility = MapVisibility(def.UnitMassVisibility),
                DefaultUnitMassUnitId = def.DefaultUnitMassUnitId,

                MonetaryValueVisibility = MapVisibility(def.MonetaryValueVisibility),
                ParticipantVisibility = MapVisibility(def.ParticipantVisibility),
                ParticipantDefinitionId = def.ParticipantDefinitionId,

                ReportDefinitions = def.ReportDefinitions?.Select(e => new DefinitionReportDefinitionForClient
                {
                    ReportDefinitionId = e.ReportDefinitionId.Value,
                    Name = e.Name,
                    Name2 = e.Name2,
                    Name3 = e.Name3,

                })?.ToList() ?? new List<DefinitionReportDefinitionForClient>()
            };
        }

        private static ReportDefinitionForClient MapReportDefinition(ReportDefinition def)
        {
            return new ReportDefinitionForClient
            {
                // Basics
                Code = def.Code,
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
                })?.ToList() ?? new List<ReportDimensionDefinitionForClient>(),
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
                })?.ToList() ?? new List<ReportDimensionDefinitionForClient>(),
                ShowColumnsTotal = def.ShowColumnsTotal ?? false,

                Measures = def.Measures?.Select(m => new ReportMeasureDefinitionForClient
                {
                    Path = m.Path,
                    Label = m.Label,
                    Label2 = m.Label2,
                    Label3 = m.Label3,
                    OrderDirection = m.OrderDirection,
                    Aggregation = m.Aggregation,
                })?.ToList() ?? new List<ReportMeasureDefinitionForClient>(),

                Select = def.Select?.Select(s => new ReportSelectDefinitionForClient
                {
                    Path = s.Path,
                    Label = s.Label,
                    Label2 = s.Label2,
                    Label3 = s.Label3,
                })?.ToList() ?? new List<ReportSelectDefinitionForClient>(),

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
                })?.ToList() ?? new List<ReportParameterDefinitionForClient>(),

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

        private static LineDefinitionForClient MapLineDefinition(LineDefinition def,
            Dictionary<int, List<int>> entryCustodianDefs,
            Dictionary<int, List<int>> entryCustodyDefs,
            Dictionary<int, List<int>> entryParticipantDefs,
            Dictionary<int, List<int>> entryResourceDefs)
        {
            var line = new LineDefinitionForClient
            {
                // Basics
                Code = def.Code,
                TitlePlural = def.TitlePlural,
                TitlePlural2 = def.TitlePlural2,
                TitlePlural3 = def.TitlePlural3,
                TitleSingular = def.TitleSingular,
                TitleSingular2 = def.TitleSingular2,
                TitleSingular3 = def.TitleSingular3,

                // Data
                AllowSelectiveSigning = def.AllowSelectiveSigning ?? false,
                ViewDefaultsToForm = def.ViewDefaultsToForm ?? false,
                GenerateScript = !string.IsNullOrWhiteSpace(def.GenerateScript),
                GenerateLabel = def.GenerateLabel,
                GenerateLabel2 = def.GenerateLabel2,
                GenerateLabel3 = def.GenerateLabel3,
                Entries = def.Entries?.Select(e => new LineDefinitionEntryForClient
                {
                    Direction = e.Direction.Value,
                    ParentAccountTypeId = e.ParentAccountTypeId,
                    EntryTypeId = e.EntryTypeId,
                    EntryTypeParentId = e.ParentAccountType?.EntryTypeParentId, // There is supposed to validation to make sure all selected account types have the same entry type parent Id

                    CustodianDefinitionIds = entryCustodianDefs.GetValueOrDefault(e.Id) ?? new List<int>(),
                    CustodyDefinitionIds = entryCustodyDefs.GetValueOrDefault(e.Id) ?? new List<int>(),
                    ParticipantDefinitionIds = entryParticipantDefs.GetValueOrDefault(e.Id) ?? new List<int>(),
                    ResourceDefinitionIds = entryResourceDefs.GetValueOrDefault(e.Id) ?? new List<int>(),
                })?.ToList() ?? new List<LineDefinitionEntryForClient>(),

                Columns = def.Columns?.Select(c => new LineDefinitionColumnForClient
                {
                    ColumnName = c.ColumnName,
                    EntryIndex = c.EntryIndex.Value,
                    Label = c.Label,
                    Label2 = c.Label2,
                    Label3 = c.Label3,
                    Filter = c.Filter,
                    ReadOnlyState = c.ReadOnlyState,
                    RequiredState = c.RequiredState,
                    InheritsFromHeader = c.InheritsFromHeader == 0 ? null : c.InheritsFromHeader,
                })?.ToList() ?? new List<LineDefinitionColumnForClient>(),

                StateReasons = def.StateReasons?.Select(r => new LineDefinitionStateReasonForClient
                {
                    Id = r.Id,
                    State = r.State,
                    Name = r.Name,
                    Name2 = r.Name2,
                    Name3 = r.Name3,
                    IsActive = r.IsActive ?? false,
                })?.ToList() ?? new List<LineDefinitionStateReasonForClient>(),

                GenerateParameters = def.GenerateParameters?.Select(p => new LineDefinitionGenerateParameterForClient
                {
                    Key = p.Key,
                    Label = p.Label,
                    Label2 = p.Label2,
                    Label3 = p.Label3,
                    DataType = p.DataType,
                    Filter = p.Filter,
                    Visibility = p.Visibility // This one can't be 'None'
                })?.ToList() ?? new List<LineDefinitionGenerateParameterForClient>(),
            };

            // For consistency, Manual lines do not have columns or entries
            if (line.Code == ManualLine)
            {
                line.Entries.Clear();
                line.Columns.Clear();
            }

            line.Columns.ForEach(col =>
            {
                if (col.ColumnName == nameof(Entry.CurrencyId) || col.ColumnName == nameof(Entry.CustodyId))
                {
                    col.RequiredState = LineState.Draft; // Those are required in the table => hard code as required
                }
            });

            return line;
        }

        private static DocumentDefinitionForClient MapDocumentDefinition(DocumentDefinition def, Dictionary<int, LineDefinitionForClient> lineDefsDic)
        {
            // IMPORTANT: Keep in sync with document-definitions-details.component.ts
            var result = new DocumentDefinitionForClient
            {
                State = def.State,
                Code = def.Code,
                IsOriginalDocument = def.IsOriginalDocument ?? false,
                DocumentType = def.DocumentType.Value,
                TitlePlural = def.TitlePlural,
                TitlePlural2 = def.TitlePlural2,
                TitlePlural3 = def.TitlePlural3,
                TitleSingular = def.TitleSingular,
                TitleSingular2 = def.TitleSingular2,
                TitleSingular3 = def.TitleSingular3,
                Prefix = def.Prefix,
                CodeWidth = def.CodeWidth ?? 4,

                MemoVisibility = MapVisibility(def.MemoVisibility),
                ClearanceVisibility = MapVisibility(def.ClearanceVisibility),

                CanReachState1 = def.CanReachState1 ?? false,
                CanReachState2 = def.CanReachState2 ?? false,
                CanReachState3 = def.CanReachState3 ?? false,
                HasWorkflow = def.HasWorkflow ?? false,

                LineDefinitions = def.LineDefinitions?.Select(d => new DocumentDefinitionLineDefinitionForClient
                {
                    LineDefinitionId = d.LineDefinitionId.Value,
                    IsVisibleByDefault = d.IsVisibleByDefault ?? false
                })?.ToList() ?? new List<DocumentDefinitionLineDefinitionForClient>(),

                MainMenuIcon = def.MainMenuIcon,
                MainMenuSortKey = def.MainMenuSortKey ?? 0m,
                MainMenuSection = def.MainMenuSection,

                // These should not be null
                ParticipantDefinitionIds = new List<int>(),
            };

            // Here we compute some values based on the associated line definitions
            var documentLineDefinitions = result.LineDefinitions
                .Select(e => lineDefsDic.GetValueOrDefault(e.LineDefinitionId))
                .Where(e => e != null && e.Columns != null);

            // Lines
            var participantDefIds = new HashSet<int>();
            var participantFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var centerFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var currencyFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var lineDef in documentLineDefinitions)
            {
                foreach (var colDef in lineDef.Columns.Where(c => c.InheritsFromHeader == InheritsFrom.DocumentHeader))
                {
                    // Memo
                    if (colDef.ColumnName == nameof(Line.Memo))
                    {
                        result.MemoIsCommonVisibility = true;
                        result.MemoVisibility ??= Visibility.Optional; // If a line inherits from header, override the header definition
                        if (string.IsNullOrWhiteSpace(result.MemoLabel))
                        {
                            result.MemoLabel = colDef.Label;
                            result.MemoLabel2 = colDef.Label2;
                            result.MemoLabel3 = colDef.Label3;
                        }
                        if (colDef.RequiredState > (result.MemoRequiredState ?? 0))
                        {
                            result.MemoRequiredState = colDef.RequiredState;
                        }

                        if (colDef.ReadOnlyState > (result.MemoReadOnlyState ?? 0))
                        {
                            result.MemoReadOnlyState = colDef.ReadOnlyState;
                        }
                    }

                    // Posting Date
                    else if (colDef.ColumnName == nameof(Line.PostingDate))
                    {
                        result.PostingDateVisibility = true;
                        if (string.IsNullOrWhiteSpace(result.PostingDateLabel))
                        {
                            result.PostingDateLabel = colDef.Label;
                            result.PostingDateLabel2 = colDef.Label2;
                            result.PostingDateLabel3 = colDef.Label3;
                        }
                        if (colDef.RequiredState > (result.PostingDateRequiredState ?? 0))
                        {
                            result.PostingDateRequiredState = colDef.RequiredState;
                        }

                        if (colDef.ReadOnlyState > (result.PostingDateReadOnlyState ?? 0))
                        {
                            result.PostingDateReadOnlyState = colDef.ReadOnlyState;
                        }
                    }

                    // Participant
                    else if (colDef.ColumnName == nameof(Entry.ParticipantId))
                    {
                        result.ParticipantVisibility = true;
                        if (string.IsNullOrWhiteSpace(result.ParticipantLabel))
                        {
                            result.ParticipantLabel = colDef.Label;
                            result.ParticipantLabel2 = colDef.Label2;
                            result.ParticipantLabel3 = colDef.Label3;
                        }

                        if (colDef.RequiredState > (result.ParticipantRequiredState ?? 0))
                        {
                            result.ParticipantRequiredState = colDef.RequiredState;
                        }

                        if (colDef.ReadOnlyState > (result.ParticipantReadOnlyState ?? 0))
                        {
                            result.ParticipantReadOnlyState = colDef.ReadOnlyState;
                        }

                        // Accumulate all the participant definition IDs in the hash set
                        if (colDef.EntryIndex < lineDef.Entries.Count)
                        {
                            var entryDef = lineDef.Entries[colDef.EntryIndex];
                            if (entryDef.ParticipantDefinitionIds == null || entryDef.ParticipantDefinitionIds.Count == 0)
                            {
                                participantDefIds = null; // Means no definitionIds will be added
                            }
                            else if (participantDefIds != null)
                            {
                                entryDef.ParticipantDefinitionIds.ForEach(defId => participantDefIds.Add(defId));
                            }
                        }

                        // Accumulate all the filter atoms in the hash set
                        if (string.IsNullOrWhiteSpace(colDef.Filter))
                        {
                            participantFilters = null; // It means no filters will be added
                        }
                        else if (participantFilters != null)
                        {
                            participantFilters.Add(colDef.Filter);
                        }
                    }

                    // Center
                    else if (colDef.ColumnName == nameof(Entry.CenterId))
                    {
                        result.CenterVisibility = true;
                        if (string.IsNullOrWhiteSpace(result.CenterLabel))
                        {
                            result.CenterLabel = colDef.Label;
                            result.CenterLabel2 = colDef.Label2;
                            result.CenterLabel3 = colDef.Label3;
                        }
                        if (colDef.RequiredState > (result.CenterRequiredState ?? 0))
                        {
                            result.CenterRequiredState = colDef.RequiredState;
                        }

                        if (colDef.ReadOnlyState > (result.CenterReadOnlyState ?? 0))
                        {
                            result.CenterReadOnlyState = colDef.ReadOnlyState;
                        }

                        // Accumulate all the filter atoms in the hash set
                        if (string.IsNullOrWhiteSpace(colDef.Filter))
                        {
                            centerFilters = null; // It means no filters will be added
                        }
                        else if (centerFilters != null)
                        {
                            centerFilters.Add(colDef.Filter);
                        }
                    }

                    // Currency
                    else if (colDef.ColumnName == nameof(Entry.CurrencyId))
                    {
                        result.CurrencyVisibility = true;
                        if (string.IsNullOrWhiteSpace(result.CurrencyLabel))
                        {
                            result.CurrencyLabel = colDef.Label;
                            result.CurrencyLabel2 = colDef.Label2;
                            result.CurrencyLabel3 = colDef.Label3;
                        }
                        if (colDef.RequiredState > (result.CurrencyRequiredState ?? 0))
                        {
                            result.CurrencyRequiredState = colDef.RequiredState;
                        }

                        if (colDef.ReadOnlyState > (result.CurrencyReadOnlyState ?? 0))
                        {
                            result.CurrencyReadOnlyState = colDef.ReadOnlyState;
                        }

                        // Accumulate all the filter atoms in the hash set
                        if (string.IsNullOrWhiteSpace(colDef.Filter))
                        {
                            currencyFilters = null; // It means no filters will be added
                        }
                        else if (currencyFilters != null)
                        {
                            currencyFilters.Add(colDef.Filter);
                        }
                    }

                    // External Reference
                    else if (colDef.ColumnName == nameof(Entry.ExternalReference))
                    {
                        result.ExternalReferenceVisibility = true;
                        if (string.IsNullOrWhiteSpace(result.ExternalReferenceLabel))
                        {
                            result.ExternalReferenceLabel = colDef.Label;
                            result.ExternalReferenceLabel2 = colDef.Label2;
                            result.ExternalReferenceLabel3 = colDef.Label3;
                        }
                        if (colDef.RequiredState > (result.ExternalReferenceRequiredState ?? 0))
                        {
                            result.ExternalReferenceRequiredState = colDef.RequiredState;
                        }

                        if (colDef.ReadOnlyState > (result.ExternalReferenceReadOnlyState ?? 0))
                        {
                            result.ExternalReferenceReadOnlyState = colDef.ReadOnlyState;
                        }
                    }

                    // Additional Reference
                    else if (colDef.ColumnName == nameof(Entry.AdditionalReference))
                    {
                        result.AdditionalReferenceVisibility = true;
                        if (string.IsNullOrWhiteSpace(result.AdditionalReferenceLabel))
                        {
                            result.AdditionalReferenceLabel = colDef.Label;
                            result.AdditionalReferenceLabel2 = colDef.Label2;
                            result.AdditionalReferenceLabel3 = colDef.Label3;
                        }
                        if (colDef.RequiredState > (result.AdditionalReferenceRequiredState ?? 0))
                        {
                            result.AdditionalReferenceRequiredState = colDef.RequiredState;
                        }

                        if (colDef.ReadOnlyState > (result.AdditionalReferenceReadOnlyState ?? 0))
                        {
                            result.AdditionalReferenceReadOnlyState = colDef.ReadOnlyState;
                        }
                    }
                }
            }

            // Calculate the definitionIds and filters
            result.ParticipantDefinitionIds = participantDefIds?.ToList() ?? new List<int>();
            result.ParticipantFilter = Disjunction(participantFilters);
            result.CenterFilter = Disjunction(centerFilters);
            result.CurrencyFilter = Disjunction(currencyFilters);

            #region Manual JV

            // JV has some hard coded values:
            if (def.Code == ManualJournalVoucher)
            {
                // PostingDate
                result.PostingDateVisibility = true;
                result.PostingDateLabel = null;
                result.PostingDateLabel2 = null;
                result.PostingDateLabel3 = null;

                // Memo
                result.MemoVisibility = Visibility.Optional;
                result.MemoIsCommonVisibility = false;
                result.MemoLabel = null;
                result.MemoLabel2 = null;
                result.MemoLabel3 = null;

                result.ParticipantVisibility = false;
                result.AdditionalReferenceVisibility = false;
                result.ExternalReferenceVisibility = false;
                result.CurrencyVisibility = false;
                result.CenterVisibility = false;
            }

            #endregion

            // Return result
            return result;
        }

        private static MarkupTemplateForClient MapMarkupTemplate(MarkupTemplate d)
        {
            return new MarkupTemplateForClient
            {
                MarkupTemplateId = d.Id,
                Name = d.Name,
                Name2 = d.Name2,
                Name3 = d.Name3,
                SupportsPrimaryLanguage = d.SupportsPrimaryLanguage.Value,
                SupportsSecondaryLanguage = d.SupportsSecondaryLanguage.Value,
                SupportsTernaryLanguage = d.SupportsTernaryLanguage.Value,
                Usage = d.Usage,
                Collection = d.Collection,
                DefinitionId = d.DefinitionId
            };
        }

        public static async Task<Versioned<DefinitionsForClient>> LoadDefinitionsForClient(ApplicationRepository repo, CancellationToken cancellation)
        {
            // Load definitions
            var (version,
                lookupDefs,
                relationDefs,
                custodyDefs,
                resourceDefs,
                reportDefs,
                docDefs,
                lineDefs,
                markupTemplates,
                entryCustodianDefs,
                entryCustodyDefs,
                entryParticipantDefs,
                entryResourceDefs) = await repo.Definitions__Load(cancellation);

            // Map Lookups, Relations, Resources, Reports (Straight forward)
            var result = new DefinitionsForClient
            {
                Lookups = lookupDefs.ToDictionary(def => def.Id, def => MapLookupDefinition(def)),
                Relations = relationDefs.ToDictionary(def => def.Id, def => MapRelationDefinition(def)),
                Custodies = custodyDefs.ToDictionary(def => def.Id, def => MapCustodyDefinition(def)),
                Resources = resourceDefs.ToDictionary(def => def.Id, def => MapResourceDefinition(def)),
                Reports = reportDefs.ToDictionary(def => def.Id, def => MapReportDefinition(def)),
                Lines = lineDefs.ToDictionary(def => def.Id, def => MapLineDefinition(def, entryCustodianDefs, entryCustodyDefs, entryParticipantDefs, entryResourceDefs)),
                MarkupTemplates = markupTemplates.Select(MapMarkupTemplate),
            };

            // Map Lines and Documents (Special handling)
            result.Documents = docDefs.ToDictionary(def => def.Id, def => MapDocumentDefinition(def, result.Lines));

            // Set built in Ids for ease of access
            result.ManualJournalVouchersDefinitionId = result.Documents.FirstOrDefault(e => e.Value.Code == ManualJournalVoucher).Key;
            if (result.ManualJournalVouchersDefinitionId == default)
            {
                throw new BadRequestException($"The database is in an inconsistent state, the built-in document definition: '{ManualJournalVoucher}' could not be found");
            }

            result.ManualLinesDefinitionId = result.Lines.FirstOrDefault(e => e.Value.Code == ManualLine).Key;
            if (result.ManualJournalVouchersDefinitionId == default)
            {
                throw new BadRequestException($"The database is in an inconsistent state, the built-in line definition: 'ManualLine' could not be found");
            }

            // Return result
            return new Versioned<DefinitionsForClient>
            (
                data: result,
                version: version.ToString()
            );
        }

        /// <summary>
        /// Helper method that ORs together a bunch of filter strings
        /// </summary>
        private static string Disjunction(HashSet<string> filters)
        {
            if (filters != null)
            {
                if (filters.Count == 1)
                {
                    return filters.Single();
                }
                else if (filters.Count > 1)
                {
                    return filters.Select(e => $"({e})")?.Aggregate((e1, e2) => $"{e1} or {e2}");
                }
            }

            return null;
        }
    }
}

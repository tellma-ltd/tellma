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
                _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }
    }

    public class DefinitionsService : ServiceBase
    {
        private static string ManualLine => nameof(ManualLine);
        private static string ManualJournalVoucher => nameof(ManualJournalVoucher);

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

                // Relation Only

                AgentVisibility = MapVisibility(def.AgentVisibility),
                TaxIdentificationNumberVisibility = MapVisibility(def.TaxIdentificationNumberVisibility),
                JobVisibility = MapVisibility(def.JobVisibility),
                BankAccountNumberVisibility = MapVisibility(def.BankAccountNumberVisibility),
                UserCardinality = MapCardinality(def.UserCardinality),
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

                ReorderLevelVisibility = MapVisibility(def.ReorderLevelVisibility),
                EconomicOrderQuantityVisibility = MapVisibility(def.EconomicOrderQuantityVisibility),
                UnitCardinality = MapCardinality(def.UnitCardinality),
                DefaultUnitId = def.DefaultUnitId,
                UnitMassVisibility = MapVisibility(def.UnitMassVisibility),
                DefaultUnitMassUnitId = def.DefaultUnitMassUnitId,

                MonetaryValueVisibility = MapVisibility(def.MonetaryValueVisibility),
                ParticipantVisibility = MapVisibility(def.ParticipantVisibility),
                ParticipantDefinitionId = def.ParticipantDefinitionId
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

        private static LineDefinitionForClient MapLineDefinition(LineDefinition def)
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
                    AccountTypeId = e.AccountTypeId,
                    EntryTypeId = e.EntryTypeId,
                    EntryTypeParentId = e.AccountType?.EntryTypeParentId, // There is supposed to validation to make sure all selected account types have the same entry type parent Id
                    CustodyDefinitionIds = e.CustodyDefinitions.Select(e => e.CustodyDefinitionId.Value).ToList(),
                    NotedRelationDefinitionIds = e.NotedRelationDefinitions.Select(e => e.NotedRelationDefinitionId.Value).ToList(),
                    ResourceDefinitionIds = e.ResourceDefinitions.Select(e => e.ResourceDefinitionId.Value).ToList(),
                })?.ToList() ?? new List<LineDefinitionEntryForClient>(),

                Columns = def.Columns?.Select(c => new LineDefinitionColumnForClient
                {
                    ColumnName = c.ColumnName,
                    EntryIndex = c.EntryIndex.Value,
                    Label = c.Label,
                    Label2 = c.Label2,
                    Label3 = c.Label3,
                    ReadOnlyState = c.ReadOnlyState,
                    RequiredState = c.RequiredState,
                    InheritsFromHeader = c.InheritsFromHeader == false ? null : c.InheritsFromHeader,
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

                MarkupTemplates = def.MarkupTemplates?.Select(d => new DocumentDefinitionMarkupTemplateForClient
                {
                    MarkupTemplateId = d.MarkupTemplateId.Value,
                    Name = d.MarkupTemplate?.Name,
                    Name2 = d.MarkupTemplate?.Name2,
                    Name3 = d.MarkupTemplate?.Name3,
                    SupportsPrimaryLanguage = d.MarkupTemplate?.SupportsPrimaryLanguage ?? false,
                    SupportsSecondaryLanguage = d.MarkupTemplate?.SupportsSecondaryLanguage ?? false,
                    SupportsTernaryLanguage = d.MarkupTemplate?.SupportsTernaryLanguage ?? false,
                    Usage = d.MarkupTemplate?.Usage
                })?.ToList() ?? new List<DocumentDefinitionMarkupTemplateForClient>(),

                MainMenuIcon = def.MainMenuIcon,
                MainMenuSortKey = def.MainMenuSortKey ?? 0m,
                MainMenuSection = def.MainMenuSection,

                // These should not be null
                CreditResourceDefinitionIds = new List<int>(),
                DebitResourceDefinitionIds = new List<int>(),
                CreditCustodyDefinitionIds = new List<int>(),
                DebitCustodyDefinitionIds = new List<int>(),
                NotedRelationDefinitionIds = new List<int>(),
            };

            // Here we compute some values based on the associated line definitions
            var documentLineDefinitions = result.LineDefinitions
                .Select(e => lineDefsDic.GetValueOrDefault(e.LineDefinitionId))
                .Where(e => e != null && e.Columns != null);

            // Lines
            foreach (var lineDef in documentLineDefinitions)
            {
                foreach (var colDef in lineDef.Columns.Where(c => c.InheritsFromHeader ?? false))
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
                        if (colDef.RequiredState < (result.MemoRequiredState ?? 5))
                        {
                            result.MemoRequiredState = colDef.RequiredState;
                        }

                        if (colDef.ReadOnlyState < (result.MemoReadOnlyState ?? 5))
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
                        if (colDef.RequiredState < (result.PostingDateRequiredState ?? 5))
                        {
                            result.PostingDateRequiredState = colDef.RequiredState;
                        }

                        if (colDef.ReadOnlyState < (result.PostingDateReadOnlyState ?? 5))
                        {
                            result.PostingDateReadOnlyState = colDef.ReadOnlyState;
                        }
                    }

                    // Relations
                    else if (colDef.EntryIndex < lineDef.Entries.Count)
                    {
                        var entryDef = lineDef.Entries[colDef.EntryIndex];

                        // DebitResource
                        if (colDef.ColumnName == nameof(Entry.ResourceId) && entryDef.Direction == 1)
                        {
                            result.DebitResourceVisibility = true;
                            if (string.IsNullOrWhiteSpace(result.DebitResourceLabel))
                            {
                                result.DebitResourceLabel ??= colDef.Label;
                                result.DebitResourceLabel2 ??= colDef.Label2;
                                result.DebitResourceLabel3 ??= colDef.Label3;

                                result.DebitResourceDefinitionIds = entryDef.ResourceDefinitionIds;
                            }

                            if (colDef.RequiredState < (result.DebitResourceRequiredState ?? 5))
                            {
                                result.DebitResourceRequiredState = colDef.RequiredState;
                            }

                            if (colDef.ReadOnlyState < (result.DebitResourceReadOnlyState ?? 5))
                            {
                                result.DebitResourceReadOnlyState = colDef.ReadOnlyState;
                            }
                        }

                        // CreditResource
                        if (colDef.ColumnName == nameof(Entry.ResourceId) && entryDef.Direction == -1)
                        {
                            result.CreditResourceVisibility = true;
                            if (string.IsNullOrWhiteSpace(result.CreditResourceLabel))
                            {
                                result.CreditResourceLabel = colDef.Label;
                                result.CreditResourceLabel2 = colDef.Label2;
                                result.CreditResourceLabel3 = colDef.Label3;

                                result.CreditResourceDefinitionIds = entryDef.ResourceDefinitionIds;
                            }

                            if (colDef.RequiredState < (result.CreditResourceRequiredState ?? 5))
                            {
                                result.CreditResourceRequiredState = colDef.RequiredState;
                            }

                            if (colDef.ReadOnlyState < (result.CreditResourceReadOnlyState ?? 5))
                            {
                                result.CreditResourceReadOnlyState = colDef.ReadOnlyState;
                            }
                        }

                        // DebitCustody
                        if (colDef.ColumnName == nameof(Entry.CustodyId) && entryDef.Direction == 1)
                        {
                            result.DebitCustodyVisibility = true;
                            if (string.IsNullOrWhiteSpace(result.DebitCustodyLabel))
                            {
                                result.DebitCustodyLabel ??= colDef.Label;
                                result.DebitCustodyLabel2 ??= colDef.Label2;
                                result.DebitCustodyLabel3 ??= colDef.Label3;

                                result.DebitCustodyDefinitionIds = entryDef.CustodyDefinitionIds;
                            }

                            if (colDef.RequiredState < (result.DebitCustodyRequiredState ?? 5))
                            {
                                result.DebitCustodyRequiredState = colDef.RequiredState;
                            }

                            if (colDef.ReadOnlyState < (result.DebitCustodyReadOnlyState ?? 5))
                            {
                                result.DebitCustodyReadOnlyState = colDef.ReadOnlyState;
                            }
                        }

                        // CreditCustody
                        if (colDef.ColumnName == nameof(Entry.CustodyId) && entryDef.Direction == -1)
                        {
                            result.CreditCustodyVisibility = true;
                            if (string.IsNullOrWhiteSpace(result.CreditCustodyLabel))
                            {
                                result.CreditCustodyLabel = colDef.Label;
                                result.CreditCustodyLabel2 = colDef.Label2;
                                result.CreditCustodyLabel3 = colDef.Label3;

                                result.CreditCustodyDefinitionIds = entryDef.CustodyDefinitionIds;
                            }

                            if (colDef.RequiredState < (result.CreditCustodyRequiredState ?? 5))
                            {
                                result.CreditCustodyRequiredState = colDef.RequiredState;
                            }

                            if (colDef.ReadOnlyState < (result.CreditCustodyReadOnlyState ?? 5))
                            {
                                result.CreditCustodyReadOnlyState = colDef.ReadOnlyState;
                            }
                        }

                        // NotedRelation
                        if (colDef.ColumnName == nameof(Entry.NotedRelationId))
                        {
                            result.NotedRelationVisibility = true;
                            if (string.IsNullOrWhiteSpace(result.NotedRelationLabel))
                            {
                                result.NotedRelationLabel = colDef.Label;
                                result.NotedRelationLabel2 = colDef.Label2;
                                result.NotedRelationLabel3 = colDef.Label3;

                                result.NotedRelationDefinitionIds = entryDef.NotedRelationDefinitionIds;
                            }

                            if (colDef.RequiredState < (result.NotedRelationRequiredState ?? 5))
                            {
                                result.NotedRelationRequiredState = colDef.RequiredState;
                            }

                            if (colDef.ReadOnlyState < (result.NotedRelationReadOnlyState ?? 5))
                            {
                                result.NotedRelationReadOnlyState = colDef.ReadOnlyState;
                            }
                        }
                    }

                    // Center
                    if (colDef.ColumnName == nameof(Entry.CenterId))
                    {
                        result.CenterVisibility = true;
                        if (string.IsNullOrWhiteSpace(result.CenterLabel))
                        {
                            result.CenterLabel = colDef.Label;
                            result.CenterLabel2 = colDef.Label2;
                            result.CenterLabel3 = colDef.Label3;
                        }
                        if (colDef.RequiredState < (result.CenterRequiredState ?? 5))
                        {
                            result.CenterRequiredState = colDef.RequiredState;
                        }

                        if (colDef.ReadOnlyState < (result.CenterReadOnlyState ?? 5))
                        {
                            result.CenterReadOnlyState = colDef.ReadOnlyState;
                        }
                    }

                    // Time1
                    if (colDef.ColumnName == nameof(Entry.Time1))
                    {
                        result.Time1Visibility = true;
                        if (string.IsNullOrWhiteSpace(result.Time1Label))
                        {
                            result.Time1Label = colDef.Label;
                            result.Time1Label2 = colDef.Label2;
                            result.Time1Label3 = colDef.Label3;
                        }
                        if (colDef.RequiredState < (result.Time1RequiredState ?? 5))
                        {
                            result.Time1RequiredState = colDef.RequiredState;
                        }

                        if (colDef.ReadOnlyState < (result.Time1ReadOnlyState ?? 5))
                        {
                            result.Time1ReadOnlyState = colDef.ReadOnlyState;
                        }
                    }

                    // Time2
                    if (colDef.ColumnName == nameof(Entry.Time2))
                    {
                        result.Time2Visibility = true;
                        if (string.IsNullOrWhiteSpace(result.Time2Label))
                        {
                            result.Time2Label = colDef.Label;
                            result.Time2Label2 = colDef.Label2;
                            result.Time2Label3 = colDef.Label3;
                        }
                        if (colDef.RequiredState < (result.Time2RequiredState ?? 5))
                        {
                            result.Time2RequiredState = colDef.RequiredState;
                        }

                        if (colDef.ReadOnlyState < (result.Time2ReadOnlyState ?? 5))
                        {
                            result.Time2ReadOnlyState = colDef.ReadOnlyState;
                        }
                    }

                    // Quantity
                    if (colDef.ColumnName == nameof(Entry.Quantity))
                    {
                        result.QuantityVisibility = true;
                        if (string.IsNullOrWhiteSpace(result.QuantityLabel))
                        {
                            result.QuantityLabel = colDef.Label;
                            result.QuantityLabel2 = colDef.Label2;
                            result.QuantityLabel3 = colDef.Label3;
                        }
                        if (colDef.RequiredState < (result.QuantityRequiredState ?? 5))
                        {
                            result.QuantityRequiredState = colDef.RequiredState;
                        }

                        if (colDef.ReadOnlyState < (result.QuantityReadOnlyState ?? 5))
                        {
                            result.QuantityReadOnlyState = colDef.ReadOnlyState;
                        }
                    }

                    // Unit
                    if (colDef.ColumnName == nameof(Entry.UnitId))
                    {
                        result.UnitVisibility = true;
                        if (string.IsNullOrWhiteSpace(result.UnitLabel))
                        {
                            result.UnitLabel = colDef.Label;
                            result.UnitLabel2 = colDef.Label2;
                            result.UnitLabel3 = colDef.Label3;
                        }
                        if (colDef.RequiredState < (result.UnitRequiredState ?? 5))
                        {
                            result.UnitRequiredState = colDef.RequiredState;
                        }

                        if (colDef.ReadOnlyState < (result.UnitReadOnlyState ?? 5))
                        {
                            result.UnitReadOnlyState = colDef.ReadOnlyState;
                        }
                    }

                    // Currency
                    if (colDef.ColumnName == nameof(Entry.CurrencyId))
                    {
                        result.CurrencyVisibility = true;
                        if (string.IsNullOrWhiteSpace(result.CurrencyLabel))
                        {
                            result.CurrencyLabel = colDef.Label;
                            result.CurrencyLabel2 = colDef.Label2;
                            result.CurrencyLabel3 = colDef.Label3;
                        }
                        if (colDef.RequiredState < (result.CurrencyRequiredState ?? 5))
                        {
                            result.CurrencyRequiredState = colDef.RequiredState;
                        }

                        if (colDef.ReadOnlyState < (result.CurrencyReadOnlyState ?? 5))
                        {
                            result.CurrencyReadOnlyState = colDef.ReadOnlyState;
                        }
                    }
                }
            }

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
            }

            return result;
        }

        public static async Task<Versioned<DefinitionsForClient>> LoadDefinitionsForClient(ApplicationRepository repo, CancellationToken cancellation)
        {
            // Load definitions
            var (version, lookupDefs, relationDefs, custodyDefs, resourceDefs, reportDefs, docDefs, lineDefs) = await repo.Definitions__Load(cancellation);

            // Map Lookups, Relations, Resources, Reports (Straight forward)
            var result = new DefinitionsForClient
            {
                Lookups = lookupDefs.ToDictionary(def => def.Id, def => MapLookupDefinition(def)),
                Relations = relationDefs.ToDictionary(def => def.Id, def => MapRelationDefinition(def)),
                Custodies = custodyDefs.ToDictionary(def => def.Id, def => MapCustodyDefinition(def)),
                Resources = resourceDefs.ToDictionary(def => def.Id, def => MapResourceDefinition(def)),
                Reports = reportDefs.ToDictionary(def => def.Id, def => MapReportDefinition(def)),
                Lines = lineDefs.ToDictionary(def => def.Id, def => MapLineDefinition(def))
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
    }
}

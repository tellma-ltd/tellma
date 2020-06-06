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
    [AuthorizeAccess]
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
    }

    public class DefinitionsService : ServiceBase
    {
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

        private static ContractDefinitionForClient MapContractDefinition(ContractDefinition def)
        {
            return new ContractDefinitionForClient
            {
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

                AgentVisibility = MapVisibility(def.AgentVisibility),
                CurrencyVisibility = MapVisibility(def.CurrencyVisibility),
                TaxIdentificationNumberVisibility = MapVisibility(def.TaxIdentificationNumberVisibility),
                ImageVisibility = MapVisibility(def.ImageVisibility),
                StartDateVisibility = MapVisibility(def.StartDateVisibility),
                StartDateLabel = def.StartDateLabel,
                StartDateLabel2 = def.StartDateLabel2,
                StartDateLabel3 = def.StartDateLabel3,
                JobVisibility = MapVisibility(def.JobVisibility),
                BankAccountNumberVisibility = MapVisibility(def.BankAccountNumberVisibility),

            };
        }

        private static ResourceDefinitionForClient MapResourceDefinition(ResourceDefinition def)
        {
            return new ResourceDefinitionForClient
            {
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

                IdentifierLabel = def.IdentifierLabel,
                IdentifierLabel2 = def.IdentifierLabel2,
                IdentifierLabel3 = def.IdentifierLabel3,
                IdentifierVisibility = MapVisibility(def.IdentifierVisibility),
                CurrencyVisibility = MapVisibility(def.CurrencyVisibility),
                DescriptionVisibility = MapVisibility(def.DescriptionVisibility),
                ExpenseEntryTypeVisibility = MapVisibility(def.ExpenseEntryTypeVisibility),
                CenterVisibility = MapVisibility(def.CenterVisibility),
                ResidualMonetaryValueVisibility = MapVisibility(def.ResidualMonetaryValueVisibility),
                ResidualValueVisibility = MapVisibility(def.ResidualValueVisibility),
                ReorderLevelVisibility = MapVisibility(def.ReorderLevelVisibility),
                EconomicOrderQuantityVisibility = MapVisibility(def.EconomicOrderQuantityVisibility),
                AvailableSinceLabel = def.AvailableSinceLabel,
                AvailableSinceLabel2 = def.AvailableSinceLabel2,
                AvailableSinceLabel3 = def.AvailableSinceLabel3,
                AvailableSinceVisibility = MapVisibility(def.AvailableSinceVisibility),
                AvailableTillLabel = def.AvailableTillLabel,
                AvailableTillLabel2 = def.AvailableTillLabel2,
                AvailableTillLabel3 = def.AvailableTillLabel3,
                AvailableTillVisibility = MapVisibility(def.AvailableTillVisibility),

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

        private static List<int> ToList(int? defId)
        {
            List<int> result = null;
            if (defId != null)
            {
                result = new List<int>(defId.Value);
            }

            return result;
        }

        private static LineDefinitionForClient MapLineDefinition(LineDefinition def, Dictionary<int, AccountType> accountTypesDic)
        {
            var result = new LineDefinitionForClient
            {
                Code = def.Code,
                TitlePlural = def.TitlePlural,
                TitlePlural2 = def.TitlePlural2,
                TitlePlural3 = def.TitlePlural3,
                TitleSingular = def.TitleSingular,
                TitleSingular2 = def.TitleSingular2,
                TitleSingular3 = def.TitleSingular3,

                AllowSelectiveSigning = def.AllowSelectiveSigning ?? false,
                ViewDefaultsToForm = def.ViewDefaultsToForm ?? false,
                Entries = def.Entries?.Select(e => new LineDefinitionEntryForClient
                {
                    Direction = e.Direction.Value,
                    AccountTypeId = e.AccountTypeId,
                    EntryTypeId = e.EntryTypeId,
                    ContractDefinitionIds = e.ContractDefinitionId == null ? null : new List<int> { e.ContractDefinitionId.Value },
                    NotedContractDefinitionIds = e.NotedContractDefinitionId == null ? null : new List<int> { e.NotedContractDefinitionId.Value },
                    ResourceDefinitionIds = e.ResourceDefinitionId == null ? null : new List<int> { e.ResourceDefinitionId.Value },
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
                    InheritsFromHeader = c.InheritsFromHeader == false ? null : c.InheritsFromHeader
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
            };

            // Copy across some values from Account Type
            foreach (var entry in result.Entries)
            {
                var accountType = accountTypesDic.GetValueOrDefault(entry.AccountTypeId ?? 0);
                if (accountType == null)
                {
                    throw new BadRequestException($"Account type with Id {entry.AccountTypeId} was not loaded"); // Just in case
                }

                entry.EntryTypeParentId = accountType.EntryTypeParentId;

                // If the LineDefinitionEntry itself does not specify a single definitionId
                // get the list of permitted definitions from the account type
                if (entry.ContractDefinitionIds == null)
                {
                    entry.ContractDefinitionIds = accountType.ContractDefinitions.Select(e => e.ContractDefinitionId.Value).ToList();
                }

                if (entry.NotedContractDefinitionIds == null)
                {
                    entry.NotedContractDefinitionIds = accountType.NotedContractDefinitions.Select(e => e.NotedContractDefinitionId.Value).ToList();
                }

                if (entry.ResourceDefinitionIds == null)
                {
                    entry.ResourceDefinitionIds = accountType.ResourceDefinitions.Select(e => e.ResourceDefinitionId.Value).ToList();
                }
            }

            foreach (var col in result.Columns)
            {
                if (col.EntryIndex < result.Entries.Count)
                {
                    var entry = result.Entries[col.EntryIndex];
                    var accountType = accountTypesDic.GetValueOrDefault(entry.AccountTypeId.Value);
                    switch (col.ColumnName)
                    {
                        //case nameof(Entry.ContractId):
                        //    if (accountType.ContractDefinitionId != null && contractDefs.TryGetValue(accountType.ContractDefinitionId, out var contractDef))
                        //    {
                        //        col.Label ??= contractDef.TitleSingular;
                        //        col.Label2 ??= contractDef.TitleSingular2;
                        //        col.Label3 ??= contractDef.TitleSingular3;
                        //    }
                        //    break;
                        //case nameof(Entry.NotedContractId):
                        //    if (accountType.NotedContractDefinitionId != null && contractDefs.TryGetValue(accountType.NotedContractDefinitionId, out var notedContractDef))
                        //    {
                        //        col.Label ??= notedContractDef.TitleSingular;
                        //        col.Label2 ??= notedContractDef.TitleSingular2;
                        //        col.Label3 ??= notedContractDef.TitleSingular3;
                        //    }
                        //    break;
                        //case nameof(Entry.ResourceId):
                        //    if (accountType.ResourceDefinitionId != null && resourceDefs.TryGetValue(accountType.ResourceDefinitionId, out var resourceDef))
                        //    {
                        //        col.Label ??= resourceDef.TitleSingular;
                        //        col.Label2 ??= resourceDef.TitleSingular2;
                        //        col.Label3 ??= resourceDef.TitleSingular3;
                        //    }
                        //    break;
                        //case nameof(Entry.AccountIdentifier):
                        //    col.Label ??= accountType?.IdentifierLabel;
                        //    col.Label2 ??= accountType?.IdentifierLabel2;
                        //    col.Label3 ??= accountType?.IdentifierLabel3;
                        //    break;
                        case nameof(Entry.DueDate):
                            col.Label ??= accountType?.DueDateLabel;
                            col.Label2 ??= accountType?.DueDateLabel2;
                            col.Label3 ??= accountType?.DueDateLabel3;
                            break;
                        case nameof(Entry.Time1):
                            col.Label ??= accountType?.Time1Label;
                            col.Label2 ??= accountType?.Time1Label2;
                            col.Label3 ??= accountType?.Time1Label3;
                            break;
                        case nameof(Entry.Time2):
                            col.Label ??= accountType?.Time2Label;
                            col.Label2 ??= accountType?.Time2Label2;
                            col.Label3 ??= accountType?.Time2Label3;
                            break;
                        case nameof(Entry.ExternalReference):
                            col.Label ??= accountType?.ExternalReferenceLabel;
                            col.Label2 ??= accountType?.ExternalReferenceLabel2;
                            col.Label3 ??= accountType?.ExternalReferenceLabel3;
                            break;
                        case nameof(Entry.AdditionalReference):
                            col.Label ??= accountType?.AdditionalReferenceLabel;
                            col.Label2 ??= accountType?.AdditionalReferenceLabel2;
                            col.Label3 ??= accountType?.AdditionalReferenceLabel3;
                            break;
                        case nameof(Entry.NotedAgentName):
                            col.Label ??= accountType?.NotedAgentNameLabel;
                            col.Label2 ??= accountType?.NotedAgentNameLabel2;
                            col.Label3 ??= accountType?.NotedAgentNameLabel3;
                            break;
                        case nameof(Entry.NotedAmount):
                            col.Label ??= accountType?.NotedAmountLabel;
                            col.Label2 ??= accountType?.NotedAmountLabel2;
                            col.Label3 ??= accountType?.NotedAmountLabel3;
                            break;
                        case nameof(Entry.NotedDate):
                            col.Label ??= accountType?.NotedDateLabel;
                            col.Label2 ??= accountType?.NotedDateLabel2;
                            col.Label3 ??= accountType?.NotedDateLabel3;
                            break;
                    }
                }
            }

            return result;
        }

        private static DocumentDefinitionForClient MapDocumentDefinition(DocumentDefinition def, Dictionary<int, LineDefinitionForClient> lineDefsDic)
        {
            var result = new DocumentDefinitionForClient
            {
                Code = def.Code,
                IsOriginalDocument = def.IsOriginalDocument ?? false,
                TitlePlural = def.TitlePlural,
                TitlePlural2 = def.TitlePlural2,
                TitlePlural3 = def.TitlePlural3,
                TitleSingular = def.TitleSingular,
                TitleSingular2 = def.TitleSingular2,
                TitleSingular3 = def.TitleSingular3,
                Prefix = def.Prefix,
                CodeWidth = def.CodeWidth ?? 4,

                MemoVisibility = def.MemoVisibility,
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
                CreditContractDefinitionIds = new List<int>(),
                DebitContractDefinitionIds = new List<int>(),
                NotedContractDefinitionIds = new List<int>(),
            };

            // Here we compute some values based on the associated line definitions
            var documentLineDefinitions = result.LineDefinitions
                .Select(e => lineDefsDic.GetValueOrDefault(e.LineDefinitionId))
                .Where(e => e != null && e.Columns != null);

            // ContractId
            foreach (var lineDef in documentLineDefinitions)
            {
                foreach (var colDef in lineDef.Columns.Where(c => c.InheritsFromHeader ?? false))
                {
                    // Memo
                    if (colDef.ColumnName == nameof(Line.Memo))
                    {
                        result.MemoIsCommonVisibility = true;
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

                    // Contracts
                    else if (colDef.EntryIndex < lineDef.Entries.Count)
                    {
                        var entryDef = lineDef.Entries[colDef.EntryIndex];

                        // DebitContract
                        if (colDef.ColumnName == nameof(Entry.ContractId) && entryDef.Direction == 1)
                        {
                            result.DebitContractVisibility = true;
                            if (string.IsNullOrWhiteSpace(result.DebitContractLabel))
                            {
                                result.DebitContractLabel ??= colDef.Label;
                                result.DebitContractLabel2 ??= colDef.Label2;
                                result.DebitContractLabel3 ??= colDef.Label3;
                            }

                            result.DebitContractDefinitionIds ??= entryDef.ContractDefinitionIds;

                            if (colDef.RequiredState < (result.DebitContractRequiredState ?? 5))
                            {
                                result.DebitContractRequiredState = colDef.RequiredState;
                            }

                            if (colDef.ReadOnlyState < (result.DebitContractReadOnlyState ?? 5))
                            {
                                result.DebitContractReadOnlyState = colDef.ReadOnlyState;
                            }
                        }

                        // CreditContract
                        if (colDef.ColumnName == nameof(Entry.ContractId) && entryDef.Direction == -1)
                        {
                            result.CreditContractVisibility = true;
                            if (string.IsNullOrWhiteSpace(result.CreditContractLabel))
                            {
                                result.CreditContractLabel = colDef.Label;
                                result.CreditContractLabel2 = colDef.Label2;
                                result.CreditContractLabel3 = colDef.Label3;
                            }

                            result.CreditContractDefinitionIds ??= entryDef.ContractDefinitionIds;

                            if (colDef.RequiredState < (result.CreditContractRequiredState ?? 5))
                            {
                                result.CreditContractRequiredState = colDef.RequiredState;
                            }

                            if (colDef.ReadOnlyState < (result.CreditContractReadOnlyState ?? 5))
                            {
                                result.CreditContractReadOnlyState = colDef.ReadOnlyState;
                            }
                        }

                        // NotedContract
                        if (colDef.ColumnName == nameof(Entry.NotedContractId))
                        {
                            result.NotedContractVisibility = true;
                            if (string.IsNullOrWhiteSpace(result.NotedContractLabel))
                            {
                                result.NotedContractLabel = colDef.Label;
                                result.NotedContractLabel2 = colDef.Label2;
                                result.NotedContractLabel3 = colDef.Label3;
                            }

                            result.NotedContractDefinitionIds ??= entryDef.NotedContractDefinitionIds;

                            if (colDef.RequiredState < (result.NotedContractRequiredState ?? 5))
                            {
                                result.NotedContractRequiredState = colDef.RequiredState;
                            }

                            if (colDef.ReadOnlyState < (result.NotedContractReadOnlyState ?? 5))
                            {
                                result.NotedContractReadOnlyState = colDef.ReadOnlyState;
                            }
                        }
                    }

                    // InvestmentCenter
                    if (colDef.ColumnName == nameof(Entry.CenterId))
                    {
                        result.InvestmentCenterVisibility = true;
                        if (string.IsNullOrWhiteSpace(result.InvestmentCenterLabel))
                        {
                            result.InvestmentCenterLabel = colDef.Label;
                            result.InvestmentCenterLabel2 = colDef.Label2;
                            result.InvestmentCenterLabel3 = colDef.Label3;
                        }
                        if (colDef.RequiredState < (result.InvestmentCenterRequiredState ?? 5))
                        {
                            result.InvestmentCenterRequiredState = colDef.RequiredState;
                        }

                        if (colDef.ReadOnlyState < (result.InvestmentCenterReadOnlyState ?? 5))
                        {
                            result.InvestmentCenterReadOnlyState = colDef.ReadOnlyState;
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

            return result;
        }

        public static async Task<Versioned<DefinitionsForClient>> LoadDefinitionsForClient(ApplicationRepository repo, CancellationToken cancellation)
        {
            // Load definitions
            var (version, lookupDefs, contractDefs, resourceDefs, reportDefs, docDefs, lineDefs, accountTypes) = await repo.Definitions__Load(cancellation);

            // Map Lookups, Contracts, Resources, Reports (Straight forward)
            var result = new DefinitionsForClient
            {
                Lookups = lookupDefs.ToDictionary(def => def.Id, def => MapLookupDefinition(def)),
                Contracts = contractDefs.ToDictionary(def => def.Id, def => MapContractDefinition(def)),
                Resources = resourceDefs.ToDictionary(def => def.Id, def => MapResourceDefinition(def)),
                Reports = reportDefs.ToDictionary(def => def.Id, def => MapReportDefinition(def)),
            };

            // Map Lines and Documents (Special handling)
            var accountTypesDic = accountTypes.ToDictionary(e => e.Id, e => e);
            result.Lines = lineDefs.ToDictionary(def => def.Id, def => MapLineDefinition(def, accountTypesDic));
            result.Documents = docDefs.ToDictionary(def => def.Id, def => MapDocumentDefinition(def, result.Lines));

            // Set built in Ids for ease of access
            result.ManualJournalVouchersDefinitionId = result.Documents.FirstOrDefault(e => e.Value.Code == "manual-journal-vouchers").Key;
            if (result.ManualJournalVouchersDefinitionId == default)
            {
                throw new BadRequestException($"The database is in an inconsistent state, the built in document definition: 'manual-journal-vouchers' could not be found");
            }

            result.ManualLinesDefinitionId = result.Documents.FirstOrDefault(e => e.Value.Code == "ManualLine").Key;
            if (result.ManualJournalVouchersDefinitionId == default)
            {
                throw new BadRequestException($"The database is in an inconsistent state, the built in line definition: 'ManualLine' could not be found");
            }


            // Return result
            return new Versioned<DefinitionsForClient>
            {
                Data = result,
                Version = version.ToString()
            };
        }
    }
}

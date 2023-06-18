using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Repository.Application;
using Tellma.Utilities.Caching;

namespace Tellma.Api
{
    internal class DefinitionsCache : VersionCache<int, DefinitionsForClient>, IDefinitionsCache
    {
        private const string ManualLine = nameof(ManualLine);
        private const string ManualJournalVoucher = nameof(ManualJournalVoucher);

        private readonly IApplicationRepositoryFactory _repoFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionsCache"/> class.
        /// </summary>
        public DefinitionsCache(IApplicationRepositoryFactory repoFactory)
        {
            _repoFactory = repoFactory;
        }

        /// <summary>
        /// Implementation of <see cref="VersionCache{TKey, TData}"/>.
        /// </summary>
        protected override async Task<(DefinitionsForClient data, string version)> GetDataFromSource(int tenantId, CancellationToken cancellation)
        {
            var repo = _repoFactory.GetRepository(tenantId);
            DefinitionsOutput defResult = await repo.Definitions__Load(cancellation);

            var version = defResult.Version.ToString();
            var referenceSourceDefCodes = defResult.ReferenceSourceDefinitionCodes;
            var lookupDefs = defResult.LookupDefinitions;
            var agentDefs = defResult.AgentDefinitions;
            var resourceDefs = defResult.ResourceDefinitions;
            var reportDefs = defResult.ReportDefinitions;
            var dashboardDefs = defResult.DashboardDefinitions;
            var docDefs = defResult.DocumentDefinitions;
            var lineDefs = defResult.LineDefinitions;
            var printingTemplates = defResult.PrintingTemplates;
            var emailTemplates = defResult.EmailTemplates;
            var messageTemplates = defResult.MessageTemplates;
            var entryAgentDefs = defResult.EntryAgentDefinitionIds;
            var entryResourceDefs = defResult.EntryResourceDefinitionIds;
            var entryNotedAgentDefs = defResult.EntryNotedAgentDefinitionIds;
            var entryNotedResourceDefs = defResult.EntryNotedResourceDefinitionIds;

            // Map Lookups, Agents, Resources, Reports (Straight forward)
            var forClient = new DefinitionsForClient
            {
                Lookups = lookupDefs.ToDictionary(def => def.Id, MapLookupDefinition),
                Agents = agentDefs.ToDictionary(def => def.Id, MapAgentDefinition),
                Resources = resourceDefs.ToDictionary(def => def.Id, MapResourceDefinition),
                Reports = reportDefs.ToDictionary(def => def.Id, MapReportDefinition),
                Dashboards = dashboardDefs.ToDictionary(def => def.Id, MapDashboardDefinition),
                Lines = lineDefs.ToDictionary(def => def.Id, def => MapLineDefinition(def, entryAgentDefs, entryResourceDefs, entryNotedAgentDefs, entryNotedResourceDefs)),
                PrintingTemplates = printingTemplates.ToDictionary(def => def.Id, MapPrintingTemplate),
                EmailTemplates = emailTemplates.ToDictionary(def => def.Id, MapEmailTemplate),
                MessageTemplates = messageTemplates.ToDictionary(def => def.Id, MapMessageTemplate),
                ReferenceSourceDefinitionIds = referenceSourceDefCodes.Split(",")
                    .Select(code => agentDefs.FirstOrDefault(def => def.Code == code))
                    .Where(r => r != null)
                    .Select(r => r.Id)
            };

            // Map Lines and Documents (Special handling)
            forClient.Documents = docDefs.ToDictionary(def => def.Id, def => MapDocumentDefinition(def, forClient.Lines));

            // Set built in Ids for ease of access
            forClient.ManualJournalVouchersDefinitionId = forClient.Documents.FirstOrDefault(e => e.Value.Code == ManualJournalVoucher).Key;
            if (forClient.ManualJournalVouchersDefinitionId == default)
            {
                throw new InvalidOperationException($"Database in an inconsistent state, the built-in document definition: '{ManualJournalVoucher}' could not be found. TenantId {tenantId}.");
            }

            forClient.ManualLinesDefinitionId = forClient.Lines.FirstOrDefault(e => e.Value.Code == ManualLine).Key;
            if (forClient.ManualJournalVouchersDefinitionId == default)
            {
                throw new InvalidOperationException($"Database in an inconsistent state, the built-in line definition: '{ManualLine}' could not be found. TenantId {tenantId}.");
            }

            return (forClient, version);
        }

        /// <summary>
        /// Returns the company definitions from the cache if <paramref name="version"/> matches 
        /// the cached version, otherwise retrieves the definitions from the database.
        /// <para/>
        /// Note: The calling service has to retrieve the <paramref name="version"/> independently using 
        /// <see cref="ApplicationRepository.OnConnect"/>, all services already do that to retrieve the 
        /// user Id so they retrieve the <paramref name="version"/> in the same database call as a performance optimization.
        /// </summary>
        /// <param name="tenantId">The ID of the company whose definitions to load.</param>
        /// <param name="version">The latest version of the definitions.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>The company's definitions packaged in a <see cref="DefinitionsForClient"/> object, together with their version.</returns>
        /// <exception cref="InvalidOperationException">When the manual JV document definition 
        /// is missing or when the manual line definition is missing.</exception>
        public async Task<Versioned<DefinitionsForClient>> GetDefinitions(int tenantId, string version, CancellationToken cancellation = default)
        {
            var (data, newVersion) = await GetData(tenantId, version, cancellation);
            return new Versioned<DefinitionsForClient>(data, newVersion);
        }

        #region Helper Functions

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

        private static AgentDefinitionForClient MapAgentDefinition(AgentDefinition def)
        {
            return new AgentDefinitionForClient
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

                ExternalReferenceLabel = def.ExternalReferenceLabel,
                ExternalReferenceLabel2 = def.ExternalReferenceLabel2,
                ExternalReferenceLabel3 = def.ExternalReferenceLabel3,
                ExternalReferenceVisibility = MapVisibility(def.ExternalReferenceVisibility),

                // Agent Only
                Agent1Label = def.Agent1Label,
                Agent1Label2 = def.Agent1Label2,
                Agent1Label3 = def.Agent1Label3,
                Agent1Visibility = MapVisibility(def.Agent1Visibility),
                Agent1DefinitionId = def.Agent1DefinitionId,

                // Agent Only
                Agent2Label = def.Agent2Label,
                Agent2Label2 = def.Agent2Label2,
                Agent2Label3 = def.Agent2Label3,
                Agent2Visibility = MapVisibility(def.Agent2Visibility),
                Agent2DefinitionId = def.Agent2DefinitionId,

                TaxIdentificationNumberVisibility = MapVisibility(def.TaxIdentificationNumberVisibility),
                BankAccountNumberVisibility = MapVisibility(def.BankAccountNumberVisibility),
                UserCardinality = MapCardinality(def.UserCardinality),
                HasAttachments = def.HasAttachments,
                AttachmentsCategoryDefinitionId = def.AttachmentsCategoryDefinitionId,

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
                ResourceDefinitionType = def.ResourceDefinitionType,

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

                Decimal3Label = def.Decimal3Label,
                Decimal3Label2 = def.Decimal3Label2,
                Decimal3Label3 = def.Decimal3Label3,
                Decimal3Visibility = MapVisibility(def.Decimal3Visibility),

                Decimal4Label = def.Decimal4Label,
                Decimal4Label2 = def.Decimal4Label2,
                Decimal4Label3 = def.Decimal4Label3,
                Decimal4Visibility = MapVisibility(def.Decimal4Visibility),

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

                Agent1Label = def.Agent1Label,
                Agent1Label2 = def.Agent1Label2,
                Agent1Label3 = def.Agent1Label3,
                Agent1Visibility = MapVisibility(def.Agent1Visibility),
                Agent1DefinitionId = def.Agent1DefinitionId,

                Agent2Label = def.Agent2Label,
                Agent2Label2 = def.Agent2Label2,
                Agent2Label3 = def.Agent2Label3,
                Agent2Visibility = MapVisibility(def.Agent2Visibility),
                Agent2DefinitionId = def.Agent2DefinitionId,

                Resource1Label = def.Resource1Label,
                Resource1Label2 = def.Resource1Label2,
                Resource1Label3 = def.Resource1Label3,
                Resource1Visibility = MapVisibility(def.Resource1Visibility),
                Resource1DefinitionId = def.Resource1DefinitionId,

                Resource2Label = def.Resource2Label,
                Resource2Label2 = def.Resource2Label2,
                Resource2Label3 = def.Resource2Label3,
                Resource2Visibility = MapVisibility(def.Resource2Visibility),
                Resource2DefinitionId = def.Resource2DefinitionId,

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
                Collection = def.Collection,
                DefinitionId = def.DefinitionId,
                Type = def.Type,

                // Title
                Id = def.Id,
                Code = def.Code,
                Title = def.Title,
                Title2 = def.Title2,
                Title3 = def.Title3,
                Description = def.Description,
                Description2 = def.Description2,
                Description3 = def.Description3,

                // Data
                Rows = def.Rows?.Select(r => new ReportDefinitionDimensionForClient
                {
                    KeyExpression = r.KeyExpression,
                    DisplayExpression = r.DisplayExpression,
                    Localize = r.Localize ?? false,
                    Label = r.Label,
                    Label2 = r.Label2,
                    Label3 = r.Label3,
                    OrderDirection = r.OrderDirection,
                    AutoExpandLevel = r.AutoExpandLevel ?? 0,
                    ShowAsTree = r.ShowAsTree ?? false,
                    Control = r.Control,
                    ControlOptions = r.ControlOptions,
                    Attributes = r.Attributes?.Select(a => new ReportDefinitionDimensionAttributeForClient
                    {
                        Expression = a.Expression,
                        Localize = a.Localize ?? false,
                        Label = a.Label,
                        Label2 = a.Label2,
                        Label3 = a.Label3,
                        OrderDirection = a.OrderDirection,
                    })?.ToList() ?? new List<ReportDefinitionDimensionAttributeForClient>(),
                })?.ToList() ?? new List<ReportDefinitionDimensionForClient>(),
                ShowRowsTotal = def.ShowRowsTotal ?? false,
                RowsTotalLabel = def.RowsTotalLabel,
                RowsTotalLabel2 = def.RowsTotalLabel2,
                RowsTotalLabel3 = def.RowsTotalLabel3,

                Columns = def.Columns?.Select(c => new ReportDefinitionDimensionForClient
                {
                    KeyExpression = c.KeyExpression,
                    DisplayExpression = c.DisplayExpression,
                    Localize = c.Localize ?? false,
                    Label = c.Label,
                    Label2 = c.Label2,
                    Label3 = c.Label3,
                    OrderDirection = c.OrderDirection,
                    AutoExpandLevel = c.AutoExpandLevel ?? 0,
                    ShowAsTree = c.ShowAsTree ?? false,
                    Control = c.Control,
                    ControlOptions = c.ControlOptions,
                    Attributes = c.Attributes?.Select(a => new ReportDefinitionDimensionAttributeForClient
                    {
                        Expression = a.Expression,
                        Localize = a.Localize ?? false,
                        Label = a.Label,
                        Label2 = a.Label2,
                        Label3 = a.Label3,
                        OrderDirection = a.OrderDirection,
                    })?.ToList() ?? new List<ReportDefinitionDimensionAttributeForClient>(),
                })?.ToList() ?? new List<ReportDefinitionDimensionForClient>(),
                ShowColumnsTotal = def.ShowColumnsTotal ?? false,
                ColumnsTotalLabel = def.ColumnsTotalLabel,
                ColumnsTotalLabel2 = def.ColumnsTotalLabel2,
                ColumnsTotalLabel3 = def.ColumnsTotalLabel3,

                Measures = def.Measures?.Select(m => new ReportDefinitionMeasureForClient
                {
                    Expression = m.Expression,
                    Label = m.Label,
                    Label2 = m.Label2,
                    Label3 = m.Label3,
                    OrderDirection = m.OrderDirection,
                    Control = m.Control,
                    ControlOptions = m.ControlOptions,
                    DangerWhen = m.DangerWhen,
                    WarningWhen = m.WarningWhen,
                    SuccessWhen = m.SuccessWhen,
                })?.ToList() ?? new List<ReportDefinitionMeasureForClient>(),

                Select = def.Select?.Select(s => new ReportDefinitionSelectForClient
                {
                    Expression = s.Expression,
                    Localize = s.Localize ?? false,
                    Label = s.Label,
                    Label2 = s.Label2,
                    Label3 = s.Label3,
                    Control = s.Control,
                    ControlOptions = s.ControlOptions,
                })?.ToList() ?? new List<ReportDefinitionSelectForClient>(),

                OrderBy = def.OrderBy,
                Top = def.Top ?? 0,

                // Filter
                Filter = def.Filter,
                Having = def.Having,
                Parameters = def.Parameters?.Select(p => new ReportDefinitionParameterForClient
                {
                    Key = p.Key,
                    Label = p.Label,
                    Label2 = p.Label2,
                    Label3 = p.Label3,
                    Visibility = p.Visibility,
                    DefaultExpression = p.DefaultExpression,
                    Control = p.Control,
                    ControlOptions = p.ControlOptions,
                })?.ToList() ?? new List<ReportDefinitionParameterForClient>(),

                // Drilldown
                IsCustomDrilldown = def.IsCustomDrilldown ?? false,

                // Chart
                Chart = def.Chart,
                DefaultsToChart = def.DefaultsToChart ?? false,
                ChartOptions = def.ChartOptions,

                // Main Menu
                ShowInMainMenu = def.ShowInMainMenu ?? false,
                MainMenuIcon = def.MainMenuIcon,
                MainMenuSortKey = def.MainMenuSortKey ?? 0m,
                MainMenuSection = def.MainMenuSection,
            };
        }

        private static DashboardDefinitionForClient MapDashboardDefinition(DashboardDefinition def)
        {
            return new DashboardDefinitionForClient
            {
                // Title
                Id = def.Id,
                Code = def.Code,
                Title = def.Title,
                Title2 = def.Title2,
                Title3 = def.Title3,

                // Widgets
                AutoRefreshPeriodInMinutes = def.AutoRefreshPeriodInMinutes ?? 0,

                Widgets = def.Widgets?.Select(m => new DashboardDefinitionWidgetForClient
                {
                    ReportDefinitionId = m.ReportDefinitionId.Value,
                    Title = m.Title,
                    Title2 = m.Title2,
                    Title3 = m.Title3,
                    AutoRefreshPeriodInMinutes = m.AutoRefreshPeriodInMinutes,
                    OffsetX = Math.Min(m.OffsetX ?? 0, 1000),
                    OffsetY = Math.Min(m.OffsetY ?? 0, 1000),
                    Width = Math.Min(m.Width ?? 0, 16),
                    Height = Math.Min(m.Height ?? 0, 16)
                })?.ToList() ?? new List<DashboardDefinitionWidgetForClient>(),

                // Main Menu
                ShowInMainMenu = def.ShowInMainMenu ?? false,
                MainMenuIcon = def.MainMenuIcon,
                MainMenuSortKey = def.MainMenuSortKey ?? 0m,
                MainMenuSection = def.MainMenuSection,
            };
        }

        private static LineDefinitionForClient MapLineDefinition(LineDefinition def,
            IReadOnlyDictionary<int, List<int>> entryAgentDefs,
            IReadOnlyDictionary<int, List<int>> entryResourceDefs,
            IReadOnlyDictionary<int, List<int>> entryNotedAgentDefs,
            IReadOnlyDictionary<int, List<int>> entryNotedResourceDefs)
        {
            var line = new LineDefinitionForClient
            {
                // Basics
                Code = def.Code,
                LineType = def.LineType.Value,
                TitlePlural = def.TitlePlural,
                TitlePlural2 = def.TitlePlural2,
                TitlePlural3 = def.TitlePlural3,
                TitleSingular = def.TitleSingular,
                TitleSingular2 = def.TitleSingular2,
                TitleSingular3 = def.TitleSingular3,

                // Data
                AllowSelectiveSigning = def.AllowSelectiveSigning ?? false,
                ViewDefaultsToForm = def.ViewDefaultsToForm ?? false,

                // Barcode stuff
                BarcodeColumnIndex = def.BarcodeColumnIndex,
                BarcodeProperty = def.BarcodeProperty,
                BarcodeExistingItemHandling = def.BarcodeExistingItemHandling,
                BarcodeBeepsEnabled = def.BarcodeBeepsEnabled ?? false,

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

                    AgentDefinitionIds = entryAgentDefs.GetValueOrDefault(e.Id) ?? new List<int>(),
                    NotedAgentDefinitionIds = entryNotedAgentDefs.GetValueOrDefault(e.Id) ?? new List<int>(),
                    ResourceDefinitionIds = entryResourceDefs.GetValueOrDefault(e.Id) ?? new List<int>(),
                    NotedResourceDefinitionIds = entryNotedResourceDefs.GetValueOrDefault(e.Id) ?? new List<int>(),
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
                    Control = p.Control,
                    ControlOptions = p.ControlOptions,
                    Visibility = MapVisibility(p.Visibility)
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
                if (col.ColumnName == nameof(Entry.CurrencyId) || col.ColumnName == nameof(Entry.CenterId))
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
                TitlePlural = def.TitlePlural,
                TitlePlural2 = def.TitlePlural2,
                TitlePlural3 = def.TitlePlural3,
                TitleSingular = def.TitleSingular,
                TitleSingular2 = def.TitleSingular2,
                TitleSingular3 = def.TitleSingular3,
                Prefix = def.Prefix,
                CodeWidth = def.CodeWidth ?? 4,

                PostingDateVisibility = MapVisibility(def.PostingDateVisibility),
                CenterVisibility = MapVisibility(def.CenterVisibility),
                MemoVisibility = MapVisibility(def.MemoVisibility),
                ClearanceVisibility = MapVisibility(def.ClearanceVisibility),

                HasBookkeeping = def.HasBookkeeping.Value,
                AttachmentVisibility = MapVisibility(def.AttachmentVisibility),

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
                AgentDefinitionIds = new List<int>(),
                ResourceDefinitionIds = new List<int>(),
                NotedAgentDefinitionIds = new List<int>(),
                NotedResourceDefinitionIds = new List<int>(),
            };

            // Here we compute some values based on the associated line definitions
            var documentLineDefinitions = result.LineDefinitions
                .Select(e => lineDefsDic.GetValueOrDefault(e.LineDefinitionId))
                .Where(e => e != null && e.Columns != null);

            // Lines
            var agentDefIds = new HashSet<int>();
            var resourceDefIds = new HashSet<int>();
            var notedAgentDefIds = new HashSet<int>();
            var notedResourceDefIds = new HashSet<int>();
            // var referenceSourceDefIds = new HashSet<int>();

            var agentFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var resourceFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var notedAgentFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var notedResourceFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var referenceSourceFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var currencyFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var centerFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var unitFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var durationUnitFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var lineDef in documentLineDefinitions.Where(e => e.Code != ManualLine))
            {
                foreach (var colDef in lineDef.Columns.Where(c => c.InheritsFromHeader == InheritsFrom.DocumentHeader))
                {
                    // The first 3 act different then the rest
                    switch (colDef.ColumnName)
                    {
                        // PostingDate
                        case nameof(Line.PostingDate):
                            {
                                result.PostingDateIsCommonVisibility = true;
                                result.PostingDateVisibility ??= Visibility.Optional;
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
                            break;

                        // Center
                        case nameof(Entry.CenterId):
                            {
                                result.CenterIsCommonVisibility = true;
                                result.CenterVisibility ??= Visibility.Optional;
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
                            break;

                        // Memo
                        case nameof(Line.Memo):
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
                            break;

                        // Currency
                        case nameof(Entry.CurrencyId):
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
                            break;

                        // Agent
                        case nameof(Entry.AgentId):
                            {
                                result.AgentVisibility = true;
                                if (string.IsNullOrWhiteSpace(result.AgentLabel))
                                {
                                    result.AgentLabel = colDef.Label;
                                    result.AgentLabel2 = colDef.Label2;
                                    result.AgentLabel3 = colDef.Label3;
                                }

                                if (colDef.RequiredState > (result.AgentRequiredState ?? 0))
                                {
                                    result.AgentRequiredState = colDef.RequiredState;
                                }

                                if (colDef.ReadOnlyState > (result.AgentReadOnlyState ?? 0))
                                {
                                    result.AgentReadOnlyState = colDef.ReadOnlyState;
                                }

                                // Accumulate all the agent definition IDs in the hash set
                                if (colDef.EntryIndex < lineDef.Entries.Count)
                                {
                                    var entryDef = lineDef.Entries[colDef.EntryIndex];
                                    if (entryDef.AgentDefinitionIds == null || entryDef.AgentDefinitionIds.Count == 0)
                                    {
                                        agentDefIds = null; // Means no definitionIds will be added
                                    }
                                    else if (agentDefIds != null)
                                    {
                                        entryDef.AgentDefinitionIds.ForEach(defId => agentDefIds.Add(defId));
                                    }
                                }

                                // Accumulate all the filter atoms in the hash set
                                if (string.IsNullOrWhiteSpace(colDef.Filter))
                                {
                                    agentFilters = null; // It means no filters will be added
                                }
                                else if (agentFilters != null)
                                {
                                    agentFilters.Add(colDef.Filter);
                                }
                            }
                            break;

                        // Resource
                        case nameof(Entry.ResourceId):
                            {
                                result.ResourceVisibility = true;
                                if (string.IsNullOrWhiteSpace(result.ResourceLabel))
                                {
                                    result.ResourceLabel = colDef.Label;
                                    result.ResourceLabel2 = colDef.Label2;
                                    result.ResourceLabel3 = colDef.Label3;
                                }

                                if (colDef.RequiredState > (result.ResourceRequiredState ?? 0))
                                {
                                    result.ResourceRequiredState = colDef.RequiredState;
                                }

                                if (colDef.ReadOnlyState > (result.ResourceReadOnlyState ?? 0))
                                {
                                    result.ResourceReadOnlyState = colDef.ReadOnlyState;
                                }

                                // Accumulate all the resource definition IDs in the hash set
                                if (colDef.EntryIndex < lineDef.Entries.Count)
                                {
                                    var entryDef = lineDef.Entries[colDef.EntryIndex];
                                    if (entryDef.ResourceDefinitionIds == null || entryDef.ResourceDefinitionIds.Count == 0)
                                    {
                                        resourceDefIds = null; // Means no definitionIds will be added
                                    }
                                    else if (resourceDefIds != null)
                                    {
                                        entryDef.ResourceDefinitionIds.ForEach(defId => resourceDefIds.Add(defId));
                                    }
                                }

                                // Accumulate all the filter atoms in the hash set
                                if (string.IsNullOrWhiteSpace(colDef.Filter))
                                {
                                    resourceFilters = null; // It means no filters will be added
                                }
                                else if (resourceFilters != null)
                                {
                                    resourceFilters.Add(colDef.Filter);
                                }
                            }
                            break;

                        // NotedAgent
                        case nameof(Entry.NotedAgentId):
                            {
                                result.NotedAgentVisibility = true;
                                if (string.IsNullOrWhiteSpace(result.NotedAgentLabel))
                                {
                                    result.NotedAgentLabel = colDef.Label;
                                    result.NotedAgentLabel2 = colDef.Label2;
                                    result.NotedAgentLabel3 = colDef.Label3;
                                }

                                if (colDef.RequiredState > (result.NotedAgentRequiredState ?? 0))
                                {
                                    result.NotedAgentRequiredState = colDef.RequiredState;
                                }

                                if (colDef.ReadOnlyState > (result.NotedAgentReadOnlyState ?? 0))
                                {
                                    result.NotedAgentReadOnlyState = colDef.ReadOnlyState;
                                }

                                // Accumulate all the notedAgent definition IDs in the hash set
                                if (colDef.EntryIndex < lineDef.Entries.Count)
                                {
                                    var entryDef = lineDef.Entries[colDef.EntryIndex];
                                    if (entryDef.NotedAgentDefinitionIds == null || entryDef.NotedAgentDefinitionIds.Count == 0)
                                    {
                                        notedAgentDefIds = null; // Means no definitionIds will be added
                                    }
                                    else if (notedAgentDefIds != null)
                                    {
                                        entryDef.NotedAgentDefinitionIds.ForEach(defId => notedAgentDefIds.Add(defId));
                                    }
                                }

                                // Accumulate all the filter atoms in the hash set
                                if (string.IsNullOrWhiteSpace(colDef.Filter))
                                {
                                    notedAgentFilters = null; // It means no filters will be added
                                }
                                else if (notedAgentFilters != null)
                                {
                                    notedAgentFilters.Add(colDef.Filter);
                                }
                            }
                            break;

                        // NotedResource
                        case nameof(Entry.NotedResourceId):
                            {
                                result.NotedResourceVisibility = true;
                                if (string.IsNullOrWhiteSpace(result.NotedResourceLabel))
                                {
                                    result.NotedResourceLabel = colDef.Label;
                                    result.NotedResourceLabel2 = colDef.Label2;
                                    result.NotedResourceLabel3 = colDef.Label3;
                                }

                                if (colDef.RequiredState > (result.NotedResourceRequiredState ?? 0))
                                {
                                    result.NotedResourceRequiredState = colDef.RequiredState;
                                }

                                if (colDef.ReadOnlyState > (result.NotedResourceReadOnlyState ?? 0))
                                {
                                    result.NotedResourceReadOnlyState = colDef.ReadOnlyState;
                                }

                                // Accumulate all the notedResource definition IDs in the hash set
                                if (colDef.EntryIndex < lineDef.Entries.Count)
                                {
                                    var entryDef = lineDef.Entries[colDef.EntryIndex];
                                    if (entryDef.NotedResourceDefinitionIds == null || entryDef.NotedResourceDefinitionIds.Count == 0)
                                    {
                                        notedResourceDefIds = null; // Means no definitionIds will be added
                                    }
                                    else if (notedResourceDefIds != null)
                                    {
                                        entryDef.NotedResourceDefinitionIds.ForEach(defId => notedResourceDefIds.Add(defId));
                                    }
                                }

                                // Accumulate all the filter atoms in the hash set
                                if (string.IsNullOrWhiteSpace(colDef.Filter))
                                {
                                    notedResourceFilters = null; // It means no filters will be added
                                }
                                else if (notedResourceFilters != null)
                                {
                                    notedResourceFilters.Add(colDef.Filter);
                                }
                            }
                            break;

                        // Quantity
                        case nameof(Entry.Quantity):
                            {
                                result.QuantityVisibility = true;
                                if (string.IsNullOrWhiteSpace(result.QuantityLabel))
                                {
                                    result.QuantityLabel = colDef.Label;
                                    result.QuantityLabel2 = colDef.Label2;
                                    result.QuantityLabel3 = colDef.Label3;
                                }
                                if (colDef.RequiredState > (result.QuantityRequiredState ?? 0))
                                {
                                    result.QuantityRequiredState = colDef.RequiredState;
                                }

                                if (colDef.ReadOnlyState > (result.QuantityReadOnlyState ?? 0))
                                {
                                    result.QuantityReadOnlyState = colDef.ReadOnlyState;
                                }
                            }
                            break;

                        // Unit
                        case nameof(Entry.UnitId):
                            {
                                result.UnitVisibility = true;
                                if (string.IsNullOrWhiteSpace(result.UnitLabel))
                                {
                                    result.UnitLabel = colDef.Label;
                                    result.UnitLabel2 = colDef.Label2;
                                    result.UnitLabel3 = colDef.Label3;
                                }

                                if (colDef.RequiredState > (result.UnitRequiredState ?? 0))
                                {
                                    result.UnitRequiredState = colDef.RequiredState;
                                }

                                if (colDef.ReadOnlyState > (result.UnitReadOnlyState ?? 0))
                                {
                                    result.UnitReadOnlyState = colDef.ReadOnlyState;
                                }

                                // Accumulate all the filter atoms in the hash set
                                if (string.IsNullOrWhiteSpace(colDef.Filter))
                                {
                                    unitFilters = null; // It means no filters will be added
                                }
                                else if (unitFilters != null)
                                {
                                    unitFilters.Add(colDef.Filter);
                                }
                            }
                            break;

                        // Time1
                        case nameof(Entry.Time1):
                            {
                                result.Time1Visibility = true;
                                if (string.IsNullOrWhiteSpace(result.Time1Label))
                                {
                                    result.Time1Label = colDef.Label;
                                    result.Time1Label2 = colDef.Label2;
                                    result.Time1Label3 = colDef.Label3;
                                }
                                if (colDef.RequiredState > (result.Time1RequiredState ?? 0))
                                {
                                    result.Time1RequiredState = colDef.RequiredState;
                                }

                                if (colDef.ReadOnlyState > (result.Time1ReadOnlyState ?? 0))
                                {
                                    result.Time1ReadOnlyState = colDef.ReadOnlyState;
                                }
                            }
                            break;

                        // Duration
                        case nameof(Entry.Duration):
                            {
                                result.DurationVisibility = true;
                                if (string.IsNullOrWhiteSpace(result.DurationLabel))
                                {
                                    result.DurationLabel = colDef.Label;
                                    result.DurationLabel2 = colDef.Label2;
                                    result.DurationLabel3 = colDef.Label3;
                                }
                                if (colDef.RequiredState > (result.DurationRequiredState ?? 0))
                                {
                                    result.DurationRequiredState = colDef.RequiredState;
                                }

                                if (colDef.ReadOnlyState > (result.DurationReadOnlyState ?? 0))
                                {
                                    result.DurationReadOnlyState = colDef.ReadOnlyState;
                                }
                            }
                            break;

                        // DurationUnit
                        case nameof(Entry.DurationUnitId):
                            {
                                result.DurationUnitVisibility = true;
                                if (string.IsNullOrWhiteSpace(result.DurationUnitLabel))
                                {
                                    result.DurationUnitLabel = colDef.Label;
                                    result.DurationUnitLabel2 = colDef.Label2;
                                    result.DurationUnitLabel3 = colDef.Label3;
                                }

                                if (colDef.RequiredState > (result.DurationUnitRequiredState ?? 0))
                                {
                                    result.DurationUnitRequiredState = colDef.RequiredState;
                                }

                                if (colDef.ReadOnlyState > (result.DurationUnitReadOnlyState ?? 0))
                                {
                                    result.DurationUnitReadOnlyState = colDef.ReadOnlyState;
                                }

                                // Accumulate all the filter atoms in the hash set
                                if (string.IsNullOrWhiteSpace(colDef.Filter))
                                {
                                    durationUnitFilters = null; // It means no filters will be added
                                }
                                else if (durationUnitFilters != null)
                                {
                                    durationUnitFilters.Add(colDef.Filter);
                                }
                            }
                            break;

                        // Time2
                        case nameof(Entry.Time2):
                            {
                                result.Time2Visibility = true;
                                if (string.IsNullOrWhiteSpace(result.Time2Label))
                                {
                                    result.Time2Label = colDef.Label;
                                    result.Time2Label2 = colDef.Label2;
                                    result.Time2Label3 = colDef.Label3;
                                }
                                if (colDef.RequiredState > (result.Time2RequiredState ?? 0))
                                {
                                    result.Time2RequiredState = colDef.RequiredState;
                                }

                                if (colDef.ReadOnlyState > (result.Time2ReadOnlyState ?? 0))
                                {
                                    result.Time2ReadOnlyState = colDef.ReadOnlyState;
                                }
                            }
                            break;

                        // NotedDate
                        case nameof(Entry.NotedDate):
                            {
                                result.NotedDateVisibility = true;
                                if (string.IsNullOrWhiteSpace(result.NotedDateLabel))
                                {
                                    result.NotedDateLabel = colDef.Label;
                                    result.NotedDateLabel2 = colDef.Label2;
                                    result.NotedDateLabel3 = colDef.Label3;
                                }
                                if (colDef.RequiredState > (result.NotedDateRequiredState ?? 0))
                                {
                                    result.NotedDateRequiredState = colDef.RequiredState;
                                }

                                if (colDef.ReadOnlyState > (result.NotedDateReadOnlyState ?? 0))
                                {
                                    result.NotedDateReadOnlyState = colDef.ReadOnlyState;
                                }
                            }
                            break;

                        // ExternalReference
                        case nameof(Entry.ExternalReference):
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
                            break;


                        // ReferenceSource
                        case nameof(Entry.ReferenceSourceId):
                            {
                                result.ReferenceSourceVisibility = true;
                                if (string.IsNullOrWhiteSpace(result.ReferenceSourceLabel))
                                {
                                    result.ReferenceSourceLabel = colDef.Label;
                                    result.ReferenceSourceLabel2 = colDef.Label2;
                                    result.ReferenceSourceLabel3 = colDef.Label3;
                                }

                                if (colDef.RequiredState > (result.ReferenceSourceRequiredState ?? 0))
                                {
                                    result.ReferenceSourceRequiredState = colDef.RequiredState;
                                }

                                if (colDef.ReadOnlyState > (result.ReferenceSourceReadOnlyState ?? 0))
                                {
                                    result.ReferenceSourceReadOnlyState = colDef.ReadOnlyState;
                                }

                                //// Accumulate all the ReferenceSource definition Ids in the hash set
                                //if (colDef.EntryIndex < lineDef.Entries.Count)
                                //{
                                //    var entryDef = lineDef.Entries[colDef.EntryIndex];
                                //    if (entryDef.ReferenceSourceDefinitionIds == null || entryDef.ReferenceSourceDefinitionIds.Count == 0)
                                //    {
                                //        referenceSourceDefIds = null; // Means no definitionIds will be added
                                //    }
                                //    else if (referenceSourceDefIds != null)
                                //    {
                                //        entryDef.ReferenceSourceDefinitionIds.ForEach(defId => referenceSourceDefIds.Add(defId));
                                //    }
                                //}

                                // Accumulate all the filter atoms in the hash set
                                if (string.IsNullOrWhiteSpace(colDef.Filter))
                                {
                                    referenceSourceFilters = null; // It means no filters will be added
                                }
                                else if (referenceSourceFilters != null)
                                {
                                    referenceSourceFilters.Add(colDef.Filter);
                                }
                            }
                            break;

                        // InternalReference
                        case nameof(Entry.InternalReference):
                            {
                                result.InternalReferenceVisibility = true;
                                if (string.IsNullOrWhiteSpace(result.InternalReferenceLabel))
                                {
                                    result.InternalReferenceLabel = colDef.Label;
                                    result.InternalReferenceLabel2 = colDef.Label2;
                                    result.InternalReferenceLabel3 = colDef.Label3;
                                }
                                if (colDef.RequiredState > (result.InternalReferenceRequiredState ?? 0))
                                {
                                    result.InternalReferenceRequiredState = colDef.RequiredState;
                                }

                                if (colDef.ReadOnlyState > (result.InternalReferenceReadOnlyState ?? 0))
                                {
                                    result.InternalReferenceReadOnlyState = colDef.ReadOnlyState;
                                }
                            }
                            break;
                    }
                }
            }

            // Calculate the definitionIds and filters
            result.AgentDefinitionIds = agentDefIds?.ToList() ?? new List<int>();
            result.ResourceDefinitionIds = resourceDefIds?.ToList() ?? new List<int>();
            result.NotedAgentDefinitionIds = notedAgentDefIds?.ToList() ?? new List<int>();
            result.NotedResourceDefinitionIds = notedResourceDefIds?.ToList() ?? new List<int>();

            result.AgentFilter = Disjunction(agentFilters);
            result.ResourceFilter = Disjunction(resourceFilters);
            result.NotedAgentFilter = Disjunction(notedAgentFilters);
            result.NotedResourceFilter = Disjunction(notedResourceFilters);
            result.CenterFilter = Disjunction(centerFilters);
            result.CurrencyFilter = Disjunction(currencyFilters);
            result.UnitFilter = Disjunction(unitFilters);
            result.DurationUnitFilter = Disjunction(durationUnitFilters);
            result.ReferenceSourceFilter = Disjunction(referenceSourceFilters);

            #region Manual JV

            // JV has some hard coded values:
            if (def.Code == ManualJournalVoucher)
            {
                // PostingDate
                result.PostingDateVisibility = Visibility.Required;
                result.PostingDateIsCommonVisibility = false;
                result.PostingDateLabel = null;
                result.PostingDateLabel2 = null;
                result.PostingDateLabel3 = null;

                // Center
                // result.CenterVisibility = Visibility.Required;
                result.CenterIsCommonVisibility = false;
                result.CenterLabel = null;
                result.CenterLabel2 = null;
                result.CenterLabel3 = null;

                // Memo
                result.MemoVisibility = Visibility.Optional;
                result.MemoIsCommonVisibility = false;
                result.MemoLabel = null;
                result.MemoLabel2 = null;
                result.MemoLabel3 = null;

                result.CurrencyVisibility = false;

                result.AgentVisibility = false;
                result.ResourceVisibility = false;
                result.NotedAgentVisibility = false;
                result.NotedResourceVisibility = false;

                result.QuantityVisibility = false;
                result.UnitVisibility = false;
                result.Time1Visibility = false;
                result.DurationVisibility = false;
                result.DurationUnitVisibility = false;
                result.Time2Visibility = false;
                result.NotedDateVisibility = false;

                result.InternalReferenceVisibility = false;
                result.ReferenceSourceVisibility = false;
                result.ExternalReferenceVisibility = false;

                result.HasBookkeeping = false;
            }

            #endregion

            // Return result
            return result;
        }

        private static PrintingTemplateForClient MapPrintingTemplate(PrintingTemplate t)
        {
            return new PrintingTemplateForClient
            {
                PrintingTemplateId = t.Id,
                Name = t.Name,
                Name2 = t.Name2,
                Name3 = t.Name3,
                SupportsPrimaryLanguage = t.SupportsPrimaryLanguage.Value,
                SupportsSecondaryLanguage = t.SupportsSecondaryLanguage.Value,
                SupportsTernaryLanguage = t.SupportsTernaryLanguage.Value,
                Usage = t.Usage,
                Collection = t.Collection,
                DefinitionId = t.DefinitionId,
                Parameters = t.Parameters?.Select(p => new TemplateParameterForClient
                {
                    Key = p.Key,
                    Label = p.Label,
                    Label2 = p.Label2,
                    Label3 = p.Label3,
                    IsRequired = p.IsRequired ?? false,
                    Control = p.Control,
                    ControlOptions = p.ControlOptions
                })?.ToList() ?? new List<TemplateParameterForClient>(),

                // Main Menu
                MainMenuIcon = t.MainMenuIcon,
                MainMenuSortKey = t.MainMenuSortKey ?? 0m,
                MainMenuSection = t.MainMenuSection,
            };
        }

        private static EmailTemplateForClient MapEmailTemplate(EmailTemplate t)
        {
            return new EmailTemplateForClient
            {
                EmailTemplateId = t.Id,
                Name = t.Name,
                Name2 = t.Name2,
                Name3 = t.Name3,
                Code = t.Code,
                Cardinality = t.Cardinality,
                Usage = t.Usage,
                Collection = t.Collection,
                DefinitionId = t.DefinitionId,
                MainMenuIcon = t.MainMenuIcon,
                MainMenuSortKey = t.MainMenuSortKey ?? 0m,
                MainMenuSection = t.MainMenuSection,
                Parameters = t.Parameters?.Select(p => new TemplateParameterForClient
                {
                    Key = p.Key,
                    Label = p.Label,
                    Label2 = p.Label2,
                    Label3 = p.Label3,
                    IsRequired = p.IsRequired ?? false,
                    Control = p.Control,
                    ControlOptions = p.ControlOptions
                })?.ToList() ?? new List<TemplateParameterForClient>(),
            };
        }

        private static MessageTemplateForClient MapMessageTemplate(MessageTemplate t)
        {
            return new MessageTemplateForClient
            {
                MessageTemplateId = t.Id,
                Name = t.Name,
                Name2 = t.Name2,
                Name3 = t.Name3,
                Code = t.Code,
                Cardinality = t.Cardinality,
                Usage = t.Usage,
                Collection = t.Collection,
                DefinitionId = t.DefinitionId,
                MainMenuIcon = t.MainMenuIcon,
                MainMenuSortKey = t.MainMenuSortKey ?? 0m,
                MainMenuSection = t.MainMenuSection,
                Parameters = t.Parameters?.Select(p => new TemplateParameterForClient
                {
                    Key = p.Key,
                    Label = p.Label,
                    Label2 = p.Label2,
                    Label3 = p.Label3,
                    IsRequired = p.IsRequired ?? false,
                    Control = p.Control,
                    ControlOptions = p.ControlOptions
                })?.ToList() ?? new List<TemplateParameterForClient>(),
            };
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

        #endregion
    }
}

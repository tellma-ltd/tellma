using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Model.Application;
using Tellma.Model.Common;
using Tellma.Repository.Common;
using Tellma.Utilities.Sharding;

namespace Tellma.Repository.Application
{
    /// <summary>
    /// A thin and lightweight client for the application database (Tellma.Database.Application).
    /// </summary>
    public class ApplicationRepository : RepositoryBase, IQueryFactory
    {
        #region Lifecycle

        private readonly int _tenantId;
        private readonly IShardResolver _shardResolver;
        private readonly ILogger _logger;
        private readonly IStatementLoader _loader;

        protected override ILogger Logger => _logger;

        public ApplicationRepository(int tenantId, IShardResolver shardResolver, ILogger<ApplicationRepository> logger)
        {
            _tenantId = tenantId;
            _shardResolver = shardResolver;
            _logger = logger;
            _loader = new StatementLoader(_logger);
        }

        #endregion

        #region Queries

        public EntityQuery<T> EntityQuery<T>() where T : Entity => new(ArgumentsFactory);

        public FactQuery<T> FactQuery<T>() where T : Entity => new(ArgumentsFactory);

        public AggregateQuery<T> AggregateQuery<T>() where T : Entity => new(ArgumentsFactory);

        private async Task<QueryArguments> ArgumentsFactory(CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);
            var queryArgs = new QueryArguments(Sources, connString, _loader);
            return queryArgs;
        }

        /// <summary>
        /// Returns a function that maps every <see cref="Entity"/> type in the application DB
        /// to the default SQL query that retrieves it. <br/>
        /// Some SQL queries may require additional parameters.
        /// </summary>
        private static string Sources(Type t) => t.Name switch
        {
            nameof(Account) => "[map].[Accounts]()",
            nameof(AccountClassification) => "[map].[AccountClassifications]()",
            nameof(AccountType) => "[map].[AccountTypes]()",
            nameof(AccountTypeNotedRelationDefinition) => "[map].[AccountTypeNotedRelationDefinitions]()",
            nameof(AccountTypeRelationDefinition) => "[map].[AccountTypeRelationDefinitions]()",
            nameof(AccountTypeResourceDefinition) => "[map].[AccountTypeResourceDefinitions]()",
            nameof(Agent) => "[map].[Agents]()",
            nameof(Attachment) => "[map].[Attachments]()",
            nameof(Center) => "[map].[Centers]()",
            nameof(Currency) => "[map].[Currencies]()",
            nameof(DashboardDefinition) => "[map].[DashboardDefinitions]()",
            nameof(DashboardDefinitionRole) => "[map].[DashboardDefinitionRoles]()",
            nameof(DashboardDefinitionWidget) => "[map].[DashboardDefinitionWidgets]()",
            nameof(DetailsEntry) => "[map].[DetailsEntries]()",
            nameof(Document) => "[map].[Documents]()",
            nameof(DocumentAssignment) => "[map].[DocumentAssignmentsHistory]()",
            nameof(DocumentDefinition) => "[map].[DocumentDefinitions]()",
            nameof(DocumentDefinitionLineDefinition) => "[map].[DocumentDefinitionLineDefinitions]()",
            nameof(DocumentLineDefinitionEntry) => "[map].[DocumentLineDefinitionEntries]()",
            nameof(DocumentStateChange) => "[map].[DocumentStatesHistory]()",
            nameof(EmailForQuery) => "[map].[Emails]()",
            nameof(Entry) => "[map].[Entries]()",
            nameof(EntryType) => "[map].[EntryTypes]()",
            nameof(ExchangeRate) => "[map].[ExchangeRates]()",
            nameof(FinancialSettings) => "[map].[FinancialSettings]()",
            nameof(GeneralSettings) => "[map].[GeneralSettings]()",
            nameof(IfrsConcept) => "[map].[IfrsConcepts]()",
            nameof(InboxRecord) => "[map].[Inbox]()",
            nameof(Line) => "[map].[Lines]()",
            nameof(LineDefinition) => "[map].[LineDefinitions]()",
            nameof(LineDefinitionColumn) => "[map].[LineDefinitionColumns]()",
            nameof(LineDefinitionEntry) => "[map].[LineDefinitionEntries]()",
            nameof(LineDefinitionEntryNotedRelationDefinition) => "[map].[LineDefinitionEntryNotedRelationDefinitions]()",
            nameof(LineDefinitionEntryRelationDefinition) => "[map].[LineDefinitionEntryRelationDefinitions]()",
            nameof(LineDefinitionEntryResourceDefinition) => "[map].[LineDefinitionEntryResourceDefinitions]()",
            nameof(LineDefinitionGenerateParameter) => "[map].[LineDefinitionGenerateParameters]()",
            nameof(LineDefinitionStateReason) => "[map].[LineDefinitionStateReasons]()",
            nameof(LineForQuery) => "[map].[Lines]()",
            nameof(Lookup) => "[map].[Lookups]()",
            nameof(LookupDefinition) => "[map].[LookupDefinitions]()",
            nameof(LookupDefinitionReportDefinition) => "[map].[LookupDefinitionReportDefinitions]()",
            nameof(MarkupTemplate) => "[map].[MarkupTemplates]()",
            nameof(OutboxRecord) => "[map].[Outbox]()",
            nameof(Permission) => "[dbo].[Permissions]",
            nameof(Relation) => "[map].[Relations]()",
            nameof(RelationAttachment) => "[map].[RelationAttachments]()",
            nameof(RelationDefinition) => "[map].[RelationDefinitions]()",
            nameof(RelationDefinitionReportDefinition) => "[map].[RelationDefinitionReportDefinitions]()",
            nameof(RelationUser) => "[map].[RelationUsers]()",
            nameof(ReportDefinition) => "[map].[ReportDefinitions]()",
            nameof(ReportDefinitionColumn) => "[map].[ReportDefinitionColumns]()",
            nameof(ReportDefinitionDimensionAttribute) => "[map].[ReportDefinitionDimensionAttributes]()",
            nameof(ReportDefinitionMeasure) => "[map].[ReportDefinitionMeasures]()",
            nameof(ReportDefinitionParameter) => "[map].[ReportDefinitionParameters]()",
            nameof(ReportDefinitionRole) => "[map].[ReportDefinitionRoles]()",
            nameof(ReportDefinitionRow) => "[map].[ReportDefinitionRows]()",
            nameof(ReportDefinitionSelect) => "[map].[ReportDefinitionSelects]()",
            nameof(RequiredSignature) => "[map].[DocumentsRequiredSignatures](@DocumentIds, @UserId)",
            nameof(Resource) => "[map].[Resources]()",
            nameof(ResourceDefinition) => "[map].[ResourceDefinitions]()",
            nameof(ResourceDefinitionReportDefinition) => "[map].[ResourceDefinitionReportDefinitions]()",
            nameof(ResourceUnit) => "[map].[ResourceUnits]()",
            nameof(Role) => "[dbo].[Roles]",
            nameof(RoleMembership) => "[dbo].[RoleMemberships]",
            nameof(SmsMessageForQuery) => "[map].[SmsMessages]()",
            nameof(Unit) => "[map].[Units]()",
            nameof(User) => "[map].[Users]()",
            nameof(Workflow) => "[map].[Workflows]()",
            nameof(WorkflowSignature) => "[map].[WorkflowSignatures]()",

            _ => throw new InvalidOperationException($"The requested type {t.Name} is not supported in {nameof(ApplicationRepository)} queries.")
        };

        public EntityQuery<AccountClassification> AccountClassifications => EntityQuery<AccountClassification>();
        public EntityQuery<AccountType> AccountTypes => EntityQuery<AccountType>();
        public EntityQuery<Agent> Agents => EntityQuery<Agent>();
        public EntityQuery<Center> Centers => EntityQuery<Center>();
        public EntityQuery<Currency> Currencies => EntityQuery<Currency>();
        public EntityQuery<ExchangeRate> ExchangeRates => EntityQuery<ExchangeRate>();
        public EntityQuery<FinancialSettings> FinancialSettings => EntityQuery<FinancialSettings>();
        public EntityQuery<GeneralSettings> GeneralSettings => EntityQuery<GeneralSettings>();
        public EntityQuery<Relation> Relations => EntityQuery<Relation>();
        public EntityQuery<Resource> Resources => EntityQuery<Resource>();
        public EntityQuery<Unit> Units => EntityQuery<Unit>();
        public EntityQuery<User> Users => EntityQuery<User>();

        #endregion

        #region Helpers

        private string _lastConnString = null;
        private string _dbName = null; // Caches the DB Name

        private Task<string> GetConnectionString(CancellationToken cancellation = default) =>
            _shardResolver.GetConnectionString(_tenantId, cancellation) ?? 
            throw new InvalidOperationException($"Connection string for database with Id {_tenantId} could not be resolved.");

        private string DatabaseName(string connString)
        {
            if (_lastConnString != connString) // connString for the same tenantId may change in rare cases
            {
                _lastConnString = connString;
                _dbName = new SqlConnectionStringBuilder(connString).InitialCatalog;
            }

            return _dbName;
        }

        private static void AddCultureAndNeutralCulture(SqlCommand cmd)
        {
            var culture = CultureInfo.CurrentUICulture.Name;
            var neutralCulture = CultureInfo.CurrentUICulture.IsNeutralCulture ? CultureInfo.CurrentUICulture.Name : CultureInfo.CurrentUICulture.Parent.Name;

            cmd.Parameters.Add("@Culture", culture);
            cmd.Parameters.Add("@NeutralCulture", neutralCulture);
        }

        #endregion

        #region Session and Cache

        public async Task<OnConnectResult> OnConnect(string externalUserId, string userEmail, bool setLastActive, CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);

            OnConnectResult result = null;
            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(OnConnect)}]";

                // Parameters
                cmd.Parameters.Add("@ExternalUserId", externalUserId);
                cmd.Parameters.Add("@UserEmail", userEmail);
                cmd.Parameters.Add("@SetLastActive", setLastActive);

                // Execute
                await conn.OpenAsync(cancellation);
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                if (await reader.ReadAsync(cancellation))
                {
                    int i = 0;

                    result = new OnConnectResult
                    (
                        userId: reader.Int32(i++),
                        externalId: reader.String(i++),
                        email: reader.String(i++),
                        permissionsVersion: reader.Guid(i++),
                        userSettingsVersion: reader.Guid(i++),
                        settingsVersion: reader.Guid(i++) ?? throw new InvalidOperationException("[dbo].[Settings] table is empty."),
                        definitionsVersion: reader.Guid(i++) ?? throw new InvalidOperationException("[dbo].[Settings] table is empty.")
                    );
                }
                else
                {
                    throw new InvalidOperationException($"[dal].[{nameof(OnConnect)}] did not return data. TenantId: {_tenantId}, ExternalUserId: {externalUserId}, UserEmail: {userEmail}.");
                }
            },
            DatabaseName(connString), nameof(OnConnect), cancellation);

            return result;
        }

        public async Task<UserSettingsResult> UserSettings__Load(int userId, CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);

            UserSettingsResult result = null;
            await TransactionalDatabaseOperation(async () =>
            {
                User user;
                Guid version;
                var customSettings = new List<(string, string)>();

                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(UserSettings__Load)}]";

                // Parameters
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync(cancellation);
                using var reader = await cmd.ExecuteReaderAsync(cancellation);

                // User + Version
                if (await reader.ReadAsync(cancellation))
                {
                    int i = 0;

                    user = new User
                    {
                        Id = reader.GetInt32(i++),
                        Name = reader.String(i++),
                        Name2 = reader.String(i++),
                        Name3 = reader.String(i++),
                        ImageId = reader.String(i++),
                        PreferredLanguage = reader.String(i++),
                        PreferredCalendar = reader.String(i++),
                    };

                    version = reader.GetGuid(i++);
                }
                else
                {
                    throw new InvalidOperationException($"[dal].[{nameof(UserSettings__Load)}] first data set was empty. TenantId: {_tenantId}, UserId: {userId}.");
                }

                // Custom Settings
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    string key = reader.GetString(0);
                    string val = reader.GetString(1);

                    customSettings.Add((key, val));
                }

                result = new UserSettingsResult(version, user, customSettings);
            },
            DatabaseName(connString), nameof(UserSettings__Load), cancellation);

            return result;
        }

        public async Task<SettingsResult> Settings__Load(CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);

            SettingsResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                int? singleBusinessUnitId;
                GeneralSettings gSettings = new();
                FinancialSettings fSettings = new();

                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Settings__Load)}]";

                // Execute
                // Version
                await conn.OpenAsync(cancellation);
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                if (await reader.ReadAsync(cancellation))
                {
                    int i = 0;

                    singleBusinessUnitId = reader.Int32(i++);
                }
                else
                {
                    // Developer mistake
                    throw new InvalidOperationException($"[dal].[{nameof(Settings__Load)}] first data set was empty. TenantId: {_tenantId}.");
                }

                // General + Financial Settings
                await reader.NextResultAsync(cancellation);

                if (await reader.ReadAsync(cancellation))
                {
                    var gProps = TypeDescriptor.Get<GeneralSettings>().SimpleProperties;
                    foreach (var prop in gProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(gSettings, propValue);
                    }

                    var fProps = TypeDescriptor.Get<FinancialSettings>().SimpleProperties;
                    foreach (var prop in fProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(fSettings, propValue);
                    }
                }
                else
                {
                    // Developer mistake
                    throw new InvalidOperationException($"[dal].[{nameof(Settings__Load)}] second data set was empty. TenantId: {_tenantId}.");
                }

                // Functional Currency
                await reader.NextResultAsync(cancellation);

                if (await reader.ReadAsync(cancellation))
                {
                    fSettings.FunctionalCurrency = new Currency();
                    var props = TypeDescriptor.Get<Currency>().SimpleProperties;
                    foreach (var prop in props)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(fSettings.FunctionalCurrency, propValue);
                    }
                }
                else
                {
                    // Developer mistake
                    throw new InvalidOperationException($"[dal].[{nameof(Settings__Load)}] third data set was empty. TenantId: {_tenantId}.");
                }

                result = new SettingsResult(gSettings.SettingsVersion, singleBusinessUnitId, gSettings, fSettings);
            },
            DatabaseName(connString), nameof(Settings__Load), cancellation);

            return result;
        }

        public async Task<PermissionsResult> Permissions__Load(int userId, CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);

            PermissionsResult result = null;
            await TransactionalDatabaseOperation(async () =>
            {
                Guid version;
                var permissions = new List<AbstractPermission>();
                var reportIds = new List<int>();
                var dashboardIds = new List<int>();

                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Permissions__Load)}]";

                // Parameters
                cmd.Parameters.AddWithValue("@UserId", userId);

                // Execute
                await conn.OpenAsync(cancellation);
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                // Version
                if (await reader.ReadAsync(cancellation))
                {
                    version = reader.GetGuid(0);
                }
                else
                {
                    // Developer mistake
                    throw new InvalidOperationException($"[dal].[{nameof(Permissions__Load)}] first data set was empty. TenantId: {_tenantId}.");
                }

                // Load the permissions
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    permissions.Add(new AbstractPermission
                    {
                        View = reader.String(i++),
                        Action = reader.String(i++),
                        Criteria = reader.String(i++),
                        Mask = reader.String(i++)
                    });
                }

                // Report Ids
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    reportIds.Add(reader.GetInt32(0));
                }

                // Dashboard Ids
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    dashboardIds.Add(reader.GetInt32(0));
                }

                result = new PermissionsResult(version, permissions, reportIds, dashboardIds);
            },
            DatabaseName(connString), nameof(Permissions__Load), cancellation);

            return result;
        }

        public async Task<DefinitionsResult> Definitions__Load(CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);

            DefinitionsResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                Guid version;
                string referenceSourceDefCodes;
                var lookupDefinitions = new List<LookupDefinition>();
                var relationDefinitions = new List<RelationDefinition>();
                var resourceDefinitions = new List<ResourceDefinition>();
                var reportDefinitions = new List<ReportDefinition>();
                var dashboardDefinitions = new List<DashboardDefinition>();
                var documentDefinitions = new List<DocumentDefinition>();
                var lineDefinitions = new List<LineDefinition>();
                var markupTemplates = new List<MarkupTemplate>();

                var entryCustodianDefs = new Dictionary<int, List<int>>();
                var entryRelationDefs = new Dictionary<int, List<int>>();
                var entryResourceDefs = new Dictionary<int, List<int>>();
                var entryNotedRelationDefs = new Dictionary<int, List<int>>();

                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Definitions__Load)}]";


                // Execute
                await conn.OpenAsync(cancellation);
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                // Load the version
                if (await reader.ReadAsync(cancellation))
                {
                    version = reader.GetGuid(0);
                    referenceSourceDefCodes = reader.String(1);
                }
                else
                {
                    version = Guid.Empty;
                    referenceSourceDefCodes = "";
                }

                // Next load lookup definitions
                var lookupDefinitionProps = TypeDescriptor.Get<LookupDefinition>().SimpleProperties;

                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new LookupDefinition();
                    foreach (var prop in lookupDefinitionProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    lookupDefinitions.Add(entity);
                }

                // LookupDefinitionReportDefinitions
                var lookupDefinitionsDic = lookupDefinitions.ToDictionary(e => e.Id);
                var lookupDefinitionReportDefinitionProps = TypeDescriptor.Get<LookupDefinitionReportDefinition>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new LookupDefinitionReportDefinition();
                    foreach (var prop in lookupDefinitionReportDefinitionProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    var lookupDefinition = lookupDefinitionsDic[entity.LookupDefinitionId.Value];
                    lookupDefinition.ReportDefinitions ??= new List<LookupDefinitionReportDefinition>();
                    lookupDefinition.ReportDefinitions.Add(entity);
                }

                // Next load relation definitions
                var relationDefinitionProps = TypeDescriptor.Get<RelationDefinition>().SimpleProperties;

                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new RelationDefinition();
                    foreach (var prop in relationDefinitionProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    relationDefinitions.Add(entity);
                }

                // RelationDefinitionReportDefinitions
                var relationDefinitionsDic = relationDefinitions.ToDictionary(e => e.Id);
                var relationDefinitionReportDefinitionProps = TypeDescriptor.Get<RelationDefinitionReportDefinition>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new RelationDefinitionReportDefinition();
                    foreach (var prop in relationDefinitionReportDefinitionProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    var relationDefinition = relationDefinitionsDic[entity.RelationDefinitionId.Value];
                    relationDefinition.ReportDefinitions ??= new List<RelationDefinitionReportDefinition>();
                    relationDefinition.ReportDefinitions.Add(entity);
                }

                // Next load resource definitions
                var resourceDefinitionProps = TypeDescriptor.Get<ResourceDefinition>().SimpleProperties;

                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new ResourceDefinition();
                    foreach (var prop in resourceDefinitionProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    resourceDefinitions.Add(entity);
                }

                // ResourceDefinitionReportDefinitions
                var resourceDefinitionsDic = resourceDefinitions.ToDictionary(e => e.Id);
                var resourceDefinitionReportDefinitionProps = TypeDescriptor.Get<ResourceDefinitionReportDefinition>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new ResourceDefinitionReportDefinition();
                    foreach (var prop in resourceDefinitionReportDefinitionProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    var resourceDefinition = resourceDefinitionsDic[entity.ResourceDefinitionId.Value];
                    resourceDefinition.ReportDefinitions ??= new List<ResourceDefinitionReportDefinition>();
                    resourceDefinition.ReportDefinitions.Add(entity);
                }

                // Next load report definitions
                await reader.NextResultAsync(cancellation);

                var reportDefinitionsDic = new Dictionary<int, ReportDefinition>();
                var reportDefinitionProps = TypeDescriptor.Get<ReportDefinition>().SimpleProperties;
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new ReportDefinition();
                    foreach (var prop in reportDefinitionProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    reportDefinitionsDic[entity.Id] = entity;
                }

                // Parameters
                var reportDefinitionParameterProps = TypeDescriptor.Get<ReportDefinitionParameter>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new ReportDefinitionParameter();
                    foreach (var prop in reportDefinitionParameterProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    var reportDefinition = reportDefinitionsDic[entity.ReportDefinitionId.Value];
                    reportDefinition.Parameters ??= new List<ReportDefinitionParameter>();
                    reportDefinition.Parameters.Add(entity);
                }

                // Select
                var reportDefinitionSelectProps = TypeDescriptor.Get<ReportDefinitionSelect>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new ReportDefinitionSelect();
                    foreach (var prop in reportDefinitionSelectProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    var reportDefinition = reportDefinitionsDic[entity.ReportDefinitionId.Value];
                    reportDefinition.Select ??= new List<ReportDefinitionSelect>();
                    reportDefinition.Select.Add(entity);
                }

                // Rows
                var attributesDic = new Dictionary<int, List<ReportDefinitionDimensionAttribute>>(); // Dimension Id => Attributes list
                var reportDefinitionRowProps = TypeDescriptor.Get<ReportDefinitionRow>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new ReportDefinitionRow();
                    foreach (var prop in reportDefinitionRowProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    var reportDefinition = reportDefinitionsDic[entity.ReportDefinitionId.Value];
                    reportDefinition.Rows ??= new List<ReportDefinitionRow>();
                    reportDefinition.Rows.Add(entity);

                    entity.Attributes ??= new List<ReportDefinitionDimensionAttribute>();
                    attributesDic[entity.Id] = entity.Attributes;
                }

                // Columns
                var reportDefinitionColumnProps = TypeDescriptor.Get<ReportDefinitionColumn>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new ReportDefinitionColumn();
                    foreach (var prop in reportDefinitionColumnProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    var reportDefinition = reportDefinitionsDic[entity.ReportDefinitionId.Value];
                    reportDefinition.Columns ??= new List<ReportDefinitionColumn>();
                    reportDefinition.Columns.Add(entity);

                    entity.Attributes ??= new List<ReportDefinitionDimensionAttribute>();
                    attributesDic[entity.Id] = entity.Attributes;
                }

                // Dimension Attributes
                var reportDefinitionAttributeProps = TypeDescriptor.Get<ReportDefinitionDimensionAttribute>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new ReportDefinitionDimensionAttribute();
                    foreach (var prop in reportDefinitionAttributeProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    var attributesList = attributesDic[entity.ReportDefinitionDimensionId.Value];
                    attributesList.Add(entity);
                }

                // Measures
                var reportDefinitionMeasureProps = TypeDescriptor.Get<ReportDefinitionMeasure>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new ReportDefinitionMeasure();
                    foreach (var prop in reportDefinitionMeasureProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    var reportDefinition = reportDefinitionsDic[entity.ReportDefinitionId.Value];
                    reportDefinition.Measures ??= new List<ReportDefinitionMeasure>();
                    reportDefinition.Measures.Add(entity);
                }

                reportDefinitions = reportDefinitionsDic.Values.ToList();

                // Dashboard Definitions
                await reader.NextResultAsync(cancellation);
                var dashboardDefinitionsDic = new Dictionary<int, DashboardDefinition>();
                var dashboardDefinitionProps = TypeDescriptor.Get<DashboardDefinition>().SimpleProperties;
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new DashboardDefinition();
                    foreach (var prop in dashboardDefinitionProps)
                    {
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    dashboardDefinitionsDic[entity.Id] = entity;
                }

                // Widgets
                var dashboardDefinitionsWidgetProps = TypeDescriptor.Get<DashboardDefinitionWidget>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new DashboardDefinitionWidget();
                    foreach (var prop in dashboardDefinitionsWidgetProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    var dashboardDefinition = dashboardDefinitionsDic[entity.DashboardDefinitionId.Value];
                    dashboardDefinition.Widgets ??= new List<DashboardDefinitionWidget>();
                    dashboardDefinition.Widgets.Add(entity);
                }


                dashboardDefinitions = dashboardDefinitionsDic.Values.ToList();

                // Next load document definitions
                await reader.NextResultAsync(cancellation);

                var documentDefinitionsDic = new Dictionary<int, DocumentDefinition>();
                var documentDefinitionProps = TypeDescriptor.Get<DocumentDefinition>().SimpleProperties;
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new DocumentDefinition();
                    foreach (var prop in documentDefinitionProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    documentDefinitionsDic[entity.Id] = entity;
                }

                // Document Definitions Line Definitions
                var documentDefinitionLineDefinitionProps = TypeDescriptor.Get<DocumentDefinitionLineDefinition>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new DocumentDefinitionLineDefinition();
                    foreach (var prop in documentDefinitionLineDefinitionProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    var documentDefinition = documentDefinitionsDic[entity.DocumentDefinitionId.Value];
                    documentDefinition.LineDefinitions ??= new List<DocumentDefinitionLineDefinition>();
                    documentDefinition.LineDefinitions.Add(entity);
                }

                documentDefinitions = documentDefinitionsDic.Values.ToList();

                // Next load account types
                var accountTypesDic = new Dictionary<int, AccountType>();
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    var entity = new AccountType
                    {
                        Id = reader.GetInt32(i++),
                        EntryTypeParentId = reader.Int32(i++),
                    };

                    accountTypesDic.Add(entity.Id, entity);
                }

                // Next load line definitions
                await reader.NextResultAsync(cancellation);

                var lineDefinitionsDic = new Dictionary<int, LineDefinition>();
                var lineDefinitionProps = TypeDescriptor.Get<LineDefinition>().SimpleProperties;
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new LineDefinition();
                    foreach (var prop in lineDefinitionProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    lineDefinitionsDic[entity.Id] = entity;
                }

                // line definition entries
                var lineDefinitionEntryProps = TypeDescriptor.Get<LineDefinitionEntry>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new LineDefinitionEntry
                    {
                        ResourceDefinitions = new List<LineDefinitionEntryResourceDefinition>(),
                        RelationDefinitions = new List<LineDefinitionEntryRelationDefinition>(),
                        NotedRelationDefinitions = new List<LineDefinitionEntryNotedRelationDefinition>(),
                    };

                    foreach (var prop in lineDefinitionEntryProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    if (entity.ParentAccountTypeId != null)
                    {
                        entity.ParentAccountType = accountTypesDic.GetValueOrDefault(entity.ParentAccountTypeId.Value);
                    }

                    var lineDefinition = lineDefinitionsDic[entity.LineDefinitionId.Value];
                    lineDefinition.Entries ??= new List<LineDefinitionEntry>();
                    lineDefinition.Entries.Add(entity);
                }

                // line definition columns
                var lineDefinitionColumnProps = TypeDescriptor.Get<LineDefinitionColumn>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new LineDefinitionColumn();
                    foreach (var prop in lineDefinitionColumnProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    var lineDefinition = lineDefinitionsDic[entity.LineDefinitionId.Value];
                    lineDefinition.Columns ??= new List<LineDefinitionColumn>();
                    lineDefinition.Columns.Add(entity);
                }

                // line definition state reason
                var lineDefinitionStateReasonProps = TypeDescriptor.Get<LineDefinitionStateReason>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new LineDefinitionStateReason();
                    foreach (var prop in lineDefinitionStateReasonProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    var lineDefinition = lineDefinitionsDic[entity.LineDefinitionId.Value];
                    lineDefinition.StateReasons ??= new List<LineDefinitionStateReason>();
                    lineDefinition.StateReasons.Add(entity);
                }

                // line definition generate parameter
                var lineDefinitionGenerateParameterProps = TypeDescriptor.Get<LineDefinitionGenerateParameter>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new LineDefinitionGenerateParameter();
                    foreach (var prop in lineDefinitionGenerateParameterProps)
                    {
                        // get property value
                        var propValue = reader.Value(prop.Name);
                        prop.SetValue(entity, propValue);
                    }

                    var lineDefinition = lineDefinitionsDic[entity.LineDefinitionId.Value];
                    lineDefinition.GenerateParameters ??= new List<LineDefinitionGenerateParameter>();
                    lineDefinition.GenerateParameters.Add(entity);
                }

                lineDefinitions = lineDefinitionsDic.Values.ToList();

                // Custodian Definitions
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    var entryId = reader.GetInt32(i++);
                    var defId = reader.GetInt32(i++);

                    if (!entryCustodianDefs.TryGetValue(entryId, out List<int> defIds))
                    {
                        defIds = new List<int>();
                        entryCustodianDefs.Add(entryId, defIds);
                    }

                    defIds.Add(defId);
                }

                // Relation Definitions
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    var entryId = reader.GetInt32(i++);
                    var defId = reader.GetInt32(i++);

                    if (!entryRelationDefs.TryGetValue(entryId, out List<int> defIds))
                    {
                        defIds = new List<int>();
                        entryRelationDefs.Add(entryId, defIds);
                    }

                    defIds.Add(defId);
                }

                // Resource Definitions
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    var entryId = reader.GetInt32(i++);
                    var defId = reader.GetInt32(i++);

                    if (!entryResourceDefs.TryGetValue(entryId, out List<int> defIds))
                    {
                        defIds = new List<int>();
                        entryResourceDefs.Add(entryId, defIds);
                    }

                    defIds.Add(defId);
                }

                // Noted Relation Definitions
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    var entryId = reader.GetInt32(i++);
                    var defId = reader.GetInt32(i++);

                    if (!entryNotedRelationDefs.TryGetValue(entryId, out List<int> defIds))
                    {
                        defIds = new List<int>();
                        entryNotedRelationDefs.Add(entryId, defIds);
                    }

                    defIds.Add(defId);
                }

                // Markup templates
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    markupTemplates.Add(new MarkupTemplate
                    {
                        Id = reader.GetInt32(i++),
                        Name = reader.String(i++),
                        Name2 = reader.String(i++),
                        Name3 = reader.String(i++),
                        SupportsPrimaryLanguage = reader.GetBoolean(i++),
                        SupportsSecondaryLanguage = reader.GetBoolean(i++),
                        SupportsTernaryLanguage = reader.GetBoolean(i++),
                        Usage = reader.String(i++),
                        Collection = reader.String(i++),
                        DefinitionId = reader.Int32(i++),
                    });
                }

                result = new DefinitionsResult(version, referenceSourceDefCodes,
                    lookupDefinitions,
                    relationDefinitions,
                    resourceDefinitions,
                    reportDefinitions,
                    dashboardDefinitions,
                    documentDefinitions,
                    lineDefinitions,
                    markupTemplates,
                    entryCustodianDefs,
                    entryRelationDefs,
                    entryResourceDefs,
                    entryNotedRelationDefs);
            },
            DatabaseName(connString), nameof(Definitions__Load), cancellation);

            return result;
        }

        #endregion

        #region Notifications

        /// <summary>
        /// Adds the Emails and SMSes to the database queue tables in state PENDING 
        /// IF the respective queue table (email, SMS or push) does not have any NEW or stale PENDING items, return TRUE for that collection, otherwise FALSE
        /// </summary>
        public async Task<(bool queueEmails, bool queueSmsMessages, bool queuePushNotifications)> Notifications_Enqueue(
            int expiryInSeconds, List<EmailForSave> emails, List<SmsMessageForSave> smses, List<PushNotificationForSave> pushes, CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);

            bool queueEmails = false;
            bool queueSmsMessages = false;
            bool queuePushNotifications = false;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Notifications_Enqueue)}]";

                // Parameters

                #region Email

                var emailTable = new DataTable();

                emailTable.Columns.Add(new DataColumn("Index", typeof(int)));
                emailTable.Columns.Add(new DataColumn(nameof(EmailForSave.ToEmail), typeof(string)) { MaxLength = 256 });
                emailTable.Columns.Add(new DataColumn(nameof(EmailForSave.Subject), typeof(string)) { MaxLength = 1024 });
                emailTable.Columns.Add(new DataColumn(nameof(EmailForSave.Body), typeof(string)));
                emailTable.Columns.Add(new DataColumn(nameof(EmailForSave.State), typeof(short)));
                emailTable.Columns.Add(new DataColumn(nameof(EmailForSave.ErrorMessage), typeof(string)) { MaxLength = 2048 });

                int emailIndex = 0;
                foreach (var email in emails)
                {
                    DataRow row = emailTable.NewRow();

                    row["Index"] = emailIndex++;
                    row[nameof(email.ToEmail)] = email.ToEmail;
                    row[nameof(email.Subject)] = email.Subject;
                    row[nameof(email.Body)] = email.Body;
                    row[nameof(email.State)] = email.State;
                    row[nameof(email.ErrorMessage)] = email.ErrorMessage;

                    emailTable.Rows.Add(row);
                }

                var emailTvp = new SqlParameter("@Emails", emailTable)
                {
                    TypeName = $"[dbo].[EmailList]",
                    SqlDbType = SqlDbType.Structured
                };

                #endregion

                #region SMS

                var smsTable = new DataTable(); // We won't use the utility function because we don't want to include Id

                smsTable.Columns.Add(new DataColumn("Index", typeof(int)));
                smsTable.Columns.Add(new DataColumn(nameof(SmsMessageForSave.ToPhoneNumber), typeof(string)) { MaxLength = 50 });
                smsTable.Columns.Add(new DataColumn(nameof(SmsMessageForSave.Message), typeof(string)) { MaxLength = 1600 });
                smsTable.Columns.Add(new DataColumn(nameof(SmsMessageForSave.State), typeof(short)));
                smsTable.Columns.Add(new DataColumn(nameof(SmsMessageForSave.ErrorMessage), typeof(string)) { MaxLength = 2048 });

                int smsIndex = 0;
                foreach (var sms in smses)
                {
                    DataRow row = smsTable.NewRow();

                    row["Index"] = smsIndex++;
                    row[nameof(sms.ToPhoneNumber)] = sms.ToPhoneNumber;
                    row[nameof(sms.Message)] = sms.Message;
                    row[nameof(sms.State)] = sms.State;
                    row[nameof(sms.ErrorMessage)] = sms.ErrorMessage;

                    smsTable.Rows.Add(row);
                }

                var smsTvp = new SqlParameter("@SmsMessages", smsTable)
                {
                    TypeName = $"[dbo].[SmsMessageList]",
                    SqlDbType = SqlDbType.Structured
                };

                #endregion

                #region Push

                // TODO

                #endregion

                #region Output Params

                var queueEmailsParam = new SqlParameter("@QueueEmails", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var queueSmsMessagesParam = new SqlParameter("@QueueSmsMessages", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var queuePushNotificationsParam = new SqlParameter("@QueuePushNotifications", SqlDbType.Bit) { Direction = ParameterDirection.Output };

                #endregion

                cmd.Parameters.Add(emailTvp);
                cmd.Parameters.Add(smsTvp);
                // cmd.Parameters.Add(pushTvp);
                cmd.Parameters.Add(queueEmailsParam);
                cmd.Parameters.Add(queueSmsMessagesParam);
                cmd.Parameters.Add(queuePushNotificationsParam);
                cmd.Parameters.AddWithValue("@ExpiryInSeconds", expiryInSeconds);

                // Execute
                await conn.OpenAsync(cancellation);
                using (var reader = await cmd.ExecuteReaderAsync(cancellation))
                {
                    // Load Email Ids
                    while (await reader.ReadAsync(cancellation))
                    {
                        var index = reader.GetInt32(0);
                        var id = reader.GetInt32(1);

                        emails[index].Id = id;
                    }

                    // Load SMS Ids
                    await reader.NextResultAsync(cancellation);
                    while (await reader.ReadAsync(cancellation))
                    {
                        var index = reader.GetInt32(0);
                        var id = reader.GetInt32(1);

                        smses[index].Id = id;
                    }


                    // Load Push Ids
                    // TODO

                } // must close the reader before reading the output params

                // Get the output parameters
                queueEmails = GetValue(queueEmailsParam.Value, false);
                queueSmsMessages = GetValue(queueSmsMessagesParam.Value, false);
                queuePushNotifications = GetValue(queuePushNotificationsParam.Value, false);
            },
            DatabaseName(connString), nameof(Notifications_Enqueue), cancellation);


            // Return the result
            return (queueEmails, queueSmsMessages, queuePushNotifications);
        }

        /// <summary>
        /// Takes a list of (Id, State, Error), and updates the state of every email with a given Id to the given state.
        /// It also marks [StateSince] to the current time and persists the given Error in the Error column if the state is negative
        /// </summary>
        public async Task Notifications_Emails__UpdateState(IEnumerable<IdStateErrorTimestamp> updates, CancellationToken cancellation = default)
        {
            if (updates == null || !updates.Any())
            {
                return;
            }

            // Prep connection
            var connString = await GetConnectionString(cancellation);

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Notifications_Emails__UpdateState)}]";

                // Parameters
                var updatesTable = RepositoryUtilities.DataTable(updates);
                var updatesTvp = new SqlParameter("@Updates", updatesTable)
                {
                    TypeName = $"[dbo].[{nameof(IdStateErrorTimestamp)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(updatesTvp);

                // Execute
                await conn.OpenAsync(cancellation);
                await cmd.ExecuteNonQueryAsync(cancellation);
            },
            DatabaseName(connString), nameof(Notifications_Emails__UpdateState), cancellation);
        }

        /// <summary>
        /// Updates the SMS message with a given Id to a new state, as long as the current 
        /// state is not terminal or greater than the new state. It also marks [StateSince] 
        /// to the current time and persists the given Error in the Error column if the state is negative.
        /// </summary>
        /// <param name="id">The Id of the SMS to update.</param>
        /// <param name="state">The new state.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        public async Task Notifications_SmsMessages__UpdateState(int id, short state, DateTimeOffset timestamp, string error = null, CancellationToken cancellation = default)
        {
            var connString = await GetConnectionString(cancellation);

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Notifications_SmsMessages__UpdateState)}]";

                // Parameters
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@NewState", state);
                cmd.Parameters.AddWithValue("@Timestamp", timestamp);
                cmd.Parameters.Add("@Error", error);

                // Execute
                await conn.OpenAsync(cancellation);
                await cmd.ExecuteNonQueryAsync(cancellation);
            },
            DatabaseName(connString), nameof(Notifications_SmsMessages__UpdateState), cancellation);
        }

        /// <summary>
        /// Returns the Top N emails that are either NEW or stale PENDING after marking them as fresh PENDING.
        /// </summary>
        /// <param name="expiryInSeconds">How many seconds should an email remain pending in the table to be considered "stale".</param>
        /// <param name="top">Maximum number of items to return.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        public async Task<IEnumerable<EmailForSave>> Notifications_Emails__Poll(int expiryInSeconds, int top, CancellationToken cancellation)
        {
            var result = new List<EmailForSave>();

            // Prep connection
            var connString = await GetConnectionString(cancellation);

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Notifications_Emails__Poll)}]";

                // Parameters
                cmd.Parameters.AddWithValue("@ExpiryInSeconds", expiryInSeconds);
                cmd.Parameters.AddWithValue("@Top", top);

                // Execute
                await conn.OpenAsync(cancellation);
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;

                    result.Add(new EmailForSave
                    {
                        Id = reader.GetInt32(i++),
                        ToEmail = reader.GetString(i++),
                        Subject = reader.String(i++),
                        Body = reader.String(i++)
                    });
                }
            },
            DatabaseName(connString), nameof(Notifications_Emails__Poll), cancellation);

            return result;
        }

        /// <summary>
        /// Returns the Top N SMS messages that are either NEW or stale PENDING after marking them as fresh PENDING.
        /// </summary>
        /// <param name="expiryInSeconds">How many seconds should an SMS remain pending in the table to be considered "stale".</param>
        /// <param name="top">Maximum number of items to return.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        public async Task<IEnumerable<SmsMessageForSave>> Notifications_SmsMessages__Poll(int expiryInSeconds, int top, CancellationToken cancellation)
        {
            var result = new List<SmsMessageForSave>();

            // Prep connection
            var connString = await GetConnectionString(cancellation);

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Notifications_SmsMessages__Poll)}]";

                // Parameters
                cmd.Parameters.AddWithValue("@ExpiryInSeconds", expiryInSeconds);
                cmd.Parameters.AddWithValue("@Top", top);

                // Execute
                await conn.OpenAsync(cancellation);
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;

                    result.Add(new SmsMessageForSave
                    {
                        Id = reader.GetInt32(i++),
                        ToPhoneNumber = reader.GetString(i++),
                        Message = reader.GetString(i++)
                    });
                }
            },
            DatabaseName(connString), nameof(Notifications_SmsMessages__Poll), cancellation);

            return result;
        }

        #region Helper Functions

        /// <summary>
        /// Utility function: if obj is <see cref="DBNull.Value"/>, returns the default value of the type, else returns cast value
        /// </summary>
        private static T GetValue<T>(object obj, T defaultValue = default)
        {
            if (obj == DBNull.Value)
            {
                return defaultValue;
            }
            else
            {
                return (T)obj;
            }
        }

        #endregion

        #endregion

        #region AccountClassifications

        public async Task<SaveResult> AccountClassifications__Save(List<AccountClassificationForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(AccountClassifications__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(AccountClassification)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(AccountClassifications__Save));

            return result;
        }

        public async Task<DeleteResult> AccountClassifications__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(AccountClassifications__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(AccountClassifications__Delete));

            return result;
        }

        public async Task<DeleteResult> AccountClassifications__DeleteWithDescendants(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(AccountClassifications__DeleteWithDescendants)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(AccountClassifications__DeleteWithDescendants));

            return result;
        }

        public async Task<OperationResult> AccountClassifications__Activate(List<int> ids, bool isActive, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(AccountClassifications__Activate)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@IsActive", isActive);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);


                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(AccountClassifications__Activate));

            return result;
        }

        #endregion

        #region Accounts

        public async Task Accounts__Preprocess(List<AccountForSave> entities)
        {
            var connString = await GetConnectionString();

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[bll].[{nameof(Accounts__Preprocess)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(Account)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                var props = TypeDescriptor.Get<AccountForSave>().SimpleProperties;
                while (await reader.ReadAsync())
                {
                    var index = reader.GetInt32(0);
                    var entity = entities[index];
                    foreach (var prop in props)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entity, propValue);
                    }
                }
            },
            DatabaseName(connString), nameof(Accounts__Save));
        }

        public async Task<SaveResult> Accounts__Save(List<AccountForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Accounts__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(Account)}List]",
                    SqlDbType = SqlDbType.Structured
                };


                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(Accounts__Save));

            return result;
        }

        public async Task<DeleteResult> Accounts__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Accounts__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(Accounts__Delete));

            return result;
        }

        public async Task<OperationResult> Accounts__Activate(List<int> ids, bool isActive, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Accounts__Activate)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@IsActive", isActive);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(Accounts__Activate));

            return result;
        }

        #endregion

        #region AccountTypes

        public async Task<SaveResult> AccountTypes__Save(List<AccountTypeForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(AccountTypes__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(AccountType)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable relationDefinitionsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.RelationDefinitions);
                var relationDefinitionsTvp = new SqlParameter("@AccountTypeRelationDefinitions", relationDefinitionsTable)
                {
                    TypeName = $"[dbo].[{nameof(AccountTypeRelationDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable resourceDefinitionsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.ResourceDefinitions);
                var resourceDefinitionsTvp = new SqlParameter("@AccountTypeResourceDefinitions", resourceDefinitionsTable)
                {
                    TypeName = $"[dbo].[{nameof(AccountTypeResourceDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable notedRelationDefinitionsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.NotedRelationDefinitions);
                var notedRelationDefinitionsTvp = new SqlParameter("@AccountTypeNotedRelationDefinitions", notedRelationDefinitionsTable)
                {
                    TypeName = $"[dbo].[{nameof(AccountTypeNotedRelationDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add(relationDefinitionsTvp);
                cmd.Parameters.Add(resourceDefinitionsTvp);
                cmd.Parameters.Add(notedRelationDefinitionsTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(AccountTypes__Save));

            return result;
        }

        public async Task<DeleteResult> AccountTypes__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(AccountTypes__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(AccountTypes__Delete));

            return result;
        }

        public async Task<DeleteResult> AccountTypes__DeleteWithDescendants(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(AccountTypes__DeleteWithDescendants)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(AccountTypes__DeleteWithDescendants));

            return result;
        }

        public async Task<OperationResult> AccountTypes__Activate(List<int> ids, bool isActive, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(AccountTypes__Activate)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@IsActive", isActive);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);


                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(AccountTypes__Activate));

            return result;
        }

        #endregion

        #region Agents

        public async Task<SaveResult> Agents__Save(List<AgentForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Agents__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(Agent)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(Agents__Save));

            return result;
        }

        public async Task<DeleteResult> Agents__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Agents__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(Agents__Delete));

            return result;
        }

        public async Task<OperationResult> Agents__Activate(List<int> ids, bool isActive, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Agents__Activate)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@IsActive", isActive);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(Agents__Activate));

            return result;
        }

        #endregion

        #region Blobs

        public async Task Blobs__Delete(IEnumerable<string> blobNames)
        {
            var connString = await GetConnectionString();

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Blobs__Delete)}]";

                // Parameters
                DataTable namesTable = RepositoryUtilities.DataTable(blobNames.Select(id => new StringListItem { Id = id }));
                var namesTvp = new SqlParameter("@BlobNames", namesTable)
                {
                    TypeName = $"[dbo].[StringList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(namesTvp);

                // Execute
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            },
            DatabaseName(connString), nameof(Blobs__Delete));
        }

        public async Task Blobs__Save(string name, byte[] blob)
        {
            var connString = await GetConnectionString();

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Blobs__Save)}]";

                // Parameters
                cmd.Parameters.Add("@Name", name);
                cmd.Parameters.Add("@Blob", blob);

                // Execute
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            },
            DatabaseName(connString), nameof(Blobs__Save));
        }

        public async Task<byte[]> Blobs__Get(string name, CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);
            byte[] result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Blobs__Get)}]";

                // Parameters
                cmd.Parameters.Add("@Name", name);

                // Execute
                await conn.OpenAsync(cancellation);
                using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellation);
                if (await reader.ReadAsync(cancellation))
                {
                    result = (byte[])reader[0];
                }
            },
            DatabaseName(connString), nameof(Blobs__Get), cancellation);

            return result;
        }

        #endregion

        #region Centers

        public async Task<SaveResult> Centers__Save(List<CenterForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Centers__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(Center)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(Centers__Save));

            return result;
        }

        public async Task<DeleteResult> Centers__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Centers__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(Centers__Delete));

            return result;
        }

        public async Task<DeleteResult> Centers__DeleteWithDescendants(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Centers__DeleteWithDescendants)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(Centers__DeleteWithDescendants));

            return result;
        }

        public async Task<OperationResult> Centers__Activate(List<int> ids, bool isActive, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Centers__Activate)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@IsActive", isActive);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(Centers__Activate));

            return result;
        }

        #endregion

        #region Currencies

        public async Task<OperationResult> Currencies__Save(List<CurrencyForSave> entities, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Currencies__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(Currency)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(Currencies__Save));

            return result;
        }

        public async Task<DeleteResult> Currencies__Delete(IEnumerable<string> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Currencies__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new StringListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedStringList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(Currencies__Delete));

            return result;
        }

        public async Task<OperationResult> Currencies__Activate(List<string> ids, bool isActive, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Currencies__Activate)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new StringListItem { Id = id }));
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[StringList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@IsActive", isActive);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(Currencies__Activate));

            return result;
        }

        #endregion

        #region DashboardDefinitions

        public async Task<SaveResult> DashboardDefinitions__Save(List<DashboardDefinitionForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(DashboardDefinitions__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(DashboardDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable widgetsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Widgets);
                var widgetsTvp = new SqlParameter("@Widgets", widgetsTable)
                {
                    TypeName = $"[dbo].[{nameof(DashboardDefinitionWidget)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable rolesTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Roles);
                var rolesTvp = new SqlParameter("@Roles", rolesTable)
                {
                    TypeName = $"[dbo].[{nameof(DashboardDefinitionRole)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add(widgetsTvp);
                cmd.Parameters.Add(rolesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(DashboardDefinitions__Save));

            return result;
        }

        public async Task<DeleteResult> DashboardDefinitions__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(DashboardDefinitions__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(DashboardDefinitions__Delete));

            return result;
        }

        #endregion

        #region DocumentDefinitions

        public async Task<SaveResult> DocumentDefinitions__Save(List<DocumentDefinitionForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(DocumentDefinitions__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(DocumentDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable linesTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.LineDefinitions);
                var linesTvp = new SqlParameter("@DocumentDefinitionLineDefinitions", linesTable)
                {
                    TypeName = $"[dbo].[{nameof(DocumentDefinitionLineDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add(linesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(DocumentDefinitions__Save));

            return result;
        }

        public async Task<DeleteResult> DocumentDefinitions__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(DocumentDefinitions__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(DocumentDefinitions__Delete));

            return result;
        }

        public async Task<OperationResult> DocumentDefinitions__UpdateState(List<int> ids, string state, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(DocumentDefinitions__UpdateState)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@State", state);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(DocumentDefinitions__UpdateState));

            return result;
        }

        #endregion

        #region Documents

        private static (SqlParameter documents, SqlParameter lineDefinitionEntries, SqlParameter lines, SqlParameter entries, SqlParameter attachments) TvpsFromDocuments(IEnumerable<DocumentForSave> documents)
        {
            // Prepare the documents table skeleton
            var docsTable = new DataTable();
            docsTable.Columns.Add(new DataColumn("Index", typeof(int)));
            var docsProps = RepositoryUtilities.AddColumnsFromProperties<DocumentForSave>(docsTable);
            docsTable.Columns.Add(new DataColumn("UpdateAttachments", typeof(bool)));

            // Prepare the line definition entries table skeleton
            var lineDefinitionEntriesTable = new DataTable();
            lineDefinitionEntriesTable.Columns.Add(new DataColumn("Index", typeof(int)));
            lineDefinitionEntriesTable.Columns.Add(new DataColumn("DocumentIndex", typeof(int)));
            var lineDefinitionEntriesProps = RepositoryUtilities.AddColumnsFromProperties<DocumentLineDefinitionEntryForSave>(lineDefinitionEntriesTable);

            // Prepare the lines table skeleton
            var linesTable = new DataTable();
            linesTable.Columns.Add(new DataColumn("Index", typeof(int)));
            linesTable.Columns.Add(new DataColumn("DocumentIndex", typeof(int)));
            var linesProps = RepositoryUtilities.AddColumnsFromProperties<LineForSave>(linesTable);

            // Prepare the entries table skeleton
            var entriesTable = new DataTable();
            entriesTable.Columns.Add(new DataColumn("Index", typeof(int)));
            entriesTable.Columns.Add(new DataColumn("LineIndex", typeof(int)));
            entriesTable.Columns.Add(new DataColumn("DocumentIndex", typeof(int)));
            var entriesProps = RepositoryUtilities.AddColumnsFromProperties<EntryForSave>(entriesTable);

            // Prepare the attachments table skeleton
            var attachmentsTable = new DataTable();
            attachmentsTable.Columns.Add(new DataColumn("DocumentIndex", typeof(int)));
            var attachmentsProps = RepositoryUtilities.AddColumnsFromProperties<AttachmentForSave>(attachmentsTable);
            attachmentsTable.Columns.Add(new DataColumn("FileId", typeof(string)));
            attachmentsTable.Columns.Add(new DataColumn("FileSize", typeof(long)));

            // Add the docs
            int docsIndex = 0;
            foreach (var doc in documents)
            {
                DataRow docsRow = docsTable.NewRow();

                docsRow["Index"] = docsIndex;

                foreach (var docsProp in docsProps)
                {
                    var docsPropValue = docsProp.GetValue(doc);
                    docsRow[docsProp.Name] = docsPropValue ?? DBNull.Value;
                }

                docsRow["UpdateAttachments"] = doc.Attachments != null; // Instructs the SP whether to update the attachments or not

                // Add line definition entries if any
                if (doc.LineDefinitionEntries != null)
                {
                    doc.LineDefinitionEntries.ForEach(lineDefinitionEntry =>
                    {
                        DataRow lineDefinitionEntriesRow = lineDefinitionEntriesTable.NewRow();

                        lineDefinitionEntriesRow["Index"] = lineDefinitionEntry.EntityMetadata.OriginalIndex; // This collection gets culled, so we rely on the preserved index here
                        lineDefinitionEntriesRow["DocumentIndex"] = docsIndex;

                        foreach (var lineDefinitionEntryProp in lineDefinitionEntriesProps)
                        {
                            var lineDefinitionEntriesPropValue = lineDefinitionEntryProp.GetValue(lineDefinitionEntry);
                            lineDefinitionEntriesRow[lineDefinitionEntryProp.Name] = lineDefinitionEntriesPropValue ?? DBNull.Value;
                        }

                        lineDefinitionEntriesTable.Rows.Add(lineDefinitionEntriesRow);
                    });
                }

                // Add the lines if any
                if (doc.Lines != null)
                {
                    int linesIndex = 0;
                    doc.Lines.ForEach(line =>
                    {
                        DataRow linesRow = linesTable.NewRow();

                        linesRow["Index"] = linesIndex;
                        linesRow["DocumentIndex"] = docsIndex;

                        foreach (var linesProp in linesProps)
                        {
                            var linesPropValue = linesProp.GetValue(line);
                            linesRow[linesProp.Name] = linesPropValue ?? DBNull.Value;
                        }

                        if (line.Entries != null)
                        {
                            int entriesIndex = 0;
                            line.Entries.ForEach(entry =>
                            {
                                DataRow entriesRow = entriesTable.NewRow();

                                entriesRow["Index"] = entriesIndex;
                                entriesRow["LineIndex"] = linesIndex;
                                entriesRow["DocumentIndex"] = docsIndex;

                                foreach (var entriesProp in entriesProps)
                                {
                                    var entriesPropValue = entriesProp.GetValue(entry);
                                    entriesRow[entriesProp.Name] = entriesPropValue ?? DBNull.Value;
                                }

                                entriesTable.Rows.Add(entriesRow);
                                entriesIndex++;
                            });
                        }

                        linesTable.Rows.Add(linesRow);
                        linesIndex++;
                    });
                }

                // Add the attachments if any
                if (doc.Attachments != null)
                {
                    doc.Attachments.ForEach(attachment =>
                    {
                        DataRow attachmentsRow = attachmentsTable.NewRow();

                        attachmentsRow["DocumentIndex"] = docsIndex;

                        foreach (var attachmentsProp in attachmentsProps)
                        {
                            var attachmentsPropValue = attachmentsProp.GetValue(attachment);
                            attachmentsRow[attachmentsProp.Name] = attachmentsPropValue ?? DBNull.Value;
                        }

                        attachmentsRow["FileId"] = attachment.EntityMetadata?.FileId;
                        attachmentsRow["FileSize"] = attachment.EntityMetadata?.FileSize;

                        attachmentsTable.Rows.Add(attachmentsRow);
                    });
                }

                docsTable.Rows.Add(docsRow);
                docsIndex++;
            }


            var docsTvp = new SqlParameter("@Documents", docsTable)
            {
                TypeName = $"[dbo].[{nameof(Document)}List]",
                SqlDbType = SqlDbType.Structured
            };

            var lineDefinitionEntriesTvp = new SqlParameter("@DocumentLineDefinitionEntries", lineDefinitionEntriesTable)
            {
                TypeName = $"[dbo].[{nameof(DocumentLineDefinitionEntry)}List]",
                SqlDbType = SqlDbType.Structured
            };

            var linesTvp = new SqlParameter("@Lines", linesTable)
            {
                TypeName = $"[dbo].[{nameof(Line)}List]",
                SqlDbType = SqlDbType.Structured
            };

            var entriesTvp = new SqlParameter("@Entries", entriesTable)
            {
                TypeName = $"[dbo].[{nameof(Entry)}List]",
                SqlDbType = SqlDbType.Structured
            };

            var attachmentsTvp = new SqlParameter("@Attachments", attachmentsTable)
            {
                TypeName = $"[dbo].[{nameof(Attachment)}List]",
                SqlDbType = SqlDbType.Structured
            };

            return (docsTvp, lineDefinitionEntriesTvp, linesTvp, entriesTvp, attachmentsTvp);
        }

        public async Task Documents__Preprocess(int definitionId, List<DocumentForSave> documents)
        {
            var connString = await GetConnectionString();

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[bll].[{nameof(Documents__Preprocess)}]";

                // Parameters

                var (docsTvp, lineDefinitionEntriesTvp, linesTvp, entriesTvp, _) = TvpsFromDocuments(documents);

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(docsTvp);
                cmd.Parameters.Add(lineDefinitionEntriesTvp);
                cmd.Parameters.Add(linesTvp);
                cmd.Parameters.Add(entriesTvp);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                // Documents
                var docProps = TypeDescriptor.Get<DocumentForSave>().SimpleProperties;
                while (await reader.ReadAsync())
                {
                    var index = reader.GetInt32(0);

                    var doc = documents[index];

                    foreach (var prop in docProps)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(doc, propValue);
                    }
                }

                // DocumentLineDefinitionEntries
                await reader.NextResultAsync();
                var lineDefEntriesProps = TypeDescriptor.Get<DocumentLineDefinitionEntryForSave>().SimpleProperties;
                while (await reader.ReadAsync())
                {
                    var index = reader.GetInt32(0);
                    var docIndex = reader.GetInt32(1);

                    var lineDefinitionEntry = documents[docIndex].LineDefinitionEntries[index];

                    foreach (var prop in lineDefEntriesProps)
                    {
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(lineDefinitionEntry, propValue);
                    }
                }

                // Lines
                await reader.NextResultAsync();
                var lineProps = TypeDescriptor.Get<LineForSave>().SimpleProperties;
                while (await reader.ReadAsync())
                {
                    var index = reader.GetInt32(0);
                    var docIndex = reader.GetInt32(1);

                    var line = documents[docIndex].Lines[index];

                    foreach (var prop in lineProps)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(line, propValue);
                    }
                }

                // Entries         
                await reader.NextResultAsync();
                var entryProps = TypeDescriptor.Get<EntryForSave>().SimpleProperties;
                while (await reader.ReadAsync())
                {
                    var index = reader.GetInt32(0);
                    var lineIndex = reader.GetInt32(1);
                    var docIndex = reader.GetInt32(2);

                    var entry = documents[docIndex].Lines[lineIndex].Entries[index];

                    foreach (var prop in entryProps)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entry, propValue);
                    }
                }
            },
            DatabaseName(connString), nameof(Documents__Preprocess));
        }

        public async Task<(SaveResult result, List<InboxStatus> inboxStatuses, List<string> deletedFileIds)> Documents__Save(int definitionId, List<DocumentForSave> documents, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();

            SaveResult result = null;
            List<InboxStatus> inboxStatuses = null;
            List<string> deletedFileIds = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Documents__Save)}]";

                // Parameters
                var (docsTvp, lineDefinitionEntriesTvp, linesTvp, entriesTvp, attachmentsTvp) = TvpsFromDocuments(documents);

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(docsTvp);
                cmd.Parameters.Add(lineDefinitionEntriesTvp);
                cmd.Parameters.Add(linesTvp);
                cmd.Parameters.Add(entriesTvp);
                cmd.Parameters.Add(attachmentsTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                // (1) Load result
                result = await reader.LoadSaveResult(returnIds, validateOnly);

                if (!result.IsError && !validateOnly)
                {
                    // (2) Load inbox statuses
                    await reader.NextResultAsync();
                    inboxStatuses = await reader.LoadInboxStatuses();

                    // (3) Load deleted file Ids
                    deletedFileIds = new List<string>();
                    await reader.NextResultAsync();
                    while (await reader.ReadAsync())
                    {
                        deletedFileIds.Add(reader.GetString(0));
                    }
                }
            },
            DatabaseName(connString), nameof(Documents__Save));

            return (result, inboxStatuses, deletedFileIds);
        }

        public async Task<(
            List<LineForSave> lines,
            List<Account> accounts,
            List<Resource> resources,
            List<Relation> relations,
            List<EntryType> entryTypes,
            List<Center> centers,
            List<Currency> currencies,
            List<Unit> units
            )> Lines__Generate(int lineDefId, Dictionary<string, string> args, CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);

            List<LineForSave> lines = default;
            List<Account> list_Account = default;
            List<Resource> list_Resource = default;
            List<Relation> list_Relation = default;
            List<EntryType> list_EntryType = default;
            List<Center> list_Center = default;
            List<Currency> list_Currency = default;
            List<Unit> list_Unit = default;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[bll].[{nameof(Lines__Generate)}]";

                // Parameters
                DataTable argsTable = RepositoryUtilities.DataTable(args.Select(e => new GenerateArgument { Key = e.Key, Value = e.Value }));
                var argsTvp = new SqlParameter("@GenerateArguments", argsTable)
                {
                    TypeName = $"[dbo].[{nameof(GenerateArgument)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add("@LineDefinitionId", lineDefId);
                cmd.Parameters.Add(argsTvp);

                // Execute
                // Lines for save
                lines = new List<LineForSave>();
                await conn.OpenAsync(cancellation);
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    lines.Add(new LineForSave
                    {
                        DefinitionId = reader.Int32(i++),
                        PostingDate = reader.DateTime(i++),
                        Memo = reader.String(i++),
                        Boolean1 = reader.Boolean(i++),
                        Decimal1 = reader.Decimal(i++),
                        Text1 = reader.String(i++),

                        Entries = new List<EntryForSave>(),
                    });

                    int index = reader.Int32(i++) ?? throw new InvalidOperationException("Returned line [Index] was null");
                    if (lines.Count != index + 1)
                    {
                        throw new InvalidOperationException($"Mismatch between line index {index} and it's actual position {lines.Count - 1} in the returned result set");
                    }
                }

                // Entries for save
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    var entry = new EntryForSave
                    {
                        AccountId = reader.Int32(i++),
                        CurrencyId = reader.String(i++),
                        RelationId = reader.Int32(i++),
                        CustodianId = reader.Int32(i++),
                        NotedRelationId = reader.Int32(i++),
                        ResourceId = reader.Int32(i++),
                        EntryTypeId = reader.Int32(i++),
                        CenterId = reader.Int32(i++),
                        UnitId = reader.Int32(i++),
                        Direction = reader.Int16(i++),
                        MonetaryValue = reader.Decimal(i++),
                        Quantity = reader.Decimal(i++),
                        Value = reader.Decimal(i++) ?? 0m,
                        Time1 = reader.DateTime(i++),
                        Time2 = reader.DateTime(i++),
                        ExternalReference = reader.String(i++),
                        ReferenceSourceId = reader.Int32(i++),
                        InternalReference = reader.String(i++),
                        NotedAgentName = reader.String(i++),
                        NotedAmount = reader.Decimal(i++),
                        NotedDate = reader.DateTime(i++),
                    };

                    int lineIndex = reader.Int32(i++) ?? throw new InvalidOperationException("Returned entry [Index] was null");
                    if (lineIndex >= lines.Count)
                    {
                        throw new InvalidOperationException($"Entry's [LineIndex] = {lineIndex} is not valid, only {lines.Count} were loaded");
                    }

                    var line = lines[lineIndex];
                    line.Entries.Add(entry);
                }

                // Account
                list_Account = new List<Account>();
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    list_Account.Add(new Account
                    {
                        Id = reader.GetInt32(i++),
                        Name = reader.String(i++),
                        Name2 = reader.String(i++),
                        Name3 = reader.String(i++),
                        Code = reader.String(i++),

                        EntityMetadata = new EntityMetadata
                    {
                        { nameof(Account.Name), FieldMetadata.Loaded },
                        { nameof(Account.Name2), FieldMetadata.Loaded },
                        { nameof(Account.Name3), FieldMetadata.Loaded },
                        { nameof(Account.Code), FieldMetadata.Loaded },
                    }
                    });
                }

                // Currency
                list_Currency = new List<Currency>();
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    list_Currency.Add(new Currency
                    {
                        Id = reader.GetString(i++),
                        Name = reader.String(i++),
                        Name2 = reader.String(i++),
                        Name3 = reader.String(i++),
                        E = reader.Int16(i++),

                        EntityMetadata = new EntityMetadata
                    {
                        { nameof(Currency.Name), FieldMetadata.Loaded },
                        { nameof(Currency.Name2), FieldMetadata.Loaded },
                        { nameof(Currency.Name3), FieldMetadata.Loaded },
                        { nameof(Currency.E), FieldMetadata.Loaded },
                    }
                    });
                }

                // Resource
                list_Resource = new List<Resource>();
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    list_Resource.Add(new Resource
                    {
                        Id = reader.GetInt32(i++),
                        Name = reader.String(i++),
                        Name2 = reader.String(i++),
                        Name3 = reader.String(i++),
                        DefinitionId = reader.Int32(i++),

                        EntityMetadata = new EntityMetadata
                    {
                        { nameof(Resource.Name), FieldMetadata.Loaded },
                        { nameof(Resource.Name2), FieldMetadata.Loaded },
                        { nameof(Resource.Name3), FieldMetadata.Loaded },
                        { nameof(Resource.DefinitionId), FieldMetadata.Loaded },
                    }
                    });
                }

                // Relation
                list_Relation = new List<Relation>();
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    list_Relation.Add(new Relation
                    {
                        Id = reader.GetInt32(i++),
                        Name = reader.String(i++),
                        Name2 = reader.String(i++),
                        Name3 = reader.String(i++),
                        DefinitionId = reader.Int32(i++),

                        EntityMetadata = new EntityMetadata
                    {
                        { nameof(Relation.Name), FieldMetadata.Loaded },
                        { nameof(Relation.Name2), FieldMetadata.Loaded },
                        { nameof(Relation.Name3), FieldMetadata.Loaded },
                        { nameof(Relation.DefinitionId), FieldMetadata.Loaded },
                    }
                    });
                }

                // EntryType
                list_EntryType = new List<EntryType>();
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    list_EntryType.Add(new EntryType
                    {
                        Id = reader.GetInt32(i++),
                        Name = reader.String(i++),
                        Name2 = reader.String(i++),
                        Name3 = reader.String(i++),

                        EntityMetadata = new EntityMetadata
                    {
                        { nameof(EntryType.Name), FieldMetadata.Loaded },
                        { nameof(EntryType.Name2), FieldMetadata.Loaded },
                        { nameof(EntryType.Name3), FieldMetadata.Loaded },
                    }
                    });
                }


                // Center
                list_Center = new List<Center>();
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    list_Center.Add(new Center
                    {
                        Id = reader.GetInt32(i++),
                        Name = reader.String(i++),
                        Name2 = reader.String(i++),
                        Name3 = reader.String(i++),

                        EntityMetadata = new EntityMetadata
                    {
                        { nameof(Center.Name), FieldMetadata.Loaded },
                        { nameof(Center.Name2), FieldMetadata.Loaded },
                        { nameof(Center.Name3), FieldMetadata.Loaded },
                    }
                    });
                }

                // Unit
                list_Unit = new List<Unit>();
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    list_Unit.Add(new Unit
                    {
                        Id = reader.GetInt32(i++),
                        Name = reader.String(i++),
                        Name2 = reader.String(i++),
                        Name3 = reader.String(i++),

                        EntityMetadata = new EntityMetadata
                    {
                        { nameof(Unit.Name), FieldMetadata.Loaded },
                        { nameof(Unit.Name2), FieldMetadata.Loaded },
                        { nameof(Unit.Name3), FieldMetadata.Loaded },
                    }
                    });
                }
            },
            DatabaseName(connString), nameof(Lines__Generate), cancellation);

            return (lines, list_Account, list_Resource, list_Relation, list_EntryType, list_Center, list_Currency, list_Unit);
        }

        public async Task<SignResult> Lines__Sign(IEnumerable<int> ids, short toState, int? reasonId, string reasonDetails, int? onBehalfOfUserId, string ruleType, int? roleId, DateTimeOffset? signedAt, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SignResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Lines__Sign)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ToState", toState);
                cmd.Parameters.Add("@ReasonId", reasonId);
                cmd.Parameters.Add("@ReasonDetails", reasonDetails);
                cmd.Parameters.Add("@OnBehalfOfUserId", onBehalfOfUserId);
                cmd.Parameters.Add("@RuleType", ruleType);
                cmd.Parameters.Add("@RoleId", roleId);
                cmd.Parameters.Add("@SignedAt", signedAt);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSignResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(Lines__Sign));

            return result;
        }

        public async Task<SignResult> LineSignatures__Delete(IEnumerable<int> ids, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SignResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(LineSignatures__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSignResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(LineSignatures__Delete));

            return result;
        }

        public async Task<AssignResult> Documents__Assign(IEnumerable<int> ids, int assigneeId, string comment, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            AssignResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Documents__Assign)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@AssigneeId", assigneeId);
                cmd.Parameters.Add("@Comment", comment);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                // (1) Load Errors
                var errors = await reader.LoadErrors();

                List<InboxStatus> inboxStatuses = default;
                User assigneeInfo = default;
                int serialNumber = default;
                if (!errors.Any() && !validateOnly)
                {
                    // (2) Inbox Statuses
                    await reader.NextResultAsync();
                    inboxStatuses = await reader.LoadInboxStatuses();

                    // (3) Assignee Info + Doc Serial
                    await reader.NextResultAsync();
                    if (await reader.ReadAsync())
                    {
                        int i = 0;

                        assigneeInfo = new User
                        {
                            Name = reader.String(i++),
                            Name2 = reader.String(i++),
                            Name3 = reader.String(i++),
                            PreferredLanguage = reader.String(i++),
                            ContactEmail = reader.String(i++),
                            ContactMobile = reader.String(i++),
                            NormalizedContactMobile = reader.String(i++),
                            PushEndpoint = reader.String(i++),
                            PushP256dh = reader.String(i++),
                            PushAuth = reader.String(i++),
                            PreferredChannel = reader.String(i++),
                            EmailNewInboxItem = reader.Boolean(i++),
                            SmsNewInboxItem = reader.Boolean(i++),
                            PushNewInboxItem = reader.Boolean(i++),

                            EntityMetadata = new EntityMetadata {
                                { nameof(User.Name), FieldMetadata.Loaded },
                                { nameof(User.Name2), FieldMetadata.Loaded },
                                { nameof(User.Name3), FieldMetadata.Loaded },
                                { nameof(User.PreferredLanguage), FieldMetadata.Loaded },
                                { nameof(User.ContactEmail), FieldMetadata.Loaded },
                                { nameof(User.ContactMobile), FieldMetadata.Loaded },
                                { nameof(User.NormalizedContactMobile), FieldMetadata.Loaded },
                                { nameof(User.PushEndpoint), FieldMetadata.Loaded },
                                { nameof(User.PushP256dh), FieldMetadata.Loaded },
                                { nameof(User.PushAuth), FieldMetadata.Loaded },
                                { nameof(User.PreferredChannel), FieldMetadata.Loaded },
                                { nameof(User.EmailNewInboxItem), FieldMetadata.Loaded },
                                { nameof(User.SmsNewInboxItem), FieldMetadata.Loaded },
                                { nameof(User.PushNewInboxItem), FieldMetadata.Loaded },
                            }
                        };

                        serialNumber = reader.Int32(i++) ?? 0;
                    }
                    else
                    {
                        // Just in case
                        throw new InvalidOperationException($"[Bug] Stored Procedure {nameof(Documents__Assign)} did not return assignee info.");
                    }
                }

                result = new AssignResult(errors, inboxStatuses, assigneeInfo, serialNumber);
            },
            DatabaseName(connString), nameof(Documents__Assign));

            return result;
        }

        public async Task<(InboxStatusResult result, List<string> deletedFileIds)> Documents__Delete(int definitionId, IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            InboxStatusResult result = null;
            List<string> deletedFileIds = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Documents__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();

                    // (1) The errors and inbox statuses
                    result = await reader.LoadInboxStatusResult(validateOnly);
                    if (!result.IsError && !validateOnly)
                    {
                        // (2) Load deleted file Ids
                        deletedFileIds = new List<string>();
                        await reader.NextResultAsync();
                        while (await reader.ReadAsync())
                        {
                            deletedFileIds.Add(reader.String(0));
                        }
                    }
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(Documents__Delete));

            return (result, deletedFileIds);
        }

        public async Task<InboxStatusResult> Documents__Close(int definitionId, List<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            InboxStatusResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Documents__Close)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadInboxStatusResult(validateOnly);
            },
            DatabaseName(connString), nameof(Documents__Close));

            return result;
        }

        public async Task<InboxStatusResult> Documents__Open(int definitionId, List<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            InboxStatusResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Documents__Open)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadInboxStatusResult(validateOnly);
            },
            DatabaseName(connString), nameof(Documents__Open));

            return result;
        }

        public async Task<InboxStatusResult> Documents__Cancel(int definitionId, List<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            InboxStatusResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Documents__Cancel)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadInboxStatusResult(validateOnly);
            },
            DatabaseName(connString), nameof(Documents__Cancel));

            return result;
        }

        public async Task<InboxStatusResult> Documents__Uncancel(int definitionId, List<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            InboxStatusResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Documents__Uncancel)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadInboxStatusResult(validateOnly);
            },
            DatabaseName(connString), nameof(Documents__Uncancel));

            return result;
        }

        public async Task<List<InboxStatus>> Documents__Preview(int documentId, DateTimeOffset createdAt, DateTimeOffset openedAt, int userId, CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);
            List<InboxStatus> result = default;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Documents__Preview)}]";

                // Parameters
                // Parameters
                cmd.Parameters.Add("@DocumentId", documentId);
                cmd.Parameters.Add("@CreatedAt", createdAt);
                cmd.Parameters.Add("@OpenedAt", openedAt);
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync(cancellation);
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                result = await reader.LoadInboxStatuses(cancellation);
            },
            DatabaseName(connString), nameof(Documents__Preview), cancellation);

            return result;
        }

        #endregion

        #region EntryTypes

        public async Task<SaveResult> EntryTypes__Save(List<EntryTypeForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(EntryTypes__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(EntryType)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(EntryTypes__Save));

            return result;
        }

        public async Task<DeleteResult> EntryTypes__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(EntryTypes__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(EntryTypes__Delete));

            return result;
        }

        public async Task<DeleteResult> EntryTypes__DeleteWithDescendants(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(EntryTypes__DeleteWithDescendants)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(EntryTypes__DeleteWithDescendants));

            return result;
        }

        public async Task<OperationResult> EntryTypes__Activate(List<int> ids, bool isActive, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(EntryTypes__Activate)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@IsActive", isActive);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(EntryTypes__Activate));

            return result;
        }

        #endregion

        #region ExchangeRates

        public async Task<SaveResult> ExchangeRates__Save(List<ExchangeRateForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(ExchangeRates__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(ExchangeRate)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(ExchangeRates__Save));

            return result;
        }

        public async Task<DeleteResult> ExchangeRates__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(ExchangeRates__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(ExchangeRates__Delete));

            return result;
        }

        public async Task<decimal?> ConvertToFunctional(DateTime date, string currencyId, decimal amount, CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);
            decimal? result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[wiz].[fn_{nameof(ConvertToFunctional)}]";

                // Parameters
                cmd.Parameters.Add("@Date", date);
                cmd.Parameters.Add("@CurrencyId", currencyId);
                cmd.Parameters.Add("@Amount", amount);

                // Output Parameter
                var resultParam = new SqlParameter("@Result", SqlDbType.Decimal)
                {
                    Direction = ParameterDirection.ReturnValue
                };

                cmd.Parameters.Add(resultParam);

                // Execute
                await conn.OpenAsync(cancellation);
                await cmd.ExecuteNonQueryAsync(cancellation);
                var resultObject = cmd.Parameters["@Result"].Value;
                if (resultObject != DBNull.Value)
                {
                    result = (decimal)resultObject;
                }
            },
            DatabaseName(connString), nameof(ExchangeRates__Delete), cancellation);

            return result;
        }

        #endregion

        #region FinancialSettings

        public async Task<OperationResult> FinancialSettings__Save(FinancialSettingsForSave settingsForSave, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(FinancialSettings__Save)}]";

                // Parameters
                var mappedProps = TypeDescriptor.Get<FinancialSettingsForSave>().SimpleProperties;
                foreach (var prop in mappedProps)
                {
                    var propName = prop.Name;
                    var key = $"@{propName}";
                    var value = prop.GetValue(settingsForSave);

                    cmd.Parameters.Add(key, value);
                }
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(FinancialSettings__Save));

            return result;
        }

        #endregion

        #region GeneralSettings

        public async Task<OperationResult> GeneralSettings__Save(GeneralSettingsForSave settingsForSave, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(GeneralSettings__Save)}]";

                // Parameters
                var mappedProps = TypeDescriptor.Get<GeneralSettingsForSave>().SimpleProperties;
                foreach (var prop in mappedProps)
                {
                    var propName = prop.Name;
                    var key = $"@{propName}";
                    var value = prop.GetValue(settingsForSave);

                    cmd.Parameters.Add(key, value);
                }
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(GeneralSettings__Save));

            return result;
        }


        #endregion

        #region Inbox

        public async Task<List<InboxStatus>> Inbox__Check(DateTimeOffset now, int userId)
        {
            var connString = await GetConnectionString();
            List<InboxStatus> result = default;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Inbox__Check)}]";

                // Parameters
                cmd.Parameters.Add("@Now", now);
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadInboxStatuses();
            },
            DatabaseName(connString), nameof(Inbox__Check));

            return result;
        }

        public async Task<List<InboxStatus>> InboxCounts__Load(IEnumerable<int> userIds, CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);
            List<InboxStatus> result = default;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(InboxCounts__Load)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(userIds.Select(id => new IdListItem { Id = id }));
                var idsTvp = new SqlParameter("@UserIds", idsTable)
                {
                    TypeName = $"[dbo].[IdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                result = await reader.LoadInboxStatuses(cancellation);
            },
            DatabaseName(connString), nameof(InboxCounts__Load), cancellation);

            return result;
        }

        #endregion

        #region LineDefinitions

        public async Task<SaveResult> LineDefinitions__Save(List<LineDefinitionForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(LineDefinitions__Save)}]";

                // Parameters

                // Tables
                var lineDefinitionsTable = new DataTable();
                lineDefinitionsTable.Columns.Add(new DataColumn("Index", typeof(int)));
                var lineDefinitionProps = RepositoryUtilities.AddColumnsFromProperties<LineDefinitionForSave>(lineDefinitionsTable);

                var lineDefinitionEntriesTable = new DataTable();
                lineDefinitionEntriesTable.Columns.Add(new DataColumn("Index", typeof(int)));
                lineDefinitionEntriesTable.Columns.Add(new DataColumn("HeaderIndex", typeof(int)));
                var lineDefinitionEntryProps = RepositoryUtilities.AddColumnsFromProperties<LineDefinitionEntryForSave>(lineDefinitionEntriesTable);

                var lineDefinitionEntryRelationDefinitionsTable = new DataTable();
                lineDefinitionEntryRelationDefinitionsTable.Columns.Add(new DataColumn("Index", typeof(int)));
                lineDefinitionEntryRelationDefinitionsTable.Columns.Add(new DataColumn("LineDefinitionEntryIndex", typeof(int)));
                lineDefinitionEntryRelationDefinitionsTable.Columns.Add(new DataColumn("LineDefinitionIndex", typeof(int)));
                var lineDefinitionEntryRelationDefinitionProps = RepositoryUtilities.AddColumnsFromProperties<LineDefinitionEntryRelationDefinitionForSave>(lineDefinitionEntryRelationDefinitionsTable);

                var lineDefinitionEntryResourceDefinitionsTable = new DataTable();
                lineDefinitionEntryResourceDefinitionsTable.Columns.Add(new DataColumn("Index", typeof(int)));
                lineDefinitionEntryResourceDefinitionsTable.Columns.Add(new DataColumn("LineDefinitionEntryIndex", typeof(int)));
                lineDefinitionEntryResourceDefinitionsTable.Columns.Add(new DataColumn("LineDefinitionIndex", typeof(int)));
                var lineDefinitionEntryResourceDefinitionProps = RepositoryUtilities.AddColumnsFromProperties<LineDefinitionEntryResourceDefinitionForSave>(lineDefinitionEntryResourceDefinitionsTable);

                var lineDefinitionEntryNotedRelationDefinitionsTable = new DataTable();
                lineDefinitionEntryNotedRelationDefinitionsTable.Columns.Add(new DataColumn("Index", typeof(int)));
                lineDefinitionEntryNotedRelationDefinitionsTable.Columns.Add(new DataColumn("LineDefinitionEntryIndex", typeof(int)));
                lineDefinitionEntryNotedRelationDefinitionsTable.Columns.Add(new DataColumn("LineDefinitionIndex", typeof(int)));
                var lineDefinitionEntryNotedRelationDefinitionProps = RepositoryUtilities.AddColumnsFromProperties<LineDefinitionEntryNotedRelationDefinitionForSave>(lineDefinitionEntryNotedRelationDefinitionsTable);

                var lineDefinitionColumnsTable = new DataTable();
                lineDefinitionColumnsTable.Columns.Add(new DataColumn("Index", typeof(int)));
                lineDefinitionColumnsTable.Columns.Add(new DataColumn("HeaderIndex", typeof(int)));
                var lineDefinitionColumnProps = RepositoryUtilities.AddColumnsFromProperties<LineDefinitionColumnForSave>(lineDefinitionColumnsTable);

                var lineDefinitionGenerateParametersTable = new DataTable();
                lineDefinitionGenerateParametersTable.Columns.Add(new DataColumn("Index", typeof(int)));
                lineDefinitionGenerateParametersTable.Columns.Add(new DataColumn("HeaderIndex", typeof(int)));
                var lineDefinitionGenerateParameterProps = RepositoryUtilities.AddColumnsFromProperties<LineDefinitionGenerateParameterForSave>(lineDefinitionGenerateParametersTable);

                var lineDefinitionStateReasonsTable = new DataTable();
                lineDefinitionStateReasonsTable.Columns.Add(new DataColumn("Index", typeof(int)));
                lineDefinitionStateReasonsTable.Columns.Add(new DataColumn("HeaderIndex", typeof(int)));
                var lineDefinitionStateReasonProps = RepositoryUtilities.AddColumnsFromProperties<LineDefinitionStateReasonForSave>(lineDefinitionStateReasonsTable);

                var workflowsTable = new DataTable();
                workflowsTable.Columns.Add(new DataColumn("Index", typeof(int)));
                workflowsTable.Columns.Add(new DataColumn("LineDefinitionIndex", typeof(int)));
                var workflowProps = RepositoryUtilities.AddColumnsFromProperties<WorkflowForSave>(workflowsTable);

                var workflowSignaturesTable = new DataTable();
                workflowSignaturesTable.Columns.Add(new DataColumn("Index", typeof(int)));
                workflowSignaturesTable.Columns.Add(new DataColumn("WorkflowIndex", typeof(int)));
                workflowSignaturesTable.Columns.Add(new DataColumn("LineDefinitionIndex", typeof(int)));
                var workflowSignatureProps = RepositoryUtilities.AddColumnsFromProperties<WorkflowSignatureForSave>(workflowSignaturesTable);

                // LineDefinitions
                int lineDefinitionIndex = 0;
                foreach (var lineDefinition in entities)
                {
                    DataRow lineDefinitionsRow = lineDefinitionsTable.NewRow();

                    lineDefinitionsRow["Index"] = lineDefinitionIndex;
                    foreach (var prop in lineDefinitionProps)
                    {
                        var value = prop.GetValue(lineDefinition);
                        lineDefinitionsRow[prop.Name] = value ?? DBNull.Value;
                    }

                    // Entries
                    if (lineDefinition.Entries != null)
                    {
                        int lineDefinitionEntryIndex = 0;
                        lineDefinition.Entries.ForEach(lineDefinitionEntry =>
                        {
                            DataRow lineDefinitionEntriesRow = lineDefinitionEntriesTable.NewRow();

                            lineDefinitionEntriesRow["Index"] = lineDefinitionEntryIndex;
                            lineDefinitionEntriesRow["HeaderIndex"] = lineDefinitionIndex;
                            foreach (var prop in lineDefinitionEntryProps)
                            {
                                var value = prop.GetValue(lineDefinitionEntry);
                                lineDefinitionEntriesRow[prop.Name] = value ?? DBNull.Value;
                            }

                            // Entries.RelationDefinitions
                            if (lineDefinitionEntry.RelationDefinitions != null)
                            {
                                int lineDefinitionEntryRelationDefinitionIndex = 0;
                                lineDefinitionEntry.RelationDefinitions.ForEach(lineDefinitionEntryRelationDefinition =>
                                {
                                    DataRow lineDefinitionEntryRelationDefinitionsRow = lineDefinitionEntryRelationDefinitionsTable.NewRow();

                                    lineDefinitionEntryRelationDefinitionsRow["Index"] = lineDefinitionEntryRelationDefinitionIndex;
                                    lineDefinitionEntryRelationDefinitionsRow["LineDefinitionEntryIndex"] = lineDefinitionEntryIndex;
                                    lineDefinitionEntryRelationDefinitionsRow["LineDefinitionIndex"] = lineDefinitionIndex;

                                    foreach (var prop in lineDefinitionEntryRelationDefinitionProps)
                                    {
                                        var value = prop.GetValue(lineDefinitionEntryRelationDefinition);
                                        lineDefinitionEntryRelationDefinitionsRow[prop.Name] = value ?? DBNull.Value;
                                    }

                                    lineDefinitionEntryRelationDefinitionsTable.Rows.Add(lineDefinitionEntryRelationDefinitionsRow);
                                    lineDefinitionEntryRelationDefinitionIndex++;
                                });
                            }

                            // Entries.ResourceDefinitions
                            if (lineDefinitionEntry.ResourceDefinitions != null)
                            {
                                int lineDefinitionEntryResourceDefinitionIndex = 0;
                                lineDefinitionEntry.ResourceDefinitions.ForEach(lineDefinitionEntryResourceDefinition =>
                                {
                                    DataRow lineDefinitionEntryResourceDefinitionsRow = lineDefinitionEntryResourceDefinitionsTable.NewRow();

                                    lineDefinitionEntryResourceDefinitionsRow["Index"] = lineDefinitionEntryResourceDefinitionIndex;
                                    lineDefinitionEntryResourceDefinitionsRow["LineDefinitionEntryIndex"] = lineDefinitionEntryIndex;
                                    lineDefinitionEntryResourceDefinitionsRow["LineDefinitionIndex"] = lineDefinitionIndex;

                                    foreach (var prop in lineDefinitionEntryResourceDefinitionProps)
                                    {
                                        var value = prop.GetValue(lineDefinitionEntryResourceDefinition);
                                        lineDefinitionEntryResourceDefinitionsRow[prop.Name] = value ?? DBNull.Value;
                                    }

                                    lineDefinitionEntryResourceDefinitionsTable.Rows.Add(lineDefinitionEntryResourceDefinitionsRow);
                                    lineDefinitionEntryResourceDefinitionIndex++;
                                });
                            }

                            // Entries.NotedRelationDefinitions
                            if (lineDefinitionEntry.NotedRelationDefinitions != null)
                            {
                                int lineDefinitionEntryNotedRelationDefinitionIndex = 0;
                                lineDefinitionEntry.NotedRelationDefinitions.ForEach(lineDefinitionEntryNotedRelationDefinition =>
                                {
                                    DataRow lineDefinitionEntryNotedRelationDefinitionsRow = lineDefinitionEntryNotedRelationDefinitionsTable.NewRow();

                                    lineDefinitionEntryNotedRelationDefinitionsRow["Index"] = lineDefinitionEntryNotedRelationDefinitionIndex;
                                    lineDefinitionEntryNotedRelationDefinitionsRow["LineDefinitionEntryIndex"] = lineDefinitionEntryIndex;
                                    lineDefinitionEntryNotedRelationDefinitionsRow["LineDefinitionIndex"] = lineDefinitionIndex;

                                    foreach (var prop in lineDefinitionEntryNotedRelationDefinitionProps)
                                    {
                                        var value = prop.GetValue(lineDefinitionEntryNotedRelationDefinition);
                                        lineDefinitionEntryNotedRelationDefinitionsRow[prop.Name] = value ?? DBNull.Value;
                                    }

                                    lineDefinitionEntryNotedRelationDefinitionsTable.Rows.Add(lineDefinitionEntryNotedRelationDefinitionsRow);
                                    lineDefinitionEntryNotedRelationDefinitionIndex++;
                                });
                            }

                            lineDefinitionEntriesTable.Rows.Add(lineDefinitionEntriesRow);
                            lineDefinitionEntryIndex++;
                        });
                    }

                    // Columns
                    if (lineDefinition.Columns != null)
                    {
                        int lineDefinitionColumnIndex = 0;
                        lineDefinition.Columns.ForEach(lineDefinitionColumn =>
                        {
                            DataRow lineDefinitionColumnsRow = lineDefinitionColumnsTable.NewRow();

                            lineDefinitionColumnsRow["Index"] = lineDefinitionColumnIndex;
                            lineDefinitionColumnsRow["HeaderIndex"] = lineDefinitionIndex;
                            foreach (var prop in lineDefinitionColumnProps)
                            {
                                var value = prop.GetValue(lineDefinitionColumn);
                                lineDefinitionColumnsRow[prop.Name] = value ?? DBNull.Value;
                            }

                            lineDefinitionColumnsTable.Rows.Add(lineDefinitionColumnsRow);
                            lineDefinitionColumnIndex++;
                        });
                    }

                    // GenerateParameters
                    if (lineDefinition.GenerateParameters != null)
                    {
                        int lineDefinitionGenerateParameterIndex = 0;
                        lineDefinition.GenerateParameters.ForEach(lineDefinitionGenerateParameter =>
                        {
                            DataRow lineDefinitionGenerateParametersRow = lineDefinitionGenerateParametersTable.NewRow();

                            lineDefinitionGenerateParametersRow["Index"] = lineDefinitionGenerateParameterIndex;
                            lineDefinitionGenerateParametersRow["HeaderIndex"] = lineDefinitionIndex;
                            foreach (var prop in lineDefinitionGenerateParameterProps)
                            {
                                var value = prop.GetValue(lineDefinitionGenerateParameter);
                                lineDefinitionGenerateParametersRow[prop.Name] = value ?? DBNull.Value;
                            }

                            lineDefinitionGenerateParametersTable.Rows.Add(lineDefinitionGenerateParametersRow);
                            lineDefinitionGenerateParameterIndex++;
                        });
                    }

                    // StateReasons
                    if (lineDefinition.StateReasons != null)
                    {
                        int lineDefinitionStateReasonIndex = 0;
                        lineDefinition.StateReasons.ForEach(lineDefinitionStateReason =>
                        {
                            DataRow lineDefinitionStateReasonsRow = lineDefinitionStateReasonsTable.NewRow();

                            lineDefinitionStateReasonsRow["Index"] = lineDefinitionStateReasonIndex;
                            lineDefinitionStateReasonsRow["HeaderIndex"] = lineDefinitionIndex;
                            foreach (var prop in lineDefinitionStateReasonProps)
                            {
                                var value = prop.GetValue(lineDefinitionStateReason);
                                lineDefinitionStateReasonsRow[prop.Name] = value ?? DBNull.Value;
                            }

                            lineDefinitionStateReasonsTable.Rows.Add(lineDefinitionStateReasonsRow);
                            lineDefinitionStateReasonIndex++;
                        });
                    }

                    // Workflows
                    if (lineDefinition.Workflows != null)
                    {
                        int workflowIndex = 0;
                        lineDefinition.Workflows.ForEach(workflow =>
                        {
                            DataRow workflowsRow = workflowsTable.NewRow();

                            workflowsRow["Index"] = workflowIndex;
                            workflowsRow["LineDefinitionIndex"] = lineDefinitionIndex;
                            foreach (var prop in workflowProps)
                            {
                                var value = prop.GetValue(workflow);
                                workflowsRow[prop.Name] = value ?? DBNull.Value;
                            }

                            // Workflows.Signatures
                            if (workflow.Signatures != null)
                            {
                                int workflowSignatureIndex = 0;
                                workflow.Signatures.ForEach(workflowSignature =>
                                {
                                    DataRow workflowSignaturesRow = workflowSignaturesTable.NewRow();

                                    workflowSignaturesRow["Index"] = workflowSignatureIndex;
                                    workflowSignaturesRow["WorkflowIndex"] = workflowIndex;
                                    workflowSignaturesRow["LineDefinitionIndex"] = lineDefinitionIndex;
                                    foreach (var prop in workflowSignatureProps)
                                    {
                                        var value = prop.GetValue(workflowSignature);
                                        workflowSignaturesRow[prop.Name] = value ?? DBNull.Value;
                                    }


                                    workflowSignaturesTable.Rows.Add(workflowSignaturesRow);
                                    workflowSignatureIndex++;
                                });
                            }

                            workflowsTable.Rows.Add(workflowsRow);
                            workflowIndex++;
                        });
                    }

                    lineDefinitionsTable.Rows.Add(lineDefinitionsRow);
                    lineDefinitionIndex++;
                }

                // TVPs
                var lineDefinitionsTvp = new SqlParameter("@Entities", lineDefinitionsTable)
                {
                    TypeName = $"[dbo].[{nameof(LineDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };
                var lineDefinitionEntriesTvp = new SqlParameter("@LineDefinitionEntries", lineDefinitionEntriesTable)
                {
                    TypeName = $"[dbo].[{nameof(LineDefinitionEntry)}List]",
                    SqlDbType = SqlDbType.Structured
                };
                var lineDefinitionEntryRelationDefinitionsTvp = new SqlParameter("@LineDefinitionEntryRelationDefinitions", lineDefinitionEntryRelationDefinitionsTable)
                {
                    TypeName = $"[dbo].[{nameof(LineDefinitionEntryRelationDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };
                var lineDefinitionEntryResourceDefinitionsTvp = new SqlParameter("@LineDefinitionEntryResourceDefinitions", lineDefinitionEntryResourceDefinitionsTable)
                {
                    TypeName = $"[dbo].[{nameof(LineDefinitionEntryResourceDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };
                var lineDefinitionEntryNotedRelationDefinitionsTvp = new SqlParameter("@LineDefinitionEntryNotedRelationDefinitions", lineDefinitionEntryNotedRelationDefinitionsTable)
                {
                    TypeName = $"[dbo].[{nameof(LineDefinitionEntryNotedRelationDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };
                var lineDefinitionColumnsTvp = new SqlParameter("@LineDefinitionColumns", lineDefinitionColumnsTable)
                {
                    TypeName = $"[dbo].[{nameof(LineDefinitionColumn)}List]",
                    SqlDbType = SqlDbType.Structured
                };
                var lineDefinitionGenerateParametersTvp = new SqlParameter("@LineDefinitionGenerateParameters", lineDefinitionGenerateParametersTable)
                {
                    TypeName = $"[dbo].[{nameof(LineDefinitionGenerateParameter)}List]",
                    SqlDbType = SqlDbType.Structured
                };
                var lineDefinitionStateReasonsTvp = new SqlParameter("@LineDefinitionStateReasons", lineDefinitionStateReasonsTable)
                {
                    TypeName = $"[dbo].[{nameof(LineDefinitionStateReason)}List]",
                    SqlDbType = SqlDbType.Structured
                };
                var workflowsTvp = new SqlParameter("@Workflows", workflowsTable)
                {
                    TypeName = $"[dbo].[{nameof(Workflow)}List]",
                    SqlDbType = SqlDbType.Structured
                };
                var workflowSignaturesTvp = new SqlParameter("@WorkflowSignatures", workflowSignaturesTable)
                {
                    TypeName = $"[dbo].[{nameof(WorkflowSignature)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(lineDefinitionsTvp);
                cmd.Parameters.Add(lineDefinitionEntriesTvp);
                cmd.Parameters.Add(lineDefinitionEntryRelationDefinitionsTvp);
                cmd.Parameters.Add(lineDefinitionEntryResourceDefinitionsTvp);
                cmd.Parameters.Add(lineDefinitionEntryNotedRelationDefinitionsTvp);
                cmd.Parameters.Add(lineDefinitionColumnsTvp);
                cmd.Parameters.Add(lineDefinitionGenerateParametersTvp);
                cmd.Parameters.Add(lineDefinitionStateReasonsTvp);
                cmd.Parameters.Add(workflowsTvp);
                cmd.Parameters.Add(workflowSignaturesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(LineDefinitions__Save));

            return result;
        }

        public async Task<DeleteResult> LineDefinitions__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(LineDefinitions__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(LineDefinitions__Delete));

            return result;
        }

        #endregion

        #region LookupDefinitions

        public async Task<SaveResult> LookupDefinitions__Save(List<LookupDefinitionForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(LookupDefinitions__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(LookupDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable reportDefinitionsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.ReportDefinitions);
                var reportDefinitionsTvp = new SqlParameter("@ReportDefinitions", reportDefinitionsTable)
                {
                    TypeName = $"[dbo].[{nameof(LookupDefinitionReportDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add(reportDefinitionsTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(LookupDefinitions__Save));

            return result;
        }

        public async Task<DeleteResult> LookupDefinitions__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(LookupDefinitions__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(LookupDefinitions__Delete));

            return result;
        }

        public async Task<OperationResult> LookupDefinitions__UpdateState(List<int> ids, string state, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(LookupDefinitions__UpdateState)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@State", state);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(LookupDefinitions__UpdateState));

            return result;
        }

        #endregion

        #region Lookups

        public async Task<SaveResult> Lookups__Save(int definitionId, List<LookupForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Lookups__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(Lookup)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(Lookups__Save));

            return result;
        }

        public async Task<DeleteResult> Lookups__Delete(int definitionId, IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Lookups__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(Lookups__Delete));

            return result;
        }

        public async Task<OperationResult> Lookups__Activate(int definitionId, List<int> ids, bool isActive, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Lookups__Activate)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@IsActive", isActive);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(Lookups__Activate));

            return result;
        }

        #endregion

        #region MarkupTemplates

        public async Task<SaveResult> MarkupTemplates__Save(List<MarkupTemplateForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(MarkupTemplates__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(MarkupTemplate)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(MarkupTemplates__Save));

            return result;
        }

        public async Task<DeleteResult> MarkupTemplates__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(MarkupTemplates__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(MarkupTemplates__Delete));

            return result;
        }

        #endregion

        #region Reconciliation

        public async Task<UnreconciledResult> Reconciliation__Load_Unreconciled(int accountId, int relationId, DateTime? asOfDate, int top, int skip, int topExternal, int skipExternal, CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);
            UnreconciledResult result = default;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Reconciliation__Load_Unreconciled)}]";

                // Parameters
                AddUnreconciledParamsInner(cmd, accountId, relationId, asOfDate, top, skip, topExternal, skipExternal);

                // Execute
                await conn.OpenAsync();
                result = await LoadUnreconciledInner(cmd);
            },
            DatabaseName(connString), nameof(Reconciliation__Load_Unreconciled), cancellation);

            // Return
            return result;
        }

        public async Task<ReconciledResult> Reconciliation__Load_Reconciled(int accountId, int relationId, DateTime? fromDate, DateTime? toDate, decimal? fromAmount, decimal? toAmount, string externalReferenceContains, int top, int skip, CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);
            ReconciledResult result = default;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Reconciliation__Load_Reconciled)}]";

                // Parameters
                AddReconciledParamsInner(cmd, accountId, relationId, fromDate, toDate, fromAmount, toAmount, externalReferenceContains, top, skip);

                // Execute
                await conn.OpenAsync();
                result = await LoadReconciledInner(cmd, cancellation);
            },
            DatabaseName(connString), nameof(Reconciliation__Load_Reconciled), cancellation);

            return result;
        }

        public async Task<IEnumerable<ValidationError>> Reconciliations_Validate__Save(int accountId, int relationId, List<ExternalEntryForSave> externalEntriesForSave, List<ReconciliationForSave> reconciliations, int top, int userId)
        {
            var connString = await GetConnectionString();
            IEnumerable<ValidationError> result = default;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[bll].[{nameof(Reconciliations_Validate__Save)}]";

                // Parameters
                cmd.Parameters.Add("@AccountId", accountId);
                cmd.Parameters.Add("@RelationId", relationId);
                cmd.Parameters.Add("@Top", top);
                AddReconciliationsAndExternalEntries(cmd, userId, externalEntriesForSave, reconciliations);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadErrors();
            },
            DatabaseName(connString), nameof(Reconciliations_Validate__Save));

            return result;
        }

        public async Task<UnreconciledResult> Reconciliations__SaveAndLoad_Unreconciled(int accountId, int relationId, List<ExternalEntryForSave> externalEntriesForSave, List<ReconciliationForSave> reconciliations, List<int> deletedExternalEntryIds, List<int> deletedReconciliationIds, DateTime? asOfDate, int top, int skip, int topExternal, int skipExternal, int userId)
        {
            var connString = await GetConnectionString();
            UnreconciledResult result = default;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Reconciliations__SaveAndLoad_Unreconciled)}]";

                // Parameters
                AddUnreconciledParamsInner(cmd, accountId, relationId, asOfDate, top, skip, topExternal, skipExternal);
                AddReconciliationsAndExternalEntries(cmd, userId, externalEntriesForSave, reconciliations, deletedExternalEntryIds, deletedReconciliationIds);

                // Execute
                await conn.OpenAsync();
                result = await LoadUnreconciledInner(cmd);
            },
            DatabaseName(connString), nameof(Reconciliations__SaveAndLoad_Unreconciled));

            return result;
        }

        public async Task<ReconciledResult> Reconciliations__SaveAndLoad_Reconciled(int accountId, int relationId, List<ExternalEntryForSave> externalEntriesForSave, List<ReconciliationForSave> reconciliations, List<int> deletedExternalEntryIds, List<int> deletedReconciliationIds, DateTime? fromDate, DateTime? toDate, decimal? fromAmount, decimal? toAmount, string externalReferenceContains, int top, int skip, int userId)
        {
            var connString = await GetConnectionString();
            ReconciledResult result = default;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Reconciliations__SaveAndLoad_Reconciled)}]";

                // Parameters
                AddReconciledParamsInner(cmd, accountId, relationId, fromDate, toDate, fromAmount, toAmount, externalReferenceContains, top, skip);
                AddReconciliationsAndExternalEntries(cmd, userId, externalEntriesForSave, reconciliations, deletedExternalEntryIds, deletedReconciliationIds);

                // Execute
                await conn.OpenAsync();
                result = await LoadReconciledInner(cmd);
            },
            DatabaseName(connString), nameof(Reconciliations__SaveAndLoad_Reconciled));

            return result;
        }

        #region Helpers

        private static void AddReconciliationsAndExternalEntries(SqlCommand cmd, int userId, List<ExternalEntryForSave> externalEntriesForSave, List<ReconciliationForSave> reconciliations, List<int> deletedExternalEntryIds = null, List<int> deletedReconciliationIds = null)
        {
            cmd.Parameters.Add("@UserId", userId);

            // ExternalEntries
            DataTable externalEntriesTable = RepositoryUtilities.DataTable(externalEntriesForSave, addIndex: true);
            var externalEntriesTvp = new SqlParameter("@ExternalEntries", externalEntriesTable)
            {
                TypeName = $"[dbo].[{nameof(ExternalEntry)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(externalEntriesTvp);

            // Reconciliations
            var reconciliationsTable = new DataTable();
            reconciliationsTable.Columns.Add(new DataColumn("Index", typeof(int)));
            for (int i = 0; i < reconciliations.Count; i++)
            {
                DataRow row = reconciliationsTable.NewRow();
                row["Index"] = i;
                reconciliationsTable.Rows.Add(row);
            }
            var reconciliationsTvp = new SqlParameter("@Reconciliations", reconciliationsTable)
            {
                TypeName = $"[dbo].[{nameof(Reconciliation)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(reconciliationsTvp);

            // ReconciliationEntries
            var reconciliationEntriesTable = new DataTable();
            reconciliationEntriesTable.Columns.Add(new DataColumn("Index", typeof(int)));
            reconciliationEntriesTable.Columns.Add(new DataColumn("HeaderIndex", typeof(int)));
            reconciliationEntriesTable.Columns.Add(new DataColumn(nameof(ReconciliationEntryForSave.EntryId), typeof(int)));
            for (int i = 0; i < reconciliations.Count; i++)
            {
                var reconciliation = reconciliations[i];
                if (reconciliation != null && reconciliation.Entries != null)
                {
                    for (int j = 0; j < reconciliation.Entries.Count; j++)
                    {
                        var entry = reconciliation.Entries[j];
                        if (entry != null)
                        {
                            DataRow row = reconciliationEntriesTable.NewRow();
                            row["Index"] = j;
                            row["HeaderIndex"] = i;
                            row[nameof(ReconciliationEntryForSave.EntryId)] = entry.EntryId;
                            reconciliationEntriesTable.Rows.Add(row);
                        }
                    }
                }
            }
            var reconciliationEntriesTvp = new SqlParameter("@ReconciliationEntries", reconciliationEntriesTable)
            {
                TypeName = $"[dbo].[{nameof(ReconciliationEntry)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(reconciliationEntriesTvp);

            // ReconciliationExternalEntries
            var reconciliationExternalEntriesTable = new DataTable();
            reconciliationExternalEntriesTable.Columns.Add(new DataColumn("Index", typeof(int)));
            reconciliationExternalEntriesTable.Columns.Add(new DataColumn("HeaderIndex", typeof(int)));
            reconciliationExternalEntriesTable.Columns.Add(new DataColumn(nameof(ReconciliationExternalEntryForSave.ExternalEntryIndex), typeof(int)));
            reconciliationExternalEntriesTable.Columns.Add(new DataColumn(nameof(ReconciliationExternalEntryForSave.ExternalEntryId), typeof(int)));
            for (int i = 0; i < reconciliations.Count; i++)
            {
                var reconciliation = reconciliations[i];
                if (reconciliation != null && reconciliation.ExternalEntries != null)
                {
                    for (int j = 0; j < reconciliation.ExternalEntries.Count; j++)
                    {
                        var exEntry = reconciliation.ExternalEntries[j];
                        if (exEntry != null)
                        {
                            DataRow row = reconciliationExternalEntriesTable.NewRow();
                            row["Index"] = j;
                            row["HeaderIndex"] = i;
                            row[nameof(ReconciliationExternalEntryForSave.ExternalEntryIndex)] = (object)exEntry.ExternalEntryIndex ?? DBNull.Value;
                            row[nameof(ReconciliationExternalEntryForSave.ExternalEntryId)] = (object)exEntry.ExternalEntryId ?? DBNull.Value;
                            reconciliationExternalEntriesTable.Rows.Add(row);
                        }
                    }
                }
            }
            var reconciliationExternalEntriesTvp = new SqlParameter("@ReconciliationExternalEntries", reconciliationExternalEntriesTable)
            {
                TypeName = $"[dbo].[{nameof(ReconciliationExternalEntry)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(reconciliationExternalEntriesTvp);

            // DeletedExternalEntryIds
            if (deletedExternalEntryIds != null) // Validate SP doesn't take this params
            {
                DataTable deletedExternalEntryIdsTable = RepositoryUtilities.DataTable(deletedExternalEntryIds.Select(e => new IdListItem { Id = e }));
                var deletedExternalEntryIdsTvp = new SqlParameter("@DeletedExternalEntryIds", deletedExternalEntryIdsTable)
                {
                    TypeName = $"[dbo].[IdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(deletedExternalEntryIdsTvp);
            }

            // DeletedReconciliationIds
            if (deletedReconciliationIds != null) // Validate SP doesn't take this params
            {
                DataTable deletedReconciliationIdsTable = RepositoryUtilities.DataTable(deletedReconciliationIds.Select(e => new IdListItem { Id = e }));
                var deletedReconciliationIdsTvp = new SqlParameter("@DeletedReconcilationIds", deletedReconciliationIdsTable)
                {
                    TypeName = $"[dbo].[IdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(deletedReconciliationIdsTvp);
            }
        }

        private static void AddReconciledParamsInner(SqlCommand cmd, int accountId, int relationId, DateTime? fromDate, DateTime? toDate, decimal? fromAmount, decimal? toAmount, string externalReferenceContains, int top, int skip)
        {
            cmd.Parameters.Add("@AccountId", accountId);
            cmd.Parameters.Add("@RelationId", relationId);
            cmd.Parameters.Add("@FromDate", fromDate);
            cmd.Parameters.Add("@ToDate", toDate);
            cmd.Parameters.Add("@FromAmount", fromAmount);
            cmd.Parameters.Add("@ToAmount", toAmount);
            cmd.Parameters.Add("@ExternalReferenceContains", externalReferenceContains);
            cmd.Parameters.Add("@Top", top);
            cmd.Parameters.Add("@Skip", skip);

            // Output parameters
            var reconciledCountParam = new SqlParameter("@ReconciledCount", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            // Parameters
            cmd.Parameters.Add(reconciledCountParam);
        }

        private static async Task<ReconciledResult> LoadReconciledInner(SqlCommand cmd, CancellationToken cancellation = default)
        {
            // Result variables
            var result = new List<Reconciliation>();

            using (var reader = await cmd.ExecuteReaderAsync(cancellation))
            {
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    result.Add(new Reconciliation
                    {
                        Id = reader.GetInt32(i++),
                        CreatedAt = reader.GetDateTimeOffset(i++),
                        CreatedById = reader.Int32(i++),
                        Entries = new List<ReconciliationEntry>(),
                        ExternalEntries = new List<ReconciliationExternalEntry>(),
                    });
                }

                // Put the reconciliations in a dictionary for fast lookup
                var resultDic = result.ToDictionary(e => e.Id);

                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    int reconciliationId = reader.GetInt32(i++);

                    resultDic[reconciliationId].Entries.Add(new ReconciliationEntry
                    {
                        Id = reconciliationId,
                        EntryId = reader.GetInt32(i),
                        Entry = new EntryForReconciliation
                        {
                            Id = reader.GetInt32(i++),
                            PostingDate = reader.DateTime(i++),
                            Direction = reader.GetInt16(i++),
                            MonetaryValue = reader.Decimal(i++),
                            ExternalReference = reader.String(i++),
                            DocumentId = reader.GetInt32(i++),
                            DocumentDefinitionId = reader.GetInt32(i++),
                            DocumentSerialNumber = reader.GetInt32(i++),
                        }
                    });
                }


                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    int reconciliationId = reader.GetInt32(i++);

                    resultDic[reconciliationId].ExternalEntries.Add(new ReconciliationExternalEntry
                    {
                        Id = reconciliationId,
                        ExternalEntryId = reader.GetInt32(i),
                        ExternalEntry = new ExternalEntry
                        {
                            Id = reader.GetInt32(i++),
                            PostingDate = reader.DateTime(i++),
                            Direction = reader.GetInt16(i++),
                            MonetaryValue = reader.Decimal(i++),
                            ExternalReference = reader.String(i++)
                        }
                    });
                }
            }

            int reconciledCount = GetValue(cmd.Parameters["@ReconciledCount"].Value, 0);
            return new ReconciledResult(reconciledCount, result);
        }

        private static void AddUnreconciledParamsInner(SqlCommand cmd, int accountId, int relationId, DateTime? asOfDate, int top, int skip, int topExternal, int skipExternal)
        {
            // Add parameters
            cmd.Parameters.Add("@AccountId", accountId);
            cmd.Parameters.Add("@RelationId", relationId);
            cmd.Parameters.Add("@AsOfDate", asOfDate);
            cmd.Parameters.Add("@Top", top);
            cmd.Parameters.Add("@Skip", skip);
            cmd.Parameters.Add("@TopExternal", topExternal);
            cmd.Parameters.Add("@SkipExternal", skipExternal);

            // Output parameters
            var entriesBalanceParam = new SqlParameter("@EntriesBalance", SqlDbType.Decimal)
            {
                Direction = ParameterDirection.Output,
                Precision = 19,
                Scale = 4
            };
            var unreconciledEntriesBalanceParam = new SqlParameter("@UnreconciledEntriesBalance", SqlDbType.Decimal)
            {
                Direction = ParameterDirection.Output,
                Precision = 19,
                Scale = 4
            };
            var unreconciledExternalEntriesBalanceParam = new SqlParameter("@UnreconciledExternalEntriesBalance", SqlDbType.Decimal)
            {
                Direction = ParameterDirection.Output,
                Precision = 19,
                Scale = 4
            };
            var unreconciledEntriesCountParam = new SqlParameter("@UnreconciledEntriesCount", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var unreconciledExternalEntriesCountParam = new SqlParameter("@UnreconciledExternalEntriesCount", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            // Parameters
            cmd.Parameters.Add(entriesBalanceParam);
            cmd.Parameters.Add(unreconciledEntriesBalanceParam);
            cmd.Parameters.Add(unreconciledExternalEntriesBalanceParam);
            cmd.Parameters.Add(unreconciledEntriesCountParam);
            cmd.Parameters.Add(unreconciledExternalEntriesCountParam);
        }

        private static async Task<UnreconciledResult> LoadUnreconciledInner(SqlCommand cmd, CancellationToken cancellation = default)
        {
            // Result variables
            var entries = new List<EntryForReconciliation>();
            var externalEntries = new List<ExternalEntry>();

            using (var reader = await cmd.ExecuteReaderAsync(cancellation))
            {
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    entries.Add(new EntryForReconciliation
                    {
                        Id = reader.GetInt32(i++),
                        PostingDate = reader.DateTime(i++),
                        Direction = reader.GetInt16(i++),
                        MonetaryValue = reader.Decimal(i++),
                        ExternalReference = reader.String(i++),
                        DocumentId = reader.GetInt32(i++),
                        DocumentDefinitionId = reader.GetInt32(i++),
                        DocumentSerialNumber = reader.GetInt32(i++),
                        IsReconciledLater = reader.GetBoolean(i++),
                    });
                }

                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    externalEntries.Add(new ExternalEntry
                    {
                        Id = reader.GetInt32(i++),
                        PostingDate = reader.DateTime(i++),
                        Direction = reader.GetInt16(i++),
                        MonetaryValue = reader.Decimal(i++),
                        ExternalReference = reader.String(i++),
                        CreatedById = reader.Int32(i++),
                        CreatedAt = reader.GetDateTimeOffset(i++),
                        ModifiedById = reader.Int32(i++),
                        ModifiedAt = reader.GetDateTimeOffset(i++),
                        IsReconciledLater = reader.GetBoolean(i++),
                    });
                }
            }

            decimal entriesBalance = GetValue(cmd.Parameters["@EntriesBalance"].Value, 0m);
            decimal unreconciledEntriesBalance = GetValue(cmd.Parameters["@UnreconciledEntriesBalance"].Value, 0m);
            decimal unreconciledExternalEntriesBalance = GetValue(cmd.Parameters["@UnreconciledExternalEntriesBalance"].Value, 0m);
            int unreconciledEntriesCount = GetValue(cmd.Parameters["@UnreconciledEntriesCount"].Value, 0);
            int unreconciledExternalEntriesCount = GetValue(cmd.Parameters["@UnreconciledExternalEntriesCount"].Value, 0);

            return new UnreconciledResult(
                entriesBalance, 
                unreconciledEntriesBalance, 
                unreconciledExternalEntriesBalance, 
                unreconciledEntriesCount, 
                unreconciledExternalEntriesCount, 
                entries, 
                externalEntries);
        }

        #endregion

        #endregion

        #region RelationDefinitions

        public async Task<SaveResult> RelationDefinitions__Save(List<RelationDefinitionForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(RelationDefinitions__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(RelationDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable reportDefinitionsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.ReportDefinitions);
                var reportDefinitionsTvp = new SqlParameter("@ReportDefinitions", reportDefinitionsTable)
                {
                    TypeName = $"[dbo].[{nameof(RelationDefinitionReportDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add(reportDefinitionsTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(RelationDefinitions__Save));

            return result;
        }

        public async Task<DeleteResult> RelationDefinitions__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(RelationDefinitions__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(RelationDefinitions__Delete));

            return result;
        }

        public async Task<OperationResult> RelationDefinitions__UpdateState(List<int> ids, string state, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(RelationDefinitions__UpdateState)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@State", state);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(RelationDefinitions__UpdateState));

            return result;
        }

        #endregion

        #region Relations

        private static SqlParameter RelationsTvp(List<RelationForSave> entities)
        {
            var extraRelationColumns = new List<ExtraColumn<RelationForSave>> {
                    RepositoryUtilities.Column("ImageId", typeof(string), (RelationForSave e) => e.Image == null ? "(Unchanged)" : e.EntityMetadata?.FileId),
                    RepositoryUtilities.Column("UpdateAttachments", typeof(bool), (RelationForSave e) => e.Attachments != null),
                };

            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true, extraColumns: extraRelationColumns);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(Relation)}List]",
                SqlDbType = SqlDbType.Structured
            };

            return entitiesTvp;
        }

        private static SqlParameter RelationUsersTvp(List<RelationForSave> entities)
        {
            DataTable usersTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Users);
            var usersTvp = new SqlParameter("@RelationUsers", usersTable)
            {
                TypeName = $"[dbo].[{nameof(RelationUser)}List]",
                SqlDbType = SqlDbType.Structured
            };

            return usersTvp;
        }

        private static SqlParameter RelationAttachmentsTvp(List<RelationForSave> entities)
        {
            var extraAttachmentColumns = new List<ExtraColumn<RelationAttachmentForSave>> {
                    RepositoryUtilities.Column("FileId", typeof(string), (RelationAttachmentForSave e) => e.EntityMetadata?.FileId),
                    RepositoryUtilities.Column("Size", typeof(long), (RelationAttachmentForSave e) => e.EntityMetadata?.FileSize)
                };

            DataTable attachmentsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Attachments, extraColumns: extraAttachmentColumns);
            var attachmentsTvp = new SqlParameter("@Attachments", attachmentsTable)
            {
                TypeName = $"[dbo].[{nameof(RelationAttachment)}List]",
                SqlDbType = SqlDbType.Structured
            };

            return attachmentsTvp;
        }

        public async Task Relations__Preprocess(int definitionId, List<RelationForSave> entities, int userId)
        {
            var connString = await GetConnectionString();

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[bll].[{nameof(Relations__Preprocess)}]";

                // Parameters
                var entitiesTvp = RelationsTvp(entities);

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                var props = TypeDescriptor.Get<RelationForSave>().SimpleProperties;
                while (await reader.ReadAsync())
                {
                    var index = reader.GetInt32(0);
                    var entity = entities[index];

                    foreach (var prop in props)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entity, propValue);
                    }
                }
            },
            DatabaseName(connString), nameof(Relations__Preprocess));
        }

        public async Task<(SaveWithImagesResult result, List<string> deletedAttachmentIds)> Relations__Save(int definitionId, List<RelationForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveWithImagesResult result = null;
            List<string> deletedAttachmentIds = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Relations__Save)}]";

                // Parameters
                var entitiesTvp = RelationsTvp(entities);
                var usersTvp = RelationUsersTvp(entities);
                var attachmentsTvp = RelationAttachmentsTvp(entities);

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add(usersTvp);
                cmd.Parameters.Add(attachmentsTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveWithImagesResult(returnIds, validateOnly);

                if (!result.IsError && !validateOnly)
                {
                    deletedAttachmentIds = new List<string>();
                    await reader.NextResultAsync();
                    while (await reader.ReadAsync())
                    {
                        deletedAttachmentIds.Add(reader.String(0));
                    }
                }
            },
            DatabaseName(connString), nameof(Relations__Save));

            return (result, deletedAttachmentIds);
        }

        public async Task<(DeleteWithImagesResult result, List<string> deletedAttachmentIds)> Relations__Delete(int definitionId, IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteWithImagesResult result = null;
            List<string> deletedAttachmentIds = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Relations__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteWithImagesResult(validateOnly);

                    if (!result.IsError && !validateOnly)
                    {
                        // LoadDeleteWithImagesResult already calls next result set
                        deletedAttachmentIds = new List<string>();
                        while (await reader.ReadAsync())
                        {
                            deletedAttachmentIds.Add(reader.String(0));
                        }
                    }
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(Relations__Save));

            return (result, deletedAttachmentIds);
        }

        public async Task<OperationResult> Relations__Activate(int definitionId, List<int> ids, bool isActive, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Relations__Activate)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@IsActive", isActive);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(Relations__Activate));

            return result;
        }

        #endregion

        #region ReportDefinitions

        public static (DataTable rows, DataTable columns) DataTableFromReportDefinitionDimensionAttributes(IEnumerable<ReportDefinitionForSave> reports)
        {
            var rowsAttributesTable = new DataTable();
            rowsAttributesTable.Columns.Add(new DataColumn("Index", typeof(int)));
            rowsAttributesTable.Columns.Add(new DataColumn("HeaderIndex", typeof(int)));
            rowsAttributesTable.Columns.Add(new DataColumn("ReportDefinitionIndex", typeof(int)));

            RepositoryUtilities.AddColumnsFromProperties<ReportDefinitionDimensionAttributeForSave>(rowsAttributesTable);

            var colsAttributesTable = new DataTable();
            colsAttributesTable.Columns.Add(new DataColumn("Index", typeof(int)));
            colsAttributesTable.Columns.Add(new DataColumn("HeaderIndex", typeof(int)));
            colsAttributesTable.Columns.Add(new DataColumn("ReportDefinitionIndex", typeof(int)));

            var attributeProps = RepositoryUtilities.AddColumnsFromProperties<ReportDefinitionDimensionAttributeForSave>(colsAttributesTable);

            int reportIndex = 0;
            foreach (var report in reports)
            {
                int rowIndex = 0;
                foreach (var row in report.Rows)
                {
                    int attIndex = 0;
                    foreach (var att in row.Attributes)
                    {
                        DataRow rowAttributeRow = rowsAttributesTable.NewRow();

                        rowAttributeRow["Index"] = attIndex;
                        rowAttributeRow["HeaderIndex"] = rowIndex;
                        rowAttributeRow["ReportDefinitionIndex"] = reportIndex;

                        foreach (var attributeProp in attributeProps)
                        {
                            var propValue = attributeProp.GetValue(att);
                            rowAttributeRow[attributeProp.Name] = propValue ?? DBNull.Value;
                        }

                        rowsAttributesTable.Rows.Add(rowAttributeRow);
                        attIndex++;
                    }

                    rowIndex++;
                }

                int colIndex = 0;
                foreach (var col in report.Columns)
                {
                    int attIndex = 0;
                    foreach (var att in col.Attributes)
                    {
                        DataRow colAttributeRow = colsAttributesTable.NewRow();

                        colAttributeRow["Index"] = attIndex;
                        colAttributeRow["HeaderIndex"] = colIndex;
                        colAttributeRow["ReportDefinitionIndex"] = reportIndex;

                        foreach (var attributeProp in attributeProps)
                        {
                            var propValue = attributeProp.GetValue(att);
                            colAttributeRow[attributeProp.Name] = propValue ?? DBNull.Value;
                        }

                        colsAttributesTable.Rows.Add(colAttributeRow);
                        attIndex++;
                    }

                    colIndex++;
                }

                reportIndex++;
            }

            return (rowsAttributesTable, colsAttributesTable);
        }

        public async Task<SaveResult> ReportDefinitions__Save(List<ReportDefinitionForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(ReportDefinitions__Save)}]";

                // Parameters
                var entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(ReportDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                var parametersTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Parameters);
                var parametersTvp = new SqlParameter("@Parameters", parametersTable)
                {
                    TypeName = $"[dbo].[{nameof(ReportDefinitionParameter)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                var selectTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Select);
                var selectTvp = new SqlParameter("@Select", selectTable)
                {
                    TypeName = $"[dbo].[{nameof(ReportDefinitionSelect)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                var rowsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Rows);
                var rowsTvp = new SqlParameter("@Rows", rowsTable)
                {
                    TypeName = $"[dbo].[ReportDefinitionDimensionList]",
                    SqlDbType = SqlDbType.Structured
                };

                var columnsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Columns);
                var columnsTvp = new SqlParameter("@Columns", columnsTable)
                {
                    TypeName = $"[dbo].[ReportDefinitionDimensionList]",
                    SqlDbType = SqlDbType.Structured
                };

                var (rowsAttributesTable, colsAttributesTable) = DataTableFromReportDefinitionDimensionAttributes(entities);
                var rowsAttributesTvp = new SqlParameter("@RowsAttributes", rowsAttributesTable)
                {
                    TypeName = $"[dbo].[{nameof(ReportDefinitionDimensionAttribute)}List]",
                    SqlDbType = SqlDbType.Structured
                };
                var columnsAttributesTvp = new SqlParameter("@ColumnsAttributes", colsAttributesTable)
                {
                    TypeName = $"[dbo].[{nameof(ReportDefinitionDimensionAttribute)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                var measuresTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Measures);
                var measuresTvp = new SqlParameter("@Measures", measuresTable)
                {
                    TypeName = $"[dbo].[{nameof(ReportDefinitionMeasure)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                var rolesTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Roles);
                var rolesTvp = new SqlParameter("@Roles", rolesTable)
                {
                    TypeName = $"[dbo].[{nameof(ReportDefinitionRole)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add(parametersTvp);
                cmd.Parameters.Add(selectTvp);
                cmd.Parameters.Add(rowsTvp);
                cmd.Parameters.Add(rowsAttributesTvp);
                cmd.Parameters.Add(columnsTvp);
                cmd.Parameters.Add(columnsAttributesTvp);
                cmd.Parameters.Add(measuresTvp);
                cmd.Parameters.Add(rolesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(ReportDefinitions__Save));

            return result;
        }

        public async Task<DeleteResult> ReportDefinitions__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(ReportDefinitions__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(DashboardDefinitions__Delete));

            return result;
        }

        #endregion

        #region ResourceDefinitions

        public async Task<SaveResult> ResourceDefinitions__Save(List<ResourceDefinitionForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(ResourceDefinitions__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(ResourceDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable reportDefinitionsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.ReportDefinitions);
                var reportDefinitionsTvp = new SqlParameter("@ReportDefinitions", reportDefinitionsTable)
                {
                    TypeName = $"[dbo].[{nameof(ResourceDefinitionReportDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add(reportDefinitionsTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(ResourceDefinitions__Save));

            return result;
        }

        public async Task<DeleteResult> ResourceDefinitions__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(ResourceDefinitions__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(ResourceDefinitions__Delete));

            return result;
        }

        public async Task<OperationResult> ResourceDefinitions__UpdateState(List<int> ids, string state, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(ResourceDefinitions__UpdateState)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@State", state);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(ResourceDefinitions__UpdateState));

            return result;
        }

        #endregion

        #region Resources

        private static SqlParameter ResourcesTvp(List<ResourceForSave> entities)
        {
            var extraColumns = new List<ExtraColumn<ResourceForSave>> {
                    RepositoryUtilities.Column("ImageId", typeof(string), (ResourceForSave e) => e.Image == null ? "(Unchanged)" : e.EntityMetadata?.FileId)
                };

            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true, extraColumns: extraColumns);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(Resource)}List]",
                SqlDbType = SqlDbType.Structured
            };

            return entitiesTvp;
        }

        private static SqlParameter ResourceUnitsTvp(List<ResourceForSave> entities)
        {
            DataTable unitsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Units);
            var unitsTvp = new SqlParameter("@ResourceUnits", unitsTable)
            {
                TypeName = $"[dbo].[{nameof(ResourceUnit)}List]",
                SqlDbType = SqlDbType.Structured
            };

            return unitsTvp;
        }

        public async Task Resources__Preprocess(int definitionId, List<ResourceForSave> entities, int userId)
        {
            var connString = await GetConnectionString();

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[bll].[{nameof(Resources__Preprocess)}]";

                // Parameters
                var entitiesTvp = ResourcesTvp(entities);

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                var props = TypeDescriptor.Get<ResourceForSave>().SimpleProperties;
                while (await reader.ReadAsync())
                {
                    var index = reader.GetInt32(0);
                    var entity = entities[index];

                    foreach (var prop in props)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entity, propValue);
                    }
                }
            },
            DatabaseName(connString), nameof(Resources__Preprocess));
        }

        public async Task<SaveWithImagesResult> Resources__Save(int definitionId, List<ResourceForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveWithImagesResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Resources__Save)}]";

                // Parameters
                var entitiesTvp = ResourcesTvp(entities);
                var unitsTvp = ResourceUnitsTvp(entities);

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add(unitsTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveWithImagesResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(Resources__Save));

            return result;
        }

        public async Task<DeleteWithImagesResult> Resources__Delete(int definitionId, IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteWithImagesResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Resources__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteWithImagesResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(Resources__Save));

            return result;
        }

        public async Task<OperationResult> Resources__Activate(int definitionId, List<int> ids, bool isActive, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Resources__Activate)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@IsActive", isActive);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(Resources__Activate));

            return result;
        }

        #endregion

        #region Roles

        public async Task<SaveResult> Roles__Save(List<RoleForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            entities.ForEach(e =>
            {
                e.Members?.ForEach(m =>
                {
                    m.RoleId = e.Id;
                });
            });

            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Roles__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(Role)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable membersTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Members);
                var membersTvp = new SqlParameter("@Members", membersTable)
                {
                    TypeName = $"[dbo].[{nameof(RoleMembership)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable permissionsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Permissions);
                var permissionsTvp = new SqlParameter("@Permissions", permissionsTable)
                {
                    TypeName = $"[dbo].[{nameof(Permission)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add(membersTvp);
                cmd.Parameters.Add(permissionsTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(Roles__Save));

            return result;
        }

        public async Task<DeleteResult> Roles__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Roles__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(Roles__Delete));

            return result;
        }

        public async Task<OperationResult> Roles__Activate(List<int> ids, bool isActive, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Roles__Activate)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@IsActive", isActive);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(Roles__Activate));

            return result;
        }

        #endregion

        #region Units

        public async Task<SaveResult> Units__Save(List<UnitForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Units__Save)}]";

                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(Unit)}List]",
                    SqlDbType = SqlDbType.Structured
                };


                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(Units__Save));

            return result;
        }

        public async Task<DeleteResult> Units__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Units__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult(validateOnly);
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(Units__Delete));

            return result;
        }

        public async Task<OperationResult> Units__Activate(List<int> ids, bool isActive, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Units__Activate)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@IsActive", isActive);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(Units__Activate));

            return result;
        }

        #endregion

        #region Users

        private static SqlParameter UsersTvp(List<UserForSave> entities)
        {
            var extraColumns = new List<ExtraColumn<UserForSave>> {
                    RepositoryUtilities.Column(
                        name: "ImageId",
                        type: typeof(string),
                        getValue: (UserForSave e) => e.Image == null ? "(Unchanged)" : e.EntityMetadata?.FileId
                        )
                };

            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true, extraColumns: extraColumns);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(User)}List]",
                SqlDbType = SqlDbType.Structured
            };

            return entitiesTvp;
        }

        public async Task Users__SaveSettings(string key, string value, int userId)
        {
            var connString = await GetConnectionString();

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Users__SaveSettings)}]";

                // Parameters
                cmd.Parameters.Add("@Key", key);
                cmd.Parameters.Add("@Value", value);
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            },
            DatabaseName(connString), nameof(Users__SaveSettings));
        }

        public async Task Users__SavePreferredLanguage(string preferredLanguage, int userId, CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Users__SavePreferredLanguage)}]";

                // Parameters
                cmd.Parameters.Add("@PreferredLanguage", preferredLanguage);
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync(cancellation);
                await cmd.ExecuteNonQueryAsync(cancellation);
            },
            DatabaseName(connString), nameof(Users__SavePreferredLanguage), cancellation);
        }

        public async Task Users__SavePreferredCalendar(string preferredCalendar, int userId, CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Users__SavePreferredCalendar)}]";

                // Parameters
                cmd.Parameters.Add("@PreferredCalendar", preferredCalendar);
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync(cancellation);
                await cmd.ExecuteNonQueryAsync(cancellation);
            },
            DatabaseName(connString), nameof(Users__SavePreferredCalendar), cancellation);
        }

        public async Task Users__SetExternalIdByUserId(int userId, string externalId)
        {
            var connString = await GetConnectionString();

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Users__SetExternalIdByUserId)}]";

                // Parameters
                cmd.Parameters.Add("@UserId", userId);
                cmd.Parameters.Add("@ExternalId", externalId);

                // Execute
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            },
            DatabaseName(connString), nameof(Users__SetExternalIdByUserId));
        }

        public async Task Users__SetEmailByUserId(int userId, string externalEmail)
        {
            var connString = await GetConnectionString();

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Users__SetEmailByUserId)}]";

                // Parameters
                cmd.Parameters.Add("UserId", userId);
                cmd.Parameters.Add("ExternalEmail", externalEmail);

                // Execute
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            },
            DatabaseName(connString), nameof(Users__SetEmailByUserId));
        }

        public async Task<SaveWithImagesResult> Users__Save(List<UserForSave> entities, bool returnIds, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            SaveWithImagesResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Users__Save)}]";

                // Parameters
                var entitiesTvp = UsersTvp(entities);

                DataTable rolesTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Roles);
                var rolesTvp = new SqlParameter("@Roles", rolesTable)
                {
                    TypeName = $"[dbo].[{nameof(RoleMembership)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add(rolesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveWithImagesResult(returnIds, validateOnly);
            },
            DatabaseName(connString), nameof(Users__Save));

            return result;
        }

        public async Task<(DeleteWithImagesResult result, IEnumerable<string> emails)> Users__Delete(IEnumerable<int> ids, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            DeleteWithImagesResult result = null;
            List<string> emails = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Users__Delete)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteWithImagesResult(validateOnly);

                    // Load the emails of deleted users
                    if (!result.IsError && !validateOnly)
                    {
                        // LoadDeleteWithImagesResult already calls next result set
                        emails = new List<string>();
                        while (await reader.ReadAsync())
                        {
                            emails.Add(reader.String(0));
                        }
                    }
                }
                catch (SqlException ex) when (IsForeignKeyViolation(ex))
                {
                    // Validation should prevent this
                    throw new ForeignKeyViolationException();
                }
            },
            DatabaseName(connString), nameof(Users__Delete));

            return (result, emails);
        }

        public async Task<OperationResult> Users__Activate(List<int> ids, bool isActive, bool validateOnly, int top, int userId)
        {
            var connString = await GetConnectionString();
            OperationResult result = null;

            await TransactionalDatabaseOperation(async () =>
            {
                // Connection
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[api].[{nameof(Users__Activate)}]";

                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@IsActive", isActive);
                cmd.Parameters.Add("@ValidateOnly", validateOnly);
                cmd.Parameters.Add("@Top", top);
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);


                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult(validateOnly);
            },
            DatabaseName(connString), nameof(Users__Activate));

            return result;
        }

        #endregion
    }
}

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
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
        private readonly ICachingShardResolver _shardResolver;
        private readonly ILogger _logger;
        private readonly IStatementLoader _loader;

        protected override ILogger Logger => _logger;

        public ApplicationRepository(int tenantId, ICachingShardResolver shardResolver, ILogger<ApplicationRepository> logger)
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
        /// Returns a function that maps every <see cref="Entity"/> type in the application dB
        /// to the default SQL query that retrieves it. 
        /// Some SQL queries may require additional parameters.
        /// </summary>
        public static string Sources(Type t) => t.Name switch
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
            nameof(RequiredSignature) => "[map].[DocumentsRequiredSignatures](@DocumentIds)",
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

        public EntityQuery<FinancialSettings> FinancialSettings => EntityQuery<FinancialSettings>();
        public EntityQuery<GeneralSettings> GeneralSettings => EntityQuery<GeneralSettings>();
        public EntityQuery<User> Users => EntityQuery<User>();
        public EntityQuery<Unit> Units => EntityQuery<Unit>();
        public EntityQuery<Relation> Relations => EntityQuery<Relation>();
        public EntityQuery<Resource> Resources => EntityQuery<Resource>();
        public EntityQuery<Currency> Currencies => EntityQuery<Currency>();
        public EntityQuery<ExchangeRate> ExchangeRates => EntityQuery<ExchangeRate>();
        public EntityQuery<AccountClassification> AccountClassifications => EntityQuery<AccountClassification>();
        public EntityQuery<AccountType> AccountTypes => EntityQuery<AccountType>();
        public EntityQuery<Agent> Agents => EntityQuery<Agent>();
        public EntityQuery<Center> Centers => EntityQuery<Center>();

        #endregion

        #region Helpers

        private string _lastConnString = null;
        private string _dbName = null; // Caches the DB Name

        private Task<string> GetConnectionString(CancellationToken cancellation = default) =>
            _shardResolver.GetConnectionString(_tenantId, cancellation);

        private string DatabaseName(string connString)
        {
            if (_lastConnString != connString)
            {
                _lastConnString = connString;
                _dbName = new SqlConnectionStringBuilder(connString).InitialCatalog;
            }

            return _dbName;
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
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                if (await reader.ReadAsync(cancellation))
                {
                    int i = 0;

                    result = new OnConnectResult
                    (
                        userId: reader.Int32(i++),
                        email: reader.String(i++),
                        externalId: reader.String(i++),
                        permissionsVersion: reader.Guid(i++),
                        userSettingsVersion: reader.Guid(i++),
                        settingsVersion: reader.GetGuid(i++),
                        definitionsVersion: reader.GetGuid(i++)
                    );

                    // TODO

                    //// The user Info
                    //userInfo = new UserInfo
                    //{
                    //    UserId = reader.Int32(i++),
                    //    Name = reader.String(i++),
                    //    Name2 = reader.String(i++),
                    //    Name3 = reader.String(i++),
                    //    ExternalId = reader.String(i++),
                    //    Email = reader.String(i++),
                    //    PermissionsVersion = reader.Guid(i++)?.ToString(),
                    //    UserSettingsVersion = reader.Guid(i++)?.ToString(),
                    //};

                    //// The tenant Info
                    //tenantInfo = new TenantInfo
                    //{
                    //    ShortCompanyName = reader.String(i++),
                    //    ShortCompanyName2 = reader.String(i++),
                    //    ShortCompanyName3 = reader.String(i++),
                    //    DefinitionsVersion = reader.Guid(i++)?.ToString(),
                    //    SettingsVersion = reader.Guid(i++)?.ToString(),
                    //    PrimaryLanguageId = reader.String(i++),
                    //    PrimaryLanguageSymbol = reader.String(i++),
                    //    SecondaryLanguageId = reader.String(i++),
                    //    SecondaryLanguageSymbol = reader.String(i++),
                    //    TernaryLanguageId = reader.String(i++),
                    //    TernaryLanguageSymbol = reader.String(i++),
                    //    DateFormat = reader.String(i++),
                    //    TimeFormat = reader.String(i++),
                    //    TaxIdentificationNumber = reader.String(i++),
                    //};
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

                SqlParameter emailTvp = new SqlParameter("@Emails", emailTable)
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
                using var reader = await cmd.ExecuteReaderAsync(cancellation);

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

        public async Task<SaveResult> AccountClassifications__Save(List<AccountClassificationForSave> entities, bool returnIds, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds);
            },
            DatabaseName(connString), nameof(AccountClassifications__Save));

            return result;
        }

        public async Task<DeleteResult> AccountClassifications__Delete(IEnumerable<int> ids, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult();
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

        public async Task<DeleteResult> AccountClassifications__DeleteWithDescendants(IEnumerable<int> ids, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult();
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

        public async Task<OperationResult> AccountClassifications__Activate(List<int> ids, bool isActive, int userId)
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
                cmd.Parameters.Add("@UserId", userId);


                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult();
            },
            _dbName, nameof(AccountClassifications__Activate));

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

                // Execute
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

        public async Task<SaveResult> Accounts__Save(List<AccountForSave> entities, bool returnIds, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds);
            },
            DatabaseName(connString), nameof(Accounts__Save));

            return result;
        }

        public async Task<DeleteResult> Accounts__Delete(IEnumerable<int> ids, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult();
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

        public async Task<OperationResult> Accounts__Activate(List<int> ids, bool isActive, int userId)
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
                cmd.Parameters.Add("@UserId", userId);


                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult();
            },
            _dbName, nameof(Accounts__Activate));

            return result;
        }

        #endregion

        #region AccountTypes

        public async Task<SaveResult> AccountTypes__Save(List<AccountTypeForSave> entities, bool returnIds, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds);
            },
            DatabaseName(connString), nameof(AccountTypes__Save));

            return result;
        }

        public async Task<DeleteResult> AccountTypes__Delete(IEnumerable<int> ids, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult();
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

        public async Task<DeleteResult> AccountTypes__DeleteWithDescendants(IEnumerable<int> ids, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult();
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

        public async Task<OperationResult> AccountTypes__Activate(List<int> ids, bool isActive, int userId)
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
                cmd.Parameters.Add("@UserId", userId);


                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult();
            },
            _dbName, nameof(AccountTypes__Activate));

            return result;
        }

        #endregion

        #region Agents

        public async Task<SaveResult> Agents__Save(List<AgentForSave> entities, bool returnIds, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds);
            },
            DatabaseName(connString), nameof(Agents__Save));

            return result;
        }

        public async Task<DeleteResult> Agents__Delete(IEnumerable<int> ids, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult();
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

        public async Task<OperationResult> Agents__Activate(List<int> ids, bool isActive, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult();
            },
            _dbName, nameof(Agents__Activate));

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

        public async Task<SaveResult> Centers__Save(List<CenterForSave> entities, bool returnIds, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds);
            },
            DatabaseName(connString), nameof(Centers__Save));

            return result;
        }

        public async Task<DeleteResult> Centers__Delete(IEnumerable<int> ids, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult();
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

        public async Task<DeleteResult> Centers__DeleteWithDescendants(IEnumerable<int> ids, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult();
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

        public async Task<OperationResult> Centers__Activate(List<int> ids, bool isActive, int userId)
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
                cmd.Parameters.Add("@UserId", userId);


                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult();
            },
            _dbName, nameof(Centers__Activate));

            return result;
        }

        #endregion

        #region Currencies

        public async Task<OperationResult> Currencies__Save(List<CurrencyForSave> entities, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult();
            },
            DatabaseName(connString), nameof(Currencies__Save));

            return result;
        }

        public async Task<DeleteResult> Currencies__Delete(IEnumerable<string> ids, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult();
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

        public async Task<OperationResult> Currencies__Activate(List<string> ids, bool isActive, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult();
            },
            _dbName, nameof(Currencies__Activate));

            return result;
        }

        #endregion

        #region DashboardDefinitions

        public async Task<SaveResult> DashboardDefinitions__Save(List<DashboardDefinitionForSave> entities, bool returnIds, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds);
            },
            DatabaseName(connString), nameof(DashboardDefinitions__Save));

            return result;
        }

        public async Task<DeleteResult> DashboardDefinitions__Delete(IEnumerable<int> ids, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult();
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

        public async Task<SaveResult> DocumentDefinitions__Save(List<DocumentDefinitionForSave> entities, bool returnIds, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds);
            },
            DatabaseName(connString), nameof(DocumentDefinitions__Save));

            return result;
        }

        public async Task<DeleteResult> DocumentDefinitions__Delete(IEnumerable<int> ids, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult();
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

        public async Task<OperationResult> DocumentDefinitions__UpdateState(List<int> ids, string state, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult();
            },
            _dbName, nameof(DocumentDefinitions__UpdateState));

            return result;
        }

        #endregion

        #region Documents

        #region Helpers

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

        private static async Task<List<InboxStatus>> LoadInboxStatuses(SqlDataReader reader)
        {
            var result = new List<InboxStatus>();

            while (await reader.ReadAsync())
            {
                int i = 0;
                var externalId = reader.GetString(i++);
                var count = reader.GetInt32(i++);
                var unknownCount = reader.GetInt32(i++);

                result.Add(new InboxStatus(externalId, count, unknownCount));
            }

            return result;
        }

        private static void AddCultureAndNeutralCulture(SqlCommand cmd)
        {
            var culture = CultureInfo.CurrentUICulture.Name;
            var neutralCulture = CultureInfo.CurrentUICulture.IsNeutralCulture ? CultureInfo.CurrentUICulture.Name : CultureInfo.CurrentUICulture.Parent.Name;

            cmd.Parameters.Add("@Culture", culture);
            cmd.Parameters.Add("@NeutralCulture", neutralCulture);
        }

        #endregion

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

        public async Task<(SaveResult result, List<InboxStatus> inboxStatuses, List<string> deletedFileIds)> Documents__Save(int definitionId, List<DocumentForSave> documents, bool returnIds, int userId)
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
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                // (1) Load result
                result = await reader.LoadSaveResult(returnIds);

                // (2) Load inbox statuses
                inboxStatuses = await LoadInboxStatuses(reader);

                // (3) Load deleted file Ids
                await reader.NextResultAsync();
                while (await reader.ReadAsync())
                {
                    deletedFileIds.Add(reader.GetString(0));
                }
            },
            DatabaseName(connString), nameof(Documents__Save));

            return (result, inboxStatuses, deletedFileIds);
        }

        public async Task<SignResult> Lines__Sign(IEnumerable<int> ids, short toState, int? reasonId, string reasonDetails, int? onBehalfOfUserId, string ruleType, int? roleId, DateTimeOffset? signedAt, bool returnIds, int userId)
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
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }));
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IdList]",
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
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSignResult(returnIds);
            },
            DatabaseName(connString), nameof(Lines__Sign));

            return result;
        }

        public async Task<SignResult> LineSignatures__Delete(IEnumerable<int> ids, bool returnIds, int userId)
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
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSignResult(returnIds);
            },
            DatabaseName(connString), nameof(LineSignatures__Delete));

            return result;
        }

        public async Task<AssignResult> Documents__Assign(IEnumerable<int> ids, int assigneeId, string comment, int userId)
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
                cmd.Parameters.Add("@UserId", userId);
                AddCultureAndNeutralCulture(cmd);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                // (1) Load Errors
                var errors = await reader.LoadErrors();

                // (2) Inbox Statuses
                await reader.NextResultAsync();
                List<InboxStatus> inboxStatuses = await LoadInboxStatuses(reader);

                // (3) Assignee Info + Doc Serial
                User assigneeInfo = default;
                int serialNumber = default;
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

                result = new AssignResult(errors, inboxStatuses, assigneeInfo, serialNumber);
            },
            DatabaseName(connString), nameof(Documents__Assign));

            return result;
        }


        public async Task<(List<InboxStatus> NotificationInfos, List<string> DeletedFileIds)> Documents__Delete(IEnumerable<int> ids)
        {
            using var _ = Instrumentation.Block("Repo." + nameof(Documents__Delete));

            // Returns the new notifification counts of affected users, and the list of File Ids to be deleted
            var notificationInfos = new List<InboxStatus>();
            var deletedFileIds = new List<string>();

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }));
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Documents__Delete)}]";

            // Execute
            try
            {
                using var reader = await cmd.ExecuteReaderAsync();

                // Load notification infos
                await RepositoryUtilities.LoadInboxStatuses(reader, notificationInfos);

                // Load deleted file Ids
                await reader.NextResultAsync();
                while (await reader.ReadAsync())
                {
                    deletedFileIds.Add(reader.String(0));
                }
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }

            return (notificationInfos, deletedFileIds);
        }

        public async Task<IEnumerable<ValidationError>> Documents_Validate__Delete(int definitionId, List<int> ids, int top)
        {
            using var _ = Instrumentation.Block("Repo." + nameof(Documents_Validate__Delete));

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add("@DefinitionId", definitionId);
            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Documents_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        // Posting State Management

        public async Task<IEnumerable<ValidationError>> Documents_Validate__Close(int definitionId, List<int> ids, int top)
        {
            using var _ = Instrumentation.Block("Repo." + nameof(Documents_Validate__Close));

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add("@DefinitionId", definitionId);
            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Documents_Validate__Close)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<InboxStatus>> Documents__Close(List<int> ids)
        {
            using var _ = Instrumentation.Block("Repo." + nameof(Documents__Close));

            var result = new List<int>();

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }));
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Documents__Close)}]";

            // Execute
            using var reader = await cmd.ExecuteReaderAsync();
            return await RepositoryUtilities.LoadInboxStatuses(reader);
        }

        public async Task<IEnumerable<ValidationError>> Documents_Validate__Open(int definitionId, List<int> ids, int top)
        {
            using var _ = Instrumentation.Block("Repo." + nameof(Documents_Validate__Open));

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add("@DefinitionId", definitionId);
            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Documents_Validate__Open)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<InboxStatus>> Documents__Open(List<int> ids)
        {
            using var _ = Instrumentation.Block("Repo." + nameof(Documents__Open));

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }));
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Documents__Open)}]";

            // Execute
            using var reader = await cmd.ExecuteReaderAsync();
            return await RepositoryUtilities.LoadInboxStatuses(reader);
        }

        public async Task<IEnumerable<ValidationError>> Documents_Validate__Cancel(int definitionId, List<int> ids, int top)
        {
            using var _ = Instrumentation.Block("Repo." + nameof(Documents_Validate__Cancel));

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add("@DefinitionId", definitionId);
            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Documents_Validate__Cancel)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<InboxStatus>> Documents__Cancel(List<int> ids)
        {
            using var _ = Instrumentation.Block("Repo." + nameof(Documents__Cancel));

            var result = new List<int>();

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }));
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Documents__Cancel)}]";

            // Execute
            using var reader = await cmd.ExecuteReaderAsync();
            return await RepositoryUtilities.LoadInboxStatuses(reader);
        }

        public async Task<IEnumerable<ValidationError>> Documents_Validate__Uncancel(int definitionId, List<int> ids, int top)
        {
            using var _ = Instrumentation.Block("Repo." + nameof(Documents_Validate__Uncancel));

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add("@DefinitionId", definitionId);
            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Documents_Validate__Uncancel)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<InboxStatus>> Documents__Uncancel(List<int> ids)
        {
            using var _ = Instrumentation.Block("Repo." + nameof(Documents__Uncancel));

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }));
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Documents__Uncancel)}]";

            // Execute
            using var reader = await cmd.ExecuteReaderAsync();
            return await RepositoryUtilities.LoadInboxStatuses(reader);
        }

        public async Task<List<InboxStatus>> Documents__Preview(int documentId, DateTimeOffset createdAt, DateTimeOffset openedAt)
        {
            using var _ = Instrumentation.Block("Repo." + nameof(Documents__Preview));

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            cmd.Parameters.Add("@DocumentId", documentId);
            cmd.Parameters.Add("@CreatedAt", createdAt);
            cmd.Parameters.Add("@OpenedAt", openedAt);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Documents__Preview)}]";

            // Execute
            using var reader = await cmd.ExecuteReaderAsync();
            return await RepositoryUtilities.LoadInboxStatuses(reader);
        }

        #endregion

        #region Units

        public async Task<SaveResult> Units__Save(List<UnitForSave> entities, bool returnIds, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveResult(returnIds);
            },
            DatabaseName(connString), nameof(Units__Save));

            return result;
        }

        public async Task<DeleteResult> Units__Delete(IEnumerable<int> ids, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteResult();
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

        public async Task<OperationResult> Units__Activate(List<int> ids, bool isActive, int userId)
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
                cmd.Parameters.Add("@UserId", userId);


                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult();
            },
            _dbName, nameof(Units__Activate));

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

        public async Task Users__SaveSettings(string key, string value)
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

                // Execute
                await cmd.ExecuteNonQueryAsync();
            },
            DatabaseName(connString), nameof(Users__SaveSettings));
        }

        public async Task Users__SavePreferredLanguage(string preferredLanguage, CancellationToken cancellation)
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

                // Execute
                await cmd.ExecuteNonQueryAsync(cancellation);
            },
            DatabaseName(connString), nameof(Users__SavePreferredLanguage), cancellation);
        }

        public async Task Users__SavePreferredCalendar(string preferredCalendar, CancellationToken cancellation)
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

                // Execute
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
                await cmd.ExecuteNonQueryAsync();
            },
            DatabaseName(connString), nameof(Users__SetEmailByUserId));
        }

        public async Task<SaveWithImagesResult> Users__Save(List<UserForSave> entities, bool returnIds, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadSaveWithImagesResult(returnIds);
            },
            DatabaseName(connString), nameof(Users__Save));

            return result;
        }

        public async Task<(DeleteWithImagesResult result, IEnumerable<string> emails)> Users__Delete(IEnumerable<int> ids, int userId)
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
                cmd.Parameters.Add("@UserId", userId);

                // Execute
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    result = await reader.LoadDeleteWithImagesResult();

                    // Load the emails of deleted users
                    if (!result.IsError && await reader.NextResultAsync())
                    {
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

        public async Task<OperationResult> Users__Activate(List<int> ids, bool isActive, int userId)
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
                cmd.Parameters.Add("@UserId", userId);


                // Execute
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                result = await reader.LoadOperationResult();
            },
            _dbName, nameof(Users__Activate));

            return result;
        }

        #endregion
    }
}

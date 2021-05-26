using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
    public class ApplicationRepository : RepositoryBase
    {
        private readonly int _tenantId;
        private readonly ICachingShardResolver _shardResolver;
        private readonly ILogger _logger;

        protected override ILogger Logger => _logger;

        public ApplicationRepository(int tenantId, ICachingShardResolver shardResolver, ILogger<ApplicationRepository> logger)
        {
            _tenantId = tenantId;
            _shardResolver = shardResolver;
            _logger = logger;
        }

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

        public async Task<OnConnectResult> OnConnect(string externalUserId, string userEmail, string culture, string neutralCulture, bool setLastActive, CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);

            OnConnectResult result = null;

            await ExponentialBackoff(async () =>
            {
                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                using var conn = new SqlConnection(connString);

                // Command
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(OnConnect)}]";

                // Parameters
                cmd.Parameters.Add("@ExternalUserId", externalUserId);
                cmd.Parameters.Add("@UserEmail", userEmail);
                cmd.Parameters.Add("@Culture", culture);
                cmd.Parameters.Add("@NeutralCulture", neutralCulture);
                cmd.Parameters.Add("@SetLastActive", setLastActive);

                // Execute
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                if (await reader.ReadAsync(cancellation))
                {
                    int i = 0;

                    result = new OnConnectResult
                    {
                        UserId = reader.Int32(i++),
                        // TODO
                    };

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

                trx.Complete();
            },
            DatabaseName(connString), nameof(OnConnect), cancellation);

            return result;
        }

        public async Task<UserSettingsResult> UserSettings__Load(int userId, CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);

            UserSettingsResult result = null;

            await ExponentialBackoff(async () =>
            {
                User user;
                Guid version;
                var customSettings = new List<(string, string)>();

                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
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

                trx.Complete();
            },
            DatabaseName(connString), nameof(UserSettings__Load), cancellation);

            return result;
        }

        public async Task<SettingsResult> Settings__Load(CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);

            SettingsResult result = null;

            await ExponentialBackoff(async () =>
            {
                int? singleBusinessUnitId;
                GeneralSettings gSettings = new();
                FinancialSettings fSettings = new();

                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
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

                trx.Complete();
            },
            DatabaseName(connString), nameof(Settings__Load), cancellation);

            return result;
        }

        public async Task<PermissionsResult> Permissions__Load(int userId, CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);

            PermissionsResult result = null;

            await ExponentialBackoff(async () =>
            {
                Guid version;
                var permissions = new List<AbstractPermission>();
                var reportIds = new List<int>();
                var dashboardIds = new List<int>();

                // Connection
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
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

                trx.Complete();
            },
            DatabaseName(connString), nameof(Permissions__Load), cancellation);

            return result;
        }

        public async Task<DefinitionsResult> Definitions__Load(CancellationToken cancellation)
        {
            var connString = await GetConnectionString(cancellation);

            DefinitionsResult result = null;

            await ExponentialBackoff(async () =>
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
                using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
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

                trx.Complete();
            },
            DatabaseName(connString), nameof(Definitions__Load), cancellation);

            return result;
        }

        #endregion
    }
}

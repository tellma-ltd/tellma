using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.ClientInfo;
using Tellma.Services.Identity;
using Tellma.Services.Sharding;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Threading;
using Tellma.Services.MultiTenancy;
using Tellma.Entities.Descriptors;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Tellma.Data
{
    /// <summary>
    /// A very thin and lightweight layer around the application database (every tenant
    /// has a dedicated application database). It's the entry point of all functionality that requires 
    /// SQL: Tables, Views, Stored Procedures etc.., it contains no logic of its own.
    /// By default it connects to the tenant Id supplied in the headers 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0067:Dispose objects before losing scope", Justification = "To maintain the SESSION_CONTEXT we keep a hold of the SqlConnection object for the lifetime of the repository")]
    public class ApplicationRepository : IDisposable, IRepository
    {
        private readonly IShardResolver _shardResolver;
        private readonly IExternalUserAccessor _externalUserAccessor;
        private readonly IClientInfoAccessor _clientInfoAccessor;
        private readonly IStringLocalizer _localizer;
        private readonly ITenantIdAccessor _tenantIdAccessor;

        private SqlConnection _conn;
        private UserInfo _userInfo;

        private TenantInfo _tenantInfo;
        private Transaction _transactionOverride;

        #region Lifecycle

        public ApplicationRepository(IShardResolver shardResolver, IExternalUserAccessor externalUserAccessor,
            IClientInfoAccessor clientInfoAccessor, IStringLocalizer<Strings> localizer,
            ITenantIdAccessor tenantIdAccessor)
        {
            _shardResolver = shardResolver;
            _externalUserAccessor = externalUserAccessor;
            _clientInfoAccessor = clientInfoAccessor;
            _localizer = localizer;
            _tenantIdAccessor = tenantIdAccessor;
        }

        public void Dispose()
        {
            if (_conn != null)
            {
                _conn.Close();
                _conn.Dispose();
            }
        }

        #endregion

        #region Connection Management

        /// <summary>
        /// By default the <see cref="ApplicationRepository"/> connects to the database corresponding to 
        /// the current tenantId which is retrieved from an injected <see cref="IShardResolver"/>,
        /// this method makes it possible to conncet to a custom connection string instead, 
        /// this is useful when connecting to multiple tenants at the same time to do aggregate reporting for example
        /// </summary>
        public async Task InitConnectionAsync(int databaseId, bool setLastActive, CancellationToken cancellation)
        {
            if (_conn != null)
            {
                throw new InvalidOperationException("The connection is already initialized");
            }

            string connectionString = await _shardResolver.GetConnectionString(databaseId, cancellation);
            _conn = new SqlConnection(connectionString);

            // Open the SQL connection
            await _conn.OpenAsync();

            // Always call OnConnect SP as soon as you create the connection
            var externalUserId = _externalUserAccessor.GetUserId();
            var externalEmail = _externalUserAccessor.GetUserEmail();
            var culture = CultureInfo.CurrentUICulture.Name;
            var neutralCulture = CultureInfo.CurrentUICulture.IsNeutralCulture ? CultureInfo.CurrentUICulture.Name : CultureInfo.CurrentUICulture.Parent.Name;

            (_userInfo, _tenantInfo) = await OnConnect(externalUserId, externalEmail, culture, neutralCulture, setLastActive, cancellation);
        }

        /// <summary>
        /// Initializes the connection if it is not already initialized
        /// </summary>
        /// <returns>The connection string that was initialized</returns>
        private async Task<SqlConnection> GetConnectionAsync(CancellationToken cancellation = default)
        {
            if (_conn == null)
            {
                int databaseId = _tenantIdAccessor.GetTenantId();
                await InitConnectionAsync(databaseId, setLastActive: true, cancellation);
            }

            // Since we opened the connection once, we need to explicitly enlist it in any ambient transaction
            // every time it is requested, otherwise commands will be executed outside the boundaries of the transaction
            _conn.EnlistInTransaction(transactionOverride: _transactionOverride);
            return _conn;
        }

        /// <summary>
        /// Returns the name of the initial catalog from the active connection's connection string
        /// </summary>
        private string InitialCatalog()
        {
            if (_conn == null || _conn.ConnectionString == null)
            {
                return null;
            }

            return new SqlConnectionStringBuilder(_conn.ConnectionString).InitialCatalog;
        }

        /// <summary>
        /// Loads a <see cref="UserInfo"/> object from the database, this occurs once per <see cref="ApplicationRepository"/> 
        /// instance, subsequent calls are satisfied from a scoped cache
        /// </summary>
        public async Task<UserInfo> GetUserInfoAsync(CancellationToken cancellation)
        {
            await GetConnectionAsync(cancellation); // This automatically initializes the user info
            return _userInfo;
        }

        /// <summary>
        /// Loads a <see cref="UserInfo"/> object from the cache, or throws an exception if it's not available
        /// </summary>
        public UserInfo GetUserInfo()
        {
            return _userInfo ?? throw new InvalidOperationException("UserInfo are not initialized, call GetConnectionAsync() first or just use GetUserInfoAsync()");
        }

        /// <summary>
        /// Loads a <see cref="TenantInfo"/> object from the database, this occurs once per <see cref="ApplicationRepository"/> 
        /// instance, subsequent calls are satisfied from a scoped cache
        /// </summary>
        public async Task<TenantInfo> GetTenantInfoAsync(CancellationToken cancellation)
        {
            await GetConnectionAsync(cancellation); // This automatically initializes the tenant info
            return _tenantInfo;
        }

        /// <summary>
        /// Loads a <see cref="TenantInfo"/> object from the cache, or throws an exception if it's not available
        /// </summary>
        public TenantInfo GetTenantInfo()
        {
            return _tenantInfo ?? throw new InvalidOperationException("TenantInfo are not initialized, call GetConnectionAsync() first or just use GetTenantInfoAsync()");
        }

        /// <summary>
        /// Enlists the repository's connection in the provided transaction such that all subsequent commands particupate in it, regardless of the ambient transaction
        /// </summary>
        /// <param name="transaction">The transaction to enlist the connection in</param>
        public void EnlistTransaction(Transaction transaction)
        {
            _transactionOverride = transaction;
        }

        #endregion

        #region Queries

        public Query<Settings> Settings => Query<Settings>();
        public Query<User> Users => Query<User>();
        public Query<Contract> Contracts => Query<Contract>();
        public Query<Resource> Resources => Query<Resource>();
        public Query<Currency> Currencies => Query<Currency>();
        public Query<ExchangeRate> ExchangeRates => Query<ExchangeRate>();

        /// <summary>
        /// Creates and returns a new <see cref="Queries.Query{T}"/>
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="Queries.Query{T}"/></typeparam>
        public Query<T> Query<T>() where T : Entity
        {
            return new Query<T>(Factory);
        }

        /// <summary>
        /// Creates and returns a new <see cref="Queries.AggregateQuery{T}"/>
        /// </summary>
        /// <typeparam name="T">The root type of the <see cref="Queries.AggregateQuery{T}"/></typeparam>
        public AggregateQuery<T> AggregateQuery<T>() where T : Entity
        {
            return new AggregateQuery<T>(Factory);
        }

        private async Task<QueryArguments> Factory(CancellationToken cancellation)
        {
            var conn = await GetConnectionAsync(cancellation);
            var tenantInfo = await GetTenantInfoAsync(cancellation);
            var userInfo = await GetUserInfoAsync(cancellation);
            var userId = userInfo.UserId ?? 0;
            var userToday = _clientInfoAccessor.GetInfo().Today;

            return new QueryArguments(conn, Sources, userId, userToday, _localizer);
        }

        /// <summary>
        /// Returns a function that maps every <see cref="Entity"/> type in <see cref="ApplicationRepository"/> 
        /// to the default SQL query that retrieves it + some optional parameters
        /// </summary>
        private static string Sources(Type t)
        {
            var result = t.Name switch
            {
                nameof(Entities.Settings) => "[dbo].[Settings]",
                nameof(User) => "[map].[Users]()",
                nameof(Contract) => "[map].[Contracts]()",
                nameof(ContractUser) => "[map].[ContractUsers]()",
                nameof(ContractDefinition) => "[map].[ContractDefinitions]()",
                nameof(Agent) => "[map].[Agents]()",
                nameof(Unit) => "[map].[Units]()",
                nameof(Permission) => "[dbo].[Permissions]",
                nameof(RoleMembership) => "[dbo].[RoleMemberships]",
                nameof(Role) => "[dbo].[Roles]",
                nameof(LookupDefinition) => "[map].[LookupDefinitions]()",
                nameof(Lookup) => "[map].[Lookups]()",
                nameof(Currency) => "[map].[Currencies]()",
                nameof(ResourceDefinition) => "[map].[ResourceDefinitions]()",
                nameof(Resource) => "[map].[Resources]()",
                nameof(ResourceUnit) => "[map].[ResourceUnits]()",
                nameof(AccountClassification) => "[map].[AccountClassifications]()",
                nameof(IfrsConcept) => "[map].[IfrsConcepts]()",
                nameof(AccountType) => "[map].[AccountTypes]()",
                nameof(AccountTypeContractDefinition) => "[map].[AccountTypeContractDefinitions]()",
                nameof(AccountTypeNotedContractDefinition) => "[map].[AccountTypeNotedContractDefinitions]()",
                nameof(AccountTypeResourceDefinition) => "[map].[AccountTypeResourceDefinitions]()",
                nameof(Account) => "[map].[Accounts]()",
                nameof(Center) => "[map].[Centers]()",
                nameof(EntryType) => "[map].[EntryTypes]()",
                nameof(DocumentDefinition) => "[map].[DocumentDefinitions]()",
                nameof(Document) => "[map].[Documents]()",
                nameof(LineDefinition) => "[map].[LineDefinitions]()",
                nameof(Line) => "[map].[Lines]()",
                nameof(LineForQuery) => "[map].[Lines]()",
                nameof(Entry) => "[map].[Entries]()",
                nameof(Attachment) => "[map].[Attachments]()",
                nameof(DocumentAssignment) => "[map].[DocumentAssignmentsHistory]()",
                nameof(InboxRecord) => "[map].[Inbox]()",
                nameof(OutboxRecord) => "[map].[Outbox]()",
                nameof(DocumentStateChange) => "[map].[DocumentStatesHistory]()",
                nameof(ReportDefinition) => "[map].[ReportDefinitions]()",
                nameof(ReportParameterDefinition) => "[map].[ReportParameterDefinitions]()",
                nameof(ReportSelectDefinition) => "[map].[ReportSelectDefinitions]()",
                nameof(ReportRowDefinition) => "[map].[ReportRowDefinitions]()",
                nameof(ReportColumnDefinition) => "[map].[ReportColumnDefinitions]()",
                nameof(ReportMeasureDefinition) => "[map].[ReportMeasureDefinitions]()",
                nameof(ExchangeRate) => "[map].[ExchangeRates]()",
                nameof(MarkupTemplate) => "[map].[MarkupTemplates]()",
                // Fact tables
                nameof(RequiredSignature) => "[map].[DocumentsRequiredSignatures](@DocumentIds)",
                nameof(DetailsEntry) => "[map].[DetailsEntries]()",
                nameof(SummaryEntry) => "[map].[SummaryEntries](@FromDate, @ToDate, NULL, NULL, NULL, NULL, NULL, NULL)",
                nameof(VoucherBooklet) => "[dbo].[VoucherBooklets]",
                _ => throw new InvalidOperationException($"The requested type '{t.Name}' is not supported in {nameof(ApplicationRepository)} queries"),
            };
            return result;
        }

        #endregion

        #region Stored Procedures

        private async Task<(UserInfo, TenantInfo)> OnConnect(string externalUserId, string userEmail, string culture, string neutralCulture, bool setLastActive, CancellationToken cancellation)
        {
            UserInfo userInfo = null;
            TenantInfo tenantInfo = null;

            using (SqlCommand cmd = _conn.CreateCommand()) // Use the private field _conn to avoid infinite recursion
            {
                // Parameters
                cmd.Parameters.Add("@ExternalUserId", externalUserId);
                cmd.Parameters.Add("@UserEmail", userEmail);
                cmd.Parameters.Add("@Culture", culture);
                cmd.Parameters.Add("@NeutralCulture", neutralCulture);
                cmd.Parameters.Add("@SetLastActive", setLastActive);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(OnConnect)}]";

                // Execute and Load
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                if (await reader.ReadAsync(cancellation))
                {
                    int i = 0;

                    // The user Info
                    userInfo = new UserInfo
                    {
                        UserId = reader.Int32(i++),
                        Name = reader.String(i++),
                        Name2 = reader.String(i++),
                        Name3 = reader.String(i++),
                        ExternalId = reader.String(i++),
                        Email = reader.String(i++),
                        PermissionsVersion = reader.Guid(i++)?.ToString(),
                        UserSettingsVersion = reader.Guid(i++)?.ToString(),
                    };

                    // The tenant Info
                    tenantInfo = new TenantInfo
                    {
                        ShortCompanyName = reader.String(i++),
                        ShortCompanyName2 = reader.String(i++),
                        ShortCompanyName3 = reader.String(i++),
                        DefinitionsVersion = reader.Guid(i++)?.ToString(),
                        SettingsVersion = reader.Guid(i++)?.ToString(),
                        PrimaryLanguageId = reader.String(i++),
                        PrimaryLanguageSymbol = reader.String(i++),
                        SecondaryLanguageId = reader.String(i++),
                        SecondaryLanguageSymbol = reader.String(i++),
                        TernaryLanguageId = reader.String(i++),
                        TernaryLanguageSymbol = reader.String(i++)
                    };
                }
                else
                {
                    throw new InvalidOperationException($"[dal].[OnConnect] did not return any data, InitialCatalog: {InitialCatalog()}, ExternalUserId: {externalUserId}, UserEmail: {userEmail}");
                }
            }

            return (userInfo, tenantInfo);
        }

        public async Task<IEnumerable<AbstractPermission>> Action_View__Permissions(string action, string view, CancellationToken cancellation)
        {
            var result = new List<AbstractPermission>();

            var conn = await GetConnectionAsync(cancellation);
            using (SqlCommand cmd = conn.CreateCommand())
            {
                // Parameters
                cmd.Parameters.Add("@Action", action);
                cmd.Parameters.Add("@View", view);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Action_View__Permissions)}]";

                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    result.Add(new AbstractPermission
                    {
                        View = reader.GetString(i++),
                        Action = reader.GetString(i++),
                        Criteria = reader.String(i++),
                        Mask = reader.String(i++)
                    });
                }
            }

            return result;
        }

        public async Task<IEnumerable<AbstractPermission>> Action_ViewPrefix__Permissions(string action, string viewPrefix, CancellationToken cancellation)
        {
            var result = new List<AbstractPermission>();

            var conn = await GetConnectionAsync(cancellation);
            using (SqlCommand cmd = conn.CreateCommand())
            {
                // Parameters
                cmd.Parameters.Add("@Action", action);
                cmd.Parameters.Add("@ViewPrefix", viewPrefix);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Action_ViewPrefix__Permissions)}]";

                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    result.Add(new AbstractPermission
                    {
                        View = reader.GetString(i++),
                        Action = reader.GetString(i++),
                        Criteria = reader.String(i++),
                        Mask = reader.String(i++)
                    });
                }
            }

            return result;
        }

        public async Task<List<InboxNotificationInfo>> InboxCounts__Load(IEnumerable<int> userIds, CancellationToken cancellation)
        {
            var result = new List<InboxNotificationInfo>(userIds.Count());
            if (userIds == null || !userIds.Any())
            {
                return result;
            }

            var conn = await GetConnectionAsync(cancellation);
            using (var cmd = conn.CreateCommand())
            {
                DataTable idsTable = RepositoryUtilities.DataTable(userIds.Select(id => new IdListItem { Id = id }));
                var idsTvp = new SqlParameter("@UserIds", idsTable)
                {
                    TypeName = $"[dbo].[IdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(InboxCounts__Load)}]";

                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    string externalId = reader.GetString(i++);
                    int count = reader.GetInt32(i++);
                    int unknownCount = reader.GetInt32(i++);

                    result.Add(new InboxNotificationInfo
                    {
                        ExternalId = externalId,
                        Count = count,
                        UnknownCount = unknownCount
                    });
                }
            }

            return result;
        }

        public async Task<(Guid, User, IEnumerable<(string Key, string Value)>)> UserSettings__Load(CancellationToken cancellation)
        {
            Guid version;
            var user = new User();
            var customSettings = new List<(string, string)>();

            var conn = await GetConnectionAsync(cancellation);
            using (var cmd = conn.CreateCommand())
            {
                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(UserSettings__Load)}]";

                // Execute
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                // User Settings
                if (await reader.ReadAsync(cancellation))
                {
                    int i = 0;

                    user.Id = reader.GetInt32(i++);
                    user.Name = reader.String(i++);
                    user.Name2 = reader.String(i++);
                    user.Name3 = reader.String(i++);
                    user.ImageId = reader.String(i++);
                    user.PreferredLanguage = reader.String(i++);

                    version = reader.GetGuid(i++);
                }
                else
                {
                    // Developer mistake
                    throw new InvalidOperationException("No settings for client were found");
                }

                // Custom settings
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    string key = reader.GetString(0);
                    string val = reader.GetString(1);

                    customSettings.Add((key, val));
                }
            }

            return (version, user, customSettings);
        }

        public async Task<(bool isMultiSegment, Settings settings)> Settings__Load(CancellationToken cancellation)
        {
            // Returns 
            // (1) whether active leaf centers are multiple or single
            // (2) the settings with the functional currency expanded

            bool isMultiSegment = false;
            Settings settings = new Settings();

            var conn = await GetConnectionAsync(cancellation);
            using (SqlCommand cmd = conn.CreateCommand())
            {
                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Settings__Load)}]";

                // Execute
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                // Load the version
                if (await reader.ReadAsync(cancellation))
                {
                    isMultiSegment = reader.GetBoolean(0);
                }
                else
                {
                    // Programmer mistake
                    throw new Exception($"IsMultiResonsibilityCenter was not returned from SP {nameof(Settings__Load)}");
                }

                // Next load settings
                await reader.NextResultAsync(cancellation);

                if (await reader.ReadAsync(cancellation))
                {
                    var props = TypeDescriptor.Get<Settings>().SimpleProperties;
                    foreach (var prop in props)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(settings, propValue);
                    }
                }
                else
                {
                    // Programmer mistake
                    throw new Exception($"Settings was not returned from SP {nameof(Settings__Load)}");
                }

                // Next load functional currency
                await reader.NextResultAsync(cancellation);

                if (await reader.ReadAsync(cancellation))
                {
                    settings.FunctionalCurrency = new Currency();
                    var props = TypeDescriptor.Get<Currency>().SimpleProperties;
                    foreach (var prop in props)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(settings.FunctionalCurrency, propValue);
                    }
                }
                else
                {
                    // Programmer mistake
                    throw new Exception($"The Functional Currency was not returned from SP {nameof(Settings__Load)}");
                }
            }

            return (isMultiSegment, settings);
        }

        public async Task<(Guid, IEnumerable<AbstractPermission>)> Permissions__Load(CancellationToken cancellation)
        {
            Guid version;
            var permissions = new List<AbstractPermission>();

            var conn = await GetConnectionAsync(cancellation);
            using (SqlCommand cmd = conn.CreateCommand())
            {
                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Permissions__Load)}]";

                // Execute
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                // Load the version
                if (await reader.ReadAsync(cancellation))
                {
                    version = reader.GetGuid(0);
                }
                else
                {
                    version = Guid.Empty;
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
            }

            return (version, permissions);
        }

        public async Task<(Guid,
            IEnumerable<LookupDefinition>,
            IEnumerable<ContractDefinition>,
            IEnumerable<ResourceDefinition>,
            IEnumerable<ReportDefinition>,
            IEnumerable<DocumentDefinition>,
            IEnumerable<LineDefinition>)>
            Definitions__Load(CancellationToken cancellation)
        {
            Guid version;
            var lookupDefinitions = new List<LookupDefinition>();
            var contractDefinitions = new List<ContractDefinition>();
            var resourceDefinitions = new List<ResourceDefinition>();
            var reportDefinitions = new List<ReportDefinition>();
            var documentDefinitions = new List<DocumentDefinition>();
            var lineDefinitions = new List<LineDefinition>();

            var conn = await GetConnectionAsync(cancellation);
            using (SqlCommand cmd = conn.CreateCommand())
            {
                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Definitions__Load)}]";

                // Execute
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                // Load the version
                if (await reader.ReadAsync(cancellation))
                {
                    version = reader.GetGuid(0);
                }
                else
                {
                    version = Guid.Empty;
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
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entity, propValue);
                    }

                    lookupDefinitions.Add(entity);
                }

                // Next load contract definitions
                var contractDefinitionProps = TypeDescriptor.Get<ContractDefinition>().SimpleProperties;

                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new ContractDefinition();
                    foreach (var prop in contractDefinitionProps)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entity, propValue);
                    }

                    contractDefinitions.Add(entity);
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
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entity, propValue);
                    }

                    resourceDefinitions.Add(entity);
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
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entity, propValue);
                    }

                    reportDefinitionsDic[entity.Id] = entity;
                }

                // Parameters
                var reportParameterDefinitionProps = TypeDescriptor.Get<ReportParameterDefinition>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new ReportParameterDefinition();
                    foreach (var prop in reportParameterDefinitionProps)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entity, propValue);
                    }

                    var reportDefinition = reportDefinitionsDic[entity.ReportDefinitionId.Value];
                    reportDefinition.Parameters ??= new List<ReportParameterDefinition>();
                    reportDefinition.Parameters.Add(entity);
                }

                // Select
                var reportSelectDefinitionProps = TypeDescriptor.Get<ReportSelectDefinition>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new ReportSelectDefinition();
                    foreach (var prop in reportSelectDefinitionProps)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entity, propValue);
                    }

                    var reportDefinition = reportDefinitionsDic[entity.ReportDefinitionId.Value];
                    reportDefinition.Select ??= new List<ReportSelectDefinition>();
                    reportDefinition.Select.Add(entity);
                }

                // Rows
                var reportRowDefinitionProps = TypeDescriptor.Get<ReportRowDefinition>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new ReportRowDefinition();
                    foreach (var prop in reportRowDefinitionProps)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entity, propValue);
                    }

                    var reportDefinition = reportDefinitionsDic[entity.ReportDefinitionId.Value];
                    reportDefinition.Rows ??= new List<ReportRowDefinition>();
                    reportDefinition.Rows.Add(entity);
                }

                // Columns
                var reportColumnDefinitionProps = TypeDescriptor.Get<ReportColumnDefinition>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new ReportColumnDefinition();
                    foreach (var prop in reportColumnDefinitionProps)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entity, propValue);
                    }

                    var reportDefinition = reportDefinitionsDic[entity.ReportDefinitionId.Value];
                    reportDefinition.Columns ??= new List<ReportColumnDefinition>();
                    reportDefinition.Columns.Add(entity);
                }

                // Measures
                var reportMeasureDefinitionProps = TypeDescriptor.Get<ReportMeasureDefinition>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new ReportMeasureDefinition();
                    foreach (var prop in reportMeasureDefinitionProps)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entity, propValue);
                    }

                    var reportDefinition = reportDefinitionsDic[entity.ReportDefinitionId.Value];
                    reportDefinition.Measures ??= new List<ReportMeasureDefinition>();
                    reportDefinition.Measures.Add(entity);
                }

                reportDefinitions = reportDefinitionsDic.Values.ToList();

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
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

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
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entity, propValue);
                    }

                    var documentDefinition = documentDefinitionsDic[entity.DocumentDefinitionId.Value];
                    documentDefinition.LineDefinitions ??= new List<DocumentDefinitionLineDefinition>();
                    documentDefinition.LineDefinitions.Add(entity);
                }

                // Load the markup templates
                var markupTemplates = new Dictionary<int, MarkupTemplate>();
                var markupTemplateProps = TypeDescriptor.Get<MarkupTemplate>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new MarkupTemplate();
                    foreach (var prop in markupTemplateProps)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entity, propValue);
                    }

                    markupTemplates.Add(entity.Id, entity);
                }

                // Document Definitions Markup Templates
                var documentDefinitionMarkupTemplateProps = TypeDescriptor.Get<DocumentDefinitionMarkupTemplate>().SimpleProperties;
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new DocumentDefinitionMarkupTemplate();
                    foreach (var prop in documentDefinitionMarkupTemplateProps)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entity, propValue);
                    }

                    // Link with the markup template
                    entity.MarkupTemplate = markupTemplates[entity.MarkupTemplateId.Value];

                    var documentDefinition = documentDefinitionsDic[entity.DocumentDefinitionId.Value];
                    documentDefinition.MarkupTemplates ??= new List<DocumentDefinitionMarkupTemplate>();
                    documentDefinition.MarkupTemplates.Add(entity);
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
                var lineDefinitionEntrisDic = new Dictionary<int, LineDefinitionEntry>();
                var lineDefinitionProps = TypeDescriptor.Get<LineDefinition>().SimpleProperties;
                while (await reader.ReadAsync(cancellation))
                {
                    var entity = new LineDefinition();
                    foreach (var prop in lineDefinitionProps)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

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
                        ContractDefinitions = new List<LineDefinitionEntryContractDefinition>(),
                        NotedContractDefinitions = new List<LineDefinitionEntryNotedContractDefinition>(),
                        ResourceDefinitions = new List<LineDefinitionEntryResourceDefinition>(),
                    };

                    foreach (var prop in lineDefinitionEntryProps)
                    {
                        // get property value
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entity, propValue);
                    }

                    if (entity.AccountTypeId != null)
                    {
                        entity.AccountType = accountTypesDic.GetValueOrDefault(entity.AccountTypeId.Value);
                    }

                    var lineDefinition = lineDefinitionsDic[entity.LineDefinitionId.Value];
                    lineDefinition.Entries ??= new List<LineDefinitionEntry>();
                    lineDefinition.Entries.Add(entity);

                    lineDefinitionEntrisDic[entity.Id] = entity;
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
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

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
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

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
                        var propValue = reader[prop.Name];
                        propValue = propValue == DBNull.Value ? null : propValue;

                        prop.SetValue(entity, propValue);
                    }

                    var lineDefinition = lineDefinitionsDic[entity.LineDefinitionId.Value];
                    lineDefinition.GenerateParameters ??= new List<LineDefinitionGenerateParameter>();
                    lineDefinition.GenerateParameters.Add(entity);
                }

                lineDefinitions = lineDefinitionsDic.Values.ToList();

                // Line Definition Entry Contract Definitions
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    var entity = new LineDefinitionEntryContractDefinition
                    {
                        Id = reader.GetInt32(i++),
                        LineDefinitionEntryId = reader.GetInt32(i++),
                        ContractDefinitionId = reader.GetInt32(i++),
                    };

                    var lineDefEntry = lineDefinitionEntrisDic[entity.LineDefinitionEntryId.Value];
                    lineDefEntry.ContractDefinitions.Add(entity);
                }

                // Line Definition Entry Noted Contract Definitions
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    var entity = new LineDefinitionEntryNotedContractDefinition
                    {
                        Id = reader.GetInt32(i++),
                        LineDefinitionEntryId = reader.GetInt32(i++),
                        NotedContractDefinitionId = reader.GetInt32(i++),
                    };

                    var lineDefEntry = lineDefinitionEntrisDic[entity.LineDefinitionEntryId.Value];
                    lineDefEntry.NotedContractDefinitions.Add(entity);
                }

                // Line Definition Entry Resource Definitions
                await reader.NextResultAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;
                    var entity = new LineDefinitionEntryResourceDefinition
                    {
                        Id = reader.GetInt32(i++),
                        LineDefinitionEntryId = reader.GetInt32(i++),
                        ResourceDefinitionId = reader.GetInt32(i++),
                    };

                    var lineDefEntry = lineDefinitionEntrisDic[entity.LineDefinitionEntryId.Value];
                    lineDefEntry.ResourceDefinitions.Add(entity);
                }
            }

            return (version, lookupDefinitions, contractDefinitions, resourceDefinitions, reportDefinitions, documentDefinitions, lineDefinitions);
        }

        #endregion

        #region Units

        public async Task<IEnumerable<ValidationError>> Units_Validate__Save(List<UnitForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(Unit)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Units_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> Units__Save(List<UnitForSave> entities, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(Unit)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Units__Save)}]";

                if (returnIds)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new IndexedId
                        {
                            Index = reader.GetInt32(i++),
                            Id = reader.GetInt32(i++)
                        });
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return ordered result
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task Units__Activate(List<int> ids, bool isActive)
        {
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
            cmd.Parameters.Add("@IsActive", isActive);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Units__Activate)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<ValidationError>> Units_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Units_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task Units__Delete(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(Units__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        #endregion

        #region Contracts

        public async Task<IEnumerable<ValidationError>> Contracts_Validate__Save(int definitionId, List<ContractForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(Contract)}List]",
                SqlDbType = SqlDbType.Structured
            };

            DataTable usersTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Users);
            var usersTvp = new SqlParameter("@ContractUsers", usersTable)
            {
                TypeName = $"[dbo].[{nameof(ContractUser)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add("@DefinitionId", definitionId);
            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add(usersTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Contracts_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> Contracts__Save(int definitionId, List<ContractForSave> entities, IEnumerable<IndexedImageId> imageIds, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(Contract)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable usersTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Users);
                var usersTvp = new SqlParameter("@ContractUsers", usersTable)
                {
                    TypeName = $"[dbo].[{nameof(ContractUser)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable imageIdsTable = RepositoryUtilities.DataTable(imageIds);
                var imageIdsTvp = new SqlParameter("@ImageIds", imageIdsTable)
                {
                    TypeName = $"[dbo].[IndexedImageIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add(usersTvp);
                cmd.Parameters.Add(imageIdsTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Contracts__Save)}]";

                // Execute
                if (returnIds)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new IndexedId
                        {
                            Index = reader.GetInt32(i++),
                            Id = reader.GetInt32(i++)
                        });
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return ordered result
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task Contracts__Delete(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(Contracts__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        public async Task<IEnumerable<ValidationError>> Contracts_Validate__Delete(int definitionId, List<int> ids, int top)
        {
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
            cmd.CommandText = $"[bll].[{nameof(Contracts_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task Contracts__Activate(List<int> ids, bool isActive)
        {
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
            cmd.Parameters.Add("@IsActive", isActive);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Contracts__Activate)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        #endregion

        #region Agents

        public async Task<IEnumerable<ValidationError>> Agents_Validate__Save(List<AgentForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(Agent)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Agents_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> Agents__Save(List<AgentForSave> entities, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(Agent)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Agents__Save)}]";

                if (returnIds)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new IndexedId
                        {
                            Index = reader.GetInt32(i++),
                            Id = reader.GetInt32(i++)
                        });
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return ordered result
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task Agents__Activate(List<int> ids, bool isActive)
        {
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
            cmd.Parameters.Add("@IsActive", isActive);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Agents__Activate)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<ValidationError>> Agents_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Agents_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task Agents__Delete(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(Agents__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        #endregion

        #region Users

        public async Task Users__SaveSettings(string key, string value)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            cmd.Parameters.Add("Key", key);
            cmd.Parameters.Add("Value", value);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Users__SaveSettings)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<ValidationError>> Users_Validate__Save(List<UserForSave> entities, int top)
        {
            entities.ForEach(e =>
            {
                e.Roles?.ForEach(r =>
                {
                    r.UserId = e.Id;
                });
            });

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[UserList]",
                SqlDbType = SqlDbType.Structured
            };

            DataTable rolesTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Roles);
            var rolesTvp = new SqlParameter("@Roles", rolesTable)
            {
                TypeName = $"[dbo].[{nameof(RoleMembership)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add(rolesTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Users_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> Users__Save(List<UserForSave> entities, IEnumerable<IndexedImageId> imageIds, bool returnIds)
        {
            entities.ForEach(e =>
            {
                e.Roles?.ForEach(r =>
                {
                    r.UserId = e.Id;
                });
            });

            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(User)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable imageIdsTable = RepositoryUtilities.DataTable(imageIds);
                var imageIdsTvp = new SqlParameter("@ImageIds", imageIdsTable)
                {
                    TypeName = $"[dbo].[IndexedImageIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable rolesTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Roles);
                var rolesTvp = new SqlParameter("@Roles", rolesTable)
                {
                    TypeName = $"[dbo].[{nameof(RoleMembership)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add(imageIdsTvp);
                cmd.Parameters.Add(rolesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Users__Save)}]";

                // Execute
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    int i = 0;
                    result.Add(new IndexedId
                    {
                        Index = reader.GetInt32(i++),
                        Id = reader.GetInt32(i++)
                    });
                }
            }

            // Return ordered result
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task<IEnumerable<ValidationError>> Users_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Users_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<IEnumerable<string>> Users__Delete(IEnumerable<int> ids)
        {
            var deletedEmails = new List<string>(); // the result

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
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
                cmd.CommandText = $"[dal].[{nameof(Users__Delete)}]";

                // Execute
                try
                {
                    // Execute
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        deletedEmails.Add(reader.GetString(0));
                    }
                }
                catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
                {
                    throw new ForeignKeyViolationException();
                }
            }

            return deletedEmails;
        }

        public async Task Users__Activate(List<int> ids, bool isActive)
        {
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
            cmd.Parameters.Add("@IsActive", isActive);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Users__Activate)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Users__SetExternalIdByUserId(int userId, string externalId)
        {
            // Finds the user with the given id and sets its ExternalId to the one supplied only if it's null

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            cmd.Parameters.Add("UserId", userId);
            cmd.Parameters.Add("ExternalId", externalId);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Users__SetExternalIdByUserId)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Users__SetEmailByUserId(int userId, string externalEmail)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            cmd.Parameters.Add("UserId", userId);
            cmd.Parameters.Add("ExternalEmail", externalEmail);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Users__SetEmailByUserId)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        #endregion

        #region Roles

        public async Task<List<int>> Roles__Save(List<RoleForSave> entities, bool returnIds)
        {
            entities.ForEach(e =>
            {
                e.Members?.ForEach(m =>
                {
                    m.RoleId = e.Id;
                });
            });

            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
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

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Roles__Save)}]";

                // Execute
                if (returnIds)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new IndexedId
                        {
                            Index = reader.GetInt32(i++),
                            Id = reader.GetInt32(i++)
                        });
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return ordered result
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task<IEnumerable<ValidationError>> Roles_Validate__Save(List<RoleForSave> entities, int top)
        {
            entities.ForEach(e =>
            {
                e.Members?.ForEach(m =>
                {
                    m.RoleId = e.Id;
                });
            });

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
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
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Roles_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task Roles__Delete(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(Roles__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        public async Task<IEnumerable<ValidationError>> Roles_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Roles_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task Roles__Activate(List<int> ids, bool isActive)
        {
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
            cmd.Parameters.Add("@IsActive", isActive);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Roles__Activate)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        #endregion

        #region Blobs

        public async Task Blobs__Delete(IEnumerable<string> blobNames)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable namesTable = RepositoryUtilities.DataTable(blobNames.Select(id => new StringListItem { Id = id }));
            var namesTvp = new SqlParameter("@BlobNames", namesTable)
            {
                TypeName = $"[dbo].[StringList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(namesTvp);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Blobs__Delete)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Blobs__Save(string name, byte[] blob)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            cmd.Parameters.Add("@Name", name);
            cmd.Parameters.Add("@Blob", blob);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Blobs__Save)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<byte[]> Blobs__Get(string name, CancellationToken cancellation)
        {
            byte[] result = null;

            var conn = await GetConnectionAsync(cancellation);
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                cmd.Parameters.Add("@Name", name);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Blobs__Get)}]";

                // Execute
                using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellation);
                if (await reader.ReadAsync(cancellation))
                {
                    result = (byte[])reader[0];
                }
            }

            return result;
        }

        #endregion

        #region Settings

        public async Task Settings__Save(SettingsForSave settingsForSave)
        {
            if (settingsForSave is null)
            {
                return;
            }

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Arguments
            var mappedProps = TypeDescriptor.Get<SettingsForSave>().SimpleProperties;

            var sqlBuilder = new System.Text.StringBuilder();
            sqlBuilder.AppendLine("UPDATE [dbo].[Settings] SET");

            foreach (var prop in mappedProps)
            {
                var propName = prop.Name;
                var key = $"@{propName}";
                var value = prop.GetValue(settingsForSave);

                cmd.Parameters.Add(key, value);
                sqlBuilder.AppendLine($"{propName} = {key},");
            }

            sqlBuilder.AppendLine($"ModifiedAt = SYSDATETIMEOFFSET(),");
            sqlBuilder.AppendLine($"ModifiedById = CONVERT(INT, SESSION_CONTEXT(N'UserId')),");
            sqlBuilder.AppendLine($"SettingsVersion = NEWID()");

            // Command
            cmd.CommandText = sqlBuilder.ToString();

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        #endregion

        #region Lookups

        public async Task<IEnumerable<ValidationError>> Lookups_Validate__Save(int definitionId, List<LookupForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(Lookup)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add("@DefinitionId", definitionId);
            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Lookups_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> Lookups__Save(int definitionId, List<LookupForSave> entities, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(Lookup)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Lookups__Save)}]";

                if (returnIds)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new IndexedId
                        {
                            Index = reader.GetInt32(i++),
                            Id = reader.GetInt32(i++)
                        });
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return ordered result
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task Lookups__Activate(List<int> ids, bool isActive)
        {
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
            cmd.Parameters.Add("@IsActive", isActive);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Lookups__Activate)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<ValidationError>> Lookups_Validate__Delete(int definitionId, List<int> ids, int top)
        {
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
            cmd.CommandText = $"[bll].[{nameof(Lookups_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task Lookups__Delete(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(Lookups__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        #endregion

        #region Currencies

        public async Task<IEnumerable<ValidationError>> Currencies_Validate__Save(List<CurrencyForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(Currency)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Currencies_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task Currencies__Save(List<CurrencyForSave> entities)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(Currency)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Currencies__Save)}]";

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Currencies__Activate(List<string> ids, bool isActive)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new StringListItem { Id = id }));
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[StringList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@IsActive", isActive);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Currencies__Activate)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<ValidationError>> Currencies_Validate__Delete(List<string> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new StringListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedStringList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Currencies_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task Currencies__Delete(IEnumerable<string> ids)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new StringListItem { Id = id }));
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[StringList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Currencies__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        #endregion

        #region Resources

        public async Task Resources__Preprocess(int definitionId, List<ResourceForSave> entities)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters

            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(Resource)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add("@DefinitionId", definitionId);
            cmd.Parameters.Add(entitiesTvp);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Resources__Preprocess)}]";

            // Execute
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
        }

        public async Task<IEnumerable<ValidationError>> Resources_Validate__Save(int definitionId, List<ResourceForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(Resource)}List]",
                SqlDbType = SqlDbType.Structured
            };

            DataTable unitsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Units);
            var unitsTvp = new SqlParameter("@ResourceUnits", unitsTable)
            {
                TypeName = $"[dbo].[{nameof(ResourceUnit)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add("@DefinitionId", definitionId);
            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add(unitsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Resources_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> Resources__Save(int definitionId, List<ResourceForSave> entities, IEnumerable<IndexedImageId> imageIds, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(Resource)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable unitsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Units);
                var unitsTvp = new SqlParameter("@ResourceUnits", unitsTable)
                {
                    TypeName = $"[dbo].[{nameof(ResourceUnit)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable imageIdsTable = RepositoryUtilities.DataTable(imageIds);
                var imageIdsTvp = new SqlParameter("@ImageIds", imageIdsTable)
                {
                    TypeName = $"[dbo].[IndexedImageIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add(unitsTvp);
                cmd.Parameters.Add(imageIdsTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Resources__Save)}]";

                if (returnIds)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new IndexedId
                        {
                            Index = reader.GetInt32(i++),
                            Id = reader.GetInt32(i++)
                        });
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return ordered result
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task Resources__Activate(List<int> ids, bool isActive)
        {
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
            cmd.Parameters.Add("@IsActive", isActive);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Resources__Activate)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<ValidationError>> Resources_Validate__Delete(int definitionId, List<int> ids, int top)
        {
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
            cmd.CommandText = $"[bll].[{nameof(Resources_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task Resources__Delete(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(Resources__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        #endregion

        #region AccountClassifications

        public async Task<IEnumerable<ValidationError>> AccountClassifications_Validate__Save(List<AccountClassificationForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTableWithParentIndex(entities, e => e.ParentIndex);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(AccountClassification)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(AccountClassifications_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> AccountClassifications__Save(List<AccountClassificationForSave> entities, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                DataTable entitiesTable = RepositoryUtilities.DataTableWithParentIndex(entities, e => e.ParentIndex);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(AccountClassification)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(AccountClassifications__Save)}]";

                if (returnIds)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new IndexedId
                        {
                            Index = reader.GetInt32(i++),
                            Id = reader.GetInt32(i++)
                        });
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return ordered result
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task AccountClassifications__Activate(List<int> ids, bool isActive)
        {
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
            cmd.Parameters.Add("@IsActive", isActive);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(AccountClassifications__Activate)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<ValidationError>> AccountClassifications_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(AccountClassifications_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task AccountClassifications__Delete(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(AccountClassifications__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        public async Task<IEnumerable<ValidationError>> AccountClassifications_Validate__DeleteWithDescendants(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(AccountClassifications_Validate__DeleteWithDescendants)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task AccountClassifications__DeleteWithDescendants(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(AccountClassifications__DeleteWithDescendants)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        #endregion

        #region AccountTypes

        public async Task<IEnumerable<ValidationError>> AccountTypes_Validate__Save(List<AccountTypeForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTableWithParentIndex(entities, e => e.ParentIndex);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(AccountType)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(AccountTypes_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> AccountTypes__Save(List<AccountTypeForSave> entities, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                DataTable entitiesTable = RepositoryUtilities.DataTableWithParentIndex(entities, e => e.ParentIndex);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(AccountType)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(AccountTypes__Save)}]";

                if (returnIds)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new IndexedId
                        {
                            Index = reader.GetInt32(i++),
                            Id = reader.GetInt32(i++)
                        });
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return ordered result
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task AccountTypes__Activate(List<int> ids, bool isActive)
        {
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
            cmd.Parameters.Add("@IsActive", isActive);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(AccountTypes__Activate)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<ValidationError>> AccountTypes_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(AccountTypes_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task AccountTypes__Delete(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(AccountTypes__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        public async Task<IEnumerable<ValidationError>> AccountTypes_Validate__DeleteWithDescendants(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(AccountTypes_Validate__DeleteWithDescendants)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task AccountTypes__DeleteWithDescendants(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(AccountTypes__DeleteWithDescendants)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        #endregion

        #region Accounts

        public async Task Accounts__Preprocess(List<AccountForSave> entities)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(Account)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Accounts__Preprocess)}]";

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
        }

        public async Task<IEnumerable<ValidationError>> Accounts_Validate__Save(List<AccountForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(Account)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Accounts_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> Accounts__Save(List<AccountForSave> entities, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(Account)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Accounts__Save)}]";

                if (returnIds)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new IndexedId
                        {
                            Index = reader.GetInt32(i++),
                            Id = reader.GetInt32(i++)
                        });
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return ordered result
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task Accounts__Deprecate(List<int> ids, bool isDeprecated)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            var isDeprecatedParam = new SqlParameter("@IsDeprecated", isDeprecated);

            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }));
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@IsDeprecated", isDeprecated);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Accounts__Deprecate)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<ValidationError>> Accounts_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Accounts_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task Accounts__Delete(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(Accounts__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        #endregion

        #region Centers

        public async Task<IEnumerable<ValidationError>> Centers_Validate__Save(List<CenterForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTableWithParentIndex(entities, e => e.ParentIndex);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(Center)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Centers_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> Centers__Save(List<CenterForSave> entities, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                DataTable entitiesTable = RepositoryUtilities.DataTableWithParentIndex(entities, e => e.ParentIndex);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(Center)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Centers__Save)}]";

                if (returnIds)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new IndexedId
                        {
                            Index = reader.GetInt32(i++),
                            Id = reader.GetInt32(i++)
                        });
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return ordered result
            if (returnIds)
            {
                var sortedResult = new int[entities.Count];
                result.ForEach(e =>
                {
                    sortedResult[e.Index] = e.Id;
                });

                return sortedResult.ToList();
            }
            else
            {
                return new List<int>();
            }
        }

        public async Task Centers__Activate(List<int> ids, bool isActive)
        {
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
            cmd.Parameters.Add("@IsActive", isActive);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Centers__Activate)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<ValidationError>> Centers_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Centers_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task Centers__Delete(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(Centers__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        public async Task<IEnumerable<ValidationError>> Centers_Validate__DeleteWithDescendants(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Centers_Validate__DeleteWithDescendants)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task Centers__DeleteWithDescendants(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(Centers__DeleteWithDescendants)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        #endregion

        #region EntryTypes

        public async Task<IEnumerable<ValidationError>> EntryTypes_Validate__Save(List<EntryTypeForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTableWithParentIndex(entities, e => e.ParentIndex);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(EntryType)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(EntryTypes_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> EntryTypes__Save(List<EntryTypeForSave> entities, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                DataTable entitiesTable = RepositoryUtilities.DataTableWithParentIndex(entities, e => e.ParentIndex);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(EntryType)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(EntryTypes__Save)}]";

                if (returnIds)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new IndexedId
                        {
                            Index = reader.GetInt32(i++),
                            Id = reader.GetInt32(i++)
                        });
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return ordered result
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task EntryTypes__Activate(List<int> ids, bool isActive)
        {
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
            cmd.Parameters.Add("@IsActive", isActive);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(EntryTypes__Activate)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<ValidationError>> EntryTypes_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(EntryTypes_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task EntryTypes__Delete(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(EntryTypes__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        public async Task<IEnumerable<ValidationError>> EntryTypes_Validate__DeleteWithDescendants(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(EntryTypes_Validate__DeleteWithDescendants)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task EntryTypes__DeleteWithDescendants(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(EntryTypes__DeleteWithDescendants)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        #endregion

        #region Documents

        public async Task Documents__Preprocess(int definitionId, List<DocumentForSave> docs)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            var (docsTable, linesTable, entriesTable) = RepositoryUtilities.DataTableFromDocuments(docs);

            var docsTvp = new SqlParameter("@Documents", docsTable)
            {
                TypeName = $"[dbo].[{nameof(Document)}List]",
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

            cmd.Parameters.Add("@DefinitionId", definitionId);
            cmd.Parameters.Add(docsTvp);
            cmd.Parameters.Add(linesTvp);
            cmd.Parameters.Add(entriesTvp);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Documents__Preprocess)}]";

            // Execute
            using var reader = await cmd.ExecuteReaderAsync();

            // Documents
            var docProps = TypeDescriptor.Get<DocumentForSave>().SimpleProperties;
            while (await reader.ReadAsync())
            {
                var index = reader.GetInt32(0);

                var doc = docs[index];

                foreach (var prop in docProps)
                {
                    // get property value
                    var propValue = reader[prop.Name];
                    propValue = propValue == DBNull.Value ? null : propValue;

                    prop.SetValue(doc, propValue);
                }
            }

            // Lines
            await reader.NextResultAsync();
            var lineProps = TypeDescriptor.Get<LineForSave>().SimpleProperties;
            while (await reader.ReadAsync())
            {
                var index = reader.GetInt32(0);
                var docIndex = reader.GetInt32(1);

                var line = docs[docIndex].Lines[index];

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

                var entry = docs[docIndex].Lines[lineIndex].Entries[index];

                foreach (var prop in entryProps)
                {
                    // get property value
                    var propValue = reader[prop.Name];
                    propValue = propValue == DBNull.Value ? null : propValue;

                    prop.SetValue(entry, propValue);
                }
            }
        }

        public async Task<IEnumerable<ValidationError>> Documents_Validate__Save(int definitionId, List<DocumentForSave> documents, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            var (docsTable, linesTable, entriesTable) = RepositoryUtilities.DataTableFromDocuments(documents);

            var docsTvp = new SqlParameter("@Documents", docsTable)
            {
                TypeName = $"[dbo].[{nameof(Document)}List]",
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

            cmd.Parameters.Add("@DefinitionId", definitionId);
            cmd.Parameters.Add(docsTvp);
            cmd.Parameters.Add(linesTvp);
            cmd.Parameters.Add(entriesTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Documents_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<(List<InboxNotificationInfo> NotificationInfos, List<string> DeletedFileIds, List<int> Ids)> Documents__SaveAndRefresh(int definitionId, List<DocumentForSave> documents, List<AttachmentWithExtras> attachments, bool returnIds)
        {
            var deletedFileIds = new List<string>();
            var notificationInfos = new List<InboxNotificationInfo>();
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                var (docsTable, linesTable, entriesTable) = RepositoryUtilities.DataTableFromDocuments(documents);

                var docsTvp = new SqlParameter("@Documents", docsTable)
                {
                    TypeName = $"[dbo].[{nameof(Document)}List]",
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

                var attachmentsTable = RepositoryUtilities.DataTable(attachments);
                var attachmentsTvp = new SqlParameter("@Attachments", attachmentsTable)
                {
                    TypeName = $"[dbo].[{nameof(Attachment)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add("@DefinitionId", definitionId);
                cmd.Parameters.Add(docsTvp);
                cmd.Parameters.Add(linesTvp);
                cmd.Parameters.Add(entriesTvp);
                cmd.Parameters.Add(attachmentsTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Documents__SaveAndRefresh)}]";

                // Execute
                using var reader = await cmd.ExecuteReaderAsync();

                // Get the assignments notifications infos
                await RepositoryUtilities.LoadAssignmentNotificationInfos(reader, notificationInfos);

                // Get the deleted file IDs
                await reader.NextResultAsync();
                while (await reader.ReadAsync())
                {
                    deletedFileIds.Add(reader.GetString(0));
                }

                // If requested, get the document Ids too
                if (returnIds)
                {
                    await reader.NextResultAsync();
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new IndexedId
                        {
                            Index = reader.GetInt32(i++),
                            Id = reader.GetInt32(i++)
                        });
                    }
                }
            }

            // Return ordered result
            var sortedResult = new int[documents.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return (notificationInfos, deletedFileIds, sortedResult.ToList());
        }

        public async Task<IEnumerable<ValidationError>> Lines_Validate__Sign(List<int> ids, int? onBehalfOfUserId, string ruleType, int? roleId, short toState, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@OnBehalfOfuserId", onBehalfOfUserId);
            cmd.Parameters.Add("@RuleType", ruleType);
            cmd.Parameters.Add("@RoleId", roleId);
            cmd.Parameters.Add("@ToState", toState);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Lines_Validate__Sign)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<IEnumerable<int>> Lines__SignAndRefresh(IEnumerable<int> ids, short toState, int? reasonId, string reasonDetails, int? onBehalfOfUserId, string ruleType, int? roleId, DateTimeOffset? signedAt, bool returnIds)
        {
            var result = new List<int>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
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

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Lines__SignAndRefresh)}]";

                // Execute                    
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(reader.GetInt32(0));
                }
            }

            return result;
        }

        public async Task<IEnumerable<ValidationError>> LineSignatures_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(LineSignatures_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<IEnumerable<int>> LineSignatures__DeleteAndRefresh(IEnumerable<int> ids, bool returnIds)
        {
            var result = new List<int>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }));
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(LineSignatures__DeleteAndRefresh)}]";

                // Execute     
                if (returnIds)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        result.Add(reader.GetInt32(0));
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return result;
        }

        public async Task<IEnumerable<ValidationError>> Documents_Validate__Assign(IEnumerable<int> ids, int assigneeId, string comment, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

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
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Documents_Validate__Assign)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<InboxNotificationInfo>> Documents__Assign(IEnumerable<int> ids, int assigneeId, string comment, bool recordInHistory)
        {
            var result = new List<InboxNotificationInfo>();

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
            cmd.Parameters.Add("@AssigneeId", assigneeId);
            cmd.Parameters.Add("@Comment", comment);
            cmd.Parameters.Add("@RecordInHistory", recordInHistory);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Documents__Assign)}]";

            // Execute                    
            using var reader = await cmd.ExecuteReaderAsync();
            return await RepositoryUtilities.LoadAssignmentNotificationInfos(reader);
        }

        public async Task<(List<InboxNotificationInfo> NotificationInfos, List<string> DeletedFileIds)> Documents__Delete(IEnumerable<int> ids)
        {
            // Returns the new notifification counts of affected users, and the list of File Ids to be deleted
            var notificationInfos = new List<InboxNotificationInfo>();
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
                await RepositoryUtilities.LoadAssignmentNotificationInfos(reader, notificationInfos);

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

        public async Task<List<InboxNotificationInfo>> Documents__Close(List<int> ids)
        {
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
            return await RepositoryUtilities.LoadAssignmentNotificationInfos(reader);
        }

        public async Task<IEnumerable<ValidationError>> Documents_Validate__Open(int definitionId, List<int> ids, int top)
        {
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

        public async Task<List<InboxNotificationInfo>> Documents__Open(List<int> ids)
        {
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
            return await RepositoryUtilities.LoadAssignmentNotificationInfos(reader);
        }

        public async Task<IEnumerable<ValidationError>> Documents_Validate__Cancel(int definitionId, List<int> ids, int top)
        {
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

        public async Task<List<InboxNotificationInfo>> Documents__Cancel(List<int> ids)
        {
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
            return await RepositoryUtilities.LoadAssignmentNotificationInfos(reader);
        }

        public async Task<IEnumerable<ValidationError>> Documents_Validate__Uncancel(int definitionId, List<int> ids, int top)
        {
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

        public async Task<List<InboxNotificationInfo>> Documents__Uncancel(List<int> ids)
        {
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
            return await RepositoryUtilities.LoadAssignmentNotificationInfos(reader);
        }

        public async Task<List<InboxNotificationInfo>> Documents__Preview(int documentId, DateTimeOffset createdAt, DateTimeOffset openedAt)
        {
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
            return await RepositoryUtilities.LoadAssignmentNotificationInfos(reader);
        }

        #endregion

        #region Lines
        
        public async Task<(
            List<LineForSave> lines,
            List<Account> accounts,
            List<Contract> contracts,
            List<Resource> resources,
            List<EntryType> entryTypes,
            List<Center> centers,
            List<Currency> currencies,
            List<Unit> units
            )> Lines__Generate(int lineDefId, Dictionary<string, string> args, CancellationToken cancellation)
        {
            List<LineForSave> lines = new List<LineForSave>();

            // Prepare SQL command
            var conn = await GetConnectionAsync(cancellation);
            using var cmd = conn.CreateCommand();

            // Add params
            DataTable argsTable = RepositoryUtilities.DataTable(args.Select(e => new GenerateArgument { Key = e.Key, Value = e.Value }));
            var argsTvp = new SqlParameter("@GenerateArguments", argsTable)
            {
                TypeName = $"[dbo].[{nameof(GenerateArgument)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add("@LineDefinitionId", lineDefId);
            cmd.Parameters.Add(argsTvp);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(Lines__Generate)}]";

            // Lines for save
            using var reader = await cmd.ExecuteReaderAsync(cancellation);
            while (await reader.ReadAsync(cancellation))
            {
                int i = 0;
                lines.Add(new LineForSave
                {
                    DefinitionId = reader.Int32(i++),
                    PostingDate = reader.DateTime(i++),
                    Memo = reader.String(i++),

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
                    ResourceId = reader.Int32(i++),
                    ContractId = reader.Int32(i++),
                    EntryTypeId = reader.Int32(i++),
                    NotedContractId = reader.Int32(i++),
                    CenterId = reader.Int32(i++),
                    UnitId = reader.Int32(i++),
                    IsSystem = reader.Boolean(i++) ?? false,
                    Direction = reader.Int16(i++),
                    DueDate = reader.DateTime(i++),
                    MonetaryValue = reader.Decimal(i++),
                    Quantity = reader.Decimal(i++),
                    Value = reader.Decimal(i++) ?? 0m,
                    Time1 = reader.DateTime(i++),
                    Time2 = reader.DateTime(i++),
                    ExternalReference = reader.String(i++),
                    AdditionalReference = reader.String(i++),
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
            var list_Account = new List<Account>();
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
            var list_Currency = new List<Currency>();
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
            var list_Resource = new List<Resource>();
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

            // Contract
            var list_Contract = new List<Contract>();
            await reader.NextResultAsync(cancellation);
            while (await reader.ReadAsync(cancellation))
            {
                int i = 0;
                list_Contract.Add(new Contract
                {
                    Id = reader.GetInt32(i++),
                    Name = reader.String(i++),
                    Name2 = reader.String(i++),
                    Name3 = reader.String(i++),
                    DefinitionId = reader.Int32(i++),

                    EntityMetadata = new EntityMetadata
                    {
                        { nameof(Contract.Name), FieldMetadata.Loaded },
                        { nameof(Contract.Name2), FieldMetadata.Loaded },
                        { nameof(Contract.Name3), FieldMetadata.Loaded },
                        { nameof(Contract.DefinitionId), FieldMetadata.Loaded },
                    }
                });
            }

            // EntryType
            var list_EntryType = new List<EntryType>();
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
            var list_Center = new List<Center>();
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
            var list_Unit = new List<Unit>();
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

            return (lines, list_Account, list_Contract, list_Resource, list_EntryType, list_Center, list_Currency, list_Unit);
        }

        #endregion

        #region ReportDefinitions

        public async Task<IEnumerable<ValidationError>> ReportDefinitions_Validate__Save(List<ReportDefinitionForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(ReportDefinition)}List]",
                SqlDbType = SqlDbType.Structured
            };

            DataTable parametersTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Parameters);
            var parametersTvp = new SqlParameter("@Parameters", parametersTable)
            {
                TypeName = $"[dbo].[{nameof(ReportParameterDefinition)}List]",
                SqlDbType = SqlDbType.Structured
            };

            DataTable selectTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Select);
            var selectTvp = new SqlParameter("@Select", selectTable)
            {
                TypeName = $"[dbo].[{nameof(ReportSelectDefinition)}List]",
                SqlDbType = SqlDbType.Structured
            };

            DataTable rowsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Rows);
            var rowsTvp = new SqlParameter("@Rows", rowsTable)
            {
                TypeName = $"[dbo].[{nameof(ReportDimensionDefinition)}List]",
                SqlDbType = SqlDbType.Structured
            };

            DataTable columnsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Columns);
            var columnsTvp = new SqlParameter("@Columns", columnsTable)
            {
                TypeName = $"[dbo].[{nameof(ReportDimensionDefinition)}List]",
                SqlDbType = SqlDbType.Structured
            };

            DataTable measuresTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Measures);
            var measuresTvp = new SqlParameter("@Measures", measuresTable)
            {
                TypeName = $"[dbo].[{nameof(ReportMeasureDefinition)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add(parametersTvp);
            cmd.Parameters.Add(selectTvp);
            cmd.Parameters.Add(rowsTvp);
            cmd.Parameters.Add(columnsTvp);
            cmd.Parameters.Add(measuresTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(ReportDefinitions_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> ReportDefinitions__Save(List<ReportDefinitionForSave> entities, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(ReportDefinition)}List]",
                SqlDbType = SqlDbType.Structured
            };

            DataTable parametersTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Parameters);
            var parametersTvp = new SqlParameter("@Parameters", parametersTable)
            {
                TypeName = $"[dbo].[{nameof(ReportParameterDefinition)}List]",
                SqlDbType = SqlDbType.Structured
            };

            DataTable selectTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Select);
            var selectTvp = new SqlParameter("@Select", selectTable)
            {
                TypeName = $"[dbo].[{nameof(ReportSelectDefinition)}List]",
                SqlDbType = SqlDbType.Structured
            };

            DataTable rowsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Rows);
            var rowsTvp = new SqlParameter("@Rows", rowsTable)
            {
                TypeName = $"[dbo].[{nameof(ReportDimensionDefinition)}List]",
                SqlDbType = SqlDbType.Structured
            };

            DataTable columnsTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Columns);
            var columnsTvp = new SqlParameter("@Columns", columnsTable)
            {
                TypeName = $"[dbo].[{nameof(ReportDimensionDefinition)}List]",
                SqlDbType = SqlDbType.Structured
            };

            DataTable measuresTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Measures);
            var measuresTvp = new SqlParameter("@Measures", measuresTable)
            {
                TypeName = $"[dbo].[{nameof(ReportMeasureDefinition)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add(parametersTvp);
            cmd.Parameters.Add(selectTvp);
            cmd.Parameters.Add(rowsTvp);
            cmd.Parameters.Add(columnsTvp);
            cmd.Parameters.Add(measuresTvp);
            cmd.Parameters.Add("@ReturnIds", returnIds);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(ReportDefinitions__Save)}]";

            if (returnIds)
            {
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    int i = 0;
                    result.Add(new IndexedId
                    {
                        Index = reader.GetInt32(i++),
                        Id = reader.GetInt32(i++)
                    });
                }
            }
            else
            {
                await cmd.ExecuteNonQueryAsync();
            }

            // Return ordered result
            if (returnIds)
            {
                var sortedResult = new int[entities.Count];
                result.ForEach(e =>
                {
                    sortedResult[e.Index] = e.Id;
                });

                return sortedResult.ToList();
            }
            else
            {
                return new List<int>();
            }
        }

        public async Task<IEnumerable<ValidationError>> ReportDefinitions_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(ReportDefinitions_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task ReportDefinitions__Delete(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(ReportDefinitions__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        #endregion

        #region ExchangeRates

        public async Task<IEnumerable<ValidationError>> ExchangeRates_Validate__Save(List<ExchangeRateForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(ExchangeRate)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(ExchangeRates_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> ExchangeRates__Save(List<ExchangeRateForSave> entities, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(ExchangeRate)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(ExchangeRates__Save)}]";

                if (returnIds)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new IndexedId
                        {
                            Index = reader.GetInt32(i++),
                            Id = reader.GetInt32(i++)
                        });
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return ordered result
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task<IEnumerable<ValidationError>> ExchangeRates_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(ExchangeRates_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task ExchangeRates__Delete(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(ExchangeRates__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        public async Task<decimal?> ConvertToFunctional(DateTime date, string currencyId, decimal amount, CancellationToken cancellation)
        {
            decimal? result = null;
            var conn = await GetConnectionAsync(cancellation);
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                cmd.Parameters.Add("@Date", date);
                cmd.Parameters.Add("@CurrencyId", currencyId);
                cmd.Parameters.Add("@Amount", amount);

                // Output Parameter
                SqlParameter resultParam = new SqlParameter("@Result", SqlDbType.Decimal)
                {
                    Direction = ParameterDirection.ReturnValue
                };

                cmd.Parameters.Add(resultParam);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[wiz].[fn_{nameof(ConvertToFunctional)}]";

                // Execute
                await cmd.ExecuteNonQueryAsync(cancellation);
                var resultObject = cmd.Parameters["@Result"].Value;
                if (resultObject != DBNull.Value)
                {
                    result = (decimal)resultObject;
                }
            }

            return result;
        }

        #endregion

        #region Inbox

        public async Task<List<InboxNotificationInfo>> Inbox__Check(DateTimeOffset now)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            cmd.Parameters.Add("@Now", now);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Inbox__Check)}]";

            // Execute
            using var reader = await cmd.ExecuteReaderAsync();
            return await RepositoryUtilities.LoadAssignmentNotificationInfos(reader);
        }

        #endregion

        #region MarkupTemplates

        public async Task<IEnumerable<ValidationError>> MarkupTemplates_Validate__Save(List<MarkupTemplateForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(MarkupTemplate)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(MarkupTemplates_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> MarkupTemplates__Save(List<MarkupTemplateForSave> entities, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(MarkupTemplate)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(MarkupTemplates__Save)}]";

                if (returnIds)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new IndexedId
                        {
                            Index = reader.GetInt32(i++),
                            Id = reader.GetInt32(i++)
                        });
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return ordered result
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task<IEnumerable<ValidationError>> MarkupTemplates_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(MarkupTemplates_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task MarkupTemplates__Delete(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(MarkupTemplates__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        #endregion

        #region ContractDefinitions

        public async Task<IEnumerable<ValidationError>> ContractDefinitions_Validate__Save(List<ContractDefinitionForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(ContractDefinition)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(ContractDefinitions_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> ContractDefinitions__Save(List<ContractDefinitionForSave> entities, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(ContractDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(ContractDefinitions__Save)}]";

                if (returnIds)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new IndexedId
                        {
                            Index = reader.GetInt32(i++),
                            Id = reader.GetInt32(i++)
                        });
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return ordered result
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task<IEnumerable<ValidationError>> ContractDefinitions_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(ContractDefinitions_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task ContractDefinitions__Delete(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(ContractDefinitions__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        public async Task<IEnumerable<ValidationError>> ContractDefinitions_Validate__UpdateState(List<int> ids, string state, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@State", state);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(ContractDefinitions_Validate__UpdateState)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task ContractDefinitions__UpdateState(List<int> ids, string state)
        {
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
            cmd.Parameters.Add("@State", state);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(ContractDefinitions__UpdateState)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        #endregion

        #region ResourceDefinitions

        public async Task<IEnumerable<ValidationError>> ResourceDefinitions_Validate__Save(List<ResourceDefinitionForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(ResourceDefinition)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(ResourceDefinitions_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> ResourceDefinitions__Save(List<ResourceDefinitionForSave> entities, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(ResourceDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(ResourceDefinitions__Save)}]";

                if (returnIds)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new IndexedId
                        {
                            Index = reader.GetInt32(i++),
                            Id = reader.GetInt32(i++)
                        });
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return ordered result
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task<IEnumerable<ValidationError>> ResourceDefinitions_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(ResourceDefinitions_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task ResourceDefinitions__Delete(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(ResourceDefinitions__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        public async Task<IEnumerable<ValidationError>> ResourceDefinitions_Validate__UpdateState(List<int> ids, string state, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@State", state);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(ResourceDefinitions_Validate__UpdateState)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task ResourceDefinitions__UpdateState(List<int> ids, string state)
        {
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
            cmd.Parameters.Add("@State", state);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(ResourceDefinitions__UpdateState)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        #endregion

        #region LookupDefinitions

        public async Task<IEnumerable<ValidationError>> LookupDefinitions_Validate__Save(List<LookupDefinitionForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[{nameof(LookupDefinition)}List]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(entitiesTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(LookupDefinitions_Validate__Save)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task<List<int>> LookupDefinitions__Save(List<LookupDefinitionForSave> entities, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[{nameof(LookupDefinition)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(LookupDefinitions__Save)}]";

                if (returnIds)
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new IndexedId
                        {
                            Index = reader.GetInt32(i++),
                            Id = reader.GetInt32(i++)
                        });
                    }
                }
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return ordered result
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task<IEnumerable<ValidationError>> LookupDefinitions_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();
            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(LookupDefinitions_Validate__Delete)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task LookupDefinitions__Delete(IEnumerable<int> ids)
        {
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
            cmd.CommandText = $"[dal].[{nameof(LookupDefinitions__Delete)}]";

            // Execute
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
            {
                throw new ForeignKeyViolationException();
            }
        }

        public async Task<IEnumerable<ValidationError>> LookupDefinitions_Validate__UpdateState(List<int> ids, string state, int top)
        {
            var conn = await GetConnectionAsync();
            using var cmd = conn.CreateCommand();

            // Parameters
            DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new IdListItem { Id = id }), addIndex: true);
            var idsTvp = new SqlParameter("@Ids", idsTable)
            {
                TypeName = $"[dbo].[IndexedIdList]",
                SqlDbType = SqlDbType.Structured
            };

            cmd.Parameters.Add(idsTvp);
            cmd.Parameters.Add("@State", state);
            cmd.Parameters.Add("@Top", top);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[bll].[{nameof(LookupDefinitions_Validate__UpdateState)}]";

            // Execute
            return await RepositoryUtilities.LoadErrors(cmd);
        }

        public async Task LookupDefinitions__UpdateState(List<int> ids, string state)
        {
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
            cmd.Parameters.Add("@State", state);

            // Command
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(LookupDefinitions__UpdateState)}]";

            // Execute
            await cmd.ExecuteNonQueryAsync();
        }

        #endregion
    }
}

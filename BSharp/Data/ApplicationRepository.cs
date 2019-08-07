using BSharp.Data.Queries;
using BSharp.EntityModel;
using BSharp.Services.Identity;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Sharding;
using BSharp.Services.Utilities;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data
{
    public interface IRepository
    {
        Query<T> CreateQuery<T>() where T : Entity;
        AggregateQuery<T> CreateAggregateQuery<T>() where T : Entity;
    }

    /// <summary>
    /// A very thin and lightweight layer around the application database (every tenant
    /// has a dedicated application database), it's the entry point of all functionality that requires 
    /// SQL: Tables, Views, Stored Procedures etc.., it contains no logic of its own.
    /// By default it connects to the tenant Id supplied in the headers 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0067:Dispose objects before losing scope", Justification = "To maintain the SESSION_CONTEXT we keep a hold of the SqlConnection object for the lifetime of the repository")]
    public class ApplicationRepository : IDisposable //, IRepository
    {
        private readonly IShardResolver _shardResolver;
        private readonly IExternalUserAccessor _externalUserProvider;
        private readonly IStringLocalizer<ApplicationRepository> _localizer;
        private SqlConnection _conn;
        private UserInfo _userInfo;
        private TenantInfo _tenantInfo;

        #region Lifecycle

        public ApplicationRepository(IShardResolver shardResolver, IExternalUserAccessor externalUserProvider, IStringLocalizer<ApplicationRepository> localizer)
        {
            _shardResolver = shardResolver;
            _externalUserProvider = externalUserProvider;
            _localizer = localizer;
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
        public async Task InitConnectionAsync(string connectionString)
        {
            if (_conn != null)
            {
                throw new InvalidOperationException("The connection is already initialized");
            }

            _conn = new SqlConnection(connectionString);
            _conn.Open();

            // Always call OnConnect SP as soon as you create the connection
            var externalUserId = _externalUserProvider.GetUserId();
            var externalEmail = _externalUserProvider.GetUserEmail();
            var culture = CultureInfo.CurrentUICulture.Name;
            var neutralCulture = CultureInfo.CurrentUICulture.IsNeutralCulture ? CultureInfo.CurrentUICulture.Name : CultureInfo.CurrentUICulture.Parent.Name;

            (_userInfo, _tenantInfo) = await OnConnect(externalUserId, externalEmail, culture, neutralCulture);
        }

        /// <summary>
        /// Initializes the connection if it is not already initialized
        /// </summary>
        /// <returns>The connection string that was initialized</returns>
        private async Task<SqlConnection> ConnectionAsync()
        {
            if (_conn == null)
            {
                string connString = _shardResolver.GetShardConnectionString();
                await InitConnectionAsync(connString);
            }

            return _conn;
        }

        /// <summary>
        /// Returns the name of the initial catalog from the active connection's connection string
        /// </summary>
        /// <returns></returns>
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
        public async Task<UserInfo> GetUserInfoAsync()
        {
            await ConnectionAsync(); // This automatically initializes the user info
            return _userInfo;
        }

        /// <summary>
        /// Loads a <see cref="TenantInfo"/> object from the database, this occurs once per <see cref="ApplicationRepository"/> 
        /// instance, subsequent calls are satisfied from a scoped cache
        /// </summary>
        public async Task<TenantInfo> GetTenantInfoAsync()
        {
            await ConnectionAsync(); // This automatically initializes the tenant info
            return _tenantInfo;
        }

        #endregion

        #region Queries

        /// <summary>
        /// Creates and returns a new <see cref="Query{T}"/>
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="Query{T}"/></typeparam>
        public async Task<Query<T>> QueryAsync<T>() where T : Entity
        {
            var conn = await ConnectionAsync();
            var tenantInfo = await GetTenantInfoAsync();
            var sources = GetSources(tenantInfo);
            var userInfo = await GetUserInfoAsync();
            var userId = userInfo.UserId ?? 0;
            var userTimeZone = TimeZoneInfo.Local; // TODO: Use value from user
            return new Query<T>(conn, sources, _localizer, userId, userTimeZone);
        }

        /// <summary>
        /// Creates and returns a new <see cref="AggregateQuery{T}"/>
        /// </summary>
        /// <typeparam name="T">The root type of the <see cref="AggregateQuery{T}"/></typeparam>
        public async Task<AggregateQuery<T>> AggregateQueryAsync<T>() where T : Entity
        {
            var conn = await ConnectionAsync();
            var tenantInfo = await GetTenantInfoAsync();
            var sources = GetSources(tenantInfo);
            var userInfo = await GetUserInfoAsync();
            var userId = userInfo.UserId ?? 0;
            var userTimeZone = TimeZoneInfo.Local; // TODO: Use value from user
            return new AggregateQuery<T>(conn, sources, _localizer, userId, userTimeZone);
        }

        /// <summary>
        /// Returns a function that maps every <see cref="Entity"/> type in <see cref="ApplicationRepository"/> 
        /// to the default SQL query that retrieves it + some optional parameters
        /// </summary>
        private Func<Type, SqlSource> GetSources(TenantInfo info)
        {
            var lang1 = info.PrimaryLanguageId;
            var lang2 = info.SecondaryLanguageId;
            var lang3 = info.TernaryLanguageId;

            var loc1 = lang1 == null ? null : _localizer.WithCulture(CultureInfo.CreateSpecificCulture(lang1));
            var loc2 = lang2 == null ? null : _localizer.WithCulture(CultureInfo.CreateSpecificCulture(lang2));
            var loc3 = lang3 == null ? null : _localizer.WithCulture(CultureInfo.CreateSpecificCulture(lang3));

            // TODO Do something about SQL injection risk
            string localize1(string s) => loc1 == null ? "NULL" : $"N'{loc1[s]?.ToString().Replace("'", "''")}'";
            string localize2(string s) => loc2 == null ? "NULL" : $"N'{loc2[s]?.ToString().Replace("'", "''")}'";
            string localize3(string s) => loc3 == null ? "NULL" : $"N'{loc3[s]?.ToString().Replace("'", "''")}'";

            string localize(string s) => $"{localize1(s)},  {localize2(s)},  {localize3(s)}";

            return (t) =>
            {
                switch (t.Name)
                {
                    case nameof(User):
                        return new SqlSource("(SELECT *, IIF(ExternalId IS NULL, 'New', 'Confirmed') As [State] FROM [dbo].[LocalUsers])");

                    case nameof(MeasurementUnit):
                        return new SqlSource("(SELECT * FROM [dbo].[MeasurementUnits] WHERE UnitType <> 'Money')");

                    case nameof(Permission):
                        return new SqlSource("[dbo].[Permissions]");

                    case nameof(RoleMembership):
                        return new SqlSource("[dbo].[RoleMemberships]");

                    case nameof(Role):
                        return new SqlSource("[dbo].[Roles]");

                    case nameof(ProductCategory):
                        return new SqlSource(@"(SELECT [Q].*,
    (SELECT COUNT(*) FROM [dbo].[ProductCategories] WHERE [IsActive] = 1 AND [Node].IsDescendantOf([Q].[Node]) = 1) As [ActiveChildCount],
    (SELECT COUNT(*) FROM [dbo].[ProductCategories] WHERE [Node].IsDescendantOf([Q].[Node]) = 1) As [ChildCount]
FROM [dbo].[ProductCategories] As [Q])");

                    case nameof(IfrsNote):
                        return new SqlSource(@"(SELECT 
	[C].*, 
	[N].[Node] As [Node],
	[N].[Level],
	[N].[ParentNode] As [ParentNode],
	[N].[IsAggregate],
	[N].[ForDebit],
	[N].[ForCredit],
	(SELECT COUNT(*) FROM [dbo].[IfrsNotes] As [NI] JOIN [dbo].[IfrsConcepts] As [CI] ON [CI].[Id] = [NI].[Id] WHERE [CI].[IsActive] = 1 AND [NI].[Node].IsDescendantOf([N].[Node]) = 1) As [ActiveChildCount],
	(SELECT COUNT(*) FROM [dbo].[IfrsNotes] As [NI] JOIN [dbo].[IfrsConcepts] As [CI] ON [CI].[Id] = [NI].[Id] WHERE [NI].[Node].IsDescendantOf([N].[Node]) = 1) As [ChildCount],
	(SELECT [Id] FROM [dbo].[IfrsNotes] WHERE [N].[Node].GetAncestor(1) = [Node]) As [ParentId]
FROM [dbo].[IfrsConcepts] As [C] JOIN [dbo].[IfrsNotes] As [N] ON [C].[Id] = [N].[Id])");

                    case nameof(View):
                        var builtInValuesCollection = _builtInViews.Select(e => $"('{e.Id}', {localize(e.Name)})");
                        var builtInValuesString = builtInValuesCollection.Aggregate((s1, s2) => $@"{s1},
{s2}");
                        var viewParameters = new List<SqlParameter>();
                        return new SqlSource($@"(SELECT
 V.[Id], 
 V.Name AS [Name], 
 V.Name2 AS [Name2], 
 V.Name3 AS [Name3], 
 V.[Id] AS [Code], 
 CASE WHEN V.[Id] = 'all' THEN CAST(1 AS BIT) ELSE IsNULL(T.[IsActive], CAST(0 AS BIT)) END AS [IsActive]
FROM 
  (
  VALUES
    {builtInValuesString}
  ) 
AS V ([Id], [Name], [Name2], [Name3])
LEFT JOIN [dbo].[Views] AS T ON V.Id = T.Id)", viewParameters);

                    case nameof(ViewAction):

                        // This takes the original list and transforms it into a friendly format, adding the very common "Read" and "Update" permissions if they are needed
                        int i = 1;
                        var builtInValueActionsCollections = _builtInViews.SelectMany(x =>
                             x.Levels.Select(y => new { Id = i++, ViewId = x.Id, y.Action, SupportsCriteria = y.Criteria, SupportsMask = false })
                            .Concat(Enumerable.Repeat(new { Id = i++, ViewId = x.Id, Action = Constants.Update, SupportsCriteria = true, SupportsMask = true }, x.Update ? 1 : 0))
                            .Concat(Enumerable.Repeat(new { Id = i++, ViewId = x.Id, Action = Constants.Read, SupportsCriteria = true, SupportsMask = true }, x.Read ? 1 : 0))
                        )
                        .Select(e => $"('{e.Id}', '{e.ViewId}', '{e.Action}', {(e.SupportsCriteria ? "1" : "0")}, {(e.SupportsMask ? "1" : "0")})");

                        var builtInValueActionsString = builtInValueActionsCollections.Aggregate((s1, s2) => $@"{s1},
{s2}");

                        return new SqlSource($@"(SELECT
 [V].[Id], 
 [V].[ViewId] AS [ViewId], 
 [V].[Action] AS [Action], 
 CAST(V.[SupportsCriteria] AS BIT) AS [SupportsCriteria], 
 CAST(V.[SupportsMask] AS BIT) AS [SupportsMask]
FROM 
  (
  VALUES
    {builtInValueActionsString}
  ) 
AS [V] ([Id], [ViewId], [Action], [SupportsCriteria], [SupportsMask])
LEFT JOIN [dbo].[Views] AS [T] ON V.Id = T.Id)");

                }

                throw new InvalidOperationException($"The requested type {t.Name} is not supported in {nameof(ApplicationRepository)} queries");
            };
        }

        private static readonly ViewInfo[] _builtInViews = new ViewInfo[]
        {
            new ViewInfo { Id = "all", Name = "View_All", Levels = new LevelInfo[] { Li("Read", false), Li("Update", false) } },
            new ViewInfo { Id = "measurement-units", Name = "MeasurementUnits", Read = true, Update = true , Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "roles", Name = "Roles", Read = true, Update = true, Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "local-users", Name = "Users", Read = true, Update = true, Levels = new LevelInfo[] { Li("IsActive"), Li("ResendInvitationEmail") } },
            new ViewInfo { Id = "views", Name = "Views", Read = true, Update = true, Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "ifrs-notes", Name = "IfrsNotes", Read = true, Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "product-categories", Name = "ProductCategories", Read = true, Update = true, Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "settings", Name = "Settings", Levels = new LevelInfo[] { Li("Read", false), Li("Update", false) } },
        };

        private static LevelInfo Li(string name, bool criteria = true)
        {
            return new LevelInfo { Action = name, Criteria = criteria };
        }

        private class ViewInfo
        {
            public string Id { get; set; }

            public string Name { get; set; }

            /// <summary>
            /// Indicates that this view is an endpoint that supports read level, both with Mask and Criteria: OData style
            /// </summary>
            public bool Read { get; set; }

            /// <summary>
            /// Indicates that this view is an endpoint that supports read level, both with Mask and Criteria: OData style
            /// </summary>
            public bool Update { get; set; }

            public LevelInfo[] Levels { get; set; }
        }

        private class LevelInfo
        {
            public string Action { get; set; }

            public bool Criteria { get; set; }
        }

        #endregion

        #region Stored Procedures

        public async Task<(UserInfo, TenantInfo)> OnConnect(string externalUserId, string userEmail, string culture, string neutralCulture)
        {
            UserInfo userInfo = null;
            TenantInfo tenantInfo = null;

            using (SqlCommand cmd = _conn.CreateCommand())
            {
                // Parameters
                cmd.Parameters.AddWithValue("@ExternalUserId", externalUserId);
                cmd.Parameters.AddWithValue("@UserEmail", userEmail);
                cmd.Parameters.AddWithValue("@Culture", culture);
                cmd.Parameters.AddWithValue("@NeutralCulture", neutralCulture);

                // Command
                cmd.CommandText = @"EXEC [dal].[OnConnect] 
@ExternalUserId = @ExternalUserId, 
@UserEmail      = @UserEmail, 
@Culture        = @Culture, 
@NeutralCulture = @NeutralCulture";

                // Execute and Load
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        int i = 0;

                        // The user Info
                        userInfo = new UserInfo
                        {
                            UserId = reader.IsDBNull(i) ? (int?)null : reader.GetInt32(i++),
                            Name = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            Name2 = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            ExternalId = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            Email = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            PermissionsVersion = reader.IsDBNull(i) ? null : reader.GetGuid(i++).ToString(),
                            UserSettingsVersion = reader.IsDBNull(i) ? null : reader.GetGuid(i++).ToString(),
                        };

                        // The tenant Info
                        tenantInfo = new TenantInfo
                        {
                            ViewsAndSpecsVersion = reader.IsDBNull(i) ? null : reader.GetGuid(i++).ToString(),
                            SettingsVersion = reader.IsDBNull(i) ? null : reader.GetGuid(i++).ToString(),
                            PrimaryLanguageId = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            PrimaryLanguageSymbol = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            SecondaryLanguageId = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            SecondaryLanguageSymbol = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            TernaryLanguageId = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            TernaryLanguageSymbol = reader.IsDBNull(i) ? null : reader.GetString(i++)
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException($"[dal].[OnConnect] did not return any data, InitialCatalog: {InitialCatalog()}, ExternalUserId: {externalUserId}, UserEmail: {userEmail}");
                    }
                }
            }

            return (userInfo, tenantInfo);
        }

        public async Task<IEnumerable<AbstractPermission>> Action_Views__Permissions(string action, IEnumerable<string> viewIds)
        {
            var result = new List<AbstractPermission>();

            using (SqlCommand cmd = _conn.CreateCommand())
            {
                // Parameters
                var viewIdsTable = RepositoryUtilities.DataTable(viewIds.Select(e => new { Code = e }));
                var viewIdsTvp = new SqlParameter("@ViewIds", viewIdsTable)
                {
                    TypeName = $"dbo.StringList",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(viewIdsTvp);
                cmd.Parameters.AddWithValue("@Action", action);

                cmd.CommandText = @"EXEC [dal].[Action_Views__Permissions]
@Action = @Action,
@ViewIds = @ViewIds
";

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new AbstractPermission
                        {
                            ViewId = reader.GetString(i++),
                            Action = reader.GetString(i++),
                            Criteria = reader.GetString(i++),
                            Mask = reader.GetString(i++)
                        });
                    }
                }
            }

            return result;
        }



        public Task SetUserExternalId(int userId, string externalId)
        {
            // Finds the user with the given id and sets its ExternalId to the one supplied only if it's null
            // $"UPDATE [dbo].[Users] SET ExternalId = {externalId} WHERE Id = {userId}";

            throw new NotImplementedException();
        }

        public Task SetUserEmail(int userId, string email)
        {
            // Finds the user with the given id and sets its Email to the one supplied
            throw new NotImplementedException();
        }

        #endregion
    }
}

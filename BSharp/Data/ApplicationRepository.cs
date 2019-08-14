using BSharp.Data.Queries;
using BSharp.EntityModel;
using BSharp.Services.ClientInfo;
using BSharp.Services.Identity;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Sharding;
using BSharp.Services.Utilities;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace BSharp.Data
{
    /// <summary>
    /// A very thin and lightweight layer around the application database (every tenant
    /// has a dedicated application database), it's the entry point of all functionality that requires 
    /// SQL: Tables, Views, Stored Procedures etc.., it contains no logic of its own.
    /// By default it connects to the tenant Id supplied in the headers 
    /// </summary>
    [SuppressMessage("Code Quality", "IDE0067:Dispose objects before losing scope", Justification = "To maintain the SESSION_CONTEXT we keep a hold of the SqlConnection object for the lifetime of the repository")]
    public class ApplicationRepository : IDisposable, IRepository
    {
        private readonly IShardResolver _shardResolver;
        private readonly IExternalUserAccessor _externalUserAccessor;
        private readonly IClientInfoAccessor _clientInfoAccessor;
        private readonly IStringLocalizer _localizer;

        private SqlConnection _conn;
        private UserInfo _userInfo;
        private TenantInfo _tenantInfo;
        private Transaction _transactionOverride;

        #region Lifecycle

        public ApplicationRepository(IShardResolver shardResolver, IExternalUserAccessor externalUserAccessor,
            IClientInfoAccessor clientInfoAccessor, IStringLocalizer<Strings> localizer)
        {
            _shardResolver = shardResolver;
            _externalUserAccessor = externalUserAccessor;
            _clientInfoAccessor = clientInfoAccessor;
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
            var externalUserId = _externalUserAccessor.GetUserId();
            var externalEmail = _externalUserAccessor.GetUserEmail();
            var culture = CultureInfo.CurrentUICulture.Name;
            var neutralCulture = CultureInfo.CurrentUICulture.IsNeutralCulture ? CultureInfo.CurrentUICulture.Name : CultureInfo.CurrentUICulture.Parent.Name;

            (_userInfo, _tenantInfo) = await OnConnect(externalUserId, externalEmail, culture, neutralCulture);
        }

        /// <summary>
        /// Initializes the connection if it is not already initialized
        /// </summary>
        /// <returns>The connection string that was initialized</returns>
        private async Task<SqlConnection> GetConnectionAsync()
        {
            if (_conn == null)
            {
                string connString = _shardResolver.GetConnectionString();
                await InitConnectionAsync(connString);
            }

            // Since we opened the connection once, we need to explicitly enlist it in any ambient transaction
            // every time it is requested, otherwise commands will be executed outside the boundaries of the transaction
            _conn.EnlistInTransaction(transactionOverride: _transactionOverride);
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
            await GetConnectionAsync(); // This automatically initializes the user info
            return _userInfo;
        }

        /// <summary>
        /// Loads a <see cref="TenantInfo"/> object from the database, this occurs once per <see cref="ApplicationRepository"/> 
        /// instance, subsequent calls are satisfied from a scoped cache
        /// </summary>
        public async Task<TenantInfo> GetTenantInfoAsync()
        {
            await GetConnectionAsync(); // This automatically initializes the tenant info
            return _tenantInfo;
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

        public Query<User> Users => Query<User>();


        /// <summary>
        /// Creates and returns a new <see cref="Queries.Query{T}"/>
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="Queries.Query{T}"/></typeparam>
        public Query<T> Query<T>() where T : Entity
        {
            return new Query<T>(GetFactory());
        }

        /// <summary>
        /// Creates and returns a new <see cref="Queries.AggregateQuery{T}"/>
        /// </summary>
        /// <typeparam name="T">The root type of the <see cref="Queries.AggregateQuery{T}"/></typeparam>
        public AggregateQuery<T> AggregateQuery<T>() where T : Entity
        {
            return new AggregateQuery<T>(GetFactory());
        }

        private QueryArgumentsFactory GetFactory()
        {
            async Task<QueryArguments> Factory()
            {
                var conn = await GetConnectionAsync();
                var tenantInfo = await GetTenantInfoAsync();
                var sources = GetSources(tenantInfo, _localizer);
                var userInfo = await GetUserInfoAsync();
                var userId = userInfo.UserId ?? 0;
                var userTimeZone = _clientInfoAccessor.GetInfo().TimeZone;

                return new QueryArguments(conn, sources, userId, userTimeZone, _localizer);
            }

            return Factory;
        }

        /// <summary>
        /// Returns a function that maps every <see cref="Entity"/> type in <see cref="ApplicationRepository"/> 
        /// to the default SQL query that retrieves it + some optional parameters
        /// </summary>
        private static Func<Type, SqlSource> GetSources(TenantInfo info, IStringLocalizer localizer)
        {
            var lang1 = info.PrimaryLanguageId;
            var lang2 = info.SecondaryLanguageId;
            var lang3 = info.TernaryLanguageId;

            var loc1 = lang1 == null ? null : localizer.WithCulture(CultureInfo.CreateSpecificCulture(lang1));
            var loc2 = lang2 == null ? null : localizer.WithCulture(CultureInfo.CreateSpecificCulture(lang2));
            var loc3 = lang3 == null ? null : localizer.WithCulture(CultureInfo.CreateSpecificCulture(lang3));

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
                        return new SqlSource("(SELECT *, IIF(ExternalId IS NULL, 'New', 'Confirmed') As [State] FROM [dbo].[Users])");

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

                        // This takes the original list and transforms it into a friendly format, adding the very common "Read", "Update" and "Delete" permissions if they are needed
                        int i = 1;
                        var builtInValueActionsCollections = _builtInViews.SelectMany(x =>
                             x.Levels.Select(y => new { Id = i++, ViewId = x.Id, y.Action, SupportsCriteria = y.Criteria, SupportsMask = false })
                            .Concat(Enumerable.Repeat(new { Id = i++, ViewId = x.Id, Action = Constants.Delete, SupportsCriteria = true, SupportsMask = false }, x.Delete ? 1 : 0))
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
            new ViewInfo { Id = "all", Name = "View_All", Levels = new LevelInfo[] { Li("Read", false) } },
            new ViewInfo { Id = "measurement-units", Name = "MeasurementUnits", Read = true, Update = true, Delete = true, Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "roles", Name = "Roles", Read = true, Update = true, Delete = true, Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "local-users", Name = "Users", Read = true, Update = true, Delete = true, Levels = new LevelInfo[] { Li("IsActive"), Li("ResendInvitationEmail") } },
            new ViewInfo { Id = "views", Name = "Views", Read = true, Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "ifrs-notes", Name = "IfrsNotes", Read = true, Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "product-categories", Name = "ProductCategories", Read = true, Update = true, Delete = true, Levels = new LevelInfo[] { Li("IsActive") } },
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

            public bool Delete { get; set; }

            public LevelInfo[] Levels { get; set; }
        }

        private class LevelInfo
        {
            public string Action { get; set; }

            public bool Criteria { get; set; }
        }

        #endregion

        #region Stored Procedures

        private async Task<(UserInfo, TenantInfo)> OnConnect(string externalUserId, string userEmail, string culture, string neutralCulture)
        {
            UserInfo userInfo = null;
            TenantInfo tenantInfo = null;

            using (SqlCommand cmd = _conn.CreateCommand()) // Use the private field _conn to avoid infinite recursion
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
                            Name3 = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            ExternalId = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            Email = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            PermissionsVersion = reader.IsDBNull(i) ? null : reader.GetGuid(i++).ToString(),
                            UserSettingsVersion = reader.IsDBNull(i) ? null : reader.GetGuid(i++).ToString(),
                        };

                        // The tenant Info
                        tenantInfo = new TenantInfo
                        {
                            ShortCompanyName = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            ShortCompanyName2 = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            ShortCompanyName3 = reader.IsDBNull(i) ? null : reader.GetString(i++),
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

        public async Task<IEnumerable<AbstractPermission>> Action_Views__Permissions(string action, IEnumerable<string> viewIds)
        {
            var result = new List<AbstractPermission>();

            var conn = await GetConnectionAsync();
            using (SqlCommand cmd = conn.CreateCommand())
            {
                // Parameters
                var viewIdsTable = RepositoryUtilities.DataTable(viewIds.Select(e => new StringListItem { Id = e }));
                var viewIdsTvp = new SqlParameter("@ViewIds", viewIdsTable)
                {
                    TypeName = $"dbo.StringList",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(viewIdsTvp);
                cmd.Parameters.AddWithValue("@Action", action);

                cmd.CommandText = $@"EXEC [dal].[{nameof(Action_Views__Permissions)}]
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

        #endregion

        #region MeasurementUnits

        public Query<MeasurementUnit> MeasurementUnits__AsQuery(List<MeasurementUnitForSave> entities)
        {
            // TODO: Move to a function in the database

            // This method returns the provided entities as a Query that can be selected, filtered etc...
            // The Ids in the result are always the indices of the original collection, even when the entity has a string key

            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            SqlParameter entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[MeasurementUnitList]",
                SqlDbType = SqlDbType.Structured
            };

            string preambleSql =
                $@"DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
DECLARE @True BIT = 1;";

            string sql =
                $@"SELECT [E].[Index] AS [Id], [E].[Name], [E].[Name2], [E].[Name3], [E].[Code], [E].[UnitType], [E].[UnitAmount], [E].[BaseAmount],
@True AS [IsActive], @Now AS [CreatedAt], @UserId AS [CreatedById], @Now AS [ModifiedAt], @UserId AS [ModifiedById] 
FROM @Entities [E]";

            var query = Query<MeasurementUnit>();
            return query.FromSql(sql, preambleSql, entitiesTvp);
        }

        public async Task<IEnumerable<ValidationError>> MeasurementUnits_Validate__Save(List<MeasurementUnitForSave> entities, int top)
        {
            var result = new List<ValidationError>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[MeasurementUnitList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.AddWithValue("@Top", top);

                cmd.CommandText = $@"EXEC [bll].[{nameof(MeasurementUnits_Validate__Save)}]
@Entities = @Entities,
@Top = @Top
";

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new ValidationError
                        {
                            Key = reader.GetString(i++),
                            ErrorName = reader.GetString(i++),
                            Argument1 = reader.GetString(i++),
                            Argument2 = reader.GetString(i++),
                            Argument3 = reader.GetString(i++),
                            Argument4 = reader.GetString(i++),
                            Argument5 = reader.GetString(i++)
                        });
                    }
                }
            }

            return result;
        }

        public async Task<List<int>> MeasurementUnits__Save(List<MeasurementUnitForSave> entities, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[MeasurementUnitList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.AddWithValue("@ReturnIds", returnIds);

                cmd.CommandText = $@"EXEC [dal].[{nameof(MeasurementUnits__Save)}]
@Entities = @Entities,
@ReturnIds = @ReturnIds
";

                if (returnIds)
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
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
                else
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            // Return ordered result
            return result
                .OrderBy(e => e.Index)
                .Select(e => e.Id)
                .ToList();
        }

        public async Task MeasurementUnits__Activate(List<int> ids, bool isActive)
        {
            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                var isActiveParam = new SqlParameter("@IsActive", isActive);

                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new { Id = id }));
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.AddWithValue("@IsActive", isActive);

                cmd.CommandText = $@"EXEC [dal].[{nameof(MeasurementUnits__Activate)}]
@IdList = @IdList,
@IsActive = @IsActive
";

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<ValidationError>> MeasurementUnits_Validate__Delete(List<int> ids, int top)
        {
            var result = new List<ValidationError>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new { Id = id }));
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.AddWithValue("@Top", top);

                // Command
                cmd.CommandText = $@"EXEC [bll].[{nameof(MeasurementUnits_Validate__Delete)}]
@Ids = @Ids,
@Top = @Top
";

                // Execute
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new ValidationError
                        {
                            Key = reader.GetString(i++),
                            ErrorName = reader.GetString(i++),
                            Argument1 = reader.GetString(i++),
                            Argument2 = reader.GetString(i++),
                            Argument3 = reader.GetString(i++),
                            Argument4 = reader.GetString(i++),
                            Argument5 = reader.GetString(i++)
                        });
                    }
                }
            }

            return result;
        }

        public async Task MeasurementUnits__Delete(List<int> ids)
        {
            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new { Id = id }));
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);

                cmd.CommandText = $@"EXEC [dal].[{nameof(MeasurementUnits__Delete)}]
@IdList = @IdList
";
                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
                {
                    throw new ForeignKeyViolationException();
                }
            }
        }

        #endregion

        #region Users

        #endregion
    }
}

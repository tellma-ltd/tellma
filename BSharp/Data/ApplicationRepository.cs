using BSharp.Data.Queries;
using BSharp.Entities;
using BSharp.Services.ClientInfo;
using BSharp.Services.Identity;
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
using System.Transactions;

namespace BSharp.Data
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
            await _conn.OpenAsync();

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
        public async Task<TenantInfo> GetTenantInfoAsync()
        {
            await GetConnectionAsync(); // This automatically initializes the tenant info
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
        public Query<Agent> Agents => Query<Agent>();


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

            var loc1 = lang1 == null ? null : localizer.WithCulture(new CultureInfo(lang1));
            var loc2 = lang2 == null ? null : localizer.WithCulture(new CultureInfo(lang2));
            var loc3 = lang3 == null ? null : localizer.WithCulture(new CultureInfo(lang3));

            // TODO Do something about SQL injection risk
            string localize1(string s) => loc1 == null ? "NULL" : $"N'{loc1[s]?.ToString().Replace("'", "''")}'";
            string localize2(string s) => loc2 == null ? "NULL" : $"N'{loc2[s]?.ToString().Replace("'", "''")}'";
            string localize3(string s) => loc3 == null ? "NULL" : $"N'{loc3[s]?.ToString().Replace("'", "''")}'";

            string localize(string s) => $"{localize1(s)},  {localize2(s)},  {localize3(s)}";

            return (t) =>
            {
                switch (t.Name)
                {
                    case nameof(Settings):
                        return new SqlSource("[dbo].[Settings]");

                    case nameof(User):
                        return new SqlSource("[rpt].[Users]()");

                    case nameof(Agent):
                        return new SqlSource("[rpt].[Agents]()");

                    case nameof(MeasurementUnit):
                        return new SqlSource("[rpt].[MeasurementUnits]()");

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
                        var builtInValuesCollection = Views.BUILT_IN.Select(e => $"('{e.Id}', {localize(e.Name)})");
                        var builtInValuesString = builtInValuesCollection.Aggregate((s1, s2) => $@"{s1},
{s2}");
                        var viewParameters = new List<SqlParameter>();
                        return new SqlSource($@"(SELECT
 V.[Id], 
 V.Name AS [Name], 
 V.Name2 AS [Name2], 
 V.Name3 AS [Name3], 
 V.[Id] AS [Code], 
 CASE WHEN V.[Id] = 'all' THEN CAST(1 AS BIT) ELSE IsNULL(T.[IsActive], CAST(1 AS BIT)) END AS [IsActive]
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
                        var builtInValueActionsCollections = Views.BUILT_IN.SelectMany(x =>
                             x.Levels.Select(y => new { Id = i++, ViewId = x.Id, y.Action, SupportsCriteria = y.Criteria, SupportsMask = false })
                            .Concat(Enumerable.Repeat(new { Id = i++, ViewId = x.Id, Action = Constants.Delete, SupportsCriteria = true, SupportsMask = false }, x.Delete ? 1 : 0))
                            .Concat(Enumerable.Repeat(new { Id = i++, ViewId = x.Id, Action = Constants.Update, SupportsCriteria = true, SupportsMask = true }, x.Update ? 1 : 0))
                            .Concat(Enumerable.Repeat(new { Id = i++, ViewId = x.Id, Action = Constants.Read, SupportsCriteria = true, SupportsMask = true }, x.Read ? 1 : 0))
                        )
                        .Select(e => $"({e.Id}, '{e.ViewId}', '{e.Action}', {(e.SupportsCriteria ? "1" : "0")}, {(e.SupportsMask ? "1" : "0")})");

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

        #endregion

        #region Stored Procedures

        private async Task<(UserInfo, TenantInfo)> OnConnect(string externalUserId, string userEmail, string culture, string neutralCulture)
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

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(OnConnect)}]";

                // Execute and Load
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
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
                            ViewsAndSpecsVersion = reader.Guid(i++)?.ToString(),
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
            }

            return (userInfo, tenantInfo);
        }

        public async Task Users__SetExternalIdByUserId(int userId, string externalId)
        {
            // Finds the user with the given id and sets its ExternalId to the one supplied only if it's null

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                cmd.Parameters.Add("UserId", userId);
                cmd.Parameters.Add("ExternalId", externalId);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Users__SetExternalIdByUserId)}]";

                // Execute
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task Users__SetEmailByUserId(int userId, string externalEmail)
        {
            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                cmd.Parameters.Add("UserId", userId);
                cmd.Parameters.Add("ExternalEmail", externalEmail);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Users__SetEmailByUserId)}]";

                // Execute
                await cmd.ExecuteNonQueryAsync();
            }
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
                    TypeName = $"[dbo].[StringList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(viewIdsTvp);
                cmd.Parameters.Add("@Action", action);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Action_Views__Permissions)}]";

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new AbstractPermission
                        {
                            ViewId = reader.GetString(i++),
                            Action = reader.GetString(i++),
                            Criteria = reader.String(i++),
                            Mask = reader.String(i++)
                        });
                    }
                }
            }

            return result;
        }

        public async Task<IEnumerable<AbstractPermission>> GetUserPermissions()
        {
            var result = new List<AbstractPermission>();

            var conn = await GetConnectionAsync();
            using (SqlCommand cmd = conn.CreateCommand())
            {
                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(GetUserPermissions)}]";

                // Execute
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int i = 0;
                        result.Add(new AbstractPermission
                        {
                            ViewId = reader.String(i++),
                            Action = reader.String(i++),
                            Criteria = reader.String(i++),
                            Mask = reader.String(i++)
                        });
                    }
                }
            }

            return result;
        }

        public async Task<Guid> GetUserPermissionsVersion()
        {
            var conn = await GetConnectionAsync();
            using (SqlCommand cmd = conn.CreateCommand())
            {
                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(GetUserPermissionsVersion)}]";

                // Execute
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return reader.GetGuid(0);
                    }
                    else
                    {
                        return Guid.Empty;
                    }
                }
            }
        }

        #endregion

        #region MeasurementUnits

        public Query<MeasurementUnit> MeasurementUnits__AsQuery(List<MeasurementUnitForSave> entities)
        {
            // This method returns the provided entities as a Query that can be selected, filtered etc...
            // The Ids in the result are always the indices of the original collection, even when the entity has a string key

            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            SqlParameter entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[MeasurementUnitList]",
                SqlDbType = SqlDbType.Structured
            };

            // Query
            var query = Query<MeasurementUnit>();
            return query.FromSql($"[bll].[{nameof(MeasurementUnits__AsQuery)}] (@Entities)", null, entitiesTvp);
        }

        public async Task<IEnumerable<ValidationError>> MeasurementUnits_Validate__Save(List<MeasurementUnitForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[MeasurementUnitList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add("@Top", top);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[bll].[{nameof(MeasurementUnits_Validate__Save)}]";

                // Execute
                return await RepositoryUtilities.LoadErrors(cmd);
            }
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
                cmd.Parameters.Add("@ReturnIds", returnIds);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(MeasurementUnits__Save)}]";

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
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task MeasurementUnits__Activate(List<int> ids, bool isActive)
        {
            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                var isActiveParam = new SqlParameter("@IsActive", isActive);

                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new { Id = id }));
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@IsActive", isActive);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(MeasurementUnits__Activate)}]";

                // Execute
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<ValidationError>> MeasurementUnits_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new { Id = id }), addIndex: true);
                var idsTvp = new SqlParameter("@Ids", idsTable)
                {
                    TypeName = $"[dbo].[IndexedIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(idsTvp);
                cmd.Parameters.Add("@Top", top);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[bll].[{nameof(MeasurementUnits_Validate__Delete)}]";

                // Execute
                return await RepositoryUtilities.LoadErrors(cmd);
            }
        }

        public async Task MeasurementUnits__Delete(IEnumerable<int> ids)
        {
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

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(MeasurementUnits__Delete)}]";

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
        }

        #endregion

        #region Agents

        public Query<Agent> Agents__AsQuery(List<AgentForSave> entities)
        {
            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            SqlParameter entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[AgentList]",
                SqlDbType = SqlDbType.Structured
            };

            // Query
            var query = Query<Agent>();
            return query.FromSql($"[bll].[{nameof(Agents__AsQuery)}] (@Entities)", null, entitiesTvp);
        }

        public async Task<List<int>> Agents__Save(List<AgentForSave> entities, IEnumerable<IndexedImageId> imageIds, bool returnIds)
        {
            var result = new List<IndexedId>();

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[AgentList]",
                    SqlDbType = SqlDbType.Structured
                };

                DataTable imageIdsTable = RepositoryUtilities.DataTable(imageIds);
                var imageIdsTvp = new SqlParameter("@ImageIds", imageIdsTable)
                {
                    TypeName = $"[dbo].[IndexedImageIdList]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add(imageIdsTvp);
                cmd.Parameters.Add("@ReturnIds", returnIds);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Agents__Save)}]";

                // Execute
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
            var sortedResult = new int[entities.Count];
            result.ForEach(e =>
            {
                sortedResult[e.Index] = e.Id;
            });

            return sortedResult.ToList();
        }

        public async Task<IEnumerable<ValidationError>> Agents_Validate__Save(List<AgentForSave> entities, int top)
        {
            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
                var entitiesTvp = new SqlParameter("@Entities", entitiesTable)
                {
                    TypeName = $"[dbo].[AgentList]",
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
        }

        public async Task Agents__Delete(IEnumerable<int> ids)
        {
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
        }

        public async Task<IEnumerable<ValidationError>> Agents_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new { Id = id }), addIndex: true);
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
        }

        public async Task Agents__Activate(List<int> ids, bool isActive)
        {
            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                var isActiveParam = new SqlParameter("@IsActive", isActive);

                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new { Id = id }));
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
        }

        #endregion

        #region Users

        public async Task<UserSettings> Users__SettingsForClient()
        {
            var result = new UserSettings()
            {
                CustomSettings = new Dictionary<string, string>()
            };

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Users__SettingsForClient)}]";

                // Execute
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    // User Settings
                    if (await reader.ReadAsync())
                    {
                        int i = 0;

                        result.UserId = reader.GetInt32(i++);
                        result.Name = reader.String(i++);
                        result.Name2 = reader.String(i++);
                        result.Name3 = reader.String(i++);
                        result.ImageId = reader.String(i++);
                        result.UserSettingsVersion = reader.GetGuid(i++);
                    }
                    else
                    {
                        // Developer mistake
                        throw new InvalidOperationException("No settings for client were found");
                    }

                    // Custom settings
                    await reader.NextResultAsync();
                    while (await reader.ReadAsync())
                    {
                        string key = reader.GetString(0);
                        string val = reader.GetString(1);

                        result.CustomSettings[key] = val;
                    }
                }
            }

            return result;
        }

        public async Task Users__SaveSettings(string key, string value)
        {
            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                cmd.Parameters.Add("Key", key);
                cmd.Parameters.Add("Value", value);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Users__SaveSettings)}]";

                // Execute
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public Query<User> Users__AsQuery(List<UserForSave> entities)
        {
            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            SqlParameter entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[UserList]",
                SqlDbType = SqlDbType.Structured
            };

            // Query
            var query = Query<User>();
            return query.FromSql($"[bll].[{nameof(Users__AsQuery)}] (@Entities)", null, entitiesTvp);
        }

        public async Task<IEnumerable<ValidationError>> Users_Validate__Save(List<UserForSave> entities, int top)
        {
            entities.ForEach(e =>
            {
                e.Roles?.ForEach(r =>
                {
                    r.RoleId = e.Id;
                });
            });

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
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
        }

        public async Task<(IEnumerable<string> newEmails, IEnumerable<string> oldEmails)> Users__Save(List<UserForSave> entities)
        {
            entities.ForEach(e =>
            {
                e.Roles?.ForEach(r =>
                {
                    r.RoleId = e.Id;
                });
            });

            var newEmails = new List<string>();
            var oldEmails = new List<string>();

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

                DataTable rolesTable = RepositoryUtilities.DataTableWithHeaderIndex(entities, e => e.Roles);
                var rolesTvp = new SqlParameter("@Roles", rolesTable)
                {
                    TypeName = $"[dbo].[{nameof(RoleMembership)}List]",
                    SqlDbType = SqlDbType.Structured
                };

                cmd.Parameters.Add(entitiesTvp);
                cmd.Parameters.Add(rolesTvp);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Users__Save)}]";

                // Execute
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        newEmails.Add(reader.GetString(0));
                    }

                    await reader.NextResultAsync();

                    while (await reader.ReadAsync())
                    {
                        oldEmails.Add(reader.GetString(0));
                    }
                }
            }

            // Return result
            return (newEmails, oldEmails);
        }

        public async Task<IEnumerable<ValidationError>> Users_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new { Id = id }), addIndex: true);
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
        }

        public async Task<IEnumerable<string>> Users__Delete(IEnumerable<int> ids)
        {
            var deletedEmails = new List<string>(); // the result

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

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Users__Delete)}]";

                // Execute
                try
                {
                    // Execute
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            deletedEmails.Add(reader.GetString(0));
                        }
                    }
                }
                catch (SqlException ex) when (RepositoryUtilities.IsForeignKeyViolation(ex))
                {
                    throw new ForeignKeyViolationException();
                }
            }

            return deletedEmails;
        }

        #endregion

        #region Roles

        public Query<Role> Roles__AsQuery(List<RoleForSave> entities)
        {
            // Parameters
            DataTable entitiesTable = RepositoryUtilities.DataTable(entities, addIndex: true);
            SqlParameter entitiesTvp = new SqlParameter("@Entities", entitiesTable)
            {
                TypeName = $"[dbo].[RoleList]",
                SqlDbType = SqlDbType.Structured
            };

            // Query
            var query = Query<Role>();
            return query.FromSql($"[bll].[{nameof(Roles__AsQuery)}] (@Entities)", null, entitiesTvp);
        }

        public async Task<List<int>> Roles__Save(List<RoleForSave> entities, bool returnIds)
        {
            entities.ForEach(e =>
            {
                e.Permissions?.ForEach(p =>
                {
                    p.RoleId = e.Id;
                });

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
                e.Permissions?.ForEach(p =>
                {
                    p.RoleId = e.Id;
                });

                e.Members?.ForEach(m =>
                {
                    m.RoleId = e.Id;
                });
            });

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
                cmd.Parameters.Add("@Top", top);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[bll].[{nameof(Roles_Validate__Save)}]";

                // Execute
                return await RepositoryUtilities.LoadErrors(cmd);
            }
        }

        public async Task Roles__Delete(IEnumerable<int> ids)
        {
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
        }

        public async Task<IEnumerable<ValidationError>> Roles_Validate__Delete(List<int> ids, int top)
        {
            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new { Id = id }), addIndex: true);
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
        }

        public async Task Roles__Activate(List<int> ids, bool isActive)
        {
            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                var isActiveParam = new SqlParameter("@IsActive", isActive);

                DataTable idsTable = RepositoryUtilities.DataTable(ids.Select(id => new { Id = id }));
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
        }

        #endregion

        #region Blobs

        public async Task Blobs__Delete(IEnumerable<string> blobNames)
        {
            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                DataTable namesTable = RepositoryUtilities.DataTable(blobNames.Select(id => new { Id = id }));
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
        }

        public async Task Blobs__Save(string name, byte[] blob)
        {
            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                cmd.Parameters.Add("@Name", name);
                cmd.Parameters.Add("@Blob", blob);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Blobs__Save)}]";

                // Execute
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<byte[]> Blobs__Get(string name)
        {
            byte[] result = null;

            var conn = await GetConnectionAsync();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                cmd.Parameters.Add("@Name", name);

                // Command
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = $"[dal].[{nameof(Blobs__Get)}]";

                // Execute
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        result = (byte[])reader[0];
                    }
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
            using (var cmd = conn.CreateCommand())
            {
                // Arguments
                var mappedProps = typeof(SettingsForSave).GetMappedProperties();

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
        }

        #endregion
    }
}

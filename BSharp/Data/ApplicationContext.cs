using BSharp.Data.Model;
using BSharp.Services.Identity;
using BSharp.Services.Migrations;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Sharding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

// [CLI Commands]
// dotnet ef migrations add Initial -c=ApplicationContext -o=Data/Migrations/Application
// dotnet ef database update Custom1 --context=ApplicationContext
// dotnet ef migrations remove --context=ApplicationContext
namespace BSharp.Data
{
    /// <summary>
    /// The context containing all tables with TenantId, this context can be sharded across multiple databases
    /// and it automatically routes itself to the correct database using the registered IShardResolver service,
    /// Application tables such as Agents and Events all live here
    /// </summary>
    public class ApplicationContext : DbContext
    {
        #region Tables

        // The database tables are listed below
        public DbSet<MeasurementUnit> MeasurementUnits { get; set; }
        public DbSet<Custody> Custodies { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Blob> Blobs { get; set; }

        // Security
        public DbSet<LocalUser> LocalUsers { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<View> Views { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RoleMembership> RoleMemberships { get; set; }

        // Settings
        public DbSet<Settings> Settings { get; set; }

        #endregion

        #region Modelling

        /// <summary>
        /// This method configures the database model with EF Core's fluent API
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            string tenantId = "TenantId";

            // Measurement Units
            AddTenantId<MeasurementUnit>(builder);
            MeasurementUnit.OnModelCreating(builder);

            // Custodies
            AddTenantId<Custody>(builder);
            Custody.OnModelCreating(builder);

            // Agents : Custodies
            Agent.OnModelCreating_Agent(builder);

            // Roles
            AddTenantId<Role>(builder);
            Role.OnModelCreating(builder);

            // Views
            AddTenantId<View>(builder);
            View.OnModelCreating(builder);

            // Local Users
            AddTenantId<LocalUser>(builder);
            LocalUser.OnModelCreating(builder);

            // Role Memberships
            AddTenantId<RoleMembership>(builder);
            RoleMembership.OnModelCreating(builder);

            // Permissions
            AddTenantId<Permission>(builder);
            Permission.OnModelCreating(builder);

            // Settings
            AddTenantId<Settings>(builder, tenantId);
            Data.Model.Settings.OnModelCreating(builder);

            // Blobs
            AddTenantId<Blob>(builder, nameof(Blob.Id), tenantId);
            Blob.OnModelCreating(builder);
        }

        /// <summary>
        /// Adds a shadow property "TenantId" to the entity collection, adds that property to the entity keys, 
        /// and adds a query filter based on the tenantIdProvider
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="keyPropertyNames">The properties to include in the primary key, it uses one property "Id" by default if none are specified</param>
        private void AddTenantId<T>(ModelBuilder builder, params string[] keyPropertyNames) where T : class
        {
            string tenantId = "TenantId";

            // Add the TenantId shadow property
            builder.Entity<T>().Property<int>(tenantId)
                .ValueGeneratedNever();

            // Add the TenantId query filter (TODO: Remove and rely on security policy instead)
            builder.Entity<T>().HasQueryFilter(e => EF.Property<int?>(e, tenantId) == _tenantIdProvider.GetTenantId());

            // Make TenantId part of the composite primary key
            List<string> keys = new List<string>(keyPropertyNames);
            if (keys.Count == 0)
            {
                keys.Add("Id");
                builder.Entity<T>()
                    .Property("Id")
                    .ValueGeneratedOnAdd();

                keys = keys.Prepend(tenantId).ToList();
            }

            builder.Entity<T>().HasKey(keys.ToArray());
        }

        #endregion

        #region Queries

        /// <summary>
        /// A query for returning strongly typed validation errors from SQL
        /// </summary>
        public DbQuery<SqlValidationResult> Validation { get; set; }

        /// <summary>
        /// A query for returning strongly typed Id mappings from SQL
        /// </summary>
        public DbQuery<IndexedId> Saving { get; set; }

        /// <summary>
        /// A query for returning the Ids that correspond to a bunch of codes
        /// </summary>
        public DbQuery<CodeId> CodeIds { get; set; }

        /// <summary>
        /// A query for returning a list of strings
        /// </summary>
        public DbQuery<DbString> Strings { get; set; }

        /// <summary>
        /// Unified model for both application and admin contexts for querying user permissions
        /// </summary>
        public DbQuery<AbstractPermission> AbstractPermissions { get; set; }

        /// <summary>
        /// A query for returning a list of GUIDs
        /// </summary>
        public DbQuery<DbGuid> Guids { get; set; }

        #endregion

        #region Constructor

        // Private fields
        private readonly ITenantIdProvider _tenantIdProvider;

        // Constructor
        public ApplicationContext(IShardResolver shardProvider, ITenantIdProvider tenantIdProvider, IUserProvider userIdProvider, ITenantUserInfoAccessor accessor) :
            base(CreateDbContextOptions(shardProvider, tenantIdProvider, userIdProvider, accessor))
        {
            _tenantIdProvider = tenantIdProvider;
        }

        /// <summary>
        /// This trick makes it possible to injected the ApplicationContext into other components via DI as usual
        /// but it automatically configures itself with the correct options. Inspired from this Microsoft sample: https://bit.ly/2TIEFMA
        /// </summary>
        /// <param name="shardResolver">The service that resolves the shard connection string</param>
        /// <param name="tenantIdProvider">The service that retrieves tenants Ids from the headers</param>
        /// <returns></returns>
        private static DbContextOptions<ApplicationContext> CreateDbContextOptions(
            IShardResolver shardResolver, ITenantIdProvider tenantIdProvider,
            IUserProvider userService, ITenantUserInfoAccessor accessor)
        {
            // Prepare the options based on the connection created with the shard manager
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            string connectionString = shardResolver.GetShardConnectionString();
            if (tenantIdProvider is DesignTimeTenantIdProvider)
            {
                // Only for design time when running "ef migrations" command from the CLI
                optionsBuilder = optionsBuilder.UseSqlServer(connectionString);
            }
            else
            {
                int tenantId = tenantIdProvider.GetTenantId() ?? throw new Controllers.Misc.BadRequestException("Tenant Id was not supplied");

                // Unless this is a fake design time resolver, apply row level security and pass context info
                SqlConnection sqlConnection = new SqlConnection(connectionString);

                SqlCommand cmd = sqlConnection.CreateCommand();
                cmd.CommandText = @"
    -- Set the global values of the session context
    EXEC sp_set_session_context @key=N'TenantId', @value=@TenantId;
    EXEC sp_set_session_context @key=N'Culture', @value=@Culture;
    EXEC sp_set_session_context @key=N'NeutralCulture', @value=@NeutralCulture;

    -- Get the User Id
    DECLARE 
        @UserId INT, 
        @Name NVARCHAR(255), 
        @Name2 NVARCHAR(255), 
        @ExternalId NVARCHAR(450), 
        @Email NVARCHAR(255), 
        @SettingsVersion UNIQUEIDENTIFIER, 
        @PermissionsVersion UNIQUEIDENTIFIER,
        @ViewsAndSpecsVersion UNIQUEIDENTIFIER,
        @UserSettingsVersion UNIQUEIDENTIFIER,
        @PrimaryLanguageId NVARCHAR(255),
        @PrimaryLanguageSymbol NVARCHAR(255),
        @SecondaryLanguageId NVARCHAR(255),
        @SecondaryLanguageSymbol NVARCHAR(255)
;

    SELECT
        @UserId = Id,
        @Name = Name,
        @Name2 = Name2,
        @ExternalId = ExternalId,
        @Email = Email,
        @PermissionsVersion = PermissionsVersion,
        @UserSettingsVersion = UserSettingsVersion
    FROM [dbo].[LocalUsers] 
    WHERE TenantId = @TenantId AND IsActive = 1 AND (ExternalId = @ExternalUserId OR Email = @UserEmail);

    -- Set LastAccess (Works only if @UserId IS NOT NULL)
    UPDATE [dbo].[LocalUsers] SET LastAccess = SYSDATETIMEOFFSET() WHERE Id = @UserId;

    -- Get hashes
    SELECT 
        @SettingsVersion = SettingsVersion,
        @ViewsAndSpecsVersion = ViewsAndSpecsVersion,
        @PrimaryLanguageId = PrimaryLanguageId,
        @PrimaryLanguageSymbol = PrimaryLanguageSymbol,
        @SecondaryLanguageId = SecondaryLanguageId,
        @SecondaryLanguageSymbol = SecondaryLanguageSymbol
    FROM [dbo].[Settings]
    WHERE TenantId = @TenantId 

    -- Set the User Id
    EXEC sp_set_session_context @key=N'UserId', @value=@UserId;

    -- Return the user information
    SELECT 
        @UserId AS userId, 
        @Name AS Name,
        @Name2 AS Name2,
        @ExternalId AS ExternalId, 
        @Email AS Email, 
        @SettingsVersion AS SettingsVersion, 
        @PermissionsVersion AS PermissionsVersion,
        @UserSettingsVersion AS UserSettingsVersion,
        @ViewsAndSpecsVersion AS ViewsAndSpecsVersion,
        @PrimaryLanguageId AS PrimaryLanguageId,
        @PrimaryLanguageSymbol AS PrimaryLanguageSymbol,
        @SecondaryLanguageId AS SecondaryLanguageId,
        @SecondaryLanguageSymbol AS SecondaryLanguageSymbol;
";
                cmd.Parameters.AddWithValue("@TenantId", tenantId);
                cmd.Parameters.AddWithValue("@ExternalUserId", userService.GetUserId());
                cmd.Parameters.AddWithValue("@UserEmail", userService.GetUserEmail());
                cmd.Parameters.AddWithValue("@Culture", CultureInfo.CurrentUICulture.Name);
                cmd.Parameters.AddWithValue("@NeutralCulture", CultureInfo.CurrentUICulture.IsNeutralCulture ? CultureInfo.CurrentUICulture.Name : CultureInfo.CurrentUICulture.Parent.Name);

                sqlConnection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int i = 0;
                        var info = new TenantUserInfo
                        {
                            UserId = reader.IsDBNull(i) ? (int?)null : reader.GetInt32(i++),
                            Name = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            Name2 = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            ExternalId = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            Email = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            SettingsVersion = reader.IsDBNull(i) ? null : reader.GetGuid(i++).ToString(),
                            PermissionsVersion = reader.IsDBNull(i) ? null : reader.GetGuid(i++).ToString(),
                            UserSettingsVersion = reader.IsDBNull(i) ? null : reader.GetGuid(i++).ToString(),
                            ViewsAndSpecsVersion = reader.IsDBNull(i) ? null : reader.GetGuid(i++).ToString(),
                            PrimaryLanguageId = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            PrimaryLanguageSymbol = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            SecondaryLanguageId = reader.IsDBNull(i) ? null : reader.GetString(i++),
                            SecondaryLanguageSymbol = reader.IsDBNull(i) ? null : reader.GetString(i++),
                        };

                        // Provide the user throughout the current session
                        accessor.SetInfo(tenantId, info);
                    }
                    else
                    {
                        throw new Controllers.Misc.BadRequestException("Something went wrong while querying the user ID from the Database");
                    }
                }

                // Prepare the options based on the connection created with the shard manager
                optionsBuilder = optionsBuilder.UseSqlServer(sqlConnection);
            }

            return optionsBuilder
                .ReplaceService<IMigrationsSqlGenerator, CustomSqlServerMigrationsSqlGenerator>()
                .Options;
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            // Since we passed an open connection to UseSqlServer, the underlying framework does 
            // not own the connection and therefore does not automatically close it, so we do it 
            // ourselves here
            var sqlConnection = Database.GetDbConnection();
            sqlConnection.Dispose();

            base.Dispose();
        }

        #endregion

        #region Design Time Factory

        /// <summary>
        /// Since <see cref="ApplicationContext"/> does not have the usual DbContext constructor and is not 
        /// registered in the DI container the standard way, this factory implementation is necessary for the 
        /// migration tools to be able to create an instance of the context at design time
        /// </summary>
        public class DesignTimeApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
        {
            public ApplicationContext CreateDbContext(string[] args)
            {
                return new ApplicationContext(
                    new DesignTimeShardResolver(),
                    new DesignTimeTenantIdProvider(),
                    new DesignTimeUserIdProvider(),
                    new DesignTimeTenantUserInfoAccessor()
                );
            }
        }

        public class DesignTimeShardResolver : IShardResolver
        {
            public string GetShardConnectionString()
            {
                return "Server=(localdb)\\MSSQLLocalDB;Database=BSharp;Trusted_Connection=true;MultipleActiveResultSets=true";
            }
        }

        public class DesignTimeTenantIdProvider : ITenantIdProvider
        {
            private readonly int? _tenantId;

            public DesignTimeTenantIdProvider(int? tenantId = null)
            {
                _tenantId = tenantId;
            }

            public int? GetTenantId()
            {
                return _tenantId ?? -1;
            }
        }

        public class DesignTimeUserIdProvider : IUserProvider
        {
            public string GetUserEmail()
            {
                return "<FakeUserEmail>";
            }

            public string GetUserId()
            {
                return "<FakeUserId>";
            }
        }

        public class DesignTimeTenantUserInfoAccessor : ITenantUserInfoAccessor
        {
            public TenantUserInfo GetCurrentInfo()
            {
                return new TenantUserInfo();
            }

            public TenantUserInfo GetInfo(int tenantId)
            {
                return new TenantUserInfo();
            }

            public void SetInfo(int tenantId, TenantUserInfo info)
            {
            }
        }

        #endregion
    }
}

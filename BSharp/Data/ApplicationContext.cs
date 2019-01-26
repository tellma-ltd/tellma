using BSharp.Data.Model;
using BSharp.Services.Identity;
using BSharp.Services.Migrations;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Sharding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

// [CLI Commands]
// dotnet ef migrations add Initial -c=ApplicationContext -o=Data/Migrations/Application
// dotnet ef database update LastGoodMigration --context=ApplicationContext
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
        // The database tables are listed below
        public DbSet<MeasurementUnit> MeasurementUnits { get; set; }
        public DbSet<Custody> Custodies { get; set; }
        public DbSet<Agent> Agents { get; set; }

        // Security
        public DbSet<LocalUser> LocalUsers { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<View> Views { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RoleMembership> RoleMemberships { get; set; }


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
        /// A query for returning the Ids that correspond to a bunch of codes
        /// </summary>
        public DbQuery<DbString> Strings { get; set; }

        // Private fields
        private readonly ITenantIdProvider _tenantIdProvider;

        // Constructor
        public ApplicationContext(IShardResolver shardProvider, ITenantIdProvider tenantIdProvider, IUserService userIdProvider) :
            base(CreateDbContextOptions(shardProvider, tenantIdProvider, userIdProvider))
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
            IShardResolver shardResolver, ITenantIdProvider tenantIdProvider, IUserService userService)
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
                // Unless this is a fake design time resolver, apply row level security and pass context info
                SqlConnection sqlConnection = new SqlConnection(connectionString);

                SqlCommand cmd = sqlConnection.CreateCommand();
                cmd.CommandText = @"
-- Get the User Id
DECLARE @UserId INT, @ExternalId NVARCHAR(450), @Email NVARCHAR(255);
SELECT
    @UserId = Id,
    @ExternalId = ExternalId,
    @Email = Email FROM [dbo].[LocalUsers] 
WHERE TenantId = @TenantId AND IsActive = 1 AND (ExternalId = @ExternalUserId OR Email = @UserEmail);

-- Set LastAccess (Works only if @UserId IS NOT NULL)
UPDATE [dbo].[LocalUsers] SET LastAccess = SYSDATETIMEOFFSET() WHERE Id = @UserId;

-- Set the global values of the session context
EXEC sp_set_session_context @key=N'TenantId', @value=@TenantId;
EXEC sp_set_session_context @key=N'UserId', @value=@UserId;
EXEC sp_set_session_context @key=N'Culture', @value=@Culture;
EXEC sp_set_session_context @key=N'NeutralCulture', @value=@NeutralCulture;

-- Return the user information
SELECT @UserId as userId, @ExternalId as ExternalId, @Email as Email;
";
                cmd.Parameters.AddWithValue("@TenantId", tenantIdProvider.GetTenantId() ?? throw new Controllers.Misc.BadRequestException("Tenant Id was not supplied"));
                cmd.Parameters.AddWithValue("@ExternalUserId", userService.GetUserId());
                cmd.Parameters.AddWithValue("@UserEmail", userService.GetUserEmail());
                cmd.Parameters.AddWithValue("@Culture", CultureInfo.CurrentCulture.Name);
                cmd.Parameters.AddWithValue("@NeutralCulture", CultureInfo.CurrentCulture.IsNeutralCulture ? CultureInfo.CurrentCulture.Name : CultureInfo.CurrentCulture.Parent.Name);

                sqlConnection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var user = new DbUser
                        {
                            Id = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0),
                            ExternalId = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                        };

                        // Provide the user throughout the current session
                        userService.SetDbUser(user);
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

        /// <summary>
        /// This method configures the database model with EF Core's fluent API
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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
        }

        public override void Dispose()
        {
            // Since we passed an open connection to UseSqlServer, the underlying framework does 
            // not own the connection and therefore does not automatically close it, so we do it 
            // ourselves here
            var sqlConnection = Database.GetDbConnection();
            sqlConnection.Dispose();

            base.Dispose();
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
            builder.Entity<T>().Property<int>(tenantId);

            // Add the TenantId query filter (TODO: Remove)
            builder.Entity<T>().HasQueryFilter(e => EF.Property<int?>(e, tenantId) == _tenantIdProvider.GetTenantId());

            // Make TenantId part of the composite primary key
            List<string> keys = new List<string>(keyPropertyNames);
            if (keys.Count == 0)
            {
                keys.Add("Id");
                builder.Entity<T>()
                    .Property("Id")
                    .ValueGeneratedOnAdd();
            }

            keys = keys.Prepend(tenantId).ToList();
            builder.Entity<T>().HasKey(keys.ToArray());
        }

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
                    new DesignTimeUserIdProvider()
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
            public int? GetTenantId()
            {
                return -1;
            }
        }

        public class DesignTimeUserIdProvider : IUserService
        {
            public DbUser GetDbUser()
            {
                return new DbUser { };
            }

            public string GetUserEmail()
            {
                return "<FakeUserEmail>";
            }

            public string GetUserId()
            {
                return "<FakeUserId>";
            }

            public void SetDbUser(DbUser user)
            {

            }
        }

        #endregion
    }
}

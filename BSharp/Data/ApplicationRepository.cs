using BSharp.Controllers.DTO;
using BSharp.Services.OData;
using BSharp.Services.Sharding;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data
{
    public interface IRepository
    {
        ODataQuery<T> CreateQuery<T>() where T : DtoBase;
    }

    public class ApplicationRepository : IDisposable, IRepository
    {
        public ApplicationRepository(IShardResolver shardResolver)
        {
            _shardResolver = shardResolver;
        }

        private readonly IShardResolver _shardResolver;
        private SqlConnection _conn;

        private async Task InitConnection(string connectionString)
        {
            _conn = new SqlConnection(connectionString);
            using (SqlCommand cmd = _conn.CreateCommand())
            {
                cmd.CommandText = @"
    -- Set the global values of the session context
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
        @TernaryLanguageId NVARCHAR(255),
        @TernaryLanguageSymbol NVARCHAR(255);

    SELECT
        @UserId = [Id],
        @Name = [Name],
        @Name2 = [Name2],
        @Name3 = [Name3],
        @ExternalId = [ExternalId],
        @Email = [Email],
        @PermissionsVersion = [PermissionsVersion],
        @UserSettingsVersion = [UserSettingsVersion]
    FROM [dbo].[LocalUsers] 
    WHERE [IsActive] = 1 AND ([ExternalId] = @ExternalUserId OR [Email] = @UserEmail);

    -- Set LastAccess (Works only if @UserId IS NOT NULL)
    UPDATE [dbo].[LocalUsers] SET [LastAccess] = SYSDATETIMEOFFSET() WHERE [Id] = @UserId;

    -- Get hashes
    SELECT 
        @SettingsVersion = [SettingsVersion],
        @ViewsAndSpecsVersion = [ViewsAndSpecsVersion],
        @PrimaryLanguageId = [PrimaryLanguageId],
        @PrimaryLanguageSymbol = [PrimaryLanguageSymbol],
        @SecondaryLanguageId = [SecondaryLanguageId],
        @SecondaryLanguageSymbol = [SecondaryLanguageSymbol]
    FROM [dbo].[Settings]

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
                cmd.Parameters.AddWithValue("@ExternalUserId", userService.GetUserId());
                cmd.Parameters.AddWithValue("@UserEmail", userService.GetUserEmail());
                cmd.Parameters.AddWithValue("@Culture", CultureInfo.CurrentUICulture.Name);
                cmd.Parameters.AddWithValue("@NeutralCulture", CultureInfo.CurrentUICulture.IsNeutralCulture ? CultureInfo.CurrentUICulture.Name : CultureInfo.CurrentUICulture.Parent.Name);

                _conn.Open();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        int i = 0;
                        var info = new TenantUserInfo
                        {
                            UserId = reader.IsDBNull(i) ? (int?)null : reader.GetInt32(i++), // User
                            Name = reader.IsDBNull(i) ? null : reader.GetString(i++), // User
                            Name2 = reader.IsDBNull(i) ? null : reader.GetString(i++), // User
                            ExternalId = reader.IsDBNull(i) ? null : reader.GetString(i++), // User
                            Email = reader.IsDBNull(i) ? null : reader.GetString(i++), // User
                            SettingsVersion = reader.IsDBNull(i) ? null : reader.GetGuid(i++).ToString(),
                            PermissionsVersion = reader.IsDBNull(i) ? null : reader.GetGuid(i++).ToString(), // User
                            UserSettingsVersion = reader.IsDBNull(i) ? null : reader.GetGuid(i++).ToString(), // User
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
                optionsBuilder = optionsBuilder.UseSqlServer(_conn);
            }

        }

        private async Task<SqlConnection> Connection()
        {
            if (_conn == null)
            {
                string connString = _shardResolver.GetShardConnectionString();
                await InitConnection(connString);
            }

            return _conn;
        }

        /// <summary>
        /// By default the <see cref="ApplicationRepository"/> connects to the database corresponding to 
        /// the current tenantId which is retrieved from an injected <see cref="IShardResolver"/>,
        /// this method makes it possible to conncet to a custom connection string instead, 
        /// this is useful when connecting to multiple tenants at the same time to do aggregate reporting for example
        /// </summary>
        public async Task Init(string connectionString)
        {
            await InitConnection(connectionString);
        }

        public ODataQuery<T> CreateQuery<T>() where T : DtoBase
        {
            throw new NotImplementedException();
        }


        publci async Task<()>

        public async Task<IEnumerable<int?>> MeasurementUnits__Save(IEnumerable<MeasurementUnit> entities)
        {
            var conn = Connection();
            using (var cmd = conn.CreateCommand())
            {
                // Parameters
                cmd.Parameters.AddWithValue("Param1", 33);

                // SQL
                cmd.CommandText = "EXEC [dal].[MeasurementUnits__Save] @Param1 = @Param1"; // Typically call a stored procedure

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        yield return reader.GetInt32(0);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_conn != null)
            {
                _conn.Close();
                _conn.Dispose();
            }
        }
    }

    public class 

    public interface IReadonlyRepository<T, TKey> where T : DtoBase
    {
        ODataQuery<T> Query();
    }

    public interface IUpdatableRepository<T, TForSave, TKey> : IReadonlyRepository<T, TKey> where TForSave : DtoBase where T : DtoBase
    {
        List<ValidationResult> ValidateSave(IEnumerable<T> entities);

        List<TKey> Save(IEnumerable<T> entities);
    }

    public class ValidationResult
    {

    }
}

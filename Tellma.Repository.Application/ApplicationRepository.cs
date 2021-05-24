//using Microsoft.Extensions.Localization;
//using System;
//using System.Data;
//using System.Data.SqlClient;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Transactions;
//using Tellma.Repository.Common;

//namespace Tellma.Repository.Application
//{
//    public class ApplicationRepository
//    {
//        public ApplicationRepository(IShardResolver shardResolver, IStringLocalizer<Strings> localizer)
//        {
//        }

//        public async Task<(UserInfo, TenantInfo)> OnConnect(string externalUserId, string userEmail, string culture, string neutralCulture, bool setLastActive, CancellationToken cancellation)
//        {
//            using var _ = Instrumentation.Block("Repo." + nameof(OnConnect));

//            UserInfo userInfo = null;
//            TenantInfo tenantInfo = null;

//            using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
//            using var conn = new SqlConnection("TODO");
//            using (SqlCommand cmd = conn.CreateCommand()) // Use the private field _conn to avoid infinite recursion
//            {
//                // Parameters
//                cmd.Parameters.Add("@ExternalUserId", externalUserId);
//                cmd.Parameters.Add("@UserEmail", userEmail);
//                cmd.Parameters.Add("@Culture", culture);
//                cmd.Parameters.Add("@NeutralCulture", neutralCulture);
//                cmd.Parameters.Add("@SetLastActive", setLastActive);

//                // Command
//                cmd.CommandType = CommandType.StoredProcedure;
//                cmd.CommandText = $"[dal].[{nameof(OnConnect)}]";

//                // Execute and Load
//                using var reader = await cmd.ExecuteReaderAsync(cancellation);
//                if (await reader.ReadAsync(cancellation))
//                {
//                    int i = 0;

//                    // The user Info
//                    userInfo = new UserInfo
//                    {
//                        UserId = reader.Int32(i++),
//                        Name = reader.String(i++),
//                        Name2 = reader.String(i++),
//                        Name3 = reader.String(i++),
//                        ExternalId = reader.String(i++),
//                        Email = reader.String(i++),
//                        PermissionsVersion = reader.Guid(i++)?.ToString(),
//                        UserSettingsVersion = reader.Guid(i++)?.ToString(),
//                    };

//                    // The tenant Info
//                    tenantInfo = new TenantInfo
//                    {
//                        ShortCompanyName = reader.String(i++),
//                        ShortCompanyName2 = reader.String(i++),
//                        ShortCompanyName3 = reader.String(i++),
//                        DefinitionsVersion = reader.Guid(i++)?.ToString(),
//                        SettingsVersion = reader.Guid(i++)?.ToString(),
//                        PrimaryLanguageId = reader.String(i++),
//                        PrimaryLanguageSymbol = reader.String(i++),
//                        SecondaryLanguageId = reader.String(i++),
//                        SecondaryLanguageSymbol = reader.String(i++),
//                        TernaryLanguageId = reader.String(i++),
//                        TernaryLanguageSymbol = reader.String(i++),
//                        DateFormat = reader.String(i++),
//                        TimeFormat = reader.String(i++),
//                        TaxIdentificationNumber = reader.String(i++),
//                    };
//                }
//                else
//                {
//                    throw new InvalidOperationException($"[dal].[OnConnect] did not return any data, InitialCatalog: {InitialCatalog()}, ExternalUserId: {externalUserId}, UserEmail: {userEmail}");
//                }
//            }

//            return (userInfo, tenantInfo);
//        }
//    }
//}

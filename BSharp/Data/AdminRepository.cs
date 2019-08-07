using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data
{
    public class AdminRepository
    {
        #region

        private string _connectionString;

        public AdminRepository(IOptions<AdminRepositoryOptions> config)
        {
            _connectionString = config?.Value?.ConnectionString ?? throw new InvalidOperationException("The admin connection string was not supplied");
        }

        #endregion

        public Task<GlobalUserInfo> GetUserInfoAsync()
        {
            // TODO
            throw new NotImplementedException();
        }

        public Task SetUserExternalIdByUserIdAsync(int userId, string externalId)
        {
            throw new NotImplementedException();
        }

        public Task SetUserEmailByUserIdAsync(int userId, string externalEmail)
        {
            throw new NotImplementedException();
        }

        public Task SetUserExternalIdByEmailAsync(string email, string externalId)
        {
            // Finds the user with the given email and sets its externalId as specified
            throw new NotImplementedException();
        }
    }
}

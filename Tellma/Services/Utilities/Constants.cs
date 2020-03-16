using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Services.Utilities
{
    public static class Constants
    {
        public const string Error_TheField0IsRequired = nameof(Error_TheField0IsRequired);

        public const string AdminConnection = nameof(AdminConnection);
        public const string IdentityConnection = nameof(IdentityConnection);
        public const string Server = "Server";
        public const string Client = "Client";
        public const string Shared = "Shared";
        public const string ApiResourceName = "tellma";

        // Permission Levels
        public const string Read = nameof(Read);
        public const string Update = nameof(Update);
        public const string Delete = nameof(Delete);

        // Caching
        public const string Fresh = "Fresh";
        public const string Stale = "Stale";
        public const string Unauthorized = "Unauthorized";

        // Tokens
        public const int TokenExpiryInDays = 3;

        // Indicates a hidden field when exporting to Excel
        public const string HIDDEN_FIELD = "<HIDDEN-FIELD>";

        // Indicates a restricted
        public const string Restricted = "*******";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.Utilities
{
    public static class Constants
    {
        public const string AdminConnection = nameof(AdminConnection);
        public const string Server = "Server";
        public const string Client = "Client";
        public const string Shared = "Shared";
        public const string BSharpAPI = "BSharpAPI";

        // Permission Levels
        public const string Read = nameof(Read);
        public const string Update = nameof(Update);
        public const string Create = nameof(Create);
        public const string ReadCreate = nameof(ReadCreate);
        public const string Sign = nameof(Sign);

        // Indicates a hidden field
        public const string Hidden = "<X-HIDDEN-FIELD>";
    }
}

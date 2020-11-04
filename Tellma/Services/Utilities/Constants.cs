namespace Tellma.Services.Utilities
{
    public static class Constants
    {
        public const string Error_Field0IsRequired = "Error_Field0IsRequired";
        public const string Error_Field0LengthMaximumOf1 = "Error_Field0LengthMaximumOf1";
        public const string Error_Field0LengthMaximumOf1MinimumOf2 = "Error_Field0LengthMaximumOf1MinimumOf2";
        public const string Error_Field0IsNotValidEmail = "Error_Field0IsNotValidEmail";
        public const string Error_Field0IsNotValidPhone = "Error_Field0IsNotValidPhone";

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

    public static class DefStates
    {
        // Definition States
        public const string Hidden = nameof(Hidden);
        public const string Visible = nameof(Visible);
        public const string Archived = nameof(Archived);

        public static readonly string[] All = new string[] { Hidden, Visible, Archived };
    }
}

namespace Tellma.Services.Utilities
{
    public static class Constants
    {
        public const string Error_Field0IsRequired = "Error_Field0IsRequired";
        public const string Error_Field0LengthMaximumOf1 = "Error_Field0LengthMaximumOf1";
        public const string Error_Field0IsNotValidEmail = "Error_Field0IsNotValidEmail";

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
    }
}

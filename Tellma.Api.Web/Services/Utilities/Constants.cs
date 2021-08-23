namespace Tellma.Services.Utilities
{
    public static class Constants
    {
        public const string Error_Field0IsRequired = "Error_Field0IsRequired";
        public const string Error_Field0LengthMaximumOf1MinimumOf2 = "Error_Field0LengthMaximumOf1MinimumOf2";
        public const string Error_Field0IsNotValidEmail = "Error_Field0IsNotValidEmail";

        public const string WebClientName = "WebClient";
        public const string ApiResourceName = "tellma";

        // Caching
        public const string Fresh = "Fresh";
        public const string Stale = "Stale";
        public const string Unauthorized = "Unauthorized";

        // Tokens
        public const int TokenExpiryInDays = 3;
    }
}

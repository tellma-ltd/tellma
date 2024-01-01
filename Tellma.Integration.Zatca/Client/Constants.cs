namespace Tellma.Integration.Zatca
{
    public class Constants
    {
        public class ReportingStatus
        {
            public const string REPORTED = nameof(REPORTED);
            public const string NOT_REPORTED = nameof(NOT_REPORTED);
        }
        public class ClearanceStatus
        {
            public const string CLEARED = nameof(CLEARED);
            public const string NOT_CLEARED = nameof(NOT_CLEARED);
        }
        public class ValidationStatus
        {
            public const string PASS = nameof(PASS);
            public const string WARNING = nameof(WARNING);
            public const string ERROR = nameof(ERROR);
        }
        public class ValidationType
        {
            public const string INFO = nameof(INFO);
            public const string WARNING = nameof(WARNING);
            public const string ERROR = nameof(ERROR);
        }
        public class Disposition
        {
            public const string ISSUED = nameof(ISSUED);
            public const string NOT_COMPLIANT = nameof(NOT_COMPLIANT);
        }
    }
}

namespace Tellma.Integration.Zatca.Tests
{
    public class ZatcaSandboxOptions
    {
        public ZatcaCredentials? Default { get; set; }
        public ZatcaCredentials? Reporting { get; set; }
        public ZatcaCredentials? Clearance { get; set; }
        public ZatcaCredentials? Compliance { get; set; }
        public ZatcaCredentials? Onboarding { get; set; }
        public ZatcaCredentials? Renewal { get; set; }
    }


    public class ZatcaCredentials
    {
        public string? Username { get; set; }

        public string? Password { get; set; }
    }

}
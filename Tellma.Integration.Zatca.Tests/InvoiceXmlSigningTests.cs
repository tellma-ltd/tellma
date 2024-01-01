namespace Tellma.Integration.Zatca.Tests
{
    public class InvoiceXmlSigningTests
    {
        [Fact(DisplayName = "Invoice signing is consistent with FATOORA tool")]
        public void SignInvoice()
        {
            // Arrange
            var invoice = InvoicesSamples.ValidStandardInvoice();

            // These were obtained from the FATOORA tool installation.
            const string certificateContent = "MIID6jCCA5CgAwIBAgITbwAAgbuRbo5tpQ+QjgABAACBuzAKBggqhkjOPQQDAjBjMRUwEwYKCZImiZPyLGQBGRYFbG9jYWwxEzARBgoJkiaJk/IsZAEZFgNnb3YxFzAVBgoJkiaJk/IsZAEZFgdleHRnYXp0MRwwGgYDVQQDExNUU1pFSU5WT0lDRS1TdWJDQS0xMB4XDTIyMTEwOTA4MDcyMloXDTI0MTEwODA4MDcyMlowTjELMAkGA1UEBhMCU0ExEzARBgNVBAoTCjM5OTk5OTk5OTkxDDAKBgNVBAsTA1RTVDEcMBoGA1UEAxMTVFNULTM5OTk5OTk5OTkwMDAwMzBWMBAGByqGSM49AgEGBSuBBAAKA0IABGGDDKDmhWAITDv7LXqLX2cmr6+qddUkpcLCvWs5rC2O29W/hS4ajAK4Qdnahym6MaijX75Cg3j4aao7ouYXJ9GjggI5MIICNTCBmgYDVR0RBIGSMIGPpIGMMIGJMTswOQYDVQQEDDIxLVRTVHwyLVRTVHwzLTlmMDkyMjM4LTFkOTctNDcxOC1iNDQxLWNiYzMwMTMyMWIwYTEfMB0GCgmSJomT8ixkAQEMDzM5OTk5OTk5OTkwMDAwMzENMAsGA1UEDAwEMTEwMDEMMAoGA1UEGgwDVFNUMQwwCgYDVQQPDANUU1QwHQYDVR0OBBYEFDuWYlOzWpFN3no1WtyNktQdrA8JMB8GA1UdIwQYMBaAFHZgjPsGoKxnVzWdz5qspyuZNbUvME4GA1UdHwRHMEUwQ6BBoD+GPWh0dHA6Ly90c3RjcmwuemF0Y2EuZ292LnNhL0NlcnRFbnJvbGwvVFNaRUlOVk9JQ0UtU3ViQ0EtMS5jcmwwga0GCCsGAQUFBwEBBIGgMIGdMG4GCCsGAQUFBzABhmJodHRwOi8vdHN0Y3JsLnphdGNhLmdvdi5zYS9DZXJ0RW5yb2xsL1RTWkVpbnZvaWNlU0NBMS5leHRnYXp0Lmdvdi5sb2NhbF9UU1pFSU5WT0lDRS1TdWJDQS0xKDEpLmNydDArBggrBgEFBQcwAYYfaHR0cDovL3RzdGNybC56YXRjYS5nb3Yuc2Evb2NzcDAOBgNVHQ8BAf8EBAMCB4AwHQYDVR0lBBYwFAYIKwYBBQUHAwIGCCsGAQUFBwMDMCcGCSsGAQQBgjcVCgQaMBgwCgYIKwYBBQUHAwIwCgYIKwYBBQUHAwMwCgYIKoZIzj0EAwIDSAAwRQIgeWUEjxXaW4s8XilH/abzbDJhHHjO3uLaD87YqioA89YCIQDNltfAU98b8FnTD7M8NYIk8cqi7OnPu7h85v5V1Bt3Hg==";
            const string privateKeyContent = "MHQCAQEEIDyLDaWIn/1/g3PGLrwupV4nTiiLKM59UEqUch1vDfhpoAcGBSuBBAAKoUQDQgAEYYMMoOaFYAhMO/steotfZyavr6p11SSlwsK9azmsLY7b1b+FLhqMArhB2dqHKboxqKNfvkKDePhpqjui5hcn0Q==";

            // Act
            var sigInfo = new TestingInvoiceXmlBuilder(invoice)
                .Build()
                .Sign(certificateContent, privateKeyContent);

            // Assert
            Assert.Equal(TestingInvoiceXmlBuilder.SIGNING_TIME, sigInfo.SigningTime);

            // These were obtained by running the FATOORA tool
            const string expectedInvoiceHash = "TqSGxNUkKSPJl85DQ8vPsGhL0rhB0Oj19dI8QgZgMYo=";
            const string expectedSignedPropertiesHash = "YWM3ZDAxODk0YmVkOWQ4OWRmMGM0YTIyNTYzNzhjNTU2NDQ5ZGU5ZmQ5NjI0ZTVmNzRlYjI5MTAwYzZhODg0NA==";
            const string expectedIssuerName = "CN=TSZEINVOICE-SubCA-1, DC=extgazt, DC=gov, DC=local";
            const string expectedCertHash = "Y2U5MzY5MTFiOTA4NTc0YmI2NjExNDFlMzBkNmM2YTljZWMxYjRlZDFmYWE3NjE1NjVlNDQzNjA3ODdkYzZjZQ==";
            const string expectedSerialNumber = "2475382889481219846080454947234981286678397371";

            Assert.Equal(expectedInvoiceHash, sigInfo.InvoiceHash);
            Assert.Equal(expectedSignedPropertiesHash, sigInfo.SignedPropertiesHash);
            Assert.Equal(expectedIssuerName, sigInfo.CertificateIssuerName);
            Assert.Equal(expectedSerialNumber, sigInfo.CertificateSerialNumber);
            Assert.Equal(expectedCertHash, sigInfo.CertificateHash);
        }

        internal class TestingInvoiceXmlBuilder : InvoiceXmlBuilder
        {
            public const string SIGNING_TIME = "2023-12-31T18:28:37";

            public TestingInvoiceXmlBuilder(Invoice inv) : base(inv) { }
            protected override string GetCurrentTime() => SIGNING_TIME;
            public override string GetXml() => File.ReadAllText("Resources/StandardInvoice_FatooraSample_Unsigned.xml");
        }
    }
}

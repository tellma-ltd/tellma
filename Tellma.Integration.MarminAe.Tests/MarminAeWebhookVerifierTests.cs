// Copyright (c) Tellma Ltd. All rights reserved.

using System.Security.Cryptography;
using System.Text;

namespace Tellma.Integration.MarminAe.Tests
{
    /// <summary>
    ///     Covers <see cref="MarminAeWebhookVerifier" />, the gate in front of the anonymous
    ///     callback endpoint.
    /// </summary>
    /// <remarks>
    ///     The webhook route is unauthenticated, so this signature check is the only thing standing
    ///     between the public internet and a write to a tenant's Documents table. Since the webhook
    ///     is not being live-tested in this round, these tests are what stands in for that.
    /// </remarks>
    public class MarminAeWebhookVerifierTests
    {
        private const string Secret = "s3cr3t-signing-key";
        private const string OtherSecret = "the-previous-signing-key";

        private static readonly byte[] Body =
            Encoding.UTF8.GetBytes("""{"org_id":"6d1b9b1e-0000-4000-8000-000000000001"}""");

        private static string Sign(byte[] body, string secret) =>
            Convert.ToBase64String(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), body));

        [Fact]
        public void Accepts_AGenuineSignature()
        {
            Assert.True(MarminAeWebhookVerifier.Verify(Body, Sign(Body, Secret), [Secret]));
        }

        [Fact]
        public void Accepts_ASignatureFromAnyConfiguredSecret()
        {
            // Rotation: for a window, deliveries signed with either the new or the old secret must
            // both be accepted, whichever order they are configured in.
            string signedWithOld = Sign(Body, OtherSecret);

            Assert.True(MarminAeWebhookVerifier.Verify(Body, signedWithOld, [Secret, OtherSecret]));
            Assert.True(MarminAeWebhookVerifier.Verify(Body, signedWithOld, [OtherSecret, Secret]));
        }

        [Fact]
        public void Accepts_ASignatureStrippedOfItsPadding()
        {
            // A proxy or portal that hands back unpadded base64 is still handing back the same MAC.
            Assert.True(MarminAeWebhookVerifier.Verify(
                Body, Sign(Body, Secret).TrimEnd('='), [Secret]));
        }

        [Fact]
        public void Rejects_ATamperedBody()
        {
            string signature = Sign(Body, Secret);
            byte[] tampered = Encoding.UTF8.GetBytes(
                """{"org_id":"6d1b9b1e-0000-4000-8000-000000000002"}""");

            Assert.False(MarminAeWebhookVerifier.Verify(tampered, signature, [Secret]));
        }

        [Fact]
        public void Rejects_ASignatureFromAnUnconfiguredSecret()
        {
            Assert.False(MarminAeWebhookVerifier.Verify(Body, Sign(Body, OtherSecret), [Secret]));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not base64 at all")]
        [InlineData("YWJj")]                                  // valid base64, wrong length for SHA-256
        [InlineData("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef")] // hex digest
        public void Rejects_AMalformedSignatureHeader(string? signature)
        {
            Assert.False(MarminAeWebhookVerifier.Verify(Body, signature, [Secret]));
        }

        [Fact]
        public void Rejects_WhenNoSecretsAreConfigured()
        {
            // A tenant that has not had its webhook secret set must not accept anything at all.
            Assert.False(MarminAeWebhookVerifier.Verify(Body, Sign(Body, Secret), []));
        }
    }
}

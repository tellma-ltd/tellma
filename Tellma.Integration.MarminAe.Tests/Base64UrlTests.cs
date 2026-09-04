// Copyright (c) Tellma Ltd. All rights reserved.

using System.Text;

namespace Tellma.Integration.MarminAe.Tests
{
    /// <summary>
    ///     Covers <see cref="MarminAeTimestamps.DecodeBase64Url" />, the net8.0 stand-in this repo
    ///     wrote for <c>System.Buffers.Text.Base64Url</c> (which is .NET 9+).
    /// </summary>
    /// <remarks>
    ///     This is the only code in the connector that Tellma authored rather than inherited from
    ///     upstream, so it gets the most attention per line. A silently wrong decoder would not
    ///     fail loudly: it would make every JWT expiry unreadable, the token cache fall back to a
    ///     five-minute guess, and the vendor's rate-limited token endpoint get hit far more often
    ///     than it should.
    /// </remarks>
    public class Base64UrlTests
    {
        private static string ToBase64Url(byte[] bytes) =>
            Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        [Theory]
        // Lengths 1..5 walk every possible remainder mod 4, i.e. every padding case.
        [InlineData("a")]
        [InlineData("ab")]
        [InlineData("abc")]
        [InlineData("abcd")]
        [InlineData("abcde")]
        [InlineData("{\"exp\":1893456000}")]
        public void RoundTrips_TextOfEveryPaddingLength(string text)
        {
            byte[] original = Encoding.UTF8.GetBytes(text);

            byte[] decoded = MarminAeTimestamps.DecodeBase64Url(ToBase64Url(original));

            Assert.Equal(original, decoded);
        }

        [Fact]
        public void Decodes_TheTwoSubstitutedCharacters()
        {
            // 0xFB 0xFF encodes as "+/8" in standard base64 and "-_8" in base64url, so this is the
            // one case that proves both substitutions are reversed rather than passed through.
            byte[] bytes = [0xFB, 0xFF];
            string standard = Convert.ToBase64String(bytes).TrimEnd('=');
            Assert.Equal("+/8", standard);

            Assert.Equal(bytes, MarminAeTimestamps.DecodeBase64Url("-_8"));
        }

        [Fact]
        public void Accepts_PaddingThatWasNotStripped()
        {
            // Base64url normally drops padding, but a producer that leaves it on is still readable.
            Assert.Equal(Encoding.UTF8.GetBytes("ab"), MarminAeTimestamps.DecodeBase64Url("YWI="));
        }

        [Fact]
        public void Decodes_Empty_ToEmpty()
        {
            Assert.Empty(MarminAeTimestamps.DecodeBase64Url(string.Empty));
        }

        [Theory]
        [InlineData("YWJjZA==$")]   // a character outside the alphabet
        [InlineData("a")]           // remainder 1 is unreachable from valid base64url
        [InlineData("YQ==YQ==")]    // padding in the middle
        public void Throws_FormatException_OnGarbage(string value)
        {
            // FormatException specifically: FromJwtExpiry catches that and only that, so anything
            // else would escape as an unhandled exception out of a token refresh.
            Assert.Throws<FormatException>(() => MarminAeTimestamps.DecodeBase64Url(value));
        }

        [Fact]
        public void Ignores_Whitespace_LikeConvertFromBase64String()
        {
            // A documented difference from the .NET 9 Base64Url this stands in for, which rejects
            // whitespace outright. Convert.FromBase64String skips it instead, so " " decodes to
            // nothing rather than throwing. Harmless for the one caller: FromJwtExpiry hands the
            // empty result to JsonDocument.Parse, which throws JsonException and yields null.
            Assert.Empty(MarminAeTimestamps.DecodeBase64Url(" "));
            Assert.Null(MarminAeTimestamps.FromJwtExpiry("header. .sig"));
        }

        [Fact]
        public void FromJwtExpiry_ReadsTheExpiryThroughTheDecoder()
        {
            // The real consumer: an unsigned-looking three-segment JWT whose payload needs padding
            // restored before it will decode at all.
            string payload = ToBase64Url(Encoding.UTF8.GetBytes("{\"exp\":1893456000,\"sub\":\"x\"}"));
            string jwt = $"{ToBase64Url(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\"}"))}.{payload}.sig";

            Assert.Equal(
                DateTimeOffset.FromUnixTimeSeconds(1893456000),
                MarminAeTimestamps.FromJwtExpiry(jwt));
        }

        [Theory]
        [InlineData("")]
        [InlineData("not-a-jwt")]
        [InlineData("only.two")]
        [InlineData("header..sig")]
        [InlineData("header.!!!not-base64!!!.sig")]
        public void FromJwtExpiry_ReturnsNull_RatherThanThrowing(string jwt)
        {
            Assert.Null(MarminAeTimestamps.FromJwtExpiry(jwt));
        }
    }
}

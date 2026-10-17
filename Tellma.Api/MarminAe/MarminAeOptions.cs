using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Tellma.Api.MarminAe
{
    /// <summary>
    /// Cross-tenant configuration for the Marmin UAE e-invoicing integration, bound from the
    /// <c>"MarminAe"</c> configuration section.
    /// </summary>
    /// <remarks>
    /// Nothing tenant-specific belongs here. Each tenant's client id, business profile id and
    /// secrets live in that tenant's own <c>dbo.Settings</c> row, exactly as ZATCA does it.
    /// </remarks>
    public class MarminAeOptions
    {
        /// <summary>
        /// Comma-separated AES keys used to encrypt the per-tenant secrets before they are stored.
        /// Each key must be exactly 16, 24 or 32 <em>ASCII</em> characters (see
        /// <see cref="MarminAeCryptoUtil"/>). Several are allowed so keys can be rotated;
        /// <c>Settings.MarminAeEncryptionKeyIndex</c> records which one encrypted a given row.
        /// </summary>
        public string EncryptionKeys { get; set; }

        /// <summary>
        /// The API host for the Production environment. The vendor publishes one host per country
        /// per environment (it is not per account), but only the sandbox host is baked into the
        /// client, so this must be supplied before any tenant can go live.
        /// </summary>
        public string ProductionBaseAddress { get; set; }

        /// <summary>
        /// Overrides the sandbox host. Defaults to
        /// <c>MarminAeClientOptions.SandboxBaseAddress</c> when left unset.
        /// </summary>
        public string SandboxBaseAddress { get; set; }

        /// <summary>
        /// Whether the inbound webhook endpoint is mapped. Off by default: the endpoint is
        /// anonymous, and it should not be exposed until it has been tested against the vendor.
        /// </summary>
        public bool CallbacksEnabled { get; set; }

        /// <summary>Per-request timeout, in seconds. Defaults to 30.</summary>
        public int TimeoutSeconds { get; set; } = 30;
    }

    /// <summary>
    /// AES helper used to protect the per-tenant Marmin secrets at rest.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A deliberate copy of <c>Tellma.Integration.Zatca.CryptoUtil</c> rather than a reference to
    /// it, so that the two integrations use independent key material and a compromise of one set
    /// of keys does not reach the other.
    /// </para>
    /// <para>
    /// Two properties inherited from that original are worth stating plainly. The key is taken as
    /// raw UTF-8 bytes, so it must be exactly 16/24/32 <em>ASCII</em> characters -- a non-ASCII
    /// character silently changes the byte length and throws at runtime. And the IV is all zeroes,
    /// so identical plaintexts encrypt to identical ciphertexts. That is acceptable for storing a
    /// handful of API credentials at rest, but this is not a general-purpose encryption utility.
    /// </para>
    /// </remarks>
    public static class MarminAeCryptoUtil
    {
        /// <summary>Encrypts <paramref name="plainText"/> with the symmetric <paramref name="key"/>.</summary>
        /// <returns>The ciphertext, Base64-encoded.</returns>
        public static string Encrypt(string plainText, string key)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using var memoryStream = new MemoryStream();
                using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                using (var streamWriter = new StreamWriter(cryptoStream))
                {
                    streamWriter.Write(plainText);
                }

                array = memoryStream.ToArray();
            }

            return Convert.ToBase64String(array);
        }

        /// <summary>Reverses <see cref="Encrypt(string, string)"/> using the same key.</summary>
        /// <returns>The original plaintext.</returns>
        public static string Decrypt(string cipherText, string key)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream(buffer);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }
    }
}

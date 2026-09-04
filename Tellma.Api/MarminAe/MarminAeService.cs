using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Connector.MarminAe;
using Tellma.Repository.Application;

namespace Tellma.Api.MarminAe
{
    /// <summary>
    /// The application-tier entry point to the Marmin UAE e-invoicing API.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Registered as a singleton, like <c>ZatcaService</c>, but the resemblance stops there.
    /// <c>MarminAeClientOptions</c> is documented as describing <em>one organisation</em>, so
    /// there cannot be a single shared client: each tenant has its own credentials and therefore
    /// its own client and its own cached bearer token.
    /// </para>
    /// <para>
    /// Clients are cached on a fingerprint of the credentials rather than on the tenant id, which
    /// gets rotation right for free: change a secret and the fingerprint changes, so the next call
    /// builds a fresh client and the stale one is dropped. Caching matters because the vendor rate
    /// limits the token endpoint to 60 calls a minute, and a client built per call would fetch a
    /// new token every time.
    /// </para>
    /// </remarks>
    public class MarminAeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly MarminAeOptions _options;

        /// <summary>Keyed by a hash of client id + secret + base address.</summary>
        private readonly ConcurrentDictionary<string, MarminAeClient> _clients = new();

        public MarminAeService(IHttpClientFactory httpClientFactory, IOptions<MarminAeOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options?.Value ?? new MarminAeOptions();
        }

        /// <summary>
        /// Whether this tenant has enough configuration to talk to the vendor at all.
        /// </summary>
        public bool IsConfigured(SettingsForClient settings) =>
            settings != null
            && !string.IsNullOrWhiteSpace(settings.MarminAeClientId)
            && !string.IsNullOrWhiteSpace(settings.MarminAeEncryptedClientSecret)
            && !string.IsNullOrWhiteSpace(settings.MarminAeBusinessProfileId);

        /// <summary>
        /// Submits one document and reports what the vendor did with it.
        /// </summary>
        /// <remarks>
        /// Never throws for a business outcome. A refusal by the vendor comes back as
        /// <see cref="MarminAeState.SubmitFailed"/> with the detail in
        /// <see cref="MarminAeSubmissionResult.ResultJson"/>, because the caller has already
        /// committed the close and needs to record the outcome rather than unwind.
        /// </remarks>
        public async Task<MarminAeSubmissionResult> SubmitAsync(
            MarminAeInvoice invoice, SettingsForClient settings, CancellationToken cancellation)
        {
            var client = GetClient(settings);
            var profileId = settings.MarminAeBusinessProfileId;
            var kind = MarminAeMapper.ToKind(invoice.DocumentType);

            try
            {
                MarminAeResponse<MarminAeDocument> response = kind switch
                {
                    MarminAeDocumentKind.SalesInvoice => await client.CreateSalesInvoiceAsync(
                        profileId, MarminAeMapper.ToSalesInvoice(invoice), cancellation),

                    MarminAeDocumentKind.SalesCreditNote => await client.CreateSalesCreditNoteAsync(
                        profileId, MarminAeMapper.ToSalesCreditNote(invoice), cancellation),

                    _ => throw new InvalidOperationException(
                        $"Marmin document kind {kind} cannot be submitted."),
                };

                var document = response.Value;
                var peppolStatus = document?.MetaInfo?.PeppolStatus?.OverallStatus;

                return new MarminAeSubmissionResult
                {
                    // A 2xx means "accepted for transmission", not "delivered". The real outcome
                    // arrives later, via the webhook or the status poll.
                    State = peppolStatus is null
                        ? MarminAeState.Submitted
                        : MarminAeMapper.ToState(peppolStatus),

                    DocumentId = document?.Id,
                    DocumentNumber = document?.DocumentNumber,
                    ResultJson = Describe(document),

                    // Compared against the ledger by the caller and reported as a warning on a
                    // mismatch. It is already on the network, so it is never grounds to unwind.
                    VendorPayableAmount = document?.PayableAmount,
                };
            }
            catch (MarminAeRequestException ex)
            {
                return new MarminAeSubmissionResult
                {
                    State = MarminAeState.SubmitFailed,
                    ResultJson = Describe(new { ex.StatusCode, Error = ex.Detail?.Describe(), ex.Message }),
                    ErrorMessage = ex.Detail?.Describe() ?? ex.Message,
                };
            }
            catch (Exception ex) when (ex is TimeoutException or HttpRequestException)
            {
                // The request may or may not have reached the vendor. Deliberately NOT reported as
                // SubmitFailed: the caller leaves the document at Submitting, and the resubmit
                // action asks the vendor what actually happened before sending anything again.
                return new MarminAeSubmissionResult
                {
                    State = null,
                    ErrorMessage = ex.Message,
                    ResultJson = Describe(new { Error = ex.Message }),
                };
            }
        }

        /// <summary>
        /// Reads the current Peppol outcome for a document the vendor already holds.
        /// </summary>
        /// <remarks>
        /// This is what the "Refresh e-invoice status" action calls, and it resolves the status
        /// exactly the way the webhook handler does, so polling exercises the same logic.
        /// </remarks>
        public async Task<MarminAeStatusResult> GetStatusAsync(
            string marminDocumentId,
            string documentType,
            SettingsForClient settings,
            CancellationToken cancellation)
        {
            var client = GetClient(settings);
            var kind = MarminAeMapper.ToKind(documentType);

            var response = await client.GetDocumentAsync(kind, marminDocumentId, cancellation);
            var document = response.Value;
            var peppolStatus = document?.MetaInfo?.PeppolStatus?.OverallStatus;

            return new MarminAeStatusResult
            {
                State = peppolStatus is null
                    ? MarminAeState.Submitted
                    : MarminAeMapper.ToState(peppolStatus),
                ResultJson = Describe(document),
            };
        }

        /// <summary>
        /// Asks the vendor whether a document with this number already exists.
        /// </summary>
        /// <remarks>
        /// The duplicate guard for the resubmit path, and the reason Tellma sends its own
        /// document code as <c>document_number</c> instead of letting the vendor auto-number.
        /// Resubmitting blind would put a second copy of a real invoice into a real counterparty's
        /// accounts payable.
        /// </remarks>
        /// <returns>The vendor's document id if it already has one, otherwise null.</returns>
        public async Task<string> FindExistingDocumentIdAsync(
            string documentNumber,
            string documentType,
            SettingsForClient settings,
            CancellationToken cancellation)
        {
            var client = GetClient(settings);
            var kind = MarminAeMapper.ToKind(documentType);

            var query = new MarminAeDocumentQuery { DocumentNumber = documentNumber, Size = 1 };
            var response = await client.ListDocumentsAsync(kind, query, cancellation);

            return response.Value?.Content?.FirstOrDefault()?.Id;
        }

        /// <summary>
        /// Decrypts the tenant's webhook signing secrets.
        /// </summary>
        /// <remarks>
        /// Returns every secret configured, because the stored value is semicolon-separated to
        /// support rotation: during the window in which the vendor may sign with either the new or
        /// the old secret, both must be accepted.
        /// </remarks>
        public IReadOnlyList<string> GetWebhookSecrets(SettingsForClient settings)
        {
            if (string.IsNullOrWhiteSpace(settings?.MarminAeEncryptedWebhookSecret))
            {
                return [];
            }

            var plain = Decrypt(settings.MarminAeEncryptedWebhookSecret, settings.MarminAeEncryptionKeyIndex);

            return [.. plain.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
        }

        /// <summary>Encrypts a secret for storage, and reports which key was used.</summary>
        public (string cipherText, int keyIndex) Encrypt(string plainText)
        {
            var keys = EncryptionKeys();

            // Always encrypt with the newest key; the index is stored so older rows stay readable.
            int keyIndex = keys.Length - 1;

            return (MarminAeCryptoUtil.Encrypt(plainText, keys[keyIndex]), keyIndex);
        }

        #region Helpers

        /// <summary>
        /// Returns the cached client for these credentials, building one if the credentials are
        /// new or have been rotated.
        /// </summary>
        private MarminAeClient GetClient(SettingsForClient settings)
        {
            if (!IsConfigured(settings))
            {
                throw new InvalidOperationException(
                    "Marmin is not configured for this tenant: the client id, client secret and business profile id are all required.");
            }

            var clientId = settings.MarminAeClientId;
            var clientSecret = Decrypt(settings.MarminAeEncryptedClientSecret, settings.MarminAeEncryptionKeyIndex);
            var baseAddress = BaseAddress(settings.MarminAeEnvironment);

            // Hashing rather than concatenating keeps the plaintext secret out of the cache key,
            // and therefore out of any memory dump that walks the dictionary.
            var fingerprint = Convert.ToBase64String(SHA256.HashData(
                Encoding.UTF8.GetBytes($"{clientId}\n{clientSecret}\n{baseAddress}")));

            return _clients.GetOrAdd(fingerprint, _ => new MarminAeClient(
                _httpClientFactory.CreateClient(),
                new MarminAeClientOptions
                {
                    BaseAddress = baseAddress,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds <= 0 ? 30 : _options.TimeoutSeconds),
                }));
        }

        private Uri BaseAddress(string environment)
        {
            if (string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(_options.ProductionBaseAddress))
                {
                    throw new InvalidOperationException(
                        "The setting 'MarminAe:ProductionBaseAddress' must be provided before a tenant can use the Production environment.");
                }

                return new Uri(_options.ProductionBaseAddress, UriKind.Absolute);
            }

            return string.IsNullOrWhiteSpace(_options.SandboxBaseAddress)
                ? MarminAeClientOptions.SandboxBaseAddress
                : new Uri(_options.SandboxBaseAddress, UriKind.Absolute);
        }

        /// <summary>
        /// Decrypts a stored secret with the key it was stored under. Public so that a partial
        /// secrets save can carry the secret it is not replacing forward onto the new key.
        /// </summary>
        public string Decrypt(string cipherText, int keyIndex)
        {
            var keys = EncryptionKeys();
            if (keyIndex < 0 || keyIndex >= keys.Length)
            {
                throw new InvalidOperationException(
                    $"Key index {keyIndex} is outside the range of keys configured in 'MarminAe:EncryptionKeys'.");
            }

            return MarminAeCryptoUtil.Decrypt(cipherText, keys[keyIndex]);
        }

        private string[] EncryptionKeys()
        {
            var configured = _options.EncryptionKeys;
            if (string.IsNullOrWhiteSpace(configured))
            {
                throw new InvalidOperationException(
                    "The setting 'MarminAe:EncryptionKeys' must be provided in a configuration provider.");
            }

            var keys = configured.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (keys.Length == 0)
            {
                throw new InvalidOperationException("The setting 'MarminAe:EncryptionKeys' is empty.");
            }

            return keys;
        }

        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        /// <summary>
        /// Serializes whatever we learned, for the MarminAeResult column and the alert email.
        /// Never throws: a document with an unexpected shape must not turn a recorded outcome into
        /// an unrecorded one.
        /// </summary>
        private static string Describe(object value)
        {
            if (value is null)
            {
                return null;
            }

            try
            {
                return JsonSerializer.Serialize(value, _jsonOptions);
            }
            catch (Exception ex)
            {
                return $"{{ \"SerializationError\": \"{ex.Message}\" }}";
            }
        }

        #endregion
    }

    /// <summary>The outcome of one submission attempt.</summary>
    public class MarminAeSubmissionResult
    {
        /// <summary>
        /// The state to record, or null when the attempt was inconclusive (a timeout or a
        /// transport failure) and the document should stay claimed for a later resubmit.
        /// </summary>
        public MarminAeState? State { get; set; }

        /// <summary>The vendor's id for the document, once it has one.</summary>
        public string DocumentId { get; set; }

        /// <summary>The document number as the vendor recorded it.</summary>
        public string DocumentNumber { get; set; }

        /// <summary>The response or the refusal, serialized for the MarminAeResult column.</summary>
        public string ResultJson { get; set; }

        /// <summary>Set when the attempt did not succeed; drives the alert to tenant admins.</summary>
        public string ErrorMessage { get; set; }

        /// <summary>The total the vendor computed, for reconciliation against the ledger.</summary>
        public decimal? VendorPayableAmount { get; set; }

        /// <summary>True when the vendor accepted the document onto the network.</summary>
        public bool IsSuccess => State is MarminAeState.Submitted or MarminAeState.Delivered;
    }

    /// <summary>The outcome of one status read.</summary>
    public class MarminAeStatusResult
    {
        /// <summary>The state to record.</summary>
        public MarminAeState State { get; set; }

        /// <summary>The status payload, serialized for the MarminAeResult column.</summary>
        public string ResultJson { get; set; }
    }
}

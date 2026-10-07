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
        /// <summary>Page size when searching the vendor for a document by number.</summary>
        private const int FindPageSize = 50;

        /// <summary>
        /// How many pages to read before giving up. A number filter should match one document;
        /// this bound only exists so a vendor that ignores the filter cannot make us read forever.
        /// </summary>
        private const int FindMaxPages = 5;

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
        /// Whether this tenant has the three credentials needed to talk to the vendor at all.
        /// </summary>
        /// <remarks>
        /// Presence only. <see cref="Validate(SettingsForClient)"/> goes further and proves the
        /// configuration actually works.
        /// </remarks>
        public bool IsConfigured(SettingsForClient settings) =>
            settings != null
            && !string.IsNullOrWhiteSpace(settings.MarminAeClientId)
            && !string.IsNullOrWhiteSpace(settings.MarminAeEncryptedClientSecret)
            && !string.IsNullOrWhiteSpace(settings.MarminAeBusinessProfileId);

        /// <summary>
        /// Proves that this tenant's configuration can actually reach the vendor: the credentials
        /// are present, the environment resolves to an API host, and the stored secret decrypts.
        /// </summary>
        /// <remarks>
        /// Called before a document is claimed, while the close can still be rolled back. Each of
        /// these would otherwise only surface after the close had committed, leaving the document
        /// claimed and unsent: a Production tenant whose <c>MarminAe:ProductionBaseAddress</c> is
        /// blank, or a secret encrypted under a key that has since been removed from
        /// <c>MarminAe:EncryptionKeys</c>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">With a message saying what is wrong.</exception>
        public void Validate(SettingsForClient settings)
        {
            if (!IsConfigured(settings))
            {
                throw new InvalidOperationException(
                    "The Marmin client id, client secret and business profile id are all required.");
            }

            _ = BaseAddress(settings.MarminAeEnvironment);

            try
            {
                _ = Decrypt(settings.MarminAeEncryptedClientSecret, settings.MarminAeEncryptionKeyIndex);
            }
            catch (Exception ex) when (ex is CryptographicException or FormatException)
            {
                throw new InvalidOperationException(
                    "The stored Marmin client secret cannot be decrypted with the configured 'MarminAe:EncryptionKeys'. Set the secret again.", ex);
            }
        }

        /// <summary>
        /// Sends one prepared document and reports what the vendor did with it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A POST for a new document, or a PUT that replaces one the vendor already holds when
        /// <see cref="MarminAePreparedSubmission.ExistingDocumentId"/> is set. The vendor accepts a
        /// PUT only while that document is VALIDATION_FAILED.
        /// </para>
        /// <para>
        /// Never throws for a business outcome, because the caller has already committed the close
        /// and needs to record the outcome rather than unwind. Returns <c>State = null</c> when the
        /// outcome is unknown -- the request may or may not have reached the vendor -- so that the
        /// document stays at <see cref="MarminAeState.SentAwaitingOutcome"/>.
        /// </para>
        /// </remarks>
        public async Task<MarminAeSubmissionResult> SubmitAsync(
            MarminAePreparedSubmission prepared, SettingsForClient settings, CancellationToken cancellation)
        {
            ArgumentNullException.ThrowIfNull(prepared);

            var client = GetClient(settings);
            var profileId = settings.MarminAeBusinessProfileId;

            try
            {
                MarminAeResponse<MarminAeDocument> response = (prepared.Kind, prepared.IsResubmission) switch
                {
                    (MarminAeDocumentKind.SalesInvoice, false) => await client.CreateSalesInvoiceAsync(
                        profileId, prepared.Invoice, cancellation),

                    (MarminAeDocumentKind.SalesInvoice, true) => await client.ResubmitSalesInvoiceAsync(
                        profileId, prepared.ExistingDocumentId, prepared.Invoice, cancellation),

                    (MarminAeDocumentKind.SalesCreditNote, false) => await client.CreateSalesCreditNoteAsync(
                        profileId, prepared.CreditNote, cancellation),

                    (MarminAeDocumentKind.SalesCreditNote, true) => await client.ResubmitSalesCreditNoteAsync(
                        profileId, prepared.ExistingDocumentId, prepared.CreditNote, cancellation),

                    _ => throw new InvalidOperationException(
                        $"Marmin document kind {prepared.Kind} cannot be submitted."),
                };

                var document = response.Value;
                var peppolStatus = document?.MetaInfo?.PeppolStatus?.OverallStatus;
                var state = peppolStatus is null ? MarminAeState.Submitted : MarminAeMapper.ToState(peppolStatus);

                return new MarminAeSubmissionResult
                {
                    // A 2xx means "accepted for transmission", not "delivered". The real outcome
                    // arrives later, via the webhook or the status poll. A verdict already present
                    // in the response is recorded as given.
                    State = state,
                    DocumentId = document?.Id,
                    DocumentNumber = document?.DocumentNumber,
                    ResultJson = Describe(document),

                    // Accepted but already failing is still a failure the tenant should hear about.
                    ErrorMessage = state is MarminAeState.PeppolValidationFailed or MarminAeState.PeppolRejected
                        ? $"The vendor accepted the document but Peppol reported {peppolStatus}."
                        : null,

                    // Compared against the ledger by the caller and reported as a warning on a
                    // mismatch. It is already on the network, so it is never grounds to unwind.
                    VendorPayableAmount = document?.PayableAmount,
                };
            }
            catch (MarminAeRequestException ex) when (prepared.IsResubmission)
            {
                // The vendor refused to replace a document it still holds -- most often because it
                // is no longer VALIDATION_FAILED. Its state is therefore whatever it already was, so
                // read it rather than guess: recording SubmitFailed here would claim a document
                // that IS at the vendor had never reached it.
                var refusal = ex.Detail?.Describe() ?? ex.Message;
                try
                {
                    var status = await ReadStatusAsync(client, prepared.Kind, prepared.ExistingDocumentId, cancellation);
                    return new MarminAeSubmissionResult
                    {
                        State = status.State,
                        ResultJson = Describe(new { ex.StatusCode, Refusal = refusal, CurrentStatus = status.ResultJson }),
                        ErrorMessage = $"The vendor refused the resubmission: {refusal}",
                    };
                }
                catch (Exception readEx) when (readEx is MarminAeRequestException or TimeoutException or HttpRequestException)
                {
                    // Unknown: leave the document awaiting an outcome for Refresh to settle.
                    return new MarminAeSubmissionResult
                    {
                        State = null,
                        ResultJson = Describe(new { ex.StatusCode, Refusal = refusal }),
                        ErrorMessage = $"The vendor refused the resubmission ({refusal}), and its current status could not be read ({readEx.Message}).",
                    };
                }
            }
            catch (MarminAeRequestException ex)
            {
                // A new document refused outright never reached the network.
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
                // SubmitFailed: the document stays at SentAwaitingOutcome, and Refresh or Resubmit
                // ask the vendor what actually happened before anything is sent again.
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

            return await ReadStatusAsync(client, kind, marminDocumentId, cancellation);
        }

        /// <summary>
        /// Asks the vendor whether it already holds a document with this number, and if so
        /// returns its id.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The duplicate guard for every path that might re-send a document whose earlier
        /// submission had an unknown outcome, and the reason Tellma sends its own document code as
        /// <c>document_number</c> instead of letting the vendor auto-number. Resubmitting blind
        /// would put a second copy of a real invoice into a real counterparty's accounts payable.
        /// </para>
        /// <para>
        /// The vendor does not document whether its number filter is exact, a prefix or a
        /// contains match, so the result is never trusted as-is: every page is read and the match
        /// is made here, by <see cref="PickExistingDocumentId"/>.
        /// </para>
        /// </remarks>
        /// <returns>The vendor's document id if it holds exactly one, otherwise null.</returns>
        /// <exception cref="InvalidOperationException">
        /// The vendor holds more than one document with this number, or more than a few pages
        /// match; either way there is no safe answer.
        /// </exception>
        public async Task<string> FindExistingDocumentIdAsync(
            string documentNumber,
            string documentType,
            SettingsForClient settings,
            CancellationToken cancellation)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(documentNumber);

            var client = GetClient(settings);
            var kind = MarminAeMapper.ToKind(documentType);
            var candidates = new List<MarminAeDocument>();

            for (int page = 0; page < FindMaxPages; page++)
            {
                var query = new MarminAeDocumentQuery { DocumentNumber = documentNumber, Page = page, Size = FindPageSize };
                var response = await client.ListDocumentsAsync(kind, query, cancellation);
                var content = response.Value?.Content ?? [];
                candidates.AddRange(content);

                bool isLastPage = content.Count < FindPageSize
                    || (response.Value?.TotalPages is int totalPages && page + 1 >= totalPages);

                if (isLastPage)
                {
                    return PickExistingDocumentId(candidates, documentNumber, settings.MarminAeBusinessProfileId);
                }
            }

            throw new InvalidOperationException(
                $"More than {FindMaxPages * FindPageSize} vendor documents match the number '{documentNumber}', so it cannot be determined whether this one was already submitted.");
        }

        /// <summary>
        /// Picks the one candidate that is this document: an exact, case-sensitive number match,
        /// issued by this tenant's business profile.
        /// </summary>
        /// <remarks>
        /// Public and static only so it can be tested without a vendor. The profile is checked only
        /// when the vendor reports one, because its responses leave optional fields out.
        /// </remarks>
        /// <exception cref="InvalidOperationException">More than one candidate matches.</exception>
        public static string PickExistingDocumentId(
            IEnumerable<MarminAeDocument> candidates, string documentNumber, string businessProfileId)
        {
            var ids = (candidates ?? [])
                .Where(d => d != null && string.Equals(d.DocumentNumber, documentNumber, StringComparison.Ordinal))
                .Where(d => string.IsNullOrWhiteSpace(businessProfileId)
                    || string.IsNullOrWhiteSpace(d.AccountingSupplierParty?.ProfileId)
                    || string.Equals(d.AccountingSupplierParty.ProfileId, businessProfileId, StringComparison.Ordinal))
                .Select(d => d.Id)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            return ids.Count switch
            {
                0 => null,
                1 => ids[0],
                _ => throw new InvalidOperationException(
                    $"The vendor holds {ids.Count} documents numbered '{documentNumber}', so it cannot be determined which one is this document."),
            };
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

        #region Helpers

        private static async Task<MarminAeStatusResult> ReadStatusAsync(
            MarminAeClient client, MarminAeDocumentKind kind, string marminDocumentId, CancellationToken cancellation)
        {
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

        /// <summary>
        /// The API host for the tenant's environment.
        /// </summary>
        /// <remarks>
        /// Throws on anything but the two known values rather than defaulting to the sandbox.
        /// <c>dbo.Settings</c> has a CHECK constraint to the same effect, but a silent fall-through
        /// here would route a value such as ZATCA's 'Simulation' to the sandbox host while the SQL
        /// reopen guard treated the same tenant as live.
        /// </remarks>
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

            if (string.Equals(environment, "Sandbox", StringComparison.OrdinalIgnoreCase))
            {
                return string.IsNullOrWhiteSpace(_options.SandboxBaseAddress)
                    ? MarminAeClientOptions.SandboxBaseAddress
                    : new Uri(_options.SandboxBaseAddress, UriKind.Absolute);
            }

            throw new InvalidOperationException(
                $"Unrecognized Marmin environment '{environment}'. It must be 'Sandbox' or 'Production'.");
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
        /// The state to record, or null when the outcome is unknown (a timeout or a transport
        /// failure) and the document should stay at SentAwaitingOutcome for Refresh to settle.
        /// </summary>
        public MarminAeState? State { get; set; }

        /// <summary>The vendor's id for the document, once it has one.</summary>
        public string DocumentId { get; set; }

        /// <summary>The document number as the vendor recorded it.</summary>
        public string DocumentNumber { get; set; }

        /// <summary>The response or the refusal, serialized for the MarminAeResult column.</summary>
        public string ResultJson { get; set; }

        /// <summary>Set whenever the tenant's administrators should be told; drives the alert.</summary>
        public string ErrorMessage { get; set; }

        /// <summary>The total the vendor computed, for reconciliation against the ledger.</summary>
        public decimal? VendorPayableAmount { get; set; }

        /// <summary>True when the vendor accepted the document and nothing about it needs attention.</summary>
        public bool IsSuccess => ErrorMessage == null
            && State is MarminAeState.Submitted or MarminAeState.Delivered;
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

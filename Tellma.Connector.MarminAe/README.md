# Tellma.Connector.MarminAe

A first-party client for the slice of the Marmin UAE e-invoicing API the platform needs: the
signature-for-token exchange, sales document submission and resubmission, the uniform read surface
over all four document families, Peppol transmission status, legal artifact downloads, and webhook
signature verification. **Zero dependencies** — no packages, no project references — so whatever
shape a future Marmin UAE adapter takes, it consumes this unchanged.

## Why `MarminAe` and not `Marmin`

The vendor slot in a connector package names the external system, not the corporation. Marmin's
country deployments are distinct external systems: separately hosted, separately versioned, and
shaped by different regulatory models — the UAE's decentralised five-corner exchange against Saudi
Arabia's clearance model. A Saudi integration would be a sibling package with a genuinely different
client, so an unqualified name would either falsely claim that scope or quietly come to mean the UAE
one.

## Why a first-party client

Marmin publishes no .NET client of any kind, for any of its deployments, so the usual question — is
the upstream client fit — does not arise. What is needed is a dozen routes, one signature recipe,
and a set of payload models, all of which the shared framework covers: `HttpClient`,
`System.Text.Json`, `HMACSHA256`, `Base64Url`, `TimeProvider`.

## What is in it

**[MarminAeClient](MarminAeClient.cs)** — one method per operation over an injected `HttpClient`.
Dedicated submission methods for the two sales families, since an invoice and a credit note differ
in what the vendor requires; family-parameterised reads, since the vendor keeps the read surface
uniform across all four. Every result carries the status and the parsed quota headers alongside the
value.

**[MarminAeTokenProvider](MarminAeTokenProvider.cs)** — the signature-for-token exchange, cached
with a refresh margin so a request never departs carrying a token that dies in flight. Concurrent
callers needing a refresh share one token request rather than stampeding an endpoint that is rate
limited like every other. The signature is recomputed per request, and the bearer is attached per
request rather than to the transport's default headers, which together are what let a rotated secret
take effect on the next call instead of the next restart.

**[MarminAeWebhookVerifier](MarminAeWebhookVerifier.cs)** and
**[MarminAeWebhookEventParser](MarminAeWebhookEventParser.cs)** — the two vendor-coupled halves of
receiving a notification, both hosting-agnostic. Receiving, dispatching, and fetching on notify
belong to whatever hosts them.

Everything else is models: the two submission types, the one document type the four families read
into, the two differently shaped transmission-status payloads, the business profile, and the error
model.

## The parts that are load-bearing

**The API version is pinned in code.** Every request carries `X-MARMIN-VERSION` from one constant.
The vendor versions this API by date and documents each version separately, so a bump changes wire
contracts: it is a deliberate package release with the live suite re-run, never a setting.

**Server-owned fields are unsendable, not merely undocumented.** The vendor derives the supplier
party from the business profile in the route and overwrites anything submitted, and computes every
total itself. The submission types therefore have no supplier, no identifiers, no sequence number,
no totals and no transmission metadata — the compiler enforces what the vendor's own notes ask of
integrators.

**Submitting and resubmitting share one type.** Resubmission is a whole-document replacement at the
vendor: it accepts no partial change and clears what a payload omits. Sharing the type makes "send
everything you would have sent the first time" structural rather than a rule someone has to
remember.

**Acceptance is not the end of the story.** A submission answered `201` has passed the vendor's own
checks only; validation and transmission run afterwards on two independent legs — delivery to the
buyer's provider, reporting to the authority — and can fail a document that was accepted. Those
outcomes are **data, never exceptions**: read `meta_info.peppol_status.overall_status` on the
document, or the status routes, and branch on the constants in
[MarminAePeppolStatus](MarminAePeppolStatus.cs). The vendor does not publish that vocabulary
exhaustively, so the set stays open and an unrecognised value passes through verbatim.

**Two status shapes, not one.** `MarminAePeppolStatusSnapshot` (from the status route, naming the
two transmission legs, camel-cased on the wire) and `MarminAeDocumentPeppolStatus` (summarised on
the document itself, snake-cased) are different payloads under similar names. Conflating them is the
easiest mistake to make here.

**Oversized submissions are refused locally.** The vendor caps a submission at 8 MiB including
Base64 attachments, which add about a third to their size on disk — its own wording says "8MB",
which is ambiguous, so the boundary was measured: 8,099,998 bytes reach validation and 8,499,998 are
refused. The client measures the serialised payload before obtaining a token and refuses an
oversized one with both byte counts in the message, rather than spending a token request and a unit
of quota to be told `413`.

**Retry-free beyond one re-authentication.** A `401` invalidates the cached token and repeats the
request exactly once; an unauthenticated request did no work at the vendor whatever its verb, so
that retry cannot duplicate a document. Nothing else is ever retried. Durable retry belongs to the
caller, which owns the durability and can be idempotent by design; a retry here would silently
duplicate legal documents the caller believes failed.

**Throttling is signal, not something to absorb.** The vendor allows sixty document calls a minute
and thirty business-profile calls, and answers a breach with quota headers and a retry hint. Those
are parsed onto every result and every refusal — not only the throttled ones, because by the time a
refusal arrives the caller has already been throttled. Pacing is the caller's policy; the client's
job is an honest reading.

**The deployment and the reference disagree, and the deployment wins.** Three places where they do,
all covered by recorded vectors: the token response carries `expires_in`, not the documented
`expires_at`; the quota headers are `RateLimit-*` and `X-RateLimit-*-Minute`, not the documented
`X-RateLimit-*`, and the reset is a wait in seconds rather than an instant; and a time comes back
as `HH:mm` where it is documented as `HH:mm:ss`. Each is read under every spelling, because reading
only the documented one produces a silent null that looks like an answer.

**Reads are tolerant, writes are strict.** No response model declares a required member: the vendor
omitting one field would otherwise fail the read of an entire document. Unknown fields are ignored,
unknown status strings pass through, and two fields the vendor spells two different ways depending
on the endpoint — the tax-breakdown category and rate, and a line's unit price — are read under both
names with a computed member reporting whichever arrived.

**Error bodies are read as far as they can be.** The vendor publishes no schema for them, so the
parser recognises the shapes it has seen, always keeps a size-capped copy of the body verbatim, and
never throws: a body nothing could be read out of is exactly the one somebody will need to look at.

## Using it

```csharp
MarminAeClientOptions options = new()
{
    BaseAddress = MarminAeClientOptions.SandboxBaseAddress,
    ClientId = configuration["MarminAe:ClientId"]!,
    ClientSecret = configuration["MarminAe:ClientSecret"]!,
};

MarminAeClient client = new(httpClient, options);

MarminAeResponse<MarminAeDocument> created = await client.CreateSalesInvoiceAsync(
    profileId, invoice, cancellationToken);

// Accepted, not cleared: poll the document or wait for a notification before treating it as issued.
string? outcome = created.Value.MetaInfo?.PeppolStatus?.OverallStatus;
```

Both the client and the token provider are thread-safe and meant to live as long as the credentials
they were built with. A download is not: it holds the response open and the caller disposes it.

## What is deliberately absent

Proforma invoices, the sale and purchase registers, the business-profile claim and update flows, the
static code lists, and authoring on the purchase side — none of them has a consumer yet, and the
client grows additively when one appears. Tax semantics are absent too: which scenario flags to set,
which tax category applies, which conditional fields a supply obliges. The client transmits what it
is given; the rule knowledge belongs to the layer that knows the supply.

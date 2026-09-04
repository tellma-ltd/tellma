# UAE e-invoicing (Marmin) in Tellma

Two UAE customers must transmit their invoices over the Peppol network. Tellma posts sales
invoices and credit notes to the Marmin UAE API when a document is **closed**, records the outcome
on the document, and refuses to reopen a document that is already on the network.

This first half of the README is Tellma-specific. Everything from
[Tellma.Connector.MarminAe](#tellmaconnectormarminae) onward documents the vendor client itself,
which was copied in from `tellma-platform` and is kept diffable against its upstream original.

---

## How it fits together

| Layer | Where |
|---|---|
| Vendor client (no Tellma dependencies) | `Tellma.Integration.MarminAe/` |
| Service, mapper, options, webhook handler | `Tellma.Api/MarminAe/` |
| Webhook endpoint (middleware) | `Tellma.Api.Web/Services/MarminAeCallbackExtensions.cs` |
| Submission trigger | `Tellma.Api/DocumentsService.cs` → `UpdateDocumentState` |
| Read path | `dal.MarminAe__GetInvoices` |
| Writes | `dal.MarminAe__MarkSubmitting`, `dal.MarminAe__UpdateDocumentInfo`, `dal.MarminAe__ApplyWebhook`, `dal.MarminAe__SaveSecrets` |
| Guards | `bll.Documents_Validate__Close`, `bll.Documents_Validate__Open`, `bll.DocumentDefinitions_Validate__Save` |
| Migration | `tools/Migrations/2026-09-MarminAe.sql` (generated) |

### What happens on close

1. **Inside the close transaction** — the document closes, `dal.MarminAe__GetInvoices` maps it,
   and `dal.MarminAe__MarkSubmitting` stamps `MarminAeState = 0`. The transaction commits.
2. **Outside any transaction** — the vendor call.
3. **In its own transaction** — the outcome is written.

This differs from ZATCA deliberately. ZATCA holds a transaction open across the HTTP call and
rolls back on failure, and carries a standing TODO about the window where a crash *after* a
successful call leaves the document reopenable and therefore re-submittable. A duplicate ZATCA
report is a compliance annoyance; a duplicate Peppol transmission lands in a real counterparty's
accounts payable. Splitting the work means the accounting is durable before anything is sent, and
a document can never be picked up for submission twice.

A failure in step 2 or 3 leaves the document at `Submitting`. **Refresh e-invoice status**
recovers it, and asks the vendor whether the original landed before anything is re-sent.

### Document states

`MarminAeState` on `dbo.Documents`:

| Value | Meaning |
|---|---|
| `0` Submitting | Claimed, not yet acknowledged. Still reopenable. |
| `1` Submitted | The vendor accepted it. Peppol validation is in progress. |
| `10` Delivered | Peppol confirmed delivery. |
| `-10` SubmitFailed | The vendor refused it; it never reached the network. |
| `-20` PeppolRejected | Accepted, then rejected by Peppol validation or delivery. |

Reopening is blocked from `1` upward (`bll.Documents_Validate__Open`). That single guard also
covers delete and cancel, which already require the document to be open first.

---

## Document Definition author guide

This is what a consultant configuring a tenant needs. **The emphasis is on what differs from
ZATCA**, since that setup is already familiar.

### Same as ZATCA

- The document's `NotedAgentId` points at the **Sales Invoice** agent, whose `Agent1Id` is the
  **Customer Account**, whose `Agent1Id` is the **Customer**. The whole mapping hangs off this chain.
- Invoice lines are the entries hitting an account whose type `Concept` is
  `CurrentValueAddedTaxPayables`, with the item in `NotedResourceId`.
- Resource `Lookup3Id` → VAT category (`S`/`Z`/`E`/`O`); `Lookup4Id` → VAT exemption reason.
- Sales-invoice agent `Lookup1Id` → payment means; `BankAccountNumber` → payee account.
- `Documents.Lookup1Id` → the supply-scenario flag code (ZATCA's `InvoiceTypeTransactions`;
  Marmin's `profile_execution_id`). Same slot, same idea, **different code vocabulary**.
- `Documents.Lookup2Id` → reason for issuance on credit notes.
- The e-invoice type is set on the **document definition screen**, as `ZatcaDocumentType` is.

### Different from ZATCA — read carefully

| Topic | ZATCA | Marmin (UAE) |
|---|---|---|
| Definition fields | one: `ZatcaDocumentType` = `381`/`383`/`386`/`388`/`389` | **two**: `MarminAeDocumentType` = `SalesInvoice`/`SalesCreditNote` (picks the route), plus `MarminAeTypeCode` (the wire code) — see [Code values](#code-values) |
| Required lookup definitions | `Lookup1DefinitionId` (ITT) **and** `Lookup2DefinitionId` | the same two, except `Lookup1DefinitionId` may be skipped when `MarminAeDefaultProfileExecutionId` is set in General Settings, and `Lookup2DefinitionId` is needed only on credit notes |
| Scenario flags | `Documents.Lookup1Id` → an `ITT…` lookup encoding **7** flags | same slot, but **8** flags and a **different vocabulary** — populate the Lookup Definition from Marmin's docs, never by copying the KSA `ITT…` codes |
| `Lookup2Id` codes | KSA reason-for-issuance text | the **UAE `discrepancy_response` codes**, and **required** on every credit note |
| Customer email | not required | **Required.** `Agents.ContactEmail` on the customer account or the customer; close is blocked without it |
| Customer tax id | `TaxIdentificationNumber`, falling back to `Text1` (CRN) | **`TaxIdentificationNumber` only** — it *is* the Peppol `endpoint_id`. No CRN fallback |
| Address province | free text | `Agents.AddressProvince` must be a three-letter **emirate code** (`AUH`, `DXB`, `SHJ`, `UAQ`, `FUJ`, `AJM`, `RAK`) — not the two-letter ISO subdivisions; close is blocked otherwise |
| Item description | not sent | **Required.** `Resources.Description` (falls back to `Name`) |
| Unit codes | sent, lenient in practice | `Units.Code` must be **UN/ECE Rec 20** (`PCE`, `KGM`, `HUR`, …); close is blocked if empty |
| Names / language | `Name2` (Arabic mandate) | **`Name`** (Latin) |
| VAT rate | default 15%, sent as a 0–1 fraction | default **5%**, sent as a **percentage** (the SP multiplies by 100) |
| Due date | n/a | `Documents.NotedDate` — **relabel it "Due Date"** on the definition, or it falls back to issue date + `MarminAeDefaultPaymentTermDays` |
| Credit note linkage | `fn_Document__BillingReferenceId` | the original is found via the same `NotedAgentId`; **exactly one** posted Marmin invoice must match, else close is blocked |
| Numbering | ZATCA assigns a serial + hash chain | Tellma's `Documents.Code` is sent as `document_number` — **turn auto-numbering OFF in the Marmin organisation** |
| Reopen / delete / cancel | blocked once reported | blocked once **submitted**, which is stricter |
| Outcome timing | synchronous at close | close returns *Submitted*; delivery lands later, via the webhook or **Refresh e-invoice status** |

> **Totals are computed by the vendor, not by Tellma.** That makes a line-mapping error silent: a
> legally transmitted invoice whose total differs from the ledger. `bll.Documents_Validate__Close`
> recomputes the VAT exactly the way the SP will emit it and blocks the close on a mismatch. If a
> tenant uses the `Discounts` resource definition, document-level allowances must be modelled
> before that tenant goes live — the check will fail the close until they are.

---

## Tenant setup

1. **Environment.** `dbo.Settings.MarminAeEnvironment` is DBA-set (`Sandbox` or `Production`),
   deliberately not editable in the browser, so a tenant cannot be flipped to Production from a form.
2. **General Settings → UAE E-Invoicing (Marmin)** — fill in Client Id, Business Profile Id,
   Organization Id, Peppol Endpoint Scheme (`0235` for a UAE TRN), and optionally the default
   profile execution id, payment means code and payment term.
3. **Set Secrets** on that same screen — the client secret, and the webhook signing secret if the
   webhook is in use. Neither is ever sent back to the browser; leave a field empty to keep the
   stored value.
4. **Document definitions** — set `MarminAeDocumentType` and `MarminAeTypeCode` on the sales
   invoice and credit note definitions.
5. **Master data** — see the author guide above.

---

## Testing against the sandbox

```bash
# from the repo root
dotnet user-secrets --project Tellma.Api.Web set "MarminAe:EncryptionKeys" "<32 ASCII chars>"
dotnet user-secrets --project Tellma.Api.Web set "MarminAe:SandboxBaseAddress" "https://api-sandbox.ae.marmin.ai"
```

Each key must be exactly 16, 24 or 32 **ASCII** characters — the AES helper takes the key as raw
UTF-8 bytes, so a non-ASCII character silently changes the length and throws at runtime.

Then:

```bash
dotnet run --project Tellma.Api.Web
```

```bash
cd Tellma.Api.Web/ClientApp && ng serve -o
```

Sign in as `admin@tellma.com` / `Admin@123`, configure the tenant as above, close an invoice, and
watch `MarminAeState`. Confirm the document appears in the Marmin sandbox portal.

Then exercise the rest:

- **Refresh e-invoice status** (`PUT api/documents/{id}/refresh-marmin-ae-status`) — the state
  should advance as the sandbox works through Peppol validation. Invoke it twice: the second call
  must be a no-op, which is the dedup/ordering guard in `dal.MarminAe__ApplyWebhook` working.
- **Reopen, delete and cancel** the submitted document — all three must be refused.
- **A credit note** against it — `billing_reference` must resolve to the original.
- **Negative paths** — a customer with no `ContactEmail`, a resource with no unit code, and a
  deliberately mismatched total must each be blocked at close with a clear message.

### The webhook

Shipped but **not yet live-tested**, so `MarminAe:CallbacksEnabled` defaults to `false`.

The route is `POST /api/marmin-ae-callback/{tenantId}` — the tenant id is in the URL because the
vendor's payload carries only *its* org and profile ids, and this repo has no lookup from either.
Each customer registers its own URL in the Marmin portal. The URL is not a secret: the body is
HMAC'd with that tenant's signing secret, so naming the wrong tenant simply fails verification.

To test it, expose the dev server with a dev tunnel or ngrok, enable
`MarminAe:CallbacksEnabled`, register `https://<tunnel-host>/api/marmin-ae-callback/<tenantId>`,
and set the webhook secret. `GET` on the route returns a welcome message, which is the quickest
way to confirm the endpoint is reachable.

The signing secret may hold two values separated by a semicolon (`new;old`) so that both are
accepted during a rotation.

---

## Database migration

`tools/Migrations/2026-09-MarminAe.sql` is **generated** — run
`python tools/Migrations/generate-marmin-ae-migration.py` rather than editing it. The generator
copies every object body verbatim out of `Tellma.Database.Application`, rewriting only
`CREATE …` to `CREATE OR ALTER …`, so the migration and the database project cannot drift apart.

It exists because a full SSDT publish is too slow and too fragile across the production databases.
It is safe to re-run: the column additions are guarded and every object is `CREATE OR ALTER`.

**Take a backup first, and run it in a maintenance window.** Section 2 drops and recreates the
`dbo.DocumentDefinitionList` table type — a table type cannot be `ALTER`ed, and SQL Server refuses
to drop one while any procedure references it, so its three dependent procedures are dropped and
recreated around it. A transaction cannot span the `GO` batches that `CREATE PROCEDURE` requires,
so a failure part-way through leaves those three procedures missing. Saving a document definition
fails for the duration.

```bash
sqlcmd -S . -E -d "Tellma.101" -i tools/Migrations/2026-09-MarminAe.sql -b
```

---

## Production app settings (Azure App Service — `__` is the section separator)

| Key | Value | Notes |
|---|---|---|
| `MarminAe__EncryptionKeys` | comma-separated AES keys | **Key Vault reference.** Each key exactly 16/24/32 ASCII characters. Kept separate from `Zatca__EncryptionKeys` so the two integrations have independent blast radius. |
| `MarminAe__ProductionBaseAddress` | *obtain from Marmin* | The client hardcodes only the sandbox host. The vendor publishes one host per country per environment — it is **not** per account — so one value serves both tenants. Confirm the hostname with Marmin rather than guessing it. |
| `MarminAe__SandboxBaseAddress` | optional | Defaults to `MarminAeClientOptions.SandboxBaseAddress`. |
| `MarminAe__CallbacksEnabled` | `false` for now | Gates the webhook middleware. Turn on once it has been live-tested. |
| `MarminAe__TimeoutSeconds` | optional, `30` | |

**Nothing tenant-specific belongs in app settings.** Each tenant's client id, business profile id
and secrets live in that tenant's own `dbo.Settings` row, exactly as ZATCA does it.

---

## Code values

Confirmed against <https://docs.ae.marmin.ai/docs/2026-05-07> and the live sandbox code-list
endpoints (`GET https://api-sandbox.ae.marmin.ai/api/codelist/{list}`, unauthenticated), which are
the authority — prefer them over this table if the two ever disagree.

### `MarminAeTypeCode` (the definition field)

| Kind | Code | Meaning | Line-item rule the vendor enforces |
|---|---|---|---|
| Sales invoice | `380` | Commercial / tax invoice. Use when the business profile is VAT registered and the supply is a standard taxable supply. | At least one line must use a standard-rate category (`S`, `AE`) — not only `E`/`O`/`Z`/`N` (Peppol IBR-151-AE) |
| Sales invoice | `480` | Invoice out of scope of tax. Use when the profile is **not** VAT registered, or the supply is outside UAE VAT scope. | Lines, charges and allowances must use `E`, `Z` or `O` only (Peppol IBR-122-AE) |
| Sales credit note | `381` or `81` | Credit note. | — |

So the choice of `380` vs `480` is a property of the **supplier's VAT position**, which is why it
belongs on the definition rather than being derived per document.

### `profile_execution_id` (`Documents.Lookup1Id`)

Exactly 8 characters, each `0` or `1` — the vendor validates `^[0-1]{8}$`. Each position is one
supply scenario (`GET /api/codelist/transaction-type-codes`):

| Position | Scenario | Also makes required |
|---|---|---|
| 1 | Free Trade Zone | `buyer_customer_party` (beneficiary id) |
| 2 | Deemed Supply | — (and `payment_means` becomes optional) |
| 3 | Profit Margin Scheme | |
| 4 | Summary Invoice | `invoice_period` |
| 5 | Continuous Supply | |
| 6 | Agent Billing | `seller_supplier_party` (principal id) |
| 7 | Supply Through E-commerce | |
| 8 | Exports | `delivery`, with a non-`AE` country code |

A plain domestic taxable supply is therefore `00000000`. Populate the tenant's Lookup Definition
with the combinations they actually issue, or set one `MarminAeDefaultProfileExecutionId`.

> The conditional fields in the right-hand column are **not** modelled in v1. A tenant issuing
> exports, free-zone, agent-billing or summary invoices needs that work first.

### `discrepancy_response` (`Documents.Lookup2Id`, credit notes)

Per UAE FTA DL8.61.1 (`GET /api/codelist/credit-note-reason-codes`):

| Code | Reason |
|---|---|
| `DL8.61.1.A` | The supply was cancelled |
| `DL8.61.1.B` | The tax treatment changed because the nature of the supply changed |
| `DL8.61.1.C` | The previously agreed consideration was altered (e.g. bad debt relief) |
| `DL8.61.1.D` | Goods or services were returned in full or in part and consideration was returned |
| `DL8.61.1.E` | Tax was charged, or a tax treatment applied, in error |
| `VD` | Volume discount |

### Other lists worth knowing

- **Tax categories** (`Resources.Lookup3Id`): `S` 5% standard, `Z` zero rated, `E` exempt,
  `O` not subject to tax, `AE` VAT reverse charge, `N` standard rate additional VAT.
- **Emirates** (`Agents.AddressProvince`): `AUH`, `DXB`, `SHJ`, `UAQ`, `FUJ`, `AJM`, `RAK`.
- **Payment means** (sales-invoice agent `Lookup1Id`): `1` not defined, `10` in cash, `20` cheque,
  `21` banker's draft, `30` credit transfer, `49` direct debit, `54` credit card, `55` debit card,
  `68` online payment service.
- **Endpoint / TIN**: `endpoint_scheme_id` is `0235` for UAE, and `endpoint_id` is the party's
  **10-digit TIN** (starts with `1`). This is *not* the 15-digit VAT/TRN (starts with `1`, ends
  with `03`), which belongs in `party_tax_scheme.company_id` — a field v1 does not send. See the
  open question below.

## Open question: which number is in `Agents.TaxIdentificationNumber`?

Marmin distinguishes two identifiers that Tellma stores in one column:

- the **10-character TIN**, which is what `endpoint_id` and `tin` must carry, and
- the **15-digit VAT/TRN**, which belongs in `party_tax_scheme.company_id`.

v1 maps `Agents.TaxIdentificationNumber` to `endpoint_id` and `tin`, which is correct only if the
tenants store the 10-digit TIN there. Confirm with both customers before go-live. If they hold the
TRN instead, either the customer records need a second field or the mapping needs to derive one
from the other, and `party_tax_scheme` should be sent as well.

## Out of scope in v1

Document-level and line-level allowances and charges, prepayments, purchase-side documents,
proforma invoices, and downloading or storing the vendor's XML/PDF renderings (they are available
in the vendor portal, and skipping them removes an entire blob-storage code path).

---
---

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

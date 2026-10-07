CREATE PROCEDURE [dal].[MarminAe__GetInvoices]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * Given a list of document Ids, maps each one to the information needed to build the
	 * Marmin UAE e-invoice payload (MarminAeSalesInvoiceRequest / MarminAeSalesCreditNoteRequest).
	 *
	 * Modelled on [dal].[Zatca__GetInvoices], but much smaller, because the vendor derives the
	 * supplier party, the document identifiers and EVERY total server-side. The request models
	 * physically cannot express them, so nothing here computes an amount that the vendor will
	 * also compute -- that is exactly the class of mismatch this integration must avoid.
	 *
	 * Unlike ZATCA this is NOT called from [dal].[Documents__Close]. It is a standalone call made
	 * from DocumentsService right after the close, so that a feature used by two tenants adds no
	 * result sets to the close path every tenant runs.
	 *
	 * Constants. These used to be tenant settings and are now fixed, because both tenants use the
	 * same values and a blank setting could only ever produce a vendor rejection:
	 *   endpoint_scheme_id     0235      the UAE Peppol participant scheme
	 *   profile_execution_id   00000000  a plain domestic supply, used when Documents.Lookup1 is unset
	 *   payment_means          30        credit transfer, used when the sales-invoice agent's Lookup1 is unset
	 *   due date               +30 days  after the issue date, used when Documents.NotedDate is unset
	 * bll.Documents_Validate__Close applies the same defaults, so it checks what is actually sent.
	 *
	 * NOTE: the column ordering is important, don't change it. LoadMarminAeInvoices in
	 * SqlDataReaderApplicationExtensions reads these positionally.
	 */

	--=-=-= 0 - Refuse to touch Production from a non-production tenant =-=-=--
	-- Same guard as dal.Zatca__GetInvoices: tenant ids >= 1000 are test/demo databases, and a
	-- restored copy of a production database pointed at the live vendor would transmit real
	-- invoices to real counterparties over Peppol.
	DECLARE @DbName NVARCHAR(50) = DB_NAME();
	DECLARE @DbNameLength INT = LEN(@DbName);
	DECLARE @DotPos INT = CHARINDEX('.', @DbName);
	DECLARE @TenantId INT = CAST(SUBSTRING(@DbName, @DotPos + 1, @DbNameLength - @DotPos) AS INT);
	IF (@TenantId >= 1000) AND (SELECT TOP 1 [MarminAeEnvironment] FROM dbo.Settings) = N'Production'
		THROW 50000, N'Marmin environment cannot be Production in a test tenant. Change it to Sandbox.', 1;

	--=-=-= 1 - Invoice headers =-=-=--
	SELECT
		I.[Index]									AS [Index],
		D.[Id]										AS [Id],

		-- Which vendor endpoint to submit to, and the literal type code the authority expects.
		DD.[MarminAeDocumentType]					AS [DocumentType],
		DD.[MarminAeTypeCode]						AS [TypeCode],

		-- Tellma's own document number becomes the vendor's document_number. Auto-numbering must
		-- be OFF in the Marmin organisation. It is also the key the resubmit path queries the
		-- vendor by, to find out whether a submission that appeared to fail actually landed.
		D.[Code]									AS [DocumentNumber],

		-- The accounting date, NOT StateAt. ZATCA stamps the clearance moment; Peppol wants the
		-- issue date, and it must stay the same if the document is ever resubmitted.
		ISNULL(D.[PostingDate], CAST(D.[StateAt] AS DATE)) AS [IssueDate],

		-- Due date: NotedDate is a free, per-document date with a definition-configurable label,
		-- so tenants relabel it "Due Date". Otherwise 30 days after issue.
		ISNULL(D.[NotedDate], DATEADD(DAY, 30,
			ISNULL(D.[PostingDate], CAST(D.[StateAt] AS DATE)))) AS [DueDate],

		-- The eight supply-scenario flags. Same slot ZATCA uses for InvoiceTypeTransactions, but a
		-- different code vocabulary, so the tenant's Lookup Definition must hold the UAE codes.
		-- With none chosen: a plain domestic supply, or an export (position 8) for a customer
		-- outside the UAE. Peppol accepts a buyer outside the UAE only on an export (ibr-135-ae:
		-- otherwise it needs a tin or TRN, which the vendor forbids on a foreign party), and the
		-- mapper sends the delivery block an export requires.
		ISNULL(dal.fn_Lookup__Code(D.[Lookup1Id]),
			IIF(ISNULL(CUST.[CountryCode], N'AE') = N'AE', N'00000000', N'00000001')) AS [ProfileExecutionId],

		ISNULL(SI.[CurrencyId], D.[CurrencyId])		AS [DocumentCurrencyCode],
		D.[Memo]									AS [Note],
		D.[ExternalReference]						AS [BuyerReference],

		-- Credit notes only: why the note was issued. Required by the vendor on every credit note.
		IIF(DD.[MarminAeDocumentType] = N'SalesCreditNote',
			dal.fn_Lookup__Code(D.[Lookup2Id]), NULL) AS [DiscrepancyResponse],
		IIF(DD.[MarminAeDocumentType] = N'SalesCreditNote',
			dal.fn_Lookup__Name(D.[Lookup2Id]), NULL) AS [Reason],

		-- Customer. Latin Name, not Name2: ZATCA uses Name2 because KSA mandates Arabic, UAE
		-- Peppol does not. Falls back from the customer group to the customer account.
		ISNULL(CG.[Name], CA.[Name])				AS [CustomerName],
		ISNULL(CA.[ContactEmail], CG.[ContactEmail]) AS [CustomerEmail],

		-- The Peppol routing address. Whether the customer is reachable on Peppol at all is
		-- recorded on the customer agent's Lookup4, which points at the YesNo lookup definition
		-- (code Y = registered), the same convention bll.ft_Employees__Deductions_SD uses:
		--   registered           -> their 10-digit TIN
		--   not registered, UAE  -> 9900000098, the FTA's placeholder for a UAE buyer not on Peppol
		--   not registered, else -> 9900000099, the FTA's placeholder for an export to a buyer
		--                           outside the UAE
		-- Since 2026-07-07 the vendor no longer defaults to 9900000098 when the endpoint is
		-- omitted, so one of these must always be sent.
		CASE
			WHEN CUST.[IsPeppolRegistered] = 1 THEN TAXID.[Tin]
			WHEN CUST.[CountryCode] = N'AE' THEN N'9900000098'
			ELSE N'9900000099'
		END											AS [CustomerEndpointId],
		N'0235'										AS [CustomerEndpointSchemeId],

		-- tin is the 10-digit TIN and the vendor rejects it on a foreign party. The 15-digit
		-- TRN is a different identifier and travels separately, in party_tax_scheme, which the
		-- vendor forbids for a non-UAE buyer too.
		IIF(CUST.[CountryCode] = N'AE', TAXID.[Tin], NULL) AS [CustomerTin],
		IIF(CUST.[CountryCode] = N'AE', TAXID.[Trn], NULL) AS [CustomerTrn],

		CA.[AddressStreet]							AS [CustomerStreetName],
		CA.[AddressAdditionalStreet]				AS [CustomerAdditionalStreetName],
		CA.[AddressCity]							AS [CustomerCityName],
		CA.[AddressPostalCode]						AS [CustomerPostalZone],
		-- Must be an emirate CODE for a UAE address. bll.Documents_Validate__Close checks it.
		CA.[AddressProvince]						AS [CustomerCountrySubentity],
		dal.fn_Lookup__Name(CA.[AddressCountryId])	AS [CustomerCountry],
		CUST.[CountryCode]							AS [CustomerCountryCode],

		-- Payment. Same slot ZATCA reads, defaulting to 30 (credit transfer), which requires the
		-- payee account (IBR-192-AE); bll.Documents_Validate__Close asserts it is there.
		ISNULL(dal.fn_Lookup__Code(SI.[Lookup1Id]), N'30') AS [PaymentMeansCode],
		SI.[BankAccountNumber]						AS [PayeeFinancialAccountId],

		-- Rounding is modelled as a zero-VAT resource called Rounding; reused from ZATCA as-is.
		dal.fn_Document__RoundingAmount(D.[Id])		AS [PayableRoundingAmount],

		-- Credit notes only: the original invoice this note adjusts. Found by the same NotedAgentId
		-- heuristic ZATCA uses, but written inline: dal.fn_Document__BillingReferenceId declares
		-- RETURNS NVARCHAR with no length, i.e. NVARCHAR(1), so it truncates every code to one
		-- character. bll.Documents_Validate__Close asserts exactly one match before we get here.
		IIF(DD.[MarminAeDocumentType] = N'SalesCreditNote', OI.[Code], NULL)			AS [BillingReferenceId],
		IIF(DD.[MarminAeDocumentType] = N'SalesCreditNote', OI.[PostingDate], NULL)	AS [BillingReferenceIssueDate],

		-- The vendor's id when it already holds this document. Set only once the vendor has
		-- accepted a submission, so its presence is what decides between a PUT (resubmit the
		-- document it holds, which the vendor allows only while it is VALIDATION_FAILED) and a
		-- POST (create a new one).
		D.[MarminAeDocumentId]						AS [MarminAeDocumentId]
	FROM [map].[Documents]() D
	INNER JOIN @Ids I ON I.[Id] = D.[Id]
	INNER JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	INNER JOIN dbo.Agents SI ON SI.[Id] = D.[NotedAgentId]	-- Sales Invoice
	INNER JOIN dbo.Agents CA ON CA.[Id] = SI.[Agent1Id]		-- Customer Account
	LEFT JOIN dbo.Agents CG ON CG.[Id] = CA.[Agent1Id]		-- Customer
	CROSS APPLY (
		SELECT
			dal.fn_Lookup__Code(CA.[AddressCountryId]) AS [CountryCode],
			LTRIM(RTRIM(ISNULL(CA.[TaxIdentificationNumber], CG.[TaxIdentificationNumber]))) AS [TaxId],
			IIF(dal.fn_Lookup__Code(ISNULL(CA.[Lookup4Id], CG.[Lookup4Id])) = N'Y', 1, 0) AS [IsPeppolRegistered]
	) CUST
	CROSS APPLY (
		-- A UAE tax number is held either as the 10-digit TIN (starts with 1) or as the 15-digit
		-- TRN (starts with 1, ends with 03), whose first ten digits are the TIN. Anything else
		-- yields NULL here, and bll.Documents_Validate__Close refuses the close for it.
		SELECT
			CASE
				WHEN LEN(CUST.[TaxId]) = 10 AND CUST.[TaxId] LIKE N'1%' AND CUST.[TaxId] NOT LIKE N'%[^0-9]%'
					THEN CUST.[TaxId]
				WHEN LEN(CUST.[TaxId]) = 15 AND CUST.[TaxId] LIKE N'1%03' AND CUST.[TaxId] NOT LIKE N'%[^0-9]%'
					THEN LEFT(CUST.[TaxId], 10)
			END AS [Tin],
			CASE
				WHEN LEN(CUST.[TaxId]) = 15 AND CUST.[TaxId] LIKE N'1%03' AND CUST.[TaxId] NOT LIKE N'%[^0-9]%'
					THEN CUST.[TaxId]
			END AS [Trn]
	) TAXID
	OUTER APPLY (
		-- An original closed before the definition became Marmin-typed has a NULL MarminAeState
		-- forever, and must still be referencable, since the UAE does not require the original
		-- to have been an e-invoice. NULL is unambiguous: a Marmin-typed close always stamps a
		-- state, so NULL can only mean "closed before go-live" or a clone reset. A rejected
		-- invoice (-30) is included because the vendor's remedy for one is a credit note.
		SELECT TOP 1 O.[Code], O.[PostingDate]
		FROM [map].[Documents]() O
		JOIN dbo.DocumentDefinitions ODD ON ODD.[Id] = O.[DefinitionId]
		WHERE ODD.[MarminAeDocumentType] = N'SalesInvoice'
		AND O.[State] = 1							-- closed
		AND (O.[MarminAeState] IS NULL OR O.[MarminAeState] >= 1 OR O.[MarminAeState] = -30)
		AND O.[NotedAgentId] = D.[NotedAgentId]
		ORDER BY O.[Id] DESC
	) OI
	WHERE DD.[MarminAeDocumentType] IS NOT NULL
	-- Only documents that may be (re)submitted: never claimed (NULL), claimed but never sent (0),
	-- refused by the vendor (-10), or failing Peppol validation (-20, resubmitted with a PUT).
	-- Everything else is on the network (1, 2, 10) or needs a credit note (-30), and the reopen
	-- guard keeps those closed, so a re-close never reaches them.
	AND (D.[MarminAeState] IS NULL OR D.[MarminAeState] IN (0, -10, -20));

	--=-=-= 2 - Invoice lines =-=-=--
	SELECT
		I.[Index]									AS [InvoiceIndex],
		L.[Index] + 1								AS [LineNumber],

		NR.[Name]									AS [Name],
		-- description is required by the vendor but nullable here, so fall back to the name.
		ISNULL(NR.[Description], NR.[Name])			AS [Description],

		-- Sign convention copied from ZATCA's 388 / 381 split.
		IIF(DD.[MarminAeDocumentType] = N'SalesInvoice', -1, +1) * E.[Direction] * E.[Quantity] AS [Quantity],

		-- Must be a UN/ECE Rec 20 code. Free text in Tellma, so validated at close.
		U.[Code]									AS [UnitCode],

		L.[Decimal1]								AS [PriceBaseAmount],
		1.00										AS [PriceBaseQuantity],

		-- UNCL5305 letter, the same vocabulary ZATCA uses.
		ISNULL(LK3.[Code], N'S')					AS [TaxCategoryId],

		-- Marmin wants a PERCENTAGE where ZATCA wants a 0..1 fraction, and the UAE standard rate
		-- is 5%, not KSA's 15%. Resources.VatRate is constrained to 0..1, hence the * 100.
		ISNULL(NR.[VatRate], 0.05) * 100			AS [TaxPercent],

		-- Required by the vendor whenever the category is exempt. ZATCA left these commented out.
		LK4.[Code]									AS [TaxExemptionReasonCode],
		LK4.[Name]									AS [TaxExemptionReason],

		-- standard_item_identification is deliberately NOT sent. Resources.Identifier is free
		-- text (a barcode, a serial, whatever the definition labels it), and PINT-AE IBR-064
		-- rejects an identifier without an ISO 6523 scheme. The vendor's API does not check it,
		-- so the rejection would only surface at Peppol, after the close.
		NR.[Code]									AS [SellerItemIdentification]
	FROM [map].[Lines]() L
	INNER JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	INNER JOIN dbo.Resources NR ON NR.[Id] = E.[NotedResourceId]
	INNER JOIN dbo.ResourceDefinitions NRD ON NRD.[Id] = NR.[DefinitionId]
	LEFT JOIN dbo.Lookups LK3 ON LK3.[Id] = NR.[Lookup3Id]
	LEFT JOIN dbo.Lookups LK4 ON LK4.[Id] = NR.[Lookup4Id]
	INNER JOIN dbo.Units U ON U.[Id] = E.[UnitId]
	INNER JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	INNER JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	INNER JOIN [map].[Documents]() D ON D.[Id] = L.[DocumentId]
	INNER JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	INNER JOIN @Ids AS I ON I.[Id] = D.[Id]
	WHERE AC.[Concept] = N'CurrentValueAddedTaxPayables'
	AND DD.[MarminAeDocumentType] IS NOT NULL
	AND (D.[MarminAeState] IS NULL OR D.[MarminAeState] IN (0, -10, -20))
	-- A workflow line that was rejected or voided (negative state) is not part of the ledger, so
	-- it must not be part of the invoice either. bll.Documents_Validate__Close lets a document
	-- close with such lines present.
	AND L.[State] >= 0
	-- Discounts, retentions and prepayments are document-level allowances/charges, which are out
	-- of scope for v1.
	AND NOT (NRD.[Code] = N'Discounts' OR NR.[Code] = N'RetentionByCustomer'
		OR NRD.[Code] LIKE N'Prepayments%' AND E.[Direction] = 1)
	ORDER BY I.[Index], L.[Index];
END;
GO

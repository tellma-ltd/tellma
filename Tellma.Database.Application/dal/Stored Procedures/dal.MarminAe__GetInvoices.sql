CREATE PROCEDURE [dal].[MarminAe__GetInvoices]
	@Ids [dbo].[IndexedIdList] READONLY,
	@DefaultProfileExecutionId NVARCHAR (50) = NULL,
	@EndpointSchemeId NVARCHAR (50) = NULL,
	@DefaultPaymentMeansCode NVARCHAR (10) = NULL,
	@DefaultPaymentTermDays INT = 0
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
	 * from DocumentsService right after the close commits, so that a feature used by two tenants
	 * adds no result sets to the close path every tenant runs.
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
		-- so tenants relabel it "Due Date". Falling back keeps a close from failing over it.
		ISNULL(D.[NotedDate], DATEADD(DAY, ISNULL(@DefaultPaymentTermDays, 0),
			ISNULL(D.[PostingDate], CAST(D.[StateAt] AS DATE)))) AS [DueDate],

		-- The eight supply-scenario flags. Same slot ZATCA uses for InvoiceTypeTransactions, but a
		-- different code vocabulary, so the tenant's Lookup Definition must hold the UAE codes.
		ISNULL(dal.fn_Lookup__Code(D.[Lookup1Id]), @DefaultProfileExecutionId) AS [ProfileExecutionId],

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

		-- The Peppol routing address. The TRN is the endpoint id; the scheme is tenant-wide.
		ISNULL(CA.[TaxIdentificationNumber], CG.[TaxIdentificationNumber]) AS [CustomerEndpointId],
		@EndpointSchemeId							AS [CustomerEndpointSchemeId],

		-- tin is rejected by the vendor on a foreign party, so only send it for a UAE customer.
		IIF(dal.fn_Lookup__Code(CA.[AddressCountryId]) = N'AE',
			ISNULL(CA.[TaxIdentificationNumber], CG.[TaxIdentificationNumber]), NULL) AS [CustomerTin],

		CA.[AddressStreet]							AS [CustomerStreetName],
		CA.[AddressAdditionalStreet]				AS [CustomerAdditionalStreetName],
		CA.[AddressCity]							AS [CustomerCityName],
		CA.[AddressPostalCode]						AS [CustomerPostalZone],
		-- Must be an emirate CODE, not a name. bll.Documents_Validate__Close checks it.
		CA.[AddressProvince]						AS [CustomerCountrySubentity],
		dal.fn_Lookup__Name(CA.[AddressCountryId])	AS [CustomerCountry],
		dal.fn_Lookup__Code(CA.[AddressCountryId])	AS [CustomerCountryCode],

		-- Payment. Same slots ZATCA reads.
		ISNULL(dal.fn_Lookup__Code(SI.[Lookup1Id]), @DefaultPaymentMeansCode) AS [PaymentMeansCode],
		SI.[BankAccountNumber]						AS [PayeeFinancialAccountId],

		-- Rounding is modelled as a zero-VAT resource called Rounding; reused from ZATCA as-is.
		dal.fn_Document__RoundingAmount(D.[Id])		AS [PayableRoundingAmount],

		-- Credit notes only: the original invoice this note adjusts. Found by the same NotedAgentId
		-- heuristic ZATCA uses, but written inline: dal.fn_Document__BillingReferenceId declares
		-- RETURNS NVARCHAR with no length, i.e. NVARCHAR(1), so it truncates every code to one
		-- character. bll.Documents_Validate__Close asserts exactly one match before we get here.
		IIF(DD.[MarminAeDocumentType] = N'SalesCreditNote', OI.[Code], NULL)			AS [BillingReferenceId],
		IIF(DD.[MarminAeDocumentType] = N'SalesCreditNote', OI.[PostingDate], NULL)	AS [BillingReferenceIssueDate]
	FROM [map].[Documents]() D
	INNER JOIN @Ids I ON I.[Id] = D.[Id]
	INNER JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	INNER JOIN dbo.Agents SI ON SI.[Id] = D.[NotedAgentId]	-- Sales Invoice
	INNER JOIN dbo.Agents CA ON CA.[Id] = SI.[Agent1Id]		-- Customer Account
	LEFT JOIN dbo.Agents CG ON CG.[Id] = CA.[Agent1Id]		-- Customer
	OUTER APPLY (
		SELECT TOP 1 O.[Code], O.[PostingDate]
		FROM [map].[Documents]() O
		JOIN dbo.DocumentDefinitions ODD ON ODD.[Id] = O.[DefinitionId]
		WHERE ODD.[MarminAeDocumentType] = N'SalesInvoice'
		AND O.[State] = 1							-- closed
		AND O.[MarminAeState] >= 1					-- and actually submitted to the vendor
		AND O.[NotedAgentId] = D.[NotedAgentId]
		ORDER BY O.[Id] DESC
	) OI
	WHERE DD.[MarminAeDocumentType] IS NOT NULL
	-- Never submit the same document twice. DocumentsService stamps MarminAeState = 0 the moment
	-- it decides to submit, so a second close cannot pick the document up again.
	AND D.[MarminAeState] IS NULL;

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

		NR.[Code]									AS [SellerItemIdentification],
		NR.[Identifier]								AS [StandardItemIdentification]
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
	AND D.[MarminAeState] IS NULL
	-- Discounts, retentions and prepayments are document-level allowances/charges, which are out
	-- of scope for v1. bll.Documents_Validate__Close blocks a close whose totals would not
	-- reconcile because of them, so they cannot silently skew an invoice.
	AND NOT (NRD.[Code] = N'Discounts' OR NR.[Code] = N'RetentionByCustomer'
		OR NRD.[Code] LIKE N'Prepayments%' AND E.[Direction] = 1)
	ORDER BY I.[Index], L.[Index];
END;
GO

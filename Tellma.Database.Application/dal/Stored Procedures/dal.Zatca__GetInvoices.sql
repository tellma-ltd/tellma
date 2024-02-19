CREATE PROCEDURE [dal].[Zatca__GetInvoices]
	@Ids [dbo].[IndexedIdList] READONLY,
    @PreviousInvoiceSerialNumber INT OUTPUT,
    @PreviousInvoiceHash NVARCHAR(MAX) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

    /* 
     * Given a list of document Ids, this SP maps each document to the information
     * needed to construct the e-Invoice for clearing or reporting with ZATCA.
     * 
     * Spec docs: https://zatca.gov.sa/en/E-Invoicing/SystemsDevelopers/Pages/E-Invoice-specifications.aspx
     * 
     * NOTE: the column ordering is important, don't change it.
     */
	 
    --=-=-= 0 - Serial and Hash =-=-=--
	SET @PreviousInvoiceSerialNumber = 0;
    SET @PreviousInvoiceHash = N'NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==';
    
    --=-=-= 1 - Invoices =-=-=--
	SELECT
		I.[Index] AS [Index],
		D.[Id] AS [Id],
		CAST(D.[Id] AS NVARCHAR) AS [InvoiceNumber], -- BT-1, Max 127 chars
		NEWID() AS [UniqueInvoiceIdentifier], -- KSA-1
        D.[StateAt] AS [InvoiceIssueDateTime], -- BT-2 and KSA-25
		-- ZatcaDocumentType NVARCHAR (3) in DD but INT in C#
        CAST(DD.ZatcaDocumentType AS INT) AS [InvoiceType], -- BT-3: [381, 383, 386, 388, 389] subset of https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred1001.htm
		-- In Documents Lookup1: InvoiceTypeTransactions ITT000000, ... Read from left to right.
        CAST(SUBSTRING(LK1.[Code], 4, 1) AS BIT) AS [IsSimplified], -- KSA-2: 0 for Standard, 1 for Simplified
        CAST(SUBSTRING(LK1.[Code], 5, 1) AS BIT) AS [IsThirdParty], -- KSA-2: Not used. Always 0.
        CAST(SUBSTRING(LK1.[Code], 6, 1) AS BIT) AS [IsNominal], -- KSA-2: Not used. Always 0.
        CAST(SUBSTRING(LK1.[Code], 7, 1) AS BIT) AS [IsExports], -- KSA-2: Export invoices cannot be simplified
        CAST(SUBSTRING(LK1.[Code], 8, 1) AS BIT) AS [IsSummary], -- KSA-2:Not used. Always 0.
        CAST(SUBSTRING(LK1.[Code], 9, 1) AS BIT) AS [IsSelfBilled], -- KSA-2: only with invoice type 389
        D.[Memo] AS [InvoiceNote], -- BT-22: max 1000 chars
        NAG.[CurrencyId] AS [InvoiceCurrency], -- BT-5
        D.[ExternalReference] AS [PurchaseOrderId], -- BT-13, max 127 chars
        [dal].[fn_Document__BillingReferenceId](D.[Id]) AS [BillingReferenceId], -- BT-25, max 5000 chars, required for Debit/Credit Notes, NULL otherwise
        AG1.[Code] AS [ContractId], -- BT-12, max 127 chars
        AG1.TaxIdentificationNumber AS [BuyerId], -- BT-29 (or BT-48 if VAT number)
		-- Customer Accounts Lookup is Buyer Scheme
        ISNULL(dal.fn_Lookup__Code(AG1.Lookup5Id), N'VAT') AS [BuyerIdScheme], -- AG1.Lookup5,  Bt-29-1: [VAT, TIN, CRN, MOM, MLS, 700, SAG, NAT, GCC, IQA, PAS, OTH]
		-- Agents: AddressStreet, ..., AddressCountryCode
        AG1.[AddressStreet] AS [BuyerAddressStreet], -- BT-50, max 1000 chars
        AG1.[AddressAdditionalStreet] AS [BuyerAddressAdditionalStreet], -- BT-51, max 127 chars
        AG1.[AddressBuildingNumber] AS [BuyerAddressBuildingNumber], -- KSA-18
        AG1.[AddressAdditionalNumber] AS [BuyerAddressAdditionalNumber], -- KSA-19 
        AG1.[AddressCity] AS [BuyerAddressCity], -- BT-52
        AG1.[AddressPostalCode] AS [BuyerAddressPostalCode], -- BT-53
        AG1.[AddressProvince] AS [BuyerAddressProvince], -- BT-54, max 127 chars
        AG1.[AddressDistrict] AS [BuyerAddressDistrict], -- KSA-4, max 127 chars
        dal.fn_Lookup__Code(AG1.[AddressCountryId]) AS [BuyerAddressCountryCode], -- BT-55
        AG1.[Name2] AS [BuyerName], -- BT-44, max 1000 chars
        [dal].[fn_Document__SupplyDate](D.[Id], NAG.[Id]) AS [SupplyDate], -- KSA-5
        [dal].[fn_Document__SupplyEndDate](D.[Id], NAG.[Id]) AS [SupplyEndDate], -- KSA-24
		-- Sales invoice Lookup 5, payment means
        CAST(ISNULL(dal.fn_Lookup__Code(NAG.Lookup1Id), '10') AS INT) AS [PaymentMeans], -- BT-81: [1, 10, 30, 42, 48] subset of https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred4461.htm
        -- Document Lookup 2 is reason of issuance
		IIF(DD.ZatcaDocumentType IN (N'381', N'383'), dal.fn_Lookup__Name2(D.[Lookup2Id]),
			NULL) AS [ReasonForIssuanceOfCreditDebitNote], -- KSA-10, max 1000 chars, required for Debit/Credit Notes, NULL otherwise
		-- Sales invoice Lookup 6, payment terms
        IIF(DD.ZatcaDocumentType = '388', dal.fn_Lookup__Name(NAG.Lookup6Id), NULL) AS [PaymentTerms], -- KSA-22, max 1000 chars
        NAG.[BankAccountNumber] AS [PaymentAccountId], -- BT-84, max 127 chars
        dal.fn_Document__InvoiceTotalVatAmountInAccountingCurrency (D.[Id]) AS [InvoiceTotalVatAmountInAccountingCurrency], -- BT-111
        -- dal.fn_Document__PrepaidAmount(D.[Id]) AS [PrepaidAmount], -- BT-113
		-- Rounding amount can be read from a separate LD.
        dal.fn_Documeny__RoundingAmount(D.[Id]) AS [RoundingAmount] -- BT-114
		-- Following is auto computed
        --1230.00 AS [VatCategoryTaxableAmount], -- BT-116
        --N'S' AS [VatCategory], -- BT-118: [E, S, Z, O]
        --0.15 AS [VatRate], -- BT-119: between 0.00 and 1.00 (NOT 100.00)
        --N'A good reason' AS [VatExemptionReason], -- BT-120, max 1000 chars, valid values in section 11.2.4 in the specs https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_ZATCA_Electronic_Invoice_XML_Implementation_Standard_%20vF.pdf
        --N'VATEX-SA-29' AS [VatExemptionReasonCode] -- BT-121, valid values in section 11.2.4 in the specs https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_ZATCA_Electronic_Invoice_XML_Implementation_Standard_%20vF.pdf	
    FROM [map].[Documents]() D
	INNER JOIN @Ids I ON I.[Id] = D.[Id]
	INNER JOIN dbo.Lookups LK1 ON LK1.[Id] = D.[Lookup1Id]
	INNER JOIN dbo.Agents NAG ON NAG.[Id] = D.[NotedAgentId]
	INNER JOIN dbo.Agents AG1 ON AG1.[Id] = NAG.[Agent1Id]
	INNER JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	WHERE DD.[ZatcaDocumentType] IS NOT NULL

    --=-=-= 2 - Invoice Allowances/Charges =-=-=--
    SELECT
		I.[Index] AS [InvoiceIndex], -- Index of the invoice this allowance/charge belongs to. Must be one of the indices returned from the first SELECT statement
        CAST(0 AS BIT) AS [IsCharge], -- 1 for charge, 0 for allowance
        E.[Direction] * E.[NotedAmount] AS [Amount], -- BT-92 for allowances, BT-99 for charges,
        NR.[Name] AS [Reason], -- BT-97 for allowances, BT-104 for charges, max 1000 chars
        NR.[Code] AS [ReasonCode], -- BT-98 for allowances, BT-105 for charges, choices from https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred5189.htm for allowances, and from https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred7161.htm for charges
        LK3.[Code] AS [VatCategory], -- BT-95 for allowances, BT-102 for charges: [E, S, Z, O]
        NR.[VatRate] AS [VatRate] -- BT-96: between 0.00 and 1.00 (NOT 100.00)
   FROM [map].[Lines]() L
	INNER JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	INNER JOIN dbo.Resources NR ON NR.[Id] = E.[NotedResourceId]
	INNER JOIN dbo.ResourceDefinitions NRD ON NRD.[Id] = NR.[DefinitionId]
	LEFT JOIN dbo.Lookups LK3 ON LK3.[Id] = NR.[Lookup3Id]
	INNER JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	INNER JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
    INNER JOIN [map].[Documents]() D ON D.[Id] = L.[DocumentId]
	INNER JOIN @Ids AS I ON I.[Id] = D.[Id]
	WHERE AC.[Concept] = N'CurrentValueAddedTaxPayables'
	AND NRD.[Code] = N'Discounts'

    --=-=-= 3 -Regular Invoice Lines =-=-=--
    SELECT TOP 1
		I.[Index] AS [InvoiceIndex], -- Index of the invoice this allowance/charge belongs to. Must be one of the indices returned from the first SELECT statement
		L.[Index] + 1 AS [Id], -- BT-126 A unique identifier for the individual line within the Invoice. This value should be only numeric value between 1 and 999,999
		-- remove any field which is pure computation
        -E.[Direction] * E.[Quantity] AS [Quantity], -- BT-129. Zero for prepayment invoice lines
        U.[Code] AS [QuantityUnit], -- BT-130. PCE for prepayment invoice lines
        -E.[Direction] * E.[NotedAmount] AS [NetAmount], -- BT-131
       -- CAST(0 AS BIT) AS [AllowanceChargeIsCharge], -- 1 for charge, 0 for allowance
		NULL AS [AllowanceChargeIsCharge], -- intends to return allocances and charges all at the document level
        NULL AS [AllowanceChargeAmount], -- BT-136 for allowances, BT-141 for charges
        NULL AS [AllowanceChargeReason], -- BT-139 for allowances BT-144 for charges, max 1000 chars
        NULL AS [AllowanceChargeReasonCode], -- BT-140 for allowances, BT-145 for charges, choices from https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred5189.htm for allowances, and from https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred7161.htm for charges
        -E.[Direction] * E.[MonetaryValue] AS [VatAmount], -- KSA-11
        -E.[Direction] * E.[NotedAmount] AS [PrepaymentVatCategoryTaxableAmount], -- KSA-31
        NR.[Name2] AS [ItemName], -- BT-153, max 1000 chars
        NULL AS [ItemBuyerIdentifier], -- BT-156, max 127 chars
        NR.[Code] AS [ItemSellerIdentifier], -- BT-155, max 127 chars
        NR.[Identifier] AS [ItemStandardIdentifier], -- BT-157, max 127 chars
        -E.[Direction] * (E.[NotedAmount] + E.[MonetaryValue]) AS [ItemNetPrice], -- BT-146
		-- Resource Lookup 3: VAT Category
        ISNULL(LK3.[Code], 'S') AS [ItemVatCategory], -- BT-151: [E, S, Z, O]
        ISNULL(NR.[VatRate], 0.15) AS [ItemVatRate], -- BT-152: between 0.00 and 1.00 (NOT 100.00)
		-- Resource Lookup 4: VAT Exemption Reason Code
		LK4.[Code] AS [ItemVatExemptionReasonCode], -- N'VATEX-SA-EDU'
		LK4.[Name] AS [ItemVatExemptionReasonText], -- N'Private Education to citizen'
        1.00 AS [ItemPriceBaseQuantity], -- BT-149
        N'PCE' AS [ItemPriceBaseQuantityUnit], -- Bt-150, max 127 chars
        10.00 AS [ItemPriceDiscount], -- BT-147
        750.00 AS [ItemGrossPrice] -- BT-148
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
	INNER JOIN @Ids AS I ON I.[Id] = D.[Id]
	WHERE AC.[Concept] = N'CurrentValueAddedTaxPayables'
	AND NRD.[Code] <> N'Discounts'

   --=-=-= 4 - Prepayment Invoice Lines =-=-=--
   /*
	Upon issuing prepayment invoice
	Dr. Cash
	  Cr. VAT Payable: Agent: VAT, Noted Resource: Prepayment.S.15
	  Cr. Deferred Income: Agent: PPSI, Resource: Prepayment.S.15

	Upon applying the prepayment
	Dr. Deferred Income: Agent: PPSI, Resource: Prepayment.S.15
	  Cr. Account Receivable: SI
   */
    SELECT TOP 1
		I.[Index] AS [InvoiceIndex], -- Index of the invoice this allowance/charge belongs to. Must be one of the indices returned from the first SELECT statement
		L.[Index] + 1 AS [Id], -- BT-126 A unique identifier for the individual line within the Invoice. This value should be only numeric value between 1 and 999,999
		-- remove any field which is pure computation
        E.[AgentId] AS [PrepaymentId], -- KSA-26
        NEWID() AS [PrepaymentUuid], -- KSA-27
        [dal].[fn_Invoice__IssueDateTime] (D.[NotedAgentId]) AS [PrepaymentIssueDateTime], -- KSA-28 & 29
        0 AS [Quantity], -- BT-129
        N'PCE' AS [QuantityUnit], -- BT-130
        E.[Direction] * E.[NotedAmount] AS [PrepaymentVatCategoryTaxableAmount], -- KSA-31
		E.[Direction] * E.[NotedAmount] * ISNULL(R.[VatRate], 0.15) AS [PrepaymentVatCategoryTaxAmount], -- KSA-32
		E.[Direction] * E.[MonetaryValue] AS [PrepaidAmountBreakdown], -- to accumulate in BT-113
		-- Resource Lookup 3: VAT Category
		ISNULL(LK3.[Code], 'S') AS [PrepaymentVatCategory], -- KSA-33: [E, S, Z, O]
        ISNULL(R.[VatRate], 0.15) AS [PrepaymentVatRate] -- KSA-34: between 0.00 and 1.00 (NOT 100.00)
    FROM [map].[Lines]() L
	INNER JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	INNER JOIN dbo.Resources R ON R.[Id] = E.[NotedResourceId]
	LEFT JOIN dbo.Lookups LK3 ON LK3.[Id] = R.[Lookup3Id]
	INNER JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	INNER JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
    INNER JOIN [map].[Documents]() D ON D.[Id] = L.[DocumentId]
	INNER JOIN @Ids AS I ON I.[Id] = D.[Id]
	WHERE AC.[Concept] = N'DeferredIncomeClassifiedAsCurrent'
	AND R.[Code] LIKE N'Prepayment%'
END;

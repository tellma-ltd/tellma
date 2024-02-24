CREATE PROCEDURE [dal].[Zatca__GetInvoices] -- declare @Ids indexedidlist, @PIS INT, @PIH nvarchar(max);insert into @Ids values (0, 17038); exec [dal].[Zatca__GetInvoices] @Ids, @PIS, @PIH
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
    
	--=-=-= 0 - Exit clause =-=-=--
	IF NOT EXISTS (
		SELECT * FROM [map].[Lines]() L
		INNER JOIN dbo.Entries E ON E.[LineId] = L.[Id]
		INNER JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		INNER JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		INNER JOIN @Ids AS I ON I.[Id] = L.[DocumentId]
		WHERE AC.[Concept] = N'CurrentValueAddedTaxPayables'
	) RETURN

    --=-=-= 1 - Invoices =-=-=--
	SELECT
		I.[Index] AS [Index],
		D.[Id] AS [Id],
		CAST(D.[Id] AS NVARCHAR) AS [InvoiceNumber], -- BT-1, Max 127 chars
		NEWID() AS [UniqueInvoiceIdentifier], -- KSA-1
        D.[StateAt] AS [InvoiceIssueDateTime], -- BT-2 and KSA-25
		-- ZatcaDocumentType NVARCHAR (3) in DD but INT in C#
        CAST(DD.ZatcaDocumentType AS INT) AS [InvoiceType], -- BT-3: [381, 383, 386, 388] subset of https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred1001.htm
		-- In Documents Lookup1: InvoiceTypeTransactions ITT000000, ... Read from left to right.
        CAST(SUBSTRING(D_LK1.[Code], 4, 1) AS BIT) AS [IsSimplified], -- KSA-2: 0 for Standard, 1 for Simplified
        CAST(SUBSTRING(D_LK1.[Code], 5, 1) AS BIT) AS [IsThirdParty], -- KSA-2: Not used. Always 0.
        CAST(SUBSTRING(D_LK1.[Code], 6, 1) AS BIT) AS [IsNominal], -- KSA-2: Not used. Always 0.
        CAST(SUBSTRING(D_LK1.[Code], 7, 1) AS BIT) AS [IsExports], -- KSA-2: Export invoices cannot be simplified
        CAST(SUBSTRING(D_LK1.[Code], 8, 1) AS BIT) AS [IsSummary], -- KSA-2:Not used. Always 0.
        CAST(SUBSTRING(D_LK1.[Code], 9, 1) AS BIT) AS [IsSelfBilled], -- KSA-2: only with invoice type 389
        D.[Memo] AS [InvoiceNote], -- BT-22: max 1000 chars
        SI.[CurrencyId] AS [InvoiceCurrency], -- BT-5
        D.[ExternalReference] AS [PurchaseOrderId], -- BT-13, max 127 chars
		IIF(DD.ZatcaDocumentType IN (N'381', N'383'), -- Applies only to credit (381) and debot (383) notes
        [dal].[fn_Document__BillingReferenceId](D.[Id]), NULL) AS [BillingReferenceId], -- BT-25, max 5000 chars, required for Debit/Credit Notes, NULL otherwise
        CA.[Code] AS [ContractId], -- BT-12, max 127 chars
        CA.TaxIdentificationNumber AS [BuyerId], -- BT-29 (or BT-48 if VAT number)
		-- Customer Accounts Lookup 5: Buyer Scheme
        ISNULL(CA_LK5.[Code], N'VAT') AS [BuyerIdScheme], -- CA.Lookup5,  Bt-29-1: [VAT, TIN, CRN, MOM, MLS, 700, SAG, NAT, GCC, IQA, PAS, OTH]
		-- Agents: AddressStreet, ..., AddressCountryCode
        CA.[AddressStreet] AS [BuyerAddressStreet], -- BT-50, max 1000 chars
        CA.[AddressAdditionalStreet] AS [BuyerAddressAdditionalStreet], -- BT-51, max 127 chars
        CA.[AddressBuildingNumber] AS [BuyerAddressBuildingNumber], -- KSA-18
        CA.[AddressAdditionalNumber] AS [BuyerAddressAdditionalNumber], -- KSA-19 
        CA.[AddressCity] AS [BuyerAddressCity], -- BT-52
        CA.[AddressPostalCode] AS [BuyerAddressPostalCode], -- BT-53
        CA.[AddressProvince] AS [BuyerAddressProvince], -- BT-54, max 127 chars
        CA.[AddressDistrict] AS [BuyerAddressDistrict], -- KSA-4, max 127 chars
        dal.fn_Lookup__Code(CA.[AddressCountryId]) AS [BuyerAddressCountryCode], -- BT-55
        ISNULL(CG.[Name2], CA.[Name2]) AS [BuyerName], -- BT-44, max 1000 chars
        [dal].[fn_Document__SupplyDate](D.[Id], SI.[Id]) AS [SupplyDate], -- KSA-5
        [dal].[fn_Document__SupplyEndDate](D.[Id], SI.[Id]) AS [SupplyEndDate], -- KSA-24
		-- Sales invoice Lookup 5: Payment means
        CAST(ISNULL(dal.fn_Lookup__Code(SI.Lookup1Id), '10') AS INT) AS [PaymentMeans], -- BT-81: [1, 10, 30, 42, 48] subset of https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred4461.htm
        -- Document Lookup 2: Reason of issuance
		IIF(DD.ZatcaDocumentType IN (N'381', N'383'), D_LK2.[Name],
			NULL) AS [ReasonForIssuanceOfCreditDebitNote], -- KSA-10, max 1000 chars, required for Debit/Credit Notes, NULL otherwise
		-- Sales invoice Lookup 6: Payment terms
        IIF(DD.ZatcaDocumentType = '388', dal.fn_Lookup__Name(SI.Lookup6Id), NULL) AS [PaymentTerms], -- KSA-22, max 1000 chars
        SI.[BankAccountNumber] AS [PaymentAccountId], -- BT-84, max 127 chars
        dal.fn_Document__InvoiceTotalVatAmountInAccountingCurrency (D.[Id]) AS [InvoiceTotalVatAmountInAccountingCurrency], -- BT-111
        -- dal.fn_Document__PrepaidAmount(D.[Id]) AS [PrepaidAmount], -- BT-113
		-- Rounding is associated with a 0-VAT resource called rounding
        dal.fn_Document__RoundingAmount(D.[Id]) AS [RoundingAmount] -- BT-114
    FROM [map].[Documents]() D
	INNER JOIN @Ids I ON I.[Id] = D.[Id]
	INNER JOIN dbo.Lookups D_LK1 ON D_LK1.[Id] = D.[Lookup1Id]
	LEFT JOIN dbo.Lookups D_LK2 ON D_LK2.[Id] = D.[Lookup2Id]
	INNER JOIN dbo.Agents SI ON SI.[Id] = D.[NotedAgentId] -- Sales Invoice
	INNER JOIN dbo.Agents CA ON CA.[Id] = SI.[Agent1Id] -- Customer Account/Contract
	LEFT JOIN dbo.Agents CG ON CG.[Id] = CA.[Agent1Id] -- Customer
	LEFT JOIN dbo.Lookups CA_LK5 ON CA_LK5.[Id] = CA.[Lookup5Id]
	INNER JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	WHERE DD.[ZatcaDocumentType] IS NOT NULL

    --=-=-= 2 - Invoice Allowances/Charges - Document level =-=-=--
    SELECT
		I.[Index] AS [InvoiceIndex], -- Index of the invoice this allowance/charge belongs to. Must be one of the indices returned from the first SELECT statement
        CAST(0 AS BIT) AS [IsCharge], -- 1 for charge, 0 for allowance
        E.[Direction] * E.[NotedAmount] AS [Amount], -- BT-92 for allowances, BT-99 for charges,
        NR.[Name] AS [Reason], -- BT-97 for allowances, BT-104 for charges, max 1000 chars
        NR.[Code] AS [ReasonCode], -- BT-98 for allowances, BT-105 for charges, choices from https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred5189.htm for allowances, and from https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred7161.htm for charges
        ISNULL(LK3.[Code], 'S') AS [VatCategory], -- BT-95 for allowances, BT-102 for charges: [E, S, Z, O]
        ISNULL(NR.[VatRate], 0.15) AS [VatRate] -- BT-96: between 0.00 and 1.00 (NOT 100.00)
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
	AND (NRD.[Code] = N'Discounts' OR NR.[Code] = N'RetentionByCustomer')

    --=-=-= 3 -Regular Invoice Lines =-=-=--
    SELECT TOP 1
		I.[Index] AS [InvoiceIndex], -- Index of the invoice this allowance/charge belongs to. Must be one of the indices returned from the first SELECT statement
		L.[Index] + 1 AS [Id], -- BT-126 A unique identifier for the individual line within the Invoice. This value should be only numeric value between 1 and 999,999
        CASE
			WHEN DD.ZatcaDocumentType IN (N'388', N'383') THEN -E.[Direction] * E.[Quantity] 
			WHEN DD.ZatcaDocumentType = N'381' THEN +E.[Direction] * E.[Quantity] 
			WHEN DD.ZatcaDocumentType = N'386' THEN 0
		END AS [Quantity], -- BT-129. Zero for prepayment invoice lines
        U.[Code] AS [QuantityUnit], -- BT-130. PCE for prepayment invoice lines
		--Net: BT-131  = Quantity: BT-129 * Unit price: BT-146 / Base Qty: BT-149 + Charge: BT-141 - Discounts: BT-136),
        CASE -- '386' prepayment is added here even though these lines are not for prepayments
			WHEN DD.ZatcaDocumentType IN (N'388', N'383') THEN -E.[Direction] * E.[NotedAmount]
			WHEN DD.ZatcaDocumentType IN (N'381', N'386') THEN +E.[Direction] * E.[NotedAmount]
		END AS [NetAmount], -- BT-131
		CAST((
			CASE
				WHEN L.[Decimal2] > 0 THEN 0
				WHEN L.[Decimal2] < 0 THEN 1
				ELSE NULL
			END) AS BIT) AS [AllowanceChargeIsCharge], -- 1 for charge, 0 for allowance
        CAST(ABS(L.[Decimal2]) AS DECIMAL(19, 6)) AS [AllowanceChargeAmount], -- BT-136 for allowances, BT-141 for charges
		-- In case of line level allowance/charge, we need to store the Allowance/Charge Code & Reason
        NULL AS [AllowanceChargeReason], -- BT-139 for allowances BT-144 for charges, max 1000 chars
        NULL AS [AllowanceChargeReasonCode], -- BT-140 for allowances, BT-145 for charges, choices from https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred5189.htm for allowances, and from https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred7161.htm for charges
        -E.[Direction] * E.[MonetaryValue] AS [VatAmount], -- KSA-11
        NR.[Name2] AS [ItemName], -- BT-153, max 1000 chars
        NULL AS [ItemBuyerIdentifier], -- BT-156, max 127 chars
        NR.[Code] AS [ItemSellerIdentifier], -- BT-155, max 127 chars
        NR.[Identifier] AS [ItemStandardIdentifier], -- BT-157, max 127 chars
        L.[Decimal1] AS [ItemNetPrice], -- BT-146, Net Item price = Gross Unit price minus Price List Discount
		-- Resource Lookup 3: VAT Category
        ISNULL(LK3.[Code], 'S') AS [ItemVatCategory], -- BT-151: [E, S, Z, O]
        ISNULL(NR.[VatRate], 0.15) AS [ItemVatRate], -- BT-152: between 0.00 and 1.00 (NOT 100.00)
		-- Resource Lookup 4: VAT Exemption Reason Code
		LK4.[Code] AS [ItemVatExemptionReasonCode], -- N'VATEX-SA-EDU'
		LK4.[Name] AS [ItemVatExemptionReasonText], -- N'Private Education to citizen'
        1.00 AS [ItemPriceBaseQuantity], -- BT-149
        U.[Code] AS [ItemPriceBaseQuantityUnit], -- Bt-150, max 127 chars
		dal.fn_Item_Date__PriceListDiscount(NR.[Id], L.[PostingDate]) AS [ItemPriceDiscount], -- BT-147, defined at the price list level. Already in Decimal1
		  --Item net price: BT-146 = Gross price: BT-148 - Allowance: BT-147 when gross price is provided.,
		L.[Decimal1] + dal.fn_Item_Date__PriceListDiscount(NR.[Id], L.[PostingDate]) AS [ItemGrossPrice] -- BT-148
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
	AND NOT (NRD.[Code] = N'Discounts' OR NR.[Code] = N'RetentionByCustomer')

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
CREATE PROCEDURE [dal].[Zatca_GetInvoices]
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
		CAST(D.[Id] AS NVARCHAR), -- BT-1, Max 127 chars
		NEWID() AS [UniqueInvoiceIdentifier], -- KSA-1
        D.[StateAt] AS [InvoiceIssueDateTime], -- BT-2 and KSA-25
        381 AS [InvoiceType], -- BT-3: [381, 383, 388, 389] subset of https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred1001.htm
        CAST(0 AS BIT) AS [IsSimplified], -- KSA-2: 0 for Standard, 1 for Simplified
        CAST(0 AS BIT) AS [IsThirdParty], -- KSA-2
        CAST(0 AS BIT) AS [IsNominal], -- KSA-2
        CAST(0 AS BIT) AS [IsExports], -- KSA-2
        CAST(0 AS BIT) AS [IsSummary], -- KSA-2
        CAST(0 AS BIT) AS [IsSelfBilled], -- KSA-2
        N'Bla bla' AS [InvoiceNote], -- BT-22: max 1000 chars
        N'USD' AS [InvoiceCurrency], -- BT-5
        N'ABC' AS [PurchaseOrderId], -- BT-13, max 127 chars
        N'ABC' AS [BillingReferenceId], -- BT-25, max 5000 chars, required for Debit/Credit Notes, NULL otherwise
        N'ABC' AS [ContractId], -- BT-12, max 127 chars
        N'300075588800003' AS [BuyerId], -- BT-29 (or BT-48 if VAT number)
        N'VAT' AS [BuyerIdScheme], -- Bt-29-1: [VAT, TIN, CRN, MOM, MLS, 700, SAG, NAT, GCC, IQA, PAS, OTH]
        N'Main street 1' AS [BuyerAddressStreet], -- BT-50, max 1000 chars
        N'PO Box 14' AS [BuyerAddressAdditionalStreet], -- BT-51, max 127 chars
        N'123' AS [BuyerAddressBuildingNumber], -- KSA-18
        N'123' AS [BuyerAddressAdditionalNumber], -- KSA-19 
        N'Riyadh' AS [BuyerAddressCity], -- BT-52
        N'12345' AS [BuyerAddressPostalCode], -- BT-53
        N'Riyadh Region' AS [BuyerAddressProvince], -- BT-54, max 127 chars
        N'123' AS [BuyerAddressDistrict], -- KSA-4, max 127 chars
        N'SA' AS [BuyerAddressCountryCode], -- BT-55
        N'Abbas' AS [BuyerName], -- BT-44, max 1000 chars
        DATEFROMPARTS(2024, 1, 31) AS [SupplyDate], -- KSA-5
        DATEFROMPARTS(2024, 1, 31) AS [SupplyEndDate], -- KSA-24
        10 AS [PaymentMeans], -- BT-81: [1, 10, 30, 42, 48] subset of https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred4461.htm
        N'A good reason' AS [ReasonForIssuanceOfCreditDebitNote], -- KSA-10, max 1000 chars, required for Debit/Credit Notes, NULL otherwise
        N'Foo' AS [PaymentTerms], -- KSA-22, max 1000 chars
        N'Bar' AS [PaymentAccountId], -- BT-84, max 127 chars
        150.00 AS [InvoiceTotalVatAmountInAccountingCurrency], -- BT-111
        0.00 AS [PrepaidAmount], -- BT-113
        0.00 AS [RoundingAmount], -- BT-114
        1230.00 AS [VatCategoryTaxableAmount], -- BT-116
        N'E' AS [VatCategory], -- BT-118: [E, S, Z, O]
        0.0 AS [VatRate], -- BT-119: between 0.00 and 1.00 (NOT 100.00)
        N'A good reason' AS [VatExemptionReason], -- BT-120, max 1000 chars, valid values in section 11.2.4 in the specs https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_ZATCA_Electronic_Invoice_XML_Implementation_Standard_%20vF.pdf
        N'VATEX-SA-29' AS [VatExemptionReasonCode] -- BT-121, valid values in section 11.2.4 in the specs https://zatca.gov.sa/ar/E-Invoicing/SystemsDevelopers/Documents/20230519_ZATCA_Electronic_Invoice_XML_Implementation_Standard_%20vF.pdf
	
    FROM [map].[Documents]() AS D
	INNER JOIN @Ids AS I ON D.[Id] = I.[Id]

    --=-=-= 2 - Invoice Allowances/Charges =-=-=--
    SELECT
		I.[Index] AS [InvoiceIndex], -- Index of the invoice this allowance/charge belongs to. Must be one of the indices returned from the first SELECT statement
        CAST(0 AS BIT) AS [IsCharge], -- 1 for charge, 0 for allowance
        100.00 AS [Amount], -- BT-92 for allowances, BT-99 for charges,
        N'A good reason' AS [Reason], -- BT-97 for allowances, BT-104 for charges, max 1000 chars
        N'29' AS [ReasonCode], -- BT-98 for allowances, BT-105 for charges, choices from https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred5189.htm for allowances, and from https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred7161.htm for charges
        N'E' AS [VatCategory], -- BT-95 for allowances, BT-102 for charges: [E, S, Z, O]
        0.0 AS [VatRate] -- BT-119: between 0.00 and 1.00 (NOT 100.00)
    
    FROM [map].[Documents]() AS D
	INNER JOIN @Ids AS I ON D.[Id] = I.[Id]

    --=-=-= 3 - Invoice Lines =-=-=--
    SELECT TOP 1
		I.[Index] AS [InvoiceIndex], -- Index of the invoice this allowance/charge belongs to. Must be one of the indices returned from the first SELECT statement
		L.[Id] AS [Id],

        N'12902348' AS [PrepaymentId], -- KSA-26
        NEWID() AS [PrepaymentUuid], -- KSA-27
        DATETIMEOFFSETFROMPARTS(2024, 1, 31, 14, 23, 23, 0, 12, 0, 7) AS [PrepaymentIssueDateTime],
        2.00 AS [Quantity], -- BT-129
        N'PCE' AS [QuantityUnit], -- BT-130
        1330.00 AS [NetAmount], -- BT-131
        CAST(0 AS BIT) AS [AllowanceChargeIsCharge], -- 1 for charge, 0 for allowance
        150.00 AS [AllowanceChargeAmount], -- BT-136 for allowances, BT-141 for charges
        N'A good reason' AS [AllowanceChargeReason], -- BT-139 for allowances BT-144 for charges, max 1000 chars
        N'29' AS [AllowanceChargeReasonCode], -- BT-140 for allowances, BT-145 for charges, choices from https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred5189.htm for allowances, and from https://unece.org/fileadmin/DAM/trade/untdid/d16b/tred/tred7161.htm for charges
        20.00 AS [VatAmount], -- KSA-11
        1000.00 AS [PrepaymentVatCategoryTaxableAmount], -- KSA-31
        N'Apples' AS [ItemName], -- BT-153, max 1000 chars
        N'123' AS [ItemBuyerIdentifier], -- BT-156, max 127 chars
        N'456' AS [ItemSellerIdentifier], -- BT-155, max 127 chars
        N'789' AS [ItemStandardIdentifier], -- BT-157, max 127 chars
        740.00 AS [ItemNetPrice], -- BT-146
        N'E' AS [ItemVatCategory], -- BT-151: [E, S, Z, O]
        0.0 AS [ItemVatRate], -- BT-152: between 0.00 and 1.00 (NOT 100.00)
        N'E' AS [PrepaymentVatCategory], -- KSA-33: [E, S, Z, O]
        0.0 AS [PrepaymentVatRate], -- KSA-34: between 0.00 and 1.00 (NOT 100.00)
        1.00 AS [ItemPriceBaseQuantity], -- BT-149
        N'PCE' AS [ItemPriceBaseQuantityUnit], -- Bt-150, max 127 chars
        10.00 AS [ItemPriceDiscount], -- BT-147
        750.00 AS [ItemGrossPrice] -- BT-148
    FROM [map].[Lines]() AS L
    INNER JOIN [map].[Documents]() AS D ON L.[DocumentId] = D.[Id]
	INNER JOIN @Ids AS I ON D.[Id] = I.[Id]

END;

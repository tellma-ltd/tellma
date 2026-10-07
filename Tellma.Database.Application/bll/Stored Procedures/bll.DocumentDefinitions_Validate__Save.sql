CREATE PROCEDURE [bll].[DocumentDefinitions_Validate__Save]
	@Entities [DocumentDefinitionList] READONLY,
	@DocumentDefinitionLineDefinitions [DocumentDefinitionLineDefinitionList] READONLY,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Lookup1DefinitionId',
		N'Error_TheLookupDefinitionForInvoiceTypeTransactionsIsRequired'
	FROM @Entities
	WHERE [ZatcaDocumentType] IN (N'381', N'383', N'388', N'389')
	AND [Lookup1DefinitionId] IS NULL
	UNION
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Lookup2DefinitionId',
		N'Error_TheLookupDefinitionForReasonForIssuanceOfCreditDebitNoteIsRequired'
	FROM @Entities
	WHERE [ZatcaDocumentType] IN (N'381', N'383')
	AND [Lookup2DefinitionId] IS NULL
	UNION
	-- Marmin (UAE): Lookup1 carries profile_execution_id, the supply-scenario flags, but it is
	-- optional: an unset Lookup1 means 00000000, a plain domestic supply, which is the common case.
	--
	-- MarminAeTypeCode is passed to the vendor verbatim, and the vendor accepts exactly these
	-- (docs.ae.marmin.ai/docs/2026-05-07, "Invoice type codes" and "Create a sales credit note"):
	--   SalesInvoice     380  commercial / tax invoice     480  invoice out of scope of tax
	--   SalesCreditNote  381  or 81
	-- Anything else would only be refused by the vendor after a document had closed.
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].MarminAeTypeCode',
		N'Error_MarminAeInvalidTypeCode'
	FROM @Entities
	WHERE ([MarminAeDocumentType] = N'SalesInvoice' AND ISNULL([MarminAeTypeCode], N'') NOT IN (N'380', N'480'))
	OR ([MarminAeDocumentType] = N'SalesCreditNote' AND ISNULL([MarminAeTypeCode], N'') NOT IN (N'381', N'81'))
	UNION
	-- Marmin (UAE): Lookup2 carries discrepancy_response, which the vendor requires on every
	-- credit note.
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Lookup2DefinitionId',
		N'Error_TheLookupDefinitionForReasonForIssuanceOfCreditDebitNoteIsRequired'
	FROM @Entities
	WHERE [MarminAeDocumentType] = N'SalesCreditNote'
	AND [Lookup2DefinitionId] IS NULL
	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;
GO
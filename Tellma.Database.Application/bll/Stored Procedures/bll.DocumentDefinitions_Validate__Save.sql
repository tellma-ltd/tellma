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
	-- Marmin (UAE): Lookup1 carries profile_execution_id, the supply-scenario flags. It may be
	-- skipped only when a tenant-wide default is configured in General Settings, which is the
	-- common case for a tenant issuing a single supply scenario. That default lives in
	-- CustomFieldsJson, hence the JSON_VALUE read.
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Lookup1DefinitionId',
		N'Error_TheLookupDefinitionForInvoiceTypeTransactionsIsRequired'
	FROM @Entities
	WHERE [MarminAeDocumentType] IS NOT NULL
	AND [Lookup1DefinitionId] IS NULL
	AND ISNULL((SELECT TOP 1 JSON_VALUE([CustomFieldsJson], '$.MarminAeDefaultProfileExecutionId')
				FROM dbo.Settings), N'') = N''
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
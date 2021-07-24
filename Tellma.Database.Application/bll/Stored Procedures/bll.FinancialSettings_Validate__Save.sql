CREATE PROCEDURE [bll].[FinancialSettings_Validate__Save]
	@FunctionalCurrencyId NCHAR (3),
	@TaxIdentificationNumber NVARCHAR (50),
	@FirstDayOfPeriod TINYINT,
	@ArchiveDate DATE,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- cannot change functional currency if there are valuable documents with finalized lines
	IF [dbo].[fn_FunctionalCurrencyId]() <> @FunctionalCurrencyId
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
    SELECT DISTINCT TOP (@Top)
		'FunctionalCurrencyId',
		N'Error_Document01HasForeignCurrency',
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]),
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N]
	FROM [dbo].[DocumentDefinitions] DD
	JOIN [dbo].[Documents] D ON D.[DefinitionId] = DD.[Id]
	JOIN [dbo].[Lines] L ON L.[DocumentId] = D.[Id]
	JOIN [dbo].[Entries] E ON E.[LineId] = L.[Id]
	WHERE L.[State] = 4
	AND E.[CurrencyId] <> [dbo].[fn_FunctionalCurrencyId]()
	AND E.[MonetaryValue] <> 0

	-- Cannot change Archive date if there are uncolosed documents on or before that date
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
    SELECT DISTINCT TOP (@Top)
		'ArchiveDate',
		N'Error_Document01IsOpenWithLinesPostedOn2BeforeArchiveDate',
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]),
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		L.[PostingDate]
	FROM [dbo].[DocumentDefinitions] DD
	JOIN [dbo].[Documents] D ON D.[DefinitionId] = DD.[Id]
	JOIN [dbo].[Lines] L ON L.[DocumentId] = D.[Id]
	JOIN [dbo].[Entries] E ON E.[LineId] = L.[Id]
	WHERE D.[State] = 0 
	AND L.[State] >= 0
	AND L.[PostingDate] <= @ArchiveDate

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;
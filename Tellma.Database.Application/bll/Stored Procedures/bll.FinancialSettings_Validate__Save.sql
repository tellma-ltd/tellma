CREATE PROCEDURE [bll].[FinancialSettings_Validate__Save]
	@FunctionalCurrencyId NCHAR(3),
	@ArchiveDate DATE = '1900.01.01',
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- cannot change functional currency if there are valueable documents with finalized lines
	IF dbo.fn_FunctionalCurrencyId() <> @FunctionalCurrencyId
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
    SELECT DISTINCT TOP (@Top)
		'FunctionalCurrencyId',
		N'Error_Document01HasForeignCurrency',
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]),
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N]
	FROM dbo.DocumentDefinitions DD
	JOIN dbo.Documents D ON D.[DefinitionId] = DD.[Id]
	JOIN dbo.Lines L ON L.[DocumentId] = D.[Id]
	JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	WHERE L.[State] = 4
	AND E.[CurrencyId] <> dbo.fn_FunctionalCurrencyId()
	AND E.[MonetaryValue] <> 0
	
 	SELECT TOP (@Top) * FROM @ValidationErrors;
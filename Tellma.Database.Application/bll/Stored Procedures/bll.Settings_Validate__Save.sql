CREATE PROCEDURE [bll].[Settings_Validate__Save]
	@ShortCompanyName NVARCHAR(255),
	@ShortCompanyName2 NVARCHAR(255) = NULL,
	@ShortCompanyName3 NVARCHAR(255) = NULL,
	@FunctionalCurrencyId NCHAR(3),
	@PrimaryLanguageId NVARCHAR(255),
	@PrimaryLanguageSymbol NVARCHAR (5) = NULL,
	@SecondaryLanguageId NVARCHAR(255) = NULL,
	@SecondaryLanguageSymbol NVARCHAR (5) = NULL,
	@TernaryLanguageId NVARCHAR(255) = NULL,
	@TernaryLanguageSymbol NVARCHAR (5) = NULL,
	@BrandColor NCHAR (7) = NULL,
	@DefinitionsVersion UNIQUEIDENTIFIER,
	@SettingsVersion UNIQUEIDENTIFIER,
	@ArchiveDate DATE = '1900.01.01',
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	-- cannot change functional currency if there are valueable documented with finanlized lines
	IF dbo.fn_FunctionalCurrencyId() <> @FunctionalCurrencyId
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
    SELECT DISTINCT TOP (@TOP)
		'FunctionalCurrencyId',
		N'Error_Document0HasForeignCurrency',
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N]
	FROM dbo.DocumentDefinitions DD
	JOIN dbo.Documents D ON D.[DefinitionId] = DD.[Id]
	JOIN dbo.Lines L ON L.[DocumentId] = D.[Id]
	JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	WHERE L.[State] = 4
	AND E.[CurrencyId] <> dbo.fn_FunctionalCurrencyId()
	AND E.[Value] <> 0
	
 	SELECT TOP (@Top) * FROM @ValidationErrors;
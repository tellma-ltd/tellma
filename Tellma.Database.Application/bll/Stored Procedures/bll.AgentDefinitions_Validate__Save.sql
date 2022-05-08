CREATE PROCEDURE [bll].[AgentDefinitions_Validate__Save]
	@Entities [AgentDefinitionList] READONLY,
	@ReportDefinitions [AgentDefinitionReportDefinitionList] READONLY,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Code must be unique
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0]) 
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsUsed',
		FE.Code
	FROM @Entities FE 
	JOIN [dbo].[AgentDefinitions] BE ON FE.Code = BE.Code
	WHERE ((FE.Id IS NULL) OR (FE.Id <> BE.Id));

	-- Code must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Id',
		N'Error_TheCode0IsDuplicated',
		[Code]
	FROM @Entities
	WHERE [Code] IN (
		SELECT [Code] FROM @Entities
		GROUP BY [Code]
		HAVING COUNT(*) > 1
	);

	-- MA: Commented 2022.05.03
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	--SELECT DISTINCT TOP (@Top)
	--	'[' + CAST([Index] AS NVARCHAR (255)) + '].CurrencyVisibility',
	--	N'Error_TheCurrencyVisibility0MustBeRequired',
	--	[Code]
	--FROM @Entities
	--WHERE [Code] LIKE N'%Member'
	--AND [MainMenuSection] = N'FixedAssets'
	--AND [CurrencyVisibility] <> N'Required'

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;
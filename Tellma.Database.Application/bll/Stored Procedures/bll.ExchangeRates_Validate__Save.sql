CREATE PROCEDURE [bll].[ExchangeRates_Validate__Save]
	@Entities [dbo].[ExchangeRateList] READONLY,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

    -- Non Null Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Id',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255))
    FROM @Entities
    WHERE [Id] IS NOT NULL AND [Id] <> 0
	AND [Id] NOT IN (SELECT [Id] from [dbo].[ExchangeRates]);

	-- TODO: Check that CurrencyId is valid

	-- [CurrencyId] and [ValidAsOf] must not be available in the DB
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCurrency0Date1AreDuplicated',
		dbo.fn_Localize(C.[Name], C.[Name2], C.[Name3]) AS CurrencyName,
		FORMAT(FE.[ValidAsOf], 'yyyy-MM-dd')
	FROM @Entities FE
	JOIN [dbo].[ExchangeRates] BE ON FE.[CurrencyId] = BE.CurrencyId AND FE.[ValidAsOf] = BE.[ValidAsOf]
	JOIN [dbo].[Currencies] C ON FE.CurrencyId = C.Id
	WHERE FE.Id = 0 OR FE.Id <> BE.Id;

	SELECT TOP(@Top) * FROM @ValidationErrors;

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;
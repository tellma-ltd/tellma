CREATE PROCEDURE [bll].[ExchangeRates_Validate__Save]
	@Entities [ExchangeRateList] READONLY, -- @ValidationErrorsJson NVARCHAR(MAX) OUTPUT,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	
    -- Non Null Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Id',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255))
    FROM @Entities
    WHERE Id <> 0
	AND Id NOT IN (SELECT Id from [dbo].[ExchangeRates]);

	-- [CurrencyId] and [ValidAsOf] must not be available in the DB
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCurrency0Date1IsDuplicated',
		FE.[CurrencyId],
		FORMAT(FE.[ValidAsOf], 'd', 'de-de') -- DD.MM.YYYY
	FROM @Entities FE
	JOIN dbo.ExchangeRates ER ON FE.[CurrencyId] = ER.CurrencyId AND FE.[ValidAsOf] = ER.[ValidAsOf]
	WHERE FE.[Id] = 0;

	SELECT TOP(@Top) * FROM @ValidationErrors;
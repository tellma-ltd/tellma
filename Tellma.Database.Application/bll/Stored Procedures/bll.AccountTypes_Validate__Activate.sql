﻿CREATE PROCEDURE [bll].[AccountTypes_Validate__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@Top INT = 10,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	IF @IsActive = 0
	BEGIN
		WITH
		ActiveAccounts([Index], [AccountId], [Value], [MonetaryValue])
		AS (
			SELECT I.[Index], E.AccountId, E.[CurrencyId],
				SUM(E.[Direction] * E.[MonetaryValue])
			-- TODO: Add the remaining units
			FROM dbo.Entries E
			JOIN dbo.Lines L ON E.[LineId] = L.[Id]
			JOIN dbo.Accounts A ON E.AccountId = A.Id
			JOIN @Ids I ON I.[Id] = A.[AccountTypeId]
			WHERE L.[State] = 4
			GROUP BY I.[Index],  E.AccountId, E.[CurrencyId]
			HAVING
				SUM(E.[Direction] * E.[MonetaryValue]) <> 0
		)
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_TheAccountTypeHasAccount0WithNonZeroBalance',
			[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3])
		FROM ActiveAccounts AA
		JOIN dbo.Accounts A ON AA.AccountId = A.[Id]
	END

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP(@Top) * FROM @ValidationErrors;
END;
-- =============================================
-- Author:      System
-- Create date: 2025-05-08
-- Description: Validates account classifications before activation/deactivation
-- Business Rules:
--     - Cannot deactivate a classification if it has active accounts with non-zero balance
--     - Validation only checks posted lines (State = 4)
--     - Validates across all currencies
-- Parameters:
--     @Ids IndexedIdList - List of account classification IDs to validate
--     @IsActive BIT - Flag indicating if the classification is being activated (1) or deactivated (0)
--     @Top INT = 10 - Maximum number of validation errors to return
--     @IsError BIT OUTPUT - Returns 1 if validation errors exist, 0 otherwise
-- Returns:
--     @IsError BIT - 1 if validation errors, 0 otherwise
--     ValidationErrorList - List of validation errors including:
--         - Account classification name
--         - Account name with non-zero balance
-- Usage:
--     -- To validate before deactivation
--     DECLARE @IsError BIT;
--     EXEC [bll].[AccountClassifications_Validate__Activate] 
--         @Ids = <IndexedIdList>,
--         @IsActive = 0,
--         @Top = 10,
--         @IsError = @IsError OUTPUT;
-- =============================================

CREATE PROCEDURE [bll].[AccountClassifications_Validate__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@Top INT = 10,
	@IsError BIT OUTPUT
AS
BEGIN
	-- Set NOCOUNT to ON to prevent the message indicating the number of rows affected from being returned
	SET NOCOUNT ON;
	
	-- Declare a table variable to store validation errors
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Check if the classification is being deactivated
	IF @IsActive = 0
	BEGIN
		-- Use a Common Table Expression (CTE) to get active accounts with non-zero balance
		WITH
		ActiveAccounts([Index], [AccountId], [Value], [MonetaryValue])
		AS (
			SELECT I.[Index], E.AccountId, E.[CurrencyId],
				SUM(E.[Direction] * E.[MonetaryValue])
			FROM dbo.Entries E
			JOIN dbo.Lines L ON E.[LineId] = L.[Id]
			JOIN dbo.Accounts A ON E.AccountId = A.Id
			JOIN @Ids I ON I.[Id] = A.[ClassificationId]
			WHERE L.[State] = 4 -- N'Posted'
			GROUP BY I.[Index], E.AccountId, E.[CurrencyId]
			HAVING
				SUM(E.[Direction] * E.[MonetaryValue]) <> 0
		)
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_TheAccountClassification0HasAccount1WithNonZeroBalance',
			[dbo].[fn_Localize](AC.[Name], AC.[Name2], AC.[Name3]),
			[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3])
		FROM ActiveAccounts AA
		JOIN dbo.[Accounts] A ON AA.[AccountId] = A.[Id]
		JOIN dbo.[AccountClassifications] AC ON A.[ClassificationId] = AC.[Id]
	END

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP(@Top) * FROM @ValidationErrors;
END;
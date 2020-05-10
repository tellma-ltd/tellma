﻿CREATE PROCEDURE [bll].[AccountClassifications_Validate__Deprecate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsDeprecated BIT,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

IF @IsDeprecated = 1
BEGIN
	WITH
	ActiveAccounts([Index], [AccountId], [Value], [MonetaryValue])
	AS (
		SELECT I.[Index], E.AccountId, 
			SUM(E.[Direction] * E.[Value]) AS [Value],
			SUM(E.[Direction] * E.[MonetaryValue])
		-- TODO: Add the remaining units
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.[LineId] = L.[Id]
		JOIN dbo.Accounts A ON E.AccountId = A.Id
		JOIN @Ids I ON I.[Id] = A.[ClassificationId]
		WHERE L.[State] = 4 -- N'Posted'
		GROUP BY I.[Index], E.AccountId
		HAVING
			SUM(E.[Direction] * E.[Value]) <> 0
		OR	SUM(E.[Direction] * E.[MonetaryValue]) <> 0
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccountClassificationHasAccount0WithNonZeroBalance',
		dbo.fn_Localize(A.[Name], A.[Name2], A.[Name3])
	FROM ActiveAccounts AA
	JOIN dbo.Accounts A ON AA.AccountId = A.[Id]
END

SELECT TOP(@Top) * FROM @ValidationErrors;


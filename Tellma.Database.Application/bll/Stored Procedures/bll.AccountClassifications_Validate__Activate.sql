CREATE PROCEDURE [bll].[AccountClassifications_Validate__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@Top INT = 10,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

IF @IsActive = 0
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
	
	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

SELECT TOP(@Top) * FROM @ValidationErrors;
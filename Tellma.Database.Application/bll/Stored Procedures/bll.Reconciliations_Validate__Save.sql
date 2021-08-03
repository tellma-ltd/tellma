CREATE PROCEDURE [bll].[Reconciliations_Validate__Save]
	@AccountId INT,
	@RelationId INT,
	@Entries IndexedIdList READONLY,
	@ExternalEntries ExternalEntryList READONLY,
	@Reconciliations ReconciliationList READONLY,
	@ReconciliationEntries ReconciliationEntryList READONLY,
	@ReconciliationExternalEntries ReconciliationExternalEntryList READONLY,
	@UserId INT,

	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Make sure account and custodies in bank, with non zero values match with books
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT
		'[' + CAST(@AccountId AS NVARCHAR (255)) + ']',
		N'Error_AccountCouldNotBeFound'
	WHERE @AccountId NOT IN (SELECT [Id] FROM dbo.Accounts)
	UNION
	SELECT
		'[' + CAST(@RelationId AS NVARCHAR (255)) + ']',
		N'Error_RelationCouldNotBeFound'
	WHERE @RelationId NOT IN (SELECT [Id] FROM dbo.[Relations]);

	WITH T AS (
		SELECT E.[Index], E.[AccountId], E.[RelationId], E.NetAmount, EX.[AccountId] AS EXAccountID, EX.[RelationId] AS EXRelationId, EX.NetAmount AS EXNetAmount
		FROM
		(
			SELECT MIN(FE.[Index]) AS [Index], E.[AccountId], E.[RelationId], SUM(E.[Direction] * [MonetaryValue]) AS NetAmount
			FROM @Entries FE
			JOIN dbo.Entries E ON FE.[Id] = E.[Id]
			GROUP BY E.[AccountId], E.[RelationId]
			HAVING SUM(E.[Direction] * [MonetaryValue]) <> 0
		) E
		JOIN
		(
			SELECT MIN(FE.[Index]) AS [Index], E.[AccountId], E.[RelationId], SUM(E.[Direction] * E.[MonetaryValue]) AS NetAmount
			FROM @ExternalEntries FE
			JOIN dbo.ExternalEntries E ON FE.[Id] = E.[Id]
			GROUP BY E.[AccountId], E.[RelationId]
			HAVING SUM(E.[Direction] * E.[MonetaryValue]) <> 0
		) EX ON E.[AccountId] = EX.[AccountId] AND E.[RelationId] = EX.[RelationId]
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP(@Top)	
		'[' + CAST(T.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Amount0DoesNotMatchAmount1',
		FORMAT(T.NetAmount, 'N', 'en-us') AS NetAmount,
		FORMAT(T.EXNetAmount, 'N', 'en-us') AS ExNetAmount
	FROM T
	WHERE T.NetAmount <> T.EXNetAmount

	SELECT TOP(@Top) * FROM @ValidationErrors;
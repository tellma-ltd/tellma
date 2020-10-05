CREATE PROCEDURE [bll].[Reconciliations_Validate__Save]
	@AccountId INT,
	@CustodyId INT,
	@Entries IndexedIdList READONLY,
	@ExternalEntries ExternalEntryList READONLY,
	@Reconciliations ReconciliationList READONLY,
	@ReconciliationEntries ReconciliationEntryList READONLY,
	@ReconciliationExternalEntries ReconciliationExternalEntryList READONLY,

	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Make sure account and custodies in bank, with non zero values match with books
	WITH T AS (
		SELECT EX.[Index], E.[AccountId], E.[CustodyId], EX.[AccountId] AS EXAccountID, EX.CustodyId AS EXCustodyId
		FROM
		(
			SELECT MIN(FE.[Index]) AS [Index], E.[AccountId], E.[CustodyId]
			FROM @Entries FE
			JOIN dbo.Entries E ON FE.[Id] = E.[Id]
			GROUP BY E.[AccountId], E.[CustodyId]
			HAVING SUM(E.[Direction] * [MonetaryValue]) <> 0
		) E
		RIGHT JOIN
		(
			SELECT MIN(FE.[Index]) AS [Index], E.[AccountId], E.[CustodyId]
			FROM @ExternalEntries FE
			JOIN dbo.ExternalEntries E ON FE.[Id] = E.[Id]
			GROUP BY E.[AccountId], E.[CustodyId]
			HAVING SUM(E.[Direction] * E.[MonetaryValue]) <> 0
		) EX ON E.[AccountId] = EX.[AccountId] AND E.[CustodyId] = EX.[CustodyId]
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP(@Top)	
		'[' + CAST(T.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Account0HasNoMatchInInternalEntries',
		dbo.fn_Localize(A.[Name], A.[Name2], A.[Name3]) AS [Account], NULL
	FROM T
	JOIN dbo.Accounts A ON T.[AccountId] = A.[Id]
	WHERE AccountId IS NULL
	UNION
	SELECT TOP(@Top)	
		'[' + CAST(T.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Custody01HasNoMatchInInternalEntries',
		dbo.fn_Localize(CD.[TitleSingular], CD.[TitleSingular2], CD.[TitleSingular3]) AS [CustodyDefinition],
		dbo.fn_Localize(C.[Name], C.[Name2], C.[Name3]) AS [Custody]
	FROM T
	JOIN dbo.Custodies C ON T.[CustodyId] = C.[Id]
	JOIN dbo.CustodyDefinitions CD ON C.[DefinitionId] = CD.[Id]
	WHERE CustodyId IS NULL;

	WITH T AS (
		SELECT E.[Index], E.[AccountId], E.[CustodyId], E.NetAmount, EX.[AccountId] AS EXAccountID, EX.CustodyId AS EXCustodyId, EX.NetAmount AS EXNetAmount
		FROM
		(
			SELECT MIN(FE.[Index]) AS [Index], E.[AccountId], E.[CustodyId], SUM(E.[Direction] * [MonetaryValue]) AS NetAmount
			FROM @Entries FE
			JOIN dbo.Entries E ON FE.[Id] = E.[Id]
			GROUP BY E.[AccountId], E.[CustodyId]
			HAVING SUM(E.[Direction] * [MonetaryValue]) <> 0
		) E
		JOIN
		(
			SELECT MIN(FE.[Index]) AS [Index], E.[AccountId], E.[CustodyId], SUM(E.[Direction] * E.[MonetaryValue]) AS NetAmount
			FROM @ExternalEntries FE
			JOIN dbo.ExternalEntries E ON FE.[Id] = E.[Id]
			GROUP BY E.[AccountId], E.[CustodyId]
			HAVING SUM(E.[Direction] * E.[MonetaryValue]) <> 0
		) EX ON E.[AccountId] = EX.[AccountId] AND E.[CustodyId] = EX.[CustodyId]
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
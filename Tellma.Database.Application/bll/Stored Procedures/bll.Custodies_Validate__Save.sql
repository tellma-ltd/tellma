CREATE PROCEDURE [bll].[Custodies_Validate__Save]
	@DefinitionId INT,
	@Entities [CustodyList] READONLY,
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
	AND Id NOT IN (SELECT Id from [dbo].[Custodies]);

	-- Code must be unique
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0]) 
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsUsed',
		FE.Code
	FROM @Entities FE 
	JOIN [dbo].[Custodies] BE ON FE.Code = BE.Code
	WHERE (BE.DefinitionId = @DefinitionId) AND ((FE.Id IS NULL) OR (FE.Id <> BE.Id));

		-- Code must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsDuplicated',
		[Code]
	FROM @Entities
	WHERE [Code] IN (
		SELECT [Code]
		FROM @Entities
		WHERE [Code] IS NOT NULL
		GROUP BY [Code]
		HAVING COUNT(*) > 1
	) OPTION (HASH JOIN);

	-- Cannot change currency if Custody is already used in Entries with different currency
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP(@Top)
		'[' + CAST(C.[Index] AS NVARCHAR (255)) + '].CurrencyId',
		N'Error_TheCustodyWasUsedInDocument0WithCurrency1',
		D.[Code],
		E.[CurrencyId]
	FROM @Entities C
	JOIN dbo.Entries E ON C.[Id] = E.CustodyId
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN map.Documents() D ON D.[Id] = L.[DocumentId]
	WHERE C.[CurrencyId] IS NOT NULL AND E.[CurrencyId] <> C.[CurrencyId]

	-- Cannot change currency if Custody is already used in Account with different currency
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP(@Top)
		'[' + CAST(C.[Index] AS NVARCHAR (255)) + '].CurrencyId',
		N'Error_TheCustodyWasUsedInAccount0WithCurrency1',
		dbo.fn_Localize(A.[Name], A.[Name2], A.[Name3]),
		A.[CurrencyId]
	FROM @Entities C
	JOIN dbo.Accounts A ON C.[Id] = A.CustodyId
	WHERE C.[CurrencyId] IS NOT NULL AND A.[CurrencyId] <> C.[CurrencyId]

	-- Cannot change Center if Custody is already used in Entries with different Center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP(@Top)
		'[' + CAST(C.[Index] AS NVARCHAR (255)) + '].CenterId',
		N'Error_TheCustodyWasUsedInDocument0WithCenter1',
		D.[Code],
		E.[CenterId]
	FROM @Entities C
	JOIN dbo.Entries E ON C.[Id] = E.CustodyId
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN map.Documents() D ON D.[Id] = L.[DocumentId]
	WHERE C.[CenterId] IS NOT NULL AND E.[CenterId] <> C.[CenterId]

	-- Cannot change Center if Custody is already used in Account with different Center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP(@Top)
		'[' + CAST(C.[Index] AS NVARCHAR (255)) + '].CenterId',
		N'Error_TheCustodyWasUsedInAccount0WithCenter1',
		dbo.fn_Localize(A.[Name], A.[Name2], A.[Name3]),
		A.[CenterId]
	FROM @Entities C
	JOIN dbo.Accounts A ON C.[Id] = A.CustodyId
	WHERE C.[CenterId] IS NOT NULL AND A.[CenterId] <> C.[CenterId]

	SELECT TOP (@Top) * FROM @ValidationErrors;
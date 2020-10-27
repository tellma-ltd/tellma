CREATE PROCEDURE [bll].[Custodies_Validate__Save]
	@DefinitionId INT,
	@Entities [CustodyList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @TitleSingular NVARCHAR (50);
	SELECT @TitleSingular = dbo.fn_Localize(TitleSingular, TitleSingular2, TitleSingular3)
	FROM dbo.CustodyDefinitions
	WHERE [Id] = @DefinitionId

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
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(C.[Index] AS NVARCHAR (255)) + '].CurrencyId',
		N'Error_TheCustody0WasUsedInDocument1WithCurrency2',
		@TitleSingular,
		D.[Code],
		E.[CurrencyId]
	FROM @Entities C
	JOIN dbo.Entries E ON C.[Id] = E.CustodyId
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN map.Documents() D ON D.[Id] = L.[DocumentId]
	WHERE C.[CurrencyId] IS NOT NULL AND E.[CurrencyId] <> C.[CurrencyId]

	-- Cannot change currency if Custody is already used in Account with different currency
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(C.[Index] AS NVARCHAR (255)) + '].CurrencyId',
		N'Error_TheCustody0WasUsedInAccount1WithCurrency2',
		@TitleSingular,
		dbo.fn_Localize(A.[Name], A.[Name2], A.[Name3]),
		A.[CurrencyId]
	FROM @Entities C
	JOIN dbo.Accounts A ON C.[Id] = A.CustodyId
	WHERE C.[CurrencyId] IS NOT NULL AND A.[CurrencyId] <> C.[CurrencyId]

	-- Cannot change Center if Custody is already used in Entries with different Center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(C.[Index] AS NVARCHAR (255)) + '].CenterId',
		N'Error_TheCustody0WasUsedInDocument1WithCenter2',
		@TitleSingular,
		D.[Code],
		dbo.fn_Localize(CC.[Name], CC.[Name2], CC.[Name3])
	FROM @Entities C
	JOIN dbo.Entries E ON C.[Id] = E.CustodyId
	JOIN map.Accounts() A ON E.AccountId = A.[Id]
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN map.Documents() D ON D.[Id] = L.[DocumentId]
	JOIN dbo.Centers CC ON E.[CenterId] = CC.[Id]
	WHERE C.[CenterId] IS NOT NULL AND E.[CenterId] <> C.[CenterId]
	AND A.IsBusinessUnit = 1;

	-- Cannot change Center if Custody is already used in Account with different Center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(C.[Index] AS NVARCHAR (255)) + '].CenterId',
		N'Error_TheCustody0WasUsedInAccount1WithCenter2',
		@TitleSingular,
		dbo.fn_Localize(A.[Name], A.[Name2], A.[Name3]),
		dbo.fn_Localize(CC.[Name], CC.[Name2], CC.[Name3])
	FROM @Entities C
	JOIN dbo.Accounts A ON C.[Id] = A.CustodyId
	JOIN dbo.Centers CC ON A.[CenterId] = CC.[Id]
	WHERE C.[CenterId] IS NOT NULL AND A.[CenterId] <> C.[CenterId]

	-- Cannot assign an inactive Custodian
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].CustodianId',
		N'Error_TheCustodian01IsInactive',
		dbo.fn_Localize(RLD.[TitleSingular], RLD.[TitleSingular2], RLD.[TitleSingular3]),
		dbo.fn_Localize(RL.[Name], RL.[Name2], RL.[Name3])
	FROM @Entities FE
	JOIN dbo.Relations RL ON FE.CustodianId = RL.Id
	JOIN dbo.RelationDefinitions RLD ON RL.DefinitionId = RLD.[Id]
	WHERE RL.IsActive = 0

	-- Cannot assign an inactive center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].CenterId',
		N'Error_TheCenter0IsInactive',
		dbo.fn_Localize(C.[Name], C.[Name2], C.[Name3])
	FROM @Entities FE
	JOIN dbo.Centers C ON FE.CenterId = C.Id
	WHERE C.IsActive = 0

	-- Cannot assign an inactive currency
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].CurrencyId',
		N'Error_TheCurrency0IsInactive',
		dbo.fn_Localize(C.[Name], C.[Name2], C.[Name3])
	FROM @Entities FE
	JOIN dbo.Currencies C ON FE.CurrencyId = C.Id
	WHERE C.IsActive = 0

	SELECT TOP (@Top) * FROM @ValidationErrors;
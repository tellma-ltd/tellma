CREATE PROCEDURE [bll].[Documents_Validate__Sign]
	@Ids [dbo].[IndexedIdList] READONLY,
	@AgentId INT,
	--@Lines DocumentLineList = NULL,
	--@Entries DocumentLineEntryList = NULL,
	@ToState NVARCHAR(30),
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
--	Verify that @AgentId = @UserId or all transition to @ToState are IsPaperless = 0
	IF @AgentId <> @UserId
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_PaperlessTransitionCannotImpersonateSignatory0', 
			(SELECT [Name] FROM dbo.Agents WHERE [Id] = @AgentId)
		FROM @Ids 
		WHERE [Id] IN (
			SELECT D.[Id] 
			FROM dbo.Documents D
			JOIN dbo.Workflows W ON W.DocumentTypeId = D.[DocumentDefinitionId]
			WHERE W.ToState = @ToState AND [IsPaperless] = 1
		);

IF @ToState = N'Posted'
BEGIN
	-- Not allowed to cause negative fixed asset life
	-- Conservation of mass
	-- conservation of volume
	-- Document Date not before last archive date

	-- Cannot post with no entries
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHasNoEntries'
	FROM @Ids 
	WHERE [Id] NOT IN (
		SELECT [DocumentId] 
		FROM dbo.DocumentLines DL
		JOIN dbo.[DocumentLineEntries] DLE ON DL.Id = DLE.DocumentLineId
	);

	-- Cannot post a non-balanced transaction
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + ISNULL(CAST(FE.[Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TransactionHasDebitCreditDifference0',
		SUM([Direction] * [Value])
	FROM @Ids FE
	JOIN dbo.[DocumentLines] DL ON FE.[Id] = DL.[DocumentId]
	JOIN dbo.[DocumentLineEntries] DLE ON DL.[Id] = DLE.[DocumentLineId]
	GROUP BY FE.[Index]
	HAVING SUM([Direction] * [Value]) <> 0;

	-- No inactive account
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + ISNULL(CAST(FE.[Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheAccount0IsDeprecated',
		A.[Name]
	FROM @Ids FE
	JOIN dbo.[DocumentLines] DL ON FE.[Id] = DL.[DocumentId]
	JOIN dbo.[DocumentLineEntries] DLE ON DL.[Id] = DLE.[DocumentLineId]
	JOIN dbo.[Accounts] A ON A.[Id] = DLE.[AccountId]
	WHERE (A.[IsDeprecated] = 1);

	-- Not allowed to cause negative inventory balance
	WITH InventoriesAccountTypes AS (
		SELECT Id FROM dbo.AccountTypes
		WHERE [Node].IsDescendantOf(
			(SELECT [Node] FROM dbo.AccountTypes WHERE Id = N'Assets')
		) = 1
	),
	AssetAccounts AS (
		SELECT [Id] FROM dbo.[Accounts] A
		WHERE A.[AccountTypeId] IN (SELECT [Id] FROM InventoriesAccountTypes)
	),
	CurrentDocs AS (
		SELECT MAX(FE.[Index]) AS [Index], DLE.AccountId,
			SUM(DLE.[Direction] * DLE.[Mass]) AS [Mass], 
			SUM(DLE.[Direction] * DLE.[Volume]) AS [Volume], 
			SUM(DLE.[Direction] * DLE.[Count]) AS [Count], 
			SUM(DLE.[Direction] * DLE.[Area]) AS [Area]
		FROM @Ids FE
		JOIN dbo.[DocumentLines] DL ON FE.[Id] = DL.[DocumentId]
		JOIN dbo.[DocumentLineEntries] DLE ON DL.[Id] = DLE.[DocumentLineId]
		WHERE DLE.AccountId IN (SELECT [Id] FROM AssetAccounts)
		GROUP BY DLE.AccountId
		HAVING SUM(DLE.[Direction] * DLE.[Mass]) < 0
		OR SUM(DLE.[Direction] * DLE.[Volume]) < 0
		OR SUM(DLE.[Direction] * DLE.[Count]) < 0
		OR SUM(DLE.[Direction] * DLE.[Area]) < 0
	),
	PostedDocs AS (
		SELECT DLE.AccountId,
			SUM(DLE.[Direction] * DLE.[Mass]) AS [Mass], 
			SUM(DLE.[Direction] * DLE.[Volume]) AS [Volume], 
			SUM(DLE.[Direction] * DLE.[Count]) AS [Count], 
			SUM(DLE.[Direction] * DLE.[Area]) AS [Area]
		FROM dbo.DocumentLineEntriesDetailsView DLE
		JOIN CurrentDocs C ON DLE.AccountId = C.AccountId 
		GROUP BY DLE.AccountId
	),
	OffendingEntries AS (
		SELECT C.[Index], C.AccountId, (C.[Mass] + P.[Mass]) AS [Mass]
		FROM CurrentDocs C
		JOIN PostedDocs P ON C.AccountId = P.AccountId
		WHERE (C.[Mass] + P.[Mass]) < 0
		OR (C.[Volume] + P.[Volume]) < 0
		OR (C.[Count] + P.[Count]) < 0
		OR (C.[Area] + P.[Area]) < 0
	)
	-- TODO: to be rewritten for each unit of measure. Also localize!
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT
		'[' + ISNULL(CAST([Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheResource0Account1Shortage2',
		R.[Name], A.[Name], [Mass] -- 
	FROM OffendingEntries D
	JOIN dbo.[Accounts] A ON D.AccountId = A.Id
	JOIN dbo.Resources R ON A.ResourceId = R.Id
END
	SELECT TOP (@Top) * FROM @ValidationErrors;
CREATE PROCEDURE [bll].[Documents_Validate__Sign]
	@Ids [dbo].[IndexedIdList] READONLY,
	--@Lines DocumentLineList = NULL,
	--@Entries DocumentLineEntryList = NULL,
	@State NVARCHAR(30),
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

IF @State = N'Posted'
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
		N'Error_TheAccount0IsInactive',
		A.[Name]
	FROM @Ids FE
	JOIN dbo.[DocumentLines] DL ON FE.[Id] = DL.[DocumentId]
	JOIN dbo.[DocumentLineEntries] DLE ON DL.[Id] = DLE.[DocumentLineId]
	JOIN dbo.[Accounts] A ON A.[Id] = DLE.[AccountId]
	WHERE (A.[IsActive] = 0);

	-- No inactive responsibility center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + ISNULL(CAST(FE.[Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheResponsibilityCenter0IsInactive',
		RC.[Name]
	FROM @Ids FE
	JOIN dbo.[DocumentLines] DL ON FE.[Id] = DL.[DocumentId]
	JOIN dbo.[DocumentLineEntries] DLE ON DL.[Id] = DLE.[DocumentLineId]
	JOIN dbo.[ResponsibilityCenters] RC ON RC.[Id] = DLE.[ResponsibilityCenterId]
	WHERE (RC.[IsActive] = 0);

	-- No inactive Resource
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + ISNULL(CAST(FE.[Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheResource0IsInactive',
		R.[Name]
	FROM @Ids FE
	JOIN dbo.[DocumentLines] DL ON FE.[Id] = DL.[DocumentId]
	JOIN dbo.[DocumentLineEntries] DLE ON DL.[Id] = DLE.[DocumentLineId]
	JOIN dbo.[Resources] R ON R.[Id] = DLE.[ResourceId]
	WHERE (R.[IsActive] = 0);

	-- Not allowed to cause negative inventory balance
	WITH IfrsAssetAccounts AS (
		SELECT Id FROM dbo.[IfrsAccounts]
		WHERE [Node].IsDescendantOf(
			(SELECT [Node] FROM dbo.IfrsAccounts WHERE Id = N'Assets')
		) = 1
	),
	AssetAccounts AS (
		SELECT [Id] FROM dbo.Accounts A
		WHERE A.IfrsAccountId IN (
			SELECT [Id] FROM IfrsAssetAccounts
		)
	),
	CurrentDocs AS (
		SELECT MAX(FE.[Index]) AS [Index], DLE.AccountId, DLE.ResourceId, 
			SUM(DLE.[Direction] * DLE.[Mass]) AS [Mass], 
			SUM(DLE.[Direction] * DLE.[Volume]) AS [Volume], 
			SUM(DLE.[Direction] * DLE.[Count]) AS [Count], 
			SUM(DLE.[Direction] * DLE.[Area]) AS [Area]
		FROM @Ids FE
		JOIN dbo.[DocumentLines] DL ON FE.[Id] = DL.[DocumentId]
		JOIN dbo.[DocumentLineEntries] DLE ON DL.[Id] = DLE.[DocumentLineId]
		WHERE DLE.AccountId IN (SELECT [Id] FROM AssetAccounts)
		GROUP BY DLE.AccountId, DLE.ResourceId
		HAVING SUM(DLE.[Direction] * DLE.[Mass]) < 0
		OR SUM(DLE.[Direction] * DLE.[Volume]) < 0
		OR SUM(DLE.[Direction] * DLE.[Count]) < 0
		OR SUM(DLE.[Direction] * DLE.[Area]) < 0
	),
	PostedDocs AS (
		SELECT DLE.AccountId, DLE.ResourceId,
			SUM(DLE.[Direction] * DLE.[Mass]) AS [Mass], 
			SUM(DLE.[Direction] * DLE.[Volume]) AS [Volume], 
			SUM(DLE.[Direction] * DLE.[Count]) AS [Count], 
			SUM(DLE.[Direction] * DLE.[Area]) AS [Area]
		FROM dbo.Documents D
		JOIN dbo.[DocumentLines] DL ON D.[Id] = DL.[DocumentId]
		JOIN dbo.[DocumentLineEntries] DLE ON DL.[Id] = DLE.[DocumentLineId]
		JOIN CurrentDocs C ON DLE.AccountId = C.AccountId AND DLE.ResourceId = C.ResourceId
		WHERE D.[State] = N'Posted'
		GROUP BY DLE.AccountId, DLE.ResourceId
	),
	OffendingEntries AS (
		SELECT C.[Index], C.AccountId, C.ResourceId, (C.[Mass] + P.[Mass]) AS [Mass]
		FROM CurrentDocs C
		JOIN PostedDocs P ON C.AccountId = P.AccountId AND C.ResourceId = P.ResourceId
		WHERE (C.[Mass] + P.[Mass]) < 0
		OR (C.[Volume] + P.[Volume]) < 0
		OR (C.[Count] + P.[Count]) < 0
		OR (C.[Area] + P.[Area]) < 0
	)
	-- TODO: to be rewritten for each unit of measure
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT
		'[' + ISNULL(CAST([Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheResource0Account1Shortage2',
		R.[Name], A.[Name], [Mass] -- 
	FROM OffendingEntries D
	JOIN dbo.Resources R ON D.ResourceId = R.Id
	JOIN dbo.Accounts A ON D.AccountId = A.Id
END
	SELECT TOP (@Top) * FROM @ValidationErrors;
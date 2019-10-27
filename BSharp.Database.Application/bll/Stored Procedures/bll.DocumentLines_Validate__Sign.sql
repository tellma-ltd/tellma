CREATE PROCEDURE [bll].[DocumentLines_Validate__Sign]
	@Entities dbo.[IndexedIdList] READONLY,
	@AgentId INT,
	@RoleId INT,
	@ToState NVARCHAR(30),
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- TODO:
	-- Not allowed to cause negative fixed asset life
	-- Conservation of mass
	-- conservation of volume

	-- If signing on behalf of Agent
	IF @AgentId <> @UserId
	BEGIN
		-- If there is no proxy role, then Agent must sign in person
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_DocumentLineCannotBeSignedOnBehalOfAgent0',
			(SELECT [Name] FROM dbo.Agents WHERE [Id] = @AgentId)
		FROM @Entities 
		WHERE [Id] IN (
			SELECT DL.[Id] 
			FROM dbo.DocumentLines DL
			JOIN dbo.Workflows W ON W.[LineDefinitionId] = DL.[LineDefinitionId]
			JOIN dbo.WorkflowSignatures WS ON W.[Id] = WS.[WorkflowId]
			WHERE W.ToState = @ToState AND WS.[ProxyRoleId] IS NULL
		);

		-- if there is a proxy role, then User must have this role
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_User0LacksPermissionToSignDocumentLineOnBehalOfAgent1',
			(SELECT [Name] FROM dbo.Agents WHERE [Id] = @UserId),
			(SELECT [Name] FROM dbo.Agents WHERE [Id] = @AgentId)
		FROM @Entities 
		WHERE [Id] IN (
			SELECT DL.[Id] 
			FROM dbo.DocumentLines DL
			JOIN dbo.Workflows W ON W.[LineDefinitionId] = DL.[LineDefinitionId]
			JOIN dbo.WorkflowSignatures WS ON W.[Id] = WS.[WorkflowId]
			WHERE W.ToState = @ToState
			AND WS.[ProxyRoleId] NOT IN (
				SELECT [RoleId] FROM dbo.RoleMemberships
				WHERE [AgentId] = @UserId
			)
		);
	END

	-- cannot sign lines unless the document is active. Document can be active, posted/filed	,
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_DocumentLineDoesNotBelongToActiveDocument'
	FROM @Entities FE
	JOIN dbo.DocumentLines DL ON FE.[Id] = DL.[Id]
	JOIN dbo.Documents D ON DL.[DocumentId] = D.[Id]
	WHERE D.[State] <> 'Active'

	-- No inactive account
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + ISNULL(CAST(FE.[Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheAccount0IsDeprecated',
		A.[Name]
	FROM @Entities FE
	JOIN dbo.[DocumentLineEntries] DLE ON FE.[Id] = DLE.[DocumentLineId]
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
	CurrentDocLines AS (
		SELECT MAX(FE.[Index]) AS [Index], DLE.AccountId,
			SUM(DLE.[Direction] * DLE.[Mass]) AS [Mass], 
			SUM(DLE.[Direction] * DLE.[Volume]) AS [Volume], 
			SUM(DLE.[Direction] * DLE.[Count]) AS [Count], 
			SUM(DLE.[Direction] * DLE.[Area]) AS [Area]
		FROM @Entities FE
		JOIN dbo.[DocumentLineEntries] DLE ON FE.[Id] = DLE.[DocumentLineId]
		WHERE DLE.AccountId IN (SELECT [Id] FROM AssetAccounts)
		GROUP BY DLE.AccountId
		HAVING SUM(DLE.[Direction] * DLE.[Mass]) < 0
		OR SUM(DLE.[Direction] * DLE.[Volume]) < 0
		OR SUM(DLE.[Direction] * DLE.[Count]) < 0
		OR SUM(DLE.[Direction] * DLE.[Area]) < 0
	),
	ReviewedDocLines AS (
		SELECT DLE.AccountId,
			SUM(DLE.[Direction] * DLE.[Mass]) AS [Mass], 
			SUM(DLE.[Direction] * DLE.[Volume]) AS [Volume], 
			SUM(DLE.[Direction] * DLE.[Count]) AS [Count], 
			SUM(DLE.[Direction] * DLE.[Area]) AS [Area]
		FROM dbo.DocumentLineEntriesDetailsView DLE
		JOIN CurrentDocLines C ON DLE.AccountId = C.AccountId 
		GROUP BY DLE.AccountId
	),
	OffendingEntries AS (
		SELECT C.[Index], C.AccountId, (C.[Mass] + P.[Mass]) AS [Mass]
		FROM CurrentDocLines C
		JOIN ReviewedDocLines P ON C.AccountId = P.AccountId
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

	SELECT TOP (@Top) * FROM @ValidationErrors;
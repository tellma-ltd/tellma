CREATE PROCEDURE [bll].[Lines_Validate__Sign]
	@Ids dbo.[IndexedIdList] READONLY,
	@AgentId INT,
	@RoleId INT,
	@ToState NVARCHAR(30),
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- TODO:
	-- Not allowed to cause negative fixed asset life among states >= completed, if neg is not allowed.
	-- Conservation of mass
	-- conservation of volume

	-- If signing on behalf of Agent
	IF (SELECT UserId FROM dbo.Agents WHERE [Id] = @AgentId) <> @UserId
	BEGIN
		-- If there is no proxy role, then Agent must sign in person
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_LineCannotBeSignedOnBehalOfAgent0',
			(SELECT [Name] FROM dbo.Agents WHERE [Id] = @AgentId)
		FROM @Ids 
		WHERE [Id] IN (
			SELECT L.[Id] 
			FROM dbo.[Lines] L
			JOIN dbo.Workflows W ON W.[LineDefinitionId] = L.[DefinitionId]
			JOIN dbo.WorkflowSignatures WS ON W.[Id] = WS.[WorkflowId]
			WHERE W.ToState = @ToState AND WS.[ProxyRoleId] IS NULL
		);

		-- if there is a proxy role, then User must have this role
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_User0LacksPermissionToSignLineOnBehalOfAgent1',
			(SELECT [Name] FROM dbo.Users WHERE [Id] = @UserId),
			(SELECT [Name] FROM dbo.Agents WHERE [Id] = @AgentId)
		FROM @Ids 
		WHERE [Id] IN (
			SELECT L.[Id] 
			FROM dbo.[Lines] L
			JOIN dbo.Workflows W ON W.[LineDefinitionId] = L.[DefinitionId]
			JOIN dbo.WorkflowSignatures WS ON W.[Id] = WS.[WorkflowId]
			WHERE W.ToState = @ToState
			AND WS.[ProxyRoleId] NOT IN (
				SELECT [RoleId] FROM dbo.RoleMemberships
				WHERE [UserId] = @UserId
			)
		);
	END
	-- verify that the line definition has a workflow transition from its current state to @ToState
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_NoDirectTransitionFromState0ToState1',
		L.[State],
		@ToState
	FROM @Ids FE
	JOIN dbo.[Lines] L ON FE.[Id] = L.[Id]
	LEFT JOIN dbo.Workflows W ON W.[LineDefinitionId] = L.[DefinitionId] AND W.[FromState] = L.[State]
	WHERE W.ToState <> @ToState

	-- cannot sign lines unless the document is active. Document can be active, posted/filed	,
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_LineDoesNotBelongToActiveDocument'
	FROM @Ids FE
	JOIN dbo.[Lines] L ON FE.[Id] = L.[Id]
	JOIN dbo.Documents D ON L.[DocumentId] = D.[Id]
	WHERE D.[State] <> 'Active'

	-- No inactive account
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + ISNULL(CAST(FE.[Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheAccount0IsDeprecated',
		A.[Name]
	FROM @Ids FE
	JOIN dbo.[Entries] E ON FE.[Id] = E.[LineId]
	JOIN dbo.[Accounts] A ON A.[Id] = E.[AccountId]
	WHERE (A.[IsDeprecated] = 1);

	-- Not allowed to cause negative balance in conservative accounts
	DECLARE @NonFinancialResourceClassificationNode HIERARCHYID = 
		(SELECT [Node] FROM dbo.ResourceClassifications WHERE Code = N'NonFinancialAssets');
	WITH
	ConservativeAccounts AS (
		SELECT [Id] FROM dbo.[Accounts] A
		WHERE A.[ContractType] = N'OnHand'
		AND A.ResourceClassificationId IN (
			SELECT [Id] FROM dbo.ResourceClassifications
			WHERE [Node].IsDescendantOf(@NonFinancialResourceClassificationNode) = 1
		)
	),
	CurrentDocLines AS (
		SELECT MAX(FE.[Index]) AS [Index],
			E.AccountId,
			E.ResourceId,
			E.AgentId,
			E.DueDate,
			E.[AccountIdentifier],
			E.[ResourceIdentifier],
			SUM(E.[Direction] * E.[Count]) AS [Count],
			SUM(E.[Direction] * E.[Mass]) AS [Mass], 
			SUM(E.[Direction] * E.[Volume]) AS [Volume] 
			
		FROM @Ids FE
		JOIN dbo.[Entries] E ON FE.[Id] = E.[LineId]
		WHERE E.AccountId IN (SELECT [Id] FROM ConservativeAccounts)
		GROUP BY
			E.AccountId,
			E.ResourceId,
			E.AgentId,
			E.DueDate,
			E.[AccountIdentifier],
			E.[ResourceIdentifier]
		HAVING SUM(E.[Direction] * E.[Mass]) < 0
		OR SUM(E.[Direction] * E.[Volume]) < 0
		OR SUM(E.[Direction] * E.[Count]) < 0
	),
	ReviewedDocLines AS (
		SELECT
			E.AccountId,
			E.ResourceId,
			E.AgentId,
			E.DueDate,
			E.[AccountIdentifier],
			E.[ResourceIdentifier],
			SUM(E.[Direction] * E.[Mass]) AS [Mass], 
			SUM(E.[Direction] * E.[Volume]) AS [Volume], 
			SUM(E.[Direction] * E.[Count]) AS [Count]
		FROM dbo.DocumentLineEntriesDetailsView E
		JOIN CurrentDocLines C ON E.AccountId = C.AccountId 
		GROUP BY
			E.AccountId,
			E.ResourceId,
			E.AgentId,
			E.[DueDate],
			E.[AccountIdentifier],
			E.[ResourceIdentifier]
	),
	OffendingEntries AS (
		SELECT C.[Index], C.AccountId, (C.[Mass] + P.[Mass]) AS [Mass]
		FROM CurrentDocLines C
		JOIN ReviewedDocLines P ON
			C.AccountId = P.AccountId
		AND (C.ResourceId = P.ResourceId)
		AND (C.AgentId = P.AgentId)
		AND (C.[DueDate] = P.[DueDate] OR (C.[DueDate] IS NULL AND P.[DueDate] IS NULL))
		AND (C.[AccountIdentifier] = P.[AccountIdentifier] OR (C.[AccountIdentifier] IS NULL AND P.[AccountIdentifier] IS NULL))
		AND (C.[ResourceIdentifier] = P.[ResourceIdentifier] OR (C.[ResourceIdentifier] IS NULL AND P.[ResourceIdentifier] IS NULL))
		WHERE
			(C.[Count] + P.[Count]) < 0
		OR	(C.[Mass] + P.[Mass]) < 0
		OR	(C.[Volume] + P.[Volume]) < 0
	)
	-- TODO: to be rewritten for each unit of measure. Also localize!
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT TOP (@Top)
		'[' + ISNULL(CAST([Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheResource0Account1Shortage2',
		R.[Name], A.[Name], D.[Mass] -- 
	FROM OffendingEntries D
	JOIN dbo.[Accounts] A ON D.AccountId = A.Id
	JOIN dbo.Resources R ON A.ResourceId = R.Id

	SELECT TOP (@Top) * FROM @ValidationErrors;
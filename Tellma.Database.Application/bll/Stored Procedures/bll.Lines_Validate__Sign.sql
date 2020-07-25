CREATE PROCEDURE [bll].[Lines_Validate__Sign]
	@Ids dbo.[IndexedIdList] READONLY,
	@OnBehalfOfuserId INT,
	@RuleType NVARCHAR (50),
	@RoleId INT = NULL,
	@ToState SMALLINT,
	@Top INT = 10--	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
	--'ToState', 'RuleType', 'RoleId', 'AgentId', 'UserId', 'SignedById', 'SignedAt', 'OnBehalfOfUserId',
   -- 'LastUnsignedState', 'LastNegativeState', 'CanSign', 'ProxyRoleId', 'CanSignOnBehalf',
    --'ReasonId', 'ReasonDetails'
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- Verify that the signing UserId fulfills one of the required signature
	-- Corollary: Signatures are not repeated if signing twice in a row 
	-- TODO:
	-- No inactive Resource, No inactive User

	IF @OnBehalfOfuserId IS NULL SET @OnBehalfOfuserId = @UserId
	-- Must not sign lines in a document that is already closed/canceled
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_CannotSignDocumentInState0',
		N'localize:Document_State_' + (CASE WHEN D.[State] = 1 THEN N'1' WHEN D.[State] = -1 THEN N'minus_1' END)
	FROM @Ids FE
	JOIN dbo.Lines L ON FE.[Id] = L.[Id]
	JOIN [dbo].[Documents] D ON D.[Id] = L.[DocumentId]
	WHERE D.[State] <> 0; -- Closed or Canceled

	IF @RuleType = N'ByRole'
	IF @RoleId NOT IN (
		SELECT RoleId FROM dbo.RoleMemberships 
		WHERE [UserId] = @OnBehalfOfuserId
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	VALUES (
		N'UserId',
		N'Error_IncompatibleUser0Role1',
		(SELECT dbo.fn_Localize([Name], [Name2], [Name3]) FROM dbo.Users WHERE [Id] = @OnBehalfOfuserId),
		(SELECT dbo.fn_Localize([Name], [Name2], [Name3]) FROM dbo.Roles WHERE [Id] = @RoleId)
	);

	DECLARE @LineIds IdList;
	INSERT INTO @LineIds([Id]) SELECT [Id] FROM @Ids;
	-- TODO: Cannot sign a line where CanSign = 0
	--INSERT INTO @ValidationErrors([Key], [ErrorName])		
	--SELECT DISTINCT TOP (@Top)
	--	'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
	--	N'Error_UserCannotSignLine'
	--FROM map.[LinesRequiredSignatures](@LineIds) RS
	--JOIN @Ids FE ON RS.LineId = FE.Id
	--WHERE RS.CanSign = 0;

	-- Cannot sign a current state, unless all states < abs (current state) are positive and signed.	
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])		
	SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_LineMustBeInState0First',
			N'localize:Line_State_minus_' + ABS([LastUnsignedState])
	FROM map.[LinesRequiredSignatures](@LineIds) RS
	JOIN @Ids FE ON RS.LineId = FE.Id
	WHERE ToState = ABS(@ToState) AND LastUnsignedState IS NOT NULL

	-- Cannot sign a current state, if it is already signed negatively in a previous state.
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])		
	SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_LineIsAlreadyInState0',
			N'localize:Line_State_minus_' + ABS([LastNegativeState])
	FROM map.[LinesRequiredSignatures](@LineIds) RS
	JOIN @Ids FE ON RS.LineId = FE.Id
	WHERE LastNegativeState IS NOT NULL

	-- If signing on behalf of User
	IF (@OnBehalfOfuserId IS NOT NULL) AND (@OnBehalfOfuserId <> @UserId)
	BEGIN
		-- If there is no proxy role, then User must sign in person
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_LineCannotBeSignedOnBehalfOfUser0',
			(SELECT dbo.fn_Localize([Name], [Name2], [Name3]) FROM dbo.Users WHERE [Id] = @OnBehalfOfuserId)
		FROM @Ids 
		WHERE [Id] IN (
			SELECT L.[Id] 
			FROM dbo.[Lines] L
			JOIN dbo.Workflows W ON W.[LineDefinitionId] = L.[DefinitionId]
			JOIN dbo.WorkflowSignatures WS ON W.[Id] = WS.[WorkflowId]
			WHERE W.ToState = @ToState AND WS.RuleType = @RuleType AND WS.[ProxyRoleId] IS NULL
		);

		-- if there is a proxy role, then User must have this role
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_User0LacksPermissionToSignLineOnBehalfOfUser1',
			(SELECT dbo.fn_Localize([Name], [Name2], [Name3]) FROM dbo.Users WHERE [Id] = @UserId),
			(SELECT dbo.fn_Localize([Name], [Name2], [Name3]) FROM dbo.Users WHERE [Id] = @OnBehalfOfuserId)
		FROM @Ids 
		WHERE [Id] IN (
			SELECT L.[Id] 
			FROM dbo.[Lines] L
			JOIN dbo.Workflows W ON W.[LineDefinitionId] = L.[DefinitionId]
			JOIN dbo.WorkflowSignatures WS ON W.[Id] = WS.[WorkflowId]
			WHERE W.ToState = @ToState
			AND WS.RuleType = @RuleType
			AND WS.[ProxyRoleId] NOT IN (
				SELECT [RoleId] FROM dbo.RoleMemberships
				WHERE [UserId] = @UserId
			)
		);
	END

	-- Cannot sign a line by Custodian, if Cuatodian/Users is empty
	IF @RuleType = N'ByCustodian'
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheContract01HasNoUsers',
		dbo.fn_Localize(CD.[TitleSingular], CD.[TitleSingular2], CD.[TitleSingular3]) AS [CustodianDefinition],
		dbo.fn_Localize(C.[Name], C.[Name2], C.[Name3]) AS [Custodian]
	FROM @Ids FE
	JOIN dbo.[Lines] L ON FE.[Id] = L.[Id]
	JOIN dbo.[Entries] E ON L.[Id] = E.[LineId]
	JOIN dbo.[Relations] C ON C.[Id] = E.[CustodianId]
	JOIN dbo.[RelationDefinitions] CD ON C.[DefinitionId] = CD.[Id]
	JOIN dbo.[Workflows] W ON W.[LineDefinitionId] = L.[DefinitionId] AND W.[ToState] = @ToState
	JOIN dbo.[WorkflowSignatures] WS ON W.[Id] = WS.[WorkflowId]
	LEFT JOIN dbo.[RelationUsers] CU ON C.[Id] = CU.[RelationId]
	WHERE WS.[RuleType] = N'ByCustodian' AND WS.[RuleTypeEntryIndex]  = E.[Index]
	AND CU.[UserId] IS NULL

	-- Cannot sign a line with no Entries
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheLineHasNoEntries'
	FROM @Ids FE
	LEFT JOIN dbo.Entries E ON FE.[Id] = E.[LineId]
	WHERE E.[Id] IS NULL;

	-- I had to use the following trick to avoid nested calls.
	IF EXISTS(SELECT * FROM @ValidationErrors)
	BEGIN
		SELECT TOP (@Top) * FROM @ValidationErrors;
		RETURN
	END;

	DECLARE @Lines LineList, @Entries EntryList;

	INSERT INTO @Lines(
			[Index],	[DocumentIndex],[Id], [DefinitionId], [PostingDate], [Memo])
	SELECT	[Index],	[DocumentId],	[Id], [DefinitionId], [PostingDate], [Memo]
	FROM dbo.Lines L
	WHERE [DocumentId] IN (SELECT [Id] FROM @Ids)
	AND [DefinitionId] IN (SELECT [Id] FROM map.LineDefinitions() WHERE [HasWorkflow] = 1)

	INSERT INTO @Entries (
	[Index], [LineIndex], [DocumentIndex], [Id],
	[Direction], [AccountId], [CurrencyId], [CustodianId], [ResourceId], [CenterId],
	[EntryTypeId], [MonetaryValue], [Quantity], [UnitId], [Value], [Time1],
	[Time2], [ExternalReference], [AdditionalReference], [NotedRelationId], [NotedAgentName],
	[NotedAmount], [NotedDate])
	SELECT
	E.[Index],L.[Index],L.[DocumentIndex],E.[Id],
	E.[Direction],E.[AccountId],E.[CurrencyId],E.[CustodianId],E.[ResourceId],E.[CenterId],
	E.[EntryTypeId], E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value],E.[Time1],
	E.[Time2],E.[ExternalReference],E.[AdditionalReference],E.[NotedRelationId],E.[NotedAgentName],
	E.[NotedAmount],E.[NotedDate]
	FROM dbo.Entries E
	JOIN @Lines L ON E.[LineId] = L.[Id];

	EXEC [bll].[Lines_Validate__State_Data]
		@Lines = @Lines, @Entries = @Entries, @State = @ToState;
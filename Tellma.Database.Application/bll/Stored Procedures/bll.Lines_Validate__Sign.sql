CREATE PROCEDURE [bll].[Lines_Validate__Sign]
	@Ids [dbo].[IndexedIdList] READONLY,
	@OnBehalfOfUserId INT,
	@RuleType NVARCHAR (50),
	@RoleId INT = NULL,
	@ToState SMALLINT,
	@Top INT = 200,
	@UserId INT,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Verify that the signing UserId fulfills one of the required signature
	-- Corollary: Signatures are not repeated if signing twice in a row 
	-- TODO:
	-- No inactive Resource, No inactive User

	IF @OnBehalfOfUserId IS NULL SET @OnBehalfOfUserId = @UserId
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
		SELECT RoleId FROM [dbo].[RoleMemberships]
		WHERE [UserId] = @OnBehalfOfUserId
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	VALUES (
		N'UserId',
		N'Error_IncompatibleUser0Role1',
		(SELECT [dbo].[fn_Localize]([Name], [Name2], [Name3]) FROM [dbo].[Users] WHERE [Id] = @OnBehalfOfUserId),
		(SELECT [dbo].[fn_Localize]([Name], [Name2], [Name3]) FROM [dbo].[Roles] WHERE [Id] = @RoleId)
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
			N'localize:Line_State_minus_' + CAST(ABS([LastUnsignedState]) AS NVARCHAR(5))
	FROM map.[LinesRequiredSignatures](@LineIds, @UserId) RS
	JOIN @Ids FE ON RS.[LineId] = FE.[Id]
	WHERE [ToState] = ABS(@ToState) AND [LastUnsignedState] IS NOT NULL

	-- Cannot sign a current state, if it is already signed negatively in a previous state.
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])		
	SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_LineIsAlreadyInState0',
			N'localize:Line_State_minus_' + CAST(ABS([LastNegativeState]) AS NVARCHAR(5))
	FROM [map].[LinesRequiredSignatures](@LineIds, @UserId) RS
	JOIN @Ids FE ON RS.[LineId] = FE.[Id]
	WHERE [LastNegativeState] IS NOT NULL

	-- If signing on behalf of User
	IF (@OnBehalfOfUserId IS NOT NULL) AND (@OnBehalfOfUserId <> @UserId)
	BEGIN
		-- If there is no proxy role, then User must sign in person
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_LineCannotBeSignedOnBehalfOfUser0',
			(SELECT [dbo].[fn_Localize]([Name], [Name2], [Name3]) FROM [dbo].[Users] WHERE [Id] = @OnBehalfOfUserId)
		FROM @Ids 
		WHERE [Id] IN (
			SELECT L.[Id] 
			FROM [dbo].[Lines] L
			JOIN [dbo].[Workflows] W ON W.[LineDefinitionId] = L.[DefinitionId]
			JOIN [dbo].[WorkflowSignatures] WS ON W.[Id] = WS.[WorkflowId]
			WHERE W.[ToState] = @ToState AND WS.[RuleType] = @RuleType AND WS.[ProxyRoleId] IS NULL
		);

		-- if there is a proxy role, then User must have this role
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_User0LacksPermissionToSignLineOnBehalfOfUser1',
			(SELECT [dbo].[fn_Localize]([Name], [Name2], [Name3]) FROM [dbo].[Users] WHERE [Id] = @UserId),
			(SELECT [dbo].[fn_Localize]([Name], [Name2], [Name3]) FROM [dbo].[Users] WHERE [Id] = @OnBehalfOfUserId)
		FROM @Ids 
		WHERE [Id] IN (
			SELECT L.[Id] 
			FROM [dbo].[Lines] L
			JOIN [dbo].[Workflows] W ON W.[LineDefinitionId] = L.[DefinitionId]
			JOIN [dbo].[WorkflowSignatures] WS ON W.[Id] = WS.[WorkflowId]
			WHERE W.[ToState] = @ToState
			AND WS.[RuleType] = @RuleType
			AND WS.[ProxyRoleId] NOT IN (
				SELECT [RoleId] FROM [dbo].[RoleMemberships]
				WHERE [UserId] = @UserId
			)
		);
	END

	-- Cannot sign a line by Custodian, if Custodian/Users is empty -- Need testing after deployment
	IF @RuleType = N'ByCustodian'
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAgent01HasNoUsers',
		[dbo].[fn_Localize](RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3]) AS [AgentDefinition],
		[dbo].[fn_Localize](RL.[Name], RL.[Name2], RL.[Name3]) AS [Agent]
	FROM @Ids FE
	JOIN [dbo].[Lines] L ON FE.[Id] = L.[Id]
	JOIN [dbo].[Entries] E ON L.[Id] = E.[LineId]
	JOIN [dbo].[Agents] RL ON RL.[Id] = E.[AgentId]
	JOIN [dbo].[AgentDefinitions] RD ON RL.[DefinitionId] = RD.[Id]
	JOIN [dbo].[Workflows] W ON W.[LineDefinitionId] = L.[DefinitionId] AND W.[ToState] = @ToState
	JOIN [dbo].[WorkflowSignatures] WS ON W.[Id] = WS.[WorkflowId]
	LEFT JOIN [dbo].[AgentUsers] RLU ON RL.[Id] = RLU.[AgentId]
	WHERE WS.[RuleType] = N'ByCustodian' AND WS.[RuleTypeEntryIndex]  = E.[Index]
	AND RLU.[UserId] IS NULL

	-- Cannot sign a line with no Entries
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheLineHasNoEntries'
	FROM @Ids FE
	LEFT JOIN [dbo].[Entries] E ON FE.[Id] = E.[LineId]
	WHERE E.[Id] IS NULL;

	-- I had to use the following trick to avoid nested calls.
	IF EXISTS(SELECT * FROM @ValidationErrors)
	BEGIN
		SET @IsError = 1;
		SELECT TOP (@Top) * FROM @ValidationErrors;
		RETURN
	END;

	DECLARE @Documents DocumentList, @DocumentLineDefinitionEntries DocumentLineDefinitionEntryList, @Lines LineList, @Entries EntryList;

	--INSERT INTO @Documents ([Index], [Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
	--	[CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon],
	--	[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon],
	--	[CurrencyId], [CurrencyIsCommon], [ExternalReference], [ExternalReferenceIsCommon],
	--	[ReferenceSourceId], [ReferenceSourceIsCommon], [InternalReference], [InternalReferenceIsCommon]	
	--)
	--SELECT [Id], [Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
	--	[CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon],
	--	[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon],
	--	[CurrencyId], [CurrencyIsCommon], [ExternalReference], [ExternalReferenceIsCommon],
	--	[ReferenceSourceId], [ReferenceSourceIsCommon], [InternalReference], [InternalReferenceIsCommon]	
	--FROM dbo.Documents
	--WHERE [Id] IN (SELECT [Id] FROM @Ids)

	INSERT INTO @Documents ([Index], [Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon], 
		[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Duration], [DurationIsCommon], [DurationUnitId], [DurationUnitIsCommon], [Time2], [Time2IsCommon],
		[NotedDate], [NotedDateIsCommon], [ExternalReference], [ExternalReferenceIsCommon], [ReferenceSourceId], [ReferenceSourceIsCommon],
		[InternalReference], [InternalReferenceIsCommon]	
	)
	SELECT Ids.[Index], D.[Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon], 
		[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Duration], [DurationIsCommon], [DurationUnitId], [DurationUnitIsCommon], [Time2], [Time2IsCommon],
		[NotedDate], [NotedDateIsCommon], [ExternalReference], [ExternalReferenceIsCommon], [ReferenceSourceId], [ReferenceSourceIsCommon],
		[InternalReference], [InternalReferenceIsCommon]
	FROM [dbo].[Documents] D JOIN @Ids Ids ON D.[Id] = Ids.[Id]

	INSERT INTO @DocumentLineDefinitionEntries(
		[Index], [DocumentIndex], [Id], [LineDefinitionId], [EntryIndex], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon], 
		[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Duration], [DurationIsCommon], [DurationUnitId], [DurationUnitIsCommon], [Time2], [Time2IsCommon],
		[NotedDate], [NotedDateIsCommon], [ExternalReference], [ExternalReferenceIsCommon], [ReferenceSourceId], [ReferenceSourceIsCommon],
		[InternalReference], [InternalReferenceIsCommon]
	)
	SELECT 	DLDE.[Id], Ids.[Index], DLDE.[Id], [LineDefinitionId], [EntryIndex], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon], 
		[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Duration], [DurationIsCommon], [DurationUnitId], [DurationUnitIsCommon], [Time2], [Time2IsCommon],
		[NotedDate], [NotedDateIsCommon], [ExternalReference], [ExternalReferenceIsCommon], [ReferenceSourceId], [ReferenceSourceIsCommon],
		[InternalReference], [InternalReferenceIsCommon]
	FROM DocumentLineDefinitionEntries DLDE
	JOIN @Ids Ids ON DLDE.[DocumentId] = Ids.[Id]
	AND [LineDefinitionId]  IN (SELECT [Id] FROM [map].[LineDefinitions]() WHERE [HasWorkflow] = 1);

	INSERT INTO @Lines(
			[Index],	[DocumentIndex],[Id], [DefinitionId], [PostingDate], [Memo])
	SELECT	[Index],	[DocumentId],	[Id], [DefinitionId], [PostingDate], [Memo]
	FROM [dbo].[Lines] L
	WHERE [DocumentId] IN (SELECT [Id] FROM @Ids)
	AND [DefinitionId] IN (SELECT [Id] FROM map.LineDefinitions() WHERE [HasWorkflow] = 1)

	INSERT INTO @Entries (
		[Index], [LineIndex], [DocumentIndex], [Id],
		[Direction], [AccountId], [CurrencyId], [AgentId], [NotedAgentId], [ResourceId], [NotedResourceId], [CenterId],
		[EntryTypeId], [MonetaryValue], [Quantity], [UnitId], [Value], [Time1],
		[Time2], [ExternalReference], [ReferenceSourceId], [InternalReference], [NotedAgentName],
		[NotedAmount], [NotedDate])
	SELECT
		E.[Index],L.[Index],L.[DocumentIndex],E.[Id],
		E.[Direction],E.[AccountId],E.[CurrencyId],E.[AgentId],E.[NotedAgentId],E.[ResourceId],E.[NotedResourceId],E.[CenterId],
		E.[EntryTypeId], E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value],E.[Time1],
		E.[Time2],E.[ExternalReference],E.[ReferenceSourceId], E.[InternalReference],E.[NotedAgentName],
		E.[NotedAmount],E.[NotedDate]
	FROM [dbo].[Entries] E
	JOIN @Lines L ON E.[LineId] = L.[Id];

	EXEC [bll].[Lines_Validate__State_Data]
		@Documents = @Documents,
		@DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines, 
		@Entries = @Entries, 
		@State = @ToState,
		@IsError = @IsError OUTPUT;

	DECLARE @PreScript NVARCHAR(MAX) = N'
	SET NOCOUNT ON
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	------
	';
	DECLARE @Script NVARCHAR (MAX);
	DECLARE @PostScript NVARCHAR(MAX) = N'
	-----
	SELECT TOP (@Top) * FROM @ValidationErrors;
	';

	DECLARE @SignValidateScriptLineDefinitions [dbo].[StringList], @LineDefinitionId INT;
	DECLARE @LineState SMALLINT, @D DocumentList, @L LineList, @E EntryList;
	INSERT INTO @SignValidateScriptLineDefinitions
	SELECT DISTINCT DefinitionId FROM @Lines
	WHERE DefinitionId IN (
		SELECT [Id] FROM dbo.LineDefinitions
		WHERE [SignValidateScript] IS NOT NULL
	);

	IF EXISTS (SELECT * FROM @SignValidateScriptLineDefinitions)
	BEGIN
		-- run script to validate information
		DECLARE LineDefinition_Cursor CURSOR FOR SELECT [Id] FROM @SignValidateScriptLineDefinitions;  
		OPEN LineDefinition_Cursor  
		FETCH NEXT FROM LineDefinition_Cursor INTO @LineDefinitionId; 
		WHILE @@FETCH_STATUS = 0  
		BEGIN 
			SELECT @Script =  @PreScript + ISNULL([SignValidateScript],N'') + @PostScript
			FROM dbo.LineDefinitions WHERE [Id] = @LineDefinitionId;
			DELETE FROM @L; DELETE FROM @E;
			INSERT INTO @L SELECT * FROM @Lines WHERE DefinitionId = @LineDefinitionId
			INSERT INTO @E SELECT E.* FROM @Entries E JOIN @L L ON E.LineIndex = L.[Index] AND E.DocumentIndex = L.DocumentIndex
			INSERT INTO @ValidationErrors
			EXECUTE	dbo.sp_executesql @Script, N'
				@LineDefinitionId INT,
				@ToState SMALLINT,
				@Documents [dbo].[DocumentList] READONLY,
				@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
				@Lines [dbo].[LineList] READONLY, 
				@Entries [dbo].EntryList READONLY,
				@Top INT', 	@LineDefinitionId = @LineDefinitionId, @ToState = @ToState, @Documents = @Documents,
				@DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries, @Lines = @L, @Entries = @E, @Top = @Top;
			
			FETCH NEXT FROM LineDefinition_Cursor INTO @LineDefinitionId;
		END
	END
END;
CREATE PROCEDURE [dal].[LineDefinitions__Save]
	@Entities [LineDefinitionList] READONLY,
	@LineDefinitionEntries [LineDefinitionEntryList] READONLY,
	@LineDefinitionEntryCustodyDefinitions [LineDefinitionEntryCustodyDefinitionList] READONLY,
	@LineDefinitionEntryResourceDefinitions LineDefinitionEntryResourceDefinitionList READONLY,
	@LineDefinitionEntryNotedRelationDefinitions [LineDefinitionEntryNotedRelationDefinitionList] READONLY,
	@LineDefinitionColumns [LineDefinitionColumnList] READONLY,
	@LineDefinitionGenerateParameters [LineDefinitionGenerateParameterList] READONLY,
	@LineDefinitionStateReasons [LineDefinitionStateReasonList] READONLY,
	@Workflows [WorkflowList] READONLY,
	@WorkflowSignatures [WorkflowSignatureList] READONLY,
	@ReturnIds BIT = 0
AS
SET NOCOUNT ON;
	DECLARE @LineDefinitionsIndexedIds [dbo].[IndexedIdList], @LineDefinitionEntriesIndexIds [dbo].[IndexIdWithHeaderList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	DECLARE @WorkflowIndexedIds [dbo].[IndexIdWithHeaderList];

	INSERT INTO @LineDefinitionsIndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[LineDefinitions] AS t
		USING (
			SELECT
				[Index],
				[Id],
				[Code],
				[Description],
				[Description2],
				[Description3],
				[TitleSingular],
				[TitleSingular2],
				[TitleSingular3],
				[TitlePlural],
				[TitlePlural2],
				[TitlePlural3],
				[AllowSelectiveSigning],
				[ViewDefaultsToForm],
				[GenerateScript],
				[GenerateLabel],
				[GenerateLabel2],
				[GenerateLabel3],
				[Script]
			FROM @Entities 
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED
			--AND (
			--t.[Code]						<> s.[Code] OR
			--t.[TitleSingular]				<> s.[TitleSingular] OR	
			--t.[TitlePlural]					<> s.[TitlePlural] OR
			--t.[AllowSelectiveSigning]		<> s.[AllowSelectiveSigning] OR
			--ISNULL(t.[Description], N'')	<> ISNULL(s.[Description], N'') OR	
			--ISNULL(t.[Description2], N'')	<> ISNULL(s.[Description2], N'') OR
			--ISNULL(t.[Description3], N'')	<> ISNULL(s.[Description3], N'') OR	
			--ISNULL(t.[TitleSingular2], N'')	<> ISNULL(s.[TitleSingular2], N'') OR	
			--ISNULL(t.[TitlePlural2], N'')	<> ISNULL(s.[TitlePlural2], N'') OR
			--ISNULL(t.[TitleSingular3], N'')	<> ISNULL(s.[TitleSingular3], N'') OR	
			--ISNULL(t.[TitlePlural3], N'')	<> ISNULL(s.[TitlePlural3], N'') OR
			--ISNULL(t.[GenerateScript], N'')	<> ISNULL(s.[GenerateScript], N'') OR
			--ISNULL(t.[Script], N'')			<> ISNULL(s.[Script], N'')
			--)
		THEN
			UPDATE SET
				t.[Code]						= s.[Code],
				t.[Description]					= s.[Description],
				t.[Description2]				= s.[Description2],
				t.[Description3]				= s.[Description3],
				t.[TitleSingular]				= s.[TitleSingular],
				t.[TitleSingular2]				= s.[TitleSingular2],
				t.[TitleSingular3]				= s.[TitleSingular3],
				t.[TitlePlural]					= s.[TitlePlural],
				t.[TitlePlural2]				= s.[TitlePlural2],
				t.[TitlePlural3]				= s.[TitlePlural3],
				t.[AllowSelectiveSigning]		= s.[AllowSelectiveSigning],
				t.[ViewDefaultsToForm]			= s.[ViewDefaultsToForm],
				t.[GenerateScript]				= s.[GenerateScript],
				t.[GenerateLabel]				= s.[GenerateLabel],
				t.[GenerateLabel2]				= s.[GenerateLabel2],
				t.[GenerateLabel3]				= s.[GenerateLabel3],
				t.[Script]						= s.[Script],
				t.[SavedById]					= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[Code],
				[Description],
				[Description2],
				[Description3],
				[TitleSingular],
				[TitleSingular2],
				[TitleSingular3],
				[TitlePlural],
				[TitlePlural2],
				[TitlePlural3],
				[AllowSelectiveSigning],
				[ViewDefaultsToForm],
				[GenerateScript],
				[GenerateLabel],
				[GenerateLabel2],
				[GenerateLabel3],
				[Script]
			)
			VALUES (
				s.[Code],
				s.[Description],
				s.[Description2],
				s.[Description3],
				s.[TitleSingular],
				s.[TitleSingular2],
				s.[TitleSingular3],
				s.[TitlePlural],
				s.[TitlePlural2],
				s.[TitlePlural3],
				s.[AllowSelectiveSigning],
				s.[ViewDefaultsToForm],
				s.[GenerateScript],
				s.[GenerateLabel],
				s.[GenerateLabel2],
				s.[GenerateLabel3],
				s.[Script])
		OUTPUT s.[Index], inserted.[Id]
	) AS x;

	WITH BLDE AS (
		SELECT * FROM dbo.[LineDefinitionEntries]
		WHERE LineDefinitionId IN (SELECT [Id] FROM @LineDefinitionsIndexedIds)
	)
	INSERT INTO @LineDefinitionEntriesIndexIds([Index], [HeaderId], [Id])
	SELECT x.[Index], x.[LineDefinitionId], x.[Id]
	FROM
	(
		MERGE INTO BLDE AS t
		USING (
			SELECT
				LDE.[Id],
				II.[Id] AS [LineDefinitionId],
				LDE.[Index],
				LDE.[Direction],
				LDE.[AccountTypeId],
				LDE.[EntryTypeId]
			FROM @LineDefinitionEntries LDE
			JOIN @LineDefinitionsIndexedIds II ON LDE.[HeaderIndex] = II.[Index]
		) AS s ON s.[Id] = t.[Id]
		WHEN MATCHED 
		AND (
				t.[Index]					<> s.[Index] OR
				t.[Direction]				<> s.[Direction] OR
				t.[AccountTypeId]			<> s.[AccountTypeId] OR
				ISNULL(t.[EntryTypeId],0)	<> ISNULL(s.[EntryTypeId],0)
		)
		THEN
			UPDATE SET
				t.[Index]					= s.[Index],
				t.[Direction]				= s.[Direction],
				t.[AccountTypeId]			= s.[AccountTypeId],
				t.[EntryTypeId]				= s.[EntryTypeId],
				t.[SavedById]				= @UserId
		WHEN NOT MATCHED BY TARGET THEN
			INSERT (
				[LineDefinitionId],
				[Index],
				[Direction],
				[AccountTypeId],
				[EntryTypeId]
			)
			VALUES (
				s.[LineDefinitionId],
				s.[Index],
				s.[Direction],
				s.[AccountTypeId],
				s.[EntryTypeId]
			)
		WHEN NOT MATCHED BY SOURCE THEN
			DELETE
		OUTPUT s.[Index], inserted.[Id], inserted.[LineDefinitionId]
	) AS x
	WHERE [Index] IS NOT NULL;

	WITH BLDERD AS (
		SELECT * FROM dbo.[LineDefinitionEntryResourceDefinitions]
		WHERE [LineDefinitionEntryId] IN (SELECT [Id] FROM @LineDefinitionEntriesIndexIds)
	)
	MERGE INTO BLDERD AS t
	USING (
		SELECT
			E.[Id], LI.Id AS [LineDefinitionEntryId], E.[ResourceDefinitionId]
		FROM @LineDefinitionEntryResourceDefinitions E
		JOIN @LineDefinitionsIndexedIds DI ON E.[LineDefinitionIndex] = DI.[Index]
		JOIN @LineDefinitionEntriesIndexIds LI ON E.[LineDefinitionEntryIndex] = LI.[Index] AND LI.[HeaderId] = DI.[Id]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED
	--AND (
	--	t.[ResourceDefinitionId] <> s.[ResourceDefinitionId]
	--)
	THEN
		UPDATE SET
			t.[ResourceDefinitionId]	= s.[ResourceDefinitionId],
			t.[ModifiedAt]				= @Now,
			t.[ModifiedById]			= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([LineDefinitionEntryId], [ResourceDefinitionId])
		VALUES (s.[LineDefinitionEntryId], s.[ResourceDefinitionId])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	WITH BLDECD AS (
		SELECT * FROM dbo.[LineDefinitionEntryCustodyDefinitions]
		WHERE [LineDefinitionEntryId] IN (SELECT [Id] FROM @LineDefinitionEntriesIndexIds)
	)
	MERGE INTO BLDECD AS t
	USING (
		SELECT
			E.[Id], LI.Id AS [LineDefinitionEntryId], E.[CustodyDefinitionId]
		FROM @LineDefinitionEntryCustodyDefinitions E
		JOIN @LineDefinitionsIndexedIds DI ON E.[LineDefinitionIndex] = DI.[Index]
		JOIN @LineDefinitionEntriesIndexIds LI ON E.[LineDefinitionEntryIndex] = LI.[Index] AND LI.[HeaderId] = DI.[Id]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED
	--AND (
	--	t.[CustodyDefinitionId]	<> s.[CustodyDefinitionId]
	--)
	THEN
		UPDATE SET
			t.[CustodyDefinitionId]	= s.[CustodyDefinitionId],
			t.[ModifiedAt]			= @Now,
			t.[ModifiedById]		= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([LineDefinitionEntryId], [CustodyDefinitionId])
		VALUES (s.[LineDefinitionEntryId], s.[CustodyDefinitionId])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	WITH BLDENRD AS (
		SELECT * FROM dbo.[LineDefinitionEntryNotedRelationDefinitions]
		WHERE [LineDefinitionEntryId] IN (SELECT [Id] FROM @LineDefinitionEntriesIndexIds)
	)
	MERGE INTO BLDENRD AS t
	USING (
		SELECT
			E.[Id], LI.Id AS [LineDefinitionEntryId], E.[NotedRelationDefinitionId]
		FROM @LineDefinitionEntryNotedRelationDefinitions E
		JOIN @LineDefinitionsIndexedIds DI ON E.[LineDefinitionIndex] = DI.[Index]
		JOIN @LineDefinitionEntriesIndexIds LI ON E.[LineDefinitionEntryIndex] = LI.[Index] AND LI.[HeaderId] = DI.[Id]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED
	--AND (
	--	t.[NotedRelationDefinitionId] <> s.[NotedRelationDefinitionId]
	--)
	THEN
		UPDATE SET
			t.[NotedRelationDefinitionId]= s.[NotedRelationDefinitionId],
			t.[ModifiedAt]				= @Now,
			t.[ModifiedById]			= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([LineDefinitionEntryId], [NotedRelationDefinitionId])
		VALUES (s.[LineDefinitionEntryId], s.[NotedRelationDefinitionId])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	WITH BLDC AS (
		SELECT * FROM dbo.[LineDefinitionColumns]
		WHERE LineDefinitionId IN (SELECT [Id] FROM @LineDefinitionsIndexedIds)
	)
	MERGE INTO BLDC AS t
	USING (
		SELECT
			LDC.[Id],
			II.[Id] AS [LineDefinitionId],
			LDC.[Index],
			LDC.[ColumnName],
			LDC.[EntryIndex],
			LDC.[Label],
			LDC.[Label2],
			LDC.[Label3],
			LDC.[VisibleState],
			LDC.[RequiredState],
			LDC.[ReadOnlyState],
			LDC.[InheritsFromHeader]
		FROM @LineDefinitionColumns LDC
	--	JOIN @Entities LD ON LDC.HeaderIndex = LD.[Index]
		JOIN @LineDefinitionsIndexedIds II ON LDC.[HeaderIndex] = II.[Index]
	) AS s
	ON s.[Id] = t.[Id]
	WHEN MATCHED 
	AND (
			t.[ColumnName]		<> s.[ColumnName] OR
			t.[ColumnName]		<> s.[ColumnName] OR
			t.[EntryIndex]		<> s.[EntryIndex] OR
			t.[Label]			<> s.[Label] OR
			t.[Label2]			<> s.[Label2] OR
			t.[Label3]			<> s.[Label3] OR
			t.[VisibleState]	<> s.[VisibleState] OR
			t.[RequiredState]	<> s.[RequiredState] OR
			t.[ReadOnlyState]	<> s.[ReadOnlyState] OR
			t.[InheritsFromHeader]<>s.[InheritsFromHeader]
	)
	THEN
		UPDATE SET
			t.[Index]			= s.[Index],
			t.[ColumnName]		= s.[ColumnName],
			t.[EntryIndex]		= s.[EntryIndex],
			t.[Label]			= s.[Label],
			t.[Label2]			= s.[Label2],
			t.[Label3]			= s.[Label3],
			t.[VisibleState]	= s.[VisibleState],
			t.[RequiredState]	= s.[RequiredState],
			t.[ReadOnlyState]	= s.[ReadOnlyState],
			t.[InheritsFromHeader]=s.[InheritsFromHeader],
			t.[SavedById]		= @UserId
	WHEN NOT MATCHED BY TARGET THEN
		INSERT ([LineDefinitionId],		[Index],	[ColumnName],	[EntryIndex], [Label],	[Label2],	[Label3], [VisibleState],	[RequiredState], [ReadOnlyState], [InheritsFromHeader])
		VALUES (s.[LineDefinitionId], s.[Index], s.[ColumnName], s.[EntryIndex], s.[Label], s.[Label2], s.[Label3],s.[VisibleState], s.[RequiredState], s.[ReadOnlyState], s.[InheritsFromHeader])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	WITH BLGDP AS (
		SELECT * FROM dbo.[LineDefinitionGenerateParameters]
		WHERE LineDefinitionId IN (SELECT [Id] FROM @LineDefinitionsIndexedIds)
	)
	MERGE INTO BLGDP AS t
	USING (
		SELECT
			LDGP.[Id],
			II.[Id] AS [LineDefinitionId],
			LDGP.[Index],
			LDGP.[Key],
			LDGP.[Label],
			LDGP.[Label2],
			LDGP.[Label3],
			LDGP.[Visibility],
			LDGP.[DataType],
			LDGP.[Filter]
		FROM @LineDefinitionGenerateParameters LDGP
	--	JOIN @Entities LD ON LDGP.HeaderIndex = LD.[Index]
		JOIN @LineDefinitionsIndexedIds II ON LDGP.[HeaderIndex] = II.[Index]
	) AS s
	ON s.[Id] = t.[Id]
	WHEN MATCHED 
	AND (
			t.[Index]			<> s.[Index] OR
			t.[Key]				<> s.[Key] OR
			t.[Label]			<> s.[Label] OR
			t.[Label2]			<> s.[Label2] OR
			t.[Label3]			<> s.[Label3] OR
			t.[Visibility]		<> s.[Visibility] OR
			t.[DataType]		<> s.[DataType] OR
			t.[Filter]			<> s.[Filter]
	)
	THEN
		UPDATE SET
			t.[Index]			= s.[Index],
			t.[Key]				= s.[Key],
			t.[Label]			= s.[Label],
			t.[Label2]			= s.[Label2],
			t.[Label3]			= s.[Label3],
			t.[Visibility]		= s.[Visibility],
			t.[DataType]		= s.[DataType],
			t.[Filter]			= s.[Filter],
			t.[SavedById]		= @UserId
	WHEN NOT MATCHED BY TARGET THEN
		INSERT ([LineDefinitionId],		[Index],	[Key],	[Label],	[Label2],	[Label3], [Visibility],	[DataType], [Filter])
		VALUES (s.[LineDefinitionId], s.[Index], s.[Key], s.[Label], s.[Label2], s.[Label3],s.[Visibility], s.[DataType], s.[Filter])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	WITH BLGSR AS (
		SELECT * FROM dbo.[LineDefinitionStateReasons]
		WHERE LineDefinitionId IN (SELECT [Id] FROM @LineDefinitionsIndexedIds)
	)
	MERGE [dbo].[LineDefinitionStateReasons] AS t
	USING (
		SELECT
			LDSR.[Id],
			II.[Id] AS [LineDefinitionId],
			LDSR.[State],
			LDSR.[Name],
			LDSR.[Name2],
			LDSR.[Name3],
			LDSR.[IsActive]
		FROM @LineDefinitionStateReasons LDSR
	--	JOIN @Entities LD ON LDSR.HeaderIndex = LD.[Index]
		JOIN @LineDefinitionsIndexedIds II ON LDSR.[HeaderIndex] = II.[Index]
	) AS s
	ON s.Id = t.Id
	WHEN MATCHED
	AND (
			t.[LineDefinitionId]<> s.[LineDefinitionId] OR
			t.[State]			<> s.[State] OR
			t.[Name]			<> s.[Name] OR
			t.[Name2]			<> s.[Name2] OR
			t.[Name3]			<> s.[Name3] OR
			t.[IsActive]		<> s.[IsActive]
	)
	THEN
		UPDATE SET
			t.[LineDefinitionId]= s.[LineDefinitionId],
			t.[State]			= s.[State],
			t.[Name]			= s.[Name],
			t.[Name2]			= s.[Name2],
			t.[Name3]			= s.[Name3],
			t.[IsActive]		= s.[IsActive],
			t.[SavedById]		= @UserId
	WHEN NOT MATCHED BY TARGET THEN
		INSERT ([LineDefinitionId],		[State], [Name],	[Name2], [Name3], [IsActive])
		VALUES (s.[LineDefinitionId], s.[State], s.[Name], s.[Name2], s.[Name3], s.[IsActive])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	WITH BLDW AS (
		SELECT * FROM dbo.[Workflows]
		WHERE LineDefinitionId IN (SELECT [Id] FROM @LineDefinitionsIndexedIds)
	)
	INSERT INTO @WorkflowIndexedIds([Index], [HeaderId], [Id])
	SELECT x.[Index], x.[LineDefinitionId], x.[Id]
	FROM
	(
		MERGE INTO BLDW AS t
		USING (
			SELECT
				W.[Index],
				W.[Id],
				II.[Id] AS [LineDefinitionId],
				W.[ToState]
			FROM @Workflows W
			JOIN @LineDefinitionsIndexedIds II ON W.[LineDefinitionIndex] = II.[Index]
		) AS s
		ON s.[Id] = t.[Id]
		WHEN MATCHED 
		--AND (
		--	t.[ToState] <> s.[ToState]
		--)
		THEN
			UPDATE SET
				t.[ToState]		= s.[ToState],
				t.[SavedById]	= @UserId
		WHEN NOT MATCHED BY TARGET THEN
			INSERT (
				[LineDefinitionId],
				[ToState]
			)
			VALUES (
				s.[LineDefinitionId],
				s.[ToState]
			)
		WHEN NOT MATCHED BY SOURCE THEN
			DELETE
		OUTPUT s.[Index], inserted.[Id], inserted.[LineDefinitionId]
	) AS x
	WHERE [Index] IS NOT NULL;

	WITH BLDWS AS (
		SELECT * FROM dbo.[WorkflowSignatures] -- check if there are already signatures for the transition
		WHERE [WorkflowId] IN (SELECT [Id] FROM @WorkflowIndexedIds)
	)
	MERGE INTO BLDWS AS t
	USING (
		SELECT
			WS.[Index],
			WS.[Id], -- when 0 then it is inserted
			WI.[Id] AS WorkflowId,
			WS.[RuleType],
			WS.[RuleTypeEntryIndex],
			WS.[RoleId],
			WS.[UserId],
			WS.[PredicateType],
			WS.[PredicateTypeEntryIndex],
			WS.[Value],
			WS.[ProxyRoleId]
		FROM @WorkflowSignatures WS
		JOIN @Workflows W ON  WS.[WorkflowIndex] = W.[Index] AND WS.[LineDefinitionIndex] = W.[LineDefinitionIndex]
		JOIN @LineDefinitionsIndexedIds LDI ON LDI.[Index] =  W.[LineDefinitionIndex]
		JOIN @WorkflowIndexedIds WI ON W.[Index] = WI.[Index] AND WI.[HeaderId] = LDI.[Id]
	) AS s ON s.[Id] = t.[Id]
	WHEN MATCHED
	--AND (
	--		t.[RuleType]				<> s.[RuleType] OR
	--		t.[RuleTypeEntryIndex]		<> s.[RuleTypeEntryIndex] OR
	--		t.[RoleId]					<> s.[RoleId] OR
	--		t.[UserId]					<> s.[UserId] OR
	--		t.[PredicateType]			<> s.[PredicateType] OR
	--		t.[PredicateTypeEntryIndex]	<> s.[PredicateTypeEntryIndex] OR
	--		t.[Value]					<> s.[Value] OR
	--		t.[ProxyRoleId]				<> s.[ProxyRoleId]
	--) 
	THEN
		UPDATE SET
			t.[RuleType]				= s.[RuleType],
			t.[RuleTypeEntryIndex]		= s.[RuleTypeEntryIndex],
			t.[RoleId]					= s.[RoleId],
			t.[UserId]					= s.[UserId],
			t.[PredicateType]			= s.[PredicateType],
			t.[PredicateTypeEntryIndex]	= s.[PredicateTypeEntryIndex],
			t.[Value]					= s.[Value],
			t.[ProxyRoleId]				= s.[ProxyRoleId],
			t.[SavedById]				= @UserId
	WHEN NOT MATCHED THEN
		INSERT (
			[WorkflowId],
			[RuleType],
			[RuleTypeEntryIndex],
			[RoleId],
			[UserId],
			[PredicateType],
			[PredicateTypeEntryIndex],
			[Value],
			[ProxyRoleId]
		)
		VALUES (
			s.[WorkflowId],
			s.[RuleType],
			s.[RuleTypeEntryIndex],
			s.[RoleId],
			s.[UserId],
			s.[PredicateType],
			s.[PredicateTypeEntryIndex],
			s.[Value],
			s.[ProxyRoleId]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- Signal clients to refresh their cache
	UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();

IF @ReturnIds = 1
	SELECT * FROM @LineDefinitionsIndexedIds;
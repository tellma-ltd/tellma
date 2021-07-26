CREATE PROCEDURE [dal].[LineDefinitions__Save]
	@Entities [LineDefinitionList] READONLY,
	@LineDefinitionEntries [LineDefinitionEntryList] READONLY,
	@LineDefinitionEntryRelationDefinitions LineDefinitionEntryRelationDefinitionList READONLY,
	@LineDefinitionEntryCustodianDefinitions LineDefinitionEntryCustodianDefinitionList READONLY,
	@LineDefinitionEntryResourceDefinitions LineDefinitionEntryResourceDefinitionList READONLY,
	@LineDefinitionEntryNotedRelationDefinitions LineDefinitionEntryNotedRelationDefinitionList READONLY,
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
				[BarcodeColumnIndex],
				[BarcodeProperty],
				[BarcodeExistingItemHandling],
				[BarcodeBeepsEnabled],
				[GenerateLabel],
				[GenerateLabel2],
				[GenerateLabel3],
				[GenerateScript],
				[PreprocessScript],
				[ValidateScript]
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
				t.[BarcodeColumnIndex]			= s.[BarcodeColumnIndex],
				t.[BarcodeProperty]				= s.[BarcodeProperty],
				t.[BarcodeExistingItemHandling]	= s.[BarcodeExistingItemHandling],
				t.[BarcodeBeepsEnabled]			= s.[BarcodeBeepsEnabled],
				t.[GenerateLabel]				= s.[GenerateLabel],
				t.[GenerateLabel2]				= s.[GenerateLabel2],
				t.[GenerateLabel3]				= s.[GenerateLabel3],
				t.[GenerateScript]				= s.[GenerateScript],
				t.[PreprocessScript]			= s.[PreprocessScript],
				t.[ValidateScript]				= s.[ValidateScript],
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
				[BarcodeColumnIndex],
				[BarcodeProperty],
				[BarcodeExistingItemHandling],
				[BarcodeBeepsEnabled],
				[GenerateLabel],
				[GenerateLabel2],
				[GenerateLabel3],
				[GenerateScript],
				[PreprocessScript],
				[ValidateScript]
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
				s.[BarcodeColumnIndex],
				s.[BarcodeProperty],
				s.[BarcodeExistingItemHandling],
				s.[BarcodeBeepsEnabled],
				s.[GenerateLabel],
				s.[GenerateLabel2],
				s.[GenerateLabel3],
				s.[GenerateScript],
				s.[PreprocessScript],
				s.[ValidateScript]
			)
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
				LDE.[ParentAccountTypeId],
				LDE.[EntryTypeId]
			FROM @LineDefinitionEntries LDE
			JOIN @LineDefinitionsIndexedIds II ON LDE.[HeaderIndex] = II.[Index]
		) AS s ON s.[Id] = t.[Id]
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[Index]					= s.[Index],
				t.[Direction]				= s.[Direction],
				t.[ParentAccountTypeId]		= s.[ParentAccountTypeId],
				t.[EntryTypeId]				= s.[EntryTypeId],
				t.[SavedById]				= @UserId
		WHEN NOT MATCHED BY TARGET THEN
			INSERT (
				[LineDefinitionId],
				[Index],
				[Direction],
				[ParentAccountTypeId],
				[EntryTypeId]
			)
			VALUES (
				s.[LineDefinitionId],
				s.[Index],
				s.[Direction],
				s.[ParentAccountTypeId],
				s.[EntryTypeId]
			)
		WHEN NOT MATCHED BY SOURCE THEN
			DELETE
		OUTPUT s.[Index], inserted.[Id], inserted.[LineDefinitionId]
	) AS x
	WHERE [Index] IS NOT NULL;

	WITH BLDERLD AS (
		SELECT * FROM dbo.[LineDefinitionEntryRelationDefinitions]
		WHERE [LineDefinitionEntryId] IN (SELECT [Id] FROM @LineDefinitionEntriesIndexIds)
	)
	MERGE INTO BLDERLD AS t
	USING (
		SELECT
			E.[Id], LI.Id AS [LineDefinitionEntryId], E.[RelationDefinitionId]
		FROM @LineDefinitionEntryRelationDefinitions E
		JOIN @LineDefinitionsIndexedIds DI ON E.[LineDefinitionIndex] = DI.[Index]
		JOIN @LineDefinitionEntriesIndexIds LI ON E.[LineDefinitionEntryIndex] = LI.[Index] AND LI.[HeaderId] = DI.[Id]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED
	AND (
		ISNULL(t.[RelationDefinitionId],0) <> ISNULL(s.[RelationDefinitionId],0)
	)
	THEN
		UPDATE SET
			t.[RelationDefinitionId]	= s.[RelationDefinitionId],
			t.[SavedById]				= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([LineDefinitionEntryId], [RelationDefinitionId])
		VALUES (s.[LineDefinitionEntryId], s.[RelationDefinitionId])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	WITH BLDECD AS (
		SELECT * FROM dbo.[LineDefinitionEntryCustodianDefinitions]
		WHERE [LineDefinitionEntryId] IN (SELECT [Id] FROM @LineDefinitionEntriesIndexIds)
	)
	MERGE INTO BLDECD AS t
	USING (
		SELECT
			E.[Id], LI.Id AS [LineDefinitionEntryId], E.[CustodianDefinitionId]
		FROM @LineDefinitionEntryCustodianDefinitions E
		JOIN @LineDefinitionsIndexedIds DI ON E.[LineDefinitionIndex] = DI.[Index]
		JOIN @LineDefinitionEntriesIndexIds LI ON E.[LineDefinitionEntryIndex] = LI.[Index] AND LI.[HeaderId] = DI.[Id]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED
	AND (
		ISNULL(t.[CustodianDefinitionId],0) <> ISNULL(s.[CustodianDefinitionId],0)
	)
	THEN
		UPDATE SET
			t.[CustodianDefinitionId]	= s.[CustodianDefinitionId],
			t.[SavedById]				= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([LineDefinitionEntryId], [CustodianDefinitionId])
		VALUES (s.[LineDefinitionEntryId], s.[CustodianDefinitionId])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

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
	AND (
		ISNULL(t.[ResourceDefinitionId],0) <> ISNULL(s.[ResourceDefinitionId],0)
	)
	THEN
		UPDATE SET
			t.[ResourceDefinitionId]	= s.[ResourceDefinitionId],
			t.[SavedById]				= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([LineDefinitionEntryId], [ResourceDefinitionId])
		VALUES (s.[LineDefinitionEntryId], s.[ResourceDefinitionId])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	WITH BLDENRLD AS (
		SELECT * FROM dbo.[LineDefinitionEntryNotedRelationDefinitions]
		WHERE [LineDefinitionEntryId] IN (SELECT [Id] FROM @LineDefinitionEntriesIndexIds)
	)
	MERGE INTO BLDENRLD AS t
	USING (
		SELECT
			E.[Id], LI.Id AS [LineDefinitionEntryId], E.[NotedRelationDefinitionId]
		FROM @LineDefinitionEntryNotedRelationDefinitions E
		JOIN @LineDefinitionsIndexedIds DI ON E.[LineDefinitionIndex] = DI.[Index]
		JOIN @LineDefinitionEntriesIndexIds LI ON E.[LineDefinitionEntryIndex] = LI.[Index] AND LI.[HeaderId] = DI.[Id]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED
	AND (
		ISNULL(t.[NotedRelationDefinitionId],0) <> ISNULL(s.[NotedRelationDefinitionId],0)
	)
	THEN
		UPDATE SET
			t.[NotedRelationDefinitionId]	= s.[NotedRelationDefinitionId],
			t.[SavedById]				= @UserId
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
			LDC.[Filter],
			LDC.[VisibleState],
			LDC.[RequiredState],
			LDC.[ReadOnlyState],
			LDC.[InheritsFromHeader]
		FROM @LineDefinitionColumns LDC
		JOIN @LineDefinitionsIndexedIds II ON LDC.[HeaderIndex] = II.[Index]
	) AS s
	ON s.[Id] = t.[Id]
	WHEN MATCHED 
	AND (
			t.[Index]				<> s.[Index] OR
			t.[ColumnName]			<> s.[ColumnName] OR
			t.[EntryIndex]			<> s.[EntryIndex] OR
			t.[Label]				<> s.[Label] OR
			ISNULL(t.[Label2],N'')	<> ISNULL(s.[Label2],N'') OR
			ISNULL(t.[Label3],N'')	<> ISNULL(s.[Label3],N'') OR
			ISNULL(t.[Filter],N'')	<> ISNULL(s.[Filter],N'') OR
			t.[VisibleState]		<> s.[VisibleState] OR
			t.[RequiredState]		<> s.[RequiredState] OR
			t.[ReadOnlyState]		<> s.[ReadOnlyState] OR
			t.[InheritsFromHeader]	<> s.[InheritsFromHeader]
	)
	THEN
		UPDATE SET
			t.[Index]				= s.[Index],
			t.[ColumnName]			= s.[ColumnName],
			t.[EntryIndex]			= s.[EntryIndex],
			t.[Label]				= s.[Label],
			t.[Label2]				= s.[Label2],
			t.[Label3]				= s.[Label3],
			t.[Filter]				= s.[Filter],
			t.[VisibleState]		= s.[VisibleState],
			t.[RequiredState]		= s.[RequiredState],
			t.[ReadOnlyState]		= s.[ReadOnlyState],
			t.[InheritsFromHeader]	=s.[InheritsFromHeader],
			t.[SavedById]			= @UserId
	WHEN NOT MATCHED BY TARGET THEN
		INSERT ([LineDefinitionId],		[Index],	[ColumnName],	[EntryIndex], [Label],	[Label2],	[Label3], [Filter], [VisibleState],	[RequiredState], [ReadOnlyState], [InheritsFromHeader])
		VALUES (s.[LineDefinitionId], s.[Index], s.[ColumnName], s.[EntryIndex], s.[Label], s.[Label2], s.[Label3],s.[Filter], s.[VisibleState], s.[RequiredState], s.[ReadOnlyState], s.[InheritsFromHeader])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	WITH BLDGP AS (
		SELECT * FROM dbo.[LineDefinitionGenerateParameters]
		WHERE LineDefinitionId IN (SELECT [Id] FROM @LineDefinitionsIndexedIds)
	)
	MERGE INTO BLDGP AS t
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
			LDGP.[Control],
			LDGP.[ControlOptions]
		FROM @LineDefinitionGenerateParameters LDGP
		JOIN @LineDefinitionsIndexedIds II ON LDGP.[HeaderIndex] = II.[Index]
	) AS s
	ON s.[Id] = t.[Id]
	WHEN MATCHED 
	AND (
			t.[Index]				<> s.[Index] OR
			t.[Key]					<> s.[Key] OR
			t.[Label]				<> s.[Label] OR
			ISNULL(t.[Label2],N'')	<> ISNULL(s.[Label2],N'') OR
			ISNULL(t.[Label3],N'')	<> ISNULL(s.[Label3],N'') OR
			t.[Visibility]			<> s.[Visibility] OR
			t.[Control]				<> s.[Control] OR
			ISNULL(t.[ControlOptions],N'')	<> ISNULL(s.[ControlOptions],N'')
	)
	THEN
		UPDATE SET
			t.[Index]			= s.[Index],
			t.[Key]				= s.[Key],
			t.[Label]			= s.[Label],
			t.[Label2]			= s.[Label2],
			t.[Label3]			= s.[Label3],
			t.[Visibility]		= s.[Visibility],
			t.[Control]			= s.[Control],
			t.[ControlOptions]	= s.[ControlOptions],
			t.[SavedById]		= @UserId
	WHEN NOT MATCHED BY TARGET THEN
		INSERT ([LineDefinitionId],		[Index],	[Key],	[Label],	[Label2],	[Label3], [Visibility],	[Control], [ControlOptions])
		VALUES (s.[LineDefinitionId], s.[Index], s.[Key], s.[Label], s.[Label2], s.[Label3],s.[Visibility], s.[Control], s.[ControlOptions])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	WITH BLDSR AS (
		SELECT * FROM dbo.[LineDefinitionStateReasons]
		WHERE LineDefinitionId IN (SELECT [Id] FROM @LineDefinitionsIndexedIds)
	)
	MERGE INTO BLDSR AS t
	USING (
		SELECT
			LDSR.[Index],
			LDSR.[Id],
			II.[Id] AS [LineDefinitionId],
			LDSR.[State],
			LDSR.[Name],
			LDSR.[Name2],
			LDSR.[Name3],
			LDSR.[IsActive]
		FROM @LineDefinitionStateReasons LDSR
		JOIN @LineDefinitionsIndexedIds II ON LDSR.[HeaderIndex] = II.[Index]
	) AS s
	ON s.Id = t.Id
	WHEN MATCHED
	AND (
			t.[Index]				<> s.[Index] OR			
			t.[LineDefinitionId]	<> s.[LineDefinitionId] OR
			t.[State]				<> s.[State] OR
			t.[Name]				<> s.[Name] OR
			ISNULL(t.[Name2],N'')	<> ISNULL(s.[Name2],N'') OR
			ISNULL(t.[Name3],N'')	<> ISNULL(s.[Name3],N'') OR
			t.[IsActive]			<> s.[IsActive]
	)
	THEN
		UPDATE SET
			t.[Index]			= s.[Index],
			t.[LineDefinitionId]= s.[LineDefinitionId],
			t.[State]			= s.[State],
			t.[Name]			= s.[Name],
			t.[Name2]			= s.[Name2],
			t.[Name3]			= s.[Name3],
			t.[IsActive]		= s.[IsActive],
			t.[SavedById]		= @UserId
	WHEN NOT MATCHED BY TARGET THEN
		INSERT ([Index], [LineDefinitionId],		[State], [Name],	[Name2], [Name3], [IsActive])
		VALUES (s.[Index], s.[LineDefinitionId], s.[State], s.[Name], s.[Name2], s.[Name3], s.[IsActive])
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
			WS.[Id],
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
	AND (
			t.[Index]								<> s.[Index] OR
			t.[RuleType]							<> s.[RuleType] OR
			t.[RuleTypeEntryIndex]					<> s.[RuleTypeEntryIndex] OR
			ISNULL(t.[RoleId],0)					<> ISNULL(s.[RoleId],0) OR
			ISNULL(t.[UserId],0)					<> ISNULL(s.[UserId],0) OR
			ISNULL(t.[PredicateType],N'')			<> ISNULL(s.[PredicateType],N'') OR
			ISNULL(t.[PredicateTypeEntryIndex],0)	<> ISNULL(s.[PredicateTypeEntryIndex],0) OR
			ISNULL(t.[Value],0)						<> ISNULL(s.[Value],0) OR
			ISNULL(t.[ProxyRoleId],0)				<> ISNULL(s.[ProxyRoleId],0)
	) 
	THEN
		UPDATE SET
			t.[Index]					= s.[Index],
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
			[Index],
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
			s.[Index],
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
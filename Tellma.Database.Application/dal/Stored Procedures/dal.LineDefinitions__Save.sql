CREATE PROCEDURE [dal].[LineDefinitions__Save]
	@Entities [LineDefinitionList] READONLY,
	@LineDefinitionColumns [LineDefinitionColumnList] READONLY,
	@LineDefinitionEntries [LineDefinitionEntryList] READONLY,
	@LineDefinitionStateReasons [LineDefinitionStateReasonList] READONLY,
	@Workflows [WorkflowList] READONLY,
	@WorkflowSignatures [WorkflowSignatureList] READONLY
AS
SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	DECLARE @WorkflowIndexedIds [dbo].[IndexIdWithStringHeaderList];

	MERGE INTO [dbo].[LineDefinitions] AS t
	USING (
		SELECT
			[Index],
			[Id],
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
			[Script]
		FROM @Entities 
	) AS s ON (t.Id = s.[Id])
	WHEN MATCHED
	-- TODO: reduce history table by excluding saves when the data did not change
	-- Completed for main table. Sill needed for weak entities.
		AND (
		t.[TitleSingular]				<> s.[TitleSingular] OR	
		t.[TitlePlural]					<> s.[TitlePlural] OR
		t.[AllowSelectiveSigning]		<> s.[AllowSelectiveSigning] OR
		ISNULL(t.[Description], N'')	<> ISNULL(s.[Description], N'') OR	
		ISNULL(t.[Description2], N'')	<> ISNULL(s.[Description2], N'') OR
		ISNULL(t.[Description3], N'')	<> ISNULL(s.[Description3], N'') OR	
		ISNULL(t.[TitleSingular2], N'')	<> ISNULL(s.[TitleSingular2], N'') OR	
		ISNULL(t.[TitlePlural2], N'')	<> ISNULL(s.[TitlePlural2], N'') OR
		ISNULL(t.[TitleSingular3], N'')	<> ISNULL(s.[TitleSingular3], N'') OR	
		ISNULL(t.[TitlePlural3], N'')	<> ISNULL(s.[TitlePlural3], N'') OR
		ISNULL(t.[Script], N'')			<> ISNULL(s.[Script], N'')
		)
	THEN
		UPDATE SET
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
			t.[Script]						= s.[Script],
			t.[SavedById]					= @UserId
	WHEN NOT MATCHED THEN
		INSERT (
			[Id],
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
			[Script]
		)
		VALUES (
			s.[Id],
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
			s.[Script]
		);

	MERGE [dbo].[LineDefinitionEntries] AS t
	USING (
		SELECT
			LDE.[Id],
			LD.[Id] AS [LineDefinitionId],
			LDE.[Index],
			LDE.[Direction]	,
			LDE.[AccountTypeParentCode]	,
			LDE.[IsCurrent],
			LDE.[AgentDefinitionId],
			LDE.[NotedAgentDefinitionId],
			LDE.[EntryTypeCode]
		FROM @LineDefinitionEntries LDE
		JOIN @Entities LD ON LDE.HeaderIndex = LD.[Index]
	) AS s
	ON s.[Id] = t.[Id]
	WHEN MATCHED THEN
		UPDATE SET
			t.[Index]					= s.[Index],
			t.[Direction]				= s.[Direction],
			t.[AccountTypeParentCode]	= s.[AccountTypeParentCode],
			t.[IsCurrent]				= s.[IsCurrent],
			t.[AgentDefinitionId]		= s.[AgentDefinitionId],
			t.[NotedAgentDefinitionId]	= s.[NotedAgentDefinitionId],
			t.[EntryTypeCode]			= s.[EntryTypeCode],
			t.[SavedById]				= @UserId
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (
			[LineDefinitionId],
			[Index],
			[Direction],
			[AccountTypeParentCode],
			[IsCurrent],
			[AgentDefinitionId],
			[NotedAgentDefinitionId],
			[EntryTypeCode]
		)
		VALUES (
			s.[LineDefinitionId],
			s.[Index],
			s.[Direction],
			s.[AccountTypeParentCode],
			s.[IsCurrent],
			s.[AgentDefinitionId],
			s.[NotedAgentDefinitionId],
			s.[EntryTypeCode]
		);
	MERGE [dbo].[LineDefinitionColumns] AS t
	USING (
		SELECT
			LDC.[Id],
			LD.[Id] AS [LineDefinitionId],
			LDC.[Index],
			LDC.[TableName],
			LDC.[ColumnName],
			LDC.[EntryIndex],
			LDC.[Label],
			LDC.[Label2],
			LDC.[Label3],
			LDC.[RequiredState],
			LDC.[ReadOnlyState],
			LDC.[InheritsFromHeader]
		FROM @LineDefinitionColumns LDC
		JOIN @Entities LD ON LDC.HeaderIndex = LD.[Index]
	) AS s
	ON s.[Id] = t.[Id]
	WHEN MATCHED THEN
		UPDATE SET
			t.[Index]			= s.[Index],
			t.[TableName]		= s.[TableName],
			t.[ColumnName]		= s.[ColumnName],
			t.[EntryIndex]		= s.[EntryIndex],
			t.[Label]			= s.[Label],
			t.[Label2]			= s.[Label2],
			t.[Label3]			= s.[Label3],
			t.[RequiredState]	= s.[RequiredState],
			t.[ReadOnlyState]	= s.[ReadOnlyState],
			t.[InheritsFromHeader]=s.[InheritsFromHeader],
			t.[SavedById]		= @UserId
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE
	WHEN NOT MATCHED BY TARGET THEN
		INSERT ([LineDefinitionId],		[Index],	[TableName], [ColumnName],	[EntryIndex], [Label],	[Label2],	[Label3],	[RequiredState], [ReadOnlyState], [InheritsFromHeader])
		VALUES (s.[LineDefinitionId], s.[Index], s.[TableName], s.[ColumnName], s.[EntryIndex], s.[Label], s.[Label2], s.[Label3], s.[RequiredState], s.[ReadOnlyState], s.[InheritsFromHeader]);
	MERGE [dbo].[LineDefinitionStateReasons] AS t
	USING (
		SELECT
			LDSR.[Id],
			LD.[Id] AS [LineDefinitionId],
			LDSR.[State],
			LDSR.[Name],
			LDSR.[Name2],
			LDSR.[Name3],
			LDSR.[IsActive]
		FROM @LineDefinitionStateReasons LDSR
		JOIN @Entities LD ON LDSR.HeaderIndex = LD.[Index]
	)AS s
	ON s.Id = t.Id
	WHEN MATCHED THEN
		UPDATE SET
			t.[LineDefinitionId]= s.[LineDefinitionId],
			t.[State]			= s.[State],
			t.[Name]			= s.[Name],
			t.[Name2]			= s.[Name2],
			t.[Name3]			= s.[Name3],
			t.[IsActive]		= s.[IsActive],
			t.[SavedById]		= @UserId
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE
	WHEN NOT MATCHED BY TARGET THEN
		INSERT ([LineDefinitionId],		[State], [Name],		[Name2], [Name3], [IsActive])
		VALUES (s.[LineDefinitionId], s.[State], s.[Name], s.[Name2], s.[Name3], s.[IsActive]);

	WITH BW AS (
		SELECT * FROM dbo.[Workflows]
		WHERE LineDefinitionId IN (SELECT [Id] FROM @Entities)
	)
	INSERT INTO @WorkflowIndexedIds([Index], [HeaderId], [Id])
	SELECT x.[Index], x.[LineDefinitionId], x.[Id]
	FROM
	(
		MERGE [dbo].[Workflows] AS t
		USING (
			SELECT
				W.[Index],
				W.[Id],
				LD.[Id] AS [LineDefinitionId],
				W.[ToState]
			FROM @Workflows W
			JOIN @Entities LD ON W.[LineDefinitionIndex] = LD.[Index]
		) AS s
		ON s.[Id] = t.[Id]
		WHEN MATCHED THEN
			UPDATE SET
				t.[ToState]		= s.[ToState],
				t.[SavedById]	= @UserId
		WHEN NOT MATCHED BY SOURCE THEN
			DELETE
		WHEN NOT MATCHED BY TARGET THEN
			INSERT (
				[LineDefinitionId],
				[ToState]
			)
			VALUES (
				s.[LineDefinitionId],
				s.[ToState]
			)
		OUTPUT s.[Index], inserted.[LineDefinitionId], inserted.[Id]
	) AS x;

	WITH BWS AS (
		SELECT * FROM dbo.[WorkflowSignatures]
		WHERE [WorkflowId] IN (SELECT [Id] FROM @WorkflowIndexedIds)
	)
	MERGE [dbo].[WorkflowSignatures] AS t
	USING (
		SELECT
			WS.[Index],
			WS.[Id],
			LD.[Id] AS [LineDefinitionId],
			WS.[RuleType],
			WS.[RuleTypeEntryIndex],
			WS.[RoleId],
			WS.[Userid],
			WS.[PredicateType],
			WS.[PredicateTypeEntryIndex],
			WS.[Value],
			WS.[ProxyRoleId]
		FROM @WorkflowSignatures WS
		JOIN @WorkflowIndexedIds WI ON WS.[WorkflowIndex] = WI.[Index]
		JOIN @Entities LD ON 
			WI.[HeaderId] = LD.[Id]
		AND WS.[LineDefinitionIndex] = LD.[Index]
	) AS s
	ON s.[Id] = t.[Id]
	WHEN MATCHED THEN
		UPDATE SET
			t.[RuleType]				= s.[RuleType],
			t.[RuleTypeEntryIndex]		= s.[RuleTypeEntryIndex],
			t.[RoleId]					= s.[RoleId],
			t.[Userid]					= s.[Userid],
			t.[PredicateType]			= s.[PredicateType],
			t.[PredicateTypeEntryIndex]	= s.[PredicateTypeEntryIndex],
			t.[Value]					= s.[Value],
			t.[ProxyRoleId]				= s.[ProxyRoleId],
			t.[SavedById]	= @UserId
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (
			[RuleType],
			[RuleTypeEntryIndex],
			[RoleId],
			[Userid],
			[PredicateType],
			[PredicateTypeEntryIndex],
			[Value],
			[ProxyRoleId]
		)
		VALUES (
			s.[RuleType],
			s.[RuleTypeEntryIndex],
			s.[RoleId],
			s.[Userid],
			s.[PredicateType],
			s.[PredicateTypeEntryIndex],
			s.[Value],
			s.[ProxyRoleId]
		);

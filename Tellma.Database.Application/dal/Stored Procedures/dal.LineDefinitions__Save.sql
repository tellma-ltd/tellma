﻿CREATE PROCEDURE [dal].[LineDefinitions__Save]
	@Entities [dbo].[LineDefinitionList] READONLY,
	@LineDefinitionEntries [dbo].[LineDefinitionEntryList] READONLY,
	@LineDefinitionEntryAgentDefinitions [dbo].[LineDefinitionEntryAgentDefinitionList] READONLY,
	@LineDefinitionEntryResourceDefinitions [dbo].[LineDefinitionEntryResourceDefinitionList] READONLY,
	@LineDefinitionEntryNotedAgentDefinitions [dbo].[LineDefinitionEntryNotedAgentDefinitionList] READONLY,
	@LineDefinitionEntryNotedResourceDefinitions [dbo].[LineDefinitionEntryNotedResourceDefinitionList] READONLY,
	@LineDefinitionColumns [dbo].[LineDefinitionColumnList] READONLY,
	@LineDefinitionGenerateParameters [dbo].[LineDefinitionGenerateParameterList] READONLY,
	@LineDefinitionStateReasons [dbo].[LineDefinitionStateReasonList] READONLY,
	@Workflows [dbo].[WorkflowList] READONLY,
	@WorkflowSignatures [dbo].[WorkflowSignatureList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @LineDefinitionsIndexedIds [dbo].[IndexedIdList], @LineDefinitionEntriesIndexIds [dbo].[IndexIdWithHeaderList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
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
				[LineType],
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
				[ValidateScript],
				[SignValidateScript],
				[UnsignValidateScript]
			FROM @Entities 
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED
		THEN
			UPDATE SET
				t.[Code]						= s.[Code],
				t.[LineType]					= s.[LineType],
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
				t.[SignValidateScript]			= s.[SignValidateScript],
				t.[UnsignValidateScript]		= s.[UnsignValidateScript],
				t.[SavedById]					= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[Code],
				[LineType],
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
				[ValidateScript],
				[SignValidateScript],
				[UnsignValidateScript],
				[SavedById]
			)
			VALUES (
				s.[Code],
				s.[LineType],
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
				s.[ValidateScript],
				s.[SignValidateScript],
				s.[UnsignValidateScript],
				@UserId
			)
		OUTPUT s.[Index], inserted.[Id]
	) AS x;

	WITH BLDE AS (
		SELECT * FROM [dbo].[LineDefinitionEntries]
		WHERE [LineDefinitionId] IN (SELECT [Id] FROM @LineDefinitionsIndexedIds)
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
				[EntryTypeId],
				[SavedById]
			)
			VALUES (
				s.[LineDefinitionId],
				s.[Index],
				s.[Direction],
				s.[ParentAccountTypeId],
				s.[EntryTypeId],
				@UserId
			)
		WHEN NOT MATCHED BY SOURCE THEN
			DELETE
		OUTPUT s.[Index], inserted.[Id], inserted.[LineDefinitionId]
	) AS x
	WHERE [Index] IS NOT NULL;

	WITH BLDEAD AS (
		SELECT * FROM dbo.[LineDefinitionEntryAgentDefinitions]
		WHERE [LineDefinitionEntryId] IN (SELECT [Id] FROM @LineDefinitionEntriesIndexIds)
	)
	MERGE INTO BLDEAD AS t
	USING (
		SELECT
			E.[Id], LI.Id AS [LineDefinitionEntryId], E.[AgentDefinitionId]
		FROM @LineDefinitionEntryAgentDefinitions E
		JOIN @LineDefinitionsIndexedIds DI ON E.[LineDefinitionIndex] = DI.[Index]
		JOIN @LineDefinitionEntriesIndexIds LI ON E.[LineDefinitionEntryIndex] = LI.[Index] AND LI.[HeaderId] = DI.[Id]
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED
	AND (
		ISNULL(t.[AgentDefinitionId],0) <> ISNULL(s.[AgentDefinitionId],0)
	)
	THEN
		UPDATE SET
			t.[AgentDefinitionId]	= s.[AgentDefinitionId],
			t.[SavedById]				= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([LineDefinitionEntryId], [AgentDefinitionId], [SavedById])
		VALUES (s.[LineDefinitionEntryId], s.[AgentDefinitionId], @UserId)
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
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED
	AND (
		ISNULL(t.[ResourceDefinitionId],0) <> ISNULL(s.[ResourceDefinitionId],0)
	)
	THEN
		UPDATE SET
			t.[ResourceDefinitionId]	= s.[ResourceDefinitionId],
			t.[SavedById]				= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([LineDefinitionEntryId], [ResourceDefinitionId], [SavedById])
		VALUES (s.[LineDefinitionEntryId], s.[ResourceDefinitionId], @UserId)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	WITH BLDENAD AS (
		SELECT * FROM dbo.[LineDefinitionEntryNotedAgentDefinitions]
		WHERE [LineDefinitionEntryId] IN (SELECT [Id] FROM @LineDefinitionEntriesIndexIds)
	)
	MERGE INTO BLDENAD AS t
	USING (
		SELECT
			E.[Id], LI.Id AS [LineDefinitionEntryId], E.[NotedAgentDefinitionId]
		FROM @LineDefinitionEntryNotedAgentDefinitions E
		JOIN @LineDefinitionsIndexedIds DI ON E.[LineDefinitionIndex] = DI.[Index]
		JOIN @LineDefinitionEntriesIndexIds LI ON E.[LineDefinitionEntryIndex] = LI.[Index] AND LI.[HeaderId] = DI.[Id]
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED
	AND (
		ISNULL(t.[NotedAgentDefinitionId],0) <> ISNULL(s.[NotedAgentDefinitionId],0)
	)
	THEN
		UPDATE SET
			t.[NotedAgentDefinitionId]	= s.[NotedAgentDefinitionId],
			t.[SavedById]				= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([LineDefinitionEntryId], [NotedAgentDefinitionId], [SavedById])
		VALUES (s.[LineDefinitionEntryId], s.[NotedAgentDefinitionId], @UserId)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	WITH BLDENRD AS (
		SELECT * FROM dbo.[LineDefinitionEntryNotedResourceDefinitions]
		WHERE [LineDefinitionEntryId] IN (SELECT [Id] FROM @LineDefinitionEntriesIndexIds)
	)
	MERGE INTO BLDENRD AS t
	USING (
		SELECT
			E.[Id], LI.Id AS [LineDefinitionEntryId], E.[NotedResourceDefinitionId]
		FROM @LineDefinitionEntryNotedResourceDefinitions E
		JOIN @LineDefinitionsIndexedIds DI ON E.[LineDefinitionIndex] = DI.[Index]
		JOIN @LineDefinitionEntriesIndexIds LI ON E.[LineDefinitionEntryIndex] = LI.[Index] AND LI.[HeaderId] = DI.[Id]
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED
	AND (
		ISNULL(t.[NotedResourceDefinitionId],0) <> ISNULL(s.[NotedResourceDefinitionId],0)
	)
	THEN
		UPDATE SET
			t.[NotedResourceDefinitionId]	= s.[NotedResourceDefinitionId],
			t.[SavedById]				= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([LineDefinitionEntryId], [NotedResourceDefinitionId], [SavedById])
		VALUES (s.[LineDefinitionEntryId], s.[NotedResourceDefinitionId], @UserId)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	WITH BLDC AS (
		SELECT * FROM [dbo].[LineDefinitionColumns]
		WHERE [LineDefinitionId] IN (SELECT [Id] FROM @LineDefinitionsIndexedIds)
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
			ISNULL(t.[Filter],N'')	<> ISNULL(s.[Filter],N'') COLLATE Latin1_General_CS_AS OR
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
		INSERT ([LineDefinitionId],		[Index],	[ColumnName],	[EntryIndex], [Label],	[Label2],	[Label3], [Filter], [VisibleState],	[RequiredState], [ReadOnlyState], [InheritsFromHeader], [SavedById])
		VALUES (s.[LineDefinitionId], s.[Index], s.[ColumnName], s.[EntryIndex], s.[Label], s.[Label2], s.[Label3],s.[Filter], s.[VisibleState], s.[RequiredState], s.[ReadOnlyState], s.[InheritsFromHeader], @UserId)
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
		INSERT ([LineDefinitionId],		[Index],	[Key],	[Label],	[Label2],	[Label3], [Visibility],	[Control], [ControlOptions], [SavedById])
		VALUES (s.[LineDefinitionId], s.[Index], s.[Key], s.[Label], s.[Label2], s.[Label3],s.[Visibility], s.[Control], s.[ControlOptions], @UserId)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	WITH BLDSR AS (
		SELECT * FROM [dbo].[LineDefinitionStateReasons]
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
		INSERT ([Index], [LineDefinitionId],		[State], [Name],	[Name2], [Name3], [IsActive], [SavedById])
		VALUES (s.[Index], s.[LineDefinitionId], s.[State], s.[Name], s.[Name2], s.[Name3], s.[IsActive], @UserId)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	WITH BLDW AS (
		SELECT * FROM [dbo].[Workflows]
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
				[ToState],
				[SavedById]
			)
			VALUES (
				s.[LineDefinitionId],
				s.[ToState],
				@UserId
			)
		WHEN NOT MATCHED BY SOURCE THEN
			DELETE
		OUTPUT s.[Index], inserted.[Id], inserted.[LineDefinitionId]
	) AS x
	WHERE [Index] IS NOT NULL;

	WITH BLDWS AS (
		SELECT * FROM [dbo].[WorkflowSignatures] -- check if there are already signatures for the transition
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
			[ProxyRoleId],
			[SavedById]
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
			s.[ProxyRoleId],
			@UserId
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- Signal clients to refresh their cache
	UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();

	IF @ReturnIds = 1
		SELECT * FROM @LineDefinitionsIndexedIds;
END;
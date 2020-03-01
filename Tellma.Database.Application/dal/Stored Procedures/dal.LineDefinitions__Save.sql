CREATE PROCEDURE [dal].[LineDefinitions__Save]
	@Entities [LineDefinitionList] READONLY,
	@LineDefinitionColumns [LineDefinitionColumnList] READONLY,
	@LineDefinitionEntries [LineDefinitionEntryList] READONLY,
	@LineDefinitionStateReasons [LineDefinitionStateReasonList] READONLY
AS
SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

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
			[AgentDefinitionList],
			[ResponsibilityTypeList],
			[AllowSelectiveSigning],
			[Script]
		FROM @Entities 
	) AS s ON (t.Id = s.[Id])
	WHEN MATCHED
	-- TODO: reduce history table by excluding saves when the data did not change
		--AND (
		--t.[TitleSingular] <> s.[TitleSingular] OR	
		--t.[TitlePlural] <> s.[TitlePlural] OR
		--...
		--)
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
			t.[AgentDefinitionList]			= s.[AgentDefinitionList],
			t.[ResponsibilityTypeList]		= s.[ResponsibilityTypeList],
			t.[AllowSelectiveSigning]		= s.[AllowSelectiveSigning],
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
			[AgentDefinitionList],
			[ResponsibilityTypeList],
			[AllowSelectiveSigning],
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
			s.[AgentDefinitionList],
			s.[ResponsibilityTypeList],
			s.[AllowSelectiveSigning],
			s.[Script]
		);

	MERGE [dbo].[LineDefinitionColumns] AS t
	USING (
		SELECT
			LDC.[Id],
			LD.[Id] AS [LineDefinitionId],
			LDC.[Index],
			LDC.[TableName],
			LDC.[ColumnName],
			LDC.[EntryNumber],
			LDC.[Label],
			LDC.[Label2],
			LDC.[Label3],
			LDC.[RequiredState],
			LDC.[ReadOnlyState]
		FROM @LineDefinitionColumns LDC
		JOIN @Entities LD ON LDC.HeaderIndex = LD.[Index]
	) AS s
	ON s.[Id] = t.[Id]
	WHEN MATCHED THEN
		UPDATE SET
			t.[Index]			= s.[Index],
			t.[TableName]		= s.[TableName],
			t.[ColumnName]		= s.[ColumnName],
			t.[EntryNumber]		= s.[EntryNumber],
			t.[Label]			= s.[Label],
			t.[Label2]			= s.[Label2],
			t.[Label3]			= s.[Label3],
			t.[RequiredState]	= s.[RequiredState],
			t.[ReadOnlyState]	= s.[ReadOnlyState],
			t.[SavedById]		= @UserId
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE
	WHEN NOT MATCHED BY TARGET THEN
		INSERT ([LineDefinitionId],		[Index],	[TableName], [ColumnName],	[EntryNumber], [Label],	[Label2],	[Label3],	[RequiredState], [ReadOnlyState])
		VALUES (s.[LineDefinitionId], s.[Index], s.[TableName], s.[ColumnName], s.[EntryNumber], s.[Label], s.[Label2], s.[Label3], s.[RequiredState], s.[ReadOnlyState]);

	MERGE [dbo].[LineDefinitionEntries] AS t
	USING (
		SELECT
			LDE.[Id],
			LD.[Id] AS [LineDefinitionId],
			LDE.[EntryNumber],
			LDE.[Direction]	,
			LDE.[AccountTypeParentCode]	,
			LDE.[AccountTagId],
			LDE.[AgentDefinitionId],
			LDE.[EntryTypeCode]
		FROM @LineDefinitionEntries LDE
		JOIN @Entities LD ON LDE.HeaderIndex = LD.[Index]
	) AS s
	ON s.[Id] = t.[Id]
	WHEN MATCHED THEN
	UPDATE SET
		t.[EntryNumber]				= s.[EntryNumber],
		t.[Direction]				= s.[Direction],
		t.[AccountTypeParentCode]	= s.[AccountTypeParentCode],
		t.[AccountTagId]			= s.[AccountTagId],
		t.[AgentDefinitionId]		= s.[AgentDefinitionId],
		t.[EntryTypeCode]			= s.[EntryTypeCode],
		t.[SavedById]				= @UserId
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT (
		[LineDefinitionId],
		[EntryNumber],
		[Direction],
		[AccountTypeParentCode]	,
		[AccountTagId],
		[AgentDefinitionId],
		[EntryTypeCode]
	)
    VALUES (
		s.[LineDefinitionId],
		s.[EntryNumber],
		s.[Direction],
		s.[AccountTypeParentCode],
		s.[AccountTagId],
		s.[AgentDefinitionId],
		s.[EntryTypeCode]
	);

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
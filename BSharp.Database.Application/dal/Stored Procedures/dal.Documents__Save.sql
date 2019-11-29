CREATE PROCEDURE [dal].[Documents__Save]
	@DefinitionId NVARCHAR(255),
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[DocumentLineList] READONLY, 
	@Entries [dbo].[DocumentLineEntryList] READONLY,
	@ReturnIds BIT = 0
AS
BEGIN
	DECLARE @DocumentsIndexedIds [dbo].[IndexedIdList], @LinesIndexedIds [dbo].[IndexedIdList], @EntriesIndexedIds [dbo].[IndexedIdList];

	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO @DocumentsIndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[Documents] AS t
		USING (
			SELECT 
				[Index], [Id], [OperatingSegmentId], [DocumentDate], [VoucherNumericReference], --[SortKey],
				[Memo],-- [Frequency], [Repetitions],
				ROW_Number() OVER (PARTITION BY [Id] ORDER BY [Index]) + (
					-- max(SerialNumber) per document type.
					SELECT ISNULL(MAX([SerialNumber]), 0) FROM dbo.Documents WHERE [DefinitionId] = @DefinitionId
				) As [SerialNumber]
			FROM @Documents D
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED THEN
			UPDATE SET
				t.[DocumentDate]			= s.[DocumentDate],
				t.[OperatingSegmentId]		= s.[OperatingSegmentId],
				t.[VoucherNumericReference]	= s.[VoucherNumericReference],
				t.[Memo]					= s.[Memo],
				--t.[Frequency]				= s.[Frequency],
				--t.[Repetitions]			= s.[Repetitions],

				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[DefinitionId], [SerialNumber], [OperatingSegmentId], [DocumentDate], [VoucherNumericReference], --[SortKey],
				[Memo]--, [Frequency], [Repetitions]
			)
			VALUES (
				@DefinitionId, s.[SerialNumber], s.[OperatingSegmentId], s.[DocumentDate], s.[VoucherNumericReference], --s.[SerialNumber], 
				s.[Memo]--, s.[Frequency], s.[Repetitions]
			)
		OUTPUT s.[Index], inserted.[Id] 
	) As x;
	
	---- Assign the new ones to self
	INSERT INTO dbo.DocumentAssignments(DocumentId, AssigneeId)
	SELECT Id, @UserId
	FROM @DocumentsIndexedIds
	WHERE [Index] IN (SELECT [Index] FROM @Documents WHERE [Id] = 0);

	WITH BL AS (
		SELECT * FROM dbo.[DocumentLines]
		WHERE DocumentId IN (SELECT [Id] FROM @DocumentsIndexedIds)
	)
	INSERT INTO @LinesIndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO BL AS t
		USING (
			SELECT
				L.[Index],
				L.[Id],
				DI.Id AS DocumentId,
				L.[DefinitionId], 
				L.[AgentDefinitionId],
				L.[AgentId],
				L.[ResourceId],
				L.[Amount],
				L.[Memo],
				L.[ExternalReference],
				L.[AdditionalReference]
			FROM @Lines L
			JOIN @DocumentsIndexedIds DI ON L.[DocumentIndex] = DI.[Index]
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED THEN
			UPDATE SET
				t.[DefinitionId]		= s.[DefinitionId],
				t.[AgentDefinitionId]	= s.[AgentDefinitionId],
				t.[AgentId]				= s.[AgentId],
				t.[ResourceId]			= s.[ResourceId],
				t.[Amount]				= s.[Amount],
				t.[Memo]				= s.[Memo],
				t.[ExternalReference]	= s.[ExternalReference],
				t.[AdditionalReference]	= s.[AdditionalReference],
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([DocumentId], [DefinitionId], [SortKey],
				[AgentDefinitionId],
				[AgentId],
				[ResourceId],
				[Amount],
				[Memo],
				[ExternalReference],
				[AdditionalReference]
			)
			VALUES (s.[DocumentId], s.[DefinitionId], s.[Index],
				s.[AgentDefinitionId],
				s.[AgentId],
				s.[ResourceId],
				s.[Amount],
				s.[Memo],
				s.[ExternalReference],
				s.[AdditionalReference]
			)
		WHEN NOT MATCHED BY SOURCE THEN
			DELETE
		OUTPUT s.[Index], inserted.[Id] 
	) AS x
	WHERE [Index] IS NOT NULL;

	WITH BE AS (
		SELECT * FROM dbo.[DocumentLineEntries]
		WHERE [DocumentLineId] IN (SELECT [Id] FROM @LinesIndexedIds)
	)
	MERGE INTO BE AS t
	USING (
		SELECT
			E.[Index], E.[Id], LI.Id AS [DocumentLineId], [EntryNumber], [SortKey], [Direction], [AccountId], 
			[ResponsibilityCenterId], [CurrencyId], [AgentDefinitionId], [AgentId], [ResourceId], [EntryTypeId], --[BatchCode], 
			[DueDate],
			[MonetaryValue], E.[Count], E.[Mass], E.[Volume], E.[Time], E.[Value],
			E.[ExternalReference], E.[AdditionalReference], E.[RelatedAgentId], E.[RelatedAgentName], E.[RelatedAmount]
				
		FROM @Entries E
		JOIN @DocumentsIndexedIds DI ON E.[DocumentIndex] = DI.[Index]
		JOIN @LinesIndexedIds LI ON E.[DocumentLineIndex] = LI.[Index]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED THEN
		UPDATE SET
			t.[SortKey]					= s.[Index],
			t.[Direction]				= s.[Direction],	
			t.[AccountId]				= s.[AccountId],
			t.[ResponsibilityCenterId]	= s.[ResponsibilityCenterId],
			t.[CurrencyId]				= s.[CurrencyId],
			t.[AgentDefinitionId]		= s.[AgentDefinitionId],
			t.[AgentId]					= s.[AgentId],
			t.[ResourceId]				= s.[ResourceId],
			t.[EntryTypeId]				= s.[EntryTypeId],
		--	t.[ResourceDescriptorId]				= s.[BatchCode],
			t.[DueDate]					= s.[DueDate],
			t.[MonetaryValue]			= s.[MonetaryValue],
			t.[Count]					= s.[Count],
			t.[Mass]					= s.[Mass],
			t.[Volume]					= s.[Volume],
			t.[Time]					= s.[Time],
			t.[Value]					= s.[Value],
			t.[ExternalReference]		= s.[ExternalReference],
			t.[AdditionalReference]		= s.[AdditionalReference],
			t.[RelatedAgentId]			= s.[RelatedAgentId],
			t.[RelatedAgentName]		= s.[RelatedAgentName],
			t.[RelatedAmount]			= s.[RelatedAmount],
	
			t.[ModifiedAt]				= @Now,
			t.[ModifiedById]			= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([DocumentLineId], [EntryNumber], [SortKey], [Direction], [AccountId], 
				[ResponsibilityCenterId], [CurrencyId], [AgentDefinitionId], [AgentId], [ResourceId], [EntryTypeId], --[BatchCode], 
				[DueDate],
				[MonetaryValue], [Count], [Mass], [Volume], [Time], [Value],
				[ExternalReference], [AdditionalReference], [RelatedAgentId], [RelatedAgentName], [RelatedAmount]
				)
		VALUES (s.[DocumentLineId], s.[EntryNumber], s.[SortKey], s.[Direction], s.[AccountId], 
				s.[ResponsibilityCenterId], s.[CurrencyId], s.[AgentDefinitionId], s.[AgentId], s.[ResourceId], s.[EntryTypeId], --s.[BatchCode], 
				s.[DueDate],
				s.[MonetaryValue], s.[Count], s.[Mass], s.[Volume], s.[Time], s.[Value],
				s.[ExternalReference], s.[AdditionalReference], s.[RelatedAgentName], s.[RelatedAgentId], s.[RelatedAmount]
				)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	IF (@ReturnIds = 1)
		SELECT * FROM @DocumentsIndexedIds;
END;
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
				[Index], [Id], [DocumentDate], [VoucherNumericReference], --[SortKey],
				[Memo],-- [Frequency], [Repetitions],
				ROW_Number() OVER (PARTITION BY [Id] ORDER BY [Index]) + (
					-- max(SerialNumber) per document type.
					SELECT ISNULL(MAX([SerialNumber]), 0) FROM dbo.Documents WHERE [DocumentDefinitionId] = @DefinitionId
				) As [SerialNumber]
			FROM @Documents D
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED
		THEN
			UPDATE SET
				t.[DocumentDate]			= s.[DocumentDate],
				t.[VoucherNumericReference]	= s.[VoucherNumericReference],
				t.[Memo]					= s.[Memo],
				--t.[Frequency]				= s.[Frequency],
				--t.[Repetitions]				= s.[Repetitions],

				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[DocumentDefinitionId], [SerialNumber], [DocumentDate], [VoucherNumericReference], [SortKey],
				[Memo]--, [Frequency], [Repetitions]
			)
			VALUES (
				@DefinitionId, s.[SerialNumber], s.[DocumentDate], s.[VoucherNumericReference], s.[SerialNumber], 
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
			SELECT L.[Index], L.[Id], DI.Id AS DocumentId, L.[LineDefinitionId], --L.[TemplateLineId], L.[ScalingFactor],
					--L.[SortKey],
					L.[Memo], L.[ExternalReference], L.[AdditionalReference], L.[RelatedResourceId], L.[RelatedAgentId], L.[RelatedMoneyAmount]
			FROM @Lines L
			JOIN @DocumentsIndexedIds DI ON L.[DocumentIndex] = DI.[Index]
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED THEN
			UPDATE SET
				t.[LineDefinitionId]	= s.[LineDefinitionId],
				--t.[TemplateLineId]		= s.[TemplateLineId], 
				--t.[ScalingFactor]		= s.[ScalingFactor],
				t.[Memo]				= s.[Memo],

				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([DocumentId], [LineDefinitionId], [SortKey]--, [TemplateLineId], [ScalingFactor]--
				, [Memo]
			)
			VALUES (s.[DocumentId], s.[LineDefinitionId], s.[Index], --s.[TemplateLineId], s.[ScalingFactor], 
			s.[Memo]
			--,s.[ExternalReference], s.[AdditionalReference], s.[RelatedResourceId], s.[RelatedAgentId], s.[RelatedMoneyAmount]
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
			E.[Index], E.[Id], LI.Id AS [DocumentLineId], [EntryNumber], [Direction], [AccountId], [EntryTypeId],
				--[AgentId], [ResponsibilityCenterId], [ResourceId], 
				[ResourcePickId], [BatchCode], [DueDate],
				[MonetaryValue], E.[Mass], E.[Volume], E.[Area], E.[Length], E.[Time], E.[Count], E.[Value],
				E.[Memo], E.[ExternalReference], E.[AdditionalReference], E.[RelatedResourceId], E.[RelatedAgentId], E.[RelatedMonetaryAmount]
				
		FROM @Entries E
		JOIN @DocumentsIndexedIds DI ON E.[DocumentIndex] = DI.[Index]
		JOIN @LinesIndexedIds LI ON E.[DocumentLineIndex] = LI.[Index]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED THEN
		UPDATE SET
			t.[Direction]				= s.[Direction],	
			t.[AccountId]				= s.[AccountId],
			t.[EntryTypeId]				= s.[EntryTypeId],

			t.[ResourcePickId]			= s.[ResourcePickId],
			t.[BatchCode]				= s.[BatchCode],
			t.[MonetaryValue]			= s.[MonetaryValue],
			t.[Mass]					= s.[Mass],
			t.[Volume]					= s.[Volume],
			t.[Area]					= s.[Area],
			t.[Length]					= s.[Length],
			t.[Time]					= s.[Time],
			t.[Count]					= s.[Count],
			t.[Value]					= s.[Value],
			t.[Memo]					= s.[Memo],
			t.[ExternalReference]		= s.[ExternalReference],
			t.[AdditionalReference]		= s.[AdditionalReference],
			t.[RelatedResourceId]		= s.[RelatedResourceId],
			t.[RelatedAgentId]			= s.[RelatedAgentId],
			t.[ModifiedAt]				= @Now,
			t.[ModifiedById]			= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([DocumentLineId], [EntryNumber], [Direction], [AccountId], [EntryTypeId],
				--[AgentId], [ResponsibilityCenterId], [ResourceId], 
				[ResourcePickId], [BatchCode],
				[MonetaryValue], [Mass], [Volume], [Area], [Length], [Time], [Count],  [Value],
				[Memo], [ExternalReference], [AdditionalReference], [RelatedResourceId], [RelatedAgentId], [RelatedMonetaryAmount]
				)
		VALUES (s.[DocumentLineId], s.[EntryNumber], s.[Direction], s.[AccountId], s.[EntryTypeId],
				--s.[AgentId], s.[ResponsibilityCenterId], s.[ResourceId], 
				s.[ResourcePickId], s.[BatchCode],
				s.[MonetaryValue], s.[Mass], s.[Volume], s.[Area], s.[Length], s.[Time], s.[Count], s.[Value],
				s.[Memo], s.[ExternalReference], s.[AdditionalReference], s.[RelatedResourceId], s.[RelatedAgentId], s.[RelatedMonetaryAmount]
				)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	IF (@ReturnIds = 1)
		SELECT * FROM @DocumentsIndexedIds;
END;